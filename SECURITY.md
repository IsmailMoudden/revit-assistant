# Security

Do not post credentials or confidential BIM context in public issues. Report
security concerns privately using GitHub's vulnerability reporting feature when
enabled, or contact the repository owner through their GitHub profile.

Use your own provider key, keep `.env` outside version control and rotate exposed
credentials. A shared backend should use HTTPS, `BACKEND_API_KEY` and request limits.
The service does not currently provide per-user accounts or quotas.
