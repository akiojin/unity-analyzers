using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace StrictRules.Analyzers
{
    /// <summary>
    /// Fail-Fast原則違反を検出するAnalyzer
    /// GetComponent後、DI注入後のnullチェックを検出
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FailFastViolationAnalyzer : DiagnosticAnalyzer
    {
        // GetComponent系メソッド名
        private static readonly string[] GetComponentMethodNames =
        {
            "GetComponent",
            "GetComponents",
            "GetComponentInChildren",
            "GetComponentsInChildren",
            "GetComponentInParent",
            "GetComponentsInParent"
        };

        // DI/SerializeField属性名
        private static readonly string[] DIAttributeNames =
        {
            "Inject",
            "SerializeField"
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(
                DiagnosticDescriptors.NullGuardAfterGetComponent,
                DiagnosticDescriptors.NullGuardAfterDIInjection
            );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // nullチェックのif文を検出
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);

            // null条件演算子を検出
            context.RegisterSyntaxNodeAction(AnalyzeConditionalAccess, SyntaxKind.ConditionalAccessExpression);
        }

        private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            // 外部パッケージは除外
            if (!AnalyzerHelper.IsProjectCode(context))
            {
                return;
            }

            var ifStatement = (IfStatementSyntax)context.Node;
            var condition = ifStatement.Condition;

            // null比較パターンを検出: if (x != null) または if (x == null)
            if (condition is BinaryExpressionSyntax binaryExpr)
            {
                if (binaryExpr.Kind() == SyntaxKind.NotEqualsExpression ||
                    binaryExpr.Kind() == SyntaxKind.EqualsExpression)
                {
                    // 左辺または右辺がnullリテラルかどうか確認
                    bool leftIsNull = binaryExpr.Left is LiteralExpressionSyntax leftLit &&
                                      leftLit.IsKind(SyntaxKind.NullLiteralExpression);
                    bool rightIsNull = binaryExpr.Right is LiteralExpressionSyntax rightLit &&
                                       rightLit.IsKind(SyntaxKind.NullLiteralExpression);

                    if (leftIsNull || rightIsNull)
                    {
                        var checkedExpr = leftIsNull ? binaryExpr.Right : binaryExpr.Left;
                        CheckNullGuardViolation(context, checkedExpr, ifStatement.GetLocation());
                    }
                }
            }
            // is null または is not null パターンを検出
            else if (condition is IsPatternExpressionSyntax isPattern)
            {
                if (isPattern.Pattern is ConstantPatternSyntax constantPattern &&
                    constantPattern.Expression is LiteralExpressionSyntax literal &&
                    literal.IsKind(SyntaxKind.NullLiteralExpression))
                {
                    CheckNullGuardViolation(context, isPattern.Expression, ifStatement.GetLocation());
                }
                else if (isPattern.Pattern is UnaryPatternSyntax unaryPattern &&
                         unaryPattern.Pattern is ConstantPatternSyntax negatedConstant &&
                         negatedConstant.Expression is LiteralExpressionSyntax negatedLiteral &&
                         negatedLiteral.IsKind(SyntaxKind.NullLiteralExpression))
                {
                    CheckNullGuardViolation(context, isPattern.Expression, ifStatement.GetLocation());
                }
            }
        }

        private static void AnalyzeConditionalAccess(SyntaxNodeAnalysisContext context)
        {
            // 外部パッケージは除外
            if (!AnalyzerHelper.IsProjectCode(context))
            {
                return;
            }

            var conditionalAccess = (ConditionalAccessExpressionSyntax)context.Node;
            var expression = conditionalAccess.Expression;

            // GetComponent系の後のnull条件演算子を検出
            if (expression is InvocationExpressionSyntax invocation)
            {
                var methodName = GetMethodName(invocation);
                if (methodName != null && IsGetComponentMethod(methodName))
                {
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.NullGuardAfterGetComponent,
                        conditionalAccess.GetLocation()
                    );
                    context.ReportDiagnostic(diagnostic);
                }
            }
            // DI/SerializeFieldフィールドへのnull条件演算子を検出
            else if (expression is IdentifierNameSyntax identifier)
            {
                CheckDIFieldNullGuard(context, identifier, conditionalAccess.GetLocation());
            }
        }

        private static void CheckNullGuardViolation(
            SyntaxNodeAnalysisContext context,
            ExpressionSyntax checkedExpr,
            Location location)
        {
            // GetComponent呼び出しの結果に対するnullチェックを検出
            if (checkedExpr is InvocationExpressionSyntax invocation)
            {
                var methodName = GetMethodName(invocation);
                if (methodName != null && IsGetComponentMethod(methodName))
                {
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.NullGuardAfterGetComponent,
                        location
                    );
                    context.ReportDiagnostic(diagnostic);
                    return;
                }
            }

            // 変数に対するnullチェックを検出（DI/SerializeField）
            if (checkedExpr is IdentifierNameSyntax identifier)
            {
                CheckDIFieldNullGuard(context, identifier, location);
            }
        }

        private static void CheckDIFieldNullGuard(
            SyntaxNodeAnalysisContext context,
            IdentifierNameSyntax identifier,
            Location location)
        {
            var symbolInfo = context.SemanticModel.GetSymbolInfo(identifier);
            var symbol = symbolInfo.Symbol;

            if (symbol == null)
            {
                return;
            }

            // フィールドまたはプロパティのシンボルを取得
            ImmutableArray<AttributeData> attributes;
            if (symbol is IFieldSymbol fieldSymbol)
            {
                attributes = fieldSymbol.GetAttributes();
            }
            else if (symbol is IPropertySymbol propertySymbol)
            {
                attributes = propertySymbol.GetAttributes();
            }
            else
            {
                return;
            }

            // DI/SerializeField属性が付いているかどうかを確認
            bool hasDIAttribute = attributes.Any(attr =>
                DIAttributeNames.Contains(attr.AttributeClass?.Name) ||
                DIAttributeNames.Any(name => attr.AttributeClass?.Name == name + "Attribute")
            );

            if (hasDIAttribute)
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.NullGuardAfterDIInjection,
                    location
                );
                context.ReportDiagnostic(diagnostic);
            }
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
    }
}
