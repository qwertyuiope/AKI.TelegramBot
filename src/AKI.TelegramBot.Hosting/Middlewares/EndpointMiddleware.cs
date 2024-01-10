using AKI.TelegramBot.Hosting.Abstract;
using AKI.TelegramBot.Hosting.Models;
using System.Threading.Tasks;

namespace AKI.TelegramBot.Hosting.Middlewares
{
    internal class EndpointMiddleware : ITelegramMiddleware
    {
        private readonly IRouteResolver _mainRouteResolver;

        public EndpointMiddleware(IRouteResolver mainRouteResolver)
        {
            _mainRouteResolver = mainRouteResolver;
        }
        public async Task RunAsync(NextAction next, TelegramContext ctx)
        {
            var handler = _mainRouteResolver.ResolveOrDefault(ctx.Route);
            await handler.Handle(ctx, ctx.CancellationToken);
        }
    }
}
