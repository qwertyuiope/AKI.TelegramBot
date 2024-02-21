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
        public virtual ITextWriter Append(string value)
        {
            _sb.Append(value);
            return this;
        }

        public virtual bool IsValid()
        {
            return true;
        }
    }
}
