using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
	public class EnumValuesUtility
	{
		public static HashSet<int> GetEnumValues(EnumDeclarationSyntax enumDeclaration, SemanticModel model)
		{
			return new HashSet<int>(enumDeclaration.Members.Select(x =>
			model.GetDeclaredSymbol(x)).Cast<IFieldSymbol>().Select(x => (int)x.ConstantValue));
		}

		public static bool IsPowerOfTwo(int n)
		{
			return (n & (n - 1)) == 0;
		}

	}
}
