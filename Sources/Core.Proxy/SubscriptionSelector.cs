using System;
using System.Collections.Generic;
using System.Linq;
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

        public void Subscribe<TData>(Expression<Func<T, Action<TData>>> methodSelector, Action<TData> notificationCallback, bool hierarchy = false, params BusHeader[] filterHeaders)
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

            }, filter: filterHeaders, hierarchy: hierarchy);
        }

        public void Subscribe(Expression<Func<T, Action>> methodSelector, Action notificationCallback, bool hierarchy = false, params BusHeader[] filterHeaders)
        {
            MethodInfo methodInfo = GetMethodInfo(methodSelector);

            ParameterInfo[] parameters = methodInfo.GetParameters();

            if (parameters.Length != 0)
            {
                throw new ArgumentOutOfRangeException("methodSelector", "Parameter number missmatch");
            }

            Type messageType = _messageFactory.GetMessageType(methodInfo);

            _subscriber.Subscribe(messageType, (object o) => notificationCallback(), filter: filterHeaders, hierarchy: hierarchy);
        }

        public void Subscribe<TData>(Expression<Func<T, Action<TData>>> methodSelector, Action<TData, IEnumerable<BusHeader>> notificationCallback, bool hierarchy = false, params BusHeader[] filterHeaders)
        {
            MethodInfo methodInfo = GetMethodInfo(methodSelector);

            ParameterInfo[] parameters = methodInfo.GetParameters();

            if (parameters.Length != 1)
            {
                throw new ArgumentOutOfRangeException("methodSelector", "Parameter number missmatch");
            }

            Type messageType = _messageFactory.GetMessageType(methodInfo);

            FieldInfo fieldInfo = _messageFactory.GetMessageFieldInfo(methodInfo, parameters[0].Name);

            _subscriber.Subscribe(messageType, m =>
            {
                object value = fieldInfo.GetValue(m.Data);

                notificationCallback((TData)value, m.Headers.OfType<BusHeader>());

            }, filter: filterHeaders, hierarchy: hierarchy);
        }

        public void Subscribe(Expression<Func<T, Action>> methodSelector, Action<IEnumerable<BusHeader>> notificationCallback, bool hierarchy = false, params BusHeader[] filterHeaders)
        {
            MethodInfo methodInfo = GetMethodInfo(methodSelector);

            ParameterInfo[] parameters = methodInfo.GetParameters();

            if (parameters.Length != 0)
            {
                throw new ArgumentOutOfRangeException("methodSelector", "Parameter number missmatch");
            }

            Type messageType = _messageFactory.GetMessageType(methodInfo);

            _subscriber.Subscribe(messageType, m => notificationCallback(m.Headers.OfType<BusHeader>()), filter: filterHeaders, hierarchy: hierarchy);
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