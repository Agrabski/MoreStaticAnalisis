using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using MoreStaticAnalisis.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace MoreStaticAnalisis
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FlagEnumCodeFixProvider)), Shared]
	public class FlagEnumCodeFixProvider : CodeFixProvider
	{
		public sealed override ImmutableArray<string> FixableDiagnosticIds
		{
			get
			{
				return ImmutableArray.Create(
			  FlagEnumAnalyzer.EnumValueIsNotPowerOfTwoId,
			  FlagEnumAnalyzer.EnumValueIsZeroId);
			}
		}

		public sealed override FixAllProvider GetFixAllProvider()
		{
			// See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
			return WellKnownFixAllProviders.BatchFixer;
		}

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			// TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
			var diagnostic = context.Diagnostics.First();
			var diagnosticSpan = diagnostic.Location.SourceSpan;

			// Find the type declaration identified by the diagnostic.
			var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<EnumDeclarationSyntax>().First();
			var model = await context.Document.GetSemanticModelAsync();
			var values = EnumValuesUtility.GetEnumValues(declaration, model);

			var offendingFieldDeclaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<EnumMemberDeclarationSyntax>().Single();
			// Register a code action that will invoke the fix.
			context.RegisterCodeFix(
				CodeAction.Create(
					title: CodeFixResources.ChangeToNextBinaryValueTitle,
					createChangedSolution: c => ChangeEnumValueAsync(context.Document, offendingFieldDeclaration, values, model, c),
					equivalenceKey: nameof(CodeFixResources.ChangeToNextBinaryValueTitle)),
				diagnostic);
			if (diagnostic.Id == FlagEnumAnalyzer.EnumValueIsZeroId)
				RegisterAppendAttributeCodefix<NoneAttribute>(
					context,
					CodeFixResources.AddNoneAttribute,
					nameof(CodeFixResources.AddNoneAttribute),
					diagnostic,
					offendingFieldDeclaration,
					model);
			else
			{
				RegisterAppendAttributeCodefix<CombinedFlagsAttribute>(
					context,
					CodeFixResources.AddCombinedFlagsAttribute,
					nameof(CodeFixResources.AddCombinedFlagsAttribute),
					diagnostic,
					offendingFieldDeclaration,
					model);
			}
		}

		private void RegisterAppendAttributeCodefix<AttributeType>(
			CodeFixContext context,
			string title,
			string equivalenceKey,
			Diagnostic diagnostic,
			EnumMemberDeclarationSyntax offendingFieldDeclaration,
			SemanticModel model) where AttributeType : Attribute
		{
			context.RegisterCodeFix(
				CodeAction.Create(
					title: title,
					createChangedSolution: c => AppendAttributeAsync(
						context.Document,
						offendingFieldDeclaration,
						AttributeUtility.GetAttributeName<AttributeType>(),
						AttributeUtility.GetAttributeNamespace<AttributeType>(),
						c),
					equivalenceKey: equivalenceKey),
			diagnostic);
		}

		private async Task<Solution> ChangeEnumValueAsync(
			Document document,
			EnumMemberDeclarationSyntax valueDeclaration,
			HashSet<int> values,
			SemanticModel model,
			CancellationToken cancellationToken)
		{

			var symbol = model.GetDeclaredSymbol(valueDeclaration);
			var newValue = Enumerable.Range(1, int.MaxValue).Where(EnumValuesUtility.IsPowerOfTwo).Where(x => !values.Contains(x)).First();

			var expression = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression);
			expression = expression.WithToken(SyntaxFactory.Literal(newValue));

			var editor = await DocumentEditor.CreateAsync(document, cancellationToken);
			editor.ReplaceNode(valueDeclaration, valueDeclaration.WithEqualsValue(SyntaxFactory.EqualsValueClause(expression)));
			return editor.GetChangedDocument().Project.Solution;
		}

		private async Task<Solution> AppendAttributeAsync(
			Document document,
			EnumMemberDeclarationSyntax valueDeclaration,
			string attributeName,
			string @namespace,
			CancellationToken cancellationToken)
		{
			var newSymbol = valueDeclaration.WithAttributeLists(
				SyntaxFactory.List(new[] 
				{
					SyntaxFactory.AttributeList(
						SyntaxFactory.SeparatedList(
							new[]
							{
								SyntaxFactory.Attribute(
									SyntaxFactory.ParseName(attributeName))
							}
						)
					)
				})
			);

			var editor = await DocumentEditor.CreateAsync(document, cancellationToken);
			var root = valueDeclaration.FirstAncestorOrSelf<CompilationUnitSyntax>();
			var newRoot= root.ReplaceNode(valueDeclaration, newSymbol);
			if (!root.Usings.Any(x => x.Name.ToFullString() == @namespace))
			{
				newRoot = newRoot.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(@namespace)));
			}
				editor.ReplaceNode(root, newRoot);
			return editor.GetChangedDocument().Project.Solution;
		}

	}
}
