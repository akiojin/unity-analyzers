# Unity Analyzers - Specifications

このディレクトリは、Unity Analyzersプロジェクトの機能仕様を管理するためのSpec Kitワークスペースです。

## ディレクトリ構造

```
specs/
├── README.md (このファイル)
└── SPEC-XXXXXXXX/ (各機能仕様)
    ├── spec.md (機能仕様書)
    ├── plan.md (実装計画)
    ├── tasks.md (タスク分解)
    └── checklists/ (品質チェックリスト)
```

## Spec駆動開発フロー

### 1. 仕様作成 (`/speckit.specify`)

新しい機能の仕様を作成します：

```
/speckit.specify <機能説明>
```

例:
```
/speckit.specify Unity 2022以降のnullable reference types対応
```

### 2. 実装計画 (`/speckit.plan`)

仕様に基づいて実装計画を作成します：

```
/speckit.plan <技術的詳細>
```

### 3. タスク分解 (`/speckit.tasks`)

実装計画を具体的なタスクに分解します：

```
/speckit.tasks
```

### 4. 実装 (`/speckit.implement`)

タスクに従って実装を進めます：

```
/speckit.implement
```

## その他のコマンド

- `/speckit.analyze` - 仕様の一貫性チェック
- `/speckit.checklist` - 品質チェックリスト生成
- `/speckit.clarify` - 仕様の曖昧性解消

## 命名規則

各機能仕様は `SPEC-` プレフィックスと8桁のUUID形式で管理されます：

```
SPEC-a1b2c3d4/
```

## 例: 既存のAnalyzer機能

Unity Analyzersプロジェクトの既存機能は以下のように仕様化できます：

- `SPEC-sr0001/` - GetComponent Outside Awake/Start
- `SPEC-sr0002/` - Forbidden Find Methods
- `SPEC-sr0003/` - Null Guard After Required Dependency
- `SPEC-sr0004/` - Null Guard After DI Injection

## 注意事項

- Spec Kitは仕様駆動開発をサポートするツールです
- 新しいAnalyzerルールを追加する際は、必ず仕様から開始してください
- Conventional Commitsと組み合わせて使用することで、自動リリースと連携します
