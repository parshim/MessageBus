using System;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class Processor<T> : IProcessor
    {
        private readonly Action<T> _callback;
        private readonly T _body;

        public Processor(Action<T> callback, T body)
        {
            _callback = callback;
            _body = body;
        }

        public void Process()
        {
            _callback(_body);
        }
    }
}