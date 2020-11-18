using System;
using System.Collections.Generic;
using System.Text;

namespace MoreStaticAnalisis
{
	[AttributeUsage(AttributeTargets.Enum)]
	public sealed class CombinedFlagsAttribute : Attribute
	{
		public CombinedFlagsAttribute(params Enum[] values)
		{
			Values = values;
		}

		public Enum[] Values { get; }
	}
}
