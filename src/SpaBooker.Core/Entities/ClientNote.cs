namespace SpaBooker.Core.Entities;

public class ClientNote
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ClientId { get; set; } = "";
    public string Content { get; set; } = "";
    public string NoteType { get; set; } = ""; // Staff, Therapist, General
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedByName { get; set; }
    
    // Navigation property
    public ApplicationUser? Client { get; set; }
}

