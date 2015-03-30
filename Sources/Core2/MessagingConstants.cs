namespace MessageBus.Core
{
    public class MessagingConstants
    {
        public class HeaderNames
        {
            public const string BusId = "BusId";
            public const string SentTime = "SentTime";
            public const string Name = "Name";
            public const string NameSpace = "Namespace";
        }
        public class Actor
        {
            public const string User = "User";
            public const string Bus = "Bus";
        }

        public class MessageAction
        {
            public const string Regular = "Message";
        }
        public class Namespace
        {
            public const string MessageBus = "www.messagebus.org";
        }
    }


}
