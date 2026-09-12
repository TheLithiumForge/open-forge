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

| Flag                         | Value                    | Meaning                                                                         |
| ---------------------------- | ------------------------ | ------------------------------------------------------------------------------- |
| `--workspace <path>`         | One exact directory path | Use that directory as the workspace instead of the current directory            |
| `--json`                     | None                     | Render one structured result from the same typed result used for human output   |
| `--view=<compact\|expanded>` | `compact` or `expanded`  | Select token-friendly or explanatory result presentation; default to `expanded` |
| `--verbose`                  | None                     | Add bounded diagnostic detail without changing operation behavior or status     |
| `--help`                     | None                     | Show help for the selected command path without running the operation           |
| `--version`                  | None                     | Show the distributed CLI version without running a domain operation             |

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

The selected workspace and selection method appear in every workspace-aware
result:

```text
Workspace: D:/work/example
Selected by: --workspace
```

or:

```text
Workspace: D:/work/example
Selected by: current directory
```

## `--json`

`--json` selects structured presentation.

```text
open-forge context --json
```

Rules:

- Render one structured result document to stdout.
- Derive it from the same typed operation result as human output.
- Do not rerun parsing, planning, inspection, application, or verification.
- Do not mix ordinary human text into structured stdout.
- Send bounded diagnostic output to stderr only when the shared output contract
  allows it.
- Preserve the operation's semantic status and process result.
- Disable prompts. Missing semantic input is invalid. Missing authority or an
  unresolved choice for an otherwise complete request is blocked.

The exact shared result schema and compatibility rules are defined by the [Shared
Result Coordinates](../result-coordinates/interface.md).

## `--view=<compact|expanded>`

`--view` selects human or JSON result density:

```text
open-forge find --view=compact
open-forge route list memory --view=expanded
```

`expanded` is the default. It includes the explanations, evidence, provenance,
locations, and next actions that make the result understandable without another
call. `compact` is token-friendly and optimized for scanning or agent use. It
keeps identities, hierarchy or order, semantic status, completeness, and
required safety or next-action information while omitting optional explanation.

Rules:

- Change only presentation. Do not change selection, parsing, planning,
  effects, verification, findings, semantic status, or process result.
- Preserve authored content bytes selected by a content projection. A view may
  change generated framing around that content but never summarize or truncate
  the content itself.
- Keep results structured by their domain relationships. Compact output may use
  rows or indented levels; expanded output may add labelled evidence, source
  locations, arrows, provenance, and explanatory trees.
- Normal --json emits the complete expanded schema-v1 document. Explicit
  --json --view=compact emits the identified schema-v2 compact document using
  each command's defined core and omitted supporting fields. The serializer
  minifies compact JSON. No collection is arbitrarily truncated or filtered.
  Mutation receipts retain their full command result; selected authored content
  remains exact in either view.
- Every command provides compact and expanded presentations. A result with no
  additional meaningful explanation may have identical output in both views.
- When a selected compact renderer is unavailable, presentation falls back to
  the expanded renderer of the same format. It uses expanded detail for that
  invocation. This does not change the operation, result, status or output stream.
  Renderer failure is not unavailability and does not trigger fallback or retry.
- `--verbose` remains a separate diagnostic dimension and does not select the
  expanded view.

## `--verbose`

`--verbose` adds diagnostic detail to the selected presentation.

```text
open-forge context --verbose
open-forge context --json --verbose
```

Rules:

- Do not change selected input, planning, effects, verification, status, or exit
  behavior.
- Ordinary success and failure output remains understandable without it.
- Add details useful for investigation, such as resolved paths, stage names,
  inclusion reasons, and bounded failure context.
- Do not expose secrets, complete environment state, private recovery bytes, or
  unsafe unescaped source content.
- In JSON mode, keep stdout as one valid structured result. Any separate verbose
  diagnostics use stderr under the shared output contract.

Diagnostic fields and redaction remain bounded implementation details under the
CLI Architecture and must be covered by Gate 5 executable evidence without
changing the public result contract.

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
- Write help to stdout and return exit `0`. A composed `--json` flag is a no-op
  in this terminal mode; help remains ordinary text rather than an operation
  result envelope.

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
- Write the version to stdout and return exit `0`. A composed `--json` flag is a
  no-op in this terminal mode; version remains ordinary text.

Exact version and wrapper mismatch behavior remains part of later distribution
design.

## Composition

Global flags compose when their meanings apply together:

```text
open-forge context \
  --workspace ../another-workspace \
  --json \
  --view=compact \
  --verbose
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
- Repeating `--view` is invalid because one result has one selected
  presentation density.

## Related Sources

- [Shared CLI Contract Set](../_shared.md)
- [CLI Architecture](../../../architecture.md)
- [Behavior Contract](behavior.md)
- [Context Interface Contract](../../context/interface.md)
- [Historical CLI Decision Agenda](../../../../../../archived/cli-release/decision-agenda-2026-08-21.md)
- [Shared CLI Operation Contract](../../../shared-operation-contract.md)
