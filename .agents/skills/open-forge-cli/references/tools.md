---
open-forge:
  description: Planned round-2 native tool setup commands awaiting implementation
  tags: [Skill, CLI, Reference]
---

# Tool setup outline

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
| `open-forge tool remove <id>`            | Run native uninstall, then remove additions                         |

Bare Install restores exact lock versions.
A bare name selects latest support. For example, `codegraph@1.6` selects that supported minor range.
Writers have planned `--yes`, `--dry-run` and path-permission options.
Install and Update also have `--channel` for a named spec.
`--yes` accepts the shown plan and downloads. It never edits PATH or deletes shared files unattended.
Dry-run says when it needs network access to resolve latest.
CodeGraph `1.6.x` and Codebase Memory `0.12.x` are the V1 targets.
Full help and examples land after implementation.
