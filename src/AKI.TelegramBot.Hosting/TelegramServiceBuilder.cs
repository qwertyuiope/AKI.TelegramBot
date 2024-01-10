using AKI.TelegramBot.Hosting.Abstract;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace AKI.TelegramBot.Hosting
{
    public interface ITelegramServiceBuilder
    {
        ITelegramServiceBuilder AddMessageHandler<TMessageHandler>() where TMessageHandler : TelegramHandlerBase;
        ITelegramServiceBuilder AddMessageHandler<TMessageHandler>(string name) where TMessageHandler : TelegramHandlerBase;
        ITelegramServiceBuilder SetDefaultMessageHandler<TMessageHandler>() where TMessageHandler : TelegramHandlerBase;
        ITelegramServiceBuilder AddCallbackHandler<TMessageHandler>() where TMessageHandler : TelegramHandlerBase;
        ITelegramServiceBuilder BotSetup<TBotSetup>() where TBotSetup : class, IBotSetup;
        //ITelegramServiceBuilder AddRouteResolver<TMessageResolver>() where TMessageResolver : class, IRouteResolverMarker;
        ITelegramServiceBuilder AddMiddleware<TContextAdapter>() where TContextAdapter : class, ITelegramMiddleware;
        ITelegramServiceBuilder AddMessageWithCallbackHandler<TMessageHandler>(string name) where TMessageHandler : TelegramHandlerBase;
        //ITelegramServiceBuilder AddAllFromAssembly<T>();
    }
    internal class TelegramServiceBuilder : ServiceCollectionWrapper, ITelegramServiceBuilder
    {
        internal TelegramServiceBuilder(IServiceCollection services)
            : base(services)
        {
        }

        public ITelegramServiceBuilder AddMessageHandler<TMessageHandler>() where TMessageHandler : TelegramHandlerBase
        {
            _serviceDescriptors.AddKeyedTransient<TelegramHandlerBase, TMessageHandler>(TelegramHelper.CreateCommandKey<TMessageHandler>());
            return this;
        }
        public ITelegramServiceBuilder AddMessageHandler<TMessageHandler>(string name) where TMessageHandler : TelegramHandlerBase
        {
            _serviceDescriptors.AddKeyedTransient<TelegramHandlerBase, TMessageHandler>(TelegramHelper.ParseCommandKey(name));
            return this;
        }
        public ITelegramServiceBuilder AddMessageWithCallbackHandler<TMessageHandler>(string name) where TMessageHandler : TelegramHandlerBase
        {
            return AddMessageHandler<TMessageHandler>(name)
                .AddCallbackHandler<TMessageHandler>();
        }
        public ITelegramServiceBuilder AddCallbackHandler<TMessageHandler>() where TMessageHandler : TelegramHandlerBase
        {
            var name = typeof(TMessageHandler).Name;
            _serviceDescriptors.AddKeyedTransient<TelegramHandlerBase, TMessageHandler>(Facts.CallbackQuery.CallbackPrefix + name);
            return this;
        }
        public ITelegramServiceBuilder SetDefaultMessageHandler<TMessageHandler>() where TMessageHandler : TelegramHandlerBase
        {
            _serviceDescriptors.AddKeyedSingleton<TelegramHandlerBase, TMessageHandler>(Facts.DefaultServiceKey);
            return this;
        }
        public ITelegramServiceBuilder BotSetup<TBotSetup>() where TBotSetup : class, IBotSetup
        {
            this.AddSingleton<IBotSetup, TBotSetup>();
            return this;
        }
        //public ITelegramServiceBuilder AddRouteResolver<TMessageResolver>() where TMessageResolver : class, IRouteResolverMarker
        //{
        //    this.AddSingleton<IRouteResolverMarker, TMessageResolver>();
        //    return this;
        //}
        public ITelegramServiceBuilder AddMiddleware<TContextAdapter>() where TContextAdapter : class, ITelegramMiddleware
        {
            this.AddSingleton<ITelegramMiddleware, TContextAdapter>();
            return this;
        }

        public ITelegramServiceBuilder AddAllFromAssembly<T>()
        {
            //TODO
            var assembly = typeof(T).Assembly;
            var types = assembly.GetTypes()
                .Where(a => a.GetInterfaces().Any(i => i == typeof(TelegramHandlerBase)));

            foreach (var item in types)
            {
                object serviceKey = null;
                _serviceDescriptors.Add(new ServiceDescriptor(typeof(TelegramHandlerBase), serviceKey, item, ServiceLifetime.Transient));
            }

            return this;
        }
    }
}
