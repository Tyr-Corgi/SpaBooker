using MossScraper.Services;

namespace MossScraper;

class Program
{
    static async Task Main(string[] args)
    {
        // Check if running in import mode
        if (args.Length > 0 && args[0] == "import")
        {
            await RunImportAsync(args);
            return;
        }

        // Check if running in update-images mode
        if (args.Length > 0 && args[0] == "update-images")
        {
            await RunUpdateImagesAsync(args);
            return;
        }

        // Otherwise run the scraper
        await RunScraperAsync();
    }

    static async Task RunImportAsync(string[] args)
    {
        Console.WriteLine("===========================================");
        Console.WriteLine("  Moss Spa Service Importer");
        Console.WriteLine("===========================================");
        Console.WriteLine();

        // Get configuration from args or environment variables
        string? connectionString = args.Length > 1 ? args[1] : Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
        string? locationIdStr = args.Length > 2 ? args[2] : Environment.GetEnvironmentVariable("LOCATION_ID");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.WriteLine("ERROR: Connection string is required!");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  dotnet run import \"<connection-string>\" <location-id>");
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine("  dotnet run import \"Host=localhost;Database=spabooker;Username=postgres;Password=pass\" 1");
            return;
        }

        if (!int.TryParse(locationIdStr, out int locationId) || locationId <= 0)
        {
            Console.WriteLine("ERROR: Valid location ID is required!");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  dotnet run import \"<connection-string>\" <location-id>");
            return;
        }

        Console.WriteLine("Configuration:");
        Console.WriteLine($"  - CSV File: output/moss_services_cleaned.csv");
        Console.WriteLine($"  - Target Location ID: {locationId}");
        Console.WriteLine();

        try
        {
            var importer = new ServiceImporter(connectionString);
            var csvPath = Path.Combine("output", "moss_services_cleaned.csv");

            if (!File.Exists(csvPath))
            {
                Console.WriteLine($"ERROR: CSV file not found at: {Path.GetFullPath(csvPath)}");
                return;
            }

            var result = await importer.ImportServicesAsync(csvPath, locationId);

            Console.WriteLine();
            Console.WriteLine("===========================================");
            if (result.Success)
            {
                Console.WriteLine("  ✓ SUCCESS!");
                Console.WriteLine("===========================================");
                Console.WriteLine();
                Console.WriteLine($"Services imported: {result.Inserted} out of {result.TotalRead}");
                Console.WriteLine();
                Console.WriteLine("Next steps:");
                Console.WriteLine("  1. Check your service management page");
                Console.WriteLine("  2. Assign therapists to services");
                Console.WriteLine("  3. Configure room capabilities");
            }
            else
            {
                Console.WriteLine("  ✗ FAILED");
                Console.WriteLine("===========================================");
                Console.WriteLine();
                Console.WriteLine($"Error: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("ERROR: " + ex.Message);
        }
    }

    static async Task RunUpdateImagesAsync(string[] args)
    {
        Console.WriteLine("===========================================");
        Console.WriteLine("  Service Image URL Updater");
        Console.WriteLine("===========================================");
        Console.WriteLine();

        // Get configuration from args or environment variables
        string? connectionString = args.Length > 1 ? args[1] : Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.WriteLine("ERROR: Connection string is required!");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  dotnet run update-images \"<connection-string>\"");
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine("  dotnet run update-images \"Host=localhost;Database=spabooker;Username=postgres;Password=pass\"");
            return;
        }

        Console.WriteLine("This will update all services with appropriate image URLs from Unsplash.");
        Console.WriteLine();

        try
        {
            var updater = new ImageUrlUpdater(connectionString);
            var updated = await updater.UpdateServiceImagesAsync();

            Console.WriteLine();
            Console.WriteLine("===========================================");
            Console.WriteLine("  ✓ SUCCESS!");
            Console.WriteLine("===========================================");
            Console.WriteLine();
            Console.WriteLine($"Updated {updated} service images!");
            Console.WriteLine();
            Console.WriteLine("Next steps:");
            Console.WriteLine("  1. Check your service management page");
            Console.WriteLine("  2. View the updated service images");
            Console.WriteLine("  3. Replace any images you want to customize");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("ERROR: " + ex.Message);
        }
    }

    static async Task RunScraperAsync()
    {
        Console.WriteLine("===========================================");
        Console.WriteLine("  Moss Spa Service Scraper");
        Console.WriteLine("===========================================");
        Console.WriteLine();
        Console.WriteLine("This tool will scrape service data from:");
        Console.WriteLine("https://www.mossspa.co.nz/christchurch");
        Console.WriteLine();
        Console.WriteLine("Output will be saved to: output/moss_services.csv");
        Console.WriteLine();
        Console.WriteLine("Starting scraper...");

        try
        {
            // Step 1: Scrape services
            Console.WriteLine("Step 1: Scraping services from website...");
            Console.WriteLine("-------------------------------------------");
            var scraper = new WebScraper();
            var services = await scraper.ScrapeAllServicesAsync();

            if (services.Count == 0)
            {
                Console.WriteLine();
                Console.WriteLine("ERROR: No services were scraped!");
                Console.WriteLine("This could be due to:");
                Console.WriteLine("  - Website structure has changed");
                Console.WriteLine("  - Network connectivity issues");
                Console.WriteLine("  - Website blocking the scraper");
                Console.WriteLine();
                Console.WriteLine("Check errors.log for details.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine($"Successfully scraped {services.Count} service categories");
            Console.WriteLine();

            // Step 2: Generate CSV
            Console.WriteLine("Step 2: Generating CSV file...");
            Console.WriteLine("-------------------------------------------");
            var csvGenerator = new CsvGenerator();
            var outputPath = Path.Combine("output", "moss_services.csv");
            await csvGenerator.GenerateCsvAsync(services, outputPath);

            Console.WriteLine();
            Console.WriteLine("===========================================");
            Console.WriteLine("  SUCCESS!");
            Console.WriteLine("===========================================");
            Console.WriteLine();
            Console.WriteLine($"CSV file has been generated: {Path.GetFullPath(outputPath)}");
            Console.WriteLine();
            Console.WriteLine("Next steps:");
            Console.WriteLine("  1. Open the CSV file and review the data");
            Console.WriteLine("  2. Update LocationId column to match your database");
            Console.WriteLine("  3. Adjust any prices or durations as needed");
            Console.WriteLine("  4. Import into your database using:");
            Console.WriteLine("     - SQL Server Import/Export Wizard");
            Console.WriteLine("     - BULK INSERT command");
            Console.WriteLine("     - Or manual entry");
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("===========================================");
            Console.WriteLine("  ERROR!");
            Console.WriteLine("===========================================");
            Console.WriteLine();
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.WriteLine();
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            Console.WriteLine();
            Console.WriteLine("Error has been logged to errors.log");
            
            File.AppendAllText("errors.log", 
                $"{DateTime.Now}: FATAL ERROR - {ex.Message}\n{ex.StackTrace}\n\n");
        }
    }
}
