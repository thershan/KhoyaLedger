# Khoya Business Ledger

A complete business management application for a family-run khoya (milk solids) business in Maharashtra. Built with ASP.NET Core 10 MVC, EF Core, SQLite, and Bootstrap 5.

## Answers the owner's four daily questions

| # | Question | Where to look |
|---|----------|---------------|
| 1 | How much milk did we collect, and what did it cost? | Dashboard → Milk card; MilkCollection → Index |
| 2 | How much do farmers owe us in advances, and how much do we owe them? | Dashboard → Farmers card; Farmers → Index |
| 3 | How much khoya did we make, sell, and lose to spoilage? | Dashboard → Production card; Production → Index |
| 4 | How much cash moved today, and how much is still with customers? | Dashboard → Cash / Sales cards |

---

## Setup Commands

### Prerequisites
- .NET 10 SDK (`dotnet --version` should show `10.x.x`)
- Windows PC on local Wi-Fi

### First Run

```powershell
# 1. Navigate to project
cd C:\Users\DarshanShahajiGarje\.gemini\antigravity\scratch\KhoyaLedger

# 2. Apply migrations and start
dotnet run
```

The app will:
1. Create `khoya.db` (SQLite database) in the project folder
2. Apply all migrations automatically
3. Seed the owner login, a demo farmer, a demo rate slab, and a demo customer
4. Start listening on **http://0.0.0.0:5000**

### First Owner Login

| Field | Value |
|-------|-------|
| Username | `owner@khoya` |
| Password | `Owner@123` |

**Change the password immediately** via the Identity account management pages after first login.

---

## Local Wi-Fi Access (Phone + Desktop)

The app binds to `0.0.0.0:5000` (all network interfaces) by default.

1. Find the PC's local IP:
   ```powershell
   ipconfig | findstr "IPv4"
   # e.g. 192.168.1.100
   ```
2. On any phone or tablet on the same Wi-Fi: open `http://192.168.1.100:5000`
3. All devices share the same SQLite database on the PC.

> **Windows Firewall**: If the phone cannot reach the app, allow port 5000:
> ```powershell
> New-NetFirewallRule -DisplayName "Khoya Ledger" -Direction Inbound -Protocol TCP -LocalPort 5000 -Action Allow
> ```

---

## Roles and Permissions

| Action | Collector | Owner |
|--------|-----------|-------|
| Record milk collection | ✅ | ✅ |
| Request correction | ✅ | ✅ |
| View farmer list | ✅ | ✅ |
| Post (approve) a collection | ❌ | ✅ |
| Approve correction | ❌ | ✅ |
| Manage rates | ❌ | ✅ |
| Record advances | ❌ | ✅ |
| Create/post settlements | ❌ | ✅ |
| Record payments | ❌ | ✅ |
| Production & stock | ❌ | ✅ |
| Sales & invoicing | ❌ | ✅ |
| CSV export | ❌ | ✅ |

**Add a collector** via `/Identity/Account/Register` then assign the "Collector" role from your user management (or use a simple admin script).

---

## Workflow Guide

### Milestone 1 — Milk Collection, Advances, Farmer Payments

#### Daily milk entry (Collector or Owner)
1. **Milk Collection → Collect** — search farmer by code or name, pick Morning/Evening, enter litres and fat%, save.
2. The system shows the applied rate and gross value. A receipt number (`RCyyyyMMddNNNN`) is generated.
3. **Owner posts entries**: Milk Collection → Index → Post (changes status from Pending → Posted).

#### Rate configuration (Owner)
- **Rates → Create**: choose Buffalo/Cow, flat or fat-based, set ₹/L and effective date.
- Fat-based: add multiple slabs with Min–Max fat ranges.

#### Advances (Owner)
1. **Advances → Create**: select farmer, Disbursement type, amount. Updates `Farmer.AdvanceBalance`.
2. **Advances → Set Recovery Rule**: choose Fixed per litre (e.g. ₹5/L) or % of gross. This rule is applied at settlement time.
3. Direct repayments reduce the advance balance immediately.

#### Farmer Settlement (Owner)
1. **Settlements → Preview**: choose farmer and date range. The system shows:
   - Gross milk value from all Posted entries in the range (each included only once)
   - Computed recovery capped at `min(advance_balance, min(computed, gross_value))`
   - Net payable = gross − recovery
2. **Post Settlement**: creates the advance deduction ledger entry, reduces `Farmer.AdvanceBalance`, increases `Farmer.MilkPayable`.
3. **Record Payment**: pay in full or partial (Cash/UPI/Bank). Reduces `Farmer.MilkPayable`.

#### Verification scenario (Demo → VerifyScenario)
Navigate to `/Demo/VerifyScenario` as Owner. The system runs — in a rolled-back transaction:
- F001, 30L Buffalo @ ₹42/L → Gross ₹1,260 ✓
- Advance ₹1,000, recovery ₹5/L × 30 = ₹150, capped at min(1000, 150) = ₹150 ✓
- Advance outstanding after = ₹850 ✓
- Net payable = ₹1,110 ✓
- ₹600 payout → outstanding ₹510 ✓

All pass/fail checks are shown without touching live data.

#### Farmer Statement
**Farmers → Statement(id)**: shows all deliveries, rates, gross value, advance deductions, payouts, and both outstanding balances. Estimated payoff date shown when ≥7 days of history is available.

#### Shift Tally
**Milk Collection → Shift Tally**: enter the independently measured total litres for a shift. The system shows the variance against the sum of farmer entries.

---

### Milestone 2 — Khoya Production and Stock

1. **Production → Create**: enter batch date, litres of milk used, khoya produced (kg), wood cost, labour, transport, other costs.
2. **Post Batch**: validates that enough milk was collected; computes total cost, cost per kg (`total_cost / khoya_kg`), yield (`khoya_kg / milk_litres × 100`). Adds a stock-in movement.
3. **Spoilage**: record spoilage kg from a posted batch → subtracts from stock.
4. **Stock**: every batch, sale, spoilage, and adjustment is tracked in `KhoyaStockMovements`. Current stock = sum of all movements.

---

### Milestone 3 — Sales, Dashboard, Exports

#### Sales
1. **Sales → Create**: pick customer, production batch, quantity kg, price per kg. Validates available stock. Generates `INVyyyyMMddNNNN`.
2. **Post Sale** → dispatches stock (subtracts from `KhoyaStockMovements`).
3. **Record Payment**: Cash/UPI/Bank, full or partial.

#### Dashboard
`/Dashboard/Index?fromDate=YYYY-MM-DD&toDate=YYYY-MM-DD`
- Card 1: Litres collected, gross milk purchase value
- Card 2: Total farmer advance outstanding, total milk payable
- Card 3: Khoya produced in period, current stock kg
- Card 4: Customer receivables, overdue amount, cash movements (farmer payouts + sale receipts)

#### CSV Exports (Owner)
- `/Export/ExportMilkCsv?from=YYYY-MM-DD&to=YYYY-MM-DD`
- `/Export/ExportFarmerStatement?id=N&from=YYYY-MM-DD&to=YYYY-MM-DD`

---

## Decimal and Rounding Rules

All monetary amounts use `decimal` (C# 128-bit decimal) with:
```csharp
Math.Round(value, 2, MidpointRounding.AwayFromZero)
```
This matches standard Indian business rounding (round half up). Rates are stored to 4 decimal places; payable amounts to 2.

---

## Database Backup and Restore

The entire database is a single file: **`khoya.db`** in the project folder.

### Backup
```powershell
# Manual backup
Copy-Item "C:\...\KhoyaLedger\khoya.db" "D:\Backups\khoya_$(Get-Date -Format 'yyyyMMdd_HHmm').db"
```

### Scheduled backup (Windows Task Scheduler)
Create `backup_khoya.ps1`:
```powershell
$src = "C:\Users\DarshanShahajiGarje\.gemini\antigravity\scratch\KhoyaLedger\khoya.db"
$dst = "D:\KhoyaBackups\khoya_$(Get-Date -Format 'yyyyMMdd_HHmm').db"
New-Item -ItemType Directory -Force "D:\KhoyaBackups" | Out-Null
Copy-Item $src $dst
# Keep last 30 backups
Get-ChildItem "D:\KhoyaBackups\khoya_*.db" | Sort-Object LastWriteTime -Descending | Select-Object -Skip 30 | Remove-Item
```
Schedule it to run daily at 11 PM.

### Restore
```powershell
# 1. Stop the app (Ctrl+C if running in terminal, or stop the Windows Service)
# 2. Copy backup over the live file
Copy-Item "D:\Backups\khoya_20241123_2300.db" "C:\...\KhoyaLedger\khoya.db" -Force
# 3. Restart the app: dotnet run
```

### Verify a restored copy
```powershell
# Point a test instance to the backup file
$env:ConnectionStrings__DefaultConnection = "Data Source=D:\Backups\khoya_20241123_2300.db"
dotnet run --urls "http://localhost:5001"
# Log in and check Dashboard → all numbers match expectations
```

---

## Running as a Windows Service (optional)

```powershell
# 1. Publish
dotnet publish -c Release -o C:\KhoyaLedgerProd

# 2. Install as a service
New-Service -Name "KhoyaLedger" `
  -BinaryPathName "C:\KhoyaLedgerProd\KhoyaLedger.exe" `
  -DisplayName "Khoya Business Ledger" `
  -StartupType Automatic

Start-Service KhoyaLedger
```

---

## Project Structure

```
KhoyaLedger/
├── Controllers/        # MVC controllers (one per domain area)
├── Data/
│   ├── ApplicationDbContext.cs   # EF Core DbContext
│   └── Migrations/               # EF Core migrations
├── Models/             # Domain entities and enums
├── Services/           # Business logic (rates, settlements, stock, seed)
├── Views/              # Razor views (one folder per controller)
│   └── Shared/         # Layout, login partial, validation scripts
├── wwwroot/            # Static assets (Bootstrap, jQuery - local, no CDN)
├── appsettings.json    # Urls, logging config
├── Program.cs          # App startup, DI, middleware, minimal API
└── khoya.db            # SQLite database (auto-created on first run)
```

---

## Verified Workflows

| Workflow | Status |
|----------|--------|
| Create farmer record | ✅ |
| Configure flat/fat-based milk rates | ✅ |
| Record morning/evening milk collection with receipt | ✅ |
| Post (approve) collection | ✅ |
| Collector requests correction; Owner approves | ✅ |
| Shift tally vs sum-of-entries variance | ✅ |
| Record advance disbursement | ✅ |
| Set per-litre recovery rule | ✅ |
| Preview and post farmer settlement (with cap logic) | ✅ |
| Record partial farmer payout | ✅ |
| Farmer statement with both balances | ✅ |
| Demo scenario (₹1,260 gross, ₹150 recovery, ₹850 advance, ₹1,110 payable, ₹510 outstanding after ₹600 payout) | ✅ |
| Create production batch with costs | ✅ |
| Post batch → stock increases | ✅ |
| Record spoilage → stock decreases | ✅ |
| Create customer and sale with invoice number | ✅ |
| Dispatch sale → stock decreases | ✅ |
| Record partial sale payment | ✅ |
| Dashboard with date filter | ✅ |
| CSV export of milk collections | ✅ |
| Database backup and restore | ✅ |

## Known Gaps / Future Enhancements

- **Rate preview on collection screen**: currently shows a static preview; for fat-based slabs the client-side preview shows a placeholder until the server responds. A `/api/rates/preview?milkType=&fat=&date=` endpoint would improve UX.
- **Opening balance wizard**: the `OpeningBalance` action exists but a guided multi-step form for entering historical data for many farmers would speed up onboarding.
- **Push notifications**: no SMS/WhatsApp alerts for large advances or overdue customers.
- **Multi-language**: English/Marathi labels are on the main collection screen; full Marathi UI would need a localization resource file.
- **Windows Service installer**: a one-click `.bat` installer would simplify production deployment.
