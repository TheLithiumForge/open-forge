---
open-forge:
  description: Accepted context implementation design subordinate to the current CLI Architecture
  responsibility: Record the accepted context realization without changing its public or technology-neutral contracts
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Context, TechnicalDesign, Implementation, CurrentTruth]
---

# Context Technical Design

## Status And Authority

This is the accepted Technical Design for `context`. It is subordinate to the
[Shared Result Coordinates](../shared/result-coordinates/interface.md), which
define the shared JSON schema and process-status mapping, and to the [CLI
Architecture](../../architecture.md), which defines source structure, runtime
boundaries, BCL-first filesystem structure, and recovery identity relationships. The [Interface
Contract](interface.md) and [Behavior Contract](behavior.md) remain authoritative
for public and technology-neutral meaning.

The CLI is available as a public beta. [CLI Distribution](../../distribution.md)
records qualified platforms and published versions. This file defines the accepted
Context implementation. [CLI Development](../../../../../working/cli-development/_cli-development.md)
records current implementation, release work, and executable evidence.

## Accepted Design

### Invocation-local graph

`context` builds an in-memory graph only when the requested closure, link
expansion, overwrite layering, section projection, or ordering needs those
relationships. The graph is derived for one invocation, is discarded after the
result is formed, and is never persisted as workspace truth. A command that does
not need the graph does not build the complete graph merely because the facility
exists.

The graph contains the parsed sources and relationships required by the request:
route and scope placement, loading, base and overwrite layers, sections and
ranges, and contained local links. It does not become a private authority or a
session cache.

### Command binding

The command surface uses `System.CommandLine` for syntax, help, terminal modes,
and parse errors. Binding is manual: the command handler converts the validated
parse result into the typed `context` request, applies the contract's repetition
rules, and passes that request to the operation. Binding does not infer routes,
authority, or workspace selection beyond the accepted contracts.

### Markdown and authored metadata

Markdown parsing uses a fixed Markdig configuration. The configuration recognizes
the accepted structural heading forms, visible heading text, source forms, and
source ranges used by exact section and heading projections. It does not add
unaccepted public syntax.

Authored frontmatter uses YamlDotNet source-generated semantics. The semantic
model preserves the distinction between authored values and CLI-generated
metadata; it does not replace authored bytes with a private normalized document.

Ordinary routed metadata reads root and scoped forms under the [shared
rule](../../shared-operation-contract.md#frontmatter-form). Output selection
does not filter readable forms. Generated metadata uses the selected Open Forge
values. Authored frontmatter projections preserve the complete original block.

Input decoding is strict UTF-8. Source ranges remain exact and stable for the
current decoded source, and range-bearing facts are retained through projection
and diagnostics. Invalid encoding or an unusable range remains the contract's
observable finding rather than being silently repaired.

### Path-scoped applicability

Framework `Documents` owns the shared `applyTo` metadata and pattern boundary.
`ApplyToMetadataReader` consumes the existing `YamlDocumentFacts` and returns
typed `ApplyToMetadataFacts`, retaining declarations from both the frontmatter
root and scoped `open-forge` mapping with their source spans. These facts carry
applicability independently of required description and tag completeness,
including for recognized native sources such as `SKILL.md`. Equivalent
normalized dual declarations represent one condition; invalid or conflicting
declarations do not become a preferred or combined value.

`ApplyToPatternExpressionParser` parses each string declaration into atomic
patterns by splitting top-level comma expressions. A list entry remains one
atomic pattern after trimming. `ApplyToPatternMatcher.Parse` parses each
atomic pattern into slash-separated path segments, and
`ApplyToPatternMatcher.IsMatch` performs ordinal, case-sensitive atomic
matching against normalized working paths. It uses bounded dynamic programming
over pattern and path segments for a standalone `**` and does not enumerate
files. Framework `Sources` uses `SourceApplicabilityEvaluator` to evaluate an
ordered chain of typed conditions against normalized working paths. Patterns
within one declaration are alternatives, each conditioned ancestor adds a
condition on the same path, and any supplied path satisfying the complete
chain makes the source match.
`SourceApplicabilityResult` retains the state, condition provenance, and matching
paths. A conditioned chain with no supplied paths is pending, while an invalid
declaration remains invalid.

`SourceWorkingPathNormalizer` resolves each supplied value lexically with
`Path.GetFullPath(path, workspaceRoot)` and
`Path.GetRelativePath(workspaceRoot, fullPath)`, then returns a canonical
slash-separated relative path. It rejects paths outside the workspace and
workspace-root-only results. It does not require a planned path to exist.

Context applies these facts during its own top-down selection of visible entries.
A condition does not select a hidden ancestor. It is a pre-load file condition,
so a matching visible `LoadNow` entry retains its mandatory first-load behavior
and `KeepInMind` retains its refresh behavior. When the workspace has no
conditions, omitting `--for` preserves existing startup loading. The Status
command stays outside this Context resolver. An explicitly or reference-selected
nonmatch remains inspectable with the ancestors needed for that inspection,
without activating its automatic descendants. With no supplied working paths,
encountered conditions remain pending and contribute their `{ source, patterns }`
pairs to `pendingConditions` under the Interface and Behavior contracts. The
same normalized path set filters both the startup closure and the
startup-plus-explicit closure before `--additions-only` calculates their
difference.

The Context operation projects shared Framework facts into Context-owned typed
result values. Presentation consumes that result, and the registered
source-generated JSON graph serializes its public projection. Framework metadata
and evaluator models do not cross directly into Rendering or become shared
command-result contracts.

### Results and filesystem boundary

The operation produces one concrete typed result conforming to the [Shared Result
Coordinates](../shared/result-coordinates/interface.md). The CLI Architecture
defines its concrete typed pipeline and source-generated serialization. Human
and JSON presentation consume the same result and do not rerun resolution or
projection.

Filesystem access is BCL-first and uses real `System.IO` behavior. The design
does not introduce a virtual filesystem or a private filesystem authority.
Physical identity, containment, workspace selection, and recovery use the
accepted Architecture boundary.

## Contract Traceability

| Contract fact                  | Design response                                                                                                                                 |
| ------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| Startup and explicit closure   | Build the invocation-local graph only when closure relationships are needed, then resolve startup and explicit sources in the contract's order. |
| Link expansion and cycles      | Traverse contained local links in the same invocation graph, retain inclusion reasons, and terminate cycles without persisting graph state.     |
| Base and overwrite layering    | Represent one logical source with ordered physical layers and preserve the contract's base-first projection.                                    |
| Heading and section projection | Use fixed Markdig structural nodes, visible text, source forms, strict UTF-8, and exact source ranges.                                          |
| Authored frontmatter           | Use YamlDotNet source-generated semantics while preserving the authored representation required by the contracts.                               |
| Path-scoped applicability      | Read typed declarations once, evaluate visible source chains against normalized paths, and retain pending conditions. |
| Human and structured results   | Form one typed result, render human views from it, and serialize the shared JSON shape with source-generated JSON.                              |
| Read-only safety               | Use real BCL-first `System.IO` reads and no persistent graph, session, receipt, cache key, or mutation plan.                                    |

These responses realize the existing Interface and Behavior facts. They do not
add flags, statuses, output fields, or alternate source meanings.

## Verification Design

Gate 5 executable proof must mirror the source boundaries:

- Unit tests cover request binding, graph construction, closure selection,
  projection, ordering, range handling, repetition, and status formation.
- Integration tests cover real temporary workspaces, real `System.IO` behavior,
  route and overwrite relationships, links, containment, and strict UTF-8.
- Applicability tests cover root and scoped YAML declarations, equivalent and
  conflicting dual declarations, path-pattern matching, lexical normalization
  of existing and planned paths, inherited same-path conditions, pending
  conditions, and reuse of one working set for startup and selected closures.
- E2E tests exercise the built CLI process and its human and structured streams.
- PackageEndToEnd tests exercise the packaged executable and thin invocation
  boundary without reimplementing command behavior.
- Native AOT publish and run proof covers trimming, source-generated JSON,
  package execution, and the accepted CLI result and exit behavior.

These are required evidence shapes, not claims that the tests or artifacts
already exist. Gate 5 accepts the implementation only after the complete
retained command set and its executable proof are available.

## Related Current Sources

- [CLI Architecture](../../architecture.md)
- [Context Command Contract Set](_context.md)
- [Context Interface Contract](interface.md)
- [Context Behavior Contract](behavior.md)
- [Global CLI Flags Behavior Contract](../shared/global-flags/behavior.md)
- [CLI Source References Behavior Contract](../shared/source-references/behavior.md)
- [Shared CLI Operation Contract](../../shared-operation-contract.md)
- [Loading And Refreshing Context](../../../framework/routing/loading.md)
- [Routing Model](../../../framework/routing/model.md)
- [Route Scope And Inheritance](../../../framework/routing/scope.md)
- [Routing Paths And Identity](../../../framework/routing/paths.md)
- [Overwrite Customization](../../../framework/routing/overwrites.md)
