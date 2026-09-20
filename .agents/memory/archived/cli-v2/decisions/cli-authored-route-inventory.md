---
open-forge:
  description: "Historical CLI-v2 source: Build replacement CLI routing from authored topology while treating generated Entries as a verified derived projection"
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Authored Route Inventory

## Context

Several commands need the same route facts. Reconstructing them separately
would create incompatible identity, metadata, traversal, loading, and stale
index behavior. Generated `Entries` can be stale, while raw directory names
cannot express the Framework's complete routed meaning.

## Decision

Build one invocation-local inventory from authored topology and compare its
expected generated navigation with the current projection. Consumers share
that inventory instead of treating generated `Entries` as the graph or
rediscovering topology privately.

Use natural exact route identities independently from workspace-relative
source paths. Parse only the bounded Open Forge metadata contract behind a
focused result boundary, use the accepted YAML syntax implementation privately
inside that boundary, and use locale-independent portable ordering.

The [Route Inventory
Contract](../../documents/cli/contracts/route-inventory.md) owns the exact current
topology, identity, metadata, ordering, projection, compatibility, and invalid
state semantics.

## Rationale

Authored sources remain recoverable when derived navigation is stale. One read
model prevents commands from disagreeing about scope, overwrites, loading,
metadata, or route identity. A focused metadata boundary preserves strictness
while a mature YAML syntax implementation handles formatter-compatible lexical
forms without granting arbitrary YAML behavior Framework meaning.

## Alternatives And Tradeoffs

- Using generated `Entries` as the graph would let stale output hide or retain authored routes.
- Traversing only directory names would lose entrypoint, scope, loading, Skill, and overwrite meaning.
- Exposing a general YAML value model would provide unused semantics and a
  larger public boundary. The accepted private YAML syntax parser remains
  constrained by Open Forge validation and parser-independent facts.
- Runtime locale ordering would produce environment-dependent derived output.

The accepted inventory does more initial work than trusting generated output,
but every route-oriented consumer reuses that work.

## Evidence

The archived focused spike proved the inventory and metadata boundary under
Node.js, Bun, and Deno. It recovered authored routes from stale generated
state, exposed source defects hidden by the frozen parser, and demonstrated
portable ordering differences. Repository-scale timings established evidence
for bounded implementation work, not production limits.

## Consequences

- Broken derived navigation cannot silently remove authored truth.
- Route-oriented commands share one identity and fact model.
- Compatibility input remains input-only and is never emitted as canonical output.
- Derived ordering changes once under the replacement and remains portable afterward.

## Related Sources

- [Route Inventory Contract](../../documents/cli/contracts/route-inventory.md)
- [Route Inventory Projection Pattern](../../../../patterns/open-forge/cli/markdown/route-inventory-projection.md)
- [Markdown Document Facts Pattern](../../../../patterns/open-forge/cli/markdown/markdown-document-facts.md)
- [Frontmatter YAML boundary](cli-frontmatter-yaml-boundary.md)
