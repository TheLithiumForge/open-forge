---
open-forge:
  description: Preserve future signatures, SBOM, provenance, and OIDC expansion behind explicit acceptance
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Distribution, SupplyChain, Security]
---

# Future Supply-Chain Expansion

## Task State

- State: Deferred outside current D1. This record is not selected and creates no
  present release requirement.
- Parent: [CLI Delivery](_delivery.md).

## Expected Outcome

Current D1 owns checksums in [Current Native CI](02-native-ci.md). Signatures,
SBOM, provenance, OIDC attestation, and any expanded RID inventory require a
later explicit maintainer decision before this Task may become Ready.

## Requirements

Before future activation, the accepting decision must freeze:

- the exact new artifact/RID inventory and threat or compliance requirement;
- signature and identity provider, key or OIDC trust and rotation boundaries;
- SBOM format, dependency/component scope, and license policy;
- provenance schema, workflow/runner authority, and privacy boundary;
- verification, tamper, independent-job, documentation, and maintenance evidence;
  and
- its interaction with the already accepted checksum manifest.

## Evidence

No signature, SBOM, provenance, OIDC, attestation, expanded-RID, or support-floor
evidence is required or claimed by current D1. A future accepted expansion must
define and then prove its own tamper, inventory, version, security, and license
conditions.

## Stop Conditions

Stop before treating this deferred design sketch as accepted scope, introducing
credentials or trust infrastructure, widening the supported RID matrix, or
adding a shipping claim without the explicit decision and authority updates
above.
