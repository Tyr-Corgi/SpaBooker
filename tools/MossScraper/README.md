# Moss Spa Service Scraper & Importer

A comprehensive tool to extract service data from [Moss Spa Christchurch](https://www.mossspa.co.nz/christchurch) and import it directly into your SpaBooker database.

## Features

- ✅ **Web Scraping**: Extracts all services from Moss Spa website
- ✅ **Data Cleaning**: Removes duplicates and fixes parsing errors
- ✅ **Direct Import**: Imports services directly into PostgreSQL database
- ✅ **Safe Deletion**: Removes demo/filler services before import
- ✅ **Transaction Safety**: All database changes are atomic (all or nothing)

## What It Does

### Phase 1: Scraping
1. Scrapes all service categories (Facials, Massage, Body, Packages, etc.)
2. Extracts names, descriptions, durations, and prices
3. Handles multiple duration/price options
4. Generates raw CSV output

### Phase 2: Importing
1. Reads cleaned CSV with 37 unique services
2. Connects to your PostgreSQL database
3. Deletes all filler services:
   - Swedish Massage
   - Deep Tissue Massage
   - Hot Stone Massage
   - Aromatherapy Massage
   - Couples Massage
   - Luxury Facial Treatment
   - Body Scrub & Wrap
   - Exclusive Spa Day Package
4. Imports real Moss Spa services
5. All within a safe transaction (rollback on any error)

## Prerequisites

- .NET 8.0 SDK
- PostgreSQL database (SpaBooker)
- Database connection string
- Internet connection (for scraping)

## Quick Start

### Option 1: Scrape Only (Generate CSV)

```bash
cd tools/MossScraper
dotnet run
```

Output: `output/moss_services.csv`

### Option 2: Direct Database Import (Recommended)

   ```bash
   cd tools/MossScraper
dotnet run import "Host=localhost;Database=spabooker;Username=postgres;Password=yourpass" 1
```

Arguments:
- `import` - Import mode
- Connection string - Your PostgreSQL connection
- `1` - Location ID in your database

## Detailed Usage

### Step 1: Scraping (Already Done!)

The scraping has already been completed and generated:
- `output/moss_services.csv` (raw, with duplicates)
- `output/moss_services_cleaned.csv` (cleaned, 37 unique services)

### Step 2: Review Cleaned Data

Open `output/moss_services_cleaned.csv` to review the services that will be imported:

| Name | Category | Duration | Price |
|------|----------|----------|-------|
| Anti-Ageing Collagen and Gold Facial | Facial | 90 min | $260 |
| CoreStone Massage | Massage | 60 min | $180 |
| Luxurious Pinotage & Lime Salt Glow | Body Treatment | 60 min | $185 |
| Mānuka Honey Indulgence | Package | 75 min | $195 |

**37 total services** across 4 categories.

### Step 3: Import to Database

#### Method A: Using Command Line (Easiest)

```bash
dotnet run import "YourConnectionString" YourLocationId
```

Example:
```bash
dotnet run import "Host=localhost;Database=spabooker;Username=postgres;Password=Admin123!" 1
```

#### Method B: Using Environment Variables

```bash
# Set environment variables
$env:DB_CONNECTION_STRING="Host=localhost;Database=spabooker;Username=postgres;Password=pass"
$env:LOCATION_ID="1"

# Run importer
dotnet run import
```

#### Method C: Using SQL Script (Manual)

1. Run the deletion script:
   ```bash
psql -U postgres -d spabooker -f sql/01_DeleteFillerServices.sql
```

2. Import CSV using your preferred method (pgAdmin, COPY command, etc.)

### Step 4: Verify Import

1. Check your SpaBooker admin panel: `/admin/services`
2. You should see 37 new services
3. Filler services should be gone

## What Gets Deleted

Before importing, these demo services are removed:
- ❌ Swedish Massage
- ❌ Deep Tissue Massage
- ❌ Hot Stone Massage
- ❌ Aromatherapy Massage
- ❌ Couples Massage
- ❌ Luxury Facial Treatment
- ❌ Body Scrub & Wrap
- ❌ Exclusive Spa Day Package

**All related data is also deleted:**
- ServiceTherapist assignments
- RoomServiceCapabilities
- Bookings (if any exist for these services)

## What Gets Imported

37 real Moss Spa services:

### Facials (7 services)
- Anti-Ageing Collagen and Gold Facial ($260)
- AcniVine Balancing Facial ($150)
- Collagen Renewal Facial (2 durations)
- Clarifying Lactic Acid Peel ($120)
- Radiating Vitamin A Peel ($160)
- Add Refreshing Retinol Eye Mask ($40)

### Massage (13 services)
- CoreStone Massage ($180)
- Soothing Scalp Massage ($95)
- Relaxing Back, Neck and Shoulder ($100)
- Full Body Harmony (2 durations)
- Deep Tissue Wellness (2 durations)
- Invigorating Bamboo Fusion ($175)
- Nourishing Maternity Massage (2 durations)
- You and Me Pamper (3 durations for couples)

### Body Treatments (10 services)
- Luxurious Pinotage & Lime Salt Glow (2 options)
- Grape Cleanse & Hydrating Milk Ceremony ($160)
- Detoxifying Thermal Mud Therapy (2 options)
- Invigorating Leg Treatment ($145)
- Muscle-Release Body Ritual ($280)
- Maternity Pampering Treatment ($190)
- Ultimate Pamper Package ($360)
- Add Restorative Spa Immersion ($65)

### Packages (7 services)
- Mānuka Honey Indulgence ($195)
- High Tea Pamper Package ($150)
- Relax and Restore ($225)
- Replenish and Pamper ($250)
- Refresh and Reconnect Couples Pamper ($690)
- Couples Connection ($550)
- Bespoke Bridal Pamper ($350+)

## Output Files

```
output/
├── moss_services.csv           # Raw scraped data (74 entries with duplicates)
└── moss_services_cleaned.csv   # Cleaned data ready for import (37 unique services)

sql/
└── 01_DeleteFillerServices.sql # Manual SQL deletion script
```

## Configuration

### Connection String Format

```
Host=localhost;Database=spabooker;Username=postgres;Password=yourpassword;Port=5432
```

Common configurations:
- **Local**: `Host=localhost;Database=spabooker;Username=postgres;Password=pass`
- **Docker**: `Host=host.docker.internal;Database=spabooker;Username=postgres;Password=pass`
- **Remote**: `Host=your-server.com;Database=spabooker;Username=user;Password=pass;SSL Mode=Require`

### Location ID

The Location ID is the `Id` field from your `Locations` table. To find it:

```sql
SELECT "Id", "Name" FROM "Locations";
```

Usually it's `1` for your first/main location.

## Error Handling

### Import Errors

The importer uses transactions - if ANY error occurs:
- ✅ All changes are rolled back
- ✅ Your database remains unchanged
- ✅ You can safely retry

### Common Errors

**"Connection failed"**
- Check connection string
- Verify PostgreSQL is running
- Check firewall/network settings

**"CSV file not found"**
- Ensure you're in the `tools/MossScraper` directory
- Check that `output/moss_services_cleaned.csv` exists

**"Location ID not found"**
- Verify the location ID exists in your database
- Run: `SELECT * FROM "Locations"`

## Next Steps After Import

1. **Assign Therapists**: Link therapists to services in Service Management
2. **Configure Rooms**: Set up which rooms can accommodate which services
3. **Add Images**: Upload service images (optional)
4. **Test Bookings**: Create test bookings to verify everything works
5. **Adjust Prices**: Update prices if needed for your location

## Technical Details

### Dependencies
- **HtmlAgilityPack**: HTML parsing for web scraping
- **CsvHelper**: CSV file generation and reading
- **Npgsql**: PostgreSQL database driver

### Database Tables Modified
- `SpaServices` - Service data
- `ServiceTherapists` - Therapist assignments (deleted for old services)
- `RoomServiceCapabilities` - Room capabilities (deleted for old services)
- `Bookings` - Booking records (deleted for old services if any exist)

### Transaction Safety
All database operations are wrapped in a transaction:
```csharp
BEGIN TRANSACTION
  - Delete old services
  - Delete related data
  - Insert new services
COMMIT (only if all succeed)
```

## Troubleshooting

### Scraper Issues
- Check `errors.log` for details
- Verify internet connection
- Website may have changed structure - update `Services/WebScraper.cs`

### Import Issues
- Check connection string format
- Verify PostgreSQL is accessible
- Confirm location ID exists
- Check user has INSERT/DELETE permissions

### Data Issues
- Review `moss_services_cleaned.csv` before importing
- Update prices/durations as needed
- Adjust descriptions for clarity

## Development

### Project Structure
```
MossScraper/
├── Program.cs              # Main entry point (scraper + importer)
├── ServiceImporter.cs      # Database import logic
├── Models/
│   └── ScrapedService.cs   # Data models
├── Services/
│   ├── WebScraper.cs       # Web scraping logic
│   └── CsvGenerator.cs     # CSV generation
└── output/
    └── *.csv               # Generated files
```

### Modifying the Scraper
1. Update selectors in `Services/WebScraper.cs`
2. Test with single category first
3. Run scraper to generate new CSV
4. Review and clean data
5. Re-import

### Custom Categories
Edit `moss_services_cleaned.csv` to:
- Change service categories
- Update durations
- Modify prices
- Add/remove services

Then re-import.

## Safety Features

✅ **Transaction-based**: All or nothing, no partial imports
✅ **Rollback on error**: Database unchanged if anything fails
✅ **Duplicate removal**: Cleaned data has no duplicates
✅ **Validated input**: Checks CSV exists, connection works
✅ **Clear logging**: See exactly what's happening

## Support

For issues:
1. Check error messages and logs
2. Verify configuration (connection string, location ID)
3. Review this README
4. Check database permissions

## License

This tool is part of the SpaBooker project.

---

**Last Updated**: 2025-11-26
**Version**: 1.0
**Services**: 37 from Moss Spa Christchurch
