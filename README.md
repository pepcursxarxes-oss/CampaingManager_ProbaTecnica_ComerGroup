# MarketingCampaignsManager — CampaignManager

Sistema de gestió de campanyes de màrqueting i control de pressupost, desenvolupat com a prova tècnica per a Comergrup. Inclou un backend en **.NET 10** seguint arquitectura neta i un frontend en **Angular 22**.

---

## 📚 Documentació detallada

A la carpeta [`docs/`](./docs/) trobareu documentació monogràfica addicional:

- **[Estat del Projecte: Abans i Després (`docs/ESTAT_ABANS_I_DESPRES.md`)](./docs/ESTAT_ABANS_I_DESPRES.md)**: Comparativa exhaustiva i taula d'auditoria de l'estat del repositori abans i després de les correccions aplicades.
- **[Arquitectura del Sistema (`docs/ARQUITECTURA.md`)](./docs/ARQUITECTURA.md)**: Detall complet de les capes del backend (Domain, Application, Infrastructure, CampaignApi), flux de dades, diagrames Mermaid, patrons utilitzats i frontend Angular.
- **[Millores Futures i Reflexió d'Enginyeria (`docs/MILLORES_I_REFLEXIO.md`)](./docs/MILLORES_I_REFLEXIO.md)**: Propostes d'escalabilitat productiva (concurrència optimista, CQRS/Domain Events, paginació, observabilitat) i **nota personal/professional del desenvolupador** sobre el balanç entre sobre-enginyeria, escalabilitat i ús transparent d'IA.

---

## 1. Requisits previs

- **.NET SDK 10.0** o superior (`dotnet --version`)
- **Node.js** (v20 o superior recomanat) i **npm** (`npm --version`)
- Navegador web modern (Chrome, Edge, Firefox)

---

## 2. Instruccions d'execució

### Backend (.NET API)

1. Situar-se a l'arrel del repositori o a la carpeta del projecte API:
   ```bash
   cd CampaignApi
   ```

2. Restaurar dependències i compilar:
   ```bash
   dotnet restore
   dotnet build
   ```

3. Iniciar l'API:
   - **Amb perfil HTTP (recomanat per a desenvolupament)**:
     ```bash
     dotnet run --launch-profile http
     ```
     L'API estarà disponible a `http://localhost:5101`.

   - **Amb perfil HTTPS**:
     ```bash
     dotnet run --launch-profile https
     ```
     L'API estarà disponible a `https://localhost:7128` i `http://localhost:5101`.

> **Nota sobre la base de dades:** En iniciar l'API, s'executa automàticament `db.Database.Migrate()`, creant el fitxer de base de dades SQLite `campanyes.db` i aplicant totes les migracions si no existia prèviament.

### Frontend (Angular)

1. Situar-se a la carpeta `Angular-UI`:
   ```bash
   cd Angular-UI
   ```

2. Instal·lar les dependències (si no estan instal·lades):
   ```bash
   npm install
   ```

3. Iniciar el servidor de desenvolupament:
   ```bash
   npm start
   ```
   L'aplicació estarà disponible a `http://localhost:4200`.

4. Compilar per a producció:
   ```bash
   npm run build
   ```

---

## 3. Comandes per executar els tests

### Tests de Backend (.NET)
Des de l'arrel del projecte:
```bash
dotnet test
```
Executa els 56 tests que cobreixen entitats de domini, serveis d'aplicació i controladors API.

### Tests de Frontend (Angular)
Des de la carpeta `Angular-UI`:
```bash
npm test
```
Executa la suite de Vitest / Angular Testing (8 tests verificant components, filtratge, títol i servei HTTP).

---

## 4. Estructura i decisions d'arquitectura

El projecte està estructurat seguint els principis d'**Arquitectura Neta** (Clean Architecture):

```text
MarketingCampaignsManager/
├── docs/                       # Documentació addicional d'arquitectura i reflexió
│   ├── ESTAT_ABANS_I_DESPRES.md # Comparativa d'auditoria abans i després
│   ├── ARQUITECTURA.md         # Detall exhaustiu de patrons, capes i diagrames
│   └── MILLORES_I_REFLEXIO.md  # Millores productives i nota professional de l'autor
├── Domain/                     # Entitats, Enums i Regles de negoci pures
│   ├── Entities/Campaign.cs    # Entitat Campanya amb invariables i mètodes de domini
│   └── Enums/EstatCampanya.cs  # ESBORRANY, ACTIVA, PAUSADA, FINALITZADA
├── Application/                # Casos d'ús, DTOs i interfícies
│   ├── DTOs/                   # DTOs de transferència i validació de dades
│   ├── Interfaces/             # ICampanyaService, IGenericRepository, IUnitOfWork
│   └── Services/               # CampanyaService (orquestració de la lògica d'aplicació)
├── Infrastructure/             # Persistència i implementació d'interfícies
│   ├── Persistence/            # AppDbContext (EF Core) i migracions SQLite
│   └── Repositories/           # GenericRepository i UnitOfWork
├── CampaignApi/                # Presentació / API REST ASP.NET Core
│   ├── Controllers/            # CampaignsController (exposició dels endpoints)
│   └── Program.cs              # Configuració de DI, CORS, JSON converters i migració
├── CampaignManager.Tests/      # Bateria de proves automatitzades .NET (xUnit)
│   ├── Domain/Entities/        # Tests unitaris de les invariables de Campanya
│   ├── Services/               # Tests de CampanyaService amb fakes en memòria
│   ├── Controllers/            # Tests dels codis HTTP i respostes de CampaignsController
│   └── Fakes/                  # Implementacions simulades (Repository, UnitOfWork, TimeProvider)
└── Angular-UI/                 # Frontend SPA en Angular
    └── src/app/
        ├── app.ts / app.html   # Component principal amb taula, estadístiques i formularis
        └── services/           # CampanyesService (comunicació HTTP amb l'API)
```

---

## 5. Persistència utilitzada

- **SQLite** mitjançant **Entity Framework Core 10**.
- Cadena de connexió: `Data Source=campanyes.db`.
- Les dades es mantenen entre reinicis de l'aplicació.
- A l'arrencada de l'API (`Program.cs`), s'executa `Database.Migrate()` de manera segura dins d'un scope de servei, garantint que l'esquema s'autogenera si el fitxer de base de dades no existeix.

---

## 6. Regles de negoci implementades

1. **Creació (`POST /campaigns`)**:
   - Estat inicial fixat automàticament a `ESBORRANY`.
   - Consum inicial fixat estrictament a `0`.
   - El pressupost total ha de ser estrictament positiu (`> 0`).
   - El nom ha de tenir almenys 3 caràcters no buits.
   - La data de finalització (`dataFi`) ha de ser estrictament posterior a la d'inici (`dataInici`).

2. **Canvi d'estat (`PATCH /campaigns/{id}/status`)**:
   - Només es permeten canvis manuals cap a `ACTIVA` o `PAUSADA`.
   - Només es pot pausar una campanya que estigui `ACTIVA`.
   - No es pot activar una campanya si la data de finalització ja ha vençut.
   - No es pot activar una campanya si no li queda saldo disponible (`pressupostConsumit >= pressupostTotal`).
   - Una campanya en estat `FINALITZADA` queda bloquejada per a qualsevol modificació posterior.
   - No existeix cap operació manual per forçar l'estat a `FINALITZADA`.

3. **Consum de pressupost (`POST /campaigns/{id}/consume`)**:
   - L'import a consumir ha de ser estrictament superior a 0 (`decimal`).
   - Només es pot consumir d'una campanya en estat `ACTIVA`.
   - La data actual ha d'estar dins del període de vigència (`dataInici <= ara <= dataFi`).
   - L'import no pot superar el pressupost restant disponible.
   - El consum s'acumula: `nouConsum = consumAnterior + import`.
   - Si el consum acumulat arriba exactament al pressupost total, la campanya passa automàticament a `FINALITZADA`.
   - Si una petició és rebutjada, cap dada es modifica parcialment.

4. **Llistat i filtratge (`GET /campaigns`)**:
   - Retorna les campanyes persistides.
   - Suporta el paràmetre de consulta opcional `?estat=...` (admetent tant el nom com `"ACTIVA"` com el valor numèric).
   - Valida que el filtre correspongui a un estat vàlid; en cas contrari retorna `400 Bad Request`.

---

## 7. Tractament d'errors i codis HTTP

L'API utilitza codis d'estat HTTP semàntics i un format JSON consistent (`{ "error": "..." }`):

- **`200 OK`**: Peticions de consulta correctes (`GET`).
- **`201 Created`**: Campanya creada correctament (`POST /campaigns`), amb la capçalera `Location`.
- **`204 NoContent`**: Actualització d'estat o consum correctes sense cos de resposta.
- **`400 BadRequest`**: Dades d'entrada invàlides, formats incorrectes, nom curt, pressupost no positiu o filtre no permès.
- **`404 NotFound`**: Campanya no trobada pel seu identificador `Guid`.
- **`409 Conflict`**: Conflicte de negoci (activar campanya caducada, consumir fora de vigència, superar pressupost disponible, reactivar una campanya finalitzada).

---

## 8. Millores futures realistes per a producció

1. **Concurrència optimista per al consum:** Incorporar un token de concurrència (`RowVersion` o `ConcurrencyCheck` a EF Core) sobre la columna de consum o campanya, per garantir la consistència en entorns d'alt volum de peticions simultànies.
2. **Paginació i ordenació al llistat:** Afegir paràmetres `pageNumber` i `pageSize` a `GET /campaigns` per evitar la càrrega excessiva de registres en entorns corporatius amb milers de campanyes.
3. **Autenticació i autorització:** Integrar seguretat mitjançant tokens JWT (OAuth2 / OpenID Connect) per restringir qui pot crear o modificar campanyes.
4. **Observabilitat:** Integrar Serilog / OpenTelemetry per a mètriques de rendiment, traçabilitat distribuïda i registre centralitzat d'auditoria.
5. **Entorns configurables a Angular:** Utilitzar fitxers `environment.ts` a Angular per alternar de manera transparent entre la URL de desenvolupament local i entorns de preproducció/producció.

*(Vegeu [`docs/MILLORES_I_REFLEXIO.md`](./docs/MILLORES_I_REFLEXIO.md) per a una anàlisi aprofundida d'aquestes millores).*

---

## 9. Prompts que s'han utilitzat amb IA

Durant la realització d'aquesta auditoria i finalització de la prova tècnica, s'han emprat les següents interaccions i prompts:

1. **Prompt d'inici i context:**
   - `@[c:\Users\joewi\Desktop\docs\pla_finalitzacio_campaignmanager.md]` — S'ha proporcionat el document amb l'enunciat funcional, els criteris de qualitat i el pla de finalització per fases.
2. **Prompt de control d'execució:**
   - `stop` — Aturada temporal sol·licitada per revisar l'estat parcial assolit (diagnòstic, backend corregit, frontend corregit i tests preparats).
   - `continue` — Represa de la feina per executar la bateria completa de tests, la validació integral amb peticions reals i la redacció de la documentació final.
3. **Prompt de documentació i reflexió d'arquitectura:**
   - Petició de generació de documents monogràfics sobre l'arquitectura creada, millores futures i nota professional/personal de l'autor sobre el balanç de sobre-enginyeria vs. escalabilitat i ús transparent d'IA.
4. **Prompt de documentació comparativa abans/després:**
   - Petició de generació d'un document amb el detall comparatiu de l'estat del projecte abans i després de les modificacions aplicades.
