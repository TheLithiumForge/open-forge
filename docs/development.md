# Developing Open Forge

Use this guide to build the CLI locally, contribute to the Framework, and check a change before review. For everyday use, start with the [README](../README.md) or [CLI guide](cli.md).

## Enter the workspace

Read `AGENTS.md` and `.agents/loader.md`, then select the scopes relevant to your change. The [Sources Of Truth map](../.agents/maps/sources-of-truth.md) helps locate the file that defines each affected question.

| Location                | What belongs there                                               |
| ----------------------- | ---------------------------------------------------------------- |
| `src/open-forge/`       | Installable Framework files                                      |
| `src/extensions/`       | First-party Extension packages                                   |
| `src/cli/`              | Native CLI implementation and tests                              |
| `scripts/`              | Repository build, delivery, package, and agent tooling           |
| `.agents/`              | This repository's own rules, current knowledge, and work context |
| `docs/` and `README.md` | Public introductions and practical guides                        |
| `artifacts/`            | Generated build, publication, and verification output            |

Keep changes in their defining sources. Generated output and a machine's installed CLI do not establish what a worktree contains.

## Build and test

The [delivery CLI guide](../scripts/delivery/README.md) is the quick reference
for setup, stage selection, target-specific packaging and publication. The CLI
is the shared entry point for local development and CI.
It runs directly as TypeScript with the required Node version, so a fresh
checkout can run setup before dependencies or a compiled build tool exist.
Run `npx forge` from the repository root. The private root package registers the
local command, so no global link or bootstrap compilation is needed. Arguments
after `forge` go directly to the tool without an extra `--` separator. Root npm
scripts remain short aliases for the same CLI:

```sh
npx forge --help
npx forge setup
npx forge build
npx forge version patch
npx forge dist --help
npx forge dist --no-restore --plan
```

The command catalog is scripts/delivery/commands.ts. It defines each command's
implementation, description and supported options. Focused task modules own
execution, and dist-plan.ts declares the pipeline and routes flags to their stages.
Use either the unified CLI or an alias such as npm run build.
Small TypeScript scripts coordinate native tests and packaging. Standard npm
and .NET commands own version increments, restore, compilation and uploads. Repository tooling uses Node and npm, including its tests.

Package scripts invoke workspace tools directly: `tsc`, `eslint`, and `prettier`.
`tsc` uses TypeScript 7. The `@typescript/native` npm alias supplies that compiler.
The `typescript` alias supplies Microsoft's TypeScript 6 API package for ESLint.
This follows the supported [side-by-side installation](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0/).
`npm run build:launcher` compiles only the thin npm launcher. Delivery and local
linking use that same command when they need emitted JavaScript.

Delivery scripts live together under `scripts/delivery/`. Each task has a direct
entry point, such as `build.ts`, `restore.ts` or `pack.ts`. Shared capabilities
stay beside their consumers, with package preparation under `npm/` and
release coordination under `release/`. Repository agent tools
remain separate under `scripts/agent-tooling/`.

One root `tsconfig.json` checks all repository TypeScript, including tests. The
launcher's emitting configuration includes only its runtime sources. Tests
remain independently runnable through their package commands. Compiler
configuration does not determine which test tier executes.

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
npm run setup
npm run build
npm test
npm run cli:dev -- --help
```

`setup` installs locked npm dependencies and restores .NET dependencies.
`npm run build` and `npm test` let `dotnet build` restore incrementally and use
Release configuration. Native builds restore once during that managed build.
Subsequent native publications reuse the restored dependencies.
Use `-- --no-restore` after an explicit restore, or `-- --offline` to restore
only from cached dependencies. These options are mutually exclusive. The test command
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

When all dependencies are already cached, `npm run setup -- --offline` or
`npm run restore -- --offline` prepares the workspace without contacting package
feeds. Offline .NET restore does not perform a fresh vulnerability audit. A
normal connected restore is required for that evidence.

### Build a copyable native package

Run the same preparation, checks and native journey used by CI on the host you
want to support:

```sh
npm run setup
npm run verify
npm run dist -- --no-restore
```

`verify` runs formatting, lint, type checking and delivery/package-layout tests.
`dist` prints its selected host, stages and effective commands, then builds the
native CLI and its test executables, runs the managed/native suites, and packs
the portable archive plus main and host npm packages. The installed-package journey
checks the exact native payload through the launcher. No upload or global
installation occurs. Use `-- --sha` for a commit-qualified development version,
`-- --offline` for cached restore, or `-- --no-restore` after preparation.

CI and focused local work can run the individual steps:

```sh
npm run build:native -- --no-restore
npm run test:built
npm run pack
```

The native build defaults to the current OS and architecture. `--rid` can state
that target explicitly, such as `npm run build:native -- --rid linux-x64`.
Use a matching native host. Native AOT does not support building Windows
executables on Linux or other cross-OS compilation. The CI matrix supplies all
six matching hosts. See [.NET cross-compilation](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/cross-compile).

`build:native` produces the native CLI and the managed/native test executables.
`test:built` runs the existing artifacts without compiling them. Its six suites
are six execution modes on one host: managed unit and integration tests, managed
public tests against the managed CLI, native integration and public tests, and
managed public tests against the native CLI. Integration tests therefore run
in two modes and public tests in three. This does not run other OS targets.

By default, `pack` requires
their successful qualification, creates the portable archive and current-host
npm packages, and checks the installed npm launcher against the native binary.
The commands print their output locations under `artifacts/delivery/<RID>/`.
The current reports are in `reports/`, and the packages are in `packages/`.
`package-path.txt` is written only after packaging succeeds.

To package locally when tests are failing or have not been run:

```sh
npm run pack -- --skip-tests                 # existing matching build
npm run dist -- --skip-tests                 # build, then package
npm run dist -- --skip-tests --plan          # inspect without executing
```

`--skip-tests` bypasses .NET qualification and the npm installation/invocation
test. It does not bypass source, artifact-hash or license checks. The native
build still compiles the test executables. Packages record `tested: false`.
Native publication and complete release collection reject them. To qualify
later, run `npm run test:built` and `npm run pack` without the flag.

Stages print START/PASS/FAIL with their names. A failed .NET suite reports its
name, log location and the end of its failure output. Later stages stop.

Copy the `open-forge-<version>-<RID>.tar.gz` archive and its checksums to a
supported machine with the same OS and architecture. Extract it, then run
`./open-forge --help` or `./open-forge.exe --help` on Windows. The native
executable needs neither Node nor an installed .NET runtime. The npm launcher
requires Node. Native operating-system requirements still apply. Matching the
architecture alone does not make an older unsupported OS compatible.

Builds and tests also use the normal `artifacts/bin`, `artifacts/obj`, and
`artifacts/publish` directories. Run these commands sequentially in one worktree.
Generated archives and reports are build artifacts, not authored source.

### Build and publish the wrapper separately

The wrapper needs Node/npm dependencies but no .NET SDK or native artifacts:

```sh
npm ci --ignore-scripts
npm run dist:wrapper
npm run publish:wrapper -- --tag preview --dry-run
```

`dist:wrapper` compiles the launcher and packs it with the license and exact
versioned optional dependencies for the selected platforms (all six by default). Its output is
`artifacts/delivery/wrapper/packages/`. `--sha` uses the same optional
commit-qualified version convention as native builds.

After `dist` succeeds, preview publication of only the host's native package:

```sh
npm run publish:native -- --tag preview --dry-run
```

Both publishers validate the existing tarball against its source, version,
hash and license. The dry run is entirely local and does not invoke npm publish
or inspect credentials. Removing `--dry-run` explicitly uploads that one public
npm package using the configured registry and account. An actual upload requires
committed matching source. A dirty trial can still preview its selection.
Before any upload, the shared publisher queries npm for each selected exact
version. Existing versions print a warning and are skipped. Missing versions
are published. Authentication, network and unexpected lookup errors stop the
run before uploads. Rerun the same command after an interrupted publication.
The release checks every selected native package and the wrapper first and publishes remaining
native packages before the wrapper. Existing versions keep their npm tags.
Skipping checks availability, not whether remote bytes match a local rebuild.
Dry runs remain offline and cannot report which versions already exist.

Both commands require a tag, and prerelease versions cannot use `latest`.
Changing source or version requires rebuilding the affected distribution.

Publish each native package at the synchronized version, then publish the main
wrapper once. No publisher needs all native tarballs locally. The wrapper's
optional dependencies do need to exist in the registry for their platforms to
work. A release is complete for its recorded selection when every listed native
package and its wrapper are published.
The platform package alone contains no npm command mapping. The main package
provides the `open-forge` command.

### Select targets for one version

Choose the wrapper’s exact dependencies when packing it:

```sh
npx forge dist:wrapper --targets linux-x64,osx-x64,win-x64
npx forge publish:wrapper --tag preview --dry-run
```

This wrapper version lists exactly those three x64 packages. It has no ARM
dependencies and requires no native builds locally. Publish its three native
packages separately before publishing the wrapper. A target selection belongs
to one version: adding ARM later requires a new wrapper version. If that
version already exists with different dependencies, publication fails before
any upload instead of skipping an incompatible wrapper.

For collection and publication of a selected release, pass the same list to
`pack` (or `dist`) on each selected host, then to collection:

```sh
npx forge pack --targets linux-x64,osx-x64,win-x64
npx forge release:collect artifacts/release-input artifacts/release --targets linux-x64,osx-x64,win-x64
npx forge publish:release --tag preview --dry-run
```

The input contains each selected host’s `package-<RID>` directory. Collection
requires their tested packages and matching wrapper dependencies. It records
the selection in `release.json`. Publication uses that exact graph and uploads
the wrapper last. Omitted target artifacts are neither required nor uploaded.
Changing selection requires repacking, but not rebuilding native executables.
Without `--targets`, packaging and collection select all six. The existing
Actions workflows continue to build and release all six by default.

### Clean generated outputs

`npm run clean` removes owned .NET outputs and current/legacy delivery output
directories. It preserves source, `node_modules`, dependency caches, the offline
feed, staged local npm links and unrelated artifact directories. Output paths
with symlinked ancestors are rejected before deletion.

Ordinary builds preserve incremental compiler outputs. Native builds replace
their selected target's publish and delivery directories, while tests and packs
replace their current reports and packages. Old runs are no longer archived
automatically. Failure diagnostics remain until the next run or explicit clean.
Run build, test, pack, link and clean commands sequentially in one worktree.
After clean, build/test/dist restore by default. Do not pass `--no-restore` until
restoration has completed again. Directory.Build.props centralizes outputs but
does not own cleanup of TypeScript-created packages and reports.

### Change the product version

The root `package.json` owns the product version. Use one command to calculate
the next version and update the .NET version:

```sh
npm run version:bump -- patch
npm run version:bump -- minor
npm run version:bump -- major
npm run version:bump -- prerelease --preid beta
npm run version:bump -- 0.1.0-beta.1
```

These are alternative examples. The command uses npm's normal version handling,
which updates package.json and its lockfile. Its version lifecycle hook projects
that value to one property in Directory.Build.props for direct .NET/IDE builds.
The informational version derives from it. Staging generates all seven npm
manifests and their exact dependency versions from the selected version and
platform definitions, so no per-platform manifests need editing. It creates no
commit, Git tag, or release. Review and commit the resulting diff normally.
If you edit the root version manually, run `npm run version` to refresh the .NET
property before using direct dotnet or IDE builds.

Builds use the committed product version by default. For a development artifact
identified by the current commit, use `npm run build:native -- --sha`.
For example, `0.1.0-beta.1` becomes `0.1.0-beta.1.sha-<commit>` in the artifacts.
The tracked version stays unchanged. Packaging reads the built version. Changing
the version afterward requires a new build and test run.

### CI and releases

`build.yml` runs `setup` and `verify` once for shared checks. Its single native
matrix covers Linux, macOS and Windows on x64 and ARM64. Each runner uses
`setup` followed by explicit `build:native -- --no-restore`, `test:built` and
`pack` steps on the same machine. These are the stages shown by `dist --plan`.
CI passes one shared RID and never supplies `--skip-tests`. GitHub shows the
failed stage directly and retains separate build/test/pack logs.
The explicit matrix RID is an assertion that the host matches the target.
The build uploads finished packages and diagnostics from stable output paths.
There are no intermediate build/test transfers or platform packaging workflows.

The `release.yml` workflow coordinates complete public releases. It can run manually with a
source ref or commit, a destination (`github`, `npm`, or `all`), and optionally
an existing successful build run for that exact commit. Without a supplied
build run, it runs the reusable build workflow once. The release job downloads
the finished `package-<RID>` artifacts from the selected run and calls
`npm run release:collect -- artifacts/release-input artifacts/release`.
The default collection checks all six targets and replaces its generated release output.
Older build runs containing only intermediate test artifacts cannot be reused.
Release does not compile, repack or change the product version.

Pushing a version tag such as `v0.1.0-beta.1` selects automatic release. The tag
must match the source version. Automatic runs use the `RELEASE_TARGET`
repository variable, defaulting to `all`. Merely bumping the local version does
not release anything.

For the default complete release, all six platform packages must be prepared before publication begins. npm
publishes the platform packages before the main package that references them.
GitHub receives one release containing the portable archives and checksums.
Prereleases use a prerelease channel. Stable releases use `latest`. Selecting
`all` requests both destinations, but there is no transaction across GitHub and
npm. A failed publication must be inspected before retrying its remaining work.

Put the workflow on the default branch to expose GitHub's **Run workflow**
button. The first beta used REST dispatch of the registered workflow at its
version tag while that button was unavailable. Its
[release record](../.agents/memory/archived/cli-development/tasks/task51-beta-release.md)
retains the setup and successful run. When retrying publication, keep the
qualified source ref and build run and select only the unfinished destination.

Configure `NPM_TOKEN` as a repository Actions secret. Its npm granular token
needs package publishing access to `@thelithiumforge` and Bypass 2FA enabled.
Organization-management access alone does not grant package publishing access.
See [npm's token instructions](https://docs.npmjs.com/creating-and-viewing-access-tokens/).
Local build/test/package commands do not publish, create GitHub
releases, or require publication credentials. The explicit `publish:native`
and `publish:wrapper` commands upload individual packages independently.
`publish:release -- --tag <channel>` consumes the collected release and uses the
same package validation and publication code, with all native packages before
the wrapper. `--dry-run` previews the complete npm publication locally. It needs
no .NET SDK, native test output or matching target host. All packages are
validated before any upload begins.

### Link the native CLI locally

The npm tooling under `scripts/delivery/npm/` can prepare the native
package for the current host on Linux (glibc), macOS, or Windows, on x64 or
ARM64. The accepted distribution contains all six target packages. Package
layout, packing, native execution and installed-launcher journeys are qualified
on all six matching hosts for `0.9.0-beta.1`.
See [CLI Distribution](../.agents/memory/crystallized/documents/cli/distribution.md).

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

## Framework and Extension changes

Treat the shipped Markdown as the product a workspace will read. Keep it understandable without repository-only explanations or the CLI. When shared meaning changes, update the defining source and its current maintenance explanation, then align the repository's own copy while preserving intentional local specialization.

Check the affected questions, rules, frontmatter, links, generated `Entries`, and installed relationships. A wording edit must preserve conditions, authority, scope, and requirement strength. Extension checks should inspect the assembled result with its dependencies, as described in the [Extension guide](extensions.md#check-the-assembled-result).

Use proportionate verification. With a current artifact available, `index --dry-run` can expose navigation drift and `doctor` can check structure without applying changes. These commands support review. They do not decide whether a sentence preserves its meaning.

Review the final diff and check whitespace:

```sh
git diff --check
git diff
```

## Measure context size

Context measurements count the Markdown in `src/open-forge/`, including hidden files, frontmatter, and generated `Entries`. Use [tiktoken](https://github.com/openai/tiktoken) with two named reference encodings:

The following measurements predate the optional-content extraction and are historical, not counts for the current payload. The current base has 14 Markdown files, with 11 selected at startup through `AGENTS.md`. Fresh tiktoken totals have not been measured.

The README quotes the CLI's own estimate, the same one `open-forge status` reports: each file's character count divided by four and rounded up, then summed. For the current base, that gives about 5.7k tokens for the 11 startup files and about 6.4k tokens for all 14. Installed Extensions and your own content add to both numbers.

| Historical source set   | Files | `o200k_base` tokens | `cl100k_base` tokens |
| ----------------------- | ----: | ------------------: | -------------------: |
| Default startup context |    19 |               6,823 |                6,863 |
| Complete base Framework |    23 |               8,267 |                8,309 |

For startup, start with the canonical `AGENTS.md` handoff and loader, then follow exposed `LoadNow` and `KeepInMind` entries through loaded parents in listed order. Include adjacent overwrites where present. The current default set leaves the archived and Templates entrypoints unloaded. Adaptive Collaboration is now supplied on demand by the optional Collaboration Extension. The alternative `CLAUDE.md` bridge is included in the complete-file count.

Count each file's raw UTF-8 text independently with `len(encoding.encode(text))`, then sum the results. The measurements use tiktoken `0.14.0`. They exclude tool response wrappers, file separators, system and conversation context, project-specific files, and Extensions. Actual model and harness costs can differ.

Recalculate after changing the shipped files or their loading policy. Report the tokenizer, included source set, and final source revision with the verification record. Keep Core, Memory, and the complete Framework distinct when describing the result.

## Documentation voice

Use [Project Voice](../.agents/memory/crystallized/documents/maintenance/project-voice.md) for READMEs and introductions: direct, concrete, and written like an engineer sharing ideas they're excited about. Give rules and reference text the precise, conversational voice described in the [Writing Standard](../.agents/memory/crystallized/documents/maintenance/writing.md). Both share the same accuracy and terminology requirements. Choose the voice for the passage's purpose, so an opinionated introduction can lead into exact setup instructions.

The README should quickly explain what Open Forge is, why it helps, how to start, and where to learn more. Public guides explain use in more detail. Current repository documents preserve the complete accepted subjects, and maintenance files explain how their sources stay aligned. Link between them instead of maintaining competing detailed explanations.

Keep category questions synchronized between the README and their defining files. A description helps a reader decide whether to open a file. An optional responsibility helps an editor decide what belongs in it.

Follow the [Writing Directive](../.agents/directives/public-facing-writing.md) and [Dictionary](../.agents/memory/crystallized/documents/maintenance/helpers/dictionary.md). The Framework itself must remain understandable without these repository documents.
