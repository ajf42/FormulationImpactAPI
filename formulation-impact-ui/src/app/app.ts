import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { FormulationService } from './services/formulation.service';
import { FormulationRequest, FormulationResult } from './models/formulation.models';

// Reactive form with 8 fields mirroring FormulationRequest.
// On submit, the form values are cast to FormulationRequest and sent to the API.
// FormulationService is injected via inject() — the Angular 19 preferred pattern.
@Component({
  selector: 'app-root',
  imports: [ReactiveFormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  private formulationService = inject(FormulationService);

  result = signal<FormulationResult | null>(null);

  form = new FormGroup({
    baselineSolvent: new FormControl('', Validators.required),
    replacementSolvent: new FormControl('', Validators.required),
    substitutionPercent: new FormControl<number | null>(null, [Validators.required, Validators.min(0)]),
    batchSizeKg: new FormControl<number | null>(null, [Validators.required, Validators.min(0)]),
    baselineVocGramsPerKg: new FormControl<number | null>(null, [Validators.required, Validators.min(0)]),
    replacementVocGramsPerKg: new FormControl<number | null>(null, [Validators.required, Validators.min(0)]),
    baselineCostPerKg: new FormControl<number | null>(null, [Validators.required, Validators.min(0)]),
    replacementCostPerKg: new FormControl<number | null>(null, [Validators.required, Validators.min(0)])
  });

  onSubmit(): void {
    if (this.form.invalid) return;
    const request = this.form.value as FormulationRequest;
    this.formulationService.calculateImpact(request).subscribe(result => {
      console.log(result);
      this.result.set(result);
    });
  }
}
