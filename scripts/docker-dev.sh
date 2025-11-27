#!/usr/bin/env bash
# Unity Analyzers Docker Development Helper Script

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

# カラー出力
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# ヘルプメッセージ
show_help() {
    cat << EOF
Unity Analyzers Docker Development Helper

使用方法:
    $0 [コマンド]

コマンド:
    build       Dockerイメージをビルド
    up          コンテナを起動して開発環境に入る
    down        コンテナを停止
    shell       実行中のコンテナに入る
    clean       コンテナとボリュームを削除
    logs        コンテナのログを表示
    rebuild     クリーンビルド（キャッシュなし）

    analyzer    Analyzerプロジェクトをビルド
    format      C#コードをフォーマット
    test        テストを実行（存在する場合）

    help        このヘルプを表示

例:
    $0 up                # コンテナを起動して開発環境に入る
    $0 analyzer          # Analyzerをビルド
    $0 format            # コードをフォーマット

EOF
}

# エラーメッセージ
error() {
    echo -e "${RED}エラー: $1${NC}" >&2
    exit 1
}

# 成功メッセージ
success() {
    echo -e "${GREEN}✓ $1${NC}"
}

# 警告メッセージ
warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

# プロジェクトルートに移動
cd "${PROJECT_ROOT}"

# コマンド処理
case "${1:-help}" in
    build)
        echo "Dockerイメージをビルドしています..."
        docker-compose build
        success "ビルド完了"
        ;;

    up)
        echo "コンテナを起動しています..."
        docker-compose up -d
        success "コンテナを起動しました"
        echo "開発環境に入ります..."
        docker-compose exec unity-analyzers-dev bash
        ;;

    down)
        echo "コンテナを停止しています..."
        docker-compose down
        success "コンテナを停止しました"
        ;;

    shell)
        docker-compose exec unity-analyzers-dev bash
        ;;

    clean)
        warning "コンテナとボリュームを削除します"
        read -p "続行しますか? (y/N): " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            docker-compose down -v
            success "クリーンアップ完了"
        else
            echo "キャンセルしました"
        fi
        ;;

    logs)
        docker-compose logs -f
        ;;

    rebuild)
        echo "クリーンビルドを実行しています..."
        docker-compose build --no-cache
        success "クリーンビルド完了"
        ;;

    analyzer)
        echo "Analyzerプロジェクトをビルドしています..."
        docker-compose exec unity-analyzers-dev bash -c "
            cd Analyzers~ && \
            dotnet restore && \
            dotnet build --configuration Release && \
            cp bin/Release/netstandard2.0/StrictRules.Analyzers.dll ../Plugins/
        "
        success "Analyzerのビルド完了"
        ;;

    format)
        echo "C#コードをフォーマットしています..."
        docker-compose exec unity-analyzers-dev bash -c "
            cd Analyzers~ && \
            dotnet format StrictRules.Analyzers.csproj
        "
        success "フォーマット完了"
        ;;

    test)
        echo "テストを実行しています..."
        docker-compose exec unity-analyzers-dev bash -c "
            cd Analyzers~ && \
            if [ -f StrictRules.Analyzers.Tests.csproj ]; then
                dotnet test StrictRules.Analyzers.Tests.csproj --configuration Release
            else
                echo 'テストプロジェクトが見つかりません'
                exit 1
            fi
        "
        ;;

    help|--help|-h)
        show_help
        ;;

    *)
        error "不明なコマンド: $1\n\n$(show_help)"
        ;;
esac
