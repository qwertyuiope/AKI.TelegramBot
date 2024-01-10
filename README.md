# AKI.TelegramBot

This repository contains a Telegram Bot framework implemented in C# using the Telegram.Bot library. The framework aims to provide developers with a convenient and extensible solution for building Telegram Bots, similar to how Microsoft provides a framework for web APIs.

## Project Goals
- **Modularity**: Build Telegram Bots with modular components, allowing easy customization and extension.
- **Abstraction**: Provide an abstraction layer for handling commands, callbacks, and middleware, simplifying bot development.
- **Integration**: Seamlessly integrate with the Telegram Bot API for sending and receiving messages, handling updates, and more.
- **Scalability**: Design the framework to support high-traffic bots and enable easy deployment to various hosting platforms.

- ## Features
- Command handling: Easily define and handle commands with customizable logic.
- Callback handling: Handle callback queries from inline keyboards and other interactive elements.
- Middleware support: Implement custom middleware for request processing and pipeline management.
- Integration with Telegram Bot API: Interact with the Telegram Bot API for sending messages, managing chats, and more.

## Usage Example

```csharp
class Program
{
    static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddTelegramBot(config =>
                {
                    config.BotToken = "YOUR_BOT_TOKEN";
                    // Add other configuration options if needed
                })
                .AddMessageHandler<HelpCommandHandler>("/help")
                .SetDefaultMessageHandler<DefaultMessageHandler>();
            })
            .Build();

        host.Run();
    }
}
```

In this example, we create a new `Host` using `Host.CreateDefaultBuilder` and configure the services using the `ConfigureServices` method. Within the service configuration, we use the `AddTelegramBot` method to register the Telegram Bot and provide the necessary configuration, such as the bot token. We then register our command handlers using the `AddMessageHandler` method, specifying the command and the corresponding handler class. Finally, we set the default message handler using the `SetDefaultMessageHandler` method.

Please note that you need to replace `"YOUR_BOT_TOKEN"` with your actual bot token obtained from the BotFather on Telegram.

## Getting Started
1. Clone the repository
2. Build and run the project using a C# development environment
3. Customize the bot's functionality by adding handlers, middleware, and modules
4. Deploy the bot to a server or cloud platform for hosting

## Contributing
Contributions are welcome! If you find a bug or have a feature request, please open an issue. Pull requests are also appreciated.

## License
This project is licensed under the [Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0). See the LICENSE file for more details.

## Acknowledgements
- [Telegram.Bot](https://github.com/TelegramBots/Telegram.Bot) library for Telegram Bot API integration
