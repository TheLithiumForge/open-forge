---
open-forge:
  description: "Historical CLI-v2 source: Replacement handlers return one five-field typed result while display adapters render human or JSON output through named protocol values and exact centralized exit semantics"
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Result And Display Boundary

## Context

The MVP writes human text and JSON directly throughout command behavior and assigns numeric process status in multiple places. The replacement must serve agents and people without coupling operations to terminal state, duplicating behavior between presentations, or requiring consumers to interpret unexplained strings and numbers.

A common result could also become too broad. Making provenance, effects, recovery, workspace facts, and every possible finding optional at the top level would create a weak schema that tells each operation little and forces every consumer to inspect empty fields.

## Decision

Named handlers return typed results and perform no final display or process completion. They do not call `console`, write output streams, select presentation, or set `process.exitCode`.

The common result envelope contains five top-level responsibilities:

```text
schemaVersion
operation
status
messages
data
```

The envelope stays small instead of accumulating every operation's workspace,
finding, mutation, provenance, or recovery evidence as optional top-level
fields. Operation-specific `data` composes focused shared projections only
when that operation has the corresponding evidence.

One executable boundary selects the human or JSON adapter and maps semantic
status to process completion. Protocol values are named once and shared by
source, documentation, and tests. The [CLI Interface](../../documents/cli/interface.md#results-and-presentation)
is authoritative for the exact schema, statuses, messages, version behavior,
and exit mapping. The [mutation execution contract](../../documents/cli/contracts/mutation-execution.md)
owns mutation evidence semantics.

## Rationale

Five required fields make every result predictable without forcing unrelated domain concerns into a giant optional object. Operation-specific data preserves strong local types and lets a command evolve without weakening every other result.

A shared workspace reference gives agents and people unambiguous target evidence without promoting workspace state into the universal envelope. Recording the selection source also makes the absence of hidden discovery observable.

Typed results keep direct handler tests independent of captured console state. Separate display adapters make human and agent presentations consistent without creating separate behavior paths.

Named protocol values keep source, JSON, tests, and documentation readable. They also make changing or reviewing a value deliberate because one definition owns it.

The simple exit sequence is easier to remember than platform-specific symbolic conventions:

- Zero means complete and clean.
- One means complete with unresolved attention.
- Two through four distinguish invalid, blocked, and failed.
- 130 retains the familiar caught-interruption meaning.

## Alternatives And Tradeoffs

- One operation-specific top-level shape per command would preserve local typing but make generic display, transport, and automation unnecessarily inconsistent.
- One giant universal result would centralize every field but accumulate optional properties that most commands cannot populate.
- An `ok` boolean would simplify some checks but duplicate status and make `attention` ambiguous.
- Including numeric exit status in JSON would preserve process metadata when relayed, but semantic status already carries the portable meaning.
- Returning zero for `attention` would describe successful execution but allow unhealthy workspaces and incomplete authoring to pass ordinary pipelines silently.
- BSD `sysexits` values would reuse a Unix convention but remain obscure to most agents, Windows users, and ordinary CLI consumers.
- Semantic-version strings for the envelope would imply independent minor and patch compatibility machinery that is not needed yet.

## Consequences

- The executable boundary alone owns final stream writes and process
  completion.
- Operation-specific data and display remain local without weakening the common
  envelope.
- Progress, prompts, and presentation cannot become alternate operation paths.
- Exact public protocol values change through the Interface and production
  contract, not by expanding this rationale record.

## Authoritative Sources

- [Open Forge CLI Interface](../../documents/cli/interface.md)
- [Open Forge CLI Architecture](../../documents/cli/architecture.md)
- [CLI result and display Pattern](../../../../patterns/open-forge/cli/commands/result-display-boundary.md)
- [Named TypeScript values Pattern](../../../../patterns/open-forge/typescript/named-values.md)

## Evidence And Context

- [CLI mutation execution contract](../../documents/cli/contracts/mutation-execution.md)

## Decision Relationships

- [CLI command surface](cli-command-surface.md)
- [CLI command framework](cli-command-framework.md)
- [CLI source locality](cli-source-locality.md)
- [CLI testing architecture](cli-testing-architecture.md)
