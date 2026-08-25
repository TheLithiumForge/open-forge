---
open-forge:
  description: The replacement CLI uses repository-root .NET tooling and an automatically built local development publication for ordinary tests
  tags: [Memory, Decision, CurrentTruth, CLI, DotNet, Testing, DeveloperExperience]
---

# Repository-Root CLI Tooling

## Context

The replacement CLI originally kept its solution, SDK selection, NuGet
configuration, shared MSBuild files, and artifacts below `src/cli/`. That boundary
made `dotnet test` from the repository root fail to discover the intended
workspace. EndToEnd tests also required maintainers and IDE launchers to provide
two published-executable environment variables before any public-process evidence
could run.

## Decision

The repository root owns `OpenForge.Cli.slnx`, `global.json`, `NuGet.Config`,
`Directory.Build.props`, and `Directory.Packages.props`. Replacement source,
projects, and tests remain below `src/cli/`. All .NET output uses the ignored root
`artifacts/` directory.

An ordinary non-RID CLI project build publishes a managed, non-AOT development
executable named `open-forge-dev` and its version marker below
`artifacts/publish/open-forge-dev/<Configuration>/`. EndToEnd tests discover this
artifact directly. Local terminal and IDE runs do not require environment
variables. Design-time builds skip publication, and the explicit
`OpenForgeSkipDevelopmentPublish=true` MSBuild property provides the opt-out for
builds that do not need the development artifact.

Native AOT and CI evidence compile the EndToEnd project with one explicit
supported target RID. That build selection resolves only the corresponding
`artifacts/publish/<RID>/open-forge/OpenForge.Cli[.exe]` artifact and its generated
version marker. Arbitrary executable and version overrides do not exist. `dotnet
test --no-build` does not publish and therefore requires the artifact selected by
the preceding build.

## Rationale

Repository-root tooling matches the way maintainers and IDEs enter the repository.
It makes the shortest expected workflow, `dotnet test`, exercise the complete
managed test graph and its public executable without manual setup. Keeping source
and project boundaries under `src/cli/` preserves the accepted dependency and
locality model.

The named development publication makes the executable boundary visible and
inspectable. It avoids a global installation, leaves Native AOT publication an
explicit release and CI concern, and prevents local test setup from leaking into
shell profiles or IDE launch configuration.

## Alternatives And Tradeoffs

- Keeping the workspace under `src/cli/` preserved a narrow physical boundary but
  required directory-specific commands and IDE configuration.
- Requiring environment variables kept test selection flexible but made ordinary
  local execution depend on ambient hidden state.
- Installing `open-forge-dev` globally would make the command convenient but
  introduce mutable machine-wide state and stale-version risk.
- Publishing after an ordinary CLI build adds managed publish work to the local
  build graph. Incremental inputs and outputs avoid unchanged republishing, while
  the explicit opt-out avoids the work in native CI builds.

## Consequences

- Root `dotnet restore`, `dotnet build`, and `dotnet test` are the ordinary
  developer workflow.
- IDE test runners build the EndToEnd project and receive the same local
  publication.
- Root `/artifacts/` is the only .NET output boundary.
- Local public-process evidence uses no environment configuration.
- Native public-process evidence uses a closed build-time RID selection rather
  than arbitrary ambient paths.
- Native AOT remains explicit evidence and is never implied by the development
  publication.

## Authoritative Sources

- [Replacement CLI Architecture](../documents/cli/architecture.md)
- [CLI Implementation Directive](../../../directives/open-forge/cli/implementation.md)
- [Development guide](../../../../docs/development.md)
- [`OpenForge.Cli.slnx`](../../../../OpenForge.Cli.slnx)
