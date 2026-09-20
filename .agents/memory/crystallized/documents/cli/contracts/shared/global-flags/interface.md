---
open-forge:
  description: Accepted current caller-visible contract for shared global CLI flags
  responsibility: Define the public spelling, inputs, composition, presentation, errors, and examples for global flags
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Global, Flag, Interface, CurrentTruth]
---

# Global CLI Flags Interface Contract

## Status And Authority

This file is the accepted current Crystallized authority for the caller-visible
shared global-flag contract. These flags do not ship yet; implementation and executable evidence are tracked in
[CLI Development](../../../../../../working/cli-development/_cli-development.md). Command contracts link here for public
spelling, values, composition, presentation, errors, and examples instead of
redefining them.

The sibling [Behavior Contract](behavior.md) defines technology-neutral
resolution, invariants, safety, result formation, and conformance. The [Shared
Result Coordinates](../result-coordinates/interface.md) define the shared result
envelope and process-status mapping. The [CLI
Architecture](../../../architecture.md) defines parser binding, runtime,
filesystem, and evidence boundaries. This file defines the public contract and does not select
command-local implementation technology.

## Meaning Of Global

A global flag keeps one spelling, value grammar, default, and meaning throughout
the CLI. The root command registers it once. Every valid command path accepts
it. When the selected operation has no applicable behavior for a well-formed
global flag, that flag is an explicit no-op rather than invalid input.

A global flag may select shared execution context or presentation. It does not
change one command into another job and does not grant unrelated authority.

## Accepted Flags

| Flag                                          | Value                         | Default           | Meaning                                                               |
| --------------------------------------------- | ----------------------------- | ----------------- | --------------------------------------------------------------------- |
| `--workspace <path>`                          | One exact directory path      | Current directory | Use that directory as the workspace instead of the current directory  |
| `--format <text\|json>`                       | `text` or `json`              | `text`            | Select text or structured JSON output                                 |
| `--detail <minimal\|standard\|full\|debug>`   | One detail level              | `minimal`         | Select how much detail the text or JSON result includes               |
| `--detail-filter <error\|warning\|info\|all>` | One or more severity values   | Not set           | Select which finding severities are listed; repeatable                |
| `--help`                                      | None                          | —                 | Show help for the selected command path without running the operation |
| `--version`                                   | None                          | —                 | Show the distributed CLI version without running a domain operation   |

Short aliases are not accepted yet. Add one only when it is familiar, useful,
globally unique, and keeps the complete meaning of the canonical flag.

## `--workspace <path>`

`--workspace` selects one exact workspace root.

```text
open-forge context --workspace ../another-workspace
```

Rules:

- When supplied, use the exact path.
- Otherwise, use the exact current working directory.
- Resolve a relative value from the process current working directory.
- Normalize the selected path for result reporting.
- Require the selected target to be a directory when the operation needs one.
- Do not search parent directories.
- Do not substitute a Git root, package root, marker location, or nearby
  `.agents` directory.
- Do not infer another workspace from route or file operands.

Workspace selection does not prove that Open Forge is installed or healthy.
Each operation reports or blocks on the exact state it requires.

The selected workspace appears in text as `Workspace: <path>` at `minimal`
when `--workspace` was supplied or the result is `blocked`, `failed`, or
`cancelled`. It appears at `standard`, `full`, and `debug` for every
workspace-aware result. Text does not print `Selected by:`. JSON always
carries the workspace and its `selectedBy` value when workspace selection
applies.

## `--format <text|json>`

`--format` selects the output format:

```text
open-forge context --format json
```

`text` is the default. It writes the human-readable result to the primary
stream selected by the result status. `json` writes one schema-3 result
envelope to stdout for every semantic status.

Rules:

- Derive text and JSON from the same typed operation result.
- Do not rerun parsing, planning, inspection, application, or verification.
- Do not mix ordinary text into JSON stdout.
- Send prompts and bounded diagnostics to stderr under the shared output
  contract.
- Preserve the operation's semantic status, process exit, effects, findings,
  counts, limitations, and next action.
- `--format json` never prompts. Missing semantic input is `invalid-input`.
  Missing authority or an unresolved choice for an otherwise complete request
  is `blocked`.

The exact shared result schema and compatibility rules are defined by the
[Shared Result Coordinates](../result-coordinates/interface.md).

## `--detail <minimal|standard|full|debug>`

`--detail` selects the detail level for both text and JSON:

```text
open-forge status --detail full
```

`minimal` is the default. Each level adds to the previous level:

- `minimal`: the subject, one-line cause, and one action.
- `standard`: the reason for the action and per-finding actions.
- `full`: evidence, possible targets with why they were included, provenance,
  SHA-256 values, and finding codes.
- `debug`: the full primary result plus bounded run diagnostics on stderr.

Requested rows, authored content, and mutation receipts are never shortened by
the detail level. The detail level changes presentation only. It does not
change selection, parsing, planning, effects, verification, findings, status,
or process exit.

## `--detail-filter <error|warning|info|all>`

`--detail-filter` replaces the normal finding-listing ladder with the
specified severity set:

```text
open-forge doctor --detail-filter warning --detail-filter error
```

The flag is repeatable. Without it, each detail level uses the shared listing
ladder. The values are `error`, `warning`, `info`, and `all`; `all` lists
every severity. The filter changes which findings are listed, not their detail
depth. Counts and limitations are never affected. `all` wins when it is
combined with other values.

## Automatic Colour

Both human views accent generated status labels and selected headings when the
primary output stream is a supported terminal. The fixed palette is green for
complete, yellow for attention/incomplete/interrupted and warnings, red for
invalid/blocked/failed and errors, and cyan for information labels and headings.
Written labels remain present. There is no colour flag or configurable palette.

Unix terminal streams use colour when `TERM` is nonempty and not `dumb` and
`NO_COLOR` is empty or absent. Redirection is checked independently for stdout
and stderr. Windows and terminals without that capability evidence use plain
text. JSON, selected authored content, Find TSV rows, preview diffs, paths,
identifiers and commands retain their plain rendering. Diagnostics, prompts,
help and version remain plain. Colour never changes facts, status, stream or exit.

## `--help`

`--help` shows help for the selected command path.

```text
open-forge --help
open-forge context --help
open-forge extension --help
```

Rules:

- Do not resolve a workspace or run the domain operation.
- Show accepted operands, flags, defaults, examples, and related commands.
- A group shows its child operations and performs no domain operation.
- Help text uses the canonical command and flag vocabulary.
- Write help to stdout and return exit `0`. Composed `--format`, `--detail`,
  and `--detail-filter` flags are no-ops in this terminal mode; help remains
  ordinary text rather than an operation result envelope.

## `--version`

`--version` shows the version of the distributed CLI.

```text
open-forge --version
```

Rules:

- Do not resolve a workspace or run a domain operation.
- Return the canonical executable version.
- A thin wrapper must report or invoke the same version rather than defining its
  own Framework version.
- Write the version to stdout and return exit `0`. Composed `--format`,
  `--detail`, and `--detail-filter` flags are no-ops in this terminal mode;
  version remains ordinary text.

Exact version and wrapper mismatch behavior remains part of later distribution
design.

## Composition

Global flags compose when their meanings apply together:

```text
open-forge context \
  --workspace ../another-workspace \
  --format json \
  --detail full \
  --detail-filter warning
```

`--help` and `--version` stop before domain execution. They are terminal
informational modes, not presentation flags for an operation result. Other
well-formed global flags are accepted as no-ops in those terminal modes.

Rules:

- `--help` and `--version` are mutually exclusive.
- Either may follow a valid root, group, or command path.
- Domain operands and operation-specific flags remain invalid with `--help` or
  `--version`. Other well-formed global flags are no-ops.
- The parser reports the conflicting input instead of choosing precedence or
  silently ignoring it.

Global flags do not replace operation-specific flag roles:

- Selection flags choose compatible inputs within one operation.
- Projection flags choose which parts of one result are shown.
- Write-policy flags control preview, confirmation, or one named safety
  boundary.

## Errors

- A missing workspace value is invalid.
- An unavailable or non-directory workspace blocks only after the selected
  operation establishes that it requires a directory.
- A well-formed global flag with no applicable behavior for the selected
  operation is a no-op.
- An unknown global spelling or invalid global value remains invalid.
- Repeating a Boolean global flag has no additional effect.
- Repeating `--workspace` is invalid because one invocation has one exact
  workspace.
- Repeating `--detail` or `--format` is invalid because one invocation has
  one selected detail level and one output format.
- Repeating `--detail-filter` is valid and combines the requested severity
  values; `all` selects every severity.

## Related Sources

- [Shared CLI Contract Set](../_shared.md)
- [CLI Architecture](../../../architecture.md)
- [Behavior Contract](behavior.md)
- [Context Interface Contract](../../context/interface.md)
- [Historical CLI Decision Agenda](../../../../../../archived/cli-release/decision-agenda-2026-08-21.md)
- [Shared CLI Operation Contract](../../../shared-operation-contract.md)
