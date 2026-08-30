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

The replacement remains non-shipping until every retained command, the current
`linux-x64` Native AOT build and smoke, packed install and invocation, checksums,
documentation, and release gate are implemented and accepted.

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
writing, silently overwriting, or losing ordinary user work through an Open Forge
operation and to leave practical recovery evidence when an operation cannot
finish cleanly.

The supported boundary includes malformed input, static links and reparse-point
aliases observable through managed APIs, ordinary filesystem and process
failures, interruption, stale plans, concurrent edits that expected-state checks
observe, and concurrent Open Forge processes that cooperate through the
workspace lock. Workspace directory mappings are expected to remain stable
during one operation. A malicious same-user process that bypasses the lock or
performs a transient namespace swap-and-restore is outside the supported threat
model because it already has direct authority to modify the workspace. The CLI
also does not claim inode or file-ID equivalence for hard links or mount
boundaries that portable managed APIs do not expose as links.

Use normal managed .NET and operating-system behavior within that boundary. Do
not add native interop or recreate platform filesystem primitives to defend
against an actor outside it. If a later product context requires a stronger
threat model, return to Architecture and evaluate the platform, portability,
Native AOT, maintenance, and evidence costs before implementation.

## Physical Workspace

The repository root owns the replacement CLI's .NET workspace configuration and
ignored build output. Replacement source, projects, and test source remain below
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

The root workspace lets maintainers and IDEs use ordinary `dotnet restore`,
`dotnet build`, and `dotnet test` commands without changing directories or
supplying a solution path. `/artifacts/` contains all C# binary, intermediate,
default publish, test, and package output through the SDK artifacts layout.
Projects do not create local `bin/` or `obj/` folders.

The former preserved route-list test inventory was audited and removed after its
useful expectations were already represented in active evidence. Completed Tasks
and Git retain its historical disposition. Do not restore preserved projects as
an executable or parallel test architecture.

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

- process arguments, environment, current directory, standard streams, prompt
  capability, and cancellation hookup;
- the explicit ordered command tree and concrete binding registration;
- construction of immutable shell services and command capabilities;
- invocation of one Core host boundary; and
- conversion of one process completion into the executable exit code.

`Program.cs` contains no command or domain behavior. `CliHost` owns one complete
process invocation. `CliCompositionRoot` visibly registers every root command,
group, and leaf in stable order. Adding a command changes this composition source
and the command's local source; it does not change a string dispatcher or runtime
registry.

The root host passes standard input, writers, prompt capability, environment
facts, and cancellation explicitly. It constructs the native interactive
session from `Console.In`, `Console.Error`, `Console.IsInputRedirected`, and
`Console.IsErrorRedirected`; Core never caches ambient console state.

## Core Source Organization

Core is organized by bounded capability rather than by artifact type.

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

### Native Interaction

`Shell/Interaction/` owns one small native question-and-answer transport. A
`CliInteractiveSession` receives a `TextReader`, a prompt `TextWriter`, and one
explicit prompt-capable fact. Its asynchronous `AskAsync` operation returns an
answered or end-of-input response and observes caller cancellation.

Prompt capability requires both standard input and the stderr prompt stream to
be terminal-capable, derived by the root from ordinary .NET redirection facts.
There is no terminal framework, PTY abstraction, native probe, or P/Invoke.

Prompts go to stderr so stdout remains either one human result or one JSON
document. The session owns no command questions, candidate lists, defaults,
validation, retry policy, confirmation meaning, or semantic result. Those remain
local to Extension Create, Install, Route Inspect, or another command that later
earns interaction. JSON, `--automatic`, or redirected operation never prompts.
The root composition layer injects the session only into prompt-capable command
operation factories. Command binding records only the command-local explicit
interaction-policy Boolean in a complete immutable request. `CliInvocation`,
generic binding and operation contracts, unrelated operation factories, and
unrelated command requests remain unchanged and do not gain interaction or
stream parameters.

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

### Filesystem And Resolved Path Identity

Filesystem code uses real `System.IO` and typed outcomes. Path strings,
normalized lexical paths, resolved physical paths, and observed link targets are
separate facts.

Within child contracts, `physical identity` names this resolved-path and
observed-alias fact under the stable-workspace boundary. It does not mean an
inode, file ID, or handle-bound object identity.

`PhysicalPathResolver` walks one existing component at a time from a proven root.
For every component it:

1. inspects the component without enumerating descendants;
2. classifies ordinary, missing, inaccessible, dangling, or reparse/link state;
3. resolves one link target;
4. proves containment immediately after that resolution;
5. records the resolved path identity used for cycle and observable-link alias
   detection; and
6. continues only from the proven contained result.

A path that leaves the root and later re-enters is blocked at the first external
transition. Final-target containment is insufficient. The CLI resolves paths
before access, revalidates expected state immediately before effects, and uses
ordinary managed BCL file operations and atomic replacement. These checks reject
static escapes and detected persistent changes; they do not claim adversarial
handle-bound identity across a transient namespace swap.

If managed BCL evidence cannot satisfy a guarantee that the accepted project
boundary actually requires, implementation stops at Architecture rather than
adding P/Invoke or silently weakening the requirement. A theoretical guarantee
outside the accepted threat model does not justify exceptional machinery.

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

Generated-navigation formation is the neutral bridge from observed catalogue
evidence and intended logical-source membership to projection. Its intended-source
overload is exactly
`Build(SourceCatalogue observedCatalogue, IReadOnlyList<SourceLogicalSource> intendedSources)`.
This extends the existing Generated Navigation formation. It does not add a
prospective catalogue or source framework. The existing `Build(SourceCatalogue)`
path preserves exact current-state behavior and member identity by delegating
with the observed catalogue's current sources. Formation continues to use the
existing `SourceRouteTopologyBuilder`.

The formation retains the observed `SourceCatalogue` as catalogue evidence.
Intended sources separately define membership, lookup, topology, Loader presence,
Loader roots, and intended target collisions. Root-entrypoint and route-parent
ambiguities derive from intended sources; physical-alias ambiguities derive only
from retained observed candidates. Target collisions compare intended base and
overwrite layers with one another and with remaining observed occupants while
excluding removed-layer evidence. An unobserved intended source never fabricates
a candidate, catalogue issue, or physical alias.

The projection request consumes the resulting cohesive immutable fact instead of
independently assembling sources and topology. Formation reads no document bodies,
discovers no generated lines, and owns no command selection, finding, status,
write policy, or application meaning. It uses real current catalogue evidence and
does not create a virtual filesystem, temporary checkout, or hidden Index. This
preserves I1's current-state formation while extending the accepted Generated
Navigation capability for later topology-changing consumers; GN1 and I1 remain
Complete. Accepted feature `f82c2b168657baf2fac50c76e2ff2cc0ed3776d7` is
squash-integrated at `cc35c853fd7b55d31e3fb2c9a454d9ab61c1884e`, with exact
tree `867be79ebee4ba60f2116a83857edadb8a5dbf0a`.

When a Loader is present, its intended roots are the structurally valid,
physically unique recognized entrypoints that directly represent each
`.agents/<slug>` folder. Generated `Entries` remain comparison input only. A
missing Loader produces zero roots: operand-free Index selection blocks, while
an explicit complete detached selection may still proceed. A missing
intermediate entrypoint keeps the lower tree detached; no parent is invented.
Multiple recognized entrypoints representing one root folder are ambiguous.
Only physical aliases proven on the current host may collapse, and only when
their route and document-form identities are compatible; proven incompatible
aliases block. Broader portable case, Unicode, and device-name equivalence is
deferred rather than guessed. These formation rules do not change the
current-visible Route or Context facts.

### Embedded Framework Distribution

`Framework/Distribution/` owns one neutral embedded Framework payload reader and
its immutable asset and inventory facts. The Core project embeds the complete
canonical `src/open-forge/` tree through ordinary C# project
`EmbeddedResource` items under one fixed logical-name prefix. Runtime code uses
the BCL manifest-resource APIs only for that exact prefix. Bounded resource-name
enumeration and exact resource access are permitted; reflective assembly/type
discovery, plug-in registration, and behavioral dispatch remain forbidden.

The reader derives canonical `/`-separated relative paths, rejects empty,
unsafe, duplicate, or noncanonical identities, reads exact bytes, orders assets
ordinally, and computes per-asset SHA-256 values plus one deterministic inventory
fingerprint. It never reads repository source paths at runtime. Root Install and
Update consume the complete inventory. Framework-aware Route Init consumes the
canonical route entrypoint assets and topology from the same payload. Source-tree
parity tests compute the source set and differences instead of preserving a
second hand-authored hash catalogue; Native AOT evidence moves the published
binary away from the checkout before reading every resource.

### Lifecycle, Mutation, And Recovery

Read-only commands never create locks, lifecycle files, caches, indexes, or
recovery bundles or drafts.

Recovery-store resolution has separate writer and observer modes. Only a
mutation recovery-bundle writer may resolve
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.Create)` so it can create the application-owned
`OpenForge/recovery/v1` subtree during pre-effect preparation. Status, Doctor,
and Cleanup are observers: they resolve the same special folder with
`Environment.SpecialFolderOption.None` and never create the OS application-data
root or the Open Forge subtree. For those observers, an absent application-data
root or recovery store means zero recognized bundles or drafts for Status and
Doctor, or a verified Cleanup no-op. An existing selected workspace bucket that
cannot be read remains unavailable or incomplete under the local command
contract; a final ZIP that fails semantic validation keeps its exact malformed,
unsupported, or unavailable condition. Neither condition is absence.

Status and Doctor do not acquire the workspace lease, report activity, or infer
activity from bundle contents, a filename, age, PID, marker, journal, or the
visible persistent lock file.

Mutation commands follow this visible shape:

```text
resolve and inspect
  -> form a command-local immutable plan
  -> validate policy and collisions
  -> acquire the real workspace lock when applicable
  -> revalidate expected state
  -> prepare and verify one external recovery bundle when existing targets require it
  -> apply bounded filesystem changes
  -> verify resulting identity and bytes
  -> write accepted lifecycle state
  -> remove the command-owned recovery bundle only after whole-command success
  -> form one concrete result
```

Shared mutation support provides file preconditions, atomic replacement,
workspace locking, expected-state revalidation, external recovery-bundle
preparation, bundle verification, and atomic replacement. Each command owns its
plan, effect ordering, findings, and result. No generic engine decides product
behavior or automatically restores, rolls back, or compensates for target
effects.

The lock has one explicit bootstrap boundary. When a fully preflighted Install
or generic Route Init plan requires a missing `.agents` container, that exact
directory is a visible planned and reported support effect. Immediately before
taking `.agents/open-forge.lock`, `WorkspaceLockManager` confirms the missing
contained path, creates it with ordinary `Directory.CreateDirectory`, re-resolves
and verifies it as the expected contained ordinary directory, then opens the
lock. Cancellation before bootstrap creates nothing. The directory remains and
is reported as residual state after later contention, failure, or interruption;
the CLI never removes or compensates for it. This is the sole pre-lease directory
effect and the sole directory excluded from the shared lease-bound applier.

`WorkspaceLockResult.BootstrapOutcome` is nullable in acquired, failed, and
cancelled results. `null` means no directory outcome was successfully observed.
`Existing` means the pre-existing `.agents` directory was validated.
`Materialized` means the manager observed absence, attempted ordinary BCL
creation, and validated the resulting directory. Preserve any reached outcome
when acquisition later fails or is cancelled; an acquired result requires a
non-null outcome. `Materialized` makes no hostile-process creator-identity claim.

Directory creation is one separate shared native effect, not a
`PlannedFileChangeKind` and not an expansion of file replacement. A command plan
names each missing descendant below `.agents` explicitly and orders parent before
child. While holding the workspace lease, the directory applier immediately
revalidates that the target is still missing and that its physical parent is the
exact expected contained ordinary directory, calls ordinary
`Directory.CreateDirectory`, and then verifies the target as the expected
contained ordinary directory.

A verified created directory remains if a later effect fails or the operation is
interrupted. The capability reports it as residual state and never rolls it back,
compensates for it, removes it, or prepares recovery data for it. It uses no
P/Invoke and makes no creator-identity guarantee against a hostile process under
the same user account. Install and Route Init share these exact mechanics when
their directory effects have identical meaning; their plan selection, ordering
among other effects, findings, and result semantics remain command-local.

For each mutating command operation whose complete plan contains one or more
existing-target effects (`Replace`, `ReplaceGeneratedRegion`, or `Delete`),
orchestration resolves
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.Create)` and uses only its application-owned
`OpenForge/recovery/v1` subtree. It never falls back to a temporary directory,
the repository, `HOME`, or a custom platform directory. Unavailable storage is
a pre-effect `incomplete` result. The operation then creates exactly one
immutable ZIP bundle outside the workspace, under a
deterministic key formed from the normalized physical workspace path and the
operation ID. An operation containing only `Create` effects or semantic or byte
no-ops does not resolve recovery storage and creates no bundle. The
source-generated schema-v1 `manifest.json` records
command and operation identity, workspace identity, and ordered relative target
entries. Each entry for an existing-target effect (`Replace`,
`ReplaceGeneratedRegion`, or `Delete`) records its change kind, prior length,
hash, and streamed ordinal payload name, together with the intended final
absence or length and hash. These fingerprints are provenance, not an evolving
journal. The ordered payload entries contain the exact prior bytes for each such
effect; `Create` targets and semantic or byte no-ops have no payload entry.

The writer may select the managed BCL's `CompressionLevel.NoCompression`, but
compression method is not part of schema-v1 recognition or a promised output
property. The format defines no ZIP entry timestamp, deterministic archive
bytes, or whole-archive length or hash. Recognition decodes the source-generated
manifest semantically and validates exact ordered entry names and counts,
declared lengths and hashes, and the exact payload bytes without a raw ZIP
parser.

The draft is created with `CreateNew` under its exact deterministic draft name
in that same external directory. The writer closes and reopens it, completes the
semantic verification above, moves it within the same directory to its
deterministic final name, and reopens and verifies the final bundle. Only a
valid final ZIP may form the opaque `RecoveryBundlePreparation`; a draft remains
`Draft`/`Incomplete` support data and never forms a preparation. The workspace
`FileChangeApplier` requires the matching opaque final preparation for
`Replace`, `Delete`, and `ReplaceGeneratedRegion`. `Create` must receive `null`;
a non-null preparation for `Create` is rejected. The applier performs one final
effect per target, and all required bundle preparation is complete before the
first target effect.

Status, Doctor, and Cleanup may stream each ZIP payload entry through fixed
bounded buffers solely to validate its exact declared length and lowercase
SHA-256. They never extract, disclose, render, log, return, retain, or materialize
payload bytes. Payload size does not increase validation memory beyond the fixed
buffer and hash state.

Each file replacement stages complete intended bytes beside its target and uses
the strongest ordinary atomic replacement that the managed platform supports. A
multi-file operation is not presented as one filesystem transaction. On handled
failure or cancellation, new effects stop and the actual residual draft or final
bundle path is reported. A valid final bundle remains available when the failure
occurs after preparation. The foundation never automatically restores a target,
rolls back an effect, compensates for target effects, or classifies current
target state from recovery provenance. A closed final ZIP may remain after an
abrupt process termination, but the CLI makes no executable crash or power-loss
durability guarantee. A fresh invocation plans again from current facts; the CLI
does not persist a journal, progress receipt, history, or replayable plan.

The mechanical `FileChangeReceipt` distinguishes `Verified`, observed
`VerificationFailed`, verification-unavailable `Applied`/`Failed`,
`NotStarted`, and `CompletionUnknown`. `NotStarted` carries exactly one neutral
reason: `Cancelled`, `TargetChanged`, `ApplicationFailed`, or
`ContractRejected`. `CompletionUnknown` is exclusive to a thrown target effect
whose completion cannot be proved; if post-observation proves the exact intended
state, the receipt is `Verified`.

After the whole command verifies successfully, command orchestration deletes
only the positively recognized bundle it created for that operation.
`Deleted`/`Removed` permits command completion with recovery removed.
`Failed`/`Retained` requires positive remaining presence and permits a
command-specific attention result with the exact residual path.
`Failed`/`Unknown` is a failed recovery outcome whose disposition remains
unknown; an exact expected path is reported only when the guard returns one.
`Blocked` and `Cancelled` remain neutral mechanical facts for command-owned
mapping. Unknown, lookalike, malformed,
different-workspace, mismatched, or otherwise unowned support artifacts remain
untouched. A workspace move is outside the automatic guarantee: rediscovery uses
the same normalized physical workspace path, and Doctor or Cleanup may report
orphaned original-root bundles but never auto-binds or restores them.

The dedicated `cleanup` operation retains its accepted monotonic exception. It
may return a verified empty no-op without acquiring a lease. Before any
deletion, Cleanup acquires the existing same-workspace `WorkspaceLockLease`
through the persistent reusable lock file and `FileShare.None`, then performs one
under-lease re-enumeration and immediate ordinary path/kind and final semantic
revalidation. The
held lease provides cooperating-process exclusion only. If Cleanup cannot
acquire it, Cleanup performs no deletion. Cleanup mechanically deletes only exact named
final or draft candidates for the selected workspace that remain ordinary files
of the expected kind under that final revalidation, then verifies their absence.
It writes no marker, PID, journal, lock metadata, or other lifecycle record and
makes no activity inference.

The deletion guard returns a mechanical state (`Deleted`, `Failed`, `Blocked`,
or `Cancelled`) and an orthogonal disposition (`Removed`, `Retained`, or
`Unknown`). Only a positive current observation can establish `Retained`, which
requires the exact residual path. Observation failure or an exception leaves
disposition `Unknown`; successful deletion plus positive absence is
`Deleted`/`Removed` with no residual.

Cleanup creates no replacement bundle and does not reverse a verified deletion.
Recovery bundles are not extracted by the CLI; recognition uses the semantic
schema, exact ordered entry inventory, declared lengths and hashes, and exact
payload bytes. The implementation uses ordinary managed BCL archive and file
APIs, source-generated serialization, and no custom archive parser, reflection,
native dependency, or extra package. Current-user LocalApplicationData is an
ordinary application-owned storage boundary under the stable workspace and
cooperating-client threat model; no special platform-permission or encryption
promise is made.

Lifecycle state remains `.agents/open-forge.lifecycle.json`, schema version 1.
Every Framework lifecycle target has one required nullable `sourceAssetPath`.
For a payload file or managed root/provider block, this is the normalized
canonical embedded asset-relative path that produced the target. It is `null`
only for a derived generated-region target. Structural validation accepts a
normalized historical source path even when a later embedded inventory no
longer contains it, while publication verifies every new non-null path against
the exact inventory being recorded. User-owned scope entrypoints are not
Framework lifecycle targets. Exact concrete target path, source asset path,
managed region, generated-region identity, and baseline fingerprint provide the
per-effect provenance; schema v1 needs no lifecycle-instance collection or
migration engine.

Root Install owns only the closed base Framework footprint. Its exact no-op and
divergence checks compare that root subset while preserving any other trusted
scoped Framework targets and generated regions in the same Framework lifecycle
section. Update may reconcile recorded targets independently. Route Init may
append scoped targets only after it verifies a trusted current root Install from
the running binary's embedded inventory.

The mutation lock remains `.agents/open-forge.lock`, is persistent and reusable,
and preserves any existing bytes. A mutating operation only holds a
`FileShare.None` handle; it never writes lock metadata and never deletes or
truncates the lock file. `LocalApplicationData` is only the external
recovery-bundle location; it is never the workspace lock location. Existing
legacy lifecycle formats are ordinary untouched content.

## Serialization And Dependencies

JSON uses `System.Text.Json` source generation with reflection disabled. YAML uses
one source-generated static context for accepted Framework metadata shapes.
Concrete command contexts register concrete result graphs. AOT evidence exercises
every registered shape.

Direct package versions are pinned centrally in the repository-root
`Directory.Packages.props`:

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
  and cover source generation, resolved-path containment and aliases, locking,
  recovery, runtime, and Native AOT internal boundaries.
- End-to-end tests invoke the published executable and prove arguments, streams,
  statuses, exits, cancellation, unchanged bytes, and public scenarios.
- TestSupport contains cohesive real-OS workspace and process fixtures shared by
  at least two active projects. Command-specific builders remain in their nearest
  test scope.

Every test has an explicit display name, one durable feature trait, and one
evidence trait. Traits refine selection and never replace project separation.

Each test owns every mutable workspace, home, temporary directory, cache, process,
and support artifact it can affect. Parallel tests share no mutable state.
Snapshots cover stable projections only; safety, identity, effects, and status
remain direct assertions.

Historical or removed tests are evidence only when a current Task maps their
expectation to an accepted contract. Active evidence belongs in the matching
active project. Do not restore old test plumbing or parallel preserved projects.

Evidence is proportional to the changed boundary. Focused leaf evidence is the
default. The first golden slice for an archetype, a recorded integration wave or
shared promotion, and any material public, composition, shared-capability,
safety, serializer, dependency, runtime/toolchain, project/build/package, or
release change runs the complete managed suite and the currently supported
Native AOT gate. A Task records which trigger applies before mutation. An exact
unchanged predecessor may supply a beginning baseline when its projects,
executable, environment, counts, and result are recorded.

## Build, Native AOT, CI, And Artifacts

The CLI uses stable .NET 10 with C# 14, nullable analysis, warnings as errors,
deterministic builds, package auditing, and no prerelease SDK. `global.json`
allows compatible stable feature-band roll-forward.

An ordinary non-RID build of the CLI project publishes a managed, non-AOT
executable to
`artifacts/publish/open-forge-dev/<Configuration>/open-forge-dev[.exe]` and writes
its informational version to `open-forge-dev.version`. Solution and EndToEnd
builds reach the same project boundary. EndToEnd tests discover that explicit
local artifact without environment configuration. Design-time builds skip the
publication; an explicit `OpenForgeSkipDevelopmentPublish=true` MSBuild property
also disables it when a build does not need the development artifact. `dotnet
test --no-build` requires an existing development publication because it
deliberately skips the build dependency.

The currently supported native delivery RID is `linux-x64`. Foundation and
command acceptance use focused managed evidence and the proportional complete
managed/Native AOT policy above. D1 proves the accepted `linux-x64` native build
and smoke, packed installation and invocation, and checksums.

Additional RIDs, signatures, SBOM, provenance, OIDC attestation, and operating-
system support-floor matrices are future delivery expansions. They require a
later explicit maintainer decision and are not implied by the current release
boundary.

The repository CI workflow may live under `.github/workflows/`. It invokes the
root solution and configuration files while project paths remain below
`src/cli/`. Current native and release evidence compiles the EndToEnd project for
the accepted `linux-x64` RID and discovers
`artifacts/publish/<RID>/open-forge/OpenForge.Cli[.exe]` plus its
`OpenForge.Cli.version` marker. Executable-path and expected-version environment
overrides do not exist. CI uploads bounded artifacts; it does not make `.github/`
a C# source root.

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
8. Pure Generated Navigation projection and bounded-region facts after source,
   route, and document facts exist (GN1).
9. Shared mutation, lock, lifecycle, and recovery foundations (M1).
10. Public `index`, including the shared body-free formation expansion and
    command-local selection, orchestration, result, and presentation (I1).
11. Add the native interaction session, embedded Framework distribution,
    lifecycle `sourceAssetPath` provenance, and lease-bound directory-create
    effect as parallel foundations.
12. Implement Extension Create, root Install, and the Route Inspect interaction
    correction in parallel after their required foundations; integrate root
    composition and executable evidence sequentially.
13. Implement generic and Framework-aware `route init` after root Install,
    followed by `route create`, `route update`, `route move`, and `route remove`.
14. Implement root `update`, `extension install`, `extension update`, and
    `extension remove` from their accepted predecessors.
15. Implement `status`, `doctor`, `repair`, and `cleanup` after all producers and recovery
    states exist.
16. Thin package wrappers, supported `linux-x64` build and smoke, packed install
    and invocation, checksums, documentation, and release.

Each command reaches complete contract and focused managed, process, and
unchanged-state acceptance before the next command consumes its facts. Complete
managed and currently supported Native AOT acceptance runs at the recorded first
golden slice, integration wave or shared promotion, material trigger, or an
explicit Architecture, contract, or Task requirement before dependent facts are
accepted.

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

The current final release publishes the canonical `linux-x64` executable,
checksums, and thin package wrappers. Wrappers contain no behavior, download,
postinstall compilation, or fallback runtime. Additional RIDs, signatures, SBOM,
provenance, OIDC attestation, and support-floor claims remain unaccepted future
work until the maintainer explicitly expands this boundary.

No partial command publication is accepted. The replacement becomes shipping only
after the maintainer accepts the complete retained command set, package graph,
native support matrix, documentation, and release evidence.
