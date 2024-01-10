using Telegram.Bot.Types;

namespace AKI.TelegramBot.ClientUtils.Models
{
    public class MultiSendResult
    {
        public MultiSendResult(Message[] messages, int lastMessageStartIndex)
        {
            Messages = messages;
            LastMessageStartIndex = lastMessageStartIndex;
        }

        public Message[] Messages { get; }
        public int LastMessageStartIndex { get; }
    }
}
