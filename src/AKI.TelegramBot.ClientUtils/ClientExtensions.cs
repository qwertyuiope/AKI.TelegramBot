using AKI.TelegramBot.ClientUtils.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AKI.TelegramBot.ClientUtils
{
    public static class ClientExtensions
    {
        public static async Task<T> TypeWhileWait<T>(this ITelegramBotClient telegramBotClient, long chatId,
            Task<T> taskResult, CancellationToken cancellationToken, int delay = 4500)
        {
            var typingTask = telegramBotClient.SendChatActionAsync(chatId, ChatAction.Typing, null, cancellationToken);

            while (!taskResult.IsCompleted)
            {
                await Task.WhenAny(Task.WhenAll(typingTask, Task.Delay(delay, cancellationToken)), taskResult);
                typingTask = telegramBotClient.SendChatActionAsync(chatId, ChatAction.Typing, null, cancellationToken);
            }

            return taskResult.Result;
        }
        public static async Task TypeWhileWait(this ITelegramBotClient telegramBotClient, long chatId, Task taskResult,
            CancellationToken cancellationToken, int delay = 4500)
        {
            var typingTask = telegramBotClient.SendChatActionAsync(chatId, ChatAction.Typing, null, cancellationToken);

            while (!taskResult.IsCompleted)
            {
                await Task.WhenAny(Task.WhenAll(typingTask, Task.Delay(delay, cancellationToken)), taskResult);
                typingTask = telegramBotClient.SendChatActionAsync(chatId, ChatAction.Typing, null, cancellationToken);
            }
        }
        public static async Task<MultiSendResult> SendMarkDownMessages(this ITelegramBotClient telegramBotClient,
            long chatId, string text, CancellationToken cancellationToken, IReplyMarkup replyMarkup = null,
            int? messageId = null, int lastMsgStartIdx = 0)
        {
            return await telegramBotClient.SendMessages(chatId,
                                                        text,
                                                        ParseMode.MarkdownV2,
                                                        cancellationToken,
                                                        replyMarkup,
                                                        messageId,
                                                        lastMsgStartIdx: lastMsgStartIdx);
        }
        private class TyperWaiter
        {
            private readonly ITelegramBotClient _telegramBotClient;
            private readonly long _chatId;
            private readonly int _delayTime;
            private readonly CancellationToken _cancellationToken;
            private byte _initialTyping;
            private byte _typing = 0;
            private Task _delayTask;

            public TyperWaiter(ITelegramBotClient telegramBotClient, long chatId, int delayTime, CancellationToken cancellationToken, byte initialTyping = 4)
            {
                _telegramBotClient = telegramBotClient;
                _chatId = chatId;
                _delayTime = delayTime;
                _cancellationToken = cancellationToken;
                _initialTyping = initialTyping;
                Restart();
            }
            public void Restart()
            {
                if (_typing-- <= 0)
                {
                    _delayTask = _telegramBotClient.TypeWhileWait(_chatId, Task.Delay(_delayTime), _cancellationToken);
                    _typing = _initialTyping++;
                    return;
                }
                _delayTask = Task.Delay(_delayTime, _cancellationToken);
            }
            public bool IsReady => _delayTask.IsCompleted;
        }
        public static async Task<string> StreamMessages(this ITelegramBotClient telegramBotClient, long chatId,
            IAsyncEnumerator<string> enumerator, CancellationToken cancellationToken, int delayTime = 750, ParseMode parseMode = ParseMode.MarkdownV2)
        {
            ITextWriter responseSb = parseMode == ParseMode.Markdown ? new MarkdownTextWriter() : new RegularTextWriter();
            Message lastMessage = null;
            var typerWaiter = new TyperWaiter(telegramBotClient, chatId, delayTime, cancellationToken, 4);
            var lastPrint = false;
            var lastMsgIdx = 0;
            while (await enumerator.MoveNextAsync())
            {
                try
                {
                    var result = enumerator.Current;
                    if (result is null)
                        continue;
                    responseSb.Append(result);
                    lastPrint = false;

                    if (typerWaiter.IsReady)
                    {
                        if (!responseSb.IsValid())
                        {
                            typerWaiter.Restart();
                            continue;
                        }
                        var message = responseSb.ToString();

                        var messages = await telegramBotClient.SendMessages(chatId: chatId,
                                                                                    text: message,
                                                                                    parseMode: parseMode,
                                                                                    cancellationToken: cancellationToken,
                                                                                    messageId: lastMessage?.MessageId,
                                                                                    lastMsgStartIdx: lastMsgIdx);
                        lastMessage = messages.Messages.LastOrDefault();
                        lastMsgIdx = messages.LastMessageStartIndex;
                        typerWaiter.Restart();
                        lastPrint = true;
                    }
                }
                catch (Exception ex)
                {
                    if (ex is ApiRequestException apiEx && apiEx.Message.Contains("Can't find end of the entity starting at byte"))
                        continue;
                    throw;
                }

            }

            var responseString = responseSb.ToString();
            if (!lastPrint && !string.IsNullOrEmpty(responseString))
            {
                _ = await telegramBotClient.SendMessages(chatId: chatId, text: responseString, parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken, messageId: lastMessage?.MessageId);
            }

            return responseString;
        }

        private static async Task<Message> SendMessageWithRetry(this ITelegramBotClient telegramBotClient, long chatId, string message,
            ParseMode parseMode, CancellationToken cancellationToken, IReplyMarkup replyMarkup = null)
        {
            var waiting = 1000;
            while (true)
            {
                try
                {
                    return await telegramBotClient.SendTextMessageAsync(
                          chatId: chatId, text: message, parseMode: parseMode,
                           replyMarkup: replyMarkup,
                          cancellationToken: cancellationToken);
                }
                catch (ApiRequestException apiEx)
                {
                    waiting = await WaitIfRetryAfter(waiting, apiEx);

                    if (waiting == -1)
                        throw;
                }
            }
        }

        public static async Task<MultiSendResult> SendMessages(this ITelegramBotClient telegramBotClient, long chatId,
            string text, ParseMode parseMode,
            CancellationToken cancellationToken, IReplyMarkup replyMarkup = null, int? messageId = null,
            int lastMsgStartIdx = 0)
        {
            switch (parseMode)
            {
                case ParseMode.MarkdownV2:
                    text = StringUtils.EscapeMarkdownV2(text);
                    break;
                case ParseMode.Markdown:
                case ParseMode.Html:
                    break;
                default:
                    break;
            }

            if (lastMsgStartIdx > 0)
                text = text[lastMsgStartIdx..];

            var maxSize = TelegramFacts.TELEGRAM_MAX_MSG_SIZE - 1;
            var total = text.Length / maxSize;
            var last = total > 0 ? text.Length % maxSize : text.Length;

            var messages = new Message[total + (last > 0 ? 1 : 0)];
            for (var i = 0; i < total; i++)
            {
                var message = text.Substring(i * maxSize, maxSize);

                switch (parseMode)
                {
                    case ParseMode.MarkdownV2:
                        message = StringUtils.EscapeFirstCharMarkdown(message);
                        break;
                    case ParseMode.Markdown:
                    case ParseMode.Html:
                        break;
                    default:
                        break;
                }
                messages[i] = await SendOrEdit(telegramBotClient, chatId,
                    parseMode, message, cancellationToken, messageId);
                messageId = null;

            }

            if (last > 0)
            {
                var message = total == 0 ? text : text.Substring(total * maxSize, last);
                switch (parseMode)
                {
                    case ParseMode.MarkdownV2:
                        message = StringUtils.EscapeFirstCharMarkdown(message);
                        break;
                    case ParseMode.Markdown:
                    case ParseMode.Html:
                        break;
                    default:
                        break;
                }
                if (!string.IsNullOrWhiteSpace(message))
                {
                    messages[total] = await SendOrEdit(telegramBotClient, chatId, parseMode,
                        message, cancellationToken, messageId);
                }
            }
            return new MultiSendResult(messages, total * maxSize + lastMsgStartIdx);
        }

        private static async Task<Message> SendOrEdit(ITelegramBotClient telegramBotClient, long chatId, ParseMode parseMode,
            string message, CancellationToken cancellationToken, int? messageId)
        {
            if (messageId is null)
            {
                return await telegramBotClient.SendMessageWithRetry(
                                  chatId: chatId, message: message, parseMode: parseMode,
                                  cancellationToken: cancellationToken);
            }

            return await telegramBotClient.EditMessageWithRetryAsync(
                  chatId: chatId, messageId: messageId.Value, text: message, parseMode: parseMode,
                  cancellationToken: cancellationToken);
        }

        private static async Task<Message> EditMessageWithRetryAsync(this ITelegramBotClient telegramBotClient, long chatId, int messageId, string text, ParseMode parseMode, CancellationToken cancellationToken)
        {
            var waiting = 1000;
            while (true)
            {
                try
                {
                    return await telegramBotClient.EditMessageTextAsync(
                     chatId: chatId, messageId: messageId, text: text, parseMode: parseMode,
                     cancellationToken: cancellationToken);
                }
                catch (ApiRequestException apiEx)
                {
                    waiting = await WaitIfRetryAfter(waiting, apiEx);

                    if (waiting == -1)
                        throw;
                }
            }
        }

        private static async Task<int> WaitIfRetryAfter(int waiting, ApiRequestException apiEx)
        {
            if (apiEx.ErrorCode == (int)HttpStatusCode.TooManyRequests)
            {
                var waitTime = TimeSpan.FromMilliseconds(apiEx.Parameters.RetryAfter ?? waiting);
                Serilog.Log.Logger.Debug($"Too many requests. Waiting {waitTime}ms");
                await Task.Delay(waitTime);
                return waiting + 1000;
            }

            return -1;

        }
    }
}
