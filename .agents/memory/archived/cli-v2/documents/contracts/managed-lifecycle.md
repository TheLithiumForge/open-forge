---
open-forge:
  description: "Historical CLI-v2 source: One reviewable workspace configuration and lifecycle record for whole-Framework reconciliation, deliberate route exclusions, Extension ownership, exact-id dependencies, and advisory checksums"
  responsibility: Define persistent Open Forge management evidence without turning it into installation presence, runtime meaning, remote provenance, or hidden authority
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# Managed Lifecycle Contract

## Scope

The running CLI contains one complete offline Framework payload and its
Open Forge Extension catalogue. It never resolves Framework files or
Open Forge Extensions over the network. The CLI version identifies that
distributed payload; Open Forge does not create per-file version chains.

`.agents/open-forge.json` is the one committed Open Forge workspace
configuration and lifecycle record. It is ordinary reviewable JSON inside the
Open Forge content boundary. It is not routed agent context, an installation
marker, a transaction journal, a cache, or semantic authority over installed
files.

Canonical Framework presence remains recognizable from the bounded Open Forge
block in `AGENTS.md` and the exact `.agents/loader.md`. Extension content may
also be managed in a detached `.agents` tree without a complete Framework
installation.

## Guarantees

### Workspace Record

The accepted semantic shape is:

```json
{
  "schema": 1,
  "formatters": [
    {
      "extensions": [".md", ".json"],
      "command": ["prettier", "--write"]
    }
  ],
  "framework": {
    "files": {
      ".agents/loader.md": "sha256:..."
    },
    "excluded": ["patterns"]
  },
  "extensions": {
    "development-toolkit": {
      "source": "open-forge",
      "requested": true,
      "dependencies": [],
      "files": {
        ".agents/directives/example.md": "sha256:..."
      }
    }
  }
}
```

[`managed-record.ts`](../../../../../../src/cli/commands/status/managed-record.ts)
is authoritative for the exact implemented schema declarations, member and
source vocabularies, path and checksum controls, and import paths. This
contract owns their meaning and validation guarantees.

The record contains only:

- One schema identifier.
- Explicit workspace formatter configuration, when supplied.
- Canonical managed Framework paths mapped to the last checksum Open Forge
  wrote or accepted.
- Exact routed Framework identities deliberately kept removed.
- Globally unique installed Extension ids.
- One Extension source classification: `open-forge` or `external`.
- Whether each Extension was directly requested or installed only as a
  dependency.
- Exact-id dependency lists.
- Canonical Extension-managed paths mapped to their last accepted checksums.

It does not contain timestamps, absolute paths, machine identities, source
locations, transaction state, file bodies, descriptive versions, derived
counts, or redundant owner arrays.

#### Strict Validation

The record is UTF-8 JSON with one exact schema. Its encoded size is initially
limited to 16 MiB through one named production constant. Every object rejects
unknown members, and duplicate JSON member names are rejected before ordinary
object materialization can discard them. `schema` must equal the one supported
integer value; another value is invalid rather than migrated or guessed.

Canonical managed paths use `/`, begin with `.agents/`, contain no empty, `.`,
or `..` segment, and satisfy the shared portable contained-path contract.
Object keys and set-like arrays are unique. Dependencies and exclusions contain
no duplicate identity. A checksum is exactly `sha256:` followed by 64 lowercase
hexadecimal digits. Deterministic writes sort object keys and set-like values by
their canonical UTF-8 spelling.

Every nested Framework, Extension, source, formatter, and file record accepts
only the members defined by this contract. A malformed or unsupported record
is invalid as a whole. Open Forge may report safe facts around it, but it never
partially trusts ownership, rewrites the record, or silently discards unknown
data.

Framework ownership is the presence of a path in `framework.files`.
`framework.excluded` preserves explicit keep-removed decisions; it does not
own absent files. Extension ownership is implied by the Extension entry
containing a path. Framework and Extension ownership never overlap. Several
Extensions may share one path only when their proposed bytes are byte-identical;
removal scans the other Extension entries before deleting or releasing that
path.

A divergent already-managed target is a cross-manager collision, not an
eligible overwrite. Complete preflight blocks before any lifecycle write, and
neither interactive approval nor `--overwrite` may transfer it. The readable
presentation shows both managers, the target, checksums, and an escaped diff.
It may offer one explicit `EXPORT INCOMING` next action. Choosing it rebuilds
the request as an export-only plan instead of applying the blocked lifecycle
plan. That plan creates proposed bytes beside each colliding target by appending
`.incoming` to its complete filename, for example `workflow.md.incoming`, and
never replaces an existing sidecar. The sidecars are user-owned and are neither
lifecycle nor route state. Open Forge never automatically merges authored
files. The user resolves the source package or workspace ownership and reruns
the original operation.

`AGENTS.md` remains workspace-owned. Open Forge recognizes and changes only its
bounded marker block and does not record ownership of the complete file.

### Extension Package

The initial replacement Extension package has one strict inspectable shape:

```text
<package>/
  extension.json
  payload/
    .agents/
      ...
```

`extension.json` requires exactly `id`, `name`, `description`, and
`dependencies`. The first three values are non-empty strings. `dependencies`
is an array of exact Extension ids and remains present when empty. The manifest
has no version and rejects unknown fields.

Extension ids use [npm's syntax for names valid for new
packages](https://docs.npmjs.com/cli/configuring-npm/package-json/#name),
including the optional `@scope/name` form. They are lowercase, URL-safe, at most 214
characters including a scope, and follow npm's leading-character and reserved
name restrictions. Registry availability, similarity, ownership, trademark,
and publication-policy checks are not local syntax validation. This grammar is
used unchanged by manifests, dependency arrays, command operands, completion,
and workspace lifecycle keys so later scoped catalogues or marketplaces do not
require an identity migration.

Dependencies are duplicate-free, contain no self-reference, and use the same
canonical Extension-id spelling. Catalogue and installed ids remain globally
unique.

Only ordinary files below `payload/.agents/` are installation candidates. A
package may omit `payload/` only when it declares at least one dependency;
otherwise it must contribute at least one payload file. Source files outside
`payload/` are never interpreted or installed as payload.

Payloads cannot contain symlinks or claim `.agents/open-forge.json`, generated
transaction or recovery state, or user-owned `.overwrite.md` companions.
Containment, protected-target, source-review, collision, and route-rebuild
contracts still apply after the package shape is valid.

### Source Classification

An `open-forge` Extension resolves by exact id from the catalogue embedded in
the running CLI. An `external` Extension resolves by exact id from one
filesystem source directory supplied for list, inspect, add, or update.

The external source shape is deterministic. If its root contains an Extension
manifest, the directory is a direct one-package catalogue and the root manifest
wins. Otherwise, immediate child directories carrying Extension manifests form
the catalogue. Discovery never recurses and folder names never replace manifest
ids. An empty catalogue, invalid manifest, or duplicate id is invalid evidence.

The committed record never stores the external location. A later external
update receives the source directory again through `--path` or guided
selection. One selected catalogue serves the complete add or update batch. This
keeps the workspace portable and avoids publishing machine-specific paths.
Remote URLs, registries, and persistent third-party provenance remain absent
until a real distribution and trust contract earns them.

External classification also activates source-visible inspection for the
complete selected closure on every add or update. Review authority is
invocation-local and fingerprint-bound; it is not stored in
`.agents/open-forge.json`. Explicit ids bypass catalogue selection only, and
neither existing installation state nor a previous review trusts later bytes.
See the [external source review contract](source-review.md).

Extension ids are globally unique. An external package cannot shadow an
Open Forge id or another installed external id.

### Dependency Graph

Dependencies form one flat acyclic graph of exact Extension ids. Every
installed dependency is an ordinary top-level Extension entry with its own
source, dependency list, file ownership, and requested state. Dependency
metadata never becomes runtime agent context.

The initial contract has no versions, ranges, optional dependencies, peer
dependencies, source locators, or dependency-specific flags. A selected
catalogue must contain every missing id in the complete dependency closure.
An already-installed exact id satisfies the dependency without changing its
source or reconciling its files. The resolver never falls back to another
catalogue or silently switches source classification.

Every id selected directly by the invocation is requested. An id installed
only because another Extension requires it is dependency-only. Direct
selection wins when the same id appears in both roles. The committed requested
state preserves that distinction after the originating command is gone.

A missing dependency, duplicate identity, self-dependency, cycle, or dependency
whose source cannot be proven blocks the complete batch before source review or
writes. Planning shows every directly selected id and every dependency that the
operation adds, reuses, retains, or makes orphaned. Presentation may improve
without changing these semantic states.

An orphan is a dependency-only Extension with no retained dependent. Add and
update include new dependency closure in the same plan. Update and remove make
newly orphaned dependencies visible and recommend removal. Interactive review
may keep an orphan installed, which changes it to requested. A confirmed safe
default may remove it through `--yes`, but file-level ownership, changed bytes,
overwrite companions, reachability, Git, and backup rules remain unchanged.

A requested Extension remains installed when its last dependent disappears.
An Extension required by any retained dependent cannot be removed. Shared
dependency files continue to use ordinary multi-owner reconciliation.

### Inventory Filter States

`extension list --state` accepts exactly these lifecycle filters:

| State             | Predicate                                                                |
| ----------------- | ------------------------------------------------------------------------ |
| `not-installed`   | The id exists in the selected catalogue and has no installed record      |
| `installed`       | The id has a valid installed record                                      |
| `changed`         | At least one existing recorded target differs from its accepted checksum |
| `missing`         | At least one recorded target is absent                                   |
| `dependency-only` | The id is installed with `requested` set to false                        |
| `installed-only`  | The id is installed but absent from the selected catalogue               |

These are independent predicates rather than one precedence-based primary
state. One Extension may therefore match several filters. `changed` classifies
only existing divergent targets; absence is `missing`.

The vocabulary deliberately excludes `current` and `updatable`. Proving
candidate parity requires the complete update planner, while list remains a
bounded inventory operation. An invalid workspace record produces an
operation-level diagnostic and does not invent package states from untrusted
partial lifecycle data.

### Advisory Checksums

A checksum answers only whether current bytes differ from the bytes Open Forge
last wrote or accepted. It does not prove semantic equivalence, distinguish
formatting from authored edits, grant replacement or deletion authority, or
replace Git review.

```text
current bytes == proposed bytes
  -> already current

current checksum == recorded checksum
  -> no byte difference since the recorded operation

current checksum != recorded checksum
  -> changed for an unknown reason

recorded path is absent
  -> previously managed content is missing
```

Formatting, editor behavior, or intentional edits may all produce the third
state. Human messages therefore say `changed since Open Forge recorded it`,
not `user modified it` or `unsafe` without further evidence.

### Whole-Framework Management

The initial `install` target is the complete Framework embedded in the running
CLI: root integration, Core, and Memory. There is no public item or category
selection. Later installation reconciles the complete current payload minus
persisted exclusions.

This distinction is mandatory:

| State                                       | Meaning                            |
| ------------------------------------------- | ---------------------------------- |
| Recorded and present                        | Framework-managed content          |
| Recorded and absent                         | Missing previously managed content |
| Current payload target outside an exclusion | Required reconciliation candidate  |
| Current payload target inside an exclusion  | Deliberately absent content        |
| Present but unrecorded                      | User-owned or unknown content      |

A newly distributed Framework file outside an exclusion joins the next
install plan. It is not a selectable optional item. A target beneath an
excluded route stays absent even when a later CLI payload adds descendants
there.

### Missing Categories And Routes

Before a managed mutation, missing recorded Framework paths are grouped into
the smallest useful category or route decision. The interactive presenter puts
the literal decision first:

```text
Decision       Category       Missing files
RESTORE        patterns       14
KEEP REMOVED   workflows       6
```

Selecting one row toggles `RESTORE` and `KEEP REMOVED`. The interface never
depends on checkmarks, crosses, color, or unexplained symbols.

`RESTORE` recreates the current embedded selections, formats and validates the
complete result, rebuilds affected navigation, and records the new checksums.

`KEEP REMOVED` preserves absence, records the narrowest exact routed identity
that represents the decision, and removes every affected Framework file
record. Any still-present managed remnants in that route remain untouched,
lose Framework management, and become user-owned. The confirmation reports
both missing files and existing files whose management will be released.

An excluded entrypoint identity covers that Framework route and every
Framework-supplied descendant. An excluded ordinary routed file covers only
that exact identity. Required parent entrypoints remain available as structural
support when retained routes or Extensions still need them. Later ordinary
installation does not ask again or add new payload files inside the exclusion.

Interactive installation lists persisted exclusions as literal `KEEP REMOVED`
or `RESTORE` toggles and defaults each one to `KEEP REMOVED`.
`install --restore <route...>` is the deterministic equivalent. Every supplied
value must exactly match an identity stored in `framework.excluded`; an unknown
route, a non-excluded route, or a descendant covered only by a broader stored
entrypoint exclusion is invalid. Selecting an entrypoint exclusion restores its
current Framework-supplied subtree. Selecting an ordinary routed-file exclusion
restores that exact file and any structural route chain required to expose it.
The selected exclusions leave final state only after the complete plan applies
and verifies successfully. Restoration neither authorizes replacement nor
weakens source, Git, formatting, containment, or recovery policy.

If installed Extensions still own descendants under a removed Framework
category, the plan preserves or creates only the structural route chain needed
to keep those Extension files reachable. It does not reinstall the complete
Framework category and states that the category is not fully absent while the
Extension remains.

### Reconciliation Decisions

One complete plan classifies every added, current, replaced, missing, dropped,
retired, shared, conflicting, and released path before the first write.

For content no longer supplied by its manager, the guided presenter uses the
same literal decision shape:

```text
Decision       File
DELETE         .agents/guidance/retired.md
KEEP           .agents/workflows/customized.md
```

Selecting one row toggles `DELETE` and `KEEP`.

- `DELETE` removes the file and its lifecycle entry.
- `KEEP` preserves the file, removes the departing manager's lifecycle entry,
  and makes the file user-owned when no other manager remains.
- A shared file loses only the departing Extension entry while another
  compatible owner remains.
- A base with a user-owned `.overwrite.md` companion is never deleted
  automatically because that would orphan user intent.
- A structural entrypoint remains when retained routed descendants need it.

Recorded checksum equality may support a recommendation; it is not hidden
permission. Unchanged dropped files may initially recommend `DELETE`, while
changed or unknown files recommend `KEEP`. The literal decision remains
visible and editable. `--yes` accepts only documented unambiguous defaults; it
does not turn an unresolved changed-file or ownership choice into deletion.

### Operation Semantics

#### Install

First installation targets the complete embedded Framework through the same
wizard and `--yes` default. Later installation reconciles the complete current
payload minus retained persisted exclusions. Exact exclusions named by
`--restore` re-enter the desired payload for that invocation.

- An absent unoccupied payload path may be created.
- Already-current bytes are a no-op.
- A recorded current path may be replaced after the complete plan is shown.
- An unrecorded occupied destination is an ownership collision and is never
  claimed silently.
- Missing recorded paths use the `RESTORE` or `KEEP REMOVED` decision.
- Retired recorded paths use the `DELETE` or `KEEP` decision.
- Newly distributed files outside exclusions join the same plan.
- Every `--restore` value must be an exact persisted exclusion and grants no
  overwrite or confirmation authority.

#### Extension Add

Add installs one or more exact absent Extension ids from the selected embedded
or external catalogue together with every missing dependency in their complete
closure. It creates absent targets, may reuse an already-installed exact
dependency, may share a byte-identical Extension-managed path, and blocks or
requests explicit collision authority for an occupied unowned target. A
Framework-managed target or divergent Extension-managed target is an
ineligible cross-manager collision. Add never becomes update when an id already
exists, and one unavailable selected id or unresolved external source finding
blocks the complete batch before writes.

#### Extension Update

Update requires every selected id to be installed and available in the one
selected embedded or external catalogue. The selected source directory serves
the complete batch; ids are independent space-separated subjects and never pair
with sources by positional ordering. Update adds new payload paths, replaces
selected current paths, preserves byte-identical shared Extension ownership,
removes one owner from shared paths, and presents dropped paths as `DELETE` or
`KEEP`. A selected update that would make shared owners diverge is an
ineligible cross-manager collision. Update never becomes add or silently
changes source classification. External bytes are inspected again; trust is
never carried from an earlier install. New dependencies join the same update
plan. Dropped dependency relationships make dependency-only Extensions with no
retained dependent visible as orphaned removal candidates.

#### Extension Remove

Remove begins from explicitly selected installed ids. It refuses to break a
retained dependent and includes newly orphaned dependency-only Extensions in
the reviewed plan with a remove-or-keep-installed decision. Keeping an orphan
makes it requested. Removal removes one owner from shared files and presents
exclusive files for deletion or ownership release. `--yes` may confirm the
documented safe removal plan, but changed, unknown, overwrite-backed, or
reachability-sensitive targets remain explicit blockers or decisions.

### Persistence And Failure

The workspace record is generated deterministically and written atomically as
the final primary effect after payload effects, route rebuild, and semantic
verification. Optional formatter post-processing runs afterward. When it
succeeds and the formatted files validate, a separate atomic finalization
refreshes only their advisory checksums from actual bytes. It never changes
ownership, dependencies, source classification, or requested state.

A formatter failure leaves the primary record intact, so changed bytes remain
visible rather than being accepted silently. The record itself is deterministic
canonical JSON and is not sent through an in-place formatter.

Handled failure restores applied state through the shared mutation contract.
Hard-stop and Gitless behavior follows the [recovery contract](workspace-recovery.md).
A missing or malformed record never causes the CLI to infer ownership from
current bytes. `doctor` reports invalid or conflicting lifecycle evidence;
repair does not invent historical ownership.

## Boundaries

The workspace record is reviewable lifecycle evidence and configuration. It is
not installation presence, runtime meaning, remote provenance, transaction
state, or authority to replace divergent content. Checksums remain advisory,
external source locations remain invocation input, and exact Extension ids do
not imply a network, marketplace, or version-range contract.

## Compatibility And Evolution

The workspace record initially accepts one exact schema. Unknown schemas,
unknown fields, mixed metadata sources, and unsupported value forms remain
invalid rather than being discarded or reinterpreted. A future record shape
must define an explicit schema and migration boundary. The CLI version and
per-file checksums do not substitute for that persisted-data contract.

Extension dependencies remain exact ids from one selected catalogue. Versions,
ranges, optional dependencies, peer dependencies, remote provenance, and
marketplace resolution require a separately accepted lifecycle contract before
they may change persisted meaning.

## Verification

- One `.agents/open-forge.json` contains configuration and lifecycle evidence.
- Framework presence never depends on that file.
- Canonical paths are the lifecycle identities; checksums remain advisory.
- Direct installation targets the complete embedded Framework rather than a
  public item selection.
- Exact routed exclusions preserve deliberate removal and bound future payload
  additions without becoming ownership evidence.
- Interactive restoration and `--restore <route...>` change the same exact
  exclusion decisions; invalid or broader inferred identities are rejected.
- External source locations never enter committed state.
- Flat Extension entries make ownership directly navigable from one id.
- Requested state distinguishes direct selection from dependency-only
  installation without nesting package records.
- Dependency resolution uses exact ids from one selected catalogue, reuses
  installed ids, and never performs hidden source switching.
- Orphaned dependency removal remains visible and cannot weaken file-level
  preservation rules.
- Missing managed content is distinguished from deliberately excluded content.
- Deliberately removed content stays absent after its exclusion is recorded.
- Every delete or ownership release is visible before mutation.
- Format, route, payload, and state changes belong to one complete plan.
- The workspace state file is the final persistent effect.

## Related Current Sources

- [CLI interface](../interface.md)
- [Mutation execution](mutation-execution.md)
- [Workspace recovery](workspace-recovery.md)
- [Source review](source-review.md)
- [Route inventory](route-inventory.md)
