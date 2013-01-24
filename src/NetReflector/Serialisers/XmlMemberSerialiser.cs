using System;
using System.Xml;
using Exortech.NetReflector.Util;

namespace Exortech.NetReflector
{
	public class XmlMemberSerialiser : IXmlMemberSerialiser
	{
	    public XmlMemberSerialiser(ReflectorMember member, ReflectorPropertyAttribute attribute)
		{
			ReflectorMember = member;
			Attribute = attribute;
			Instantiator = new DefaultInstantiator();
		}


	    public ReflectorPropertyAttribute Attribute { get; private set; }

	    public ReflectorMember ReflectorMember { get; private set; }

	    protected IInstantiator Instantiator { get; private set; }


	    public virtual void Write(XmlWriter writer, object target)
		{
			object value = ReflectorMember.GetValue(target);
			if (value != null && IsSerializableValue(value))
			{
				writer.WriteStartElement(Attribute.Name);
				if (Attribute.InstanceTypeKey != null)
				{
					ReflectorTypeAttribute typeAttribute = ReflectorTypeAttribute.GetAttribute(value);
					writer.WriteAttributeString(Attribute.InstanceTypeKey, typeAttribute.Name);
				}
				WriteValue(writer, value);
				writer.WriteEndElement();
			}
		}

		private bool IsSerializableValue(object value)
		{
			return (Attribute.InstanceTypeKey == null || ReflectorTypeAttribute.GetAttribute(value) != null);
		}

		protected virtual void WriteValue(XmlWriter writer, object value)
		{
			ReflectorTypeAttribute attribute = ReflectorTypeAttribute.GetAttribute(value);
			if (attribute == null)
			{
				writer.WriteString(value.ToString());
			}
			else
			{
				XmlTypeSerialiser serialiser = (XmlTypeSerialiser) attribute.CreateSerialiser(value.GetType());
				serialiser.WriteMembers(writer, value);
			}
		}

		public virtual object Read(XmlNode node, NetReflectorTypeTable table)
		{
			if (node == null)
			{
				CheckIfMemberIsRequired();
				return null;
			}
		    Type targetType = GetTargetType(node, table);
		    return Read(node, targetType, table);
		}

        private void CheckIfMemberIsRequired()
		{
			if (Attribute.Required)
			{
                throw new NetReflectorItemRequiredException(
                    String.Format("Missing Xml node ({0}) for required member ({1}).",
                        Attribute.Name, 
                        ReflectorMember.MemberName));
			}
		}

		private Type GetTargetType(XmlNode childNode, NetReflectorTypeTable table)
		{
            // Attempt to find the type
            XmlAttribute typeAttribute = null;
            if ((Attribute.InstanceTypeKey != null) && (childNode.Attributes != null))
            {
                typeAttribute = childNode.Attributes[Attribute.InstanceTypeKey];

                // This is a special case - the element may be an abstract element (see XSD) and needs the xsi namespace
                if ((typeAttribute == null) && (Attribute.InstanceTypeKey == "type"))
                {
                    typeAttribute = childNode.Attributes["type", "http://www.w3.org/2001/XMLSchema-instance"];
                }
            }

			if ((Attribute.InstanceTypeKey != null) &&
                (childNode.Attributes != null) &&
                (typeAttribute != null))
			{
                IXmlTypeSerialiser serialiser = table[typeAttribute.InnerText];
				if (serialiser == null)
				{
				    const string msg = @"Type with NetReflector name ""{0}"" does not exist.  The name may be incorrect or the assembly containing the type might not be loaded.
Xml: {1}";
				    throw new NetReflectorException(string.Format(msg, typeAttribute.InnerText, childNode.OuterXml));
				}
			    // HACK: no way of indicating that attribute is InstanceTypeKey. If this is removed then attribute will generate warning.
                childNode.Attributes.Remove(typeAttribute);
				return serialiser.Type;
			}
		    if (Attribute.InstanceType != null)
		    {
		        return Attribute.InstanceType;
		    }
		    return ReflectorMember.MemberType;
		}

		protected virtual object Read(XmlNode childNode, Type instanceType, NetReflectorTypeTable table)
		{
			if (ReflectionUtil.IsCommonType(instanceType))
			{
                if ((childNode.Attributes != null) && (childNode.Attributes.Count > 0))
                {
                    throw new NetReflectorException(
                        string.Format("Attributes are not allowed on {3} types - {0} attributes(s) found on '{1}'" + Environment.NewLine +
                                        "Xml: {2}",
                            childNode.Attributes.Count,
                            childNode.Name,
                            childNode.OuterXml,
                            instanceType.Name));
                }
				return childNode.InnerText;
			}

		    ReflectorTypeAttribute reflectorTypeAttribute = ReflectorTypeAttribute.GetAttribute(instanceType);
		    if (reflectorTypeAttribute == null)
		    {
		        if (!string.IsNullOrEmpty(Attribute.InstanceTypeKey))
		        {
		            throw new NetReflectorException(
		                string.Format("Unable to find reflector type for '{0}' when deserialising '{1}' - '{3}' has not been set" + Environment.NewLine +
		                              "Xml: {2}",
		                              instanceType.Name,
		                              childNode.Name,
		                              childNode.OuterXml,
		                              Attribute.InstanceTypeKey));
		        }
		        throw new NetReflectorException(
		            string.Format("Unable to find reflector type for '{0}' when deserialising '{1}'" + Environment.NewLine +
		                          "Xml: {2}",
		                          instanceType.Name,
		                          childNode.Name,
		                          childNode.OuterXml));
		    }
		    IXmlSerialiser serialiser = table[reflectorTypeAttribute.Name];
		    // null check
		    return serialiser.Read(childNode, table);
		}

		// refactor with method above???
		protected object ReadValue(XmlNode node, NetReflectorTypeTable table)
		{
			IXmlSerialiser serialiser = table[node.Name];
			if (serialiser == null)
			{
				return node.InnerText;
			}
		    // fix
		    return serialiser.Read(node, table);
		}

		public virtual void SetValue(object instance, object value)
		{
			ReflectorMember.SetValue(instance, value);
		}
	}
}