using NUnit.Framework;
using AKI.TelegramBot.ClientUtils;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using AKI.TelegramBot.UnitTests.Mocks;
using System.Linq;
using System.Collections.Generic;

namespace AKI.TelegramBot.UnitTests.Fixtures.ClientUtils
{
    [TestFixture]
    public class ClientExtensionsTest
    {
        private TelegramBotClientMock _telegramClient;

        [SetUp]
        public void SetUp()
        {
            _telegramClient = new TelegramBotClientMock();
        }
        [Test]
        public async Task ClientExtensions_TypeWhileWait_CompletesTask()
        {
            // Arrange
            long chatId = 123456789;
            var taskResult = Task.FromResult(42);
            var cancellationToken = CancellationToken.None;
            var delay = 0;

            // Act
            var result = await _telegramClient.TypeWhileWait(chatId, taskResult, cancellationToken, delay);

            // Assert
            Assert.That(result, Is.EqualTo(42));
        }

        [Test]
        public async Task ClientExtensions_SendMarkDownMessages_ReturnsMultiSendResult()
        {
            // Arrange
            long chatId = 123456789;
            var text = "Hello *World*!";
            var cancellationToken = CancellationToken.None;
            IReplyMarkup replyMarkup = null;
            int? messageId = null;
            var lastMsgStartIdx = 0;
            var expected = StringUtils.EscapeMarkdown(text);

            // Act
            var result = await _telegramClient.SendMarkDownMessages(chatId, text, cancellationToken, replyMarkup, messageId, lastMsgStartIdx);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Messages.Length, Is.EqualTo(1));
            Assert.That(result.Messages.FirstOrDefault().Text, Is.EqualTo(expected));
        }

        [Test]
        public async Task ClientExtensions_StreamMessages_ReturnsResponseString()
        {
            // Arrange
            long chatId = 123456789;
            async static IAsyncEnumerable<string> enumerator()
            {
                yield return "Hello";
                yield return " ";
                yield return "World";
                yield return "!";
            }
            CancellationToken cancellationToken = CancellationToken.None;
            int delayTime = 750;

            // Act
            string result = await _telegramClient.StreamMessages(chatId, enumerator().GetAsyncEnumerator(), cancellationToken, delayTime);

            // Assert
            Assert.That(result, Is.EqualTo("Hello World!"));
        }

        [Test]
        public async Task ClientExtensions_StreamMessages_ReturnsBigResponseString()
        {
            // Arrange
            long chatId = 123456789;
            var length = 10000;
            var word = "HELLO!";
            async static IAsyncEnumerable<string> enumerator(string word, int length)
            {
                for (int i = 0; i < length; i++)
                    yield return word;
            }
            CancellationToken cancellationToken = CancellationToken.None;
            int delayTime = 750;
            var expected = string.Concat(Enumerable.Repeat(word, length));

            // Act
            string result = await _telegramClient.StreamMessages(chatId, enumerator(word, length).GetAsyncEnumerator(), cancellationToken, delayTime);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public async Task ClientExtensions_SendMessages_ReturnsMultiSendResult()
        {
            // Arrange
            long chatId = 123456789;
            var text = "Hello *World*!";
            var parseMode = ParseMode.MarkdownV2;
            var cancellationToken = CancellationToken.None;
            IReplyMarkup replyMarkup = null;
            int? messageId = null;
            var lastMsgStartIdx = 0;

            // Act
            var result = await _telegramClient.SendMessages(chatId, text, parseMode, cancellationToken, replyMarkup, messageId, lastMsgStartIdx);

            // Assert
            Assert.IsNotNull(result);
        }

    }
}
