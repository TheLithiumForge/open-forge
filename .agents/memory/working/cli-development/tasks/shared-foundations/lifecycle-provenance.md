---
open-forge:
  description: Add exact per-target canonical source-asset provenance to Framework lifecycle schema v1
  tags: [Memory, Working, CLI, Task, Foundation, Framework, Lifecycle, Provenance, Contextual]
---

# Add Framework Lifecycle Source-Asset Provenance

## Task State

- State: Ready after contract-freeze integration.
- Parent: [Next-Wave Shared Foundations](_shared-foundations.md).
- Consumers: root Install, root Update, Framework-aware Route Init, and later
  lifecycle-release design.

## Outcome

Every `FrameworkLifecycleTarget` has required nullable JSON
`sourceAssetPath`. Payload files and managed root/provider blocks record their
normalized canonical embedded asset-relative source path. Derived generated-
region targets record `null`. Schema remains version 1.

## Architecture And Ownership

- Production: `Framework/Lifecycle/**` and focused lifecycle tests only.
- Structural validation accepts normalized historical non-null source paths even
  when the current payload no longer contains them. Publishing operations must
  separately verify new non-null paths against their exact current inventory.
- User-owned scope entrypoints are never Framework lifecycle targets.
- Keep the existing single Framework lifecycle state, targets, and generated
  regions. Do not add instance collections, root/scoped sections, migration
  machinery, or command policy.
- Route Remove remains positive-unmanaged-only. Provenance does not grant release
  authority.

## Evidence

Cover source-generated JSON property presence/order/nullability, valid payload and
managed-block paths, derived-region `null`, unsafe/noncanonical rejection,
historical missing-current-asset acceptance, duplicate/cross-section validation,
and full existing lifecycle regressions. Run the affected Native AOT
serialization and lifecycle boundary.

## Stop Conditions

Stop if one target cannot be identified by concrete path, source asset path,
region/generated-region identity, and baseline fingerprint; if a schema bump or
migration appears necessary; or if any feature addition/removal lacks maintainer
acceptance.
