from __future__ import annotations

from dataclasses import dataclass
from typing import Literal
from uuid import UUID

from pydantic import BaseModel, ConfigDict, Field, field_validator


class CamelModel(BaseModel):
    model_config = ConfigDict(
        alias_generator=lambda value: "".join(
            word if index == 0 else word.capitalize()
            for index, word in enumerate(value.split("_"))
        ),
        populate_by_name=True,
    )


class VectorMatchRequest(CamelModel):
    student_id: UUID
    job_description: str = Field(min_length=10, max_length=10_000)
    selected_course_ids: list[UUID] | None = None
    top_k: int = Field(default=10, ge=1, le=100)
    similarity_threshold: float = Field(default=0.65, ge=0.50, le=1.0)

    @field_validator("job_description")
    @classmethod
    def normalize_job_description(cls, value: str) -> str:
        normalized = " ".join(value.split())
        if len(normalized) < 10:
            raise ValueError("jobDescription must contain meaningful text.")
        return normalized

    @field_validator("selected_course_ids")
    @classmethod
    def deduplicate_courses(
        cls, value: list[UUID] | None
    ) -> list[UUID] | None:
        if not value:
            return None
        return list(dict.fromkeys(value))


class MatchedOutcome(CamelModel):
    clo_id: int | None = None
    criteria_id: UUID | None = None
    source_type: Literal["CLO", "EvaluationCriteria"]
    subject_code: str
    subject_name: str
    clo_code: str
    description: str
    similarity_score: float
    progression_level: Literal["E", "R", "D"] | None = None


class FallbackEvidence(CamelModel):
    source_type: Literal["Project", "Internship"]
    source_id: int
    title: str
    description: str
    similarity_score: float


class VectorMatchResponse(CamelModel):
    student_id: UUID
    target_job_matched: bool
    is_fallback: bool
    applied_similarity_threshold: float
    matched_outcome_count: int
    top_matched_outcomes: list[MatchedOutcome]
    fallback_evidence: list[FallbackEvidence] = Field(default_factory=list)
    warnings: list[str] = Field(default_factory=list)


class ReindexResponse(CamelModel):
    student_id: UUID
    indexed_vector_count: int


@dataclass(frozen=True, slots=True)
class OutcomeDocument:
    clo_id: int | None
    criteria_id: UUID | None
    subject_id: UUID
    subject_code: str
    subject_name: str
    clo_code: str
    description: str
    progression_level: str | None
    embedding_text: str


@dataclass(frozen=True, slots=True)
class EvidenceDocument:
    source_type: Literal["Project", "Internship"]
    source_id: int
    title: str
    description: str
    embedding_text: str

