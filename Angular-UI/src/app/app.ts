
import { Component, OnInit, inject } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import {
  CampanyesService,
  Campanya as CampanyaApi
} from './services/campanyes.service';

type EstatCampanya =
  | 'ACTIVA'
  | 'ESBORRANY'
  | 'PAUSADA'
  | 'FINALITZADA';

interface Campanya {
  id: string;
  nom: string;
  estat: EstatCampanya;
  pressupostTotal: number;
  pressupostConsumit: number;
  dataInici: string;
  dataFi: string;
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [DecimalPipe],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  private readonly campanyesService = inject(CampanyesService);

  filtre = 'TOTES';
  campanyes: Campanya[] = [];
  carregant = false;
  error = '';

  ngOnInit(): void {
    this.carregarCampanyes();
  }

  carregarCampanyes(): void {
    this.carregant = true;
    this.error = '';

    this.campanyesService.llistar().subscribe({
      next: (resultat: CampanyaApi[]) => {
        this.campanyes = resultat.map(c => ({
          id: c.id,
          nom: c.nom,
          estat: this.convertirEstat(c.estat),
          pressupostTotal: c.pressupostTotal,
          pressupostConsumit: c.pressupostConsumit,
          dataInici: c.dataInici.substring(0, 10),
          dataFi: c.dataFi.substring(0, 10)
        }));

        this.carregant = false;
      },
      error: (err: unknown) => {
        console.error('Error carregant les campanyes:', err);
        this.error =
          "No s'han pogut carregar les campanyes. Comprova que l'API estigui en execució.";
        this.carregant = false;
      }
    });
  }

  private convertirEstat(estat: number | string): EstatCampanya {
    const estats: Record<number, EstatCampanya> = {
      1: 'ESBORRANY',
      2: 'ACTIVA',
      3: 'PAUSADA',
      4: 'FINALITZADA'
    };

    if (typeof estat === 'string') {
      const normalitzat = estat.toUpperCase();

      if (
        normalitzat === 'ESBORRANY' ||
        normalitzat === 'ACTIVA' ||
        normalitzat === 'PAUSADA' ||
        normalitzat === 'FINALITZADA'
      ) {
        return normalitzat;
      }

      const valorNumeric = Number(estat);

      if (estats[valorNumeric]) {
        return estats[valorNumeric];
      }
    }

    if (typeof estat === 'number' && estats[estat]) {
      return estats[estat];
    }

    throw new Error(`Estat de campanya desconegut: ${estat}`);
  }

  get campanyesFiltrades(): Campanya[] {
    if (this.filtre === 'TOTES') {
      return this.campanyes;
    }

    return this.campanyes.filter(c => c.estat === this.filtre);
  }

  get pressupostTotal(): number {
    return this.campanyes.reduce(
      (total, c) => total + c.pressupostTotal, 0
    );
  }

  get pressupostConsumit(): number {
    return this.campanyes.reduce(
      (total, c) => total + c.pressupostConsumit, 0
    );
  }

  get campanyesActives(): number {
    return this.campanyes.filter(c => c.estat === 'ACTIVA').length;
  }

  get percentatgeConsumit(): number {
    return this.pressupostTotal > 0
      ? this.pressupostConsumit / this.pressupostTotal * 100
      : 0;
  }

  formatImport(importEuro: number): string {
    return new Intl.NumberFormat('ca-ES', {
      style: 'currency',
      currency: 'EUR',
      maximumFractionDigits: 0
    }).format(importEuro);
  }

  formatData(data: string): string {
    return new Intl.DateTimeFormat('ca-ES').format(
      new Date(`${data}T12:00:00`)
    );
  }

  percentatgeCampanya(c: Campanya): number {
    return c.pressupostTotal > 0
      ? c.pressupostConsumit / c.pressupostTotal * 100
      : 0;
  }

  seleccionarFiltre(filtre: string): void {
    this.filtre = filtre;
  }
}
