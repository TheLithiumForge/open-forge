---
open-forge:
  description: CLI setup and development documentation facts inspected at the recorded source revision
  tags: [Memory, Archived, Contextual, Historical, Framework, Review]
---

# Current CLI development command facts

This is an earlier review snapshot retained for the active Task 28 Git review. Its status and paths describe the recorded stage. Use [Task 28](../../cli-development/tasks/source-framework-review.md) for current decisions, completion, and remaining work.

Read-only packet against `develop` at `2b54fdc598a48a12e44772a5cb323cf45e9e5a73`.

## Exact ordinary workflow

From the repository root, the intended ordinary managed workflow is:

```sh
dotnet restore
dotnet build
dotnet test
```

An explicit locked restore is:

```sh
dotnet restore OpenForge.Cli.slnx --locked-mode --configfile NuGet.Config --nologo
```

`--solution` is not required for the normal root workflow. The root owns
`OpenForge.Cli.slnx`, and the repository decision explicitly treats ordinary
root `dotnet restore`, `dotnet build`, and `dotnet test` as the supported path.
`global.json` selects the Microsoft Testing Platform runner, while the
EndToEnd project sets `UseMicrosoftTestingPlatformRunner=true`.

For an explicit managed Release build:

```sh
dotnet build OpenForge.Cli.slnx --configuration Release --no-restore --nologo
```

Use `-p:OpenForgeSkipDevelopmentPublish=true` only when that build deliberately
does not need the local development executable. A `dotnet test --no-build` run
does not publish it and therefore requires the artifact from an earlier build.

## Development artifact

An ordinary non-RID build publishes:

```text
artifacts/publish/open-forge-dev/<Configuration>/open-forge-dev[.exe]
```

The direct checks documented by the repository are:

```sh
./artifacts/publish/open-forge-dev/Debug/open-forge-dev --help
./artifacts/publish/open-forge-dev/Debug/open-forge-dev context
./artifacts/publish/open-forge-dev/Debug/open-forge-dev doctor
```

The package script is a convenience invocation of the project:

```sh
npm run cli:dev -- --help
```

It runs `dotnet run --project src/cli/root/OpenForge.Cli/OpenForge.Cli.csproj
--no-restore -- ...`; it is not a globally installed command and does not
establish release/package evidence.

## Native AOT publication

The root project has `PublishAot=true`, `IsAotCompatible=true`, and the six
declared RIDs in `Directory.Build.props`. The explicit native publication shape
used by the current CI/delivery records is:

```sh
dotnet publish src/cli/root/OpenForge.Cli/OpenForge.Cli.csproj \
  --configuration Release \
  --runtime linux-x64 \
  --self-contained true \
  --no-restore \
  --output artifacts/publish/linux-x64/open-forge \
  -p:OpenForgeSkipDevelopmentPublish=true
```

The resulting native executable is
`artifacts/publish/linux-x64/open-forge/OpenForge.Cli` and the project writes a
version marker beside it. Replace `linux-x64` with one of
`win-x64`, `win-arm64`, `linux-x64`, `linux-arm64`, `osx-x64`, or `osx-arm64`
only when the selected host/toolchain and delivery evidence support that RID.
Native AOT publication is explicit evidence; the managed `open-forge-dev`
publication does not prove it.

The current CI publication also publishes the Integration and EndToEnd test
projects to sibling `artifacts/publish/<RID>/integration` and
`artifacts/publish/<RID>/end-to-end` directories, then builds EndToEnd with
`-p:OpenForgeEndToEndTargetRuntimeIdentifier=<RID>` so its tests select that
RID's CLI. That is release/CI evidence rather than the shortest local setup.

Prerequisites visible in repository configuration are .NET SDK `10.0.100` with
latest-feature roll-forward, the target platform's Native AOT toolchain, and
the locked NuGet graph from `NuGet.Config`. The repository declares Node
`>=22.18.0` and Bun `>=1.3.0` for its root/npm support scripts.

## Local npm link

The exact maintainer sequence is:

```sh
dotnet restore OpenForge.Cli.slnx --locked-mode --configfile NuGet.Config --nologo
npm run cli:link
open-forge --version
```

`cli:link` invokes `src/cli/package-managers/npm/manage.ts link`. It publishes
the current host's CLI in Release/self-contained Native AOT mode, stages a local
package version containing the full lowercase Git SHA, then creates npm links
for the platform package, main package, and repository. It does not restore,
publish to a registry, update the lockfile, or run package scripts. It does
create global npm links, so it is a maintainer convenience rather than a
verification prerequisite.

The local-link runtime map supports these six host/RID pairs:

| Host              | RID           | Native file      |
| ----------------- | ------------- | ---------------- |
| macOS x64         | `osx-x64`     | `open-forge`     |
| Linux x64/glibc   | `linux-x64`   | `open-forge`     |
| Windows x64       | `win-x64`     | `open-forge.exe` |
| macOS ARM64       | `osx-arm64`   | `open-forge`     |
| Linux ARM64/glibc | `linux-arm64` | `open-forge`     |
| Windows ARM64     | `win-arm64`   | `open-forge.exe` |

The script rejects other `process.platform`/`process.arch` pairs. Remove the
known links with:

```sh
npm run cli:unlink
```

Worktree review and acceptance must invoke that worktree's artifact directly;
the machine-global `open-forge` link is not evidence for another worktree.

## Evidence and authority

| Fact                                         | Source                                                                                    |
| -------------------------------------------- | ----------------------------------------------------------------------------------------- |
| Root solution and SDK/test runner            | `OpenForge.Cli.slnx`, `global.json`                                                       |
| Shared target framework, artifacts, six RIDs | `Directory.Build.props`                                                                   |
| Development publication path and target      | `src/cli/root/OpenForge.Cli/OpenForge.Cli.csproj`                                         |
| EndToEnd RID selection and MTP runner        | `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj` |
| Root scripts                                 | `package.json` (`cli:dev`, `cli:link`, `cli:unlink`)                                      |
| Local-link behavior                          | `src/cli/package-managers/npm/local-link.ts`, `manage.ts`, `package-model.ts`             |
| Native CI publication                        | `.github/workflows/cli.yml`                                                               |
| Root workflow decision                       | `.agents/memory/crystallized/decisions/repository-root-cli-tooling.md`                    |
| Public setup wording                         | `docs/development.md` on `develop`                                                        |

## Material caveat

The current docs say the replacement CLI is implemented but unreleased: only
Linux x64 matching-host/package evidence is described as proven, while the
other host receipts and final delivery acceptance remain incomplete. Keep that
release qualification until the delivery evidence changes.
