---
open-forge:
  description: Current cross-cutting structure and invariants for the C# replacement CLI, with the four-layer model and links to each layer's own record
  responsibility: Define the replacement CLI system boundaries, layer model, dependency direction, composition, safety, evidence, and release invariants
  tags: [Memory, Crystallized, Document, CurrentTruth, Evergreen, CLI, Architecture, Layers, DotNet, NativeAOT, Testing, Release]
---

# Replacement CLI Architecture

This is the land record. It holds what crosses every layer: the boundaries, the
layer model, dependency direction, and the invariants for serialization,
testing, build and release. Each layer's inner workings live in its own record
under [CLI Layers](layers/_layers.md).

## Status And Authority

This document defines the accepted implementation architecture for the C#
replacement CLI. The [Command Contract Set](command-contract-set.md), [Shared
CLI Operation Contract](shared-operation-contract.md), and detailed [command
contracts](contracts/_contracts.md) define product behavior. The shared
[Result Coordinates](contracts/shared/result-coordinates/_result-coordinates.md)
define the public envelope, source locations, statuses, exits, streams, and
compatibility. The routed [Technical Designs](technical-designs/_technical-designs.md)
define exact realization that is narrower than system Architecture.

The [Distribution](distribution.md) document defines the accepted package and
platform target. The active [CLI
Development](../../../working/cli-development/_cli-development.md) route records
implementation, evidence, and release state.

Programme-era prose from the 2026-08-21 greenfield reset — the build sequence,
Task delegation rules, and the non-shipping framing — was extracted and
[archived](../../../archived/cli-release/architecture-programme-prose.md) when
this document was split into layer records. The durable rules it carried are
restated here.

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
  containment required by the accepted threat boundary cannot be established.

The implementation must not add runtime plug-in discovery, dependency injection
for shell composition, a service locator, a fake filesystem, a universal command
result, a universal mutation engine, or native interop.

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

## Layer Model

The CLI has four layers. Each answers exactly one question, and the question is
what decides where something belongs:

| Layer            | The question it answers                                                 | Record                                    |
| ---------------- | ----------------------------------------------------------------------- | ----------------------------------------- |
| **Shell**        | What was asked, and how does the answer leave the process?              | [shell.md](layers/shell.md)               |
| **Framework**    | What is true about this workspace, and what may change it?              | [framework.md](layers/framework.md)       |
| **Operations**   | What does this command mean?                                            | [operations.md](layers/operations.md)     |
| **Presentation** | Of everything found, what does this reader need, and how is it written? | [presentation.md](layers/presentation.md) |

A request passes through them in this order:

```text
arguments
  -> Shell          parse, select the binding, select the workspace, form a request
  -> Operations     select what this request needs
  -> Framework      Documents parse bytes; Sources and Routing build identity and
                    the graph; State reads lifecycle, permissions, and inventory
  -> Operations     plan, apply if the command writes, form one result
  -> Presentation   select what to show, then render text or JSON
  -> Shell          write the output, return one exit code
```

Shell appears at both ends because it _is_ the boundary. Operations appears
twice because selection precedes reading and planning follows it. Framework is
entered only through an operation, never directly from Shell.

**A layer owns its question.** This is a structural property, not a coding
style. If two layers can both answer a question, neither owns it, and the
answers drift — measurably. The selection stage in
[Presentation](layers/presentation.md) exists because _what to show_ had no
owner, and the retired per-command renderers each answered it locally. The split of Documents,
Sources and Routing into three sections in [Framework](layers/framework.md)
exists because three subjects under one heading let a command grow its own
`## Axioms` parser without visibly violating anything.

### Vocabulary

- **Routing** is the workspace's document route graph — the Loader, route
  chains, entrypoints, generated Entries. Choosing which command to run is
  **binding selection** and belongs to Shell. Never use "routing" for dispatch.
- **Selection** means narrowing, in both places it appears: Operations selects
  which sources a request needs; Presentation selects which facts a reader sees.

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

  framework/OpenForge.Cli.Framework/Framework/
  shell/OpenForge.Cli.Shell/Shell/
  operations/OpenForge.Cli.Operations/Commands/
  rendering/OpenForge.Cli.Rendering/Presentation/
  output-text/OpenForge.Cli.OutputText/

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

### Accepted Modularization Target

On 2026-09-19 the maintainer accepted four libraries replacing Core:
`OpenForge.Cli.Framework`, `OpenForge.Cli.Shell`, `OpenForge.Cli.Operations`
and `OpenForge.Cli.Rendering`. The existing `OpenForge.Cli` executable remains
the composition and hosting root. The fifth library,
`OpenForge.Cli.OutputText`, now owns typed C# human-wording factories. The
implemented graph has six production projects. The four logical layers remain
unchanged; OutputText supplies pure wording to their existing semantic owners.

Shell references Framework and OutputText; Operations references Framework and
Shell; Rendering references Operations result models, neutral Shell contracts
and OutputText. The host references all five libraries. Cross-layer execution
orchestration and stream writing belong in the host. Rendering receives display
facts instead of observing the filesystem. OutputText uses ordinary C# and BCL
types and has no project or package dependency. Command schema metadata, machine
vocabulary, outcome selection, escaping, layout and authored content retain their
existing owners. Existing Wording callables preserve their signatures and forward
pure prose to the typed factories.

Definition markers use `@OpenForgeText`; selected current contracts use
`@OpenForgeTextRef` with factory-file links. Existing `@OpenForge` path annotations
retain their separate meaning. Integration architecture checks validate unique
definitions, resolved references and the BCL-only dependency boundary. Expected
outputs remain independent of production factories; relocated contract forms and
transcripts preserve the original reviewed evidence.

Keep one Unit, one Integration and one EndToEnd test project initially, with
selectable boundaries within them. Project count does not multiply test tiers.
Keep compiler-enforced dependency restrictions and retain checks for permitted
API subsets and intra-assembly ownership. Transitive references and friend access
must be addressed deliberately; a project reference alone does not prove every
layer rule. Exact visibility and callable seams are frozen before file moves.

[Task 38](../../../archived/cli-development/tasks/task38-project-and-test-split.md)
owns the behavior-preserving migration and its execution gates. Its packet also
records the still-conditional scenario continuation. No unreviewed scenario or
new filesystem abstraction is accepted merely by accepting this project graph.

### Current Implementation

The solution has ten direct entries: six production projects and the existing
Unit, Integration, EndToEnd and TestSupport projects.

```text
OutputText -> no CLI project
Framework  -> no CLI project
Shell      -> Framework, OutputText
Operations -> Framework, Shell
Rendering  -> Operations, Shell, OutputText
OpenForge.Cli -> Framework, Shell, Operations, Rendering, OutputText
```

`OpenForge.Cli` remains the only production executable and publish root. The
five libraries and host disable transitive project references. Rendering has
no Framework reference and consumes command-owned display facts. Existing
namespaces and internal accessibility remain unchanged.

Production friend access follows these exact edges: Framework grants Shell,
Operations and host; Shell grants Operations, Rendering and host; Operations
grants Rendering and host; Rendering grants host. OutputText grants only Shell,
Rendering and host. The four original libraries grant Unit and Integration access;
the host grants Integration only. Unit references those four libraries without
the host. Integration references those libraries and the host. Both retain the
existing callable seams and have no direct OutputText reference or friend access.
EndToEnd references TestSupport and exercises the published process; TestSupport
has no production reference.

No assembly is a supported third-party library API. Friend access does not
authorize Rendering to invoke Operations behavior: semantic boundary checks
retain the permitted result-model subset. The compiler enforces the absent
Framework edge, while source checks cover intra-assembly ownership.

## Root Host Boundary

The root project owns only process and composition concerns:

- process arguments, environment, current directory, standard streams, prompt
  capability, and cancellation hookup;
- the explicit ordered command tree and concrete binding registration;
- construction of immutable shell services and command capabilities;
- orchestration of one complete operation, presentation and output pipeline; and
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
ordinary .NET stream and redirection facts; the libraries never cache ambient console
state.

## Production Source Organization

Each library retains its logical layer tree, organized by bounded capability rather than artifact type:

```text
Logical layer trees across the four libraries:
  Shell/
    Parsing/
    Invocation/
    Definitions/
    Pipeline/
    Interaction/
    Serialization/
    Composition/

  Framework/
    Documents/
    Serialization/
    Filesystem/
    Workspace/
    Sources/
    GeneratedNavigation/
    OperationalContributors/
    Distribution/
    Lifecycle/
    Mutation/
    Recovery/
    Permissions/
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

  Presentation/
    Shared/
    <Command>/
```

Presentation is organized separately from command operations in the Rendering project:

```text
Presentation/
  Shared/
    Models/ Selection/ Rendering/ Text/ Prompts/ Wording/ Help/
  <Command>/
    Models/<Command>Data
    Shared/Selection/<Command>ReportSelector
    Shared/Rendering/<Command>DataTextRenderer
                <Command>DataJsonContext
    Shared/Wording/
    Shared/Help/
```

`OpenForge.Cli.Rendering` owns this complete `Presentation/` tree and the render
stage. Neutral validation/completion stages remain in Shell. The host owns
application/report binding, complete pipeline orchestration and stream writing.
The 4096-character diagnostic bound belongs to neutral Shell definitions.

Both lists are in dependency order, base first. Only folders with cohesive
source exist. This tree is a placement map, not authorization to create empty
directories. `Shell` contains process-wide CLI
mechanics with no Framework-domain behavior. `Framework` contains reusable facts
and effect boundaries derived from accepted Framework contracts. `Commands`
contains operation meaning and projections. `Presentation` contains the
complete report, report selection, shared renderers, and command-owned data
renderers.

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

Presentation has its own checked direction. No `Presentation/**` file may import
any `Framework.*` namespace. A command presentation under
`Presentation/<Owner>/` may import `Commands.<Owner>.Models.*` only for its own
owner, plus the permitted neutral Shell contracts and shared Presentation
types. No file under `Commands/` or `Framework/` references `Presentation/`.
When a command result needs to expose a Framework fact, the operation projects
it into a command-owned value before report selection. `LayerBoundaryTests`
checks these rules.

Cross-command facts remain free of command-specific status, findings, output,
and next-action policy. A command translates shared facts into its own result.
The Library record and inventory are neutral Framework facts; they do not grant
Library command policy or Framework runtime authority. `CliCompositionRoot`
constructs the Library capabilities and registers each Library leaf explicitly
alongside the other command bindings.

### Measured direction inside Framework

Framework capabilities form a layered order, with two capabilities at the base
that depend on nothing else in Framework — `Documents` and `Serialization` —
then `Filesystem` and `Workspace`, then `Sources`, then the state and effect
capabilities above them.

Six pairs currently depend on each other in both directions:

| Pair                                    | Dominant direction                 | The edge to invert                                              |
| --------------------------------------- | ---------------------------------- | --------------------------------------------------------------- |
| `Sources` and `Workspace`               | Sources -> Workspace               | `Workspace/Operational/WorkspaceEntryOperationalContributor.cs` |
| `Sources` and `OperationalContributors` | Sources -> OperationalContributors | three files under `OperationalContributors/`                    |
| `Sources` and `GeneratedNavigation`     | GeneratedNavigation -> Sources     | `Sources` reaching back into generated navigation               |
| `Sources` and `Mutation`                | —                                  | `Sources/Reading/SourceDocumentSnapshotReader.cs`               |
| `Extensions` and `Lifecycle`            | Extensions -> Lifecycle            | two files under `Lifecycle/Shared/Validation/`                  |
| `Mutation` and `Recovery`               | Recovery -> Mutation               | two files under `Mutation/`                                     |
| `Libraries` and `Permissions`           | Libraries -> Permissions           | `Permissions/Shared/Serialization/WorkspacePermissionCodec.cs`  |

A cycle means the two capabilities are one capability that has not been named,
or that one of them reached for a fact it should have been given. Inverting the
minority edge is the smaller change in every case above. These are recorded as
current state, not accepted as correct.

`Framework/Permissions` and `Framework/OperationalContributors` exist in the
tree and are named here; earlier revisions of this document omitted them and
listed a `Framework/Routing` folder that has never existed. Routing facts live
inside `Framework/Sources`.

## Serialization And Dependencies

The report pipeline writes one minified schema-3 JSON envelope for every
semantic status. The envelope carries the shared command, status, detail,
filter, workspace, summary, findings, effects, counts, limitations, command
data, recovery and next coordinates. Each command's data is a concrete
source-generated graph; members omitted by detail are absent rather than empty
placeholders. Text and JSON are two renderings of the same selected report.

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

Boundary traits provide focused selection within the independent projects:
Input, Processing and Output in Unit; Host, OS and Architecture identify
Integration evidence. Existing data/output cases in Integration retain their
actual Input/Processing/Output classification. Complete host composition and
the snapshot-corpus invariants live in Integration; original qualified case
identities and independently authored snapshot paths remain stable.

Each test owns every mutable workspace, home, temporary directory, cache,
process, and support artifact it can affect. Parallel tests share no mutable
state. Snapshots cover stable projections only; safety, identity, effects, and
status remain direct assertions.

Focused evidence proves local changes. System acceptance proves the complete
managed graph, supported Native AOT execution, public process, package, and
release boundary at the applicable integration points. Current testing and CLI
Directives define authoring, traits, historical-test promotion, proportional
selection, predecessor reuse, and exact gate triggers.

The cross-command report gate is part of the current architecture. Five
`CliReportInvariantsTests` facts cover the thirteen guarantees listed in
[CLI Layers](layers/_layers.md#cross-command-presentation-guarantees): detail
ladder, stderr diagnostics, severity and identity ordering, vocabulary and
escaping, UTF-8 framing, finding subjects, next-action placement, JSON
membership and scalar counts, catalogue code coverage, and dependency
direction. The complete managed and Native AOT evidence includes this gate.

Workspace Library first-release executable evidence targets Linux x64 and must
prove real relative file-link creation, inspection, dangling-link identity,
source and destination containment, per-effect source-tree exclusion, complete inventory,
record-last application, and no copy fallback. Other platform behavior remains
capability-gated and nonshipping until the same real-link evidence exists; no
platform expansion or Git behavior follows from this design.

## Build, Native AOT, CI, And Artifacts

The accepted local delivery trial adds host-detected `dist`, independent
`dist:wrapper`, and explicit `publish:native` / `publish:wrapper` commands.
Local publication consumes existing validated tarballs; wrapper packaging needs
no native artifact or .NET SDK. Complete-release orchestration still checks all
six native packages before the wrapper. Authoring these commands does not
authorize an agent to publish. Distribution defines the package boundaries.

Developer builds restore by default; explicit offline and no-restore modes
support prepared environments. Each distribution run replaces its selected
target's output and current reports/packages. Compiler intermediates remain
incremental. The cleanup command removes known build and delivery outputs,
preserving offline feeds, local npm link staging and unrelated artifact scopes.

The CLI uses stable .NET 10 with C# 14, nullable analysis, warnings as errors,
deterministic builds, package auditing, and no prerelease SDK. `global.json`
allows compatible stable feature-band roll-forward.

The supported Windows Native AOT gate is host-only. It needs `vswhere` on
`PATH` so the Visual Studio linker can be located, and a sandboxed worker
cannot run that gate. Assign Native AOT qualification to the overseer or
another unsandboxed host; managed build and test evidence remains separately
qualified.

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

The root package.json scripts own the repeatable local and CI build, test,
version and package commands. Focused TypeScript under `scripts/delivery/`
coordinates the standard .NET, Node and npm tools. Each delivery task has a
direct entry point. Shared capabilities stay at their nearest common delivery
scope. A dependency-free Node entry point, cli.ts, routes named commands from
one command/options catalog. The private root package registers its `forge` bin
for `npx forge <command> [options]`; npm scripts are short aliases. No bootstrap
build or global link is needed for setup. The `npm/` child contains the launcher, manifest generation and staging;
the `release/` child contains release coordination. Tests stay beside the
behavior they verify and remain independently selectable. One root strict Node
configuration checks scripts and tests, while one focused configuration emits
only the shipped launcher.
Target selection is shared by wrapper generation, host packing and release
collection. The recorded wrapper dependency graph defines that version’s
selected platforms. Release publication validates exactly those native packages
and publishes the wrapper last. Default selection remains all six targets.
These scripts contain no CLI domain behavior or general build framework.
`src/cli/` contains only C# implementation, projects and their required resources.
Maintained repository agent tooling belongs under `scripts/agent-tooling/`.

CI keeps one native matrix for all six accepted targets. A shared-check job
runs setup and verify once; each matrix runner runs setup and the explicit
build:native, test:built and pack stages on that host. Local dist declares those
stages, prints effective arguments and identifies stage failures. Explicit
--skip-tests enables unqualified local packing; its packages are marked untested
and rejected by native publication and complete release collection. Only finished packages and diagnostics cross
the job boundary. One release coordinator handles manual or version-tag
publication to selected destinations. Workflow YAML owns runners, scheduling,
toolchain installation, artifact transfer and credentials; repeatable build,
test, collection and npm publication behavior belongs to shared root scripts.

Root package.json owns one product version. Standard npm version handling and a
small lifecycle hook project that exact version to one .NET property. Staging
generates all seven npm manifests and dependencies from the version and platform
table. The informational version derives from the .NET version. Builds stamp the selected version before compilation. Optional SHA builds
change only the produced artifact version. Tests, packages and releases consume
that identity rather than supplying a second version. A release may reuse a
successful build only for the selected source commit and version.

Build jobs preserve complete managed test closures before native publication
and a separate managed-public-on-native closure afterward. Tests and packaging
consume those exact artifacts within the same job without rebuilding. Finished
archives preserve executable modes across transfer. Source, native and package identities stay explicit;
checksums do not establish independent reproducibility or expand publication
authority. The current Task defines the finite implementation and acceptance
scope, while the source workflows own exact action and tool pins.

## Release Boundary

Public distribution uses thin package wrappers with no CLI domain behavior,
download, postinstall compilation, or fallback runtime. No partial command or
package publication is accepted. The replacement becomes shipping only after
the complete retained command set, accepted package graph and platform target,
documentation, and release evidence are accepted together.

The exact six-target package graph, platform horizon, implementation state,
synchronized versions, staging, packing, checksums, proof ownership, and complete
publication boundary live in [CLI Distribution](distribution.md). New RIDs,
architectures, operating systems, libc variants, channels, signatures, SBOM,
provenance, OIDC attestation, or support-floor claims require a later explicit
maintainer decision.
