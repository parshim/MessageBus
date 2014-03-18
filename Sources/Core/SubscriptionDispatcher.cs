using System;
using System.Collections.Generic;
using System.Reflection;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    internal class SubscriptionDispatcher : DispatcherBase, ISubscriptionDispatcher
    {
        public SubscriptionDispatcher(IErrorSubscriber errorSubscriber, string busId) : base(errorSubscriber, busId)
        {
        }

        public void RegisterSubscribtion(object instance)
        {
            Type type = instance.GetType();

            object[] attributes = type.GetCustomAttributes(typeof (SubscribtionAttribute), false);

            if (attributes.Length == 0)
            {
                throw new Exception("SubscribtionAttribute is missing");
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
                    throw new Exception("Method anotated by MessageSubscribtionAttribute must have exacly one parameter");
                }

                ParameterInfo parameterInfo = parameterInfos[0];
                MessageSubscribtionAttribute messageSubscribtionAttribute = (MessageSubscribtionAttribute) methodAttributes[0];
                
                Type parameterType = parameterInfo.ParameterType;
                DataContract dataContract;
                ICallHandler handler;

                MethodInfo m = method;
                Action<object> action = o => m.Invoke(instance, new[] {o});

                if (parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof (BusMessage<>))
                {
                    Type[] genericArguments = parameterType.GetGenericArguments();

                    dataContract = new DataContract(genericArguments[0]);

                    Type handlerType = typeof(BusMessageHandler<>).MakeGenericType(genericArguments);

                    handler = (ICallHandler) Activator.CreateInstance(handlerType, action);
                }
                else
                {
                    dataContract = new DataContract(parameterType);

                    handler = new ActionHandler(action);
                }

                List<BusHeader> filters = new List<BusHeader>();
                object[] filterAttributes = parameterInfo.GetCustomAttributes(typeof (HeaderFilterAttribute), true);

                foreach (HeaderFilterAttribute filterAttribute in filterAttributes)
                {
                    filters.Add(new BusHeader(filterAttribute.Name, filterAttribute.Value));
                }
                
                RegisterType(dataContract.Key,
                                        new MessageSubscribtionInfo(dataContract.Key, handler,
                                                                    dataContract.Serializer,
                                                                    messageSubscribtionAttribute.ReceiveSelfPublish,
                                                                    filters));

            }
        }
    }

    
}
