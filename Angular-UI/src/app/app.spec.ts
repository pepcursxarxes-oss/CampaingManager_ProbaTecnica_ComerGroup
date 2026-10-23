import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { App } from './app';
import { CampanyesService } from './services/campanyes.service';

describe('App', () => {
  const fakeCampanyesService = {
    llistar: () => of([]),
    crear: () => of({ id: 'test-id' }),
    canviarEstat: () => of(void 0),
    consumirPressupost: () => of(void 0),
    obtenirPerId: () => of({
      id: 'test-id',
      nom: 'Campanya Test',
      pressupostTotal: 1000,
      pressupostConsumit: 0,
      dataInici: '2026-10-01',
      dataFi: '2026-11-01',
      estat: 1
    })
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        { provide: CampanyesService, useValue: fakeCampanyesService }
      ]
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render title', async () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Les teves campanyes');
  });

  it('should filter campaigns correctly', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    app.campanyes = [
      {
        id: '1',
        nom: 'Campanya 1',
        estat: 'ACTIVA',
        pressupostTotal: 500,
        pressupostConsumit: 100,
        dataInici: '2026-10-01',
        dataFi: '2026-11-01'
      },
      {
        id: '2',
        nom: 'Campanya 2',
        estat: 'ESBORRANY',
        pressupostTotal: 1000,
        pressupostConsumit: 0,
        dataInici: '2026-10-01',
        dataFi: '2026-11-01'
      }
    ];

    app.seleccionarFiltre('TOTES');
    expect(app.campanyesFiltrades.length).toBe(2);

    app.seleccionarFiltre('ACTIVA');
    expect(app.campanyesFiltrades.length).toBe(1);
    expect(app.campanyesFiltrades[0].id).toBe('1');

    app.seleccionarFiltre('FINALITZADA');
    expect(app.campanyesFiltrades.length).toBe(0);
  });
});
