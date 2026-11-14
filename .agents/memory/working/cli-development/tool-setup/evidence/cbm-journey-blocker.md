---
open-forge:
  description: Identify E-J1's exact missing CBM token and route the bounded setup-planning correction
  tags: [Memory, Working, CLI, Tool, Contextual, Evidence]
---

# CBM planning blocker

The CBM planning blocker is E-J1, a first-install defect that expands a conditional client path before native client discovery is available.
The first missing value is `${client:continue-config}` for an absent Continue configuration in E's fresh owned account.

Status: DIAGNOSIS_READY. Read-only diagnosis, with this report as the only new file in this follow-up.

## Existing finding

E already records E-J1 in `D:/Repositories/open-forge-worktrees/tool-setup-e/.agents/memory/working/cli-development/tool-setup/evidence/windows-journeys.md:118-141`.
Keep that finding ID and high severity. This report adds exact token and artifact checks to its diagnosis.
It creates no separate review or docs task.

The inspected E tree combines core `885b17db3e56ceb6839ba23609e85a4380c75018`, C's checked Root candidate and the supplied pending inputs.
Its manual host reports prospective version `0.9.0-beta.19` through the normal build property.
F stays at `ffef620bca8ae506c611fc3f244e63a72ceae1ec`.
The [Task 90 record](../../tasks/task90-tool-setup.md) keeps integration and acceptance with the coordinator.

## Exact failure

The deciding artifacts are under E's `artifacts/task90-lane-e-final/`:

| Artifact                                  | SHA-256                                                            |
| ----------------------------------------- | ------------------------------------------------------------------ |
| `codebase-memory-install.json`            | `232898ad70bd6f400bf44e638ac9873f4993ce790a41adb06e9aa4d6238dc276` |
| `codebase-memory-preinstall-inspect.json` | `2d77d959e7ffaeeec621b5a2bae1ace9ea06900f3d6c68f0abb806c6379f2063` |
| `codebase-memory-install-before.json`     | `efd34070ec7a27a638b5b91ba00e67676aeb589b051ef7771eb8525552dfee72` |
| `codebase-memory-install-after.json`      | `efd34070ec7a27a638b5b91ba00e67676aeb589b051ef7771eb8525552dfee72` |

The result is `blocked` with `tool-install.native-plan-blocked`, zero completed steps and `usedNetwork=false`.
Native preview is `Description`.
The before/after file-hash maps are equal, including the owned account and Git repository.
No `.continue/config.yaml` or explicit Continue override is present in the recorded inputs.

A read-only Node comparison of the two JSON reports gives this exact prefix:

1. Read the 326 authored setup output declarations from preinstall Inspect.
2. Keep the 303 declarations applicable to Windows.
3. Compare them with the 208 outputs resolved in the blocked Install result.
4. The last resolved path is the account's `.tabnine/mcp_servers.json`.
5. Windows declaration 209, zero-based index 208, is `PathTemplate="${client:continue-config}"`, `ClientId="continue"`, account scope, exact match and backup purpose.

This identifies the failing token without running a tool or another planner process.
The plan fails during declaration expansion, before B reaches backup feasibility checks.
The serialized declarations contain 61 unique client-root keys across setup and uninstall, matching F's complete binding inventory.
Coverage means every key has either a path or an unavailable fact. It doesn't mean every client has an available path in a fresh home.

## Source trace

All paths in this section are relative to the inspected E worktree.

| Source                                                                                                                             | Deciding behavior                                                                                                                       |
| ---------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------- |
| `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Catalogue/Shared/CodebaseMemory/CodebaseMemoryRegistryOutputs.cs:13-19` | Authors Continue after Tabnine and gives the output native client ID `continue`                                                         |
| `src/cli/root/OpenForge.Cli/Composition/Shared/ToolHomes/ToolClientRootResolver.Registry.cs:24` and `:52-57`                       | Uses `CBM_CONTINUE_CONFIG_PATH`, otherwise `.continue/config.yaml`. Returns no choice when the selected file doesn't exist              |
| `src/cli/root/OpenForge.Cli/Composition/Shared/ToolHomes/ToolClientRootResolver.cs:47-64`                                          | Records that absent choice in `Clients.Unavailable`, leaving it out of `Clients.Bindings`                                               |
| `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Tool/Shared/WindowsJourneys/ManualToolJourneyHost.cs:29-60`     | Resolves the real provider with owned known folders, copies available bindings and injects owned homes, roots and environment into Root |
| `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Tool/Shared/WindowsJourneys/ManualToolJourneyHost.cs:71-90`     | Gives no Continue, TRAE, Roo or Cody config override. All home, cache and runtime redirection stays owned                               |
| `src/cli/root/OpenForge.Cli/Composition/Shared/ToolComposition/ToolCompositionFactory.cs:56-75`                                    | Resolves profiles and forwards the available binding map to writer facts                                                                |
| `src/cli/operations/OpenForge.Cli.Operations/Commands/Tool/Shared/Writing/Planning/ToolWriteToolPlanner.cs:56-62` and `:132-145`   | Passes those bindings into native setup context, with `InstalledInvocation=detected.Invocation`                                         |
| `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/ConfigChanges/ToolSetupPlanner.cs:19-48`                         | Keeps `clients=null` when no installed invocation exists. It correctly doesn't run a future binary                                      |
| `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/ConfigChanges/ToolSetupPlanner.cs:50-57`                         | Filters by client only when inventory exists, then attempts to expand every other declaration                                           |
| `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/ConfigChanges/ToolRecipeExpansion.cs:73-80`                      | Looks up `continue-config`, finds no value and raises the recorded error                                                                |
| `src/cli/operations/OpenForge.Cli.Operations/Commands/Tool/Shared/Writing/Planning/ToolWritePlanner.cs:103-105`                    | Turns the setup problem into `NativePlanBlocked` before effects                                                                         |

The provider's existing-file choice follows E's checked native resolver facts at `evidence/upstream-recipes.md:317-342`.
The complete 61-key map is recorded in `evidence/native-home-roots.md:137-154`.
`ToolClientRootTests.ConditionalPaths`, lines 128-139, already checks that `continue-config`, `trae-config`, `roo-code-config` and `sourcegraph-cody-config` stay unavailable until their required files exist.
If Continue alone were seeded, TRAE and Roo would expose the same first-install planning problem later in the declaration order.

## Classification

This is B's conditional setup-planning gap at the Root-to-Framework connection.
It reproduces for a normal fresh account, so a manual-harness redirection change isn't the product correction.
The exact token spelling and catalogue `ClientId` are already present.
F's provider keeps the reviewed native path choices, and C forwards its available bindings as the current interfaces require.
The expansion function correctly rejects a value it wasn't given.
The planner must decide which declarations can be expanded at this stage before calling it.

The existing contract supplies the deciding rule:

- `technical-designs/tool-setup.md:245-257` requires description-only planning before the binary exists, conditional potential outputs and an unavailable detected-client set.
- `technical-designs/tool-setup.md:231` requires unknown tokens, missing required roots, malformed tokens and invalid paths to block planning.
- `technical-designs/tool-setup.md:212-215` requires checking available facts before effects while allowing later native discovery to fail with completed receipts.

These locations are below `.agents/memory/crystallized/documents/cli/`.
The correction must keep both conditional output handling and required-root validation.

## Minimal correction

Use E-J1's bounded correction in B's setup planner:

1. When `InstalledInvocation` is absent and `NativePlan` is declared, keep an unavailable client-specific output as a conditional declaration for the description. Don't treat it as a concrete backup target or a detected client.
2. Continue planning and checking every destination that can be resolved. Keep unknown or malformed tokens, invalid paths and unavailable required destinations blocked. Don't catch and suppress every expansion exception.
3. After verified channel placement, use D's existing `ToolWriteNativeApplication.RefreshAsync` at lines 16-24. It replans with the installed invocation. B must use the full native detected-client set, skip declarations for absent clients and require every selected client's destination before setup and backups.
4. If a detected client still needs a missing binding, block native setup. Keep inventory containment and complete backup checks. Don't fabricate a path from returned inventory to fill a missing Root binding.

The existing authored `ToolNativeOutputSpec.ClientId`, `ToolSetupRequest.InstalledInvocation` and `ToolSetupSpec.NativePlan` identify this stage and condition.
No new client selector, catalogue path, native discovery adapter or per-client file needs to be invented.
Creating Continue files or injecting guessed paths would change native discovery and hide the defect.

B handles the planner and its bounded expansion result handling.
The coordinator handles any shared contract increment required to represent unresolved conditional outputs in a report.
The current `ToolSetupPlan` has only resolved `PossibleOutputs` (`Framework/Tools/Models/Services/ToolSetupModels.cs:24-32`).
Its request still contains the authored declarations.
D's projection currently reads only resolved outputs (`Commands/Tool/Shared/Writing/Application/ToolWriteProjection.cs:140-143`).
If the correction needs an additional shared result field for unresolved declarations, report `CONTRACT_REQUEST` before editing frozen models or changing the public report shape.
Don't put templates into physical backup paths or silently report them as resolved.
C connects such a decided input change if needed. F's native path rules stay unchanged.

## Focused regression

The repair needs these cases at owned deterministic boundaries:

- B - First-install description with `NativePlan`, no invocation and absent optional client bindings. Keep conditional declarations, zero process calls and zero writes. Resolve available outputs in the same plan.
- B - Installed inventory omits those clients, so their declarations don't require expansion. Installed inventory selects one, so its missing binding blocks before setup. Keep the existing unknown-token and backup-feasibility checks.
- C - Production CBM catalogue plus the real provider and owned empty homes through a composed exact-version install preview. Supply process, download and PATH sentinels. Check that the first-install description doesn't require installing unrelated clients, running a native command or downloading an archive.
- F - Keep the existing unavailable-root behavior as the input expectation. The repair mustn't make all 61 keys pretend to have paths.

Current independently selectable classes and counts are:

| Class after `OpenForge.Cli.IntegrationTests.`                | Current cases | Use                                                              |
| ------------------------------------------------------------ | ------------- | ---------------------------------------------------------------- |
| `Framework.Tools.Shared.ConfigChanges.ToolSetupPlannerTests` | 11            | B's planner and conditional inventory regression                 |
| `Commands.Tool.ToolCompositionIntegrationTests`              | 18            | C's connected preview regression                                 |
| `Composition.Shared.ToolHomes.ToolClientRootTests`           | 23            | Existing native path-choice checks, including `ConditionalPaths` |

The first two classes need the new cases. Use their actual post-edit counts when running the repair.
The existing focused command shape is:

```powershell
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.ConfigChanges.ToolSetupPlannerTests --minimum-expected-tests 11 --fail-warns on --fail-skips on --no-ansi --progress off
```

The `11` above describes the current class, not the future expanded class.
Use semantic facts and effects, with reviewed Imprint only if the description changes.
No new test, build, process reproduction, native execution, package download or snapshot capture ran in this diagnosis.
The existing E artifacts and the declaration-prefix check identify the failing production branch directly.
