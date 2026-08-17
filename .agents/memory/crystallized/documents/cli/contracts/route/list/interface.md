---
open-forge:
  description: Current public interface and observable result for read-only routed-topology enumeration
  responsibility: Define the public grammar, route selection, row facts, views, results, errors, and verification for `route list`
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Route, List, Interface, Topology, CurrentTruth]
---

# route list Interface Contract

## Status And Authority

This is the accepted current Crystallized Interface Contract for `open-forge
route list`. It owns the public purpose, grammar, route selection, structural depth,
observable rows, human and structured presentation, semantic results, errors,
examples, non-goals, and caller-visible verification. The command does not ship
yet; implementation and executable proof remain pending Gate 5.

The sibling [Behavior Contract](behavior.md) defines deterministic,
technology-neutral resolution and conformance. The [Route group entrypoint](../_route.md)
defines routing-only help. The shared [CLI Source References](../../shared/source-references/interface.md)
contract owns source IDs, exact `.agents` paths, quoting, collisions, and
disambiguation. The shared [Global CLI Flags](../../shared/global-flags/interface.md)
contract owns the six global flags and their common presentation and error rules.
Those sources remain authoritative for their complete shared meaning; this file
records only how `route list` uses them.

## Purpose And Boundary

`route list` answers which routed material exists in the current workspace and
which routed descendants occur at a requested structural depth. It exposes the
current authored route topology as deterministic rows that a caller can use to
choose a later route operation.

The command is read-only, stateless, and non-shipping. Given the same selected
workspace bytes and the same request, it returns the same route rows, order,
coverage, findings, and semantic result. It does not rebuild or trust generated
`Entries`, load source bodies into context, infer relevance, or mutate the
workspace.

## Syntax

The complete public command form is:

```text
open-forge route list [source-reference]
  [--depth=<non-negative-integer|all>]
  [global flags]
```

There is at most one optional source operand. It uses the shared source-reference
grammar: an automatic source ID or an exact `.agents/...` path. The command has
no alias and no alternate source or depth spelling.

### Operands And Flags

| Input                                 | Role                                                                  | Accepted value                                          | Omission                                                                 | Repetition and composition                                                |
| ------------------------------------- | --------------------------------------------------------------------- | ------------------------------------------------------- | ------------------------------------------------------------------------ | ------------------------------------------------------------------------- |
| `source-reference`                    | Select one routed entrypoint or routed leaf                           | One shared source ID or exact `.agents` path            | Select every current root route mechanically exposed by the exact Loader | At most one operand; a second operand is invalid                          |
| `--depth=<non-negative-integer\|all>` | Select route descendants by structural edges                          | A decimal non-negative integer or the exact value `all` | `1`                                                                      | One scalar value; repetition is invalid rather than choosing a precedence |
| global flags                          | Select workspace, presentation, diagnostics, or terminal help/version | The six values defined by the shared contract           | Shared defaults                                                          | Shared repetition, terminal, and composition rules apply                  |

The applicable global flags are `--workspace <path>`, `--json`,
`--view=compact|expanded`, `--verbose`, `--help`, and `--version`. `--view`
changes only human presentation. `--view` is accepted as a no-op when `--json`
selects structured presentation. `--help` and `--version` stop before route
resolution under the shared terminal-mode rules.

An empty depth value, a negative value, a non-integer value, an unknown value, or
an additional depth occurrence is invalid. `--depth=all` is the complete
descendant closure, not a result cap or a request to follow links.

## Workspace And Source Resolution

Workspace selection follows the shared global contract: the exact current
directory is used unless `--workspace <path>` supplies one exact directory. The
command does not search parent directories or infer a workspace from an operand.

Every source operand is interpreted by the shared Source References contract. In
particular:

- An ID is matched exactly. An exact `.agents/...` path is resolved from the
  selected workspace and reported canonically with `/` separators.
- Selecting a base or a valid overwrite path resolves the one logical source to
  its base route. The overwrite is never an independent row.
- A routed entrypoint selects itself and its routed descendants.
- A routed leaf selects itself only; it has no routed children to enumerate.
- An unrouted source is invalid for this command. Use `find` to discover flat
  sources or `route inspect` to inspect a source's not-routed identity.
- An unsafe reference or structurally ambiguous route is blocked. An interactive
  source-ID choice may identify one physical source, but it cannot repair
  ambiguous route meaning.

The Loader is used only to establish the operand-free root set. It is not a route
subject and is never emitted as a row. An explicit Loader reference is therefore
invalid for route enumeration rather than a request to list the Loader itself.

### Root Selection

With no source operand, the command mechanically selects every current root route
exposed directly by the exact authored Loader. This includes workspace-defined
roots and is not a hardcoded list of standard categories. A root that exists in
the workspace but is not exposed by the current Loader is not silently promoted
into the operand-free result.

An explicitly selected detached entrypoint may enumerate its own local routed
subtree. A detached tree is not claimed to be Loader-rooted. Detached trees are
not included by the operand-free form; an entrypoint must be selected explicitly.

## Structural Depth

Depth counts routed ancestry edges, not filesystem segments, heading levels, link
edges, or content size. The selected root has relative depth `0`.

| Request           | Included rows                                                                     |
| ----------------- | --------------------------------------------------------------------------------- |
| Omitted `--depth` | Each selected root at relative `0` and its direct routed children at relative `1` |
| `--depth=0`       | Selected roots only                                                               |
| `--depth=N`       | Selected roots and routed descendants through relative depth `N`                  |
| `--depth=all`     | The complete routed descendant closure                                            |

When the operand is omitted, every selected Loader root has relative depth `0`.
When one route is selected explicitly, its own row has relative depth `0` even if
it is below another root. Absolute route depth remains the depth in the current
authored route topology: for a Loader-rooted tree it counts from that tree's
root. A detached tree has no mechanically established Loader-rooted absolute
depth, so that field is null while relative depth still counts from the
explicitly selected detached root. Detached provenance makes clear that no
Loader ancestry was fabricated.

`all` has no implicit size limit. If the requested closure cannot be established
completely, the result must say so rather than silently returning a complete-looking
prefix.

## Authoritative Topology And Included Sources

The current authored filesystem topology and the source contracts that establish
route meaning are authoritative. Generated `Entries` are a navigation projection;
they cannot add a row, hide a row, change a parent, or choose row order. Stale
generated lines therefore do not change the route-list result.

The result includes:

- Recognized routed entrypoints, including current workspace-defined roots and
  detached entrypoints when explicitly selected.
- Ordinary routed Markdown leaves.
- Routed native sources whose source contract establishes route metadata, such as
  a routed `SKILL.md`.

It excludes supported but unrouted sources and excludes a valid overwrite companion
as an independent row. A base row represents the logical source; its overwrite
relationship is retained in provenance when relevant.

## Route Rows

Every emitted row is a typed fact from the same resolved topology. It retains:

- **ID**: the automatic source ID, derived under the shared Source References
  contract.
- **Path**: the canonical workspace-relative source path.
- **Parent and hierarchy**: the actual routed parent ID/path when mechanically
  established, or null for a true Loader root or parentless detached root, plus
  the visible hierarchy used by human output. An explicitly selected nested
  route remains at relative depth `0` in this result without erasing its actual
  parent fact.
- **Depth**: absolute route depth and relative depth from the selected root.
- **Kind**: `entrypoint` or `routed-leaf`; provenance identifies routed native
  source forms when applicable.
- **Description**: the exact authored description, without generated, inherited,
  normalized, or inferred replacement text.
- **Tags**: the exact authored tags, clearly reported as source metadata rather
  than inherited authority or a filter result.
- **Direct children**: the deterministic count for an entrypoint where child
  enumeration applies. A routed leaf has no child count to enumerate and is
  represented as such rather than assigned a hidden route.
- **Provenance and coverage**: why the row was selected, whether it is a Loader
  root, explicit root, descendant, or detached selection, and the evidence and
  boundary supporting the reported coverage.

Rows do not reconstruct missing metadata or treat a generated entry description
as authored source metadata. If an authored fact is malformed or cannot be
confirmed, the safe row and the finding remain distinguishable in the result.

## Ordering

Canonical order is parent before child. Within the same structural position,
ordering is deterministic by canonical route/path order from the current authored
topology. The order is not generated `Entries` order, filesystem enumeration
order, modification time, or a relevance ranking.

The same row order is used by compact human output, expanded human output, and
structured output. An explicitly selected route does not cause its ancestors to
be emitted as rows, but its mechanically established parent remains a row fact
even when that parent is outside the selected result. Detached or genuinely
parentless selection reports a null parent with the matching provenance.

## Human And Structured Output

The default human view is expanded. Both human views begin with the semantic
result and coverage; a non-complete result states the required finding and next
action before safe rows.

### Compact View

Compact output is token-friendly and retains the facts needed to navigate the
result:

```text
result=complete  coverage=complete  roots=2  depth=1  rows=5

<root-id>  .agents/<root-entrypoint>.md  description="<exact authored description>"  tags=[<exact authored tags>]
  <child-id>  .agents/<child>.md  description="<exact authored description>"  tags=[<exact authored tags>]
```

Rows are deterministically indented to show hierarchy. The summary identifies
the selected root set, effective depth, coverage, and count. The compact row does
not replace authored descriptions or tags with a summary, inferred label, or
metadata filter.

### Expanded View

Expanded output adds the workspace framing, root-selection explanation, explicit
parent, absolute and relative depths, kind, direct-child count where applicable,
source layer/provenance, coverage evidence, and bounded explanations:

```text
Open Forge route list
Workspace: <selected workspace>
Selected roots: <root selection summary>
Depth: 1 (selected roots plus direct routed children)
Result: complete
Coverage: complete
Rows: 2

ID: <root-id>
Path: .agents/<root-entrypoint>.md
Parent: none
Absolute depth: 0
Relative depth: 0
Kind: entrypoint
Direct children: 1
Description: <exact authored description>
Tags: [<exact authored tags>]
Provenance: Loader root; established from authored topology

  ID: <child-id>
  Path: .agents/<child>.md
  Parent: <root-id>
  Absolute depth: 1
  Relative depth: 1
  Kind: routed-leaf
  Description: <exact authored description>
  Tags: [<exact authored tags>]
  Provenance: direct routed child of <root-id>
```

The labels above describe the observable facts; implementations may arrange
framing without changing their meaning. `--verbose` adds bounded diagnostic
detail under the shared global contract but does not change rows, selection,
order, coverage, or status.

### JSON

`--json` emits one complete structured result derived from the same typed result.
It retains the selected workspace and root-selection facts, requested and
effective depth, semantic result, coverage, findings, and every route row with
all fields listed above. It retains empty result sets and incomplete coverage
explicitly. `--view` is accepted but has no effect under JSON.

There is no minimal projection. `route list` has no `--show`, `--display`,
metadata filter, literal filter, result cap, graph mode, or semantic query mode.

## Results And Errors

The command uses the shared semantic result and error meanings:

| Result        | Meaning for `route list`                                                                                                                     |
| ------------- | -------------------------------------------------------------------------------------------------------------------------------------------- |
| `complete`    | The selected roots and requested depth were enumerated completely with no unresolved finding that changes the result                         |
| `attention`   | Safe route rows and the requested coverage are complete, with a non-blocking identity or authored-form finding that does not weaken coverage |
| `incomplete`  | Safe confirmed rows are available, but the requested route coverage could not be established completely                                      |
| `invalid`     | The command grammar, depth, source kind, or routed-subject requirement is not valid                                                          |
| `blocked`     | An unsafe path, ambiguous identity, ambiguous route structure, or other boundary prevents safe route resolution                              |
| `failed`      | An unexpected failure prevented normal completion                                                                                            |
| `interrupted` | The caller cancelled or interrupted the operation before completion                                                                          |

A complete empty root or descendant set is valid after the exact requested
topology has been inspected completely. A partial list never implies complete
coverage. Every human error names the operation, affected reference or route
when known, direct cause, and useful next action. Shared JSON and global-flag
rules apply to every semantic result.

## Complete Examples

List every current Loader-exposed root and its direct routed children:

```text
open-forge route list
```

List only the current roots:

```text
open-forge route list --depth=0
```

List a selected route's complete subtree by ID:

```text
open-forge route list memory/working/cli-release --depth=all
```

Select an exact detached entrypoint explicitly:

```text
open-forge route list .agents/workspace/_workspace.md --depth=all
```

The last request is valid only when that exact path is one unambiguous routed
entrypoint; it does not make `workspace` a Loader root.

Request complete machine facts. The view value is a no-op here:

```text
open-forge route list --depth=2 --json --view=compact
```

## Non-Goals

`route list` does not:

- Rebuild, validate, repair, or write generated `Entries`.
- Treat generated navigation as the route inventory or as authority for order.
- List the Loader as a route row.
- Discover or include unrouted Markdown merely because it is below a routed
  folder; use `find` for the flat source universe.
- Return source bodies or sections; use `context` for content.
- Explain one route's loading and inherited-rule behavior; use `route inspect`.
- Search by tag, literal text, heading, fuzzy relevance, or semantic meaning.
- Follow ordinary links or produce incoming/outgoing reference edges.
- Build a graph, infer parentage, rank routes, cap results, or mutate files.
- Diagnose workspace health or perform repair.

## Verification Requirements

Conformance evidence must demonstrate the public result, not merely a plausible
filesystem walk. It must cover:

- Operand-free selection from the exact Loader, including workspace-defined roots,
  no hardcoded standard-root list, and no Loader row.
- Explicit entrypoint, routed leaf, valid overwrite, unrouted, unknown, unsafe,
  ambiguous, and detached-tree selections.
- Omitted depth `1`, explicit `0`, bounded positive depths, and `all`, including
  root and descendant relative-depth facts.
- Routed ordinary leaves and routed native sources such as `SKILL.md`; excluded
  unrouted sources and non-independent overwrite rows.
- Stale, missing, or reordered generated `Entries` proving that authored topology
  remains the authority for membership and order.
- Exact authored descriptions and tags, parent/hierarchy, absolute and relative
  depths, kind, applicable child counts, and provenance.
- Parent-before-child deterministic ordering across repeated invocations.
- Compact, expanded, and JSON parity, including `--view` being a no-op under
  JSON and complete empty results.
- Honest `attention`, `incomplete`, `invalid`, `blocked`, `failed`, and
  `interrupted` outcomes without a silent result cap or false `complete` status.

## Related Current Sources

- [route list Behavior Contract](behavior.md)
- [Route group entrypoint](../_route.md)
- [CLI Source References Interface Contract](../../shared/source-references/interface.md)
- [Global CLI Flags Interface Contract](../../shared/global-flags/interface.md)
- [Context Interface Contract](../../context/interface.md)
- [CLI Command Contract Set overview](../../../command-contract-set.md)
- [CLI Architecture](../../../architecture.md)
