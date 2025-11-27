# Unity Analyzers 開発ガイドライン

このドキュメントは、Unity Analyzersリポジトリでの開発に関する包括的なガイダンスを提供します。

## 開発優先順位

現在の優先事項：
1. コアアナライザールールの実装と改善
2. テストカバレッジの向上
3. ドキュメントの充実
4. リリースプロセスの自動化

## Spec駆動開発（Spec-Driven Development）

このプロジェクトは **Spec Kit** を使用した仕様駆動開発をサポートしています。

### Spec Kit ワークフロー

新機能の開発は以下の順序で進めます：

1. `/speckit.specify` - 機能仕様を作成
2. `/speckit.clarify` - 曖昧性を解消（オプション）
3. `/speckit.plan` - 実装計画を作成
4. `/speckit.tasks` - タスクに分解
5. `/speckit.implement` - 実装を実行

### 使用例

```
# 新しいAnalyzerルールの仕様を作成
/speckit.specify Unity 2022以降のnullable reference types対応Analyzerを追加

# 実装計画を作成
/speckit.plan Roslyn Analyzer APIを使用してC# 8.0のnullable context awarを検証

# タスクに分解
/speckit.tasks

# 実装を実行
/speckit.implement
```

詳細は[specs/README.md](specs/README.md)を参照してください。

## 実装哲学

> 設計・実装は複雑にせずに、シンプルさの極限を追求してください

ただし、以下の点は決して妥協しません：
- **ユーザビリティ**: Unity開発者が簡単に導入・使用できること
- **開発者体験**: コントリビューターが容易に参加できること
- **保守性**: コードの可読性と保守性の維持
- **品質**: 高品質なアナライザールールの提供

## プロジェクト構造

```
unity-analyzers/
├── Analyzers~/                    # C# Roslyn Analyzer ソースコード
│   └── StrictRules.Analyzers.csproj
├── Plugins/                       # ビルド済み DLL
│   └── StrictRules.Analyzers.dll
├── Editor/                        # Unity Editor 拡張
├── Runtime/                       # ランタイムコード（該当する場合）
├── package.json                   # Unity パッケージ定義
└── CHANGELOG.md                   # 変更履歴
```

## 品質保証

### Commit メッセージ規約

このプロジェクトでは **Conventional Commits** を採用しています。すべてのコミットは以下の形式に従う必要があります：

```
<type>(<scope>): <subject>

<body>

<footer>
```

**許可される type:**
- `feat`: 新機能
- `fix`: バグ修正
- `docs`: ドキュメントのみの変更
- `style`: コードの意味に影響しない変更（空白、フォーマット等）
- `refactor`: バグ修正も機能追加もしないコード変更
- `perf`: パフォーマンス改善
- `test`: テストの追加・修正
- `build`: ビルドシステムや外部依存関係に関する変更
- `ci`: CI設定ファイルやスクリプトの変更
- `chore`: その他の変更
- `revert`: 変更の取り消し

**例:**
```
feat(analyzer): add new rule for Unity lifecycle methods

Add UNITY001 rule to enforce proper Unity lifecycle method usage.

Closes #123
```

### コミットメッセージの検証

すべてのプルリクエストは自動的にコミットメッセージが検証されます：
- commitlint によるフォーマットチェック
- PRタイトルもConventional Commits形式に従う必要があります

### ビルド検証

すべてのプルリクエストで以下が自動実行されます：
- C# Analyzer プロジェクトのビルド
- テストの実行（テストが存在する場合）
- パッケージ構造の検証

## 開発ワークフロー

### 1. ローカル開発

```bash
# Analyzer プロジェクトのビルド
cd Analyzers~
dotnet build --configuration Release

# ビルド済み DLL を Plugins にコピー
cp bin/Release/netstandard2.0/StrictRules.Analyzers.dll ../Plugins/
```

### 2. 新機能・修正の追加

1. 適切なブランチを作成（例: `feature/add-new-rule`, `fix/analyzer-crash`）
2. Conventional Commits に従ってコミット
3. プルリクエストを作成
4. 自動チェックが通過することを確認
5. レビューを受ける
6. `main` ブランチにマージ

### 3. リリースプロセス

このプロジェクトは **release-please** を使用した自動リリースを採用しています：

1. `main` ブランチへのマージ時、release-pleaseが自動的にリリースPRを作成/更新
2. リリースPRには以下が含まれます：
   - バージョン番号の自動更新（Conventional Commitsに基づく）
   - CHANGELOG.md の自動更新
3. リリースPRをマージすると：
   - GitHubリリースとタグが自動作成
   - C# Analyzer が自動ビルド
   - ビルド済みDLLが更新
   - OpenUPMへの公開がトリガー（設定済みの場合）

### バージョニング規則

Semantic Versioningに従います：
- `feat`: MINOR バージョンアップ（例: 0.1.0 → 0.2.0）
- `fix`: PATCH バージョンアップ（例: 0.1.0 → 0.1.1）
- `feat!` または `BREAKING CHANGE`: MAJOR バージョンアップ（例: 0.1.0 → 1.0.0）

## コーディング規約

### C# アナライザーコード

- .NET Standard 2.0 互換性を維持
- Roslyn APIのベストプラクティスに従う
- すべてのルールに適切なドキュメントを提供
- テストケースを含める

### Unity パッケージ

- Unity 2021.3 LTS以降をサポート
- UPM（Unity Package Manager）標準に準拠
- 必要なmetaファイルを含める

## テスト

新しいアナライザールールには、以下のテストを含める必要があります：
- 正常なコードでルールが発火しないことを確認
- 問題のあるコードでルールが正しく発火することを確認
- Code Fixが提供される場合、正しく適用されることを確認

## ドキュメント

以下のドキュメントを最新に保つ必要があります：
- `README.md`: プロジェクトの概要、インストール方法、使用方法
- `CHANGELOG.md`: 自動更新されますが、手動で編集が必要な場合もあります
- `CONTRIBUTING.md`: コントリビューションガイド
- アナライザールールごとのドキュメント

## 制約事項

- **ブランチ作成**: 機能ブランチは自由に作成可能ですが、`main`ブランチを保護します
- **直接プッシュ禁止**: `main`ブランチへの直接プッシュは禁止され、すべてプルリクエスト経由
- **テストなしのマージ禁止**: ビルドとテストが通過しない限りマージ不可

## サポート

質問や問題がある場合：
1. まず既存のIssueを確認
2. 該当するIssueがない場合、新しいIssueを作成
3. コントリビューションについては CONTRIBUTING.md を参照

## ライセンス

このプロジェクトは MIT ライセンスの下で公開されています。詳細は LICENSE.md を参照してください。
