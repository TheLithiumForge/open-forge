---
open-forge:
  description: Apply accepted truthful nullability, construction, and modern C# syntax rules across the complete replacement solution
  tags: [Memory, Working, CLI, Task, CSharp, Nullability, Initialization, Refactoring, Contextual, Complete]
---

# Modernize The C# Solution

## Task State

- State: Complete. The Preflight was accepted at exact commit `55eb82e`. The
  Framework modernization batch was accepted at exact commit `a90af59`; the
  Shell/root batch at `fe10525` (`Modernize Shell nullable flow`); the Route
  Inspect/family batch at `62a1dd9` (`Modernize Route Inspect nullable flow`); the
  Route List batch at `273eb45` (`Modernize Route List nullable flow`); and the
  Tests/support batch at exact clean source commit `6af5fb1` (`Modernize test
  support nullable flow`). All five ordered mutation batches are accepted, and no
  task-local correction pass was consumed. The final managed, Native AOT, package,
  audit, and public no-write gate passed from clean source commit `6af5fb1`. This
  Task is accepted at exact `a1cbf09`. Find Child 2's focused Preflight is accepted
  at `e24b9fe`; its strict Gray contract is accepted in the commit containing this
  record, and Red is next.
- Responsible role: Mastermind until the solution-wide migration, evidence, and
  acceptance boundaries close.
- Parent: [Complete The Replacement CLI](00-cli-development.md).
- Plan: [CLI Development Plan](../plan.md).
- Checkpoint: [CLI Development Checkpoint](../../checkpoints/cli-development.md).
- Accepted direction: The maintainer authorized a solution-wide improvement after
  the current Child 1 Task. That authorization is the Accepted direction for this
  Task and the Plan's ordered Child 1 → Modern C# → Child 2 sequence. The
  Mastermind implements that authorized continuation; no additional maintainer
  decision is needed. The [C# callable design Directive](../../../../directives/csharp/design.md)
  defines the accepted nullability, construction, and modern-syntax rules for new
  and materially changed C# now.
- Predecessor: Find Child 1 is Complete and accepted at exact commit `96fe413`.
  See the [Child 1
  acceptance record](read-only/find-source-catalogue.md) for the exact neutral
  authority, Route-local projections, evidence, and residual limitation.
- Baseline: The implementation baseline is exact clean Child 1 acceptance commit
  `96fe413` (`Accept Find source catalogue`) on `feature/cli-find`.
- Framework batch baseline: The batch began from the exact Preflight commit
  `55eb82e` on `feature/cli-find`.
- Shell/root batch baseline: The batch began from exact predecessor `a90af59` on
  `feature/cli-find`.
- Route Inspect/family batch baseline: The batch began from exact accepted Shell
  commit `fe10525` (`Modernize Shell nullable flow`) on `feature/cli-find`.
- Route List batch baseline: The batch began from exact accepted Route Inspect
  commit `62a1dd9` (`Modernize Route Inspect nullable flow`) on `feature/cli-find`.
- Isolation: Remain on `feature/cli-find` as an explicit exception to the normal
  exact-develop branch rule. Starting from `develop` at exact tip `e77902a`
  would omit accepted Child 1 from the starting tree. The continuation boundary
  is the ordered Child 1 → Modern C# → Child 2 sequence. Continue on
  `feature/cli-find` through Find Child 2; do not integrate into `develop` at this
  boundary. Integration remains at the authorized parent/integration boundary
  after that sequence. This Task makes no `develop` or six-RID claim.
- Current boundary: All five ordered modernization batches and the final managed,
  Native AOT, package, audit, and public no-write gate are Complete and accepted
  in the commit containing this record. The clean source predecessor is exact
  `6af5fb1`; Find Child 2's focused Preflight is accepted at `e24b9fe`, its strict
  Gray contract is accepted in the commit containing this record, and Red is next.

## Authority

| Source | Relevance to this Task |
| --- | --- |
| [CLI Architecture](../../../crystallized/documents/cli/architecture.md) | Defines the accepted source, project, dependency, serializer, Native AOT, and integration boundaries. |
| [C# callable design Directive](../../../../directives/csharp/design.md) | Defines truthful nullability, callable, construction, conditional, and collection rules. |
| [CLI Implementation Directive](../../../../directives/open-forge/cli/implementation.md) | Binds CLI source ownership, test tiers, parser, filesystem, generated, package, and AOT constraints. |
| [Test Evidence Integrity](../../../../directives/open-forge/testing/evidence-integrity.md) | Defines evidence isolation, test ownership, and acceptance integrity. |
| [Task Lifecycle](../../../../workflows/development/task-lifecycle.md) | Governs Preflight, the exact baseline/isolation exception, coherent commits, same-commit progress, review, correction, and acceptance. |
| [CLI Development Plan](../plan.md) | Defines the maintainer-authorized Child 1 → Modern C# → Child 2 sequence and integration boundary. |
| [Complete The Replacement CLI](00-cli-development.md) | Defines the parent outcome, authority, and program acceptance boundary. |
| [Accepted Child 1 predecessor](read-only/find-source-catalogue.md) | Defines the accepted Green behavior, evidence, and exact predecessor for this migration. |

## Decisions Needed

None.

## Current Step

This Task is Complete and accepted in the commit containing this record. Begin
Find Child 2, [Implement The Find Query Operation](read-only/find-query-operation.md),
with its focused Preflight from the accepted Modern C# boundary. Child 2 is
Planned and ready/unblocked; no Find query or presentation implementation exists,
and Child 3 remains after Child 2.

## Progress

### Framework modernization batch

- Result: Complete and accepted at exact predecessor `a90af59`. The batch began
  from exact Preflight commit `55eb82e`.
- Scope: Exactly 15 production files changed, all under
  `src/cli/core/OpenForge.Cli.Core/Framework/**`. No tests, projects, packages,
  generated files, or public contracts changed.
- Nullability and guard ledger:
  - All 48 baseline Framework postfix null suppressions were removed. The
    current Framework suppression audit is zero.
  - Twelve routine `ArgumentNullException.ThrowIfNull` calls were removed from
    trusted internal typed flow.
  - Forty-four guards were retained at uncertain parser, filesystem, model,
    state, and constructor boundaries. This is the Framework deliberate-retention
    ledger. No suppression was retained.
- Truthful contracts and accessors now cover the `SourceLogicalPath` canonical
  predicates, `SourceCatalogueReader.TryDequeue`, the
  `SourceLoaderDeclarationParser` and `SourceLoaderDestinationParser` `Try`
  outputs, the `PhysicalPathResolution` contained-path accessor, and local
  correlated failure readers. Non-null failure factory contracts remain
  unchanged.
- Catalogue, selection, reading, Loader, routing/topology, path/link/containment,
  cancellation, ordering, comparer, collection, exception, and allocation
  behavior remain unchanged.
- Verification passed: the Release build had zero warnings and errors. Format,
  diff/path, and suppression audits passed. Framework Unit is `313/313`, Framework
  Integration is `48/48`, focused Source/Route Unit is `562/562`, and focused
  Source/Route Integration is `183/183`, with zero skips.
- Fresh correctness review: PASS. Improvement review: PASS after centralizing
  contained-path use and reshaping `Decode` to truthful `TryDecode`. Impossible
  malformed states now fail explicitly, but accepted factories make those states
  unreachable.
- No task-local correction pass was consumed.

### Shell/root modernization batch

- Result: Complete and accepted at exact predecessor `fe10525`
  (`Modernize Shell nullable flow`). The batch began from exact predecessor
  `a90af59`.
- Scope: Exactly seven authored production files changed, all within the exact
  owned `Shell/**` and root-host `src/cli/root/OpenForge.Cli/**` paths:
  - `src/cli/core/OpenForge.Cli.Core/Shell/Composition/CliCoreApplication.cs`
  - `src/cli/core/OpenForge.Cli.Core/Shell/Definitions/CliSyntaxDefinitions.cs`
  - `src/cli/core/OpenForge.Cli.Core/Shell/Invocation/CliInvocationResolver.cs`
  - `src/cli/core/OpenForge.Cli.Core/Shell/Parsing/CliCommandTree.cs`
  - `src/cli/core/OpenForge.Cli.Core/Shell/Parsing/CliParser.cs`
  - `src/cli/core/OpenForge.Cli.Core/Shell/Parsing/CliRootDefinitionFactory.cs`
  - `src/cli/root/OpenForge.Cli/Composition/CliCompositionRoot.cs`
  No `Commands/**`, `Framework/**`, tests, projects, packages, generated files,
  JSON/YAML, or public contracts changed.
- Nullability and guard ledger:
  - Both baseline Shell/root postfix null suppressions were removed. The current
    owned suppression count is zero.
  - `ArgumentNullException.ThrowIfNull` calls fell from `75` to `64`: eleven
    trusted internal guards were removed and 64 were retained.
  - The other 17 null/whitespace guards were retained.
  - The retained argument guards are categorized as follows:

    | Retention category | Guards retained |
    | --- | ---: |
    | Root host | 2 |
    | Core boundary | 6 |
    | Binding/interface/delegate | 13 |
    | Pipeline/output | 18 |
    | Parser/System.CommandLine | 19 |
    | Syntax/help/data | 6 |

  - The ledger retains process, parser/library, interface/delegate, writer,
    stage, and frozen test boundaries.
- Truthful flow and preserved semantics: The generic finite-lookup suppression is
  replaced by truthful `[MaybeNullWhen(false)]`. `CliCoreApplication` uses
  correlated invocation flow and `parse.Tree` without duplicate tree state.
  Parser argument copies, collection/comparer/read-only semantics, process
  args/streams/cancellation, serializer/source-generation/AOT shape, and
  accepted public exception behavior remain unchanged.
- Verification passed: The Release build had zero warnings and errors. Format,
  analyzer, diff, and path audits passed. Shell/direct Unit is `84/84`,
  Hosting/Parsing/Serialization Integration is `47/47`, and managed published
  EndToEnd is `57/57`, with zero skips.
- Fresh correctness review: PASS. Improvement review: PASS after simplifying
  invocation narrowing and removing duplicate tree state.
- No task-local correction pass was consumed.

### Route Inspect/family modernization batch

- Result: Complete and accepted at exact commit `62a1dd9`
  (`Modernize Route Inspect nullable flow`). The accepted predecessor/baseline is
  exact Shell commit `fe10525` (`Modernize Shell nullable flow`).
- Scope: Exact production ownership is `Commands/Route/Shared/**`,
  `Commands/Route/Inspect/**`, and the exact `Commands/Route/RouteBinding.cs` and
  `Commands/Route/RouteDefinitions.cs` files. The two exact binding/definition
  files were unchanged. Exactly 32 production C# files changed. No tests,
  projects, packages, dependencies, generated files, or configuration changed.
- Scoped audits, using directory pathspecs, reduced ordinary postfix null
  suppressions from `50` to `0` and `ArgumentNullException.ThrowIfNull` from `113`
  to `83` (30 removed). After Framework, Shell/root, and Route Inspect/family,
  the global authored-production current counts are `26` suppressions and `327`
  `ArgumentNullException.ThrowIfNull` calls.
- Retained guards remain at metadata/YAML/parser/filesystem/physical-containment
  boundaries; binding/interface/delegate/writer/output/serializer ingress;
  collection/model/constructor/state invariants; cancellation; and protected
  direct exception boundaries. No unjustified scoped suppression remains.
- Truthful changes include `RouteInspectFact<T>`, `RouteInspectResolution`, and
  the internal parsed-model `RouteInspectGeneratedEntries` invariant accessors;
  `[NotNullWhen]` `TryParseEntry` flow;
  explicit metadata/frontmatter and projection state; no `string.Empty` identity
  fallback; cached immutable graph/identity; one state branch for JSON Boolean; a
  switch expression replacing the chained Axioms ternary; and non-null text
  escaping inputs.
- Parser/filesystem/cancellation/status/output byte shape, JSON/YAML/source-
  generation/AOT shape, collection/order/allocation/comparer semantics, and
  accepted public exception behavior remain unchanged.
- Verification after final local improvements passed `git diff --check`, format,
  the Release solution build with zero warnings/errors, and the informational
  `CA1062`/`CA1510`/`CA2264` analyzer with zero diagnostics. Unit Shared/Inspect
  passed `171/171`; Integration List/Inspect passed `159/159`; a fresh managed
  publish drove `PublishedRouteInspect` EndToEnd `31/31`; and every run had zero
  skips. This batch makes no Native AOT claim.
- Fresh correctness review: PASS. Improvement review: closed
  `NO_MATERIAL_IMPROVEMENTS` after its two local findings were applied. The
  initial build found the missing internal
  `RouteInspectGeneratedEntries.ReadReason` parsed-model accessor; it was added
  before acceptance and all evidence was rerun. No task-local correction pass was
  consumed.

### Route List modernization batch

- Result: Complete and accepted at exact commit `273eb45` (`Modernize Route List
  nullable flow`). The accepted predecessor/baseline is exact Route Inspect commit
  `62a1dd9` (`Modernize Route Inspect nullable flow`).
- Scope: Exactly `Commands/Route/List/**`; exactly 18 production C# files changed.
  No tests, projects, packages, dependencies, generated files, configuration, or
  product contracts changed. No output or test expectation changed.
- Scoped directory audits reduced ordinary postfix null suppressions from `26` to
  `0` and `ArgumentNullException.ThrowIfNull` from `136` to `106` (30 trusted
  internal duplicates removed). After all four production batches, global authored
  production counts are `0` ordinary postfix null suppressions and `297`
  `ArgumentNullException.ThrowIfNull` calls.
- The retained 106 guards cover System.CommandLine/parser, physical filesystem,
  read/writer/serializer ingress; request, operation, resolver, and topology stage
  boundaries; constructors, models, state factories, and invariants; cancellation;
  and frozen direct exception boundaries. An initial review found over-aggressive
  boundary removals. The guards were restored before acceptance and all evidence was
  rerun. No task-local correction pass was consumed.
- Truthful changes are limited to a private immutable `RouteListSelectionStage` for
  repeated ResolveId/ResolvePath context, one derivation of the attempted explicit
  reference, `RouteListDepth.FiniteValue` for finite-depth consumers while
  preserving the `<=`/`<` off-by-one behavior, filesystem `Entries` and failure-state
  accessors/patterns, complete metadata patterns, and non-null
  `RouteListTextEscaping` inputs.
- Loader/topology/depth, physical containment/alias/cycle/symlink, UTF-8/read,
  cancellation, status/finding/next-action/no-write, exact human/diagnostic/JSON
  output and source-generation/AOT shape, collection/order/allocation/comparer
  semantics, and accepted public/direct exception behavior remain unchanged.
- Verification after final corrections passed git diff/path checks, format, the
  Release build with zero warnings/errors, and `CA1062`/`CA1510`/`CA2264` with zero
  diagnostics. Route List Unit passed `117/117`; Integration passed `89/89`; a
  fresh managed publish drove `CliProcessTests` EndToEnd `26/26`; and every run had
  zero skips. This batch makes no Native AOT claim.
- Fresh correctness review: PASS after the guards were restored. Improvement review:
  `NO_MATERIAL_IMPROVEMENTS` after applying both local findings, single reference
  derivation and centralized `FiniteValue` use.

### Tests/support modernization batch

- Result: Complete and accepted at exact clean source commit `6af5fb1`
  (`Modernize test support nullable flow`). The exact predecessor is Route List
  commit `273eb45` (`Modernize Route List nullable flow`).
- Scope: Exactly 40 active test/support C# files changed: Unit 24, Integration 14,
  EndToEnd 1, and TestSupport 1. The exact active scope is
  `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/**`,
  `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/**`,
  `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/**`, and
  `src/cli/tests/support/OpenForge.Cli.TestSupport/**` only. Production, project,
  package, dependency, generated, configuration, and product-contract surfaces are
  unchanged. Test identities, expectations, tiers, order, fixtures, and count are
  unchanged.
- Nullability and guard ledger: active test postfix suppressions fell from `169` to
  `8`; the remaining eight are exactly the frozen intentional non-nullable
  injections already enumerated in this task ledger. Test
  `ArgumentNullException.ThrowIfNull` calls fell from `21` to `8`; the retained
  eight remain at `ProcessRunner`, `PublishedExecutableEnvironment`,
  `GeneratedLoaderDocumentBuilder`, and `TemporaryWorkspace` process, filesystem,
  interface, and fixture boundaries. Production remains at `0` suppressions and
  `297` guards.
- Truthful changes: xUnit typed assertions and pattern locals replace ordinary
  suppressions; the `SourceIdentity.DeriveId` null case remains without a
  suppression because the source accepts nullable input; `TemporaryWorkspace`
  symlink `Try` outputs use truthful `[NotNullWhen(true)]`; `ProcessRunner` timeout
  flow uses a proven local; and typed locals are reused. Raw unavailable and
  not-applicable checks and xUnit diagnostics remain intact. Production accessors
  are deliberately not substituted for `Assert.IsType`.
- Verification: diff/path and format checks pass. The Release build has zero
  warnings and errors, and `CA1062`/`CA1510`/`CA2264` have zero diagnostics. Full
  Unit is `617/617`, Integration is `241/241`, and a fresh managed publish drives
  full managed EndToEnd `57/57`; the final EndToEnd rerun is also `57/57`, with
  zero skips. Static audits pass for `500/500` Fact/Theory, DisplayName, Feature,
  and Evidence entries; argument assertions are `103/103`; Unit identities are
  `298` and Integration identities are `167`, unchanged. The production diff is
  zero. Initial compile found one missing `CliNextAction` using, which was fixed
  before acceptance and the evidence rerun. This batch makes no Native AOT claim.
- Reviews: Fresh correctness review is PASS. Improvement review is
  `NO_MATERIAL_IMPROVEMENTS / PASS` after typed-local reuse; accessor substitution
  is deferred for valid xUnit raw-shape diagnostics. No task-local correction pass
  was consumed.

### Next action

Begin Find Child 2's complete affected Unit and Integration Red matrix from the
accepted Gray commit containing this record. Its focused Preflight is accepted at
`e24b9fe`; Find Child 3 remains after Child 2. No Find query behavior, test, or
presentation implementation exists.

## Outcome

The complete `src/cli/` C# solution uses truthful nullable contracts and
warning-clean nullable flow for trusted internal values, explicit runtime
validation at uncertain boundaries, and modern C# syntax selected by fitness.
Shorter forms are used only when type, ownership, compatibility,
serialization/source generation, Native AOT, allocation, and collection semantics
remain clear. The improvement preserves every accepted public command behavior,
JSON shape, parser and filesystem contract, cancellation rule, status, finding,
next action, Native AOT boundary, and test expectation.

## Accepted Decisions And Findings

### Nullability

- Nullable reference analysis is compile-time only. Non-nullable declarations,
  `required`, `init`, nullable-analysis attributes, and `!` do not insert runtime
  checks.
- Internal values that remain inside warning-clean typed flow use accurate `T` or
  `T?` contracts instead of repetitive null guards. Possible-null diagnostics are
  fixed at their source rather than suppressed.
- Runtime validation remains at public, library, CLI-library, and other uncertain
  ingress boundaries, including values from parsers and libraries,
  deserialization, reflection, interop, `dynamic`, legacy, nullable-oblivious,
  and other externally supplied state. Validate once, then hand the trusted
  domain a proper non-nullable value.
- `required` expresses construction presence, not non-nullness or semantic
  validity. Truthful nullable-analysis attributes are preferred over `!`;
  intentional null tests may still use a narrowly scoped suppression.
- Preflight evaluates nullable warnings as errors and the applicable `CA1062`,
  `CA1510`, and `CA2264` analyzer settings before changing project policy. It does
  not mechanically delete guards or alter accepted public exception behavior.
- Inspect every production and test suppression and guard by boundary. Replace
  ordinary suppressions with compiler-proven flow, truthful nullable contracts or
  attributes, or narrow invariant accessors.
- Add truthful contracts where proven for `SourceLogicalPath` predicates, Loader
  and generated `Entries` `Try` methods, and `TemporaryWorkspace` symlink `Try`
  methods. Tighten Route List and Route Inspect text-escaping inputs from
  `string?` to `string` where every real caller is non-null.
- Preserve raw nullable state models, adding and using state-specific accessors
  where needed to express a proven state invariant.
- Retain runtime guards at process, parser/library, filesystem,
  serializer/deserializer, interface/delegate, JSON callback, writer,
  architecture-required pipeline-stage, genuinely nullable state-invariant, and
  frozen exception-test boundaries.
- Preserve all 103 argument-family assertions and exactly the intentional
  non-nullable null injections. `SourceIdentity.DeriveId(null)` remains a valid
  null-return case. Maintain a deliberate-retention ledger in progress rather
  than mechanically deleting all guards.

### Frozen Intentional-Null Ledger

Exactly eight intentional non-nullable null injections remain frozen for the
Tests/support modernization batch:

| Test method | Intentional injection(s) |
| --- | --- |
| `RouteListDefinitionsSyntaxAndBindingTests.RequestRequiresWorkspaceDepthAndNonEmptyOptionalSubject` | null workspace; null depth |
| `RouteListSelectionRowsAndProvenanceTests.SelectionsRetainAttemptedAndResolvedIdentity` | null resolved source |
| `RouteListSelectionFactoryTests.FactoryRejectsInvalidIdentityInputs` | null resolved ID source; null resolved path source |
| `BindingTests.BindingConstructorsRejectNullCallables` | null Binder; null Operation; null output writer |

The five frozen `ArgumentNullException` assertions are the two assertions in
`RouteListDefinitionsSyntaxAndBindingTests.RequestRequiresWorkspaceDepthAndNonEmptyOptionalSubject`
for null workspace and null depth, and the three assertions in
`BindingTests.BindingConstructorsRejectNullCallables` for null Binder, null
Operation, and null output writer.

`SourceIdentityListRegressionTests.DeriveIdRejectsUnusablePaths` currently uses
`null!`, but it is not a non-nullable injection because production accepts
`string?`. The Tests/support modernization batch removes only that suppression
and retains the null case. The repeatable 103 audit, unchanged test diff, and
unchanged test count freeze the broader argument-family set; this record does
not enumerate all 103 assertions.

### Construction And Syntax

- Use constructors or factories where values establish invariants, authorization,
  resource ownership, required dependencies, or atomic validity.
- Use named object initializers with `required init` for independent data-shaped
  members when encapsulation and compatibility remain sound for serialization,
  source generation, and Native AOT.
- Treat a long behavioral constructor as a cohesion signal. Do not create an
  options/property bag merely to hide dependencies or satisfy an arity target.
- Use primary constructors, target-typed `new()`, and collection expressions such
  as `[]` when they shorten code and keep type, storage, allocation, eager
  materialization, collection identity, overload, and mutation semantics clear.
  Keep explicit construction when those distinctions matter.
- `new { Prop = value }` creates an anonymous local projection. Reusable or public
  data uses a named type and named members.
- Only `RouteListSelectionResolver.ResolveId` and `ResolvePath` receive one narrow
  immutable stage input. Keep other callables over five parameters when they are
  data, invariant, or framework signatures whose separation remains clearer.
- Fix the one chained conditional expression. Use modern syntax only in materially
  changed files after accessibility, guard, storage, and collection semantics are
  proved; there is no standalone syntax sweep.
- Preserve JSON required-init and positional wire models, YAML mutable setters,
  serializer shapes, order, and null presence, component inputs, loading defaults,
  and concrete comparer, copy, reference, and read-only collection semantics.
- Do not add a presentation strategy table or perform large-class or locality
  extraction in this Task. Defer those changes because identical semantics and
  ownership are not sufficiently proved. Do not globally group test attributes;
  group them only when a substantive touched declaration remains readable and
  attribute order and identity are unchanged.

## Preflight And Scope

The read-only Preflight inspected the entire current solution before mutation.
Its authored C# change scope is:

- `src/cli/core`;
- `src/cli/root`; and
- `src/cli/tests/{unit,integration,end-to-end,support}`.

The Preflight authorizes no changes to `tests/preserved`, generated and build
output, projects, packages, dependencies, configuration policy, public behavior
and contracts, JSON, parser behavior, filesystem behavior, cancellation, status,
findings, next actions, or test expectations, counts, and identity.

The inventory from exact clean baseline
`96fe413d0e2e0fa92b0724a36b3f96bdb40ebb7e` records:

- 236 production C# files and 107 active test/support C# files;
- 380 production `ThrowIfNull` calls and 21 test `ThrowIfNull` calls;
- 126 production and 169 test postfix null suppressions;
- 97 production `required` declarations and 122 production `init` accessors;
- 36 production callables with more than five parameters;
- one chained conditional expression; and
- no nullable directives or pragmas, `SuppressMessage` entries, or
  nullable-analysis attributes.

All 500 authored tests retain `DisplayName`, `Feature`, and `Evidence`.

The pinned SDK and default project policy remain unchanged. Nullable analysis,
C#14, warnings-as-errors, the code-style build, AOT and trimming settings, and
reflection-free settings remain in force. From `src/cli/`, the exact
informational analyzer command executed was:

```bash
dotnet format OpenForge.Cli.slnx analyzers --no-restore --verify-no-changes --severity info --diagnostics CA1062 CA1510 CA2264 --verbosity diagnostic
```

It scanned all six projects, reported zero diagnostics, and formatted zero files.
No analyzer, `.editorconfig`, or project change is authorized.

The Preflight classified trusted internal flow separately from process, parser or
library, filesystem, serializer or deserializer, interface or delegate, JSON
callback, writer, reflection, and other uncertain ingress. It also classified
long constructors as invariant-bearing behavior surfaces, cohesive input
candidates, independent data shapes, or actual responsibility splits before
selecting syntax. No unresolved nullable, serializer, required-member, analyzer,
or collection-semantics issue remains for the accepted batches.

## Reproducible Preflight Audits

The following project paths are independently addressable from `src/cli/`:

| Project | Exact project path |
| --- | --- |
| Core | `core/OpenForge.Cli.Core/OpenForge.Cli.Core.csproj` |
| root | `root/OpenForge.Cli/OpenForge.Cli.csproj` |
| Unit | `tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj` |
| Integration | `tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj` |
| EndToEnd | `tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj` |
| TestSupport | `tests/support/OpenForge.Cli.TestSupport/OpenForge.Cli.TestSupport.csproj` |

From the repository root, baseline and isolation are reproducible with:

```bash
git status --short --branch
test "$(git branch --show-current)" = "feature/cli-find"
test "$(git rev-parse develop)" = "$(git rev-parse 'e77902a^{commit}')"
git merge-base --is-ancestor 96fe413d0e2e0fa92b0724a36b3f96bdb40ebb7e HEAD
# Pre-mutation Preflight acceptance check only:
git diff --quiet 96fe413d0e2e0fa92b0724a36b3f96bdb40ebb7e -- src/cli
git diff --check
```

The `git diff --quiet` baseline check applies only before the first mutation;
later accepted batch commits intentionally differ from the Preflight baseline.

The source audits use only the authored production and active test paths:

```bash
PRODUCTION_CSHARP=(
  'src/cli/core/**/*.cs'
  'src/cli/root/**/*.cs'
)
ACTIVE_TEST_CSHARP=(
  'src/cli/tests/unit/**/*.cs'
  'src/cli/tests/integration/**/*.cs'
  'src/cli/tests/end-to-end/**/*.cs'
  'src/cli/tests/support/**/*.cs'
)

git grep -n -E 'ArgumentNullException\.ThrowIfNull' -- "${PRODUCTION_CSHARP[@]}"
test "$(git grep -h -o -E 'ArgumentNullException\.ThrowIfNull' -- "${PRODUCTION_CSHARP[@]}" | wc -l)" -eq 380
git grep -n -E 'ArgumentNullException\.ThrowIfNull' -- "${ACTIVE_TEST_CSHARP[@]}"
test "$(git grep -h -o -E 'ArgumentNullException\.ThrowIfNull' -- "${ACTIVE_TEST_CSHARP[@]}" | wc -l)" -eq 21
git grep -n -P '[A-Za-z0-9_.)\]]+!' -- "${PRODUCTION_CSHARP[@]}"
test "$(git grep -h -P -o '[A-Za-z0-9_.)\]]+!' -- "${PRODUCTION_CSHARP[@]}" | wc -l)" -eq 126
git grep -n -P '[A-Za-z0-9_.)\]]+!' -- "${ACTIVE_TEST_CSHARP[@]}"
test "$(git grep -h -P -o '[A-Za-z0-9_.)\]]+!' -- "${ACTIVE_TEST_CSHARP[@]}" | wc -l)" -eq 169
if git grep -n -E '#[[:space:]]*(nullable|pragma)|SuppressMessage|\[[[:space:]]*(MaybeNull|MaybeNullWhen|NotNull|NotNullWhen|NotNullIfNotNull|MemberNotNull|MemberNotNullWhen|DisallowNull|AllowNull)' -- "${PRODUCTION_CSHARP[@]}" "${ACTIVE_TEST_CSHARP[@]}"; then exit 1; fi
git grep -n -E '\brequired\b[^;{=]*\{' -- "${PRODUCTION_CSHARP[@]}"
test "$(git grep -h -o -E '\brequired\b[^;{=]*\{' -- "${PRODUCTION_CSHARP[@]}" | wc -l)" -eq 97
git grep -n -E '\binit[[:space:]]*;' -- "${PRODUCTION_CSHARP[@]}"
test "$(git grep -h -o -E '\binit[[:space:]]*;' -- "${PRODUCTION_CSHARP[@]}" | wc -l)" -eq 122
```

`git grep` cannot calculate C# parameter lists. This Python audit prints the
production declarations with more than five parameters for review; the accepted
inventory contains 36 production callables over five parameters:

```bash
python - <<'PY'
from pathlib import Path
import re

roots = (Path("src/cli/core"), Path("src/cli/root"))
files = [path for root in roots for path in root.rglob("*.cs")]
literal_or_comment = re.compile(r'//[^\n]*|/\*.*?\*/|"(?:\\.|[^"\\])*"|@"(?:""|[^"])*"', re.S)
callable_name = re.compile(r'\b([A-Za-z_]\w*)\s*\(')
control_names = {"if", "for", "foreach", "while", "switch", "catch", "using", "lock", "nameof", "typeof", "sizeof", "when", "new"}

def blank(match):
    return "".join("\n" if char == "\n" else " " for char in match.group(0))

def parameter_count(parameters):
    depth = 0
    commas = 0
    for char in parameters:
        if char in "([{<":
            depth += 1
        elif char in ")]}>":
            depth = max(0, depth - 1)
        elif char == "," and depth == 0:
            commas += 1
    return 0 if not parameters.strip() else commas + 1

for path in files:
    source = literal_or_comment.sub(blank, path.read_text(encoding="utf-8"))
    for match in callable_name.finditer(source):
        name = match.group(1)
        if name in control_names:
            continue
        if not re.search(r"\b(?:public|private|protected|internal)\b", source[max(0, match.start() - 240):match.start()]):
            continue
        opening = match.end() - 1
        depth = 0
        closing = None
        for index in range(opening, len(source)):
            if source[index] == "(":
                depth += 1
            elif source[index] == ")":
                depth -= 1
                if depth == 0:
                    closing = index
                    break
        if closing is None:
            continue
        after = source[closing + 1:].lstrip()
        if not (after.startswith(("{", "=>", ";")) or after.startswith("where ")):
            continue
        count = parameter_count(source[opening + 1:closing])
        if count > 5:
            line = source.count("\n", 0, match.start()) + 1
            print(f"{path.as_posix()}:{line}:{name}:{count}")
PY
```

The exact test-identity and argument-assertion audits run from the repository
root are:

```bash
test "$(git grep -h -E '\[(Fact|Theory)' -- 'src/cli/tests/**/*.cs' ':(exclude)src/cli/tests/preserved/**' | wc -l)" -eq 500
test "$(git grep -h -E 'DisplayName[[:space:]]*=' -- 'src/cli/tests/**/*.cs' ':(exclude)src/cli/tests/preserved/**' | wc -l)" -eq 500
test "$(git grep -h -E 'Trait\(\"Feature\"' -- 'src/cli/tests/**/*.cs' ':(exclude)src/cli/tests/preserved/**' | wc -l)" -eq 500
test "$(git grep -h -E 'Trait\(\"Evidence\"' -- 'src/cli/tests/**/*.cs' ':(exclude)src/cli/tests/preserved/**' | wc -l)" -eq 500
test "$(git grep -h -o -E 'Assert\.(Throws|ThrowsAny)(Async)?<Argument(Null|OutOfRange)?Exception>' -- 'src/cli/tests/**/*.cs' ':(exclude)src/cli/tests/preserved/**' | wc -l)" -eq 103
```

## Protected Surfaces

- Do not change public command meaning, parser behavior, JSON, diagnostics,
  filesystem effects, cancellation, status, finding, next action, or test
  expectation to fit a style preference.
- Do not replace explicit construction when a concrete collection, identity,
  allocation, `Add`/indexer behavior, laziness, serializer contract, or ownership
  boundary matters.
- Do not remove runtime guards from uncertain ingress or add `required init`
  setters that weaken encapsulation.
- Do not change projects, packages, dependencies, generated code, configuration
  policy, or AOT shape without a separate accepted decision and evidence.

## Ordered Implementation Batches

This Task is an adaptive, behavior-neutral structural migration from an already
accepted behavior and evidence baseline, outside the strict Gray/Red/Green
feature cycle. It defines no new callable contract, behavior, or expectation.
Separate modernization batches preserve reviewable ownership and evidence
boundaries; manufacturing Gray, Red, or Green snapshots for this work would be
misleading. The Task Lifecycle still governs Preflight, the exact
baseline/isolation exception, coherent commits, same-commit progress, review,
correction, and acceptance. Those rules do not turn this structural Task into a
strict feature cycle.

Each mutating batch is separately reviewed and committed with its Task progress
in the same commit. Syntax changes stay within files already materially touched
by the batch. No batch receives an empty commit.

All Core paths below are relative to `src/cli/core/OpenForge.Cli.Core/` unless the
full repository path is shown.

| Batch | Exact owned paths and boundary |
| --- | --- |
| **Framework modernization batch** | `Framework/**` only; production nullability and guard cleanup. |
| **Shell/root modernization batch** | `Shell/**` plus exact root host `src/cli/root/OpenForge.Cli/**`; preserve ingress, stage, and public exception boundaries. |
| **Route Inspect/family modernization batch** | `Commands/Route/Shared/**`, `Commands/Route/Inspect/**`, and exact `Commands/Route/RouteBinding.cs` and `Commands/Route/RouteDefinitions.cs`; include the chained-conditional fix and state accessors. `Commands/Route/List/**` is explicitly excluded and reserved for the Route List modernization batch. |
| **Route List modernization batch** | `Commands/Route/List/**` only; include the cohesive selection input. |
| **Tests/support modernization batch** | `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/**`, `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/**`, `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/**`, and `src/cli/tests/support/OpenForge.Cli.TestSupport/**` only. |

No path is owned by two mutating batches.

## Focused Batch Commands

The following commands run from `src/cli/` after the common Release build and
format checks. The filters are test-tier filters; they do not replace Unit,
Integration, or EndToEnd project boundaries.

```bash
dotnet build OpenForge.Cli.slnx --configuration Release --no-restore --nologo
dotnet format OpenForge.Cli.slnx --no-restore --verify-no-changes
```

**Framework modernization batch** uses Framework-wide evidence followed by the established
Source/Route focused commands:

```bash
dotnet test --project tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-progress --filter "FullyQualifiedName~Framework|FullyQualifiedName~Filesystem"
dotnet test --project tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-progress --filter "FullyQualifiedName~Framework|FullyQualifiedName~Filesystem"
dotnet test --project tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-progress --filter "FullyQualifiedName~Framework.Sources|FullyQualifiedName~Commands.Route.Shared|FullyQualifiedName~Commands.Route.List|FullyQualifiedName~Commands.Route.Inspect"
dotnet test --project tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-progress --filter "FullyQualifiedName~Framework.Sources|FullyQualifiedName~Commands.Route.List|FullyQualifiedName~Commands.Route.Inspect"
```

These focused regressions remain Unit `562/562` and Integration `183/183`, with
zero skips.

**Shell/root modernization batch** uses Shell Unit, Hosting/Parsing/Serialization Integration,
and the established managed published EndToEnd command:

```bash
dotnet test --project tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-progress --filter "FullyQualifiedName~OpenForge.Cli.Core.UnitTests.Shell|FullyQualifiedName~RouteInspectBindingAndCompositionTests|FullyQualifiedName~RouteListPresentationTests|FullyQualifiedName~RouteTopology"
dotnet test --project tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-progress --filter "FullyQualifiedName~SystemCommandLineBehaviorTests|FullyQualifiedName~CliHostTests|FullyQualifiedName~GeneratedSerializationTests|FullyQualifiedName~RouteInspectGeneratedSerializationTests"
dotnet publish root/OpenForge.Cli/OpenForge.Cli.csproj --configuration Release --no-restore --runtime win-x64 -p:PublishAot=false
OPEN_FORGE_CLI_PATH="$(pwd)/artifacts/publish/OpenForge.Cli/release_win-x64/OpenForge.Cli.exe" OPEN_FORGE_CLI_EXPECTED_VERSION="0.0.0-dev" dotnet test --project tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj --configuration Release --no-build --no-progress
```

The managed published EndToEnd result is `57/57`, with zero skips.

**Route Inspect/family modernization batch** uses the shared/Inspect Unit filter, Route
List/Inspect Integration filter, and the route-inspect process filter:

```bash
dotnet test --project tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-progress --filter "FullyQualifiedName~Commands.Route.Shared|FullyQualifiedName~Commands.Route.Inspect"
dotnet test --project tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-progress --filter "FullyQualifiedName~Commands.Route.List|FullyQualifiedName~Commands.Route.Inspect"
OPEN_FORGE_CLI_PATH="$(pwd)/artifacts/publish/OpenForge.Cli/release_win-x64/OpenForge.Cli.exe" OPEN_FORGE_CLI_EXPECTED_VERSION="0.0.0-dev" dotnet test --project tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj --configuration Release --no-build --no-progress --filter "FullyQualifiedName~PublishedRouteInspect"
```

**Route List modernization batch** uses the Route List Unit/Integration filters and the Route
List process filter:

```bash
dotnet test --project tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-progress --filter "FullyQualifiedName~Commands.Route.List"
dotnet test --project tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-progress --filter "FullyQualifiedName~Commands.Route.List"
OPEN_FORGE_CLI_PATH="$(pwd)/artifacts/publish/OpenForge.Cli/release_win-x64/OpenForge.Cli.exe" OPEN_FORGE_CLI_EXPECTED_VERSION="0.0.0-dev" dotnet test --project tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj --configuration Release --no-build --no-progress --filter "FullyQualifiedName~OpenForge.Cli.EndToEndTests.CliProcessTests"
```

**Tests/support modernization batch** reruns the complete affected managed projects and the
same published managed EndToEnd command, then runs the static audits above:

```bash
dotnet test --project tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-progress
dotnet test --project tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-progress
OPEN_FORGE_CLI_PATH="$(pwd)/artifacts/publish/OpenForge.Cli/release_win-x64/OpenForge.Cli.exe" OPEN_FORGE_CLI_EXPECTED_VERSION="0.0.0-dev" dotnet test --project tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj --configuration Release --no-build --no-progress
```

## Beginning Evidence

Adopt the beginning evidence from the exact unchanged predecessor
`96fe413d0e2e0fa92b0724a36b3f96bdb40ebb7e`:

- Release build, format, and audits;
- focused Source/Route Unit `562/562` and Integration `183/183`;
- full Unit `617/617` and Integration `241/241`;
- managed published EndToEnd `57/57`;
- managed EndToEnd `57/57` against the native root;
- Native AOT Integration `241/241` and EndToEnd `57/57`;
- zero skips; and
- clear vulnerability and public no-write gates.

## Per-Batch Evidence

Every batch includes Release build, format, diff and path audits:

- Framework modernization batch adds Framework-focused Unit and Integration evidence and
  re-establishes the `562/562` and `183/183` Source/Route baseline.
- Shell/root modernization batch adds Shell Unit, Hosting/Parsing/Serialization Integration, and
  managed EndToEnd `57/57` evidence.
- Route Inspect/family modernization batch adds Shared/Inspect Unit, Route List/Inspect
  Integration, and route-inspect EndToEnd evidence.
- Route List modernization batch adds Route List Unit and Integration and route-list EndToEnd
  evidence.
- Tests/support modernization batch runs the complete affected managed projects and static audits proving
  500/500 authored tests retain `DisplayName`/`Feature`/`Evidence` and all 103
  argument assertions remain present.

Use project-tier boundaries for evidence selection. Traits are filters only; they
do not replace project tiers.

## Final Commands And Public No-Write Gate

The exact final commands run from `src/cli/` are:

```bash
dotnet build OpenForge.Cli.slnx --configuration Release --no-restore --nologo
dotnet format OpenForge.Cli.slnx --no-restore --verify-no-changes
dotnet format OpenForge.Cli.slnx analyzers --no-restore --verify-no-changes --severity info --diagnostics CA1062 CA1510 CA2264 --verbosity diagnostic
dotnet test --project tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-progress
dotnet test --project tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-progress
dotnet publish root/OpenForge.Cli/OpenForge.Cli.csproj --configuration Release --no-restore --runtime win-x64 -p:PublishAot=false
OPEN_FORGE_CLI_PATH="$(pwd)/artifacts/publish/OpenForge.Cli/release_win-x64/OpenForge.Cli.exe" OPEN_FORGE_CLI_EXPECTED_VERSION="0.0.0-dev" dotnet test --project tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj --configuration Release --no-build --no-progress
dotnet publish root/OpenForge.Cli/OpenForge.Cli.csproj --configuration Release --runtime win-x64 --no-restore --output artifacts/publish/win-x64/open-forge
OPEN_FORGE_CLI_PATH="$(pwd)/artifacts/publish/win-x64/open-forge/OpenForge.Cli.exe" OPEN_FORGE_CLI_EXPECTED_VERSION="0.0.0-dev" dotnet test --project tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj --configuration Release --no-build --no-progress
dotnet publish tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --runtime win-x64 --no-restore --output artifacts/publish/win-x64/integration
"./artifacts/publish/win-x64/integration/OpenForge.Cli.IntegrationTests.exe" --progress off
dotnet publish tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj --configuration Release --runtime win-x64 --no-restore --output artifacts/publish/win-x64/end-to-end
OPEN_FORGE_CLI_PATH="$(pwd)/artifacts/publish/win-x64/open-forge/OpenForge.Cli.exe" OPEN_FORGE_CLI_EXPECTED_VERSION="0.0.0-dev" "./artifacts/publish/win-x64/end-to-end/OpenForge.Cli.EndToEndTests.exe" --progress off
dotnet package list --project OpenForge.Cli.slnx --vulnerable --include-transitive --format json --no-restore
```

From the repository root, also run `git diff --check`, the exact changed-path
audit, and the source/dependency/path audits. The package command reports no
vulnerable package.

The public no-write fixture and per-invocation procedure are reproducible from
`src/cli/`. The fixture uses the accepted Loader and metadata bodies and keeps
all capture files in a separate temporary directory outside the workspace:

```bash
set -euo pipefail
WORK_ROOT="$(mktemp -d "${TMPDIR:-/tmp}/modern-csharp-work.XXXXXX")"
CAPTURE_ROOT="$(mktemp -d "${TMPDIR:-/tmp}/modern-csharp-capture.XXXXXX")"
OWNED_WORKSPACE="$WORK_ROOT/workspace"
trap 'rm -rf "$WORK_ROOT" "$CAPTURE_ROOT"' EXIT

python - "$OWNED_WORKSPACE" <<'PY'
from pathlib import Path
from textwrap import dedent
import sys

root = Path(sys.argv[1])
files = {
    ".agents/loader.md": dedent("""\
        # Open Forge Loader

        ## Entries

        <!-- open-forge:generated-index:start -->
        - [Root](root/_root.md) - #Root
        <!-- open-forge:generated-index:end -->
        """),
    ".agents/root/_root.md": dedent("""\
        ---
        open-forge:
          description: Root
          tags: [Root]
        ---
        # Root

        ## Entries

        <!-- open-forge:generated-index:start -->
        - none - No entries - #Empty
        <!-- open-forge:generated-index:end -->
        """),
    ".agents/root/collision.md": dedent("""\
        ---
        open-forge:
          description: Collision
          tags: [Route]
        ---
        # Collision
        """),
    ".agents/root/collision/_collision.md": dedent("""\
        ---
        open-forge:
          description: Collision entrypoint
          tags: [Route]
        ---
        # Collision entrypoint

        ## Entries

        <!-- open-forge:generated-index:start -->
        - none - No entries - #Empty
        <!-- open-forge:generated-index:end -->
        """),
    ".agents/root/_root.overwrite.md": "Root customization\n",
}

for relative_path, body in files.items():
    destination = root / relative_path
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(body.encode("utf-8"))
PY

snapshot_workspace() {
  python - "$1" <<'PY'
from pathlib import Path
import hashlib
import os
import sys

root = Path(sys.argv[1])
for entry in sorted(root.rglob("*"), key=lambda path: path.as_posix()):
    relative = entry.relative_to(root).as_posix()
    if entry.is_symlink():
        print(f"L\t{relative}\t{os.readlink(entry)}")
    elif entry.is_dir():
        print(f"D\t{relative}")
    elif entry.is_file():
        print(f"F\t{relative}\t{hashlib.sha256(entry.read_bytes()).hexdigest()}")
PY
}

run_public_case() {
  local label="$1"
  local executable="$2"
  local command_name="$3"
  local expected_exit="$4"
  local expected_status="$5"
  local before="$CAPTURE_ROOT/$label.before"
  local after="$CAPTURE_ROOT/$label.after"
  local stdout="$CAPTURE_ROOT/$label.stdout"
  local stderr="$CAPTURE_ROOT/$label.stderr"
  local actual_exit

  snapshot_workspace "$OWNED_WORKSPACE" > "$before"
  set +e
  case "$command_name" in
    "route list")
      "$executable" route list --workspace "$OWNED_WORKSPACE" --json > "$stdout" 2> "$stderr"
      actual_exit=$?
      ;;
    "route inspect")
      "$executable" route inspect root --workspace "$OWNED_WORKSPACE" --json > "$stdout" 2> "$stderr"
      actual_exit=$?
      ;;
    *)
      actual_exit=2
      ;;
  esac
  set -e

  test "$actual_exit" -eq "$expected_exit"
  test ! -s "$stderr"
  python - "$stdout" "$command_name" "$expected_status" <<'PY'
import json
from pathlib import Path
import sys

payload = json.loads(Path(sys.argv[1]).read_text(encoding="utf-8"))
assert payload["command"] == sys.argv[2]
assert payload["status"] == sys.argv[3]
PY
  snapshot_workspace "$OWNED_WORKSPACE" > "$after"
  cmp --silent "$before" "$after"
  rm -f "$before" "$after" "$stdout" "$stderr"
}

MANAGED_EXECUTABLE="$PWD/artifacts/publish/OpenForge.Cli/release_win-x64/OpenForge.Cli.exe"
NATIVE_EXECUTABLE="$PWD/artifacts/publish/win-x64/open-forge/OpenForge.Cli.exe"
run_public_case managed-list "$MANAGED_EXECUTABLE" "route list" 5 blocked
run_public_case managed-inspect "$MANAGED_EXECUTABLE" "route inspect" 3 incomplete
run_public_case native-list "$NATIVE_EXECUTABLE" "route list" 5 blocked
run_public_case native-inspect "$NATIVE_EXECUTABLE" "route inspect" 3 incomplete
rm -rf "$WORK_ROOT" "$CAPTURE_ROOT"
trap - EXIT
```

Each invocation takes a fresh recursive entry/hash snapshot, captures output
without aborting on the expected nonzero exit, checks the typed JSON command and
status, checks empty stderr, compares the post-snapshot, and removes its capture
files. The managed and native paths are the exact published paths above. The
combined fixture therefore remains List blocked with exit `5` and Inspect
incomplete with exit `3`, without workspace writes.

## Final Gate

Final acceptance requires:

- exact full managed counts Unit `617/617`, Integration `241/241`, and EndToEnd
  `57/57`; focused Unit `562/562` and Integration `183/183`; zero skips;
- warning, analyzer, format, and diff checks;
- serializer, source-generation, and reflection-disabled evidence;
- managed and local `win-x64` native-root/process evidence;
- Native AOT Integration `241/241` and EndToEnd `57/57`;
- both managed and native public no-write Route List and Route Inspect snapshots;
- package, dependency, artifact, and project audits; and
- fresh correctness and improvement review.

### Final Gate Result

The final gate passed from exact clean source commit `6af5fb1` (`Modernize test
support nullable flow`). The Release build was warning-free, format and diff
checks passed, and informational `CA1062`, `CA1510`, and `CA2264` diagnostics were
zero. Full managed Unit, Integration, and freshly managed-published EndToEnd
passed `617/617`, `241/241`, and `57/57`; focused Source/Route Unit and
Integration passed `562/562` and `183/183`. Every run had zero skips.

The local `win-x64` Native AOT root publication drove managed EndToEnd `57/57`
against the native root. The Native AOT Integration executable passed `241/241`,
and the Native AOT EndToEnd executable against the native root passed `57/57`,
with zero skips.

The public no-write fixture passed four invocations: managed and native Route List
returned exit `5` with `blocked`, and managed and native Route Inspect returned
exit `3` with `incomplete`. Each invocation had empty stderr, typed JSON
`command` and `status`, and unchanged byte/hash/entry snapshots.

The package audit listed all six projects and found no vulnerable transitive
package. Project, dependency, configuration, and generated surfaces remained
unchanged. The accepted dependency graph remains root→Core, Unit→Core,
Integration→Core+root+TestSupport, and EndToEnd→TestSupport. Core's
reflection-disabled JSON/source-generation/AOT settings remain unchanged. The
At this historical acceptance boundary, the workspace had one `.slnx`, six
projects, and 343 authored active C# files. No project-local `bin/obj`
directories existed, and ignored outputs remained under `src/cli/artifacts/`.
The later repository-root developer workflow moved the solution and output to
the repository root without changing the six-project graph.

The final static audits report production suppressions `126 → 0` and production
`ArgumentNullException.ThrowIfNull` `380 → 297`. Active test suppressions are
`169 → 8` frozen intentional injections, and test guards are `21 → 8`.
`required` remains `97`, `init` remains `122`, nullable-analysis attributes are
`14`, and active test identities remain `500/500` for `DisplayName`, `Feature`,
and `Evidence`, with `103` argument assertions. No forbidden nullable pragmas or
`SuppressMessage` entries exist.

The changed-path audit from accepted Child 1 `96fe413` is exactly 121 authorized
paths: 112 C# paths plus these nine Working records. No project, configuration,
dependency, or generated path changed. Fresh final integrated production
correctness review: `PASS`. Final test/evidence review: `PASS`. Final improvement
review: `NO_MATERIAL_IMPROVEMENTS/PASS`. Optional pre-existing shared projection
and failure-reader extraction is deferred because it is not a blocker and would
reopen accepted architecture or mutation batches.

The local native claim is `win-x64` only. This Task does not claim six-RID parity
or integration into `develop`. The branch remains `feature/cli-find`, `develop`
remains `e77902a`, and no push occurred.

## Stop Conditions And Correction Boundary

Stop if any behavior, test expectation, count, identity, or exception contract
drifts. Stop when a null boundary is uncertain and would require a suppression or
an untruthful attribute. Stop when collection concrete type, comparer, copy,
identity, laziness, allocation, or overload semantics are ambiguous. Stop for a
serializer, YAML, JSON, Native AOT shape, project, package, generated,
dependency, or configuration-policy change, or for a dirty-worktree or baseline
conflict.

After an accepted batch starts, only one exceptional task-local correction pass is
allowed. It starts at the earliest invalidated batch, reruns and reviews every
downstream batch, then reruns the complete final managed, Native AOT, public, and
package gates with fresh final reviews. Environment-only retries that change no
artifacts do not consume the pass. Any behavior, architecture, project, or
dependency change returns to the Mastermind and stops this Task.

## Deferred Deliberate Exceptions

- Serializer, YAML, and JSON construction shapes.
- Most long signatures.
- Broad primary-constructor and target-typed-`new()` churn.
- Presentation strategy tables.
- `SourceCatalogueReader` and large-class extractions.
- Six-RID parity.

## Evidence And Completion

The Preflight was accepted at exact commit `55eb82e`. The Framework modernization
batch was accepted at exact commit `a90af59`. The Shell/root modernization batch
was accepted at exact commit `fe10525` (`Modernize Shell nullable flow`); its
evidence and deliberate-retention ledger are recorded in Progress above. The Route
Inspect/family modernization batch was accepted at exact commit `62a1dd9`
(`Modernize Route Inspect nullable flow`). The Route List modernization batch was
accepted at exact commit `273eb45` (`Modernize Route List nullable flow`). The
Tests/support modernization batch was accepted at exact clean source commit
`6af5fb1` (`Modernize test support nullable flow`). Each non-empty batch records
its evidence and deliberate-retention ledger in the same commit as its batch
changes. No task-local correction pass was consumed.

The final gate and fresh reviews pass as recorded above. This Task is Complete and
accepted at exact `a1cbf09`. Find Child 2's focused Preflight is accepted at
`e24b9fe`, and its strict Gray contract is accepted in the commit containing its
current record, with Red next. Find Child 3 remains after Child 2. Keep the
replacement CLI non-shipping until its broader program and delivery gates pass.
