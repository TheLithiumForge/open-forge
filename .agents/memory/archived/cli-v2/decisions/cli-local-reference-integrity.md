---
open-forge:
  description: "Historical CLI-v2 source: Inspect local Markdown references once, keep traversal separate from loading, and repair only identity-preserving corrections"
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Local Reference Integrity

## Context

Route inspection, context, and Doctor need consistent reference facts without
turning ordinary links into a hidden loading graph. Raw filesystem identifiers
also proved unreliable as universal traversal identity on the supported
runtime matrix.

## Decision

Build one invocation-local document-fact and reference inventory. Resolve
contained local destinations from their source, key traversal by canonical
physical path, keep ordinary links separate from Framework loading, never
fetch network destinations, and repair only corrections that preserve proven
authored identity.

Use a small bounded scanner behind stable document facts. Keep its parser
representation private so a later audited syntax-tree parser can replace it
without changing consumers. Use `github-slugger` behind a workspace-owned
fragment boundary for Unicode-compatible GitHub heading fragments.

The [Local Reference
Contract](../../documents/cli/contracts/local-references.md) owns exact current
scanner, destination, traversal, consumer, status, and repair semantics.

## Rationale

One inventory prevents consumers from disagreeing about source positions,
fragments, containment, or cycles. A bounded scanner avoids a broad Markdown
and HTML dependency while retaining a replaceable domain boundary. Canonical
physical paths terminate traversal without trusting imprecise runtime file
identifiers.

## Alternatives And Tradeoffs

- Following links during context loading would recreate hidden transitive dependencies.
- Separate parsers per consumer would drift.
- A complete CommonMark syntax tree would add unused behavior and dependencies initially.
- Regular expressions over complete Markdown would be brittle and obscure ignored regions.
- File identifiers alone can collapse distinct documents when runtime precision is insufficient.

The initial scanner deliberately reports unsupported constructs as incomplete
or unverified rather than guessing them into validity.

## Evidence

An archived CommonMark candidate ran under Node.js, Bun, and Deno but did not
demonstrate that its broader dependency graph was necessary. Repository-scale
inventory produced the same reference results across runtimes and exposed the
cost of repeated sequential physical resolution, supporting shared evidence
and bounded concurrency.

## Consequences

- Document inspection remains bounded and independent of rendering.
- Context diagnoses outgoing links without loading their bodies.
- Doctor can inspect contained local references outside `.agents` without becoming a network validator.
- Cycles terminate without becoming defects.
- Missing or ambiguous targets remain authored decisions rather than unsafe fixes.

## Related Sources

- [Local Reference Contract](../../documents/cli/contracts/local-references.md)
- [Markdown Document Facts Pattern](../../../../patterns/open-forge/cli/markdown/markdown-document-facts.md)
- [Local Reference Inventory Pattern](../../../../patterns/open-forge/cli/markdown/local-reference-inventory.md)
