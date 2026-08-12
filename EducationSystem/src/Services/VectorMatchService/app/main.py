from __future__ import annotations

import hmac
import logging
from contextlib import asynccontextmanager
from typing import Annotated
from uuid import UUID

import pyodbc
from fastapi import Depends, FastAPI, Header, HTTPException, Query, Request, status
from fastapi.responses import JSONResponse

from .config import Settings
from .embedding import EmbeddingService
from .index_manager import FAISSIndexManager
from .matcher import VectorMatchService
from .models import ReindexResponse, VectorMatchRequest, VectorMatchResponse
from .repository import SqlServerAcademicVectorRepository


logger = logging.getLogger("uvicorn.error")


@asynccontextmanager
async def lifespan(app: FastAPI):
    settings = Settings.from_environment()
    embedding = EmbeddingService(settings.model_name, settings.model_device)
    indexes = FAISSIndexManager(
        embedding.dimension, settings.index_ttl_seconds
    )
    repository = SqlServerAcademicVectorRepository(
        settings.academic_database_connection_string,
        settings.career_database_connection_string,
    )
    app.state.settings = settings
    app.state.matcher = VectorMatchService(
        embedding, indexes, repository, settings.eligible_gpa_threshold
    )
    yield


app = FastAPI(
    title="EducationSystem Vector Match Service",
    version="1.0.0",
    lifespan=lifespan,
)


@app.exception_handler(pyodbc.Error)
async def database_error_handler(
    request: Request, exception: pyodbc.Error
) -> JSONResponse:
    logger.exception(
        "VectorMatchService could not read its SQL Server data",
        exc_info=exception,
    )
    # Do not leak server, login, SQL text, or driver diagnostics to callers.
    return JSONResponse(
        status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
        content={
            "title": "Academic vector data is unavailable.",
            "detail": (
                "The service could not read the required academic/career data. "
                "Check database availability and vector_match_reader grants."
            ),
        },
    )


def authorize(
    request: Request,
    x_internal_api_key: Annotated[str | None, Header()] = None,
) -> None:
    expected = request.app.state.settings.internal_api_key
    if expected is None:
        return
    if x_internal_api_key is None or not hmac.compare_digest(
        x_internal_api_key, expected
    ):
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="A valid X-Internal-Api-Key is required.",
        )


def matcher(request: Request) -> VectorMatchService:
    return request.app.state.matcher


@app.get("/health")
async def health() -> dict[str, str]:
    return {"status": "Healthy"}


@app.post(
    "/api/ai/vector-match/clos",
    response_model=VectorMatchResponse,
    dependencies=[Depends(authorize)],
)
async def match_clos(
    payload: VectorMatchRequest,
    service: Annotated[VectorMatchService, Depends(matcher)],
) -> VectorMatchResponse:
    return await service.match(payload)


@app.post(
    "/api/ai/vector-match/students/{student_id}/reindex",
    response_model=ReindexResponse,
    dependencies=[Depends(authorize)],
)
async def reindex_student(
    student_id: UUID,
    service: Annotated[VectorMatchService, Depends(matcher)],
    selected_course_ids: Annotated[list[UUID] | None, Query()] = None,
) -> ReindexResponse:
    return await service.reindex(student_id, selected_course_ids)
