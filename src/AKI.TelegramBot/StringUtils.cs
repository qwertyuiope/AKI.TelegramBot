using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AKI.TelegramBot
{
    public class StringUtils
    {
        private const string MarkDownChars = @"*_`\[\]()<>#+\-=|{}.!";
        private static readonly IReadOnlyCollection<char> _markDownCharArray = MarkDownChars.ToArray();
        private static readonly Regex _markDownEscapeRegex = new($"([{MarkDownChars}])", RegexOptions.Compiled, TimeSpan.FromSeconds(5));
        public static string EscapeMarkdown(string text)
        {
            return _markDownEscapeRegex.Replace(text, @"\$1");
        }
        public static string EscapeFirstCharMarkdown(string text)
        {
            var firstChar = text.FirstOrDefault();
            if (!_markDownCharArray.Contains(firstChar))
                return text;

            if (firstChar == '\\')
            {
                if (text.Length < 2)
                    return string.Empty;
                var secondChar = text[1];
                if (_markDownCharArray.Contains(secondChar))
                    return text;
            }

            return '\\' + text;
        }

    }
}
