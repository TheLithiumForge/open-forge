---
open-forge:
  description: Start an optional implementation design that traces choices to accepted command contracts and Architecture
  tags: [Template, CLI, Command, Contract, TechnicalDesign, Implementation, Traceability]
---

# `{command-path}` Technical Design

{
Use when concrete implementation choices need a visible source: which technology and structure satisfy the accepted contracts, and which choices remain open?

Do not create this file when the contracts are complete and no concrete
implementation choice needs a visible source. Copy it to
`contracts/{command-path}/technical-design.md` only when technical content
exists. Replace metadata, including removing the `Template` tag, so it describes
the independent file's scope, state, and authority. This is a copy-ready
starter, not a current command instance. Later Template changes do not update it. Replace the title, links, link depth, and prompts. Keep each design explanation in one section, with a short traceability link. Add rows or subsections only for distinct choices. Do not add permanent requirement IDs. Keep accepted Architecture and command contracts authoritative. Use the states defined below. Remove this guidance and unused optional sections while preserving applicable design responsibilities.
}

## Status

{Use one explicit state for each choice:

- `accepted` means the direction has been accepted by the applicable authority.
- `experimental` means the direction is being tried or investigated.
- `recommended` means the direction is proposed but not accepted.
- `open` means the choice remains unresolved.
- `pending executable proof` means the choice is accepted but its executable
  evidence is not yet available.

Keep states in the traceability table. Add an overall lifecycle note here only when useful. Explain unresolved choices and their acceptance boundaries under Open Decisions.}

## Design Boundary

Technical Design records implementation choices for one command, component, or
shared scope. It is subordinate to accepted Architecture and cannot replace,
reopen, or contradict Architecture or change the Interface or Behavior
contracts.

## Contract Traceability

| Contract fact or section           | Design response         | State                                                                    |
| ---------------------------------- | ----------------------- | ------------------------------------------------------------------------ |
| {Exact Interface or Behavior link} | {implementation choice} | {accepted, experimental, recommended, open, or pending executable proof} |

## Runtime And Dependencies

{Name accepted runtimes, libraries, parsers, serializers, filesystem APIs, and external tools with material constraints, tradeoffs, and available or pending executable proof. Link from traceability rather than repeating the explanation.}

## Source Structure

{Describe command-local and shared modules, local dependency direction, typed data
boundaries, and why support belongs at this scope. Keep accepted system
structure, dependency direction, cross-cutting boundaries, and file/module
organization aligned with Architecture.}

## Algorithms And Data

{Describe implementation algorithms, data structures, schemas, comparison APIs,
and persistence or backup forms. Do not redefine contract semantics.}

## Verification Design

{Map applicable `Unit`, `Integration`, `EndToEnd`, and `PackageEndToEnd` evidence,
including any performance or diagnostic evidence, to exact contract facts or
sections.}

## Open Decisions

{OPTIONAL: Keep unresolved choices here, with traceability links rather than duplicate explanations. Accepted choices awaiting executable proof stay with their design response.}

- {Unresolved choice, alternatives, evidence needed, and acceptance boundary.}

## Related Sources

- {Interface Contract in the destination}
- {Behavior Contract in the destination}
- {Accepted Architecture in the destination}
