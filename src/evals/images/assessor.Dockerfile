ARG NODE_DIGEST
FROM node:26.12.1-bookworm-slim@${NODE_DIGEST}
ARG CLAUDE_VERSION
ARG CODEX_VERSION
RUN apt-get update \
  && apt-get install -y --no-install-recommends ca-certificates libicu72 \
  && rm -rf /var/lib/apt/lists/*
RUN npm install --global "@anthropic-ai/claude-code@${CLAUDE_VERSION}" "@openai/codex@${CODEX_VERSION}"
RUN groupadd --gid 10000 agent && useradd --uid 10000 --gid 10000 --create-home agent \
  && mkdir -p /work /home/agent/.codex /home/agent/.claude /opt/evals && chmod 777 /work \
  && chown -R root:root /home/agent && chmod -R a+rwX /home/agent
COPY prepare.ts /opt/evals/prepare.ts
COPY stop-tools.ts /opt/evals/stop-tools.ts
WORKDIR /work
