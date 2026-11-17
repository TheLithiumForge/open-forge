---
open-forge:
  description: Record the grouped native output and commented ignore corrections for real journeys
  tags: [Memory, Working, CLI, Tool, Evidence, Contextual]
---

# Journey config fixes

Journey config fixes is the lane B correction that defers unavailable conditional native outputs before installation and recognizes commented all-data ignore files.
For example, an absent optional client can wait for native discovery while a detected client still needs its independently checked path.

Status: Complete. The maintainer accepted the nine-file candidate and authorized its local commit. E's actual journey rerun is pending.

## Scope

The work starts from integrated core `885b17db3e56ceb6839ba23609e85a4380c75018` on `feature/tool-setup-b-journey-fixes`.
The previous worktree was clean at `46834001af0470d5cff565e772339856e335fa52`.
The branch `feature/tool-setup-b` keeps that commit.
The assignment fixes E-J1 and E-J2 from E's `windows-journeys.md`, lines 118-155.
Only B's config implementation, matching tests, packet Progress and Evidence and this record can change.
The current coordinator checkout's `tool-setup.md` Native outputs clarification was read without changing that checkout.

## Applicability

This is a focused CLI correction for developer files and native output planning.
Expected-state checks, physical containment, atomic writes and sibling backups keep the existing recovery behavior.
The supported failures are missing inputs, malformed declarations, ordinary file or process failures and concurrent edits seen by expected-state checks.
The work adds no protection against a malicious same-user process.

The existing BCL token expansion and Git process integration cover both corrections.
Native discovery, Root bindings, D's refresh guard and the frozen shared models stay unchanged.
No new parser, dependency, platform mechanism or exception is needed.
Unit tests cover token validation.
Integration tests cover planning with owned files and effective-ignore decisions with owned Git repositories.
The current local gate is a managed solution build and both mandatory .NET format commands before each commit.
The maintainer prohibits all local test runs. CI runs every test.
Snapshot regeneration is integrator-only.

## Corrections

E-J1 requires deferral only for unavailable client-specific outputs in first-install description planning with a declared native plan.
All tokens in such an output still need validation.
Detected clients, nonconditional outputs and setups without a native plan still require available safe roots.
Returned native paths still need a matching reviewed output declaration.
`ToolRecipeExpansion.ExpandAvailable` validates the complete template before returning unavailable for a missing well-formed client binding.
The normal `Expand` call still requires every value.
`ToolNativeOutputs` resolves available values once, so inserted token-like text stays literal.
`ToolSetupPlanner` uses the unavailable result only before an installed invocation exists, with a declared native plan and a client-conditional output.
The original request keeps all authored conditional output declarations.
The resolved output set and backup expectations contain only available destinations.

E-J2 requires the existing reviewed all-data forms to accept comment and blank lines.
Git still decides matching and precedence.
Other active patterns stay uncertain and require the existing conditional block behavior.
The recognizer drops only empty lines and lines starting with `#` before comparing the active lines to the same two reviewed forms.
It interprets no arbitrary ignore pattern.

## Regression cases

The focused regressions cover these cases:

- Missing optional first-install clients wait for discovery while available conditional destinations keep their null absence expectations.
- Missing nonconditional bindings and setups without a native plan still block.
- Unknown or malformed tokens after an unavailable client token still block.
- Dollar escapes and inserted values stay nonrecursive.
- A detected client without its binding blocks after the read-only inventory call.
- A detected client's path that escapes its independent declared root blocks.
- Bound detected-client outputs pass, and the complete detected set stays unchanged.
- An inventory path outside the reviewed set blocks even when its conditional declaration is unselected.
- Commented all-data files with LF or CRLF avoid a new Open Forge block and keep the native file bytes.
- A commented file with an active data exception still needs the existing conditional block.

Existing focused cases still cover partial and probe-only ignore rules, later negations, unterminated final lines, reapply and guarded removal.
All inputs are small synthetic declarations and owned files.
The native inventory host is injected and never invokes a real tool.
Git tests use the existing owned Git host and redirected Git home.

## Composition

Service constructors are unchanged:

```csharp
var backups = new ToolBackupService(processHost);
var config = new ToolConfigService(processHost, backups, nativePlanReader);
```

The optional fourth `Func<string, byte[]> readTemplate` argument is unchanged.
There is no shared contract request.

## Source identity

The seven changed C# sources have these SHA-256 values:

| File                                                                                                                        | SHA-256                                                            |
| --------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------ |
| `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/ConfigChanges/ToolEffectiveIgnore.cs`                     | `a5f9ce93a4786d3ffee41b2db868a3774b1f62ac95921bf4f3f82f1aefa2944f` |
| `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/ConfigChanges/ToolNativeOutputs.cs`                       | `df5d9149fb0032527bdf0f1f601ead238af36a29a4608783ce3d4ca944675ac0` |
| `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/ConfigChanges/ToolRecipeExpansion.cs`                     | `53e1089cd886080674cac68cd5617429181491db16721d4080fbf9c697397b6d` |
| `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/ConfigChanges/ToolSetupPlanner.cs`                        | `08292f0c39848a5907582c3b36dd2e377ff5f2cbc6479ae199ca411473cf2b94` |
| `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Tools/Shared/ConfigChanges/ToolEffectiveIgnoreTests.cs` | `0a18d553173dc7830ab843a230e84567aa216425d45c10b70ab722af568c52ac` |
| `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Tools/Shared/ConfigChanges/ToolSetupPlannerTests.cs`    | `b89a18cd43f4777b4e7b00852f608191694277c237a5f6c0afcfa05ce53817fc` |
| `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Tools/Shared/ConfigChanges/ToolRecipeExpansionTests.cs`          | `d96f8dbfa71f1181a2e1fc065728a3c4631676344511bbfa688c1a9d9d2c202b` |

The machine copy is `artifacts/lane-b/journey-fixes/source-sha256.json`.

## Journey patch

The source-only patch for E is `artifacts/lane-b/journey-fixes/journey-config-fixes.patch`.
Its SHA-256 is `27b3c7a09eb374d8e61f95b852cef21ff6315d0dafada8a727371c18b2c4cabc`.
It contains exactly the seven C# files above against integrated core `885b17db3e56ceb6839ba23609e85a4380c75018`.
The patch passed reverse application checking against this candidate without changing any file.
The coordinator can give E the patch for its actual owned journey rerun.
This lane doesn't apply it to E's worktree.

The patch can be recreated from the checked uncommitted source:

```powershell
git diff --binary --output=artifacts/lane-b/journey-fixes/journey-config-fixes.patch 885b17db3e56ceb6839ba23609e85a4380c75018 -- src/cli/
git apply --reverse --check artifacts/lane-b/journey-fixes/journey-config-fixes.patch
```

## Checks

The maintainer accepted this candidate and authorized the local commit on `feature/tool-setup-b-journey-fixes`.
The commit continuation ran only the managed solution build and both mandatory .NET format commands.
All three exited 0. The build had zero warnings and errors.
Their artifacts are `artifacts/lane-b/journey-commit/build.log`, `artifacts/lane-b/journey-commit/whitespace.log` and `artifacts/lane-b/journey-commit/style.log`.
No local tests or snapshot regeneration ran in this continuation.
The seven accepted C# hashes above are unchanged.
The remaining checks and test commands in this section predate the redirect and are historical evidence.

`npm ci` exited 0 in this worktree after branch creation.
The final managed solution build exited 0 with zero warnings and errors.
Its artifact is `artifacts/lane-b/journey-fixes/build.log`.
The three touched classes selected and executed 66 passing cases.
Every command exited 0 with zero failures, skips and runner warnings.

The focused results are:

| Class                      | Selected and executed | Passed | Exit code | Artifact                                     |
| -------------------------- | --------------------- | ------ | --------- | -------------------------------------------- |
| `ToolRecipeExpansionTests` | 27                    | 27     | 0         | `artifacts/lane-b/journey-fixes/recipes.log` |
| `ToolSetupPlannerTests`    | 20                    | 20     | 0         | `artifacts/lane-b/journey-fixes/setups.log`  |
| `ToolEffectiveIgnoreTests` | 19                    | 19     | 0         | `artifacts/lane-b/journey-fixes/ignore.log`  |

After two whitespace-only line breaks in the planner test fixture, the final solution build and its 20 cases passed again.
The other 46 cases passed on the same unchanged production and test source hashes.
Both mandatory format commands were repeated after that final source correction.
Their exact exit codes are:

| Command                           | Exit code | Artifact                                        |
| --------------------------------- | --------- | ----------------------------------------------- |
| `npm run check:dotnet:whitespace` | 0         | `artifacts/lane-b/journey-fixes/whitespace.log` |
| `npm run check:dotnet:style`      | 0         | `artifacts/lane-b/journey-fixes/style.log`      |

The runtime target is managed `net10.0` on Windows x64 with SDK `10.0.101`.
The local NU1903 override stays on the invocation.
The managed solution build command is:

```powershell
dotnet build OpenForge.Cli.slnx --configuration Release -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
```

The three touched classes use these exact commands:

```powershell
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true -- --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.ConfigChanges.ToolRecipeExpansionTests --minimum-expected-tests 27 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.ConfigChanges.ToolSetupPlannerTests --minimum-expected-tests 20 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.ConfigChanges.ToolEffectiveIgnoreTests --minimum-expected-tests 19 --fail-warns on --fail-skips on --no-ansi --progress off
```

Both mandatory format commands run after the final source correction:

```powershell
npm run check:dotnet:whitespace
npm run check:dotnet:style
```

The first planner run selected 20 cases and failed one superseded fixture case with exit code 2.
A fixture edit during the first build left the earlier compiled input newer than its source timestamp.
The incremental build then kept that assembly.
The old parameter name in the failure identified the stale input.
That run is invalidated and its artifact is `artifacts/lane-b/journey-fixes/failed-setups-stale.log`.
A fresh compilation passed all 66 cases before the final whitespace correction:

```powershell
dotnet build OpenForge.Cli.slnx --configuration Release --no-incremental -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
```

That build exited 0 with zero warnings and errors.
Its artifact is `artifacts/lane-b/journey-fixes/fresh-build.log`.
The earlier incremental log is `artifacts/lane-b/journey-fixes/incremental-build.log`.
The first whitespace and style checks each exited 2 for the two line breaks.
Their artifacts are `artifacts/lane-b/journey-fixes/failed-whitespace.log` and `artifacts/lane-b/journey-fixes/failed-style.log`.
The final checks above replace those failed results.
No flakes were seen.

The final Markdown checks are:

```powershell
npx --no-install prettier --write .agents/memory/working/cli-development/tool-setup/packets/lane-b.md .agents/memory/working/cli-development/tool-setup/evidence/journey-config-fixes.md
npx --no-install prettier --check .agents/memory/working/cli-development/tool-setup/packets/lane-b.md .agents/memory/working/cli-development/tool-setup/evidence/journey-config-fixes.md
```

Both Markdown commands exited 0.
Their artifacts are `artifacts/lane-b/journey-fixes/markdown-format.log` and `artifacts/lane-b/journey-fixes/markdown.log`.
The final LF, Git whitespace, nine-file assignment, source hashes, patch, evidence links, frozen packet instructions, accepted receipt text and preserved branch checks passed.
Their artifact is `artifacts/lane-b/journey-fixes/verification.log`.

There is no rendered-text change or synthetic snapshot impact in these classes.
No Imprint or payload snapshot was regenerated or accepted.
No full suite, Native AOT or actual native journey ran here.
Native account, home, PATH and registry state stayed untouched.
Root, D sequencing, native discovery, frozen models, catalogue entries and E's documents stayed unchanged.
The earlier rejected cleanup wasn't retried.
The [lane B packet](../packets/lane-b.md) keeps the accepted 138, 67 and 69 receipts unchanged.
The coordinator handles the new record's grouped Index pass and integration.
