# IndustrialApp

An ASP.NET Core (.NET 10) industrial production assistant that integrates a locally-running **Ollama** LLM via **Microsoft Semantic Kernel**. Users can ask questions through a Razor Pages UI or REST API; every interaction is permission-checked and audit-logged.

---

## Architecture

```
IndustrialApp.sln
├── IndustrialApp.Api            # ASP.NET Core host – controllers, Razor Pages, DI wiring
├── IndustrialApp.Application    # Use-cases & service interfaces (Ai, Audit, Security)
├── IndustrialApp.Domain         # Domain models (e.g. AiRequest)
└── IndustrialApp.Infrastructure # Concrete implementations (AuditLogService → ILogger)
```

The solution follows **Clean Architecture**: the domain and application layers have zero infrastructure dependencies.

---

## Prerequisites

| Tool | Version |
|------|---------|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0 or later |
| [Ollama](https://ollama.com/) | latest |
| Ollama model | `qwen2.5-coder:14b` (default) |

Pull the default model once:

```bash
ollama pull qwen2.5-coder:14b
```

---

## Getting Started

```bash
# 1. Clone
git clone https://github.com/alizeinali70/IndustrialApp.git
cd IndustrialApp

# 2. Start Ollama (must be running before the app)
ollama serve

# 3. Run the API
dotnet run --project IndustrialApp.Api
```

Open your browser at **https://localhost:5000** — you will be redirected to the AI Assistant page.  
Swagger UI is available at **https://localhost:5000/swagger** in Development mode.

---

## Configuration

Settings live in `IndustrialApp.Api/appsettings.json`:

```json
{
  "Ai": {
    "ModelId": "qwen2.5-coder:14b",
    "Endpoint": "http://localhost:11434"
  }
}
```

Override either value via environment variables or `appsettings.Development.json`:

```bash
export Ai__ModelId=llama3
export Ai__Endpoint=http://localhost:11434
```

---

## REST API

### `POST /api/ai-assistant/ask`

**Request body**
```json
{ "question": "What is the current production status?" }
```

**Response** – `200 OK`
```
"The current production status is …"
```

**Error responses**

| Status | Reason |
|--------|--------|
| `400 Bad Request` | Question is empty |
| `401 Unauthorized` | User not permitted to use AI |
| `500 Internal Server Error` | Ollama is not running |

---

## Key Features

- **AI Assistant** – Powered by Ollama + Semantic Kernel. The system prompt constrains the model to factual, concise, non-destructive answers suitable for industrial environments.
- **Permission Guard** – `IPermissionService` is evaluated before every AI call; swap the implementation to plug in your own RBAC/LDAP logic.
- **Audit Logging** – Every question and answer is logged via `ILogger` (Event ID 1001) using high-performance source-generated log messages.
- **Strict Code Quality** – All warnings treated as errors, latest Roslyn analyzers enabled, nullable reference types on.

---

## Project Dependencies

| Package | Purpose |
|---------|---------|
| `Microsoft.SemanticKernel` 1.77.0 | LLM orchestration |
| `Microsoft.SemanticKernel.Connectors.Ollama` 1.77.0-alpha | Ollama chat-completion connector |
| `Microsoft.EntityFrameworkCore.SqlServer` 10.0.9 | (Ready for data persistence) |
| `Swashbuckle.AspNetCore` 10.2.3 | Swagger / OpenAPI UI |

---

## License

This project is provided as-is for educational and industrial prototyping purposes.
