ARG NODE_DIGEST
FROM node:24.19.0-bookworm-slim@${NODE_DIGEST}
ARG CLAUDE_VERSION
ARG CODEX_VERSION
ARG OPEN_FORGE_VERSION
RUN apt-get update \
  && apt-get install -y --no-install-recommends ca-certificates git ripgrep libicu72 \
  && rm -rf /var/lib/apt/lists/*
RUN npm install --global "@anthropic-ai/claude-code@${CLAUDE_VERSION}" "@openai/codex@${CODEX_VERSION}" "@thelithiumforge/open-forge@${OPEN_FORGE_VERSION}"
RUN groupadd --gid 10000 agent && useradd --uid 10000 --gid 10000 --create-home agent \
  && mkdir -p /work /home/agent/.codex /home/agent/.claude /opt/evals \
  && chmod 777 /work
COPY prepare.ts /opt/evals/prepare.ts
COPY stop-tools.ts /opt/evals/stop-tools.ts
WORKDIR /work
