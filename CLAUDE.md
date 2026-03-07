# FormulationImpactApi — CLAUDE.md

---

## Section 1 — What This Is

### What This Is
A .NET 10 Web API that calculates the environmental and financial impact of
solvent substitutions in industrial paint and coatings formulations. Built as
a demo project to illustrate AI-assisted development in a domain-specific
manufacturing context.

### Domain Context
Paint formulations contain solvents that contribute to VOC (Volatile Organic
Compound) emissions. Manufacturers frequently substitute solvents to reduce
emissions, cut costs, or meet regulatory requirements. This API takes a
proposed substitution and returns the calculated impact before any physical
change is made.

VOC emissions are regulated at the federal level (EPA) and in many states.
The regulatory flag in this API triggers when a substitution would increase
total VOC output by more than 50 grams per batch — a simplified threshold
used here for demo purposes.

### Stack
- .NET 10 Web API (controller-based, not minimal API)
- C# 14
- No database — all calculations are stateless and in-memory
- OpenAPI via Microsoft.AspNetCore.OpenApi (built-in to .NET 10)
- Scalar UI for API browsing at /scalar/v1 (replaces Swagger UI from .NET 8)
- CORS enabled for http://localhost:4200 (Angular frontend)

### Architecture
Controllers are thin — they accept requests, call the service, and return
results. No business logic lives in controllers.

All calculation logic lives in Services/FormulationService.cs. If a
calculation is wrong, look there first.

### Key Decisions and Why
Full rationale for each decision is logged in DECISIONS.md.
Decision titles are listed here as a scannable index only.

- .NET 10 over .NET 8
- Controller-Based API over Minimal API
- Scalar over Swagger UI
- Scoped Lifetime for FormulationService
- No Persistence Layer
- Incremental Prompt-Based Build with Claude Code
- Singleton Lifetime for SolventLibraryService (Planned)
- Singleton Lifetime for AuditLogService (Planned)
- Named HttpClient via IHttpClientFactory for Anthropic API

---

## Section 2 — Current State

### Endpoints Currently Live
  POST /api/formulation/impact — accepts FormulationRequest, returns
  FormulationResult. Core calculation endpoint.

### File Structure as Built
  Controllers/
    FormulationController.cs — POST /impact endpoint, thin, no logic
  Models/
    FormulationRequest.cs — inbound DTO, 8 fields
    FormulationResult.cs — outbound DTO, 4 fields
  Services/
    FormulationService.cs — all calculation logic, registered scoped
  Program.cs — composition root, middleware pipeline, CORS, OpenAPI
  DECISIONS.md — architectural decision log, updated on every
    significant decision
  TASKS.md — task board with acceptance criteria, status, and
    commit references for every unit of work
  CLAUDE.md — this file
  README.md — developer guide and project overview
  .claude/commands/ — slash command prompt files for each build step

### Service Registrations Currently in Program.cs
  FormulationService — Scoped

---

## Section 3 — Planned State

Three features are planned and will be built in this order.
Task numbers reference TASKS.md entries.

### Feature 1: Solvent Library (TASK-005, TASK-006, TASK-007)
New files:
  Models/Solvent.cs
  Services/SolventLibraryService.cs — singleton, 15 embedded solvents
  Controllers/SolventController.cs — GET /api/solvents
New registration: SolventLibraryService as singleton
Purpose: Replace manual numeric entry with dropdown selectors populated
from a pre-loaded library of common industrial solvents with typical
VOC and cost values. Users can override values for proprietary data.

### Feature 2: Natural Language Query Mode (TASK-008, TASK-009, TASK-010)
New files:
  Models/NaturalLanguageRequest.cs
  Models/NaturalLanguageResult.cs
  Services/NaturalLanguageService.cs — calls Claude API, then calls
  FormulationService
New registration: NaturalLanguageService as scoped, named HttpClient
via IHttpClientFactory for Anthropic API
New endpoint: POST /api/formulation/query on FormulationController
Config: AnthropicApiKey in appsettings.Development.json
Purpose: Accept a plain English query, use Claude API to extract
FormulationRequest parameters, call existing calculation logic, return
structured result plus narrative.

### Feature 3: Audit Log (TASK-011, TASK-012, TASK-013)
New files:
  Models/AuditEntry.cs
  Services/AuditLogService.cs — singleton, ConcurrentQueue capped at 50
New registration: AuditLogService as singleton
New endpoint: GET /api/formulation/history on FormulationController
Purpose: Log every calculation with timestamp, query mode, request,
and result. Required because regulated industries need audit trails
for every formulation decision.

### Full File Structure When All Features Are Complete
  Controllers/
    FormulationController.cs
    SolventController.cs
  Models/
    FormulationRequest.cs
    FormulationResult.cs
    Solvent.cs
    NaturalLanguageRequest.cs
    NaturalLanguageResult.cs
    AuditEntry.cs
  Services/
    FormulationService.cs — Scoped
    SolventLibraryService.cs — Singleton
    NaturalLanguageService.cs — Scoped
    AuditLogService.cs — Singleton

---

## Section 4 — Rules

### Mandatory on Every Change
Every code change made to this repository must be accompanied by:
1. The code runs without errors before the task is considered done
2. All tests pass (dotnet test) — do not mark done if tests fail
3. Updated inline documentation for any modified logic
4. README.md updated to reflect any changes to the API, architecture,
   or project structure
5. DECISIONS.md updated if the change involved a significant
   architectural or engineering decision — new library chosen,
   lifetime registration changed, pattern adopted, feature scoped.
   If in doubt, log it.
6. TASKS.md updated — move the task to Done, add the commit hash,
   and update the Current State section of CLAUDE.md to reflect
   all new files, endpoints, and service registrations introduced
   by this task
7. CLAUDE.md updated if this task changed anything relevant to it —
   new files added, endpoints changed, registrations changed, planned
   features promoted to current state, or rules revised. See the
   CLAUDE.md Self-Maintenance rule below for what triggers an update
   and how to notify the user.
8. The work is committed using the commit convention in TASKS.md,
   one commit per task, before moving to the next task

Do not consider a task complete until all eight steps are done.

Before responding that any task is complete, re-read this
Mandatory on Every Change section and confirm each step has
been completed. Do not rely on memory of these steps from
earlier in the session — read them again.

### CLAUDE.md Self-Maintenance
CLAUDE.md must be updated whenever a completed task changes anything it
describes. Triggers for an update include:

  - A new file is created — add it to the File Structure in Section 2
  - A new endpoint goes live — add it to Endpoints Currently Live in Section 2
  - A new service registration is added — add it to Service Registrations
    in Section 2
  - A planned feature becomes current — move it from Section 3 to Section 2
    and remove it from the planned feature list
  - A new architectural rule or pattern is established — add it to Section 4
  - A slash command is added — add it to the Slash Commands list

When CLAUDE.md is modified as part of completing a task, you must notify
the user with the following block at the end of your response, filled in
with the actual details of what changed:

**## CLAUDE.md WAS MODIFIED**
**Section updated:** [Section number and name]
**What changed:** [One sentence describing the specific lines added, removed,
or revised]
**Why:** [One sentence explaining what task or decision triggered the update]

Do not skip this notification. Do not summarize it into prose. Output it as
a standalone block so the user can see at a glance that the project's
working memory has been updated.

### The Core Calculation Logic
These formulas must not be altered without explicit instruction:

  VocDeltaGrams = (ReplacementVocGramsPerKg - BaselineVocGramsPerKg)
                  * BatchSizeKg * (SubstitutionPercent / 100)

  CostDelta = (ReplacementCostPerKg - BaselineCostPerKg)
              * BatchSizeKg * (SubstitutionPercent / 100)

  RegulatoryFlag = true if VocDeltaGrams > 50

A negative VocDeltaGrams means the substitution reduces emissions — this is
the desirable outcome. A negative CostDelta means the substitution is cheaper.

### Service Lifetimes
Fixed — must not be changed without explicit instruction and a corresponding
DECISIONS.md entry:

  FormulationService — Scoped
  SolventLibraryService — Singleton (static data)
  NaturalLanguageService — Scoped (stateless per-request)
  AuditLogService — Singleton (shared in-memory state)

### Patterns
Must be used consistently — deviations require explicit instruction:

  Dependency injection: constructor injection in controllers and
  services. Do not mix patterns.

  Controller shape: thin controllers only. If logic appears in a
  controller action beyond calling a service and returning a result,
  it must be moved to a service.

  Error handling: controllers return appropriate HTTP status codes.
  Services throw exceptions. Controllers catch and map to status codes.

  Async: use async/await for all I/O operations including all
  Anthropic API calls. Synchronous HTTP calls are not permitted.

  Commits: one commit per completed task following the commit
  convention in TASKS.md. Do not bundle multiple tasks into one
  commit. Do not commit incomplete work.

### Common Bug Sources
- CORS errors from the Angular frontend usually mean the CORS policy in
  Program.cs is misconfigured or applied in the wrong order relative to
  routing middleware
- Null reference on FormulationRequest properties usually means [FromBody]
  is missing on the controller parameter or the JSON field names don't match
  the DTO property names (check camelCase vs PascalCase)
- Scalar UI not loading usually means MapScalarApiReference() is inside a
  conditional block that isn't running in the current environment
- Natural language endpoint returning 422 immediately usually means
  the Anthropic API key is missing or empty in
  appsettings.Development.json
- Claude returning malformed JSON instead of UNPARSEABLE usually
  means the system prompt in NaturalLanguageService needs to be more
  explicit about response format
- IHttpClientFactory not resolving usually means AddHttpClient() is
  missing from Program.cs
- AuditLogService not persisting entries across requests usually
  means it was registered as Scoped instead of Singleton

### Slash Commands
Each build step is available as a slash command in Claude Code.
Type the command name to load the full structured prompt for that step.

Backend (run in order):
  /scaffold-api      — scaffold .NET 10 project with OpenAPI and CORS
  /add-models        — add FormulationRequest and FormulationResult DTOs
  /add-service       — add FormulationService with calculation logic
  /add-controller    — add FormulationController with POST /impact endpoint

Frontend (run in order, after backend is complete):
  /scaffold-ui       — scaffold Angular 19 project with provideHttpClient()
  /add-ui-service    — add TypeScript models and Angular HTTP service
  /add-form          — build reactive form component (console.log output only)
  /add-results       — add results panel with @if conditional display

Features (run after base build is complete):
  /add-solvent-library    — TASK-005: Solvent model and SolventLibraryService
  /add-solvent-endpoint   — TASK-006: GET /api/solvents controller
  /add-solvent-dropdowns  — TASK-007: Angular dropdown selectors
  /add-nl-service         — TASK-008: NaturalLanguageService with Claude API
  /add-nl-endpoint        — TASK-009: POST /api/formulation/query action
  /add-nl-ui              — TASK-010: plain English mode toggle in Angular
  /add-audit-log          — TASK-011: AuditLogService and logging calls
  /add-history-endpoint   — TASK-012: GET /api/formulation/history
  /add-history-ui         — TASK-013: Angular history panel

Command files live in .claude/commands/. Each file is a self-contained prompt
with full context so any step can be re-run independently if needed.
