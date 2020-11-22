using System;
using System.Collections.Generic;
using System.Text;

namespace MoreStaticAnalisis.Attributes
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class NoneAttribute : Attribute
	{
	}
}
