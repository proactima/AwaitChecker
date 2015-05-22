using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;

namespace AwaitChecker
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AwaitCheckerCodeFixProvider)), Shared]
    public class AwaitCheckerCodeFixProvider : CodeFixProvider
    {
        public override sealed ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(AwaitCheckerAnalyzer.DiagnosticId); }
        }

        public override sealed FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override sealed async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var invocationExpr =
                root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf()
                    .OfType<InvocationExpressionSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create("Add ConfigureAwait(false)", c => FixConfigureAwait(context.Document, invocationExpr, c)),
                diagnostic);
        }

        private async Task<Document> FixConfigureAwait(Document document, SyntaxNode invocationExpr,
            CancellationToken cancellationToken)
        {
            var oldExpression = invocationExpr.FirstAncestorOrSelf<InvocationExpressionSyntax>();

            // TODO Cheaty as fuck...
            var newLiteral = SyntaxFactory.ParseExpression(oldExpression + ".ConfigureAwait(false)");

            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var newroot = oldRoot.ReplaceNode(oldExpression, newLiteral);
            var newDocument = document.WithSyntaxRoot(newroot);
            return newDocument;
        }
    }
}