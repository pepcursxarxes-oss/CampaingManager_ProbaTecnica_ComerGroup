# Guia d'Ús i Manual de Proves del Programari — CampaignManager

Aquesta guia proporciona un manual complet i pas a pas per provar totes les funcionalitats del projecte **CampaignManager**, tant a través de la **interfície visual d'usuari (UI Angular)** com mitjançant **crides directes a l'API REST**.

---

## 1. Arrencada de l'Entorn

Per provar el sistema cal tenir en marxa tant el servei d'API com el servidor web d'Angular.

### Terminal 1: Iniciar el Backend (.NET API)
```bash
cd C:\MarketingCampaignsManager\CampaignApi
dotnet run --launch-profile http
```
- L'API arrencarà escoltant a: **`http://localhost:5101`**
- La base de dades SQLite (`campanyes.db`) s'autogenerarà i aplicarà les migracions automàticament en iniciar.

### Terminal 2: Iniciar el Frontend (Angular)
```bash
cd C:\MarketingCampaignsManager\Angular-UI
npm start
```
- L'aplicació estarà disponible al navegador a: **`http://localhost:4200`**

---

## 2. Proves a través de la Interfície Web (UI Angular)

Obriu el navegador web a [http://localhost:4200](http://localhost:4200).

```text
+---------------------------------------------------------------------------------+
|  campaignmanager   | Espai de treball / Campanyes                               |
|--------------------+------------------------------------------------------------|
|  [▦] Campanyes     |  Les teves campanyes          [Esborranys] [+ Nova campanya]
|  [◫] Pressupostos  |  --------------------------------------------------------  |
|  [↗] Anàlisi       |  [ Total: 7 ]  [ Actives: 4 ]  [ Total: 375k€ ] [ Consum ] |
|                    |  Filtres: [Totes] [Actives] [Esborranys] [Pausades] [...]  |
|                    |  Taula de campanyes amb accions per fila                   |
+---------------------------------------------------------------------------------+
```

### Prova 1: Visualització de mètriques i llistat existent
1. En carregar la pàgina, observeu les 4 targetes d'estadístiques superiors:
   - **Total de campanyes**: Nombre total registrat a la base de dades.
   - **Campanyes actives**: Nombre de campanyes en execució.
   - **Pressupost total**: Suma del pressupost de totes les campanyes.
   - **Pressupost consumit**: Import global gastat i barra de progrés percentual.
2. A la taula inferior, observeu les columnes:
   - `CAMPANYA`, `ESTAT`, `PERÍODE`, `PRESSUPOST`, **`PRESSUPOST RESTANT`** (calculat automàticament), `CONSUM` i `ACCIONS`.

### Prova 2: Filtrar campanyes per estat
1. Feu clic als botons de filtre situats sobre la taula:
   - **Actives**: Només mostra les campanyes en estat `ACTIVA`.
   - **Esborranys**: Mostra les campanyes en estat `ESBORRANY`.
   - **Pausades**: Mostra les campanyes en estat `PAUSADA`.
   - **Finalitzades**: Mostra les campanyes en estat `FINALITZADA`.
   - **Totes**: Restableix la vista completa.
2. Comproveu que el comptador de «X resultats» s'actualitza de forma instantània.

### Prova 3: Crear una nova campanya amb validacions
1. Feu clic al botó blau superior **`+ Nova campanya`**. Es desplegarà el formulari de creació.
2. **Provant validacions**:
   - Deixeu el nom amb menys de 3 caràcters (ex. `AB`) i feu clic a «Crear campanya».
     - *Resultat:* Es mostra l'error en vermell: *«El nom ha de tenir almenys 3 caràcters.»*
   - Poseu un pressupost de `0` o negatiu.
     - *Resultat:* Error: *«El pressupost total ha de ser positiu.»*
   - Poseu una data de finalització anterior a la data d'inici.
     - *Resultat:* Error: *«La data de finalització ha de ser posterior a la d'inici.»*
3. **Creació correcta**:
   - Nom: `Campanya Black Friday`
   - Pressupost: `1000`
   - Data d'inici: Trieu la data d'ahir o d'avui.
   - Data de finalització: Trieu una data futura (ex. d'aquí a un mes).
   - Feu clic a **«Crear campanya»**.
4. *Resultat:* El formulari es tanca, la taula s'actualitza automàticament i la nova campanya apareix al llistat en estat **`ESBORRANY`**, amb consum **`0,00 €`** i pressupost restant de **`1.000,00 €`**.

### Prova 4: Activar la campanya
1. Cerqueu la campanya acabada de crear (`Campanya Black Friday`).
2. A la columna d'accions, observeu que només hi ha el botó **`Activar`**.
3. Feu clic a **`Activar`**.
4. *Resultat:*
   - Es mostra el missatge d'èxit verd: *«Estat de «Campanya Black Friday» actualitzat correctament.»*
   - L'estat canvia visualment a **`ACTIVA`** (amb xip verd).
   - La targeta superior de «Campanyes actives» s'incrementa en 1.
   - Apareixen les noves accions: **`Pausar`** i **`Registrar consum`**.

### Prova 5: Pausar i reprendre
1. A la mateixa campanya activa, feu clic al botó **`Pausar`**.
   - *Resultat:* L'estat canvia a **`PAUSADA`** (xip groc) i el botó canvia a **`Reprendre`**. Desapareix l'opció de registrar consum.
2. Feu clic a **`Reprendre`**.
   - *Resultat:* L'estat torna a **`ACTIVA`** i reapareix el botó de consum.

### Prova 6: Registrar consum parcial i acumulació
1. A la fila de la campanya activa, feu clic a **`Registrar consum`**.
2. Es desplegarà el formulari en línia de consum:
   - Proveu d'introduir `0` o un import negatiu: l'aplicació no ho permetrà.
   - Proveu d'introduir un import superior a `1000`: el sistema mostrarà: *«El consum supera el pressupost disponible.»*
3. Introduïu **`300`** i feu clic a **`Registrar`**.
   - *Resultat:*
     - Missatge verd de confirmació.
     - Consum: **`300,00 € (30%)`** amb barra de progrés actualitzada.
     - Pressupost restant: actualitzat automàticament a **`700,00 €`**.
4. Torneu a fer clic a **`Registrar consum`** i introduïu **`200`**.
   - *Resultat:*
     - El consum s'acumula: **`500,00 € (50%)`**.
     - Pressupost restant: **`500,00 €`**.

### Prova 7: Arribar al 100% i finalització automàtica
1. Feu clic a **`Registrar consum`** i introduïu exactament l'import restant: **`500`**.
2. Feu clic a **`Registrar`**.
3. *Resultat clau de la prova:*
   - El consum acumulat arriba a **`1.000,00 € (100%)`**.
   - Pressupost restant: **`0,00 €`**.
   - L'estat canvia automàticament a **`FINALITZADA`** (xip gris).
   - A la columna d'accions desapareixen tots els botons i es mostra el text informatiu: *«Sense accions disponibles»*.
   - **No existeix cap botó manual per finalitzar ni per reactivar.**

---

## 3. Proves Directes de l'API REST (cURL / PowerShell)

Podeu obrir una finestra de PowerShell o un terminal amb `curl` per provar els endpoints de l'API independentment de la interfície.

> **Nota:** La URL base per a aquestes proves és `http://localhost:5101`.

### 3.1. Crear una campanya (`POST /campaigns`)

**Comanda PowerShell:**
```powershell
$body = @{
    nom = "Campanya Test API"
    pressupostTotal = 250
    dataInici = (Get-Date).AddDays(-1).ToString("yyyy-MM-ddTHH:mm:ssZ")
    dataFi = (Get-Date).AddDays(15).ToString("yyyy-MM-ddTHH:mm:ssZ")
} | ConvertTo-Json

$res = Invoke-RestMethod -Uri "http://localhost:5101/campaigns" -Method Post -Body $body -ContentType "application/json"
$id = $res.id
Write-Output "ID generat: $id"
```
- **Codi esperat:** `201 Created`
- **Cos de resposta:** `{"id": "..."}`

**Provar error de validació (nom curt):**
```powershell
$badBody = @{ nom = "AB"; pressupostTotal = 100; dataInici = "2026-10-01"; dataFi = "2026-11-01" } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:5101/campaigns" -Method Post -Body $badBody -ContentType "application/json"
```
- **Codi esperat:** `400 Bad Request`
- **Cos:** `{"error": "El nom ha de tenir almenys 3 caràcters."}`

---

### 3.2. Consultar campanya per identificador (`GET /campaigns/{id}`)

```powershell
Invoke-RestMethod -Uri "http://localhost:5101/campaigns/$id" -Method Get
```
- **Codi esperat:** `200 OK`
- **Estat retornat:** `"ESBORRANY"`, `pressupostConsumit: 0`.

---

### 3.3. Canviar estat a ACTIVA (`PATCH /campaigns/{id}/status`)

```powershell
$activa = @{ estat = "ACTIVA" } | ConvertTo-Json
Invoke-WebRequest -Uri "http://localhost:5101/campaigns/$id/status" -Method Patch -Body $activa -ContentType "application/json"
```
- **Codi esperat:** `204 NoContent`

---

### 3.4. Intentar finalitzar manualment (Prohibit per negoci)

```powershell
try {
    $finalitzar = @{ estat = "FINALITZADA" } | ConvertTo-Json
    Invoke-RestMethod -Uri "http://localhost:5101/campaigns/$id/status" -Method Patch -Body $finalitzar -ContentType "application/json"
} catch {
    Write-Output "Status: $($_.Exception.Response.StatusCode) - $($_.ErrorDetails.Message)"
}
```
- **Codi esperat:** `400 Bad Request`
- **Missatge:** `{"error": "Només es permet activar o pausar una campanya."}`

---

### 3.5. Registrar consum (`POST /campaigns/{id}/consume`)

**Primer consum (100 €):**
```powershell
$c1 = @{ import = 100 } | ConvertTo-Json
Invoke-WebRequest -Uri "http://localhost:5101/campaigns/$id/consume" -Method Post -Body $c1 -ContentType "application/json"
```
- **Codi esperat:** `204 NoContent`

**Segon consum complet (150 € restants):**
```powershell
$c2 = @{ import = 150 } | ConvertTo-Json
Invoke-WebRequest -Uri "http://localhost:5101/campaigns/$id/consume" -Method Post -Body $c2 -ContentType "application/json"
```
- **Codi esperat:** `204 NoContent`

**Verificar auto-finalització:**
```powershell
Invoke-RestMethod -Uri "http://localhost:5101/campaigns/$id" -Method Get
```
- **Estat esperat:** `"FINALITZADA"`, `pressupostConsumit: 250`.

**Intent de reactivació (Rebuig):**
```powershell
try {
    Invoke-RestMethod -Uri "http://localhost:5101/campaigns/$id/status" -Method Patch -Body $activa -ContentType "application/json"
} catch {
    Write-Output "Status: $($_.Exception.Response.StatusCode) - $($_.ErrorDetails.Message)"
}
```
- **Codi esperat:** `409 Conflict`
- **Missatge:** `{"error": "Una campanya finalitzada no es pot reactivar."}`

---

### 3.6. Llistat i filtres (`GET /campaigns`)

```powershell
# Totes les campanyes
Invoke-RestMethod -Uri "http://localhost:5101/campaigns" -Method Get

# Filtrar només per actives
Invoke-RestMethod -Uri "http://localhost:5101/campaigns?estat=ACTIVA" -Method Get

# Provar filtre invàlid
try {
    Invoke-RestMethod -Uri "http://localhost:5101/campaigns?estat=99" -Method Get
} catch {
    Write-Output "Status: $($_.Exception.Response.StatusCode)"
}
```
- Filtre `?estat=99` retorna: **`400 Bad Request`**.

---

## 4. Execució de la Bateria de Proves Automatitzades

Per verificar que el codi satisfà el 100% de les proves en qualsevol moment:

```bash
# 1. Tests de Backend (.NET - 56 proves)
cd C:\MarketingCampaignsManager
dotnet test

# 2. Tests de Frontend (Angular - 8 proves)
cd C:\MarketingCampaignsManager\Angular-UI
npm test
```

---

## 5. Taula Checklist de Verificació Ràpida per a l'Avaluador

| Requisit Funcional | Prova a executar | Resultat esperat |
| :--- | :--- | :--- |
| **Creació amb nom curt (<3 caràcters)** | Crear campanya amb nom `"A"` | Rebutjat amb error d'almenys 3 caràcters (`400 Bad Request`). |
| **Creació amb pressupost <= 0** | Crear campanya amb pressupost `0` o `-50` | Rebutjat amb error de pressupost positiu (`400 Bad Request`). |
| **Creació amb dataFi <= dataInici** | Data fi anterior a inici | Rebutjat amb error de dates (`400 Bad Request`). |
| **Estat inicial i consum** | Crear campanya vàlida | Nascuda en `ESBORRANY` i consum estrictament `0`. |
| **Pausar esborrany** | Intentar pausar abans d'activar | Rebutjat (`409 Conflict`), només es pot pausar una campanya activa. |
| **Finalització manual** | Buscar botó «Finalitzar» a la UI | No existeix cap botó ni transició per finalitzar manualment. |
| **Acumulació de consum** | Consumir 30€ i després 20€ | Consum total acumulat = 50€; saldo restant actualitzat. |
| **Auto-finalització** | Consumir fins al total exacte | Estat passa immediatament a `FINALITZADA`. |
| **Bloqueig de finalitzades** | Intentar reactivar o modificar campanya finalitzada | Rebutjat amb `409 Conflict`. |
| **Filtre invàlid a l'API** | `GET /campaigns?estat=99` | Retorna `400 Bad Request`. |
