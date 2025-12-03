using Npgsql;

namespace MossScraper;

/// <summary>
/// Updates service records with appropriate image URLs from Unsplash
/// </summary>
public class ImageUrlUpdater
{
    private readonly string _connectionString;

    public ImageUrlUpdater(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<int> UpdateServiceImagesAsync()
    {
        var updated = 0;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        Console.WriteLine("Connected to database");
        Console.WriteLine();

        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            // Get all services
            var services = await GetServicesAsync(connection, transaction);
            Console.WriteLine($"Found {services.Count} services");
            Console.WriteLine();

            foreach (var service in services)
            {
                var imageUrl = GetImageUrlForService(service.Name, service.Description);
                
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    await UpdateServiceImageAsync(connection, transaction, service.Id, imageUrl);
                    updated++;
                    Console.WriteLine($"✓ [{updated}/{services.Count}] {service.Name}");
                    Console.WriteLine($"  → {imageUrl}");
                }
            }

            await transaction.CommitAsync();
            Console.WriteLine();
            Console.WriteLine($"Successfully updated {updated} service images!");

            return updated;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"ERROR: {ex.Message}");
            throw;
        }
    }

    private async Task<List<ServiceInfo>> GetServicesAsync(NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        var services = new List<ServiceInfo>();

        var sql = @"SELECT ""Id"", ""Name"", ""Description"" FROM ""SpaServices"" ORDER BY ""Name""";

        await using var cmd = new NpgsqlCommand(sql, connection, transaction);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            services.Add(new ServiceInfo
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2)
            });
        }

        return services;
    }

    private async Task UpdateServiceImageAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, int serviceId, string imageUrl)
    {
        var sql = @"UPDATE ""SpaServices"" SET ""ImageUrl"" = @imageUrl WHERE ""Id"" = @id";

        await using var cmd = new NpgsqlCommand(sql, connection, transaction);
        cmd.Parameters.AddWithValue("imageUrl", imageUrl);
        cmd.Parameters.AddWithValue("id", serviceId);

        await cmd.ExecuteNonQueryAsync();
    }

    private string GetImageUrlForService(string name, string description)
    {
        var nameLower = name.ToLower();
        var descLower = description.ToLower();

        // Facials
        if (nameLower.Contains("facial") || nameLower.Contains("peel"))
        {
            if (nameLower.Contains("gold") || nameLower.Contains("collagen"))
                return "https://images.unsplash.com/photo-1570172619644-dfd03ed5d881?w=800&h=600&fit=crop"; // Luxury facial
            if (nameLower.Contains("vitamin") || nameLower.Contains("radiat"))
                return "https://images.unsplash.com/photo-1560750588-73207b1ef5b8?w=800&h=600&fit=crop"; // Vitamin facial
            if (nameLower.Contains("acne") || nameLower.Contains("balanc"))
                return "https://images.unsplash.com/photo-1616394584738-fc6e612e71b9?w=800&h=600&fit=crop"; // Acne treatment
            if (nameLower.Contains("eye") || nameLower.Contains("mask"))
                return "https://images.unsplash.com/photo-1562322140-8baeececf3df?w=800&h=600&fit=crop"; // Eye treatment
            
            return "https://images.unsplash.com/photo-1560750123-5d1e4e7fb992?w=800&h=600&fit=crop"; // General facial
        }

        // Massages
        if (nameLower.Contains("massage") || nameLower.Contains("corestone") || 
            nameLower.Contains("scalp") || nameLower.Contains("pamper"))
        {
            if (nameLower.Contains("stone") || nameLower.Contains("corestone"))
                return "https://images.unsplash.com/photo-1544161515-4ab6ce6db874?w=800&h=600&fit=crop"; // Hot stone massage
            if (nameLower.Contains("deep tissue"))
                return "https://images.unsplash.com/photo-1519823551278-64ac92734fb1?w=800&h=600&fit=crop"; // Deep tissue
            if (nameLower.Contains("scalp") || nameLower.Contains("head"))
                return "https://images.unsplash.com/photo-1559056199-641a0ac8b55e?w=800&h=600&fit=crop"; // Scalp massage
            if (nameLower.Contains("back") || nameLower.Contains("shoulder") || nameLower.Contains("neck"))
                return "https://images.unsplash.com/photo-1600334089648-b0d9d3028eb2?w=800&h=600&fit=crop"; // Back massage
            if (nameLower.Contains("bamboo") || nameLower.Contains("fusion"))
                return "https://images.unsplash.com/photo-1540555700478-4be289fbecef?w=800&h=600&fit=crop"; // Bamboo massage
            if (nameLower.Contains("maternity") || nameLower.Contains("pregnancy"))
                return "https://images.unsplash.com/photo-1622287162716-f311baa1a2b8?w=800&h=600&fit=crop"; // Pregnancy massage
            if (nameLower.Contains("couple") || nameLower.Contains("you and me"))
                return "https://images.unsplash.com/photo-1600334129128-685c5582fd35?w=800&h=600&fit=crop"; // Couples massage
            if (nameLower.Contains("harmony") || nameLower.Contains("full body"))
                return "https://images.unsplash.com/photo-1596178060810-1a194f0c7234?w=800&h=600&fit=crop"; // Full body massage
            
            return "https://images.unsplash.com/photo-1607632916265-fe2ba6c7c098?w=800&h=600&fit=crop"; // General massage
        }

        // Body Treatments
        if (nameLower.Contains("body") || nameLower.Contains("scrub") || nameLower.Contains("wrap") || 
            nameLower.Contains("mud") || nameLower.Contains("salt") || nameLower.Contains("glow"))
        {
            if (nameLower.Contains("salt") || nameLower.Contains("glow") || nameLower.Contains("pinotage"))
                return "https://images.unsplash.com/photo-1556228720-195a672e8a03?w=800&h=600&fit=crop"; // Salt scrub
            if (nameLower.Contains("mud") || nameLower.Contains("thermal") || nameLower.Contains("detox"))
                return "https://images.unsplash.com/photo-1571772996211-2f02c9727629?w=800&h=600&fit=crop"; // Mud therapy
            if (nameLower.Contains("grape") || nameLower.Contains("milk") || nameLower.Contains("ceremony"))
                return "https://images.unsplash.com/photo-1552693673-1bf958298935?w=800&h=600&fit=crop"; // Milk bath
            if (nameLower.Contains("leg") || nameLower.Contains("foot"))
                return "https://images.unsplash.com/photo-1598966739654-5e9baa31faa6?w=800&h=600&fit=crop"; // Leg treatment
            if (nameLower.Contains("muscle") || nameLower.Contains("ritual"))
                return "https://images.unsplash.com/photo-1544161515-4ab6ce6db874?w=800&h=600&fit=crop"; // Muscle ritual
            if (nameLower.Contains("ultimate") || nameLower.Contains("immersion"))
                return "https://images.unsplash.com/photo-1540555700478-4be289fbecef?w=800&h=600&fit=crop"; // Ultimate package
            
            return "https://images.unsplash.com/photo-1544161515-4ab6ce6db874?w=800&h=600&fit=crop"; // General body treatment
        }

        // Packages
        if (nameLower.Contains("package") || nameLower.Contains("honey") || nameLower.Contains("tea") || 
            nameLower.Contains("bridal") || nameLower.Contains("restore") || nameLower.Contains("reconnect"))
        {
            if (nameLower.Contains("honey") || nameLower.Contains("manuka"))
                return "https://images.unsplash.com/photo-1608571423902-eed4a5ad8108?w=800&h=600&fit=crop"; // Honey treatment
            if (nameLower.Contains("tea") || nameLower.Contains("high tea"))
                return "https://images.unsplash.com/photo-1564890369478-c89ca6d9cde9?w=800&h=600&fit=crop"; // High tea
            if (nameLower.Contains("bridal") || nameLower.Contains("bride") || nameLower.Contains("bespoke"))
                return "https://images.unsplash.com/photo-1519741497674-611481863552?w=800&h=600&fit=crop"; // Bridal package
            if (nameLower.Contains("couple") || nameLower.Contains("connection") || nameLower.Contains("reconnect"))
                return "https://images.unsplash.com/photo-1600334129128-685c5582fd35?w=800&h=600&fit=crop"; // Couples package
            if (nameLower.Contains("relax") || nameLower.Contains("restore"))
                return "https://images.unsplash.com/photo-1600334089648-b0d9d3028eb2?w=800&h=600&fit=crop"; // Relaxation package
            if (nameLower.Contains("replenish") || nameLower.Contains("pamper"))
                return "https://images.unsplash.com/photo-1540555700478-4be289fbecef?w=800&h=600&fit=crop"; // Pamper package
            
            return "https://images.unsplash.com/photo-1544161515-4ab6ce6db874?w=800&h=600&fit=crop"; // General package
        }

        // Default spa image
        return "https://images.unsplash.com/photo-1540555700478-4be289fbecef?w=800&h=600&fit=crop";
    }

    private class ServiceInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}

