using FoodHub.Data;
using Microsoft.EntityFrameworkCore;

public class SpecialsStatusService : IHostedService, IDisposable
{
    private Timer _timer;
    private readonly IServiceScopeFactory _scopeFactory;

    public SpecialsStatusService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Every minute (production). For testing, can be every 5 sec
        _timer = new Timer(UpdateSpecialsStatus, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        return Task.CompletedTask;
    }

    private void UpdateSpecialsStatus(object state)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FoodHubContext>();
        var now = DateTime.Now;

        var specials = context.Specials.ToList();
        bool changed = false;

        foreach (var special in specials)
        {
            var newStatus =
        special.StartDate.HasValue &&
        special.EndDate.HasValue &&
        special.StartDate.Value.Date <= now.Date &&
        special.EndDate.Value.Date >= now.Date;

            if (special.IsActive != newStatus)
            {
                special.IsActive = newStatus;
                changed = true;
            }
        }

        if (changed)
            context.SaveChanges();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public void Dispose() => _timer?.Dispose();
}
