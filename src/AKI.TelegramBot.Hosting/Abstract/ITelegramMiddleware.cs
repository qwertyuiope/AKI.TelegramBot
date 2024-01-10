using AKI.TelegramBot.Hosting.Models;
using System.Threading.Tasks;

namespace AKI.TelegramBot.Hosting.Abstract
{
    public interface ITelegramMiddleware
    {
        Task RunAsync(NextAction next, TelegramContext telegramContext);
    }
    public delegate Task NextAction();
}
