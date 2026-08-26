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

Find, References, Context, and Extension List are Complete. Context is
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
Inspect's exact public contract is accepted and squash-integrated at `92313a0`;
the leaf is implementation-ready from `db037ee`. No remote action or push
occurred.

Routed Authored Metadata is Complete and squash-integrated at `5924698`. Its
accepted neutral parser preserves `SourceDocumentForm` as the sole classifier
and passes full managed Unit `1012/1012`, Integration `354/354`, and local
`linux-x64` Native AOT Integration `354/354`, all with zero failures/skips.
Generated Navigation's sole project-change request is therefore closed.

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

Continue the authorized wave from integrated routed-metadata baseline `5924698`:

1. implement Extension Inspect from its accepted exact
   command-local result/finding/fingerprint contract in an independent lane; and
2. rebase and complete the pure read-only
   `Framework/GeneratedNavigation` foundation with no command, lock, write,
   recovery, root registration, or JSON surface.

Public Index application and Mutation Foundation remain sequential. Do not start
either until their current dependency-order contradiction is resolved in the
accepted Architecture and Plan. Do not push.

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

1. Rebase Generated Navigation onto the post-closeout baseline, consume the
   promoted fact, and rerun its complete affected evidence and independent review.
2. Review and integrate only accepted Extension Inspect and Generated Navigation
   candidates, reproduce the combined evidence, and keep remotes unchanged.

## Current Sources

- [Replacement CLI Architecture](../../crystallized/documents/cli/architecture.md)
- [Development Plan](../cli-development/plan.md)
- [Program Task](../cli-development/tasks/00-cli-development.md)
- [Repository-Root Developer Workflow Task](../cli-development/tasks/repository-root-developer-workflow.md)
