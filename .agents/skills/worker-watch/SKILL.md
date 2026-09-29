---
name: worker-watch
description: Delegate coding or review tasks to supervised Codex workers with per-agent status, persistent threads, provenance, isolated worktrees, and a human-visible monitor. Use when the user asks for monitored or parallel workers, or to delegate through Worker Watch instead of raw codex exec.
---

# Worker Watch

Use `ww` (also installed as `worker-watch`), not raw `codex exec`, for tasks the user wants monitored. The daemon owns the workers. Do not open the interactive TUI from an agent shell tool.

If the workspace has a [ROSTER.md](ROSTER.md) next to this Skill, follow it for model and effort choices. It overrides any default below.

## Delegate

1. Check `ww daemon status --json`. If it is not running, start it as [REFERENCE.md](REFERENCE.md#start-the-daemon) describes for your platform, or report the setup requirement. Do not fall back to an unmonitored process.
2. Pick or create an agent. `ww agents list --available --json` shows enabled agents with `canStart: true`. New agents and wider permissions must stay within the user's authorization. Pass the exact model ID and effort from `ww models`. A display title such as "Luna" does not select a model.
3. Check that `ww agents add` returned an `id` before you use it. With an empty `--agent` value, `ww start` silently creates an ad-hoc read-only run in the current directory.
4. Put a bounded task in a prompt file: goal, owned files, constraints, acceptance tests, expected report. Do not copy the whole parent conversation.
5. Submit once:

```bash
ww start --agent AGENT_ID \
  --title "Implement snapshot pruning" \
  --from "Claude overseer" \
  --request-id UNIQUE_TASK_REQUEST_KEY \
  --prompt-file /absolute/path/task.md --json
```

Use a stable, unique request key for that exact submission. Retrying the identical request returns the original run. Different work needs a new key. Preserve `run.id` and `agentId`. On Windows, pass the prompt path with forward slashes.

Supply `--session` when the parent session identifier is known, and `--origin-path` when the parent's working directory differs. Do not invent harness IDs. `--parent RUN_ID` records lineage explicitly.

## Observe and finish

```bash
ww status RUN_ID --json
ww wait RUN_ID --timeout 30 --json
ww result RUN_ID --json
```

Use `wait`, or a background loop that reports only state changes, when blocked on results. Do not wake the parent model on every poll. Timeout exit code 2 means work continues, not failure. Fetch detailed logs only when diagnosing a problem.

`quiet` and `very-quiet` mean no recent Codex event. An alive process may still be reasoning, retrying, or waiting on a command. Never call it stuck from silence alone. A worker's self-reported model label is unreliable. Read the agent or run record instead.

For a related follow-up on an idle agent:

```bash
ww send AGENT_ID "Fix the review findings" --json
```

This resumes its saved thread. It is not live steering. Busy, disabled, or conflicting agents return HTTP 409. Do not bypass that protection. `--fresh` starts a new conversation for unrelated work. `ww stop RUN_ID` cancels a run when authorized.

## Parallel work

Give each independent writer its own worktree with `--worktree`. Worker Watch does not coordinate concurrent edits. [REFERENCE.md](REFERENCE.md#parallel-waves) covers freezing a contract, splitting work into one packet per file, integrating uncommitted worker diffs, and cleaning up. Review every result before integrating it. `completed` means Codex finished, not that the change is correct.

Read [REFERENCE.md](REFERENCE.md) for agent creation, permissions, the Codex sandbox on Windows, and uncommon commands.
