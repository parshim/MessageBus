using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class RpcPublisherConfigurator : PublisherConfigurator, IRpcPublisherConfigurator
    {
        private bool _useFastReply;
        private string _replyExchange;
        private string _consumerTag = "";

        public RpcPublisherConfigurator(string exchange, bool useFastReply, string replyExchange, IPublishingErrorHandler errorHandler, ITrace trace)
            : base(exchange, errorHandler, trace)
        {
            _useFastReply = useFastReply;
            _replyExchange = replyExchange;
        }

        public bool UseFastReply
        {
            get { return _useFastReply; }
        }

        public string ReplyExchange
        {
            get { return _replyExchange; }
        }
        public string ConsumerTag
        {
            get { return _consumerTag; }
        }

        public IRpcPublisherConfigurator DisableFastReply()
        {
            _useFastReply = false;

            return this;
        }


        public IRpcPublisherConfigurator SetReplyExchange(string replyExchange)
        {
            _replyExchange = replyExchange;

            return this;
        }

        public IRpcPublisherConfigurator SetConsumerTag(string consumerTag)
        {
            _consumerTag = consumerTag;

            return this;
        }
    }
}