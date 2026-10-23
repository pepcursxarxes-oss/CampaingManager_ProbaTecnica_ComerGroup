
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Campanya {
  id: string;
  nom: string;
  pressupostTotal: number;
  pressupostConsumit: number;
  dataInici: string;
  dataFi: string;
  estat: number;
}

export interface CrearCampanya {
  nom: string;
  pressupostTotal: number;
  pressupostConsumit: number;
  dataInici: string;
  dataFi: string;
}

export interface CanviarEstatCampanya {
  estat: number;
}

export interface ConsumirPressupost {
  import: number;
}

@Injectable({
  providedIn: 'root'
})
export class CampanyesService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5101/campaigns';

  llistar(): Observable<Campanya[]> {
    return this.http.get<Campanya[]>(this.apiUrl);
  }

  obtenirPerId(id: string): Observable<Campanya> {
    return this.http.get<Campanya>(`${this.apiUrl}/${id}`);
  }

  crear(dto: CrearCampanya): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.apiUrl, dto);
  }

  canviarEstat(
    id: string,
    dto: CanviarEstatCampanya
  ): Observable<void> {
    return this.http.patch<void>(
      `${this.apiUrl}/${id}/status`,
      dto
    );
  }

  consumirPressupost(
    id: string,
    dto: ConsumirPressupost
  ): Observable<void> {
    return this.http.post<void>(
      `${this.apiUrl}/${id}/consume`,
      dto
    );
  }
}
