# Unity Analyzers

Fail-Fast原則とUnity最適化ルールを強制するRoslyn Analyzerパッケージです。

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

## ライセンス

MIT License - 詳細は[LICENSE.md](LICENSE.md)を参照してください。
