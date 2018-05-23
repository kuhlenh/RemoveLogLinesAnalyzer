using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RemoveLogLines {
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class RemoveLogLinesAnalyzer : DiagnosticAnalyzer {

		public const string DiagnosticId = "RemoveLogLines";
		private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
		private const string Category = "Clean up";

		private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
		}

		private void AnalyzeNode(SyntaxNodeAnalysisContext context)
		{
			// grab our syntax node for our invocation expression
			var invocation = (InvocationExpressionSyntax)context.Node;
			// get our semantic information from the symbol
			var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation.Expression);
			// use pattern-matching to see if we have a method symbol
			// and then grab the name of the method
			if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
			{
				if (methodSymbol.Name == "LogLine")
				{
					var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
					context.ReportDiagnostic(diagnostic);
				}

			}
		}
	}
}
