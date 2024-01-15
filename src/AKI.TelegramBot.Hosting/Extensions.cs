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
    //public static IHostBuilder AddTelegramBot(this IHostBuilder hostBuilder, Action<TelegramConfiguration> telegramBotClientOptionsSetup, Action<ITelegramServiceBuilder> telegramBuilder)
    //{
    //    return AddTelegramBot(hostBuilder, config =>
    //    {
    //        var telegramConfiguration = config.GetSection(TelegramConfiguration.ConfigurationName).Get<TelegramConfiguration>();
    //        telegramBotClientOptionsSetup?.Invoke(telegramConfiguration);
    //        return telegramConfiguration;
    //    }, telegramBuilder);
    //}
    //public static IHostBuilder AddTelegramBot(this IHostBuilder hostBuilder, Func<IConfiguration, TelegramConfiguration> telegramBotClientOptionsSetup, Action<ITelegramServiceBuilder> telegramBuilder)
    //{
    //    hostBuilder.ConfigureServices((context, services) =>
    //    {
    //        var telegramConfiguration = telegramBotClientOptionsSetup?.Invoke(context.Configuration)
    //        ?? context.Configuration.GetSection(TelegramConfiguration.ConfigurationName).Get<TelegramConfiguration>();
    //        var telegramServiceBuilder = services.AddTelegramBot(telegramConfiguration);
    //        telegramBuilder.Invoke(telegramServiceBuilder);
    //    });
    //    return hostBuilder;
    //}

    public static ITelegramServiceBuilder AddTelegramBot(this IServiceCollection services, Action<TelegramConfiguration> telegramBotClientOptionsSetup)
    {
        var options = new TelegramConfiguration
        {
            MessageWorkers = 1
        };
        telegramBotClientOptionsSetup?.Invoke(options);
        return AddTelegramBot(services, options);
    }
    public static ITelegramServiceBuilder AddTelegramBot(this IServiceCollection services, string botToken)
    {
        var options = new TelegramConfiguration
        {
            MessageWorkers = 1,
            BotToken = botToken
        };
        return AddTelegramBot(services, options);
    }

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
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
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

        return new TelegramServiceBuilder(services);
    }

    private static void ValidateConfiguration(TelegramConfiguration telegramBotClientOptions)
    {
        ArgumentNullException.ThrowIfNull(telegramBotClientOptions);

        telegramBotClientOptions.MessageWorkers = Math.Max(1, telegramBotClientOptions.MessageWorkers);
    }
}
