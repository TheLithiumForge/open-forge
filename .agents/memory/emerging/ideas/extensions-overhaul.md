---
open-forge:
  description: Explore post-initial Extension distribution, compatibility, migration, multi-root ownership, dependency expressiveness, and catalogue governance
  tags: [Memory, Idea, Contextual, Candidate, Extension, Architecture, Product, Distribution]
---

# Extensions Evolution

## Current Boundaries

The shipped [Extensions MVP Architecture](../../crystallized/documents/extensions/architecture.md)
owns current runtime behavior. Deleted CLI v2 proposed an initial next boundary
for:

- A strict inspectable package and manifest shape.
- Embedded Open Forge and explicit external filesystem catalogues.
- Invocation-local review of third-party source.
- Exact-id acyclic dependencies.
- One committed workspace lifecycle record.
- Whole-file ownership, collision, reconciliation, and removal.
- Git-first recovery, Gitless backups, formatting, route rebuilding, and
  explicit preservation decisions.

Those choices are raw historical input, not accepted direction. Inspect them in
the [CLI-v2 archive](../../archived/cli-v2/_cli-v2.md) when the new CLI reaches
Extension lifecycle design.

## Remaining Opportunities

### Remote Distribution And Provenance

- Should Open Forge ever fetch Extensions from URLs, registries, or another
  remote catalogue?
- Which integrity, authorship, provenance, reproducibility, and review evidence
  must precede installation?
- How can discovery remain cheap without turning remote metadata into runtime
  agent authority or silently trusting later bytes?

### Compatibility And Migration

- Which version or compatibility statement would provide a real guarantee
  across an Extension, Framework payload, CLI, and agent runtime?
- How should deprecation, replacement, migrations, and required user decisions
  remain inspectable and reversible?
- Which state actually needs migration before adding a migration language?

### Dependency Expressiveness

- Do repeated needs justify optional dependencies, conflicts, compatible
  alternatives, capabilities, or external satisfaction?
- How can any richer relation remain visible without recreating package-manager
  complexity or weakening exact ownership?

### Multi-root And Multi-manager Ownership

- How should lifecycle authority work across nested repositories, submodules,
  package-local `.agents` trees, or several managers?
- Can one path remain safely managed when scopes or repositories disagree about
  its owner and recovery boundary?
- Which manager identity and lock placement remain portable without leaking
  machine-local source locations?

### Catalogue Governance

- Which admission, review, stability, support, and deprecation criteria justify
  first-party or marketplace inclusion?
- What evidence proves that a package materially improves outcomes rather than
  merely adding content?

## Constraints

Any future extension must preserve these boundaries:

- Extensions remain optional, explicitly selected installation units.
- Installed human-readable files retain complete runtime meaning.
- Package metadata and lifecycle evidence do not become agent authority.
- Effects remain previewable, reviewable, contained, and recoverable.
- Local content, deliberate removals, shared ownership, route reachability, and
  user-controlled Git history remain protected.
- Remote distribution is absent by default until its trust model is accepted.

## Evidence Before Promotion

1. Implement and dogfood the accepted initial replacement lifecycle first.
2. Identify a recurring limitation that cannot be solved by an explicit local
   or embedded catalogue.
3. Define the smallest additional identity, trust, compatibility, or ownership
   contract needed for that limitation.
4. Prove it with representative package, collision, update, removal, recovery,
   and adversarial source scenarios.
5. Promote only the independently useful concept supported by that evidence.
