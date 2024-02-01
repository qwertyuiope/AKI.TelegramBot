using AKI.TelegramBot.ClientUtils.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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
        public static async Task<string> StreamMessages(this ITelegramBotClient telegramBotClient, long chatId,
            IAsyncEnumerator<string> enumerator, CancellationToken cancellationToken, int delayTime = 750, ParseMode parseMode = ParseMode.MarkdownV2)
        {
            var responseSb = new StringBuilder();
            Message lastMessage = null;

            var delay = telegramBotClient.TypeWhileWait(chatId, Task.Delay(delayTime), cancellationToken);
            byte initialTyping = 4;
            var typing = initialTyping++;
            var lastPrint = false;
            var lastMsgIdx = 0;
            while (await enumerator.MoveNextAsync())
            {
                var result = enumerator.Current;
                if (result is null)
                    continue;
                responseSb.Append(result);
                lastPrint = false;
                if (delay.IsCompleted)
                {
                    var messages = await telegramBotClient.SendMessages(chatId: chatId,
                                                                                text: responseSb.ToString(),
                                                                                parseMode: parseMode,
                                                                                cancellationToken: cancellationToken,
                                                                                messageId: lastMessage?.MessageId,
                                                                                lastMsgStartIdx: lastMsgIdx);
                    lastMessage = messages.Messages.LastOrDefault();
                    lastMsgIdx = messages.LastMessageStartIndex;
                    if (typing-- == 0)
                    {
                        delay = telegramBotClient.TypeWhileWait(chatId, Task.Delay(delayTime), cancellationToken);
                        typing = initialTyping++;
                    }
                    else
                    {
                        delay = Task.Delay(delayTime);
                    }
                    lastPrint = true;
                }
            }

            var responseString = responseSb.ToString();
            if (!lastPrint)
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
                    text = StringUtils.EscapeMarkdown(text);
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
                    case ParseMode.Markdown:
                    case ParseMode.MarkdownV2:
                        message = StringUtils.EscapeFirstCharMarkdown(message);
                        break;
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
                    case ParseMode.Markdown:
                    case ParseMode.MarkdownV2:
                        message = StringUtils.EscapeFirstCharMarkdown(message);
                        break;
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
