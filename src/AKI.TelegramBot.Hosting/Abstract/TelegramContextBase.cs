using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using Telegram.Bot.Types;

namespace AKI.TelegramBot.Hosting.Abstract
{
    public abstract class TelegramContextBase<T> where T : class
    {
        internal TelegramContextBase(Update telegramUpdate, string callbackData, T context, string route, CancellationToken cancellationToken)
        {
            TelegramUpdate = telegramUpdate;
            CallbackData = callbackData;
            Context = context;
            Route = route;
            CancellationToken = cancellationToken;
            var theMessage = telegramUpdate.Message ?? telegramUpdate.CallbackQuery.Message;
            var from = telegramUpdate.Message?.From ?? telegramUpdate.CallbackQuery.From;
            ChatId = theMessage.Chat.Id;
            RequestId = telegramUpdate.Id;
            CallbackId = telegramUpdate.CallbackQuery?.Id;
            TraceId = Guid.NewGuid().ToString();

            User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier,from.Id.ToString()),
                new Claim(ClaimTypes.Name,from.Username),
            }));

            Items = new Dictionary<object, object>();
        }

        public Update TelegramUpdate { get; }
        public string CallbackData { get; }
        public string Route { get; }
        public CancellationToken CancellationToken { get; }
        public T Context { get; }
        public long ChatId { get; }
        public int RequestId { get; }
        public string CallbackId { get; }
        public string TraceId { get; }
        public ClaimsPrincipal User { get; set; }
        public IDictionary<object, object> Items { get; set; }
    }
}
