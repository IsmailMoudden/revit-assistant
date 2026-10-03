import secrets
from fastapi import APIRouter, HTTPException, Header
from openai import APIError, APITimeoutError
from app.core.config import settings

from app.schemas.request import GenerateActionRequest, GenerateActionResponse
from app.services.action_generator import generate_bim_action

router = APIRouter()


@router.post("/generate-action", response_model=GenerateActionResponse)
def generate_action(body: GenerateActionRequest, authorization: str | None = Header(default=None)) -> GenerateActionResponse:
    key = settings.backend_api_key.get_secret_value()
    if key and not secrets.compare_digest(authorization or "", f"Bearer {key}"):
        raise HTTPException(status_code=401, detail="Invalid backend access token")
    try:
        return generate_bim_action(body)
    except ValueError as e:
        raise HTTPException(status_code=422, detail="Invalid action response or model configuration") from e
    except APITimeoutError as e:
        raise HTTPException(status_code=504, detail="Model provider timed out") from e
    except APIError as e:
        raise HTTPException(status_code=502, detail="Model provider request failed") from e
