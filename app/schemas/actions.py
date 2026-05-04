from typing import Literal, Union, Annotated
from pydantic import BaseModel, Field

# All dimensions are in METERS. The Revit plugin converts to feet (× 3.28084).
# Validators below catch accidental foot values slipping through from the LLM.


class Position(BaseModel):
    x: float
    y: float
    z: float = 0.0


# ── Architectural ──────────────────────────────────────────────────────────────

class CreateWallAction(BaseModel):
    action: Literal["create_wall"]
    start: Position
    end: Position
    height: float = Field(default=3.0, gt=0, le=500)
    thickness: float = Field(default=0.2, gt=0, le=10)
    level: str = "Level 1"


class AddWindowAction(BaseModel):
    action: Literal["add_window"]
    wall_id: str | None = None
    position: Position
    width: float = Field(default=1.2, gt=0, le=50)
    height: float = Field(default=1.5, gt=0, le=50)
    count: int = Field(default=1, ge=1, le=100)
    spacing: float | None = Field(default=None, gt=0, le=100)


class AddDoorAction(BaseModel):
    action: Literal["add_door"]
    wall_id: str | None = None
    position: Position
    width: float = Field(default=0.9, gt=0, le=10)
    height: float = Field(default=2.1, gt=0, le=10)


# ── Structural ─────────────────────────────────────────────────────────────────

class CreateColumnAction(BaseModel):
    action: Literal["create_column"]
    position: Position
    height: float = Field(default=3.0, gt=0, le=500)
    section: str = "HEA200"
    level: str = "Level 1"


class CreateBeamAction(BaseModel):
    action: Literal["create_beam"]
    start: Position
    end: Position
    section: str = "IPE300"
    level: str = "Level 1"


# ── Parametric ─────────────────────────────────────────────────────────────────

class CreateGridAction(BaseModel):
    """
    High-level parametric action — expanded into columns + beams by grid_expander.
    Never sent to Revit directly.
    """
    action: Literal["create_grid"]
    origin: Position = Position(x=0, y=0, z=0)
    bays_x: int = Field(default=2, ge=1, le=50)
    bays_y: int = Field(default=2, ge=1, le=50)
    spacing_x: float = Field(default=5.0, gt=0, le=50)
    spacing_y: float = Field(default=5.0, gt=0, le=50)
    floors: int = Field(default=1, ge=1, le=50)
    floor_height: float = Field(default=3.0, gt=0, le=20)
    column_section: str | None = None   # None → auto-selected from Eurocode rules
    beam_section_x: str | None = None  # None → auto-selected from Eurocode rules
    beam_section_y: str | None = None  # None → auto-selected from Eurocode rules
    base_level: str = "Level 1"        # name of the first floor level


# ── Edit actions ───────────────────────────────────────────────────────────────

class DeleteElementAction(BaseModel):
    action: Literal["delete_element"]
    element_id: str   # Revit element ID from bim_context


class MoveElementAction(BaseModel):
    action: Literal["move_element"]
    element_id: str
    delta: Position   # relative displacement in meters


# ── Discriminated union — add new actions here ─────────────────────────────────

BIMAction = Annotated[
    Union[
        CreateWallAction,
        AddWindowAction,
        AddDoorAction,
        CreateColumnAction,
        CreateBeamAction,
        CreateGridAction,
        DeleteElementAction,
        MoveElementAction,
    ],
    Field(discriminator="action"),
]
