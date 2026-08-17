---
open-forge:
  description: Historical council evidence for the settled route-family contract closure
  responsibility: Preserve route-family council history, exact source coverage, material dissent, and the accepted authorities without making the record current contract authority
  tags: [Memory, Archived, Contextual, Historical, CLI, Release, Council, Analysis, Route, Contract, Migration, Locality]
---

# Route Family Contract Migration Council

## Status And Authority

This is archived, contextual, historical review material from the route-family
contract review. It records three independent grounded lenses, a lossless
heading map, material disagreements, and the council-time execution
recommendation. The record does not become a command authority source.

## Status At Archival

- **Origin:** `.agents/memory/working/cli-release/review/route-family-contract-migration-council.md`
- **Archived because:** Queues 25, 26, and 27 settled the `route init`, `route
  create`, and `route update` contract refinements, so this council no longer
  represents an unresolved review unit.
- **Current meaning authorities:** the [Route Inspect Interface](../../../crystallized/documents/cli/contracts/route/inspect/interface.md)
  and [Behavior](../../../crystallized/documents/cli/contracts/route/inspect/behavior.md),
  [Route Init Interface](../../../crystallized/documents/cli/contracts/route/init/interface.md)
  and [Behavior](../../../crystallized/documents/cli/contracts/route/init/behavior.md),
  [Route Create Interface](../../../crystallized/documents/cli/contracts/route/create/interface.md)
  and [Behavior](../../../crystallized/documents/cli/contracts/route/create/behavior.md),
  [Route Update Interface](../../../crystallized/documents/cli/contracts/route/update/interface.md)
  and [Behavior](../../../crystallized/documents/cli/contracts/route/update/behavior.md),
  together with [CLI-D090](../../../working/cli-release/decision-agenda.md),
  define current route-family meaning. Command-set entrypoints provide routing
  and help only. The [settled Review Queue](queue.md)
  records disposition rather than command meaning. Gate 2 remains open.

The former mixed paths are named below only as council-time history. They are
deleted and are not linked. The section under **Historical Council-Time Record**
preserves the authority boundary and recommendations that applied before
cutover; it is not current status. The remainder of this file preserves that
historical body. Its recommendations, alternatives, and unresolved questions
are context, not current authority.

## Historical Council-Time Record

At council time, the four mixed command files remained authoritative for every
command fact until an explicit reviewed cutover named replacements:

- `commands/route-inspect-command.md`
- `commands/route-init-command.md`
- `commands/route-create-command.md`
- `commands/route-update-command.md`

The destinations below were a preparation map during the council round. This
record did not create them or update generated navigation. Later authorized
preparation created the then-non-authoritative files and their generated
navigation. The recommendation and its later execution did not transition
authority at that time.

## Question

How should the four authoritative mixed `route` command sources be migrated
additively into public-path-local Interface and Behavior contract sets so that
the grouped route boundary is clear, every source fact remains accounted for,
the result stays simple to use and verify, and no implementation design or
shared contract is promoted before it has earned that scope?

## Decision Frame

The audience was the maintainer reviewing the next additive contract-migration
unit. The purpose was to make the route-family preparation boundary explicit
before candidate files or generated navigation were authored.

The criteria are authority clarity, public-path locality, lossless fact
accounting, absence of duplicate definitions, grouped-help discoverability,
conformance effort, implementation neutrality, reversibility, and the smallest
permanent structure that preserves the accepted contract roles.

The preparation stopping condition was a candidate set with a complete exact
map, no unresolved authority conflict hidden as a choice, no unnecessary shared
scope, and no premature Technical Design. That candidate set now exists. The
mixed sources remain authoritative until the maintainer accepts an explicit
cutover.

## Evidence Basis

The lenses were grounded against the four council-time mixed command sources,
the Decision Agenda, the Contract Migration Ledger, the current CLI
command-contract Working Documents and Templates, the shared CLI contracts, and the linked Framework
contracts for routing, Markdown, paths, overwrites, and Templates. The council
preserves factual constraints from those sources and keeps structural and UX
recommendations contextual.

## Constraints

- Preserve the accepted Gate 2 meaning in all four mixed sources. They define
  intended behavior, not shipped executable behavior.
- Keep Interface meaning complete and caller-facing. Keep Behavior meaning
  deterministic, technology-neutral, and subordinate to the Interface.
- Put each leaf's files under its final public command path. A group entrypoint
  may route children, but it must not become a second contract.
- Stage any future replacement additively. The four mixed sources remain the
  authority until fact coverage, conflict resolution, independent review, and
  explicit maintainer acceptance complete.
- Use the existing Framework contracts for routing, loading, scope, path
  identity, overwrites, Markdown representation, compatibility, and Templates.
  Link to those sources instead of copying their detailed definitions.
- Add no route-family shared contract beyond the routing-only group entrypoint.
  Shared meaning must have real consumers and a nearest useful common scope.
- Keep the `route create` and `route update` Template overlap local for now. Do
  not promote it to a command-family Template contract.
- Do not create a `technical-design.md` yet. The current sources describe
  technology-neutral behavior and defer Gate 3 choices; they do not select an
  implementation design.
- Do not assign permanent requirement IDs or extend the temporary migration
  label system to this record or the future route-family files.
- This council round created only this review record. It did not author the
  proposed candidates, claim cutover, regenerate other files, or edit another
  path. Later authorized preparation performed the separate additive work.

## Common Ground

The three lenses agree on these points:

- The final public command paths are `route inspect`, `route init`,
  `route create`, and `route update`. The group is a coherent family because
  the four operations share a route subject and lifecycle boundary, not because
  they share one domain result.
- `route` is a group, not a domain operation. Group help exposes its child
  operations; invoking the group does not inspect, initialize, create, or
  update a source.
- Each leaf needs a routing-only contract-set entrypoint plus local Interface
  and Behavior files. The entrypoints make the files discoverable and state
  authority; they do not repeat the detailed command contracts.
- The group entrypoint's generated `Entries` and the leaf entrypoints' generated
  `Entries` are navigation metadata. They cannot accept candidates, define
  behavior, or replace the mixed sources.
- `route inspect` is read-only. `route init`, `route create`, and `route update`
  are bounded mutations with explicit planning, dry-run parity, preflight,
  revalidation, verification, and recovery boundaries from their current
  contracts.
- The shared global-flag and source-reference contracts remain shared by link,
  not by copied definitions. The Index contract remains the source for the
  generated-navigation projection used by the three mutation commands.
- The routing and Markdown Framework documents remain authoritative for the
  meaning of entrypoints, direct entries, loading, scope, overwrites, paths,
  canonical representation, compatibility input, and metadata. The Template
  Framework document remains authoritative for one-time instantiation and
  relinquished ownership.
- A candidate's completeness, routability, generated navigation, or review
  status cannot change its authority. No candidate is ready for cutover while
  any source fact is omitted, weakened, strengthened, contradicted, or left
  without a visible unresolved state.

## Independent Grounded Lenses

### Grouped-Route Authority And Locality

The routing and locality lens starts from the [Routing
Model](../../../crystallized/documents/framework/routing/model.md), the
[Shared CLI Operation Contract](../../../crystallized/documents/cli/shared-operation-contract.md),
the [CLI Command Contract Set Working Document](../../../crystallized/documents/cli/command-contract-set.md),
and the [Nearest Shared Scope Pattern](../../../../patterns/software/source-locality/nearest-shared-scope.md).
Those sources keep direct-child navigation, authority, public command identity,
and local behavior distinct.

This lens finds the public path to be the correct final locality boundary. A
group entrypoint can expose the four leaf scopes and explain their routing-only
relationship. Each leaf entrypoint can expose its Interface and Behavior
documents and state that the mixed source remains authoritative. Neither kind
of entrypoint should carry command syntax, mutation rules, result schemas, or a
summary detailed enough to compete with a leaf contract.

The lens rejects a family-wide behavior file merely because the commands share
the word `route`. Inspection and the three mutations have different authority,
effects, and result conditions. It also rejects treating a generated `Entries`
line as a contract or treating the group folder as a source of inherited
domain meaning. The nearest useful shared scope is the group router itself,
and its role is navigation only.

The main risk is authority drift: a readable group file or leaf entrypoint can
quietly become the place where a short version of each command is maintained.
The remedy is a narrow entrypoint responsibility, explicit links to the local
Interface and Behavior files, and a hard rule that the mixed source remains the
authority through cutover.

### Lossless Accounting

The lossless lens starts from the [CLI Contract Migration
Ledger](../contract-migration-ledger.md), the four mixed sources, the
[Command Interface Contract Working Document](../../../crystallized/documents/cli/command-interface-contract.md),
and the [Command Behavior Contract Working Document](../../../crystallized/documents/cli/command-behavior-contract.md).
It treats every heading as a coverage boundary rather than as a suggestion for
where a convenient summary could go.

This lens requires a source-heading map that accounts for the title, status,
purpose, syntax, each subject or state section, every output example subsection,
semantic results, errors, non-goals, verification requirements, related links,
and the fenced scaffold example in `route init`. It preserves requirement
strength, defaults, omission behavior, conditions, exceptions, examples,
verification modality, and explicit Gate 3 uncertainty. It does not use
synthetic fact labels; the source path and exact heading are the traceability
coordinates.

The lens allows one source heading to feed both destination files only when the
meaning is genuinely split by responsibility. The Interface keeps the
caller-visible promise. The Behavior links to that promise and defines only
the deterministic resolution, effect, safety, recovery, or conformance
mechanics needed to satisfy it. A second detailed copy is a migration defect,
not extra coverage.

The main risks are omissions hidden by broad ranges, changes from `must` to
`should` or from unresolved to decided, lost examples, and accidental transfer
of authority when a candidate looks more organized than its source. The remedy
is the exact map below, a source-by-source comparison, and a hard stop on any
unaccounted fact.

### Simplicity, UX, And Conformance

The simplicity and conformance lens starts from the [CLI Command Contract Set
Working Document](../../../crystallized/documents/cli/command-contract-set.md),
the [CLI contract document Templates](../../../../templates/cli/documents/_documents.md),
the [Global CLI Flags contract set](../../../crystallized/documents/cli/contracts/shared/global-flags/_global-flags.md), the [Open Forge
Principles](../../../crystallized/documents/principles.md), and the existing
review council style. It asks whether the proposed structure reduces reader
guesswork without adding empty ceremony.

This lens accepts the group plus four leaves because the public paths make help,
review, and future implementation ownership predictable. It also rejects an
empty Technical Design file and a speculative shared Template contract. An
unneeded file is not a neutral abstraction: it adds a route, an entry, a review
surface, and another place that might appear authoritative. The group therefore
needs only routing content, and a leaf needs only the two contract files that
have current material to express.

For the command journey, this lens checks discoverability, hierarchy,
comprehension, feedback, control, efficiency, consistency, error prevention,
recovery, trust, and the accessibility of help and result wording. The relevant
states include group help, leaf help, valid execution, empty or no-op results,
invalid input, blocked authority or identity, interruption, and mutation
recovery. Those checks constrain conformance evidence; they do not add a new
public command or choose an implementation.

The material dissent is that a simpler one-file-per-command shape could reduce
ceremony further. That alternative would reopen the accepted command-local
Interface/Behavior file shape and would make the public/technology-neutral
boundary less inspectable. It is preserved as dissent, not selected for this
migration. The lens instead removes ceremony at the group and Template-sharing
boundaries while retaining the accepted leaf shape.

The main UX risk is a group help page that is either empty and confusing or so
detailed that it becomes a competing contract. The conformance boundary is
therefore narrow: group help must make the four child operations findable and
say that the group performs no domain operation; leaf help must come from the
leaf Interface; neither group navigation nor help may invent a shortcut,
default, or alias.

## Material Disagreements And Synthesis

### Technical-Design Placement

The current sources mention parser and filesystem libraries, backup filenames,
exact schemas, numeric exits, and .NET source boundaries as deferred Gate 3
questions. They also describe typed stages, derived facts, bounded effects, and
recovery properties in technology-neutral terms. One possible reading would
reserve a Technical Design file now so those details have somewhere to land.

The stronger reading is that a placeholder would misclassify the current
material. A deferred choice is not a selected design, and a technology-neutral
invariant belongs in Behavior. The current Technical Design Working Document also says
not to create the file when concrete technology choices do not yet need a
visible home.

**Synthesis:** do not create a Technical Design for the group or any of the four
leaves now. Keep the current typed-stage, determinism, effect, safety, and
recovery meaning in the future Behavior files. Keep each explicit Gate 3
boundary visible in the future Interface or Behavior status and deferred-detail
sections. Add a leaf-local Technical Design only after a concrete
implementation choice exists and can be traced to the leaf contracts. A
cross-command Technical Design would require demonstrated shared choices and a
separate accepted scope; this recommendation does not create one.

### Shared Template Overlap

`route create` and `route update` both select a routed Template, strip its
frontmatter, copy body content without substitution, and reject an overwrite
Template. That overlap makes a shared command-family contract tempting.

The command-specific boundaries are not the same. Create requires destination
metadata for a new file, never overwrites a different target, and makes the
destination independently maintained. Update patches an existing source,
protects any authored body byte-for-byte, and applies a Template body only to a
frontmatter-only target. The [Templates](../../../crystallized/documents/framework/primitives/templates.md)
contract defines the shared Framework role, but it does not erase these command
boundaries.

**Synthesis:** retain `Template Selection` in the future create Interface and
Behavior files and `Template Body Completion` in the future update Interface and
Behavior files. Link both to the Framework Templates contract and, where a
command-specific relationship is useful, link to the other leaf. Do not create
`commands/route/template.md` or another route-family shared Template contract
now. Promotion requires multiple real consumers of one identical command
meaning, not merely similar vocabulary.

### Overall Synthesis

The three lenses converge on one additive execution recommendation:

```text
commands/
  route/
    _route.md                         # routing-only group entrypoint
    inspect/
      _inspect.md                     # routing-only leaf entrypoint
      interface.md
      behavior.md
    init/
      _init.md
      interface.md
      behavior.md
    create/
      _create.md
      interface.md
      behavior.md
    update/
      _update.md
      interface.md
      behavior.md
```

This was the council's proposed candidate shape. Later authorized preparation
created these files. The shape deliberately has no Technical Design file and no
route-family shared contract. The four existing mixed sources remain
authoritative while the candidates are reviewed and until the maintainer accepts
an explicit cutover.

## Group-Versus-Leaf Authority

### Group Entrypoint

The then-proposed, now-prepared `commands/route/_route.md` is a routing-only group entrypoint. Its
permitted job is to expose the four direct child entrypoints, state the
candidate authority boundary, and make the group-help relationship discoverable.
It may link to the child contract sets and the shared help rules.

It must not define a route-family syntax, operand grammar, flag, default,
Template rule, result, error, mutation, or shared behavior. It must not become
an authority merely because it is the visible parent or because its generated
`Entries` are complete. No source heading from a leaf command maps to a
domain-bearing section in this file.

The group-help boundary is narrow and comes from the shared CLI help behavior
and the four current sources: `route --help` exposes `inspect`, `init`,
`create`, and `update`; the group performs no domain operation and does not
resolve a workspace merely to display help. Detailed operand, flag, example,
result, error, and non-goal help belongs to the selected leaf Interface. The
group entrypoint records only the routing relationship, not a second help
contract.

### Leaf Entrypoints

Each now-prepared `commands/route/<leaf>/_<leaf>.md` is a routing-only contract-set
entrypoint. It identifies the leaf's local `interface.md` and `behavior.md`,
states that both are candidates during migration, and links the authoritative
mixed source. Its `Axioms` and `Entries` are routing structure, not command
meaning.

The leaf entrypoint must not summarize the leaf's public syntax, effects,
results, errors, or verification. It must not inherit authority from the group
or transfer authority to its children. Its generated `Entries` expose the local
files and do not define their content.

### Contract Files

The prepared `interface.md` is the proposed complete public contract for one
leaf. The prepared `behavior.md` is the proposed complete technology-neutral
behavior behind that Interface. The four mixed sources remain the authority for
both until cutover. The source map below identifies where each current heading
is expressed. The map did not itself authorize writing those files; later
maintainer direction authorized their additive preparation.

## Cross-Layer Link-Only Rules

- Put caller-visible syntax, operands, flags, defaults, repetition, composition,
  output, statuses, errors, scenarios, and non-goals in the leaf Interface.
  Behavior may cite those sections but must not add another public surface or a
  second status definition.
- Put deterministic resolution, current-fact coverage, selection, projection,
  read-only boundaries, mutation planning, dry-run parity, revalidation,
  verification, recovery, no-op behavior, and conformance mechanics in the
  leaf Behavior. When a rule is observable, link it to the exact Interface
  section that states the promise.
- Keep the group and leaf entrypoints routing-only. Their links express
  relationships; they do not merge authority, loading, scope, responsibility,
  or lifecycle.
- Link the current shared [Global Flags](../../../crystallized/documents/cli/contracts/shared/global-flags/_global-flags.md),
   [Source References](../../../crystallized/documents/cli/contracts/shared/source-references/_source-references.md),
   [Context](../../../crystallized/documents/cli/contracts/context/_context.md), [Status](../../../crystallized/documents/cli/contracts/status/_status.md),
   and [Index](../../../crystallized/documents/cli/contracts/index/_index.md) contract sets for
  shared CLI meaning. Do not copy their complete definitions into every leaf.
- Link the Framework routing, Markdown, and Template documents for Framework
  meaning. A command Behavior may state how it consumes that meaning, but it
  must not redefine the Framework contract.
- Keep create/update Template rules in their own leaf files. Link to the
  Framework Templates document for one-time instantiation and to the other leaf
  only for a narrow relationship; do not maintain a shared detailed copy.
- Split a mixed source heading by responsibility when necessary, but make one
  side the detailed definition and make the other side a citation or
  conformance relationship. Do not copy a paragraph into both files merely to
  make each file look complete.
- Split verification by evidence question: Interface records public-surface
  coverage, while Behavior records semantic, effect, safety, recovery, and
  process conformance. Preserve each source's `must` and `should` strength.
- Use source paths and exact headings for migration accounting. Do not add
  permanent requirement IDs, temporary labels, or a second identity system.

## Exact Source-Heading-To-Destination Map

The following map covers every authored heading and subsection in the four
mixed sources. Destinations are written relative to
`.agents/memory/working/cli-release/`; `<leaf>` is one of `inspect`, `init`,
`create`, or `update`. The destination files are proposed and do not currently
exist as a result of this record.

The standard destination headings are:

- `commands/route/<leaf>/_<leaf>.md`: `# route <leaf> Command Contract Set`,
  `## Status And Authority`, `## Contract Roles`, `## Axioms`, and `## Entries`.
- `commands/route/<leaf>/interface.md`: `# route <leaf> Interface Contract`,
  `## Status And Authority`, `## Purpose`, `## Syntax`, `## Operands`,
  `## Flags`, public fact sections retained from the source, `## Human Output`,
  `## Structured Output`, `## Semantic Results`, `## Errors`, `## Scenarios`,
  `## Non-Goals`, `## Verification`, and `## Related Current Sources`.
- `commands/route/<leaf>/behavior.md`: `# route <leaf> Behavior Contract`,
  `## Status And Authority`, `## Operation Invariants`, `## Request Resolution`,
  `## Current Facts And Coverage`, `## Selection And Result Formation`,
  `## Effects`, `## Safety And Recovery` where applicable,
  `## Presentation Relationship`, `## Conformance Evidence`, and
  `## Related Current Sources`.

The future Interface `## Scenarios` section will collect representative
invocation and result examples currently nested under source `Syntax`, target,
subject, and `Human Output` sections. It is an organizing destination, not a
new source fact or a reason to duplicate the examples in Behavior.

For every H1 row below, the same source title also maps to the leaf entrypoint
title `commands/route/<leaf>/_<leaf>.md` → `# route <leaf> Command Contract Set`.
For every `## Status` row, the lifecycle and authority statement also maps to
the leaf entrypoint's `## Status And Authority`. For every `## Related Accepted
Direction` row, the leaf entrypoint receives only the relevant links under
`## Contract Roles`; it receives no detailed command meaning. No other source
heading maps to a detailed section in `_leaf.md`.

A destination named in both the Interface and Behavior columns is a deliberate
responsibility split: the Interface owns the public statement, and Behavior
links to it while defining only the technology-neutral mechanics. A dash means
that the file receives no detailed definition for that heading.

### Route Inspect

| Source heading or subsection | Interface destination | Behavior destination |
| --- | --- | --- |
| `# Route Inspect Command` | `commands/route/inspect/interface.md` → `# route inspect Interface Contract` | `commands/route/inspect/behavior.md` → `# route inspect Behavior Contract` |
| `## Status` | `## Status And Authority` | `## Status And Authority` |
| `## Purpose` | `## Purpose` | `## Operation Invariants` for the deterministic route-profile promise and operation boundary |
| `## Syntax` | `## Syntax`, `## Operands`, and `## Flags`; the group-only help statement links to `commands/route/_route.md` and the shared help contract | `## Request Resolution` for input and flag resolution |
| `## Workspace And Subject` | `## Workspace And Subject` | `## Request Resolution` and `## Current Facts And Coverage` |
| `## Inspection Model` | `## Non-Goals` for no content, authority, or mutation side effects | `## Request Resolution`, `## Current Facts And Coverage`, and read-only `## Effects` |
| `## Identity` | `## Identity` | `## Request Resolution` and `## Current Facts And Coverage` |
| `## Reading Behavior` | `## Reading Behavior` | `## Current Facts And Coverage` |
| `### Task Start Or Resume` | `## Reading Behavior` → `### Task Start Or Resume` | `## Current Facts And Coverage` → `### Task Start Or Resume` |
| `### Automatic Reading Trigger` | `## Reading Behavior` → `### Automatic Reading Trigger` | `## Current Facts And Coverage` → `### Automatic Reading Trigger` |
| `### Later Reads` | `## Reading Behavior` → `### Later Reads` | `## Current Facts And Coverage` → `### Later Reads` |
| `## Context Cost` | `## Context Cost` | `## Current Facts And Coverage` and `## Selection And Result Formation` |
| `### Own Source` | `## Context Cost` → `### Own Source` | `## Current Facts And Coverage` → `### Own Source` |
| `### Added By Selection` | `## Context Cost` → `### Added By Selection` | `## Selection And Result Formation` → `### Added By Selection` |
| <code>### Automatically Read Below Through `#LoadNow`</code> | `## Context Cost` → <code>### Automatically Read Below Through `#LoadNow`</code> | `## Selection And Result Formation` → <code>### Automatically Read Below Through `#LoadNow`</code> |
| `### No Heaviness Score` | `## Context Cost` → `### No Heaviness Score` | `## Selection And Result Formation` for the no-recommendation boundary |
| `## Route Structure` | `## Route Structure` | `## Current Facts And Coverage` |
| `### Why There Is No Scope Count` | `## Route Structure` → `### Why There Is No Scope Count` | `## Current Facts And Coverage` → `### Why There Is No Scope Count` |
| `## Rules And Customization` | `## Rules And Customization` | `## Current Facts And Coverage` |
| `## Human Output` | `## Human Output` | `## Presentation Relationship` |
| `## Structured Output` | `## Structured Output` | `## Presentation Relationship` |
| `## Semantic Results` | `## Semantic Results` | `## Selection And Result Formation` |
| `## Errors` | `## Errors` | `## Request Resolution` and `## Current Facts And Coverage` for the conditions that make a result invalid, blocked, or incomplete |
| `## Non-Goals` | `## Non-Goals` | `## Operation Invariants` and `## Effects` by link only; no second non-goal list |
| `## Verification Requirements` | `## Verification` for public inputs, outputs, statuses, and boundaries | `## Conformance Evidence` for graph, reading, measurement, topology, inheritance, presentation, and semantic-result evidence |
| `## Related Accepted Direction` | `## Related Current Sources` | `## Related Current Sources` |

### Route Init

| Source heading or subsection | Interface destination | Behavior destination |
| --- | --- | --- |
| `# Route Init Command` | `commands/route/init/interface.md` → `# route init Interface Contract` | `commands/route/init/behavior.md` → `# route init Behavior Contract` |
| `## Status` | `## Status And Authority` | `## Status And Authority` |
| `## Purpose` | `## Purpose` | `## Operation Invariants` for deterministic chain selection, convergence, and no-op behavior |
| `## Syntax` | `## Syntax`, `## Operands`, and `## Flags`; the group-only help statement links to the routing-only group entrypoint and shared help contract | `## Request Resolution` |
| `## Route Target` | `## Route Target` | `## Request Resolution` and `## Current Facts And Coverage` |
| `## Chain Selection` | `## Chain Selection` | `## Request Resolution` and `## Current Facts And Coverage` |
| `## Fixed Entrypoint Scaffold` | `## Fixed Entrypoint Scaffold` for the exact public scaffold and its byte-level example | `## Effects` for intended scaffold bytes and valid route representation |
| `## Draft Metadata` | `## Draft Metadata` | `## Request Resolution` and `## Effects` |
| `## Intended Topology And Generated Entries` | `## Intended Topology And Generated Entries` | `## Effects` and `## Safety And Recovery` |
| `## Planning And Effects` | `## Planning And Effects` for the observable plan, dry-run, and application boundary | `## Effects` and `## Safety And Recovery` |
| `## Dry Run And Apply` | `## Dry Run And Apply` | `## Effects` and `## Safety And Recovery` |
| `## Human Output` | `## Human Output` | `## Presentation Relationship` |
| `### Verified No-Op` | `## Human Output` → `### Verified No-Op` | `## Presentation Relationship` |
| `### Successful Application` | `## Human Output` → `### Successful Application` | `## Presentation Relationship` |
| `### Successful Dry Run` | `## Human Output` → `### Successful Dry Run` | `## Presentation Relationship` |
| `## Structured Output` | `## Structured Output` | `## Presentation Relationship` |
| `## Semantic Results` | `## Semantic Results` | `## Selection And Result Formation` |
| `## Errors And Non-Goals` | `## Errors` and `## Non-Goals` | `## Request Resolution`, `## Effects`, and `## Safety And Recovery` by link only |
| `## Verification Requirements` | `## Verification` for target grammar, metadata, help-visible results, and statuses | `## Conformance Evidence` for scaffold bytes, topology, planning, dry-run, writes, recovery, and convergence |
| `## Related Accepted Direction` | `## Related Current Sources` | `## Related Current Sources` |

The lines `# documents`, `## Axioms`, and `## Entries` inside the fenced
scaffold example under `## Fixed Entrypoint Scaffold` are example bytes, not
authored headings in the source document. They are nevertheless accounted for:
the exact literal slug, inherited `Axioms` sentinel, final `Entries` heading,
marker pair, and empty generated line remain under the Interface scaffold
section and the Behavior effect/conformance sections. They do not become
separate contract sections in the migration map.

### Route Create

| Source heading or subsection | Interface destination | Behavior destination |
| --- | --- | --- |
| `# Route Create Command` | `commands/route/create/interface.md` → `# route create Interface Contract` | `commands/route/create/behavior.md` → `# route create Behavior Contract` |
| `## Status` | `## Status And Authority` | `## Status And Authority` |
| `## Purpose` | `## Purpose` | `## Operation Invariants` for deterministic target selection, independent destination content, and verified no-op behavior |
| `## Syntax` | `## Syntax`, `## Operands`, and `## Flags` | `## Request Resolution` |
| `## File Target` | `## File Target` | `## Request Resolution` and `## Current Facts And Coverage` |
| `## Destination Metadata` | `## Destination Metadata` | `## Request Resolution` and `## Effects` |
| `## Template Selection` | `## Template Selection` | `## Current Facts And Coverage`, `## Selection And Result Formation`, and `## Effects`; link to the Framework Templates contract rather than a family shared contract |
| `## Existing Target` | `## Existing Target` | `## Current Facts And Coverage`, `## Selection And Result Formation`, and `## Safety And Recovery` |
| `## Generated Navigation` | `## Generated Navigation` | `## Effects` and `## Safety And Recovery` |
| `## Planning And Effects` | `## Planning And Effects` | `## Effects` and `## Safety And Recovery` |
| `## Dry Run And Apply` | `## Dry Run And Apply` | `## Effects` and `## Safety And Recovery` |
| `## Human Output` | `## Human Output` | `## Presentation Relationship` |
| `### Verified No-Op` | `## Human Output` → `### Verified No-Op` | `## Presentation Relationship` |
| `### Successful Application` | `## Human Output` → `### Successful Application` | `## Presentation Relationship` |
| `### Successful Dry Run` | `## Human Output` → `### Successful Dry Run` | `## Presentation Relationship` |
| `## Structured Output` | `## Structured Output` | `## Presentation Relationship` |
| `## Semantic Results` | `## Semantic Results` | `## Selection And Result Formation` |
| `## Errors And Non-Goals` | `## Errors` and `## Non-Goals` | `## Request Resolution`, `## Effects`, and `## Safety And Recovery` by link only |
| `## Verification Requirements` | `## Verification` for target grammar, metadata, Template selection, output, and statuses | `## Conformance Evidence` for parent projection, planning, dry-run, application, verification, recovery, and convergence |
| `## Related Accepted Direction` | `## Related Current Sources` | `## Related Current Sources` |

### Route Update

| Source heading or subsection | Interface destination | Behavior destination |
| --- | --- | --- |
| `# Route Update Command` | `commands/route/update/interface.md` → `# route update Interface Contract` | `commands/route/update/behavior.md` → `# route update Behavior Contract` |
| `## Status` | `## Status And Authority` | `## Status And Authority` |
| `## Purpose` | `## Purpose` | `## Operation Invariants` for field-patch semantics, authored-body preservation, and verified no-op behavior |
| `## Syntax` | `## Syntax`, `## Operands`, and `## Flags` | `## Request Resolution` |
| `## Target Source` | `## Target Source` | `## Request Resolution` and `## Current Facts And Coverage` |
| `## Metadata Patch` | `## Metadata Patch` | `## Request Resolution` and `## Effects` |
| `## Template Body Completion` | `## Template Body Completion` | `## Current Facts And Coverage`, `## Selection And Result Formation`, and `## Effects`; keep this local rather than promoting Template overlap |
| `## Body And Generated Preservation` | `## Body And Generated Preservation` | `## Effects` and `## Safety And Recovery` |
| `## Existing State And No-Ops` | `## Existing State And No-Ops` | `## Selection And Result Formation` and `## Effects` |
| `## Planning And Effects` | `## Planning And Effects` | `## Effects` and `## Safety And Recovery` |
| `## Dry Run And Apply` | `## Dry Run And Apply` | `## Effects` and `## Safety And Recovery` |
| `## Human Output` | `## Human Output` | `## Presentation Relationship` |
| `### Verified No-Op` | `## Human Output` → `### Verified No-Op` | `## Presentation Relationship` |
| `### Successful Application` | `## Human Output` → `### Successful Application` | `## Presentation Relationship` |
| `### Successful Dry Run` | `## Human Output` → `### Successful Dry Run` | `## Presentation Relationship` |
| `## Structured Output` | `## Structured Output` | `## Presentation Relationship` |
| `## Semantic Results` | `## Semantic Results` | `## Selection And Result Formation` |
| `## Errors And Non-Goals` | `## Errors` and `## Non-Goals` | `## Request Resolution`, `## Effects`, and `## Safety And Recovery` by link only |
| `## Verification Requirements` | `## Verification` for target grammar, field states, Template decisions, output, and statuses | `## Conformance Evidence` for preservation, intended-state planning, dry-run, writes, verification, recovery, and convergence |
| `## Related Accepted Direction` | `## Related Current Sources` | `## Related Current Sources` |

## Grouped Help Requirements

The group-help rules are a boundary to preserve during migration, not a new
route-family contract:

- `route --help` must expose the four child operations and their canonical
  command names. It must not run a domain operation or require workspace
  resolution.
- Help for `route inspect`, `route init`, `route create`, and `route update`
  must be generated from the selected leaf's complete Interface surface:
  operands, flags, defaults, examples, and related operations. It must not be
  reconstructed from the group entrypoint's description.
- The group has no `--all`, generic operation, implicit child selection, or
  domain result. A caller must choose a leaf to run an operation.
- Shared `--help` and `--version` terminal behavior remains in the Global CLI
  Flags contract. A group or leaf help request does not run the domain
  operation; domain operands and operation-specific flags remain invalid in a
  terminal help or version mode.
- Group child order, visible descriptions, and completion should derive from
  one command-definition source when implementation design is later chosen.
  This is a conformance check, not permission for `_route.md` to contain a
  second syntax or result definition.
- A complete group help surface must not be mistaken for acceptance. The
  candidate entrypoint and generated `Entries` remain contextual until explicit
  cutover.

## Preserved Unresolved Facts

The migration must carry these open details forward without choosing a value:

- **Streams:** The mixed route sources describe human and structured content but
  do not assign the normal human result stream or ordinary error stream. The
  shared contract assigns structured JSON to stdout and constrains diagnostics;
  it does not resolve every ordinary route-command stream. Do not infer stdout
  or stderr for those states.
- **Command-specific repetition:** Repeated `--tag` values have source-defined
  ordering and validation where stated. The route mutation sources do not
  define every repetition case for command-specific Boolean flags such as
  `--dry-run` and `--skip-git-check`, or for every one-value Template reference.
  Do not apply a global repetition rule to a command-specific flag without a
  source decision.
- **Schemas and exits:** Exact structured field names, schema versioning,
  compatibility rules, and numeric exit mapping remain deferred Gate 3 details.
  Semantic result names and meanings remain part of the current contracts.
- **Parser and serialization:** Exact YAML and Markdown parser behavior,
  compatibility parsing, frontmatter preservation, line endings, encoding,
  heading and link handling, and canonical serialization remain open where the
  sources defer them.
- **Filesystem and identity:** Exact filesystem APIs, physical identity,
  symlink and junction behavior, case and Unicode rules, atomic replacement,
  containment implementation, and test seams remain implementation-boundary
  choices unless a current contract states the observable safety property.
- **Backups:** The recovery policy and preservation goals are current, but exact
  backup filenames, collision handling mechanics, and related implementation
  boundaries remain deferred.
- **Concurrency:** Expected-state revalidation and preservation of unexpected
  concurrent edits remain current safety meaning. Lock scope, stale-lock
  handling, and broader cross-platform concurrency guarantees remain open.
- **Source boundaries:** Exact .NET modules, parser and serializer ownership,
  shared graph or mutation boundaries, and other implementation source
  boundaries remain open. No Technical Design should be invented to fill this
  gap.

## Likely Loss And Duplication Traps

- Treating `commands/route/_route.md` as a family contract because it is the
  first visible route file.
- Treating a leaf entrypoint or generated `Entries` as authoritative because it
  is routable, complete, or easy to load.
- Mapping an entire mixed source to Interface or Behavior without separating
  caller-visible facts from deterministic mechanics.
- Repeating exact syntax, result, error, or status definitions in Behavior and
  allowing them to drift from Interface.
- Losing the distinction between read-only inspection and the three mutations
  when a shared route family summary is written.
- Dropping source examples, omission states, finite flag values, no-op output,
  non-goals, or the difference between `attention`, `invalid`, `blocked`,
  `failed`, and `interrupted`.
- Weakening `must`, `should`, optional, prohibited, or unresolved language while
  compressing the source into a map.
- Losing the `route init` literal slug, inherited `Axioms` sentinel, final
  generated `Entries` section, marker pair, `NeedsAuthoring` behavior, or
  compatibility-file preservation by treating the scaffold as a mere example.
- Collapsing create and update Template behavior and accidentally transferring
  Template metadata, ownership, provenance, or continuing updates.
- Promoting similar Template vocabulary to a shared command contract before the
  two commands have one identical reusable meaning.
- Creating empty Technical Design files and later filling them with product or
  Behavior guarantees.
- Copying global flags, source-reference grammar, Index projection, or Framework
  routing meaning into each leaf instead of linking to the authoritative source.
- Inventing normal output streams, flag repetition behavior, schemas, exit
  numbers, parser choices, backup names, lock semantics, or module boundaries.
- Confusing automatic path-derived source identity with a permanent requirement
  identity system.
- Deleting or editing the mixed sources, changing their links, or claiming a
  generated-index cutover before the complete map and review are accepted.
- Updating generated navigation as part of this contextual record and thereby
  exceeding the explicit one-file task boundary.

## Verification Checklist

- [ ] Confirm that every authored heading and subsection in each of the four
  mixed sources appears in the map, including all three `Human Output`
  subsections and the fenced `route init` scaffold facts.
- [ ] Compare every mapped destination against the source text and preserve
  defaults, conditions, exceptions, examples, prohibitions, and requirement
  strength.
- [ ] Confirm that Interface files contain the complete public surface and that
  Behavior files contain no new operand, flag, alias, output shape, status, or
  other public meaning.
- [ ] Confirm that each Interface/Behavior cross-layer relationship is a link
  or conformance reference rather than a second detailed definition.
- [ ] Confirm that the group entrypoint and all four leaf entrypoints are
  routing-only, that group help performs no domain operation, and that leaf help
  comes from the corresponding Interface.
- [ ] Confirm that the final public path is represented by `route/inspect`,
  `route/init`, `route/create`, and `route/update`, with no staging-name
  substitution.
- [ ] Confirm that no route-family shared contract, shared Template contract,
  or Technical Design is introduced by the future preparation.
- [ ] Confirm that shared global flags, source references, Index behavior, and
  Framework meaning are linked to their authoritative sources rather than
  copied.
- [ ] Confirm that the unresolved stream, repetition, schema, exit, parser,
  filesystem, backup, concurrency, and source-boundary facts remain visibly
  unresolved.
- [ ] Confirm that no permanent requirement IDs or temporary migration labels
  occur in the future contract files.
- [ ] Confirm that the four mixed sources remain untouched and authoritative
  until an explicit maintainer-accepted cutover.
- [ ] For this record, confirm that only this new file is created. Do not run an
  index or repair action that changes another file.
- [ ] During a later authorized candidate-preparation step, validate local
  links, entrypoint structure, generated navigation, and source coverage before
  any authority cutover. That later validation is not performed by this record.

## Hard Stops

Stop candidate preparation or cutover when any of these conditions occurs:

- A source heading, subsection, example, or fact has no exact destination, or
  its requirement strength, default, exception, or uncertainty changes.
- A candidate or generated entry is treated as authoritative before explicit
  cutover, or the mixed source is deleted, weakened, or silently superseded.
- The group entrypoint begins to define domain syntax, behavior, result, error,
  Template, or implementation meaning.
- A leaf entrypoint duplicates detailed command meaning or presents a
  generated navigation line as authority.
- Interface and Behavior disagree, or Behavior adds public surface instead of
  linking to Interface.
- A Technical Design is created without a concrete implementation choice, or
  it is used to hide a product, safety, or technology-neutral contract conflict.
- Create/update Template overlap is promoted without evidence of one identical
  shared contract and an accepted nearest shared scope.
- An unresolved stream, repetition, schema, exit, parser, filesystem, backup,
  concurrency, or source-boundary question is answered by inference.
- Permanent IDs or another requirement-identity system are introduced.
- Group help becomes a second manually maintained contract, invents aliases or
  defaults, or runs a domain operation.
- Additive staging cannot preserve the current four-source authority, exact
  rollback path, or complete fact-level review.
- The work would edit a file outside the explicitly authorized new review
  record.

## Sources Consulted

### Authority, Routing, And Prose Rules

- [Open Forge Loader](../../../../loader.md), including its authority, routing,
  loading, and tag rules.
- [Writing Directive](../../../../directives/writing.md), [Open Forge Writing
  Standard](../../../crystallized/documents/maintenance/writing.md), and [Open
  Forge Dictionary](../../../crystallized/documents/maintenance/helpers/dictionary.md).
- [Maintainer Decision Authority](../../../../directives/decision-authority.md),
  [Source Locality Directive](../../../../directives/source-locality.md), and
  [Deliberate Framework Change Directive](../../../../directives/open-forge/framework/deliberate-framework-change.md).
- [Experience Design Skill](../../../../skills/experience-design/SKILL.md),
  [Review Experience Design](../../../../skills/experience-design/references/review-experience-design.md),
  and [Map User Experience](../../../../skills/experience-design/references/map-user-experience.md)
  for the grouped-help journey, states, discoverability, comprehension,
  consistency, trust, and evidence boundaries.
- [CLI Review Queue](queue.md), [Archived Command Contract Set Council](command-contract-set-council.md),
  [Path, Reference, And Discovery Council](path-reference-council.md),
  [Diagnosis And Repair Council](check-fix-council.md), and [Additional Agent
  Accelerator Council](agent-accelerator-council.md) for the established council
  style and contextual authority boundary.

### CLI Contract Sources

- The current split [route group](../../../crystallized/documents/cli/contracts/route/_route.md), [route inspect](../../../crystallized/documents/cli/contracts/route/inspect/_inspect.md),
  [route init](../../../crystallized/documents/cli/contracts/route/init/_init.md), [route create](../../../crystallized/documents/cli/contracts/route/create/_create.md),
  and [route update](../../../crystallized/documents/cli/contracts/route/update/_update.md) contract sets.
- [Global CLI Flags contract set](../../../crystallized/documents/cli/contracts/shared/global-flags/_global-flags.md), [CLI Source
  References contract set](../../../crystallized/documents/cli/contracts/shared/source-references/_source-references.md), [Context contract set](../../../crystallized/documents/cli/contracts/context/_context.md),
  [Status contract set](../../../crystallized/documents/cli/contracts/status/_status.md), and [Index contract set](../../../crystallized/documents/cli/contracts/index/_index.md)
  as shared or directly linked CLI contracts.
- [CLI Command Contract Set Working Document](../../../crystallized/documents/cli/command-contract-set.md),
  [CLI Command Interface Contract Working Document](../../../crystallized/documents/cli/command-interface-contract.md),
  [CLI Command Behavior Contract Working Document](../../../crystallized/documents/cli/command-behavior-contract.md),
  [CLI Command Technical Design Working Document](../../../crystallized/documents/cli/command-technical-design.md),
  [Shared CLI Operation Contract](../../../crystallized/documents/cli/shared-operation-contract.md),
  and the [Nearest Shared Scope Pattern](../../../../patterns/software/source-locality/nearest-shared-scope.md).
- [CLI Contract Document Templates](../../../../templates/cli/documents/_documents.md),
  including the [entrypoint](../../../../templates/cli/documents/entrypoint.md),
  [Interface](../../../../templates/cli/documents/interface.md),
  [Behavior](../../../../templates/cli/documents/behavior.md), and optional
  [Technical Design](../../../../templates/cli/documents/technical-design.md)
  starters.

### Framework Contracts

- [Routing Model](../../../crystallized/documents/framework/routing/model.md),
  [Routing Loading And Continuity](../../../crystallized/documents/framework/routing/loading.md),
  [Route Scope And Inheritance](../../../crystallized/documents/framework/routing/scope.md),
  [Routing Paths And Identity](../../../crystallized/documents/framework/routing/paths.md),
  and [Overwrite Customization](../../../crystallized/documents/framework/routing/overwrites.md).
- [Routed Markdown Representation](../../../crystallized/documents/framework/markdown/routes.md),
  [Markdown Compatibility Boundary](../../../crystallized/documents/framework/markdown/compatibility.md),
  and [Canonical Markdown Syntax](../../../crystallized/documents/framework/markdown/syntax.md).
- [Templates](../../../crystallized/documents/framework/primitives/templates.md),
  [Core Primitive Model](../../../crystallized/documents/framework/primitives/model.md),
  [Patterns](../../../crystallized/documents/framework/primitives/patterns.md),
  [Open Forge Framework Architecture](../../../crystallized/documents/framework/architecture.md),
  and [Accepted State And Synchronization](../../../crystallized/documents/framework/truth.md).
- [Open Forge Principles](../../../crystallized/documents/principles.md) for
  user-owned meaning, one authoritative source, start-small growth, and
  deterministic assistance.

### Migration And Decision Sources

- [CLI Decision Agenda](../../../working/cli-release/decision-agenda.md) for accepted contract roles,
  locality, additive migration, grouped help, Template boundaries, and Gate 3
  deferrals.
- [CLI Contract Migration Ledger](../contract-migration-ledger.md) for
  authority, additive staging, fact-level coverage, conflict handling, and
  cutover rules.
