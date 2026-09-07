---
open-forge:
  description: Current state and next action for the greenfield replacement CLI development program
  tags: [Memory, Working, Checkpoint, Active, KeepInMind, CLI, Architecture, Plan, Task, Contextual]
---

# CLI Development Checkpoint

## Current Accepted Checkpoint

Task 20 “Cleanup” (phase 5/5): milestone 8/8 is complete and root-accepted.
Candidate `4be87eaab21080f51eb7e2d753db96cc3ed7e2b2` is squash-integrated at
`148d378da376d196de183d9564261658164d9d23`, with exact candidate tree
`ab7e488192b435fdefa0b8d30bf1dc853a6b2327`. Root verified every one of 2,705
tracked file identities and all 59 delta paths by bytes and modes. Final focused
250 Unit, 117 Integration, and three public cases pass. Full managed
2021/1077/193, native 1077/193, and managed-on-native 193 pass with zero
failures, skips, or warnings. Both accepted R1 findings resolved in the single
C1; no additional review or full execution is required for exact integration
and prose closeout.

Task 23 “Workspace Libraries” (phase 2/5): milestone 2/8 is active under
Noether, Task Mastermind (`.apm/agents/task-mastermind.agent.md`), GPT-6
Astra/high. Merge the exact `148d378d` codebase, then complete consumer Gray
and final Red before Green. Prepared `1aa461dc` evidence and accepted contract
`c3f01acb`, including the future typed Library Repair exception, remain intact.
Task 19 “Repair” remains complete and dequeued with grace zero. The project
control ledger records consumed Task 20 completion grace.

This checkpoint supersedes earlier execution snapshots below; their commit
and evidence histories remain historical. The project control ledger and child
Task records define current status. Tasks 24–26, Task 10, conditional Task 21,
Task 13 (phase 1/3, milestone 2/6), and Task 22 retain their queue order.
Task 24/25 functional drafts await user comments; Task 26 is a pure refactor.
Publication is not authorized. Test-environment cleanup remains an idea.
Substantive work uses Astra/high, with high the maximum; routine execution
uses Luna/max.

## Latest Transfer Boundary

The user re-enabled every pending task on 2026-09-07. The sealed restart
[handoff](../handoffs/2026-09-07_cli-astra-restart.md) and
[inventory](../handoffs/2026-09-07_cli-astra-restart-inventory.md) preserve the transfer evidence;
current queue state is reconciled here and in the project control ledger.
Repair remains phase 4/5, milestone 3/8 with an unfinished Green draft.
Library remains phase 2/5, milestone 1/8 with Gray correction active and Red
read-only until immutable Gray acceptance. Cleanup retains accepted Red at
phase 4/5, milestone 3/8, with Green held for Repair integration and refreeze.

The transfer captured 28 dirty Repair paths, 102 dirty Library paths, and
eleven dirty queue paths with empty indexes. These are historical inventories,
not fresh correctness evidence or live counts. Main overlapping draft bytes
and the queue draft were preserved before reconciliation. Current owners
inspect their own live lanes and use only their own worktree artifacts. Every
C# author and reviewer personally reads and fingerprints the complete current
C# directives and applies the relevant Patterns throughout the work.

## Goal

Complete the replacement CLI from its accepted Architecture and command
contracts, with safe local development, reproducible evidence, and no partial
release.

- Last updated: 2026-09-07.

Task 23 “Workspace Libraries” is the remaining command task, active at phase
2/5, milestone 2/8 for the post-Cleanup consumer boundary and final Red before
Green. Tasks 19 and 20 are complete and integrated. Task 19 is dequeued; Task
20 completion visibility follows the project control ledger.

Tasks 24 “Extensions Evolution”, 25 “Workspace Library Destination
Projections”, and 26 “Extension Internal Consolidation” are queued post-command work
with no phase or milestone horizon. Task 10, conditional Task 21, Task 13, and Task 22
also remain queued.

## Current State

Task 12 “CLI Architecture Authority Remediation” is Complete and dequeued at
phase 5/5, milestone 6/6. Corrected source
`a0e6bc8dc25ae9395beae98289f594e1f9f56af2`, exact tree
`9c4a33b1c16617cf79beefd9f16a6e1d2d551382`, is squash-integrated at
`495a7ed6b55bca2a879ece83818f89e530c33af2`, the same exact tree, from local
`develop` parent `d3d2dc1362ec1ba03844927f44fefdffb2fd466d`, tree
`6959b51e148af44d59512d8bdd801350d88fc651`. Its two-update completion grace
is consumed.

Task 4 “Route Move” remains Complete and dequeued at phase 6/6, milestone
12/12. Accepted closeout `631983ea1ec7d7ad5f5fc3999f938ba5f445ed81`, tree
`d6f6fdf7d1caf62a9ac68582609c7282921f557c`, is squash-integrated at
`d3d2dc1362ec1ba03844927f44fefdffb2fd466d`, tree
`6959b51e148af44d59512d8bdd801350d88fc651`, from local `develop` parent
`83902c8849bc98e44812b33175c5122421171e8b`, tree
`4b8324b2e9e0cb4c876ef9a5db293968b37a9192`. Its two-update completion grace
is consumed. The [project control ledger](../cli-development/project-control.md)
defines permanent task identities and the current dynamic queue.

Task 14 “Extension Install” is Complete at phase 5/5, milestone 8/8. Accepted
lane `a6b44f07`, tree `cd4c074d`, is squash-integrated at `20807781`, tree
`4592a139`. Final warning-free managed evidence passes Unit `1711/1711`,
Integration `917/917`, and EndToEnd `169/169`; Native AOT passes Integration
`917/917` and EndToEnd `169/169`; managed EndToEnd against the fresh native
root passes `169/169`; and the final isolated offline package journey verifies
the launcher, Root Install's 44 effects, and Extension Install's 28 effects.
Its completion grace is consumed and it is dequeued.

Task 15 “Status” is Complete and dequeued at phase 5/5, milestone 8/8. Its
historical phase-3 lane records Gray `dac2aece`/`874bdcae`, bounded callable
correction `df46fb9c`/`eda74e79`, and accepted Red `f633fe1f`/`5dbfe9d7`. One
Sol/xhigh Brilliant Implementer was activated at `de27fcd6`/`ebdcb4a6`, then the early architecture
checkpoint found that the two lifecycle Status signatures cannot guarantee one
common lifecycle-document observation without prohibited mutable state.
Corrected raw-snapshot Gray `b859d0aa`/`8f417c58` now makes that stateless
invocation-local observation explicit and passes a warning-free Core Release
build. Corrected Red `d768ca52`/`7a9b785b` is accepted with 14 Unit, 10
Integration, and 3 unchanged EndToEnd tests. Resume checkpoint
`d120d49e`/`28f23430` reapplied the interrupted Implementer's 94-line partial
`StatusDefinitions` work losslessly; recovery stash `9b51b3f8` remains retained.
The same Sol/xhigh Brilliant Implementer then completed coherent production and
focused verification.
After `T15-S1`, the immutable pre-acceptance Status continuation remains
historical:
`cf00fb86`/`bf69e7c2` → `48309613`/`b8e0ad0f` → `e79a9767`/`17068f0b` →
`2521574d`/`11eb89ee` → `b8ac2dd7`/`453eb1b8`, covering rendering and
compatibility Green, direct JSON context, plural-source Gray, direct producer
construction authority, and focused plural-source Red. Red selected, discovered,
and executed `1/1/1`; only the expected three-observation versus one-scaffold
oracle failed, while lifecycle completeness/trust and five package/source facts
passed. Independent Route selected-view work was also present. The accepted
Status lane, correction, and Green identities are recorded below. Its integration
parent is `e90b22f9a6d4fdd2043516e718fc782490396cb2`, tree
`74713c32473603e7f9100378fafd314c50f813e3`; the integration identity is the
commit containing this record. Final gates had Release `0` warnings and `0`
errors; managed Unit `1739/1739`, Integration `933/933`, EndToEnd `172/172`;
managed EndToEnd against the native root `172/172`; native Integration
`933/933`; native EndToEnd `172/172`; all with `0` failures, `0` skips, and
`0` warnings. Every later producer extends the explicit contributor inventory
and affected Status evidence before acceptance. Task 16 “Doctor” is complete at
phase 5/5, milestone 8/8. Accepted lane `a48a16cd`, exact tree `90e66b05`, is
squash-integrated at `59276c3b` with the same tree. Full managed, public,
supported `linux-x64` Native AOT, managed-on-native, focused, structural, and
post-integration gates passed. The unreleased schema-v1 Doctor catalogue
contains 108 kinds, including 19 Extension kinds; Task 17 closed the
bridge-registration observation and no Extension observation horizon remains.
Task 5 “Route Remove” is complete at phase
5/5, milestone 8/8. Accepted lane `ab8da620`, tree `bb41e1c9`, is
squash-integrated at `5a2e650a` with exact tree equality. Final managed
`1797/966/178`, managed-on-native `178`, native `966/178`, focused, structural,
and post-integration Route Remove `3/3` gates passed. Route Inspect's synthetic
overwrite-ambiguity conflict remains one bounded Task 10 audit input, not a
Doctor finding or a new task. Recovery attribution remains unreleased schema
v1, and old unattributed or malformed final bundles fail closed. Its completion
grace is consumed and it is dequeued. Task 6 “Root
Update” is Complete at phase 5/5, milestone 8/8. Its
accepted immutable lineage is activation `0bc82357a4fc7bd54ecbca1d58501d721c04baa6`
→ Preflight `48ac549c241e769246cfecb773124ccbc5076dfa` → Gray
`ef584350a48d08b2d6307eea70f87f2f3c46283a` → bounded recovery-deletion
addendum `7a9ded305e9ee185c02aaf636b4ff78f301b1fb1` → Red
`13fe18a9d1cc9f829cc0cf778c44c96f97abddcb` → Green
`a59d4df80a1f5d23cd7848140d7462495ca0a77b`, tree
`d4a530e785dadd5e899fc73a410aec228b536201` → logical `T6-C1` physical
follow-ups `c03c057cbc5fe204ce115ef4c4001968b27df04c`, tree
`01769edbff77400bb54507a1d694d68015c29210`, `a252890756d166169c3d5b9bbd8a7adaa62cb896`,
tree `b55f15814e5f5153f0cbc37ce812b7bd56fa8a68`, and
`2e6b669d9ec2f8ce6fda7e176c1ba13d913cd9ba`, tree
`893160afdc52ac5d7fac966cef1e1f308da4817b` → separately exposed full-gate
direct-consumer evidence correction and final candidate
`ac96f4cc57550a83ef8651b40869ae4ff35da34e`, tree
`195f15388d6251f69244946183209d3dfe86b24a`. No immutable commit was amended;
the final correction changes Install help evidence only and adds no runtime
behavior. T6-R1 is consumed with final focused rechecks PASS, and one logical
T6-C1 is consumed with the immutable physical follow-ups retained.

Final pre-integration acceptance was a warning-free local-only locked restore
for all six projects,
Release solution build with zero warnings and errors, managed Unit `1832/1832`,
Integration `988/988`, EndToEnd `181/181`, managed-on-native EndToEnd `181/181`,
Native Integration `988/988`, and Native EndToEnd `181/181`. Native root,
Integration, and EndToEnd publishes each have literal exit `0` and no
warning/error lines; all failures and skips are zero. Focused Update evidence is
exactly 35 Unit, 22 Integration, and three EndToEnd cases; the exactly three
Doctor EndToEnd cases remain unchanged. The worktree-built public executable is
`0.0.0-dev`. The global PATH CLI was not invoked. The accepted candidate was
squash-integrated as `c6eec9d26ad6b798d418d260027241795fb4aefc`, exact tree
`f31cacb0c54bda8ecf8a91d3516c6ffa48f48753`, from `develop` parent
`d0d475f3b8106dfa7c8552cabab4c197bad53a71`, tree
`c3d621989a4b6698d5a9a8de1505da16a750dbcb`. Integration adoption covered 65
paths with sorted-path SHA-256
`88cbcb641e9a164fa61de9e357a26cf7f007a265b2b7686460d3e8f324ec5dcd`; the
expected and actual pre-commit tree was
`f31cacb0c54bda8ecf8a91d3516c6ffa48f48753`; and the develop-relative binary
SHA-256 was `b6975342bb29cb99fec7eef30683b4438ba537083eb7c377205581c7d433bc18`.
Candidate mismatches and eight develop-only path mismatches were zero.

The fresh post-integration main Release build passed with zero warnings and
errors. Its freshly built managed EndToEnd app selected the sole
`PublishedUpdateProcessTests` class containing exactly three public facts;
total, discovered, executed, and passed were `3`, with failed and skipped both
`0`. The same-workspace development executable is
`artifacts/publish/open-forge-dev/Release/open-forge-dev`, SHA-256
`4450c4552ac44a4e463db6c9adad89a39d803da47801801019ee08c02045bd61`; its
`artifacts/publish/open-forge-dev/Release/open-forge-dev.version` marker has
SHA-256 `fe4d33c8c2c76a67725ea7d54dafb79c485bd2819a317d7235a6910157ef76f4` and
reports `0.0.0-dev`. The global PATH CLI was not used as acceptance evidence.
The earlier prepared alignment and its Status `2521574d` snapshot remain
historical context.

Task 7 “npm Package Manager Release and Local Linking” retains its immutable
complete historical horizons and completed its disjoint provisional local-use
horizon at phase 3/3, milestone 5/5. Candidate `092ead98`, tree `1c4ab3ae`, is
integrated into current `develop`. Locked restore and the exact Linux x64 local
link succeeded; one user-local PATH shim was required. Version and root help
matched the candidate, the installed Loader contained the replacement commands,
and the isolated fresh-workspace JSON smoke returned the accepted
`0`/`2`/`0`/`0`/`3` exits with empty stderr for Install, Context, Route List,
Status, and Doctor. Ordinary projects use machine-global `open-forge`, while
development, review, and acceptance worktrees use only same-worktree artifacts
as evidence. The integrated worktree remained clean. No package publication,
registry or remote mutation, or user-project smoke occurred. ARM remains
outside scope. Its completion grace is consumed and it is dequeued.

Task 3 “Route Update” remains Complete from accepted closeout `27df8325`, tree
`b68d4349`, with final executable behavior `8a398f2e`, tree `3bb4a224`.

The preceding Route Create integration command is
`19412d2a562ae66d1b4642256d854438df75366f`.
Route Init
is Complete and squash-integrated at `cc5085ce` from reviewed closeout
`c5801494ac6426add2c64e32cafbba6f0162561a`, with exact tree equality. Its
executable evidence candidate remains `cb62b19f73afcace163371af9093d877821fa800`,
tree `be93900d0dc102fcf2d5a351651c0b0134de39a0`. The develop baseline includes the
integrated D0 contract freeze, SF1-SF4 shared foundations, M2 preparation
record, Route Inspect interaction correction, and protected Extension Create and
root Install public integrations described below. It also includes canonical
lifecycle creation at `1d404c5` and the neutral Markdown link-label projection at
`89a35a7`. The preceding C2 baseline was
`c60fcb98a57e9ec80769b9cb1d399ce13a227863`, exact tree
`464a4a6b6ef6447209edffbf53df7348c70691ed`. The preceding C1 baseline was
`4c85d1d62004e8bdc885c51873ff6d9cdb6e6db5`, exact tree
`fa29bd9572df39b2d5457c35bb8a0bd6ba5a9945`. The preceding C3 baseline was
`fa3db1ee1dbfb687715b5b90b35f6104cbc45c6c` with exact tree
`605620d990622e093b12e85e50c6e40083896a18`.

Proportional CLI Corrections are Complete and squash-integrated through
implementation `0d88606`, with clean local `develop` closeout `7abde56`.
Proportionate guidance is `5f9f59e`; Route List
bounded escaping and proved-dead support removal are `2cd525d`; the authoritative
Markdown generated-region and fingerprint state flow is `0d88606`. The integrated
Release build is warning-free; managed Unit `1053/1053`, Integration `411/411`,
and EndToEnd `120/120` pass. Portable `linux-x64` Native AOT root publication and
execution pass, with Native AOT Integration `411/411` and EndToEnd `120/120`.
Native dogfood makes no writes, Doctor reports zero errors and the known C#
`Axioms` warning, and final review is `ROBUST`. No push occurred.

Find, References, Context, Extension List, and Extension Inspect are Complete. Context is
squash-integrated into local `develop` at `ca097a2`, and Extension List is
squash-integrated at current clean baseline `db0d39a`. The final tree exactly
matches the rebased Extension List integration tip `5e6babf` and retains both
Context and Extension List composition and source-generated JSON registration.

The combined accepted baseline passes locked restore, warning-free Release
build, format verification, managed Unit `978/978`, Integration `354/354`, and
EndToEnd `111/111`, all with zero skips. The supported local `linux-x64` Native
AOT root publishes and executes both commands; Native AOT Integration is
`354/354` and EndToEnd is `111/111`, with zero skips. Final independent Context
and Extension List correctness reviews found no material issues. Extension
Inspect's exact public contract is accepted and squash-integrated at `92313a0`.
Its accepted rebased feature `2b1e63d` is squash-integrated at `73b01be`. The
combined baseline passes warning-free Release build, managed Unit `1054/1054`,
Integration `378/378`, EndToEnd `116/116`, and portable `linux-x64` Native AOT
Integration `378/378` and EndToEnd `116/116`, all with zero failures or skips.
The final correction recheck is `ACCEPTED`. No remote action or push
occurred.

Routed Authored Metadata is Complete and squash-integrated at `5924698`. Its
accepted neutral parser preserves `SourceDocumentForm` as the sole classifier
and passes full managed Unit `1012/1012`, Integration `354/354`, and local
`linux-x64` Native AOT Integration `354/354`, all with zero failures/skips.
Generated Navigation's sole project-change request is therefore closed.

Generated Navigation is Complete. Accepted feature `5a882e8` is
squash-integrated into local `develop` at `21e5200`. Final evidence passes focused
Unit `18/18`, focused real-filesystem Integration `3/3`, full managed Unit
`1030/1030`, Integration `357/357`, and republished local `linux-x64` Native AOT
Integration `357/357`, with zero failures or skips. The final recheck is
`ACCEPTED`; post-integration focused Unit `18/18` and Integration `3/3` pass.

The constants/test-architecture audit is Complete and squash-integrated into
local `develop` at `b6ce31f`. The integrated tree exactly matches its accepted
feature tree and retains final managed Unit `1024/1024`, Integration `409/409`,
EndToEnd `116/116`, and supported local `linux-x64` Native AOT Integration
`409/409` evidence. No push or remote action occurred.

Mutation Foundation is Complete at exact production candidate `e7d937f` under
current authority `01dd552`. It retains the historical contract, lock/lifecycle,
and atomic-application commits while correcting their superseded recovery and
lock decisions. The persistent reusable lock preserves existing bytes and is
owned only through a held `FileShare.None` handle. Existing-target application
requires one matching opaque final recovery preparation; Create requires none.
The external ordinary-BCL LocalApplicationData bundle store, neutral catalogue,
and lease-gated mechanical deletion guard are implemented without Git,
restoration, rollback, compensation, journal, or command policy.

Final acceptance passes focused M1 Unit `43/43` and Integration `47/47`; full
managed Unit `1096/1096`, Integration `458/458`, and EndToEnd `120/120`; focused
published `linux-x64` Recovery/source-generation Native AOT `10/10`; and the
already-published full Native AOT Integration `458/458`, all with zero failures
or skips. Locked restore, the warning-free Release build, whitespace-format and
diff checks, source-generation/reflection-disabled execution, the static zero
replacement-product Git audit, and final independent review (`ROBUST`, safe
to commit, confidence `0.98`) pass. The complete solution-format command exits
successfully with existing workspace reference-load warnings; the narrower
whitespace oracle is clean. No executable process-crash, permission-manipulation,
or fake OS-failure seam is claimed.

The later lifecycle correction is integrated at
`1d404c5cef3f5fd464ca771fc132a657f792f533`. New documents emit all five
ordered root keys. Framework-created documents use complete empty Extensions.
Existing missing, null, malformed, or incomplete Framework or Extensions state
remains untrusted and blocks planning without repair. Focused and full managed,
Native AOT dogfood, diff, and independent review gates pass.

Public Index is Complete in exact feature candidate
`4e89d945b38a2d1e24600dd22789b55e4395a534`. Operation orchestration is
committed at `125b6a2a`; public composition and presentation are committed at
`4e89d945`. The Release solution build is warning-free. Managed Unit
`1206/1206`, Integration `480/480`, and EndToEnd `125/125` pass; portable
`linux-x64` Native AOT Integration `480/480`, EndToEnd `125/125`, and root
publish/version smoke pass. Every stated test run has zero failures and zero
skips.

Controlled public scenarios prove safe dry run, JSON, application, and second-run
idempotence. Application changes one exact bounded generated interior, preserves
unrelated tracked content, and returns the tracked aggregate hash to its baseline
after the apply and idempotence sequence. Automatic whole-repository dogfood
blocks on 14 archived metadata sources while known handoff-routing incompleteness
remains visible; both are repository state rather than candidate failures. The
later accepted shared correction relocates the reusable zero-byte lock to the
external application-owned lock catalogue. Verification used the existing cached and offline prepared dependency
state. No fresh remote NuGet vulnerability audit, remote CI, push, deployment,
release, or publication is claimed. Final Index closeout tip
`2b353c48978ee88e53345be8037776181612222c` is locally squash-integrated into
`develop` at `09aa03eddb97831ff544afe1eac54ad9af501f5c`; both commits have exact
tree `2dcfca18020980a9cafbc429a72930af3368df5f`.

The existing Generated Navigation formation is now extended by accepted feature
`f82c2b168657baf2fac50c76e2ff2cc0ed3776d7`, squash-integrated into local
`develop` at `cc35c853fd7b55d31e3fb2c9a454d9ab61c1884e` and closed at the
pre-foundation commit `18f2acff31cfd6600a16430ac5d689d05482e297`, exact tree
`39f0a8c4e6d3695cfbe7407dfd6043dc5ec9680a`. The overload
`Build(SourceCatalogue observedCatalogue, IReadOnlyList<SourceLogicalSource> intendedSources)`
separates observed catalogue evidence from intended membership, lookup, topology,
Loader, roots, ambiguities, and target-collision facts. Existing
`Build(SourceCatalogue)` behavior is unchanged and delegates to that overload.
This is not a prospective catalogue/source framework and adds no virtual
filesystem, temporary checkout, or hidden Index.

Final formation evidence passes a Release build with 0 warnings and 0 errors;
managed Unit `1227/1227`, Integration `481/481`, and EndToEnd `125/125`; and
portable `linux-x64` Native AOT Integration `481/481` and EndToEnd `125/125`.
Real-workspace projection proves zero writes. Independent Sol/xhigh review is
`ROBUST` with 99% confidence. The first managed EndToEnd invocation preceded its
required published-root version marker and failed at harness discovery. After the
canonical root publish step satisfied that sequencing precondition, EndToEnd
passed `125/125`; no implementation correction was required.

The next-wave contract freeze D0 is integrated at `38e1498`. The reviewed shared
foundations are integrated at `e782090` (SF1 native interaction), `680915a`
(SF2 embedded Framework payload), `0989356` (SF3 lifecycle source-asset
provenance), and `33913df` (SF4 directory mutation). The combined reviewed
baseline has a Release build with `0` warnings and `0` errors; managed Unit
`1284/1284`, Integration `500/500`, and EndToEnd `125/125`; Native AOT
Integration `500/500` and EndToEnd `125/125`; and zero skips in every stated run.

The accepted shared correction removes lock bootstrap state. The persistent
zero-byte lock is external under `LocalApplicationData/OpenForge/locks/v1`, and
missing `.agents` is an ordinary lease-bound directory-create effect.

The earlier shared-foundation closeout checks were read-only and scoped to that
record update: `open-forge load --bodies` and the selected memory `chain` both
exited successfully; the legacy
`open-forge doctor` reports `0` errors and one pre-existing C# metadata warning;
the replacement `index --dry-run` reports exit `5` with no files changed because
the known archived metadata and handoff-routing gaps remain; the changed-
Markdown link check, stale-status check, commit-presence check, and
`git diff --check` pass.

For this C2 closeout, `open-forge load --bodies` exits successfully;
`open-forge doctor --json` reports `0` errors and the same one pre-existing C#
metadata warning; `git diff --check` and worktree cleanliness pass. No index ran
because no routing metadata changed.

Extension Create is Complete at protected public integration
`4c85d1d62004e8bdc885c51873ff6d9cdb6e6db5`, with command-local squash
`3ef81227ba50fba869f0129b958eabc6d0c29fbc`, exact final candidate
`789cc917f2d0cb38c5229cc2dc7fee013218d341`, and exact tree
`fa29bd9572df39b2d5457c35bb8a0bd6ba5a9945`. Its final managed Unit
`1356/1356`, Integration `575/575`, EndToEnd `132/132`, and `linux-x64`
Native AOT root version/ELF, Integration `575/575`, and EndToEnd `132/132`
evidence pass with zero failures and zero skips. Native dogfood makes no writes;
the final independent Sol/xhigh review is `PASS — ROBUST` at `0.98` confidence,
with direct PTY process proof deferred. C2 root Install is Complete. Final
reviewed candidate `11994e4d21ddc807b7480afc39ae3612e5a69a56` is
squash-integrated at `c60fcb98a57e9ec80769b9cb1d399ce13a227863`; both have
exact tree `464a4a6b6ef6447209edffbf53df7348c70691ed`. Final managed
`1390/598/136`, supported `linux-x64` Native AOT `598/136`, post-rebase focused,
and two independent Sol/xhigh review gates pass. Native dry-run dogfood safely
blocks on the repository's existing generated-region state without effects,
workspace changes, lifecycle publication, or a new external lock. C3 Route
Inspect interaction is squash-integrated at `fa3db1ee` with exact tree equality
to final reviewed candidate `37c9360`. C2's exact fully present ordered public
Install JSON result is accepted and frozen, including typed residual values
`none`, `retained`, and `unknown`. Generic and Framework-aware Route Init is
Complete at `cc5085ce`; Route Create is Complete at `19412d2`, exact tree
`2bbba7e`, and supplies the `RouteCreateJsonContext` and Route-help predecessor
slices. CLI Quality Remediation is Complete and squash-integrated at
`862cbf2a847b5adac77c6923e51f2c28c315415f`, exact tree
`571f104f507c3f72b7404cc0dacd86c818ec0a2e`, from reviewed implementation
`a4ccf19a3489d06f20a0fd940219be1acb22646d`. Task 3 “Route Update” is Complete
at accepted closeout `27df8325`, tree `b68d4349`. Root Update has accepted Gray
`ef584350`, bounded Gray addendum `7a9ded30`, and accepted Red `13fe18a9`, tree
`a834c59b`, after the complete Route Mutation M2 lane. Root Update is Complete
at phase 5/5, milestone 8/8; its squash integration, post-integration build, and
public Update `3/3` passed.

Route Init candidate `cb62b19`, tree `be93900`, has a warning-free Release
build; managed Unit `1481/1481`, Integration `695/695`, generated serialization
`18/18`, and published Find/Index/Route Init `14/5/6`; supported `linux-x64`
Native AOT serialization `18/18`, full Integration `695/695`, and the same
published `14/5/6` process suites. Every stated run has zero failures and skips.
Detached exact-tip Generic apply/Find/Index/no-op dogfood proves canonical flow
tags, unchanged post-application hashes, zero-byte external locks, and clean
worktree cleanup. Fresh independent YAML/Markdown-boundary and whole-task
Sol/xhigh reviews return `PASS` at `0.98` confidence. Open Forge metadata uses
only root `open-forge`; one shared Framework YAML boundary combines a typed
flow/block reader with a source-generated writer that emits canonical flow tags,
and one shared Markdown third-party boundary serves bounded typed consumers.

The neutral Markdown link-label projection is integrated at
`89a35a7876f39123d9538bca24126ff7197b9459`. It derives typed `Supported` or
`Unsupported` labels only from the pinned Markdig AST and fails unhandled or
incomplete labels closed. Release is `0` warnings and `0` errors; parser
`31/31`, Unit `1410/1410`, and Integration `614/614` pass with zero skips; fresh
independent Sol/xhigh review is `ROBUST PASS`. This adds no Route Remove command,
public wire, or shared mutation behavior.

Implemented boundaries:

- Repository-root `OpenForge.Cli.slnx`, SDK, NuGet, and shared MSBuild files.
- Root `artifacts/` for .NET binary, intermediate, test, publish, and package
  output.
- Automatic managed `open-forge-dev` publication from an ordinary non-RID CLI
  project build, including solution and EndToEnd dependency builds, with one
  explicit MSBuild opt-out.
- Environment-free EndToEnd discovery. Ordinary builds select the development
  publication; CI and native evidence compile the EndToEnd project with one
  supported target RID and no executable-path override.
- Linux and WSL portability corrections for symlink cleanup and file-sharing
  classification.
- Removal of the obsolete preserved route-list suite after its only unique
  empty-YAML expectation moved into active Integration evidence.
- Temporary Index and References compatibility-name routes and Task filenames
  remain in place. Index has validated the compatibility-name identity behavior;
  changing those route paths is outside this candidate closeout.

The Ubuntu SDK installation exposes a distro-specific `ubuntu.24.04-x64` local
AOT pack, while the accepted product RID remains portable `linux-x64`. With exact
restore authority, standard SDK publication restored
`Microsoft.NETCore.App.Runtime.NativeAOT.linux-x64` `10.0.11` from the configured
NuGet source and produced the native root without a project workaround. The
explicit `linux-x64` build passed with zero warnings and errors; build-selected
References EndToEnd passed `12/12`; Native AOT Integration passed `308/308`; and
Native AOT EndToEnd passed `82/82`, all with zero skips. No global installation,
external publication, remote action, or push occurred.

Route Create is Complete and squash-integrated at `19412d2`, exact tree
`2bbba7e`, from reviewed candidate `392114a` on `codex/route-create`. The
maintainer-approved final model makes each result and JSON
effect `Change` required while keeping `Change.Before` nullable. Restored locked
offline restore and format verification pass; the warning-free managed gate is
Unit `1507/1507`, Integration `720/720`, and EndToEnd `146/146`. Supported
`linux-x64` Native AOT passes root publication/version/ELF, EndToEnd against the
native root `146/146`, native Integration `720/720`, and native EndToEnd
`146/146`, all with zero failures or skips. Isolated native help, JSON dry-run,
apply, verified hashes, persistent zero-byte lock, and repeat no-op dogfood pass
without an authored repository mutation. Fresh Sol/xhigh `RC-R2` returns `PASS`
at `0.91` confidence with no material finding. The command-local serializer and
Route-help predecessor slices were consumed and revalidated by the Complete
CLI Quality Remediation Task.

## Current Step

Task 18 “Extension Remove” is Complete at phase 5/5, milestone 8/8. Accepted
candidate `9326a921`, final feature tip `8a6fa8de`, and squash integration
`f445a55a`, tree `3b0fb29d`, are recorded in the Task and project control
ledger. The post-integration Release build had zero warnings and errors;
focused Extension Remove Unit `55/55`, Integration `27/27`, public Remove
`3/3`, and retained public Doctor `3/3` passed with zero failures and skips.
Task 19 “Repair” is active at phase 4/5, milestone 3/8. Its accepted lane is
record tip `fb8ce672`, tree `129ef2ab`, with Gray `140920d3` and Red
`af59c957`. Integrated Task 18 was merged at `f0437512`, tree `25d88102`; the
refrozen transition `eae8eb36`, tree `4b2523e8`, opened coherent Green.

[Mutation Foundation](../cli-development/tasks/mutation-foundation/_mutation-foundation.md)
is Complete. Public [Index](../cli-development/tasks/read-only/index-command.md)
is Complete in exact feature candidate
`4e89d945b38a2d1e24600dd22789b55e4395a534`. Its mapping lists every named reason
in one grouped switch and retains an undefined-value runtime guard because C#
enums admit unnamed numeric values; no compiler-enforced exhaustiveness or
warning suppression is claimed. Final closeout tip
`2b353c48978ee88e53345be8037776181612222c` is locally integrated at
`09aa03eddb97831ff544afe1eac54ad9af501f5c` with exact tree equality. The
intended-membership formation prerequisite is integrated. Route Init, Route
Create, Route Update, Route Move, Route Remove, and root Install/Update consume
its accepted generated-navigation formation or projection capabilities.
Its integration closeout is the pre-foundation commit
`18f2acff31cfd6600a16430ac5d689d05482e297`, exact tree
`39f0a8c4e6d3695cfbe7407dfd6043dc5ec9680a`.

D0 and SF1-SF4 are now complete at the integrated baseline recorded above. C1
Extension Create is Complete at protected public integration
`4c85d1d62004e8bdc885c51873ff6d9cdb6e6db5`, with exact final candidate
`789cc917f2d0cb38c5229cc2dc7fee013218d341` and tree
`fa29bd9572df39b2d5457c35bb8a0bd6ba5a9945`. C2 root Install is Complete at
local integration `c60fcb98a57e9ec80769b9cb1d399ce13a227863`, exact tree
`464a4a6b6ef6447209edffbf53df7348c70691ed`. Its exact public Install JSON
result schema is accepted and frozen, including typed residual values `none`,
`retained`, and `unknown`. Generic and Framework-aware Route Init is Complete at
`cc5085ce`, and Route Create is Complete at `19412d2`, exact tree `2bbba7e`. It
supplies the `RouteCreateJsonContext` and Route-help predecessor slices. The
separate CLI Quality Remediation Task is Complete and squash-integrated at
`862cbf2a`, exact tree `571f104f`, from accepted implementation candidate
`a4ccf19a`, tree `97254e65`. Task 3 “Route Update” is Complete at phase 7/7,
milestone 12/12, on accepted closeout `27df8325`, tree `b68d4349`.
Task 4 “Route Move” is Complete at phase 6/6, milestone 12/12, on accepted
closeout `631983ea`, tree `d6f6fdf7`, and is squash-integrated at `d3d2dc1`,
tree `6959b51e`. Task 12 “CLI Architecture Authority Remediation”
is Complete at phase 5/5, milestone 6/6, from corrected source `a0e6bc8d`, tree
`9c4a33b1`, and is squash-integrated at `495a7ed6`, the same exact tree.
Route Init's Framework mode accepts one concrete
uniquely aligned route, slugs only ID-form inserted scope labels, creates one
sparse chain, keeps scope entrypoints user-owned, and lifecycle-manages only
copied canonical assets and derived generated regions.

Task 15 “Status” is complete and dequeued at phase 5/5, milestone 8/8. Its
accepted lane is `0c19b7053ef2b8c48898cfebadedff0c5702bf34`, tree
`8ff2ac4ac0f5985863a9aaf85911c2deb6152980`; correction
`f2dcdcae8b7c2cc9522b5ce7df250c73a55347db`, tree
`b6f83beb50cefe56a0aae7d9a4019dab08c19aea`; and Green
`7dd8de6c4f9eae3355f5ac4ee32d533e6ffb2564`, tree
`f6c37550b78a01e7611178bd563e274f94bb2bb1`. The integration parent is
`e90b22f9a6d4fdd2043516e718fc782490396cb2`, tree
`74713c32473603e7f9100378fafd314c50f813e3`; the integration identity is the
commit containing this record. Final gates had Release `0` warnings and `0`
errors; managed Unit `1739/1739`, Integration `933/933`, EndToEnd `172/172`;
managed EndToEnd against native root `172/172`; native Integration `933/933`;
native EndToEnd `172/172`; all with `0` failures, `0` skips, and `0` warnings.
Every later producer extends the explicit contributor inventory and affected
Status evidence before acceptance. Its historical completion-bearing update
had a `2 updates remaining` grace; that grace is consumed.

Task 16 “Doctor” is complete at phase 5/5, milestone 8/8. Its accepted lane is
`a48a16cd80102331bca6d6cb3498160f37eb6b3f`, exact tree
`90e66b0562fea90c2aa2fed9bd573c59676124bd`; local squash integration is
`59276c3bd764be4601ea92acbf4742b9bfa86837`, the same exact tree. Full managed,
public, supported `linux-x64` Native AOT, managed-on-native, focused,
structural, and post-integration Doctor evidence passed. Recovery attribution
remains unreleased schema v1; old unattributed or malformed final bundles fail
closed. The current Doctor catalogue contains exactly 108 kinds and 19
Extension kinds. Task 17 closed the bridge-registration observation; no
Extension observation horizon remains. Route Inspect's synthetic
overwrite-ambiguity conflict remains one bounded Task 10 audit input, not a
Doctor finding or a new task. See
the [Task 16 Doctor record](../cli-development/tasks/operations/doctor.md) and
[project control ledger](../cli-development/project-control.md) for the exact
accepted identities and evidence. Task 16's completion grace is consumed. Task
5 “Route Remove” is complete at phase 5/5, milestone 8/8. Its accepted lane is
`ab8da6202c2f638d3041de46fafeeced054c50b3`, exact tree
`bb41e1c9993a7ea28a7b380e442350fc2bbae73c`; local squash integration is
`5a2e650aa7623a2f19ff885ae664e56daded2df0`, the same exact tree, from parent
`0d269b7a`, tree `8863d176`. Its prior `1 update remaining` grace is consumed by
the accepted Task 6 Preflight update; Task 5 is dequeued. Task 6 “Root Update” is
Complete at phase 5/5, milestone 8/8. Its
exact activation is `0bc82357a4fc7bd54ecbca1d58501d721c04baa6`, tree
`41b74d9cad9c5d16ed8a02bb60ba06a4b8df8903`. Accepted Gray is
`ef584350a48d08b2d6307eea70f87f2f3c46283a`, tree
`f49f4be76607aec8d03d72252600e6c6718bc6a5`; its exact 13 production and three
Unit paths freeze 15 green contract facts and three intentional Red seams. The
bounded recovery-deletion addendum is
`7a9ded305e9ee185c02aaf636b4ff78f301b1fb1`, tree
`d2ebd52378095775897bbb64c59eebfef68cc0cf`. Accepted Red is
`13fe18a9d1cc9f829cc0cf778c44c96f97abddcb`, tree
`a834c59be9cc1c5409ee8502ac951ca02030fe40`; its eight test paths freeze 20
Unit, 22 Integration, and exactly three EndToEnd failures. The clean provisional
Gray/Red lane remains preserved, but both commits are rejected as transplant
units. T6-R1 is consumed with final focused rechecks PASS, and one logical T6-C1
is consumed with the immutable physical follow-ups retained. It is
squash-integrated at `c6eec9d26ad6b798d418d260027241795fb4aefc`, exact tree
`f31cacb0c54bda8ecf8a91d3516c6ffa48f48753`; the post-integration Release build
and public Update `3/3` passed.

Task 6's two subsequent progress-update grace is consumed and it is dequeued.
Task 17 “Extension Update” is Complete at phase 5/5, milestone 8/8 and is
dequeued after its completion grace was consumed. Its
accepted candidate `3bcb602569e7e2243a0780e1ff9cbd8f424b6457`, tree
`63f5b22c9c73dd9aaf2044c88401e5b91638e143`, is integrated into local
`develop` as `ae055a73597c4d2310b217dc67d053aa200282db`, the same tree, from
parent `a9987d5c208272370fc0fc1f647b7f253d12056c`. Its streamlined-assured
Preflight, Gray, Red, coherent Green, milestone-5 gates, grouped correction, and
final acceptance are accepted under Curie III and Sagan IV. Green is commit
`e25a721f0099de7d7ecd160e0e1563edad430194`, tree
`ddf020d66f2457dbb055b95eb26ca2cb8eae7639`. Red is commit
`4c6d68de09bc45070e42cb184963ba9ebd3c398a`, tree
`50a77b0d7b49313c2edd6266dc1bcd87f38a090b`; Gray is
`fce4d7f2ab054709b6b6b5ed2dd902842cfcbafb`, with correction
`1b3f90abee85103018ac9239342e994a297665bc`. The accepted immutable gate basis
is commit `10c2963f8e07109e5c6fb4afb8e566ba067d22b4`, tree
`570959dc93e3db7989c2ea662ebdaf0659c2b0ee`. The immutable review target is
the same commit and tree; the separately accepted milestone-5 record is commit
`38cc702801def02e2d8c59eb2d39b21aa91479fa`, tree
`511293c084dc5a4fda175b48e3918cc661438ea2`. Fresh whole-task review `T17-R1`
is consumed with final `CHANGES_REQUIRED` for exactly one accepted High finding,
`T17-R1-F2`: the planner collapses lifecycle `Invalid` and `Blocked` into
`LifecycleUnavailable` or `Incomplete`. The accepted repair maps `Invalid` and
`Blocked` to `LifecycleBlocked`; `DocumentMissing`, `SectionMissing`, and
`Unavailable` to `LifecycleUnavailable`; and `Cancelled` to `Interrupted`, with
focused lifecycle-gate evidence. `T17-R1-F1` is withdrawn as a false positive
because Extension Update next is only at-most-one and blocked null was frozen.
Recovery and cancellation have no finding. The absent JSON `frameworkLifecycle`
field is contract-correct. No architecture, C#, evidence, Native AOT, package,
or public journey finding was accepted. Exactly three Update and three Doctor
EndToEnd journeys remain preserved. Review budget is consumed; grouped correction
`T17-C1` was active under Curie III and is accepted below, and the review and
correction budgets are consumed. Curie III is paused. At this historical receipt,
Task 18 was `QUEUED` and inactive; the current checkpoint records it Complete
and activates Task 19 at the phase and milestone recorded above.

Milestone 7/8 correction `T17-C1` is accepted at commit
`8a3a754a4d20a8247a248b59e56abd1881530dd6`, tree
`f56ff6677974a949a336c51ab382b83229bf60b5`, from parent commit
`38cc702801def02e2d8c59eb2d39b21aa91479fa`, tree
`511293c084dc5a4fda175b48e3918cc661438ea2`. It maps `Invalid` and `Blocked`
to `LifecycleBlocked`; `DocumentMissing`, `SectionMissing`, and `Unavailable`
to `LifecycleUnavailable`; `Cancelled` to `Interrupted`; and `Available` to
the normal outcome. One three-row existing Integration theory covers malformed,
non-ordinary-path, and missing-lifecycle inputs, proving no effects and
unchanged workspace and source. The six-path path manifest SHA-256 is
`8c62f4aa43c05ae5b9923e140b29eb6963ae44bf57d16e99e86280a691c695b7`; the
ordered content manifest SHA-256 is
`48fcd926c79924d8a4afa3d4a4937442d23aab6d278327cdf0f4334fece3367c`.
Focused Integration and full solution Release builds reported `0` warnings and
`0` errors; Update Integration passed `11/11`, with zero failures and skips.
Formatting, diff, protected-path, callable-shape, prohibited-pattern, C# line,
durable host-path, and exact public-inventory checks are clean. Exactly three
Update and three Doctor EndToEnd journeys remain preserved. Unstaged and
untracked counts are zero. Curie III is paused. The next boundary is milestone
8/8 fresh final managed/public, supported `linux-x64` Native AOT, and packed
same-worktree acceptance.

Green's six-project full solution Release build exited `0` with `0` warnings
and `0` errors. Focused Update Unit, Update Integration, lifecycle observation,
Status Integration, and Doctor Integration passed `22/22`, `8/8`, `2/2`,
`15/15`, and `3/3`; public Update EndToEnd passed exactly `3/3`; and public
Doctor EndToEnd separately passed exactly `3/3`. Failures, skips, and warnings
were all `0`. Formatting, diff, protected-path, durable host-path,
prohibited-pattern, changed-line, and callable-shape checks are clean. Exactly
22 relative C# paths are accounted for, with 16 tracked and 6 formerly
untracked; the path and content manifest SHA-256 values are
`2a11af62c794a6918ccc4e620942bf8ed70cf13cccbd4bb7b54305e3bdd373aa` and
`f27b55be74212b46520384e2ce082920ac040d3d10edd99214232c8c410b4bd4`. The
complete path list remains in the [Task 17 record](../cli-development/tasks/lifecycle/extension-update.md).

Milestone 5 managed restore and the full solution Release build covered six
projects, exited `0`, and reported `0` warnings and `0` errors. Full managed
Unit, Integration, and EndToEnd passed `1854/1854`, `996/996`, and `184/184`.
The canonical default-version supported `linux-x64` root SHA-256 is
`beeb545a3b968681d79f231b089ecffbe7d6c55276508bae60815e3ee8c662a7`; native
Integration passed `996/996` at SHA-256
`2f7b8051a4b3360cf7c4f62d5459e1cca102b95cf5072e2677fb3ce3b0daa9bc`; native
EndToEnd passed `184/184` at SHA-256
`29e2c0adcc871aa5022bd12a607ca4a07a7a4432a9d8efa7d8b96e65f41795da`; and
managed-on-native EndToEnd passed `184/184`. Public Update and Doctor EndToEnd
separately passed exactly `3/3` each. Failures, skips, and warnings were `0`.

The accepted packed journey used version
`0.0.0-dev.sha-10c2963f8e07109e5c6fb4afb8e566ba067d22b4`; main and supported
`linux-x64` tarball SHA-256 values are
`e07a02f2969b855195cbc8d3639c2ca5bd80a2c29ce3176a07721ad288a63805` and
`3c37e2ecbe2863abab6199964a5cb7dbb2d51f8d26e4e1362d72c5da78fab209`.
Native root, staged native payload, and installed native payload match the root
SHA-256 above. Framework Install completed 44 effects, Extension Install 28
effects, embedded Update was a no-op, changed external Update completed one
effect with source-target SHA-256
`9c99cf2c3d165cfa06078a009d10f6120f356c1644f6f366da37102e8a413c00`, and the
repeat Update was a no-op. No findings, residuals, or warnings were reported.

Milestone 8/8 final acceptance is locked at immutable gate basis commit
`d3c29a2b20e77c18484b5ab58056063e019d469e`, tree
`18f76ed403fbe2e9047ad68f289b7c89100421b4`. The locked restore and full
solution Release build covered six projects, exited `0`, and reported `0`
warnings and `0` errors. Managed Unit, Integration, and EndToEnd passed
`1854/1854`, `999/999`, and `184/184`; public Update and Doctor each passed
exactly `3/3`; native Update and Doctor each passed exactly `3/3`; and
managed-on-native EndToEnd passed `184/184`. The default-version supported
`linux-x64` native root SHA-256 is
`67d561a5d877fd4516fa4e35a8a6e3accc67bb68f4f08266b229044c5fbbc154`;
native Integration passed `999/999` at SHA-256
`00ea4e7854371562ae9b28b7336484e5344d2e5ab3f197cd35ad3af0fdf25cfa`; native
EndToEnd passed `184/184` at SHA-256
`6abdf5551b94148942fd09f3434e943a4844afd96771d5a2879a473fcd10e04d`.
Failures, skips, and warnings were all `0`; prior native outputs remain
archived intact.

The final packed same-worktree journey had stage, pack, and install exit `0`
with SHA-versioned package manifests. Main and supported `linux-x64` tarball
SHA-256 values are
`d3c9b2eb24623729c5a225ca0112760bb27c77bbdd627344c4418e59aa81fc93` and
`118ba7b036b92caef55f856b152415c93f83bdefacbb09100a2be610e30ff594`.
Native root, staged native payload, and installed native payload all match the
native root hash above. Framework Install completed 44 verified effects and
Extension Install completed 28 verified effects. Embedded Update was a no-op;
the edited external Update completed one verified effect, with source-target
SHA-256 `9c99cf2c3d165cfa06078a009d10f6120f356c1644f6f366da37102e8a413c00`;
the exact repeat was a no-op. Findings, residuals, and warnings were zero, and
prior package outputs remain archived intact.

Task 17 “Extension Update” is Complete at phase 5/5 and milestone 8/8. Review
`T17-R1` and correction `T17-C1` are consumed; completion grace is consumed.
Task 18 “Extension Remove” is Complete, integrated, and dequeued after consumed
grace. Task 19 “Repair” is active at phase 4/5, milestone 3/8 with coherent
Green open from refrozen transition `eae8eb36`. Task 20 “Cleanup” is
task-locally active at phase 4/5, milestone 3/8 with accepted Red and Green
held. Task 23 “Workspace Libraries” is task-locally active at phase 2/5,
milestone 1/8 for Gray correction while queued after Task 20 for Green and
integration. Tasks 24 “Extensions Evolution”, 25 “Workspace Library
Destination Projections”, and 26 “Extension Internal Consolidation” are
queued post-command work with no active phase or milestone horizon.

The accepted candidate commit `3bcb602569e7e2243a0780e1ff9cbd8f424b6457`, tree
`63f5b22c9c73dd9aaf2044c88401e5b91638e143`, is integrated into local `develop`
as commit `ae055a73597c4d2310b217dc67d053aa200282db`, tree
`63f5b22c9c73dd9aaf2044c88401e5b91638e143`, from parent commit
`a9987d5c208272370fc0fc1f647b7f253d12056c`. The integration tree equals the
accepted candidate tree. The declared integration delta covers 60 paths: 40
added, 20 modified, and 0 deleted; its sorted-path SHA-256 is
`56d0dab3ba1f9463c864d3570c4c4875ef5975e2c935a39c2c56ed46d2b73911`. The
final full-gate basis is commit `d3c29a2b20e77c18484b5ab58056063e019d469e`,
tree `18f76ed403fbe2e9047ad68f289b7c89100421b4`.

Task 18 “Extension Remove” is Complete, integrated, and dequeued after consumed
grace. Task 17 is complete and dequeued after its completion grace was consumed.
Task 19 “Repair” is the active task at phase 4/5, milestone 3/8 with coherent
Green open. Task 20 retains accepted Red and holds Green behind Task 19.

Rejected evidence included SHA-qualified native build configuration, main-repo
`NODE_PATH`, npm offline materializations, and pre-closure stage attempts. Only
rejected SHA-qualified native outputs and partial failed-stage output were
preserved. Failed npm/`NODE_PATH` attempts were rejected and recorded, not
claimed as preserved artifacts. Accepted evidence used only worktree-local
ignored exact Bun-lock materializations (`TypeScript 6.0.2`, `@types/node
26.1.2`, and `undici-types 8.3.0`) and same-worktree source/artifacts.

Green implements the accepted `extension.bridge-registration` producer horizon,
which is set-valued:
one typed observation per exact lifecycle-owned routed Extension payload target
whose exact reviewed source facts form exactly one ordinary generated-navigation
parent `Entries` registration. Lifecycle supplies target identity and owners;
reviewed source bytes and metadata establish routed role; neutral formation and
projection supply the parent host and expected entry; and generated-entry
comparison supplies current, missing, unreadable, or inconsistent state.
Source-unavailable and ambiguous mapping remain incomplete or blocked without
inference. No manifest or lifecycle field, schema or compatibility change,
provider bridge, symbolic link, registry, dependency injection, fuzzy inference,
or installed-manifest scan is accepted.

Task 6 Preflight accepted the existing neutral payload, lifecycle, generated-
navigation, mutation, lock, and recovery providers without adding a generic
engine, sibling-private dependency, or seventh operational contributor. Update
owns historical retirement, complete comparison, policy, result, recovery
mapping, and orchestration. The exact one-confirmation matrix, schema-v1 result,
30 findings, next actions, 15 green Gray cases, exactly 45 intentional Red
failures, and exactly three Update public journeys are frozen in the Update
contracts and Task record. Existing Framework lifecycle and recovery
contributors own the durable Status/Doctor facts; exactly three Doctor public
journeys remain.

Task 6 Red closes the experimental authoring comparison. Four cold semantic
lanes produced no useful file before the stop rule fired; one warm context then
completed the disjoint test paths. First-file checkpoints improved visibility,
while central Task Mastermind reconciliation rejected false retirement,
revalidation, recovery-deletion, and Unit-tier evidence before builds. Exact
serialized mechanical gates then found fixture ownership, trim-safe JSON,
missing restore assets, and missing publication setup without shared-artifact
conflicts. Keep frozen read-only discovery and exact mechanical gates parallel;
reuse warm semantic context and serialize shared/public Green. No reliable
numeric speedup is claimed.

Task 17's flow observation is one serialized fresh build followed by parallel
exact no-build focused and public lanes that gave fast independent receipts
without overlapping semantic writes. Command-private lanes were effective after
the shared Green/build freeze; shared semantics remained serialized. Callable
tightening needed a later serialized pass, showing the core-first dependency.
No elapsed-time or numeric speedup claim is made.

Route Remove completes the Route Mutation M2 lane, and root Update M3 is
Complete at phase 5/5, milestone 8/8, after accepted Preflight and squash
integration. Its post-integration build and public Update `3/3` passed. Task 12
and the adoption slice remain integrated.

Initial read-only M2 preparation completed on five clean no-op branches from
`33913dfe7f8f80598ca4765c516d308ed179c3ab` without a preparation commit, Gray,
Red, or Green change. The later lifecycle and parser prerequisites above are now
integrated. The Route Mutation parent and leaf Tasks retain each accepted
preparation decision or remaining maintainer-authority frontier, expected and
protected path boundary, and evidence scope. Route Move and Task 12 are
Complete. Route Remove is accepted and integrated from a fresh Doctor-closeout
baseline. The M2
dependency remains ordered Install → Init → Create → protected Route Create
predecessor slices → Route Create integration → CLI Quality Remediation → Update
→ Move → Remove → M3.

The accepted future Status/Doctor composition boundary is one explicit immutable
application-scoped `OperationalContributorCatalogue` built by
`CliCompositionRoot`. Producer-owned typed contributors expose narrow Status and
Doctor views from fresh invocation observations. There is no dependency
injection, service locator, reflection, runtime registry, generic operational
engine, or ambient registration. Task 12 owns the durable architecture rewrite;
Task 15 Gray freezes exact signatures. Composition alone does not change Status
or Doctor public contracts.

The root composition boundary and all localized D0 choices are closed. Route
Inspect accepts one answer by one-based number or exact displayed path only when
stdin and stderr are terminal-capable. The shared directory effect holds the
workspace lease, revalidates the missing target and physical parent, uses
ordinary BCL creation, verifies, and retains residuals without recovery or
rollback. A missing `.agents` is the first ordinary visible planned/reported
lease-bound directory effect. The persistent external zero-byte lock uses the
full normalized-workspace SHA-256 key under the application-owned lock catalogue;
bootstrap result state no longer exists. Extension Create has exact defaults, an existing safely
resolved marker-free catalogue-parent boundary, exact-destination-only collision
inspection, one ordered command-local JSON result, and a missing-fact wizard
with local invalid correction, EOF-invalid, and cancellation-interrupted
semantics. Install prompts once only
for a prompt-capable human apply that would write after preflight; refusal, EOF,
or cancellation is a no-write interruption, while dry-run, no-op, automatic,
JSON, and redirected flows never prompt.

## Protected State

- Do not stage or alter unrelated `.apm` and `apm.lock.yaml` worktree changes.
- Do not edit sealed Handoffs or archived evidence to rewrite history.
- Preserve Task 12's integrated routed authority and its unchanged executable,
  public-contract, package, dependency, platform, and test meaning.
- Do not implement public Index selection, binding, application, locking,
  recovery, or result presentation in the Generated Navigation lane.
- Do not download additional dependencies without exact authorization.
- Do not contact remotes, publish, globally install, deploy, or push.

## Next Actions

1. Complete Task 19 Repair Green and milestone-4 acceptance. Preserve Task
   20's accepted Red, then refreeze and open its Green after Task 19 integration.
2. Complete Task 23 Gray correction, then its accepted Red boundary. Keep
   semantic Green and integration after Task 20.
3. Prepare final functional drafts for Tasks 24 and 25 during independent
   command work and return them to the user before implementation. Complete
   accepted Task 24 work, then Task 25, before Task 26's pure six-command
   refactor. Do not assign their phase or milestone horizons in this queue edit.
4. Run Task 10, conditional Task 21 for accepted findings, Task 13, and Task 22
   in that order. Publication still requires its separately authorized boundary.

## Current Sources

- [Replacement CLI Architecture](../../crystallized/documents/cli/architecture.md)
- [CLI Project Control Ledger](../cli-development/project-control.md)
- [Development Plan](../cli-development/plan.md)
- [Program Task](../cli-development/tasks/00-cli-development.md)
- [Repository-Root Developer Workflow Task](../cli-development/tasks/repository-root-developer-workflow.md)

## Extension Queue Reconciliation

Done: Preserved all eleven inherited queue drafts and reconciled the older main
idea and checkpoint changes with the user's re-enabled queue. The invalid old
Task 24 milestone 0/8 draft was superseded; no horizon is accepted.
Now: Task 24 “Extensions Evolution”, with no accepted phase or milestone
horizon, has functional draft preparation active.
Next: Return final Tasks 24 and 25 functional drafts to the user before
implementation; maintain the dependency order in the project control ledger.
Blocker: Package vocabulary and destination-permission proposals still require
user review. They do not block Repair or bounded Library Gray work.
