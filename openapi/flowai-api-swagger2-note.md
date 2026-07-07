# FlowAI OpenAPI export note

FlowAI Business Platform exposes OpenAPI JSON during local development.

Local OpenAPI endpoints:

- `https://localhost:7168/openapi/v1.json`
- `https://localhost:7168/swagger/v1/swagger.json`
- `https://localhost:7168/swagger`
- `https://localhost:7168/health`

The `/api` routes require the `X-FlowAI-Api-Key` header. The OpenAPI, Swagger UI, and health endpoints are intentionally left unprotected so they can be inspected during local or Dev Tunnel testing.

Power Platform Custom Connector may require an OpenAPI 2.0, also called Swagger 2.0, document depending on the import path being used. If the exported file is OpenAPI 3.x, convert it to Swagger 2.0 before importing.

Manual export example:

```powershell
Invoke-WebRequest `
  -Uri "https://localhost:7168/swagger/v1/swagger.json" `
  -OutFile "openapi/flowai-api.json" `
  -SkipCertificateCheck
```

Keep generated exports reviewed before committing. Do not include real API keys, Dev Tunnel secrets, tenant identifiers, or private endpoint values in committed OpenAPI files.
