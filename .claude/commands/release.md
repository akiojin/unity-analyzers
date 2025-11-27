# リリースコマンド（release-please 自動リリース）

Unity Analyzersプロジェクトは **release-please** と **2-branch戦略** による完全自動リリースフローを採用しています。

## リリースコマンドの実行

Claude Codeで以下のコマンドを実行してください：

```bash
/release
```

このコマンドは `prepare-release.yml` ワークフローをトリガーし、以下のプロセスを自動化します。

## 自動リリースフロー

### ステップ1：リリースPRの自動作成

`/release` コマンドを実行すると：
- `develop` → `main` へのPRが自動作成される
- PRタイトル：`chore(release): merge develop to main`
- 重複PRがある場合は既存のPRを通知

### ステップ2：Auto-Mergeの自動設定

`auto-merge.yml` ワークフローが自動的に：
- PRにauto-mergeを設定
- CIチェック（Build & Test、Commit Message Lint）が通過すると自動マージ
- ドラフトPRは除外

### ステップ3：release-pleaseの自動実行

`main` ブランチへのマージが完了すると、`release.yml` が自動的に：
- コミットメッセージを分析してバージョン番号を決定
- CHANGELOG.mdを自動更新
- GitHubリリースとタグを作成
- C# Analyzerプロジェクトを自動ビルド
- `Plugins/StrictRules.Analyzers.dll`を自動更新
- OpenUPMへの公開をトリガー

## ブランチ戦略

Unity Analyzersは **2-branch戦略** を採用：

```
[feature branch]
      ↓ PR
[develop branch] ← デフォルトブランチ（開発・CI検証）
      ↓ /release コマンド + auto-merge
[main branch] ← プロダクションブランチ（リリース専用）
```

### developブランチ（デフォルトブランチ）

- すべての開発作業はここで行われる
- Required Checks：
  - Build & Test
  - Commit Message Lint
- featureブランチからのPRは `develop` へマージ

### mainブランチ（リリース専用）

- `develop` からのマージのみ受け付ける
- マージ時に自動的にリリースプロセスが実行される
- 直接の開発作業は禁止

## 日常的な開発フロー

### 1. 機能開発

```bash
# featureブランチで開発
git checkout -b feature/new-analyzer
git commit -m "feat(analyzer): add new Unity optimization rule"
```

### 2. developへマージ

```bash
# PRを作成してdevelopにマージ
gh pr create --base develop --title "feat: add new optimization rule"
```

### 3. リリース実行（開発が完了したら）

```bash
# Claude Codeで実行
/release
```

**これだけでリリースまで完全自動化されます！**

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

# リリースPRの状態を確認
gh pr list --base main
```

## バージョニング規則

Semantic Versioningに従います：

- `feat:` → MINORバージョンアップ（例: 0.1.0 → 0.2.0）
- `fix:` → PATCHバージョンアップ（例: 0.1.0 → 0.1.1）
- `feat!:` または `BREAKING CHANGE:` → MAJORバージョンアップ（例: 0.1.0 → 1.0.0）

## トラブルシューティング

### CIチェックが失敗する

```bash
# ビルドとテストをローカルで実行
make build
make test

# コードフォーマットを修正
make format
```

### auto-mergeを無効化したい

```bash
# PRのauto-mergeを無効化
gh pr merge <PR番号> --disable-auto
```

### リリースPRがマージされない

- ドラフトPRになっていないか確認
- すべてのRequired Checksが通過しているか確認
- `gh pr view <PR番号> --json statusCheckRollup` で詳細確認

詳細は[CONTRIBUTING.md](../../CONTRIBUTING.md)を参照してください。
