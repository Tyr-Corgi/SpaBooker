namespace SpaBooker.Core.Interfaces;

/// <summary>
/// Interface for entities that support soft delete
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Indicates if the entity is deleted
    /// </summary>
    bool IsDeleted { get; set; }
    
    /// <summary>
    /// When the entity was deleted
    /// </summary>
    DateTime? DeletedAt { get; set; }
    
    /// <summary>
    /// User who deleted the entity
    /// </summary>
    string? DeletedBy { get; set; }
}

