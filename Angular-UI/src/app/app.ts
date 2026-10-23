
import {
  Component,
  OnInit,
  ChangeDetectorRef,
  inject
} from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';

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
  imports: [DecimalPipe, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  private readonly campanyesService = inject(CampanyesService);
  private readonly cdr = inject(ChangeDetectorRef);

  // Creació de campanyes
  mostrarFormulari = false;
  creantCampanya = false;
  missatgeFormulari = '';

  novaCampanya = {
    nom: '',
    pressupostTotal: 0,
    dataInici: '',
    dataFi: ''
  };

  // Llistat i filtres
  filtre = 'TOTES';
  campanyes: Campanya[] = [];
  carregant = false;
  error = '';

  // Operacions sobre campanyes
  operacionsEnCurs = new Set<string>();
  missatgeOperacio = '';
  errorOperacio = '';

  // Formulari de consum de pressupost
  idCampanyaConsum: string | null = null;
  importConsum = 0;

  ngOnInit(): void {
    this.carregarCampanyes();
  }

  // --------------------------------------------------
  // CREAR CAMPANYA
  // --------------------------------------------------

  crearCampanya(): void {
    this.missatgeFormulari = '';

    const nom = this.novaCampanya.nom.trim();
    const pressupostTotal = Number(
      this.novaCampanya.pressupostTotal
    );
    const dataInici = this.novaCampanya.dataInici;
    const dataFi = this.novaCampanya.dataFi;

    if (nom.length < 3) {
      this.missatgeFormulari =
        'El nom ha de tenir almenys 3 caràcters.';
      return;
    }

    if (
      !Number.isFinite(pressupostTotal) ||
      pressupostTotal <= 0
    ) {
      this.missatgeFormulari =
        'El pressupost total ha de ser positiu.';
      return;
    }

    if (!dataInici || !dataFi || dataFi <= dataInici) {
      this.missatgeFormulari =
        'La data de finalització ha de ser posterior a la d’inici.';
      return;
    }

    this.creantCampanya = true;

    this.campanyesService.crear({
      nom,
      pressupostTotal,
      pressupostConsumit: 0,
      dataInici,
      dataFi
    }).subscribe({
      next: () => {
        this.creantCampanya = false;

        this.novaCampanya = {
          nom: '',
          pressupostTotal: 0,
          dataInici: '',
          dataFi: ''
        };

        this.mostrarFormulari = false;
        this.seleccionarFiltre('TOTES');
        this.carregarCampanyes();
        this.cdr.markForCheck();
      },
      error: (err: unknown) => {
        this.creantCampanya = false;

        this.missatgeFormulari = this.obtenirMissatgeError(
          err,
          "No s'ha pogut crear la campanya."
        );

        this.cdr.markForCheck();
      }
    });
  }

  // --------------------------------------------------
  // CARREGAR CAMPANYES
  // --------------------------------------------------

  carregarCampanyes(): void {
    this.carregant = true;
    this.error = '';

    this.campanyesService.llistar().subscribe({
      next: (resultat: CampanyaApi[]) => {
        try {
          this.campanyes = resultat.map(c => ({
            id: c.id,
            nom: c.nom,
            estat: this.convertirEstat(c.estat),
            pressupostTotal: Number(c.pressupostTotal),
            pressupostConsumit: Number(c.pressupostConsumit),
            dataInici: c.dataInici.substring(0, 10),
            dataFi: c.dataFi.substring(0, 10)
          }));

          this.carregant = false;
          this.cdr.markForCheck();
        } catch (err: unknown) {
          console.error(
            'Error processant les campanyes rebudes:',
            err
          );

          this.error =
            'Les dades rebudes de l’API no tenen el format esperat.';
          this.carregant = false;
          this.cdr.markForCheck();
        }
      },
      error: (err: unknown) => {
        console.error('Error carregant les campanyes:', err);

        this.error = this.obtenirMissatgeError(
          err,
          "No s'han pogut carregar les campanyes. Comprova que l'API estigui en execució."
        );

        this.carregant = false;
        this.cdr.markForCheck();
      }
    });
  }

  // --------------------------------------------------
  // CANVIAR ESTAT
  // --------------------------------------------------

  canviarEstat(
    campanya: Campanya,
    nouEstat: EstatCampanya
  ): void {
    if (campanya.estat === nouEstat) {
      return;
    }

    if (!this.transicioPermesa(campanya.estat, nouEstat)) {
      this.errorOperacio =
        `No es pot passar de ${campanya.estat} a ${nouEstat}.`;
      return;
    }

    const clau = `estat-${campanya.id}`;

    if (this.operacionsEnCurs.has(clau)) {
      return;
    }

    this.operacionsEnCurs.add(clau);
    this.errorOperacio = '';
    this.missatgeOperacio = '';

    this.campanyesService.canviarEstat(
      campanya.id,
      { estat: this.estatApi(nouEstat) }
    ).subscribe({
      next: () => {
        this.operacionsEnCurs.delete(clau);

        this.missatgeOperacio =
          `Estat de «${campanya.nom}» actualitzat correctament.`;

        this.carregarCampanyes();
        this.cdr.markForCheck();
      },
      error: (err: unknown) => {
        this.operacionsEnCurs.delete(clau);

        this.errorOperacio = this.obtenirMissatgeError(
          err,
          "No s'ha pogut canviar l'estat."
        );

        this.cdr.markForCheck();
      }
    });
  }

  private transicioPermesa(
    actual: EstatCampanya,
    nou: EstatCampanya
  ): boolean {
    const transicions: Record<EstatCampanya, EstatCampanya[]> = {
      ESBORRANY: ['ACTIVA'],
      ACTIVA: ['PAUSADA', 'FINALITZADA'],
      PAUSADA: ['ACTIVA', 'FINALITZADA'],
      FINALITZADA: []
    };

    return transicions[actual].includes(nou);
  }

  private estatApi(estat: EstatCampanya): number {
    const valors: Record<EstatCampanya, number> = {
      ESBORRANY: 1,
      ACTIVA: 2,
      PAUSADA: 3,
      FINALITZADA: 4
    };

    return valors[estat];
  }

  // --------------------------------------------------
  // COMPROVAR VIGÈNCIA
  // --------------------------------------------------

  campanyaEnVigencia(campanya: Campanya): boolean {
    const ara = new Date();

    const avui =
      `${ara.getFullYear()}-` +
      `${String(ara.getMonth() + 1).padStart(2, '0')}-` +
      `${String(ara.getDate()).padStart(2, '0')}`;

    return (
      campanya.dataInici <= avui &&
      avui <= campanya.dataFi
    );
  }

  // --------------------------------------------------
  // CONSUMIR PRESSUPOST
  // --------------------------------------------------

  obrirConsum(campanya: Campanya): void {
    if (campanya.estat !== 'ACTIVA') {
      this.errorOperacio =
        'Només es pot registrar consum en campanyes actives.';
      return;
    }

    if (!this.campanyaEnVigencia(campanya)) {
      this.errorOperacio =
        'No es pot registrar consum: la campanya està fora del període de vigència.';
      return;
    }

    this.idCampanyaConsum = campanya.id;
    this.importConsum = 0;
    this.errorOperacio = '';
    this.missatgeOperacio = '';
  }

  cancelarConsum(): void {
    this.idCampanyaConsum = null;
    this.importConsum = 0;
    this.errorOperacio = '';
  }

  registrarConsum(campanya: Campanya): void {
    const importEuro = Number(this.importConsum);

    const disponible =
      campanya.pressupostTotal - campanya.pressupostConsumit;

    if (campanya.estat !== 'ACTIVA') {
      this.errorOperacio =
        'Només es pot registrar consum en campanyes actives.';
      return;
    }

    if (!this.campanyaEnVigencia(campanya)) {
      this.errorOperacio =
        'No es pot registrar consum: la campanya està fora del període de vigència.';
      this.cdr.markForCheck();
      return;
    }

    if (!Number.isFinite(importEuro) || importEuro <= 0) {
      this.errorOperacio =
        "L'import ha de ser superior a zero.";
      return;
    }

    if (importEuro > disponible) {
      this.errorOperacio =
        `El consum supera el pressupost disponible (${this.formatImport(disponible)}).`;
      return;
    }

    const clau = `consum-${campanya.id}`;

    if (this.operacionsEnCurs.has(clau)) {
      return;
    }

    this.operacionsEnCurs.add(clau);
    this.errorOperacio = '';
    this.missatgeOperacio = '';

    this.campanyesService.consumirPressupost(
      campanya.id,
      { import: importEuro }
    ).subscribe({
      next: () => {
        this.operacionsEnCurs.delete(clau);
        this.idCampanyaConsum = null;
        this.importConsum = 0;

        this.missatgeOperacio =
          `Consum registrat correctament per a «${campanya.nom}».`;

        this.carregarCampanyes();
        this.cdr.markForCheck();
      },
      error: (err: unknown) => {
        this.operacionsEnCurs.delete(clau);

        this.errorOperacio = this.obtenirMissatgeError(
          err,
          "No s'ha pogut registrar el consum."
        );

        this.cdr.markForCheck();
      }
    });
  }

  // --------------------------------------------------
  // CONVERSIÓ D'ESTATS DE L'API
  // --------------------------------------------------

  private convertirEstat(
    estat: number | string
  ): EstatCampanya {
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

  // --------------------------------------------------
  // MISSATGES D'ERROR
  // --------------------------------------------------

  private obtenirMissatgeError(
    err: unknown,
    missatgeDefecte: string
  ): string {
    const errorHttp = err as {
      status?: number;
      error?: {
        error?: string;
        detail?: string;
        title?: string;
        message?: string;
      };
      message?: string;
    };

    return (
      errorHttp.error?.error ??
      errorHttp.error?.detail ??
      errorHttp.error?.title ??
      errorHttp.error?.message ??
      errorHttp.message ??
      `${missatgeDefecte} HTTP ${errorHttp.status ?? 'desconegut'}.`
    );
  }

  // --------------------------------------------------
  // FILTRES I ESTADÍSTIQUES
  // --------------------------------------------------

  get campanyesFiltrades(): Campanya[] {
    if (this.filtre === 'TOTES') {
      return this.campanyes;
    }

    return this.campanyes.filter(
      c => c.estat === this.filtre
    );
  }

  get pressupostTotal(): number {
    return this.campanyes.reduce(
      (total, c) => total + c.pressupostTotal,
      0
    );
  }

  get pressupostConsumit(): number {
    return this.campanyes.reduce(
      (total, c) => total + c.pressupostConsumit,
      0
    );
  }

  get campanyesActives(): number {
    return this.campanyes.filter(
      c => c.estat === 'ACTIVA'
    ).length;
  }

  get percentatgeConsumit(): number {
    return this.pressupostTotal > 0
      ? this.pressupostConsumit / this.pressupostTotal * 100
      : 0;
  }

  percentatgeCampanya(c: Campanya): number {
    return c.pressupostTotal > 0
      ? Math.min(
          100,
          c.pressupostConsumit / c.pressupostTotal * 100
        )
      : 0;
  }

  formatImport(importEuro: number): string {
    return new Intl.NumberFormat('ca-ES', {
      style: 'currency',
      currency: 'EUR',
      maximumFractionDigits: 2
    }).format(importEuro);
  }

  formatData(data: string): string {
    return new Intl.DateTimeFormat('ca-ES').format(
      new Date(`${data.substring(0, 10)}T12:00:00`)
    );
  }

  seleccionarFiltre(filtre: string): void {
    this.filtre = filtre;
  }
}
