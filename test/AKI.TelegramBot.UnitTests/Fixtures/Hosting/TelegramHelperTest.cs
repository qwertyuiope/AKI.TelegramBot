using NUnit.Framework;
using AKI.TelegramBot.Hosting;
using NUnit.Framework.Internal;

namespace AKI.TelegramBot.UnitTests.Fixtures.Hosting
{
    [TestFixture]
    public class TelegramHelperTest
    {
        [Test]
        public void TelegramHelper_CreateCommandKey_ReturnsCommandKey()
        {
            // Arrange
            var expected = $"message:AKI.TelegramBot.UnitTests.Fixtures.Hosting.{nameof(TelegramHelperTest)}";

            // Act
            var result = TelegramHelper.CreateCommandKey<TelegramHelperTest>();

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }
        [Test]
        [TestCase("/start", "message:/start")]
        [TestCase("start", "message:start")]
        [TestCase(null, "message:")]
        [TestCase("", "message:")]
        [TestCase("", "message:")]
        [TestCase("/start command", "message:/start")]
        public void TelegramHelper_ParseCommandKey_Success(string text, string expected)
        {
            // Act
            var result = TelegramHelper.ParseCommandKey(text);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
