using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class SubscriptionHelper : ISubscriptionHelper
    {
        private readonly Func<Type, MessageFilterInfo, ICallHandler, bool> _registrationAction;

        public SubscriptionHelper(Func<Type, MessageFilterInfo, ICallHandler, bool> registrationAction)
        {
            _registrationAction = registrationAction;
        }

        public bool Subscribe(Type dataType, ICallHandler handler, bool hierarchy, IEnumerable<BusHeader> filter)
        {
            if (hierarchy)
            {
                return SubscribeHierarchy(dataType, handler, filter);
            }

            return Subscribe(dataType, handler, filter);
        }

        public bool Subscribe(Type dataType, ICallHandler handler, IEnumerable<BusHeader> filter)
        {
            DataContract dataContract = new DataContract(dataType);

            MessageFilterInfo filterInfo = new MessageFilterInfo(dataContract.Key, filter ?? Enumerable.Empty<BusHeader>());

            return _registrationAction(dataType, filterInfo, handler);
        }

        public bool SubscribeHierarchy(Type baseType, ICallHandler handler, IEnumerable<BusHeader> filter)
        {
            var types = from type in baseType.Assembly.GetTypes()
                where type != baseType && baseType.IsAssignableFrom(type)
                select type;

            bool atLeastOne = false;

            foreach (Type type in types)
            {
                atLeastOne = Subscribe(type, handler, filter) || atLeastOne;
            }

            return atLeastOne;
        }

        public void RegisterSubscription(object instance)
        {
            Type type = instance.GetType();

            object[] attributes = type.GetCustomAttributes(typeof(SubscribtionAttribute), false);

            if (attributes.Length == 0)
            {
                throw new Exception("SubscriptionAttribute is missing");
            }

            foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                object[] methodAttributes = method.GetCustomAttributes(typeof (MessageSubscribtionAttribute), false);

                if (methodAttributes.Length == 0)
                {
                    continue;
                }

                ParameterInfo[] parameterInfos = method.GetParameters();

                if (parameterInfos.Length != 1)
                {
                    throw new Exception(
                        "Method annotated by MessageSubscribtionAttribute must have exactly one parameter");
                }

                ParameterInfo parameterInfo = parameterInfos[0];
                MessageSubscribtionAttribute messageSubscriptionAttribute = (MessageSubscribtionAttribute) methodAttributes[0];

                Type parameterType = parameterInfo.ParameterType;
                ICallHandler handler;
                Type dataType;

                MethodInfo m = method;
                Action<object> action = o => m.Invoke(instance, new[] {o});

                if (parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof (BusMessage<>))
                {
                    Type[] genericArguments = parameterType.GetGenericArguments();

                    dataType = genericArguments[0];

                    Type handlerType = typeof (BusMessageHandler<>).MakeGenericType(genericArguments);

                    handler = (ICallHandler) Activator.CreateInstance(handlerType, action);
                }
                else
                {
                    dataType = parameterType;

                    handler = new ActionHandler(action);
                }

                List<BusHeader> filters = new List<BusHeader>();
                object[] filterAttributes = parameterInfo.GetCustomAttributes(typeof (HeaderFilterAttribute), true);

                foreach (HeaderFilterAttribute filterAttribute in filterAttributes)
                {
                    filters.Add(new BusHeader(filterAttribute.Name, filterAttribute.Value));
                }

                Subscribe(dataType, handler, messageSubscriptionAttribute.RegisterHierarchy, filters);
            }
        }
    }
}