---
open-forge:
  description: The approved eval commands, prerequisites, provider sessions, sign-in, setup preparation and recovery
  tags: [Memory, Document, CurrentTruth, Evaluation, Harness, Isolation]
---

# Running Evals

Running Evals is the operating guide that prepares a comparison, drives the coding tools and saves results for later checks.

Status: approved design on 2026-11-01. The complete command flow is Not built yet.

A draft runner exists under `src/evals/`. Its earlier real runs are recorded in [Task 58](../../../working/cli-development/tasks/task58-demo-evals.md#current-state). The approved flow below replaces its folder-only and single-image arrangements. For example, a saved Layer 1 plan will run the four current setups and leave frozen games and separate assessments.

The public [run guide](https://thelithiumforge.github.io/open-forge/docs/evals/run) covers the implemented commands, model presets, reference data point and local UI. Published checks and judge pairs appear in [Results](https://thelithiumforge.github.io/open-forge/docs/evals/results). CLI and reference trends appear in [History](https://thelithiumforge.github.io/open-forge/docs/evals/history). These pages live in the main documentation site and read records from the results repository.

## Requirements

The approved product needs these prerequisites:

- Node `24.19.0` and npm.
- Docker and the three locked images.
- Claude Code and Codex at their pinned versions.
- The chosen provider's subscription sign-in or API key.
- A saved plan with frozen inputs, setups, checks, answer bank and limits.

Container rules follow [Architecture](architecture.md#images). The [proposal's product design](../../../emerging/analysis/eval-product-proposal.md#product) defines the runtime and images. Scored attempts use containers. The draft's `local` mode can't provide that isolation.

## Commands

Run commands from the repository root as `npm run evals -- <command>`. The public run guide above describes the implemented commands. The `publish` command below is planned.

| Command | Result |
| --- | --- |
| `doctor` | Docker, Node, tool and sign-in checks |
| `images` | Three images built from locked digests |
| `smoke` | A mock attempt through the full flow without a model |
| `qualify` | Setup delivery checks outside the scored sample |
| `run --plan <plan>` | Attempts from a saved plan |
| `ui` | Local blind comparison and saved results without model calls |
| `recheck` | New check assessment on saved files |
| `rejudge` | New judge assessment on saved files |
| `publish` | Finished batch prepared for the results repository |

The [proposal's command table](../../../emerging/analysis/eval-product-proposal.md#product) defines these commands.

In the draft, `npm start` opens the local web app from `src/evals/`, and `npm run batch` runs the saved config. `eval.config.json` holds defaults, with `eval.config.local.json` for UI changes. Local preference edits change the input fingerprint. Keep those results separate. The [draft README](../../../../../src/evals/README.md) describes its current controls.

The approved run begins with prerequisite and image checks, then a model-free smoke attempt, then setup checks. A saved plan gives `run` its attempts. The resulting files give `ui`, `recheck` and `rejudge` their inputs. The [protocol](protocol.md#run-steps) defines the attempt steps.

## Provider sessions

Drive the pinned CLIs directly. Each attempt gets fresh tool settings without user-level instructions, native memory, plugins, hooks or MCP servers. Open Forge files are the setup, so they stay. A fresh install starts with empty Memory.

The session adapters keep these behaviors:

| Provider | Session behavior |
| --- | --- |
| Claude Code | One `claude -p` process, stream-json input and output, answers on stdin, fresh `CLAUDE_CONFIG_DIR` and `HOME` |
| Codex | One `codex exec` per turn, later turns resume the same conversation, fresh `CODEX_HOME` and `HOME`, prompt on stdin |

Claude uses `CLAUDE_CODE_DISABLE_AUTO_MEMORY=1`, strict MCP configuration and disabled web tools. Don't use `--bare` or `--safe-mode`, because they skip `CLAUDE.md`. Codex disables web search. Both tools execute within the container without approval prompts. Check flags and instruction loading for both providers before each tool version bump.

The [proposal's delivery requirements](../../../emerging/analysis/eval-product-proposal.md#delivery-phases) require provider instruction loading and equal judge evidence before scored runs. The simulated user stays in the same coding session, under the [answer-bank rules](protocol.md#simulated-user).

## Sign-in

Both tools can use subscriptions or API keys. Keep sign-in data out of prompts, logs, records and exported evidence.

Claude's draft adapter reads a long-lived `CLAUDE_CODE_OAUTH_TOKEN` obtained through `claude setup-token`, or `ANTHROPIC_API_KEY`, from `src/evals/.env.local`. Codex's draft login command is `npm run login:codex` from `src/evals/`. It keeps an eval-only sign-in and gives each attempt only that sign-in. `OPENAI_API_KEY` is also supported.

Keep a refreshed Codex sign-in after the attempt. Pass sign-in values to containers by variable name, so values don't appear on command lines. The chosen provider's credentials reach only its agent container. Verifier containers get none. The [proposal's image rules](../../../emerging/analysis/eval-product-proposal.md#product) define this separation.

## Setup preparation

Generate every setup from one source. Install the pinned Open Forge version once per batch and setup, then copy that frozen setup for attempts. Local `npm pack` candidates are allowed when their source and package hashes are recorded.

Save install commands, output and every installed file's path, hash and size. Keep the setup manifest outside the agent's folder. The [fair comparison rules](protocol.md#fair-comparison) define the remaining setup checks, and the [proposal's product plan](../../../emerging/analysis/eval-product-proposal.md#product) assigns generation to `materialize/`.

## Recovery

Stop the agent and its leftover processes before freezing files or starting the build. The verifier rebuilds offline and runs browser checks one at a time. Save files, built output, screenshots, events and measurements before deleting an attempt's container and temporary files.

A usage or rate limit pauses the batch. Keep the interrupted record when resuming starts another attempt. Infrastructure replacement follows the registered rule. Failed coding attempts and timeouts stay in the comparison.

Recheck saved files after a grader error. Recheck and rejudge append assessments and keep the earlier outcomes. The [grading contract](grading.md#counting) defines the counting, and the [results contract](results.md#records) defines the saved records. The [proposal's delivery plan](../../../emerging/analysis/eval-product-proposal.md#delivery-phases) requires cancellation, recovery and attempt accounting.

## Not decided yet

- Restricting agent network access to model services and the package registry.
- A frozen npm registry mirror.
- Hosted runners if local runs are too slow.

Back to [Evals](_evals.md).
