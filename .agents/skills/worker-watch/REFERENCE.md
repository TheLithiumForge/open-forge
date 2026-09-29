# Worker Watch reference

The CLI must be installed and visible in the same environment as the parent process. On WSL, install and run the CLI and daemon in that WSL distribution.

## Start the daemon

```bash
ww daemon start --max-running 30
ww daemon status --json
```

On Windows, start it with an explicit Codex binary. Otherwise the daemon can resolve the extensionless npm shell shim first, and every run fails within a second with `spawn ... ENOENT`:

```bash
ww daemon start --max-running 30 --codex-bin "<npm-global-root>\@openai\codex\bin\codex.js"
```

`npm root -g` prints the npm global root.

A restart drops flags you do not repeat, so pass `--codex-bin` again after every restart. Restart only when `ww list --active` is empty. Agents and run history survive in the daemon home. Check `ww models` for exact model IDs and supported efforts after the daemon starts.

## Create an authorized agent

```bash
ww agents add "Explorer" --cwd /absolute/repo --tag explorer --json
ww agents add "Implementer" --cwd /absolute/repo --write --worktree --tag implementer --json
ww agents add "Dot-folder writer" --cwd /absolute/repo --worktree --write-path .agents --json
ww agents add "Trusted unrestricted worker" --cwd /absolute/repo --full-access --json
ww agents add "Shared writer" --cwd /absolute/repo --write --shared-workspace team-a --json
```

Read-only is the default. A display title does not select a model. Omit `--model` to use the local Codex configuration, or pass the exact model ID the user supplied. `--effort` passes an actual Codex effort value, not a guessed translation of a UI label.

`--worktree` creates a branch `ww/<agent-id>` and a working tree from the repository's current HEAD. It refuses a dirty repository rather than silently omitting uncommitted work, so commit the intended base first. Worktrees are retained, never automatically merged or deleted.

## Permission levels

`--write` selects standard workspace-write. `--network` allows command network access only together with workspace-write. On a read-only agent it is ignored. For networked research that must not touch a repository, root the agent in an empty scratch folder with `--write --network --skip-git-repo-check`, and have it read the repository by absolute path.

`--write-path PATH` may be repeated. It implies workspace-write and grants write access to the named literal workspace-relative path, such as `.agents`, while keeping the surrounding sandbox. The server rejects absolute paths, parent traversal, glob patterns, and paths that escape through an existing symlink.

Set write paths when you create the agent. On Windows the Codex sandbox denies writes under `.agents` by default. A write path granted at creation lifts that for `.agents` and nested ones such as `src/app/.agents`. A write path added later to an agent that has already run did not lift the deny in practice.

`--full-access` is an explicit operator choice that maps to Codex `danger-full-access`. It disables the filesystem sandbox and treats network access as enabled. It cannot be combined with `--write` or `--write-path`.

Never silently increase an agent's permissions because a task failed. Worker Watch never falls back from read-only or workspace-write to full access. Permission and model settings can change only while a worker is idle. The CLI's `agents update` edits only `--shared-workspace`. Other fields go through the authenticated HTTP API, with the URL and token from `daemon.json` in the daemon home:

```bash
curl -X PATCH "$URL/api/agents/AGENT_ID" -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" -d '{"model":"MODEL_ID","effort":"medium"}'
```

`--shared-workspace GROUP` is a separate scheduling opt-in. Agents with the same nonempty group may run concurrently only when their canonical workspace roots are identical. It does not change permissions, and Worker Watch does not coordinate concurrent edits.

## The Codex sandbox on Windows

Workers in workspace-write run under a restricted identity. These are the known consequences and their fixes:

- **NuGet restore fails** because NuGet reads `%APPDATA%\NuGet\NuGet.Config`, outside the sandbox. Redirect APPDATA into the worktree, create the folder, and pass the repository config:

  ```powershell
  $env:APPDATA = Join-Path (Get-Location) 'artifacts\nuget-appdata'
  New-Item -ItemType Directory -Force artifacts\nuget-appdata\NuGet | Out-Null
  dotnet restore Solution.slnx --configfile NuGet.Config
  ```

- **MSBuild reports access errors under the build output** because the shared build server runs outside the sandbox. Disable it for the whole build:

  ```powershell
  $env:DOTNET_CLI_USE_MSBUILD_SERVER = '0'
  dotnet build Solution.slnx --disable-build-servers -m:1
  ```

- **Git rejects the worktree** as dubious ownership. Use `git -c safe.directory=<worktree> ...`.
- **Workers cannot commit.** A worktree's Git metadata lives in the main repository, outside the sandbox. Ask workers to leave their diff uncommitted and integrate it yourself.
- **Per-user stores are unwritable**, so tests that write application data outside the worktree fail inside the sandbox. Run those tests yourself after integration.

## Parallel waves

This shape has carried dozens of single-file packets in one session with one integrating overseer:

1. Freeze the contract in a tracked plan file and commit it. Workers read it instead of reinterpreting the task. Their worktrees branch from that commit.
2. Give each packet exactly one or two owned files, the change, and the focused test command. Put the build and sandbox workarounds in every packet. Keep one reasoning owner for any shared API, and let other packets depend on its frozen signatures.
3. Order packets into waves by build dependency. A packet that tests new behavior starts after the behavior is integrated.
4. Watch completions with a background loop that prints only state changes.
5. Integrate each finished worker from its worktree:

   ```bash
   git -C WORKER_TREE add -A
   git -C WORKER_TREE diff --cached --binary > packet.patch
   git -C INTEGRATION_TREE apply --index packet.patch
   ```

   If a worker had to return a diff in its message instead, its hunk context is often slightly wrong. Try `git apply --recount`, then `patch -p1 -l --fuzz=2`, and apply small edits by hand when both fail.

6. Build and run the complete test suite after each wave. Commit the integrated wave before creating the next wave's worktrees.

When a model reports that it is at capacity, retry once. After a second identical failure, stop retrying and move the packet to another permitted model or wait. A workspace roster, when present, names the permitted models.

## Clean up

Worktrees and `ww/<agent-id>` branches stay until you remove them. Before removing, confirm each branch tip equals the base commit it was created from, so no worker commit is lost. Then:

```bash
git worktree remove --force WORKER_TREE
git branch -D ww/AGENT_ID
ww agents disable AGENT_ID
```

## Commands

```text
ww agents list --available --json
ww start --agent ID --title TITLE --prompt-file FILE --from NAME --json
ww start --agent ID --fresh --title TITLE --prompt-file FILE --json
ww status RUN_ID --json
ww status RUN_ID --detail --json
ww result RUN_ID --json
ww wait RUN_ID --timeout 30 --json
ww logs RUN_ID --after 0 --limit 100 --json
ww send AGENT_ID "Follow-up" --json
ww stop RUN_ID --json
```

Agent IDs identify reusable workers. Run IDs identify immutable task records. Prefer run IDs when collecting results so a later task cannot change what you retrieve.

A run result has `status`, `result`, `error`, `threadId`, and optional reported `usage`. Completed means Codex finished, not that the code is correct. Verify claimed test results and inspect changes. A worker's own model label in its message is not evidence of the model that ran. The agent record is.

The operator controls availability with `ww agents enable ID` and `disable ID`, or `e` and `d` in the TUI. Disabling does not cancel active work. Use stop for cancellation. Do not override these choices.

HTTP starts are idempotent only when the same `requestId` is reused with an identical JSON request. An HTTP timeout is not proof a task failed to start. Retry the identical request or inspect existing runs before starting another.

Workers created through Worker Watch inherit `WW_HOME`, `WW_PARENT_RUN_ID`, `WW_PARENT_AGENT_ID`, and `WW_ORIGIN`. The CLI records those labels automatically. Provenance is caller-reported, not proof of identity.

No raw event stream is fed into the parent by default. Do not read entire logs unless needed. Native Codex subagents may appear as observed child events, but only agents submitted through Worker Watch have separately managed rows and cancellation.

## Web sessions, discovery, and monitoring

The default human interface is `ww web`, and `ww watch` remains optional. Inspect `ww agents list --available --json` for allocatable registered workers. Inspect `ww models --refresh`, `ww profiles --refresh`, or `ww catalog --refresh` for reported models, named profiles, and executable diagnostics.

A task's `--session-id` selects a persistent web session. The server preserves a separate provider thread for each session and agent pair. `--session` still supplies only an origin label. Supervised workers receive `WW_SESSION_ID`, and child CLI tasks inherit it automatically. `ww send RUN_ID` follows the referenced run's managed session unless `--session-id` overrides it. This does not enable live steering or automatic orchestration.

The web UI is monitoring-first. Event feeds default to newest first, own their scroll position, and can pause display updates without pausing the worker. The Agents page shows the effective filesystem permission mode and any explicit workspace write exceptions.
