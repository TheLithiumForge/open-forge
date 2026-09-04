---
open-forge:
  description: Current state and next action for the greenfield replacement CLI development program
  tags: [Memory, Working, Checkpoint, Active, KeepInMind, CLI, Architecture, Plan, Task, Contextual]
---

# CLI Development Checkpoint

## Goal

Complete the replacement CLI from its accepted Architecture and command
contracts, with safe local development, reproducible evidence, and no partial
release.

- Last updated: 2026-09-04.

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

Task 15 “Status” is ACTIVE at phase 3/5, milestone 3/8. Gray
`dac2aece`/`874bdcae`, bounded callable correction `df46fb9c`/`eda74e79`, and
accepted Red `f633fe1f`/`5dbfe9d7` are recorded. One Sol/xhigh Brilliant
Implementer was activated at `de27fcd6`/`ebdcb4a6`, then the early architecture
checkpoint found that the two lifecycle Status signatures cannot guarantee one
common lifecycle-document observation without prohibited mutable state.
Corrected raw-snapshot Gray `b859d0aa`/`8f417c58` now makes that stateless
invocation-local observation explicit and passes a warning-free Core Release
build. Corrected Red `d768ca52`/`7a9b785b` is accepted with 14 Unit, 10
Integration, and 3 unchanged EndToEnd tests. Resume checkpoint
`d120d49e`/`28f23430` reapplied the interrupted Implementer's 94-line partial
`StatusDefinitions` work losslessly; recovery stash `9b51b3f8` remains retained
while the same Sol/xhigh Brilliant Implementer owns coherent production and
focused verification.
After `T15-S1`, the immutable Status continuation is
`cf00fb86`/`bf69e7c2` → `48309613`/`b8e0ad0f` → `e79a9767`/`17068f0b` →
`2521574d`/`11eb89ee` → `b8ac2dd7`/`453eb1b8`, covering rendering and
compatibility Green, direct JSON context, plural-source Gray, direct producer
construction authority, and focused plural-source Red. Red selected, discovered,
and executed `1/1/1`; only the expected three-observation versus one-scaffold
oracle failed, while lifecycle completeness/trust and five package/source facts
passed. The same Sol/xhigh Brilliant Implementer is implementing plural
semantics, and independent Route selected-view work is present. No stable
full-producer evidence or completion is claimed.
Task 16 “Doctor” is PREPARED at phase 2/5, milestone 2/8 from the exact Status
Gray snapshot. Activation `26e245e4`/`680df032` and accepted Gray
`ce627593`/`1f07abb7` are recorded. The lane is cleanly parked before
Red/production until Task 15 acceptance/integration. No tests or completion are
claimed.
Its prepared alignment consumes the complete plural Extension view and never
reads the removed singular source. The six Doctor-facing signatures and
catalogue shape remain unchanged through Status `2521574d`, while two
Status-only methods changed. After Status acceptance, reapply 17 C# files and
two contract amendments, then add Red that consumes every supplied observation
without fallback, substitution, reconstruction, or reread. No Task 16 files,
tests, builds, or implementation occurred in this alignment.

Task 7 “npm Package Manager Release and Local Linking” is Complete at phase 4/4,
milestone 7/7. Accepted lane `a2942781`, tree `fe36fc3f`, is squash-integrated at
`e19d429e` with the same tree. Its Linux host journey passed; Darwin and Windows
have stage-and-pack evidence only. ARM, publication, and live link or unlink
remain unproven and unauthorized.

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
at accepted closeout `27df8325`, tree `b68d4349`. Root Update remains after the
complete Route Mutation M2 lane.

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
- Root `/artifacts/` for .NET binary, intermediate, test, publish, and package
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

Route Remove is the remaining Route Mutation M2 leaf, and the full M2 lane still
precedes root Update M3. Task 12 is integrated, and the current project-control
priority places the adoption slice before Route Remove. Independent preparation
may still run early in isolated lanes, but dependent Update behavior waits for
M2 completion.

Initial read-only M2 preparation completed on five clean no-op branches from
`33913dfe7f8f80598ca4765c516d308ed179c3ab` without a preparation commit, Gray,
Red, or Green change. The later lifecycle and parser prerequisites above are now
integrated. The Route Mutation parent and leaf Tasks retain each accepted
preparation decision or remaining maintainer-authority frontier, expected and
protected path boundary, and evidence scope. Route Move and Task 12 are
Complete. Route Remove remains Planned after the current adoption slice. The M2
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

1. Continue active Task 15 Status at phase 3/5, milestone 3/8 through the bounded
   lifecycle-observation Gray/Red correction before coherent production resumes.
   Continue Task 16 Doctor at its prepared phase 2/5, milestone 2/8 boundary
   until Task 15 is accepted and integrated. Task 7 package expansion is
   complete.
2. After Task 15 integration, revalidate Doctor's six shared view blobs before
   Red; do not parse Status output or add a dynamic registry.
3. Resume Task 5 Route Remove, then Task 6 root Update and the remaining
   project-control order.
4. Retain thin D1 as current `linux-x64` build/smoke, packed
   install/invocation, and checksums. Keep future RIDs, signatures, SBOM,
   provenance, OIDC, and support floors behind a later explicit decision.

## Current Sources

- [Replacement CLI Architecture](../../crystallized/documents/cli/architecture.md)
- [CLI Project Control Ledger](../cli-development/project-control.md)
- [Development Plan](../cli-development/plan.md)
- [Program Task](../cli-development/tasks/00-cli-development.md)
- [Repository-Root Developer Workflow Task](../cli-development/tasks/repository-root-developer-workflow.md)
