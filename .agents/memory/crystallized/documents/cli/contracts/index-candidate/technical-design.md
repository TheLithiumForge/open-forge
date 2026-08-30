---
open-forge:
  description: Current accepted technical design for the non-shipping `index` command
  responsibility: Define the accepted command-local realization subordinate to the `index` contracts and CLI Architecture
  tags: [Memory, Crystallized, CLI, Release, Command, Index, TechnicalDesign, CurrentTruth]
---

# Index Technical Design

## Status And Authority

This is the current accepted Technical Design for the non-shipping `index`
command. The [Interface Contract](interface.md) and [Behavior
Contract](behavior.md) remain authoritative for public and technology-neutral
meaning. The accepted [Open Forge CLI Architecture](../../architecture.md#result-json-coordinates-and-process-status)
defines the shared exact structured-result schema and numeric process-exit
mapping. This design is subordinate to those contracts and that Architecture.

The design cannot add a public flag, weaken a safety invariant, change a
semantic result, or make a private implementation artifact authoritative for
user-authored meaning. It records the accepted realization and accepted evidence
at exact feature candidate
`4e89d945b38a2d1e24600dd22789b55e4395a534`. Local squash integration is proved
at `09aa03eddb97831ff544afe1eac54ad9af501f5c`: its tree
`2dcfca18020980a9cafbc429a72930af3368df5f` exactly equals final Index closeout
tip `2b353c48978ee88e53345be8037776181612222c`. The command does not ship.

## Accepted Runtime Boundary

The implementation is C# on .NET 10 or newer in the modern `.slnx` solution
shape. Native AOT and trimming compatibility apply to every runtime feature,
dependency, and serialization path. The accepted candidate passes the portable
`linux-x64` Native AOT Integration and EndToEnd gates plus root publish and
version smoke. This command-level evidence does not make the replacement CLI a
shipping release.

The command uses real BCL-first `System.IO` filesystem boundaries and real
isolated temporary directories for filesystem tests. It does not introduce a
virtual filesystem, fake hierarchy, or filesystem abstraction only to make
tests convenient. Physical identity, containment, expected-state revalidation,
safe replacement, verification, concurrency, and recovery remain real
filesystem concerns.

GN1 remains Complete. I1 adds one neutral shared generated-navigation formation
boundary before public Index composition. It consumes one complete
`SourceCatalogue`, uses the existing `SourceRouteTopologyBuilder`, and returns
one cohesive immutable formation containing intended sources, topology, the
Loader fact, proven alias groups, ambiguities, and issues. It is body-free: it
does not read Markdown bodies, discover generated lines, select Index targets,
or own command policy. Existing `GeneratedNavigationProjectionRequest` is
refined to consume that formation instead of separately accepting independently
assembled topology and source collections.

Mutating application coordinates through the persistent external zero-byte
workspace operation lock below `LocalApplicationData/OpenForge/locks/v1`. The
lock is held through the mutation planning, application, verification, and
recovery boundary. It does not become route
content or a source of workspace meaning, and it does not replace
expected-state checks. Dry-run and read-only inspection do not acquire mutation
authority merely to inspect. Lock lifecycle and platform mechanics follow the
accepted Architecture.

## Command And Result Boundaries

`System.CommandLine` is the accepted command parser. The accepted Architecture
pins its dependency version and owns the exact dependency evidence. The parser
owns command syntax, positional operands, command-specific flags, shared flags,
help, version, input validation, and terminal-input handling at the process
boundary. It does not own `index` selection, projection, mutation, verification,
or recovery meaning.

The accepted Architecture selects System.CommandLine 2.0.11, Markdig 1.3.2,
and YamlDotNet 18.1.0. The Architecture remains authoritative for those versions.
The accepted candidate evidence recorded below exercised those dependencies in
managed and Native AOT execution; this design does not independently repin them.

The operation remains a directly testable typed flow:

```text
parsed input
  -> complete request
  -> complete SourceCatalogue
  -> neutral generated-navigation formation
  -> normalized logical selection
  -> complete expected projection
  -> complete ordered plan
  -> preflight
  -> dry-run or application
  -> verification and retained partial-state reporting
  -> one typed result
  -> human or structured rendering
  -> process completion
```

The handler and domain stages return typed values. Human and structured
renderers consume the same typed result and do not rerun the operation. The
shared exact JSON schema and numeric exits come from the accepted [Open Forge CLI
Architecture](../../architecture.md#result-json-coordinates-and-process-status); this command adds no
private schema or exit mapping.

Index owns binding, source-reference resolution, rooted/detached target closure,
command planning, orchestration, result formation, and presentation. Formation
and Generated Navigation return immutable facts only. M1 owns workspace locking,
post-lock expected-state revalidation, recovery preparation, atomic one-change
application, receipts, and verification mechanics. Index consumes those
callables; it does not create command-local substitutes or turn Generated
Navigation into an applier or universal coordinator.

## Markdown, YAML, And Byte Boundaries

Markdig is used through one fixed CommonMark pipeline only where Markdown
structure is needed. The pipeline supplies the structural facts required to
recognize the accepted final `Entries` boundary and relevant routed Markdown
structure. Its configuration is not selected per file or inferred from authored
content.

YamlDotNet is the accepted generated semantic path for the bounded authored
metadata models that the source contracts admit. The accepted Architecture pins
its dependency version and evidence. It contributes typed semantic facts
without inventing descriptions, tags, routing, scope, or authority, and it does
not rewrite authored YAML bytes.

Source reads use strict UTF-8 validation and retain exact UTF-8 byte ranges. A
local generated-region scanner establishes the marker pair and returns the byte
range of the generated interior. It fails closed when the accepted ownership
boundary cannot be established. The scanner handles bounded replacement; it is
not a whole-document formatter.

The design never renders a whole Markdown or YAML document back from parsed
facts. Markdig provides structure where structure is needed, while replacement uses the exact
local byte range and preserves every byte outside the generated interior,
including authored frontmatter, headings, prose, links, whitespace, line
endings, and marker tokens. Serialization emits stable accepted generated
bytes, canonical containing-file-relative destinations, and no query strings or
fragments.

Formation admits Loader roots only from structurally valid, physically unique
recognized entrypoints directly representing `.agents/<slug>` folders. Missing
Loader means zero roots; missing intermediate entrypoints remain detached; and
multiple recognized entrypoints representing one root folder are ambiguous.
Only aliases proven on the current host collapse, and only when route and
recognized document-form identity are compatible. Proven incompatible aliases
remain blocking facts. The design adds no speculative portable case, Unicode,
or device-name equivalence and does not change current-visible Route or Context
facts.

## JSON And Presentation

Structured output uses source-generated `System.Text.Json` metadata for the
shared Architecture schema. `--json` renders one complete document from the
same typed result used by human output. It never prompts, reruns planning,
applies effects, or mixes ordinary human text into JSON stdout. Bounded
diagnostics remain on stderr.

The design keeps the Interface Contract's stream assignment, compact and
expanded views, exact dry-run diffs, semantic statuses, and next-action rules.
It does not expose private staging, recovery-bundle payload, or other recovery
material through an ordinary result.

The source-generated Index result graph preserves the exact top-level member
order `mode`, `selection`, `regions`, `recovery`, `findings`, `counts`; normalized
logical selection contains no raw operand. `IndexSelectionV1` preserves
`origin`, `scope`, `sources`; `IndexLogicalSourceV1` preserves `id`, `path`,
`scope`; `IndexRegionV1` preserves `source`, `action`, `beforeEntryCount`,
`expectedEntryCount`, `change`, `outcome`; `IndexChangeV1` preserves
`beforeBody`, `expectedBody`; `IndexRecoveryV1` preserves `state`,
`residualPath`; `IndexFindingV1` preserves `code`, `status`, `sourceOccurrence`,
`source`, `cause`, `candidates`; and `IndexCountsV1` preserves `regions`,
`updates`, `unchanged`, `applied`, `verified`. All members are required.
Nullable values serialize explicit `null`; `sources`, `regions`, `findings`, and
`candidates` are never null. The finite sets, coherence, counts, exact 25
finding-code/status mapping, finding ordering, status precedence,
recovery/residual-path rules, and deterministic `next` matrix are implemented
once from the Interface Contract and fail closed for undefined values.

Projection delegates generated-interior interpretation only to
`SourceGeneratedEntriesParser`. Each projected region retains a nullable
command-local before count tied one-to-one to that region: the count exists only
for a completely parsed bounded interior; a valid replaceable but unparseable
interior retains `null`. Available projections always retain a non-null expected
count. No second generated-entry parser or cause-string classification exists.

Human dry-run presentation emits every exact generated-interior diff in compact
and expanded views. It constructs the JSON-escaped header from typed source
facts, tokenizes before and expected bodies with exact LF/CRLF retention, and
does not truncate, elide, add context, or inspect bytes outside the generated
interior. JSON retains the exact before and expected bodies rather than the
textual diff.

## Safe Replacement And Recovery

When an operation has one or more existing-target effects (`Replace`,
`ReplaceGeneratedRegion`, or `Delete`), application
uses one immutable ZIP recovery bundle for that complete
operation, outside the workspace in the current user's
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.Create)/OpenForge/recovery/v1` subtree. No
temporary, repository, `HOME`, or custom-platform fallback exists.
The deterministic storage key combines the normalized physical workspace path
and operation ID. A source-generated schema-v1 `manifest.json` carries
command/operation/workspace identity, ordered relative targets, change kinds,
prior lengths/hashes/payload names, and intended final absence or length/hash.
Streamed ordinal payload entries contain the exact old bytes for every
existing-target effect. This is recovery provenance, not an evolving journal or a second
authority for workspace meaning.

The draft is written with CreateNew under its exact name in the same external
directory, closed and reopened for semantic manifest, exact ordered entry,
length, hash, and payload-byte validation, moved within that directory to the
deterministic final name, and reopened and verified again. Only the valid final
ZIP forms the opaque `RecoveryBundlePreparation`; the draft remains
`Incomplete`. `FileChangeApplier` requires that matching
preparation for every existing-target effect; Create and no-op effects have none, and
all preparation finishes before the first target effect. Each target uses the
accepted same-directory safe-replacement property, never edits in place, and
never falls back to a weaker write. The target and complete projection are
verified after effects. Successful preparation is Index's apply boundary;
per-target drift after it maps to `index.target-changed-during-apply`, leaves
that region `not-started`, retains the final bundle, and stops new effects.

After whole-command verification, delete only the positively recognized bundle
created by that operation. `Deleted`/`Removed` maps to recovery `removed` and,
absent another finding, `complete`. `Failed`/`Retained` requires positive
remaining presence and maps to `attention`,
`index.recovery-artifact-retained`, and the exact residual path.
`Failed`/`Unknown` maps to `failed`, `index.recovery-failed`, and recovery
`unknown`, carrying the exact expected path only when M1 returns it. `Blocked`
and `Cancelled` remain neutral typed M1 facts until the operation maps its
context. A handled application, verification, or cancellation failure after
preparation but before post-verification deletion stops new effects and reports
the exact final path; that positively verified final remains because deletion
has not begun. A deletion result with disposition `Unknown` makes no retention
claim. A closed final ZIP may remain after abrupt process termination,
without an executable crash or power-loss guarantee. An unexpected concurrent
edit is preserved and reported as residual state. The bundle is never extracted
or used to restore a target, and current target state is not derived from its
provenance. Cleanup owns exact named final and draft deletion under its separate
lease-bound contract. A rerun computes fresh facts and never replays a
saved plan, receipt, journal, history, or progress record.

The result adapter maps M1 evidence without guessing: dry run, no-op, and no
existing target are `not-required`; required-but-never-created is `not-created`;
positive removal is `removed`; positive presence is `retained`; and unprovable
disposition is `unknown`. An exact residual path is mandatory for `retained` and
may accompany `unknown` only when M1 returns the exact observed or expected
support path.

Index consumes `WorkspaceLockResult` without inference: `Acquired` continues;
`Failed` with `InvalidPath` maps to `index.workspace-unsafe`; `Failed` with
`AccessDenied`, `InputOutput`, or `Unsupported` maps to
`index.workspace-lock-unavailable`; and `Cancelled` maps to
`index.interrupted`. No lock result is reclassified as contention, and no cause,
exception, or HResult text is parsed.

## Test Design And Evidence

Tests mirror the Interface, Behavior, and technical boundaries. Managed unit
and integration test subjects mirror their production command or capability
paths, while complete process tests remain in the separate system boundary.
Direct tests cover typed request normalization, resolved-path and alias behavior, topology-derived
projection, metadata admission, canonical output, byte ranges, marker
ownership, ordering, result formation, and no-op behavior. Focused integration
tests use real temporary rooted and detached source trees, external recovery
bundles, filesystem failures, expected-state changes, concurrency changes,
safe replacement, verification, residual preservation, interruption, and rerun
convergence.

Focused evidence also freezes complete-catalogue formation, present/missing
Loader behavior, unique root-folder representation, detached missing-
intermediate topology, compatible/incompatible aliases, unchanged Route/Context
facts, all finite result values and coherence rules, every one of the 25 finding
mappings, finding/status/next ordering, recovery ambiguity, and exact non-
truncating diff round trips.

At exact candidate `4e89d945b38a2d1e24600dd22789b55e4395a534`, verification
used the existing cached and offline prepared dependency state. The Release
solution build is warning-free. Managed Unit `1206/1206`, Integration `480/480`,
and EndToEnd `125/125` pass. Portable `linux-x64` Native AOT Integration
`480/480`, EndToEnd `125/125`, and root publish/version smoke pass. Every stated
test run has zero failures and zero skips.

Controlled public scenarios prove safe dry run, JSON output, application, and
second-run idempotence. Application changes one exact bounded generated interior,
preserves unrelated tracked content, and returns the tracked aggregate hash to
its baseline after the apply and idempotence sequence. This evidence claims no
fresh remote NuGet vulnerability audit, remote CI, push, deployment, release,
publication, or packed delivery.

## Related Current Sources

- [Index Interface Contract](interface.md)
- [Index Behavior Contract](behavior.md)
- [Open Forge CLI Architecture](../../architecture.md#result-json-coordinates-and-process-status)
- [Global CLI Flags](../shared/global-flags/interface.md)
- [CLI Source References](../shared/source-references/interface.md)
- [Shared CLI Operation Contract](../../shared-operation-contract.md)
