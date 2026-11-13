# CodeGraph consent

CodeGraph consent is the source review that lists the writes made by the pinned CodeGraph 1.6.2 installer for Claude Code and Codex.

The reviewed scope is `install --location local --target claude,codex --yes --no-permissions`, run in the project. This describes upstream behavior. It isn't an approved Open Forge command. R2 keeps its install arguments pending in the [draft definition](../../src/tools/drafts/codegraph-1.6.2.json).

Status: Draft consent. No installer was run.

## Consent text

Proposed text for the core to show after resolving R2:

"CodeGraph 1.6.2 will configure Claude Code and Codex in this project. It will add MCP server `codegraph`, instruction blocks in `AGENTS.md` and `.claude/CLAUDE.md`, and a Claude `UserPromptSubmit` hook in `.claude/settings.json`. It can remove recognized legacy CodeGraph entries. It won't build an index. The installer writes a command resolved through PATH. Open Forge still needs a reviewed way to launch its private verified executable with the declared process settings."

## Project writes

The [pinned target implementations](https://github.com/colbymchenry/codegraph/tree/6560052a6f856855d3f71eee838fd66ccfa4285d/src/installer/targets) and their shared writer define these paths relative to the process directory:

| Path                      | Entry or effect                                                                                            | Condition                              |
| ------------------------- | ---------------------------------------------------------------------------------------------------------- | -------------------------------------- |
| `.mcp.json`               | `mcpServers.codegraph`                                                                                     | Claude selected                        |
| `.codex/config.toml`      | `[mcp_servers.codegraph]`                                                                                  | Codex selected                         |
| `AGENTS.md`               | `CODEGRAPH_START` to `CODEGRAPH_END` instruction block                                                     | Codex selected                         |
| `.claude/CLAUDE.md`       | Same instruction block                                                                                     | Claude selected                        |
| `.claude/settings.json`   | `hooks.UserPromptSubmit` command `codegraph.cmd prompt-hook` on Windows, `codegraph prompt-hook` elsewhere | `--yes` sets `promptHook=true`         |
| `.claude/settings.json`   | Removes recognized `mark-dirty` and `sync-if-dirty` legacy hook commands                                   | Recognized old entries exist           |
| `.claude.json`            | Removes the legacy project-local `mcpServers.codegraph` entry                                              | That legacy entry exists               |
| `<edited-file>.tmp.<pid>` | Temporary file, renamed into the target and removed on rename failure                                      | Shared atomic writer runs              |
| `<json-file>.backup`      | Copy before replacing unparseable JSON                                                                     | A JSON write finds unparseable content |

The MCP command is `codegraph`, with arguments `serve --mcp`. The installer doesn't write the Open Forge child environment or an absolute private launcher. `--no-permissions` suppresses the `permissions.allow` entry `mcp__codegraph__*`. It doesn't suppress the prompt hook. Without that flag, `--yes` enables the permission entry too.

The writer creates `.claude` and `.codex` directories as needed. It can leave empty directories or empty JSON after removal. It writes no native Skill or agent profile for these targets. The required Open Forge Skill therefore still needs a tool-owned delivery decision. Codex's local configuration takes effect only in a trusted project.

## Account writes

The reviewed explicit target and location options keep agent configuration in the project. `--yes` also skips the optional global npm install prompt. Global location, auto target discovery, `--init` and `--refresh` aren't part of this proposed invocation.

CodeGraph's telemetry state uses `~/.codegraph/telemetry.json`, `telemetry-queue.jsonl` and temporary `telemetry-queue.sending.<pid>.jsonl` files when telemetry is enabled. Its update cache is `~/.codegraph/update-check.json`. `DO_NOT_TRACK=1` and `CODEGRAPH_NO_UPDATE_CHECK=1` are required to suppress that activity. The visible draft setting `telemetryEnabled` defaults to false. Enabling it must leave the dedicated update-check opt-out set. Native verification of these effects is pending.

## Removal

The source-reviewed cleanup form is `uninstall --location local --target claude,codex --yes --keep-cli`. It removes recognized MCP entries, instruction blocks, CodeGraph permissions and hooks at those local targets. It keeps the CLI. The separate `uninit <code-root> --force` command removes that code root's `.codegraph` index and tries to remove recognized `post-commit`, `post-merge` and `post-checkout` sync hooks from Git's resolved hook directory. Every invocation therefore needs the private child hook sink too. Neither command is approved for execution by this draft.

Open Forge's proposed managed ignore entry is `.codegraph/`. Git or sibling `.bak` handling covers Open Forge's own writes. Upstream `.backup` and `.tmp.<pid>` files are separate effects and need their own inventory in the real journey.

## Not decided yet

- R2 must resolve the noninteractive Claude hook and private absolute launcher with process settings.
- Decide tool-owned delivery of the required Open Forge Skill.
- Confirm removal inventories and transient files in the real Windows journey, then through native CI on the other five targets.

The [tool reference](../tools.md) explains Open Forge's consent and record model.
