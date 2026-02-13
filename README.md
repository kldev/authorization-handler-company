# AuthorizationDemo

Demonstracja **policy-based authorization** w ASP.NET Core 10.0 z uwierzytelnianiem JWT.

Projekt pokazuje jak zbudowac granularny system uprawnien oparty o role, zasoby (resource-based) i kontekst (kwota faktury) — uzywajac wylacznie wbudowanych mechanizmow ASP.NET Core.

## Stos technologiczny

- .NET 10.0 / ASP.NET Core Minimal API
- JWT Bearer Authentication (HMAC-SHA256)
- Policy-based Authorization z wieloma handlerami
- Swagger / OpenAPI (Swashbuckle)
- OpenTelemetry (tracing via Alloy)

## Uruchomienie

```bash
dotnet run --project src/AuthorizationDemo/AuthorizationDemo.csproj
```

Aplikacja startuje pod `http://localhost:5100`.
Swagger UI: [http://localhost:5100/swagger](http://localhost:5100/swagger)

## Struktura projektu

```
src/AuthorizationDemo/
├── Program.cs                          # Konfiguracja DI, JWT, polityk, rejestracja serwisow
├── Authorization/
│   ├── Roles.cs                        # Root, PolandManager, InternationalManager, FinancePerson
│   ├── Policies.cs                     # CanAccessCompany, CanCreateInvoice
│   ├── CompanyAccessRequirement.cs     # Requirement dla dostepu do firmy
│   ├── CompanyAccessHandler.cs         # Handler — kto widzi ktora firme
│   ├── InvoiceCreateRequirement.cs     # Requirement dla tworzenia faktur
│   ├── InvoiceCreateHandler.cs         # Handler — Root moze wszystko
│   ├── InvoiceAmountLimitHandler.cs    # Handler — FinancePerson do 100k
│   └── InvoiceContext.cs               # Kontekst z kwota faktury
├── Domain/
│   └── Company.cs                      # Model firmy (Id, Name, Country, TaxId, City)
├── Repositories/
│   ├── ICompanyRepository.cs           # Interfejs repozytorium
│   └── FakeCompanyRepository.cs        # 10 firm w pamieci (5 PL + 5 zagranicznych)
├── Extensions/
│   ├── AuthenticationExtensions.cs     # Konfiguracja JWT Bearer (issuer, audience, klucz)
│   ├── AuthorizationExtensions.cs      # Rejestracja polityk i handlerow autoryzacji
│   ├── ClaimsPrincipalExtensions.cs    # Konwersja claimow na dictionary (endpoint /user/me)
│   ├── ObservabilityExtensions.cs      # OpenTelemetry — tracing, logging, metryki (OTLP → Alloy)
│   └── SwaggerExtensions.cs           # Swagger z obsluga JWT Bearer token
├── Services/
│   ├── IAuthTokenService.cs            # Interfejs + DTO (LoginRequest, LoginResponse)
│   ├── AuthTokenService.cs             # Generowanie tokenow JWT
│   ├── ICompanyService.cs              # Interfejs serwisu firm
│   ├── CompanyService.cs               # Logika autoryzacji dostepu do firm
│   ├── IInvoiceService.cs              # Interfejs + result type (InvoiceCreateResult)
│   └── InvoiceService.cs               # Logika tworzenia faktur z autoryzacja
└── Endpoints/
    ├── AuthEndpoints.cs                # POST /auth/login, GET /auth/roles — deleguje do AuthTokenService
    ├── CompanyEndpoints.cs             # GET /api/companies, GET /api/companies/{id} — deleguje do CompanyService
    └── InvoiceEndpoints.cs             # POST /api/invoices — deleguje do InvoiceService
```

## Role i uprawnienia

| Rola | Dostep do firm | Tworzenie faktur |
|------|---------------|-----------------|
| **Root** | Wszystkie firmy | Bez limitu kwoty |
| **FinancePerson** | Wszystkie firmy | Do 100 000 |
| **PolandManager** | Tylko firmy z Country = "PL" | Brak |
| **InternationalManager** | Tylko firmy z Country != "PL" | Brak |

## Endpointy

| Metoda | Sciezka | Opis | Auth |
|--------|---------|------|------|
| POST | `/auth/login` | Generuj token JWT (username + rola) | Nie |
| GET | `/auth/roles` | Lista dostepnych rol | Nie |
| GET | `/api/companies` | Firmy widoczne dla uzytkownika | Tak |
| GET | `/api/companies/{id}` | Pojedyncza firma (jesli autoryzowany) | Tak |
| POST | `/api/invoices` | Utworz fakture (sprawdzenie kwoty + dostepu do firmy) | Tak |

## Jak dziala autoryzacja faktur

Wywołanie `AuthorizeAsync(user, new InvoiceContext(amount), CanCreateInvoice)` odpala **oba handlery** zarejestrowane na `InvoiceCreateRequirement`:

```
InvoiceCreateHandler (bez resource)          InvoiceAmountLimitHandler (z InvoiceContext)
─────────────────────────────────            ────────────────────────────────────────────
Root? → Succeed                              FinancePerson + amount <= 100k? → Succeed
(inni: cisza)                                (inni: cisza)
```

Wystarczy ze **jeden** handler powie `Succeed`. Dlatego:
- **Root** przechodzi przez pierwszy handler niezaleznie od kwoty
- **FinancePerson** przechodzi przez drugi tylko gdy kwota <= 100 000
- **PolandManager / InternationalManager** — zaden handler nie mowi Succeed = `403 Forbidden`

## Wyniki testow

Wszystko zgodne z wymaganiami:

| Rola | Firma | Kwota | HTTP | Dlaczego |
|------|-------|-------|------|----------|
| Root | Orlen (PL) | 50 000 | **201** | Root moze wszystko |
| Root | Siemens (DE) | 999 999 | **201** | Root — bez limitu kwoty |
| FinancePerson | Orlen (PL) | 50 000 | **201** | Finanse + kwota <= 100k |
| FinancePerson | Siemens (DE) | 50 000 | **201** | Finanse widzi wszystkie firmy + kwota OK |
| FinancePerson | Orlen (PL) | 100 000 | **201** | Dokladnie na granicy limitu |
| FinancePerson | Orlen (PL) | 100 001 | **403** | Przekroczony limit 100k |
| PolandManager | Orlen (PL) | 1 000 | **403** | Nie ma prawa tworzyc faktur |
| InternationalManager | Siemens (DE) | 1 000 | **403** | Nie ma prawa tworzyc faktur |

## Przyklad uzycia (curl)

```bash
# 1. Zaloguj sie jako FinancePerson
TOKEN=$(curl -s http://localhost:5100/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"jan","role":"FinancePerson"}' | jq -r '.token')

# 2. Lista firm
curl -s http://localhost:5100/api/companies \
  -H "Authorization: Bearer $TOKEN" | jq

# 3. Utworz fakture (50 000 — przejdzie)
curl -s -X POST http://localhost:5100/api/invoices \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"companyId":"a1b2c3d4-0001-0000-0000-000000000001","amount":50000,"description":"Usluga doradcza"}' | jq

# 4. Utworz fakture (100 001 — 403 Forbidden)
curl -s -X POST http://localhost:5100/api/invoices \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"companyId":"a1b2c3d4-0001-0000-0000-000000000001","amount":100001,"description":"Za duza kwota"}'
```

## Dane testowe (firmy)

| Nazwa | Kraj | Miasto | ID |
|-------|------|--------|----|
| Orlen S.A. | PL | Plock | `a1b2c3d4-0001-...01` |
| CD Projekt S.A. | PL | Warszawa | `a1b2c3d4-0002-...02` |
| KGHM Polska Miedz S.A. | PL | Lubin | `a1b2c3d4-0003-...03` |
| Allegro S.A. | PL | Warszawa | `a1b2c3d4-0004-...04` |
| InPost S.A. | PL | Krakow | `a1b2c3d4-0010-...10` |
| Siemens AG | DE | Munchen | `a1b2c3d4-0005-...05` |
| Microsoft Corp. | US | Redmond | `a1b2c3d4-0006-...06` |
| Toyota Motor Corp. | JP | Toyota City | `a1b2c3d4-0007-...07` |
| Samsung Electronics Co. | KR | Suwon | `a1b2c3d4-0008-...08` |
| SAP SE | DE | Walldorf | `a1b2c3d4-0009-...09` |
