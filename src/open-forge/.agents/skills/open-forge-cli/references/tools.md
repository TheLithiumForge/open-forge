---
open-forge:
  description: Planned tool setup command outline awaiting implementation
  tags: [Skill, CLI, Reference]
---

# Tool setup outline

Tool setup is a planned CLI capability that installs published tools and adds project configuration.
For example, bare Tool Install will set up every tool declared by a teammate's project.

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

Use the direct tool command after setup. Each active tool will add its own usage Skill and one conditional managed-block line.
