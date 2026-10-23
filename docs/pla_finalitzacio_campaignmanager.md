# Pla de finalització i auditoria — Prova tècnica CampaignManager

## 1. Rol i objectiu

Actua com a enginyer/a de programari sènior especialitzat/ada en .NET, ASP.NET Core, Angular, arquitectura neta, persistència i proves automatitzades.

Treballa sobre el repositori existent del projecte **CampaignManager**, creat per a una prova tècnica de Comergrup. L'objectiu és auditar-lo, completar les funcionalitats que faltin, corregir errors i deixar-lo verificat i documentat per al lliurament.

**No reconstrueixis el projecte des de zero.** Conserva l'arquitectura, els noms, els estils i el codi que ja siguin correctes. No substitueixis fitxers sencers si n'hi ha prou amb canvis localitzats. No eliminis funcionalitats que ja funcionin sense una justificació tècnica clara.

Treballa de manera autònoma dins del repositori, però informa dels passos, dels fitxers modificats, de les proves executades i dels resultats reals. No afirmis que una tasca està completada si no l'has verificat.

## 2. Enunciat funcional que cal respectar

### Model de campanya

Cada campanya ha de tenir, com a mínim:

- `id`: identificador únic.
- `nom`: obligatori, amb un mínim de 3 caràcters.
- `pressupostTotal`: import estrictament superior a zero.
- `pressupostConsumit`: import decimal, inicialment zero.
- `dataInici`: data ISO 8601.
- `dataFi`: data ISO 8601.
- `estat`: `ESBORRANY`, `ACTIVA`, `PAUSADA` o `FINALITZADA`.

### Crear campanya — `POST /campaigns`

- L'estat inicial ha de ser `ESBORRANY`.
- El consum inicial ha de ser zero.
- El pressupost total ha de ser superior a zero.
- `dataFi` ha de ser posterior a `dataInici`.
- Validar el nom i les dades d'entrada.
- Retornar codis HTTP adequats i errors comprensibles.

### Canviar estat — `PATCH /campaigns/{id}/status`

- Les úniques transicions manuals permeses són activar (`ACTIVA`) o pausar (`PAUSADA`).
- No permetre activar una campanya si la data de finalització ja ha passat.
- No permetre activar una campanya si `pressupostConsumit >= pressupostTotal`.
- Una campanya `FINALITZADA` no pot tornar a canviar d'estat.
- No afegir una operació manual per finalitzar campanyes: l'enunciat no la permet.
- Comprovar les transicions d'estat existents i retornar un error HTTP adequat per a transicions invàlides.

### Registrar consum — `POST /campaigns/{id}/consume`

- L'import ha de ser estrictament superior a zero.
- Només es pot registrar consum si la campanya està `ACTIVA`.
- La data actual ha d'estar dins del període de vigència, incloent els límits segons la interpretació coherent del projecte.
- El consum no pot superar el pressupost total.
- El consum s'ha d'acumular: `nouConsum = consumAnterior + importNou`.
- Si el consum acumulat arriba exactament al pressupost total, canviar automàticament l'estat a `FINALITZADA`.
- Una petició rebutjada no ha de modificar parcialment les dades.
- Fer servir operacions monetàries amb `decimal`, no `double` ni `float`.

### Llistar campanyes — `GET /campaigns`

- Retornar les campanyes persistides.
- Admetre el filtre opcional per estat, per exemple `GET /campaigns?estat=ACTIVA`.
- Validar el valor del filtre i tractar-lo de manera coherent amb la resta de l'API.

### Persistència

La prova permet SQLite, una base de dades en memòria o un repositori en memòria. Identifica què fa servir realment el projecte i conserva-ho si és correcte. No migris a una tecnologia diferent sense necessitat.

Si es fa servir SQLite, verifica la cadena de connexió, el registre del `DbContext`, les migracions o la creació de l'esquema i la persistència entre reinicis quan correspongui.

## 3. Context conegut del repositori

Aquestes dades provenen del treball previ i s'han de verificar contra els fitxers actuals abans d'actuar:

- Projecte API: `C:\MarketingCampaignsManager\CampaignApi`
- Projecte Angular: `C:\MarketingCampaignsManager\Angular-UI`
- Ports que s'havien utilitzat: `https://localhost:7128` i `http://localhost:5101` per a l'API, i `http://localhost:4200` per a Angular.
- S'ha observat anteriorment `ERR_CONNECTION_REFUSED` en carregar les campanyes. Comprova si l'API està en execució i si la URL configurada a Angular coincideix amb el port real. No assumeixis que aquests ports continuen vigents.
- El backend ha retornat el missatge `Només es permet activar o pausar una campanya.` en intentar finalitzar manualment. Aquest comportament és coherent amb l'enunciat.
- La interfície original conté una barra lateral, una capçalera, estadístiques, filtres, una taula de campanyes i formularis de creació i consum. Preserva aquest disseny.
- La interfície havia de mostrar una columna `Pressupost restant`, calculada com `pressupostTotal - pressupostConsumit`. Comprova si ja està afegida abans de fer canvis.
- La UI utilitza estats en català: `ESBORRANY`, `ACTIVA`, `PAUSADA` i `FINALITZADA`.
- S'ha fet servir `app.ts` com a fitxer del component Angular i `app.html` com a plantilla, però comprova els noms i l'estructura reals.

No donis per fet que el codi actual coincideix exactament amb aquest resum: inspecciona el repositori primer.

## 4. Procés obligatori de treball

### Fase 1 — Inspecció i diagnòstic

1. Examina l'estructura del repositori i identifica la solució .NET, els projectes, el frontend Angular, la persistència i els tests existents.
2. Llegeix els fitxers rellevants abans de modificar-los.
3. Revisa l'estat de Git (`git status`) i conserva els canvis locals de l'usuari. No descartis ni sobrescriguis canvis sense autorització.
4. Revisa les dependències i versions instal·lades, sense actualitzar paquets per rutina.
5. Executa build i tests existents abans dels canvis, si l'entorn ho permet.
6. Registra els errors inicials i separa:
   - errors que ja existien;
   - errors introduïts pels canvis que facis;
   - limitacions de l'entorn, com ara serveis no iniciats o ports ocupats.

### Fase 2 — Pla breu

Abans de modificar, resumeix:
- problemes detectats;
- fitxers que probablement cal tocar;
- ordre de correcció;
- riscos o dubtes que necessiten decisió.

No esperis aprovació per a canvis locals, reversibles i de baix risc que siguin clarament necessaris per complir l'enunciat. Demana confirmació abans d'accions destructives, d'eliminar dades, de canviar secrets o credencials, de fer push/publicació, o d'afegir serveis de pagament.

### Fase 3 — Correcció del backend

1. Revisa els quatre endpoints i les seves validacions.
2. Comprova les regles de negoci i la persistència.
3. Evita duplicar regles de negoci entre controladors, serveis i entitats.
4. Conserva els patrons i les capes existents si són coherents; no introdueixis una arquitectura nova només per embellir la prova.
5. Utilitza codis HTTP apropiats i un format d'error coherent.
6. Comprova els casos de concurrència bàsics en registrar consum si el disseny actual ho permet; evita que dues peticions superin el pressupost per una comprovació obsoleta.
7. No introdueixis autenticació, missatgeria, microserveis o infraestructura addicional que no siguin necessaris per a l'exercici.

### Fase 4 — Correcció d'Angular

1. Preserva la plantilla HTML i el disseny originals. Fes canvis locals i petits.
2. Assegura't que la URL de l'API coincideix amb la configuració real del servidor.
3. Comprova que es carrega el llistat i que els errors es mostren de manera útil.
4. Conserva filtres, estadístiques, formularis i accions que compleixin l'enunciat.
5. Retira el botó manual `Finalitzar` per a campanyes actives i pausades, perquè no és una transició permesa per l'enunciat.
6. Permet registrar consum només quan la campanya està activa i dins del període de vigència. Si el botó s'amaga fora del període, mostra un indicador textual discret i accessible. El backend continua sent l'autoritat i ha de validar la regla.
7. Mostra correctament el pressupost restant, calculat a partir del total menys el consum acumulat.
8. Després d'una operació correcta, actualitza les dades des de l'API o actualitza l'estat local de manera coherent, sense duplicar consums.
9. Desactiva els controls mentre una operació equivalent està en curs i evita enviaments duplicats.
10. No canviïs tota la plantilla HTML per reescriure-la ni eliminis parts del disseny original.

### Fase 5 — Tests

Afegeix o completa tests automatitzats segons l'estructura existent. Com a mínim, cobreix:

**Creació**
- dades vàlides;
- nom massa curt o buit;
- pressupost zero o negatiu;
- data final anterior o igual a la inicial;
- estat inicial `ESBORRANY` i consum inicial zero.

**Estats**
- activar una campanya vàlida;
- pausar una campanya activa;
- rebutjar estats manuals diferents de `ACTIVA` i `PAUSADA`;
- rebutjar l'activació d'una campanya caducada;
- rebutjar l'activació d'una campanya sense pressupost disponible;
- impedir qualsevol canvi d'estat d'una campanya `FINALITZADA`.

**Consum**
- consumir un import vàlid;
- acumular imports successius;
- rebutjar import zero o negatiu;
- rebutjar consum que superi el pressupost;
- rebutjar consum en una campanya no activa;
- rebutjar consum abans de `dataInici` o després de `dataFi`;
- finalitzar automàticament quan el consum arriba exactament al total;
- comprovar que una operació rebutjada no altera el consum ni l'estat.

**Llistat**
- retornar totes les campanyes;
- filtrar per estat;
- tractar filtres invàlids.

Afegeix tests d'Angular només si són proporcionats i viables dins l'estructura actual. No afegeixis un framework nou de testing sense valorar-ne el cost i la compatibilitat.

### Fase 6 — Validació integral

1. Executa restore, build i tests de .NET.
2. Executa instal·lació/verificació de dependències, build i tests d'Angular segons els scripts reals del `package.json`.
3. Inicia l'API i Angular si l'entorn ho permet.
4. Comprova els endpoints amb peticions reals i els codis HTTP.
5. Verifica manualment el flux principal:
   - crear campanya;
   - consultar-la al llistat;
   - activar-la;
   - registrar un primer consum;
   - registrar un segon consum i verificar l'acumulació;
   - arribar exactament al pressupost i verificar `FINALITZADA`;
   - comprovar que no es pot reactivar una campanya finalitzada;
   - comprovar els filtres.
6. No afirmis que s'ha fet una prova manual si no s'ha executat.
7. Si una prova no es pot executar per una limitació externa, explica-ho i dona la comanda concreta perquè es pugui reproduir.

### Fase 7 — README i lliurament

Completa el README/Markdown de lliurament amb:

- requisits previs;
- comandes per restaurar dependències i iniciar l'API;
- comandes per iniciar Angular;
- com executar els tests de cada projecte;
- estructura i decisions d'arquitectura;
- persistència utilitzada;
- regles de negoci importants;
- tractament d'errors;
- millores futures realistes per a producció;
- apartat `Prompts que s'han utilitzat amb IA`, amb un resum fidel dels usos reals de la IA. No inventis prompts que no s'hagin utilitzat.

Fes servir les comandes reals i comprovades del repositori, no exemples genèrics que no hagis verificat.

Revisa `git status`, comprova que no hi hagi secrets, fitxers de configuració locals sensibles, binaris innecessaris o carpetes generades incloses per error. No facis `git push`, no publiquis el repositori i no eliminis fitxers ni dades sense autorització.

## 5. Criteris de qualitat

- Codi llegible, coherent amb el projecte i sense complexitat innecessària.
- Responsabilitats ben separades d'acord amb l'arquitectura existent.
- Regles de negoci protegides al backend; el frontend només millora l'experiència.
- Import monetari amb `decimal`.
- Gestió clara d'errors i codis HTTP consistents.
- Tests repetibles i significatius.
- No ocultar errors amb `catch` buits ni desactivar validacions per fer passar els tests.
- No canviar l'enunciat ni relaxar regles per fer que una funcionalitat de la UI sembli funcionar.
- No afegir funcionalitats extra en detriment dels requisits principals.
- Preservar els canvis de l'usuari i el disseny original.
- Evitar comentaris redundants i abstraccions innecessàries.

## 6. Informe final obligatori

Quan acabis, presenta un informe en català amb:

1. **Resum:** què s'ha completat.
2. **Fitxers modificats:** ruta i motiu de cada canvi.
3. **Validació:** comandes executades i resultats exactes de build i tests.
4. **Proves funcionals:** quins fluxos s'han verificat realment.
5. **Pendents o limitacions:** tot el que no s'ha pogut verificar i per què.
6. **Com executar-ho:** comandes exactes per iniciar API, Angular i tests.
7. **Estat de lliurament:** què queda perquè el repositori es pugui enviar.

No declaris el projecte «acabat» si hi ha tests fallits, errors de build o validacions crítiques sense comprovar. Prioritza la precisió per sobre d'una impressió de progrés.

## 7. Instrucció inicial

Comença inspeccionant el repositori i l'estat de Git. Després presenta un diagnòstic breu i executa el pla de correcció per fases, completant tot allò que sigui possible sense accions destructives ni publicacions externes.
