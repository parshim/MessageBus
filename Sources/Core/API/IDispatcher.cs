using System.Xml;

namespace MessageBus.Core.API
{
    internal interface IDispatcher
    {
        void Dispatch(XmlDictionaryReader bodyContents);
    }
}