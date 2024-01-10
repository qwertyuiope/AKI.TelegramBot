using System.Threading.Tasks;

namespace AKI.TelegramBot.Hosting.Abstract
{
    public interface IBotSetup
    {
        Task OnStartAsync();
        Task OnStopAsync();
    }
}
