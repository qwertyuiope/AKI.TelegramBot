using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Requests;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;

namespace AKI.TelegramBot.UnitTests.Mocks
{
    internal class TelegramBotClientMock : ITelegramBotClient
    {
        public bool LocalBotServer { get; set; }

        public long? BotId { get; set; }

        public TimeSpan Timeout { get; set; }
        public IExceptionParser ExceptionsParser { get; set; }

        public event AsyncEventHandler<ApiRequestEventArgs> OnMakingApiRequest;
        public event AsyncEventHandler<ApiResponseEventArgs> OnApiResponseReceived;

        public Task DownloadFileAsync(string filePath, Stream destination, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<TResponse> MakeRequestAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {

            if (typeof(TResponse) == typeof(Message) && request is SendMessageRequest messageRequest)
            {
                return Task.FromResult((TResponse)(object)new Message
                {
                    Text = messageRequest.Text
                });
            }
            return Task.FromResult(default(TResponse));
        }

        public Task<bool> TestApiAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);
    }
}
