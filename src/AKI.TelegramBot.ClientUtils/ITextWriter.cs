namespace AKI.TelegramBot.ClientUtils
{
    internal interface ITextWriter
    {
        ITextWriter Append(string value);
        bool IsValid();
        string ToString();
        int Length {  get; }
    }
}
