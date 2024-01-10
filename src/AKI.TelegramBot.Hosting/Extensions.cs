using AKI.TelegramBot.Hosting.Abstract;
using AKI.TelegramBot.Hosting.Middlewares;
using AKI.TelegramBot.Hosting.Services;
using AKI.TelegramBot.Hosting.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;
using Telegram.Bot;

namespace AKI.TelegramBot.Hosting;
public static class Extensions
{
    public static ITelegramServiceBuilder AddTelegramBot(this IServiceCollection services,
       TelegramConfiguration telegramBotClientOptions)
    {
        ValidateConfiguration(telegramBotClientOptions);

        var httpClientBuilder = services.AddHttpClient("telegram_bot_client")
            .AddTypedClient<ITelegramBotClient>((httpClient, sp) => new TelegramBotClient(telegramBotClientOptions.BotToken, httpClient));

        if (telegramBotClientOptions.SkipSslValidation)
        {
            httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
                }
           );
        }

        for (var i = 0; i < telegramBotClientOptions.MessageWorkers; i++)
            services.AddSingleton<IHostedService, MessageWorkers>();

        services.AddScoped<ReceiverService>()
            .AddSingleton<EndpointMiddleware>()
            .AddSingleton<ICallbackKeyHandler, CallbackKeyHandler>()
            .AddSingleton<IRouteResolver, RouteResolver>()
            .AddSingleton<MessageHandlerService>()
            .AddSingleton(new MessageChannelService(telegramBotClientOptions));

        services.AddHostedService<TelegramPollingWorker>();

        //var scopedServicesContainer = services.AddNamedContainer<TelegramHandlerBase>();

        return new TelegramServiceBuilder(services);
    }

    private static void ValidateConfiguration(TelegramConfiguration telegramBotClientOptions)
    {
        telegramBotClientOptions.MessageWorkers = Math.Max(1, telegramBotClientOptions.MessageWorkers);
    }
}
