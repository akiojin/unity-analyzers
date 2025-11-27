using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace StrictRules.Analyzers
{
    /// <summary>
    /// Analyzer共通のヘルパーメソッド
    /// </summary>
    internal static class AnalyzerHelper
    {
        /// <summary>
        /// ソースファイルがプロジェクトコード（Assets/配下）かどうかを判定
        /// 外部パッケージ（PackageCache/, Packages/の外部パッケージ）は除外
        /// </summary>
        public static bool IsProjectCode(SyntaxNodeAnalysisContext context)
        {
            var filePath = context.Node.SyntaxTree.FilePath;
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            // パスを正規化（バックスラッシュをスラッシュに統一）
            var normalizedPath = filePath.Replace('\\', '/');

            // Assets/配下のコードは常に対象
            if (normalizedPath.Contains("/Assets/"))
            {
                return true;
            }

            // PackageCacheは除外（UPMでインストールされた外部パッケージ）
            if (normalizedPath.Contains("/PackageCache/") ||
                normalizedPath.Contains("/Library/PackageCache/"))
            {
                return false;
            }

            // Packages/配下でも、プロジェクトに埋め込まれた自作パッケージ（com.akiojin.）は対象
            // ただし、unity-analyzers自体は除外（自己参照を避けるため）
            if (normalizedPath.Contains("/Packages/"))
            {
                // com.akiojin.unity.analyzers自体は除外
                if (normalizedPath.Contains("/com.akiojin.unity.analyzers/"))
                {
                    return false;
                }

                // その他の自作パッケージは対象
                if (normalizedPath.Contains("/com.akiojin.") ||
                    normalizedPath.Contains("/com.cloud-creative-studios.") ||
                    normalizedPath.Contains("/com.grimoireengine."))
                {
                    return true;
                }

                // Unity公式パッケージ（com.unity.）やサードパーティは除外
                return false;
            }

            // その他の場所（Editor拡張など）は除外
            return false;
        }
    }
}
