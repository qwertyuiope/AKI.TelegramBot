using AKI.TelegramBot.Hosting.Models;
using Telegram.Bot.Types;

namespace AKI.TelegramBot.Hosting.Abstract
{
    public abstract class TelegramHandlerBase : TelegramHandlerGenericBase<TelegramContext, Update>
    {
    }
}
