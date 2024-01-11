# AKI.TelegramBot.ClientUtils

AKI.TelegramBot.ClientUtils is a NuGet package providing a set of extension methods for the ITelegramBotClient interface from the Telegram.Bot library.

## Features

- Send typing actions until a certain task is completed
- Send messages formatted with Markdown
- Handle API request exceptions with retries
- Stream messages from an asynchronous stream
- Send multiple messages with optional reply markup and specific parse mode

## Usage

Add the NuGet package to your project and use the extension methods provided on your ITelegramBotClient objects.

### Examples

```csharp
// Send a typing action until a task is completed
await botClient.TypeWhileWait(chatId, task, cancellationToken);

// Send a markdown formatted message
await botClient.SendMarkDownMessages(chatId, text, cancellationToken);

// Stream messages from an asynchronous stream
await botClient.StreamMessages(chatId, enumerator, cancellationToken);

// Send multiple messages with optional reply markup and specific parse mode
await botClient.SendMessages(chatId, text, ParseMode.Markdown, cancellationToken);
```

## Contribution

Contributions are welcome. Please open an issue if you find a bug or have a feature request.

## License

Licensed under the [Apache License 2.0](http://www.apache.org/licenses/LICENSE-2.0).
