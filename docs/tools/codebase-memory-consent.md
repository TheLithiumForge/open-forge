# Codebase Memory consent

Codebase Memory consent is the source review that lists the writes made by the pinned Codebase Memory 0.12.1 installer for Claude Code and Codex.

The reviewed client selection is `--clients=claude,codex`. `--skip-binary` avoids binary placement and the install command's PATH changes. These options don't disable hooks, instruction writes or account-wide activation. R1 keeps install and uninstall arguments pending in the [draft definition](../../src/tools/drafts/codebase-memory-0.12.1.json).

Status: Draft consent. No installer was run.

## Consent text

Proposed text for the core to show if the maintainer permits this scope:

"Codebase Memory 0.12.1 configures your account's Claude Code and Codex clients. It writes MCP entries, a Codex `AGENTS.md` instruction block, one Skill and three agent profiles for each client, and Claude and Codex hooks. Activation can cancel every running Codebase Memory operation in this account. These changes apply across projects. This release has no `--no-hooks`, instruction opt-out or project-scope flag. Its uninstaller sweeps detected clients, can edit the Windows user PATH, and can delete every cached project index if `--delete-indexes` is supplied. Open Forge currently blocks these lifecycle commands."

## Path roots

The [pinned installer source](https://github.com/DeusData/codebase-memory-mcp/blob/a7158ac2f48ca646b4527341b76f930f2aeff87e/src/cli/cli.c) resolves these roots before writing:

| Symbol         | Default                                      | Override                      |
| -------------- | -------------------------------------------- | ----------------------------- |
| `home`         | Account home, using `USERPROFILE` on Windows | Upstream home resolution      |
| `claudeConfig` | `home/.claude`                               | `CLAUDE_CONFIG_DIR`           |
| `claudeRoot`   | `home`                                       | `CLAUDE_CONFIG_DIR`, when set |
| `codexConfig`  | `home/.codex`                                | `CODEX_HOME`                  |
| `cache`        | `home/.cache/codebase-memory-mcp`            | `CBM_CACHE_DIR`               |
| `bin`          | `home/.local/bin`                            | `--dir`                       |

The core must resolve actual paths before consent. These symbols aren't new manifest template variables. Redirecting a client environment variable isn't project-scope support, and changing `CODEX_HOME` isn't part of this proposal.

## Client writes

For the explicit Claude and Codex client selection, the source declares these writes:

| Path                                              | Entry or effect                                                             | Condition                             |
| ------------------------------------------------- | --------------------------------------------------------------------------- | ------------------------------------- |
| `claudeRoot/.claude.json`                         | `mcpServers.codebase-memory-mcp` with an absolute executable                | Claude selected                       |
| `claudeConfig/.mcp.json`                          | Removes the recognized old MCP member                                       | Legacy member exists                  |
| `claudeConfig/skills/codebase-memory/SKILL.md`    | Native usage Skill                                                          | Claude selected                       |
| `claudeConfig/agents/codebase-memory-scout.md`    | Scout profile                                                               | Claude selected                       |
| `claudeConfig/agents/codebase-memory.md`          | Verify profile                                                              | Claude selected                       |
| `claudeConfig/agents/codebase-memory-auditor.md`  | Auditor profile                                                             | Claude selected                       |
| `claudeConfig/settings.json`                      | `PreToolUse` for `Grep                                                      | Glob                                  | Bash`, `PostToolUse`for`Read`, `SessionStart`and`SubagentStart` hooks | Corresponding script write succeeds |
| `claudeConfig/hooks/cbm-code-discovery-gate.cmd`  | Search augmentation and read-coverage launcher                              | Windows                               |
| `claudeConfig/hooks/cbm-session-reminder.cmd`     | Session reminder launcher                                                   | Windows                               |
| `claudeConfig/hooks/cbm-subagent-reminder.cmd`    | Subagent reminder launcher                                                  | Windows                               |
| Same three hook paths without `.cmd`              | Hook launchers                                                              | macOS and Linux                       |
| Same three extensionless paths                    | Removes recognized legacy launchers                                         | Windows upgrade                       |
| `codexConfig/config.toml`                         | `[mcp_servers.codebase-memory-mcp]`                                         | Codex selected                        |
| `codexConfig/AGENTS.md`                           | `codebase-memory-mcp:start` to `codebase-memory-mcp:end` activation pointer | Codex selected                        |
| `codexConfig/skills/codebase-memory/SKILL.md`     | Native usage Skill                                                          | Codex selected                        |
| `codexConfig/agents/codebase-memory-scout.toml`   | Scout profile with its MCP declaration                                      | Codex selected                        |
| `codexConfig/agents/codebase-memory.toml`         | Verify profile with its MCP declaration                                     | Codex selected                        |
| `codexConfig/agents/codebase-memory-auditor.toml` | Auditor profile with its MCP declaration                                    | Codex selected                        |
| `codexConfig/hooks.json`                          | `SessionStart` and `SubagentStart` hooks                                    | That JSON hook file already exists    |
| `codexConfig/config.toml`                         | Same hooks in TOML, or removal of duplicate TOML hook registrations         | TOML used when `hooks.json` is absent |

The installer creates parent directories. In both clients' Skill directories, it removes empty old `codebase-memory-exploring`, `codebase-memory-tracing`, `codebase-memory-quality` and `codebase-memory-reference` directories. It keeps files in those old directories. Claude setup also removes the old monolithic `codebase-memory-mcp` Skill when recognized. Upstream checks can keep edited files or stop partway through configuration. No project `.gitignore` entry is needed because indexes live in the user cache.

## Activation writes

Binary placement writes `bin/codebase-memory-mcp.exe` on Windows or `bin/codebase-memory-mcp` elsewhere. It can stage a candidate in the OS temporary directory and use adjacent binary transaction files. `--skip-binary` skips placement and the install command's PATH edit. Ordinary Windows installation edits the current-user PATH through the registry. POSIX installation can append the bin directory to the detected shell startup file. These effects are outside the permitted Open Forge profile.

Account activation uses the private coordination directory and `cbm-version-cohort-admission-v1.lock`, `cbm-version-cohort-lifetime-v1.lock`, `cbm-version-cohort-maintenance-v1.lock` and `cbm-version-cohort-daemon-v1.lock`. Logs under `cache/logs` can include `activation-events.ndjson`, `daemon-conflicts.ndjson`, `cbm-daemon.log` and `cbm-daemon.start-failures.log`. Existing sessions can be cancelled before configuration. Native runtime settings use `cache/_config.db`, SQLite sidecars and `cache/config.json`. Setting those values is separate from installer client selection.

The [pinned IPC implementation](https://github.com/DeusData/codebase-memory-mcp/blob/a7158ac2f48ca646b4527341b76f930f2aeff87e/src/daemon/ipc.c) defines platform coordination paths. Windows uses the local application-data directory's `cbm-daemon-<instance-key>` directory, with `cbm-startup-v2.lock`, `cbm-participant-group-v1.lock` and `cbm-lifetime.lock`. It also uses an account-bound named pipe and startup mutex. POSIX uses `/tmp/cbm-daemon-<uid>`, or `/private/tmp/cbm-daemon-<uid>` on macOS. Its endpoint names are `cbm-<instance-key>.sock`, `.anc`, `.sock.identity`, `.sock.pending`, `.lock`, `.startup-v2.lock` and `.lifetime.lock`. Private log rotation can also write each log's `.1` sibling. The instance key and actual account roots must be resolved before consent.

The shared text editor uses `<target>.cbm-text-<pid>-<counter>.tmp` files. Binary transactions use `.cbm-stage-<pid>-<sequence>` and `.cbm-backup-<pid>-<sequence>` in the binary directory. A missing binary directory can require OS temporary `cbm-install-XXXXXX` staging first. Coordination-root resolution and account-wide uninstall are still unresolved effects. The core must enumerate actual paths from a reviewed profile before consent. This draft doesn't assert that the inventory is sufficient for activation.

## Removal

`uninstall --dry-run` previews upstream removal. `uninstall -y` keeps project indexes unless `--delete-indexes` is given. That flag deletes every project index in the selected cache. `--dir` selects a binary directory. Uninstall has no client-set, project-scope or keep-binary option. On Windows it attempts to remove the binary directory from the persistent user PATH, even if installation used `--skip-binary`. It scans all cleanup candidates, so the two-client install selection doesn't bound its removal scope.

Uninstall removes recognized Skill, profile, instruction, MCP and hook content. It keeps modified content and reports a neighboring `install.ps1` or `install.sh` instead of deleting it. `cli delete_project --project <verified-id>` is the separate exact-project cleanup form. It mustn't be replaced with account-wide index deletion.

## Not decided yet

- R1 must resolve hook and instruction opt-outs, account scope and uninstall's PATH and client sweep.
- Decide tool-owned delivery of the required Open Forge Skill alongside the native Skill.
- Resolve the complete activation and removal path set before allowing any real journey. No installer, including a dry-run installer, is authorized during this drafting task.

The [tool reference](../tools.md) explains Open Forge's consent and record model.
