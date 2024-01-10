using AKI.TelegramBot.Hosting.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;

namespace AKI.TelegramBot.Hosting
{
    internal class CallbackKeyHandler : ICallbackKeyHandler
    {
        private readonly Dictionary<Type, string> _nameIdMap = [];
        private readonly Dictionary<string, Type> _idNameMap = [];
        private readonly object _lock = new();
        private readonly object _reflectionLock = new();
        private const char _callbackInlikeKeySeparator = (char)254;
        private bool started = false;

        public InlineKeyboardButton[] BigButtonWithCallback<T>(string text, string value = null) where T : TelegramHandlerBase
        {
            return [ButtonWithCallback<T>(text, value)];
        }
        public InlineKeyboardButton ButtonWithCallback<T>(string text, string value = null) where T : TelegramHandlerBase
        {
            return InlineKeyboardButton.WithCallbackData(text, GenerateInlineCallbackKey<T>(value));
        }
        public (string key, string value) ParseInlineCallbackKey(string id)
        {
            if (id is null)
                return (null, null);

            var idx = id.IndexOf(_callbackInlikeKeySeparator);

            var routeKey = idx == -1 ? id : id[..idx];

            if (_idNameMap.TryGetValue(routeKey, out var route))
            {
                routeKey = route.Name;
            }
            else if (TryGetByReflection(routeKey, out route))
            {
                routeKey = route.Name;
            }

            idx++;
            return (Facts.CallbackQuery.CallbackPrefix + routeKey, id.Length > idx ? id[idx..] : null);
        }

        private string GenerateInlineCallbackKey<T>(string value = null)
        {
            var type = typeof(T);

            if (!_nameIdMap.TryGetValue(type, out var id))
                id = SetToDictionary(type);

            return $"{id}{_callbackInlikeKeySeparator}{value}";
        }
        private string SetToDictionary(Type type)
        {
            lock (_lock)
            {
                var id = GetIdFromType(type);

                _nameIdMap[type] = id;

                if (_idNameMap.TryGetValue(id, out var val) && val != type)
                    throw new Exception($"Colision between types: {val} and {type}");

                _idNameMap[id] = type;
                return id;

            }
        }
        private static string GetIdFromType(Type type)
        {
            var guid = BitConverter.GetBytes(Adler32(type.Namespace))
                                .Concat(BitConverter.GetBytes(Adler32(type.Name)))
                                .ToArray();

            var id = Convert.ToBase64String(guid).TrimEnd('=');
            return id;
        }
        private bool TryGetByReflection(string routeKey, out Type route)
        {
            LoadReflectionTypes();
            return _idNameMap.TryGetValue(routeKey, out route);
        }
        private void LoadReflectionTypes()
        {
            if (started)
                return;

            lock (_reflectionLock)
            {
                if (started)
                    return;

                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    var types = assembly.DefinedTypes
                        .Where(a => !a.IsAbstract && !a.IsInterface && typeof(TelegramHandlerBase).IsAssignableFrom(a));

                    foreach (var item in types)
                        SetToDictionary(item);
                }

                started = true;
            }
        }
        private static uint Adler32(string str)
        {
            const int mod = 65521;
            uint a = 1, b = 0;
            foreach (char c in str)
            {
                a = (a + c) % mod;
                b = (b + a) % mod;
            }

            return (b << 16) | a;
        }
    }
}
