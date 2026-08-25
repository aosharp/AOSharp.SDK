using AOSharp.Common.Unmanaged.DataTypes;
using AOSharp.Common.Unmanaged.Imports;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;

namespace AOSharp.Core
{
    public static class FormatFeedbackMessageExtensions
    {
        public static string GetFormattedMessage(this FormatFeedbackMessage message)
        {
            StdString stdStr = StdString.Create();
            RemoteFormat.ParseString(stdStr.Pointer, message.Message);
            string formattedMessage = stdStr.ToString();
            stdStr.Dispose();
            return formattedMessage;
        }
    }
}
