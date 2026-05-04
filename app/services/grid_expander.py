"""
Parametric grid expander — pure Python, no LLM.

Takes a CreateGridAction and returns a flat list of atomic BIM actions:
  - N_columns_x × N_columns_y × floors  create_column actions
  - Primary beams along X per floor
  - Secondary beams along Y per floor

All coordinates in METERS. Plugin converts to feet.
"""

from app.schemas.actions import (
    CreateColumnAction, CreateBeamAction, Position, BIMAction
)
from app.services.eurocode import (
    select_column_section, select_beam_section, validate_spans
)


def expand_grid(
    origin_x: float,
    origin_y: float,
    bays_x: int,
    bays_y: int,
    spacing_x: float,
    spacing_y: float,
    floors: int,
    floor_height: float,
    base_level: str,
    column_section: str | None,
    beam_section_x: str | None,
    beam_section_y: str | None,
) -> tuple[list[BIMAction], list[str]]:
    """
    Returns (actions, warnings).
    warnings is a list of engineering notes to surface to the user.
    """
    warnings = validate_spans(spacing_x, spacing_y)

    # Auto-select sections from Eurocode if not specified
    col_sec  = column_section  or select_column_section(floor_height)
    beam_x   = beam_section_x  or select_beam_section(spacing_x)
    beam_y   = beam_section_y  or select_beam_section(spacing_y)

    n_cols_x = bays_x + 1   # number of column lines in X
    n_cols_y = bays_y + 1   # number of column lines in Y

    actions: list[BIMAction] = []

    for floor in range(floors):
        z_base  = floor * floor_height          # column base z
        z_top   = z_base + floor_height         # beam z (top of columns)
        level   = f"{base_level} {floor + 1}" if floors > 1 else base_level

        # ── Columns ───────────────────────────────────────────────────────────
        for ix in range(n_cols_x):
            for iy in range(n_cols_y):
                x = origin_x + ix * spacing_x
                y = origin_y + iy * spacing_y
                actions.append(CreateColumnAction(
                    action="create_column",
                    position=Position(x=x, y=y, z=z_base),
                    height=floor_height,
                    section=col_sec,
                    level=level,
                ))

        # ── Primary beams — along X (connecting columns in X direction) ───────
        for iy in range(n_cols_y):
            y = origin_y + iy * spacing_y
            for ix in range(bays_x):
                x_start = origin_x + ix * spacing_x
                x_end   = origin_x + (ix + 1) * spacing_x
                actions.append(CreateBeamAction(
                    action="create_beam",
                    start=Position(x=x_start, y=y, z=z_top),
                    end=Position(x=x_end,   y=y, z=z_top),
                    section=beam_x,
                    level=level,
                ))

        # ── Secondary beams — along Y (connecting columns in Y direction) ─────
        for ix in range(n_cols_x):
            x = origin_x + ix * spacing_x
            for iy in range(bays_y):
                y_start = origin_y + iy * spacing_y
                y_end   = origin_y + (iy + 1) * spacing_y
                actions.append(CreateBeamAction(
                    action="create_beam",
                    start=Position(x=x, y=y_start, z=z_top),
                    end=Position(x=x, y=y_end,   z=z_top),
                    section=beam_y,
                    level=level,
                ))

    return actions, warnings
