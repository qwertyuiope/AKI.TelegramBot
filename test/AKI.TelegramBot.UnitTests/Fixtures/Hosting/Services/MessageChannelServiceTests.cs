using AKI.TelegramBot.Hosting;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using AKI.TelegramBot.Hosting.Services;
using System;

namespace AKI.TelegramBot.UnitTests.Fixtures.Hosting.Services
{
    [TestFixture]
    public class MessageChannelServiceTests
    {
        [Test]
        public async Task MessageChannelService_ProduceConsume_AddsMessageToChannel()
        {
            // Arrange
            var telegramConfiguration = new TelegramConfiguration
            {
                MessageWorkers = 1
            };
            var messageChannelService = new MessageChannelService(telegramConfiguration);
            var update = new Update();

            // Act
            await messageChannelService.Produce(update, CancellationToken.None);
            var message = await messageChannelService.Consume(CancellationToken.None);

            // Assert
            Assert.That(message.update, Is.Not.Null);
            Assert.That(message.update, Is.EqualTo(update));
        }
        [Test]
        public async Task MessageChannelService_ProduceConsume_Multiple_AddsMessageToChannel()
        {
            // Arrange
            var telegramConfiguration = new TelegramConfiguration
            {
                MessageWorkers = 5
            };
            var messageChannelService = new MessageChannelService(telegramConfiguration);
            var update = new Update();

            // Act
            CancellationTokenSource cancelationTokenSource;
            for (int i = 0; i < 5; i++)
            {
                cancelationTokenSource = new CancellationTokenSource();
                cancelationTokenSource.CancelAfter(100);
                await messageChannelService.Produce(update, cancelationTokenSource.Token);
            }
            cancelationTokenSource = new CancellationTokenSource();
            cancelationTokenSource.CancelAfter(100);

            //Assert
            Assert.ThrowsAsync<OperationCanceledException>(async () => await messageChannelService.Produce(update, cancelationTokenSource.Token));

            // Act
            for (int i = 0; i < 5; i++)
            {
                cancelationTokenSource = new CancellationTokenSource();
                cancelationTokenSource.CancelAfter(100);
                await messageChannelService.Consume(cancelationTokenSource.Token);
            }

            cancelationTokenSource = new CancellationTokenSource();
            cancelationTokenSource.CancelAfter(100);

            //Assert
            Assert.ThrowsAsync<OperationCanceledException>(async () => await messageChannelService.Consume(cancelationTokenSource.Token));
        }
    }
}
