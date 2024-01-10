namespace AKI.TelegramBot.Hosting
{
    public class TelegramConfiguration
    {
        public const string ConfigurationName = nameof(TelegramConfiguration);
        public string BotToken { get; set; }
        public bool SkipSslValidation { get; set; }
        public int MessageWorkers { get; set; }
    }
}
