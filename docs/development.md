# Developing Open Forge

This guide describes how to develop and verify Open Forge. Start with the
[README](../README.md) when you want to use the Framework.

## Enter The Workspace

Read `AGENTS.md` and `.agents/loader.md` before changing Open Forge. Use the
[Sources Of Truth map](../.agents/maps/sources-of-truth.md) to locate the source
that defines each affected question.

Current responsibilities are:

| Source                            | Responsibility                                                |
| --------------------------------- | ------------------------------------------------------------- |
| `src/open-forge/`                 | Installable Framework wording                                 |
| `src/extensions/`                 | First-party Extension packages                                |
| `.agents/`                        | Repository dogfood, current knowledge, rules, and active work |
| `src/cli-mvp/`                    | Frozen legacy CLI source, build support, and tests            |
| `src/cli/`                        | Replacement CLI source, projects, and active tests            |
| `.agents/memory/archived/cli-v2/` | Deleted CLI-v2 raw historical input                           |

## CLI Transition

The TypeScript MVP in `src/cli-mvp/` is frozen historical source. Do not modify,
build, test, repair, or otherwise exercise its source or tests while developing
the native CLI. Use the replacement CLI built from this worktree for repository
routing, development, review, and acceptance.

CLI v2 was deleted. Its former Documents, Decisions, Directives, Patterns,
plans, and implementation records live under
`.agents/memory/archived/cli-v2/`. They are raw input, not accepted design.

The native CLI uses:

- One canonical .NET Native AOT executable
- Optional agent-first acceleration over a complete Markdown Framework
- Boring, explicit, predictable behavior for agents and occasional human use
- Native AOT and trimming compatibility as hard implementation constraints
- Thin package-manager wrappers that do not implement Framework behavior
- npm as the first wrapper
- The root `package.json` retained as an ecosystem-neutral orchestration layer

The replacement CLI implements all 28 commands but remains unreleased. Its
six-target npm package graph is implemented, with matching-host execution and
package invocation proven locally on Linux x64. The other five native-host
receipts and final delivery acceptance remain incomplete. Local linking is
available for development; package publication remains a separate explicitly
authorized release effect.

Rune is outside the current release effort.

## Current Repository Tooling

The root `package.json` contains transitional Bun and TypeScript scripts for the
frozen MVP and repository build. They are not replacement CLI implementation or
a replacement release gate. The non-shipping native CLI toolchain remains
separate from this frozen support. Package-manager source below
`src/cli/package-managers/` prepares the non-shipping distribution wrappers and
local links without repointing the frozen root package.

The replacement C# implementation follows the current [CLI
Architecture](../.agents/memory/crystallized/documents/cli/architecture.md) and
[active Plan](../.agents/memory/working/cli-development/plan.md). Source and
projects live below `src/cli/`, divided first into `root/`, `core/`, and `tests/`.
The repository root owns `OpenForge.Cli.slnx`, `global.json`, `NuGet.Config`,
`Directory.Build.props`, and `Directory.Packages.props`. All .NET output goes to
the ignored root `artifacts/` directory.

Use ordinary commands from the repository root:

```sh
dotnet restore
dotnet build
dotnet test
```

After a Debug build, invoke that worktree's development artifact directly:

```sh
./artifacts/publish/open-forge-dev/Debug/open-forge-dev --help
./artifacts/publish/open-forge-dev/Debug/open-forge-dev context
./artifacts/publish/open-forge-dev/Debug/open-forge-dev doctor
```

On Windows, use `open-forge-dev.exe`; use the configuration built in this
worktree. For ordinary development, `npm run cli:dev -- --help` runs this
worktree's CLI project after the workspace has been restored. Public-process
verification invokes the already built development or selected native artifact
directly. Use `--help` on a command from that same artifact when checking
documentation examples.

Building the CLI project directly, through the solution, or through the EndToEnd
project publishes the local managed development executable as
`artifacts/publish/open-forge-dev/<Configuration>/open-forge-dev[.exe]` and writes
`open-forge-dev.version`. EndToEnd tests discover that artifact directly, so local
terminal, Visual Studio, and VS Code test runs require no environment variables.
Use `-p:OpenForgeSkipDevelopmentPublish=true` only for a build that deliberately
does not need this artifact. A `dotnet test --no-build` run requires the artifact
selected by an earlier build. CI and explicit Native AOT evidence compile the
EndToEnd project for one supported target RID and use the corresponding
`artifacts/publish/<RID>/open-forge/OpenForge.Cli[.exe]` publication.

Use the .NET commands above for replacement CLI builds. The frozen MVP build
scripts are not a replacement CLI gate.

### Link The Native CLI Locally

The npm tooling under `src/cli/package-managers/npm/` can prepare the native
package for the current host on Linux (glibc), macOS, or Windows, on x64 or
ARM64. The accepted distribution contains all six target packages. Package
layout and packing are implemented; five matching-host receipts and final
delivery acceptance remain incomplete. See [CLI Distribution](../.agents/memory/crystallized/documents/cli/distribution.md).

The root link command publishes the current host in Release mode without
restoring, stages a local version containing the full Git commit SHA, and links
the platform package through the main package into this repository. This is an
explicit maintainer workflow that creates global links, not a verification
prerequisite.

Restore the locked .NET workspace before the first link or after its
dependencies change. Then link and invoke the current native CLI:

```sh
dotnet restore OpenForge.Cli.slnx --locked-mode --configfile NuGet.Config --nologo
npm run cli:link
open-forge --version
```

The command uses ordinary npm package links and therefore creates npm's normal
global package links as well as repository-local links. It does not save a
dependency, update the lockfile, run package scripts, contact the registry, or
publish a package. Generated JavaScript, native publications, and staged package
files remain below the ignored `artifacts/` directory.

Ordinary projects use the machine-global `open-forge` command. Development,
review, and acceptance in an Open Forge worktree must instead invoke an
artifact built and published from that same worktree, such as
`artifacts/publish/open-forge-dev/<Configuration>/open-forge-dev[.exe]` or the
selected `artifacts/publish/<RID>/open-forge/OpenForge.Cli[.exe]`. Never use the
machine-global CLI as worktree evidence. A package-manager-specific user-local
PATH bridge may be needed when shell-facing shims differ from npm's active bin,
but that bridge does not establish artifact identity.

Remove the known repository and global links when they are no longer needed:

```sh
npm run cli:unlink
```

That command removes the known npm links. Remove an explicitly owned user-local
PATH bridge only after resolving and revalidating its exact target.

The public command name is `open-forge`. The frozen MVP remains available only
through `open-forge-old` and `npm run cli:old`.

Do not treat `dist/` or `.temp/` as authored authority. Do not edit generated
output manually.

For Framework-only changes, use proportionate checks with the artifact built
from the same worktree. Preview generated navigation with `index --dry-run` when
useful, then apply the authorized changes. Review the complete Git diff before
closeout:

```sh
./artifacts/publish/open-forge-dev/Debug/open-forge-dev index
./artifacts/publish/open-forge-dev/Debug/open-forge-dev doctor
git diff --check
```

Do not run frozen MVP tests or builds as part of new-CLI development.

## New CLI Evidence

No prototype is working merely because source or test files exist. A native CLI
claim requires reproducible restore, compilation, focused tests, Native AOT
publication, actual binary execution, and wrapper evidence appropriate to the
accepted slice.

Design tests from observable risk and independent evidence needs. Do not inherit
the deleted CLI-v2 test volume or tier structure automatically.

## Documentation

When accepted direction changes, update every source that answers a distinct
affected question. Preserve useful old reasoning in the appropriate historical
route instead of leaving competing current descriptions.

Follow the repository [Writing Directive](../.agents/directives/public-facing-writing.md),
[Writing Standard](../.agents/memory/crystallized/documents/maintenance/writing.md),
and [Dictionary](../.agents/memory/crystallized/documents/maintenance/helpers/dictionary.md).
