# FormulationImpactApi — Task Board

This file is the project task board. It is not a backlog dump. Every task has
an acceptance criterion, a status, and a corresponding prompt or commit reference.

A task is not complete when Claude Code says it is done. A task is complete when:

1. The code runs without errors
2. All tests pass (`dotnet test`)
3. Inline documentation is updated
4. README.md reflects any API or architecture changes
5. DECISIONS.md is updated if an architectural decision was made
6. The work is committed with a descriptive message referencing this task

---

## Completed

### TASK-001 — Scaffold .NET 10 Web API
Status: Done
Prompt: /scaffold-api
Acceptance criterion: `dotnet run` starts the server, Scalar UI loads at `/scalar/v1`, and CORS allows requests from `http://localhost:4200`.
Commit: (update with actual hash)

---

### TASK-002 — Add FormulationRequest and FormulationResult DTOs
Status: Done
Prompt: /add-models
Acceptance criterion: Both model files exist in `Models/` with all required properties and inline comments, and the project builds cleanly.
Commit: (update with actual hash)

---

### TASK-003 — Add FormulationService with Calculation Logic
Status: Done
Prompt: /add-service
Acceptance criterion: Service calculates VOC delta, cost delta, regulatory flag, and summary correctly for a known input; registered as scoped in `Program.cs`; unit tests pass.
Commit: (update with actual hash)

---

### TASK-004 — Add FormulationController with POST /impact Endpoint
Status: Done
Prompt: /add-controller
Acceptance criterion: `POST /api/formulation/impact` returns correct `FormulationResult` for a valid request body tested in Scalar UI.
Commit: (update with actual hash)

---

## Planned

### TASK-005 — Add Solvent Model and SolventLibraryService
Status: Todo
Prompt: /add-solvent-library
Acceptance criterion: `SolventLibraryService` returns all 15 solvents from `GetAll()`, `GetByName()` returns the correct solvent or null, registered as singleton in `Program.cs`, and unit tests cover both methods.
Commit:

---

### TASK-006 — Add GET /api/solvents Endpoint
Status: Todo
Prompt: /add-solvent-endpoint
Acceptance criterion: `GET /api/solvents` returns all 15 solvents as a JSON array in Scalar UI with correct field names and values.
Commit:

---

### TASK-007 — Add Solvent Dropdown Selectors to Angular UI
Status: Todo
Prompt: /add-solvent-dropdowns
Acceptance criterion: Dropdowns populate from the API on load, selecting a solvent auto-fills VOC and cost fields, and fields remain editable after auto-fill.
Commit:

---

### TASK-008 — Add NaturalLanguageService with Claude API Integration
Status: Todo
Prompt: /add-nl-service
Acceptance criterion: Service correctly parses a plain English solvent substitution query into a `FormulationRequest`, calls `FormulationService`, and returns a `NaturalLanguageResult` with `parseSucceeded` true and a populated narrative.
Commit:

---

### TASK-009 — Add POST /api/formulation/query Endpoint
Status: Todo
Prompt: /add-nl-endpoint
Acceptance criterion: A valid natural language query returns 200 with a full `NaturalLanguageResult`; an unparseable query returns 422 with a descriptive narrative; both cases tested in Scalar UI.
Commit:

---

### TASK-010 — Add Plain English Mode to Angular UI
Status: Todo
Prompt: /add-nl-ui
Acceptance criterion: Mode toggle switches between form and textarea input, a successful NL query populates the results panel, and a failed parse displays a red error message.
Commit:

---

### TASK-011 — Add AuditLogService and Logging Calls
Status: Todo
Prompt: /add-audit-log
Acceptance criterion: Every call to `/impact` and `/query` logs an `AuditEntry`, the log is capped at 50 entries, registered as singleton, and concurrent access is handled safely.
Commit:

---

### TASK-012 — Add GET /api/formulation/history Endpoint
Status: Todo
Prompt: /add-history-endpoint
Acceptance criterion: `GET /api/formulation/history` returns the last 10 entries by default, the `count` parameter is respected up to a max of 50, and entries appear in Scalar UI after running calculations.
Commit:

---

### TASK-013 — Add History Panel to Angular UI
Status: Todo
Prompt: /add-history-ui
Acceptance criterion: History panel appears below results, auto-refreshes after each calculation, show/hide toggle works, and each entry displays timestamp, mode badge, solvent names, VOC delta, and cost delta with correct colors.
Commit:

---

## Commit Convention

Every completed task is committed separately. Commit messages follow this format:

```
TASK-###: Short title matching the task

- Bullet describing the primary change
- Bullet describing tests added
- Bullet describing documentation updated
```

Example:

```
TASK-003: Add FormulationService with calculation logic

- FormulationService.cs implements VOC delta, cost delta,
  regulatory flag, and summary generation
- Unit tests added for known input/output pairs including
  boundary case at regulatory threshold
- README.md API reference updated, DECISIONS.md updated with
  scoped lifetime rationale
```

This format means the commit history is the audit trail. Each commit maps to
one task, one prompt, and one acceptance criterion.
