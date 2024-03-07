using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AKI.TelegramBot.ClientUtils
{
    internal class MarkdownTextWriter : BaseTextWriter
    {
        //private static readonly IReadOnlyDictionary<string, MarkdownSyntaxRule> _openingSyntax = new Dictionary<string, MarkdownSyntaxRule>
        //{
        //    ["***"] = new MarkdownSyntaxRule("bold and italic"),
        //    ["_"] = new MarkdownSyntaxRule("bold not in middle of word"),
        //    ["*"] = new MarkdownSyntaxRule("bold"),
        //    ["__"] = new MarkdownSyntaxRule("italic not in middle of word"),
        //    ["**"] = new MarkdownSyntaxRule("italic"),
        //    ["`"] = new MarkdownSyntaxRule("code"),
        //    ["``"] = new MarkdownSyntaxRule("escape"),
        //    [">"] = new MarkdownSyntaxRule("block"),
        //    ["***"] = new MarkdownSyntaxRule("horizontal line"),
        //    ["---"] = new MarkdownSyntaxRule("horizontal line"),
        //    ["___"] = new MarkdownSyntaxRule("horizontal line"),
        //    ["~~~"] = new MarkdownSyntaxRule("block"),
        //};
        public MarkdownTextWriter() : base()
        {
        }
        public override ITextWriter Append(string value)
        {
            foreach (var c in value.AsSpan())
            {
                if (c == '`')
                    isCodeBlock = !isCodeBlock;
            }
            return base.Append(value);
        }
        private bool isCodeBlock = false;
        public override bool IsValid() => !isCodeBlock;
    }
}
