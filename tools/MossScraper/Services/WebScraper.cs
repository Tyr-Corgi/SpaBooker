using HtmlAgilityPack;
using MossScraper.Models;
using System.Text.RegularExpressions;

namespace MossScraper.Services;

public class WebScraper
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://www.mossspa.co.nz/christchurch";
    
    public WebScraper()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "MossSpaDataImporter/1.0");
    }

    public async Task<List<ScrapedService>> ScrapeAllServicesAsync()
    {
        var categories = new[] { "facials", "massage", "body", "packages", "nails-brows-wax" };
        var allServices = new List<ScrapedService>();

        foreach (var category in categories)
        {
            Console.WriteLine($"Scraping category: {category}...");
            
            try
            {
                var services = await ScrapeCategoryAsync(category);
                allServices.AddRange(services);
                
                // Be respectful - wait 1 second between requests
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scraping {category}: {ex.Message}");
                File.AppendAllText("errors.log", $"{DateTime.Now}: Error in {category} - {ex.Message}\n");
            }
        }

        return allServices;
    }

    private async Task<List<ScrapedService>> ScrapeCategoryAsync(string category)
    {
        var url = $"{_baseUrl}/{category}/";
        var services = new List<ScrapedService>();
        
        var html = await FetchWithRetryAsync(url);
        if (string.IsNullOrEmpty(html))
        {
            return services;
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Find all service containers
        var serviceNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'service')]") 
                          ?? doc.DocumentNode.SelectNodes("//div[contains(@class, 'treatment')]")
                          ?? doc.DocumentNode.SelectNodes("//div[contains(@class, 'item')]")
                          ?? new HtmlNodeCollection(null);

        // Fallback: Look for any section with a heading and price pattern
        if (serviceNodes.Count == 0)
        {
            serviceNodes = doc.DocumentNode.SelectNodes("//div[.//h3 and .//text()[contains(., '$')]]")
                          ?? doc.DocumentNode.SelectNodes("//div[.//h4 and .//text()[contains(., '$')]]")
                          ?? new HtmlNodeCollection(null);
        }

        foreach (var node in serviceNodes)
        {
            try
            {
                var service = ParseServiceNode(node, category);
                if (service != null && !string.IsNullOrEmpty(service.Name))
                {
                    services.Add(service);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error parsing service node: {ex.Message}");
            }
        }

        // If we didn't find structured data, try parsing the text more directly
        if (services.Count == 0)
        {
            Console.WriteLine($"  No structured services found for {category}, attempting text extraction...");
            services = ParseTextBasedServices(doc.DocumentNode.InnerText, category);
        }

        Console.WriteLine($"  Found {services.Count} services in {category}");
        return services;
    }

    private ScrapedService? ParseServiceNode(HtmlNode node, string category)
    {
        // Try to find name
        var nameNode = node.SelectSingleNode(".//h3") 
                      ?? node.SelectSingleNode(".//h4") 
                      ?? node.SelectSingleNode(".//h2");
        
        if (nameNode == null) return null;

        var name = CleanText(nameNode.InnerText);
        if (string.IsNullOrWhiteSpace(name)) return null;

        // Try to find description
        var descNode = node.SelectSingleNode(".//p") 
                      ?? node.SelectSingleNode(".//div[@class='description']");
        var description = descNode != null ? CleanText(descNode.InnerText) : "";

        // Extract all text to find prices and durations
        var allText = node.InnerText;
        var variants = ExtractVariants(allText);

        return new ScrapedService
        {
            Name = name,
            Category = FormatCategory(category),
            Description = description,
            Variants = variants
        };
    }

    private List<ScrapedService> ParseTextBasedServices(string text, string category)
    {
        var services = new List<ScrapedService>();
        
        // Pattern: Look for lines with service name followed by duration and price
        // Example: "CoreStone Massage 60 minutes $180"
        // Example: "Full Body Harmony Massage 60 minutes $165 | 90 minutes $230"
        
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var line in lines)
        {
            // Skip if line doesn't contain a price
            if (!line.Contains('$')) continue;
            
            // Look for pattern with price
            var match = Regex.Match(line, @"([A-Z][A-Za-z\s&,-]+?)\s+(\d+)\s*(?:minutes?|mins?)\s+\$(\d+)", RegexOptions.IgnoreCase);
            
            if (match.Success)
            {
                var name = CleanText(match.Groups[1].Value);
                var duration = int.Parse(match.Groups[2].Value);
                var price = decimal.Parse(match.Groups[3].Value);
                
                var service = new ScrapedService
                {
                    Name = name,
                    Category = FormatCategory(category),
                    Description = "",
                    Variants = new List<ServiceVariant>
                    {
                        new ServiceVariant { DurationMinutes = duration, Price = price }
                    }
                };
                
                // Check for additional variants in the same line
                var additionalMatches = Regex.Matches(line, @"(\d+)\s*(?:minutes?|mins?)\s+\$(\d+)");
                if (additionalMatches.Count > 1)
                {
                    service.Variants.Clear();
                    foreach (Match m in additionalMatches)
                    {
                        service.Variants.Add(new ServiceVariant
                        {
                            DurationMinutes = int.Parse(m.Groups[1].Value),
                            Price = decimal.Parse(m.Groups[2].Value)
                        });
                    }
                }
                
                services.Add(service);
            }
        }
        
        return services;
    }

    private List<ServiceVariant> ExtractVariants(string text)
    {
        var variants = new List<ServiceVariant>();
        
        // Pattern: "60 minutes $180" or "60min $180" or "60 minutes for $180"
        var matches = Regex.Matches(text, @"(\d+)\s*(?:minutes?|mins?)\s*(?:for)?\s*\$(\d+)", RegexOptions.IgnoreCase);
        
        foreach (Match match in matches)
        {
            if (int.TryParse(match.Groups[1].Value, out var duration) &&
                decimal.TryParse(match.Groups[2].Value, out var price))
            {
                variants.Add(new ServiceVariant
                {
                    DurationMinutes = duration,
                    Price = price
                });
            }
        }

        // If no variants found, try just finding a price
        if (variants.Count == 0)
        {
            var priceMatch = Regex.Match(text, @"\$(\d+)");
            if (priceMatch.Success && decimal.TryParse(priceMatch.Groups[1].Value, out var price))
            {
                // Default to 60 minutes if duration not specified
                variants.Add(new ServiceVariant
                {
                    DurationMinutes = 60,
                    Price = price
                });
            }
        }

        return variants;
    }

    private async Task<string> FetchWithRetryAsync(string url, int maxRetries = 3)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                if (i == maxRetries - 1) throw;
                Console.WriteLine($"  Retry {i + 1}/{maxRetries} for {url}: {ex.Message}");
                await Task.Delay(2000 * (i + 1)); // Exponential backoff
            }
        }
        return string.Empty;
    }

    private string CleanText(string text)
    {
        return Regex.Replace(text, @"\s+", " ").Trim();
    }

    private string FormatCategory(string category)
    {
        return category switch
        {
            "facials" => "Facial",
            "massage" => "Massage",
            "body" => "Body Treatment",
            "packages" => "Package",
            "nails-brows-wax" => "Nails/Brows/Wax",
            _ => category
        };
    }
}

