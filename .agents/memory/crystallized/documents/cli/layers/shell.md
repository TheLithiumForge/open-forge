---
open-forge:
  description: The Shell layer - the process boundary that turns arguments into a request and a result into an exit code
  responsibility: Define parsing, binding selection, invocation, presentation settings, stream routing, the execution pipeline, and process completion for the replacement CLI
  tags: [Memory, Crystallized, Document, CurrentTruth, Evergreen, CLI, Architecture, Shell, Parsing, Pipeline]
---

# Shell Layer

Shell is the process boundary. Arguments enter, an exit code leaves, and
nothing in between knows what a route or a Markdown document is.

Shell answers **"what was asked, and how does the answer leave the process?"**
It owns no Framework-domain behavior and never owns a command payload. The
[land Architecture](../architecture.md) records how this layer sits against
the others.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Parsing And Invocation

`System.CommandLine` exclusively owns command selection, arity, occurrence
aggregation, typed conversion, unknown symbols, parser diagnostics, standard
syntax help, and version dispatch.

The implementation performs one parse, validates its typed facts, resolves
terminal conflicts, selects the exact binding, handles help or version,
normalizes one global invocation and optional workspace, and forms one
command-local request or concrete invalid result.

Global presentation settings are typed and explicit: `--format text|json`
selects the primary representation, `--detail minimal|standard|full|debug`
selects the detail level, and repeatable `--detail-filter
<error|warning|info|all>` selects the listed severities. `text` and `minimal`
are the defaults. Repeating `--format` or `--detail` is invalid. The normalized
`CliPresentation` contains `Format`, `Detail`, `Filter`, and `Colors`.

Ordinary long-option values use the parser's native space, equals and colon
delimiters. This includes Find and Route tags and Route List depth. There is no
raw delimiter-policy scan or policy shared across unrelated commands.

The implementation does not rescan raw arguments for facts exposed by the parse
tree. Route Update alone retains the attached-empty responsibility recognizer
bounded by its [Technical Design](../contracts/route/update/technical-design.md),
because that distinction is erased by the pinned parser. It does not parse
values, count occurrences, select commands, or produce parser diagnostics.

`CliInvocation` contains normalized process-wide facts only. A command request
is complete and immutable. Neither carries `ParseResult`, parser symbols,
writers, service collections, raw arguments, or an unrelated context bag.
Workspace-free commands preserve genuine workspace absence. Workspace-aware
commands receive one selected normalized workspace before domain work.

## Shell Definitions And Composition

Typed definitions own every executable, command, argument, option, finite value,
machine code, result-command, and next-action identity exactly once.

Global definitions are split by responsibility: syntax and executable identity;
format, detail, severity filter, and output targets; workspace selection;
terminal modes and conflict policy; semantic statuses and process exits; process
completion and output disposition; and parser and shell error identities. Each
command owns its group, leaf, operands, local options, finite values, finding
codes, result command name, and next-action contents.

One closed generic binding owns one request/result pair. A non-generic boundary
stores heterogeneous bindings without erasing concrete operation or
serialization types. Dispatch uses exact `System.CommandLine.Command` identity,
never strings. No command binding locates services. The composition root
supplies complete immutable dependencies through direct construction or narrow
capability records.

The operational contributor catalogue follows the same explicit-composition
direction. Each producer owns its contributor and typed observation. Status and
Doctor consume only their narrow views. Neither command locates producers or
receives a broad service collection, registry, or generic operational context.

## Native Interaction

`Shell/Interaction/` owns the neutral `CliTerminal` transport. The host supplies
`CanPrompt`, `CanReadKeys`, and `CanRedraw`, plus `WriteAsync` to stderr,
`ReadKeyAsync`, `ReadLineAsync`, and caller cancellation. `CanPrompt` requires
standard input and the stderr prompt stream to be terminal-capable. Key reads
also require a supported non-`dumb` terminal; redraw requires a known ANSI host
policy. The libraries never read `Console` directly.

Prompt meaning and rendering live in
`Presentation/Shared/Prompts/CliPrompts`. Prompts go to stderr so stdout
remains one human result or one JSON document. Arrow-key single select,
checkbox multi-select with dependency marks and a legend, numbered line-mode
fallback, and plan review before confirmation are Presentation concerns. The
transport owns no command questions, candidates, defaults, validation, retry
policy, confirmation meaning, or result.

JSON, `--automatic`, and redirected operation never prompt. When the terminal
cannot prompt, the command reports the applicable confirmation, selection or
permission finding and its flag. Unrelated commands and requests gain no
interaction or stream parameter.

## Execution Pipeline

The host composes immutable messages and directly callable stages. Neutral
operation, presentation-validation and completion stages live in Shell;
rendering lives in Rendering and stream writing lives in the host:

```text
CliInvocationResolution
  -> CliOperationRequest<TRequest>
  -> CliOperationResult<TResult>
  -> CliReport<TData>            (command selector, then trimmer)
  -> CliRenderedOutput            (text or JSON, plus diagnostics)
  -> CliOutputReceipt
  -> CliProcessCompletion
```

Each stage validates its input before invoking an operation, selector, renderer,
or writer. Direct stage entry remains testable. Unknown finite values fail
closed.

The report-selection stage reads the complete concrete result and the
normalized `CliPresentation` settings. It performs no formatting, reads no
file, invokes no operation, and never changes a fact. The selector first builds
the command-owned report; the trimmer then applies the shared detail and
severity rules. Shell owns this position in the chain; what it selects is
defined by [the Presentation layer](presentation.md#report-model-and-report-selection).

The pipeline invokes one operation at most once, propagates caller
cancellation, never reruns work during report selection, rendering or output,
selects one cached concrete command data renderer, writes one primary result
and at most one bounded diagnostic projection, and returns one fixed process
completion from the concrete semantic status.

`ICliCommandResult` exposes only shared process facts needed by the pipeline:
command identity, semantic status, workspace presence, and next-action presence.
Concrete result records retain complete command payloads and serialize through
concrete source-generated metadata. The interface is never a wire type.

## Result JSON Coordinates And Process Status

The shared [Result Coordinates Interface
Contract](../contracts/shared/result-coordinates/interface.md) defines one
schema-3 envelope, source-location coordinates, semantic statuses, numeric
exits, primary streams, and compatibility. Its [Behavior
Contract](../contracts/shared/result-coordinates/behavior.md) defines
technology-neutral formation and conformance.

JSON is one minified document on stdout for every semantic status. It carries
the renamed status token, the requested detail and filter, workspace, summary,
root findings, effects, counts, limitations, command-owned data, recovery and
the next action. Command data is serialized through the concrete command's
source-generated context. Members not selected by detail are absent, not empty
placeholders. Parser failures before binding remain text on stderr and have no
envelope.

Text uses the following shared process mapping:

| Semantic status           | Exit | Text primary stream |
| ------------------------- | ---: | ------------------- |
| `completed`               |    0 | stdout              |
| `completed-with-warnings` |    2 | stdout              |
| `incomplete`              |    3 | stdout              |
| `invalid-input`           |    4 | stderr              |
| `blocked`                 |    5 | stderr              |
| `failed`                  |    1 | stderr              |
| `cancelled`               |  130 | stderr              |

The numeric exits are unchanged from the former process mapping. Diagnostics
and prompts always use stderr. JSON keeps stdout as its sole primary stream,
including for invalid, blocked, failed and cancelled results. Read-only
commands do not print `No files were changed.`
