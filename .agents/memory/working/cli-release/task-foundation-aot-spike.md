---
open-forge:
  description: Current progress and evidence plan for the stable .NET 10 foundation and six-RID Native AOT spike
  tags: [Memory, Working, CLI, Release, Task, Development, DotNet, NativeAOT, Contextual, Active]
---

# Foundation And Native AOT Spike Task

## Outcome

Establish the non-shipping .NET foundation for the replacement CLI, prove its
accepted boundaries through local Windows Native AOT execution, and prepare the
six-RID native workflow for later execution. Prove the accepted parser,
serialization, filesystem, typed-result, test, build, trimming, and Native AOT
assumptions without implementing any retained command.

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

| Phase              | State            | Evidence                                                                                                                          |
| ------------------ | ---------------- | --------------------------------------------------------------------------------------------------------------------------------- |
| Preflight          | Adopted          | Exact scope, toolchain, support policy, evidence matrix, and continuation boundary established before Task mutation.              |
| Gray contract      | Frozen           | The internal request/result/report union and process-entry signatures compile; both callables fail explicitly as not implemented. |
| Red                | Complete         | Eight initial cases failed at the Gray boundaries; correction Red added one empty-YAML case that failed at the exposed null path. |
| Green              | Complete         | The smallest production implementation makes all nine frozen Red cases pass.                                                      |
| Blue               | Complete         | The fixed Markdown pipeline is explicit and reusable; YAML and JSON source-generation contexts have separate focused files.       |
| Purple             | Complete         | Native process support passes cancellation through I/O and wait, then requests and awaits child-tree termination on cancellation. |
| Public scenario    | Complete         | The published `win-x64` binary returned help and version on stdout with exit 0 and invalid input on stderr with exit 4.           |
| Full gate          | Accepted locally | The complete Windows `win-x64` gate passes; WSL, macOS, and the six native CI jobs are deferred to later Gate 5 evidence.         |
| Independent review | Passed           | Correction and acceptance rereviews found no blocking issue; six native runner jobs remain later Gate 5 evidence.                 |
| Correction         | Complete         | Fresh review found an empty-YAML null path; correction Red through Purple, the public scenario, and the local full gate pass.     |
| Acceptance         | Accepted locally | The maintainer directed local acceptance and integration with cross-platform execution retained as later Gate 5 evidence.         |

## Decisions Needed

None. The maintainer authorized Gate 5, accepted alignment to the current
official .NET 10 support floors, and replaced the exact SDK-patch and NuGet-lock
policy with compatibility across stable .NET 10 SDKs. Hosted CI execution is
spike evidence on current native runners; it does not replace the later
support-floor execution required before release.

## Evidence

Local evidence currently uses .NET SDK `10.0.101`, C# `14.0`, `net10.0`, xUnit
v3 `4.0.0` through Microsoft Testing Platform `2.3.3`, and the exact accepted
direct packages. `global.json` establishes stable .NET 10 baseline `10.0.100`
with `rollForward=latestFeature`; CI selects `10.0.x`. Central Package Management
keeps direct package versions exact. NuGet lock files are not committed because
the Native AOT compiler and linker package patches follow the selected SDK.

YamlDotNet's runtime package does not carry its source generator. The accepted
static semantic path therefore uses the matching
`Vecc.YamlDotNet.Analyzers.StaticGenerator` `18.1.0` package as a compile-time
private asset. It adds no runtime dependency, and the central version policy
sets its exact package version.

The complete affected matrix is:

- Unit: relative-workspace validation before filesystem access.
- Integration: deterministic typed values, source-generated JSON metadata,
  valid bounded Markdown and YAML, malformed and empty YAML, real isolated
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
dotnet restore OpenForge.slnx
dotnet format OpenForge.slnx --verify-no-changes --no-restore
dotnet build OpenForge.slnx --configuration Release --no-restore
dotnet test --project tests/open-forge-cli/OpenForge.Cli.Tests/OpenForge.Cli.Tests.csproj --configuration Release --no-build
git diff --check
git diff --cached --check
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
bunx prettier --check ".github/workflows/cli-foundation-aot.yml" "global.json" ".agents/directives/open-forge/cli/implementation.md" ".agents/memory/crystallized/documents/cli/_cli.md" ".agents/memory/crystallized/documents/cli/architecture.md" ".agents/memory/working/cli-release/_cli-release.md" ".agents/memory/working/cli-release/decision-agenda.md" ".agents/memory/working/cli-release/release-plan.md" ".agents/memory/working/cli-release/task-foundation-aot-spike.md" ".agents/memory/working/checkpoints/cli-release.md" "docs/cli.md" "docs/development.md"
```

The public scenario runs the published production executable for `--help`,
`--version`, and an invalid token, then inspects exit status, stdout, and stderr.
Integrated inspection covers the actual Git diff, resolved package graph,
solution and physical topology, generated build outputs only under ignored
temporary paths, and absence of retained-command or legacy changes.

If behavior or evidence is incomplete, correction returns to the earliest
invalidated Gray, Red, or Green phase and repeats all downstream phases. A
Native AOT or dependency failure that materially invalidates an accepted
assumption stops the Task and returns to Architecture.

The Gate 5 activation record is commit `f3b74f2`, and the complete local
foundation cycle is commit `c867ec6`. The current acceptance change contains the
stable .NET 10 policy, local workflow, compact formatting policy, final evidence,
and Acceptance record. The branch has no remote upstream. Keep all work local
until the maintainer explicitly chooses to run CI. No package publication or
release occurs from this branch.

The frozen Gray signatures are:

```text
CliApplication.RunAsync(string[] args, CancellationToken cancellationToken = default) -> Task<int>
FoundationProbe.RunAsync(FoundationRequest request, CancellationToken cancellationToken = default) -> Task<FoundationResult>
FoundationRequest(string WorkspacePath, string Markdown, string Yaml)
FoundationResult = FoundationSucceeded(FoundationReport) | FoundationRejected(FoundationFailure)
FoundationReport(FoundationSnapshot Snapshot, string Json)
FoundationSnapshot(int SchemaVersion, int MarkdownHeadingCount, string YamlName, int FileByteCount, string FileSha256, bool ExclusiveLockObserved)
FoundationFailure(FoundationFailureKind Kind, string Message)
```

Gray created the `.slnx`, production project, central package versions, NuGet
source mapping, initial lock-file input, source-generated YAML and JSON contexts,
and explicit not-implemented callables. `dotnet restore OpenForge.slnx` and the
Release build completed with zero warnings and zero errors. No test or domain
behavior was added. The later stable .NET 10 policy supersedes that initial lock
input.

Red added four managed cases and four system cases. The Unit filter selected one
case and the Integration filter selected three; all four failed at
`FoundationProbe.RunAsync` with the explicit not-implemented exception. The
production Gray skeleton published successfully as a warning-free `win-x64`
Native AOT binary. Running the four system cases produced one direct probe not-
implemented failure and three native process exits caused by the not-implemented
composition root; only the three process cases used the published binary. Test
setup, discovery, traits, filtering, build, and Native AOT publication succeeded,
so the failures represent the missing accepted behavior rather than environment
or configuration defects.

Green made all eight initial frozen Red cases pass. The warning-free local Native
AOT production publish and managed system run passed. Blue then made the fixed
Markdig pipeline one explicit immutable configuration and separated the YAML and
JSON source-generation contexts without changing behavior. Purple made the
native process tests cancellation-responsive by requesting and awaiting child-
tree termination on cancellation. Focused managed and system evidence remained
green after each pass.

The public scenario executed the published `win-x64` binary directly. `--help`
printed the description, usage, help option, and version option; `--version`
printed exactly `0.0.0-dev`; and `unexpected` printed its parser diagnostic and
returned the fixed invalid exit `4`. The separately published Native AOT xUnit
system executable then ran all four system cases successfully; its three process
cases used that binary. This is local `win-x64` evidence only; the six native
workflow jobs have no accepted successful run.

Fresh review found that an empty YAML document could deserialize to `null` and
escape the typed invalid-input result. The one allowed correction cycle returned
to Red. Its ninth case reproduced the null path before Green added the focused
guard. Correction Blue centralized the six project RIDs while keeping the CI
runner mapping explicit. Correction Purple awaits child-process termination
after requesting cancellation cleanup. The local `win-x64` run passes all five
managed and four system cases and the public scenario. Correction rereview found
no blocking issue; the six native CI jobs remain the incomplete part of the full
gate.

A discarded remote attempt contributed no accepted evidence. It exposed that
SDK `10.0.111` supplies Native AOT compiler package `10.0.11`, while locks
created by SDK `10.0.101` require compiler package `10.0.1`. The maintainer
accepted stable .NET 10 compatibility across development PCs instead of one SDK
patch. The lock files and locked restore gate were therefore removed, while
central direct package versions remain exact. The workflow is local only and
uses major action tags rather than commit SHAs. It is configured for every push
and for pull requests targeting `develop` when one of the listed workflow paths
changes and the maintainer later chooses to run it.

The final local rerun used Windows `10.0.22631.6199`, .NET SDK `10.0.101`, and
the native `win-x64` RID. Restore, `dotnet format` verification, the warning-free
Release build, all five managed tests, both warning-free Native AOT publishes,
all four Native AOT system cases, and the direct help, version, and invalid-input
scenario passed. The dependency audit found no vulnerable direct or transitive
packages. The ten authored C# files contain no line over 200 characters; the
longest line is 191 characters. Every authored C# file is 120 lines or fewer.
Class size remains a separate review signal. WSL and macOS execution remain later
evidence and are not claimed here.

Residual risk after this Task includes WSL, macOS, the prepared six-RID native
workflow, and support-floor execution. Current hosted native runners can later
provide six-RID evidence on their available images, while exact Windows, macOS,
and glibc floor proof remains part of the complete Gate 5 release boundary.

Adding this Task affected the generated `Entries` in the CLI release program.
`open-forge-old index` was attempted after the Task record was added and before
executable-source mutation. It stopped at the preserved duplicate physical-
identity defect for `contracts/index/_index.md`. The matching Task entry is
present but has not been regenerated successfully. Do not treat Gate 4's older
index evidence as proof for this entry. Recheck the entry against this file's
path, description, and tags, and retain this limitation until replacement Index
evidence can own the projection.

## Completion

This Task is complete under the maintainer-directed local acceptance boundary.
The frozen contract and affected evidence agree; Windows restore, managed build,
trimming, Native AOT publication and execution, test identity and isolation, the
public scenario, focused review, and Mastermind final review pass. WSL, macOS,
the six native runner jobs, and support-floor execution remain explicit later
Gate 5 evidence rather than blockers to integrating this local foundation.
