.PHONY: help build test clean format lint docker-build docker-up docker-down analyzer package

# デフォルトターゲット
.DEFAULT_GOAL := help

# カラー出力
CYAN := \033[0;36m
GREEN := \033[0;32m
YELLOW := \033[1;33m
RED := \033[0;31m
NC := \033[0m

# プロジェクト変数
ANALYZER_PROJECT := Analyzers~/StrictRules.Analyzers.csproj
ANALYZER_DLL := Analyzers~/bin/Release/netstandard2.0/StrictRules.Analyzers.dll
PLUGINS_DIR := Plugins

##@ ヘルプ

help: ## ヘルプを表示
	@echo "$(CYAN)Unity Analyzers - Makefile Commands$(NC)"
	@echo ""
	@awk 'BEGIN {FS = ":.*##"; printf "使用方法:\n  make $(CYAN)<target>$(NC)\n"} /^[a-zA-Z_-]+:.*?##/ { printf "  $(CYAN)%-15s$(NC) %s\n", $$1, $$2 } /^##@/ { printf "\n$(YELLOW)%s$(NC)\n", substr($$0, 5) } ' $(MAKEFILE_LIST)

##@ ビルド

build: ## Analyzerプロジェクトをビルド
	@echo "$(GREEN)Analyzerをビルドしています...$(NC)"
	cd Analyzers~ && dotnet restore
	cd Analyzers~ && dotnet build $(ANALYZER_PROJECT) --configuration Release --no-restore
	@echo "$(GREEN)✓ ビルド完了$(NC)"

analyzer: build ## Analyzerをビルドして Plugins/ にコピー
	@echo "$(GREEN)DLLを Plugins/ にコピーしています...$(NC)"
	mkdir -p $(PLUGINS_DIR)
	cp $(ANALYZER_DLL) $(PLUGINS_DIR)/
	@echo "$(GREEN)✓ Analyzer更新完了$(NC)"

package: analyzer ## Unityパッケージをビルド
	@echo "$(GREEN)パッケージを検証しています...$(NC)"
	@if [ ! -f "package.json" ]; then \
		echo "$(RED)✗ package.json not found$(NC)"; \
		exit 1; \
	fi
	@if [ ! -f "$(PLUGINS_DIR)/StrictRules.Analyzers.dll" ]; then \
		echo "$(RED)✗ Analyzer DLL not found$(NC)"; \
		exit 1; \
	fi
	@echo "$(GREEN)✓ パッケージ検証完了$(NC)"

##@ テスト

test: ## テストを実行
	@echo "$(GREEN)テストを実行しています...$(NC)"
	@if [ -f "Analyzers~/StrictRules.Analyzers.Tests.csproj" ]; then \
		cd Analyzers~ && dotnet test --configuration Release; \
	else \
		echo "$(YELLOW)⚠ テストプロジェクトが見つかりません$(NC)"; \
	fi

##@ コード品質

format: ## C#コードをフォーマット
	@echo "$(GREEN)コードをフォーマットしています...$(NC)"
	cd Analyzers~ && dotnet format $(ANALYZER_PROJECT)
	@echo "$(GREEN)✓ フォーマット完了$(NC)"

format-check: ## フォーマットをチェック（変更なし）
	@echo "$(GREEN)フォーマットをチェックしています...$(NC)"
	cd Analyzers~ && dotnet format $(ANALYZER_PROJECT) --verify-no-changes

lint: format-check ## Lintチェックを実行
	@echo "$(GREEN)Markdownをチェックしています...$(NC)"
	@if command -v markdownlint >/dev/null 2>&1; then \
		markdownlint '**/*.md' --ignore node_modules --ignore Analyzers~; \
	else \
		echo "$(YELLOW)⚠ markdownlint-cli がインストールされていません$(NC)"; \
		echo "$(YELLOW)  インストール: npm install -g markdownlint-cli$(NC)"; \
	fi

quality-checks: format-check lint test ## すべての品質チェックを実行
	@echo "$(GREEN)✓ すべての品質チェック完了$(NC)"

##@ クリーンアップ

clean: ## ビルド成果物を削除
	@echo "$(GREEN)クリーンアップしています...$(NC)"
	rm -rf Analyzers~/bin
	rm -rf Analyzers~/obj
	@echo "$(GREEN)✓ クリーンアップ完了$(NC)"

clean-all: clean ## すべての生成ファイルを削除
	rm -rf $(PLUGINS_DIR)/*.dll
	@echo "$(GREEN)✓ 完全クリーンアップ完了$(NC)"

##@ Docker

docker-build: ## Dockerイメージをビルド
	@echo "$(GREEN)Dockerイメージをビルドしています...$(NC)"
	docker-compose build
	@echo "$(GREEN)✓ Dockerビルド完了$(NC)"

docker-up: ## Dockerコンテナを起動
	@echo "$(GREEN)Dockerコンテナを起動しています...$(NC)"
	docker-compose up -d
	@echo "$(GREEN)✓ コンテナ起動完了$(NC)"
	@echo "$(CYAN)開発環境に入るには: make docker-shell$(NC)"

docker-down: ## Dockerコンテナを停止
	@echo "$(GREEN)Dockerコンテナを停止しています...$(NC)"
	docker-compose down
	@echo "$(GREEN)✓ コンテナ停止完了$(NC)"

docker-shell: ## Dockerコンテナに入る
	docker-compose exec unity-analyzers-dev bash

docker-clean: ## Dockerコンテナとボリュームを削除
	@echo "$(RED)Dockerコンテナとボリュームを削除します$(NC)"
	docker-compose down -v
	@echo "$(GREEN)✓ Docker削除完了$(NC)"

docker-rebuild: ## Dockerイメージをクリーンビルド
	@echo "$(GREEN)Dockerイメージをクリーンビルドしています...$(NC)"
	docker-compose build --no-cache
	@echo "$(GREEN)✓ Dockerクリーンビルド完了$(NC)"

##@ 開発ワークフロー

dev: docker-up docker-shell ## Docker開発環境を起動してシェルに入る

watch: ## ファイル変更を監視して自動ビルド（要fswatch）
	@echo "$(GREEN)ファイル変更を監視しています...$(NC)"
	@echo "$(YELLOW)Ctrl+C で終了$(NC)"
	@if command -v fswatch >/dev/null 2>&1; then \
		fswatch -o Analyzers~ | xargs -n1 -I{} make analyzer; \
	else \
		echo "$(RED)✗ fswatch がインストールされていません$(NC)"; \
		echo "$(YELLOW)  macOS: brew install fswatch$(NC)"; \
		echo "$(YELLOW)  Linux: apt-get install fswatch$(NC)"; \
		exit 1; \
	fi

##@ リリース

release-check: quality-checks ## リリース前チェック
	@echo "$(GREEN)リリース準備をチェックしています...$(NC)"
	@echo "$(CYAN)コミットメッセージがConventional Commitsに従っているか確認してください$(NC)"
	@git log --oneline -10
	@echo "$(GREEN)✓ リリースチェック完了$(NC)"
