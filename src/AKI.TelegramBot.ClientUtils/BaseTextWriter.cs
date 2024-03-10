using System.Text;

namespace AKI.TelegramBot.ClientUtils
{
    internal abstract class BaseTextWriter : ITextWriter
    {
        protected readonly StringBuilder _sb;

        public BaseTextWriter()
        {
            _sb = new StringBuilder();
        }

        public int Length => _sb.Length;

        public virtual ITextWriter Append(string value)
        {
            _sb.Append(value);
            return this;
        }

        public virtual bool IsValid()
        {
            return true;
        }
        public override string ToString()
        {
            return _sb.ToString();
        }
    }
}
