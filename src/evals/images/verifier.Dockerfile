ARG NODE_DIGEST
ARG PLAYWRIGHT_DIGEST
FROM node:24.19.0-bookworm-slim@${NODE_DIGEST} AS node-runtime
FROM mcr.microsoft.com/playwright:v1.64.1-noble@${PLAYWRIGHT_DIGEST}
COPY --from=node-runtime /usr/local/bin/node /usr/local/bin/node
COPY --from=node-runtime /usr/local/lib/node_modules/npm /usr/local/lib/node_modules/npm
RUN ln -sf ../lib/node_modules/npm/bin/npm-cli.js /usr/local/bin/npm \
  && ln -sf ../lib/node_modules/npm/bin/npx-cli.js /usr/local/bin/npx \
  && groupadd --gid 10000 agent && useradd --uid 10000 --gid 10000 --create-home agent \
  && groupadd --gid 10001 build && useradd --uid 10001 --gid 10001 --create-home build \
  && mkdir -p /work /output /opt/evals && chmod 777 /work /output
COPY prepare.ts /opt/evals/prepare.ts
COPY collect.ts /opt/evals/collect.ts
WORKDIR /work
