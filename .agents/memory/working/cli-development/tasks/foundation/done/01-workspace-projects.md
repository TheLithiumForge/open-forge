---
open-forge:
  description: Create the scoped .NET workspace, six-project graph, dependencies, artifacts, and preserved-test quarantine
  tags: [Memory, Working, CLI, Task, Foundation, DotNet, Workspace, Project, Contextual, Complete]
---

# Create The CLI Workspace And Project Graph

## Task State

- State: Complete.
- Implementer: Mastermind.
- Responsible role: Mastermind.
- Parent: [CLI Foundation](../_foundation.md).
- Plan step: F1.

## Expected Outcome

The accepted six-project .NET workspace exists entirely under `src/cli/`, restores
with exact dependencies, routes every C# output to `src/cli/artifacts/`, and keeps
preserved tests outside active compile roots.

## Exact Physical Result

Create:

```text
src/cli/global.json
src/cli/NuGet.Config
src/cli/Directory.Build.props
src/cli/Directory.Packages.props
src/cli/OpenForge.Cli.slnx
src/cli/root/OpenForge.Cli/OpenForge.Cli.csproj
src/cli/core/OpenForge.Cli.Core/OpenForge.Cli.Core.csproj
src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj
src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj
src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj
src/cli/tests/support/OpenForge.Cli.TestSupport/OpenForge.Cli.TestSupport.csproj
src/cli/tests/preserved/route-list-v1/
```

Move every currently preserved C# test and project file below
`src/cli/tests/preserved/route-list-v1/`. Preserve bytes and relative grouping.
Do not compile those files or retain their old project references as active
workspace configuration.

## Project And Build Decisions

- SDK: stable .NET 10 baseline `10.0.100`, `latestFeature`, no prerelease.
- Language: C# 14, nullable and implicit usings enabled.
- Build: warnings and analyzer/code-style warnings are errors; deterministic;
  package audit mode `all`.
- Artifacts: `UseArtifactsOutput=true` and one absolute artifacts root derived
  from `src/cli/Directory.Build.props`.
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
- `.gitignore` entries needed for `src/cli/artifacts/` and forbidden local output.
- No C# production or test behavior beyond project metadata.

## Protected Boundaries

- No repository-root C# workspace file.
- No command source, fake operation, test double, generated version source, CI
  behavior, or production class.
- No project-local `bin/` or `obj/` acceptance.
- No package version update or additional dependency.

## Verification

1. `dotnet --version` satisfies `src/cli/global.json`.
2. `dotnet sln src/cli/OpenForge.Cli.slnx list` reports exactly six projects.
3. `dotnet restore src/cli/OpenForge.Cli.slnx --configfile src/cli/NuGet.Config`
   resolves the exact graph without warnings.
4. MSBuild property inspection proves the artifact root for every project.
5. A repository scan finds no replacement C# control file at root, no authored
   Compile inventory, no virtual solution folder, and no active compile path under
   `tests/preserved`.
6. `git diff --check` passes.

The integrated Foundation, not this substep alone, owns the first full build.

## Stop Conditions

Stop before changing topology if the SDK artifacts layout cannot keep all binary
and intermediate output below `src/cli/artifacts/`, if a test package cannot
support the accepted AOT runner, or if the six-project dependency graph requires
an additional production assembly.

## Completion

Complete when the physical workspace and restore graph exactly match this Task and
the next Shell-contract Task can add source without changing project topology.
