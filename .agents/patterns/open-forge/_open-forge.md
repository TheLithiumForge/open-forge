---
open-forge:
  description: Reusable structural patterns for changing and operating Open Forge without losing its minimal routed design
  tags: [Pattern, Framework, Dogfood]
---

# Open Forge Patterns

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [Reusable, product-agnostic CLI operation shapes for predictable typed interfaces](cli/_cli.md) - #Pattern #CLI #Interface #Command #Flag #Predictability #Testing
- [Structure current maintenance documents around their source, maintainer contract, and verification](maintenance-contract.md) - #Pattern #Framework #Maintenance #Governance #Documentation
- [Keep canonical root instructions and harness bridges safely replaceable inside one workspace-owned file](managed-root-entry.md) - #Pattern #Framework #Entry #Bridge #Installation #Safety
- [Reusable TypeScript module and source-locality shapes for readable strict ESM code](typescript/_typescript.md) - #Pattern #TypeScript #ESM #Modularity #Locality #Readability
