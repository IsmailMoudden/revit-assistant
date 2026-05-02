from pydantic import BaseModel, Field
from app.schemas.actions import BIMAction


class GenerateActionRequest(BaseModel):
    instruction: str


class GenerateActionResponse(BaseModel):
    instruction: str
    action: BIMAction  # ← the ONLY field the Revit plugin should read
    raw_llm_output: str = Field(
        description="Raw LLM string. For debugging/logging only. Never use in plugin logic."
    )
