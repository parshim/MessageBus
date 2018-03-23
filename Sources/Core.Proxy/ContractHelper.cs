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
                // Support generic types
                name = type.ToString();
                if (name.IndexOf(type.Namespace ?? string.Empty, 0, StringComparison.Ordinal) == 0)
                {
                    name = name.Substring((type.Namespace?.Length ?? -1) + 1);
                }
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