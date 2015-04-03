using System;
using System.Linq.Expressions;
using System.Reflection;
using MessageBus.Core.API;

namespace MessageBus.Core.Proxy
{
    public class SubscriptionSelector<T> : ISubscriptionSelector<T>
    {
        private readonly ISubscriber _subscriber;
        private readonly IMessageFactory _messageFactory;

        public SubscriptionSelector(ISubscriber subscriber, IMessageFactory messageFactory)
        {
            _subscriber = subscriber;
            _messageFactory = messageFactory;
        }

        public void Subscribe<TData>(Expression<Func<T, Action<TData>>> methodSelector, Action<TData> notificationCallback, params BusHeader[] filterHeaders)
        {
            MethodInfo methodInfo = GetMethodInfo(methodSelector);

            ParameterInfo[] parameters = methodInfo.GetParameters();

            if (parameters.Length != 1)
            {
                throw new ArgumentOutOfRangeException("methodSelector", "Parameter number missmatch");
            }

            Type messageType = _messageFactory.GetMessageType(methodInfo);

            FieldInfo fieldInfo = _messageFactory.GetMessageFieldInfo(methodInfo, parameters[0].Name);

            _subscriber.Subscribe(messageType, (object o) =>
            {
                object value = fieldInfo.GetValue(o);

                notificationCallback((TData) value);

            }, filter: filterHeaders);
        }

        public void Subscribe<TData>(Expression<Func<T, Action>> methodSelector, Action notificationCallback, params BusHeader[] filterHeaders)
        {
            MethodInfo methodInfo = GetMethodInfo(methodSelector);

            ParameterInfo[] parameters = methodInfo.GetParameters();

            if (parameters.Length != 0)
            {
                throw new ArgumentOutOfRangeException("methodSelector", "Parameter number missmatch");
            }

            Type messageType = _messageFactory.GetMessageType(methodInfo);

            _subscriber.Subscribe(messageType, (object o) => notificationCallback(), filter: filterHeaders);
        }

        public void Dispose()
        {
            _subscriber.Dispose();
        }

        public static MethodInfo GetMethodInfo(LambdaExpression methodSelector)
        {
            var exp = ((MethodCallExpression)(((UnaryExpression)methodSelector.Body).Operand)).Object as ConstantExpression;
            if (exp != null) return exp.Value as MethodInfo;
            return null;
        }
    }
}