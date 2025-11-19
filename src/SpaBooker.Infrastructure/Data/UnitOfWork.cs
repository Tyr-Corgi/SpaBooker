using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using SpaBooker.Core.Interfaces;

namespace SpaBooker.Infrastructure.Data;

/// <summary>
/// Unit of Work implementation for managing database transactions
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;
    private IDbContextTransaction? _currentTransaction;

    public UnitOfWork(ApplicationDbContext context, ILogger<UnitOfWork> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress");
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        _logger.LogDebug("Transaction {TransactionId} started", _currentTransaction.TransactionId);
        
        return _currentTransaction;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No active transaction to commit");
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
            _logger.LogDebug("Transaction {TransactionId} committed successfully", _currentTransaction.TransactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error committing transaction {TransactionId}", _currentTransaction.TransactionId);
            await RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            return; // Nothing to rollback
        }

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
            _logger.LogWarning("Transaction {TransactionId} rolled back", _currentTransaction.TransactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rolling back transaction {TransactionId}", _currentTransaction.TransactionId);
            throw;
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes to database");
            throw;
        }
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context?.Dispose();
    }
}

