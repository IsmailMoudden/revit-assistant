# Contributing

Start with the README and configuration guide. For a bug report, include the
Revit version, provider/model name, steps to reproduce and expected behavior.
Use a minimal example model and redact credentials and private project information.

Keep pull requests focused. Explain the behavior change and how you checked it.
Changes to action contracts must update both Python schemas and C# payloads/handlers.
Coordinates use meters; element identifiers use strings on the wire.

Run `python -m unittest discover -s tests -v` for backend checks. Build the add-in
on Windows and verify geometry changes in Revit 2024 before claiming runtime support.
Do not commit `.env`, credentials, build outputs or third-party Revit binaries.

Use concise commit subjects such as `fix: preserve column base elevation` or
`docs: explain local provider setup`. Describe the change itself.

By contributing, you agree that your contributions are distributed under the
project's MIT license. Be respectful and constructive in discussions and reviews.
