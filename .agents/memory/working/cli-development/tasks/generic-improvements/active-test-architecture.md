---
open-forge:
  description: Improve active-test locality and reuse without changing test meaning or CLI behavior
  tags: [Memory, Working, CLI, Task, Generic, Testing, Locality, Architecture, Refactoring, Contextual, Complete]
---

# Improve Active-Test Architecture

## Task State

- State: Complete through accepted Purple commit `e277227`.
- Responsible role: Mastermind.
- Task source: This file.
- Last updated: 2026-08-22.
- Read-only Preflight: Complete from exact clean predecessor `a14f66d` (`Accept parser remediation`) on branch `feature/cli-generic-improvements`.
- Next phase: None in this child. [Refine Callable And Project Architecture](callable-project-architecture.md)
  was later completed at `cc2387d`; the parent Final Full Gate and integrated review
  now pass from clean `7871764`.

## Problem Statement

At exact parser acceptance, active test support crossed locality boundaries.
`TestSupport` contained published-process support that only EndToEnd consumed, six
byte-equivalent simple Loader writers were duplicated across Integration and
EndToEnd, and the Route List and Route Inspect application Integration tests each
contained the same `CliHost` capture helper. The Unit project also referenced
`TestSupport` without consuming a TestSupport symbol.

This is routine test-infrastructure refactoring. The issue is support placement and
duplication, not a production or test-expectation defect.

## Expected Outcome

Improve active-test locality and reuse without changing any test expectation,
fixture meaning, production behavior, contract, public result or wire shape,
production dependency or package, generated area, callable signature, or root-
host/Core ownership. The accepted Unit test-project reference removal is the only
dependency-graph change. The resulting support remains composable and static; it
does not become a base-class or generic utility layer.

## Relationships And Backlinks

| Relationship | Link | Relevance |
| --- | --- | --- |
| Parent Task | [Improve Generic CLI Structure](_generic-improvements.md) | Makes this child #2 in the accepted generic-improvements sequence. |
| Predecessor | [Parser And Standard Behavior Remediation](parser-remediation.md) | Complete through Purple `d445e20`; supplies the exact clean active baseline `a14f66d`. |
| Maintainer-authorized sequence | [Route Inspect sequence](../route-discovery/route-inspect.md#maintainer-authorized-post-completion-sequence) | Requires parser remediation before active-test architecture, then callable/project architecture. |
| Plan | [CLI Development Plan](../../plan.md) | Defines the generic-improvements integration gate and later callable/project stage. |
| Checkpoint | [CLI Development Checkpoint](../../../checkpoints/cli-development.md) | Records the current phase and resumption boundary. |
| Program Task | [Complete The Replacement CLI](../00-cli-development.md) | Keeps the parent program route aligned. |

## References And Authority

| Source | Question it answers | Status or authority | May this Task change it? |
| --- | --- | --- | --- |
| [CLI Architecture](../../../../crystallized/documents/cli/architecture.md) | Which project, root-host/Core, dependency, and evidence boundaries remain protected? | Accepted current architecture | No. |
| [CLI implementation Directive](../../../../../directives/open-forge/cli/implementation.md) | Which test-project, locality, process, and evidence rules apply? | Binding Directive | No. |
| [Test Evidence Integrity](../../../../../directives/open-forge/testing/evidence-integrity.md) | When may real test support be promoted or kept local? | Binding Directive | No. |
| [Evidence Tiers](../../../../../patterns/testing/evidence-tiers.md) | Which boundary must each selected test continue to prove? | Accepted Pattern | No. |
| [Nearest Shared Scope](../../../../../patterns/software/source-locality/nearest-shared-scope.md) | Where should support with identical real consumers live? | Accepted Pattern | No. |
| [Task Lifecycle](../../../../../workflows/development/task-lifecycle.md) | How does one bounded Purple phase reach review and acceptance? | Applicable Workflow | No. |
| [Improve Generic CLI Structure](_generic-improvements.md) | What parent outcome, sequence, and final gate does this child inherit? | Active parent Task | Only its child state, sequence/progress, and backlink may be updated; its outcome, authority, and final gate remain unchanged. |
| Read-only Preflight source audit at `a14f66d` | Which active consumers and exact duplicate implementations exist? | Direct predecessor evidence | No production or test meaning. |

## Baseline And Preflight

The frozen non-Working paths are from the exact clean `a14f66d` parser-acceptance
commit on `feature/cli-generic-improvements`. Parser remediation is Complete at
that boundary. This Task packet is committed before Purple; that clean planning
commit is the operational predecessor and may differ from `a14f66d` only in the
active Working records that authorize this child. The parent generic-improvements
full managed, local `win-x64` Native AOT, package, and vulnerability gates remain
deferred until every generic child is complete.

The read-only Preflight changed no source, tests, contracts, dependencies,
generated area, or runtime behavior. It established the following bounded facts:

- `TemporaryWorkspace` and `SnapshotHashes` remain in
  `OpenForge.Cli.TestSupport` because real Integration and EndToEnd consumers use
  them. They are not changed by this child.
- `ProcessRunner`, `ProcessRunRequest`, `ProcessRunResult`,
  `ProcessRunCanceledException`, and `PublishedExecutableEnvironment` have only
  EndToEnd consumers. Their current location in TestSupport contradicts the
  two-active-project locality rule. Moving them to EndToEnd-local support resolves
  that contradiction. The Unit project reference alone is not a real consumer.
- The identical Integration `CliHost` stdout/stderr/result capture helpers are
  `RunAsync` and `HostResult` in
  `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/List/RouteListApplicationIntegrationTests.cs`
  and
  `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Inspect/RouteInspectApplicationIntegrationTests.cs`.
- The identical EndToEnd published-process run/no-write helpers are
  `CliProcessTests.RunWithoutWritesAsync` and
  `PublishedRouteInspectProcessTestSupport.RunWithoutWritesAsync`.
- Exactly six simple generated `# Open Forge Loader` writers are byte-equivalent:
  `RouteListApplicationIntegrationTests.WriteLoader`,
  `RouteListSelectionIntegrationWorkspace.WriteLoader`,
  `RouteListTopologyIntegrationTests.WriteLoader`, and
  `RouteInspectResolutionIntegrationWorkspace.WriteLoader` under
  `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/`, plus
  `PublishedRouteWorkspace.WriteLoader` and
  `PublishedRouteInspectWorkspace.WriteLoader` under
  `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/`.
- Measurement helpers do not have identical meaning: synthetic strings differ
  from physical-path and byte measurements. The richer Route Inspect Profile
  snapshot also proves directory entries and inspection state beyond file hashes.

## Accepted Decisions

1. Add one narrow, composable, pure Loader-document builder to TestSupport. It
   accepts caller-supplied entries and preserves the exact bytes and whitespace
   of the six simple `# Open Forge Loader` generated `Entries` documents. Migrate
   only those six full writers. Do not absorb richer Profile loaders, entrypoints,
   metadata builders, invalid parser literals, or empty-entry policy.
2. Relocate `ProcessRunner`, its request/result/cancellation types, and
   `PublishedExecutableEnvironment` to EndToEnd-local support. Preserve timeout,
   stream, process-start, drain, cancellation, and result behavior. Delete the
   TestSupport definitions without forwarding or compatibility types.
3. Add one Integration-local composable `CliHost` stdout/stderr/result capture
   fixture and replace only the two exact duplicate application-test helpers.
   Preserve the current test cancellation token, separate writers, exit code,
   stdout, and stderr values.
4. Add one EndToEnd-local composable published-process run/no-write fixture and
   replace only the two duplicate helpers. Preserve timeout, current test
   cancellation, separate process streams, snapshot-hash comparison, and process
   cancellation semantics.
5. Remove the unused Unit-to-TestSupport project reference after a source audit
   confirms that no Unit source consumes a TestSupport symbol. Keep the
   Integration and EndToEnd project references.
6. Do not promote measurement helpers. Keep the richer Route Inspect Profile
   snapshot, command-specific workspaces, metadata builders, generated
   entrypoints, preserved tests, and intentionally malformed parser literals
   local and untouched.
7. Use static or composable utilities only. Do not add a base class, dependency,
   package, production/Core/root change, or root-host/Core ownership change.

## Scope

### Included

- The pure TestSupport Loader builder and the six exact full-writer migrations
  identified by Preflight.
- EndToEnd-local ownership for the published-process types and their direct
  consumers.
- The Integration-local `CliHost` capture fixture and the direct Route List and
  Route Inspect application consumers.
- The EndToEnd-local published-process run/no-write fixture and its direct
  `CliProcessTests` and `PublishedRouteInspectProcessTests` consumers.
- Removal of the Unit project reference after the required source audit.
- The active Working routing records needed to identify this child, its Purple
  boundary, its progress, and its completion.

### Excluded

- Production behavior, Core, root-host, command contracts, public results, wire
  shapes, production dependency or package changes, generated areas, crystallized
  documents, Directives, and preserved or quarantined test meaning.
- Any change to `TemporaryWorkspace` or `SnapshotHashes`.
- Any richer Loader/Profile builder, entrypoint or metadata builder, measurement
  helper, command-specific workspace, malformed parser literal, or unrelated test
  fixture.
- Parser remediation, long-signature cleanup, callable architecture, project
  architecture, and the parent final complete-suite or Native AOT gate.
- A full Unit or Integration suite, Native AOT, package, or vulnerability gate in
  this child.

### Constraints

- Purple may touch only `TestSupport`, the Unit project reference, directly
  migrated Integration/EndToEnd fixtures and consumers, and the allowed active
  Working records.
- The exact six simple Loader documents must remain byte-equivalent, including
  whitespace and final-newline behavior.
- Test identities, expectations, fixture meaning, no-write checks, measurement
  semantics, process cancellation, and selected evidence counts remain unchanged.
- Shared support must have real identical consumers at its nearest common scope.
  The Unit project reference does not establish a consumer.

## Requirements

- The six eligible Loader writers use one pure caller-supplied-entry builder, and
  no duplicate eligible implementation or non-identical helper is introduced.
- Process support is EndToEnd-local with no TestSupport forwarding types, while
  `TemporaryWorkspace` and `SnapshotHashes` remain shared and unchanged.
- The two application Integration tests use one composable capture fixture, and
  the two published-process EndToEnd surfaces use one composable no-write fixture.
- The Unit project has no TestSupport reference after source audit; Integration
  and EndToEnd retain their references.
- The Purple diff changes no test identity, expectation, fixture meaning,
  production behavior, contract, public shape, new dependency, package, generated
  area, callable signature, or root-host/Core ownership.

## One-Phase Execution

This section records the Purple migration completed at `e277227`; the execution,
evidence, and stop-condition text below is historical and does not authorize another
mutation in this child.

Gray, Red, Green, and Blue are not applicable. This is routine test-infrastructure
refactoring with no failing product behavior to invent. From the clean Working-only
planning commit whose non-Working paths match exact `a14f66d`, execute one bounded
Purple migration:

1. Recheck the clean branch and exact frozen non-Working predecessor before mutation.
2. Add and migrate the six eligible Loader writers, relocate the EndToEnd-only
   process support, add the two local capture fixtures, migrate only their direct
   consumers, and remove the Unit reference after the source audit.
3. Inspect the exact Purple diff and direct consumers. Run the bounded evidence
   below, then obtain fresh correctness and local-improvement reviews. Accept the
   child only when every boundary and stop condition remains green.

The active Working records may be updated as part of this bounded phase and its
acceptance. Do not regenerate or hand-edit generated `Entries` regions.

## Evidence Boundaries

Run commands from `src/cli/` unless noted. The Release solution build must include
the Unit project after its project-reference removal, but this child does not run
the complete Unit or Integration suites.

```text
dotnet restore OpenForge.Cli.slnx --nologo
dotnet build OpenForge.Cli.slnx --configuration Release --no-restore --nologo
dotnet format OpenForge.Cli.slnx --no-restore --verify-no-changes
dotnet test --project tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-progress --filter "FullyQualifiedName~RouteListApplicationIntegrationTests|FullyQualifiedName~RouteInspectApplicationIntegrationTests|FullyQualifiedName~LoaderDestinationResolverIntegrationTests|FullyQualifiedName~RouteListSelectionResolverIntegrationTests|FullyQualifiedName~RouteListTopologyIntegrationTests|FullyQualifiedName~RouteInspectResolver"
dotnet publish root/OpenForge.Cli/OpenForge.Cli.csproj --configuration Release --no-restore --runtime win-x64 -p:PublishAot=false
OPEN_FORGE_CLI_PATH="$(pwd)/artifacts/publish/OpenForge.Cli/release_win-x64/OpenForge.Cli.exe" OPEN_FORGE_CLI_EXPECTED_VERSION="0.0.0-dev" dotnet test --project tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj --configuration Release --no-build --no-progress
```

From the repository root, run:

```text
git diff --check
```

The pre-Purple focused baseline on the unchanged source/test tree passes selected
Integration `129/129` and managed EndToEnd `57/57`, both with zero skips. Acceptance
must preserve those selected identities and counts. The focused Integration
evidence covers Route List and Route Inspect application behavior plus the directly
affected Loader, topology, selection, and resolution consumers. All current
EndToEnd evidence is directly affected by the relocated process support; the fresh
managed publish therefore exercises all current process, Route List, and Route
Inspect journeys, including no-write and cancellation boundaries.

The source and dependency audit must prove that process types are EndToEnd-local,
the Unit project no longer references TestSupport, Integration and EndToEnd still
do, the Loader builder has real Integration and EndToEnd consumers, and no
duplicate eligible implementation or forwarding type remains. Reviews inspect the
exact Purple diff and every direct consumer. Do not run or claim the parent final
full managed Unit/Integration suite, local `win-x64` Native AOT gate, package
gate, or vulnerability gate in this child.

## Stop Conditions

- Stop and return to Preflight if the branch is dirty, its non-Working paths differ
  from exact `a14f66d` before Purple, or any unrelated change is present.
- Stop if any expectation, test identity, fixture meaning, production behavior,
  contract, public result/wire shape, production dependency, package, generated
  area, callable signature, or root-host/Core ownership would change beyond the
  accepted unused Unit test-project reference removal.
- Stop if a helper is not byte- or meaning-identical, if a richer Loader/Profile
  or measurement helper would be absorbed, or if no-write, physical-byte,
  snapshot, stream, timeout, or cancellation semantics would weaken.
- Stop if process support gains a real consumer outside the EndToEnd project, if
  the Unit source audit finds a TestSupport symbol consumer, or if a shared utility
  would need a base class or a broader scope. Return to Preflight rather than
  choosing a new architecture.
- Stop if the migration needs production, Core, root, contract, production
  dependency, new test dependency, package, generated, preserved-test, or later
  callable/project architecture changes.
- Stop if focused evidence changes counts or identities, skips a selected case, or
  cannot prove the affected Integration and managed EndToEnd boundaries.

## Progress And Evidence

- Current result: Purple is accepted at `e277227` from clean planning predecessor
  `5950651`. One shared `GeneratedLoaderDocumentBuilder` replaces the
  six eligible simple Loader bodies; process/environment support is EndToEnd-local;
  Integration and EndToEnd each own one composable capture fixture; and the unused
  Unit-to-TestSupport reference is removed. `TemporaryWorkspace`, `SnapshotHashes`,
  measurement/Profile support, production, contracts, packages, and test identities
  are unchanged.
- Purple evidence: The project-reference change required one bounded `dotnet restore`
  to refresh stale Integration assets; no package or version changed. The subsequent
  warning-free Release build and format verification pass. Focused Integration is
  unchanged at `129/129`, and a fresh managed publish drives all current EndToEnd at
  `57/57`, both with zero skips. `git diff --check` passes.
- Purple audits: The relocated process and environment sources are text-equivalent
  after only namespace/accessibility localization. Process definitions occur only in
  EndToEnd; Unit no longer references TestSupport; Integration and EndToEnd retain
  references; the Loader builder has exactly six Integration/EndToEnd call sites; and
  the protected workspace, measurement, and richer Profile snapshot files have no
  diff.
- Purple review: Fresh correctness review passes with no material behavior,
  integration, locality, evidence, or Task-claim finding. Fresh local-improvement
  review finds the diff cohesive and recommends no material change. The remaining
  untracked-file whitespace caveat was discharged by the staged diff check before
  commit.
- Current phase: Complete. The exact Purple commit is independently reviewed and
  satisfies the closed test-locality boundary.
- Blockers: None.
- Parent boundary: At this child's completion boundary, the parent final full
  managed, local `win-x64` Native AOT, package, vulnerability, artifact, and
  integrated-review gate remained deferred until all generic children were
  complete. This child did not claim that parent gate.
- Next Task-level action: Return to the accepted generic parent. Its final feature
  gate passes from clean `7871764`, and authorized squash-merge into `develop` is
  next.

## Completion And Closeout

Mark this child Complete only after the one Purple migration is inspected and
accepted, the exact affected Integration and fresh managed EndToEnd evidence pass
with zero skips and unchanged selected identities/counts, the warning-free Release
build, format verification, and `git diff --check` pass, and the source/dependency
audit proves the accepted locality and no-forwarding boundaries. Correctness and
local-improvement reviews must pass on the exact diff. At this child's completion,
the parent, Plan, Checkpoint, and program Task were updated while callable/project
architecture remained pending and the parent gates remained deferred. This child
did not claim a full Unit/Integration suite, Native AOT, package, or vulnerability
gate.

Completion status: Met at `e277227`. The accepted selected evidence, audits, and
reviews pass with no unresolved material finding; this child did not claim the
parent final gate.
