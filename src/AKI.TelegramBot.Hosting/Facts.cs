using System;

namespace AKI.TelegramBot.Hosting
{
    internal static class Facts
    {
        public static class CallbackQuery
        {
            public const string CallbackPrefix = "callback:";
        }
        public static class CommandMessage
        {
            public const string CommandPrefix = "message:";
        }
        internal static readonly string DefaultServiceKey = Guid.NewGuid().ToString();
    }
}
