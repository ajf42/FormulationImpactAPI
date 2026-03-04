using FormulationImpactApi.Models;

namespace FormulationImpactApi.Services;

public class FormulationService
{
    public FormulationResult CalculateImpact(FormulationRequest request)
    {
        // Net change in VOC emissions for the substituted portion of the batch,
        // in grams. Negative value means the substitution reduces emissions.
        double vocDeltaGrams = (request.ReplacementVocGramsPerKg - request.BaselineVocGramsPerKg)
                               * request.BatchSizeKg
                               * (request.SubstitutionPercent / 100);

        // Net change in cost for the substituted portion of the batch,
        // in dollars. Negative value means the substitution is cheaper.
        double costDelta = (request.ReplacementCostPerKg - request.BaselineCostPerKg)
                           * request.BatchSizeKg
                           * (request.SubstitutionPercent / 100);

        // Regulatory threshold: a VOC increase greater than 50 grams per batch
        // triggers a flag indicating the substitution may require regulatory review.
        bool regulatoryFlag = vocDeltaGrams > 50;

        // Plain English summary naming both solvents, the substitution percentage,
        // and the direction of change for both VOC emissions and cost.
        string vocDirection = vocDeltaGrams < 0 ? "reduces" : "increases";
        string costDirection = costDelta < 0 ? "cheaper" : "more expensive";
        string summary = $"Substituting {request.SubstitutionPercent}% of {request.BaselineSolvent} " +
                         $"with {request.ReplacementSolvent} {vocDirection} VOC emissions by " +
                         $"{Math.Abs(vocDeltaGrams):F1}g per batch and is {costDirection} by " +
                         $"${Math.Abs(costDelta):F2}.";

        return new FormulationResult
        {
            VocDeltaGrams = vocDeltaGrams,
            CostDelta = costDelta,
            RegulatoryFlag = regulatoryFlag,
            Summary = summary
        };
    }
}
