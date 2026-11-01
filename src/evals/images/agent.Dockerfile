ARG NODE_DIGEST
ARG PLAYWRIGHT_DIGEST
FROM node:26.12.1-bookworm-slim@${NODE_DIGEST} AS node-runtime
# Playwright's image carries Chromium and its system libraries, so agents can run their own browser tests.
FROM mcr.microsoft.com/playwright:v1.64.1-noble@${PLAYWRIGHT_DIGEST}
ARG CLAUDE_VERSION
ARG CODEX_VERSION
ARG OPEN_FORGE_VERSION
COPY --from=node-runtime /usr/local/bin/node /usr/local/bin/node
COPY --from=node-runtime /usr/local/lib/node_modules/npm /usr/local/lib/node_modules/npm
RUN ln -sf ../lib/node_modules/npm/bin/npm-cli.js /usr/local/bin/npm \
  && ln -sf ../lib/node_modules/npm/bin/npx-cli.js /usr/local/bin/npx \
  && apt-get update \
  && apt-get install -y --no-install-recommends ca-certificates git ripgrep \
  && rm -rf /var/lib/apt/lists/*
RUN npm install --global "@anthropic-ai/claude-code@${CLAUDE_VERSION}" "@openai/codex@${CODEX_VERSION}" "@thelithiumforge/open-forge@${OPEN_FORGE_VERSION}"
RUN groupadd --gid 10000 agent && useradd --uid 10000 --gid 10000 --create-home agent \
  && mkdir -p /work /home/agent/.codex /home/agent/.claude /opt/evals \
  && chmod 777 /work \
  && chown -R root:root /home/agent && chmod -R a+rwX /home/agent
ENV PLAYWRIGHT_BROWSERS_PATH=/ms-playwright
COPY prepare.ts /opt/evals/prepare.ts
COPY stop-tools.ts /opt/evals/stop-tools.ts
WORKDIR /work
