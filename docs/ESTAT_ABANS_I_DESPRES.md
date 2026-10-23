# Estat del Projecte: Abans i Després de les Modificacions

Aquest document detalla de manera exhaustiva l'estat del repositori **CampaignManager** abans d'iniciar la intervenció d'auditoria i correcció, i l'estat final verificat un cop completades totes les fases del pla.

---

## 1. Resum comparatiu global

| Àmbit | Estat Inicial (Abans) | Estat Final (Després) |
| :--- | :--- | :--- |
| **Tests Backend (.NET)** | 19 tests (18 de domini bàsics + 1 buit). Cap test de serveis ni controladors. | **56 tests automatitzats** (Domini, Serveis i Controladors) amb fakes en memòria. 100% exitosos. |
| **Tests Frontend (Angular)** | **1 test fallit** per text de plantilla CLI i 1 test d'una classe orfe. `npm test` fallava amb codi 1. | **8 tests automatitzats** (Components i Serveis HTTP amb mocks). 100% exitosos en ~1.5 segons. |
| **Plantilla Angular (`app.html`)** | Encoixinada amb blocs literals de markdown (```` ```html ```` i ```` ``` ````) i `colspan="6"` erroni. | Codi HTML pur, net, accessible i `colspan="7"` corregit segons les columnes reals. |
| **Regles de negoci (UI)** | Botons manuals `Finalitzar` presents a la taula per a campanyes actives i pausades. | **Botons manuals eliminats.** Només s'admet la finalització automàtica per consum total. |
| **Transicions manuals (`app.ts`)** | Admetia `FINALITZADA` com a transició manual a `transicioPermesa()`. | Restringit estrictament a: `ESBORRANY -> ACTIVA`, `ACTIVA -> PAUSADA`, `PAUSADA -> ACTIVA`. |
| **Entitat de domini (`Campaign.cs`)** | Permetia pausar una campanya en estat `ESBORRANY` sense haver estat mai activada. | `Pausar()` exigeix estrictament que la campanya estigui en estat `ACTIVA`. |
| **Contracte DTO (`CrearCampanyaDTO`)** | Exigia `PressupostConsumit` obligatòriament al JSON d'entrada. | `PressupostConsumit` té valor per defecte `0`, opcional a la càrrega útil del client. |
| **Serialització d'Enums** | Sense `JsonStringEnumConverter`: retornava enters (`1, 2, 3, 4`) en lloc dels estats de l'enunciat. | `JsonStringEnumConverter` afegit: retorna i accepta `"ESBORRANY"`, `"ACTIVA"`, etc. |
| **Filtre de llistat (`GET /campaigns`)** | Un filtre invàlid (`?estat=99`) retornava llista buida amb codi 200 OK. | Retorna codi **`400 Bad Request`** amb missatge d'error estructurat. |
| **Persistència SQLite a l'inici** | Si no existia `campanyes.db`, l'arrencada fallava amb taules no trobades. | **`db.Database.Migrate()`** s'executa automàticament a l'inici creant la base de dades. |
| **Documentació** | `README.md` d'una sola línia (`# MarketingCampaignsManager`). | Documentació completa: `README.md`, `ARQUITECTURA.md`, `MILLORES_I_REFLEXIO.md` i `ESTAT_ABANS_I_DESPRES.md`. |

---

## 2. Detall dels Canvis per Capa

### 2.1. Capa de Domini (`Domain`)

#### Fitxer: `Domain/Entities/Campaign.cs`
- **Abans:**
  ```csharp
  public void Pausar()
  {
      if (Estat == EstatCampanya.FINALITZADA)
          throw new InvalidOperationException("Una campanya finalitzada no es pot modificar.");

      Estat = EstatCampanya.PAUSADA;
  }
  ```
  *Problema:* Si la campanya estava en estat `ESBORRANY`, passava directament a `PAUSADA`, saltant-se el cicle de vida vàlid d'una campanya.
- **Després:**
  ```csharp
  public void Pausar()
  {
      if (Estat == EstatCampanya.FINALITZADA)
          throw new InvalidOperationException("Una campanya finalitzada no es pot modificar.");

      if (Estat != EstatCampanya.ACTIVA)
          throw new InvalidOperationException("Només es pot pausar una campanya activa.");

      Estat = EstatCampanya.PAUSADA;
  }
  ```
  *Benefici:* El domini protegeix que només es pugui pausar una campanya que hagi estat prèviament activada i estigui en curs.

---

### 2.2. Capa d'Aplicació (`Application`)

#### Fitxer: `Application/DTOs/CrearCampanyaDTO.cs`
- **Abans:**
  ```csharp
  public sealed record CrearCampanyaDTO(
      string Nom,
      decimal PressupostTotal,
      decimal PressupostConsumit,
      DateTimeOffset DataInici,
      DateTimeOffset DataFi);
  ```
  *Problema:* Si un client o test extern enviava un JSON estàndard `{ nom, pressupostTotal, dataInici, dataFi }` sense `pressupostConsumit`, la deserialització podia fallar o exigir un camp que per negoci sempre neix a zero.
- **Després:**
  ```csharp
  public sealed record CrearCampanyaDTO(
      string Nom,
      decimal PressupostTotal,
      DateTimeOffset DataInici,
      DateTimeOffset DataFi,
      decimal PressupostConsumit = 0);
  ```

#### Fitxer: `Application/Services/CampanyaService.cs`
- **Abans:**
  A `LlistarAsync`, no es validava si `estat` era un valor definit a l'enum:
  ```csharp
  var resultat = estat.HasValue
      ? campanyes.Where(c => c.Estat == estat.Value)
      : campanyes;
  ```
- **Després:**
  S'ha afegit la validació explícita d'existència de l'enum:
  ```csharp
  if (estat.HasValue && !Enum.IsDefined(estat.Value))
  {
      throw new ArgumentException("L'estat especificat no és vàlid.", nameof(estat));
  }
  ```

---

### 2.3. Capa de Presentació / API (`CampaignApi`)

#### Fitxer: `CampaignApi/Controllers/CampaignsController.cs`
- **Abans:**
  El mètode `Llistar` no capturava cap excepció d'arguments, exposant possibles errors 500 davant de paràmetres fora de rang.
- **Després:**
  S'ha afegit gestió d'excepcions per retornar `400 BadRequest`:
  ```csharp
  [HttpGet]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<IReadOnlyList<CampanyaDTO>>> Llistar(
      [FromQuery] EstatCampanya? estat,
      CancellationToken cancellationToken)
  {
      try
      {
          var campanyes = await _service.LlistarAsync(estat, cancellationToken);
          return Ok(campanyes);
      }
      catch (ArgumentException ex)
      {
          return BadRequest(new { error = ex.Message });
      }
  }
  ```

#### Fitxer: `CampaignApi/Program.cs`
- **Abans:**
  - `builder.Services.AddControllers();` sense convertidors d'enum. Les respostes JSON mostraven enters `1, 2, 3, 4` en lloc dels textos de l'enunciat (`"ACTIVA"`, `"ESBORRANY"`, etc.).
  - Sense migració automàtica de la base de dades a l'arrencada.
- **Després:**
  - S'ha afegit `.AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));`.
  - S'ha afegit el bloc d'autocreació i migració de la base de dades SQLite:
    ```csharp
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
    ```

---

### 2.4. Frontend Angular (`Angular-UI`)

#### Fitxer: `Angular-UI/src/app/app.html`
- **Abans:**
  1. La primera línia contenia ```` ```html ```` i la darrera ```` ``` ````, provocant renderitzats visuals defectuosos de marcadors markdown a la pantalla.
  2. A les accions de la taula (línies 427-434 i 445-452) existien dos botons de **Finalitzar**:
     ```html
     <button type="button" class="secondary-button" (click)="canviarEstat(campanya, 'FINALITZADA')">
       Finalitzar
     </button>
     ```
     *Incompliment:* L'enunciat funcional prohibeix explícitament transicions manuals cap a `FINALITZADA`.
  3. A les files d'estat buit es feia servir `colspan="6"` quan la taula té 7 columnes (`CAMPANYA`, `ESTAT`, `PERÍODE`, `PRESSUPOST`, `PRESSUPOST RESTANT`, `CONSUM`, `ACCIONS`).
- **Després:**
  1. Eliminats els marcadors de markdown.
  2. Eliminats els dos botons manuals de `Finalitzar`.
  3. Corregit el `colspan="7"` a les files buides.
  4. Afegida protecció `[disabled]="operacionsEnCurs.has('consum-' + campanya.id)"` al botó d'obertura de consum.

#### Fitxer: `Angular-UI/src/app/app.ts`
- **Abans:**
  ```typescript
  const transicions: Record<EstatCampanya, EstatCampanya[]> = {
    ESBORRANY: ['ACTIVA'],
    ACTIVA: ['PAUSADA', 'FINALITZADA'],
    PAUSADA: ['ACTIVA', 'FINALITZADA'],
    FINALITZADA: []
  };
  ```
- **Després:**
  ```typescript
  const transicions: Record<EstatCampanya, EstatCampanya[]> = {
    ESBORRANY: ['ACTIVA'],
    ACTIVA: ['PAUSADA'],
    PAUSADA: ['ACTIVA'],
    FINALITZADA: []
  };
  ```

#### Fitxers de test: `app.spec.ts` i `campanyes.spec.ts`
- **Abans:**
  - `app.spec.ts`: Fallava amb `AssertionError: expected 'Les teves campanyes' to contain 'Hello, campaign-manager-ui'` i llançava errors `ECONNREFUSED` perquè `ngOnInit` feia crides reals per xarxa sense mock.
  - `campanyes.spec.ts`: Testejava la classe `Campanyes` (fitxer buit no utilitzat) en lloc del servei real `CampanyesService`.
- **Després:**
  - `app.spec.ts`: Proveeix un `CampanyesService` simulat en memòria, verifica la creació, el títol real i la lògica de filtratge de campanyes.
  - `campanyes.spec.ts`: Suite completa amb `HttpTestingController` verificant tots els mètodes HTTP del servei (`llistar`, `obtenirPerId`, `crear`, `canviarEstat`, `consumirPressupost`).

---

### 2.5. Bateria de Proves (.NET)

#### Projecte: `CampaignManager.Tests`
- **Abans:**
  - El fitxer de projecte `Tests.csproj` només referenciava `Domain.csproj`. No podia compilar ni executar tests contra `Application` o `CampaignApi`.
  - Només hi havia 1 fitxer de test de domini (`CampanyaTests.cs`) amb 18 proves i un `UnitTest1.cs` amb un test buit.
  - **Mancances greus de cobertura:** No es comprovava la validació de dates invertides a la creació, ni l'acumulació de consums successius, ni el rebuig de consums posteriors a la data final, ni el rebuig de pausar esborranys, ni el comportament de la capa de serveis, ni els codis HTTP (201, 204, 400, 404, 409) dels controladors.
- **Després:**
  - `Tests.csproj` referencia `Domain`, `Application` i `CampaignApi`.
  - Creat el directori `Fakes/` amb implementacions lleugeres en memòria:
    - `FakeGenericRepository<TEntity>`
    - `FakeUnitOfWork`
    - `TestTimeProvider`
  - Creat `Services/CampanyaServiceTests.cs` (17 tests cobrint tots els casos d'ús i excepcions).
  - Creat `Controllers/CampaignsControllerTests.cs` (19 tests cobrint tots els codis de resposta HTTP).
  - Ampliat `CampanyaTests.cs` a 20 proves unitàries de domini.
  - Eliminat `UnitTest1.cs`.
  - **Resultat final: 56 tests passats al 100% en 60 ms.**

---

## 3. Taula Resum de Verificació Funcional

| Flux Funcional | Comportament Inicial | Comportament Corregit | Codi HTTP / Resposta |
| :--- | :--- | :--- | :--- |
| **Crear campanya vàlida** | Funcionava amb consum opcional ambigu. | Genera ID, estat inicial `ESBORRANY`, consum `0`. | `201 Created` |
| **Activar campanya** | Permès per a esborrany i pausada. | Validat temporalment i per saldo disponible. | `204 NoContent` |
| **Pausar esborrany** | **S'acceptava incorrectament.** | **Es rebutja amb conflicte.** | `409 Conflict` |
| **Finalitzar manualment** | **Permès des del botó de la UI.** | **Prohibit. El botó s'ha suprimit.** | N/A (Bloquejat a UI i API) |
| **Consum parcial** | Registrava consum. | Registra consum i s'acumula correctament. | `204 NoContent` |
| **Consum exacte (100%)** | Passava a `FINALITZADA`. | Passa automàticament a `FINALITZADA`. | `204 NoContent` |
| **Reactivar campanya finalitzada** | Rebutjat pel domini. | Rebutjat amb missatge d'error clar. | `409 Conflict` |
| **Filtre d'estat invàlid** | **Retornava llista buida amb 200 OK.** | **Retorna error de validació controlat.** | `400 BadRequest` |
| **Execució de tests .NET** | 19 tests. | **56 tests.** | `dotnet test` (0 fallits) |
| **Execució de tests Angular** | **1 fallit / errors de xarxa.** | **8 tests passats.** | `npm test` (0 fallits) |
