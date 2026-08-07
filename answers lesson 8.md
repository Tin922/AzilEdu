# Lekcija 8 — predaja

## Tablica rezultata

| Funkcionalnost | Status |
|---|---|
| CRUD i filtri donacija | radi |
| Dashboard podaci | radi |
| Upload, promjena i brisanje naslovne slike | radi |
| Prijava i odjava | radi |
| Navigacija za sve četiri uloge | radi |
| MyTasks.razor i MyDonations.razor | radi |
| Prazna baza iz migracija i seed podataka | radi |

## Sigurnosna ograničenja

1. **API nema autentikaciju** — endpointi ne provjeravaju tko je prijavljen.
2. **Prijava je samo u Blazor memoriji** — nema cookie/JWT sesije.
3. **Skriveni linkovi i query parametri nisu API autorizacija** — samo UI sakriva stranice; API i dalje prima zahtjeve od svih.

U produkciji: cookie autentikacija (Blazor Server) ili JWT (API) + `[Authorize(Roles = "...")]` na kontrolerima koji mijenjaju podatke.
