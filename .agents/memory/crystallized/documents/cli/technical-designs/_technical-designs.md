---
open-forge:
  description: Concrete designs for shared replacement CLI capabilities whose exact realization does not belong in system Architecture
  responsibility: Route accepted shared-capability Technical Designs without changing their contracts or program state
  tags: [Memory, Crystallized, Document, CurrentTruth, Evergreen, CLI, TechnicalDesign, Implementation]
---

# CLI Technical Designs

These Technical Designs define exact realization for shared replacement CLI
capabilities. They remain subordinate to the [CLI Architecture](../architecture.md)
and to the shared and command-local Interface and Behavior contracts. They do
not record active delivery state, completion receipts, commit identities, or
public behavior that belongs to those sources.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [Exact lease-bound ordinary-BCL directory-create effect design](directory-creation.md) - #Memory #Crystallized #Document #CurrentTruth #Evergreen #CLI #TechnicalDesign #Mutation #Directory #Filesystem
- [Define automatic embedded Framework and Extension resources, source identity, hashes, and parity evidence](embedded-payload.md) - #Memory #Crystallized #Document #CurrentTruth #Evergreen #CLI #TechnicalDesign #Framework #Distribution #EmbeddedResource
- [Exact shared formation and projection design for Generated Navigation](generated-navigation.md) - #Memory #Crystallized #Document #CurrentTruth #Evergreen #CLI #TechnicalDesign #GeneratedNavigation #Routing
- [Ownership receipts and operation-time Framework source alignment](lifecycle-provenance.md) - #Memory #Crystallized #Document #CurrentTruth #Evergreen #CLI #TechnicalDesign #Framework #Lifecycle #Provenance
- [Exact BCL-first locking, recovery-bundle, expected-state, atomic-file, receipt, and guarded-deletion design](mutation-and-recovery.md) - #Memory #Crystallized #Document #CurrentTruth #Evergreen #CLI #TechnicalDesign #Mutation #Recovery #Filesystem #Lock
- [Exact local-only Workspace Libraries record, inventory, projection, and recovery design](workspace-libraries.md) - #Memory #Crystallized #Document #CurrentTruth #Evergreen #CLI #TechnicalDesign #Framework #Library #Workspace #Filesystem #Mutation #Recovery
- [Realize shared destination admission through authored settings and existing mutation receipts](workspace-permissions.md) - #Memory #Crystallized #CLI #TechnicalDesign #Permissions #Settings #CurrentTruth
