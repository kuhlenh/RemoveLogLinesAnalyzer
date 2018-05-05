using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace RemoveLogLines
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveLogLinesCodeFixProvider)), Shared]
    public class RemoveLogLinesCodeFixProvider : CodeFixProvider
    {
        private const string title = "Remove log";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(RemoveLogLinesAnalyzer.DiagnosticId); }
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
            //var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();
            var expression = root.FindNode(diagnosticSpan);
            var expressionStatement = expression.FirstAncestorOrSelf<ExpressionStatementSyntax>();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => RemoveLogsAsync(context.Document, expressionStatement, c),//MakeUppercaseAsync(context.Document, declaration, c), 
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> RemoveLogsAsync(Document document, SyntaxNode expression, CancellationToken c)
        {
            var root = await document.GetSyntaxRootAsync();
            var newRoot = root.RemoveNode(expression, SyntaxRemoveOptions.KeepNoTrivia);
            var doc = document.WithSyntaxRoot(newRoot);
            return doc;
        }
    }
}
