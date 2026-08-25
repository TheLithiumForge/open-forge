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
  -> verification or recovery
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
It does not expose private staging, backup, or recovery material through an
ordinary result.

## Safe Replacement And Recovery

Application uses adjacent staged and backup artifacts with the accepted
structured provenance identity envelope. The envelope carries the accepted workspace identity,
operation identity, target logical and physical identity, artifact kind,
expected before-and-after identity, and recovery state needed before recovery
uses it. It is recovery evidence, not a second authority for workspace meaning.

The operation proves staging and backup readiness before the first write and
never overwrites an unknown adjacent artifact. Each changed target is replaced
through the accepted same-directory safe-replacement property, never edited in
place and never written through a weaker fallback after an identity or
atomicity check fails. The target is verified after each replacement, and the
complete selected projection is verified before recovery artifacts are removed.

An application or verification failure stops new effects and recovers already
applied targets in reverse effect order. Recovery changes only targets that
still match the applied identity. An unexpected concurrent edit is preserved
and reported as residual state. Interrupted or incompletely recovered work
retains and reports every backup still needed for recovery. A rerun computes a
fresh plan from current facts rather than replaying a saved plan. The design
creates no persistent transaction journal.

## Test Design And Evidence

Tests mirror the Interface, Behavior, and technical boundaries. Managed unit
and integration test subjects mirror their production command or capability
paths, while complete process tests remain in the separate system boundary.
Direct tests cover typed request normalization, physical identity, topology-derived
projection, metadata admission, canonical output, byte ranges, marker
ownership, ordering, result formation, and no-op behavior. Focused integration
tests use real temporary rooted and detached source trees, Git and Gitless
state, adjacent staging and backups, filesystem failures, expected-state
changes, concurrency changes, safe replacement, verification, reverse
recovery, residual preservation, interruption, and rerun convergence.

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
