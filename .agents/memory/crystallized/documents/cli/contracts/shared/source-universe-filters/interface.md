---
open-forge:
  description: Shared caller-visible contract for operation-specific source-universe selection
  responsibility: Define the accepted include and exclude flags, expansion rules, identity outcomes, and selector reporting
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Shared, Source, Universe, Filter, Interface, CurrentTruth]
---

# Shared Source-Universe Filters Interface Contract

## Status And Authority

This file is the accepted current Crystallized authority for the caller-visible
meaning of the shared source-universe filters. Their final route is under
`cli/contracts/shared/`. The commands that apply the contract do not ship yet;
implementation and executable evidence are tracked in
[CLI Development](../../../../../../working/cli-development/_cli-development.md). The [Shared Result
Coordinates](../result-coordinates/interface.md) define the shared result
boundary. The [CLI Architecture](../../../architecture.md) defines filesystem,
runtime, and evidence boundaries; this contract defines only selector meaning.

This is not a global-flag contract. `--include` and `--exclude` are reusable
operation-specific flags. Only a consuming command that explicitly declares
their applicability accepts them. An unrelated command rejects either flag as
unknown; it does not accept the flag and silently do nothing.

This contract uses the shared source-reference vocabulary rather than adding a
second source-reference language. For source-universe selection, it is the
shared authority for how source identity, collisions, disambiguation, physical
safety, selector errors, expansion, set composition, and supplied/resolved
selector reporting behave. See the related [CLI Source References Interface
Contract](../source-references/interface.md) for the reusable source-reference
forms.

## Applicability And Exact Spellings

An applicable consumer declares these operation-specific options in its own
command form:

| Flag                           | Accepted value                     | Repetition and role                             |
| ------------------------------ | ---------------------------------- | ----------------------------------------------- |
| `--include=<source-reference>` | One scalar shared source reference | Repeatable; occurrences form the include union. |
| `--exclude=<source-reference>` | One scalar shared source reference | Repeatable; occurrences form the exclude union. |

Each occurrence consumes exactly one source reference. The shared filter
contract adds no comma-separated selector list, glob, arbitrary-directory,
positional-operand substitution, or new qualifier grammar. An applicable
consumer may not reinterpret either flag as a predicate, content projection,
result cap, or lifecycle or authority query.

The exact operation-specific declaration determines whether the flags are
accepted. Their absence from an unrelated command is not an omission that
selects a default universe; the flags are unknown there.

## Omission And Set Composition

Let `D` be the default source universe declared by the consuming command. Let
each include occurrence expand to a logical-source set `I`, and each exclude
occurrence expand to a logical-source set `X`. The effective source universe is:

```text
effective = (D if no --include occurs
             else the union of all include expansions)
            minus the union of all exclude expansions
```

- Omitted `--include` means the consuming command's declared default universe;
  this contract does not define that default.
- Omitted `--exclude` subtracts nothing.
- Repeated includes form one union, and repeated excludes form one union.
- Exclusion is applied after the include or default universe is established, so
  exclusion wins an overlap regardless of argument order.
- Duplicate selectors and overlapping expansions are deduplicated in the
  effective logical-source set. Repetition or overlap does not create another
  logical source in that set.

Set formation is independent of the order in which include and exclude
occurrences appear. The supplied occurrence stream remains available for
reporting in command-line order; that reporting order does not change the
effective set.

## Selector Expansion And Logical Identity

After a source reference resolves to a source identity, expansion follows the
source's physical kind:

| Resolved source kind                           | Shared expansion                                                                                                                                                                                       |
| ---------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Loader, a recognized entrypoint, or `SKILL.md` | Select the physical folder containing that source and every Markdown source eligible under the consuming command that is physically contained below that folder, including eligible unrouted Markdown. |
| Ordinary source                                | Select that one logical source.                                                                                                                                                                        |

Physical folder-root expansion is not route-descendant traversal. It does not
follow links, infer authority, lifecycle, route, scope, or relevance. A routed
child outside the selected physical folder is not added, while an eligible
unrouted Markdown source physically inside it is not removed for lacking a
route.

A valid base file and overwrite companion are one logical source. A selector
that resolves through the logical source's ID, base path, or overwrite path
selects or excludes that logical source with both physical layers; the
overwrite is not an independent candidate. The consumer may inspect the layers
according to its own contract, but source-universe selection does not split
them.

The shared filter contract does not turn an arbitrary directory operand into a
folder root. Folder-root expansion is available only when the shared source
reference resolves to Loader, a recognized entrypoint, or `SKILL.md`.

## Identity, Collision, Safety, And Errors

Each occurrence uses the shared exact source-ID or `.agents/...` path forms and
their shared normalization and identity rules. Resolution never guesses
whether a value meant a different ID, path, source, or physical layer.

- A missing, empty, unknown, or unsupported selector is invalid.
- A comma list, glob, arbitrary directory, positional substitution, or
  unaccepted qualifier is invalid rather than an alternate selector form.
- An ambiguous source ID is blocked unless the shared interactive
  disambiguation flow resolves it to a specific identity. JSON and other
  non-interactive uses do not prompt; unresolved ambiguity remains blocked.
- A source-ID collision is not silently merged or selected by likely intent.
  Exact disambiguation preserves the distinct logical identity and physical
  path needed by the consumer.
- An unsafe or escaping physical boundary is blocked. Resolution does not
  bypass containment, identity, or safety checks to satisfy a selector.

The applicable consumer reports the shared distinction between invalid selector
input and a blocked ambiguity or safety boundary. Exact command wording and
status presentation remain consumer-owned, but no consumer may replace a
shared failure with a guessed source.

## Supplied And Resolved Selector Reporting

An applicable consumer preserves two distinct facts:

1. **Supplied occurrences** — the scalar selector values as supplied, including
   repetition, in command-line order.
2. **Resolved selector records** — the identity, expansion class, and resolution
   outcome associated with each supplied occurrence, using the shared
   normalization, collision, disambiguation, and safety rules.

Resolved selector sets and effective logical sources are deduplicated, but
supplied occurrence records are not. A consumer that exposes selector detail
must not use set deduplication to erase what the caller supplied. The consumer
owns the placement and exact output-row schema for these facts; this contract
owns their meaning and distinction.

## Examples

These examples use `find`, which explicitly applies this shared contract. Other
commands may use the same flags only after declaring applicability.

```text
# Repeated scalar selectors: include union, then exclude union.
open-forge find \
  --include=memory/crystallized/documents \
  --include=memory/crystallized/documents/cli/contracts/find \
  --exclude=memory/crystallized/documents/architecture

# A source reference to an ordinary source selects one logical source.
open-forge find --include=memory/crystallized/documents/architecture
```

The following are not alternate forms of the shared filters:

```text
open-forge find --include=memory/crystallized/documents,skills/experience-design
open-forge find --include=memory/**/architecture
open-forge find .agents/memory/crystallized/documents/
```

Use one shared source reference per occurrence. Use a source reference that
resolves to a recognized entrypoint, Loader, or `SKILL.md` when physical
folder-root expansion is intended.

## Consumer-Owned Meaning

The consumer, not this shared contract, defines:

- whether the flags apply at all;
- the default universe used when `--include` is omitted;
- which sources are eligible for that operation;
- coverage and semantic-result rules after the effective universe is formed; and
- the output row schema and the placement of selector detail.

The consumer's predicates, projections, result caps, and other operation
options do not change the shared source-selector grammar or set algebra.

## Related Contracts

- [Shared Source-Universe Filters route](_source-universe-filters.md)
- [CLI Architecture](../../../architecture.md)
- [CLI Source References Interface Contract](../source-references/interface.md)
- [CLI Source References Behavior Contract](../source-references/behavior.md)
