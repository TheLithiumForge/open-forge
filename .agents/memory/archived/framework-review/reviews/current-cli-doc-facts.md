---
open-forge:
  description: CLI command and package documentation facts inspected at the recorded source revision
  tags: [Memory, Archived, Contextual, Historical, Framework, Review]
---

# Current CLI documentation facts

This is an earlier review snapshot retained for the active Task 28 Git review. Its status and paths describe the recorded stage. Use [Task 28](../../cli-development/tasks/source-framework-review.md) for current decisions, completion, and remaining work.

Read-only evidence packet for the public-docs rewrite. The immutable comparison
ref is `develop` at `2b54fdc598a48a12e44772a5cb323cf45e9e5a73`; the review
worktree is `28cac0fc47e43671aa6458d318293594542d9bbf`.

## Direct answer

- The current replacement CLI exposes 28 commands under `open-forge`: install,
  update, cleanup; six Extension operations; five Workspace Library operations;
  status, context, find; seven route operations; index, references, doctor,
  and repair.
- The stable global options are `--workspace <path>`, `--json`,
  `--view <compact|expanded>`, `--verbose`, `--help`, and `--version`.
  Human output is expanded by default. JSON writes one complete structured
  result to stdout; bounded diagnostics use stderr. Human output routes
  complete/attention/incomplete to stdout and invalid/blocked/failed/interrupted
  to stderr. Exit codes are 0, 1, 2, 3, 4, 5, and 130 in that status order.
- The Framework is usable without the CLI. The CLI is the native .NET 10
  accelerator for selecting context, inspecting and maintaining routes,
  rebuilding generated navigation, diagnosing workspaces, and managing
  lifecycle state.
- The public package is not yet a release fact: the source contains six npm
  platform package manifests and a thin launcher, but `docs/development.md`
  and the delivery records still describe matching-host and final delivery
  evidence as incomplete. Documentation should say “development/replacement
  CLI” until the release gate changes that status.
- Two prepared follow-up tasks are not current behavior: scoped continuity
  loading and embedded Extension catalogue synchronization. Keep their
  accepted target behavior separate from the currently implemented interface.

## Command surface and useful examples

| Family                    | Current commands and a stable example                                                                                                                                                                                                                                                                      | Source anchor                                                                        |
| ------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------ |
| Framework lifecycle       | `open-forge install --dry-run`; `open-forge update --dry-run [--force] [--prune]`                                                                                                                                                                                                                          | `Commands/Install/InstallDefinitions.cs`; `Commands/Update/UpdateDefinitions.cs`     |
| Recovery support          | `open-forge cleanup --dry-run`                                                                                                                                                                                                                                                                             | `Commands/Cleanup/CleanupDefinitions.cs`                                             |
| Extensions                | `open-forge extension list --available`; `open-forge extension inspect development-toolkit`; `open-forge extension install development-toolkit --dry-run`; `open-forge extension update --all --dry-run`; `open-forge extension remove development-toolkit --dry-run`                                      | `Commands/Extension/*/*Definitions.cs` and `ExtensionBinding.cs`                     |
| Workspace Libraries       | `open-forge library list`; `open-forge library inspect team-knowledge --json`; `open-forge library attach team-knowledge shared/team-knowledge --dry-run`; `open-forge library sync team-knowledge --dry-run`; `open-forge library detach team-knowledge --dry-run`                                        | `Commands/Library/*/*Definitions.cs` and `LibraryBinding.cs`                         |
| Status and context        | `open-forge status`; `open-forge context`; `open-forge context memory/crystallized/documents --additions-only`                                                                                                                                                                                             | `Commands/Status/StatusDefinitions.cs`; `Commands/Context/ContextDefinitions.cs`     |
| Find                      | `open-forge find --tag=Memory --tag=CurrentTruth`; `open-forge find --heading=Axioms --within=body`                                                                                                                                                                                                        | `Commands/Find/FindDefinitions.cs`                                                   |
| Routes                    | `open-forge route list`; `open-forge route inspect memory/crystallized/documents`; `open-forge route init memory/project-alpha/documents`; `open-forge route create memory/project-alpha/documents/decision.md`; `open-forge route update ...`; `open-forge route move ...`; `open-forge route remove ...` | `Commands/Route/*/*Definitions.cs` and `RouteBinding.cs`                             |
| Navigation and references | `open-forge index --dry-run`; `open-forge references memory/crystallized/documents`                                                                                                                                                                                                                        | `Commands/Index/IndexDefinitions.cs`; `Commands/References/ReferencesDefinitions.cs` |
| Diagnosis and repair      | `open-forge doctor`; `open-forge repair --automatic --dry-run --json`                                                                                                                                                                                                                                      | `Commands/Doctor/DoctorDefinitions.cs`; `Commands/Repair/RepairDefinitions.cs`       |

The seven route operations are `inspect`, `list`, `init`, `create`, `update`,
`move`, and `remove`. The six Extension operations are `list`, `inspect`,
`create`, `install`, `update`, and `remove`. The five Library operations are
`list`, `inspect`, `attach`, `sync`, and `detach`.

## User-visible boundaries

- Read-only discovery/retrieval includes `status`, `context`, `find`,
  `references`, `route inspect`, `route list`, `extension list`,
  `extension inspect`, `library list`, and `library inspect`.
- `--dry-run` is the preview boundary for supported mutations. It plans and
  reports the same requested operation without applying target effects. It does
  not grant authority or make lifecycle state trusted.
- Framework mutations are `install`, `update`, route `init/create/update/move/
remove`, and `index` (generated navigation). Extension mutations are
  `extension create/install/update/remove`; Library mutations are
  `library attach/sync/detach`; `cleanup` deletes recognized recovery support
  artifacts; `repair` applies only accepted repair proposals and requires its
  own explicit selection/automatic boundary.
- `install` establishes management for the shipped Framework. `update` works
  from trusted lifecycle state; ordinary update preserves changed, missing, and
  retired divergence, while `--force` and `--prune` select their named
  boundaries. There is no generic apply, restore, reinstall, or Framework
  remove command.
- Extension `create` writes a package scaffold at the selected catalogue
  destination. Extension install/update sources are read-only and must be
  separate from the target workspace. Dependencies resolve offline inside the
  selected source universe. Remove releases trusted ownership and protects
  shared or changed files; `--prune` is the explicit deletion boundary.
- The native CLI uses `.agents/open-forge.lifecycle.json` with isolated
  `framework` and `extensions` sections. The old `open-forge.extensions.json`
  is not read by the replacement CLI. External destination permissions are
  separately represented in `.agents/open-forge.permissions.json` where the
  applicable operation requires them.
- The CLI's managed writes plan and verify before effects and preserve recovery
  evidence for existing-target effects. Public docs should describe the
  observable safety boundary and link to contracts, without reproducing the
  recovery implementation.

## Extension package facts

The current first-party source catalogue under `src/extensions/` contains one
managed package: `development-toolkit` (`version` `0.1.0`, no dependencies).
The package contains `extension.json`, an optional `README.md`, and complete
target-relative files under `content/.agents/`. The manifest fields are
`id`, `name`, `description`, `version`, and duplicate-free sorted
`dependencies`; unknown fields are rejected. The package provides six
Workflows, one native `SKILL.md` Skill with references, and nine Templates.

The review worktree's source changes add or revise catalogue material compared
with its base. The embedded CLI catalogue on `develop` is a separate generated
asset and must be synchronized by the prepared catalogue task before docs can
claim that omitted `--source` exposes every accepted first-party package.
The source catalogue and embedded catalogue are therefore distinct authority
surfaces until that task is completed.

Manual installation remains supported: copy reviewed `content/` files into the
workspace, update affected generated `Entries`, and review the assembled diff.
Manual copying does not create managed lifecycle state.

## Setup and package availability

- Build prerequisites declared by the repository are .NET SDK `10.0.100` with
  latest-feature roll-forward, Node `>=22.18.0`, and Bun `>=1.3.0` for the
  transitional root scripts. The native CLI targets `net10.0` and has
  Native-AOT RIDs `win-x64`, `win-arm64`, `linux-x64`, `linux-arm64`, `osx-x64`,
  and `osx-arm64`.
- Restore the locked solution with
  `dotnet restore OpenForge.Cli.slnx --locked-mode --configfile NuGet.Config
--nologo`. A Debug development artifact is documented at
  `artifacts/publish/open-forge-dev/Debug/open-forge-dev`; the root convenience
  command is `npm run cli:dev -- --help` after restore.
- The npm distribution is a thin launcher package
  `@thelithiumforge/open-forge` with optional platform packages for six
  host/RID combinations. `npm run cli:link` is an explicit maintainer action
  that creates global npm links; ordinary worktree evidence should invoke that
  worktree's published artifact instead. Publication and final delivery are
  not established by source presence alone.
- The frozen TypeScript MVP remains a separate historical executable exposed
  by `open-forge-old` and `npm run cli:old`; new public setup should lead with
  the replacement CLI and label the MVP historical.

## Evidence table

| Evidence                          | Exact location or symbol                                                                                                          | Why it matters                                                                             |
| --------------------------------- | --------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------ |
| Command names and global options  | `src/cli/core/OpenForge.Cli.Core/Shell/Definitions/CliSyntaxDefinitions.cs`, `CliTerminalPolicy`                                  | Canonical executable name, six global flags, terminal help/version rules                   |
| Status names, exit codes, streams | `Shell/Definitions/CliStatusDefinitions.cs`                                                                                       | Complete/failed/attention/incomplete/invalid/blocked/interrupted policy                    |
| Command family bindings           | `Commands/*/*Binding.cs` and `*Definitions.cs`                                                                                    | Public command grammar and operation grouping                                              |
| Current public command guide      | `docs/cli.md` on `develop`                                                                                                        | Stable examples and user-facing boundaries; verify against contracts before release claims |
| Extension package meaning         | `src/extensions/README.md`, `src/extensions/development-toolkit/extension.json`                                                   | Current source catalogue, package shape, first-party contents                              |
| Extension lifecycle source        | `Commands/Extension/*`, `.agents/memory/crystallized/documents/cli/contracts/extension/`                                          | Managed ownership, dependencies, install/update/remove boundaries                          |
| Repository setup                  | `global.json`, `Directory.Build.props`, `Directory.Packages.props`, `package.json`, `src/cli/package-managers/npm/*/package.json` | Tool versions, build outputs, six package targets                                          |
| Prepared future behavior          | `.agents/memory/working/cli-development/tasks/scoped-continuity-loading.md`; `extension-catalogue-synchronization.md`             | Explicitly pending CLI work; do not document target behavior as shipped                    |

## Conflicts and uncertainty

- The review branch's public docs contain mixed `content/` and historical
  `payload/` wording. The current source package shape uses `content/`; keep
  `payload` only when describing serialized lifecycle fields or clearly
  historical material, and scrub the mixed public prose during the docs rewrite.
- `develop` does not contain a record literally named Task 29. The review
  worktree does contain the two prepared CLI tasks above. Treat “Task 29” as a
  user/project reference that must be mapped to the selected implementation
  branch before stating its status.
- The source catalogue and embedded catalogue are not proven equal by this
  read-only packet. The embedded-catalogue synchronization task owns that
  proof.

## Smallest useful next check

Before finalizing public CLI docs, select the actual CLI implementation baseline
for the separate CLI chat, run that artifact's `--help` and a read-only command
per family, and reconcile those outputs with the current develop contracts.

Confidence: high for command grammar, global options, package shape, setup files,
and status policy; medium for release availability and embedded catalogue
parity because those are explicitly pending delivery evidence.
