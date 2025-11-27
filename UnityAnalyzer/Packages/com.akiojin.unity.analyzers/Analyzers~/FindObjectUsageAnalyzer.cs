using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace StrictRules.Analyzers
{
    /// <summary>
    /// Find系メソッドの使用を検出するAnalyzer
    /// Update/FixedUpdate/LateUpdate/OnGUI内でのFind系メソッド呼び出しのみ禁止
    /// 初期化時や条件付きの一時的な呼び出しは許容
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FindObjectUsageAnalyzer : DiagnosticAnalyzer
    {
        // 検出対象の禁止メソッド名
        private static readonly string[] ForbiddenMethodNames =
        {
            // Object.FindObjectOfType系
            "FindObjectOfType",
            "FindObjectsOfType",
            "FindAnyObjectByType",
            "FindObjectsByType",
            "FindFirstObjectByType",
            // GameObject.Find系
            "Find",
            "FindWithTag",
            "FindGameObjectWithTag",
            "FindGameObjectsWithTag"
        };

        // 禁止メソッドが属するクラス
        private static readonly string[] ForbiddenMethodOwners =
        {
            "Object",
            "GameObject"
        };

        // 禁止されるメソッド（毎フレーム呼び出されるメソッド）
        private static readonly string[] ForbiddenContainingMethods =
        {
            "Update",
            "FixedUpdate",
            "LateUpdate",
            "OnGUI"
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.ForbiddenFindMethod);

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
            var methodInfo = GetMethodInfo(invocation);
            if (methodInfo == null)
            {
                return;
            }

            var (ownerName, methodName) = methodInfo.Value;

            // 禁止メソッドかどうかを確認
            if (!IsForbiddenMethod(ownerName, methodName))
            {
                return;
            }

            // 親メソッドを取得して、Update系メソッド内かどうかを確認
            var containingMethodName = GetContainingMethodName(invocation);
            if (!IsInForbiddenMethod(containingMethodName))
            {
                // Update系メソッド外での呼び出しは許容
                return;
            }

            // シンボル解析で確認（UnityEngineの型かどうか）
            var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
            var methodSymbol = symbolInfo.Symbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                return;
            }

            // UnityEngine名前空間のメソッドかどうかを確認
            var containingType = methodSymbol.ContainingType;
            if (containingType == null)
            {
                return;
            }

            var namespaceName = containingType.ContainingNamespace?.ToString();
            if (namespaceName != "UnityEngine")
            {
                return;
            }

            // 診断を報告
            string fullMethodName = $"{containingType.Name}.{methodName}";
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.ForbiddenFindMethod,
                invocation.GetLocation(),
                fullMethodName
            );

            context.ReportDiagnostic(diagnostic);
        }

        private static string GetContainingMethodName(SyntaxNode node)
        {
            var current = node.Parent;
            while (current != null)
            {
                if (current is MethodDeclarationSyntax method)
                {
                    return method.Identifier.Text;
                }
                current = current.Parent;
            }
            return null;
        }

        private static bool IsInForbiddenMethod(string methodName)
        {
            if (methodName == null)
            {
                return false;
            }
            return ForbiddenContainingMethods.Contains(methodName);
        }

        private static (string owner, string method)? GetMethodInfo(InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                string methodName = memberAccess.Name switch
                {
                    IdentifierNameSyntax id => id.Identifier.Text,
                    GenericNameSyntax generic => generic.Identifier.Text,
                    _ => null
                };

                if (methodName == null)
                {
                    return null;
                }

                // 静的呼び出し: GameObject.Find(...) or Object.FindObjectOfType(...)
                if (memberAccess.Expression is IdentifierNameSyntax ownerIdentifier)
                {
                    return (ownerIdentifier.Identifier.Text, methodName);
                }

                // インスタンス呼び出しは通常Find系では使わないが念のため
                return (null, methodName);
            }

            return null;
        }

        private static bool IsForbiddenMethod(string ownerName, string methodName)
        {
            // メソッド名が禁止リストに含まれているか確認
            bool isMethodForbidden = ForbiddenMethodNames.Any(name =>
                methodName.StartsWith(name));

            if (!isMethodForbidden)
            {
                return false;
            }

            // オーナー名が指定されていない場合、メソッド名のみで判定
            if (ownerName == null)
            {
                return true;
            }

            // オーナー名が禁止リストに含まれているか確認
            return ForbiddenMethodOwners.Contains(ownerName);
        }
    }
}
