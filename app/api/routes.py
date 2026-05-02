from fastapi import APIRouter, HTTPException

from app.schemas.request import GenerateActionRequest, GenerateActionResponse
from app.services.action_generator import generate_bim_action

router = APIRouter()


@router.post("/generate-action", response_model=GenerateActionResponse)
def generate_action(body: GenerateActionRequest) -> GenerateActionResponse:
    try:
        return generate_bim_action(body)
    except ValueError as e:
        raise HTTPException(status_code=422, detail=str(e))
