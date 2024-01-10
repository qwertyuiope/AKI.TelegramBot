namespace AKI.TelegramBot.Hosting
{
    public static class TelegramHelper
    {
        public static string ParseCommandKey(string text)
            => CreateCommandKey(text?.Split(' ')?[0] ?? string.Empty);
        public static string CreateCommandKey<T>()
            => CreateCommandKey(typeof(T).FullName);
        private static string CreateCommandKey(string key)
            => Facts.CommandMessage.CommandPrefix + key;
    }
}
