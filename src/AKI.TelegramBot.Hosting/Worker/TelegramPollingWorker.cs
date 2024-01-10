using AKI.TelegramBot.Hosting.Abstract;
using AKI.TelegramBot.Hosting.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AKI.TelegramBot.Hosting.Worker;

// Compose Polling and ReceiverService implementations
internal class TelegramPollingWorker : PollingServiceBase<ReceiverService>
{
    private readonly IBotSetup _botSetup;

    public TelegramPollingWorker(IServiceProvider serviceProvider,
        ILogger<TelegramPollingWorker> logger, IBotSetup botSetup = null) : base(serviceProvider, logger)
    {
        _botSetup = botSetup;
    }
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await _botSetup.OnStartAsync();
        await base.StartAsync(cancellationToken);
    }
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _botSetup.OnStopAsync();
        await base.StopAsync(cancellationToken);
    }
}
