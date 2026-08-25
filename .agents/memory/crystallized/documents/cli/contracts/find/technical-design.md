---
open-forge:
  description: Accepted current implementation design for deterministic `find` discovery
  responsibility: Record accepted .NET implementation choices and evidence boundaries without redefining the `find` Interface or Behavior
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Find, TechnicalDesign, Implementation, Traceability, CurrentTruth]
---

# Find Technical Design

## Status And Authority

This is the current Crystallized Technical Design for the non-shipping `find`
command. The [Interface](interface.md) and [Behavior](behavior.md) files are
the current authorities for public and technology-neutral meaning. This design
is subordinate to those contracts and the [CLI Architecture](../../architecture.md).
It cannot add public syntax, weaken a safety guarantee, change a semantic result,
or make an implementation artifact authoritative for user-authored meaning.

The accepted implementation direction is a C# CLI on .NET 10 or newer with
Native AOT. The command does not ship. This document claims no existing code,
executable, test suite, package, or proof. Actual Native AOT proof is pending
Gate 5.

The Architecture defines only the shared schema-v1 JSON envelope,
source-location primitive, and status/process coordinates. The Find Interface
defines the exact command-local result schema, finding codes, finite values, and
`next` contents. This design uses those definitions and does not redefine either
authority.

## Contract Traceability

| Contract boundary                                                                                                                                                                    | Accepted design response                                                                                                                                                                                                                                                                         | Evidence state                                       |
| ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------- |
| [Structured Result Fields](interface.md#structured-result-fields) and [Deterministic Conformance Responsibilities](behavior.md#deterministic-conformance-responsibilities)           | Produce one typed result rich enough for the public workspace, universe, query, projection, counts, coverage, ordered sources, evidence, content, findings, and semantic status. Follow the exact Find Interface command-local schema inside the shared Architecture envelope and use the Architecture status-to-exit mapping. | Design accepted; executable proof is pending Gate 5. |
| [Semantic Results](interface.md#semantic-results) and [Deterministic Conformance Responsibilities](behavior.md#deterministic-conformance-responsibilities)                           | Map semantic results to process completion behavior without changing the named semantic conditions.                                                                                                                                                                                              | Design accepted; executable proof is pending Gate 5. |
| [Public Heading Matching](interface.md#public-heading-matching) and [Deterministic Conformance Responsibilities](behavior.md#deterministic-conformance-responsibilities)             | Use the fixed Markdig 1.3.2 CommonMark pipeline to expose the required structural heading nodes, visible inline text, source forms, levels, locations, and section boundaries.                                                                                                                   | Design accepted; executable proof is pending Gate 5. |
| [Workspace And Source Universe](interface.md#workspace-and-source-universe) and [Deterministic Conformance Responsibilities](behavior.md#deterministic-conformance-responsibilities) | Use real `System.IO` boundaries and command-local source units that establish the declared workspace and source-universe containment without hiding eligible files.                                                                                                                              | Design accepted; executable proof is pending Gate 5. |
| [Public Tag Matching](interface.md#public-tag-matching) and [Public Heading Matching](interface.md#public-heading-matching)                                                          | Use strict UTF-8 byte-origin mapping for source locations and keep body-tag scanners and compatibility patches local to `find`. Preserve exact authored spelling, line, occurrence, layer, and source-span facts without guessing unavailable origins.                                           | Design accepted; executable proof is pending Gate 5. |

## Runtime And Dependencies

The command uses System.CommandLine with manual binding. Binding is explicit and
produces the typed request described by the Interface Contract; it does not create
an alternate convention- or reflection-defined public grammar.

Markdown parsing uses Markdig 1.3.2 with one fixed CommonMark pipeline. Plugin
discovery is disabled. The fixed pipeline does not discover extensions or add a
public heading form beyond the accepted CommonMark ATX and Setext boundary.

YAML frontmatter uses the shared neutral YamlDotNet 18.1.0 syntax boundary plus
the one source-generated semantic metadata path. The operation matches parsed
authored values, not serialized YAML text, and does not make unknown metadata or
generated `Entries` copies part of tag semantics.

Structured JSON uses System.Text.Json source generation. The serializer realizes
the shared Architecture envelope and the exact Find Interface command-local
result graph without reflection-based discovery becoming a private compatibility
contract.

All selected dependencies and runtime features must remain compatible with
Native AOT and trimming. The implementation uses real `System.IO` filesystem
boundaries and no virtual filesystem abstraction. No code or dependency proof is
claimed here; actual Native AOT proof is pending Gate 5.

## Source Structure And Boundaries

The source tree mirrors the `find` contract boundary. Enumeration, comparison,
result formation, rendering, body-tag scanning, UTF-8 origin mapping, and
compatibility patches stay local to `find`. Neutral Markdown frontmatter and YAML
syntax foundations sit under `Framework/Documents` because Find, Route discovery,
and accepted later Route mutation require the same mechanical boundary. No remote
`utils` folder, universal engine, or speculative shared abstraction is introduced.

Request binding, source enumeration, layer resolution, inspection, parsing,
region evaluation, evidence aggregation, coverage, ordering, projection, result
construction, and rendering preserve the conceptual responsibilities in the
[Behavior Contract](behavior.md). An implementation may combine units only when
it preserves those boundaries and the Interface Contract. It does not persist an
index, graph, receipt, session, repair artifact, or mutation authority.

The filesystem boundary uses the exact selected workspace and real temporary
filesystem state in evidence. Source identity and containment are established
before a candidate is admitted as an inspectable layer, and an unsafe identity,
unreadable byte sequence, orphan, ambiguity, or incomplete origin remains a
finding rather than a guessed fact.

The optional Find metadata projection consumes neutral shared route facts rather
than Route command results. Those facts distinguish routed, unrouted, ambiguous,
and unavailable states. Find exposes a route only for one unambiguous
Loader-rooted source identity, treats ambiguous or unavailable metadata as an
incomplete projection, and does not infer a Framework scope from path segments.
The route-facts resolver reads only supporting sources in the effective Find
universe; metadata projection does not widen source filters to inspect excluded
sources.

## Parsing And Origin Mapping

The fixed Markdig 1.3.2 CommonMark pipeline supplies ATX and Setext heading nodes,
visible inline text, source forms, levels, and section boundaries. It preserves
the technology-neutral rules for formatted, linked, code-containing, duplicate,
and malformed headings. No plugin discovery changes that boundary.

The shared Markdown frontmatter boundary preserves exact delimiter, YAML, and
body spans. The shared neutral YamlDotNet 18.1.0 syntax parser preserves the
decoded node shape and half-open UTF-16 scalar spans while reporting aliases,
duplicate keys, and non-scalar mapping keys without importing Find policy. The
one source-generated semantic path supplies accepted metadata values. Together
they retain authored list-value tags, source order, physical layer, canonical
path, and the locations needed by the typed result. They do not turn serialized
YAML, unknown fields, descriptions, responsibilities, or generated `Entries`
into tag matches.

The Find-local frontmatter reader pairs source-generated semantic values with
shared neutral scalar facts for `open-forge.tags`, then maps their spans through
the strict UTF-8 origin map. A mismatch or unavailable span remains a typed
frontmatter finding rather than a guessed location. Find owns no YAML event
reader or handwritten YAML grammar; the shared parser remains subject to
trimming and Native AOT evidence.

Strict UTF-8 byte-origin mapping is the source-location boundary for parser and
scanner observations. Local scanners and compatibility patches preserve the
mapping when a library does not expose the needed byte origin, line, column, or
span directly. They do not normalize Unicode, repair malformed authored input,
or broaden the accepted public syntax. Exact column and source-span fields follow
the shared Architecture location primitive and retain the Interface Contract's
authored spelling, layer, region, line, occurrence, and source-location
requirements.

## Comparison APIs

Case-insensitive tag and heading equality uses `StringComparison.OrdinalIgnoreCase`.
Tag sets and keys use `StringComparer.OrdinalIgnoreCase`. These ordinal-ignore-
case choices are culture-independent and avoid current-culture behavior while
preserving the Interface Contract's lack of Unicode normalization, accent
folding, whitespace folding, approximation, or relevance ranking. Evidence
includes invariant and Turkish-culture coverage, and results retain exact
authored spelling.

## Results And Process Status

Result construction produces the one typed result consumed by compact, expanded,
content-projected, verbose, and structured renderers. Human and structured
renderers do not rerun enumeration, parsing, matching, projection, or verification.
The shared envelope fields, location primitive, compatibility rules, and numeric
process-status mapping come from the [CLI Architecture](../../architecture.md#result-json-coordinates-and-process-status).
The exact `find.result` schema, finding codes, finite values, and `next` contents
come from the [Find Interface](interface.md). This design does not replace either
authority.

## Evidence Design

The test tree mirrors the `find` source tree and separates evidence by boundary.
These are evidence requirements and planned proof surfaces, not claims that tests
or artifacts already exist.

- **Unit** evidence should exercise manual binding, comparison APIs under
  invariant and Turkish cultures, ordinal-ignore-case behavior, tag and heading
  aggregation, region selection, ordering, result formation, and semantic
  statuses without claiming filesystem effects.
- **Integration** evidence should use real isolated temporary workspaces and
  filesystem state to exercise the selected parser and source boundary,
  overwrite layers, malformed and non-UTF-8 candidates, structural headings,
  body tags, regions, source locations, and containment failures.
- **EndToEnd** evidence should exercise a built Native AOT process through the
  public boundary for compact, expanded, structured, culture, and exit behavior.
- **PackageEndToEnd** evidence should exercise packaged execution and the same
  public behavior from the delivered package boundary.

Every evidence tier remains independently selectable, owns the mutable temporary
state it can affect, and proves the corresponding Interface and Behavior facts.
Actual Native AOT publish, execution, package, and release proof is pending Gate 5.

## Related Current Sources

- [Find Interface Contract](interface.md)
- [Find Behavior Contract](behavior.md)
- [CLI Command Contract Set — Technical Design](../../command-contract-set.md#technical-design)
- [CLI Architecture](../../architecture.md)
- [Global CLI Flags Behavior Contract](../shared/global-flags/behavior.md)
- [CLI Source References Behavior Contract](../shared/source-references/behavior.md)
- [Context Interface Contract](../context/interface.md)
- [Status Interface Contract](../status/interface.md)
- [Shared CLI Operation Contract](../../shared-operation-contract.md)
- [Microsoft .NET string comparison guidance](https://learn.microsoft.com/dotnet/standard/base-types/best-practices-strings)
