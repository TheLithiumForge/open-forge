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

## Current State

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
The final Sol/xhigh correction recheck is `ACCEPTED`. No remote action or push
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
Integration `357/357`, with zero failures or skips. The final Sol/xhigh recheck is
`ACCEPTED`; post-integration focused Unit `18/18` and Integration `3/3` pass.

The constants/test-architecture audit is Complete and squash-integrated into
local `develop` at `b6ce31f`. The integrated tree exactly matches its accepted
feature tree and retains final managed Unit `1024/1024`, Integration `409/409`,
EndToEnd `116/116`, and supported local `linux-x64` Native AOT Integration
`409/409` evidence. No push or remote action occurred.

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
- Temporary Index and References compatibility-name routes and Task filenames,
  pending replacement Index validation of their final names.

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

[Correct Read-Only CLI Dogfooding
Defects](../cli-development/tasks/read-only-dogfooding-corrections.md) is Complete
on accepted local feature candidate `deb3f14` from exact clean integrated
baseline `b6ce31f`. It closes only
`CLI-EDGE-011`, `CLI-EDGE-012`, and `CLI-EDGE-015`: consume the accepted
form-aware metadata fact in Context, make compact Context and Find findings name
known affected coordinates, and make Route Inspect help describe implemented
Context truthfully. It must retain malformed Skill fail-closed behavior,
structured results, statuses, no-write behavior, and managed/Native AOT parity.
Final managed Unit `1031/1031`, Integration `411/411`, and EndToEnd `120/120`
pass; freshly published portable `linux-x64` Native AOT Integration `411/411`
and EndToEnd `120/120` pass. All suites have zero failures/skips. Repository
dogfooding preserves the expected 65 Context and 66 Find metadata findings while
naming every compact coordinate and no longer blaming the valid
`experience-design/SKILL.md`.

Locally squash-integrate the accepted candidate when authorized, then resume
Mutation Foundation at the exact persisted Framework lifecycle-schema decision.
Public Index remains sequential after Mutation Foundation, followed by route
mutation, lifecycle mutation, operations, delivery, and final acceptance. Do not
create an Index-local mutation substitute. Do not push.

## Protected State

- Do not stage or alter unrelated `.apm` and `apm.lock.yaml` worktree changes.
- Do not edit sealed Handoffs or archived evidence to rewrite history.
- Do not change accepted Find, References, Context, Extension List, Route, Shell,
  or source-reference meaning during the wave.
- Do not implement public Index selection, binding, application, locking,
  recovery, Git policy, or result presentation in the Generated Navigation lane.
- Do not download additional dependencies without exact authorization.
- Do not contact remotes, publish, globally install, deploy, or push.

## Next Actions

1. Squash-integrate accepted local dogfooding candidate `deb3f14` without a
   remote action.
2. Return to the exact Mutation Foundation lifecycle-schema decision, then freeze
   and accept its callable contracts before implementing lock, lifecycle,
   revalidation, atomic-apply, receipt, recovery, and Git children in dependency
   order. Keep remotes unchanged.

## Current Sources

- [Replacement CLI Architecture](../../crystallized/documents/cli/architecture.md)
- [Development Plan](../cli-development/plan.md)
- [Program Task](../cli-development/tasks/00-cli-development.md)
- [Repository-Root Developer Workflow Task](../cli-development/tasks/repository-root-developer-workflow.md)
