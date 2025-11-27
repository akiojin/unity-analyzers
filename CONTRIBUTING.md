# Contributing to Unity Analyzers

Unity Analyzersへのコントリビューションをご検討いただき、ありがとうございます！

## 目次

- [行動規範](#行動規範)
- [ブランチ戦略](#ブランチ戦略)
- [開発環境のセットアップ](#開発環境のセットアップ)
- [コントリビューションの流れ](#コントリビューションの流れ)
- [コミットメッセージ規約](#コミットメッセージ規約)
- [プルリクエストのガイドライン](#プルリクエストのガイドライン)
- [コーディング規約](#コーディング規約)
- [テストの書き方](#テストの書き方)

## 行動規範

このプロジェクトでは、すべての参加者に対して敬意を持った対応を期待しています。建設的で友好的なコミュニティを維持するため、以下を心がけてください：

- 他者の意見を尊重する
- 建設的なフィードバックを提供する
- プロジェクトの目標に焦点を当てる
- 初心者を歓迎し、サポートする

## ブランチ戦略

Unity Analyzersプロジェクトは **2ブランチ戦略** を採用しています：

### `develop` ブランチ（デフォルトブランチ）

- **目的**: 開発中の機能を統合し、全てのCIチェックで品質を検証
- **対象**: すべての feature ブランチからのプルリクエストは `develop` へ
- **Required Checks**: 以下のすべてが必須
  - Build & Test
  - Markdown Linting
  - C# Code Formatting
  - Commit Message Lint
- **strict mode**: 有効（最新のdevelopブランチとの同期が必要）

### `main` ブランチ

- **目的**: リリース済みコードを保持
- **対象**: `develop` ブランチからのマージのみ（`/release` コマンド経由）
- **Required Checks**: なし（developで既に検証済み）
- **保護**: フォースプッシュ禁止、ブランチ削除禁止

### ワークフロー図

```
┌─────────────┐
│   feature   │  新機能開発
│   branch    │
└──────┬──────┘
       │ PR (CI checks required)
       ↓
┌─────────────┐
│   develop   │  全CIチェック必須
│   branch    │  - Build & Test
└──────┬──────┘  - Code Formatting
       │          - Commit Lint
       │          - Markdown Lint
       │ /release コマンドでマージ
       ↓
┌─────────────┐
│    main     │  リリース準備完了
│   branch    │
└──────┬──────┘
       │ release-please
       ↓
┌─────────────┐
│   Release   │  タグ作成 & パッケージ公開
│   Created   │
└─────────────┘
```

詳細は [`.github/config/README.md`](.github/config/README.md) を参照してください。

## 開発環境のセットアップ

### 必要なツール

- **.NET SDK 8.0以上**: C# Analyzerのビルドに必要
- **Unity 2021.3 LTS以上**: パッケージのテストに必要（オプション）
- **Git**: バージョン管理
- **エディタ**: Visual Studio, Visual Studio Code, または Rider

### セットアップ手順

#### オプション1: Docker環境を使用（推奨）

1. リポジトリをフォーク

2. ローカルにクローン
```bash
git clone https://github.com/<your-username>/unity-analyzers.git
cd unity-analyzers
```

3. 環境変数を設定
```bash
cp .env.example .env
# 必要に応じて.envを編集
```

4. Docker開発環境を起動
```bash
# Makefileを使用（推奨）
make dev

# またはdocker-composeを直接使用
docker-compose up -d
docker-compose exec unity-analyzers-dev bash
```

5. コンテナ内でビルド
```bash
make analyzer
```

#### オプション2: ローカル環境でビルド

1. リポジトリをフォーク

2. ローカルにクローン
```bash
git clone https://github.com/<your-username>/unity-analyzers.git
cd unity-analyzers
```

3. Analyzerプロジェクトをビルド

**Makefileを使用（推奨）:**
```bash
make analyzer
```

**手動ビルド:**
```bash
cd Analyzers~
dotnet restore
dotnet build --configuration Release
cp bin/Release/netstandard2.0/StrictRules.Analyzers.dll ../Plugins/
```

## コントリビューションの流れ

1. **Issue を作成または選択**
   - 新しい機能やバグ修正を始める前に、関連するIssueを作成または選択
   - 大きな変更の場合は、実装前にIssueで議論することを推奨

2. **ブランチを作成**

   `develop` ブランチから作業ブランチを作成してください：

```bash
# まず develop ブランチに切り替え
git checkout develop
git pull origin develop

# feature ブランチを作成
git checkout -b feature/your-feature-name
# または
git checkout -b fix/your-bug-fix
```

3. **変更を実装**
   - コードを書く
   - テストを追加する
   - ドキュメントを更新する

4. **ローカルでテスト**

**Makefileを使用:**
```bash
# ビルドとテスト
make quality-checks

# または個別に実行
make build        # ビルド
make test         # テスト
make format       # フォーマット
make lint         # リント
```

**手動実行:**
```bash
cd Analyzers~
dotnet build --configuration Release
dotnet test  # テストプロジェクトがある場合
```

5. **変更をコミット**
   - [コミットメッセージ規約](#コミットメッセージ規約)に従う
```bash
git add .
git commit -m "feat(analyzer): add new rule for Unity best practices"
```

6. **プルリクエストを作成**
   - フォークしたリポジトリにプッシュ
```bash
git push origin feature/your-feature-name
```
   - GitHubでプルリクエストを作成
   - **重要**: PRのベースブランチを `develop` に設定してください（デフォルト設定）

## コミットメッセージ規約

このプロジェクトは **Conventional Commits** を採用しています。すべてのコミットメッセージは以下の形式に従う必要があります：

### 基本形式

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Type（必須）

以下のいずれかを使用：

- `feat`: 新機能の追加
- `fix`: バグ修正
- `docs`: ドキュメントのみの変更
- `style`: コードの意味に影響しない変更（空白、フォーマット、セミコロン等）
- `refactor`: バグ修正も機能追加もしないコード変更
- `perf`: パフォーマンス改善
- `test`: テストの追加・修正
- `build`: ビルドシステムや外部依存関係に関する変更
- `ci`: CI設定ファイルやスクリプトの変更
- `chore`: その他の変更（リリース準備等）
- `revert`: 変更の取り消し

### Scope（オプション）

変更の影響範囲を示します：

- `analyzer`: Analyzerルール関連
- `editor`: Unity Editor拡張
- `runtime`: ランタイムコード
- `workflow`: GitHub Actions
- `deps`: 依存関係

### Subject（必須）

- 変更の簡潔な説明（50文字以内推奨）
- 命令形で書く（"add" であって "added" ではない）
- 最初の文字は小文字
- 末尾にピリオドを付けない

### Body（オプション）

- 変更の詳細な説明
- 「なぜ」この変更が必要かを説明
- 72文字で改行

### Footer（オプション）

- Breaking changeの場合: `BREAKING CHANGE: 説明`
- Issue参照: `Closes #123`, `Refs #456`

### 例

```
feat(analyzer): add rule to detect improper SerializeField usage

Add UNITY002 rule that detects when SerializeField is used on
public fields, which is redundant and can cause confusion.

Closes #45
```

```
fix(analyzer): resolve crash when analyzing generic methods

The analyzer was crashing when encountering generic methods with
constraints. This fix properly handles type parameters.

Fixes #78
```

```
docs: update installation instructions for UPM

Add clearer steps for installing via Unity Package Manager,
including screenshots and troubleshooting section.
```

## プルリクエストのガイドライン

### PRを作成する前に

- [ ] すべてのテストが通ることを確認
- [ ] コードが規約に従っていることを確認
- [ ] ドキュメントを更新（必要な場合）
- [ ] コミットメッセージがConventional Commitsに従っていることを確認

### PR のタイトル

PRのタイトルもConventional Commitsの形式に従ってください：

```
feat(analyzer): add new Unity lifecycle rule
fix(workflow): resolve release build failure
```

### PR の説明

以下の情報を含めてください：

1. **変更の概要**: 何を変更したか
2. **変更の理由**: なぜこの変更が必要か
3. **テスト方法**: どのようにテストしたか
4. **関連Issue**: `Closes #123`, `Refs #456`
5. **スクリーンショット**: UI変更がある場合（該当する場合）

### レビュープロセス

- メンテナーがPRをレビューします
- フィードバックに対応してください
- すべてのCIチェックが通る必要があります
- 承認後、メンテナーがマージします

### Required Checks（必須チェック）

`develop` ブランチへのすべてのプルリクエストは、以下のチェックをパスする必要があります：

1. **Build & Test** - C# Analyzerのビルドとテスト
2. **Commit Message Lint** - コミットメッセージの形式検証
3. **Markdown Linting** - ドキュメントの品質チェック
4. **C# Code Formatting** - コードフォーマットの検証

これらのチェックが失敗した場合：

- **Build & Test**: ビルドエラーまたはテストエラーを修正してください
- **Commit Message Lint**: コミットメッセージをConventional Commits形式に修正してください
- **Markdown Linting**: Markdownファイルのフォーマットエラーを修正してください
- **C# Code Formatting**: `dotnet format`を実行してコードをフォーマットしてください

**注意**: `main` ブランチへの直接のプルリクエストは受け付けていません。すべての変更は `develop` ブランチ経由で行ってください。

詳細は[Branch Protection設定](.github/BRANCH_PROTECTION.md)および[`.github/config/README.md`](.github/config/README.md)を参照してください。

## コーディング規約

### C# コーディング規約

- **命名規則**:
  - クラス名: `PascalCase`
  - メソッド名: `PascalCase`
  - プライベートフィールド: `_camelCase`（アンダースコアプレフィックス）
  - パラメータ/ローカル変数: `camelCase`

- **インデント**: スペース4つ

- **アクセス修飾子**: 常に明示的に指定

- **Using ディレクティブ**: ファイルの先頭、名前空間の外

### Analyzerルールの命名

- ルールID: `UNITY001`, `UNITY002` 等
- カテゴリ: `"Usage"`, `"Performance"`, `"Design"` 等
- 重要度: `Error`, `Warning`, または `Info`

### 例

```csharp
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace StrictRules.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MyAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "UNITY001";
        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            title: "Title of the rule",
            messageFormat: "Message format with {0}",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Detailed description of the rule");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(
                GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // Register analyzer actions
        }
    }
}
```

## テストの書き方

### テストプロジェクトの構造

テストは `Analyzers~/Tests/` ディレクトリに配置します（テストプロジェクトを作成する場合）。

### テストケースの例

```csharp
[Fact]
public async Task TestAnalyzerDetectsIssue()
{
    var test = @"
    using UnityEngine;

    public class TestClass : MonoBehaviour
    {
        [SerializeField]
        public int publicField; // Should trigger warning
    }";

    var expected = VerifyCS.Diagnostic("UNITY002")
        .WithLocation(7, 20)
        .WithArguments("publicField");

    await VerifyCS.VerifyAnalyzerAsync(test, expected);
}
```

### テストカバレッジ

新しいAnalyzerルールには、以下のテストケースを含めてください：

1. **正常ケース**: ルールに違反しないコード
2. **違反ケース**: ルールに違反するコード
3. **エッジケース**: 境界条件のテスト
4. **Code Fix**: Code Fixが提供される場合、その動作確認

## リリースプロセス

リリースは自動化されています：

1. **開発**: feature ブランチで開発し、`develop` ブランチへPR作成
2. **統合**: `develop` ブランチで全CIチェックが通過後、マージ
3. **リリース準備**: メンテナーが `/release` コマンドで `develop` → `main` へマージ
4. **自動リリース**: `main` ブランチへのマージ時、release-pleaseが自動的にリリースPRを作成
5. **公開**: リリースPRがマージされると、新しいバージョンが自動的にリリースされ、GitHubリリースとOpenUPM公開がトリガーされます

**コントリビューターの役割**: `develop` ブランチへのPRを作成するだけです。リリースプロセスはメンテナーが管理します。Conventional Commitsに従っていれば、適切にバージョンが管理されます。

## 質問やサポート

- **質問**: GitHubのDiscussionsまたはIssueで質問してください
- **バグ報告**: Issueを作成してください
- **機能リクエスト**: Issueで提案してください

## ライセンス

コントリビューションを行うことで、あなたの貢献がプロジェクトのライセンス（MIT License）の下で公開されることに同意したものとみなされます。

---

ご協力ありがとうございます！
