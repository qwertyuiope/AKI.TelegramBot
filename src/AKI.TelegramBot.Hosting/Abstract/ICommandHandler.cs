using System.Threading;
using System.Threading.Tasks;

namespace AKI.TelegramBot.Hosting.Abstract;
public interface ICommandHandler<T>
{
    Task Handle(T message, CancellationToken cancellationToken);
}
