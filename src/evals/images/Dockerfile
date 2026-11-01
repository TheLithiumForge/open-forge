# The image every Docker run uses. The tool versions come from evals/package.json.
FROM node:24-bookworm-slim

ARG CLAUDE_VERSION
ARG CODEX_VERSION

RUN apt-get update \
  && apt-get install -y --no-install-recommends ca-certificates git ripgrep \
  && rm -rf /var/lib/apt/lists/*

RUN npm install --global "@anthropic-ai/claude-code@${CLAUDE_VERSION}" "@openai/codex@${CODEX_VERSION}"

RUN mkdir -p /work /home/agent && chown node:node /work /home/agent
USER node
ENV HOME=/home/agent
WORKDIR /work
