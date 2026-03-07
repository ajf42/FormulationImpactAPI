# Architectural Decision Log

This file is a permanent record of significant architectural decisions made during
the design and build of FormulationImpactApi. It is not a task list or a README.
Each entry captures what was chosen, why it was chosen, and what was rejected.
Update this file whenever a new significant decision is made.

---

## 2026-03-07 — .NET 10 over .NET 8

**Decision:** Use .NET 10 as the target framework for FormulationImpactApi.

**Rationale:** .NET 8 reaches end of support in November 2026, making it the wrong foundation for a new project. .NET 10 is the current LTS release supported through November 2028 and is what Microsoft recommends for new production applications. Starting on a version with a known expiry would require a framework migration before this project reaches any meaningful production lifetime.

**Alternatives considered:** .NET 9 was rejected because it is an STS (Standard Term Support) release with an 18-month support window rather than the three-year LTS window. An STS release is appropriate for teams that ship frequently and upgrade continuously; it is not appropriate for a project that may remain in its initial state for an extended period.

---

## 2026-03-07 — Controller-Based API over Minimal API

**Decision:** Use the traditional controller pattern with classes inheriting from `ControllerBase` rather than Minimal API route registration.

**Rationale:** Minimal APIs are appropriate for lightweight microservices and simple function-style endpoints, but controllers are the established enterprise convention. PPG and similar large manufacturing companies standardize on controllers in production codebases, so controller-based code is more recognizable to the engineers who would work on a real implementation of this project. The `[ApiController]` attribute also provides automatic model validation and 400 Bad Request responses without additional code, and the controller pattern scales better in a team environment where multiple engineers contribute to the same project.

**Alternatives considered:** Minimal API was evaluated but rejected because its flat, file-scoped registration style does not reflect the patterns used in enterprise .NET applications. The goal of this project is to demonstrate practices that apply in a production PPG context, not to demonstrate the minimum possible boilerplate.

---

## 2026-03-07 — Scalar over Swagger UI

**Decision:** Use Scalar as the interactive API browser at `/scalar/v1`.

**Rationale:** Swashbuckle — the library that powered Swagger UI in .NET 5 through 8 — is no longer actively maintained. .NET 10 ships with built-in OpenAPI document generation via `Microsoft.AspNetCore.OpenApi`, and Scalar is the current community and Microsoft-endorsed companion UI for that package. Using Scalar signals awareness of the current .NET ecosystem rather than defaulting to a legacy tool that requires a third-party package and additional configuration.

**Alternatives considered:** Swashbuckle/Swagger UI was rejected because it requires a separate NuGet package (`Swashbuckle.AspNetCore`) that is no longer under active development, and its integration with the built-in .NET 10 OpenAPI generator is non-trivial. NSwag was also considered but rejected for the same reasons — it adds an external dependency where a well-supported first-party path now exists.

---

## 2026-03-07 — Scoped Lifetime for FormulationService

**Decision:** Register `FormulationService` with a scoped lifetime — one instance per HTTP request.

**Rationale:** `FormulationService` is stateless. It accepts a `FormulationRequest`, performs a calculation, and returns a `FormulationResult` with no shared state between calls. Scoped is the correct lifetime for per-request services that hold no state between requests. It aligns with the .NET dependency injection conventions that treat scoped as the default for services whose behavior is bounded to a single unit of work.

**Alternatives considered:** Singleton was rejected because it implies shared state across requests, which would be misleading about the service's intent and could cause subtle bugs if state is ever added. Transient was rejected as unnecessarily wasteful — it would create a new instance for every injection point within the same request, which adds allocation overhead for no benefit when the service carries no per-injection state.

---

## 2026-03-07 — No Persistence Layer

**Decision:** The API is intentionally stateless with no database or persistent storage.

**Rationale:** All calculations in this API are purely functional — the same inputs always produce the same outputs, and there is no workflow state that needs to survive between requests. Adding a database would introduce connection management, migration scripts, environment configuration, and deployment complexity that would obscure the core domain logic this project is meant to demonstrate. The value of this project lies in showing how a VOC impact calculation can be modeled and exposed as an API, not in demonstrating data persistence patterns.

**Alternatives considered:** SQLite was considered as a lightweight embedded option that would avoid the operational overhead of a hosted database. It was rejected because even an embedded database adds a schema, an ORM or query layer, and migration tooling that would distract from the calculation logic. In a production system a persistence layer would be introduced once the MVP is validated and the data model is understood.

---

## 2026-03-07 — Incremental Prompt-Based Build with Claude Code

**Decision:** Build the project using a structured multi-step prompting approach, with one prompt per architectural layer, rather than a single large prompt.

**Rationale:** Each prompt addresses one concern — scaffold, models, service, controller — and includes a verification step before the next prompt runs. This approach keeps generated code readable and scoped to a single responsibility, makes errors easier to isolate to the layer where they were introduced, and ensures the developer reviews each layer before building on top of it. It also produces a clearer audit trail of how the project was constructed, which is valuable for demonstration and teaching purposes.

**Alternatives considered:** A single large prompt that generates the entire project at once was rejected because it produces opaque output that is harder to review, harder to debug, and harder to learn from. The incremental approach trades speed of initial generation for clarity, reviewability, and confidence in each layer.

---

## 2026-03-07 — Singleton Lifetime for SolventLibraryService (Planned)

**Decision:** Register `SolventLibraryService` with a singleton lifetime when it is introduced.

**Rationale:** `SolventLibraryService` will hold static reference data — solvent names, VOC values, cost ranges — that is loaded once and never changes at runtime. Instantiating it once and reusing it across all requests is both semantically correct and efficient. The data it holds is read-only from the perspective of any individual request, so there are no thread-safety concerns with shared access.

**Alternatives considered:** Scoped registration was rejected because it would recreate the reference data set on every request, which is wasteful for data that never changes. Transient was rejected for the same reason. This decision deliberately contrasts with `FormulationService`, which is scoped because it processes per-request input data rather than holding shared static reference data.

---

## 2026-03-07 — Singleton Lifetime for AuditLogService (Planned)

**Decision:** Register `AuditLogService` with a singleton lifetime when it is introduced.

**Rationale:** `AuditLogService` will maintain shared in-memory state — a `ConcurrentQueue<AuditEntry>` of calculation audit entries — that must persist and accumulate across requests for the lifetime of the application process. This is a fundamentally different case from `FormulationService`: the purpose of the audit log is to survive beyond any single request. A singleton is the only lifetime that makes this possible in the .NET DI container. `ConcurrentQueue<T>` is used internally to ensure thread safety when multiple concurrent requests write to the queue simultaneously.

**Alternatives considered:** Scoped registration was rejected because it would create a new, empty queue on every request, making the audit log useless — each request would only ever see its own entries. Transient was rejected for the same reason.

---

## 2026-03-07 — Named HttpClient via IHttpClientFactory for Anthropic API

**Decision:** Use `IHttpClientFactory` with a named client to make HTTP calls to the Anthropic API rather than instantiating `HttpClient` directly.

**Rationale:** Direct instantiation of `HttpClient` is a well-documented anti-pattern in .NET that causes socket exhaustion under load. Each directly instantiated client holds a socket connection open after disposal, and under sustained traffic this depletes the available socket pool. `IHttpClientFactory` manages client lifetimes correctly by pooling the underlying `HttpMessageHandler` instances, which avoids this problem. Using a named client also centralizes the Anthropic base URL and authentication headers in the `Program.cs` registration rather than scattering them across the service implementation.

**Alternatives considered:** Direct `HttpClient` instantiation was rejected due to the socket exhaustion risk described above. A typed client was considered as an alternative to a named client — it provides stronger typing by injecting the configured client directly into a specific service class — but a named client was chosen because it is simpler to configure and sufficient for a single-consumer integration.
