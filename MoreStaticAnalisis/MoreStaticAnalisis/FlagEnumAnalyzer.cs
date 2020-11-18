using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Utilities;

namespace MoreStaticAnalisis
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class FlagEnumAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "MSA_Enum_Flags001";

		// You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
		// See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
		private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
		private const string Category = "Usage";

		private static DiagnosticDescriptor InvalidEnumValue = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(InvalidEnumValue); } }

		public override void Initialize(AnalysisContext context)
		{
			// TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
			// See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
			context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.EnumDeclaration);
		}

		private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
		{
			// TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
			var semanticModel = context.SemanticModel;

			if(context.Node is EnumDeclarationSyntax enumDeclaration)
			{
				var symbol = (ITypeSymbol)semanticModel.GetDeclaredSymbol(enumDeclaration);
				if(symbol.TypeKind == TypeKind.Enum)
				{
					var attributes = symbol.GetAttributes();
					if (attributes.Any(x => x.AttributeClass.Name == nameof(FlagsAttribute)))
					{
						var takenValues = new HashSet<int>();
						foreach (var value in enumDeclaration.Members)
						{
							var model = semanticModel.GetDeclaredSymbol(value);
							if (!model.GetAttributes().Any(x => x.AttributeClass.Name == nameof(CombinedFlagsAttribute)))
							{
								if (EnumValuesUtility.IsPowerOfTwo((int)model.ConstantValue) && !takenValues.Contains((int)model.ConstantValue))
									takenValues.Add((int)model.ConstantValue);
								else
									context.ReportDiagnostic(Diagnostic.Create(InvalidEnumValue, value.GetLocation(), model.Name));

							}
						}
					}
				}
			}
		}
	}
}
