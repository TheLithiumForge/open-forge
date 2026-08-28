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
user-authored meaning. It records the accepted realization and the evidence
still required before implementation acceptance. The command has no
implementation yet and does not ship.

## Accepted Runtime Boundary

The implementation direction is C# on .NET 10 or newer in the modern `.slnx`
solution shape. Native AOT and trimming compatibility apply to every runtime
feature, dependency, and serialization path. The required Gate 5 Native AOT
publish evidence is pending; this design does not claim that implementation or
evidence exists.

The command uses real BCL-first `System.IO` filesystem boundaries and real
isolated temporary directories for filesystem tests. It does not introduce a
virtual filesystem, fake hierarchy, or filesystem abstraction only to make
tests convenient. Physical identity, containment, expected-state revalidation,
safe replacement, verification, concurrency, and recovery remain real
filesystem concerns.

Mutating application coordinates through the workspace operation lock at
`.agents/open-forge.lock`. The lock is held through the mutation planning,
application, verification, and recovery boundary. It does not become route
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
and YamlDotNet 18.1.0. Those versions and their Native AOT evidence remain
Architecture-owned inputs to this design; this file does not claim that the
dependencies are restored or running.

The operation remains a directly testable typed flow:

```text
parsed input
  -> complete request
  -> current filesystem and routing facts
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

The recognized `_index.md` entrypoint shape is canonicalized by physical
identity. If more than one traversal path exposes the same recognized physical
entrypoint, the operation creates one target, one plan item, and at most one
effect for that identity. This is conformance behavior, not migration or
staging behavior.

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
verified after effects.

After whole-command verification, delete only the positively recognized bundle
created by that operation. Deletion failure leaves successful effects and
returns `attention` with the exact residual path and cleanup guidance. An
handled application, verification, or cancellation failure stops new effects
and reports the actual residual draft or final path; a valid final remains after
preparation. A closed final ZIP may remain after abrupt process termination,
without an executable crash or power-loss guarantee. An unexpected concurrent
edit is preserved and reported as residual state. The bundle is never extracted
or used to restore a target, and current target state is not derived from its
provenance. Cleanup owns exact named final and draft deletion under its separate
lease-bound contract. A rerun computes fresh facts and never replays a
saved plan, receipt, journal, history, or progress record.

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

The process-boundary suite uses a built Native AOT process to prove command
parsing, exact dry-run output, human and structured results, stream assignment,
exit behavior, and packaged execution. These are required evidence shapes, not
claims that the implementation or package already exists. Gate 5 AOT publish
evidence remains pending.

## Related Current Sources

- [Index Interface Contract](interface.md)
- [Index Behavior Contract](behavior.md)
- [Open Forge CLI Architecture](../../architecture.md#result-json-coordinates-and-process-status)
- [Global CLI Flags](../shared/global-flags/interface.md)
- [CLI Source References](../shared/source-references/interface.md)
- [Shared CLI Operation Contract](../../shared-operation-contract.md)
