# Millores Futures i Reflexió d'Enginyeria

Aquest document recull les propostes d'evolució tècnica del projecte per a un entorn productiu real, així com una **nota personal i professional del desenvolupador** sobre les decisions adoptades i el balanç de l'exercici.

---

## 1. Propostes de Millora Tècnica per a Producció

Tot i que la solució compleix rigorosament els requisits de la prova tècnica, per a un entorn comercial amb alta concurrència i volum de dades s'haurien d'implementar les següents millores:

### 1.1. Concurrència optimista en el registre de consum
- **Situació actual:** El consum valida el saldo disponible en memòria abans de persistir. En un context amb múltiples peticions concurrents per a una mateixa campanya, dues transaccions simultànies podrien llegir el mateix saldo abans que una d'elles guardi el nou valor.
- **Millora:** Incorporar un token de concurrència (`RowVersion` o `[ConcurrencyCheck]`) a l'entitat `Campanya` mitjançant EF Core. Si es produeix un conflicte, EF Core llançarà una `DbUpdateConcurrencyException`, que es pot capturar per reintentar l'operació o retornar un codi HTTP `409 Conflict` controlat.

### 1.2. Paginació, filtratge avançat i ordenació a l'API
- **Situació actual:** `GET /campaigns` retorna totes les campanyes persistides en una única llista (filtrada només per estat en memòria/base de dades).
- **Millora:** Introduir paràmetres de consulta com `pageNumber`, `pageSize`, `sortBy` i `sortDescending`. Aplicar la paginació directament a nivell de base de dades amb `.Skip()` i `.Take()` a EF Core per evitar transferències massives de dades.

### 1.3. Esdeveniments de Domini (Domain Events) i CQRS
- **Situació actual:** La finalització automàtica d'una campanya s'executa internament dins del mètode `Consumir()`.
- **Millora:** Emetre un esdeveniment de domini `CampanyaFinalitzadaEvent` quan el consum assoleixi el 100%. Aquest esdeveniment podria ser escoltat per serveis secundaris (mitjançant MediatR o un bus de missatgeria com RabbitMQ/Azure Service Bus) per enviar notificacions per correu, alertar equips de màrqueting o sincronitzar amb sistemes de facturació externs sense acoblar el servei principal.

### 1.4. Seguretat, Autenticació i Autorització
- **Situació actual:** Els endpoints són d'accés públic per facilitar l'avaluació de la prova.
- **Millora:** Protegir l'API amb tokens JWT (OAuth2 / OpenID Connect), afegint polítiques d'autorització basades en rols (`Admin`, `MarketingManager`, `Viewer`) per limitar operacions crítiques com l'activació o el consum.

### 1.5. Observabilitat, Mètriques i Resiliència
- **Situació actual:** Registres d'errors i consoles estàndard.
- **Millora:**
  - Integrar **Serilog** amb destinació centralitzada (Elasticsearch, Seq o CloudWatch).
  - Configurar **OpenTelemetry** per a traçabilitat distribuïda de peticions.
  - Implementar **Health Checks** (`/healthz`, `/ready`) per a orquestradors com Kubernetes.
  - Utilitzar **Polly** per a polítiques de reintent i circuit-breaker en crides externes.

### 1.6. Millores a la interfície Angular
- **Estat reactiu amb Signals:** Migrar l'estat local del component principal a **Angular Signals** (`signal`, `computed`, `effect`) per a una reactivitat més granular i un millor rendiment en renderitzat.
- **Rutes i vistes dedicades:** Crear rutes (`/campanyes`, `/campanyes/:id`, `/estadistiques`) emprant l'Angular Router per millorar la usabilitat i la compartició d'enllaços directes.
- **Sistema de notificacions Toast:** Substituir els missatges en línia per alertes temporals tipus "Toast" per millorar l'experiència d'usuari (UX).

---

## 2. Nota Personal del Desenvolupador / Reflexió d'Enginyeria

> *"El millor codi no és el més complex, sinó el que equilibra de manera òptima la simplicitat present amb l'escalabilitat futura."*

### Sobre-enginyeria vs. Escalabilitat
En abordar aquesta prova tècnica, vaig malinterpretar inicialment l'abast temporal i la dimensió requerida de la solució. Vaig optar des d'un principi per una **Clean Architecture modular en múltiples projectes** (`Domain`, `Application`, `Infrastructure`, `CampaignApi`, `Tests`), acompanyada d'abstraccions com *Generic Repository*, *Unit of Work* i aïllament estricte del domini.

Fent una anàlisi crítica i pragmàtica d'enginyeria:
- **Per a una prova tècnica de 4-5 endpoints**, aquesta estructura suposa cert grau de **sobre-enginyeria (over-engineering)**. Un projecte monòlit senzill (una sola Web API amb carpetes internes ben organitzades o fins i tot Minimal APIs) hauria estat més lleuger, ràpid de desenvolupar i suficient per cobrir l'enunciat funcional.
- **No obstant això, com a base arquitectònica**, el patró triat és **idoni si el projecte hagués de créixer exponencialment**:
  1. Permet afegir noves regles de negoci complexes sense impactar la capa web ni la base de dades.
  2. Fa possible substituir la tecnologia de persistència (passar de SQLite a PostgreSQL, SQL Server o Mongo) sense tocar una sola línia de la lògica de negoci.
  3. Proporciona una **testabilitat immillorable**: la prova compta amb **56 tests automatitzats a .NET** que s'executen en pocs mil·lisegons gràcies als fakes en memòria i al desacoblament total de la infraestructura.

### Ús d'Eines d'Intel·ligència Artificial
Vull ser completament transparent respecte al procés de desenvolupament:
- He utilitzat diverses sessions i xats d'assistents gratuïts de ChatGPT per generar estructures inicials de scaffolding, maquetació base en HTML/CSS i codi estàndard repetitiu (*boilerplate*).
- No he inclòs el detall exhaustiu de cadascun d'aquests fragments de conversa perquè hauria resultat en una acumulació excessiva i poc pràctica d'informació a la documentació.
- El valor decisiu de l'exercici ha estat la **revisió crítica, l'auditoria rigorosa, la refactorització i la correcció minuciosa**: ajustar les invariables de domini, protegir les transicions d'estat reals, eliminar operacions no permeses a la interfície d'usuari, assegurar el comportament dels codis HTTP i construir una suite de tests robusta que garanteixi que tot funciona exactament com demana l'enunciat.

Aquesta experiència reafirma la meva convicció que les eines d'IA són acceleradors molt potents de desenvolupament, però requereixen sempre del criteri, la verificació i l'ofici d'un enginyer de programari per garantir la solidesa, la coherència i la qualitat del lliurament final.
