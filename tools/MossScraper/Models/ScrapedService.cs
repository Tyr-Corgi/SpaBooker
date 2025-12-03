namespace MossScraper.Models;

public class ScrapedService
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<ServiceVariant> Variants { get; set; } = new();
}

public class ServiceVariant
{
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
}

public class ServiceCsvRecord
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public decimal BasePrice { get; set; }
    public bool IsActive { get; set; } = true;
    public int LocationId { get; set; } = 1;
}

