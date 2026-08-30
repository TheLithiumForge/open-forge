---
open-forge:
  description: Implement root and Extension creation, installation, update, and removal lifecycle commands
  tags: [Memory, Working, CLI, Task, Lifecycle, Extension, Install, Update, Contextual]
---

# Lifecycle Commands

## Task State

- State: Active. The next-wave shared foundations, Route Inspect correction,
  Extension Create, and root Install are integrated. Root Install is complete at
  `c60fcb98a57e9ec80769b9cb1d399ce13a227863`, exact tree
  `464a4a6b6ef6447209edffbf53df7348c70691ed`. Generic and Framework-aware Route
  Init is the next lifecycle boundary in the accepted Route Mutation sequence.
- Parent: [Complete The Replacement CLI](../00-cli-development.md).
- Common prerequisites: Extension discovery and `index`; workspace mutations also
  consume Mutation Foundation.
- Intended-membership formation prerequisite: accepted feature
  `f82c2b168657baf2fac50c76e2ff2cc0ed3776d7`, squash-integrated at
  `cc35c853fd7b55d31e3fb2c9a454d9ab61c1884e`, with closeout integrated at
  `18f2acff31cfd6600a16430ac5d689d05482e297`, exact tree
  `39f0a8c4e6d3695cfbe7407dfd6043dc5ec9680a`. It applies only to root Install
  and root Update in this group. Extension Create is independent of it.

## Shared Boundary

Lifecycle commands consume accepted package identities, catalogue facts,
workspace state, lifecycle schema, locks, external recovery-bundle support,
source review, and generated-navigation primitives. Each command retains its
selection, plan, effects, findings, and result.

No command recognizes legacy lifecycle files. Package wrappers do not implement
lifecycle behavior. `extension create` has no workspace subject and uses a
separate exact-destination, collision, and revalidation path with no workspace
lease, no Replace/Delete, and no recovery bundle.

Root Install and root Update later consume
`Build(SourceCatalogue observedCatalogue, IReadOnlyList<SourceLogicalSource> intendedSources)`
to project their complete post-operation Generated Navigation graph from real
observed evidence. This shared formation adds no prospective catalogue/source
framework, virtual filesystem, temporary checkout, or hidden Index. Extension
Create does not consume it. The maintainer accepted explicit `--name`,
`--description`, `--package-version`, repeatable `--dependency` overrides, and a
command-local wizard that asks only for missing stable ID and catalogue facts.
Exact name/description defaults, catalogue-parent and sibling eligibility, local
invalid-input correction, EOF/cancellation semantics, and the command-local JSON
result are accepted and frozen in its contracts. Root Install's exact fully
present ordered public JSON result is also accepted and frozen, including typed
residual values `none`, `retained`, and `unknown`; command-local result and
presentation work may proceed against it.
Extension Create depends only on the native interaction foundation. Root Install
depends on interaction, embedded Framework distribution, lifecycle
`sourceAssetPath` provenance, the shared directory-create foundation, and
intended-membership formation. Its exact command-local interaction is accepted.
The Extension Create and root Install command lanes executed in parallel after
their prerequisites. Both commands' protected root composition and executable
acceptance seams are complete.
Root Update remains sequenced after the complete Route Mutation M2 lane. Only
independent preparation such as scope discovery, contract and ownership audits,
callable-surface analysis, Gray/Red readiness, and worktree setup may proceed
earlier; dependent Update behavior does not overlap unfinished M2 behavior.

Every other lifecycle mutation that replaces or deletes an existing ordinary
target prepares one immutable, strictly verified external recovery bundle for
the complete operation before its first target effect. Creates and no-ops create
none; failures retain the bundle and report target state without restoration,
rollback, or compensation. The persistent external workspace lock remains zero
bytes and is owned only through a read/write `FileShare.None` handle.

## Child Tasks

- [x] [Implement creation of one reviewable Extension package outside a workspace](extension-create.md) — Complete; command-local squash `3ef81227ba50fba869f0129b958eabc6d0c29fbc`, protected public integration `4c85d1d62004e8bdc885c51873ff6d9cdb6e6db5`
- [x] [Implement root Framework installation into a selected workspace](install.md) — Complete; final candidate `11994e4d21ddc807b7480afc39ae3612e5a69a56` is squash-integrated at `c60fcb98a57e9ec80769b9cb1d399ce13a227863`, exact tree `464a4a6b6ef6447209edffbf53df7348c70691ed`
- [ ] [Implement root Framework update from accepted lifecycle identity](update.md) — Planned after full Route Mutation M2 — Implementer: Not assigned
- [ ] [Implement Extension installation from exact reviewed package identity](extension-install.md) — Planned — Implementer: Not assigned
- [ ] [Implement Extension update with source review, ownership, and recovery integrity](extension-update.md) — Planned — Implementer: Not assigned
- [ ] [Implement Extension removal with preserved user content and recovery integrity](extension-remove.md) — Planned — Implementer: Not assigned

## Entries

<!-- open-forge:generated-index:start -->
- [Implement creation of one reviewable Extension package outside a workspace](extension-create.md) - #Memory #Working #CLI #Task #Extension #Create #Lifecycle #Contextual
- [Implement Extension installation from exact reviewed package identity](extension-install.md) - #Memory #Working #CLI #Task #Extension #Install #Lifecycle #Contextual
- [Implement Extension removal with preserved user content and recovery integrity](extension-remove.md) - #Memory #Working #CLI #Task #Extension #Remove #Lifecycle #Contextual
- [Implement Extension update with source review, ownership, and recovery integrity](extension-update.md) - #Memory #Working #CLI #Task #Extension #Update #Lifecycle #Contextual
- [Implement root Framework installation into a selected workspace](install.md) - #Memory #Working #CLI #Task #Install #Framework #Lifecycle #Contextual
- [Implement root Framework update from accepted lifecycle identity](update.md) - #Memory #Working #CLI #Task #Update #Framework #Lifecycle #Contextual
<!-- open-forge:generated-index:end -->
