using Microsoft.AspNetCore.Components.Server.Circuits;

namespace SpaBooker.Web.Handlers;

/// <summary>
/// Circuit handler for cleaning up resources when a Blazor Server circuit is terminated
/// </summary>
public class CleanupCircuitHandler : CircuitHandler
{
    private readonly ILogger<CleanupCircuitHandler> _logger;

    public CleanupCircuitHandler(ILogger<CleanupCircuitHandler> logger)
    {
        _logger = logger;
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Circuit {CircuitId} connected", circuit.Id);
        return Task.CompletedTask;
    }

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Circuit {CircuitId} disconnected", circuit.Id);
        return Task.CompletedTask;
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Circuit {CircuitId} opened", circuit.Id);
        return Task.CompletedTask;
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Circuit {CircuitId} closed", circuit.Id);
        
        // Perform any cleanup operations here
        // For example: cancel long-running operations, dispose resources, etc.
        
        return Task.CompletedTask;
    }
}

