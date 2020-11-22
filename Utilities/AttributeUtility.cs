using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities
{
	public static class AttributeUtility
	{
		public static string GetAttributeName<T>() where T : Attribute
		{
			return typeof(T).Name.Replace("Attribute", "");
		}

		public static string GetAttributeNamespace<T>() where T : Attribute
		{
			return typeof(T).Namespace;
		}
	}
}
