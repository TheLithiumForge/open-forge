---
open-forge:
  description: Start an optional implementation design that traces choices to accepted command contracts and Architecture
  tags: [Template, CLI, Command, Contract, TechnicalDesign, Implementation, Traceability]
---

# `{command-path}` Technical Design

{
Template selection:

- Need: Concrete implementation choices for one command or shared command scope need a visible design source.
- Primary question: Which technology and source structure should satisfy the accepted Interface and Behavior contracts, and which choices remain open?

Do not create this file when the contracts are complete and no concrete
implementation choice needs a visible source. Copy it to
`contracts/{command-path}/technical-design.md` only when technical content
exists. Replace metadata, including removing the `Template` tag, so it describes
the independent file's scope, state, and authority. This is a copy-ready
starter, not a current command instance. Copying it creates an independent file;
later Template changes do not update that file. Replace the title, links, link
depth, and placeholders. Repeat rows and subsections for every applicable design
choice and unresolved decision. Do not add permanent requirement IDs. Keep
accepted Architecture and command contracts authoritative. Use the distinct
states `accepted`, `experimental`, `recommended`, `open`, and `pending executable
proof`; do not treat pending proof as an open choice. Remove this braced source
guidance.
}

## Status

{Use one explicit state for each choice:

- `accepted` means the direction has been accepted by the applicable authority.
- `experimental` means the direction is being tried or investigated.
- `recommended` means the direction is proposed but not accepted.
- `open` means the choice remains unresolved.
- `pending executable proof` means the choice is accepted but its executable
  evidence is not yet available.

Keep these states distinct. State any unresolved choice and its acceptance
boundary.}

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

{Name accepted runtimes, libraries, parsers, serializers, filesystem APIs, and
external tools. State which choices have executable proof and which proof remains
pending, with material constraints and tradeoffs.}

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

- {State the unresolved choice, alternatives, evidence needed, and acceptance boundary.}

{Use `open` only for decisions that remain unresolved. Keep `experimental` and
`recommended` distinct from accepted direction. Keep an accepted choice whose
evidence is still missing in `pending executable proof`, not `open`.}

## Related Sources

- {Interface Contract in the destination}
- {Behavior Contract in the destination}
- {Accepted Architecture in the destination}
