---
open-forge:
  description: Current progress and evidence plan for the exact .NET foundation and six-RID Native AOT spike
  tags: [Memory, Working, CLI, Release, Task, Development, DotNet, NativeAOT, Contextual, Active]
---

# Foundation And Native AOT Spike Task

## Outcome

Establish the reproducible, non-shipping .NET foundation for the replacement
CLI and execute one real Native AOT spike for each accepted RID. Prove the
accepted parser, serialization, filesystem, typed-result, test, build, trimming,
and Native AOT assumptions without implementing any retained command.

## Authority

- The [CLI Architecture](../../crystallized/documents/cli/architecture.md)
  defines the accepted topology, dependencies, runtime boundaries, test model,
  RIDs, and stop conditions.
- The [CLI Implementation Directive](../../../directives/open-forge/cli/implementation.md)
  defines implementation, Native AOT, filesystem, locality, and evidence rules.
- The [Test Evidence Directive](../../../directives/open-forge/testing/evidence-integrity.md)
  defines durable selection, isolation, and real-boundary requirements.
- The [Development Workflow](../../../workflows/development/_development.md)
  defines the Gray, Red, Green, Blue, Purple, public-scenario, full-gate, review,
  correction, and Acceptance sequence.

## Baseline

- Starting commit: `768bd51a5a9f205c2b752d004a7c8635cc82486c`
- Branch: `feature/cli-foundation-aot-spike`
- The branch started from exact local `develop` at the same commit.
- The frozen `src/cli-mvp/` implementation, its build support, tests, and output
  are outside this Task.

## Scope

Allowed product and evidence surfaces are the accepted root .NET configuration,
`OpenForge.slnx`, `src/open-forge-cli/OpenForge.Cli/`, the two mirrored test
projects under `tests/open-forge-cli/`, one six-RID CI workflow, focused root
ignore rules, and the current CLI program records that describe this Task.

The callable Gray contract is an internal typed foundation probe plus the
process entry boundary. The probe accepts an isolated workspace path and bounded
Markdown and YAML inputs. It returns a concrete report that proves the fixed
Markdig pipeline, YamlDotNet static context, source-generated STJ metadata with
reflection disabled, exact filesystem byte round-trip, and actual exclusive
file-handle contention. The process entry exposes only accepted root help,
version, and invalid-input behavior. The probe is not a command or a shipping
public interface.

Forbidden surfaces are retained-command behavior, package wrappers, lifecycle
or mutation semantics beyond the focused lock probe, release or publication,
legacy compatibility, `src/cli-mvp/`, generated output, sealed handoffs, and any
new library or Architecture choice.

## Progress

| Phase              | State       | Evidence                                                                                                                                  |
| ------------------ | ----------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| Preflight          | Adopted     | Exact scope, toolchain, support policy, evidence matrix, and continuation boundary established before Task mutation.                      |
| Gray contract      | Not started | Internal request/report/result and process-entry signatures remain to be frozen.                                                          |
| Red                | Not started | Complete affected managed, integration, process, and Native AOT evidence remains to be created and observed failing for missing behavior. |
| Green              | Not started | Minimal production behavior remains to be implemented.                                                                                    |
| Blue               | Not started | One bounded production-structure pass remains.                                                                                            |
| Purple             | Not started | One bounded test-structure pass remains.                                                                                                  |
| Public scenario    | Not started | The published native executable must prove root help, version, and invalid input.                                                         |
| Full gate          | Not started | Locked restore, build, tests, local native execution, and all six native CI jobs remain.                                                  |
| Independent review | Not started | Targeted correctness and local-improvement reviews are planned after the full gate.                                                       |
| Correction         | Not used    | At most one complete correction cycle is available.                                                                                       |
| Acceptance         | Not started | Requires the complete gate and Mastermind final review.                                                                                   |

## Decisions Needed

None. The maintainer authorized Gate 5, accepted the existing Architecture, and
accepted alignment to the current official .NET 10 support floors. Hosted CI
execution is spike evidence on current native runners; it does not replace the
later support-floor execution required before release.

## Evidence

The active toolchain is .NET SDK `10.0.101`, C# `14.0`, `net10.0`, xUnit v3
`4.0.0` through Microsoft Testing Platform `2.3.3`, and the exact accepted
runtime packages. Central Package Management and committed NuGet lock files
must make all direct and transitive inputs reproducible.

The complete affected matrix is:

- Unit: deterministic typed probe values and source-generated JSON metadata.
- Integration: valid bounded Markdown and YAML, invalid input, real isolated
  filesystem byte round-trip, actual exclusive-handle contention, and failure
  without unintended outside mutation.
- EndToEnd: the real published Native AOT executable returns correct exits and
  stream allocation for root help, version, and invalid input.
- Native AOT: a published xUnit v3 AOT system-test executable invokes the
  internal production probe and the published production executable on each
  matching native RID runner.

Every `Fact` or `Theory` must have an explicit `DisplayName`, one durable
`Feature` trait, and exactly one `Evidence` trait. Tests own isolated temporary
state and must be independently selectable.

Focused evidence uses these exact commands after the applicable projects exist:

```sh
dotnet restore OpenForge.slnx
dotnet build OpenForge.slnx --configuration Release --no-restore
dotnet test --project tests/open-forge-cli/OpenForge.Cli.Tests/OpenForge.Cli.Tests.csproj --configuration Release --no-build --filter-trait "Evidence=Unit"
dotnet test --project tests/open-forge-cli/OpenForge.Cli.Tests/OpenForge.Cli.Tests.csproj --configuration Release --no-build --filter-trait "Evidence=Integration"
dotnet publish src/open-forge-cli/OpenForge.Cli/OpenForge.Cli.csproj --configuration Release --runtime win-x64 --no-restore --output .temp/cli-foundation/win-x64/cli
dotnet publish tests/open-forge-cli/OpenForge.Cli.SystemTests/OpenForge.Cli.SystemTests.csproj --configuration Release --runtime win-x64 --no-restore --output .temp/cli-foundation/win-x64/tests
OPEN_FORGE_CLI_PATH="$(pwd)/.temp/cli-foundation/win-x64/cli/OpenForge.Cli.exe" ./.temp/cli-foundation/win-x64/tests/OpenForge.Cli.SystemTests.exe
```

The selected full gate is:

```sh
dotnet restore OpenForge.slnx --locked-mode
dotnet format OpenForge.slnx --verify-no-changes --no-restore
dotnet build OpenForge.slnx --configuration Release --no-restore
dotnet test --project tests/open-forge-cli/OpenForge.Cli.Tests/OpenForge.Cli.Tests.csproj --configuration Release --no-build
git diff --check
```

It also includes the local `win-x64` publish and direct system-test executable
commands above plus the same publish and execution sequence in native CI jobs
for `win-x64`, `win-arm64`, `linux-x64`, `linux-arm64`, `osx-x64`, and
`osx-arm64`. Exact-path prose formatting uses `bunx prettier --check` over the
changed authored Markdown files that do not contain a formatter-incompatible
generated representation. The CLI Directive entrypoint preserves its existing
manager-owned `Entries` representation instead. This does not make a whole-
repository formatting or frozen-MVP validation claim.

```sh
bunx prettier --check ".agents/directives/open-forge/cli/implementation.md" ".agents/memory/crystallized/documents/cli/architecture.md" ".agents/memory/working/cli-release/_cli-release.md" ".agents/memory/working/cli-release/decision-agenda.md" ".agents/memory/working/cli-release/release-plan.md" ".agents/memory/working/cli-release/task-foundation-aot-spike.md" ".agents/memory/working/checkpoints/cli-release.md"
```

The public scenario runs the published production executable for `--help`,
`--version`, and an invalid token, then inspects exit status, stdout, and stderr.
Integrated inspection covers the actual Git diff, package locks, solution and
physical topology, generated build outputs only under ignored temporary paths,
and absence of retained-command or legacy changes.

If behavior or evidence is incomplete, correction returns to the earliest
invalidated Gray, Red, or Green phase and repeats all downstream phases. A
Native AOT or dependency failure that materially invalidates an accepted
assumption stops the Task and returns to Architecture.

No Task commit exists yet. Likely coherent commits are one Gate 5 activation and
support-policy record commit, followed by one complete foundation-cycle commit.
No merge, push, package publication, or release occurs before Task Acceptance.

Residual risk after this Task will include support-floor execution. Current
hosted native runners can provide the planned six-RID spike evidence on their
available images, while exact Windows, macOS, and glibc floor proof remains part
of the complete Gate 5 release boundary.

Adding this Task affected the generated `Entries` in the CLI release program.
`open-forge-old index` was attempted after the Task record was added and before
executable-source mutation. It stopped at the preserved duplicate physical-
identity defect for `contracts/index/_index.md`. The matching Task entry is
present but has not been regenerated successfully. Do not treat Gate 4's older
index evidence as proof for this entry. Recheck the entry against this file's
path, description, and tags, and retain this limitation until replacement Index
evidence can own the projection.

## Completion

Complete this Task only when the frozen contract and affected evidence agree;
warning-free locked restore, managed build, trimming, and Native AOT publication
pass; the published production and AOT test executables run successfully for all
six RIDs on matching native runners; required test identities and isolation are
present; the public scenario and full gate pass; targeted review has no blocking
finding; no correction remains open; and the Mastermind records Acceptance and
the final evidence and commits here.
