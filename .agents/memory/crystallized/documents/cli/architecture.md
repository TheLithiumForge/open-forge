---
open-forge:
  description: Current cross-cutting structure and invariants for the greenfield C# replacement CLI
  responsibility: Define the replacement CLI system boundaries, dependency direction, composition, safety, evidence, and release invariants
  tags: [Memory, Crystallized, Document, CurrentTruth, Evergreen, CLI, Architecture, Greenfield, DotNet, NativeAOT, Testing, Release]
---

# Replacement CLI Architecture

## Status And Authority

This document defines the accepted implementation architecture for the
non-shipping replacement CLI after the 2026-08-21 greenfield reset. The
[Command Contract Set](command-contract-set.md), [Shared CLI Operation
Contract](shared-operation-contract.md), and detailed [command
contracts](contracts/_contracts.md) define product behavior. The shared
[Result Coordinates](contracts/shared/result-coordinates/_result-coordinates.md)
define the public envelope, source locations, statuses, exits, streams, and
compatibility. The routed [Technical Designs](technical-designs/_technical-designs.md)
define exact realization that is narrower than system Architecture.

The removed implementation remains historical evidence in the [reset
record](../../../archived/cli-release/implementation-reset-2026-08-21.md).
Historical source may inform a Task, but it does not constrain class shape,
source placement, or implementation.

The replacement remains non-shipping. The [Distribution](distribution.md)
document defines the accepted package and platform target while the active [CLI
Development](../../../working/cli-development/_cli-development.md) route records
implementation, evidence, and release state.

## Architectural Goals

The implementation must:

- provide one predictable native executable for agents and occasional human use;
- keep the complete retained command surface in view without building
  speculative universal engines;
- establish process-wide and cross-command abstractions before command slices
  depend on them;
- keep command meaning, facts, plans, results, renderers, and evidence locally
  discoverable;
- make parser, filesystem, serialization, mutation, recovery, output, and
  process boundaries explicit and directly testable;
- remain deterministic, source-visible, free of reflective type or behavior
  discovery, and compatible with trimming and Native AOT;
- use real operating-system filesystems and fail closed when the identity or
  containment required by the accepted threat boundary cannot be established;
  and
- let bounded implementers execute closed Tasks without inventing architecture.

The implementation must not add runtime plug-in discovery, dependency injection
for shell composition, a service locator, a fake filesystem, a universal command
result, a universal mutation engine, native interop, or a compatibility path to
`open-forge-old`.

### Project Criticality And Threat Boundary

The CLI manages user-owned Markdown and supporting files in a local development
workspace. It is not a security boundary, database, or mission-critical
transaction processor. Its safety target is to avoid corrupting, partially
writing, silently overwriting, or losing ordinary user work through an Open
Forge operation and to leave practical recovery evidence when an operation
cannot finish cleanly.

The supported boundary includes malformed input, static links and reparse-point
aliases observable through managed APIs, ordinary filesystem and process
failures, interruption, stale plans, concurrent edits observed by expected-state
checks, and concurrent Open Forge processes that cooperate through the workspace
lock. Workspace directory mappings are expected to remain stable during one
operation. A malicious same-user process that bypasses the lock or performs a
transient namespace swap-and-restore is outside the supported threat model. The
CLI does not claim inode or file-ID equivalence for hard links or mount
boundaries that portable managed APIs do not expose as links.

Use normal managed .NET and operating-system behavior within that boundary. Do
not add native interop or recreate platform filesystem primitives to defend
against an actor outside it. A stronger later threat model requires a new
Architecture decision covering platform, portability, Native AOT, maintenance,
and evidence costs.

## Physical Workspace

The repository root owns the replacement CLI's .NET workspace configuration and
ignored build output. Replacement source, projects, and tests remain below
`src/cli/`:

```text
global.json
NuGet.Config
Directory.Build.props
Directory.Packages.props
OpenForge.Cli.slnx
artifacts/

src/cli/
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
```

The root workspace supports ordinary `dotnet restore`, `dotnet build`, and
`dotnet test` entry from the repository root. `/artifacts/` contains all C#
binary, intermediate, default publish, test, and package output through the SDK
artifacts layout. Projects create no local `bin/` or `obj/` folders.

Projects use SDK default recursive authored-source inclusion. They do not list
ordinary C# files, use virtual solution folders, or link production source into
another project. Deterministic generated source is the only production compile
exception. The test-support library is an ordinary non-shipping project rather
than linked source. Historical tests do not create a parallel executable test
architecture.

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
Framework-facing capabilities, and commands. The root project may depend on
Core. Core never depends on the root. Tests depend only on the production and
support projects required by their evidence tier. End-to-end tests do not call
production internals.

Friend assembly access is limited to named test projects and the root
composition assembly when a closed internal contract would otherwise become
public solely because of the project boundary. No assembly is a supported
third-party library API. Public C# visibility is an implementation necessity,
not a compatibility promise.

## Root Host Boundary

The root project owns only process and composition concerns:

- process arguments, environment, current directory, standard streams, prompt
  capability, and cancellation hookup;
- the explicit ordered command tree and concrete binding registration;
- construction of immutable shell services and command capabilities;
- invocation of one Core host boundary; and
- conversion of one process completion into the executable exit code.

`Program.cs` contains no command or domain behavior. `CliHost` owns one complete
process invocation. `CliCompositionRoot` visibly registers every root command,
group, and leaf in stable order. Adding a command changes this source and the
command's local source; it does not change a string dispatcher or runtime
registry.

Future operational composition uses one immutable application-scoped
`OperationalContributorCatalogue` explicitly built by `CliCompositionRoot`.
Producer-owned typed contributors project narrow Status and Doctor views from
fresh per-invocation observations. The catalogue is not dependency injection, a
service locator, reflection discovery, a runtime registry, a generic operational
engine, or ambient registration. Composition alone changes no public Status or
Doctor contract. Exact contributor, catalogue, and view callable signatures are
deliberately deferred to the later Gray boundary against its then-current
producer baseline.

The root host passes standard input, writers, prompt capability, environment
facts, and cancellation explicitly. It constructs native interaction from
ordinary .NET stream and redirection facts; Core never caches ambient console
state.

## Core Source Organization

Core is organized by bounded capability rather than artifact type:

```text
OpenForge.Cli.Core/
  Shell/
    Composition/
    Definitions/
    Interaction/
    Invocation/
    Parsing/
    Pipeline/
    Presentation/
    Output/
    Serialization/

  Framework/
    Distribution/
    Workspace/
    Filesystem/
    Sources/
    Routing/
    Documents/
    GeneratedNavigation/
    Lifecycle/
    Mutation/
    Recovery/
    Libraries/
    Extensions/

  Commands/
    Shared/
    Route/
      Shared/
      List/
      Inspect/
    Extension/
      Shared/
      List/
      Inspect/
    Library/
    <RootLeaf>/
```

Only folders with cohesive source exist. This tree is a placement map, not
authorization to create empty directories. `Shell` contains process-wide CLI
mechanics with no Framework-domain behavior. `Framework` contains reusable facts
and effect boundaries derived from accepted Framework contracts. `Commands`
contains operation meaning and projections.

Workspace Libraries follow this ownership map. The neutral no-follow logical-leaf
observation belongs to the focused `Framework/Filesystem/` capability. Relative
file-link effects belong to `Framework/Mutation/`, and their typed recovery
identities and guarded application belong to `Framework/Recovery/`. Library
record and complete-inventory facts belong to `Framework/Libraries/`. Library
attach, sync, detach, list, and inspect policy, plans, findings, results, and
rendering remain under `Commands/Library/`. This placement adds no dependency
injection, runtime registry, reflection, or sibling-private import.

Definitions, binding, behavior-owning composition, models, and supporting
behavior remain within the narrowest command or capability owner. A private
`Shared/<Capability>/` child marks its support boundary. Semantic behavior moves
to a wider parent only when another real consumer needs identical meaning. A
neutral mechanical foundation required by several accepted outcomes sits at
their nearest shared scope before dependent slices.

Namespaces match physical folders. One command never imports another command's
private `Shared` namespace. No forwarding namespace preserves a removed layout.
The C# Style and CLI Implementation Directives define authoring and migration
procedure without duplicating it here.

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

Cross-command facts remain free of command-specific status, findings, output,
and next-action policy. A command translates shared facts into its own result.
The Library record and inventory are neutral Framework facts; they do not grant
Library command policy or Framework runtime authority. `CliCompositionRoot`
constructs the Library capabilities and registers each Library leaf explicitly
alongside the other command bindings.

## Shell Definitions And Composition

Typed definitions own every executable, command, argument, option, finite value,
machine code, result-command, and next-action identity exactly once.

Global definitions are split by responsibility: syntax and executable identity;
presentation format, view, verbosity, and output targets; workspace selection;
terminal modes and conflict policy; semantic statuses and process exits; process
completion and output disposition; and parser and shell error identities. Each
command owns its group, leaf, operands, local options, finite values, finding
codes, result command name, and next-action contents.

One closed generic binding owns one request/result pair. A non-generic boundary
stores heterogeneous bindings without erasing concrete operation or
serialization types. Dispatch uses exact `System.CommandLine.Command` identity,
never strings. No command binding locates services. The composition root supplies
complete immutable dependencies through direct construction or narrow capability
records.

The operational contributor catalogue follows the same explicit-composition
direction. Each producer owns its contributor and typed observation. Status and
Doctor consume only their narrow views. Neither command locates producers or
receives a broad service collection, registry, or generic operational context.

### Native Interaction

`Shell/Interaction/` owns one small native question-and-answer transport. It
receives explicit input, prompt output, prompt capability, and caller
cancellation. Prompt capability requires both standard input and the stderr
prompt stream to be terminal-capable. There is no terminal framework, PTY
abstraction, native probe, or P/Invoke.

Prompts go to stderr so stdout remains one human result or one JSON document.
The shared transport owns no command questions, candidates, defaults,
validation, retry policy, confirmation meaning, or result. Those remain local
to a prompt-capable command. JSON, `--automatic`, and redirected operation never
prompt. Unrelated commands and requests gain no interaction or stream parameter.

## Parsing And Invocation

`System.CommandLine` exclusively owns command selection, arity, occurrence
aggregation, typed conversion, unknown symbols, parser diagnostics, standard
syntax help, and version dispatch.

The implementation performs one parse, validates parser and bounded accepted
delimiter facts, resolves terminal conflicts, selects the exact binding, handles
help or version, normalizes one global invocation and optional workspace, and
forms one command-local request or concrete invalid result.

It does not rescan raw arguments for facts exposed by the parse tree. A lexical
guard may inspect only one exact recognized option and its attached delimiter
when an accepted syntax distinction cannot be obtained from typed parser facts.
Such a guard does not parse values, count occurrences, select commands, or
produce parser diagnostics. The accepted Route Update exception is bounded by
its [Technical Design](contracts/route/update/technical-design.md). Route List
retains its separate command-contract exception.

`CliInvocation` contains normalized process-wide facts only. A command request
is complete and immutable. Neither carries `ParseResult`, parser symbols,
writers, service collections, raw arguments, or an unrelated context bag.
Workspace-free commands preserve genuine workspace absence. Workspace-aware
commands receive one selected normalized workspace before domain work.

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

Each stage validates its input before invoking an operation, renderer, or
writer. Direct stage entry remains testable. Unknown finite values fail closed.

The pipeline invokes one operation at most once, propagates caller cancellation,
never reruns work during rendering or output, selects one cached concrete
renderer, writes one primary result and at most one bounded diagnostic
projection, and returns one fixed process completion from the concrete semantic
status.

`ICliCommandResult` exposes only shared process facts needed by the pipeline:
command identity, semantic status, workspace presence, and next-action presence.
Concrete result records retain complete command payloads and serialize through
concrete source-generated metadata. The interface is never a wire type.

## Result JSON Coordinates And Process Status

The shared [Result Coordinates Interface
Contract](contracts/shared/result-coordinates/interface.md) defines the exact
schema-v1 envelope, authored source-location coordinates, semantic statuses,
numeric exits, primary streams, and compatibility. Its [Behavior
Contract](contracts/shared/result-coordinates/behavior.md) defines technology-
neutral formation and conformance.

Architecture requires one concrete command result before presentation. The
pipeline exposes shared process facts through the non-wire result interface,
keeps command payloads on concrete records, serializes only concrete source-
generated graphs, and derives one process completion from the selected semantic
status. It does not create a universal wire result or command-independent
payload.

## Presentation, Help, And Diagnostics

Human and JSON renderers are command-local because they project command meaning.
Shell presentation owns finite format selection, primary target selection,
diagnostic target, output messages, and process completion. Both renderers
consume the same concrete result.

Standard help comes from the exact composed `System.CommandLine` symbol graph.
Bindings provide ordered product sections such as Discovery, examples, related
commands, bounded notes, and unavailable planned commands. The implementation
does not maintain a second command catalogue or normalize library output through
ad hoc string replacement.

Verbose diagnostics are bounded, escaped, and redacted command-local projections
of already-known facts. They never change operation status, rows, effects,
primary content, or exit. JSON stdout remains one valid document under the
shared result contract.

## Framework Capability Model

The complete command set demonstrates several shared capabilities before their
first consumer is implemented. Architecture establishes neutral mechanical
foundations at their nearest shared scope. Other shared contracts may be
established early, while semantic behavior is implemented only when a consuming
Task proves identical meaning.

### Workspace

Workspace selection resolves explicit and inferred subjects, records the exact
selection method, normalizes identity once, and never invents a fallback fact.
Terminal modes bypass workspace selection. Workspace-free commands preserve null
workspace in concrete results.

### Filesystem And Resolved Path Identity

Filesystem code uses real `System.IO` and typed outcomes. Path strings,
normalized lexical paths, resolved physical paths, and observed link targets are
separate facts. `physical identity` names this resolved-path and observed-alias
fact under the stable-workspace boundary. It does not mean an inode, file ID, or
handle-bound object identity.

`PhysicalPathResolver` walks one existing directory component at a time from a
proven root. For each directory component needed to reach a final leaf it
inspects without enumerating descendants, classifies ordinary, missing,
inaccessible, dangling, or reparse/link state, resolves one permitted link
target, immediately proves containment, records the identity used for cycle and
observable-link alias detection, and continues only from the proven contained
result. The final logical leaf is handed to the no-follow observation below
before ordinary file identity resolution.

A path that leaves the root and later re-enters is blocked at the first external
transition. Final-target containment is insufficient. Paths are resolved before
access, expected state is revalidated immediately before effects, and ordinary
managed BCL file operations provide atomic replacement. These checks reject
static escapes and detected persistent changes; they do not claim adversarial
handle-bound identity across a transient namespace swap.

Every effect that addresses a logical file leaf first obtains a neutral typed
no-follow observation of that leaf before ordinary physical resolution. The same
observation is repeated during initial preflight, under-lease revalidation, and
immediately before the effect. A present link, reparse point, or special final
leaf blocks ordinary `Create`, `Replace`, `Delete`, and `ReplaceGeneratedRegion`.
The guard does not resolve or follow that final component. Stable contained
directory-link ancestry remains governed by this ordinary path contract; this
Architecture does not broaden rejection of that ancestry. Library source and
destination rules may require the stricter real-directory boundary defined by
the [Workspace Libraries Technical Design](technical-designs/workspace-libraries.md).

The no-follow guard is neutral and does not consult Library records. Therefore a
Route Update, Index, Route Move, or Route Remove operation cannot write through
or delete a Library projection, even when the projection has no readable or
matching Library record.

If managed BCL evidence cannot satisfy an accepted required guarantee,
implementation stops at Architecture rather than adding P/Invoke or silently
weakening it. A theoretical guarantee outside the accepted threat model does not
justify exceptional machinery. Typed reads distinguish complete, missing,
invalid encoding or syntax, access denied, and I/O failure while retaining
bounded direct causes without leaking sensitive content.

### Sources, Routing, And Documents

Source references use one shared grammar and typed identity model. Commands
retain attempted identity separately from resolved identity.

The source catalogue and route graph expose immutable facts only: canonical
source identity, recognized entrypoint form, Loader root, route chain,
overwrites, loading behavior, and safe topology. A Framework scope is authored
meaning, not a mechanically identifiable path segment; a consumer reports it
only from explicit contract evidence. Commands translate shared facts into local
meaning.

Markdown capabilities use one fixed CommonMark pipeline only when a real
consumer requires body parsing. One neutral frontmatter parser owns delimiter
and body boundaries. One neutral YAML syntax parser owns the source-preserving
node shape, scalar spans, aliases, and unsupported-mapping facts. Semantic
metadata uses one CLI-root generated YAML context and small models. Command
interpretation, findings, and status remain local.

Generated navigation is a projection of routed sources, never an independent
authority. One neutral formation combines retained observed catalogue evidence
with intended logical membership, topology, Loader, alias, and collision facts
without command policy or effects. Exact callables, formation rules, missing-
Loader behavior, collision handling, projection, and region mechanics live in
the [Generated Navigation Technical
Design](technical-designs/generated-navigation.md).

### Workspace Libraries

Workspace Libraries are local filesystem composition over ordinary consumer
paths, not a new Framework root or a Loader federation. One Library record names
one workspace-contained real source directory. Its complete eligible inventory
under the source directory's real `.agents/` directory maps to the same
consumer-relative `.agents/` paths through relative file symlinks. The consumer
keeps one Loader and its own route chain; a projected file has the meaning of
its consumer destination.

Attach, Sync, and Inspect require the source root and consumer destination
namespace to be physically disjoint, with the source root and its `.agents/`
directory having no linked or reparse ancestry. Detach uses only exact
consumer-side registered destinations and does not resolve a source root. The
source inventory is strict and complete: an unavailable,
unreadable, externally resolving, aliased, or otherwise unsafe item prevents a
complete mutating plan. Source bytes are read-only facts and are never effect
targets. Library projection effects can create or delete only declared relative
file-link objects and their real parent directories; an existing consumer-owned
generated `Entries` region may be replaced under the Index contract. The
consumer-side Library record is published last after link and generated effects
verify. Only a typed Library relative-file-link effect may create or delete a
link object; ordinary file effects reject a link final leaf.

The exact schema-v1 record, inventory closure, relative-link identity, capability
gate, and command-facing fact shapes are defined in the [Workspace Libraries
Technical Design](technical-designs/workspace-libraries.md). No copy fallback,
Git operation, native interop, external destination, path remapping, glob, or
write-through mutation is part of this architecture.

### Consumer Workspace Permissions

`Framework/Permissions/` owns strict consumer grant representation, observation,
exact identity lookup and proposed permission-file changes. Extension commands
own required-grant policy, prompting, results, lease orchestration and content
application. Permission is separate from lifecycle ownership. Neutral immutable
facts may also serve Library consumers when their contracts select this shared
meaning; no consumer imports another command's policy.

The [Workspace Permissions contracts](contracts/shared/workspace-permissions/_workspace-permissions.md)
and [Technical Design](technical-designs/workspace-permissions.md) own the exact
schema, approval, result and recovery behavior. Existing BCL file effects and
recovery cover permission writes. There is no new automatic Repair catalogue
entry, DI, runtime registry, independent transaction or compatibility reader.

### Embedded Framework Distribution

`Framework/Distribution/` owns one neutral embedded Framework payload reader and
immutable asset and inventory facts. The Core project embeds the complete
canonical `src/open-forge/` tree under one fixed resource prefix. Runtime reads
that payload through ordinary BCL resource APIs and never reads repository source
paths.

The payload has canonical asset identity, exact bytes, deterministic ordinal
inventory, per-asset hashes, and one aggregate inventory fingerprint. Root
Install and Update consume the complete inventory. Framework-aware Route Init
consumes route entrypoint assets and topology from the same canonical payload.
Exact resource, hashing, parity, and isolated-binary mechanics live in the
[Embedded Payload Technical Design](technical-designs/embedded-payload.md).

### Lifecycle, Mutation, And Recovery

Read-only commands create no locks, lifecycle files, caches, indexes, recovery
bundles, or drafts.

Mutation commands follow this cross-cutting stage order:

```text
resolve and inspect, including no-follow final-leaf facts
  -> form a command-local immutable plan
  -> validate policy and collisions
  -> acquire the real workspace lock when applicable
  -> revalidate every planned fact under the lease
  -> prepare and verify one external recovery bundle for every reversible non-no-op effect
  -> apply bounded filesystem changes with an immediate no-follow check per effect
  -> verify resulting identity and bytes
  -> write accepted lifecycle or Library record state last
  -> remove the command-owned recovery bundle only after whole-command success
  -> form one concrete result
```

The lock provides exclusion only among cooperating Open Forge processes. It is
external to the workspace and distinct from lifecycle and recovery. Existence is
not ownership, activity, lifecycle authority, or recovery history.

Every complete plan has one immutable verified final external bundle before the
first effect whenever it contains a non-no-op effect that the operation must be
able to reverse. This includes relative file-link creates and deletes and the
prior-missing Library record Create; semantic or byte no-ops have none. A
multi-file operation is not presented as one filesystem transaction. Shared
support never automatically restores, rolls back, or compensates for target
effects and never classifies current target state from recovery provenance.
Handled failure, interruption, and post-verification cleanup retain truthful
residual state for explicit Repair.

Shared mutation support provides facts and mechanical capabilities. Commands
retain their plan, effect ordering, findings, lifecycle publication, recovery
mapping, and result. Cleanup retains its narrow monotonic command-contract
exception. Status and Doctor observe recovery facts without acquiring the lease
or inferring activity.

Directory creation remains a separate Create-only effect rather than a file-
change kind. A verified directory may remain as residual state and has no
recovery payload. The [Directory Creation Technical
Design](technical-designs/directory-creation.md) defines its exact mechanics.

Lifecycle state remains schema version 1 at
`.agents/open-forge.lifecycle.json`. Each Framework target retains exact
canonical source-asset provenance or typed derived-region absence. Concrete
target, source asset, region identities, and baseline fingerprint establish
per-effect provenance. The [Lifecycle Provenance Technical
Design](technical-designs/lifecycle-provenance.md) defines the exact fields and
validation.

The [Mutation And Recovery Technical
Design](technical-designs/mutation-and-recovery.md) defines application-data
stores, persistent lock identity, ZIP and manifest realization, expected-state
checks, same-directory atomic file mechanics, receipts, bounded validation, and
guarded deletion, no-follow recovery comparison, and relative-file-link
application. The [Workspace Libraries Technical Design](technical-designs/workspace-libraries.md)
defines the separate Library record and inventory. The [Shared CLI Operation
Contract](shared-operation-contract.md) and command contracts define observable
operation and cleanup policy.

## Serialization And Dependencies

JSON uses `System.Text.Json` source generation with reflection disabled. YAML
uses one source-generated static context for accepted Framework metadata shapes.
Concrete command contexts register concrete result graphs. AOT evidence executes
every registered shape. The strict Library record uses one source-generated
schema-v1 JSON model and does not add a reflective or compatibility reader.

Accepted dependency roles are `System.CommandLine` for command parsing,
YamlDotNet plus its static generator for Framework metadata, Markdig only after a
retained body consumer exists, and xUnit v3 on Microsoft Testing Platform for
tests. Every dependency must earn its Native AOT, trimming, maintenance,
security, and binary-size cost.

The [CLI Dependency Policy](../../decisions/cli-dependency-policy.md) records the
rationale and change boundary. Repository-root
[`Directory.Packages.props`](../../../../../Directory.Packages.props) is the sole
exact-version source. A package or role change requires an explicit dependency
decision and complete affected evidence.

## Test Architecture

The active test projects have distinct evidence boundaries:

- Unit tests cover pure values, definitions, parsers, binders, ordering, result
  formation, renderers, serialization contracts, and directly callable shell
  stages without claiming real filesystem or process behavior.
- Integration tests call production modules with owned real temporary
  filesystems and cover source generation, resolved-path containment and aliases,
  no-follow leaf observations, relative-file links, Library inventory and
  records, locking, recovery, runtime, and Native AOT internal boundaries.
- End-to-end tests invoke the published executable and prove arguments, streams,
  statuses, exits, cancellation, unchanged bytes, and public scenarios.
- TestSupport contains cohesive real-OS workspace and process fixtures shared by
  at least two active projects. Command-specific builders remain local.

Each test owns every mutable workspace, home, temporary directory, cache,
process, and support artifact it can affect. Parallel tests share no mutable
state. Snapshots cover stable projections only; safety, identity, effects, and
status remain direct assertions.

Focused evidence proves local changes. System acceptance proves the complete
managed graph, supported Native AOT execution, public process, package, and
release boundary at the applicable integration points. Current testing and CLI
Directives define authoring, traits, historical-test promotion, proportional
selection, predecessor reuse, and exact gate triggers.

Workspace Library first-release executable evidence targets Linux x64 and must
prove real relative file-link creation, inspection, dangling-link identity,
source and destination containment, physical disjointness, complete inventory,
record-last application, and no copy fallback. Other platform behavior remains
capability-gated and nonshipping until the same real-link evidence exists; no
platform expansion or Git behavior follows from this design.

## Build, Native AOT, CI, And Artifacts

The CLI uses stable .NET 10 with C# 14, nullable analysis, warnings as errors,
deterministic builds, package auditing, and no prerelease SDK. `global.json`
allows compatible stable feature-band roll-forward.

The repository-root build graph and `/artifacts/` topology produce one explicit
managed development publication and explicit RID-selected native publications.
End-to-end evidence discovers build-owned artifacts rather than ambient
executable or expected-version overrides. Design-time and explicitly opted-out
builds do not produce the managed development publication. The
[Repository-Root CLI Tooling
Decision](../../decisions/repository-root-cli-tooling.md) records exact developer
publication rationale and consequences; MSBuild configuration defines current
target and path mechanics.

CI invokes the root solution and configuration while project source remains
below `src/cli/`. It may upload bounded artifacts but does not make `.github/` a
C# source root. Current platform and package targets, present implementation
gaps, staging, packing, checksums, and proof ownership are defined by
[Distribution](distribution.md). Additional support and supply-chain claims
require explicit acceptance.

## Durable Implementation Sequence

Stable dependency order runs from cross-cutting shell and filesystem foundations
to read-only fact formation, then generated navigation and mutation foundations,
then mutation producers, aggregate Status and Doctor views, repair and cleanup,
and complete distribution. A consumer never invents a missing shared contract,
identity, safety primitive, composition boundary, or evidence foundation.

The exact queue, readiness, completion state, and integration receipts belong to
the active [CLI Development](../../../working/cli-development/_cli-development.md)
route, its Plan, Tasks, and project control. They are not durable Architecture.

## Planning, Tasks, And Delegation

A bounded implementation Task cannot invent or reinterpret cross-cutting
architecture, public behavior, shared schema, safety, package, platform, or
release meaning. It returns an unresolved boundary to the current project
authority before code continues.

Current Directives, Workflows, role sources, and Task records define work
orchestration, evidence procedure, review, and acceptance. Local passing tests
do not accept a change that violates this Architecture or its linked contracts.

## Release Boundary

Public distribution uses thin package wrappers with no CLI domain behavior,
download, postinstall compilation, or fallback runtime. No partial command or
package publication is accepted. The replacement becomes shipping only after
the complete retained command set, accepted package graph and platform target,
documentation, and release evidence are accepted together.

The exact x64 package graph, platform horizon, current implementation gaps,
synchronized versions, staging, packing, checksums, proof ownership, and atomic
publication boundary live in [CLI Distribution](distribution.md). New RIDs,
architectures, operating systems, libc variants, channels, signatures, SBOM,
provenance, OIDC attestation, or support-floor claims require a later explicit
maintainer decision.
