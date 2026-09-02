# Developing Open Forge

This guide describes the current repository transition. Start with the
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

The TypeScript MVP is frozen. Do not modify, build, test, repair, or otherwise
exercise its source or tests while developing the new CLI. Use
`open-forge-old` only when repository routing assistance is needed, such as:

```sh
open-forge-old load --bodies
open-forge-old index
open-forge-old doctor
```

CLI v2 was deleted. Its former Documents, Decisions, Directives, Patterns,
plans, and implementation records live under
`.agents/memory/archived/cli-v2/`. They are raw input, not accepted design.

The new CLI direction is:

- One canonical .NET Native AOT executable
- Optional agent-first acceleration over a complete Markdown Framework
- Boring, explicit, predictable behavior for agents and occasional human use
- Native AOT and trimming compatibility as hard implementation constraints
- Thin package-manager wrappers that do not implement Framework behavior
- npm as the first wrapper
- The root `package.json` retained as an ecosystem-neutral orchestration layer

The command surface, architecture, libraries, tests, safety model, native
artifacts, and public wrapper remain subject to explicit design and maintainer
acceptance. The private development link described below is repository-local
tooling. It does not settle the future public package design.

Rune is outside the current release effort.

## Current Repository Tooling

The root `package.json` contains transitional Bun and TypeScript scripts for the
frozen MVP and repository build. They are not replacement CLI implementation or
a replacement release gate. The non-shipping native CLI toolchain remains
separate from this frozen support. Public distribution wrappers do not exist
yet. The private development link below is not a distribution package.

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

The root `package.json` and frozen MVP tooling do not provide a replacement CLI
build.

### Link The Development CLI

The repository includes an optional private npm package that links the managed
development publication as `open-forge-dev`. Build the selected configuration
before linking or invoking it. The launcher does not build the CLI or search for
another publication.

Debug is the default configuration:

```sh
dotnet build
npm run cli:link
npm run cli:dev -- --version
```

Set `OPEN_FORGE_DEV_CONFIGURATION` to the exact value `Release` to use a Release
publication. Rebuild that configuration whenever its source changes:

```sh
dotnet build --configuration Release
OPEN_FORGE_DEV_CONFIGURATION=Release npm run cli:dev -- --version
```

In PowerShell, set the same value for the current session before invoking the
command:

```powershell
dotnet build --configuration Release
$env:OPEN_FORGE_DEV_CONFIGURATION = "Release"
npm run cli:dev -- --version
```

Only `Debug` and `Release` are valid configuration values. `cli:link` creates a
repository-local npm link and the package-manager global link used to maintain
it. Invoke the command through `cli:dev`, which uses npm's repository-local bin
path on Windows and Linux. No bare `open-forge-dev` command or machine `PATH`
configuration is promised.

The link remains active until you remove it:

```sh
npm run cli:unlink
```

`cli:unlink` removes the repository-local link first and then removes the npm
global package link. Neither link action saves a dependency or changes a
lockfile. The linked `open-forge-dev` launcher resolves the managed publication
in this repository and preserves the caller's current directory, arguments,
streams, and environment. It is separate from the frozen `open-forge-old`
command and from the future public `open-forge` npm package.

Do not treat `dist/` or `.temp/` as authored authority. Do not edit generated
output manually.

For Framework-only changes, use proportionate checks and the available legacy
routing commands. Review the complete Git diff before closeout:

```sh
open-forge-old index
open-forge-old doctor
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

Follow the repository [Writing Directive](../.agents/directives/writing.md),
[Writing Standard](../.agents/memory/crystallized/documents/maintenance/writing.md),
and [Dictionary](../.agents/memory/crystallized/documents/maintenance/helpers/dictionary.md).
