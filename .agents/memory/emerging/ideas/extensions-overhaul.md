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

### Destinations And Package Directory Vocabulary

The current replacement CLI deliberately restricts Extension installation to
strict descendants of `.agents/`; `.apm/` and every other manager root are
rejected. [Workspace Libraries](workspace-libraries.md) is the separate accepted
first-release design for live projection into ordinary `.agents/**` paths. It
does not add non-`.agents` destinations and does not change the Extension
boundary. Permanent Task 25 preserves a later Library-specific destination
projection decision. A neutral destination-admission capability may be shared
only after Extension copies and Library links prove identical path-safety
meaning; their permission and ownership lifecycles remain separate.

Task 24 must compare `payload/`, the original singular `content/` candidate,
and the user's `contents/` suggestion as one atomic package-layout decision.
This remains narrower than a general distribution redesign. No spelling is
accepted yet. Consumer-owned exact destination allowlists must
remain separate from package and source metadata: neither can authorize its
own destinations. Permission may admit a path but does not transfer another
manager's meaning or ownership.

- Should a package be able to request typed content for another manager without
  claiming that manager's semantics?
- Which exact destination roots may the consumer approve, and where does that
  durable approval live?
- How do interactive approval and explicit non-interactive configuration avoid
  letting source-controlled metadata grant itself new destinations?
- Does another manager treat an installed file as instructions, configuration,
  or executable behavior that needs a stronger review boundary?
- Would `content/` or `contents/` eventually describe multi-manager package
  files more clearly than `payload/`, or should `payload/` remain the transport
  directory? The choice must be reviewed atomically; recording it does not
  authorize compatibility machinery.

### Multi-root And Multi-manager Ownership

- How should lifecycle authority work across nested repositories, submodules,
  package-local `.agents` trees, or several managers?
- Can one path remain safely managed when scopes or repositories disagree about
  its owner and recovery boundary?
- Which manager identity and lock placement remain portable without leaking
  machine-local source locations?

### Centralized Framework Content And Loader Bridges

[Workspace Libraries](workspace-libraries.md) is the contextual candidate for
projecting live shared files from one contained source root into ordinary
`.agents/**` paths. It keeps one Framework Loader and treats projection as a
lifecycle distinct from Extension installation. It is accepted contextual input
for queued Task 23, does not create another Framework root, and does not
authorize packaging or externalizing `local/extensions`.

## Post-command Task Split

This idea remains the source for queued permanent Task 24 “Extensions
Evolution”. Task 24 owns the package-directory decision and consumer-owned
Extension destination permissions. [Task
25](../../working/cli-development/tasks/workspace-library-destination-projections.md)
separately owns any later Library relative-link destinations beyond `.agents/`.
[Task 26](../../working/cli-development/tasks/extension-internal-consolidation.md)
owns only the differential-locked six-command internal refactor and public-test
streamlining after both behavior tasks are settled.

Task 19 “Repair”, Task 20 “Cleanup”, and Task 23 “Workspace Libraries” finish
the remaining command sequence first. Tasks 24, 25, and 26 are queued with no phase or
milestone horizon. Read-only functional preparation may overlap command work. Shared and
public
implementation remains serialized. The final drafted contracts for
Tasks 24 and 25 require user review before implementation. The user has
authorized Task 26; the Overseer freezes its exact differential oracles and
execution plan before activation. No release plan follows from this queue. The earlier preparation tip
`8a153f23` remains non-authoritative internal-refactor input for Task 26.

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
