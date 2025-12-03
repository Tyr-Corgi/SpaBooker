using CsvHelper;
using CsvHelper.Configuration;
using MossScraper.Models;
using System.Globalization;

namespace MossScraper.Services;

public class CsvGenerator
{
    public async Task GenerateCsvAsync(List<ScrapedService> services, string outputPath)
    {
        var records = new List<ServiceCsvRecord>();

        foreach (var service in services)
        {
            if (service.Variants.Count == 0)
            {
                // Service without variants - add with default values
                records.Add(new ServiceCsvRecord
                {
                    Name = service.Name,
                    Category = service.Category,
                    Description = service.Description,
                    DurationMinutes = 60, // Default
                    BasePrice = 0,
                    IsActive = true,
                    LocationId = 1
                });
            }
            else
            {
                // Create a record for each variant
                foreach (var variant in service.Variants)
                {
                    var recordName = service.Variants.Count > 1
                        ? $"{service.Name} ({variant.DurationMinutes} min)"
                        : service.Name;

                    records.Add(new ServiceCsvRecord
                    {
                        Name = recordName,
                        Category = service.Category,
                        Description = service.Description,
                        DurationMinutes = variant.DurationMinutes,
                        BasePrice = variant.Price,
                        IsActive = true,
                        LocationId = 1
                    });
                }
            }
        }

        // Ensure output directory exists
        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Write CSV
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
        };

        using var writer = new StreamWriter(outputPath);
        using var csv = new CsvWriter(writer, config);
        
        // Write header comment
        await writer.WriteLineAsync("# Moss Spa Services - Scraped from mossspa.co.nz/christchurch");
        await writer.WriteLineAsync($"# Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        await writer.WriteLineAsync($"# Total Services: {records.Count}");
        await writer.WriteLineAsync("# To import: Update LocationId to match your database, then use SQL bulk insert or manual import");
        await writer.WriteLineAsync("#");
        
        await csv.WriteRecordsAsync(records);

        Console.WriteLine($"\nCSV generated successfully: {outputPath}");
        Console.WriteLine($"Total records: {records.Count}");
        Console.WriteLine("\nBreakdown by category:");
        
        foreach (var group in records.GroupBy(r => r.Category))
        {
            Console.WriteLine($"  {group.Key}: {group.Count()} services");
        }
    }
}

