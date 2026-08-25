---
open-forge:
  description: Improve cross-cutting CLI parser, test, callable, and project structure without changing accepted command meaning
  tags: [Memory, Working, CLI, Task, Generic, Parser, Testing, Architecture, Contextual, Complete]
---

# Improve Generic CLI Structure

## Task State

- State: Complete; accepted feature tip `a107afe` is squash-integrated into `develop`
  at `063c59d` (`Improve and accept generic CLI structure`). The integrated tree is
  exactly equal to the accepted feature tree. The final implementation boundary is
  `cc2387d` (`Refine callable architecture`).
- Implementer: Mastermind. Feature-branch phase commits are pre-authorized after
  the applicable Working-only planning boundary; each phase still requires its
  evidence, stop conditions, and acceptance boundary.
- Parent: [Complete The Replacement CLI](../00-cli-development.md).
- Predecessor: Accepted [Route Inspect and Route Discovery](../route-discovery/route-inspect.md).
- Task source: This file.
- Last updated: 2026-08-23.

## Baseline

The branch started from exact `develop` commit `bd5d280` (`Implement and accept
route inspect`). Active-test acceptance is `eb2e336`; callable/project planning is
`e54f2b1`; and the accepted callable refinement is `cc2387d`. The complete parent
gate and integrated review pass from clean `7871764` after that implementation
boundary.

## Outcome

Improve proven cross-cutting structure in one focused branch without starting the
next product command. Parser and standard-behavior remediation, active-test
architecture, and callable/project architecture are complete. The accepted
root-host/Core split remains retained from dependency, AOT, test-boundary, and
maintenance evidence. Each stage preserves accepted command
contracts and receives its own bounded evidence and review.

## Authority

- [CLI Architecture](../../../../crystallized/documents/cli/architecture.md).
- [CLI implementation Directive](../../../../../directives/open-forge/cli/implementation.md).
- [C# callable design](../../../../../directives/csharp/design.md) and
  [C# style](../../../../../directives/csharp/style.md).
- [Task Lifecycle](../../../../../workflows/development/task-lifecycle.md).
- [CLI Development Plan](../../plan.md) and
  [Checkpoint](../../../checkpoints/cli-development.md).
- [Replacement CLI edge cases](../../edge-cases.md), especially CLI-EDGE-005 and
  CLI-EDGE-007.
- The maintainer-authorized sequence recorded in the completed
  [Route Inspect Task](../route-discovery/route-inspect.md#maintainer-authorized-post-completion-sequence).

## Analysis And Accepted Plan

1. [Remediate parser and standard behavior](parser-remediation.md) — Complete
   through Purple `d445e20`.
2. [Improve active-test architecture](active-test-architecture.md) — Complete at
   `e277227`; one exact cross-project Loader-document builder, EndToEnd-local process
   support, and nearest-scope capture fixtures preserve all selected evidence.
   Existing shared workspace/hash snapshots and distinct measurement/rich snapshots
   remain unchanged.
3. [Refine callable and project architecture](callable-project-architecture.md) —
   Complete at `cc2387d`; named component inputs are accepted and root/Core remains
   retained without project mutation.
4. Run one final complete-suite acceptance and review the integrated result —
   Complete from clean `7871764` after exact `cc2387d`; accepted feature tip
   `a107afe` is squash-integrated at `063c59d` with exact tree equality.

Later children receive their own accepted Preflight packets. Parser work must not
quietly perform their cleanup.

## Decisions Needed

None. All three children, feature-branch acceptance, integrated correctness review,
squash integration at `063c59d`, and accepted-tree verification are Complete.

## Evidence

### Beginning Full-Suite Baseline

Exact baseline `bd5d280` and the new branch initially had identical trees. Commands
ran from `src/cli/` on managed Windows x64 with .NET 10:

```bash
dotnet build OpenForge.Cli.slnx --configuration Release --no-restore --nologo
dotnet format OpenForge.Cli.slnx --no-restore --verify-no-changes
dotnet test --project tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-progress
dotnet test --project tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-progress
dotnet publish root/OpenForge.Cli/OpenForge.Cli.csproj --configuration Release --no-restore --runtime win-x64 -p:PublishAot=false
OPEN_FORGE_CLI_PATH="$(pwd)/artifacts/publish/OpenForge.Cli/release_win-x64/OpenForge.Cli.exe" OPEN_FORGE_CLI_EXPECTED_VERSION="0.0.0-dev" dotnet test --project tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj --configuration Release --no-build --no-progress
```

The warning-free build, format, and `git diff --check` pass. Unit is `549/549`,
Integration is `166/166`, and freshly published managed EndToEnd is `36/36`, all
with zero skip. This is the one complete beginning baseline; inner loops use only
authored and directly affected evidence until final branch acceptance.

### Final Full Gate

After the final generic production, test, fixture, composition, or configuration
change—including the parser child's post-Green callable migration—rerun the
managed commands above and the exact local Native AOT gate from `src/cli/`:

```bash
dotnet publish root/OpenForge.Cli/OpenForge.Cli.csproj --configuration Release --runtime win-x64 --no-restore --output artifacts/publish/win-x64/open-forge
OPEN_FORGE_CLI_PATH="$(pwd)/artifacts/publish/win-x64/open-forge/OpenForge.Cli.exe" OPEN_FORGE_CLI_EXPECTED_VERSION="0.0.0-dev" dotnet test --project tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj --configuration Release --no-build --no-progress
dotnet publish tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --runtime win-x64 --no-restore --output artifacts/publish/win-x64/integration
"./artifacts/publish/win-x64/integration/OpenForge.Cli.IntegrationTests.exe" --progress off
dotnet publish tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj --configuration Release --runtime win-x64 --no-restore --output artifacts/publish/win-x64/end-to-end
OPEN_FORGE_CLI_PATH="$(pwd)/artifacts/publish/win-x64/open-forge/OpenForge.Cli.exe" OPEN_FORGE_CLI_EXPECTED_VERSION="0.0.0-dev" "./artifacts/publish/win-x64/end-to-end/OpenForge.Cli.EndToEndTests.exe" --progress off
dotnet package list --project OpenForge.Cli.slnx --vulnerable --include-transitive --format json --no-restore
```

Final acceptance also repeats format and `git diff --check`, audits project-local
artifacts/dependencies, reviews the actual integrated diff, and records the same
explicit one-RID limitation as Route Discovery.

### Final Full Gate Result

The one deferred parent gate passes from clean `7871764`, after the final production,
test, fixture, composition, and configuration change:

- warning-free Release solution build, format verification, and `git diff --check`;
- managed Unit `580/580`, Integration `213/213`, and freshly published EndToEnd
  `57/57`, all with zero skips;
- local `win-x64` Native AOT root publication, with managed EndToEnd `57/57` against
  that native executable;
- local `win-x64` Native AOT Integration `213/213` and Native AOT EndToEnd `57/57`
  against the native root, all with zero skips;
- transitive vulnerability audit across all six projects with no vulnerable package
  reported;
- accepted project references (`root -> Core`, `Unit -> Core`, `Integration ->
  Core + root + TestSupport`, `EndToEnd -> TestSupport`), no project-local `bin/`
  or `obj/`, and, at this historical acceptance boundary, all ignored outputs
  centralized under `src/cli/artifacts/`; the later repository-root developer
  workflow moved those outputs to root `/artifacts/`;
- exact native terminal-conflict, ordinary option-like post-terminator Inspect, and
  attached-empty Route List depth public scenarios with the required exits, streams,
  and typed results; and
- fresh integrated correctness review of `bd5d280..7871764` passes with no
  implementation or evidence blocker. The local-improvement review requires only
  this final Working-record synchronization; it finds no accepted production change.
  `CliBindingParse` remains the explicit accepted parser boundary, and workspace/view
  facts are already read once and reused.

Local Native AOT evidence is explicitly one-RID (`win-x64`) evidence. It does not
claim six-RID parity; the existing later CI/release owner remains unchanged.

## Protected Boundaries

- Accepted command Interface/Behavior contracts and wire shapes.
- Route List and Route Inspect domain behavior, status, results, rendering, and
  public no-write guarantees.
- Dependencies, package versions, Native AOT policy, and generated serialization.
- Preserved/quarantined tests and unrelated product Tasks.
- Later child scope before its predecessor is accepted.

## Progress

- Development cycle: parent Task `Generic CLI Improvements`; exact-develop branch
  isolation and the complete beginning baseline are complete; parser-remediation
  Gray is accepted at `ddd2683`; corrected Red is accepted at `68455f6`; parser
  Green is accepted at `661b89b`; its post-Green callable migration is accepted at
  `d7056e8`; Blue is accepted at `f9f4dbe`; Purple is accepted at `d445e20`; parser
  remediation is Complete.
- Accepted child: [Improve Active-Test Architecture](active-test-architecture.md) is
  Complete at `e277227`, with its accepted Working record at `eb2e336`, focused
  Integration `129/129`, freshly published managed EndToEnd `57/57`, zero skips,
  warning-free build, format/diff checks, locality audits, and fresh
  correctness/local-improvement review.
- Accepted child: [Refine Callable And Project Architecture](callable-project-architecture.md)
  is Complete at `cc2387d` with focused Unit `53/53`, Integration `49/49`, freshly
  published managed EndToEnd `57/57`, zero skips, warning-free build, format/diff
  checks, source/project audits, and fresh correctness/local-improvement review.
- Final parent acceptance: The complete managed/native/package/artifact/public gate
  and integrated review pass from clean `7871764`; no blocker or unresolved material
  finding remains.
- Blockers: None.
- Integration: `develop` squash `063c59d` exactly matches accepted feature tip
  `a107afe`; no post-acceptance implementation difference exists.
- Next action: Begin read-only Preflight for [Find](../read-only/find.md) from exact
  `063c59d`; do not mutate the next product Task before its boundary closes.

## Completion

Complete. All three ordered improvement stages, the complete post-change gate,
independent integrated review, squash integration at `063c59d`, and exact
accepted-tree equality pass.

## Entries

<!-- open-forge:generated-index:start -->
- [Improve active-test locality and reuse without changing test meaning or CLI behavior](active-test-architecture.md) - #Memory #Working #CLI #Task #Generic #Testing #Locality #Architecture #Refactoring #Contextual #Complete
- [Close callable-input and root-host/Core architecture decisions before the behavior-neutral CLI refactor](callable-project-architecture.md) - #Memory #Working #CLI #Task #Generic #Callable #Architecture #Project #Refactoring #Contextual #Complete
- [Replace generic parser rescans and terminal bypass gaps with pinned library facts and typed validation](parser-remediation.md) - #Memory #Working #CLI #Task #Generic #Parser #SystemCommandLine #Evidence #Contextual #Complete
<!-- open-forge:generated-index:end -->
