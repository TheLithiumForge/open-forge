---
open-forge:
  description: Define Route Update's bounded parser exception and byte-preserving applicability metadata patch
  responsibility: Record the accepted Route Update realization for attached-empty responsibility input and source-span metadata edits
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Route, Update, TechnicalDesign, Parser, EdgeCase, Metadata, Mutation, CurrentTruth]
---

# route update Technical Design

## Status And Authority

This Technical Design records the bounded Route Update parser exception and the
byte-preserving realization of its `applyTo` metadata patch. The [Interface
Contract](interface.md) remains authoritative for public syntax and field
meaning. The [Behavior Contract](behavior.md) remains authoritative for
technology-neutral request resolution and operation behavior. This design adds
no spelling, result, diagnostic, or domain fact.

The generic parser and typed-request boundaries remain in the [CLI
Architecture](../../../architecture.md). The active [edge-case
ledger](../../../../../../archived/cli-development/edge-cases.md) records this
exception as `CLI-EDGE-016`. Option delimiters use the parser defaults.

## CLI-EDGE-016 Recognizer

Pinned `System.CommandLine` erases the distinction between a bare zero-token
`--responsibility` option and the two accepted attached-empty forms from its
typed parse result. `CliBindingParse` therefore retains the immutable
`ParseResult` and original argument sequence together at the binding boundary.

Only the Route Update binding may consult that original sequence. It may do so
only after typed parsing proves exactly one selected `--responsibility` option
with zero value tokens. The bounded lexical recognizer then accepts exactly:

```text
--responsibility=
--responsibility:
```

It rejects the bare `--responsibility` form. The ordinary valued forms,
including `--responsibility ""`, retain their Interface-contract meaning and do
not enter this attached-empty recognizer.

Inspection stops at the first `--`. The recognizer does not inspect, parse,
count, select, or diagnose tokens after that terminator. It does not parse a
value, aggregate occurrences, select a command, replace parser diagnostics, or
reinterpret any other option spelling.

Original arguments do not enter `CliInvocation`, the Route Update request, or
domain behavior. All other parser facts remain exclusively parser-owned.

## Shared filesystem safety boundary

The parser exception does not change Route Update's filesystem safety boundary.
The Behavior Contract consumes the neutral Framework no-follow observation of
each logical final leaf before ordinary physical resolution, at initial
preflight, under-lease revalidation, and immediately before every ordinary
effect. A present link, reparse point, or special final leaf blocks the
corresponding Create, Replace, Delete, or ReplaceGeneratedRegion effect. Route
Update does not follow, write, or delete a Library projection, and this guard
does not consult Library record authority. Stable contained directory-link
ancestry remains governed by the existing ordinary path contract.

This design owns no duplicate guard, Library lookup, fallback, or parser
diagnostic. Its implementation evidence must show that the shared fact is
consumed at the Route Update plan, revalidation, and effect boundaries without
changing the request or result schema.

## `applyTo` source-span patch

The repeated `--apply-to` occurrences use ordinary typed `System.CommandLine`
binding. Binding supplies atomic patch values through
`ApplyToPatternExpressionParser`, and planning never splits them again. The
existing `CLI-EDGE-016` recognizer remains limited to attached-empty
responsibility input. Route Update reads root and scoped `applyTo` declarations
through `ApplyToMetadataReader` and shared typed YAML facts, which retain each
declaration's location and source span. It validates both locations before
planning an edit.

For equivalent normalized dual declarations, a set operation patches both
declarations in their existing locations, and a clear operation removes both.
A new declaration is written as a quoted list under the scoped `open-forge`
mapping. The operation replaces or inserts only the exact spans supported by the
parsed mapping and preserves unrelated frontmatter, body bytes, line endings,
and encoding. It does not serialize the whole YAML document or use whole-file
serialization as a fallback.

Quoted-string flow and block lists are eligible. Replacing a block list retains
its terminal line ending and a safely separable trailing comment. Interior
sequence comments, or a clear that would consume a retained comment, produce
the existing metadata-preservation failure before any write. Clearing an absent
field is unchanged and does not refresh an otherwise stale parent Entries region.

Invalid or conflicting declarations block mutation. If the parsed document does
not provide safe spans for every required replacement or insertion, planning
fails before effects. The operation projects normalized before-and-after values
into Route Update's command-owned result types; Framework YAML nodes and
applicability facts do not pass directly to Rendering. Structured output remains
part of the source-generated command result graph.

## Conformance Evidence

Focused parser and process evidence must prove both accepted attached-empty
forms, rejection of the bare form, unchanged behavior for ordinary valued forms
including the explicit empty string, the exactly-one-occurrence and zero-token
precondition, the `--` stop boundary, and absence of raw arguments from the
request and domain. Focused Route Update filesystem evidence must additionally
prove no-follow final-leaf checks at physical resolution, initial preflight,
under-lease revalidation, and immediately before each ordinary effect, including
refusal to follow or mutate an eligible `.agents/...` Library projection.
Route List evidence separately proves equivalent native space, equals and colon
depth forms. It adds no raw argument inspection.

Applicability evidence must also cover root and scoped declarations, equivalent
dual updates and clears, conflicts, malformed patterns, bounded exact-span
insertion, preservation of unrelated bytes, and failure before effects when a
safe span cannot be established. It must confirm that binding supplies atomic
patch values through `ApplyToPatternExpressionParser` once and that planning
does not split them again. The new flags stay on the ordinary parser path and
do not expand `CLI-EDGE-016`.

## Related Current Sources

- [route update Interface Contract](interface.md)
- [route update Behavior Contract](behavior.md)
- [CLI Architecture](../../../architecture.md)
- [Shared CLI Operation Contract](../../../shared-operation-contract.md)
