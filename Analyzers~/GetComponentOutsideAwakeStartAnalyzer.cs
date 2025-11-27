using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace StrictRules.Analyzers
{
    /// <summary>
    /// Awake/Start以外でのGetComponent呼び出しを検出するAnalyzer
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class GetComponentOutsideAwakeStartAnalyzer : DiagnosticAnalyzer
    {
        // GetComponentが許可されるメソッド名
        private static readonly string[] AllowedMethodNames =
        {
            "Awake",
            "Start"
        };

        // 検出対象のGetComponent系メソッド名
        private static readonly string[] GetComponentMethodNames =
        {
            "GetComponent",
            "GetComponents",
            "GetComponentInChildren",
            "GetComponentsInChildren",
            "GetComponentInParent",
            "GetComponentsInParent"
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.GetComponentOutsideAwakeStart);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            // 外部パッケージは除外
            if (!AnalyzerHelper.IsProjectCode(context))
            {
                return;
            }

            var invocation = (InvocationExpressionSyntax)context.Node;

            // メソッド名を取得
            string methodName = GetMethodName(invocation);
            if (methodName == null)
            {
                return;
            }

            // GetComponent系メソッドかどうかを確認
            if (!IsGetComponentMethod(methodName))
            {
                return;
            }

            // 親メソッドを取得
            var containingMethod = invocation.Ancestors()
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault();

            if (containingMethod == null)
            {
                return;
            }

            string containingMethodName = containingMethod.Identifier.Text;

            // Awake/Startの中であれば許可
            if (IsAllowedMethod(containingMethodName))
            {
                return;
            }

            // MonoBehaviourを継承しているクラス内かどうかを確認
            var containingClass = containingMethod.Ancestors()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault();

            if (containingClass == null)
            {
                return;
            }

            // 基底クラスの確認（シンボル解析）
            var classSymbol = context.SemanticModel.GetDeclaredSymbol(containingClass);
            if (classSymbol == null || !InheritsFromMonoBehaviour(classSymbol))
            {
                return;
            }

            // 診断を報告
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.GetComponentOutsideAwakeStart,
                invocation.GetLocation(),
                methodName,
                containingMethodName
            );

            context.ReportDiagnostic(diagnostic);
        }

        private static string GetMethodName(InvocationExpressionSyntax invocation)
        {
            return invocation.Expression switch
            {
                MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,
                IdentifierNameSyntax identifier => identifier.Identifier.Text,
                GenericNameSyntax genericName => genericName.Identifier.Text,
                MemberBindingExpressionSyntax memberBinding => memberBinding.Name.Identifier.Text,
                _ => null
            };
        }

        private static bool IsGetComponentMethod(string methodName)
        {
            return GetComponentMethodNames.Any(name =>
                methodName.StartsWith(name));
        }

        private static bool IsAllowedMethod(string methodName)
        {
            return AllowedMethodNames.Contains(methodName);
        }

        private static bool InheritsFromMonoBehaviour(INamedTypeSymbol classSymbol)
        {
            var currentType = classSymbol;
            while (currentType != null)
            {
                if (currentType.Name == "MonoBehaviour" &&
                    currentType.ContainingNamespace?.ToString() == "UnityEngine")
                {
                    return true;
                }
                currentType = currentType.BaseType;
            }
            return false;
        }
    }
}
