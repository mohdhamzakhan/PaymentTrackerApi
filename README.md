# Payment Tracker API (.NET 8)

Backend-only API covering:
- AiSensy WhatsApp campaign integration (send + log every call)
- Payment details table (amount, UTR, date) searchable by phone/UTR
- JWT login for Admin, User, Supplier, InHouseTeam, AccountsTeam roles
- Admin-only user registration + "view everything" endpoints

> This project was written without a live .NET SDK / NuGet access in the
> sandbox it was created in, so it hasn't been compiled here. Read through
> it before running — it's a solid, standard scaffold, but review it like
> you would any new teammate's first PR.

## 1. Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB, Express, or full) — or swap the provider (see below)

## 2. Configure

Edit `appsettings.json`:

- `ConnectionStrings:DefaultConnection` — your SQL Server connection string
- `Jwt:Key` — replace with a long random secret (32+ chars). **Never commit the real value.**
- `AiSensy:ApiKey` — from AiSensy dashboard → Manage → API Key
- `AiSensy:Endpoint` — leave as-is unless AiSensy changes it

For real projects, move `Jwt:Key` and `AiSensy:ApiKey` into `dotnet user-secrets`
(local dev) or environment variables / a key vault (production) instead of
appsettings.json.

## 3. Restore, migrate, run

```bash
dotnet restore
dotnet tool install --global dotnet-ef   # if you don't have it
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

Swagger UI comes up at `https://localhost:{port}/swagger` in Development mode.

## 4. First login

On first startup the app seeds all five roles and one default admin:

- username: `admin`
- password: `Admin@12345`

**Change this password immediately** (there's no change-password endpoint yet —
add one, or update it directly via `UserManager` in a one-off script/seed).

## 5. Core endpoints

| Method | Route | Access | Purpose |
|---|---|---|---|
| POST | `/api/auth/login` | Anyone | Login, returns JWT |
| POST | `/api/auth/register` | Admin | Create a new user with a role |
| GET | `/api/auth/users` | Admin | List all users + roles |
| GET | `/api/auth/me` | Any logged-in user | Own profile |
| POST | `/api/aisensy/send` | Admin, InHouseTeam, AccountsTeam | Send a WhatsApp campaign via AiSensy, logs request+response |
| GET | `/api/aisensy?phone=...` | Any logged-in user | List past campaign sends |
| POST | `/api/payment` | Admin, AccountsTeam | Create a payment record |
| PUT | `/api/payment/{id}` | Admin, AccountsTeam | Update a payment record (e.g. mark Verified) |
| GET | `/api/payment/{id}` | Any logged-in user | Get one payment record |
| GET | `/api/payment/search?phone=...&utr=...` | Any logged-in user | **Point 3**: look up payments by phone or UTR |
| GET | `/api/payment` | Admin, AccountsTeam, InHouseTeam | Full payment list |

All routes except `/api/auth/login` require `Authorization: Bearer {token}`.

## 6. Example: send a campaign

```
POST /api/aisensy/send
Authorization: Bearer {token}
Content-Type: application/json

{
  "campaignName": "order-confirmation",
  "destination": "+917428526285",
  "userName": "Ramesh Kumar",
  "templateParams": ["Ramesh", "12345"],
  "tags": ["order-confirmed"]
}
```

The server attaches the configured `apiKey` itself — clients never send it.
The full AiSensy request + raw JSON response are saved to `CampaignLogs`.

## 7. Example: search payments

```
GET /api/payment/search?phone=9427526285
GET /api/payment/search?utr=UTR20260813001
```

## 8. Switching database provider

Currently uses `Microsoft.EntityFrameworkCore.SqlServer`. To use PostgreSQL
or MySQL instead:

1. Swap the NuGet package in `PaymentTrackerApi.csproj` (e.g.
   `Npgsql.EntityFrameworkCore.PostgreSQL`)
2. Change `options.UseSqlServer(...)` to `options.UseNpgsql(...)` in
   `Program.cs`
3. Update the connection string format in `appsettings.json`
4. Re-run migrations

## 9. What's intentionally left for you to add next

- Password reset / change-password endpoints
- Refresh tokens (current JWT just expires after `Jwt:ExpiryMinutes`)
- Pagination on the list endpoints (`GetAll` for campaigns and payments)
- Rate limiting / input sanitization hardening
- Locking CORS down to your real frontend origin instead of `AllowAnyOrigin`
- The frontend itself (this deliverable is API-only, as requested)
