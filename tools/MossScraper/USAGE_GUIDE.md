# Quick Start Guide

## Running the Scraper

### Step 1: Run the Tool
```bash
cd tools/MossScraper
dotnet run
```

Press any key when prompted to start.

### Step 2: Wait for Completion
The tool will:
- Scrape 5 categories (Facials, Massage, Body, Packages, Nails/Brows/Wax)
- Extract approximately 40-50 services
- Generate a CSV file in `output/moss_services.csv`

Expected runtime: 10-15 seconds

### Step 3: Review the CSV
Open `output/moss_services.csv` and verify:
- ✅ Service names are correct
- ✅ Prices match the website
- ✅ Durations are accurate
- ✅ Descriptions are present

### Step 4: Update LocationId
Change the `LocationId` column from `1` to match your SpaBooker database location ID.

### Step 5: Import to Database

**Quick Import (pgAdmin for PostgreSQL):**
1. Open pgAdmin
2. Right-click on `SpaServices` table → Import/Export
3. Select the CSV file
4. Map columns appropriately
5. Click OK

**SQL Import:**
```sql
COPY SpaServices (Name, Category, Description, DurationMinutes, BasePrice, IsActive, LocationId)
FROM '/path/to/moss_services.csv'
DELIMITER ','
CSV HEADER;
```

## Expected Output

The CSV will contain services like:

| Name | Category | DurationMinutes | BasePrice |
|------|----------|----------------|-----------|
| CoreStone Massage | Massage | 60 | 180.00 |
| Soothing Scalp Massage | Massage | 30 | 95.00 |
| Full Body Harmony Massage (60 min) | Massage | 60 | 165.00 |
| Full Body Harmony Massage (90 min) | Massage | 90 | 230.00 |
| Anti-Ageing Collagen and Gold Facial | Facial | 90 | 260.00 |
| Mānuka Honey Indulgence | Package | 75 | 195.00 |
| ... | ... | ... | ... |

## Troubleshooting

**"No services were scraped!"**
- Check your internet connection
- The website might be down
- Website structure may have changed

**"Error fetching URL"**
- Network firewall blocking the request
- Website blocking the scraper
- Try again in a few minutes

**Prices seem wrong**
- Website may have updated prices
- Manually verify against the website
- Update the CSV before importing

## Notes

- Services with multiple duration/price options are split into separate rows
- Each row represents one bookable service variant
- The `LocationId` defaults to 1 - update this before importing
- All services are set to `IsActive = true` by default

