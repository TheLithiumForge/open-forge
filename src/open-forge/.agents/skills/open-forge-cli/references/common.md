---
open-forge:
  description: Shared CLI flags, source syntax, previews, and result meanings
  tags: [Skill, CLI, Reference]
---

# Shared CLI Options And Results

Use this reference for options and result meanings shared by all commands. Command references list their own flags and link here instead of repeating these rules. The running executable's `--help` defines the interface when an older installation differs.

## Command And Source Syntax

```text
open-forge <command> [operands] [command flags] [global flags]
```

Use `open-forge --help` for the top-level inventory. `route --help`, `extension --help`, and `library --help` list their subcommands. Add `--help` to a complete command for its operands, flags, defaults, and examples.

A source reference is an exact source ID, such as `memory/crystallized`, or an exact `.agents/...` Markdown path. Use the exact path when an ID is ambiguous. Source IDs, Extension stable IDs, and Library management IDs identify different things. Examples assume their routes or packages exist. Substitute IDs from the selected workspace's inventory.

## Global Flags

Every command accepts these flags. Extension Create accepts `--workspace` but creates in its explicit catalogue, so that flag has no effect there.

| Flag                                          | Meaning and useful choice                                                                                                                    |
| --------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------- |
| `--workspace <path>`                          | Select one exact workspace directory. Relative values resolve from the process directory. The CLI does not search parents                    |
| `--format <text\|json>`                       | Text for reading, JSON for scripts. Default: `text`. JSON returns one schema-version-3 envelope on stdout                                    |
| `--detail <minimal\|standard\|full\|debug>`   | Default: `minimal`. Use `standard` for reasons, `full` for evidence, `debug` for bounded diagnostics on stderr. Detail affects text and JSON |
| `--detail-filter <error\|warning\|info\|all>` | Select listed diagnostic severities. Repeat to combine. It never changes semantic status or exit code                                        |
| `--help`                                      | Print the exact command interface and exit                                                                                                   |
| `--version`                                   | Print the executable version and exit                                                                                                        |

Quote values containing spaces and glob expressions. Repeat flags only where the command supports repetition. `route list --depth=2` requires the equals form. Content and region selectors take one comma-separated value, such as `--content frontmatter,body`.

## Preview And Apply

`--dry-run` reports the complete checked mutation plan without writing files. Review destinations, replacements, deletions, reference changes, and recovery effects. Apply the same authorized request without `--dry-run` after review.

Only some commands support `--automatic`. It suppresses supported interaction without adding inputs, selecting packages, enabling force, or granting authority. A command that requires confirmation returns `invalid-input` without writes in a noninteractive shell unless its supported unattended mode is supplied. JSON and redirected execution never prompt.

Route Init, Create, Update, Move, and Index apply their checked plans without a confirmation flag. Cleanup also applies directly, so review its deletion preview before use. Check each command's help rather than adding unsupported `--automatic`.

`--allow-path <path>` records a supported CLI permission for an exact destination outside `.agents`. It is a write boundary, not human authorization. Supply the reviewed destination within the task's authority. `--force` permits only its command's documented initial replacement. On Framework and Extension Update it is accepted for compatibility and adds no authority.

## Read Results

Check both semantic status and exit code after every command:

| Status                    | Exit | Next action                                                         |
| ------------------------- | ---- | ------------------------------------------------------------------- |
| `completed`               | 0    | Use the result                                                      |
| `completed-with-warnings` | 2    | Use the result after reviewing warnings                             |
| `incomplete`              | 3    | Supply required missing facts before relying on the affected result |
| `invalid-input`           | 4    | Correct syntax, required inputs, or unattended mode                 |
| `blocked`                 | 5    | Resolve the reported boundary within the task, or report it         |
| `failed`                  | 1    | Inspect the failure before choosing a retry                         |
| `cancelled`               | 130  | Treat the operation as cancelled and inspect any reported state     |

Successful, warning, and incomplete text results use stdout. Invalid, blocked, failed, and cancelled text results use stderr. JSON always uses stdout for its result envelope. More detail or filtering never turns an unsuccessful operation into success.

For related tasks, read [discovery](discovery.md), [route maintenance](routes.md), or [Framework and packages](packages.md).
