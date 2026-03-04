# FormulationImpactApi — CLAUDE.md

## What This Is
A .NET 10 Web API that calculates the environmental and financial impact of
solvent substitutions in industrial paint and coatings formulations. Built as
a demo project to illustrate AI-assisted development in a domain-specific
manufacturing context.

## Domain Context
Paint formulations contain solvents that contribute to VOC (Volatile Organic
Compound) emissions. Manufacturers frequently substitute solvents to reduce
emissions, cut costs, or meet regulatory requirements. This API takes a
proposed substitution and returns the calculated impact before any physical
change is made.

VOC emissions are regulated at the federal level (EPA) and in many states.
The regulatory flag in this API triggers when a substitution would increase
total VOC output by more than 50 grams per batch — a simplified threshold
used here for demo purposes.

## Stack
- .NET 10 Web API (controller-based, not minimal API)
- C# 14
- No database — all calculations are stateless and in-memory
- OpenAPI via Microsoft.AspNetCore.OpenApi (built-in to .NET 10)
- Scalar UI for API browsing at /scalar/v1 (replaces Swagger UI from .NET 8)
- CORS enabled for http://localhost:4200 (Angular frontend)

## Architecture
Controllers are thin — they accept requests, call the service, and return
results. No business logic lives in controllers.

All calculation logic lives in Services/FormulationService.cs. If a
calculation is wrong, look there first.

Models/FormulationRequest.cs — inbound DTO
Models/FormulationResult.cs — outbound DTO
Services/FormulationService.cs — all business logic
Controllers/FormulationController.cs — single POST endpoint

## The Core Calculation Logic
These formulas must not be altered without explicit instruction:

  VocDeltaGrams = (ReplacementVocGramsPerKg - BaselineVocGramsPerKg)
                  * BatchSizeKg * (SubstitutionPercent / 100)

  CostDelta = (ReplacementCostPerKg - BaselineCostPerKg)
              * BatchSizeKg * (SubstitutionPercent / 100)

  RegulatoryFlag = true if VocDeltaGrams > 50

A negative VocDeltaGrams means the substitution reduces emissions — this is
the desirable outcome. A negative CostDelta means the substitution is cheaper.

## Single Endpoint
POST /api/formulation/impact
Accepts: FormulationRequest (JSON body)
Returns: FormulationResult (JSON)
Registered route prefix: api/formulation on the controller

## Key Decisions and Why
- Controller-based not minimal API: mirrors enterprise .NET conventions that
  PPG and similar companies use in production
- Scoped service registration: FormulationService is registered as scoped in
  Program.cs, appropriate for a per-request stateless calculation service
- No persistence layer: intentional for demo simplicity — the calculation is
  purely functional
- Scalar over Swagger UI: Scalar is the .NET 10 standard; Swashbuckle is no
  longer the default

## Common Bug Sources
- CORS errors from the Angular frontend usually mean the CORS policy in
  Program.cs is misconfigured or applied in the wrong order relative to
  routing middleware
- Null reference on FormulationRequest properties usually means [FromBody]
  is missing on the controller parameter or the JSON field names don't match
  the DTO property names (check camelCase vs PascalCase)
- Scalar UI not loading usually means MapScalarApiReference() is inside a
  conditional block that isn't running in the current environment