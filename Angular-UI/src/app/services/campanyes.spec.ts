import { TestBed } from '@angular/core/testing';
import { Campanyes } from './campanyes';

describe('Campanyes', () => {
  let service: Campanyes;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Campanyes);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
