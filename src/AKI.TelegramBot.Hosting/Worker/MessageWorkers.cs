using AKI.TelegramBot.Hosting.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace AKI.TelegramBot.Hosting.Worker
{
    internal class MessageWorkers : BackgroundService
    {
        private readonly MessageChannelService _messageBus;
        private readonly MessageHandlerService _messageHandler;
        private readonly ILogger<MessageWorkers> _logger;

        public MessageWorkers(MessageChannelService messageBus, MessageHandlerService messageHandler, ILogger<MessageWorkers> logger)
        {
            _messageBus = messageBus;
            _messageHandler = messageHandler;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var (update, cancellationToken) = await _messageBus.Consume(stoppingToken);

                    _logger.LogInformation("Receive message type: {UpdateType}", update.Type);

                    var linkedCancelationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, stoppingToken).Token;

                    var handler = update.Type switch
                    {
                        // UpdateType.Unknown:
                        // UpdateType.ChannelPost:
                        // UpdateType.EditedChannelPost:
                        // UpdateType.ShippingQuery:
                        // UpdateType.PreCheckoutQuery:
                        // UpdateType.Poll:
                        //UpdateType.InlineQuery => BotOnInlineQueryReceived(update.InlineQuery, cancellationToken),
                        //UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(update.ChosenInlineResult, cancellationToken),
                        UpdateType.Message => BotOnMessageReceived(update, linkedCancelationToken),
                        UpdateType.EditedMessage => BotOnMessageReceived(update, linkedCancelationToken),
                        UpdateType.CallbackQuery => BotOnCallbackQueryReceived(update, linkedCancelationToken),
                        _ => UnknownUpdateHandlerAsync(update, linkedCancelationToken)
                    };

                    await handler;
                }
                catch (System.Exception ex)
                {

                    _logger.LogError("Something went wrong while proccessing message", ex);
                }
            }
        }

        private async Task BotOnMessageReceived(Update update, CancellationToken cancellationToken)
        {
            _ = await _messageHandler.RunAsync(update, cancellationToken);
        }

        private async Task BotOnCallbackQueryReceived(Update update, CancellationToken cancellationToken)
        {
            _ = await _messageHandler.RunAsync(update, cancellationToken);
        }
        private Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
        {
            _logger.LogError("Unknown update type: {0}", update.Type);
            return Task.CompletedTask;
        }
    }
}
