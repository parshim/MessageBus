using System;
using System.Runtime.Serialization;
using System.Xml;

namespace MessageBus.Core
{
    public class DataContract : XmlDictionaryWriter
    {
        private DataContractKey _key;

        private readonly DataContractSerializer _serializer;
        private readonly Action<object> _callback;

        public DataContract(Type contractType, Action<object> callback)
        {
            _callback = callback;
            _serializer = new DataContractSerializer(contractType);

            object o = Activator.CreateInstance(contractType, true);

            _serializer.WriteStartObject(this, o);
        }

        public DataContractKey Key
        {
            get { return _key; }
        }

        public DataContractSerializer Serializer
        {
            get { return _serializer; }
        }

        public Action<object> Callback
        {
            get { return _callback; }
        }

        #region XmlDictionaryWriter
        
        public override void WriteStartDocument()
        {
            
        }

        public override void WriteStartDocument(bool standalone)
        {
            
        }

        public override void WriteEndDocument()
        {
            
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            _key = new DataContractKey(localName, ns);
        }

        public override void WriteEndElement()
        {
            
        }

        public override void WriteFullEndElement()
        {
            
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            
        }

        public override void WriteEndAttribute()
        {
            
        }

        public override void WriteCData(string text)
        {
            
        }

        public override void WriteComment(string text)
        {
            
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            
        }

        public override void WriteEntityRef(string name)
        {
            
        }

        public override void WriteCharEntity(char ch)
        {
            
        }

        public override void WriteWhitespace(string ws)
        {
            
        }

        public override void WriteString(string text)
        {
            
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            
        }

        public override void WriteChars(char[] buffer, int index, int count)
        {
            
        }

        public override void WriteRaw(char[] buffer, int index, int count)
        {
            
        }

        public override void WriteRaw(string data)
        {
            
        }

        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            
        }

        public override void Close()
        {
            
        }

        public override void Flush()
        {
            
        }

        public override string LookupPrefix(string ns)
        {
            return "";
        }

        public override WriteState WriteState
        {
            get { return WriteState.Start; }
        }

        #endregion
    }
}