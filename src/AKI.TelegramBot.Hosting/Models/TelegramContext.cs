using AKI.TelegramBot.Hosting.Abstract;
using System.Threading;
using Telegram.Bot.Types;

namespace AKI.TelegramBot.Hosting.Models
{
    public class TelegramContext : TelegramContextBase<Update>
    {
        public TelegramContext(Update telegramUpdate, string callbackData, Update context, string route, CancellationToken cancellationToken)
            : base(telegramUpdate, callbackData, context, route, cancellationToken)
        {
        }
    }
}
