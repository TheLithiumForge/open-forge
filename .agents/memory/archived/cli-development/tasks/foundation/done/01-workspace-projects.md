---
open-forge:
  description: Create the repository-root .NET workspace controls, six-project graph, dependencies, root artifacts, and active-test boundary
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Foundation, DotNet, Workspace, Project, Complete]
---

# Create The CLI Workspace And Project Graph

## Task State

- State: Complete.
- Implementer: Mastermind.
- Responsible role: Mastermind.
- Parent: [CLI Foundation](../_foundation.md).
- Plan step: F1.

## Expected Outcome

The accepted six-project .NET workspace uses repository-root control files,
restores exact dependencies, routes every C# output to root `/artifacts/`, and
keeps projects, source, and active tests below `src/cli/`.

## Exact Physical Result

Create:

```text
global.json
NuGet.Config
Directory.Build.props
Directory.Packages.props
OpenForge.Cli.slnx
src/cli/root/OpenForge.Cli/OpenForge.Cli.csproj
src/cli/core/OpenForge.Cli.Core/OpenForge.Cli.Core.csproj
src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj
src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj
src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj
src/cli/tests/support/OpenForge.Cli.TestSupport/OpenForge.Cli.TestSupport.csproj
```

The original foundation quarantined a preserved route-list inventory. A later
audit removed it after active tests already represented its useful expectations;
the only unique empty-YAML assertion moved into active Integration evidence.

## Project And Build Decisions

- SDK: stable .NET 10 baseline `10.0.100`, `latestFeature`, no prerelease.
- Language: C# 14, nullable and implicit usings enabled.
- Build: warnings and analyzer/code-style warnings are errors; deterministic;
  package audit mode `all`.
- Artifacts: `UseArtifactsOutput=true` and one absolute root `/artifacts/` path
  derived from repository-root `Directory.Build.props`.
- Solution: six direct project entries and no virtual folders.
- Root project: executable, `PublishAot`, `IsAotCompatible`, invariant version
  source, no command package references beyond Core and accepted host needs.
- Core project: class library, AOT/trimming compatible, package references for
  `System.CommandLine`, source-generated YAML, and STJ only as used.
- Unit: references Core and TestSupport.
- Integration: references Core, root, and TestSupport; publishable AOT test host.
- EndToEnd: references TestSupport only; publishable AOT test host.
- TestSupport: class library with no production project reference.

Pin the accepted package versions from the Architecture. Do not reference Markdig
until the Find Task activates the first body consumer.

## Allowed Changes

- Exact paths above.
- `.gitignore` entries needed for root `/artifacts/` and forbidden local output.
- No C# production or test behavior beyond project metadata.

## Protected Boundaries

- No repository-root C# source project or authored production source.
- No command source, fake operation, test double, generated version source, CI
  behavior, or production class.
- No project-local `bin/` or `obj/` acceptance.
- No package version update or additional dependency.

## Verification

1. `dotnet --version` satisfies root `global.json`.
2. `dotnet sln OpenForge.Cli.slnx list` reports exactly six projects.
3. `dotnet restore OpenForge.Cli.slnx --configfile NuGet.Config`
   resolves the exact graph without warnings.
4. MSBuild property inspection proves the artifact root for every project.
5. A repository scan finds no replacement C# control file at root, no authored
   Compile inventory, no virtual solution folder, and no obsolete preserved-test
   compile path.
6. `git diff --check` passes.

The integrated Foundation, not this substep alone, owns the first full build.

## Stop Conditions

Stop before changing topology if the SDK artifacts layout cannot keep all binary
and intermediate output below root `/artifacts/`, if a test package cannot
support the accepted AOT runner, or if the six-project dependency graph requires
an additional production assembly.

## Completion

Complete when the physical workspace and restore graph exactly match this Task and
the next Shell-contract Task can add source without changing project topology.
