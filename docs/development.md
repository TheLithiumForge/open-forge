# Developing Open Forge

Use this guide to build the CLI locally, contribute to the Framework, and check a change before review. For everyday use, start with the [README](../README.md) or [CLI guide](cli.md).

## Enter The Workspace

Read `AGENTS.md` and `.agents/loader.md`, then select the scopes relevant to your change. The [Sources Of Truth map](../.agents/maps/sources-of-truth.md) helps locate the file that defines each affected question.

| Location                | What belongs there                                               |
| ----------------------- | ---------------------------------------------------------------- |
| `src/open-forge/`       | Installable Framework files                                      |
| `src/extensions/`       | First-party Extension packages                                   |
| `src/cli/`              | Native CLI implementation and tests                             |
| `scripts/`              | Repository build, delivery, package, and agent tooling            |
| `.agents/`              | This repository's own rules, current knowledge, and work context |
| `docs/` and `README.md` | Public introductions and practical guides                        |
| `artifacts/`            | Generated build, publication, and verification output            |

Keep changes in their defining sources. Generated output and a machine's installed CLI do not establish what a worktree contains.

## Build And Test


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

The public command name is `open-forge`.


## Framework And Extension Changes

Treat the shipped Markdown as the product a workspace will read. Keep it understandable without repository-only explanations or the CLI. When shared meaning changes, update the defining source and its current maintenance explanation, then align the repository's own copy while preserving intentional local specialization.

Check the affected questions, rules, frontmatter, links, generated `Entries`, and installed relationships. A wording edit must preserve conditions, authority, scope, and requirement strength. Extension checks should inspect the assembled result with its dependencies, as described in the [Extension guide](extensions.md#check-the-assembled-result).

Use proportionate verification. With a current artifact available, `index --dry-run` can expose navigation drift and `doctor` can check structure without applying changes. These commands support review; they do not decide whether a sentence preserves its meaning.

Review the final diff and check whitespace:

```sh
git diff --check
git diff
```

## Measure Context Size

The README's context figures measure the Markdown in `src/open-forge/`, including hidden files, frontmatter, and generated `Entries`. They use [tiktoken](https://github.com/openai/tiktoken) with two named reference encodings:

| Source set              | Files | `o200k_base` tokens | `cl100k_base` tokens |
| ----------------------- | ----: | ------------------: | -------------------: |
| Default startup context |    19 |               6,823 |                6,863 |
| Complete base Framework |    23 |               8,267 |                8,309 |

For startup, start with the canonical `AGENTS.md` handoff and loader, then follow exposed `LoadNow` and `KeepInMind` entries through loaded parents in listed order. Include adjacent overwrites where present. The current default set leaves the archived entrypoint, Templates entrypoint, and on-demand Adaptive Collaboration guidance unloaded. The alternative `CLAUDE.md` bridge adds 24 `o200k_base` tokens when used and is included in the complete-file count.

Count each file's raw UTF-8 text independently with `len(encoding.encode(text))`, then sum the results. The measurements use tiktoken `0.14.0`. They exclude tool response wrappers, file separators, system and conversation context, project-specific files, and Extensions. Actual model and harness costs can differ.

Recalculate after changing the shipped files or their loading policy. Report the tokenizer, included source set, and final source revision with the verification record. Keep Core, Memory, and the complete Framework distinct when describing the result.

## Documentation Voice

Use [Project Voice](../.agents/memory/crystallized/documents/maintenance/project-voice.md) for READMEs and introductions: natural, welcoming, and quietly proud of what the project offers. Give rules and reference text the precise, conversational voice described in the [Writing Standard](../.agents/memory/crystallized/documents/maintenance/writing.md). Both share the same accuracy and terminology requirements. Choose the voice for the passage's purpose, so an inviting introduction can lead into exact setup instructions.

The README should quickly explain what Open Forge is, why it helps, how to start, and where to learn more. Public guides explain use in more detail. Current repository documents preserve the complete accepted subjects, and maintenance files explain how their sources stay aligned. Link between them instead of maintaining competing detailed explanations.

Keep category questions synchronized between the README and their defining files. A description helps a reader decide whether to open a file. An optional responsibility helps an editor decide what belongs in it.

Follow the [Writing Directive](../.agents/directives/public-facing-writing.md) and [Dictionary](../.agents/memory/crystallized/documents/maintenance/helpers/dictionary.md). The Framework itself must remain understandable without these repository documents.
