using AKI.TelegramBot.Hosting.Abstract;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AKI.TelegramBot.Hosting
{
    internal class RouteResolver : IRouteResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public RouteResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public TelegramHandlerBase ResolveOrDefault(string key)
        {
            return Resolve(key) ?? Resolve(Facts.DefaultServiceKey);
        }

        public TelegramHandlerBase Resolve(string key)
        {
            return _serviceProvider.GetKeyedService<TelegramHandlerBase>(key);
        }
    }
}
