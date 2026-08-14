# AzilEdu — završni domaći zadatak (Lekcija 9)

Sustav za upravljanje azilom: ASP.NET Core Web API + Blazor Server aplikacija.

## Pokretanje

### Preduvjeti

- .NET 10 SDK
- (Opcionalno) Rider / Visual Studio s compound konfiguracijom **AzilEdu**

### API

```bash
cd AzilEdu.Api
dotnet run --launch-profile https
```

- Swagger (Development): https://localhost:7205/swagger  
- HTTP: http://localhost:5086  

Pri prvom pokretanju primjenjuju se migracije i seed podaci (životinje, lookup tablice, demo korisnici).

### App (Blazor)

U drugom terminalu:

```bash
cd AzilEdu.App
dotnet run --launch-profile https
```

- Aplikacija: https://localhost:7094  

App koristi `HttpClient` prema API-ju na `https://localhost:7205/`.

### Rebuild Solution

```bash
dotnet build AzilEdu.sln
```

**Rezultat provjere:** build uspješan, 0 grešaka, 0 upozorenja.

### Migracije baze (kopija lokalne baze)

Na kopiji `AzilEdu.Api/AzilEdu.db` (backup u `AzilEdu.Api/db-backups/`):

```bash
cd AzilEdu.Api
dotnet tool restore
~/.dotnet/tools/dotnet-ef database update 0
~/.dotnet/tools/dotnet-ef database update
```

**Rezultat:** sve migracije vraćene na 0, zatim ponovno primijenjene (`AddPeopleModules` → `AddUsersAndRoles`). Seed se izvršava pri sljedećem pokretanju API-ja.

---

## Demo računi

Lozinke **nisu** u repozitoriju — postavljene su lokalno u `AppUserSeeder` samo za razvoj.

| Email | Uloge | Poslovna veza |
|-------|-------|----------------|
| `admin@aziledu.local` | User, Admin | — |
| `employee@aziledu.local` | User, Employee | Djelatnik (EMP-001) |
| `volunteer@aziledu.local` | User, Volunteer | Volonter (Ana Horvat) |
| `donor@aziledu.local` | User, Donor | Donator (Ivana Babić) |

Za lokalno testiranje koristi lozinke iz seed koda (`AppUserSeeder.cs`) ili kreiraj vlastite korisnike kroz `/users`.

### Novi korisnik (domaći zadatak)

Kreiran putem `POST /api/users` (Admin):

| Polje | Vrijednost |
|-------|------------|
| Email | `test.korisnik@aziledu.local` |
| DisplayName | Test Korisnik |
| Uloge | User, Employee, Donor (3 uloge) |
| EmployeeId | 2 (Ivan Marić) |
| DonorId | 2 (Pet Plus d.o.o.) |

---

## Relacije korisnika

```
AppUser ──< AppUserRole >── AppRole
   │
   ├── VolunteerId?  → Volunteers (volonterski profil)
   ├── DonorId?    → Donors (donatorski profil)
   └── EmployeeId? → Employees (djelatnički profil)
```

- **AppUser–AppRole:** many-to-many preko `AppUserRoles`. JWT sadrži role claimove; API koristi `[Authorize(Roles=...)]` i policy `Staff` / `AdminOnly`.
- **AppUser–Volunteer:** opcionalni FK. Volonter vidi samo svoje zadatke (`/api/volunteertasks/mine`) preko `VolunteerId` claima.
- **AppUser–Donor:** opcionalni FK. Donator vidi samo svoje donacije (`/api/donations/mine`) preko `DonorId` claima.
- **AppUser–Employee:** opcionalni FK. Služi za povezivanje internog računa s HR evidencijom; staff uloge ne ovise striktno o FK-u, ali veza omogućuje buduće proširenje.

---

## 401 vs 403

| Status | Značenje u AzilEdu |
|--------|---------------------|
| **401 Unauthorized** | Nema valjanog JWT tokena (nisi prijavljen ili je token istekao). Primjer: `GET /api/animals` bez `Authorization` headera. |
| **403 Forbidden** | Korisnik **je** autentificiran, ali nema pravo na resurs. Primjer: Donor poziva `GET /api/donations` (staff endpoint) ili `POST /api/animals`. |

UI sakrivanje gumba/ linkova **nije** sigurnosna granica — svaki osjetljivi endpoint ima provjeru na API-ju.

---

## AI endpointi i podaci poslani provideru

API **nikad** ne šalje lozinke, `PasswordHash` ni API ključeve. Osobni podaci donatora/volontera svode se na minimum (npr. ime donatora u zahvali, naslov zadatka u sažetku).

| Endpoint | Auth | Podaci poslani AI servisu |
|----------|------|---------------------------|
| `GET /api/ai/status` | Staff | Ništa (samo lokalni status providera) |
| `POST /api/ai/text` | Staff | `purpose` + `input` (max 4000 znakova). Svrhe: `animal-adoption`, `donor-thank-you`, `social-post` |
| `GET /api/ai/daily-summary` | Staff | Agregatni brojevi: ukupno životinja, dostupne za udomljenje, otvoreni/zakašnjeli zadaci, donacije u 7 dana |
| `GET /api/ai/volunteer-summary/mine` | Volunteer | Do 10 otvorenih zadataka: naslov, tip, životinja, status, rok (JSON) |
| `POST /api/ai/animal-intake` | Staff | Slobodna bilješka s terena (max 4000 znakova) |
| `POST /api/ai/animal-data-check` | Staff | Samo polja životinje: ime, vrsta, pasmina, spol, dob, datum dolaska, status, opis (bez slika/ID-a korisnika) |

Strukturirani odgovori (`animal-intake`, `animal-data-check`) ponovno se validiraju na API-ju prije vraćanja klijentu. AI rezultat u UI-ju **nije** automatski spremljen — korisnik ga može urediti ili odbaciti.

U produkciji: rate limit 30 AI poziva/sat/korisnik + audit log (svrha, UserId, Email).

---

## Mock i OpenAI način rada

Konfiguracija je **samo** u API projektu (`AiOptions`), ne u repozitoriju s ključem.

### Mock (preporučeno za domaći i CI)

```bash
cd AzilEdu.Api
dotnet user-secrets set "Ai:Provider" "Mock"
```

Ili pri pokretanju:

```bash
Ai__Provider=Mock dotnet run --launch-profile https
```

Mock vraća predvidljive lokalne odgovore (`MockAiService`) — nema vanjskog servisa.

### OpenAI

```bash
cd AzilEdu.Api
dotnet user-secrets set "Ai:Provider" "OpenAI"
dotnet user-secrets set "Ai:Model" "gpt-4o-mini"
dotnet user-secrets set "Ai:ApiKey" "<tvoj-ključ>"
```

**Nikad** ne commitaj `ApiKey` u `appsettings.json`, repozitorij ili README.

---

## Autorizacijski testovi (8+)

| URL | Korisnik | Očekivano | Stvarno |
|-----|----------|-----------|---------|
| `GET /api/animals` | bez tokena | 401 | **401** |
| `GET /api/animals` | Employee | 200 | **200** |
| `POST /api/animals` | Donor | 403 | **403** |
| `POST /api/animals` | Employee | 201 | **201** |
| `GET /api/donations` | Donor | 403 | **403** |
| `GET /api/donations/mine` | Donor | 200 | **200** |
| `GET /api/volunteertasks/mine` | Volunteer | 200 | **200** |
| `GET /api/volunteertasks/mine` | Donor | 403 | **403** |
| `GET /api/volunteertasks` | Volunteer | 403 | **403** |
| `GET /api/users` | Employee | 403 | **403** |
| `GET /api/users` | Admin | 200 | **200** |
| `PUT /api/animals/1/media/{id}/cover` | Employee | 204 | **204** |

### Izolacija podataka

- **Donator:** `GET /api/donations/mine` vraća samo donacije s `DonorId` iz JWT claima (tuđe donacije nisu dostupne).
- **Volonter:** `GET /api/volunteertasks` (staff lista) → **403**. `GET /api/volunteertasks/mine` vraća samo zadatke tog volontera; ne može filtrirati tuđe zadatke jer nema pristup staff endpointu.

---

## Multimedija (profil životinje)

Testirano na `GET/POST /api/animals/1/media`:

1. Upload PNG slike → **201**, `mediaType: Image`
2. Upload WebM videa → **201**, `mediaType: Video`
3. `PUT .../media/{id}/cover` → **204** (naslovna slika)

U UI-ju (`/animals/{id}`): upload prikazuje dopuštene formate (JPEG, PNG, WebP, MP4, WebM) i max 25 MB.

---

## AI demonstracija (Mock provider)

| Značajka | Gdje u UI | Napomena |
|----------|-----------|----------|
| AI opis životinje | `/animals/edit/{id}` | Gumb „AI prijedlog opisa”; moguće odbaciti |
| AI objava | `/animals/{id}` | „Predloži objavu”; uredivo / odbaci |
| AI zahvala donatoru | `/donations/edit/{id}` | Generiraj zahvalu; ne sprema se automatski |
| Dnevni sažetak | `/` (Admin/Employee) | Dashboard AI kartica |
| Sažetak zadataka | `/my-tasks` (Volunteer) | AI sažetak otvorenih zadataka |
| Pametni unos | `/animals/create` | Bilješka → strukturirani prijedlog polja |
| Provjera kvalitete | `/animals/edit/{id}` | AI provjera prije spremanja |

**Odbacivanje prijedloga:** na svim AI ekranima postoji gumb „Odbaci prijedlog/sažetak”; korisnik ručno odlučuje što spremiti.

---

## UX stanja (loading / empty / error)

| Stranica | Loading | Empty | Error |
|----------|---------|-------|-------|
| `/animals` | spinner | „Nema rezultata…” | alert s porukom API-ja |
| `/donors` | spinner | „Nema donatora…” | alert |
| `/donations` | spinner | „Nema donacija…” | alert |
| `/` (dashboard) | spinner | upozorenje ako nema sažetka | alert |

Brisanje entiteta traži potvrdu (`MudDialog` / `ShowMessageBoxAsync`).

---

## Poznata ograničenja

1. **JWT u localStorage** — token u pregledniku; nema refresh tokena ni revoke liste.
2. **Rate limiting AI** — aktivan samo izvan Development okruženja.
3. **OpenAI placeholder ključ** — bez valjanog ključa ili Mock providera AI tekstualni endpointi vraćaju grešku providera.
4. **SQLite** — jedan writer; nije namijenjen produkcijskom opterećenju.

## Prijedlozi za sljedeću verziju

1. **Refresh tokeni + httpOnly cookie** — sigurnija sesija bez JWT-a u JS storageu.
2. **Audit tablica u bazi** — trajni zapis AI poziva i administratorskih akcija umjesto samo log datoteke.
3. **E2E testovi (Playwright)** — automatizirana provjera auth matrice i AI discard flowa.

---

## Struktura projekta

```
AzilEdu.sln
├── AzilEdu.Api/      Web API, EF Core, JWT, AI servisi
├── AzilEdu.App/      Blazor Server + MudBlazor UI
└── AzilEdu.Shared/   DTO-ovi i modeli
```

Skripta za brzu provjeru auth testova: `scripts/final-homework-tests.sh`
