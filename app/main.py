from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from app.api.routes import router
from app.core.config import settings

app = FastAPI(
    title="AI BIM Assistant",
    description="Converts natural language into structured Revit actions",
    version="0.1.0",
)

origins = [o.strip() for o in settings.allowed_origins.split(",")]

app.add_middleware(
    CORSMiddleware,
    allow_origins=origins,
    allow_methods=["POST", "GET"],
    allow_headers=["Content-Type", "Authorization"],
)

app.include_router(router, prefix="/api/v1")


@app.get("/health")
def health():
    return {"status": "ok"}
