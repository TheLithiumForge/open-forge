---
open-forge:
  description: "Historical CLI-v2 source: Exact help, version, parse-failure, diagnostic, ordering, stream, and exit behavior at the replacement CLI parser boundary"
  responsibility: Define parser-level operation results without turning Commander output or raw process input into the public protocol
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# Parser Result Contract

## Scope

The parser boundary returns ordinary typed operation results for help, version,
and invocations that cannot select a valid domain request. Commander supplies
parsing and metadata, but its strings, thrown errors, process exits, and output
streams do not become the public contract.

This contract owns `cli.help`, `cli.version`, and `cli.parse` data and stream
behavior. The [CLI Interface](../interface.md) owns the public command and flag
surface. The [result and display boundary](../interface.md#results-and-presentation)
owns the shared five-field envelope, semantic statuses, and exit mapping.
Unexpected exceptions use the separate `cli.failure` contract rather than
pretending to be parser validation.

## Guarantees

### Operation Selection

| Invocation state                                                     | Operation     | Status    |
| -------------------------------------------------------------------- | ------------- | --------- |
| Bare root                                                            | `cli.help`    | `success` |
| Root, group, or leaf with valid `--help`                             | `cli.help`    | `success` |
| Bare group                                                           | `cli.help`    | `invalid` |
| Valid `--version`                                                    | `cli.version` | `success` |
| Syntax or parser-policy failure before a valid domain request exists | `cli.parse`   | `invalid` |

A selected leaf with valid syntax retains its domain operation id when later
semantic resolution fails. For example, `extension add --json` returns
`extension.add` with `invalid` status because the leaf is known but required
non-interactive Extension selection is absent. An unknown command, missing
required positional argument, malformed option, or inapplicable global never
enters a domain handler and therefore returns `cli.parse`.

`--help` and `--version` are mutually exclusive. Any workspace, confirmation,
mutation, filter, or domain flag combined with either meta flag is invalid.
`--json` is the only non-meta flag that may accompany them.

### Named Values

Production source defines each inventory once through a readonly const object
and derives its union type from that object.

`CliHelpKind` contains:

| Value   | Meaning                           |
| ------- | --------------------------------- |
| `root`  | Top-level command catalogue       |
| `group` | One command family and its leaves |
| `leaf`  | One executable domain command     |

`CliParseIssueKind` contains:

| Value                  | Required issue evidence                                |
| ---------------------- | ------------------------------------------------------ |
| `conflicting-options`  | Every conflicting canonical option                     |
| `unknown-command`      | Offending token and any bounded suggestions            |
| `unknown-option`       | Offending token and any bounded suggestions            |
| `invalid-flag-form`    | Offending token and canonical double-dash suggestion   |
| `missing-option-value` | Canonical option and expected value grammar            |
| `invalid-value`        | Offending value and bounded accepted values or grammar |
| `missing-argument`     | Argument name and expected value grammar               |
| `unexpected-argument`  | Offending token                                        |
| `inapplicable-global`  | Canonical global option and selected command path      |

Every token beginning with one leading dash rather than the required two is
invalid. When adding the missing dash produces exactly one public canonical or
alias spelling, the issue is `invalid-flag-form` and its suggestion is that
option's canonical double-dash spelling. Clustered, partial, and unrelated
single-dash tokens are `unknown-option` issues with no fabricated suggestion.

`CliMessageCode` contains one code for each parse issue using the `cli.` prefix
and kebab-case issue value, plus `cli.missing-operation` for bare-group help.
The names and values are not restated in renderers or tests as raw control
strings.

Suggestions are advisory and never execute. Production source owns one named
maximum and one deterministic confidence rule. Suggestions sort by confidence
and then public command or option order. A suggestion is a truthful correction
to a different accepted spelling; a candidate identical to the rejected token
is never a suggestion. Returning no suggestion is valid when no truthful
high-confidence correction exists.

### Help Data

`CliHelpData` contains exactly:

| Field             | Meaning                                                                                                                                                              |
| ----------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `path`            | Command words after `open-forge`; empty for root                                                                                                                     |
| `kind`            | One `CliHelpKind` value                                                                                                                                              |
| `summary`         | Concise purpose of the selected path                                                                                                                                 |
| `usage`           | One or more complete accepted invocation shapes                                                                                                                      |
| `arguments`       | Declared argument name, required state, variadic state, and description in declaration order                                                                         |
| `options`         | Options applicable to the selected path, including canonical name, optional alias, value grammar, repeatability, description, and documented default when one exists |
| `commands`        | Direct child command paths and summaries in public command-map order                                                                                                 |
| `examples`        | Representative complete invocations in authored order                                                                                                                |
| `relatedCommands` | Useful complete neighboring or next invocations in authored order                                                                                                    |

Arrays are present even when empty. A leaf exposes only options applicable to
that leaf. Root and group help expose options applicable to that exact parser
path and direct child discovery through `commands`; they do not imply that a
workspace or confirmation operation was selected.

Help metadata is the single input to human and JSON presentation. Human help is
rendered from these fields rather than captured from Commander's formatted
output. A bare group returns the same `CliHelpData` as explicit group help, but
adds `cli.missing-operation` and uses `invalid` status.

### Version Data

`CliVersionData` contains exactly:

| Field     | Meaning                                               |
| --------- | ----------------------------------------------------- |
| `name`    | Distributed package and binary identity, `open-forge` |
| `version` | Version embedded in the built artifact                |

Runtime, operating system, Framework payload identity, current directory, and
workspace state do not belong in version data. `status` exposes relevant build
and payload orientation.

Human presentation is:

```text
open-forge 0.0.0
```

### Parse Data

`CliParseData` contains exactly:

| Field   | Meaning                                                                               |
| ------- | ------------------------------------------------------------------------------------- |
| `path`  | Longest valid command prefix; empty when no command word matched                      |
| `issue` | One discriminated `CliParseIssue` variant with only the evidence required by its kind |
| `usage` | Complete valid invocation shapes for the nearest valid parser path                    |

The issue union uses the fields stated in the named-value table rather than one
object containing many unrelated optional properties. It never contains the
complete argv, environment, current directory, workspace reference, stack
trace, Commander error text, or library-specific error code.

Every `cli.parse` result contains one error-level message whose code corresponds
to `issue.kind`. Its human message names the rejected input and the accepted
shape. A high-confidence correction appears as a suggestion and remains
advisory.

### Streams And Completion

Parser-level results obey one deterministic stream contract:

| Presentation and result  | Standard output                                        | Standard error                                                                      |           Exit |
| ------------------------ | ------------------------------------------------------ | ----------------------------------------------------------------------------------- | -------------: |
| Human successful help    | Complete rendered help plus one trailing newline       | Empty                                                                               |            `0` |
| Human successful version | `open-forge <version>` plus one trailing newline       | Empty                                                                               |            `0` |
| Human invalid bare group | Empty                                                  | Missing-operation message followed by complete group help and one trailing newline  |            `2` |
| Human parse failure      | Empty                                                  | Complete diagnostic, suggestion when present, valid usage, and one trailing newline |            `2` |
| JSON parser-level result | One five-field JSON document plus one trailing newline | Empty                                                                               | Status-derived |

JSON never receives human help text as an opaque field. It serializes the same
typed help, version, or parse data used by the terminal renderer. Prompts,
progress, incidental logs, ANSI control sequences, and stack traces never
enter structured output.

The executable boundary writes each completed stream once and applies the
shared named exit mapping. Commander output and exit callbacks are intercepted
before they can write or terminate the process independently.

### Examples

```text
open-forge route --help
  -> cli.help, success
  -> group help on stdout
  -> stderr empty

open-forge route
  -> cli.help, invalid
  -> stdout empty
  -> missing operation and group help on stderr

open-forge stats
  -> cli.parse, invalid
  -> stdout empty
  -> unknown command and optional `status` suggestion on stderr

open-forge stats --json
  -> cli.parse, invalid
  -> one structured result on stdout
  -> stderr empty

open-forge status --yes
  -> cli.parse, invalid
  -> inapplicable-global issue before workspace selection
```

## Boundaries

The parser boundary handles invocation selection, help, version, and syntax
failure only. It never selects a workspace, invokes domain inspection, emits
raw Commander text as the public protocol, or turns a likely correction into
execution. Once valid syntax selects a leaf, semantic validation and failure
belong to that domain operation.

## Verification

Direct parser and registration tests prove operation selection, exact data,
ordering, option applicability, aliases, double-dash enforcement, syntax versus
semantic failure ownership, single-dash classification, truthful optional
suggestions, and suggestion non-execution. Display tests prove stream placement
and trailing-newline behavior. A small built-process journey proves
interception, JSON purity, and status-derived exits.

## Related Current Sources

- [CLI Interface](../interface.md)
- [CLI Architecture](../architecture.md)
- [Request construction](request-construction.md)
- [Result and display Pattern](../../../../../patterns/open-forge/cli/commands/result-display-boundary.md)
- [Named TypeScript values Pattern](../../../../../patterns/open-forge/typescript/named-values.md)
