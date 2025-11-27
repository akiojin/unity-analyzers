using Microsoft.CodeAnalysis;

namespace StrictRules.Analyzers
{
    /// <summary>
    /// すべての診断記述子を定義するクラス
    /// </summary>
    public static class DiagnosticDescriptors
    {
        private const string Category = "Unity.StrictRules";

        /// <summary>
        /// SR0001: Awake/Start以外でのGetComponent呼び出し
        /// </summary>
        public static readonly DiagnosticDescriptor GetComponentOutsideAwakeStart = new(
            id: "SR0001",
            title: "GetComponent Outside Awake/Start",
            messageFormat: "'{0}' is only allowed in Awake or Start. Found in '{1}'. Use cached field or DI instead.",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "GetComponent系メソッドはAwake/Startでのみ許可されています。Update等で呼び出すと毎フレームGCアロケーションが発生しパフォーマンスが著しく低下します。Awake/Startでキャッシュするか、VContainerでInjectしてください。"
        );

        /// <summary>
        /// SR0002: 禁止されたFindメソッドの使用
        /// </summary>
        public static readonly DiagnosticDescriptor ForbiddenFindMethod = new(
            id: "SR0002",
            title: "Forbidden Find Method",
            messageFormat: "'{0}' is forbidden. Use DI (VContainer) or SerializeField instead.",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Find系メソッドはシーン全体をスキャンするため非常に重く、文字列ベースの検索は保守性も低いです。DIまたはSerializeFieldで参照を解決してください。"
        );

        /// <summary>
        /// SR0003: GetComponent後のnullチェック（Fail-Fast違反）
        /// </summary>
        public static readonly DiagnosticDescriptor NullGuardAfterGetComponent = new(
            id: "SR0003",
            title: "Null Guard After GetComponent",
            messageFormat: "Null check after GetComponent violates Fail-Fast principle. Use [RequireComponent] and access directly.",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "GetComponent後のnullチェックは設計ミスを隠蔽します。必須コンポーネントが存在しない場合は即座にクラッシュさせ、[RequireComponent]属性で依存を明示してください。"
        );

        /// <summary>
        /// SR0004: DI注入後のnullチェック（Fail-Fast違反）
        /// </summary>
        public static readonly DiagnosticDescriptor NullGuardAfterDIInjection = new(
            id: "SR0004",
            title: "Null Guard After DI Injection",
            messageFormat: "Null check after DI injection ([Inject] or [SerializeField]) violates Fail-Fast principle.",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "DIコンテナは依存解決を保証します。未設定の場合は設計ミスであり、即座にクラッシュさせるべきです。フォールバック処理はバグの温床となります。"
        );
    }
}
