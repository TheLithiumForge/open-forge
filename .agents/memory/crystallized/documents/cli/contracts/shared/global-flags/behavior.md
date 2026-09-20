---
open-forge:
  description: Accepted current technology-neutral resolution and conformance for shared global CLI flags
  responsibility: Define how a conforming implementation resolves shared flags and forms their results without selecting technology
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Global, Flag, Behavior, CurrentTruth]
---

# Global CLI Flags Behavior Contract

## Status And Boundary

This file is the accepted current Crystallized authority for technology-neutral
resolution, invariants, safety, result formation, and conformance behind the
[Interface Contract](interface.md). The flags do not ship yet; implementation and executable evidence are tracked in
[CLI Development](../../../../../../working/cli-development/_cli-development.md). The Interface Contract remains the
complete public authority for spelling, value grammar, defaults, observable
presentation, errors, examples, and non-goals. This file links to those
definitions rather than maintaining a second public contract.

The [Shared Result Coordinates](../result-coordinates/interface.md) define the
shared result envelope and process-status mapping. The [CLI
Architecture](../../../architecture.md) defines parser binding, runtime,
filesystem, and evidence boundaries. This behavior remains technology-neutral within those
accepted choices.

## Shared Flag Invariants

- One root registration supplies each flag's spelling, value grammar, default,
  and meaning to every valid command path, as defined by
  [Meaning Of Global](interface.md#meaning-of-global) and [Accepted Flags](interface.md#accepted-flags).
- A well-formed flag with no applicable behavior remains an explicit no-op; flag
  resolution does not turn one operation into another job or grant unrelated
  authority. See [Meaning Of Global](interface.md#meaning-of-global).
- Flag resolution preserves the operation-specific roles and the composition
  boundary in [Composition](interface.md#composition).

## Request Resolution

Request resolution validates the shared flag spellings, values, omissions,
repetitions, and terminal conflicts against the [Interface Contract](interface.md).
It produces the one complete shared-flag request for the selected command path.
It does not invent aliases, precedence, defaults, or alternate value grammars.

When the request selects `--help` or `--version`, resolution establishes the
terminal informational mode before workspace selection or domain execution. The
remaining terminal-mode conditions are defined in [Terminal Informational Modes](#terminal-informational-modes)
and [Composition](interface.md#composition).

## Workspace Resolution

Workspace resolution applies the exact public rules in
[`--workspace <path>`](interface.md#--workspace-path):

- It selects the supplied exact path, or the exact process current working
  directory when the flag is omitted.
- It resolves a relative supplied value from the process current working
  directory and normalizes the selected path for workspace-aware result
  reporting.
- It establishes the directory precondition only when the selected operation
  requires one.
- It performs no parent, Git-root, package-root, marker, nearby-`.agents`, route,
  or file-operand discovery.
- It records the selection method in every workspace-aware result and does not
  treat workspace selection as proof that Open Forge is installed or healthy.

The operation reports or blocks on the exact state it requires. No second
workspace-resolution path is available to a command.

## Format Presentation

For `--format json`, the operation runs once and forms one typed operation
result. The JSON renderer consumes that result and emits one schema-3 envelope
to stdout. It does not rerun parsing, planning, inspection, application, or
verification, and it does not mix ordinary text into JSON stdout. The renderer
preserves semantic status and process result. Bounded diagnostics use stderr.

For `--format text`, the text renderer consumes the same typed operation result
and writes the primary text stream selected by the result status. Text prompts
and bounded diagnostics use stderr. The two formats never cause a second
operation run.

JSON resolution disables prompts. Missing semantic input forms the public
`invalid-input` result. Missing authority or an unresolved choice for an
otherwise complete request forms the public `blocked` result. The exact shared
result schema and compatibility rules are defined by the [Shared Result
Coordinates](../result-coordinates/interface.md), as stated in [the public
format contract](interface.md#--format-textjson).

## Detail Presentation

The selected `--detail` value applies to both formats. `minimal` is the default,
and each level adds to the previous level:

- `minimal` keeps the subject, one-line cause, and one action.
- `standard` adds the reason for the action and per-finding actions.
- `full` adds evidence, possible targets with why they were included, provenance,
  SHA-256 values, and finding codes.
- `debug` keeps the full primary result and adds bounded run diagnostics on
  stderr.

The shared listing ladder lists errors and, except for diagnosis-shaped
commands, warnings at `minimal`; lists warnings for diagnosis-shaped commands
at `standard`; and lists info findings at `full` and `debug`. Findings hidden
by a level remain counted. Requested rows, authored content, and mutation
receipts are never shortened. Detail changes presentation only and does not
change selection, parsing, planning, effects, verification, findings, status,
or process exit.

## Detail Filter

`--detail-filter` replaces the shared listing ladder with the requested
severity set. The flag is repeatable and accepts `error`, `warning`, `info`,
and `all`. `all` lists every severity and wins when combined with other values.
The filter changes which findings are listed, not their per-finding detail
depth. Counts and limitations are never affected. When the flag is absent, the
selected detail level supplies the listing ladder.

## Terminal Informational Modes

`--help` and `--version` terminate before workspace selection or domain
execution. Help resolves the selected command path, preserves canonical command
and flag vocabulary, and shows the public operands, flags, defaults, examples,
and related commands. A group shows its child operations and performs no domain
operation.

Version resolution returns the canonical distributed executable version without
workspace access or a domain operation. A thin wrapper reports or invokes that
same version rather than defining its own Framework version. Exact version and
wrapper mismatch behavior remains later distribution design. These behaviors
are the public definitions in [`--help`](interface.md#--help) and
[`--version`](interface.md#--version), not new Behavior choices.

Both terminal modes write ordinary text to stdout and return exit `0`.
`--format`, `--detail`, and `--detail-filter` are well-formed no-ops in terminal
mode and do not wrap help or version in the operation-result envelope.

## Composition And Terminal Modes

The request resolver permits compatible shared flags to compose when their
meanings apply together. It treats `--help` and `--version` as mutually exclusive
terminal modes. Either may follow a valid root, group, or command path. Domain
operands and operation-specific flags are rejected in either terminal mode, while
other well-formed global flags become no-ops.

When terminal inputs conflict, the resolver reports the conflicting input. It
does not choose precedence or silently ignore one input. These conditions
conform to [Composition](interface.md#composition).

## Error Conformance

Error formation follows [Errors](interface.md#errors) exactly. Validation
preserves the distinction between invalid input, a conditional workspace block,
an explicit no-op, and invalid repetition. It does not add alternate errors,
weaken a condition, or turn a no-op into an invalid result.

Every public error and no-op remains the result of the same resolved request used
by human and structured presentation. The renderer does not rerun validation to
choose a different result.

## Verification Requirements

Gate 5 executable proof must cover:

- Registration from one shared source.
- Command-specific applicability.
- Exact CWD and explicit workspace selection.
- Relative and absolute workspace values.
- No upward or marker-based discovery.
- Human and JSON rendering from one typed result.
- All four detail levels in text and JSON from the same typed result, with
  `minimal` as the default and `debug` diagnostics isolated on stderr.
- `--detail-filter` severity unions, including `all`, without changing counts.
- No-op handling for well-formed global flags that do not apply to the selected
  operation or terminal informational mode.
- Structured stdout isolation.
- `--format json` stdout isolation and `--detail debug` diagnostics that do not
  change results.
- Root, group, and command help.
- Version output without workspace access.
- Repeated and incompatible flag behavior.
- Thin-wrapper version parity when the wrapper exists.

## Related Sources

- [Global CLI Flags Interface Contract](interface.md)
- [Shared CLI Contract Set](../_shared.md)
- [CLI Architecture](../../../architecture.md)
- [Context Behavior Contract](../../context/behavior.md)
- [Historical CLI Decision Agenda](../../../../../../archived/cli-release/decision-agenda-2026-08-21.md)
- [Shared CLI Operation Contract](../../../shared-operation-contract.md)

## Colour Conformance

Apply the [automatic colour policy](interface.md#automatic-colour) only after
forming the command result. Select capability for its actual primary output
stream. Style generated labels from typed statuses or severities, then restore
the default foreground immediately. Do not recognize words in finished output.
Removing generated accents must recover the plain result exactly. JSON must be
byte-identical with colour capability enabled or disabled. Verify both formats,
independent stream capabilities, terminal fallback and selected-content/preview
preservation.
