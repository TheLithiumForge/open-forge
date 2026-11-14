# Tool help outline

Tool setup is a planned command group that installs published tools and adds configuration.
The tools keep their native installers, MCP entries, hooks, permissions, blocks and Skills.

Status: Planned commands. Implementation follows the round-2 spec.

The planned commands are:

| Command                                  | Job                                                                 |
| ---------------------------------------- | ------------------------------------------------------------------- |
| `open-forge tool list`                   | List catalogue entries                                              |
| `open-forge tool inspect <id>`           | Show channels, requirements, native writes, additions and telemetry |
| `open-forge tool status [id]`            | Check exact versions, additions and PATH readiness                  |
| `open-forge tool install [id[@version]]` | Run native setup and add configuration                              |
| `open-forge tool update [id[@version]]`  | Select latest support or an explicit version                        |
| `open-forge tool remove <id>`            | Remove project additions and offer native uninstall                 |

Bare Install restores exact lock versions.
A bare name selects latest support. For example, `codegraph@1.6` selects that supported minor range.
Writers have planned `--yes`, `--dry-run` and path-permission options.
Install and Update also have `--channel` for a named spec.
`--yes` accepts the shown plan and downloads. It never edits PATH or deletes shared files unattended.
Codebase Memory account uninstall needs live confirmation naming its binary and all agent entries.
With `--yes` or no terminal, Remove only removes Open Forge's project additions and prints the native uninstall command.
Project Remove keeps account-wide settings such as telemetry off and prints their manual change commands.
Dry-run says when it needs network access to resolve latest.
CodeGraph `1.6.x` and Codebase Memory `0.12.x` are the V1 targets.
Full help and examples land after implementation.

## Help sections

Each leaf will have these sections:

- Examples - normal input and a writer preview.
- Behavior - default version selection and native scope.
- Arguments - ID or version spec.
- Options - local options and unchanged global options.
- Details - native writes, additive config, telemetry and protected effects.
- Automation - the single --yes plan-consent rule.
- Related commands - the next useful operation.
