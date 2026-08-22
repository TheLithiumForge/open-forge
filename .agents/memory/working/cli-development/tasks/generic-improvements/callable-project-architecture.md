---
open-forge:
  description: Close callable-input and root-host/Core architecture decisions before the behavior-neutral CLI refactor
  tags: [Memory, Working, CLI, Task, Generic, Callable, Architecture, Project, Refactoring, Contextual, Complete]
---

# Refine Callable And Project Architecture

## Task State

- State: Complete through accepted callable-refinement commit `cc2387d`.
- Responsible role: Mastermind.
- Parent: [Improve Generic CLI Structure](_generic-improvements.md).
- Task source: This file.
- Last updated: 2026-08-23.
- Branch: `feature/cli-generic-improvements`.
- Read-only Preflight: Complete from exact clean accepted predecessor `eb2e336`
  (`Accept active test architecture`).
- Current phase: Complete. The parent Final Full Gate and integrated review pass
  from clean `7871764` after exact accepted predecessor `cc2387d`.

## Problem Statement

At exact Preflight, several composition callables passed distinct stages through
long positional inputs. `RouteListBinding.Close` took seven inputs,
`CliCommandBinding<TRequest, TResult>` took eight constructor inputs, and
`RouteListOperationCoordinator` took six stage inputs. `RouteInspectBindingComponents`
also used a four-positional record even though its values formed a named composition
group. These shapes made the composition relationship harder to inspect and made
call-site ordering carry more meaning than the callables needed.

The root-host/Core project split also has real maintenance costs. Preflight evaluated
whether current dependency, Native AOT, test-boundary, and maintenance evidence
warranted changing that split and found no measured defect requiring it. The
accepted decision is to retain the root/Core split.

## Expected Outcome

The current callable composition uses small, property-only component groups at
their nearest owning boundaries. Long construction inputs become named and
cohesive without introducing a broad `Args` or `Context` object. Route List and
Route Inspect retain their direct binding and presentation evidence, and the
Route List operation retains its existing stage order and command-local scope.

The root/Core split remains unchanged. No public behavior, command contract, wire
shape, dependency graph, project boundary, Native AOT policy, test identity, or
test expectation changes.

## Relationships And Backlinks

| Relationship | Link | Relevance |
| --- | --- | --- |
| Parent Task | [Improve Generic CLI Structure](_generic-improvements.md) | This is child #3 in the accepted generic-improvements sequence, now complete before the parent final gate. |
| Predecessor | [Improve Active-Test Architecture](active-test-architecture.md) | Complete at production commit `e277227`; its accepted Working record is at exact clean predecessor `eb2e336`. |
| Earlier related Task | [Parser And Standard Behavior Remediation](parser-remediation.md) | Complete through `d445e20`; parser meaning and its accepted callable migration are outside this child. |
| Plan | [CLI Development Plan](../../plan.md) | Defines the generic-improvements integration order and the deferred parent gate. |
| Checkpoint | [CLI Development Checkpoint](../../../checkpoints/cli-development.md) | Records the parent final-gate boundary and resumption path. |
| Program Task | [Complete The Replacement CLI](../00-cli-development.md) | Keeps the parent program route aligned with the completed callable result and active parent gate. |

## References And Authority

| Source | Question it answers | Status or authority | May this Task change it? |
| --- | --- | --- | --- |
| [CLI Architecture](../../../../crystallized/documents/cli/architecture.md) | Which project, host, Core, dependency, composition, source-locality, test, and Native AOT boundaries must remain intact? | Accepted current architecture | No. |
| [CLI implementation Directive](../../../../../directives/open-forge/cli/implementation.md) | Which callable, composition, locality, dependency, and evidence rules apply? | Binding Directive | No. |
| [C# callable design](../../../../../directives/csharp/design.md) | When should a callable use a cohesive named input instead of positional plumbing? | Binding Directive | No. |
| [C# style](../../../../../directives/csharp/style.md) | Where do property-only models live and how must namespaces match paths? | Binding Directive | No. |
| [Source Locality](../../../../../directives/source-locality.md) | Which narrow scope should own the new component models and their consumers? | Binding Directive | No. |
| [Task Lifecycle](../../../../../workflows/development/task-lifecycle.md) | Which Task, phase, review, commit, and acceptance boundaries apply? | Applicable Workflow | No. |
| [Improve Generic CLI Structure](_generic-improvements.md) | Which parent outcome and final managed/Native AOT/package/vulnerability gates does this child inherit? | Active parent Task | Only its child state, progress, and backlink may be updated. |
| `eb2e336` (`Accept active test architecture`) | What exact clean source/test predecessor does this child inspect? | Accepted branch predecessor and read-only Preflight source | No. |

## Baseline And Preflight

Read-only Preflight ran from exact clean accepted predecessor `eb2e336` (`Accept
active test architecture`). It changed no source, tests, contracts, dependencies,
generated content, project files, or runtime behavior. This Working-only Task
packet must be committed before implementation. The one callable refinement starts
from that clean planning commit, and its non-Working paths must match exact
`eb2e336`. Until that planning commit exists, the execution text below is a plan,
not authorization to mutate production or test source.

The focused pre-refinement baseline was run against the unchanged `eb2e336` tree:

| Boundary | Selection | Result |
| --- | --- | --- |
| Unit | `BindingTests`, `RouteListDefinitionsSyntaxAndBindingTests`, `RouteListPresentationTests`, and `RouteInspectBindingAndCompositionTests` | `53/53`, zero skips |
| Integration | `RouteListApplicationIntegrationTests` and `RouteInspectApplicationIntegrationTests` | `49/49`, zero skips |
| Managed EndToEnd | Fresh current managed `win-x64` CLI publish and all current EndToEnd tests | `57/57`, zero skips |

Preflight established these decision-relevant facts:

- The target callable inputs exceed the three-parameter named-input threshold at
  Route List binding, the generic command binding, and the Route List operation
  coordinator. Route Inspect already demonstrates the accepted two-input
  `Close(symbols, components)` shape.
- Route List binding can derive its binder and contextual-invalid factory from
  `RouteListSymbols` inside `Close`, as Route Inspect already does. Direct
  `Bind` tests and generic delegate-injection tests remain the evidence boundary.
- The Route List coordinator has six stable command-local stages: inventory
  reader, selection resolver, topology builder, topology selector, coverage
  builder, and result builder. They form one construction group and do not justify
  an interface, service locator, dependency-injection framework, or wider shared
  component abstraction.
- The root project owns top-level `Program`, `CliHost`, the explicit concrete
  `CliCompositionRoot`, executable/AOT/runtime/version/generated `CliBuildVersion`
  concerns, and the published process boundary. Core owns Shell, Framework,
  commands, source-generated serialization, and the AOT-compatible/trimmable
  implementation. Unit references Core, Integration references Core and root,
  and EndToEnd invokes the published executable.
- The root/Core costs are bounded: one project edge, an Integration dual
  reference, friend seams, and explicit imports. No measured root/Core AOT, build,
  test-boundary, or maintenance defect earns collapse in this child.
- The private five-input `RouteInspectSourceSelectionResolver.ResolveSource` remains
  unchanged: four/five inputs are directive-normal here, and the helper
  centralizes one repack. Using its existing five-field input would duplicate
  caller construction or merely move positional plumbing, so it is not part of
  this callable migration.

## Accepted Project Decision And Options

Retain `src/cli/root/OpenForge.Cli` and
`src/cli/core/OpenForge.Cli.Core` as-is. The project split is not a defect to
remove for symmetry or parameter cleanup.

| Option | Evidence and tradeoff | Decision |
| --- | --- | --- |
| Retain the root host and Core split | Preserves the explicit executable/composition boundary, Core independence, distinct Integration and EndToEnd evidence boundaries, and current AOT/trimming ownership. The costs are bounded and measured evidence shows no failure. | Accepted. |
| Collapse the root host into Core or move root responsibilities into Core | Could remove a project edge and some friend/import seams, but would weaken the current executable, composition, publish/version, and test-boundary evidence without a concrete defect. | Not accepted in this child. |

No project, root/Core, namespace, IVT, solution, CI, or publish-path mutation is
authorized here beyond the existing root composition call-site migration required
by the callable changes. Reopen the project decision only for a concrete future
trigger: material root domain growth, a second executable or composition consumer,
a measured root/Core AOT or build failure/cost, or a fragile repeated
publish/version migration.

## Accepted Callable Scope

The following structural changes are closed and accepted. They preserve the
existing callable behavior and use named properties only where the values form one
construction boundary.

1. Add property-only `internal sealed class RouteListBindingComponents` at
   `Commands/Route/List/Models/Binding/RouteListBindingComponents.cs`. It has
   required-init named properties `Help`, `Operation`, and `Renderers`, plus the
   optional `DiagnosticRenderer` property.
2. Change `RouteListBinding.Close` from seven positional inputs to
   `(RouteListSymbols symbols, RouteListBindingComponents components)`. `Close`
   derives the binder and contextual-invalid result factory from `symbols` inside
   the method, matching Route Inspect. Preserve direct `Bind` evidence and generic
   delegate-injection evidence. Do not retain an old overload or forwarding
   method.
3. Convert the existing `RouteInspectBindingComponents` from its four-positional
   record to a property-only `internal sealed class` with the same required named
   properties and optional diagnostic property. Migrate root and Unit consumers to
   object initializers.
4. Add property-only `internal sealed class
   CliCommandBindingComponents<TRequest, TResult>` at
   `Shell/Composition/Models/CliCommandBindingComponents.cs`. Retain the existing
   `TResult : ICliCommandResult` constraint and use the required constraints for
   its delegate properties. Its required-init named properties are `Help`,
   `WorkspaceRequirement`, `Binder`, `InvalidResultFactory`, `Operation`, and
   `Renderers`; `DiagnosticRenderer` is optional.
5. Change `CliCommandBinding<TRequest, TResult>` from its eight-positional
   constructor to `(Command command,
   CliCommandBindingComponents<TRequest, TResult> components)`. Preserve null
   validation, workspace-requirement validation, all injected delegates, pipeline
   stage order, operation and renderer behavior, optional diagnostics, and the
   direct Unit authority. Do not retain an old overload or forwarding constructor.
6. Add property-only `internal sealed class RouteListOperationComponents` at
   `Commands/Route/List/Models/Operation/RouteListOperationComponents.cs`. It has
   exactly six required-init named coordinator stages: `InventoryReader`,
   `SelectionResolver`, `TopologyBuilder`, `TopologySelector`, `CoverageBuilder`,
   and `ResultBuilder`.
7. Change `RouteListOperationCoordinator` to accept only
   `RouteListOperationComponents`, and migrate its one factory construction. Keep
   the coordinator command-local. Do not add an interface, dependency-injection
   container, service locator, or general component framework.
8. Atomically migrate `CliCompositionRoot`, `RouteInspectBinding`, `RouteListBinding`,
   the one Route List operation factory construction, and the direct Unit
   consumers. Use object initializers for property-only component models. Change no
   test identity, expectation, fixture meaning, or evidence count.

## Scope

### Included

- The four property-only component models and the exact properties listed above.
- The `RouteListBinding.Close`, `RouteInspectBinding.Close`, and
  `CliCommandBinding<TRequest, TResult>` construction changes.
- The one named-input construction change for
  `RouteListOperationCoordinator` and its factory.
- Direct root composition and Unit call-site migration required to compile and
  preserve the accepted evidence. The direct Unit authority is:
  `BindingTests`, `RouteListDefinitionsSyntaxAndBindingTests`,
  `RouteListPresentationTests`, and `RouteInspectBindingAndCompositionTests`.
- Focused source, dependency, locality, project, and diff audits; fresh
  correctness and local-improvement reviews of the exact refinement diff and direct
  consumers.
- The active Working routing records needed to make this child active, keep the
  parent final gate deferred, and route resumption to this file.

### Excluded

- Any project/root/Core/namespace/IVT/solution/CI/publish-path mutation, except the
  root composition call-site migration required by the callable changes.
- Any public, wire, command, parser, test-architecture, package, dependency,
  generated, serialization, or Native AOT policy change.
- Any change to `CliCoreApplication.RunAsync`, pipeline/interface/process/host
  boundaries, `CliInvocationResolver.Resolve`, `CliCommandTree.Create`,
  recursive or domain helpers, or immutable domain/result/fact/serialization
  models.
- The private five-input `RouteInspectSourceSelectionResolver.ResolveSource`.
- Route List operation behavior, exception handling, cancellation, result
  formation, and stage order.
- New interfaces, dependency injection, service location, broad `Args` or
  `Context` bags, or a general component framework.
- Parser remediation, active-test architecture, the next product command, and the
  parent final complete managed Unit/Integration, Native AOT, package, and
  vulnerability gates.
- Any test identity, expectation, count, stream, status, no-write, cancellation,
  validation, fixture, or public scenario change.

### Allowed Implementation Paths

The callable-refinement source/test change is limited to these declarations, models,
call sites, and direct Unit consumers:

- `src/cli/core/OpenForge.Cli.Core/Commands/Route/List/Models/Binding/RouteListBindingComponents.cs`
  (new).
- `src/cli/core/OpenForge.Cli.Core/Commands/Route/List/RouteListBinding.cs`.
- `src/cli/core/OpenForge.Cli.Core/Commands/Route/Inspect/Models/Binding/RouteInspectBindingComponents.cs`.
- `src/cli/core/OpenForge.Cli.Core/Commands/Route/Inspect/RouteInspectBinding.cs`.
- `src/cli/core/OpenForge.Cli.Core/Shell/Composition/Models/CliCommandBindingComponents.cs`
  (new).
- `src/cli/core/OpenForge.Cli.Core/Shell/Composition/CliCommandBinding.cs`.
- `src/cli/core/OpenForge.Cli.Core/Commands/Route/List/Models/Operation/RouteListOperationComponents.cs`
  (new).
- `src/cli/core/OpenForge.Cli.Core/Commands/Route/List/RouteListOperationFactory.cs`.
- `src/cli/root/OpenForge.Cli/Composition/CliCompositionRoot.cs`.
- `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Shell/BindingTests.cs`.
- `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Route/List/RouteListDefinitionsSyntaxAndBindingTests.cs`.
- `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Route/List/RouteListPresentationTests.cs`.
- `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Route/Inspect/RouteInspectBindingAndCompositionTests.cs`.

No Integration, EndToEnd, contract, preserved-test, package, generated, project,
solution, CI, or Native AOT policy file is an implementation path for this child.

## Requirements

- **CALL-001:** The new component models are property-only internal sealed
  classes at the exact nearest `Models/` paths, with the exact required-init and
  optional named properties. They do not gain behavior, constructors, unrelated
  fields, or broad authority.
- **CALL-002:** Route List binding derives its binder and contextual-invalid
  factory from `RouteListSymbols` inside the new two-input `Close`, with no old
  overload or forwarder.
- **CALL-003:** The generic command binding receives one component model after its
  `Command`, preserves all existing validation and injected stage behavior, and
  exposes no old eight-input overload or forwarder.
- **CALL-004:** Route List operation construction receives exactly one component
  model containing the six existing stages. Execution behavior and stage order are
  unchanged.
- **CALL-005:** Root composition and direct Unit consumers use the named models and
  object initializers without changing test identities, expectations, or counts.
- **CALL-006:** The root/Core project decision remains retained and every project,
  dependency, IVT, solution, CI, publish, generated, package, and Native AOT
  policy audit remains unchanged.
- **CALL-007:** Direct `Bind` behavior, generic delegate injection, null and
  workspace validation, operation-at-most-once behavior, renderer selection,
  diagnostic behavior, cancellation, streams, status, and no-write boundaries
  remain covered by the existing evidence.

## One Callable-Architecture Refinement

This section records the single refinement implemented from `e54f2b1`; the execution
text below is now historical and does not authorize another mutation.

This is a routine behavior-neutral production-structure refactor. Gray, Red,
Green, Blue, and Purple do not apply. The maintainer direction arrived after the
ordinary behavior phases and authorizes this explicit Task-local refinement under
the Task Lifecycle. This is a new direction and does not consume an exceptional
correction cycle. Its exact baseline, allowed and protected surfaces, frozen
behavior and expectations, evidence, and separate commit are recorded here. The
refinement may atomically change internal production call surfaces and their
mechanical compile-only call sites in four Unit consumer files; it may not change any test identity,
fixture, assertion, expected value, or evidence boundary. After the Working-only
planning commit exists, perform one bounded callable-architecture refinement from
that clean boundary:

1. Recheck branch cleanliness, the exact planning commit, and the requirement that
   all non-Working paths match `eb2e336`. Re-audit direct consumers and stop if a
   hidden consumer or changed protected surface appears.
2. Add the three new property-only models, convert
   `RouteInspectBindingComponents`, and preserve the exact named property types,
   constraints, and nearest namespaces.
3. Change the three accepted construction boundaries and migrate the root and
   direct Unit call sites atomically. Derive Route List binder factories from
   symbols inside `Close`, use object initializers, and remove old positional
   overloads or forwarding forms.
4. Inspect the exact refinement diff, every direct consumer, the stage order,
   validation guards, project graph, root/Core ownership, namespaces, model locality, and
   excluded surfaces before evidence runs.
5. Run the focused evidence and source audits below. Obtain fresh correctness and
   local-improvement reviews of the exact refinement diff and direct consumers.
6. Accept the child only when every requirement and stop condition remains green.
   Update the parent, Plan, Checkpoint, and program Task with the truthful result;
   do not consume or claim the parent final gate.

The callable refinement may make only the listed mechanical direct Unit call-site
updates. It may not change an expectation, fixture, identity, or test boundary.

## Evidence And Acceptance Boundary

The pre-refinement counts above are the focused authority for this child. Run
commands from `src/cli/` unless noted. The Release build must be warning-free, formatting
must report no changes, and the focused test identities and counts must remain
unchanged with zero skips.

```text
dotnet build OpenForge.Cli.slnx --configuration Release --no-restore --nologo
dotnet format OpenForge.Cli.slnx --no-restore --verify-no-changes
dotnet test --project tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-progress --filter "FullyQualifiedName~BindingTests|FullyQualifiedName~RouteListDefinitionsSyntaxAndBindingTests|FullyQualifiedName~RouteListPresentationTests|FullyQualifiedName~RouteInspectBindingAndCompositionTests"
dotnet test --project tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-progress --filter "FullyQualifiedName~RouteListApplicationIntegrationTests|FullyQualifiedName~RouteInspectApplicationIntegrationTests"
dotnet publish root/OpenForge.Cli/OpenForge.Cli.csproj --configuration Release --no-restore --runtime win-x64 -p:PublishAot=false
OPEN_FORGE_CLI_PATH="$(pwd)/artifacts/publish/OpenForge.Cli/release_win-x64/OpenForge.Cli.exe" OPEN_FORGE_CLI_EXPECTED_VERSION="0.0.0-dev" dotnet test --project tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj --configuration Release --no-build --no-progress
```

From the repository root, run:

```text
git diff --check
```

Acceptance also requires these audits:

- The old seven-input Route List `Close`, old eight-input generic binding
  constructor, positional Route Inspect component record, and old
  Route List coordinator constructor are absent. No compatibility overload,
  forwarding constructor, or forwarding type remains.
- The four required models occur once at the exact paths above, are internal
  sealed property-only classes, expose only the accepted named properties, and
  match their physical namespaces. `RouteListOperationComponents` has exactly
  the six accepted coordinator stages.
- Direct `Bind` evidence and generic delegate-injection evidence remain direct
  Unit authorities. The focused Unit and Integration counts and identities remain
  `53/53` and `49/49`; the fresh managed published EndToEnd authority remains
  `57/57`; every result has zero skips.
- The private five-input Inspect source-selection helper, recursive/domain
  helpers, immutable data models, result and serialization models, and excluded
  shell/process/pipeline surfaces have no diff.
- Project references, root project files, IVT declarations, solution membership,
  CI files, publish paths, package versions, generated content, and Native AOT
  policy have no diff. The root/Core evidence still supports retaining the split.
- The production diff is limited to the listed callable declarations/models, their
  direct call sites, and the required root composition call site. Public, wire,
  generated, and package surfaces have no diff. No unrelated production or test
  surface changes.
- Fresh correctness review finds no behavior, validation, stream, status,
  cancellation, stage-order, evidence, dependency, locality, or architecture
  defect. Fresh local-improvement review finds no material broadening, duplicate
  plumbing, or named-input improvement within the closed scope.

Do not run or claim the parent final full Unit/Integration suite, local `win-x64`
Native AOT gate, package gate, or vulnerability gate in this child. The parent
generic-improvements Task owns that final boundary after every generic child is
complete.

## Stop Conditions

Stop the callable refinement and return to Preflight if any of the following
occurs:

- Any behavior, expectation, test identity, count, stream, status, no-write,
  cancellation, stage-order, validation, fixture, public scenario, or direct
  evidence defect appears.
- The change requires a production, package, generated, dependency, project, IVT,
  solution, CI, publish-path, root/Core, public, wire, parser, serialization, or
  Native AOT policy change outside the accepted call sites.
- A component model becomes a broad unrelated `Args` or `Context`, gains service
  lookup, or needs an interface, dependency injection, service locator, or
  general component framework.
- A hidden consumer exists outside the listed scope, an old overload/forwarder is
  needed for compatibility, or the direct consumer map is incomplete.
- The branch is dirty, the planning commit is not clean, or its non-Working paths
  do not match exact `eb2e336` before the callable refinement.
- The excluded five-input private Inspect helper, recursive/domain helper, or
  immutable data/result/fact/serialization model must change.
- Focused evidence changes identity or count, contains a skip, fails to reach the
  required direct Unit and Integration boundaries, or the fresh managed publish
  cannot drive all current EndToEnd cases.
- The root/Core retention decision can be supported only by an unmeasured
  convenience claim rather than the accepted dependency, AOT, test-boundary, and
  maintenance evidence. Reopen it only through one of the concrete future
  triggers recorded above.

## Progress And Evidence

- Current result: The one bounded Task-local callable-architecture refinement is
  accepted at `cc2387d` from clean planning boundary `e54f2b1`.
- Implementation evidence: The four property-only component models, three closed
  callable construction boundaries, root composition, and the four named Unit
  consumers now use the accepted named inputs. Route List Close derives its binder
  and contextual-invalid factory from symbols; Route List operation stage order and
  root/Core ownership remain unchanged.
- Post-refinement evidence: Warning-free Release solution build and format
  verification pass. Exact focused Unit is `53/53`, focused Integration is `49/49`,
  and a fresh managed `win-x64` non-AOT publish drives all current EndToEnd
  `57/57`, all with zero skips. Repository-root `git diff --check` passes.
- Audits: Old positional signatures, the Inspect positional record, compatibility
  overloads, and forwarding forms are absent. The four component classes and exact
  properties are present at their required paths. No Integration, EndToEnd source,
  project, dependency, IVT, solution, CI, package, generated, excluded helper/data,
  or Native AOT policy path changed; source changes are limited to the allowed
  production, root, and Unit paths plus this Progress/Evidence section.
- Review: Fresh correctness review found one Route List operation null-guard
  regression and stale phase wording. The refinement now restores the prior
  `ArgumentNullException` guard before delegate dereference and records truthful
  implemented state; the post-fix warning-free build, format, focused Unit `53/53`,
  and diff check pass. Correctness re-review passes. Fresh local-improvement review
  finds no material improvement within the frozen scope.
- Blockers: None. This child did not claim the parent full managed, local Native AOT,
  package, vulnerability, artifact, or integrated-review gates. The parent later
  ran those gates from clean `7871764`, and they pass.
- Next Task-level action: Run read-only [Find](../read-only/find.md) Preflight from
  exact `063c59d`; no next-product implementation is active.
- Completion is claimed at `cc2387d`; the focused evidence, audits, and fresh reviews
  pass with no unresolved material finding.

## Completion And Closeout

Complete this child only when the one callable-architecture refinement is accepted
from the Working-only planning boundary; the warning-free Release build, format check, and
`git diff --check` pass; focused Unit `53/53`, focused Integration `49/49`, and
fresh managed published EndToEnd `57/57` pass with zero skips; every required
source and project audit is clean; and fresh correctness and local-improvement
reviews pass on the exact diff and direct consumers.

At this child's completion, the parent, Plan, Checkpoint, and program Task recorded
the accepted callable result while the parent gates remained deferred. The parent
later ran those gates from clean `7871764`, and they pass. Do not alter crystallized
documents, contracts, Directives, generated content, or historical parser records. Keep this
Working record until the generic-improvements branch is integrated, then archive
or prune it with the other temporary phase detail when it no longer supports
resumption.

Completion status: Met at `cc2387d`. Root/Core remains retained without project
mutation. The parent final full managed, local `win-x64` Native AOT, package,
vulnerability, artifact, and integrated-review gate passes from clean `7871764`;
the accepted result is squash-integrated into `develop` at `063c59d` with exact tree
equality.
