---
open-forge:
  description: Current top-down architecture for the greenfield C# replacement CLI, its source, projects, boundaries, evidence, and delivery sequence
  responsibility: Define the complete replacement CLI structure and the cross-cutting contracts that implementation Tasks must preserve
  tags: [Memory, Crystallized, Document, CurrentTruth, Evergreen, CLI, Architecture, Greenfield, DotNet, NativeAOT, Testing, Release]
---

# Replacement CLI Architecture

## Status And Authority

This document defines the accepted implementation architecture for the
non-shipping replacement CLI after the 2026-08-21 greenfield reset. The
[Command Contract Set](command-contract-set.md), [Shared CLI Operation
Contract](shared-operation-contract.md), and detailed
[command contracts](contracts/_contracts.md) define product behavior. This
Architecture defines how the implementation realizes those contracts.

The removed implementation remains historical evidence at Git commit `4b873de`.
Its [reset record](../../../archived/cli-release/implementation-reset-2026-08-21.md)
summarizes useful ideas and rejected boundaries. Historical source may inform a
Task, but it does not constrain class shape, source placement, or implementation.

The replacement remains non-shipping until every retained command, Native AOT
target, package, supply-chain control, support-floor journey, and release gate is
implemented and accepted.

## Architectural Goals

The implementation must:

- provide one predictable native executable for agents and occasional human use;
- keep the complete retained command surface in view without building speculative
  universal engines;
- establish process-wide and cross-command abstractions before command slices
  depend on them;
- keep command meaning, facts, plans, results, renderers, and evidence locally
  discoverable;
- make parser, filesystem, serialization, mutation, recovery, output, and process
  boundaries explicit and directly testable;
- remain deterministic, source-visible, reflection-free in product behavior, and
  compatible with trimming and Native AOT;
- use real operating-system filesystems and fail closed when required identity or
  containment cannot be proved; and
- let bounded implementers execute closed Tasks without inventing architecture.

The implementation must not add runtime plug-in discovery, dependency injection
for shell composition, a service locator, a fake filesystem, a universal command
result, a universal mutation engine, native interop, or a compatibility path to
`open-forge-old`.

## Physical Workspace

All replacement-specific C# workspace configuration, source, projects, and test
source live below `src/cli/`:

```text
src/cli/
  global.json
  NuGet.Config
  Directory.Build.props
  Directory.Packages.props
  OpenForge.Cli.slnx
  artifacts/

  root/
    OpenForge.Cli/
      OpenForge.Cli.csproj
      Program.cs
      Hosting/
      Composition/

  core/
    OpenForge.Cli.Core/
      OpenForge.Cli.Core.csproj
      Shell/
      Framework/
      Commands/
      Properties/

  tests/
    unit/
      OpenForge.Cli.Core.UnitTests/
    integration/
      OpenForge.Cli.IntegrationTests/
    end-to-end/
      OpenForge.Cli.EndToEndTests/
    support/
      OpenForge.Cli.TestSupport/
    preserved/
```

The repository root contains no replacement-specific solution, SDK selection,
NuGet configuration, central package version file, or MSBuild configuration.
`src/cli/artifacts/` contains all C# binary, intermediate, default publish, test,
and package output through the SDK artifacts layout. Projects do not create local
`bin/` or `obj/` folders.

The preserved test files currently below `src/cli/tests/` move to `preserved/`
before active projects are created. A Task adopts each relevant expectation into
the matching active test project. Preserved files never compile implicitly.

Projects use SDK default recursive authored-source inclusion. They do not list
ordinary C# files, use virtual solution folders, or link production source into
another project. Deterministic generated source is the only production compile
exception. The test-support library is an ordinary non-shipping project rather
than linked source.

## Project Graph

The solution has six direct project entries:

```text
OpenForge.Cli --------------------> OpenForge.Cli.Core

OpenForge.Cli.Core.UnitTests -----> OpenForge.Cli.Core
OpenForge.Cli.IntegrationTests ---> OpenForge.Cli.Core
                                \-> OpenForge.Cli
OpenForge.Cli.EndToEndTests ------> OpenForge.Cli.TestSupport
OpenForge.Cli.TestSupport --------> no production project

UnitTests, IntegrationTests ------> OpenForge.Cli.TestSupport when needed
```

`OpenForge.Cli` is the only production executable and publish root.
`OpenForge.Cli.Core` is one non-shipping class library containing the shell,
Framework-facing capabilities, and commands. The split establishes a hard host
boundary without turning capabilities into many assemblies.

The root project may depend on Core. Core never depends on the root project.
Tests depend only on the production and support projects required by their
evidence tier. End-to-end tests do not call production internals. Friend assembly
access is limited to named test projects and the root composition assembly when a
closed internal contract would otherwise need to become public solely because of
the project boundary.

No assembly in this repository is a supported third-party library API. Public C#
visibility is an implementation necessity, not a compatibility promise.

## Root Host Boundary

The root project owns only process and composition concerns:

- process arguments, environment, current directory, standard streams, and
  cancellation hookup;
- the explicit ordered command tree and concrete binding registration;
- construction of immutable shell services and command capabilities;
- invocation of one Core host boundary; and
- conversion of one process completion into the executable exit code.

`Program.cs` contains no command or domain behavior. `CliHost` owns one complete
process invocation. `CliCompositionRoot` visibly registers every root command,
group, and leaf in stable order. Adding a command changes this composition source
and the command's local source; it does not change a string dispatcher or runtime
registry.

The root host passes writers, environment facts, and cancellation explicitly. It
does not cache ambient console state in Core.

## Core Source Organization

Core is organized by bounded capability rather than by artifact type.

```text
OpenForge.Cli.Core/
  Shell/
    Composition/
    Definitions/
    Invocation/
    Parsing/
    Pipeline/
    Presentation/
    Output/
    Serialization/

  Framework/
    Workspace/
    Filesystem/
    Sources/
    Routing/
    Documents/
    GeneratedNavigation/
    Lifecycle/
    Mutation/
    Recovery/
    Extensions/

  Commands/
    Shared/
    Route/
      Shared/
      List/
        RouteListBinding.cs
        RouteListDefinitions.cs
        RouteListRequest.cs
        RouteListOperation.cs
        RouteListResult.cs
        Shared/<Capability>/
      Inspect/
        Models/<Topic>/
    Extension/
      Shared/
      List/
      Inspect/
    <RootLeaf>/
```

Only folders with actual cohesive source exist. The tree above is a placement map,
not authorization to create empty directories.

`Shell` contains process-wide CLI mechanics with no Framework-domain behavior.
`Framework` contains reusable facts and effect boundaries derived from accepted
Framework contracts. `Commands` contains operation meaning and projections.

At a command leaf, definitions, binding, and behavior-owning composition remain
visible at the leaf root. New records, interfaces, and property-only classes, and
existing models materially changed or promoted by the current Task, sit under
`Models/` within their nearest owning command or capability. Untouched accepted
models retain their paths. Once roughly five to ten models collect under one
owner, split them further by cohesive topic. Supporting behavior sits under
`Shared/<Capability>/` at the narrowest owning command, family, or cross-family
scope. A private `Shared` child marks the support boundary. Semantic behavior
moves to a wider parent only when another real consumer needs identical meaning.
A neutral mechanical foundation required by several accepted program outcomes is
implemented at their nearest shared scope before the first dependent slice.

Namespaces match physical folders. One command never imports another command's
private `Shared` namespace. No forwarding namespace preserves a removed layout.

## Dependency Direction

Dependencies flow inward from composition and commands toward stable facts and
effect boundaries:

```text
Root host
  -> Shell composition and pipeline
  -> Concrete command bindings
  -> Command operations and projections
  -> Framework facts and explicit effect boundaries
  -> BCL and accepted libraries
```

Shell types do not depend on concrete commands. Framework capabilities do not
depend on command requests, results, renderers, parser symbols, or process
writers. Commands may depend on Shell contracts and Framework capabilities.

Cross-command facts remain free of command-specific status, findings, output, and
next-action policy. A command translates shared facts into its own result.

## Shell Definitions And Composition

Typed definitions own every executable, command, argument, option, finite value,
machine code, result-command, and next-action identity exactly once.

Global definitions are split by responsibility:

- syntax and executable identity;
- presentation format, view, verbosity, and output targets;
- workspace selection;
- terminal modes and conflict policy;
- semantic statuses and process exits;
- process completion and output disposition; and
- parser and shell error identities.

Each command owns its group, leaf, operands, local options, finite values,
machine finding codes, result command name, and next-action contents.

The central composition model uses these ideal call surfaces:

```csharp
CliCommandBinding<TRequest, TResult>
  CommandDefinition
  Bind(ParseResult, CliInvocation) -> CliBindResult<TRequest, TResult>
  Execute(TRequest, CancellationToken) -> ValueTask<TResult>
  RendererSet<TResult>
  DiagnosticRenderer<TResult>?

ICliCommandBinding
  Command
  InvokeAsync(CliInvocationContext) -> ValueTask<CliProcessCompletion>

CliCommandTree
  RootCommand
  Ordered group and leaf bindings
  Exact Command-identity lookup
```

The generic binding closes one request/result pair. The non-generic boundary
stores heterogeneous bindings without erasing their concrete operation or
serialization types. Dispatch uses exact `System.CommandLine.Command` identity,
never strings.

No command binding locates services. The composition root supplies its complete
immutable dependencies through direct construction or small capability records.

## Parsing And Invocation

`System.CommandLine` exclusively owns command selection, arity, occurrence
aggregation, typed conversion, unknown symbols, parser diagnostics, standard
syntax help, and version action dispatch.

The shell flow is:

```text
arguments
  -> one System.CommandLine parse
  -> parser diagnostics
  -> bounded accepted delimiter validation, if still required
  -> global terminal conflict validation
  -> exact selected binding
  -> terminal help or version short-circuit
  -> normalized global invocation and optional workspace
  -> command-local request binding or concrete invalid result
```

The implementation does not rescan raw arguments for facts exposed by the parse
tree. A lexical guard may inspect only an exact recognized option and its attached
delimiter when an accepted syntax distinction cannot be obtained from the pinned
library. It does not parse values, count occurrences, select commands, or produce
parser diagnostics.

`CliInvocation` contains normalized process-wide facts only. A command request is
complete and immutable. It does not carry `ParseResult`, parser symbols, writers,
service collections, or an unrelated context bag.

Workspace-free commands carry genuine workspace absence. Workspace-aware
commands receive one selected and normalized `CliWorkspace` before domain work.

## Execution Pipeline

The shell uses immutable messages and directly callable stages:

```text
CliInvocationResolution
  -> CliOperationRequest<TRequest>
  -> CliOperationResult<TResult>
  -> CliPresentation<TResult>
  -> CliRenderedOutput
  -> CliOutputReceipt
  -> CliProcessCompletion
```

Each stage validates its own input before invoking an operation, renderer, or
writer. Direct stage entry is supported for tests and debugging. Unknown finite
values fail closed.

The pipeline:

- invokes one operation at most once;
- propagates caller cancellation to the operation;
- never reruns operation work during rendering or output;
- selects one cached concrete renderer;
- renders one primary result;
- optionally renders one bounded diagnostic projection;
- writes primary content once to its selected stream;
- writes diagnostics at most once to stderr; and
- returns one fixed process completion from the concrete semantic status.

`ICliCommandResult` exposes only shared process facts needed by the pipeline:
command identity, semantic status, workspace presence, and next-action presence.
Concrete result records retain complete command payloads and serialize through
concrete source-generated metadata. The interface is never a wire type.

## Result JSON Coordinates And Process Status

The Architecture defines the shared schema-v1 JSON envelope, source-location
primitive, and status/process coordinates. Each command contract set defines its
exact `result` object, finding codes, command-local finite values, and `next`
contents.

For a domain operation using JSON presentation, the top-level JSON envelope uses
camel-case members in exactly this order. Every member is present, including
members whose value is `null`:

```text
CliJsonEnvelopeV1 {
  schemaVersion: integer(1),
  command: exact command-owned machine identity,
  status: "complete" | "attention" | "incomplete" | "invalid" | "blocked" | "failed" | "interrupted",
  workspace: { path: string, selectedBy: "current-directory" | "explicit-workspace" } | null,
  result: command-owned object,
  next: { command: string, reason: string } | null
}
```

`schemaVersion` is the integer `1`. `command` is the exact machine identity
owned by the selected command. `workspace` is either `{ path, selectedBy }` or
`null`; `selectedBy` is exactly `current-directory` or
`explicit-workspace`. `result` is the command-owned object and is never `null`.
`next` is either `{ command, reason }` or `null`. `status`, `workspace`,
`command`, and `next` are not duplicated inside `result`.

For a domain operation using JSON presentation, JSON is one stdout document for
every status. Terminal help and version retain their shared text-only bypass.
Human primary output follows this exhaustive status table, and bounded
diagnostics remain on stderr:

| Status        | Process exit | Human primary stream |
| ------------- | ------------ | -------------------- |
| `complete`    | `0`          | stdout               |
| `attention`   | `2`          | stdout               |
| `incomplete`  | `3`          | stdout               |
| `invalid`     | `4`          | stderr               |
| `blocked`     | `5`          | stderr               |
| `failed`      | `1`          | stderr               |
| `interrupted` | `130`        | stderr               |

Architecture also defines one authored source-location primitive for command
results that expose an authored occurrence or span:

```text
SourceLocation {
  line: integer >= 1,
  column: integer >= 1,
  byteOffset: integer >= 0,
  byteLength: integer >= 0
}
```

`line` and `column` are 1-based Unicode-scalar positions. `byteOffset` is
0-based from the start of the exact UTF-8 physical layer, and `byteLength` is a
nonnegative UTF-8 byte count. The byte span is half-open:
`[byteOffset, byteOffset + byteLength)`. When a location applies to a public
fact, it is `null` only when it is unavailable and a typed finding explains
that unavailability. A field may also be `null` when location does not apply to
that fact. Parser-native spans are not exposed.

Shared schema-v1 compatibility includes the envelope field names and order, JSON
types, required-versus-`null` rules, source-location shape, and shared finite
values. A breaking change to those shared coordinates increments
`schemaVersion`. Architecture owns only the shared envelope, location, and
status coordinates. Each command contract set owns its exact result object,
finding codes, command-local finite values, `next` contents, and compatibility
rules. It may keep an additive optional command-local field in schema v1 only
when absence preserves prior meaning. References in command contracts to the
Architecture's exact shared JSON schema mean the shared coordinates; they do not
assign an unstated command-local result shape to Architecture.

## Presentation, Help, And Diagnostics

Every operation forms one concrete result before presentation. The JSON envelope
remains schema version 1 and contains `schemaVersion`, `command`, `status`,
`workspace`, `result`, and `next`. The seven semantic statuses retain one
exhaustive process-exit and primary-stream policy.

Human and JSON renderers are command-local because they project command meaning.
Shell presentation owns finite format selection, primary target selection,
diagnostic target, output messages, and process completion.

Standard help comes from the exact composed `System.CommandLine` symbol graph.
Bindings provide ordered product sections such as Discovery, examples, related
commands, bounded notes, and unavailable planned commands. The implementation
does not maintain a second command catalogue or normalize library output through
ad hoc string replacement.

Verbose diagnostics are bounded, escaped, and redacted command-local projections
of already-known facts. They never change operation status, rows, effects, primary
content, or exit. JSON stdout remains one valid document.

## Framework Capability Model

The complete command set demonstrates several shared capabilities before the
first command is implemented. Architecture establishes neutral mechanical
foundations at their nearest shared scope before the first dependent slice.
Other shared contracts may be established early, while semantic behavior is
implemented only when a consuming Task proves identical meaning.

### Workspace

Workspace selection resolves explicit and inferred subjects, records the exact
selection method, normalizes identity once, and never invents a fallback fact.
Terminal modes bypass workspace selection. Workspace-free commands preserve null
workspace in concrete results.

### Filesystem And Physical Identity

Filesystem code uses real `System.IO` and typed outcomes. Path strings,
normalized lexical paths, physical identities, and resolved link targets are
separate facts.

`PhysicalPathResolver` walks one existing component at a time from a proven root.
For every component it:

1. inspects the component without enumerating descendants;
2. classifies ordinary, missing, inaccessible, dangling, or reparse/link state;
3. resolves one link target;
4. proves containment immediately after that resolution;
5. records physical identity for cycle and alias detection; and
6. continues only from the proven contained result.

A path that leaves the root and later re-enters is blocked at the first external
transition. Final-target containment is insufficient. If managed BCL evidence
cannot prove the accepted guarantee on a target platform, implementation stops at
Architecture rather than adding P/Invoke or weakening the contract.

Typed reads distinguish complete, missing, invalid encoding or syntax, access
denied, and I/O failure. They preserve bounded direct causes without leaking
sensitive content.

### Sources, Routing, And Documents

Source references use one shared grammar and typed identity model. Commands
retain attempted identity separately from resolved identity.

The source catalogue and route graph expose immutable facts only: canonical
source identity, recognized entrypoint form, Loader root, route chain,
overwrites, loading behavior, and safe topology. A Framework scope is authored
meaning, not a mechanically identifiable path segment; a consumer reports scope
only when an applicable component contract supplies explicit evidence and never
infers it from route shape. List, inspect, context, find, index, diagnostics, and
mutations translate the shared facts into command-local meaning.

Markdown capabilities use one fixed CommonMark pipeline only when the first real
consumer requires body parsing. One neutral Markdown frontmatter parser owns
exact delimiter and body boundaries without requiring body parsing. One neutral
YAML syntax parser owns the source-preserving node shape, scalar spans, alias and
unsupported-mapping facts required by Find, Route discovery, and later Route
mutation. Semantic metadata uses one CLI-root generated YAML context and small
models; command interpretation, policy, findings, and status remain local.
Generated navigation remains a projection of routed sources, never an independent
authority.

### Lifecycle, Mutation, And Recovery

Read-only commands never create locks, lifecycle files, caches, indexes, or
recovery artifacts.

Mutation commands follow this visible shape:

```text
resolve and inspect
  -> form a command-local immutable plan
  -> validate policy and collisions
  -> acquire the real workspace lock when applicable
  -> revalidate expected state
  -> apply bounded filesystem changes
  -> verify resulting identity and bytes
  -> write accepted lifecycle or recovery state
  -> form one concrete result
```

Shared mutation support provides file preconditions, atomic replacement,
workspace locking, expected-state revalidation, Git or recovery primitives, and
receipts. Each command owns its plan, effect ordering, rollback or compensation
meaning, findings, and result. No generic engine decides product behavior.

Lifecycle state remains `.agents/open-forge.lifecycle.json`, schema version 1.
The mutation lock remains `.agents/open-forge.lock`. Existing legacy lifecycle
formats are ordinary untouched content.

## Serialization And Dependencies

JSON uses `System.Text.Json` source generation with reflection disabled. YAML uses
one source-generated static context for accepted Framework metadata shapes.
Concrete command contexts register concrete result graphs. AOT evidence exercises
every registered shape.

Direct package versions are pinned centrally below `src/cli/`:

- `System.CommandLine` 2.0.11;
- `YamlDotNet` and its accepted static generator 18.1.0;
- `Markdig` 1.3.2 only after a retained body consumer exists; and
- xUnit v3 Microsoft Testing Platform packages 4.0.0.

Every dependency must earn Native AOT, trimming, maintenance, security, and
binary-size cost. A later Task may update an exact version only through an
explicit dependency decision and complete evidence.

## Test Architecture

The active test projects have distinct evidence boundaries:

- Unit tests cover pure values, definitions, parsers, binders, ordering, result
  formation, renderers, serialization contracts, and directly callable shell
  stages without claiming real filesystem or process behavior.
- Integration tests call production modules with owned real temporary filesystems
  and cover source generation, physical identity, locking, Git, runtime, and
  Native AOT internal boundaries.
- End-to-end tests invoke the published executable and prove arguments, streams,
  statuses, exits, cancellation, unchanged bytes, and public scenarios.
- TestSupport contains cohesive real-OS workspace and process fixtures shared by
  at least two active projects. Command-specific builders remain in their nearest
  test scope.

Every test has an explicit display name, one durable feature trait, and one
evidence trait. Traits refine selection and never replace project separation.

Each test owns every mutable workspace, home, temporary directory, Git repository,
cache, and process it can affect. Parallel tests share no mutable state. Snapshots
cover stable projections only; safety, identity, effects, and status remain direct
assertions.

Preserved tests are an evidence inventory. The owning Task maps each case to a
current contract, observes the required failure against the new boundary, and
then ports the expectation. No Task bulk-copies old test plumbing.

## Build, Native AOT, CI, And Artifacts

The CLI uses stable .NET 10 with C# 14, nullable analysis, warnings as errors,
deterministic builds, package auditing, and no prerelease SDK. `global.json`
allows compatible stable feature-band roll-forward.

The six production RIDs remain:

- `win-x64`
- `win-arm64`
- `linux-x64`
- `linux-arm64`
- `osx-x64`
- `osx-arm64`

Foundation acceptance requires managed build and tests plus at least local
`win-x64` Native AOT publication and execution. Command Tasks repeat the affected
managed and local Native AOT evidence. Final delivery runs all six RIDs on their
native runners and support floors.

The repository CI workflow may live under `.github/workflows/`, but every C# path
and command it invokes starts below `src/cli/`. CI uploads bounded artifacts; it
does not make `.github/` a C# source root.

## Durable Implementation Sequence

Implementation proceeds from cross-cutting foundation to read-only facts, then
mutations and aggregate diagnosis:

1. C# workspace, project graph, artifacts, dependencies, and active test roots.
2. Core shell contracts, root composition, parser, invocation, pipeline, output,
   help boundary, serialization, and Native AOT host proof with no command.
3. Shared workspace and physical-filesystem safety foundations.
   Shared document foundations include neutral Markdown frontmatter boundaries
   and YAML syntax facts before command-local metadata interpretation.
4. `route list` as the first complete read-only command.
5. `route inspect`, promoting only identical route facts proved by the second
   consumer.
6. `find`, `references`, and `context` on shared source and document facts.
7. `extension list` and `extension inspect` on shared extension-source facts.
8. `index` after source, route, document, and generated-navigation facts exist.
9. Shared mutation, lock, lifecycle, recovery, and Git foundations.
10. `route init`, `route create`, `route update`, `route move`, and `route remove`.
11. `extension create`, root `install`, root `update`, `extension install`,
    `extension update`, and `extension remove`.
12. `status`, `doctor`, `repair`, and `cleanup` after all producers and recovery
    states exist.
13. Thin npm wrappers, package evidence, six-RID CI, supply-chain evidence,
    support-floor execution, documentation, and release.

Each command reaches complete contract, managed, process, unchanged-state, and
Native AOT acceptance before the next command consumes or promotes its facts.

## Planning, Tasks, And Delegation

The active top-down Plan defines the work graph. Every implementation file belongs
to one hierarchical Task with explicit architecture references, predecessor
outputs, accepted classes or algorithms, allowed paths, protected boundaries,
tests, verification, integration, and stop conditions.

The Mastermind implements architectural foundations and cross-cutting callable
contracts directly. Advisors may challenge a named unresolved boundary. Smaller
implementers receive only closed behavior or mechanical Tasks after their
foundation exists. A Task that exposes an unresolved system choice returns to the
Mastermind before code continues.

Local passing tests do not accept a change that violates this Architecture.
Acceptance combines focused evidence, direct diff inspection, dependency and
locality checks, affected integration, Native AOT proof, and the parent Task's
observable outcome.

## Release Boundary

The final release publishes the canonical executable for six RIDs, checksums,
signatures, SBOM, provenance, OIDC attestation, and thin package wrappers. Wrappers
contain no behavior, download, postinstall compilation, or fallback runtime.

No partial command publication is accepted. The replacement becomes shipping only
after the maintainer accepts the complete retained command set, package graph,
native support matrix, documentation, and release evidence.
