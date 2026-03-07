# FormulationImpactApi

A .NET 10 Web API that calculates the environmental and financial impact of solvent substitutions in industrial paint and coatings formulations. Built as a demo project illustrating AI-assisted development in a domain-specific manufacturing context.

---

## What It Does

Paint formulations contain solvents that contribute to VOC (Volatile Organic Compound) emissions. Manufacturers frequently substitute solvents to reduce emissions, cut costs, or meet regulatory requirements. This API accepts a proposed substitution scenario and returns the calculated impact — before any physical change is made.

---

## Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 |
| Language | C# 14 |
| API style | Controller-based (not minimal API) |
| OpenAPI | `Microsoft.AspNetCore.OpenApi` (built-in to .NET 10) |
| API browser | Scalar UI at `/scalar/v1` |
| Persistence | None — calculations are stateless and in-memory |
| Frontend (separate) | Angular 19 on `localhost:4200` |

---

## Project Structure

```
FormulationImpactApi/
  Controllers/
    FormulationController.cs        POST /api/formulation/impact
  Models/
    FormulationRequest.cs           Inbound DTO
    FormulationResult.cs            Outbound DTO
  Services/
    FormulationService.cs           All calculation logic
  Program.cs                        Host configuration, DI, middleware
  appsettings.json
  Properties/
    launchSettings.json             Runs on http://localhost:5275

FormulationImpactApi.Tests/
  FormulationServiceTests.cs        xUnit unit tests for FormulationService
```

---

## Running the API

```bash
cd FormulationImpactApi
dotnet run --launch-profile http
```

The API starts at `http://localhost:5275`.
The Scalar API browser is available at `http://localhost:5275/scalar/v1` (Development only).

---

## API Reference

### POST `/api/formulation/impact`

Calculates the VOC and cost impact of a solvent substitution.

**Request body**

```json
{
  "baselineSolvent": "Toluene",
  "replacementSolvent": "Ethyl Acetate",
  "substitutionPercent": 50,
  "batchSizeKg": 200,
  "baselineVocGramsPerKg": 320,
  "replacementVocGramsPerKg": 280,
  "baselineCostPerKg": 1.20,
  "replacementCostPerKg": 1.05
}
```

| Field | Type | Description |
|---|---|---|
| `baselineSolvent` | string | Name of the solvent currently in use |
| `replacementSolvent` | string | Name of the proposed replacement solvent |
| `substitutionPercent` | number | Percentage of the baseline being replaced (0–100) |
| `batchSizeKg` | number | Total batch size in kilograms |
| `baselineVocGramsPerKg` | number | VOC content of the baseline solvent (g/kg) |
| `replacementVocGramsPerKg` | number | VOC content of the replacement solvent (g/kg) |
| `baselineCostPerKg` | number | Cost of the baseline solvent ($/kg) |
| `replacementCostPerKg` | number | Cost of the replacement solvent ($/kg) |

**Response body**

```json
{
  "vocDeltaGrams": -4000.0,
  "costDelta": -15.00,
  "regulatoryFlag": false,
  "summary": "Substituting 50% of Toluene with Ethyl Acetate reduces VOC emissions by 4000.0g per batch and is cheaper by $15.00."
}
```

| Field | Type | Description |
|---|---|---|
| `vocDeltaGrams` | number | VOC change in grams per batch. Negative = reduction (desirable) |
| `costDelta` | number | Cost change in dollars per batch. Negative = cheaper |
| `regulatoryFlag` | boolean | `true` if VOC increases by more than 50 g/batch |
| `summary` | string | Plain English description of the substitution impact |

---

## Calculation Formulas

These formulas are the core of the service and must not be changed without explicit instruction:

```
VocDeltaGrams = (ReplacementVocGramsPerKg - BaselineVocGramsPerKg)
                * BatchSizeKg * (SubstitutionPercent / 100)

CostDelta = (ReplacementCostPerKg - BaselineCostPerKg)
            * BatchSizeKg * (SubstitutionPercent / 100)

RegulatoryFlag = true  if VocDeltaGrams > 50
```

A **negative** `VocDeltaGrams` means the substitution reduces emissions — the desirable outcome.
A **negative** `CostDelta` means the substitution is cheaper.

The regulatory threshold (50 g/batch) is a simplified value used for demo purposes; real-world thresholds vary by jurisdiction and facility permit.

---

## Architecture Notes

- **Controllers are thin.** They accept the request, call the service, and return the result. No business logic lives in controllers.
- **All calculation logic lives in `FormulationService.cs`.** If a result is wrong, look there first.
- **`FormulationService` is registered as scoped** in `Program.cs` — a fresh instance per HTTP request, appropriate for a stateless service.
- **No database.** The calculation is purely functional; no reads or writes to external storage.
- **CORS** is configured to allow `http://localhost:4200` so the Angular frontend can call the API during development.

---

## Frontend (Angular 19)

The companion frontend is a separate Angular 19 project (`formulation-impact-ui`) that runs on `localhost:4200`. It sends `FormulationRequest` objects to this API and displays the result.

See [prompts.txt](prompts.txt) for the step-by-step AI-assisted build prompts for both the backend (B1–B4) and frontend (F1–F4).

See `.claude/commands/` for slash commands that execute each build step directly inside Claude Code.

---

## Configuration Notes

- Scalar UI only loads in `Development` environment. If it is not rendering, check that `ASPNETCORE_ENVIRONMENT=Development` is set.
- If the Angular frontend gets a CORS error, verify that the `AllowAngularDev` policy in `Program.cs` is applied before `UseAuthorization` and after routing.

---

## Running Tests

```bash
cd FormulationImpactApi.Tests
dotnet test
```

Tests cover `FormulationService.CalculateImpact()` — VOC delta sign and formula correctness, regulatory flag threshold (strictly > 50 g), cost delta sign and formula correctness, and summary string content.

---

## Building Step by Step with Claude Code

This project was built incrementally using structured AI prompts. Each step is available as a slash command in Claude Code:

| Command | What it does |
|---|---|
| `/scaffold-api` | Scaffolds the .NET 10 project with OpenAPI and CORS |
| `/add-models` | Adds `FormulationRequest` and `FormulationResult` DTOs |
| `/add-service` | Adds `FormulationService` with calculation logic |
| `/add-controller` | Adds `FormulationController` with the POST endpoint |
| `/scaffold-ui` | Scaffolds the Angular 19 project with `provideHttpClient()` |
| `/add-ui-service` | Adds TypeScript models and the Angular service |
| `/add-form` | Builds the reactive form component |
| `/add-results` | Adds the results panel with conditional display |
