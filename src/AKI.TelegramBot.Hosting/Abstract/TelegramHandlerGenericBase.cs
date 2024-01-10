using System.Threading;
using System.Threading.Tasks;

namespace AKI.TelegramBot.Hosting.Abstract
{
    public abstract class TelegramHandlerGenericBase<TContext, T> : ICommandHandler<TContext>
        where TContext : TelegramContextBase<T> where T : class
    {
        public abstract Task Handle(TContext message, CancellationToken cancellationToken);
    }
}
