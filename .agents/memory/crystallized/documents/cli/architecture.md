---
open-forge:
  description: Accepted Gate 3 architecture for the non-shipping C# replacement CLI, its evidence, and its release boundary
  responsibility: Define the current replacement CLI structure, dependency direction, filesystem and recovery boundaries, test evidence, and delivery shape
  tags: [Memory, Crystallized, CLI, Architecture, Gate, CurrentTruth, Evergreen, DotNet, NativeAOT, Testing, Release]
---

# Open Forge CLI Architecture

## Status And Scope

This is the accepted Gate 3 Architecture for the replacement Open Forge CLI. The
replacement is an optional deterministic Framework accelerator with a human
maintenance surface. It remains explicitly non-shipping. No C# source file,
project, solution, executable, package, or Native AOT artifact exists yet.

Gate 5 implementation is authorized and active through bounded Tasks. Release
proof remains pending. This document accepts the structure and boundaries that
Gate 5 must implement. It does not turn architecture acceptance into
implementation, AOT, package, or release evidence. The active foundation spike
and its stop conditions are Gate 5 evidence, not current proof.

The [Command Contract Set](command-contract-set.md) defines the current
command-contract roles, topology, and authority boundaries. The [Shared CLI
Operation Contract](shared-operation-contract.md) defines cross-command
conventions, and the detailed [command contracts](contracts/_contracts.md)
define exact command-local meaning. This Architecture defines the high-level
implementation structure and the cross-command boundaries that realize those
contracts.

The [Framework Architecture](../framework/architecture.md) and its routed
Markdown, authority, and lifecycle contracts remain authoritative for the
meaning of the files that the CLI reads or changes. The CLI never becomes a
private source of Framework truth.

## Replacement Boundary

The accepted design specifies one future production executable, `OpenForge.Cli`,
eventually exposed as the `open-forge` command. It is separate from the frozen
`open-forge-old` executable:

- `src/cli-mvp/` contains the frozen TypeScript source for `open-forge-old`, at
  [`src/cli-mvp/cli.ts`](../../../../../src/cli-mvp/cli.ts). Neither
  `src/cli-mvp/` nor `open-forge-old` is replacement implementation or contract
  authority. `open-forge-old` remains available for its existing
  repository-routing assistance and historical evidence.
- The accepted design places the future replacement executable's source under
  `src/open-forge-cli/OpenForge.Cli`. That source tree is not present yet.
- `src/cli-mvp/build/` contains the frozen executable's TypeScript build support.
  It remains part of the frozen MVP boundary and is not replacement source.
- The replacement does not import, dispatch to, build, test, or fall back to
  `open-forge-old`. There is no compatibility router, command alias layer,
  behavior selector, migration path, or staged control of one command root.

The replacement accepts only the current command contracts. An unimplemented or
invalid replacement command is an ordinary replacement result; it is never
silently sent to the frozen executable. Existing old-format workspace files are
outside replacement authority and remain untouched.

## Product Boundary And Command Tree

The CLI makes deterministic inspection and bounded file operations cheap,
repeatable, inspectable, and mechanically verifiable. It does not infer product
intent, approve candidate Memory, execute agent reasoning, provide semantic or
fuzzy search, define Framework meaning, or maintain a private workspace database.

The retained delivery surface is exactly this command tree:

```text
open-forge find
open-forge index
open-forge status
open-forge context
open-forge references
open-forge doctor
open-forge repair
open-forge install
open-forge update
open-forge cleanup
open-forge route inspect
open-forge route list
open-forge route init
open-forge route create
open-forge route update
open-forge route move
open-forge route remove
open-forge extension list
open-forge extension inspect
open-forge extension create
open-forge extension install
open-forge extension update
open-forge extension remove
```

`route` and `extension` are real groups of related operations. A group performs
no domain operation and its bare form shows help. Every leaf performs one
complete operation. Shell completion is not part of the product, so there is no
completion command, generator, profile edit, or completion dependency.

## Runtime, Solution, And Physical Topology

The replacement targets C# on .NET 10 or newer, with `net10.0` as the initial
target. Gate 5 pins the SDK to `10.0.101`, uses `rollForward=latestPatch`, and
sets C# `14.0`. The SDK policy must not silently move to another feature band. A
future intentional baseline change is a new Architecture decision, not an
implementation convenience.

The solution is `OpenForge.slnx`. It contains one production project,
`OpenForge.Cli`, specified to build the one future production executable. The
solution does not introduce production class-library projects merely to create
architectural layers. Test projects are not shipped executables.

The required physical topology is:

```text
OpenForge.slnx
src/
  open-forge-cli/
    OpenForge.Cli/
      OpenForge.Cli.csproj
      ... command and capability source ...
tests/
  open-forge-cli/
    OpenForge.Cli.Tests/
      OpenForge.Cli.Tests.csproj
      ... mirrored managed unit and integration tests ...
    OpenForge.Cli.SystemTests/
      OpenForge.Cli.SystemTests.csproj
      ... complete-boundary system and end-to-end tests ...
```

The source project is physically rooted at
`src/open-forge-cli/OpenForge.Cli`. The managed test tree is separate and
physically rooted at `tests/open-forge-cli/OpenForge.Cli.Tests`. Its production
subject paths mirror the source paths, so a command or capability has one
obvious corresponding test location. `OpenForge.Cli.SystemTests` is the separate
system/E2E project for the complete CLI boundary rather than a unit-test seam.

The shown tree is the accepted future topology, not a report of present files.
No C# files or C# projects are present yet. Every solution folder must correspond
to a real physical folder. The `.slnx` must contain no solution-only virtual
folders that have no filesystem counterpart, and physical source or test folders
must not be hidden behind invented solution groupings.

Gate 5 centrally records exact package versions in `Directory.Packages.props`
and restore locks in committed `packages.lock.json` files. The central
dependency policy and lock files are part of the implementation evidence, not
untracked per-project version drift. The SDK pin, project settings, dependency
versions, lock state, and publish inputs must reproduce the same restore and
build without floating package resolution.

## Source Organization And Composition

The composition root builds the explicit command tree, creates the concrete
capabilities it needs, binds parsed values to a command-local request, invokes
the command, selects the renderer, and maps the final status to the process
exit. It is the only place that composes unrelated command branches.

Each command keeps its own local slice. The normal shape is:

```text
Commands/<command-or-group>/<operation>/
  Request.cs
  Facts.cs
  Result.cs
  Renderer.cs
  ... local planning and mutation stages ...
```

The exact files may split when a command grows, but the responsibilities remain
local and named. `Request` represents complete validated operation input.
`Facts` represents the current invocation's inspected state. `Result` represents
the concrete semantic outcome. `Renderer` projects that result to the accepted
human or structured surface. A handler does not write streams, choose a view, or
rerun the operation.

Mutating commands keep their local plan, preflight, stage, revalidation, apply,
verification, and recovery coordination beside the command that coordinates the
mutation. Read-only commands stop after their typed result. Dry-run and apply
use the same request, facts, planner, and preflight; dry-run stops before
persistent effects.

The command tree is bound explicitly. Command registration is visible in source,
and manual binding maps parser values to concrete request types. Command-local
meaning is not discovered from strings, reflection, a registry, or a universal
operation table.

Use direct construction and pure capability-named static functions or extensions
first. Promote code only when multiple real consumers demonstrate the same
meaning, and place it at their nearest common physical scope. A shared capability
must remain explicit at each call site and must not hide a command's subject,
authority, stages, effects, or result.

The architecture prohibits a universal operation engine, reflective dispatch,
string-keyed behavior registries, a service locator, a fake filesystem, a virtual
filesystem hierarchy, and a remote `utils` folder. It does not prescribe classes
over records or functions; local state, lifecycle, resource ownership, and
clarity decide. Dependency injection is permitted only when real composition or
lifecycle proves it is needed. Any DI path must be source-generated and
Native-AOT-safe. A general container is not the default composition model.

## Dependencies And Native AOT Boundary

The accepted runtime dependencies are exact and deliberately narrow:

| Concern                            | Dependency and accepted boundary                                                                                         |
| ---------------------------------- | ------------------------------------------------------------------------------------------------------------------------ |
| Command parsing                    | `System.CommandLine` 2.0.11, with an explicitly constructed command tree and manual binding to concrete requests.        |
| Markdown facts                     | Markdig 1.3.2 through one fixed CommonMark pipeline. The pipeline has no plugin discovery or runtime extension loading.  |
| YAML facts                         | YamlDotNet 18.1.0 through the source-generated semantic path for the accepted typed models, with no reflective fallback. |
| JSON results and lifecycle values  | `System.Text.Json` source-generated concrete metadata. Reflection-based serialization and deserialization are disabled.  |
| Filesystem and process foundations | The cross-platform .NET BCL first, especially `System.IO`, with no speculative platform layer.                           |
| Test execution                     | xUnit v3 through Microsoft Testing Platform (MTP), with the evidence rules below.                                        |

System.CommandLine defines only the explicit syntax tree and parser boundary. It
does not define domain behavior. Markdig parses the bounded Markdown facts needed by
the operation. YamlDotNet parses the accepted semantic YAML models. Neither
library renders a whole document. The CLI does not render a whole Markdown
document or a whole YAML document to produce a mutation. It reads facts and
applies bounded lossless patches.

Every dependency, parser path, serializer path, source-generator path, runtime
feature, and DI path must pass a real Native AOT publish. Source inspection,
package claims, a nominal `PublishAot` property, or a successful managed build
is not proof. Gate 5 requires warning-free restore, compilation, trimming, and
Native AOT publish evidence for the complete production executable. Warnings may
not be hidden to make the evidence pass. A dependency that cannot meet the
boundary is removed or returned to Architecture; it is not wrapped in an
unproved compatibility shim.

## Source Bytes, Parsing, And Semantic Identity

The CLI treats source bytes as the primary preservation boundary.

- Supported text sources must be valid UTF-8. The implementation records exact
  source bytes and maps parsed facts to byte ranges. Line and column coordinates
  are derived from those same bytes; they do not replace byte offsets and
  lengths.
- A patch replaces only explicitly selected ranges or generated interiors. All
  unrelated bytes, encoding, line endings, labels, fragments, and surrounding
  authored content remain byte-for-byte unchanged.
- Markdown parsing produces headings, visible text, links, sections, ignored
  regions, generated-region boundaries, and source coordinates. YAML parsing
  produces only the accepted semantic fields and their coordinates. Parsing does
  not make a renderer or a second authored document.
- The accepted conservative semantic fingerprint identifier is exactly
  `open-forge-markdown-v1`. Its length-delimited, domain-separated input removes
  a UTF-8 byte-order mark, normalizes CRLF and CR to LF, and preserves every
  other Unicode scalar, authored spelling, order, and whitespace. It replaces
  only a parser-proven generated `Entries` interior with one typed sentinel that
  contains neither the omitted bytes nor their length, while retaining the exact
  normalized marker boundaries. It does not normalize YAML quoting or order,
  Markdown marker style, emphasis, list style, Unicode, case, or ordinary
  whitespace. Unsupported or ambiguous equivalence fails closed; unsupported,
  binary, or unparseable managed content uses domain-separated exact-byte
  identity where its local contract permits it.
- The fingerprint is an identity and comparison aid, not permission to rewrite
  a document. The CLI does not run a formatter, persist a formatter receipt, or
  claim that semantically equal bytes are interchangeable when the parser cannot
  prove that fact.

Every invocation builds an in-memory content graph only when its selected
operation needs relationships. The graph contains the parsed sources and the
explicit route, loading, overwrite, section, and link relationships needed by
that invocation. It is discarded at invocation end. There is no persistent
content cache, session, receipt, hidden index, remote graph, or private truth.
Generated `Entries` are a derived projection. They never become the source of
topology, authority, or semantic meaning.

## Lifecycle State And Legacy Boundary

The replacement's only lifecycle document is:

```text
.agents/open-forge.lifecycle.json
```

It uses schema version 1 with one common envelope and two isolated logical
sections, `framework` and `extensions`. The common envelope carries the shared
workspace and schema/fingerprint identity required to validate the document.
Subject facts stay in their own section. A Framework operation reads and writes
only `framework`; an Extension operation reads and writes only `extensions`.

An operation patches its selected section while preserving the exact bytes and
meaning of the common envelope and unrelated section. It never drops, rewrites,
normalizes, migrates, or repairs unrelated state as a side effect. If exact
preservation, parsing, round-tripping, or verification is not possible, the
operation writes nothing and returns the contract's incomplete or blocked result.
Cross-section identity or path collisions block preflight.

The replacement has no legacy compatibility for lifecycle state. It does not
read, recognize, migrate, alias, or fall back to any old-format lifecycle or
Extension file, including the old `open-forge.extensions.json` receipt. Existing
old-format files remain outside new-CLI authority and are untouched. A missing
new document is not permission to interpret an old document as schema version 1.

Lifecycle metadata records transparent operational facts. It does not define
Framework routing, Memory authority, Extension runtime meaning, a session, a
saved plan, a cache, or a private database of workspace truth.

## Filesystem, Platform, And Workspace Lock

The CLI uses the real cross-platform .NET BCL and `System.IO` as its first
filesystem boundary. It proves lexical containment, physical identity,
workspace association, supported file identity, and expected-state conditions
at the real filesystem boundary. Tests use real operating-system temporary
directories and real files, directories, links, processes, and permissions where
the selected guarantee depends on them.

The implementation must not add speculative Windows, Linux, or macOS code. If a
critical guarantee cannot be proved with cross-platform .NET APIs on the
supported floor, implementation stops and returns to Architecture before a
narrow platform adapter is proposed. It must not silently weaken the guarantee,
use a fake filesystem, or make a platform promise from an untested assumption.

`.agents/open-forge.lock` coordinates operations that mutate the selected
workspace. The exact workspace lock path is:

```text
.agents/open-forge.lock
```

The lock is an actual operating-system file lock held through the mutation
planning, application, verification, and recovery boundary for those operations.
The implementation uses the cross-platform `System.IO` file-handle lock boundary
and proves its OS semantics at Gate 5. Merely finding the file does not mean that
the workspace is locked. The implementation must open the file and acquire the
OS-level exclusive lock, retain the handle for the operation, and release it when
the handle closes.
A process crash releases the OS lock even if the file remains.

An unlocked lock file may be reused or removed only after the implementation has
successfully established that no process currently holds the lock. There is no
stale-PID heuristic, force-delete path, or existence-based bypass. An active
lock blocks another mutating operation. Read-only operations do not acquire
mutation authority merely to inspect the file.

`extension create` has no workspace subject and therefore does not acquire that
lock; it uses exact catalogue-destination identity, expected-state revalidation,
Git/recovery, and collision guards. This is the only current no-workspace
mutation exception.

## Mutation, Recovery, And Cleanup

Every mutating command has one explicit local flow:

```text
validated input
  -> complete Request
  -> current Facts
  -> complete plan
  -> preflight
  -> stage
  -> revalidate
  -> apply
  -> verify
  -> recover when required
  -> cleanup boundary
  -> concrete Result
  -> Renderer
```

The complete plan contains every selected effect, target identity, expected
current state, intended bytes, generated projection, lifecycle-section change,
Git policy, staging or backup readiness, verification condition, and recovery
condition before the first persistent effect. Preflight rejects any incomplete,
ambiguous, colliding, unsafe, dirty, or unauthorized effect as one plan; the
operation does not apply a safe subset around it.

Staging prepares contained temporary material and required adjacent recovery
artifacts. Revalidation checks the plan's volatile assumptions immediately
before each effect. Application uses the planned bytes or bounded regions, then
verifies each effect and the complete operation. Handled failures reverse applied
effects in reverse order only while identity guards still match. Unexpected
concurrent edits are preserved as residual state rather than overwritten by
recovery.

This is not a global transaction and makes no power-loss atomicity claim across
multiple files. A process, machine, or power failure can leave a residual
artifact or partially applied set. The next invocation must inspect fresh facts,
report residual state, and use only a valid recovery identity; it never replays a
saved plan or assumes that all files changed atomically.

Recovery artifacts use a structured provenance identity envelope. The envelope
binds the workspace identity, operation identity, target logical and physical
identity, artifact kind, expected before/after identity, and recovery state.
Recovery acts only when that envelope and the current target satisfy the
identity and containment guards. The envelope is provenance evidence, not a
cryptographic signature. The threat model protects against accidental or
unrelated collisions and concurrent changes. It does not protect against a
same-user actor who can deliberately forge both the workspace content and the
provenance envelope; that same-user deliberate-forgery limit is accepted.

Git remains the affected-path review and cleanliness boundary when it can classify
the target. `--skip-git-check` changes only that cleanliness check; it never
grants overwrite, delete, ownership, identity, containment, verification, or
recovery authority. Adjacent backups and structured recovery evidence remain
required where the local operation needs them. Backups are removed only after
complete verification proves they are no longer needed.

`cleanup` is a narrow explicit exception. It builds a fresh catalogue and may
delete only inactive artifacts whose positive Open Forge provenance, workspace
association, physical containment, and expected identity are proved. It does
not delete arbitrary backups, user files, old-format lifecycle files, receipts,
generated navigation, source content, build output, package caches, or anything
identified only by a suffix, age, location, or temporary-looking name. It does
not create a replacement backup, staging copy, receipt, journal, or tombstone.
Verified deletions are not reversed after a later failure or interruption; the
remaining catalogue is visible to a fresh invocation. Cleanup never becomes a
hidden Index, Doctor, Repair, lifecycle, package, or release operation.

## Result, JSON, Coordinates, And Process Status

Every command forms one concrete typed result. Human output and JSON consume
that result and never rerun the operation. Result types are concrete and
source-generated. Polymorphic reflection and untyped object graphs are not a
serialization escape hatch.

The structured output protocol is fixed at top-level schema version 1. Its
top-level envelope contains exactly:

```text
schemaVersion
command
status
workspace
result
next
```

`schemaVersion` is the integer `1`. `result` is the concrete result for the
selected command, not a generic map.
`next` is one concrete next action or `null`, never an array or an unbounded
recommendation list. `workspace` records the exact selected workspace and
selection method. Source facts and findings carry canonical coordinates, with
workspace-relative paths and authoritative UTF-8 byte ranges. All paths use one
canonical spelling, and all arrays, findings, effects, sources, and coordinates
use explicit deterministic order independent of filesystem enumeration order.

The semantic statuses and process exits are fixed:

| Status        |  Exit |
| ------------- | ----: |
| `complete`    |   `0` |
| `failed`      |   `1` |
| `attention`   |   `2` |
| `incomplete`  |   `3` |
| `invalid`     |   `4` |
| `blocked`     |   `5` |
| `interrupted` | `130` |

Help and version exit `0`. Human `complete`, `attention`, and `incomplete`
results use stdout. Human `invalid`, `blocked`, `failed`, and `interrupted`
results use stderr. JSON emits its one complete result on stdout for every
semantic status; bounded diagnostics use stderr and never contaminate JSON.

## Test Architecture And Evidence

Tests use xUnit v3 through Microsoft Testing Platform. Every `Fact` and `Theory`
has an explicit readable `DisplayName`. Every test declares a durable feature
trait and exactly one evidence trait with one of these values:

```text
Feature=<command-or-capability>
Evidence=Unit
Evidence=Integration
Evidence=EndToEnd
Evidence=PackageEndToEnd
```

`Feature` names the command or capability under test. The evidence value names
the boundary crossed. MTP selection must be able to run these categories
independently without relying on filename conventions or random prose.

- Unit tests cover every cheap pure parser, value, coordinate, planner,
  fingerprint, ordering, and result function that can be proven without a
  process or filesystem effect.
- Integration tests use real production modules and real command composition
  across the boundary they claim. Filesystem integration uses real OS temporary
  workspaces, not an in-memory or fake hierarchy.
- End-to-end tests execute the complete built Native AOT CLI and assert the
  process boundary, arguments, streams, exit, workspace bytes, and structured
  result.
- Package end-to-end tests invoke the npm launcher and its optional platform
  package boundary, not a test-only direct method.

Every mutable test resource is created in an isolated OS temporary workspace or
an equally isolated OS temporary resource. Tests own their files, directories,
processes, lock handles, environment, and package state, and are safe to run in
parallel. Shared fixtures are immutable or independently copied. A test may not
depend on another test's cleanup or on the repository's mutable workspace.

Embedded Framework and catalogue assets are proved once at the focused build or
package boundary through an inventory and exact hashes. Other tests verify
behavior and typed facts rather than repeating large embedded prose. Tests do
not assert random prose, incidental formatting, timestamps, or filesystem
enumeration order. Focused snapshots are allowed only when one stable public
projection is clearer than structured assertions; snapshots do not replace
semantic, byte, safety, or process evidence.

## Distribution And Release Boundary

The accepted release design specifies publication of the canonical executable
for exactly these six runtime identifiers:

| RID           | Optional npm package                       |
| ------------- | ------------------------------------------ |
| `win-x64`     | `@thelithiumforge/open-forge-win32-x64`    |
| `win-arm64`   | `@thelithiumforge/open-forge-win32-arm64`  |
| `linux-x64`   | `@thelithiumforge/open-forge-linux-x64`    |
| `linux-arm64` | `@thelithiumforge/open-forge-linux-arm64`  |
| `osx-x64`     | `@thelithiumforge/open-forge-darwin-x64`   |
| `osx-arm64`   | `@thelithiumforge/open-forge-darwin-arm64` |

The launcher package is `@thelithiumforge/open-forge`. It selects and invokes
the installed optional native package for the current supported platform. The
launcher and platform packages contain no postinstall script, download step,
compilation step, or behavioral wrapper. They do not reimplement command
parsing, filesystem work, output, or recovery. The accepted design makes the
future native executable the only behavior implementation.

The initial support floors follow the current official .NET 10 policy. The
`win-x64` floor is Windows 10 1607 LTSC or Enterprise, or Windows Server 2012
with its required prerequisites and extended support; the `win-arm64` floor is
Windows 10 1607 LTSC or Enterprise because the policy lists no Arm64 Windows
Server floor. Both macOS RIDs start at macOS 14. Both portable 64-bit Linux RIDs
start at glibc 2.27. The first release has no musl artifact.

These policy floors are targets, not product evidence. Each RID needs execution
evidence on its applicable support floor, not only cross-compilation or
execution on a newer hosted runner. Release artifacts carry checksums and
signatures, an SBOM, and build provenance. The release workflow uses OIDC for
trusted attestation and publication credentials. Publication is main-only.
Feature and development branches may build evidence, but they cannot publish
release artifacts or packages.

## Gate 5 Acceptance Boundary

Gate 5 starts from this accepted structure and proves, at minimum:

1. The pinned SDK, C# 14 build, `OpenForge.slnx`, exact central dependencies,
   committed locks, warning-free managed build, and warning-free Native AOT
   publish are reproducible.
2. System.CommandLine 2.0.11, Markdig 1.3.2, YamlDotNet 18.1.0, source
   generation, disabled JSON reflection, the fixed Markdown pipeline, and the
   real `System.IO` boundary work in the published executable.
3. The six RIDs execute at their support floors, including the actual workspace
   lock, lifecycle preservation, UTF-8 ranges, lossless patches, recovery
   provenance, and cleanup boundaries.
4. Unit, integration, complete Native AOT end-to-end, and npm package
   end-to-end evidence passes with the required traits, display names, real
   isolation, inventory/hash checks, and deterministic assertions.
5. Every retained command in the accepted command tree is implemented and
   verified. There is no partial `context` plus `find` publication and no
   retained-later release slice.
6. Checksums, signatures, SBOM, provenance, OIDC, main-only publication, and
   the thin launcher journeys are accepted before any release claim.

If a foundation spike cannot prove a critical guarantee, implementation stops
and returns to this Architecture for a narrow decision. A managed build, a
source-level claim, a package's marketing claim, or a partial command slice
does not satisfy Gate 5. Until that evidence exists, the replacement remains
non-shipping and `open-forge-old` remains the only executable CLI reference.

## Related Current Views

- [CLI route](_cli.md)
- [Command Contract Set](command-contract-set.md)
- [Shared CLI Operation Contract](shared-operation-contract.md)
- [Detailed command contracts](contracts/_contracts.md)
- [Frozen MVP Architecture](mvp-architecture.md)
- [Open Forge Architecture](../architecture.md)
- [Framework Architecture](../framework/architecture.md)
- [Extensions MVP Architecture](../extensions/architecture.md)
