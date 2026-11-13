# Codebase Memory research

Codebase Memory research is the maintenance record that lists the writes made by the pinned Codebase Memory 0.12.1 installer for Claude Code and Codex.

Codebase Memory is unsupported in V1. The pinned installer configures the account, writes Codex instructions, profiles and hooks and has no project scope or hook opt-out. Open Forge ships no installable definition, required Skill or public tool page for it.

Status: Source review and release verification complete. Native installation untested.

## Verified release

The [release evidence](research/codebase-memory-0.12.1.json) records `v0.12.1`, tag commit `a7158ac2f48ca646b4527341b76f930f2aeff87e`, exact URLs, all six selected archive sizes and SHA-256 values. Each downloaded archive matched the published checksums. Each individual Sigstore bundle and the checksum bundle verified with issuer `https://token.actions.githubusercontent.com` and identity `https://github.com/DeusData/codebase-memory-mcp/.github/workflows/release.yml@refs/heads/main`. Blob signatures bind bytes and signer identity. They don't provide source-build evidence. The [checksum fixture](research/codebase-memory-checksums.txt) keeps the upstream metadata.

## Missing controls

An upstream change needs these controls:

- Project-scoped install and uninstall, including MCP entries, instructions, Skills and profiles.
- A hook opt-out that works in non-interactive installation.
- Uninstall limited to the chosen clients and project, without account PATH edits.

`--clients=claude,codex` limits installation clients. `--skip-config` skips agent setup. `--skip-binary` skips binary placement and installation's PATH edit. These flags don't turn account activation into a project operation. The installer can cancel existing account sessions before configuring clients. No installer, including dry-run installation, is authorized during this work. Don't contact upstream.

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

Any future support must resolve actual paths before consent. These symbols aren't new manifest template variables. Redirecting a client environment variable isn't project-scope support, and changing `CODEX_HOME` isn't part of this proposal.

## Client writes

For the explicit Claude and Codex client selection, the source declares these writes:

| Path                                              | Entry or effect                                                                                        | Condition                             |
| ------------------------------------------------- | ------------------------------------------------------------------------------------------------------ | ------------------------------------- |
| `claudeRoot/.claude.json`                         | `mcpServers.codebase-memory-mcp` with an absolute executable                                           | Claude selected                       |
| `claudeConfig/.mcp.json`                          | Removes the recognized old MCP member                                                                  | Legacy member exists                  |
| `claudeConfig/skills/codebase-memory/SKILL.md`    | Native usage Skill                                                                                     | Claude selected                       |
| `claudeConfig/agents/codebase-memory-scout.md`    | Scout profile                                                                                          | Claude selected                       |
| `claudeConfig/agents/codebase-memory.md`          | Verify profile                                                                                         | Claude selected                       |
| `claudeConfig/agents/codebase-memory-auditor.md`  | Auditor profile                                                                                        | Claude selected                       |
| `claudeConfig/settings.json`                      | `PreToolUse` for Grep, Glob and Bash, `PostToolUse` for Read, `SessionStart` and `SubagentStart` hooks | Corresponding script write succeeds   |
| `claudeConfig/hooks/cbm-code-discovery-gate.cmd`  | Search augmentation and read-coverage launcher                                                         | Windows                               |
| `claudeConfig/hooks/cbm-session-reminder.cmd`     | Session reminder launcher                                                                              | Windows                               |
| `claudeConfig/hooks/cbm-subagent-reminder.cmd`    | Subagent reminder launcher                                                                             | Windows                               |
| Same three hook paths without `.cmd`              | Hook launchers                                                                                         | macOS and Linux                       |
| Same three extensionless paths                    | Removes recognized legacy launchers                                                                    | Windows upgrade                       |
| `codexConfig/config.toml`                         | `[mcp_servers.codebase-memory-mcp]`                                                                    | Codex selected                        |
| `codexConfig/AGENTS.md`                           | `codebase-memory-mcp:start` to `codebase-memory-mcp:end` activation pointer                            | Codex selected                        |
| `codexConfig/skills/codebase-memory/SKILL.md`     | Native usage Skill                                                                                     | Codex selected                        |
| `codexConfig/agents/codebase-memory-scout.toml`   | Scout profile with its MCP declaration                                                                 | Codex selected                        |
| `codexConfig/agents/codebase-memory.toml`         | Verify profile with its MCP declaration                                                                | Codex selected                        |
| `codexConfig/agents/codebase-memory-auditor.toml` | Auditor profile with its MCP declaration                                                               | Codex selected                        |
| `codexConfig/hooks.json`                          | `SessionStart` and `SubagentStart` hooks                                                               | That JSON hook file already exists    |
| `codexConfig/config.toml`                         | Same hooks in TOML, or removal of duplicate TOML hook registrations                                    | TOML used when `hooks.json` is absent |

The installer creates parent directories. In both clients' Skill directories, it removes empty old `codebase-memory-exploring`, `codebase-memory-tracing`, `codebase-memory-quality` and `codebase-memory-reference` directories. It keeps files in those old directories. Claude setup also removes the old monolithic `codebase-memory-mcp` Skill when recognized. Upstream checks can keep edited files or stop partway through configuration. No project `.gitignore` entry is needed because indexes live in the user cache.

## Activation writes

Binary placement writes `bin/codebase-memory-mcp.exe` on Windows or `bin/codebase-memory-mcp` elsewhere. It can stage a candidate in the OS temporary directory and use adjacent binary transaction files. `--skip-binary` skips placement and the install command's PATH edit. Ordinary Windows installation edits the current-user PATH through the registry. POSIX installation can append the bin directory to the detected shell startup file. These effects are outside the permitted Open Forge profile.

Account activation uses the private coordination directory and `cbm-version-cohort-admission-v1.lock`, `cbm-version-cohort-lifetime-v1.lock`, `cbm-version-cohort-maintenance-v1.lock` and `cbm-version-cohort-daemon-v1.lock`. Logs under `cache/logs` can include `activation-events.ndjson`, `daemon-conflicts.ndjson`, `cbm-daemon.log` and `cbm-daemon.start-failures.log`. Existing sessions can be cancelled before configuration. Native runtime settings use `cache/_config.db`, SQLite sidecars and `cache/config.json`. Setting those values is separate from installer client selection.

The [pinned IPC implementation](https://github.com/DeusData/codebase-memory-mcp/blob/a7158ac2f48ca646b4527341b76f930f2aeff87e/src/daemon/ipc.c) defines platform coordination paths. Windows uses the local application-data directory's `cbm-daemon-<instance-key>` directory, with `cbm-startup-v2.lock`, `cbm-participant-group-v1.lock` and `cbm-lifetime.lock`. It also uses an account-bound named pipe and startup mutex. POSIX uses `/tmp/cbm-daemon-<uid>`, or `/private/tmp/cbm-daemon-<uid>` on macOS. Its endpoint names are `cbm-<instance-key>.sock`, `.anc`, `.sock.identity`, `.sock.pending`, `.lock`, `.startup-v2.lock` and `.lifetime.lock`. Private log rotation can also write each log's `.1` sibling. The instance key and actual account roots must be resolved before consent.

The shared text editor uses `<target>.cbm-text-<pid>-<counter>.tmp` files. Binary transactions use `.cbm-stage-<pid>-<sequence>` and `.cbm-backup-<pid>-<sequence>` in the binary directory. A missing binary directory can require OS temporary `cbm-install-XXXXXX` staging first. Coordination-root resolution and account-wide uninstall are still unresolved effects. The core must enumerate actual paths from a reviewed profile before consent. This draft doesn't assert that the inventory is sufficient for activation.

## Removal

`uninstall --dry-run` previews upstream removal. `uninstall -y` keeps project indexes unless `--delete-indexes` is given. That flag deletes every project index in the selected cache. `--dir` selects a binary directory. Uninstall has no client-set, project-scope or keep-binary option. On Windows it attempts to remove the binary directory from the persistent user PATH, even if installation used `--skip-binary`. It scans all cleanup candidates, so the two-client install selection doesn't bound its removal scope.

Uninstall removes recognized Skill, profile, instruction, MCP and hook content. It keeps modified content and reports a neighboring `install.ps1` or `install.sh` instead of deleting it. `cli delete_project --project <verified-id>` is the separate exact-project cleanup form. It mustn't be replaced with account-wide index deletion.

## Future support

Before supporting this tool, verify project-scoped installation and removal with hooks disabled against a new upstream pin. Keep this inventory as the comparison case. The separate `cli --quiet --json delete_project --project <verified-id>` operation can delete one project index. Never replace it with `uninstall --delete-indexes`, which deletes every project index in the cache.

The [maintenance checklist](../tool-maintenance.md) defines the review steps.
