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
  `464a4a6b6ef6447209edffbf53df7348c70691ed`. Task 14 Extension Install is
  complete at `20807781`, tree `4592a139`. Root Update is Complete at phase
  5/5, milestone 8/8. Its accepted squash integration is
  `c6eec9d26ad6b798d418d260027241795fb4aefc`, exact tree
  `f31cacb0c54bda8ecf8a91d3516c6ffa48f48753`, from `develop` parent
  `d0d475f3b8106dfa7c8552cabab4c197bad53a71`, tree
  `c3d621989a4b6698d5a9a8de1505da16a750dbcb`. The post-integration Release
  build and public Update `3/3` passed. Task 6's completion grace is consumed.
  Task 17 “Extension Update” is complete and dequeued after its completion grace
  was consumed. Task 18 “Extension Remove” is active at phase 2/5, milestone
  1/8 with Gray callable/public-shape review active from its accepted clean
  activation base; its Preflight capsule is frozen without a manifest or
  lifecycle schema change. Tasks 19 and 20 follow, then queued last-stage Tasks
  23 “Workspace Libraries” and 24 “Extensions Evolution” in that order.
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
Root Update is Complete at phase 5/5, milestone 8/8 after the
accepted and integrated Route Mutation M2 lane. Its accepted immutable lineage is
activation `0bc82357a4fc7bd54ecbca1d58501d721c04baa6` → Preflight
`48ac549c241e769246cfecb773124ccbc5076dfa` → Gray
`ef584350a48d08b2d6307eea70f87f2f3c46283a` → bounded recovery-deletion addendum
`7a9ded305e9ee185c02aaf636b4ff78f301b1fb1` → Red
`13fe18a9d1cc9f829cc0cf778c44c96f97abddcb` → coherent Green
`a59d4df80a1f5d23cd7848140d7462495ca0a77b`, tree
`d4a530e785dadd5e899fc73a410aec228b536201` → logical `T6-C1` physical
follow-ups `c03c057cbc5fe204ce115ef4c4001968b27df04c`, tree
`01769edbff77400bb54507a1d694d68015c29210`, `a252890756d166169c3d5b9bbd8a7adaa62cb896`,
tree `b55f15814e5f5153f0cbc37ce812b7bd56fa8a68`, and
`2e6b669d9ec2f8ce6fda7e176c1ba13d913cd9ba`, tree
`893160afdc52ac5d7fac966cef1e1f308da4817b` → final candidate
`ac96f4cc57550a83ef8651b40869ae4ff35da34e`, tree
`195f15388d6251f69244946183209d3dfe86b24a`. The final correction changes
Install help evidence only and adds no runtime behavior. No immutable commit was
amended. `T6-R1` is consumed with final focused rechecks PASS, and one logical
`T6-C1` correction is consumed while its three physical follow-ups remain
retained.

Focused evidence contains exactly 35 Update Unit cases, 22 Update Integration
cases, and three Update EndToEnd cases, all passing. The exactly three Doctor
EndToEnd cases remain unchanged and pass. The contributor catalogue remains six
members with the existing Framework and recovery attribution; no Doctor
vocabulary was added. The full managed suite passes Unit `1832/1832`,
Integration `988/988`, and EndToEnd `181/181`. Managed-on-native EndToEnd passes
`181/181`; Native Integration passes `988/988`; Native EndToEnd passes
`181/181`; all failures and skips are zero. Native root, Integration, and
EndToEnd publishes each have literal exit `0` with no warning or error lines.
All evidence uses only Task-built artifacts, and the global PATH CLI was not
invoked.

Task 6 is squash-integrated at `c6eec9d26ad6b798d418d260027241795fb4aefc`, exact
tree `f31cacb0c54bda8ecf8a91d3516c6ffa48f48753`, from `develop` parent
`d0d475f3b8106dfa7c8552cabab4c197bad53a71`, tree
`c3d621989a4b6698d5a9a8de1505da16a750dbcb`. Its post-integration Release build
and public Update `3/3` passed. The former Task 6 stop boundary was later
superseded by explicit Task 17 activation. Task 17 is complete and dequeued;
Task 18 is active at the phase and milestone recorded above. Task 19 “Repair”
and Task 20 “Cleanup” follow Task 18.
Extension Install was independent of Route Remove and root Update, so it formed
the first adoption slice after Task 12. Later lifecycle producers
must extend the accepted explicit Status/Doctor contributor inventory and its
affected evidence before their own acceptance.

Every other lifecycle mutation that replaces or deletes an existing ordinary
target prepares one immutable, strictly verified external recovery bundle for
the complete operation before its first target effect. Creates and no-ops create
none; failures retain the bundle and report target state without restoration,
rollback, or compensation. The persistent external workspace lock remains zero
bytes and is owned only through a read/write `FileShare.None` handle.

## Child Tasks

- [x] [Implement creation of one reviewable Extension package outside a workspace](extension-create.md) — Complete; command-local squash `3ef81227ba50fba869f0129b958eabc6d0c29fbc`, protected public integration `4c85d1d62004e8bdc885c51873ff6d9cdb6e6db5`
- [x] [Implement root Framework installation into a selected workspace](install.md) — Complete; final candidate `11994e4d21ddc807b7480afc39ae3612e5a69a56` is squash-integrated at `c60fcb98a57e9ec80769b9cb1d399ce13a227863`, exact tree `464a4a6b6ef6447209edffbf53df7348c70691ed`
- [x] [Task 14: implement Extension installation from exact reviewed package identity](extension-install.md) — Complete at phase 5/5, milestone 8/8; accepted lane `a6b44f07`, tree `cd4c074d`, squash-integrated at `20807781`, tree `4592a139`
- [x] [Task 6: implement root Framework update from accepted lifecycle identity](update.md) — Complete at phase 5/5, milestone 8/8; accepted candidate `ac96f4cc57550a83ef8651b40869ae4ff35da34e`, tree `195f15388d6251f69244946183209d3dfe86b24a`, is squash-integrated at `c6eec9d26ad6b798d418d260027241795fb4aefc`, exact tree `f31cacb0c54bda8ecf8a91d3516c6ffa48f48753`; post-integration Release build and public Update `3/3` passed
- [x] [Task 17: implement Extension update with source review, ownership, and recovery integrity](extension-update.md) — Complete; completion grace consumed and dequeued
- [ ] [Task 18: implement Extension removal with preserved user content and recovery integrity](extension-remove.md) — Active at phase 2/5, milestone 1/8; Gray callable/public-shape review active; Preflight capsule frozen — Task Mastermind: Sagan VI

## Entries

<!-- open-forge:generated-index:start -->

- [Implement creation of one reviewable Extension package outside a workspace](extension-create.md) - #Memory #Working #CLI #Task #Extension #Create #Lifecycle #Contextual
- [Implement Extension installation from exact reviewed package identity](extension-install.md) - #Memory #Working #CLI #Task #Extension #Install #Lifecycle #Contextual
- [Implement Extension removal with preserved user content and recovery integrity](extension-remove.md) - #Memory #Working #CLI #Task #Extension #Remove #Lifecycle #Contextual
- [Implement Extension update with source review, ownership, and recovery integrity](extension-update.md) - #Memory #Working #CLI #Task #Extension #Update #Lifecycle #Contextual
- [Implement root Framework installation into a selected workspace](install.md) - #Memory #Working #CLI #Task #Install #Framework #Lifecycle #Contextual
- [Implement root Framework update from accepted lifecycle identity](update.md) - #Memory #Working #CLI #Task #Update #Framework #Lifecycle #Contextual

<!-- open-forge:generated-index:end -->
