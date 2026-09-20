---
open-forge:
  description: Copy-ready document files for a command-local CLI contract set under contracts/{command-path}
  tags: [Template, CLI, Document, Command, Contract, Interface, Behavior, TechnicalDesign]
---

# CLI Contract Document Templates

These files are copy-ready Templates, not current Documents or authoritative
sources. Copy and adapt only what the destination needs into one command-local
routed scope under `contracts/{command-path}`. The created files become
independent, and later Template changes do not update them. The starters contain
their own responsibilities and do not depend on transient task state.

After command contracts are defined, maintainers must reconcile these Templates
against real use. Obsolete or unhelpful starters or sections may be removed, and
unclear parts may be revised. This reconciliation does not imply that every
command needs a Technical Design. Add one only when concrete implementation
choices need a visible source. A Technical Design remains subordinate to
accepted Architecture and the Interface and Behavior Contracts.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [Start a technology-neutral command behavior definition for deterministic resolution, effects, safety, recovery, and conformance](behavior.md) - #Template #CLI #Command #Contract #Behavior
- [Start the routed command scope that exposes one local Interface, Behavior, and optional Technical Design set](entrypoint.md) - #Template #CLI #Command #Contract #Entrypoint #Routing
- [Start a complete public command surface with explicit inputs, composition, outputs, results, errors, and scenarios](interface.md) - #Template #CLI #Command #Contract #Interface
- [Start an optional implementation design that traces choices to accepted command contracts and Architecture](technical-design.md) - #Template #CLI #Command #Contract #TechnicalDesign #Implementation #Traceability
