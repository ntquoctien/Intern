from __future__ import annotations

from collections import defaultdict
from collections.abc import Sequence
from dataclasses import dataclass
from typing import Protocol
from uuid import UUID

import pyodbc

from .models import EvidenceDocument, OutcomeDocument


class AcademicVectorRepository(Protocol):
    def load_outcomes(
        self,
        student_id: UUID,
        selected_course_ids: list[UUID] | None,
        eligible_score: float,
    ) -> list[OutcomeDocument]: ...

    def load_fallback_evidence(
        self, student_id: UUID
    ) -> list[EvidenceDocument]: ...


@dataclass(frozen=True, slots=True)
class _Course:
    subject_id: UUID
    subject_code: str
    subject_name: str


@dataclass(frozen=True, slots=True)
class _Criterion:
    criteria_id: UUID
    name: str
    description: str


class SqlServerAcademicVectorRepository:
    """Reads approved career outcomes and verified academic evidence.

    EducationSystem currently stores the academic and career schemas in the
    same TayDoV2 database, so one read-only connection can join them safely
    without introducing a write path into this AI service.
    """

    def __init__(
        self,
        academic_connection_string: str,
        career_connection_string: str,
    ) -> None:
        if not academic_connection_string:
            raise ValueError(
                "VECTOR_ACADEMIC_DB_CONNECTION_STRING is required."
            )
        if not career_connection_string:
            raise ValueError(
                "VECTOR_CAREER_DB_CONNECTION_STRING is required."
            )
        self._academic_connection_string = academic_connection_string
        self._career_connection_string = career_connection_string

    def load_outcomes(
        self,
        student_id: UUID,
        selected_course_ids: list[UUID] | None,
        eligible_score: float,
    ) -> list[OutcomeDocument]:
        with pyodbc.connect(self._academic_connection_string) as connection:
            courses = self._load_eligible_courses(
                connection, student_id, selected_course_ids, eligible_score
            )
            if not courses:
                return []
            criteria = self._load_criteria(
                connection, student_id, list(courses)
            )
        with pyodbc.connect(self._career_connection_string) as connection:
            clos = self._load_clos(connection, list(courses))

        documents: list[OutcomeDocument] = []
        subjects_with_clos: set[UUID] = set()
        for row in clos:
            subject_id = UUID(str(row.SubjectExternalId))
            course = courses[subject_id]
            subjects_with_clos.add(subject_id)
            course_criteria = criteria.get(subject_id, [])
            criteria_text = " ".join(
                f"Tiêu chí {item.name}: {item.description}"
                for item in course_criteria
            )
            description = str(row.Description).strip()
            documents.append(
                OutcomeDocument(
                    clo_id=int(row.Id),
                    criteria_id=None,
                    subject_id=subject_id,
                    subject_code=course.subject_code,
                    subject_name=course.subject_name,
                    clo_code=str(row.CloCode).strip(),
                    description=description,
                    progression_level=(
                        self._progression_code(int(row.ProgressionRank))
                    ),
                    embedding_text=self._join_text(
                        f"Học phần {course.subject_code} {course.subject_name}.",
                        f"{row.CloCode}: {description}.",
                        criteria_text,
                    ),
                )
            )

        # Keep criteria searchable even while a subject has no approved CLO.
        for subject_id, course_criteria in criteria.items():
            if subject_id in subjects_with_clos:
                continue
            course = courses[subject_id]
            for item in course_criteria:
                documents.append(
                    OutcomeDocument(
                        clo_id=None,
                        criteria_id=item.criteria_id,
                        subject_id=subject_id,
                        subject_code=course.subject_code,
                        subject_name=course.subject_name,
                        clo_code=item.name,
                        description=item.description,
                        progression_level=None,
                        embedding_text=self._join_text(
                            f"Học phần {course.subject_code} {course.subject_name}.",
                            f"Tiêu chí đánh giá {item.name}: {item.description}",
                        ),
                    )
                )
        return documents

    def load_fallback_evidence(
        self, student_id: UUID
    ) -> list[EvidenceDocument]:
        documents: list[EvidenceDocument] = []
        with pyodbc.connect(
            self._academic_connection_string
        ) as connection:
            cursor = connection.cursor()
            cursor.execute(
                """
                SELECT ProjectID, ProjectName, TechStack, ProjectDescription,
                       MyRole, MyContributions
                FROM academic.StudentProjects
                WHERE StudentID = ?
                ORDER BY ProjectID DESC;
                """,
                str(student_id),
            )
            for row in cursor.fetchall():
                description = self._join_text(
                    str(row.TechStack or ""),
                    str(row.ProjectDescription or ""),
                    str(row.MyRole or ""),
                    str(row.MyContributions or ""),
                )
                if description:
                    documents.append(
                        EvidenceDocument(
                            source_type="Project",
                            source_id=int(row.ProjectID),
                            title=str(row.ProjectName),
                            description=description,
                            embedding_text=self._join_text(
                                f"Dự án {row.ProjectName}.", description
                            ),
                        )
                    )

            cursor.execute(
                """
                SELECT InternshipID, CompanyName, Position, TaskDescription
                FROM academic.StudentInternships
                WHERE StudentID = ?
                ORDER BY StartDate DESC;
                """,
                str(student_id),
            )
            for row in cursor.fetchall():
                description = str(row.TaskDescription or "").strip()
                if description or row.Position:
                    documents.append(
                        EvidenceDocument(
                            source_type="Internship",
                            source_id=int(row.InternshipID),
                            title=f"{row.Position} tại {row.CompanyName}",
                            description=description,
                            embedding_text=self._join_text(
                                f"Thực tập {row.Position} tại {row.CompanyName}.",
                                description,
                            ),
                        )
                    )
        return documents

    @staticmethod
    def _load_eligible_courses(
        connection: pyodbc.Connection,
        student_id: UUID,
        selected_course_ids: list[UUID] | None,
        eligible_score: float,
    ) -> dict[UUID, _Course]:
        selected_filter = ""
        parameters: list[object] = [str(student_id)]
        if selected_course_ids:
            placeholders = ",".join("?" for _ in selected_course_ids)
            selected_filter = f"AND st.SubjectId IN ({placeholders})"
            parameters.extend(str(value) for value in selected_course_ids)
        parameters.append(eligible_score)

        cursor = connection.cursor()
        cursor.execute(
            f"""
            SELECT st.SubjectId, s.SubjectCode, s.Name
            FROM academic.StudentEvaluations se
            INNER JOIN academic.SubjectTeachings st
                ON st.Id = se.SubjectTeachingId
            INNER JOIN academic.Subjects s ON s.Id = st.SubjectId
            WHERE se.StudentId = ?
              AND se.IsDeleted = 0
              AND se.TotalScore BETWEEN 0 AND 10
              AND st.IsDeleted = 0
              AND s.IsDeleted = 0
              {selected_filter}
            GROUP BY st.SubjectId, s.SubjectCode, s.Name
            HAVING AVG(CAST(se.TotalScore AS float)) >= ?;
            """,
            parameters,
        )
        return {
            UUID(str(row.SubjectId)): _Course(
                UUID(str(row.SubjectId)),
                str(row.SubjectCode).strip(),
                str(row.Name).strip(),
            )
            for row in cursor.fetchall()
        }

    @staticmethod
    def _load_criteria(
        connection: pyodbc.Connection,
        student_id: UUID,
        subject_ids: Sequence[UUID],
    ) -> dict[UUID, list[_Criterion]]:
        placeholders = ",".join("?" for _ in subject_ids)
        parameters = [str(student_id), *(str(value) for value in subject_ids)]
        cursor = connection.cursor()
        cursor.execute(
            f"""
            SELECT DISTINCT st.SubjectId, ec.Id, ec.Name, ec.Description
            FROM academic.StudentEvaluations se
            INNER JOIN academic.SubjectTeachings st
                ON st.Id = se.SubjectTeachingId
            INNER JOIN academic.StudentEvaluationDetails sed
                ON sed.StudentEvaluationId = se.Id AND sed.IsDeleted = 0
            INNER JOIN academic.EvaluationCriterias ec
                ON ec.Id = sed.EvaluationCriteriaId
            WHERE se.StudentId = ?
              AND se.IsDeleted = 0
              AND st.SubjectId IN ({placeholders});
            """,
            parameters,
        )
        result: defaultdict[UUID, list[_Criterion]] = defaultdict(list)
        for row in cursor.fetchall():
            result[UUID(str(row.SubjectId))].append(
                _Criterion(
                    UUID(str(row.Id)),
                    str(row.Name).strip(),
                    str(row.Description or "").strip(),
                )
            )
        return dict(result)

    @staticmethod
    def _load_clos(
        connection: pyodbc.Connection, subject_ids: Sequence[UUID]
    ) -> list[pyodbc.Row]:
        placeholders = ",".join("?" for _ in subject_ids)
        cursor = connection.cursor()
        cursor.execute(
            f"""
            SELECT clo.Id, clo.SubjectExternalId, clo.CloCode, clo.Description,
                   MAX(CASE mapping.ProgressionLevelCode
                       WHEN 'D' THEN 3 WHEN 'R' THEN 2 WHEN 'E' THEN 1
                       ELSE 0 END) AS ProgressionRank
            FROM career.CourseLearningOutcome clo
            LEFT JOIN career.CloPloMapping mapping
                ON mapping.CloId = clo.Id AND mapping.IsApproved = 1
            WHERE clo.Status = N'Approved'
              AND clo.SubjectExternalId IN ({placeholders})
            GROUP BY clo.Id, clo.SubjectExternalId, clo.CloCode, clo.Description,
                     clo.SortOrder
            ORDER BY clo.SubjectExternalId, clo.SortOrder, clo.CloCode;
            """,
            [str(value) for value in subject_ids],
        )
        return list(cursor.fetchall())

    @staticmethod
    def _progression_code(rank: int) -> str | None:
        return {1: "E", 2: "R", 3: "D"}.get(rank)

    @staticmethod
    def _join_text(*parts: str) -> str:
        return " ".join(part.strip() for part in parts if part and part.strip())
