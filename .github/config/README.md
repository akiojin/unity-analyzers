# GitHub Repository Configuration

このディレクトリは、GitHubリポジトリの設定を**コードとして管理**するための設定ファイルを含んでいます。

## セットアップ

[`gh-repo-config`](https://github.com/twelvelabs/gh-repo-config) GitHub CLI拡張を使用して、これらの設定をリポジトリに適用できます：

```bash
# 拡張機能のインストール
gh extension install twelvelabs/gh-repo-config

# 設定の適用
gh repo-config apply
```

## ディレクトリ構造

```
.github/config/
├── README.md                     # このファイル
├── repo.json                     # リポジトリ基本設定
└── branch-protection/            # ブランチ保護ルール
    ├── main.json                 # mainブランチの保護設定
    └── develop.json              # developブランチの保護設定
```

## Unity Analyzers のブランチ戦略

### ブランチ構成

Unity Analyzersプロジェクトでは、**2ブランチ戦略**を採用しています：

#### `develop` ブランチ（デフォルトブランチ）

- **目的**: 開発中の機能を統合し、全てのCIチェックで品質を検証
- **Required Checks**: 以下の全てのチェックが必須
  - Build & Test
  - Markdown Linting
  - C# Code Formatting
  - Commit Message Lint
- **マージ条件**: 全CIチェック通過 + PR承認
- **strict mode**: 有効（最新のdevelopブランチとの同期が必要）

#### `main` ブランチ

- **目的**: リリース済みコードを保持
- **Required Checks**: なし（developで既に検証済み）
- **マージ条件**: PR必須（レビューは任意）
- **保護**: フォースプッシュ禁止、ブランチ削除禁止

### ワークフロー

```
┌─────────────┐
│   feature   │  新機能開発
│   branch    │
└──────┬──────┘
       │ PR
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

### リリースフロー

1. **開発**: `feature/*` ブランチで機能を開発
2. **統合**: `develop` ブランチへPR → 全CIチェック通過後マージ
3. **リリース準備**: `/release` コマンドで `develop` → `main` へマージ
4. **自動リリース**: release-pleaseが変更を検出してリリースPRを作成
5. **パッケージ配布**: リリースPRマージ時に自動でタグ作成 & GitHub Release公開

## 設定ファイルの詳細

### `repo.json`

リポジトリの基本設定を定義：

- リポジトリ名、説明
- 可視性（public/private）
- Issues、Wiki、Discussionsの有効/無効
- デフォルトブランチ
- マージ方法の許可設定
- ブランチ自動削除

### `branch-protection/*.json`

各ブランチの保護ルールを定義：

- Required status checks（必須CIチェック）
- PR承認要件
- フォースプッシュ/削除の可否
- Linear historyの強制
- Conversation解決の必須化

## 設定の適用

```bash
# 設定内容を確認
gh repo-config validate

# 現在の設定との差分を表示
gh repo-config diff

# 設定を適用
gh repo-config apply

# 特定のブランチ保護ルールのみ適用
gh repo-config apply --only branch-protection
```

## トラブルシューティング

### Required Checksが表示されない

GitHub ActionsワークフローがPRに対して少なくとも1回実行される必要があります。
初回は手動でPRを作成してワークフローを実行してください。

### `gh-repo-config` が見つからない

```bash
# GitHub CLI拡張の一覧を確認
gh extension list

# 再インストール
gh extension install twelvelabs/gh-repo-config
```

## 参考資料

- [gh-repo-config GitHub Repository](https://github.com/twelvelabs/gh-repo-config)
- [GitHub Branch Protection Documentation](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/about-protected-branches)
- [Unity Analyzers CONTRIBUTING.md](../../CONTRIBUTING.md)
