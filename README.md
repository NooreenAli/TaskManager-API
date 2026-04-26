# TaskManager API

![CI](https://github.com/NooreenAli/TaskManager-API/actions/workflows/ci.yml/badge.svg)

A production-grade Task Management REST API built with **.NET 9** and **C#**, demonstrating clean layered architecture, event-driven messaging, containerisation, and automated testing. Built as a portfolio project to showcase backend development skills across the full stack of technologies commonly found in modern .NET roles.

---

## Features

- Full **CRUD REST API** with correct HTTP semantics (`200`, `201`, `204`, `404`)
- **Event-driven architecture** — task creation publishes a message to Azure Service Bus, consumed asynchronously by an independent Worker service
- **Clean layered architecture** — Core, API, Infrastructure, and Worker projects with dependencies pointing inward only
- **Entity Framework Core** with Code First migrations and SQL Server
- **AutoMapper** for clean separation between domain models and API DTOs
- **OpenAPI** documentation with interactive Scalar UI
- **Dockerised** — entire stack (API, Worker, SQL Server) runs from a single command
- **Unit tested** with xUnit, Moq, and FluentAssertions
- **CI/CD pipeline** via GitHub Actions — builds and tests on every push

---

## Tech Stack

| Concern | Technology |
|---|---|
| Language | C# / .NET 9 |
| API Framework | ASP.NET Core 9 |
| ORM | Entity Framework Core 9 |
| Database | SQL Server 2022 |
| Messaging | Azure Service Bus |
| Background Processing | .NET Worker Service |
| Object Mapping | AutoMapper 16 |
| API Documentation | OpenAPI + Scalar |
| Containerisation | Docker, Docker Compose |
| Testing | xUnit, Moq, FluentAssertions |
| CI/CD | GitHub Actions |

---

## Architecture

The solution follows a **clean layered architecture** where dependencies only ever point inward. The innermost layer (Core) has zero external dependencies — it knows nothing about databases, HTTP, or messaging.

```
TaskManager/
├── src/
│   ├── TaskManager.Core/             # Domain models, interfaces — no dependencies
│   ├── TaskManager.API/              # ASP.NET Core Web API, controllers, DTOs
│   ├── TaskManager.Infrastructure/   # EF Core, SQL Server, Service Bus publisher
│   └── TaskManager.Worker/           # Background service, Service Bus consumer
├── tests/
│   └── TaskManager.Tests/            # xUnit unit tests
├── docker-compose.yml
├── docker-compose.override.yml       # Local secrets (gitignored)
└── .github/workflows/ci.yml
```

### Layer responsibilities

**Core** — Contains `TaskItem` domain model and the `ITaskRepository` and `IMessagePublisher` interfaces. Pure C# with no framework dependencies. This layer defines *what* the application does without knowing *how*.

**Infrastructure** — Implements the interfaces defined in Core. Contains `AppDbContext`, `TaskRepository` (EF Core), and `ServiceBusMessagePublisher`. All external system concerns live here — swapping SQL Server for PostgreSQL or Service Bus for RabbitMQ would only require changes in this layer.

**API** — ASP.NET Core Web API handling HTTP concerns. Controllers depend on Core interfaces only, never on Infrastructure directly. Uses DTOs to decouple the API contract from the domain model, with AutoMapper handling the translation.

**Worker** — A long-running `BackgroundService` that connects to Azure Service Bus and processes `TaskCreatedMessage` events independently of the API. Demonstrates the decoupling benefit of event-driven design — the API fires and forgets, the Worker handles downstream processing.

### Request flow

```
POST /api/tasks
      │
      ▼
TasksController
      │
      ├──▶ ITaskRepository.CreateAsync()  ──▶ SQL Server (via EF Core)
      │
      └──▶ IMessagePublisher.PublishAsync() ──▶ Azure Service Bus
                                                        │
                                                        ▼
                                                  [task-created queue]
                                                        │
                                                        ▼
                                                  TaskWorker
                                          (processes message asynchronously)
```

---

## API Endpoints

| Method | Endpoint | Description | Success Response |
|---|---|---|---|
| `GET` | `/api/tasks` | Get all tasks | `200 OK` |
| `GET` | `/api/tasks/{id}` | Get task by ID | `200 OK` / `404 Not Found` |
| `POST` | `/api/tasks` | Create a new task | `201 Created` |
| `PUT` | `/api/tasks/{id}` | Update an existing task | `200 OK` / `404 Not Found` |
| `DELETE` | `/api/tasks/{id}` | Delete a task | `204 No Content` / `404 Not Found` |

### Example request — Create a task

```http
POST /api/tasks
Content-Type: application/json

{
  "title": "Write unit tests",
  "description": "Cover all controller actions with xUnit and Moq"
}
```

### Example response

```json
{
  "id": 1,
  "title": "Write unit tests",
  "description": "Cover all controller actions with xUnit and Moq",
  "isCompleted": false,
  "createdAt": "2026-04-22T21:00:00Z"
}
```

---

## Getting Started

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET 9 SDK](https://dotnet.microsoft.com/download) (for running locally outside Docker)
- An [Azure account](https://azure.microsoft.com/free/) with a Service Bus namespace and a queue named `task-created`

### Running with Docker Compose

Clone the repository:

```bash
git clone https://github.com/NooreenAli/TaskManager-API.git
cd TaskManager-API
```

Create a `docker-compose.override.yml` file in the root with your real Service Bus connection string (this file is gitignored and never committed):

```yaml
services:
  api:
    environment:
      ServiceBus__ConnectionString: "your-service-bus-connection-string"
  worker:
    environment:
      ServiceBus__ConnectionString: "your-service-bus-connection-string"
```

Start the entire stack:

```bash
docker compose up --build
```

This starts three containers:
- **SQL Server 2022** on port `1433`
- **API** on port `8080` — EF Core migrations run automatically on startup
- **Worker** — connects to Azure Service Bus and begins listening

The API will be available at:

```
http://localhost:8080/scalar/v1
```

To stop the stack:

```bash
docker compose down
```

To stop without removing containers (faster restart):

```bash
docker compose stop
docker compose start
```

### Running locally in Visual Studio

1. Ensure Docker Desktop is running (SQL Server runs in a container)
2. Create `src/TaskManager.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=TaskManagerDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
  },
  "ServiceBus": {
    "ConnectionString": "your-service-bus-connection-string",
    "QueueName": "task-created"
  }
}
```

3. Create `src/TaskManager.Worker/appsettings.Development.json`:

```json
{
  "ServiceBus": {
    "ConnectionString": "your-service-bus-connection-string",
    "QueueName": "task-created"
  }
}
```

4. Apply EF Core migrations:

```bash
dotnet ef database update --project src/TaskManager.Infrastructure --startup-project src/TaskManager.API
```

5. In Visual Studio, right-click the solution → **Properties** → **Multiple startup projects** → set both `TaskManager.API` and `TaskManager.Worker` to **Start**

6. Press **F5**

---

## Running Tests

```bash
dotnet test
```

The test suite uses **xUnit** for test structure, **Moq** to mock `ITaskRepository` and `IMessagePublisher` interfaces, and **FluentAssertions** for readable assertions. Tests run in complete isolation with no database or Service Bus dependencies — all external systems are mocked.

```
Passed! - Failed: 0, Passed: 10, Skipped: 0, Total: 10
```

### What is tested

| Test | Description |
|---|---|
| `GetAll_WhenTasksExist_ReturnsOkWithAllTasks` | Returns 200 with correct task list |
| `GetAll_WhenNoTasksExist_ReturnsOkWithEmptyList` | Returns 200 with empty list |
| `GetById_WhenTaskExists_ReturnsOkWithTask` | Returns 200 with correct task |
| `GetById_WhenTaskDoesNotExist_ReturnsNotFound` | Returns 404 for missing task |
| `Create_WithValidData_ReturnsCreatedAtActionWithTask` | Returns 201 and verifies Service Bus publish |
| `Create_VerifiesRepositoryIsCalledOnce` | Asserts repository is called exactly once |
| `Update_WhenTaskExists_ReturnsOkWithUpdatedTask` | Returns 200 with updated values |
| `Update_WhenTaskDoesNotExist_ReturnsNotFound` | Returns 404 for missing task |
| `Delete_WhenTaskExists_ReturnsNoContent` | Returns 204 on success |
| `Delete_WhenTaskDoesNotExist_ReturnsNotFound` | Returns 404 for missing task |

---

## CI/CD

Every push to any branch triggers a GitHub Actions workflow that:

1. Checks out the code
2. Installs the .NET 9 SDK
3. Restores NuGet packages
4. Builds the solution in Release configuration
5. Runs the full test suite

A green badge on this README confirms the current state of the `main` branch.

---

## Design Decisions

**Why separate projects instead of one?**
Enforces architectural boundaries at the compiler level. Infrastructure cannot reference API, Core cannot reference anything. This makes the codebase easier to maintain, test, and extend.

**Why the Repository pattern?**
Abstracts data access behind an interface, making controllers testable without a database and making the underlying data store swappable without touching business logic.

**Why event-driven messaging for task creation?**
Demonstrates decoupling — the API doesn't need to know what happens after a task is created. The Worker can be scaled, replaced, or extended independently. In a real application this pattern is used for emails, notifications, search indexing, and audit logging.

**Why DTOs instead of exposing domain models directly?**
Gives full control over the API contract independently of the internal domain model. Prevents accidental exposure of internal fields and allows input validation rules to differ from domain rules.

**Why xUnit over NUnit?**
xUnit creates a fresh test class instance per test, enforcing isolation by design. It is also the testing framework used internally by Microsoft for .NET itself, and appears more frequently in modern .NET job descriptions.

**Why Scalar over Swagger UI?**
Scalar is the modern replacement for Swagger UI, pairing with .NET 9's first-party `Microsoft.AspNetCore.OpenApi` package rather than the third-party Swashbuckle dependency.