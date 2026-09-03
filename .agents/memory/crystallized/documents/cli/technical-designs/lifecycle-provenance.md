---
open-forge:
  description: Exact Framework lifecycle schema-v1 target provenance and identity design
  responsibility: Define how lifecycle targets retain canonical embedded-asset provenance without adding lifecycle instances or migration
  tags: [Memory, Crystallized, Document, CurrentTruth, Evergreen, CLI, TechnicalDesign, Framework, Lifecycle, Provenance]
---

# Lifecycle Provenance Technical Design

## Boundary

Framework lifecycle state remains one `.agents/open-forge.lifecycle.json`
document at schema version 1. This design defines exact per-target source-asset
provenance and identity realization. Command contracts retain creation,
reconciliation, release, and consumer policy.

## Source-Asset Provenance

Every `FrameworkLifecycleTarget` has required nullable JSON
`sourceAssetPath`. A payload file or managed root/provider block records the
normalized canonical embedded asset-relative path that produced it. A derived
generated-region target records `null`.

Structural validation accepts a normalized historical non-null source path even
when the current embedded inventory no longer contains it. A publishing
operation separately verifies every new non-null path against the exact embedded
inventory it records. User-owned scope entrypoints are never Framework lifecycle
targets.

## Target Identity

Exact concrete target path, source asset path, optional managed region,
generated-region identity, fingerprint kind, and baseline fingerprint provide
per-effect identity and provenance. The Framework section retains its one
embedded-source identity, ordered managed targets, and ordered generated-region
identities. Source identity retains its ID, optional descriptive version, and
lowercase SHA-256 inventory fingerprint.

The schema retains the existing single Framework state. It adds no lifecycle-
instance collection, root/scoped duplicate sections, migration engine, command
history, recovery receipt, or continuing Template ownership.

## Serialization And Validation

Lifecycle JSON uses the existing source-generated schema-v1 graph. Evidence
proves property presence, order, and nullability; normalized canonical payload
paths; derived-region `null`; unsafe and noncanonical rejection; acceptance of a
historical path missing from the current inventory; duplicate and cross-section
validation; and Native AOT serialization with reflection disabled.

## Related Current Sources

- [CLI Architecture](../architecture.md)
- [Embedded Payload Technical Design](embedded-payload.md)
- [Mutation And Recovery Technical Design](mutation-and-recovery.md)
- [Install Contract](../contracts/install/_install.md)
- [Update Contract](../contracts/update/_update.md)
- [Route Init Contract](../contracts/route/init/_init.md)
