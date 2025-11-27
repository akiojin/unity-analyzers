# Unity Analyzers Development Container
# Multi-stage build for .NET C# Roslyn Analyzer development

FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm AS base

# Install essential system utilities
RUN apt-get update && apt-get install -y \
    jq \
    ripgrep \
    curl \
    dos2unix \
    ca-certificates \
    gnupg \
    vim \
    git \
    && rm -rf /var/lib/apt/lists/*

# Install GitHub CLI
RUN curl -fsSL https://cli.github.com/packages/githubcli-archive-keyring.gpg | \
    dd of=/usr/share/keyrings/githubcli-archive-keyring.gpg && \
    chmod go+r /usr/share/keyrings/githubcli-archive-keyring.gpg && \
    echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/githubcli-archive-keyring.gpg] https://cli.github.com/packages stable main" | \
    tee /etc/apt/sources.list.d/github-cli.list > /dev/null && \
    apt-get update && \
    apt-get install -y gh && \
    rm -rf /var/lib/apt/lists/*

# Install gh-repo-config extension
RUN gh extension install twelvelabs/gh-repo-config

# Install Node.js 20 LTS for workflow tooling (commitlint, etc.)
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs && \
    rm -rf /var/lib/apt/lists/*

# Install global npm packages for development
RUN npm install -g \
    npm@latest \
    @commitlint/cli@latest \
    @commitlint/config-conventional@latest

# Set working directory
WORKDIR /unity-analyzers

# Set environment variables
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

# Expose no ports (this is a build/dev container, not a service)

# Use bash as entrypoint
# This handles potential Windows file mount issues with permission bits
ENTRYPOINT ["bash"]
