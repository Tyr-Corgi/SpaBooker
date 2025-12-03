using SpaBooker.Core.Common;
using SpaBooker.Core.DTOs.Client;
using SpaBooker.Core.Entities;

namespace SpaBooker.Core.Interfaces;

public interface IClientService
{
    /// <summary>
    /// Gets all clients with optional filtering
    /// </summary>
    Task<List<ApplicationUser>> GetAllClientsAsync(string? searchQuery = null, bool includeInactive = false);
    
    /// <summary>
    /// Gets a client by ID
    /// </summary>
    Task<Result<ApplicationUser>> GetClientByIdAsync(string clientId);
    
    /// <summary>
    /// Gets comprehensive statistics for a client
    /// </summary>
    Task<Result<ClientStatisticsDto>> GetClientStatisticsAsync(string clientId);
    
    /// <summary>
    /// Gets client booking history
    /// </summary>
    Task<List<Booking>> GetClientBookingHistoryAsync(string clientId, int? take = null);
    
    /// <summary>
    /// Gets client's active membership
    /// </summary>
    Task<UserMembership?> GetClientActiveMembershipAsync(string clientId);
    
    /// <summary>
    /// Gets client notes
    /// </summary>
    Task<List<ClientNote>> GetClientNotesAsync(string clientId);
    
    /// <summary>
    /// Adds a note to a client
    /// </summary>
    Task<Result<ClientNote>> AddClientNoteAsync(string clientId, string content, string noteType, string createdBy);
    
    /// <summary>
    /// Updates client information
    /// </summary>
    Task<Result> UpdateClientAsync(string clientId, string firstName, string lastName, string? phoneNumber);
}

