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

Find is Complete and squash-integrated into local `develop` at `1f03d16`. The
repository-root developer workflow is Complete. References is Complete and
squash-integrated into local `develop` at `53b2cf7`, with exact tree equality to
accepted feature tip `1312368`.

The maintainer selected References before Context. References Preflight is
closed. Its neutral foundation is accepted at `e7516f0`, operation at `f7bb9f9`,
structure improvement at `6922ca6`, final Red at `43b75f3`, and public Green at
`23d5e2e`. Managed `1270/1270`, published process, no-write, architecture,
protected-surface, and supported local `linux-x64` Native AOT evidence pass.

The accepted References feature branch remains `codex/cli-references` in its
dedicated worktree; the current integrated program baseline is local `develop`
at `53b2cf7`. The accepted developer-workflow direction is recorded in the
[repository-root developer workflow
Task](../cli-development/tasks/repository-root-developer-workflow.md) and the
[Repository-Root CLI Tooling Decision](../../crystallized/decisions/repository-root-cli-tooling.md).

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

Discuss the pending task graph and acceptable parallel lanes before selecting the
next implementation Task. Context is now dependency-eligible, and Extension
discovery remains an independent candidate. Do not start either implicitly and
do not push.

## Protected State

- Do not stage or alter unrelated `.apm` and `apm.lock.yaml` worktree changes.
- Do not edit sealed Handoffs or archived evidence to rewrite history.
- Do not change the accepted References result, finding-code, supported-link,
  fragment, or generated-region meanings during implementation. Do not implement
  Index behavior.
- Do not download additional dependencies without exact authorization.
- Do not contact remotes, publish, globally install, deploy, or push.

## Next Actions

1. Review the remaining task order and parallelization boundaries with the
   maintainer.
2. Select and close the preflight for the next Task or explicitly authorized
   parallel wave; keep remotes unchanged.

## Current Sources

- [Replacement CLI Architecture](../../crystallized/documents/cli/architecture.md)
- [Development Plan](../cli-development/plan.md)
- [Program Task](../cli-development/tasks/00-cli-development.md)
- [Repository-Root Developer Workflow Task](../cli-development/tasks/repository-root-developer-workflow.md)
