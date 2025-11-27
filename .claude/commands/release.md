# リリースコマンド（release-please 自動リリース）

Unity Analyzersプロジェクトは **release-please** を使用した自動リリースフローを採用しています。

## リリースプロセス

### 1. feature/developブランチで開発

```bash
# Conventional Commitsに従ってコミット
git commit -m "feat(analyzer): add new rule for Unity lifecycle methods"
git commit -m "fix(workflow): resolve build failure"
```

### 2. mainブランチにマージ

プルリクエストをmainブランチにマージすると、release-pleaseが自動的に：
- リリースPRを作成または更新
- コミットメッセージに基づいてバージョン番号を決定
- CHANGELOG.mdを自動更新

### 3. リリースPRをマージ

release-pleaseが作成したPRをマージすると、以下が自動実行されます：
- GitHubリリースとタグを作成
- C# Analyzerプロジェクトを自動ビルド
- `Plugins/StrictRules.Analyzers.dll`を自動更新
- OpenUPMへの公開をトリガー

## Required Checks（mainブランチ）

すべてのPRは以下のチェックをパスする必要があります：
- **Build & Test** - C# Analyzerのビルドとテスト
- **Markdown Linting** - ドキュメント品質チェック
- **C# Code Formatting** - コードフォーマット検証
- **Commit Message Lint** - Conventional Commits準拠チェック

## 進捗確認

### GitHub Actions画面で確認
```
https://github.com/akiojin/unity-analyzers/actions
```

### GitHub CLIで確認
```bash
# 認証確認
gh auth status

# ワークフロー実行状況を確認
gh run list

# 特定のワークフローを監視
gh run watch
```

## バージョニング規則

Semantic Versioningに従います：
- `feat:` → MINORバージョンアップ（例: 0.1.0 → 0.2.0）
- `fix:` → PATCHバージョンアップ（例: 0.1.0 → 0.1.1）
- `feat!:` または `BREAKING CHANGE:` → MAJORバージョンアップ（例: 0.1.0 → 1.0.0）

詳細は[CONTRIBUTING.md](../../CONTRIBUTING.md)を参照してください。
