using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace AKI.TelegramBot.Hosting.Services
{
    internal class MessageChannelService
    {
        private readonly Channel<Message> _channel;

        public MessageChannelService(TelegramConfiguration telegramConfiguration)
        {
            _channel = Channel.CreateBounded<Message>(new BoundedChannelOptions(telegramConfiguration.MessageWorkers)
            {
                SingleWriter = true,
                AllowSynchronousContinuations = true,
            });
        }

        public async Task Produce(Update update, CancellationToken cancellationToken)
        {
            await _channel.Writer.WriteAsync(new Message(update, cancellationToken), cancellationToken);
        }

        public async ValueTask<(Update update, CancellationToken cancellationToken)> Consume(CancellationToken cancellationToken)
        {
            return await _channel.Reader.ReadAsync(cancellationToken);
        }

        private class Message
        {
            public Message(Update telegramUpdate, CancellationToken cancellationToken)
            {
                TelegramUpdate = telegramUpdate;
                CancellationToken = cancellationToken;
            }
            public Update TelegramUpdate { get; }
            public CancellationToken CancellationToken { get; }

            public static implicit operator (Update telegramUpdate, CancellationToken cancellationToken)(Message message)
                => (message.TelegramUpdate, message.CancellationToken);
        }
    }
}
