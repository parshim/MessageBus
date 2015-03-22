namespace MessageBus.Core
{
    public class DataContractKey
    {
        private readonly string _name;
        private readonly string _namespace;

        public static readonly DataContractKey BinaryBlob = new DataContractKey("base64Binary", "http://schemas.microsoft.com/2003/10/Serialization/");

        public DataContractKey(string name, string ns)
        {
            _name = name;
            _namespace = ns;
        }

        public string Name
        {
            get { return _name; }
        }

        public string Ns
        {
            get { return _namespace; }
        }

        public bool Equals(DataContractKey other)
        {
            return string.Equals(_name, other._name) && string.Equals(_namespace, other._namespace);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DataContractKey) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_name != null ? _name.GetHashCode() : 0)*397) ^ (_namespace != null ? _namespace.GetHashCode() : 0);
            }
        }
    }
}