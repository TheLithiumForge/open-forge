---
open-forge:
  description: Move replacement CLI tooling to the repository root and make ordinary test runs publish and discover the local development executable
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, DotNet, Testing, DeveloperExperience, Complete]
---

# Improve The Repository-Root CLI Developer Workflow

## Task State

- State: Complete. The accepted feature tree is squash-integrated into local
  `develop` at `d9e0686`.
- Responsible role: Mastermind.
- Parent: [Complete The Replacement CLI](00-cli-development.md).
- Branch: `feature/cli-root-developer-experience`.
- Integration: `d9e0686` (`Improve CLI developer workflow`).
- Review consumed: `DX1-R1` (`CHANGES_REQUIRED`; accepted finding `DX1-R1-F1`).
- Correction consumed: `DX1-C1` (complete; corrected `DX1-R1-F1`).

## Execution Capsule

- Profile: Assured.
- Review budget: One; consumed by `DX1-R1`.
- Council budget: Zero.
- Correction budget: One; consumed by `DX1-C1`.

## Outcome

Maintainers and IDEs can restore, build, and test the complete replacement CLI
from the repository root. Building the CLI project creates a managed local
`open-forge-dev` publication that public-process tests discover without ambient
configuration.

## Accepted Decisions

- Keep C# projects, source, and active tests below `src/cli/`.
- Keep `OpenForge.Cli.slnx`, `global.json`, `NuGet.Config`,
  `Directory.Build.props`, and `Directory.Packages.props` at the repository root.
- Route every .NET output to ignored root `/artifacts/`.
- Publish local managed development output to
  `artifacts/publish/open-forge-dev/<Configuration>/open-forge-dev[.exe]`.
- Remove published-executable environment-variable selection. Ordinary EndToEnd
  builds target the development publication. Native evidence compiles the
  EndToEnd project with one explicit target RID and uses the corresponding
  repository artifact convention.
- Publish the development executable after an ordinary non-RID CLI build. The
  explicit `OpenForgeSkipDevelopmentPublish=true` MSBuild property disables this
  step for CI or another build that does not need the development artifact.
- Keep the CLI informational version in one repository-root MSBuild property
  consumed by both the executable and EndToEnd target selection.
- Keep Native AOT publication explicit. Do not install or publish the development
  executable globally.
- Remove the obsolete preserved route-list test tree after retaining its only
  unique active assertion.
- Temporarily avoid compatibility entrypoint names until the replacement Index
  command proves physical-identity handling.

## Expected And Protected Paths

Expected paths include the root .NET control files and ignore rule, CLI projects,
active test support, CLI workflow, developer documentation, affected current CLI
Architecture and Working records, candidate Index and References contract routes,
one Crystallized Decision, and one Emerging Observation.

Protected paths include frozen CLI MVP source and tests, current command behavior,
References and Index production, archived and sealed history, package publication,
remotes, credentials, and unrelated `.apm` or lockfile worktree changes.

## Acceptance

- Root solution membership reports exactly six projects.
- Building the CLI project directly, building the solution, root `dotnet test`,
  and an IDE test build create or reuse the same development publication without
  environment variables.
- EndToEnd tests execute the development publication by default. An explicitly
  compiled target RID selects only its known native publication path; arbitrary
  executable overrides and ambient target state do not exist.
- `dotnet test --no-build` retains its deliberate existing-publication contract.
- All .NET output is below root `/artifacts/`; no project-local or former
  `src/cli/artifacts/` output remains.
- Candidate route links and generated Entries are coherent, and available routing
  validation is clean or an exact tool blocker is recorded.
- The intended feature diff passes formatting, whitespace, and one final
  independent review before one local squash commit on `develop`.

## Evidence

The unchanged managed baseline passed `1190/1190` before the final
target-selection change. Focused final evidence passes:

- direct ordinary CLI build with generated `open-forge-dev` and exact
  `0.0.0-dev` marker;
- direct CLI build with `OpenForgeSkipDevelopmentPublish=true` and no development
  publication for its isolated configuration;
- root `dotnet test` discovery with all affected EndToEnd evidence plus one Unit
  and one Integration smoke test, `72/72` with zero skips;
- ordinary publication-selected EndToEnd evidence, `70/70` with zero skips;
- explicit `linux-x64` build selection against its repository publication,
  `70/70` EndToEnd with zero skips;
- warning-free Release solution build and `dotnet format --verify-no-changes`.

The installed WSL SDK contains only the distro-specific
`Microsoft.NETCore.App.Runtime.NativeAOT.ubuntu.24.04-x64` pack. A no-restore
`linux-x64` Native AOT publish therefore stops in the SDK before compilation
because `PrivateSdkAssemblies` is empty. No dependency download was authorized.
A framework-dependent `linux-x64` publication proved the changed version-marker
and build-selected EndToEnd path; previously accepted Native AOT product evidence
remains unchanged.

This was a local pack-availability limitation, not evidence that the portable
`linux-x64` RID was unsupported. During later References acceptance, standard
SDK publication with restore authority obtained the exact portable
`Microsoft.NETCore.App.Runtime.NativeAOT.linux-x64` `10.0.11` pack from the
configured NuGet source. The same `linux-x64` command then published and executed
the root, Integration, and EndToEnd Native AOT artifacts successfully. No
distro-specific RID, direct package reference, symlink, or project workaround was
needed.

## Review Disposition

`DX1-R1-F1` found that five direct publication-target tests used temporary files
without invoking the published executable while claiming EndToEnd evidence.
`DX1-C1` removed those tests and their test-only `DiscoverAt` seam instead of
moving EndToEnd-local support into an unrelated test tier. Every remaining
EndToEnd test invokes the build-selected published executable. The focused root,
ordinary-development, and explicit `linux-x64` reruns above correct the affected
counts and pass with zero skips.

## Stop Conditions

Stop before downloading dependencies without exact authorization, contacting a
remote, publishing or globally installing an executable, implementing References
or Index behavior, editing generated regions without the repository manager, or
including unrelated worktree changes in a commit.
