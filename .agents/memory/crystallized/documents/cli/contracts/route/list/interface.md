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
examples, non-goals, and caller-visible verification. The command is implemented
in the merged native CLI; implementation and executable evidence are tracked in
[CLI Development](../../../../../../working/cli-development/_cli-development.md).

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

The command is read-only and stateless. Given the same selected
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
no alias. Depth accepts the native value forms `--depth 2`, `--depth=2`, and
`--depth:2`, with identical meaning. Source-reference syntax is unchanged.

### Operands And Flags

| Input                                 | Role                                                                  | Accepted value                                          | Omission                                                                 | Repetition and composition                                                |
| ------------------------------------- | --------------------------------------------------------------------- | ------------------------------------------------------- | ------------------------------------------------------------------------ | ------------------------------------------------------------------------- |
| `source-reference`                    | Select one routed entrypoint or routed leaf                           | One shared source ID or exact `.agents` path            | Select every current root route mechanically exposed by the exact Loader | At most one operand; a second operand is invalid                          |
| `--depth=<non-negative-integer\|all>` | Select route descendants by structural edges                          | A decimal non-negative integer or the exact value `all` | `1`                                                                      | One scalar value; repetition is invalid rather than choosing a precedence |
| global flags                          | Select workspace, presentation, diagnostics, or terminal help/version | The six values defined by the shared contract           | Shared defaults                                                          | Shared repetition, terminal, and composition rules apply                  |

The applicable global flags are `--workspace <path>`, `--format <text|json>`,
`--detail <minimal|standard|full|debug>`, repeatable
`--detail-filter <error|warning|info|all>`, `--help`, and `--version`. `--detail`
selects detail in text and JSON presentation. `--help` and `--version` stop before route
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

The Loader's current direct route declarations establish this operand-free root
set. This is the only route-list use of a generated `Entries` interior as a
selection boundary. It does not make Loader row order canonical and does not let
generated `Entries` establish descendant membership, parentage, metadata, depth,
or order. Missing or malformed Loader root declarations make root coverage
incomplete rather than authorizing a hardcoded fallback.

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
- Supported direct sibling sources with valid Open Forge metadata. These are
  ordinary routed leaves whether or not the parent entrypoint's generated
  `Entries` currently names them.
- Routed native sources whose source contract establishes route metadata, such as
  a routed `SKILL.md`.

It excludes supported but unrouted sources and excludes a valid overwrite companion
as an independent row. A base row represents the logical source; its overwrite
relationship is retained in provenance when relevant.

A child folder participates only through exactly one recognized entrypoint. A
supported file below an unrepresented intermediate folder is not a routed
descendant merely because it is physically below a routed ancestor.

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

The same row order is used by minimal-detail human output, full-detail human output, and
structured output. An explicitly selected route does not cause its ancestors to
be emitted as rows, but its mechanically established parent remains a row fact
even when that parent is outside the selected result. Detached or genuinely
parentless selection reports a null parent with the matching provenance.

## Human Output

Every semantic result is rendered by the shared native report. --format text
is the default text format. The applicable global flags are --workspace <path>,
--format <text|json>, --detail <minimal|standard|full|debug>, repeatable
--detail-filter <error|warning|info|all>, --help, and --version.
The default detail is minimal; standard adds workspace and tags, full adds row
provenance and structural facts, and debug adds bounded diagnostics on stderr.
Detail changes presentation only; it does not change rows, counts, ordering,
coverage, or status. Filters select finding severities; all is the default
filter.

The catalogue text by detail level is:

`minimal`:

```text
directives                       Required instructions loaded through selected routes
guidance                         Advice for recurring choices, tradeoffs, and work situations
  guidance/adaptive-collaboration  Explore ideas, match the depth to the decision, integrate accepted outcomes, and offer useful independent review
maps                             Concise maps to important local and external sources and when to use them
memory                           Self-growing Markdown memory for active work, coordination, accepted knowledge, candidates, and history
  memory/archived                Useful history that no longer controls current work
  memory/crystallized            Accepted knowledge that should remain current
  memory/emerging                Useful material that is not accepted yet
  memory/working                 Temporary memory that helps agents continue or resume active work
patterns                         Reusable default shapes for code, files, APIs, documents, and other work
skills                           Specialized capabilities provided through native SKILL.md packages
templates                        Copy-ready files for starting independently maintained workspace content
workflows                        Repeatable Markdown recipes for reaching a defined goal
13 routes to depth 1. Deeper routes: open-forge route list --depth=all
```

Child rows use the full ID so any row can be pasted into `route inspect`.
Descriptions are authored content and are never shortened. The trailer
appears only when at least one listed entrypoint has children beyond the
requested depth; it never states a count of hidden routes.

`standard` adds `Workspace:`, then under each row the path and the tags:

```text
guidance                         Advice for recurring choices, tradeoffs, and work situations
                                 .agents/guidance/_guidance.md   #LoadNow #Core #Guidance
```

`full` adds per row: parent, depth, kind (`entrypoint` or `file`), direct
child count, how the row was selected, and whether an overwrite file exists.

Warnings, incomplete results, and completed results use stdout. Invalid-input,
blocked, failed, and cancelled results use stderr. A parser failure is text on
stderr without a result envelope. Next: is the catalogue's action when one is
defined.

## Structured Output

--format json emits one schema-3 envelope on stdout for every semantic result,
including invalid-input, blocked, failed, and cancelled results formed after
parsing. It has exactly these top-level fields:

```text
{
  schemaVersion: 3,
  command,
  status,
  detail,
  filter,
  workspace,
  summary,
  findings,
  effects,
  counts,
  limitations,
  data,
  recovery,
  next
}
```

The command is exactly route list. data follows the catalogue:

| Level    | `data`                                                                                               |
| -------- | ---------------------------------------------------------------------------------------------------- |
| minimal  | `{ subject: { id, path } \| null, depth, rows: [ { id, path, description, tags, relativeDepth } ] }` |
| standard | same (path and tags are already present)                                                             |
| full     | + per row `parentId`, `absoluteDepth`, `kind`, `directChildren`, `selectedAs`, `hasOverwrite`        |

data is null only for a parser-level failure before command binding. Human and
JSON results are formed from the same typed result; no alternate projection
exists.

## Semantic Results

| Status                  | When                                                          | Text                                                                                   | Exit | Stream |
| ----------------------- | ------------------------------------------------------------- | -------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | rows                                                          | rows, then the depth trailer when depth stopped the listing                            |    0 | stdout |
| completed               | no routes under the subject                                   | `No routes under <id>.` / `The Loader exposes no routes.`                              |    0 | stdout |
| completed-with-warnings | metadata missing or malformed on a listed route, ID collision | warning rows, blank line, rows                                                         |    2 | stdout |
| incomplete              | an entrypoint could not be read                               | warning rows, blank line, the confirmed rows; `standard`: `The listing is incomplete.` |    3 | stdout |
| invalid-input           | unknown source, bad depth, a second operand                   | `Cannot list routes: <problem>.`                                                       |    4 | stderr |
| blocked                 | ambiguous source or route, malformed Loader, unsafe path      | `Cannot list routes: <reason>.`                                                        |    5 | stderr |
| failed                  | unexpected error                                              | `Route list stopped because of an unexpected error: <reason>.`                         |    1 | stderr |
| cancelled               | Ctrl+C                                                        | `Route list was cancelled.`                                                            |  130 | stderr |

### Current merged behavior and open questions

The catalogue assigns unreadable metadata to completed-with-warnings (exit 2)
with a (no description) row. The merged operation instead returns incomplete
(exit 3) and omits the unreadable row. Maintainer decision remains open; this
contract records both the catalogue rule and the observed result.

The catalogue assigns a malformed Loader to blocked (exit 5). The merged
operation instead returns incomplete (exit 3) with open-forge doctor.
Maintainer decision remains open; this contract records both outcomes.

## Errors And Boundaries

The finding catalogue is:

| Code                                | Severity | Family                | Message                                                                                           | Next                                               |
| ----------------------------------- | -------- | --------------------- | ------------------------------------------------------------------------------------------------- | -------------------------------------------------- |
| route-list.invalid-source-reference | error    | invalid-input         | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/List/Shared/Wording/RouteListWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-list.invalid-source-reference`).                                           | `open-forge route list --depth=all`                |
| route-list.unknown-source           | error    | unknown-source        |                                                                                                   |                                                    |
| route-list.invalid-depth            | error    | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/List/Shared/Wording/RouteListWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-list.invalid-depth`).                                                          | none                                               |
| route-list.loader-subject           | error    | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/List/Shared/Wording/RouteListWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-list.loader-subject`).                       | `open-forge route list`                            |
| route-list.invalid-workspace        | error    | workspace-unavailable |                                                                                                   |                                                    |
| route-list.workspace-unavailable    | error    | workspace-unavailable |                                                                                                   |                                                    |
| route-list.ambiguous-source         | error    | source-ambiguous      |                                                                                                   |                                                    |
| route-list.route-ambiguous          | error    | route-ambiguous       |                                                                                                   |                                                    |
| route-list.unsafe-source            | error    | source-unsafe         |                                                                                                   |                                                    |
| route-list.physical-boundary        | error    | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/List/Shared/Wording/RouteListWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-list.physical-boundary`).                                       | none                                               |
| route-list.loader-unavailable       | error    | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/List/Shared/Wording/RouteListWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-list.loader-unavailable`).                                                            | `open-forge doctor`                                |
| route-list.loader-malformed         | error    | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/List/Shared/Wording/RouteListWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-list.loader-malformed`).                                                | `open-forge doctor`                                |
| route-list.unsupported-source       | error    | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/List/Shared/Wording/RouteListWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-list.unsupported-source`).                                                         | none                                               |
| route-list.read-unavailable         | warning  | inspection-incomplete | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/List/Shared/Wording/RouteListWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-list.read-unavailable`).                                | `open-forge doctor`                                |
| route-list.metadata-missing         | warning  | local                 | row description shows `(no description)`; finding `<path> has no description in its frontmatter.` | `open-forge route update <id> --description "..."` |
| route-list.metadata-malformed       | warning  | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/List/Shared/Wording/RouteListWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-list.metadata-malformed`).                                          | fix by hand                                        |
| route-list.authored-form            | warning  | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/List/Shared/Wording/RouteListWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-list.authored-form`).                                           | rename to `_<folder>.md`                           |
| route-list.identity-collision       | warning  | identity-collision    |                                                                                                   |                                                    |
| route-list.operation-failed         | error    | operation-failed      |                                                                                                   |                                                    |
| route-list.interrupted              | error    | interrupted           |                                                                                                   |                                                    |

Findings retain code, severity, family, message, subject, cause, and next action
when available. Counts are:

`routes`, `roots`, `depth`.

## Scenarios

`roots-depth-1`, `subtree`, `depth-all`, `depth-0`, `empty-subtree`,
`unknown-source` (invalid), `invalid-depth`, `ambiguous-source` (blocked),
`metadata-missing` (warnings), `unreadable-entrypoint` (incomplete),
`loader-malformed` (blocked).

Unknown or invalid source -> `open-forge route list --depth=all`; unreadable
-> `open-forge doctor`; otherwise none.

## Representative Transcripts

### completed

```text
  docs              Documents
    docs/guide      Guide
    docs/reference  Reference
```

Dry run keeps status completed and replaces effects with Would list.

### completed-with-warnings

```text
  Warning  .agents/docs/_docs.md  Route metadata incomplete
         .agents/docs/_docs.md has no description in its frontmatter.
  docs              (no description)
```

### incomplete

```text
The listing is incomplete.
  Warning  .agents/docs/_docs.md  Route boundary is unreadable
         .agents/docs/_docs.md could not be read, so the routes below it are not listed.
Next: open-forge doctor
```

### invalid-input

```text
Cannot list routes: --depth must be a whole number or all.
```

### blocked

```text
Cannot list routes: docs matches more than one source. Use the exact path.
Workspace: <workspace>
```

### failed

```text
Route list stopped because of an unexpected error: <reason>.
```

### cancelled

```text
Route list was cancelled.
```

## Related Current Sources

- [route list Behavior Contract](behavior.md)
- [Route group entrypoint](../_route.md)
- [CLI Source References Interface Contract](../../shared/source-references/interface.md)
- [Global CLI Flags Interface Contract](../../shared/global-flags/interface.md)
- [Context Interface Contract](../../context/interface.md)
- [CLI Command Contract Set overview](../../../command-contract-set.md)
- [CLI Architecture](../../../architecture.md)

## Executable Wording References

Exact wording is owned by the linked typed factories. Selection, output coordinates and behavioral requirements remain in this contract and its existing semantic owners. The independent fixture preserves the original reviewed message forms.

CLI help syntax: [`route.list.help.syntax`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Route/List/RouteListText.cs).

<!-- @OpenForgeTextRef route.list.help.syntax -->
