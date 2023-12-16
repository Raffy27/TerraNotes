using System.Collections.Concurrent;
using TerraNotes.Data;

public class NoteProcessor : BackgroundService, IDisposable
{
    public static int MAX_RUNNING_TASKS = 5;
    public static int MAX_WAITING_TASKS = 10;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NoteProcessor> _logger;
    private readonly SemaphoreSlim semaphore;
    private readonly NoteTaskQueue waitingTasks;

    public NoteProcessor(ILogger<NoteProcessor> logger, IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        semaphore = new SemaphoreSlim(MAX_RUNNING_TASKS);
        waitingTasks = new NoteTaskQueue(MAX_WAITING_TASKS);
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken);
        _logger.LogInformation("Note processor started");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        _logger.LogInformation("Note processor stopped");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await semaphore.WaitAsync(stoppingToken);
            _logger.LogInformation("Note processor waiting for task");
            var task = await waitingTasks.DequeueAsync(stoppingToken);
            _logger.LogInformation("Note processor starting task");

            var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var rocketVision = scope.ServiceProvider.GetRequiredService<RocketVision>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<NoteTask>>();
            _logger.LogInformation("Note processor got database context");
            // Start the task as a background task
            _ = Task.Run(async () =>
            {
                _logger.LogInformation("Inside of the task that was started asynchronously");
                await task.Run(stoppingToken, context, rocketVision, logger);
                _logger.LogInformation("Note processor finished task");
                scope.Dispose();
                semaphore.Release();
                _logger.LogInformation("Note processor released semaphore and disposed of scope");
            }, stoppingToken);
            _logger.LogInformation("Note processor finished starting task");
        }
    }

    public async Task<bool> TrySubmitTask(NoteTask task)
    {
        return await waitingTasks.EnqueueAsync(task);
    }
}