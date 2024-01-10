using Telegram.Bot.Types.ReplyMarkups;

namespace AKI.TelegramBot.Hosting.Abstract
{
    public interface ICallbackKeyHandler
    {
        InlineKeyboardButton[] BigButtonWithCallback<T>(string text, string value = null) where T : TelegramHandlerBase;
        InlineKeyboardButton ButtonWithCallback<T>(string text, string value = null) where T : TelegramHandlerBase;
        internal (string key, string value) ParseInlineCallbackKey(string id);

    }
}