---
open-forge:
  description: Revisit focused CLI presentation snapshots after LithSnap has a Native-AOT-safe explicit baseline contract
  tags: [Memory, Idea, Contextual, Candidate, CLI, Testing, Snapshot, NativeAOT, LithSnap]
---

# LithSnap-Backed CLI Presentation Snapshots

## Opportunity

Open Forge has several presentation suites where one reviewed semantic projection
could eventually communicate the intended artifact more clearly than many
disconnected string assertions. LithSnap is the preferred candidate because its
small API and source ownership make a Native-AOT-first design practical, but it is
not ready for this repository yet.

Do not add a second snapshot dependency or an Open Forge-specific update harness
in the meantime. Continue writing focused assertions at the cheapest evidence
tier, with direct positive and negative checks for every relevant behavior.

## LithSnap Readiness Gate

Revisit adoption only after LithSnap proves all of the following:

- its core comparison and text snapshot path is trim- and Native-AOT-safe;
- structured snapshots require caller-supplied source-generated JSON metadata or
  another explicit reflection-free serializer;
- a missing baseline fails in ordinary verification mode instead of silently
  creating a passing expectation;
- creation and updates are explicit, targeted, serialized, and reported;
- CI and ordinary test execution remain read-only;
- snapshot identity does not depend on runtime stack inspection, assembly
  scanning, dynamic code, current culture, machine paths, or unstable ordering;
- the package can run under the repository's xUnit v3 Native AOT test boundary.

The detailed LithSnap implementation analysis is owned by that repository's
private idea record. This idea records only Open Forge's possible adoption
boundary.

## Candidate Migrations

The adoption review should start with these concrete candidates rather than a
suite-wide conversion:

1. `ContextPresentationTests`: one representative compact human projection, one
   expanded projection, and one JSON projection formed directly from typed test
   data. Keep schema order, status, bounded diagnostics, required inclusions, and
   safety omissions as direct assertions.
2. `FindHumanRenderingRedTests` and `FindJsonRenderingRedTests`: a deliberately
   small set of representative complete, attention, incomplete, and failed
   presentation artifacts. Keep the scenario matrix and each matching,
   projection, finding, escaping, omission, and next-action invariant direct.
3. `RouteInspectPresentationTests` and `RouteInspectJsonPresentationTests`: one
   complete representative profile in each public format. Keep condition,
   observation, measurement, provenance, ordering, and failure-branch assertions
   direct at the typed boundaries that own them.
4. `EmbeddedExtensionCatalogueIntegrationTests`: optionally snapshot a stable
   semantic projection of authored package identities, versions, dependency
   edges, payload paths, and hashes. Never replace direct authored-to-embedded
   byte equality, inventory-hash verification, archive regeneration, or
   dependency-closure evidence with a snapshot.

These are candidates, not accepted migrations. Adopt only the projections that
become materially easier to review as complete artifacts after a small spike.

## Evidence Boundary

A snapshot may describe a stable, deterministic output projection. It must not
become the only proof of:

- filesystem containment, symlink handling, case behavior, or cleanup ownership;
- no-write guarantees, exact mutations, recovery, locking, or interruption;
- semantic status, exit code, output stream, required next action, or diagnostic
  bounds;
- parser rejection, malformed input, public grammar, or independent compatibility
  bytes;
- inventory completeness, hashes, embedded byte equality, archive determinism,
  or other relational integrity claims.

Prefer a normalized semantic projection over a raw object graph. Exclude
temporary roots, absolute machine paths, timestamps, unordered collections, and
incidental implementation members. One expectation must have one obvious owner.

## Promotion Signals

Promote this idea only when LithSnap satisfies the readiness gate and a focused
spike demonstrates clearer review, local failures, explicit update ergonomics,
warning-free Native AOT publication, and no loss of direct safety evidence.
Reject an individual migration when the current assertions already express the
contract more clearly than a golden artifact.

## Related Sources

- [Test Evidence Integrity](../../../directives/open-forge/testing/evidence-integrity.md)
- [Evidence Tiers](../../../patterns/testing/evidence-tiers.md)
- [Audit CLI Constants And Test Architecture](../../working/cli-development/tasks/test-architecture-and-constants.md)
