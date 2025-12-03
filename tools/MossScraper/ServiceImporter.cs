using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Npgsql;

namespace MossScraper;

/// <summary>
/// Imports cleaned Moss Spa services directly into the PostgreSQL database
/// </summary>
public class ServiceImporter
{
    private readonly string _connectionString;

    public ServiceImporter(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<ImportResult> ImportServicesAsync(string csvFilePath, int locationId)
    {
        var result = new ImportResult();

        try
        {
            // Read services from CSV
            var services = ReadServicesFromCsv(csvFilePath, locationId);
            result.TotalRead = services.Count;

            Console.WriteLine($"Read {services.Count} services from CSV");
            Console.WriteLine();

            // Connect to database
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            Console.WriteLine("Connected to database");
            Console.WriteLine();

            // Start transaction
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Step 1: Delete filler services
                Console.WriteLine("Step 1: Deleting filler services...");
                await DeleteFillerServicesAsync(connection, transaction);
                Console.WriteLine("✓ Filler services deleted");
                Console.WriteLine();

                // Step 2: Insert new services
                Console.WriteLine("Step 2: Importing Moss Spa services...");
                result.Inserted = await InsertServicesAsync(connection, transaction, services);
                Console.WriteLine($"✓ Imported {result.Inserted} services");
                Console.WriteLine();

                // Commit transaction
                await transaction.CommitAsync();
                result.Success = true;

                Console.WriteLine("Import completed successfully!");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                result.Success = false;
                result.ErrorMessage = $"Transaction failed: {ex.Message}";
                throw;
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            Console.WriteLine($"ERROR: {ex.Message}");
        }

        return result;
    }

    private List<ServiceToImport> ReadServicesFromCsv(string csvFilePath, int locationId)
    {
        var services = new List<ServiceToImport>();

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Comment = '#',
            AllowComments = true,
            TrimOptions = TrimOptions.Trim
        };

        using var reader = new StreamReader(csvFilePath);
        using var csv = new CsvReader(reader, config);

        csv.Read();
        csv.ReadHeader();

        while (csv.Read())
        {
            var service = new ServiceToImport
            {
                Name = csv.GetField<string>("Name") ?? "",
                Category = csv.GetField<string>("Category") ?? "",
                Description = csv.GetField<string>("Description") ?? "",
                DurationMinutes = csv.GetField<int>("DurationMinutes"),
                BasePrice = csv.GetField<decimal>("BasePrice"),
                IsActive = csv.GetField<bool>("IsActive"),
                LocationId = locationId // Use the provided location ID
            };

            services.Add(service);
        }

        return services;
    }

    private async Task DeleteFillerServicesAsync(NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        var fillerServices = new[]
        {
            "Swedish Massage",
            "Deep Tissue Massage",
            "Hot Stone Massage",
            "Aromatherapy Massage",
            "Couples Massage",
            "Luxury Facial Treatment",
            "Body Scrub & Wrap",
            "Exclusive Spa Day Package"
        };

        // Delete ServiceTherapists
        var deleteServiceTherapistsSql = @"
            DELETE FROM ""ServiceTherapists""
            WHERE ""ServiceId"" IN (
                SELECT ""Id"" FROM ""SpaServices""
                WHERE ""Name"" = ANY(@names)
            )";

        await using (var cmd = new NpgsqlCommand(deleteServiceTherapistsSql, connection, transaction))
        {
            cmd.Parameters.AddWithValue("names", fillerServices);
            var deleted = await cmd.ExecuteNonQueryAsync();
            Console.WriteLine($"  - Deleted {deleted} ServiceTherapist assignments");
        }

        // Delete RoomServiceCapabilities
        var deleteRoomCapabilitiesSql = @"
            DELETE FROM ""RoomServiceCapabilities""
            WHERE ""ServiceId"" IN (
                SELECT ""Id"" FROM ""SpaServices""
                WHERE ""Name"" = ANY(@names)
            )";

        await using (var cmd = new NpgsqlCommand(deleteRoomCapabilitiesSql, connection, transaction))
        {
            cmd.Parameters.AddWithValue("names", fillerServices);
            var deleted = await cmd.ExecuteNonQueryAsync();
            Console.WriteLine($"  - Deleted {deleted} RoomServiceCapabilities");
        }

        // Delete Bookings (optional - preserves data integrity)
        var deleteBookingsSql = @"
            DELETE FROM ""Bookings""
            WHERE ""ServiceId"" IN (
                SELECT ""Id"" FROM ""SpaServices""
                WHERE ""Name"" = ANY(@names)
            )";

        await using (var cmd = new NpgsqlCommand(deleteBookingsSql, connection, transaction))
        {
            cmd.Parameters.AddWithValue("names", fillerServices);
            var deleted = await cmd.ExecuteNonQueryAsync();
            Console.WriteLine($"  - Deleted {deleted} Bookings");
        }

        // Delete Services
        var deleteServicesSql = @"
            DELETE FROM ""SpaServices""
            WHERE ""Name"" = ANY(@names)";

        await using (var cmd = new NpgsqlCommand(deleteServicesSql, connection, transaction))
        {
            cmd.Parameters.AddWithValue("names", fillerServices);
            var deleted = await cmd.ExecuteNonQueryAsync();
            Console.WriteLine($"  - Deleted {deleted} SpaServices");
        }
    }

    private async Task<int> InsertServicesAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        List<ServiceToImport> services)
    {
        var insertSql = @"
            INSERT INTO ""SpaServices"" 
            (""Name"", ""Description"", ""DurationMinutes"", ""BasePrice"", ""LocationId"", ""IsActive"", ""RequiresMembership"", ""CreatedAt"")
            VALUES 
            (@Name, @Description, @DurationMinutes, @BasePrice, @LocationId, @IsActive, false, @CreatedAt)";

        var inserted = 0;

        foreach (var service in services)
        {
            await using var cmd = new NpgsqlCommand(insertSql, connection, transaction);
            cmd.Parameters.AddWithValue("Name", service.Name);
            cmd.Parameters.AddWithValue("Description", service.Description);
            cmd.Parameters.AddWithValue("DurationMinutes", service.DurationMinutes);
            cmd.Parameters.AddWithValue("BasePrice", service.BasePrice);
            cmd.Parameters.AddWithValue("LocationId", service.LocationId);
            cmd.Parameters.AddWithValue("IsActive", service.IsActive);
            cmd.Parameters.AddWithValue("CreatedAt", DateTime.UtcNow);

            await cmd.ExecuteNonQueryAsync();
            inserted++;

            Console.WriteLine($"  [{inserted}/{services.Count}] {service.Name} (${service.BasePrice}, {service.DurationMinutes} min)");
        }

        return inserted;
    }
}

public class ServiceToImport
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public decimal BasePrice { get; set; }
    public bool IsActive { get; set; }
    public int LocationId { get; set; }
}

public class ImportResult
{
    public bool Success { get; set; }
    public int TotalRead { get; set; }
    public int Inserted { get; set; }
    public string? ErrorMessage { get; set; }
}

