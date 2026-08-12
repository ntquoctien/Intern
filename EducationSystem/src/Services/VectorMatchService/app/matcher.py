from __future__ import annotations

import asyncio
from uuid import UUID

from .embedding import EmbeddingService
from .index_manager import FAISSIndexManager, OutcomeIndex
from .models import (
    FallbackEvidence,
    MatchedOutcome,
    ReindexResponse,
    VectorMatchRequest,
    VectorMatchResponse,
)
from .repository import AcademicVectorRepository


class VectorMatchService:
    FALLBACK_THRESHOLD = 0.50

    def __init__(
        self,
        embedding_service: EmbeddingService,
        index_manager: FAISSIndexManager,
        repository: AcademicVectorRepository,
        eligible_score: float = 7.0,
    ) -> None:
        self._embedding = embedding_service
        self._indexes = index_manager
        self._repository = repository
        self._eligible_score = eligible_score
        self._build_locks: dict[str, asyncio.Lock] = {}
        self._locks_guard = asyncio.Lock()

    async def match(self, request: VectorMatchRequest) -> VectorMatchResponse:
        index = await self._get_or_build_index(
            request.student_id, request.selected_course_ids
        )
        query = await asyncio.to_thread(
            self._embedding.encode_query, request.job_description
        )
        initial = self._search_outcomes(
            index, query, request.top_k, request.similarity_threshold
        )
        if initial:
            return self._response(
                request, initial, request.similarity_threshold, False, []
            )

        relaxed = self._search_outcomes(
            index, query, request.top_k, self.FALLBACK_THRESHOLD
        )
        evidence = await self._search_fallback_evidence(
            request.student_id, query, request.top_k
        )
        warnings = [
            "No academic outcome met the requested similarity threshold; "
            "the service applied the 0.50 fallback threshold."
        ]
        if evidence:
            warnings.append(
                "Project/internship evidence is returned only from verified "
                "database fields and must not be expanded with invented claims."
            )
        return self._response(
            request,
            relaxed,
            self.FALLBACK_THRESHOLD,
            True,
            evidence,
            warnings,
        )

    async def reindex(
        self, student_id: UUID, selected_course_ids: list[UUID] | None = None
    ) -> ReindexResponse:
        self._indexes.invalidate(student_id)
        index = await self._get_or_build_index(student_id, selected_course_ids)
        return ReindexResponse(
            student_id=student_id, indexed_vector_count=index.index.ntotal
        )

    async def _get_or_build_index(
        self, student_id: UUID, selected_course_ids: list[UUID] | None
    ) -> OutcomeIndex:
        cached = self._indexes.get(student_id, selected_course_ids)
        if cached is not None:
            return cached
        key = self._indexes.cache_key(student_id, selected_course_ids)
        lock = await self._lock_for(key)
        async with lock:
            cached = self._indexes.get(student_id, selected_course_ids)
            if cached is not None:
                return cached
            documents = await asyncio.to_thread(
                self._repository.load_outcomes,
                student_id,
                selected_course_ids,
                self._eligible_score,
            )
            vectors = await asyncio.to_thread(
                self._embedding.encode,
                [document.embedding_text for document in documents],
            )
            return self._indexes.put(
                student_id, selected_course_ids, documents, vectors
            )

    async def _lock_for(self, key: str) -> asyncio.Lock:
        async with self._locks_guard:
            return self._build_locks.setdefault(key, asyncio.Lock())

    def _search_outcomes(
        self,
        outcome_index: OutcomeIndex,
        query,
        top_k: int,
        threshold: float,
    ) -> list[MatchedOutcome]:
        matches: list[MatchedOutcome] = []
        for vector_id, score in self._indexes.search(
            outcome_index.index, query, top_k
        ):
            if score < threshold:
                continue
            item = outcome_index.documents[vector_id]
            matches.append(
                MatchedOutcome(
                    clo_id=item.clo_id,
                    criteria_id=item.criteria_id,
                    source_type=(
                        "CLO" if item.clo_id is not None else "EvaluationCriteria"
                    ),
                    subject_code=item.subject_code,
                    subject_name=item.subject_name,
                    clo_code=item.clo_code,
                    description=item.description,
                    similarity_score=round(score, 6),
                    progression_level=item.progression_level,
                )
            )
        return matches

    async def _search_fallback_evidence(
        self,
        student_id: UUID,
        query,
        top_k: int,
    ) -> list[FallbackEvidence]:
        documents = await asyncio.to_thread(
            self._repository.load_fallback_evidence, student_id
        )
        if not documents:
            return []
        vectors = await asyncio.to_thread(
            self._embedding.encode,
            [document.embedding_text for document in documents],
        )
        index = self._indexes.build_evidence_index(documents, vectors)
        result: list[FallbackEvidence] = []
        for vector_id, score in self._indexes.search(index.index, query, top_k):
            if score < self.FALLBACK_THRESHOLD:
                continue
            item = index.documents[vector_id]
            result.append(
                FallbackEvidence(
                    source_type=item.source_type,
                    source_id=item.source_id,
                    title=item.title,
                    description=item.description,
                    similarity_score=round(score, 6),
                )
            )
        return result

    @staticmethod
    def _response(
        request: VectorMatchRequest,
        outcomes: list[MatchedOutcome],
        threshold: float,
        is_fallback: bool,
        evidence: list[FallbackEvidence],
        warnings: list[str] | None = None,
    ) -> VectorMatchResponse:
        return VectorMatchResponse(
            student_id=request.student_id,
            target_job_matched=bool(outcomes or evidence),
            is_fallback=is_fallback,
            applied_similarity_threshold=threshold,
            matched_outcome_count=len(outcomes),
            top_matched_outcomes=outcomes,
            fallback_evidence=evidence,
            warnings=warnings or [],
        )

