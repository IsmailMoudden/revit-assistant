"""
Eurocode-based section selection rules (EN 1993-1-1 steel, EN 1992-1-1 concrete).
All spans in meters. Returns standard European steel section names.
"""

# ── Column sections — based on floor height ───────────────────────────────────
# Rule: taller columns need heavier sections (buckling)
_COLUMN_RULES = [
    (3.5,  "HEA160"),
    (5.0,  "HEA200"),
    (7.0,  "HEA240"),
    (9.0,  "HEB260"),
    (12.0, "HEB300"),
    (float("inf"), "HEB360"),
]

# ── Beam sections — based on span length ──────────────────────────────────────
# Rule: L/15 depth minimum for ULS, standard IPE series
_BEAM_RULES = [
    (3.0,  "IPE160"),
    (4.5,  "IPE200"),
    (6.0,  "IPE270"),
    (7.5,  "IPE300"),
    (9.0,  "IPE360"),
    (11.0, "IPE400"),
    (13.0, "IPE450"),
    (float("inf"), "IPE500"),
]


def select_column_section(height_m: float) -> str:
    for threshold, section in _COLUMN_RULES:
        if height_m <= threshold:
            return section
    return "HEB360"


def select_beam_section(span_m: float) -> str:
    for threshold, section in _BEAM_RULES:
        if span_m <= threshold:
            return section
    return "IPE500"


def validate_spans(spacing_x: float, spacing_y: float) -> list[str]:
    """Return a list of engineering warnings for the given bay spacings."""
    warnings = []
    max_span = max(spacing_x, spacing_y)
    if max_span > 12.0:
        warnings.append(
            f"Span of {max_span}m exceeds recommended 12m for steel frames (EN 1993). "
            "Consider intermediate columns."
        )
    if max_span > 8.0:
        warnings.append(
            f"Span of {max_span}m may require a detailed deflection check (L/300 serviceability limit)."
        )
    if min(spacing_x, spacing_y) < 2.5:
        warnings.append("Bay spacing below 2.5m is unusual — verify clearance requirements.")
    return warnings
