from __future__ import annotations

import hashlib
import threading
import time
from dataclasses import dataclass
from uuid import UUID

import faiss
import numpy as np

from .models import EvidenceDocument, OutcomeDocument


@dataclass(slots=True)
class OutcomeIndex:
    index: faiss.IndexIDMap2
    documents: dict[int, OutcomeDocument]
    created_at: float


@dataclass(slots=True)
class EvidenceIndex:
    index: faiss.IndexIDMap2
    documents: dict[int, EvidenceDocument]


class FAISSIndexManager:
    """Owns per-student FAISS indexes and their non-vector metadata."""

    def __init__(self, dimension: int, ttl_seconds: int = 900) -> None:
        self.dimension = dimension
        self.ttl_seconds = ttl_seconds
        self._indexes: dict[str, OutcomeIndex] = {}
        self._lock = threading.RLock()

    @staticmethod
    def cache_key(student_id: UUID, selected_course_ids: list[UUID] | None) -> str:
        selected = ",".join(
            sorted(str(value) for value in (selected_course_ids or []))
        )
        return f"{student_id}:{selected}"

    def get(
        self, student_id: UUID, selected_course_ids: list[UUID] | None
    ) -> OutcomeIndex | None:
        key = self.cache_key(student_id, selected_course_ids)
        with self._lock:
            value = self._indexes.get(key)
            if value is None:
                return None
            if time.monotonic() - value.created_at > self.ttl_seconds:
                del self._indexes[key]
                return None
            return value

    def put(
        self,
        student_id: UUID,
        selected_course_ids: list[UUID] | None,
        documents: list[OutcomeDocument],
        vectors: np.ndarray,
    ) -> OutcomeIndex:
        index, metadata = self._build_outcome_index(documents, vectors)
        value = OutcomeIndex(index, metadata, time.monotonic())
        key = self.cache_key(student_id, selected_course_ids)
        with self._lock:
            self._indexes[key] = value
        return value

    def invalidate(self, student_id: UUID) -> None:
        prefix = f"{student_id}:"
        with self._lock:
            for key in [key for key in self._indexes if key.startswith(prefix)]:
                del self._indexes[key]

    def build_evidence_index(
        self, documents: list[EvidenceDocument], vectors: np.ndarray
    ) -> EvidenceIndex:
        self._validate_vectors(documents, vectors)
        index = faiss.IndexIDMap2(faiss.IndexFlatIP(self.dimension))
        ids = np.arange(1, len(documents) + 1, dtype=np.int64)
        if len(documents):
            index.add_with_ids(vectors, ids)
        return EvidenceIndex(
            index=index,
            documents={
                int(vector_id): document
                for vector_id, document in zip(ids, documents, strict=True)
            },
        )

    @staticmethod
    def search(
        index: faiss.IndexIDMap2, query_vector: np.ndarray, top_k: int
    ) -> list[tuple[int, float]]:
        if index.ntotal == 0:
            return []
        limit = min(top_k, index.ntotal)
        scores, ids = index.search(query_vector, limit)
        return [
            (int(vector_id), float(score))
            for vector_id, score in zip(ids[0], scores[0], strict=True)
            if vector_id != -1
        ]

    def _build_outcome_index(
        self, documents: list[OutcomeDocument], vectors: np.ndarray
    ) -> tuple[faiss.IndexIDMap2, dict[int, OutcomeDocument]]:
        self._validate_vectors(documents, vectors)
        index = faiss.IndexIDMap2(faiss.IndexFlatIP(self.dimension))
        metadata: dict[int, OutcomeDocument] = {}
        ids: list[int] = []
        for document in documents:
            vector_id = self._outcome_vector_id(document, metadata)
            metadata[vector_id] = document
            ids.append(vector_id)
        if ids:
            index.add_with_ids(vectors, np.asarray(ids, dtype=np.int64))
        return index, metadata

    @staticmethod
    def _outcome_vector_id(
        document: OutcomeDocument, existing: dict[int, OutcomeDocument]
    ) -> int:
        # SQL bigint CLO IDs map directly to FAISS int64 IDs.
        if document.clo_id is not None:
            if document.clo_id <= 0:
                raise ValueError("CLO IDs must be positive.")
            if document.clo_id in existing:
                raise ValueError(f"Duplicate CLO ID {document.clo_id}.")
            return document.clo_id

        # EvaluationCriteria uses UUID, which FAISS cannot store directly.
        # Use a deterministic negative int64 and retain the UUID in metadata.
        if document.criteria_id is None:
            raise ValueError("An outcome requires either clo_id or criteria_id.")
        digest = hashlib.blake2b(
            document.criteria_id.bytes, digest_size=8
        ).digest()
        hashed_value = int.from_bytes(digest, "big", signed=False) & ((2**63) - 1)
        candidate = -hashed_value
        if candidate == 0:
            candidate = -1
        while candidate in existing:
            candidate += 1
            if candidate >= 0:
                candidate = -(2**63) + 1
        return candidate

    def _validate_vectors(
        self,
        documents: list[OutcomeDocument] | list[EvidenceDocument],
        vectors: np.ndarray,
    ) -> None:
        if vectors.dtype != np.float32:
            raise ValueError("FAISS vectors must be float32.")
        if vectors.shape != (len(documents), self.dimension):
            raise ValueError(
                f"Expected vector shape {(len(documents), self.dimension)}, "
                f"received {vectors.shape}."
            )

