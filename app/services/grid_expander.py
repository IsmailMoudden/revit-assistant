"""
Parametric grid expander — pure Python, no LLM.

Takes a CreateGridAction and returns a flat list of atomic BIM actions:
  - N_columns_x × N_columns_y × floors  create_column actions
  - Primary beams along X per floor
  - Secondary beams along Y per floor

All coordinates in METERS. Plugin converts to feet.
"""

from app.schemas.actions import CreateColumnAction, CreateBeamAction, Position, BIMAction
from app.services.eurocode import (
    select_column_section, select_beam_section, validate_spans,
    best_available_column, best_available_beam,
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
    loaded_column_families: list[str] | None = None,
    loaded_beam_families: list[str] | None = None,
) -> tuple[list[BIMAction], list[str]]:
    """
    Returns (actions, warnings).
    warnings surfaces engineering notes and family substitutions to the plugin UI.
    """
    warnings = validate_spans(spacing_x, spacing_y)

    # Step 1 — Eurocode ideal sections
    ideal_col    = column_section   or select_column_section(floor_height)
    ideal_beam_x = beam_section_x   or select_beam_section(spacing_x)
    ideal_beam_y = beam_section_y   or select_beam_section(spacing_y)

    # Step 2 — Map to best available loaded family (avoids "family not loaded" crash)
    loaded_cols  = loaded_column_families or []
    loaded_beams = loaded_beam_families   or []

    col_sec,  w1 = best_available_column(ideal_col,    loaded_cols)
    beam_x,   w2 = best_available_beam(ideal_beam_x,   loaded_beams)
    beam_y,   w3 = best_available_beam(ideal_beam_y,   loaded_beams)

    for w in (w1, w2, w3):
        if w:
            warnings.append(w)

    n_cols_x = bays_x + 1
    n_cols_y = bays_y + 1
    actions: list[BIMAction] = []

    for floor in range(floors):
        z_base = floor * floor_height
        z_top  = z_base + floor_height
        level  = f"{base_level} {floor + 1}" if floors > 1 else base_level

        # Columns
        for ix in range(n_cols_x):
            for iy in range(n_cols_y):
                actions.append(CreateColumnAction(
                    action="create_column",
                    position=Position(x=origin_x + ix * spacing_x, y=origin_y + iy * spacing_y, z=z_base),
                    height=floor_height,
                    section=col_sec,
                    level=level,
                ))

        # Primary beams — X direction
        for iy in range(n_cols_y):
            y = origin_y + iy * spacing_y
            for ix in range(bays_x):
                actions.append(CreateBeamAction(
                    action="create_beam",
                    start=Position(x=origin_x + ix * spacing_x,       y=y, z=z_top),
                    end=Position(  x=origin_x + (ix + 1) * spacing_x, y=y, z=z_top),
                    section=beam_x,
                    level=level,
                ))

        # Secondary beams — Y direction
        for ix in range(n_cols_x):
            x = origin_x + ix * spacing_x
            for iy in range(bays_y):
                actions.append(CreateBeamAction(
                    action="create_beam",
                    start=Position(x=x, y=origin_y + iy * spacing_y,       z=z_top),
                    end=Position(  x=x, y=origin_y + (iy + 1) * spacing_y, z=z_top),
                    section=beam_y,
                    level=level,
                ))

    return actions, warnings
