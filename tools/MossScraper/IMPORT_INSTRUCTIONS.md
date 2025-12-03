# Quick Import Instructions

## What You Need

1. Your PostgreSQL connection string
2. Your Location ID (usually `1`)

## Step-by-Step

### 1. Get Your Connection String

From your `appsettings.json` or user secrets:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=spabooker;Username=postgres;Password=yourpassword"
}
```

### 2. Find Your Location ID

Run this SQL query or check your admin panel:

```sql
SELECT "Id", "Name" FROM "Locations";
```

Usually it's `1` for Moss Spa Christchurch.

### 3. Navigate to the Scraper Directory

```bash
cd C:\Mac\Home\Desktop\Repos\SpaBooker\tools\MossScraper
```

### 4. Run the Import

Replace the values below with your actual connection string and location ID:

```bash
dotnet run import "Host=localhost;Database=spabooker;Username=postgres;Password=yourpass" 1
```

**Example with typical local setup:**

```bash
dotnet run import "Host=localhost;Database=spabooker;Username=postgres;Password=Admin123!" 1
```

### 5. What Happens

The tool will:

1. ✅ Connect to your database
2. ✅ Delete 8 filler/demo services:
   - Swedish Massage
   - Deep Tissue Massage
   - Hot Stone Massage
   - Aromatherapy Massage
   - Couples Massage
   - Luxury Facial Treatment
   - Body Scrub & Wrap
   - Exclusive Spa Day Package
3. ✅ Import 37 real Moss Spa services
4. ✅ Show progress for each service
5. ✅ Commit all changes (or rollback if any error)

### 6. Verify Import

1. Go to your SpaBooker admin panel: `http://localhost:5000/admin/services`
2. You should see 37 services
3. Old filler services should be gone

## Example Output

```
===========================================
  Moss Spa Service Importer
===========================================

Configuration:
  - CSV File: output/moss_services_cleaned.csv
  - Target Location ID: 1

Step 1: Deleting filler services...
  - Deleted 0 ServiceTherapist assignments
  - Deleted 8 RoomServiceCapabilities
  - Deleted 0 Bookings
  - Deleted 8 SpaServices
✓ Filler services deleted

Step 2: Importing Moss Spa services...
  [1/37] Anti-Ageing Collagen and Gold Facial ($260, 90 min)
  [2/37] AcniVine Balancing Facial ($150, 60 min)
  ...
  [37/37] Bespoke Bridal Pamper ($350, 120 min)
✓ Imported 37 services

===========================================
  ✓ SUCCESS!
===========================================

Services imported: 37 out of 37

Next steps:
  1. Check your service management page
  2. Assign therapists to services
  3. Configure room capabilities
```

## Troubleshooting

### "Connection failed"
- Check your connection string is correct
- Verify PostgreSQL is running
- Check username/password

### "CSV file not found"
- Make sure you're in the `tools/MossScraper` directory
- The file `output/moss_services_cleaned.csv` should exist

### "Invalid location ID"
- Run: `SELECT "Id", "Name" FROM "Locations";`
- Use the correct ID number

## Need Help?

Check the full README.md for more details:
```bash
cat README.md
```

## Safe to Run Multiple Times?

⚠️ **Warning**: Running multiple times will:
- Delete the filler services again (if they exist)
- Create duplicate real services

If you need to re-import:
1. Manually delete all services first, OR
2. Modify the deletion list in `ServiceImporter.cs`

## Next Steps

After successful import:

1. **Assign Therapists**: Go to each service and assign qualified therapists
2. **Set Room Capabilities**: Configure which rooms can accommodate which services
3. **Add Images**: Upload service images for better presentation
4. **Test Bookings**: Create test bookings to verify everything works

---

**Need the connection string?** Check your `appsettings.json` or user secrets.
**Need the location ID?** Run `SELECT * FROM "Locations"` or check admin panel.

