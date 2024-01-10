using AKI.TelegramBot.Hosting.Abstract;
using AKI.TelegramBot.Hosting.Middlewares;
using AKI.TelegramBot.Hosting.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace AKI.TelegramBot.Hosting.Services
{
    internal class MessageHandlerService
    {
        private readonly ICallbackKeyHandler _callbackQueryResolver;
        private readonly ILogger<MessageHandlerService> _logger;
        private readonly IReadOnlyList<ITelegramMiddleware> _middlewares;

        public MessageHandlerService(IEnumerable<ITelegramMiddleware> telegramMiddlewares,
            ICallbackKeyHandler callbackQueryResolver, EndpointMiddleware endpointMiddleware, ILogger<MessageHandlerService> logger)
        {
            var middlewares = telegramMiddlewares?.ToList() ?? new List<ITelegramMiddleware>(1);
            middlewares.Add(endpointMiddleware);
            _middlewares = middlewares;

            _callbackQueryResolver = callbackQueryResolver;
            _logger = logger;
        }
        public async Task<TelegramContext> RunAsync(Update update, CancellationToken cancellationToken)
        {
            var context = GenerateContext(update, cancellationToken);

            var currentCount = 0;
            var requestId = update.Id;

            async Task Run()
            {
                if (_middlewares.Count <= currentCount)
                    return;

                var current = _middlewares[currentCount++];

                _logger.LogDebug("Running {0} for request {1}", current.GetType().Name, requestId);

                await current.RunAsync(Run, context);
            }

            _logger.LogDebug("Starting pipeline for request {0}", requestId);

            await Run();

            _logger.LogDebug("Finished pipeline for request {0}", requestId);

            return context;
        }
        private TelegramContext GenerateContext(Update update, CancellationToken cancellationToken)
        {
            var (route, val) = _callbackQueryResolver.ParseInlineCallbackKey(update.CallbackQuery?.Data);

            route ??= TelegramHelper.ParseCommandKey(update.Message?.Text);

            return new TelegramContext(update, val, update, route, cancellationToken);
        }
    }
}
