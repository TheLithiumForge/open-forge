---
open-forge:
  description: Explore post-initial Extension distribution, compatibility, migration, multi-root ownership, dependency expressiveness, and catalogue governance
  tags: [Memory, Idea, Contextual, Candidate, Extension, Architecture, Product, Distribution]
---

# Extensions Evolution

## Current Boundaries

The [Extensions Architecture](../../crystallized/documents/extensions/architecture.md) defines current package meaning, composition, and runtime boundaries. The [Extension command contracts](../../crystallized/documents/cli/contracts/extension/_extension.md) define accepted implementation mechanics. Deleted CLI-v2 proposals remain historical input for these topics:

- A strict inspectable package and manifest shape.
- Embedded Open Forge and explicit external filesystem catalogues.
- Invocation-local review of third-party source.
- Exact-id acyclic dependencies.
- One committed workspace lifecycle record.
- Whole-file ownership, collision, reconciliation, and removal.
- Git-first recovery, Gitless backups, formatting, route rebuilding, and
  explicit preservation decisions.

Those proposals do not establish accepted direction. Consult the [CLI-v2 archive](../../archived/cli-v2/_cli-v2.md) for relevant historical reasoning. Use the current contracts for accepted behavior.

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

### Current Destinations And Package Layout

The [Extension contracts](../../crystallized/documents/cli/contracts/extension/_extension.md) use `content/` only. [Task 24](../../archived/cli-development/tasks/extensions-evolution.md) settled the earlier package-directory alternatives and implemented consumer-owned destination permissions. Source or package metadata cannot grant itself permission. The [Workspace Permissions contracts](../../crystallized/documents/cli/contracts/shared/workspace-permissions/_workspace-permissions.md) define that boundary.

The [Library contracts](../../crystallized/documents/cli/contracts/library/_library.md) project eligible files from a contained source root through relative file links. `--to` selects the destination root and defaults to the workspace root. They do not require a `content/` or `.agents/` source child. Extension copies and Library links retain separate ownership and lifecycle meaning.

The remaining manager questions concern additional semantics beyond those accepted destination mechanics:

- Should a package request typed content for another manager without claiming that manager's semantics?
- Does another manager treat an installed file as instructions, configuration, or executable behavior that needs a stronger review boundary?
- How should several managers coordinate meaning and ownership when their scopes overlap?

### Multi-root And Multi-manager Ownership

- How should lifecycle authority work across nested repositories, submodules,
  package-local `.agents` trees, or several managers?
- Can one path remain safely managed when scopes or repositories disagree about
  its owner and recovery boundary?
- Which manager identity and lock placement remain portable without leaking
  machine-local source locations?

### Centralized Framework Content And Loader Bridges

[Workspace Libraries](../../crystallized/documents/cli/contracts/library/_library.md) now provide live projection from one contained source root to a selected destination root. Eligible `.agents` files retain ordinary Framework routing. Library projection remains distinct from Extension installation and does not create another Framework root or authorize packaging `local/extensions`. The [earlier Idea](../../archived/ideas/workspace-libraries.md) remains historical reasoning.

## Completed Initial Evolution

Tasks [24](../../archived/cli-development/tasks/extensions-evolution.md), [25](../../archived/cli-development/tasks/workspace-library-destination-projections.md), and [26](../../archived/cli-development/tasks/extension-internal-consolidation.md) are complete and locally integrated. They preserve the package-layout and permission decision, Library destination projection, and later Extension internal consolidation. Their former queue and preparation sequence are historical. This Idea retains opportunities beyond that implemented baseline.

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

1. Use the accepted initial replacement lifecycle as the baseline.
2. Identify a recurring limitation that cannot be solved by an explicit local
   or embedded catalogue.
3. Define the smallest additional identity, trust, compatibility, or ownership
   contract needed for that limitation.
4. Prove it with representative package, collision, update, removal, recovery,
   and adversarial source scenarios.
5. Promote only the independently useful concept supported by that evidence.
