using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AwaitChecker
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AwaitCheckerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AwaitChecker";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        internal static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        internal static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        internal static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        internal const string Category = "Asynchronous";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AwaitChecker, SyntaxKind.AwaitExpression);
        }

        private static void AwaitChecker(SyntaxNodeAnalysisContext obj)
        {
            var awaitExpression = obj.Node as AwaitExpressionSyntax;

            var invocationExpr = awaitExpression?.Expression as InvocationExpressionSyntax;
            if (invocationExpr == null) return;

            var crystalMeth = invocationExpr.ToString();

            var memberAccessExpr = invocationExpr.Expression as MemberAccessExpressionSyntax;

            if (memberAccessExpr == null)
            {
                var diagnostic = Diagnostic.Create(Rule, invocationExpr.GetLocation(), crystalMeth);
                obj.ReportDiagnostic(diagnostic);
            }

        }
    }
}
