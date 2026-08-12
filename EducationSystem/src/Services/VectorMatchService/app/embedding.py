from __future__ import annotations

from collections.abc import Sequence

import faiss
import numpy as np
from sentence_transformers import SentenceTransformer


class EmbeddingService:
    """Loads Sentence-BERT once and returns unit-length float32 vectors."""

    EXPECTED_DIMENSION = 384

    def __init__(self, model_name: str, device: str = "cpu") -> None:
        self._model = SentenceTransformer(model_name, device=device)
        dimension = self._model.get_sentence_embedding_dimension()
        if dimension != self.EXPECTED_DIMENSION:
            raise ValueError(
                f"Model '{model_name}' emits {dimension} dimensions; "
                f"{self.EXPECTED_DIMENSION} are required."
            )
        self.dimension = dimension

    def encode(self, texts: Sequence[str]) -> np.ndarray:
        if not texts:
            return np.empty((0, self.dimension), dtype=np.float32)
        vectors = self._model.encode(
            list(texts),
            convert_to_numpy=True,
            show_progress_bar=False,
        )
        vectors = np.ascontiguousarray(vectors, dtype=np.float32)
        faiss.normalize_L2(vectors)
        return vectors

    def encode_query(self, text: str) -> np.ndarray:
        return self.encode([text])


