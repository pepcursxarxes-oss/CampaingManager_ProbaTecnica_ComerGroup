import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  provideHttpClientTesting,
  HttpTestingController
} from '@angular/common/http/testing';
import {
  CampanyesService,
  CrearCampanya,
  CanviarEstatCampanya,
  ConsumirPressupost
} from './campanyes.service';

describe('CampanyesService', () => {
  let service: CampanyesService;
  let httpTesting: HttpTestingController;
  const baseUrl = 'https://localhost:7128/campaigns';

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        CampanyesService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(CampanyesService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('llistar() should call GET /campaigns', () => {
    service.llistar().subscribe(data => {
      expect(data.length).toBe(1);
      expect(data[0].nom).toBe('Test');
    });

    const req = httpTesting.expectOne(baseUrl);
    expect(req.request.method).toBe('GET');
    req.flush([
      {
        id: '1',
        nom: 'Test',
        pressupostTotal: 1000,
        pressupostConsumit: 0,
        dataInici: '2026-10-01',
        dataFi: '2026-11-01',
        estat: 1
      }
    ]);
  });

  it('crear() should call POST /campaigns', () => {
    const dto: CrearCampanya = {
      nom: 'Nova Campanya',
      pressupostTotal: 500,
      pressupostConsumit: 0,
      dataInici: '2026-10-01',
      dataFi: '2026-11-01'
    };

    service.crear(dto).subscribe(res => {
      expect(res.id).toBe('generated-id');
    });

    const req = httpTesting.expectOne(baseUrl);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(dto);
    req.flush({ id: 'generated-id' });
  });

  it('canviarEstat() should call PATCH /campaigns/{id}/status', () => {
    const dto: CanviarEstatCampanya = { estat: 2 };

    service.canviarEstat('test-id', dto).subscribe();

    const req = httpTesting.expectOne(`${baseUrl}/test-id/status`);
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual(dto);
    req.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('consumirPressupost() should call POST /campaigns/{id}/consume', () => {
    const dto: ConsumirPressupost = { import: 50 };

    service.consumirPressupost('test-id', dto).subscribe();

    const req = httpTesting.expectOne(`${baseUrl}/test-id/consume`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(dto);
    req.flush(null, { status: 204, statusText: 'No Content' });
  });
});
