using System;

namespace MoreStaticAnalisis.Attributes
{
	/// <summary>
	/// Marks flag enum value as being a combination of other enum values. For example:
	/// <code>
	/// enum Subjects
	///{
	///  Math = 1,
	///  Physics = 2,
	///  History = 4,
	///  [CombinedFlags]
	///  All = Math | Physics | History
	///}
	/// </code>
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class CombinedFlagsAttribute : Attribute
	{
	}
}
