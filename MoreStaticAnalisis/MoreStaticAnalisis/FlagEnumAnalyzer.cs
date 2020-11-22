using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using MoreStaticAnalisis.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Utilities;

namespace MoreStaticAnalisis
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class FlagEnumAnalyzer : DiagnosticAnalyzer
	{
		public const string EnumValueIsNotPowerOfTwoId = "MSA_Enum_Flags001";
		public const string EnumValueIsZeroId = "MSA_Enum_Flags002";

		// You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
		// See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
		private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
		private const string Category = "Usage";

		private static DiagnosticDescriptor EnumValueIsNotPowerOfTwo = new DiagnosticDescriptor(EnumValueIsNotPowerOfTwoId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);
		private static DiagnosticDescriptor EnumValueIsZero = new DiagnosticDescriptor(EnumValueIsZeroId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(EnumValueIsNotPowerOfTwo, EnumValueIsZero); } }

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
			context.EnableConcurrentExecution();
			// TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
			// See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
			context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.EnumDeclaration);
		}

		private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
		{
			// TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
			var semanticModel = context.SemanticModel;

			if (context.Node is EnumDeclarationSyntax enumDeclaration)
			{
				var symbol = (ITypeSymbol)semanticModel.GetDeclaredSymbol(enumDeclaration);
				if (symbol.TypeKind == TypeKind.Enum)
				{
					var attributes = symbol.GetAttributes();
					if (attributes.Any(x => x.AttributeClass.Name == nameof(FlagsAttribute)))
					{
						var takenValues = new HashSet<int>();
						foreach (var value in enumDeclaration.Members)
						{
							var model = semanticModel.GetDeclaredSymbol(value);
							var valueAttributes = model.GetAttributes();
							if (!valueAttributes.Any(x => x.AttributeClass.Name == nameof(CombinedFlagsAttribute)))
							{
								if (EnumValuesUtility.IsPowerOfTwo((int)model.ConstantValue) && !takenValues.Contains((int)model.ConstantValue))
								{
									if (model.ConstantValue is 0 && !valueAttributes.Any(x => x.AttributeClass.Name == nameof(NoneAttribute)))
										context.ReportDiagnostic(Diagnostic.Create(EnumValueIsZero, value.GetLocation(), model.Name, nameof(NoneAttribute)));
									takenValues.Add((int)model.ConstantValue);
								}
								else
									context.ReportDiagnostic(Diagnostic.Create(EnumValueIsNotPowerOfTwo, value.GetLocation(), model.Name, nameof(CombinedFlagsAttribute)));

							}
						}
					}
				}
			}
		}
	}
}
