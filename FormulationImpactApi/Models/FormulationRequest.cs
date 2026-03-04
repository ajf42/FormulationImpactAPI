namespace FormulationImpactApi.Models;

public class FormulationRequest
{
    // Name of the solvent currently used in the formulation
    public string BaselineSolvent { get; set; } = string.Empty;

    // Name of the solvent proposed to replace the baseline
    public string ReplacementSolvent { get; set; } = string.Empty;

    // Percentage of the baseline solvent being replaced, expressed as 0–100
    public double SubstitutionPercent { get; set; }

    // Total size of the production batch in kilograms
    public double BatchSizeKg { get; set; }

    // VOC content of the baseline solvent in grams per kilogram
    public double BaselineVocGramsPerKg { get; set; }

    // VOC content of the replacement solvent in grams per kilogram
    public double ReplacementVocGramsPerKg { get; set; }

    // Cost of the baseline solvent in dollars per kilogram
    public double BaselineCostPerKg { get; set; }

    // Cost of the replacement solvent in dollars per kilogram
    public double ReplacementCostPerKg { get; set; }
}
