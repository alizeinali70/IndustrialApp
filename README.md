# IndustrialApp

An ASP.NET Core (.NET 10) industrial production assistant that integrates a locally running **Ollama** LLM via **Microsoft Semantic Kernel**. Users can ask questions through a Razor Pages UI or a REST API, with every interaction protected by permission checks and tracked via audit logging.

---

## What This Project Is

This project acts as a blueprint for implementing **Clean Architecture** in an AI-driven ecosystem. 

* `Program.cs` only handles app startup and dependency injection.
* Application services focus strictly on business flow.
* Infrastructure components manage AI connectivity and logging wrappers.
* The UI layer remains entirely responsible for presentation and routing input.

### Why This Architecture Matters

* **Decoupled AI Orchestration:** Semantic Kernel acts as the orchestration layer, while Ollama serves as the local model provider. If you decide to swap Ollama for Azure OpenAI or another LLM provider later, you only change the Infrastructure layer.
* **Safer Changes & Maintainability:** Isolating business use cases from external technical APIs ensures the project scales smoothly as more industrial operations are added.

---

## System Architecture

The project is split into explicit structural layers, ensuring the core domain and application use cases carry zero dependencies on external frameworks or infrastructure.

```
IndustrialApp.sln
├── IndustrialApp.Api          # Presentation Layer: Razor Pages UI, REST API, DI wiring
├── IndustrialApp.Application  # Core Logic: Use cases, interfaces (AI, Audit, Security)
├── IndustrialApp.Infrastructure # Infrastructure: Semantic Kernel, Ollama, Logging, Health checks
└── IndustrialApp.Domain         # Domain Layer: Core enterprise rules and business entities

```

### Project Breakdowns

#### 1. IndustrialApp.Api

The entry point and hosting environment for the application.

* **Contains:** `Program.cs` configuration, Razor Pages UI components, static files (`wwwroot`), and Web API controllers.
* **Role:** Handles the direct user interaction, serves the AI assistant web interface, and forwards incoming requests down the pipeline.

#### 2. IndustrialApp.Application

The use-case layer where the application's orchestrational rules live.

* **Contains:** Application interfaces (e.g., `IAiAssistantService`), primary logic (`AiAssistantService`), permission contracts, and request/response DTOs.
* **Role:** Agnostic to technical implementation details (it does not know Ollama or HTTP exist). It simply dictates: *"Verify permissions, then get me an answer for this question."*

#### 3. IndustrialApp.Infrastructure

The boundary layer handling communication with external systems.

* **Contains:** Semantic Kernel setup, direct Ollama HTTP connections, system health checks, audit logging implementations, and mock permission providers.
* **Role:** Satisfies application interfaces with actual technical execution frameworks.

#### 4. IndustrialApp.Domain

The core heart of the business model.

* **Contains:** Basic request signatures (e.g., `AiRequest`).
* **Role:** Currently lightweight and AI-focused. As the system scales to accommodate industrial processes, enterprise domain concepts like `Machine`, `ProductionOrder`, or `QualityIssue` belong here.

---

### Request Execution Flow

```
[ Browser / UI ] ──(1. Ask Question)──> [ IndustrialApp.Api ]
                                                │
                                         (2. Execute Use Case)
                                                ▼
                                    [ IndustrialApp.Application ]
                                                │
                                         (3. Check Permissions)
                                                ▼
                                   [ IndustrialApp.Infrastructure ]
                                                │
                                         (4. Semantic Kernel)
                                                ▼
                                          [ Local Ollama ]

```

1. A user enters a prompt into the AI Assistant interface.
2. The UI pushes the request to the application service pipeline.
3. The Application layer evaluates user clearance rules.
4. The Infrastructure layer packages the prompt into Semantic Kernel, queries the local Ollama instance, logs the interaction via an asynchronous audit provider, and returns the result up to the UI.

---

## Key Features

* **Industrial AI Guardrails:** Driven by Ollama + Semantic Kernel. The backing system instructions restrict the model to deliver factual, concise, and non-destructive answers tailored for industrial plant environments.
* **Permission Guards:** An explicit `IPermissionService` intercepts every execution step before hitting the LLM, making it easy to wire up custom RBAC, Active Directory, or LDAP authorization.
* **High-Performance Audit Logging:** Captures all text interactions via standard `ILogger` targets (using high-performance, source-generated Event ID 1001 log messages).
* **Strict Code Quality:** Engineered with `TreatWarningsAsErrors` enabled, strict nullable reference types (`<Nullable>enable</Nullable>`), and modern Roslyn analyzer rules.

---

## Prerequisites

| Tool | Version / Tag |
| --- | --- |
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0 or later |
| [Ollama](https://ollama.com/) | Latest |
| Default LLM Model | `llama3` |

Download and cache the required model before running the application:

```bash
ollama pull llama3

```

---

## Getting Started

```bash
# 1. Clone the repository
git clone https://github.com/alizeinali70/IndustrialApp.git
cd IndustrialApp

# 2. Launch the local Ollama daemon (must be active before app boot)
ollama serve

# 3. Spin up the ASP.NET Core host
dotnet run --project IndustrialApp.Api

```

* **Web Interface:** Navigate to `https://localhost:5000` (automatically redirects to the AI Assistant dashboard).
* **API Exploration:** Access the Swagger/OpenAPI documentation directly at `https://localhost:5000/swagger` during development.

---

## Configuration

Default properties are set inside `IndustrialApp.Api/appsettings.json`:

```json
{
  "Ai": {
    "ModelId": "llama3",
    "Endpoint": "http://localhost:11434"
  }
}

```

You can customize the underlying architecture at runtime via environment variables:

```bash
export Ai__ModelId=llama3
export Ai__Endpoint=http://localhost:11434

```

---

## REST API Specification

### `POST /api/ai-assistant/ask`

**Request Payload**

```json
{
  "question": "What is the current production status of Line 2?"
}

```

**Response Payload (`200 OK`)**

```json
"The current production status of Line 2 is operational, running at 94% efficiency..."

```

**Error Profiles**

| Status Code | Context / Trigger |
| --- | --- |
| `400 Bad Request` | Provided `question` field is empty or missing. |
| `401 Unauthorized` | Identity context failed the permission guard validation. |
| `500 Internal Server Error` | The application could not establish communication with the Ollama service endpoint. |

---

## Key Dependencies

| Package Reference | Version | Practical Utilization |
| --- | --- | --- |
| `Microsoft.SemanticKernel` | 1.77.0 | Core AI orchestration abstraction layer. |
| `Microsoft.SemanticKernel.Connectors.Ollama` | 1.77.0-alpha | Chat completion integration targeting native Ollama endpoints. |
| `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.9 | Ready-to-configure hook for structural enterprise data persistence. |
| `Swashbuckle.AspNetCore` | 10.2.3 | Automatic Swagger OpenAPI generation and visual testing sandbox. |

---

## License

This project is provided as-is for educational, architectural demonstration, and industrial prototyping purposes.