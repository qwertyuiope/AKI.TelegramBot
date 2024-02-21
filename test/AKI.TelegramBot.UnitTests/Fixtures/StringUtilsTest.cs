using NUnit.Framework;

namespace AKI.TelegramBot.UnitTests.Fixtures
{
    [TestFixture]
    public class StringUtilsTest
    {
        [Test]
        public void StringUtils_EscapeMarkdown_ReturnsEscapedText()
        {
            // Arrange
            var text = "Hello *World*!";
            var expected = "Hello \\*World\\*\\!";

            // Act
            var result = StringUtils.EscapeMarkdownV2(text);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void StringUtils_EscapeFirstCharMarkdown_TextWithMarkdownChars_ReturnsTextWithEscapeChar()
        {
            // Arrange
            var text = "]Hello World";
            var expected = "\\]Hello World";

            // Act
            var result = StringUtils.EscapeFirstCharMarkdown(text);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void StringUtils_EscapeFirstCharMarkdown_TextWithoutMarkdownChars_ReturnsOriginalText()
        {
            // Arrange
            var text = "Hello World";

            // Act
            var result = StringUtils.EscapeFirstCharMarkdown(text);

            // Assert
            Assert.That(result, Is.EqualTo(text));
        }

        [Test]
        public void StringUtils_EscapeFirstCharMarkdown_TextWithMarkdownCharsAtBeginning_ReturnsTextWithEscapeChar()
        {
            // Arrange
            var text = "*Hello World";
            var expected = "\\*Hello World";

            // Act
            var result = StringUtils.EscapeFirstCharMarkdown(text);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void StringUtils_EscapeFirstCharMarkdown_TextWithMarkdownCharsAfterEscapeChar_ReturnsTextWithoutEscapeChar()
        {
            // Arrange
            var text = "\\*Hello World";
            var expected = "\\*Hello World";

            // Act
            var result = StringUtils.EscapeFirstCharMarkdown(text);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
