namespace AKI.TelegramBot.Hosting.Abstract
{
    public interface IRouteResolver
    {
        TelegramHandlerBase Resolve(string key);
        TelegramHandlerBase ResolveOrDefault(string key);
    }
}