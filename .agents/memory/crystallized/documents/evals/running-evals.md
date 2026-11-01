---
open-forge:
  description: How every Open Forge eval run starts, runs and ends in isolation on Claude Code and Codex, how the eval tool drives each CLI on a subscription, and what it captures
  tags: [Memory, Document, CurrentTruth, Evaluation, Harness, Isolation]
---

# Running Evals

Running Evals says how every Open Forge eval run starts, runs and ends, so that runs can't see each other or anything else on the machine.

A run that can read the maintainer's settings, memory or earlier runs no longer measures its setup. So each run gets its own fresh folder, its own fresh tool settings and, with Docker, its own container. The [eval tool](../../../../../src/evals/README.md) does all of it: `npm start` opens a local web app, Run starts a batch, and results come back as tables, charts and games to play. For example, a [Greenfield Baseline](greenfield-baseline.md) run copies the Essentials snapshot into a fresh `neon-pairs` folder, runs Codex there with the prompt, freezes the files, and only then builds and checks the game.

Status: proposed on 2026-10-30. A first draft of the tool is built, and only its mock runs are tested. No real model run has happened yet.

## Isolation

Every run follows these rules:

- A fresh folder per run, outside any Git repository, named with a random ID: `~/.workspaces/<id>/neon-pairs`. The name says nothing about the setup.
- With Docker, one container per run from one image with the pinned tool versions. The container sees only the project folder and a fresh home.
- Fresh tool homes: no user-level instructions, no native memory, no plugins, hooks or MCP servers. Only sign-in variables pass from the maintainer's shell.
- Web search is off in both tools.
- The agent edits and runs commands without prompts, the same for every setup. The container or the run folder is the sandbox.
- Runs go in random order, so a slow hour or a warm provider cache doesn't favor one setup.
- Several runs can go at once. Their hidden checks still run one at a time, since they time real flips and delays.
- We delete the run folder after its files and records are saved.

Open Forge's own files are the setup, so they stay. Its Memory starts empty in every run, as in a fresh install.

Without Docker, isolation is `local`: the run still gets a fresh folder and fresh tool homes, but the agent can read the rest of the disk.

## Driving the tools

Every way to drive Claude Code or Codex from a program starts the tool's own process. The SDKs only wrap that process, so the eval tool runs the CLIs directly:

- Claude Code and Codex come from npm, pinned in the tool's `package.json`. Every machine runs the same versions, and the Docker image installs the same ones.
- The Claude Agent SDK's defaults differ from the CLI's, so it would need extra settings to behave like Claude Code.
- Codex's app server speaks JSON-RPC to editors and is marked experimental. It's an option later, if `codex exec` misses events we need.
- [NASDE](https://noesisvision.github.io/nasde-toolkit/), built on [Harbor](https://github.com/harbor-framework/harbor), comes closest as a ready-made runner, but it has no simulated user and no context data. [Inspect SWE](https://meridianlabs-ai.github.io/inspect_swe) routes model calls through its own layer, which most likely needs API keys. All three are Python.

### Claude Code

- One `claude -p` process per run, with `--input-format stream-json --output-format stream-json --verbose`. The simulated user's answers go in on stdin, so there's no resuming by session ID.
- A fresh `CLAUDE_CONFIG_DIR` and `HOME`, with `CLAUDE_CODE_DISABLE_AUTO_MEMORY=1`.
- `--strict-mcp-config`, `--permission-mode bypassPermissions`, and `--disallowedTools WebSearch WebFetch`.
- No `--bare` or `--safe-mode`. Both skip `CLAUDE.md`, which would drop Open Forge's instructions.

### Codex

- One `codex exec --json --skip-git-repo-check` per turn. Later turns run `codex exec resume` with the run's thread ID.
- A fresh `CODEX_HOME` and `HOME`, with `web_search="disabled"` and `--dangerously-bypass-approvals-and-sandbox`.
- The prompt goes in on stdin, so it never lands in a command line.

## Sign-in

Both tools run on the maintainer's subscriptions, and API keys work as well:

- Claude Code reads a long-lived token from `claude setup-token`, kept in `evals/.env.local` as `CLAUDE_CODE_OAUTH_TOKEN`.
- Codex signs in once into a home kept only for evals, with `npm run login:codex`. Each run gets a copy of that sign-in and nothing else. Codex refreshes its sign-in during a run, so the newer copy goes back after the run.
- Sign-in values reach a container by name only, so they never appear in a command line.
- A usage or rate limit pauses the batch. Resume starts the interrupted run again.

Subscriptions don't bill per run, so a dollar figure is what the tokens would cost at API prices. Claude Code reports its own estimate. Codex reports none yet.

## Installs

Every Open Forge setup installs the same way:

- From the Open Forge version pinned in the tool's `package.json`.
- Once per batch and setup, into an empty `neon-pairs` folder. Every run of the setup starts from a copy.
- The setup record keeps the commands, their output and every installed file's path, hash and size.

## The simulated user

When the agent's turn ends, a fixed model reads its last message with the hidden wishes:

- If the message asks the user something, it answers from the wishes, and the agent gets that answer.
- If not, the run ends.
- Its tokens count as user effort, never as the agent's cost.

The eval sets what the wishes are and how many rounds a run allows.

## What we capture

The tool keeps these for every run:

- The raw event stream, in `events.jsonl`.
- Tokens and context per model request, counted once per request. Claude Code's stream gives each request's usage. For Codex, the session log in its fresh home gives each request's context and every compaction.
- Every tool call, failed tool calls, and reads of Open Forge's files with their size.
- Time for the agent, the build and the checks.
- The final files without `node_modules`, the built game and the screenshots.

## After the run

Each run ends in this order:

1. Stop the agent and freeze the files.
2. Run the project's `build` script again with no network: a container with `--network none`, or npm offline for `local`.
3. Run the hidden checks in a pinned browser that blocks outside requests, and take the screenshots.
4. Copy the code without Open Forge's files, instruction files or notes to a scratch folder outside any repository, and run the judge there.
5. Save the run record. Nothing from the checks reaches the agent.

Before the build and before the folder is deleted, the tool stops every process still running from the run's folder, such as a dev server the agent left behind. When the checks change, `npm run recheck` scores the saved games again without running the agents or the judge.

## Not decided yet

- Allowing only the model API and the npm registry on the network. The draft doesn't limit the network yet.
- A frozen npm registry mirror, so every run gets the same package versions.
- Which flags change in later tool versions. We check every flag before a version bump.
- The model behind the simulated user. The default is a small Claude model.
- Hosted runners, if local runs get too slow.

Back to [Evals](_evals.md).
