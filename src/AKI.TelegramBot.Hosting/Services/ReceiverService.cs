using AKI.TelegramBot.Hosting.Abstract;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace AKI.TelegramBot.Hosting.Services;

/// <summary>
/// An abstract class to compose Receiver Service and Update Handler classes
/// </summary>
/// <typeparam name="TUpdateHandler">Update Handler to use in Update Receiver</typeparam>
internal class ReceiverService : IReceiverService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<ReceiverService> _logger;
    private readonly MessageChannelService _channelService;

    public ReceiverService(ITelegramBotClient botClient, ILogger<ReceiverService> logger,
        MessageChannelService channelService)
    {
        _botClient = botClient;
        _logger = logger;
        _channelService = channelService;
    }

    /// <summary>
    /// Start to service Updates with provided Update Handler class
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    public async Task ReceiveAsync(CancellationToken stoppingToken)
    {
        // ToDo: we can inject ReceiverOptions through IOptions container
        var receiverOptions = new ReceiverOptions()
        {
            AllowedUpdates = Array.Empty<UpdateType>(),
            ThrowPendingUpdates = true,
        };

        var me = await _botClient.GetMeAsync(stoppingToken);
        _logger.LogInformation("Start receiving updates for {BotName}", me.Username ?? "My Awesome Bot");

        // Start receiving updates
        await _botClient.ReceiveAsync(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken);
    }

    private async Task HandlePollingErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);

        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }

    private Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        => _channelService.Produce(update, cancellationToken);
}
