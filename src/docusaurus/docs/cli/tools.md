---
title: Tool setup
---

# Tool setup

Tool setup is a planned CLI capability that installs tools through published channels and adds project configuration.
For example, a teammate will install the declared CodeGraph range and then call `codegraph` directly.

Status: Planned commands. Commands aren't implemented in this step.

The planned commands are:

| Command                        | Job                                                                    |
| ------------------------------ | ---------------------------------------------------------------------- |
| `open-forge tool list`         | List catalogue tools                                                   |
| `open-forge tool inspect <id>` | Show channels, requirements, paths, setup writes, config and telemetry |
| `open-forge tool status [id]`  | Check versions, config and PATH readiness                              |
| `open-forge tool install [id]` | Install and configure one tool or all declared tools                   |
| `open-forge tool update [id]`  | Move to the newest supported range                                     |
| `open-forge tool remove <id>`  | Undo this project's changes                                            |

Install has planned `--version <range>`, `--channel <channel>`, `--dry-run` and consent options.
CodeGraph `1.6.x` and Codebase Memory `0.12.x` are the V1 catalogue targets.
Rune waits for publication.
Full help and examples land with the implementation lanes.

The bin folder will be `~/.open-forge/bin` and Inspect and Status will show whether it is on PATH.
Terminals and agent apps need a restart after a PATH change.
