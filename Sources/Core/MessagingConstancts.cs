namespace MessageBus.Core
{
    internal class MessagingConstancts
    {
        internal class Namespace
        {
            public const string MessageBus = "www.messagebus.org";
        }

        internal class Actor
        {
            public const string User = "User";
            public const string Bus = "Bus";
        }

        internal class MessageAction
        {
            public const string Regular = "Message";
        }

        internal class HeaderNames
        {
            public const string BusId = "BusId";
            public const string SentTime = "SentTime";
        }
    }


}
