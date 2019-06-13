using System;

namespace Moq.Dapper.Test
{
    public class ComplexType
    {
        public int IntegerProperty { get; set; }
        public string StringProperty { get; set; }
        public Guid GuidProperty { get; set; }
        public DateTime DateTimeProperty { get; set; }
        public DateTime? NullableDateTimeProperty { get; set; }
        public int? NullableIntegerProperty { get; set; }
        public byte[] ByteArrayPropery { get; set; }
    }
}
