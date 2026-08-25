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
repository-root developer workflow is Complete and squash-integrated into local
`develop` at `d9e0686`. References remains Planned and is not authorized.

The active branch is `develop`. The accepted developer-workflow direction is
recorded in the [repository-root developer workflow
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

The unchanged managed baseline is `1190/1190`. After review correction
`DX1-C1`, focused final evidence passes root selection at `72/72`, ordinary
publication-selected EndToEnd at `70/70`, and explicit `linux-x64`
publication-selected EndToEnd at `70/70`, all with zero skips. The local Native
AOT rerun is blocked before
compilation because the installed WSL SDK lacks the accepted `linux-x64` Native
AOT runtime pack; no download was authorized. No global installation, external
publication, remote action, or push occurred.

## Current Step

Await maintainer selection of the next CLI Task. Do not begin References, Index,
or mutation behavior without that selection.

## Protected State

- Do not stage or alter unrelated `.apm` and `apm.lock.yaml` worktree changes.
- Do not edit sealed Handoffs or archived evidence to rewrite history.
- Do not implement References or Index behavior.
- Do not download dependencies without exact authorization.
- Do not contact remotes, publish, globally install, deploy, or push.

## Next Actions

1. Keep References and Index Planned until the maintainer selects the next Task.
2. Create the next feature branch only after that Task and its dependencies are
   accepted for implementation.

## Current Sources

- [Replacement CLI Architecture](../../crystallized/documents/cli/architecture.md)
- [Development Plan](../cli-development/plan.md)
- [Program Task](../cli-development/tasks/00-cli-development.md)
- [Repository-Root Developer Workflow Task](../cli-development/tasks/repository-root-developer-workflow.md)
