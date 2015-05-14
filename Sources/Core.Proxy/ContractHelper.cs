using System;
using System.Reflection;
using System.ServiceModel;

namespace MessageBus.Core.Proxy
{
    internal static class ContractHelper
    {
        public static string GetMessageName(this MethodInfo info)
        {
            OperationContractAttribute attribute = info.GetCustomAttribute<OperationContractAttribute>();

            if (attribute == null || string.IsNullOrEmpty(attribute.Name))
            {
                return info.Name;
            }
            
            return attribute.Name;
        }

        public static string GetMessageNamespace(this Type type)
        {
            ServiceContractAttribute attribute = type.GetCustomAttribute<ServiceContractAttribute>();

            string name;

            if (attribute == null || string.IsNullOrEmpty(attribute.Name))
            {
                name = type.Name;
            }
            else
            {
                name = attribute.Name;
            }

            string ns = type.Namespace;
            
            return ns + "." + name;
        }
    }
}