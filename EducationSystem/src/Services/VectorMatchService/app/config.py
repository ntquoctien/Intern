from __future__ import annotations

import os
from dataclasses import dataclass
from pathlib import Path

from dotenv import load_dotenv


@dataclass(frozen=True, slots=True)
class Settings:
    academic_database_connection_string: str
    career_database_connection_string: str
    model_name: str
    model_device: str
    eligible_gpa_threshold: float
    index_ttl_seconds: int
    internal_api_key: str | None

    @classmethod
    def from_environment(cls) -> "Settings":
        load_dotenv(Path(__file__).resolve().parents[1] / ".env")
        legacy_connection = os.getenv(
            "VECTOR_DB_CONNECTION_STRING", ""
        ).strip()
        return cls(
            academic_database_connection_string=os.getenv(
                "VECTOR_ACADEMIC_DB_CONNECTION_STRING", legacy_connection
            ).strip(),
            career_database_connection_string=os.getenv(
                "VECTOR_CAREER_DB_CONNECTION_STRING", legacy_connection
            ).strip(),
            model_name=os.getenv(
                "VECTOR_MODEL_NAME",
                "sentence-transformers/multi-qa-MiniLM-L6-cos-v1",
            ).strip(),
            model_device=os.getenv("VECTOR_MODEL_DEVICE", "cpu").strip(),
            eligible_gpa_threshold=float(
                os.getenv("VECTOR_ELIGIBLE_GPA_THRESHOLD", "7.0")
            ),
            index_ttl_seconds=max(
                1, int(os.getenv("VECTOR_INDEX_TTL_SECONDS", "900"))
            ),
            internal_api_key=(
                os.getenv("VECTOR_INTERNAL_API_KEY", "").strip() or None
            ),
        )
