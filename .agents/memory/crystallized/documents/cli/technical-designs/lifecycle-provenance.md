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
an absent ownership file is known empty. Malformed, stale, unreadable, or
unsupported facts do not become an unrelated-effects integrity gate, and they
do not license adoption when a competing ownership claim cannot be ruled out.
Old state files are unrelated user content, with no migration, legacy reader or
automatic deletion.

Framework Install and Update selection also reads the authored `removedFiles`
list. Its canonical workspace-relative paths match concrete Framework
destinations exactly, so a scoped copy sharing an embedded source remains
eligible unless its own destination is listed. An excluded existing destination
is omitted from comparison and write planning, leaving both its bytes and its
ownership facts unchanged. Missing excluded destinations remain missing until a
person removes the entry; absence alone never changes settings.

Explicit Install configuration can change those exclusions and restore missing
selected defaults under the [Install Behavior](../contracts/install/behavior.md#configuration-planning).
Omitting supplied defaults releases only their selected Framework receipts,
preserving existing content and other managers' claims. Configuration creates
no receipt merely because an existing file matches the current payload.

## Stored Ownership

The optional schema-1 `framework.gitIgnoredRoutes` array stores private directory
and shared direct-entrypoint pairs. Absent records mean no sharing policy.
These facts grant no whole-file or region ownership and no deletion authority.
Ownership rewrites preserve them. Explicit Install configuration updates them
before Gitignore takes effect, while Index, Install and Update generated
navigation omit the private contents. Local Context, Find and References retain
complete discovery.

Recognized malformed sharing records make the lock invalid. Index refuses an
invalid or unreadable existing lock to avoid publishing private filenames. This
bounded prerequisite does not change ordinary Install's best-effort receipt
behavior or make unknown schema versions a gate.

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

## Install User-Adoption Targets

Install keeps embedded payload-owned targets separate from user-adoption
targets. A migrated source and a newly created local resource entrypoint remain
user-owned. Migration rows are operation-result evidence, not ownership
receipts, history, or a new lock schema.

Install observes existing ownership before forming adoption candidates.
Unavailable or untrusted ownership facts do not authorize adoption. A known
competing claim retains its existing protection. A migrated source and newly
created local resource entrypoint remain user-owned, including when adopting
into an already managed root. A verified generated `Entries` edit may be
recorded as its own region receipt, but the host file does not become
whole-file Framework ownership because metadata or an entrypoint was completed.
When adoption changes navigation in an existing Framework host, its migration
row is operation-result evidence and preserves that host's existing ownership;
`UserOwnedPaths` does not enumerate every migration row. Schema-3
`data.classification` is `managed-adoption` only when an authored managed base
verifies and scoped adoption effects are planned. The exact no-effect result
remains `TrustedExact` internally and `trusted-exact` publicly. Classification
and migration facts are not serialized into the ownership lock.

## Update User-Adoption Targets

Update adoption is eligible only when a complete, trustworthy ownership
observation and an existing Framework registration establish the managed base.
Its targets are compatible documents under selected standard local routes.
Candidate admission applies `FrameworkPayloadSelection.IncludesPath` with its
complete file, directory, and category exclusions while retaining the full
source catalogue and strict projection inputs for validation. Adoption cannot
establish an absent, unknown, or incomplete Framework state.

The shared topology and document planners form local route facts and intended
document bytes before generated navigation projection. They complete only
missing required native Skill fields and required resource catalogue documents,
preserving optional and unknown frontmatter, body bytes, encoding, newlines,
and companions. Metadata and generated Entries changes for one path coalesce
into one exact physical effect. Local adopted files remain user-authored facts:
they have no Framework source-asset path, are not counted as bundled payload,
and receive no whole-file Framework receipt. A verified generated Entries
region may receive its own region receipt. A `NavigationUpdated` migration row
on an existing Framework host preserves that host's existing ownership and
does not make the host user-owned.

Update migration rows are operation-result evidence only. They are not
ownership receipts, history, or a new lock schema. Excluded, malformed,
foreign, ambiguous, and unsafe facts remain available to the ordinary strict
diagnostics and never become guessed ownership or effects.

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

For safe initial Install adoption and adoption from a verified managed base,
the prospective migrated source bytes and route facts are formed before
navigation projection. A managed-base admission independently verifies each
selected non-user payload source fingerprint and required managed block; a
planned generated navigation change may differ only within its verified
bounded region while authored identity matches. This leaves shared parser and
Index acceptance unchanged. User-adoption targets remain separate from payload
targets throughout planning, verification, and ownership publication.

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
