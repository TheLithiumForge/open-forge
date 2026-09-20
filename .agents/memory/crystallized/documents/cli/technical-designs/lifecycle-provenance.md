---
open-forge:
  description: Ownership receipts and operation-time Framework source alignment
  responsibility: Define stored ownership separately from current payload comparison and mutation evidence
  tags: [Memory, Crystallized, Document, CurrentTruth, Evergreen, CLI, TechnicalDesign, Framework, Lifecycle, Provenance]
---

# Ownership And Source Alignment Technical Design

## Boundary

The generated `.agents/open-forge.lock.json` records Framework, Extension and
Library ownership through the shared source-generated document. The authored
`.agents/open-forge.json` owns settings. The lock is best-effort bookkeeping;
missing, stale or unreadable ownership is reported without treating it as an
integrity gate. Old state files are unrelated user content, with no migration,
legacy reader or automatic deletion.

## Stored Ownership

Framework ownership stores source ID and optional descriptive version, whole
paths and named regions. Extension receipts add package IDs, source locations
and dependencies. Library registrations store source root, destination root and
source-relative paths. Each receipt corresponds to effects that actually
verified. A whole-path receipt may support deletion within its admitted physical
boundary; a region receipt cannot authorize deletion of its host file.

The lock stores no target baseline, fingerprint kind, fingerprint policy,
workspace binding, per-target source asset provenance, command history or
recovery evidence. Generated Entries region ownership survives; the duplicate
stored fingerprint of generated content does not.

## Current Source Alignment And Comparison

Distribution aligns owned Framework destinations with the running payload,
including scoped entrypoint mappings. Alignment is computed for the operation
and is never a stored cross-release integrity claim. A retired destination may
have no current payload asset. Current bytes and intended payload bytes are
compared within the same invocation using Markdown semantic identity where
applicable; opaque files and generated navigation use their local exact-byte
rules. Root managed blocks preserve surrounding authored host content.

The mutation plan retains exact expected current bytes, intended changes and
recovery targets. Those operation-time facts support revalidation, application
and verification; the ownership lock does not replace them. Recovery bundles
are prepared before reversible effects and retained where the command contract
requires review of previous content.

## Serialization And Validation

The existing source-generated ownership codec writes deterministic whole-document
UTF-8. Equal intended ownership is unchanged. Unknown members are not retained
when a real change rewrites the machine-owned file. Readers tolerate unknown
shape and report unusable ownership; command-specific destination, shared
allow-list, collision and physical checks remain mandatory before any effect.
Native AOT uses the same typed serialization without reflection.

## Related Current Sources

- [CLI Architecture](../architecture.md)
- [Embedded Payload Technical Design](embedded-payload.md)
- [Mutation And Recovery Technical Design](mutation-and-recovery.md)
- [Install Contract](../contracts/install/_install.md)
- [Update Contract](../contracts/update/_update.md)
- [Route Init Contract](../contracts/route/init/_init.md)
