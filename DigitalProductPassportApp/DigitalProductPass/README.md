# Digital Product Passport

ASP.NET Core MVC (.NET 10) + EF Core (SQL Server) + QRCoder.
Botanical editorial theme (Fraunces + Plus Jakarta Sans), light/dark mode, EN / TR / DE.

## Run
1. Put your connection string in `DigitalProductPass/appsettings.json` (`ConnectionStrings:DefaultConnection`).
2. `cd DigitalProductPass && dotnet run`

## Store owner login
`/Account/Login` — default `admin` / `DNZ2026`, read from the `StoreOwner` section of `appsettings.json`.
Change it before deploying (use user-secrets or environment variables `StoreOwner__Username` / `StoreOwner__Password`).
Everything in `ProductsController` (create products, issue variant SKUs, revoke items, materials, supply chain),
`BrandsController` and `DashboardController` requires the `StoreOwner` role. The public passport, scanner and QR images stay open.

## Database migration (existing database)
Run from the folder that contains `DigitalProductPass.csproj`:

```bash
dotnet tool install --global dotnet-ef        # once (or: dotnet tool update --global dotnet-ef)
dotnet ef migrations add AddVariantSkusAndCarbonIndex
dotnet ef database update
```

All new columns are nullable (or have a default), so existing rows in `Products`, `ProductItems` and `Materials` are kept.
In Development, `Program.cs` also applies pending migrations on startup, so `dotnet ef database update` is optional locally.
The seeder back-fills the demo product (`TSH-ORG-001`) with the new values once; other products are never touched.

If `database update` fails with "There is already an object named 'Brands'", your DB was created with `EnsureCreated`
and has no `__EFMigrationsHistory` table. For a dev database just drop it (`dotnet ef database drop`) and update again.

### New columns
| Table | Column |
|---|---|
| Products | Gtin, DifferentiationCode, WeightKg, WaterUsageLiters, RecyclabilityScore, RepairabilityIndex, WashInstructions, EndOfLifeGuidance |
| ProductItems | DifferentiationCode, BatchNumber, IsRevoked, RevokedAt (Color and Size already existed) |
| Materials | CarbonIndexKgPerKg, CertificateCode |

## Carbon calculation
Weighted index = Σ (share % / 100 × material `CarbonIndexKgPerKg`), only when shares total exactly 100% and every material has an index.
Footprint (kg CO₂e) = weighted index × `Product.WeightKg`. The manually declared `CarbonFootprintKg` is the fallback.

## Variant serial SKUs
`SerialCodeGenerator.New(diffCode, color, size)` → `TSHORG-WHI-M-7K3M9` (max 20 chars, valid as GS1 AI 21).
Uniqueness is checked against the DB and enforced by a unique index.

## GS1 Digital Link
With a GTIN on the product the QR encodes `{base}/01/{gtin14}[/10/{batch}]/21/{serial}`; these URLs resolve to the passport.
Without a GTIN it falls back to `{base}/p/{guid}`. Set `PublicBaseUrl` for phone testing (ngrok / LAN address).

## Routes
- `/p/{guid}` and `/01/{gtin}/21/{serial}` public passport · `/scan` scanner · `/qr/{guid}.png|svg`
- `/Account/Login`, `/Products`, `/Brands/Create`, `/Dashboard` (store owner)
- `/lang/{en|tr|de}` switches language (cookie `dpp.lang`)
