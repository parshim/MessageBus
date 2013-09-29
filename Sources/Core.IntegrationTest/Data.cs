using System.Runtime.Serialization;

namespace Core.IntegrationTest
{
    [DataContract]
    public class Data
    {
        
    }
    
    [DataContract]
    public class Person : Data
    {
        [DataMember]
        public int Id { get; set; }

        protected bool Equals(Person other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Person)obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }

    [DataContract]
    public class Car : Data
    {
        [DataMember]
        public string Number { get; set; }

        protected bool Equals(Car other)
        {
            return string.Equals(Number, other.Number);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Car)obj);
        }

        public override int GetHashCode()
        {
            return (Number != null ? Number.GetHashCode() : 0);
        }
    }

    [DataContract]
    public class OK
    {

    }
}