using FormulationImpactApi.Models;
using FormulationImpactApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FormulationImpactApi.Controllers;

[ApiController]
[Route("api/formulation")]
public class FormulationController : ControllerBase
{
    private readonly FormulationService _formulationService;

    // FormulationService is injected by the DI container.
    // It is registered as scoped in Program.cs, so a fresh instance
    // is provided for each HTTP request.
    public FormulationController(FormulationService formulationService)
    {
        _formulationService = formulationService;
    }

    // Accepts a substitution scenario as JSON, runs the impact calculation,
    // and returns the VOC delta, cost delta, regulatory flag, and summary.
    [HttpPost("impact")]
    public ActionResult<FormulationResult> CalculateImpact([FromBody] FormulationRequest request)
    {
        FormulationResult result = _formulationService.CalculateImpact(request);
        return Ok(result);
    }
}
