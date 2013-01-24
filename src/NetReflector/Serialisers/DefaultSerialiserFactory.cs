using System;
using System.Collections;
using Exortech.NetReflector.Serialisers;
using Exortech.NetReflector.Util;

namespace Exortech.NetReflector
{
	public class DefaultSerialiserFactory : ISerialiserFactory
	{
		public IXmlMemberSerialiser Create(ReflectorMember member, ReflectorPropertyAttribute attribute)
		{
			if (member.MemberType.IsArray)
			{
				return new XmlArraySerialiser(member, attribute);
			}
			if (typeof(ICollection).IsAssignableFrom(member.MemberType) || 
				(attribute.InstanceType != null && typeof(ICollection).IsAssignableFrom(attribute.InstanceType)))
			{
				return new XmlCollectionSerialiser(member, attribute);
			}
            if (member.MemberType == typeof(DateTime))
            {
                return new XmlDateTimeSerialiser(member, attribute);
            }
			return new XmlMemberSerialiser(member, attribute);
		}
	}
}