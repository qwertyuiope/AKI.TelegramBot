using AKI.TelegramBot.Hosting;
using AKI.TelegramBot.Hosting.Abstract;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace AKI.TelegramBot.UnitTests.Fixtures.Hosting
{
    [TestFixture]
    public class BotSetupTest
    {
        [Test]
        public async Task Test()
        {
            //Arrange
            var config = new TelegramConfiguration
            {
                BotToken = ""
            };
            IHost host = Host.CreateDefaultBuilder().ConfigureServices(services =>
            {
                services.AddTelegramBot(config).BotSetup<BotSetup>();
            }).Build();

            //Act & Assert
            Assert.IsFalse(BotSetup.Started);

            await host.StartAsync(cancellationToken: CancellationToken.None);

            Assert.IsTrue(BotSetup.Started);
            Assert.IsFalse(BotSetup.Stopped);

            await host.StopAsync(cancellationToken: CancellationToken.None);

            Assert.IsTrue(BotSetup.Stopped);


        }
        private class BotSetup : IBotSetup
        {
            public static bool Started { get; private set; }
            public static bool Stopped { get; private set; }
            public Task OnStartAsync()
            {
                Started = true;
                return Task.CompletedTask;
            }

            public Task OnStopAsync()
            {
                Stopped = true;
                return Task.CompletedTask;
            }
        }
    }
}
