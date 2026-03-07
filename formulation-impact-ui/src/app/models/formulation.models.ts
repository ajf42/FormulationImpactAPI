export interface FormulationRequest {
  baselineSolvent: string;
  replacementSolvent: string;
  substitutionPercent: number;
  batchSizeKg: number;
  baselineVocGramsPerKg: number;
  replacementVocGramsPerKg: number;
  baselineCostPerKg: number;
  replacementCostPerKg: number;
}

export interface FormulationResult {
  vocDeltaGrams: number;
  costDelta: number;
  regulatoryFlag: boolean;
  summary: string;
}
