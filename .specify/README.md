# .specify - Spec Kit Configuration

This directory contains templates, scripts, and configuration for the Spec Kit spec-driven development workflow.

## Directory Structure

```
.specify/
├── memory/         # Project constitution and governance
├── scripts/        # Automation scripts for spec workflow
└── templates/      # Templates for spec, plan, and tasks
```

## Note for Unity Analyzers

Unity Analyzersプロジェクトでは、Spec Kitの簡易版を使用しています。

完全なSpec Kit機能が必要な場合は、以下を参照してください：
- https://github.com/akiojin/unity-mcp-server/.specify

現在の実装は、Claude Codeの `/speckit.*` コマンドをサポートするための
最小限の構造です。実際のスクリプトやテンプレートは、必要に応じて
追加実装してください。

## Future Implementation

将来的に実装する可能性のある機能：
- `.specify/scripts/bash/create-new-feature.sh` - 新規機能の初期化
- `.specify/scripts/bash/setup-plan.sh` - 実装計画のセットアップ
- `.specify/scripts/bash/check-prerequisites.sh` - 前提条件チェック
- `.specify/templates/spec-template.md` - 仕様書テンプレート
- `.specify/templates/plan-template.md` - 実装計画テンプレート
- `.specify/templates/tasks-template.md` - タスクリストテンプレート
- `.specify/memory/constitution.md` - プロジェクト憲章
