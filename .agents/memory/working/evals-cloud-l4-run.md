---
open-forge:
  description: Run the Claude Layer 4 evals for the bloated scenarios in a Claude Code cloud session, work stage only
  tags: [Memory, Working, Contextual, Active, Evals]
---

# Cloud L4 eval run

The maintainer asked for the Claude Layer 4 evals, then Layer 3, one plan per chat. Usage is limited, so Layer 4 runs only the bloated scenarios, and only the work stage. Checks and judges come later, on a machine the maintainer chooses.

## Scope

- Plan: a copy of `neon-pairs-l4-check.json` with `"checking": "later"`, `"judging": "later"` and `"parallelRuns": 2`.
- Scenarios: `overloaded-docs` and `full-core-scoped-memory`. The runner adds the control `organized-docs`.
- Cases: all three, `l4-f1-name-saves`, `l4-b5-pasted-saves` and `l4-p2-name-and-pasted-saves`.
- Runs: 3 cases x 3 scenarios x 1 repeat = 9, up to 60 minutes each.

"Bloated" is our reading of the maintainer's word. It matches the two scenarios that add a real project's notes beside the spec, as [Scenarios](../../../src/docusaurus/docs/evals/scenarios.mdx) describes.

## Cloud machine

The runner works here after two local changes, approved by the maintainer. Neither changes tracked files:

- Docker Hub refuses this machine's pulls with 429. Docker uses `mirror.gcr.io` as a registry mirror, and the pinned digest still checks the image.
- The network's egress gateway re-signs TLS, so containers fail with `SELF_SIGNED_CERT_IN_CHAIN`. The images are built locally with the CAs from the host's `/usr/local/share/ca-certificates`, under the tags the unchanged runner expects. `run` then finds them and skips its own build. Don't run `npm run evals -- images`, since it rebuilds without the CAs.
- Tested: from the agent image, `fetch` to `api.anthropic.com` returns 401, and Claude Code with a fake key answers "Invalid API key". The local proxy CA alone in `/root/.ccr` doesn't cover container traffic.

The sign-in is `CLAUDE_CODE_OAUTH_TOKEN` in the Default cloud environment, so a new session has it in its process environment. Never print it.

## Steps

Run these from the repository root:

1. Set the mirror and start Docker:

   ```sh
   mkdir -p /etc/docker && echo '{ "registry-mirrors": ["https://mirror.gcr.io"] }' > /etc/docker/daemon.json
   (dockerd > /tmp/dockerd.log 2>&1 &)
   ```

2. Run `npm ci --prefix src/evals`.
3. Save the build script below as `build-ca-images.sh` in your scratchpad, then run `bash <scratchpad>/build-ca-images.sh`. It takes about 10 minutes.
4. Save the plan below as `neon-pairs-l4-claude-work.json` in your scratchpad.
5. Run `npm run evals -- doctor`. It needs no usage.
6. Start the run in the background:

   ```sh
   npm run evals -- run --plan <scratchpad>/neon-pairs-l4-claude-work.json --setups overloaded-docs,full-core-scoped-memory --parallel 2
   ```

7. Follow it with `npm run evals -- progress`. If it pauses at a usage limit, continue with `npm run evals -- resume <batch>`. Don't restart failed runs.
8. Publish when it's done, as [Run evals](../../../src/docusaurus/docs/evals/run.md) describes under Add results. It needs the results repository `TheLithiumForge/open-forge-eval-results` added to the session with push access.

Build script:

```sh
#!/usr/bin/env bash
set -euo pipefail
CAS=${CAS:-/usr/local/share/ca-certificates}
CTX=$(mktemp -d)
cp -r src/evals/images/. "$CTX/"
mkdir "$CTX/extra-ca"
cp "$CAS"/*.crt "$CTX/extra-ca/"
# awk adds the final newline a certificate file can lack, so the joined file stays valid PEM.
awk 1 "$CAS"/*.crt > "$CTX/extra-ca.pem"
# The tracked .dockerignore lets only the listed files into the build, so let the CAs in too.
printf '%s\n' '!extra-ca.pem' '!extra-ca' '!extra-ca/*' >> "$CTX/.dockerignore"
for kind in agent verifier assessor; do
  awk -v last="$(grep -n '^FROM' "$CTX/$kind.Dockerfile" | tail -1 | cut -d: -f1)" '
    { print }
    NR == last { print "COPY extra-ca/ /usr/local/share/ca-certificates/extra/"; print "COPY extra-ca.pem /etc/ssl/extra-ca.pem"; print "ENV NODE_EXTRA_CA_CERTS=/etc/ssl/extra-ca.pem" }
    END { print "RUN update-ca-certificates" }' "$CTX/$kind.Dockerfile" > "$CTX/$kind.ca.Dockerfile"
done
ARGS=$(cd src/evals && node --input-type=module -e '
  const { lockedImageArguments } = await import("./images/lock.ts");
  const { ImageVersions } = await import("./execution/containers/docker-image.ts");
  const values = [...lockedImageArguments(), `CLAUDE_VERSION=${ImageVersions.claude}`, `CODEX_VERSION=${ImageVersions.codex}`, `OPEN_FORGE_VERSION=${ImageVersions.openForge}`];
  console.log(values.map((v) => `--build-arg ${v}`).join(" "));')
for kind in agent verifier assessor; do
  TAG=$(cd src/evals && node --input-type=module -e "const m = await import('./execution/containers/docker-image.ts'); console.log(m.evalImageTag('$kind'));")
  docker build --quiet --tag "$TAG" --file "$CTX/$kind.ca.Dockerfile" $ARGS "$CTX"
done
rm -rf "$CTX"
```

Plan:

```json
{
  "schemaVersion": 1,
  "layer": 4,
  "cases": ["l4-f1-name-saves", "l4-b5-pasted-saves", "l4-p2-name-and-pasted-saves"],
  "setups": ["organized-docs", "linked-agents", "minimal", "essentials", "full-core", "full-core-scoped", "code-only", "plain-docs", "overloaded-docs", "full-core-scoped-memory"],
  "runsPerSetup": 1,
  "parallelRuns": 2,
  "agents": "claude",
  "judges": "claude",
  "simulatedUser": "claude",
  "isolation": "docker",
  "limits": { "questionRounds": 3, "runMinutes": 60, "checkMinutes": 90 },
  "checking": "later",
  "judging": "later"
}
```

## Next step

Run the steps above in a new session on branch `ccr-17a2a289-jmta3y`. After Layer 4 is published, Layer 3 follows in another chat. Retire this record once both are published.
