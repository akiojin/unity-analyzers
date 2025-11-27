# Unity Analyzers

Fail-Fast原則とUnity最適化ルールを強制するRoslyn Analyzerパッケージです。

## プロジェクト構造

このリポジトリは以下の構成になっています：

- **UnityAnalyzer/** - Unity 6000.2.14f1プロジェクト（テスト・開発用）
  - **Packages/com.akiojin.unity.analyzers/** - 埋め込みUnityパッケージ
    - `Analyzers~/` - C# Roslyn Analyzerソースコード
    - `Plugins/` - ビルド済み Analyzer DLL
    - `Editor/` - Unity Editor拡張
    - `package.json` - パッケージ定義

Unityプロジェクトを開くと、埋め込みパッケージが自動的にインポートされます。レジストリ設定は不要です。

## 概要

このパッケージは、Unity開発における以下の重要なルールをコンパイル時に検証します：

- **Fail-Fast原則の徹底** - エラー隠蔽コードの検出
- **GetComponent/DIキャッシュの強制** - パフォーマンス劣化の防止
- **禁止メソッドの検出** - Find系メソッドの使用禁止

## インストール

### OpenUPM経由（推奨）

#### OpenUPM CLIを使用

```bash
openupm add com.akiojin.unity.analyzers
```

#### manifest.jsonにScoped Registryを設定

`Packages/manifest.json`に以下を追加：

```json
{
  "scopedRegistries": [
    {
      "name": "OpenUPM",
      "url": "https://package.openupm.com",
      "scopes": [
        "com.akiojin"
      ]
    }
  ],
  "dependencies": {
    "com.akiojin.unity.analyzers": "0.1.0"
  }
}
```

### Git URL経由

1. `Window > Package Manager`を開く
2. `+`ボタンをクリック
3. `Add package from git URL...`を選択
4. 以下のURLを入力：

```text
https://github.com/akiojin/unity-analyzers.git
```

または`Packages/manifest.json`に直接追加：

```json
{
  "dependencies": {
    "com.akiojin.unity.analyzers": "https://github.com/akiojin/unity-analyzers.git"
  }
}
```

### ローカルパッケージとして使用

`Packages/`フォルダにこのパッケージをコピーしてください。

## 検出ルール

### SR0001: GetComponent Outside Awake/Start

Awake/Start以外でのGetComponent系メソッド呼び出しを検出します。
GetComponent系メソッドはAwake/Startでのみ許可されています。

**重大度**: Warning

**検出対象**:

- `GetComponent<T>()`
- `GetComponentInChildren<T>()`
- `GetComponentInParent<T>()`
- `GetComponents<T>()`
- `GetComponentsInChildren<T>()`
- `GetComponentsInParent<T>()`

**なぜ禁止か**:

- 毎フレームのGCアロケーションが発生（60FPSで毎秒60回のガベージ生成）
- CPU負荷が10倍以上増大
- モバイル環境では致命的なフレームドロップの原因
- Awake/Startでキャッシュするか、VContainerでInjectすべき

### SR0002: Forbidden Find Methods

Update/FixedUpdate/LateUpdate/OnGUI内でのFind系メソッドの使用を検出します。
初期化時や条件付きの一時的な呼び出しは許容されます。

**重大度**: Warning

**検出対象（Update系メソッド内のみ）**:

- `FindObjectOfType<T>()`
- `FindObjectsOfType<T>()`
- `FindAnyObjectByType<T>()`
- `FindObjectsByType<T>()`
- `FindFirstObjectByType<T>()`
- `GameObject.Find()`
- `GameObject.FindWithTag()`
- `GameObject.FindGameObjectWithTag()`
- `GameObject.FindGameObjectsWithTag()`

**許容される使用例**:

- Awake/Start内での初期化時の使用
- イベントハンドラー内での一時的な使用
- エディタースクリプト内での使用

**なぜUpdate内で禁止か**:

- 毎フレームシーン全体をスキャンするため致命的なパフォーマンス低下
- 60FPSで毎秒60回の重い処理が発生
- Awake/Startでキャッシュするか、DIまたはSerializeFieldで参照を解決すべき

### SR0003: Null Guard After Required Dependency

GetComponent後のnullチェックを検出します（Fail-Fast違反）。

**重大度**: Warning

**検出対象**:

```csharp
// ❌ これは禁止
var rb = GetComponent<Rigidbody>();
if (rb != null)
{
    rb.velocity = Vector3.zero;
}

// ❌ null条件演算子も禁止
GetComponent<Rigidbody>()?.AddForce(Vector3.up);
```

**なぜ禁止か**:

- 設計ミスを隠蔽してしまう
- 必須コンポーネントが存在しない場合は即座にクラッシュさせるべき
- `[RequireComponent]`属性で依存を明示すべき

### SR0004: Null Guard After DI Injection

DI注入後のnullチェックを検出します（Fail-Fast違反）。

**重大度**: Warning

**検出対象**:

```csharp
// ❌ これは禁止
[Inject]
public IGameService GameService { get; set; }

void Start()
{
    if (GameService != null) // Fail-Fast違反！
    {
        GameService.Initialize();
    }
}

// ❌ SerializeFieldのnullチェックも禁止
[SerializeField] Transform target;

void Update()
{
    if (target != null) // Fail-Fast違反！
    {
        target.position = Vector3.zero;
    }
}
```

**なぜ禁止か**:

- DIコンテナは依存解決を保証する
- 未設定の場合は設計ミスであり、即座にクラッシュさせるべき
- フォールバック処理はバグの温床

## Unity実装ベストプラクティス

このパッケージが強制するルールの背景にある、Unity開発のベストプラクティスを解説します。

### Fail-Fast原則

**最重要：エラーを隠蔽するコードは絶対に書いてはならない**

Fail-Fast原則とは、エラーが発生した時点で即座にクラッシュさせることで、問題を早期に発見する設計哲学です。

#### 正しいコード例

```csharp
// ✅ 直接使用（存在前提）
GetComponent<Rigidbody>().velocity = Vector3.zero;

// ✅ DIは必ず成功する前提
GameService.Initialize();

// ✅ SerializeFieldは設定済み前提
target.position = Vector3.zero;

// ✅ RequireComponentで依存を明示
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody _rigidbody;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }
}
```

#### nullチェックが許可される例外ケース

以下の場合のみnullチェック・例外処理を許可：

- ネットワーク通信
- ファイルシステムアクセス
- プレイヤーの入力データ
- 外部プラグインとの連携

### GetComponent/DIキャッシュルール

**GetComponentはAwake/Startでのみ許可される**

他のメソッド（Update/FixedUpdate/LateUpdate/UniTask/コルーチン等）での使用は禁止です。

#### 正しいパターン

```csharp
public class PlayerController : MonoBehaviour
{
    // Awakeでキャッシュ
    private Rigidbody _rigidbody;
    private Animator _animator;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        // キャッシュした参照を使用
        _rigidbody.velocity = CalculateVelocity();
    }
}
```

#### VContainer（DI）を使用するパターン

```csharp
public class GameManager : MonoBehaviour
{
    [Inject]
    public IPlayerService PlayerService { get; set; }

    [Inject]
    public IScoreService ScoreService { get; set; }

    void Start()
    {
        // DIで注入された参照を直接使用
        PlayerService.Initialize();
        ScoreService.Reset();
    }
}
```

### VContainer運用ガイド

#### LifetimeScopeの配置

- 各シーンで`LifetimeScope`を1つ配置し、`Auto Run`を有効化
- ルートとなる`LifetimeScope`はプレハブ化して`VContainerSettings`に登録

#### MonoBehaviourへの依存注入

`[Inject]`だけでは注入されないため、以下のいずれかで明示的に対象を登録：

1. LifetimeScopeの`Auto Inject Game Objects`に対象を追加
2. `builder.RegisterComponentInHierarchy<T>()`を使用
3. `IObjectResolver.Instantiate`で生成

#### LifetimeScopeの無効化防止

GameObjectや`autoRun`が無効化されているとコンテナが構築されず、全Injectがnullになります。
シーン保存前にLifetimeScopeの有効状態を確認してください。

## トラブルシューティング

### Analyzerが動作しない

1. Unity 2021.3以上であることを確認
2. パッケージが正しくインストールされていることを確認
3. `Plugins/StrictRules.Analyzers.dll`の`.meta`ファイルで`RoslynAnalyzer`が有効になっていることを確認

### 誤検出がある場合

特定の行でAnalyzerを無効化するには：

```csharp
#pragma warning disable SR0001
var component = GetComponent<SomeComponent>(); // この行は警告されない
#pragma warning restore SR0001
```

ただし、この方法は本当に必要な場合のみ使用してください。

## 開発とコントリビューション

### コントリビューションガイド

このプロジェクトへのコントリビューションを歓迎します！詳細は[CONTRIBUTING.md](CONTRIBUTING.md)を参照してください。

### リリースプロセス

このプロジェクトは自動リリースフローを採用しています：

#### Conventional Commits

すべてのコミットは**Conventional Commits**形式に従う必要があります：

```
<type>(<scope>): <subject>

例:
feat(analyzer): add new Unity lifecycle rule
fix(workflow): resolve build failure
docs: update installation guide
```

主な`type`:
- `feat`: 新機能（MINORバージョンアップ）
- `fix`: バグ修正（PATCHバージョンアップ）
- `feat!` または `BREAKING CHANGE`: 破壊的変更（MAJORバージョンアップ）

詳細は[CONTRIBUTING.md](CONTRIBUTING.md#コミットメッセージ規約)を参照。

#### 自動リリース

1. **リリースPRの自動作成**
   - `main`ブランチへのマージ時、[release-please](https://github.com/googleapis/release-please)が自動的にリリースPRを作成・更新
   - コミットメッセージに基づいてバージョン番号を自動決定
   - CHANGELOG.mdを自動更新

2. **リリースの実行**
   - リリースPRがマージされると：
     - GitHubリリースとタグを自動作成
     - C# Analyzerプロジェクトを自動ビルド
     - `Plugins/StrictRules.Analyzers.dll`を自動更新
     - OpenUPMへの公開をトリガー

3. **継続的インテグレーション**
   - すべてのPRで自動ビルド・テストを実行
   - コミットメッセージ形式を自動検証
   - パッケージ構造を自動検証

### ローカル開発

#### Makefileを使用（推奨）

プロジェクトにはMakefileが用意されています：

```bash
# ヘルプを表示
make help

# Analyzerをビルド
make build

# ビルドしてPluginsにコピー
make analyzer

# コードをフォーマット
make format

# すべての品質チェックを実行
make quality-checks

# クリーンアップ
make clean
```

#### 手動ビルド

```bash
# Analyzerプロジェクトをビルド
cd Analyzers~
dotnet build --configuration Release

# ビルド済みDLLをPluginsにコピー
cp bin/Release/netstandard2.0/StrictRules.Analyzers.dll ../Plugins/
```

詳細な開発ガイドラインは[CLAUDE.md](CLAUDE.md)を参照してください。

### ワークフロー

このプロジェクトは以下のGitHub Actionsワークフローを使用しています：

#### Required Checks（必須チェック）

すべてのプルリクエストで実行される必須チェック：

- **Build** (`.github/workflows/build.yml`): C# Analyzerのビルドとテスト
- **Lint** (`.github/workflows/lint.yml`): MarkdownとC#コードのリンティング
- **Commit Lint** (`.github/workflows/commitlint.yml`): コミットメッセージ検証

#### 自動化ワークフロー

- **Release** (`.github/workflows/release.yml`): 自動リリース処理
- **OpenUPM** (`.github/workflows/openupm.yml`): OpenUPM公開

詳細は[Branch Protection設定](.github/BRANCH_PROTECTION.md)を参照してください。

### Docker開発環境

プロジェクトにはDocker開発環境が用意されています。

#### 環境変数の設定

まず、`.env`ファイルを作成します：

```bash
# .env.exampleをコピー
cp .env.example .env

# 必要に応じて.envを編集
vim .env  # または任意のエディタ
```

#### Docker Composeを使用（推奨）

```bash
# コンテナをビルドして起動
docker-compose up -d

# 開発環境のシェルに入る
docker-compose exec unity-analyzers-dev bash

# コンテナ内でビルド
make analyzer

# コンテナを停止
docker-compose down
```

#### Makefileでdocker-compose操作

```bash
# Docker開発環境を起動してシェルに入る
make dev

# Dockerイメージをビルド
make docker-build

# コンテナを起動
make docker-up

# コンテナに入る
make docker-shell

# コンテナを停止
make docker-down

# コンテナとボリュームを削除
make docker-clean
```

#### ヘルパースクリプトを使用

```bash
# スクリプトに実行権限を付与
chmod +x scripts/docker-dev.sh

# コンテナを起動して開発環境に入る
./scripts/docker-dev.sh up

# Analyzerをビルド
./scripts/docker-dev.sh analyzer

# ヘルプを表示
./scripts/docker-dev.sh help
```

#### Docker環境に含まれるツール

- .NET 8 SDK
- Node.js 20 LTS（ワークフローツール用）
- GitHub CLI
- commitlint
- その他の開発ツール

## ライセンス

MIT License - 詳細は[LICENSE.md](LICENSE.md)を参照してください。
