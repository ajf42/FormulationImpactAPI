namespace FormulationImpactApi.Models;

public class FormulationResult
{
    // Change in VOC emissions in grams per batch; negative means a reduction
    public double VocDeltaGrams { get; set; }

    // Change in cost in dollars per batch; negative means cheaper
    public double CostDelta { get; set; }

    // True if the substitution increases VOC output beyond the regulatory threshold
    public bool RegulatoryFlag { get; set; }

    // Plain English description of the overall impact of the substitution
    public string Summary { get; set; } = string.Empty;
}
