---
open-forge:
  description: "Historical CLI-v2 source: Shared authored route topology, natural identity, metadata, generated-navigation, context projection, and invalid-state semantics for replacement CLI consumers"
  responsibility: Define one route inventory behind context, find, route inspection, diagnosis, and derived navigation
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Route Inventory Contract

## Scope

The Framework routing, scope, loading, overwrite, path, and Markdown contracts
own route meaning. This contract defines one replacement CLI read model over
those files. Production TypeScript owns exact declarations, finding codes, and
resource limits.

## Guarantees

### Natural Route Identity

CLI route arguments and results use one exact natural identity:

| Route identity                                        | Source                                                           |
| ----------------------------------------------------- | ---------------------------------------------------------------- |
| `loader`                                              | `.agents/loader.md`                                              |
| `directives`                                          | `.agents/directives/_directives.md`                              |
| `directives/open-forge/cli/cli-interface-consistency` | `.agents/directives/open-forge/cli/cli-interface-consistency.md` |
| `skills/experience-design`                            | `.agents/skills/experience-design/SKILL.md`                      |

Each segment is the exact routed file stem or child folder slug. Entrypoint
filenames, `.agents`, and `.md` are representation details and do not appear in
the identity. Route identities are case-sensitive and use `/`.

There are no path aliases, optional extensions, leading slash, current or
parent segments, or implicit case correction. `route list`, `route inspect`,
`context`, and `find` consume these identities and report the canonical
workspace-relative source separately. Completion and high-confidence error
suggestions may show an exact identity but never execute a correction.

Topology mutations use a separate explicit path grammar. `route init` and
`route rebuild` never reinterpret one operand between a natural identity and a
filesystem path.

An ordinary file and a child entrypoint with the same exact route segment are
ambiguous. Neither is selected until the collision is resolved. Exact
case-distinct identities remain distinct and are displayed with their exact
spelling.

`route init ./.agents/guidance/product` initializes the entrypoint chain ending
at `.agents/guidance/product/_product.md`. Generic `create` continues to accept
an explicit destination filename. The two commands therefore expose different,
visible jobs.

### Authored Topology

The exact selected `.agents/loader.md` is the canonical inventory root. The
CLI does not parse `AGENTS.md` to discover another loader or search ancestors
for a different installation. Every routable folder uses its exact canonical
`_{folder}.md` entrypoint. Direct Markdown files are leaf routes. Discovery
derives direct filesystem children independently of generated `Entries` and
descends only through a recognized child entrypoint.

The canonical `SKILL.md` is a native leaf beneath the Skills root route or one
of its routed scopes. Its package resources follow the Skill contract and do
not become route children automatically.

The replacement accepts no legacy entrypoint aliases, backtick entries, or
renamed skill entrypoint. The frozen CLI may continue to read those forms until
its command root is retired; replacement findings state the required canonical
migration. Accepted `rune:` and unscoped metadata are a deliberate input-only
compatibility boundary rather than legacy route syntax.

Links, junctions, special nodes, physical escapes, and portable path ambiguity
use the shared containment contract. A blocked child does not redirect or
expand discovery.

### Topology Mutation Paths

Topology mutation paths are exact, contained, and workspace-relative beneath
the selected workspace. They begin with `.agents` or `./.agents`, normalize to
the same canonical `.agents/...` path, and never receive an implicit prefix.
Absolute paths, parent traversal, another content root, physical escape, and
correction based on filesystem similarity are invalid.

`route init <path>` identifies one directory chain after the selected
workspace's `.agents` boundary. Every missing directory and canonical entrypoint in that
chain becomes one planned effect. Existing valid entrypoints remain authored
truth. An incompatible or unsafe segment blocks the complete chain.

The final entrypoint receives explicit metadata and optional Template body.
Every intermediate entrypoint, and a final entrypoint without explicit
metadata, receives its exact folder slug as placeholder `description`,
`NeedsAuthoring` as its tag, and no `responsibility`. Template frontmatter and
generated-region markers never transfer. The CLI creates the canonical
generated `Entries` region as derived state.

An explicit rebuild starts from one or more exact workspace-relative
`.agents/...` paths, with an optional leading `./`. A path
may identify the `.agents` directory with a loader, a routed
directory with its canonical entrypoint, an exact entrypoint, or an ordinary
routed Markdown file with a direct exposing entrypoint available in the same
topology.

Selection follows the ordinary rebuild closure: the exposing or selected
entrypoint, its complete locally routed subtree, and its direct parent when
that parent is present. It does not invent a loader, search ancestors for a
different workspace, synthesize an unavailable parent, or classify the source
tree as an installed Framework.

The result retains every source path and reports any unavailable owner or
parent explicitly. A recognizable loader makes the selected workspace's
`.agents` tree rooted. Without one, the same tree remains detached. Selecting
a Framework source or Extension payload through `--workspace` therefore
permits source maintenance without nested-root discovery or installation
inference.

### Metadata

Ordinary files and entrypoints require canonical `open-forge` frontmatter:

- Non-empty `description`.
- Non-empty, unique string `tags`.
- Optional non-empty `responsibility`.

The focused frontmatter boundary recognizes only `description`,
`responsibility`, and `tags`. Canonical output places them beneath
`open-forge:`. Input compatibility may place the same complete metadata set
beneath `rune:` or at the document root. One document cannot mix recognized
sources or repeat a supported field.

Descriptions and responsibilities accept non-empty YAML string scalars in
plain, single-quoted, double-quoted, literal block, or folded block form. Tags
accept one non-empty YAML sequence of string scalars in inline flow, multiline
flow, or block form. Duplicate or empty tags, malformed syntax, unsupported
value kinds, and ambiguous sources are findings. Plain-scalar comments follow
YAML separation rules; quote authored text when `#` could be read as a comment.

Unknown canonical `open-forge:` fields are invalid. Unknown `rune:` or root
fields are ignored and preserved for their external consumer. The boundary
parses one size-bounded frontmatter document with strict YAML 1.2 syntax and a
failsafe scalar schema, then applies Open Forge validation. It rejects multiple
documents, aliases, anchors, explicit tags, directives, nested unsupported
values, and every other construct outside this metadata contract. It never
salvages a malformed supported field or exposes parser-library nodes and errors
to consumers.

Canonical writing remains independent from compatible input style. Open Forge
constructs the exact ordered scoped metadata shape, emits its preferred inline
tag sequence, and uses the syntax implementation only for correct scalar
representation. Accepting a syntactic form never broadens the Framework fields
or value semantics.

Native Skills use their required `name` and `description`; the inventory adds
their established Skill classification without rewriting the package.
The loader has its own fixed root identity and does not require routed-file
frontmatter.

### Authored Truth And Generated Projection

Filesystem topology and source frontmatter are authoritative for the route
inventory. Generated `Entries` are a verified, reconstructable projection.

For every loader or entrypoint, the inventory independently derives its direct
children, canonical descriptions, tags, destinations, and order, then compares
that expected region with the marker-bounded authored file:

```text
direct authored children
  -> validated metadata
  -> deterministic expected Entries
  -> compare current generated region
```

Ordering uses a runtime-independent portable folded key followed by exact
spelling. It never depends on process locale.

A stale generated region is an `attention` finding. It does not erase an
authored child, add a deleted child, or become a second semantic authority.
Read-only consumers use the authored projection and expose the divergence.
`route rebuild` or `repair` plans the derived correction.

Extension add, update, and remove also project affected generated regions from
the complete assembled destination. Those rebuilds are mandatory planned
effects of the selected lifecycle operation, not authored payload collisions,
Extension-owned bytes, or hidden post-application work.

This preservation-first choice also applies to loading tags. A stale generated
copy cannot silently suppress or invent #HistoricalLoadNow or #KeepInMind behavior in
deterministic CLI output.

### Scope And Overwrites

Every node retains its exact parent chain. The inventory does not classify
arbitrary slugs as special scope types; loaded ancestors establish scope and
inheritance.

An adjacent `{name}.overwrite.md` is attached to its base node. It shares the
base route identity, is emitted immediately after the base, and is never
independently indexed or selected. An orphan overwrite is a finding.

### Context Projection

`context` projects one ordered stream from the inventory:

1. Canonical workspace `AGENTS.md`.
2. Loader and visible authored #HistoricalLoadNow closure.
3. Every routed #KeepInMind result in inventory order, including any missing
   parent chain needed for scope, then its visible #HistoricalLoadNow closure.
4. Explicit selected routes in argument order, including any missing parent
   chain, then each selected entrypoint's visible #HistoricalLoadNow closure.
5. Every adjacent overwrite immediately after its base.

Sources are globally deduplicated by canonical source identity. The first
placement wins; every additional loading reason is retained as provenance.
Multiple selected scopes remain separate chains rather than receiving a
synthetic merge or precedence.

Example:

```text
AGENTS.md                                      workspace-entry
.agents/loader.md                              baseline
.agents/directives/_directives.md              load-now
.agents/directives/root.md                     load-now
.agents/memory/_memory.md                      continuity-ancestor
.agents/memory/note.md                         continuity
.agents/memory/note.overwrite.md               overwrite
.agents/memory/manual.md                       selected
```

Ordinary Markdown links are checked through the local-reference inventory.
They never add bodies to this projection.

### Consumer Projections

| Consumer        | Projection                                                                                |
| --------------- | ----------------------------------------------------------------------------------------- |
| `context`       | Ordered bodies, reasons, chains, and overwrite provenance                                 |
| `find`          | Exact metadata matches within optional route subtrees                                     |
| `route list`    | Direct or bounded descendant summaries                                                    |
| `route inspect` | One node's source, chain, metadata, children, references, and findings                    |
| `doctor`        | Complete topology, metadata, generated, overwrite, containment, and reachability findings |
| `route rebuild` | Expected generated regions for the selected rooted or detached effect closure             |

Consumers do not rescan, privately parse metadata, or reinterpret route
identity.

### Invalid And Incomplete State

The inventory retains every safe fact it can prove and attaches findings to
their exact source or boundary. A syntactically valid but unknown selected
route is an invalid request. Ambiguous route identity or unavailable
containment blocks that selection. Unrelated findings do not erase valid
routes.

If malformed metadata could hide a #KeepInMind route, `context` returns known
bodies with `attention` and explicitly states that continuity discovery is
incomplete. It never claims a complete context set after silent omission.

Named resource limits bound directories, files, bytes, depth, and elapsed
work. A reached limit produces explicit incomplete evidence rather than a
partial success disguised as complete.

## Boundaries

The inventory is one read model over authored Open Forge topology. It never
uses generated `Entries` as semantic authority, discovers another workspace or
loader, interprets one operand as both a route identity and filesystem path,
turns ordinary links into route children, guesses an ambiguous identity, or
hides incomplete discovery behind partial success.

## Verification

Direct tests prove natural route identity, canonical and compatible metadata
forms, rejected ambiguity, portable ordering, generated-region projection,
overwrite attachment, context ordering, provenance retention, and every named
invalid or incomplete state.

Frontmatter evidence covers plain, quoted, literal, and folded strings; inline
and multiline flow sequences; block sequences; comments and `#` boundaries;
duplicate keys and tags; mixed metadata sources; aliases, anchors, explicit
tags, directives, multiple documents, nested values, malformed input, resource
limits, stable positions, and canonical serialization. Tests assert Open Forge
facts and findings rather than parser-library messages or node shapes.

Real-workspace integration tests prove that authored topology remains
authoritative when generated `Entries` are stale, linked or escaping nodes are
present, exact case and segment collisions occur, and rooted or detached
rebuild closures select an entrypoint, its subtree, and its available direct
parent. The same inventory must produce the documented projections for every
consumer without private rescanning or reinterpretation.

Bounded-work tests prove explicit incomplete evidence for every resource
limit. Built-process tests cover representative `context`, `find`, route
inspection, and rebuild results while leaving exhaustive semantic coverage at
the direct and integration boundaries.

## Related Current Sources

- [Framework routing model](../../framework/routing/model.md)
- [Framework scope and inheritance](../../framework/routing/scope.md)
- [Framework loading and continuity](../../framework/routing/loading.md)
- [Routed Markdown contract](../../framework/markdown/routes.md)
- [Local references](local-references.md)
- [Diagnosis and repair](diagnosis-and-repair.md)
- [Markdown document facts Pattern](../../../../../patterns/open-forge/cli/markdown/markdown-document-facts.md)
- [Frontmatter YAML boundary Decision](../../../decisions/cli/cli-frontmatter-yaml-boundary.md)
