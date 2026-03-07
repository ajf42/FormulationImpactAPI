import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { FormulationRequest, FormulationResult } from '../models/formulation.models';

const API_URL = 'http://localhost:5000/api/formulation/impact';

@Injectable({ providedIn: 'root' })
export class FormulationService {
  // inject() is the Angular 19 preferred pattern over constructor injection —
  // it works outside the constructor and keeps the class signature clean.
  private http = inject(HttpClient);

  // Posts a substitution request to the API and returns the calculated impact.
  calculateImpact(request: FormulationRequest): Observable<FormulationResult> {
    return this.http.post<FormulationResult>(API_URL, request);
  }
}
