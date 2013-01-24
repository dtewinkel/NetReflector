using System;
using Exortech.NetReflector.Util;

namespace Exortech.NetReflector.Serialisers
{
    public class XmlDateTimeSerialiser : XmlMemberSerialiser
    {
        public XmlDateTimeSerialiser(ReflectorMember member, ReflectorPropertyAttribute attribute)
            : base(member, attribute)
        {
        }

        protected override void WriteValue(System.Xml.XmlWriter writer, object value)
        {
            if (value is DateTime)
            {
                base.WriteValue(writer, ((DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss"));
            }
            else
            {
                base.WriteValue(writer, value);
            }
        }

        public override void Write(System.Xml.XmlWriter writer, object target)
        {
            base.Write(writer, target);
        }
    }
}