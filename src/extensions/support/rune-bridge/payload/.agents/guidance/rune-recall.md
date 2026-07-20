---
open-forge:
  description: Choose between optional Rune-assisted recall and deterministic Open Forge routing without creating a second source of truth
  tags: [Extension, Guidance, Rune, Routing, Recall]
---

# Rune Recall

## Scenario

Relevant context may be distributed across a large workspace, and an installed Rune integration could help narrow the search.

## Preferred Approach

1. Confirm that Rune is available and consult the installed integration's own current documentation.
2. Use Rune-assisted recall for broad relevance discovery when that capability is documented and useful.
3. Open the returned workspace files and follow Open Forge Required Routes, directives, tags, paths, and entrypoints directly.
4. Use deterministic Open Forge routing or ordinary workspace search when exact coverage, mandatory instruction loading, or verification matters.
5. Treat Rune-derived data as disposable and rebuildable; keep authoritative content in its existing Open Forge or project owner.

## Reasoning

Relevance assistance can reduce discovery cost, but mandatory behavior and accepted truth must remain inspectable in plain workspace files. Verifying recalled results against their owners prevents a convenience index from becoming hidden authority.

## Tradeoffs

- Rune-assisted recall may find non-obvious context faster, but availability, freshness, and completeness can vary.
- Deterministic routing is more predictable and auditable, but broad discovery can require more manual search.
- Maintaining one source of truth avoids drift, while derived relevance data may need rebuilding before it is useful.

This guidance intentionally defines no Rune command, configuration, installation, or storage contract.
