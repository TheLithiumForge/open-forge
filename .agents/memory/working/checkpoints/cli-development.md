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

- Last updated: 2026-08-29.

## Current State

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
persistent zero-byte `.agents/open-forge.lock` is the accepted reusable lock-file
result. Verification used the existing cached and offline prepared dependency
state. No fresh remote NuGet vulnerability audit, remote CI, push, deployment,
release, or publication is claimed. Local squash integration into `develop` and
exact tree-equality proof remain pending.

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

## Current Step

[Mutation Foundation](../cli-development/tasks/mutation-foundation/_mutation-foundation.md)
is Complete. Public [Index](../cli-development/tasks/read-only/index-command.md)
is Complete in exact feature candidate
`4e89d945b38a2d1e24600dd22789b55e4395a534`. Its mapping lists every named reason
in one grouped switch and retains an undefined-value runtime guard because C#
enums admit unnamed numeric values; no compiler-enforced exhaustiveness or
warning suppression is claimed. The current step is to squash-integrate this
accepted candidate into local `develop` and prove exact tree equality. Do not
push. M2 remains Pending and becomes eligible only after that integration proof.

## Protected State

- Do not stage or alter unrelated `.apm` and `apm.lock.yaml` worktree changes.
- Do not edit sealed Handoffs or archived evidence to rewrite history.
- Do not change accepted Find, References, Context, Extension List, Route, Shell,
  or source-reference meaning during the wave.
- Do not implement public Index selection, binding, application, locking,
  recovery, or result presentation in the Generated Navigation lane.
- Do not download additional dependencies without exact authorization.
- Do not contact remotes, publish, globally install, deploy, or push.

## Next Actions

1. Squash-integrate accepted Index candidate
   `4e89d945b38a2d1e24600dd22789b55e4395a534` into local `develop`, then prove
   exact tree equality. Do not push.
2. Only after that integration proof, keep the Route Init exact-chain decision
   and lane graph queued before M2;
   retain thin D1 as current `linux-x64` build/smoke, packed install/invocation,
   and checksums; keep future RIDs, signatures, SBOM, provenance, OIDC, and
   support floors behind a later explicit decision; retain relevant-domain
   Repair and the simplified O1/O2 split.
3. Keep M2 Pending until the accepted I1 candidate is present in local `develop`
   with exact tree equality; do not mark it Active or Ready earlier.
4. Keep actual Status/Doctor implementation in O1 and actual Cleanup selection,
   default, empty-no-lease policy, orchestration, guidance, aggregation, and E2E
   in O2.

## Current Sources

- [Replacement CLI Architecture](../../crystallized/documents/cli/architecture.md)
- [Development Plan](../cli-development/plan.md)
- [Program Task](../cli-development/tasks/00-cli-development.md)
- [Repository-Root Developer Workflow Task](../cli-development/tasks/repository-root-developer-workflow.md)
