using FormulationImpactApi.Models;
using FormulationImpactApi.Services;

namespace FormulationImpactApi.Tests;

// Unit tests for FormulationService.CalculateImpact().
// Each test targets a specific formula branch or output field.
// The service is stateless, so tests instantiate it directly with no mocking needed.
public class FormulationServiceTests
{
    private readonly FormulationService _sut = new();

    // --- VOC Delta ---

    [Fact]
    public void VocDelta_IsNegative_WhenReplacementHasLowerVoc()
    {
        // Replacement VOC (200) < Baseline VOC (320) → reduction → negative delta
        var result = _sut.CalculateImpact(new FormulationRequest
        {
            BaselineSolvent = "A", ReplacementSolvent = "B",
            SubstitutionPercent = 50, BatchSizeKg = 100,
            BaselineVocGramsPerKg = 320, ReplacementVocGramsPerKg = 200,
            BaselineCostPerKg = 1.00, ReplacementCostPerKg = 1.00
        });
        Assert.True(result.VocDeltaGrams < 0, "VocDeltaGrams should be negative when replacement has lower VOC");
    }

    [Fact]
    public void VocDelta_IsPositive_WhenReplacementHasHigherVoc()
    {
        // Replacement VOC (400) > Baseline VOC (320) → increase → positive delta
        var result = _sut.CalculateImpact(new FormulationRequest
        {
            BaselineSolvent = "A", ReplacementSolvent = "B",
            SubstitutionPercent = 50, BatchSizeKg = 100,
            BaselineVocGramsPerKg = 320, ReplacementVocGramsPerKg = 400,
            BaselineCostPerKg = 1.00, ReplacementCostPerKg = 1.00
        });
        Assert.True(result.VocDeltaGrams > 0, "VocDeltaGrams should be positive when replacement has higher VOC");
    }

    [Fact]
    public void VocDelta_Formula_IsCorrect()
    {
        // (400 - 320) * 100 * (50 / 100) = 80 * 100 * 0.5 = 4000
        var result = _sut.CalculateImpact(new FormulationRequest
        {
            BaselineSolvent = "A", ReplacementSolvent = "B",
            SubstitutionPercent = 50, BatchSizeKg = 100,
            BaselineVocGramsPerKg = 320, ReplacementVocGramsPerKg = 400,
            BaselineCostPerKg = 1.00, ReplacementCostPerKg = 1.00
        });
        Assert.Equal(4000.0, result.VocDeltaGrams);
    }

    // --- Regulatory Flag ---

    [Fact]
    public void RegulatoryFlag_IsFalse_WhenVocIncreaseIsExactlyThreshold()
    {
        // VocDeltaGrams = exactly 50 → flag is false (threshold is > 50, not >= 50)
        // (1) * 100 * (50/100) = 50
        var result = _sut.CalculateImpact(new FormulationRequest
        {
            BaselineSolvent = "A", ReplacementSolvent = "B",
            SubstitutionPercent = 50, BatchSizeKg = 100,
            BaselineVocGramsPerKg = 0, ReplacementVocGramsPerKg = 1,
            BaselineCostPerKg = 1.00, ReplacementCostPerKg = 1.00
        });
        Assert.Equal(50.0, result.VocDeltaGrams);
        Assert.False(result.RegulatoryFlag, "Flag should be false when VocDeltaGrams == 50 (threshold is strictly > 50)");
    }

    [Fact]
    public void RegulatoryFlag_IsFalse_WhenVocIncreaseBelowThreshold()
    {
        // (10) * 10 * (10/100) = 10g — below 50g threshold
        var result = _sut.CalculateImpact(new FormulationRequest
        {
            BaselineSolvent = "A", ReplacementSolvent = "B",
            SubstitutionPercent = 10, BatchSizeKg = 10,
            BaselineVocGramsPerKg = 0, ReplacementVocGramsPerKg = 10,
            BaselineCostPerKg = 1.00, ReplacementCostPerKg = 1.00
        });
        Assert.False(result.RegulatoryFlag);
    }

    [Fact]
    public void RegulatoryFlag_IsTrue_WhenVocIncreaseExceedsThreshold()
    {
        // (200) * 10 * (50/100) = 1000g — well above 50g threshold
        var result = _sut.CalculateImpact(new FormulationRequest
        {
            BaselineSolvent = "A", ReplacementSolvent = "B",
            SubstitutionPercent = 50, BatchSizeKg = 10,
            BaselineVocGramsPerKg = 0, ReplacementVocGramsPerKg = 200,
            BaselineCostPerKg = 1.00, ReplacementCostPerKg = 1.00
        });
        Assert.True(result.RegulatoryFlag, "Flag should be true when VocDeltaGrams > 50");
    }

    // --- Cost Delta ---

    [Fact]
    public void CostDelta_IsNegative_WhenReplacementIsCheaper()
    {
        // Replacement cost (1.00) < Baseline cost (1.50) → cheaper → negative delta
        var result = _sut.CalculateImpact(new FormulationRequest
        {
            BaselineSolvent = "A", ReplacementSolvent = "B",
            SubstitutionPercent = 50, BatchSizeKg = 100,
            BaselineVocGramsPerKg = 300, ReplacementVocGramsPerKg = 300,
            BaselineCostPerKg = 1.50, ReplacementCostPerKg = 1.00
        });
        Assert.True(result.CostDelta < 0, "CostDelta should be negative when replacement is cheaper");
    }

    [Fact]
    public void CostDelta_Formula_IsCorrect()
    {
        // (1.50 - 1.00) * 200 * (50/100) = 0.50 * 200 * 0.5 = 50.00
        var result = _sut.CalculateImpact(new FormulationRequest
        {
            BaselineSolvent = "A", ReplacementSolvent = "B",
            SubstitutionPercent = 50, BatchSizeKg = 200,
            BaselineVocGramsPerKg = 300, ReplacementVocGramsPerKg = 300,
            BaselineCostPerKg = 1.00, ReplacementCostPerKg = 1.50
        });
        Assert.Equal(50.00, result.CostDelta);
    }

    // --- Summary ---

    [Fact]
    public void Summary_ContainsBothSolventNames()
    {
        var result = _sut.CalculateImpact(new FormulationRequest
        {
            BaselineSolvent = "Toluene", ReplacementSolvent = "Ethyl Acetate",
            SubstitutionPercent = 50, BatchSizeKg = 100,
            BaselineVocGramsPerKg = 300, ReplacementVocGramsPerKg = 300,
            BaselineCostPerKg = 1.00, ReplacementCostPerKg = 1.00
        });
        Assert.Contains("Toluene", result.Summary);
        Assert.Contains("Ethyl Acetate", result.Summary);
    }

    [Fact]
    public void Summary_ContainsSubstitutionPercent()
    {
        var result = _sut.CalculateImpact(new FormulationRequest
        {
            BaselineSolvent = "A", ReplacementSolvent = "B",
            SubstitutionPercent = 75, BatchSizeKg = 100,
            BaselineVocGramsPerKg = 300, ReplacementVocGramsPerKg = 300,
            BaselineCostPerKg = 1.00, ReplacementCostPerKg = 1.00
        });
        Assert.Contains("75", result.Summary);
    }

    [Fact]
    public void Summary_Says_Reduces_WhenVocGoesDown()
    {
        var result = _sut.CalculateImpact(new FormulationRequest
        {
            BaselineSolvent = "A", ReplacementSolvent = "B",
            SubstitutionPercent = 50, BatchSizeKg = 100,
            BaselineVocGramsPerKg = 320, ReplacementVocGramsPerKg = 200,
            BaselineCostPerKg = 1.00, ReplacementCostPerKg = 1.00
        });
        Assert.Contains("reduces", result.Summary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Summary_Says_Increases_WhenVocGoesUp()
    {
        var result = _sut.CalculateImpact(new FormulationRequest
        {
            BaselineSolvent = "A", ReplacementSolvent = "B",
            SubstitutionPercent = 50, BatchSizeKg = 100,
            BaselineVocGramsPerKg = 200, ReplacementVocGramsPerKg = 320,
            BaselineCostPerKg = 1.00, ReplacementCostPerKg = 1.00
        });
        Assert.Contains("increases", result.Summary, StringComparison.OrdinalIgnoreCase);
    }
}
