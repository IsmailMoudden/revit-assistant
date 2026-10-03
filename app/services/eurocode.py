"""
Heuristic section suggestions for preliminary model geometry.
These tables do not perform Eurocode checks or establish structural adequacy.
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


# ── Section preference order — from lightest to heaviest ──────────────────────
# Used to pick the best available loaded family
_COLUMN_PREFERENCE = ["HEA160","HEA200","HEA220","HEA240","HEA260","HEB200","HEB240","HEB260","HEB300","HEB320","HEB360"]
_BEAM_PREFERENCE   = ["IPE160","IPE200","IPE220","IPE240","IPE270","IPE300","IPE330","IPE360","IPE400","IPE450","IPE500"]


def best_available_column(required: str, loaded: list[str]) -> tuple[str, str | None]:
    """
    Returns (section_to_use, warning_or_None).
    If required is loaded → use it.
    If not → find the next heavier available section.
    If nothing available → use required anyway and warn.
    """
    if not loaded:
        return required, None  # no context provided — use as-is, plugin must handle

    if required in loaded:
        return required, None

    # find index of required in preference list, pick next heavier that is loaded
    try:
        idx = _COLUMN_PREFERENCE.index(required)
    except ValueError:
        # unknown section — use first available
        return loaded[0], f"Section '{required}' not in known list. Using '{loaded[0]}' (first loaded)."

    for candidate in _COLUMN_PREFERENCE[idx:]:
        if candidate in loaded:
            return candidate, f"'{required}' not loaded in project. Using '{candidate}' (next heavier available)."

    # nothing heavier available — use heaviest loaded
    fallback = loaded[-1]
    return fallback, f"'{required}' not loaded. Using '{fallback}' (heaviest available). Verify adequacy."


def best_available_beam(required: str, loaded: list[str]) -> tuple[str, str | None]:
    if not loaded:
        return required, None

    if required in loaded:
        return required, None

    try:
        idx = _BEAM_PREFERENCE.index(required)
    except ValueError:
        return loaded[0], f"Section '{required}' not in known list. Using '{loaded[0]}' (first loaded)."

    for candidate in _BEAM_PREFERENCE[idx:]:
        if candidate in loaded:
            return candidate, f"'{required}' not loaded in project. Using '{candidate}' (next heavier available)."

    fallback = loaded[-1]
    return fallback, f"'{required}' not loaded. Using '{fallback}' (heaviest available). Verify adequacy."


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
