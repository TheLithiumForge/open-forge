# CodeGraph consent

CodeGraph consent is the write inventory that describes the pinned CodeGraph 1.6.2 lifecycle and Open Forge's command rewrites.

CodeGraph is third-party software from `colbymchenry/codegraph`, under the MIT licence. Open Forge downloads its verified archive from the official GitHub release. CodeGraph is maintained by its authors. Open Forge maintains the integration.

Status: Final V1 decisions recorded. Native journey waits for the core writers.

## Consent text

Proposed core display:

"Install third-party software CodeGraph 1.6.2 from colbymchenry/codegraph under MIT, downloaded from its official release and maintained by its authors. Open Forge will keep the verified archive's launcher and bundled Node in your per-user versioned store. It will run CodeGraph's local installer for Claude Code and Codex. This writes the codegraph MCP entries in .mcp.json and .codex/config.toml, CodeGraph instruction blocks in .claude/CLAUDE.md and AGENTS.md and a Claude UserPromptSubmit hook in .claude/settings.json. Open Forge rewrites only their launch commands to open-forge tool run codegraph. It adds the required open-forge-codegraph Skill and .codegraph/ ignore entry. While the code-navigation role is active, its managed agent block adds one CodeGraph usage line. It runs telemetry off by default, which saves the account preference in ~/.codegraph/telemetry.json. Update checks, watchers and automatic indexing are off. Installation doesn't build an index or choose a role. Git is the backup in a Git workspace. Elsewhere, existing edited files get sibling .bak backups."

The prompt-hook question is separate:

"Keep CodeGraph's Claude prompt hook? On structural prompts it adds context from the local index. Nothing leaves the machine. CODEGRAPH_NO_PROMPT_HOOK=1 disables it. Defaults to yes."

Non-interactive installation takes `--prompt-hook yes|no`. A choice of `no` removes only the complete matching CodeGraph hook entry and keeps its containing group. A visible `tools.codegraph.telemetry` setting defaults to false. Turning it on uses CodeGraph's own `telemetry on` command and keeps the update-check opt-out.

## Project writes

The pinned [Claude target](https://github.com/colbymchenry/codegraph/blob/6560052a6f856855d3f71eee838fd66ccfa4285d/src/installer/targets/claude.ts), [Codex target](https://github.com/colbymchenry/codegraph/blob/6560052a6f856855d3f71eee838fd66ccfa4285d/src/installer/targets/codex.ts) and [shared writer](https://github.com/colbymchenry/codegraph/blob/6560052a6f856855d3f71eee838fd66ccfa4285d/src/installer/targets/shared.ts) define these paths relative to the project:

| Path                                                  | Installer output                                                                                            | Open Forge change                                                           |
| ----------------------------------------------------- | ----------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------- |
| `.mcp.json`                                           | `mcpServers.codegraph`, `type:stdio`, `command:codegraph`, `args:[serve,--mcp]`, `alwaysLoad:true`          | Replace only command and args                                               |
| `.codex/config.toml`                                  | `[mcp_servers.codegraph]`, `command = "codegraph"`, `args = ["serve", "--mcp"]`                             | Replace only those two lines as text                                        |
| `.claude/settings.json`                               | `hooks.UserPromptSubmit`, command `codegraph.cmd prompt-hook` on Windows, `codegraph prompt-hook` elsewhere | Rewrite command, or remove just that entry on opt-out                       |
| `.claude/CLAUDE.md`                                   | `<!-- CODEGRAPH_START -->` through `<!-- CODEGRAPH_END -->`                                                 | Replace only `codegraph explore "<symbol names or question>"` in that block |
| `AGENTS.md`                                           | Same CodeGraph block                                                                                        | Same shell-line rewrite                                                     |
| `.claude.json`                                        | Remove legacy project `mcpServers.codegraph` if present                                                     | Report native cleanup                                                       |
| `.claude/settings.json`                               | Remove legacy CodeGraph `mark-dirty` and `sync-if-dirty` hook commands if present                           | Report native cleanup                                                       |
| `<edited-file>.tmp.<pid>`                             | Atomic write temporary, then rename                                                                         | Inventory any leftover                                                      |
| `<json-file>.backup`                                  | Upstream copy of unparseable JSON before replacement                                                        | Preflight rejects malformed input before installer effects                  |
| `.agents/skills/open-forge-codegraph/SKILL.md`        | No native Skill for these targets                                                                           | Install required Open Forge usage Skill                                     |
| `AGENTS.md` and `CLAUDE.md` Open Forge managed blocks | No Open Forge role line                                                                                     | Add one line only while CodeGraph fills code-navigation                     |
| `.gitignore`                                          | No Open Forge ignore section                                                                                | Add managed `.codegraph/` entry                                             |

`--no-permissions` suppresses `permissions.allow` entry `mcp__codegraph__*`. The installer creates `.claude` and `.codex` as needed. It writes no native Skill or profile for these targets. The actual Claude instruction path is `.claude/CLAUDE.md`, not root `CLAUDE.md`. Codex loads project configuration only for trusted projects.

## Command rewrites

After every install, configure or update, rewrite only the entries the reviewed installer wrote. Keep every server name, marker, unrelated member, hook and instruction line.

MCP command and arguments become `open-forge` and `[tool,run,codegraph,serve,--mcp]`. The hook becomes `open-forge tool run codegraph prompt-hook`. Keep the literal substring `codegraph prompt-hook`, because upstream uninstall matches it. The marked shell example becomes `open-forge tool run codegraph explore "<symbol names or question>"`.

One resolver chooses `open-forge` when it is on PATH. Otherwise it gives the absolute path of the running Open Forge executable. Quote that path for the destination's syntax. JSON uses the existing serializer. Codex edits only the expected command and args lines in the exact named table, with no TOML parser. An already recorded matching rewritten hook stays unchanged because native reinstall can keep it. An unexpected or ambiguous shape stops the operation and reports the path. It never broadens the rewrite. The current draft has a pristine Windows hook selector. Registration must resolve platform spelling and the appended array position before qualification, as the maintenance procedure records.

## Account state

The archive contains `bin/codegraph.cmd` on Windows and `bin/codegraph` elsewhere. These launch bundled Node 24.16.0. The shared launcher binding runs that contained runtime directly with the same flags and `lib/dist/bin/codegraph.js`, so Windows needs no command shell. The installer invocation is `install --location local --target claude,codex --yes --no-permissions`. Its `--yes` skips global npm installation and the PATH prompt. System Node and npm aren't needed.

Run with telemetry suppressed before other commands, then run the disclosed `telemetry off` unless the saved user setting enables telemetry. That command saves `~/.codegraph/telemetry.json` through temporary `telemetry.json.<pid>.<uuid>.tmp` and deletes buffered `telemetry-queue.jsonl` and `telemetry-queue.sending.<pid>.jsonl` files from that account directory. The temporary file is removed after the write attempt. When enabled, telemetry can write `telemetry-queue.jsonl` and `telemetry-queue.sending.<pid>.jsonl` under the same directory. `CODEGRAPH_NO_UPDATE_CHECK=1` prevents `~/.codegraph/update-check.json` activity. No PATH or registry edits are part of this lifecycle.

Every supplied launch disables daemon startup, watching and automatic indexing during Open Forge writes. The declared child Git environment redirects hook lookup to the private hook sink. It leaves file-level Git configuration and real hooks unchanged. The [maintenance procedure](../tool-maintenance.md) covers these process settings and the first index recipe.

## Removal

Run `open-forge tool run codegraph uninstall --location local --target claude,codex --yes --keep-cli`, then `open-forge tool run codegraph uninit <root> --force`. Native uninstall removes recognized MCP entries, CodeGraph instruction blocks, permissions and prompt hooks. Uninit deletes this project's `.codegraph` and checks Git sync hooks through the private hook sink.

Then remove the required Skill, the conditional Open Forge role line and its managed ignore entry. Release store bytes only after no workspace references that version. Report any empty JSON files, empty directories or native temporary files the commands leave behind. Don't delete unrelated content. The future Windows journey must verify that the rewritten hook is recognized by native uninstall and list every agreed leftover.

## Upstream controls

Future upstream requests are:

- An installer launch-command option, or an absolute launcher path when CodeGraph isn't on PATH. This removes Open Forge's rewrites.
- `--no-prompt-hook` for non-interactive installation.

No upstream contact is authorized.
