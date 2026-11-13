# Tool help outline

Tool help is a planned set of help sections that explains the six tool setup commands.
For example, Inspect will show the tool's installer writes before you approve setup.

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

## Help sections

Each leaf's help will use these sections:

- Examples - one normal request and one preview where the command writes.
- Behavior - its default selection and result.
- Arguments - tool ID where applicable.
- Options - local flags, then the shared options.
- Details - installer writes, telemetry and shared-file scope where needed.
- Automation - required consent and missing-input behavior.
- Related commands - the next useful command.
