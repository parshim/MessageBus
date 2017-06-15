using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;

namespace MessageBus.Core.Proxy
{
    public class MessageFactory : IMessageFactory
    {
        private readonly string _namespace;
        private readonly ModuleBuilder _moduleBuilder;

        private readonly ConcurrentDictionary<string, Lazy<Type>> _types = new ConcurrentDictionary<string, Lazy<Type>>();
        
        public MessageFactory(string ns)
        {
            _namespace = ns;

            AssemblyName aName = new AssemblyName(ns);

            AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);

            _moduleBuilder = ab.DefineDynamicModule(aName.Name);
        }

        public object CreateMessage(MethodInfo methodInfo, object[] values)
        {
            Type messageType = GetMessageType(methodInfo);

            object instance = Activator.CreateInstance(messageType);

            ParameterInfo[] parameters = methodInfo.GetParameters();

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameter = parameters[i];
                object value = values[i];

                FieldInfo fieldInfo = messageType.GetField(parameter.Name);

                fieldInfo.SetValue(instance, value);
            }

            return instance;
        }

        public Type GetMessageType(MethodInfo methodInfo)
        {
            string name = methodInfo.GetMessageName();

            ParameterInfo[] parameters = methodInfo.GetParameters();

            var lazy = _types.GetOrAdd(name, n => new Lazy<Type>(() => CreateMessageType(n, parameters)));

            return lazy.Value;
        }

        public FieldInfo GetMessageFieldInfo(MethodInfo methodInfo, string fieldName)
        {
            Type messageType = GetMessageType(methodInfo);

            return messageType.GetField(fieldName);
        }

        private Type CreateMessageType(string name, ParameterInfo[] parameters)
        {
            TypeBuilder typeBuilder = _moduleBuilder.DefineType(name, TypeAttributes.Class | TypeAttributes.Public);

            //typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);

            AddDataContractAttribute(name, typeBuilder);

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameter = parameters[i];

                FieldBuilder fieldBuilder = typeBuilder.DefineField(parameter.Name, parameter.ParameterType, FieldAttributes.Public);

                AddDataMemberAttribute(parameter.Name, fieldBuilder);
            }

            return typeBuilder.CreateType();
        }

        private void AddDataMemberAttribute(string name, FieldBuilder fieldBuilder)
        {
            ConstructorInfo classCtorInfo = typeof(DataMemberAttribute).GetConstructor(new Type[0]);

            PropertyInfo nameProperty = typeof(DataMemberAttribute).GetProperty("Name");

            CustomAttributeBuilder attributeBuilder = new CustomAttributeBuilder(classCtorInfo, new object[0], new[] {nameProperty}, new object[] {name});

            fieldBuilder.SetCustomAttribute(attributeBuilder);
        }
        
        private void AddDataContractAttribute(string name, TypeBuilder typeBuilder)
        {
            ConstructorInfo classCtorInfo = typeof (DataContractAttribute).GetConstructor(new Type[0]);

            PropertyInfo nameProperty = typeof (DataContractAttribute).GetProperty("Name");
            PropertyInfo namespaceProperty = typeof (DataContractAttribute).GetProperty("Namespace");

            CustomAttributeBuilder attributeBuilder = new CustomAttributeBuilder(classCtorInfo, new object[0],
                new[] {nameProperty, namespaceProperty}, new object[] {name, _namespace});

            typeBuilder.SetCustomAttribute(attributeBuilder);
        }
    }
}