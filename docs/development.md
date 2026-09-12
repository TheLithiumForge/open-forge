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
| `scripts/`                        | Repository build, delivery, package, and agent tooling        |
| `src/cli/`                        | Replacement CLI source, projects, and active tests            |
| `.agents/memory/archived/cli-v2/` | Deleted CLI-v2 raw historical input                           |

## CLI Transition

The retired TypeScript MVP and its build scripts remain available in Git history
at `c4428a90`. The current CLI is C# under `src/cli/`; repository coordination
scripts live under `scripts/`. Use the CLI built from this worktree for repository
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
package invocation proven locally on Linux x64. The other five native hosts
have static workflow review; their runtime receipts remain unproven. Local linking is
available for development; package publication remains a separate explicitly
authorized release effect.

Rune is outside the current release effort.

## Current Repository Tooling

The root `package.json` is the shared entry point for local development and CI.
Small TypeScript scripts run the .NET build and test tools, synchronize versions,
and prepare packages. Repository tooling uses Node and npm, including its tests.

The replacement C# implementation follows the current [CLI
Architecture](../.agents/memory/crystallized/documents/cli/architecture.md) and
[active Plan](../.agents/memory/working/cli-development/plan.md). Source and
projects live below `src/cli/`, divided first into `root/`, `core/`, and `tests/`.
The repository root owns `OpenForge.Cli.slnx`, `global.json`, `NuGet.Config`,
`Directory.Build.props`, and `Directory.Packages.props`. All .NET output goes to
the ignored root `artifacts/` directory.

Install Node 24 and the stable .NET 10 SDK selected by the repository. Native
publishing also needs the platform's native toolchain: Clang and development
libraries on Linux, Xcode command-line tools on macOS, or Visual Studio Build
Tools with the C++ workload on Windows. See the [.NET Native AOT prerequisites](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/).

Set up dependencies once, then use the same commands from any supported host:

```sh
npm ci
npm run restore
npm run build
npm test
npm run cli:dev -- --help
```

`npm run build` and `npm test` use Release configuration. The test command
builds the managed solution and runs its Unit, Integration and public-process
tests. IDEs can continue using the root .NET solution directly.

Use `npm run check:delivery` for delivery TypeScript, lint and formatting,
`npm run check:dotnet` for C# formatting and warning-level diagnostics, and
`npm run test:delivery` for the delivery helpers. `npm run test:agent-tooling`
checks the separate repository agent tools.

After a build, invoke that worktree's development artifact directly:

```sh
./artifacts/publish/open-forge-dev/Release/open-forge-dev --help
./artifacts/publish/open-forge-dev/Release/open-forge-dev context
./artifacts/publish/open-forge-dev/Release/open-forge-dev doctor
```

On Windows, use `open-forge-dev.exe`. Use `--help` on the artifact built in this
worktree when checking documentation examples.

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

When all dependencies are already cached, `npm ci --offline` and
`npm run restore -- --offline` prepare the workspace without contacting package
feeds. Offline .NET restore does not perform a fresh vulnerability audit; a
normal connected restore is required for that evidence.

### Build A Copyable Native Package

Run the complete native journey on the host you want to support:

```sh
npm run build:native
npm run test:built
npm run pack
```

The native build defaults to the current OS and architecture. `--rid` can state
that target explicitly, such as `npm run build:native -- --rid linux-x64`.
Use a matching native host. Native AOT does not support building Windows
executables on Linux or other cross-OS compilation. The CI matrix supplies all
six matching hosts. See [.NET cross-compilation](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/cross-compile).

`build:native` produces the native CLI and the managed/native test executables.
`test:built` runs the existing artifacts without compiling them. `pack` requires
their successful qualification, creates the portable archive and current-host
npm packages, and checks the installed npm launcher against the native binary.
The commands print their output locations under `artifacts/delivery/<RID>/`.

Copy the `open-forge-<version>-<RID>.tar.gz` archive and its checksums to a
supported machine with the same OS and architecture. Extract it, then run
`./open-forge --help` or `./open-forge.exe --help` on Windows. The native
executable needs neither Node nor an installed .NET runtime. The npm launcher
requires Node. Native operating-system requirements still apply; matching the
architecture alone does not make an older unsupported OS compatible.

Builds and tests also use the normal `artifacts/bin`, `artifacts/obj`, and
`artifacts/publish` directories. Run these commands sequentially in one worktree.
Generated archives and reports are build artifacts, not authored source.

### Change The Product Version

The root `package.json` owns the product version. Use one command to calculate
the next version and synchronize .NET and every implemented package shim:

```sh
npm run version:bump -- patch
npm run version:bump -- minor
npm run version:bump -- major
npm run version:bump -- prerelease --preid beta
npm run version:bump -- 0.1.0-beta.1
```

These are alternative examples. The command uses npm's normal version handling,
then sets that exact value in the other version-bearing files. It creates no
commit, Git tag, or release. Review and commit the resulting diff normally.

Builds use the committed product version by default. For a development artifact
identified by the current commit, use `npm run build:native -- --sha`.
For example, `0.1.0-beta.1` becomes `0.1.0-beta.1.sha-<commit>` in the artifacts;
the tracked version stays unchanged. Packaging reads the built version. Changing
the version afterward requires a new build and test run.

### CI And Releases

`build.yml` checks the tooling once and builds/tests Linux, macOS, and Windows
on x64 and ARM64. The test jobs download the build outputs and call the same
`npm run test:built` command used locally. The reusable platform packaging
workflows download those tested artifacts and call `npm run pack`.

The `release.yml` workflow owns public publication. It can run manually with a
source ref or commit, a destination (`github`, `npm`, or `all`), and optionally
an existing successful build run for that exact commit. Without a supplied
build run, it runs the reusable build workflow once. The release jobs do not
recompile or change the product version.

Pushing a version tag such as `v0.1.0-beta.1` selects automatic release. The tag
must match the source version. Automatic runs use the `RELEASE_TARGET`
repository variable, defaulting to `all`. Merely bumping the local version does
not release anything.

All six platform packages must be prepared before publication begins. npm
publishes the platform packages before the main package that references them.
GitHub receives one release containing the portable archives and checksums.
Prereleases use a prerelease channel; stable releases use `latest`. Selecting
`all` requests both destinations, but there is no transaction across GitHub and
npm. A failed publication must be inspected before retrying its remaining work.

The workflow must be available on the repository's default branch for manual
dispatch. Configure the required publication credentials only when enabling
releases. Local build/test/package commands do not publish, create GitHub
releases, or require publication credentials.

### Link The Native CLI Locally

The npm tooling under `scripts/package-managers/npm/` can prepare the native
package for the current host on Linux (glibc), macOS, or Windows, on x64 or
ARM64. The accepted distribution contains all six target packages. Package
layout and packing are implemented; five matching-host runtime receipts remain
unproven. See [CLI Distribution](../.agents/memory/crystallized/documents/cli/distribution.md).

The root link command publishes the current host in Release mode without
restoring, stages the product version with the full Git commit SHA, and links
the platform package through the main package globally. This is an
explicit maintainer workflow that creates global links, not a verification
prerequisite.

Restore the locked .NET workspace before the first link or after its
dependencies change. Then link and invoke the current native CLI:

```sh
npm run restore
npm run cli:link
open-forge --version
```

The command uses ordinary npm package links and therefore creates npm's normal
global package links and a link between the staged packages. It does not save a
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

Remove the known global npm links when they are no longer needed:

```sh
npm run cli:unlink
```

That command removes the known npm links. Remove an explicitly owned user-local
PATH bridge only after resolving and revalidating its exact target.

The public command name is `open-forge`. Retired MVP entry points are available
in Git history and are no longer part of the repository's active tooling.

Do not treat `dist/` or `.temp/` as authored authority. Do not edit generated
output manually.

For Framework-only changes, use proportionate checks with the artifact built
from the same worktree. Preview generated navigation with `index --dry-run` when
useful, then apply the authorized changes. Review the complete Git diff before
closeout:

```sh
./artifacts/publish/open-forge-dev/Release/open-forge-dev index
./artifacts/publish/open-forge-dev/Release/open-forge-dev doctor
git diff --check
```

Historical MVP tests and builds do not qualify the current CLI.

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
