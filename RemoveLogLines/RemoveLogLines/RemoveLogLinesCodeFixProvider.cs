using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RemoveLogLines {
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveLogLinesCodeFixProvider)), Shared]
	public class RemoveLogLinesCodeFixProvider : CodeFixProvider {
		
		private const string title = "Remove log";

		public sealed override ImmutableArray<string> FixableDiagnosticIds {
			get { return ImmutableArray.Create(RemoveLogLinesAnalyzer.DiagnosticId); }
		}

		public sealed override FixAllProvider GetFixAllProvider() {
			return WellKnownFixAllProviders.BatchFixer;
		}

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			var diagnostic = context.Diagnostics.First();
			var diagnosticSpan = diagnostic.Location.SourceSpan;

			var expression = root.FindNode(diagnosticSpan);
			var expressionStatement = expression.FirstAncestorOrSelf<ExpressionStatementSyntax>();

			// Register a code action that will invoke the fix.
			context.RegisterCodeFix(
				CodeAction.Create(
					title: title,
					createChangedDocument: c => RemoveLogsAsync(context.Document, expressionStatement, c),
					equivalenceKey: title),
				diagnostic);
		}

		private async Task<Document> RemoveLogsAsync(Document document, SyntaxNode expression, CancellationToken c) {
			var root = await document.GetSyntaxRootAsync();
			var newRoot = root.RemoveNode(expression, SyntaxRemoveOptions.KeepNoTrivia);
			var doc = document.WithSyntaxRoot(newRoot);
			return doc;
		}
	}
}
