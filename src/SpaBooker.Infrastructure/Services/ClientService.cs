using Microsoft.EntityFrameworkCore;
using SpaBooker.Core.Common;
using SpaBooker.Core.DTOs.Client;
using SpaBooker.Core.Entities;
using SpaBooker.Core.Enums;
using SpaBooker.Core.Interfaces;
using SpaBooker.Infrastructure.Data;

namespace SpaBooker.Infrastructure.Services;

public class ClientService : IClientService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public ClientService(
        ApplicationDbContext context,
        IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<List<ApplicationUser>> GetAllClientsAsync(string? searchQuery = null, bool includeInactive = false)
    {
        var query = _context.Users
            .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id &&
                _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Client")));

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            query = query.Where(u =>
                u.FirstName.Contains(searchQuery) ||
                u.LastName.Contains(searchQuery) ||
                (u.Email != null && u.Email.Contains(searchQuery)) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(searchQuery)));
        }

        return await query
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync();
    }

    public async Task<Result<ApplicationUser>> GetClientByIdAsync(string clientId)
    {
        var client = await _context.Users.FindAsync(clientId);

        if (client == null)
        {
            return Result.Failure<ApplicationUser>(Error.ClientNotFound);
        }

        return Result.Success(client);
    }

    public async Task<Result<ClientStatisticsDto>> GetClientStatisticsAsync(string clientId)
    {
        var clientExists = await _context.Users.AnyAsync(u => u.Id == clientId);
        if (!clientExists)
        {
            return Result.Failure<ClientStatisticsDto>(Error.ClientNotFound);
        }

        var bookings = await _context.Bookings
            .Include(b => b.Service)
            .Include(b => b.Therapist)
            .Where(b => b.ClientId == clientId)
            .ToListAsync();

        var membership = await GetClientActiveMembershipAsync(clientId);

        var completedBookings = bookings.Where(b => b.Status == BookingStatus.Completed).ToList();
        var cancelledBookings = bookings.Where(b => b.Status == BookingStatus.Cancelled).ToList();

        var lastBooking = bookings
            .Where(b => b.Status == BookingStatus.Completed)
            .OrderByDescending(b => b.StartTime)
            .FirstOrDefault();

        var nextBooking = bookings
            .Where(b => b.Status == BookingStatus.Confirmed && b.StartTime > DateTime.UtcNow)
            .OrderBy(b => b.StartTime)
            .FirstOrDefault();

        var favoriteService = completedBookings
            .GroupBy(b => b.Service.Name)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()
            ?.Key;

        var favoriteTherapist = completedBookings
            .Where(b => b.Therapist != null)
            .GroupBy(b => $"{b.Therapist!.FirstName} {b.Therapist.LastName}")
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()
            ?.Key;

        var daysSinceLastVisit = lastBooking != null
            ? (DateTime.UtcNow - lastBooking.StartTime).Days
            : 0;

        var statistics = new ClientStatisticsDto
        {
            TotalBookings = bookings.Count,
            CompletedBookings = completedBookings.Count,
            CancelledBookings = cancelledBookings.Count,
            LifetimeValue = completedBookings.Sum(b => b.TotalPrice),
            LastBookingDate = lastBooking?.StartTime,
            NextBookingDate = nextBooking?.StartTime,
            DaysSinceLastVisit = daysSinceLastVisit,
            HasActiveMembership = membership != null && membership.Status == MembershipStatus.Active,
            MembershipPlanName = membership?.MembershipPlan?.Name ?? string.Empty,
            CurrentCredits = membership?.CurrentCredits ?? 0,
            FavoriteService = favoriteService,
            FavoriteTherapist = favoriteTherapist
        };

        return Result.Success(statistics);
    }

    public async Task<List<Booking>> GetClientBookingHistoryAsync(string clientId, int? take = null)
    {
        var query = _context.Bookings
            .Include(b => b.Service)
            .Include(b => b.Therapist)
            .Include(b => b.Room)
            .Where(b => b.ClientId == clientId)
            .OrderByDescending(b => b.StartTime);

        if (take.HasValue)
        {
            return await query.Take(take.Value).ToListAsync();
        }

        return await query.ToListAsync();
    }

    public async Task<UserMembership?> GetClientActiveMembershipAsync(string clientId)
    {
        return await _context.UserMemberships
            .Include(um => um.MembershipPlan)
            .FirstOrDefaultAsync(um => um.UserId == clientId &&
                                      um.Status == MembershipStatus.Active);
    }

    public async Task<List<ClientNote>> GetClientNotesAsync(string clientId)
    {
        return await _context.ClientNotes
            .Include(cn => cn.Client)
            .Where(cn => cn.ClientId == clientId)
            .OrderByDescending(cn => cn.CreatedAt)
            .ToListAsync();
    }

    public async Task<Result<ClientNote>> AddClientNoteAsync(string clientId, string content, string noteType, string createdBy)
    {
        var clientExists = await _context.Users.AnyAsync(u => u.Id == clientId);
        if (!clientExists)
        {
            return Result.Failure<ClientNote>(Error.ClientNotFound);
        }

        var note = new ClientNote
        {
            ClientId = clientId,
            Content = content,
            NoteType = noteType,
            CreatedByName = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        _context.ClientNotes.Add(note);

        try
        {
            await _context.SaveChangesAsync();
            
            await _auditService.LogAsync(
                "Create",
                "ClientNote",
                note.Id,
                null,
                null,
                $"Note added for client {clientId}"
            );

            return Result.Success(note);
        }
        catch (Exception ex)
        {
            return Result.Failure<ClientNote>(new Error("ClientNote.CreateFailed", ex.Message));
        }
    }

    public async Task<Result> UpdateClientAsync(string clientId, string firstName, string lastName, string? phoneNumber)
    {
        var client = await _context.Users.FindAsync(clientId);

        if (client == null)
        {
            return Result.Failure(Error.ClientNotFound);
        }

        client.FirstName = firstName;
        client.LastName = lastName;
        client.PhoneNumber = phoneNumber;

        try
        {
            await _context.SaveChangesAsync();
            
            await _auditService.LogAsync(
                "Update",
                "User",
                clientId,
                null,
                null,
                "Client information updated"
            );

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Client.UpdateFailed", ex.Message));
        }
    }
}

