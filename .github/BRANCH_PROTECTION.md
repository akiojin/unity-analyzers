# Branch Protection Settings

このドキュメントでは、Unity AnalyzersリポジトリのブランチプロテクションとRequired Checksの設定方法を説明します。

## Required Checks（必須チェック）

以下のGitHub Actionsワークフローを**Required status checks**として設定してください：

### mainブランチの保護

1. **Build & Test** (`.github/workflows/build.yml`)
   - C# Analyzerプロジェクトのビルド
   - テストの実行
   - パッケージ構造の検証

2. **Commit Message Lint** (`.github/workflows/commitlint.yml`)
   - コミットメッセージのConventional Commits準拠チェック
   - PRタイトルの検証

3. **Markdown Linting** (`.github/workflows/lint.yml`)
   - Markdownファイルの品質チェック

4. **C# Code Formatting** (`.github/workflows/lint.yml`)
   - C#コードのフォーマットチェック

## Branch Protection Rules設定手順

### 1. GitHubリポジトリ設定を開く

1. GitHubリポジトリページを開く
2. `Settings` タブをクリック
3. 左サイドバーの `Branches` をクリック

### 2. Branch protection ruleを追加

1. `Add branch protection rule` ボタンをクリック
2. 以下の設定を行う：

#### Branch name pattern
```
main
```

#### Protect matching branches

以下のオプションを有効化：

- ✅ **Require a pull request before merging**
  - ✅ Require approvals: `1`
  - ✅ Dismiss stale pull request approvals when new commits are pushed
  - ✅ Require review from Code Owners（CODEOWNERSファイルがある場合）

- ✅ **Require status checks to pass before merging**
  - ✅ Require branches to be up to date before merging
  - 以下のステータスチェックを検索して追加：
    - `Build & Test`
    - `Commit Message Lint`
    - `Markdown Linting`
    - `C# Code Formatting`

- ✅ **Require conversation resolution before merging**
  - PRコメントの解決を必須化

- ✅ **Require linear history**
  - マージコミットを禁止（Squash and merge または Rebase and merge のみ許可）

- ✅ **Do not allow bypassing the above settings**
  - 管理者も含めてルールを適用

#### Rules applied to everyone including administrators

- ✅ **Include administrators**
  - 管理者にもルールを適用

### 3. developブランチの保護（推奨）

同様の設定を `develop` ブランチにも適用することを推奨します。

#### Branch name pattern
```
develop
```

#### 設定内容

mainブランチと同じRequired Checksを設定します。

## CODEOWNERSファイル（オプション）

プロジェクトの特定部分に対してコードオーナーを設定する場合、`.github/CODEOWNERS`ファイルを作成します：

```
# Default owners for everything in the repo
* @akiojin

# Analyzer code
/Analyzers~/ @akiojin

# GitHub Actions workflows
/.github/workflows/ @akiojin

# Documentation
*.md @akiojin
```

## 設定確認方法

設定が正しく適用されているか確認するには：

1. テストブランチを作成してPRを開く
2. Required checksが自動的に実行されることを確認
3. すべてのチェックが通るまでマージボタンが無効化されていることを確認

## トラブルシューティング

### Required checksが表示されない

- ワークフローが少なくとも一度実行されている必要があります
- ブランチ名が正しいか確認してください（`main`/`develop`）
- ワークフローファイルにエラーがないか確認してください

### チェックが失敗する

各ワークフローのログを確認してください：
- `Actions` タブで失敗したワークフローをクリック
- ジョブの詳細ログで原因を特定

## 参考資料

- [GitHub Docs - Branch protection rules](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/about-protected-branches)
- [GitHub Docs - Required status checks](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/about-protected-branches#require-status-checks-before-merging)
