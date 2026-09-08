---
open-forge:
  description: Define the exact public syntax, source boundary, collision policy, projections, record, and results for `library attach`
  responsibility: Define what attach accepts, creates, preserves, rejects, and reports for one Workspace Library
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Library, Attach, Interface, Mutation, Recovery, Safety, CurrentTruth]
---

# library attach Interface Contract

## Status And Authority

This is the current Crystallized Interface Contract for
`open-forge library attach`. It owns the public purpose, exact syntax, operand
grammar, source-root and consumer boundaries, projection and record effects,
statuses, output, errors, examples, non-goals, and public verification.

The sibling [Behavior Contract](behavior.md) defines deterministic
technology-neutral resolution, complete inventory, planning, preflight,
application, verification, and recovery. The shared [Global CLI Flags](../../shared/global-flags/interface.md),
[Shared Result Coordinates](../../shared/result-coordinates/interface.md), and
[Shared CLI Operation Contract](../../../shared-operation-contract.md) retain
their shared meanings.

The [Workspace Libraries Technical Design](../../../technical-designs/workspace-libraries.md)
and [Mutation And Recovery Technical Design](../../../technical-designs/mutation-and-recovery.md)
define accepted shared realization boundaries. The [Index Behavior Contract](../../index-candidate/behavior.md)
defines existing generated-navigation projection. This Interface Contract adds
no callable or implementation choice. Implementation and executable proof
remain pending the Task 23 contract freeze and later acceptance gates.

## Purpose And Operation Boundary

`library attach` registers one new consumer-local library ID and projects the
complete current eligible ordinary-file inventory below one contained source
root's `.agents/` directory into the selected consumer workspace. Each source
path is projected at the identical consumer-relative `.agents/...` path as a
real relative file symlink. The source remains at its source-root path and is
never copied, moved, deleted, or written through by this operation.

Attach is one complete mutation for one library. It establishes the source
root, inventories every eligible file, detects every destination collision,
forms any permitted generated-region changes, and publishes the consumer
record as one plan. It never applies a safe subset after a collision,
incomplete source inventory, unsafe boundary, or other preflight blocker.

The managed library identity is separate from automatic source identity. A
projected file keeps the normal identity of its consumer destination when a
read-only command later observes it. The library ID is never a source-reference
operand and never creates a second route identity.

## Syntax

```text
open-forge library attach <library-id> <source-root> [--dry-run] [global flags]
```

The command path selects the attach operation. The shared [Global CLI Flags](../../shared/global-flags/interface.md)
contract defines `--workspace`, `--json`, `--view`, `--verbose`, `--help`, and
`--version`; all six retain their shared spelling, composition, repetition,
terminal behavior, and output meaning.

`--dry-run` is the only preview spelling. `--help` and `--version` are terminal
forms and stop before workspace, source-root, record, or projection work.

Attach has no aliases, extra operands, `--force`, `--automatic`, `--yes`,
`--apply`, collection selector, remapping flag, glob, copy mode, saved plan,
or generic mutation dispatcher.

## Operands And Repetition

| Operand or flag | Role | Accepted value | Omission and repetition |
| --- | --- | --- | --- |
| `<library-id>` | Select the new management identity | One value matching the library-ID grammar below | Required and singleton. A repeated positional value is invalid. An already registered ID is blocked, not last-wins. |
| `<source-root>` | Select the source directory relative to the selected workspace | One portable workspace-relative path satisfying the source-root boundary below | Required and singleton. A repeated positional value is invalid. |
| `--dry-run` | Write policy | Boolean flag with no value | Application is selected when omitted. Repetition is accepted and idempotent. |
| Shared global flags | Workspace and presentation | Defined by the shared global contract | Shared defaults and repetition rules apply. |

No operation-specific flag changes source selection, ownership, collision,
containment, record, recovery, or route authority. A dry run does not grant
application authority.

## Library-ID Grammar And Identity

`<library-id>` must contain 1–128 characters and match exactly:

```text
[a-z0-9]+(-[a-z0-9]+)*
```

The grammar uses lowercase ASCII letters and digits in non-empty segments
separated by one hyphen. Uppercase, empty segments, leading or trailing
hyphens, repeated hyphens, whitespace, slashes, backslashes, and other
characters are invalid. The ID is caller-supplied, is not derived from
`<source-root>`, and is unique within the consumer library-record namespace.

Automatic source IDs remain a separate namespace. A projected eligible
`.agents/...` destination retains its ordinary destination-derived automatic
source ID. The library ID is never passed to a source-reference operation,
treated as a route identity, or confused with the Extension `--source` value.

## Source-Root Boundary

`<source-root>` is one normalized portable workspace-relative path. It must be
strictly contained by the selected workspace both lexically and physically.
An empty, absolute, backslash-separated, dot-traversing, escaping, or otherwise
non-portable spelling is `invalid`; the command does not normalize it into an
accepted path. An alias for a path outside the workspace, a physical escape, or
an ambiguous identity is `blocked`.

The resolved source root must be a real ordinary directory. Its `.agents`
child must also be a real ordinary directory. Every ancestor from the selected
workspace to the source root and to that `.agents` child must be free of links
and reparse points. The source tree must be physically disjoint from the
consumer workspace's `.agents` tree. Lexically different paths that overlap or
alias either tree are unsafe and block the operation.

The selected consumer workspace and its `.agents` tree remain consumer-owned.
The consumer `.agents` root must already be a real ordinary directory without
link or reparse ancestry; Attach never creates or replaces it. Attach does not
create a workspace root, replace a consumer Loader, or use an external
destination. A missing or non-ordinary mandatory source root, or a missing or
non-ordinary mandatory source `.agents` child, is `invalid`. An existing source
boundary or required source fact that is unavailable or inaccessible is
`incomplete`. An unsafe, aliased, or ambiguous source identity is `blocked`.
None of these conditions begins an effect.

## Complete Eligible Inventory

Attach forms one complete narrow inventory below the source root's ordinary
`.agents/` directory. Eligible entries are ordinary regular files whose
source-relative paths are portable `.agents/...` paths and are not one of the
accepted exclusions. The inventory is complete only when every relevant
directory can be safely enumerated and every excluded link or special entry is
identified without traversal.

The inventory excludes:

- the Loader;
- recognized entrypoints;
- adjacent overwrite companions;
- lifecycle, library, and other manager-control files;
- symlinks, junctions, reparse points, and other links; and
- special, non-ordinary, or otherwise unsafe filesystem entries.

The command never traverses a link or reparse point. A readable excluded link
is not an eligible file; it is not followed to discover descendants. An
inaccessible directory or subtree, an enumeration failure, or another fact
that prevents a complete safe inventory produces `incomplete` and no effect.

An eligible ordinary file may be any supported ordinary file type. The command
does not use a file's content to grant destination authority, and it does not
store or mutate source bytes. A source file that is itself a route entrypoint,
overwrite companion, lifecycle/library record, or manager control remains at
its source path and is not projected.

## Destination Mapping And Projection

For each eligible source-relative path, attach forms one mapping whose source
and destination path strings are identical `.agents/...` paths. The destination
is the same path below the selected consumer workspace. Its link target is the
relative path from the destination's parent to the source file. The expected
relative target is derived when needed and is not stored in the consumer
record.

The projection is a real relative file symlink. Its parent directories may be
created only as real ordinary directories required by declared mappings. A
local sibling file may remain beside a projected file. Attach has no copy
fallback and never makes a regular file stand in for a link.

The destination leaf must be missing before a new link is created. An existing
ordinary file, directory, symlink, junction, special entry, unsafe path,
unknown state, or even an unregistered link with the expected target is a
collision. Attach does not overwrite, adopt, rename, or release any occupant.
It also blocks against Extension-owned paths, lifecycle records, library
records, and other consumer manager controls. Two libraries mapping to one
destination have no implicit winner.

Attach may update an existing consumer-owned generated `Entries` region only
when the ordinary route chain already exists and the [Index Behavior Contract](../../index-candidate/behavior.md)
permits the bounded projection. It preserves authored content outside that
region. It does not create an entrypoint, materialize a source entrypoint,
invent a route, create a missing route chain, or rewrite the Loader. When no
pre-existing route chain exposes a projected file, the link remains an ordinary
consumer path without a fabricated route.

## Consumer Record

The separate consumer-owned record is exactly:

```text
.agents/open-forge.libraries.json
```

It is ordinary workspace content and is not part of
`.agents/open-forge.lifecycle.json`. A valid record has exactly the following
schema-v1 shape and no additional properties:

```json
{
  "schemaVersion": 1,
  "libraries": [
    {
      "id": "team-knowledge",
      "sourceRoot": "shared/team-knowledge",
      "paths": [
        ".agents/directives/review.md"
      ]
    }
  ]
}
```

`libraries` is an array of records with exactly `id`, `sourceRoot`, and
`paths`. `paths` is the sorted array of complete eligible source-relative
`.agents/...` path strings. Each path is also its identical destination path;
there is no separate source-path or destination-path field. Library records
are sorted by ID, and each `paths` array is sorted by portable path spelling.
Duplicate IDs or paths are blocked as ambiguous or colliding record identity.
An empty `libraries` array and an empty `paths` array are valid where the
operation establishes them.

The record contains no expected link target, absolute path, timestamp, Git
revision, source metadata, permission, dependency, glob, remapping, exclusion,
collection, or source bytes. The relative link target is derived from the
record's normalized `sourceRoot` and the path at operation time. The record is
the only consumer-side ownership and authorization evidence for a library;
source content cannot grant itself a destination.

If the record is absent, attach may create it as the final record effect. If a
record exists, it must be a complete valid schema-v1 record before attach can
plan. A malformed existing record is `invalid`; if the existing record cannot
be read, its state is `incomplete`. Attach never migrates, repairs, or silently
rewrites malformed record data. An existing library ID is a blocking duplicate
even when its source root or paths appear equal.

## Dry Run And Application

`--dry-run` forms the same typed request, source facts, complete inventory,
mapping set, destination collision checks, generated-region projection,
record bytes, ordered plan, expected-state facts, and effect-free preflight as
application. It reports every projected path, generated-region change, record
change, and blocker, then writes nothing. It creates no directories, links,
record, generated navigation, recovery artifact, or lock. It performs no lease
or recovery capability probe.

Omitting `--dry-run` selects application. Application completes preflight and
all collision checks before acquiring one workspace lease for the effectful
plan. Under that lease it revalidates the workspace, record, source/destination
set, and expected states. Each link or record effect has an immediate
no-follow final-component check immediately before its effect. For an effectful
application, typed recovery is prepared and verified before the first effect;
an effect-free plan has no recovery bundle. Effects are monotonic:
the operation never rolls back or compensates for a verified effect.

The plan may create declared real parent directories, create exact relative
file symlinks, update permitted existing generated regions, and publish the
consumer record last. It never writes source bytes. If an application or
verification failure occurs after an effect, already verified effects remain
true effects, the record is not published early, and the result reports exact
residual state and retained recovery evidence. A later invocation plans from
current facts; it does not replay a saved plan or claim an automatic rollback.

Library recovery distinguishes a prior-missing ordinary consumer-record
`Create` from relative-file-link `Create` and `Delete` entries. It retains
consumer record bytes and exact relative-link identity only. It never stores,
opens, follows, restores, or deletes source bytes. Strong no-follow recovery
can remove an exact created link or recreate an exact deleted link when its
recorded relative target is still the same, including a dangling target. The
shared [Mutation And Recovery Technical Design](../../../technical-designs/mutation-and-recovery.md)
defines the external preparation, verification, and residual boundary.

## Human Output

Human output comes from one typed result. Expanded output includes the selected
workspace, library ID, normalized source root, complete eligible path set,
excluded and incomplete inventory facts, exact destination mappings, collision
facts, generated-region effects, record effect, dry-run or application mode,
verification, recovery disposition, residual paths, and semantic status.

Compact output retains the library ID, source root, mode, status, completeness,
safety, every affected path or blocker, and at most one required `Next:` action.
The primary human result for `complete`, `attention`, and `incomplete` goes to
stdout. The primary human result for `invalid`, `blocked`, `failed`, and
`interrupted` goes to stderr. Bounded diagnostics use stderr.

### Dry-run example

```text
The library would be attached.
Library: team-knowledge
Source root: shared/team-knowledge
Projected paths: 1
Record: create

No files changed (--dry-run).
```

### Applied example

```text
The library was attached.
Library: team-knowledge
Source root: shared/team-knowledge
Projected paths: 1
Generated regions updated: 1
Record: published
```

### Collision example

```text
The library was not attached.
Library: team-knowledge
Blocked: .agents/directives/review.md has an existing consumer occupant.
Next: move or remove the occupant, then run attach again.
```

The paths in these examples are repository-relative illustrative values. A
complete result has no required `Next:` action.

## Structured Output

`--json` emits one complete structured result to stdout for every semantic
status from the same typed result used by human output. It never prompts and
never reruns resolution, inventory, planning, application, verification, or
residual reporting. Human text is not mixed into JSON stdout; bounded
diagnostics use stderr.

The result exposes the concrete attach facts under the exact shared result
envelope, including:

- selected workspace and library identity;
- normalized source root and its lexical and physical boundary facts;
- complete eligible inventory and excluded or incomplete coverage facts;
- identical source/destination `.agents/...` paths and derived-link identity;
- every collision, Extension or manager ownership fact, and generated-region
  effect;
- the exact schema-v1 record effect and sorted resulting record projection;
- dry-run or application mode, expected-state, verification, recovery, and
  residual facts without source bytes; and
- semantic status and at most one required `Next:` action.

The result does not add an expected-link field to the persisted record and does
not disclose or materialize source content.

## Semantic Results

| Result | Meaning |
| --- | --- |
| `complete` | A complete safe dry-run plan was established, or application and final verification completed. An empty eligible source inventory is complete when its record and any permitted generated projection are verified. |
| `attention` | Target effects verified, but post-verification recovery cleanup has a positively observed retained residual under the shared recovery boundary. Planned changes alone do not create `attention`. |
| `incomplete` | A valid request has an unavailable or inaccessible existing record or source fact, incomplete source inventory, incomplete generated projection, or unavailable required application recovery preparation. No effect begins. |
| `invalid` | Command input, operand cardinality, library-ID grammar, source-root spelling, a malformed strict record, a missing or non-ordinary mandatory source root or `.agents` child, or terminal-mode use is outside this interface. |
| `blocked` | The request is syntactically valid but duplicate or ambiguous identity, unsafe containment or physical aliasing, an unsafe or colliding record or destination, an unsafe generated region, unavailable real-link capability, or another mutation precondition prevents a safe complete attach. No effect begins. |
| `failed` | An unexpected application, verification, or unknown recovery-disposition failure occurs after a persistent effect begins. |
| `interrupted` | The caller cancels before completion. Effects already verified remain residual truth; an unexpected post-effect failure remains `failed`. |

For ordinary conditions, status precedence is `blocked` > `incomplete` >
`attention` > `complete`. Invalid input stops before operation resolution. The
shared [Result Coordinates](../../shared/result-coordinates/interface.md)
define numeric exits, stream coordinates, and the structured envelope.

## Errors And Boundaries

Attach reports `invalid` for zero or several positional operands, an ID outside
the exact grammar, a repeated positional operand, a malformed or extra-field
strict record, an invalid source-root spelling, a missing or non-ordinary
mandatory source root or `.agents` child, or invalid terminal-mode use.

Attach reports `incomplete` for an existing record or required source fact that
is unavailable or inaccessible, an incomplete source enumeration, an
inaccessible subtree, or unavailable required application recovery preparation.

Attach reports `blocked` for an existing library ID, duplicate or unsafe record
identity, an aliased, ambiguous, or externally resolving source boundary,
physical overlap with consumer `.agents`, a missing or non-ordinary consumer
`.agents` root or linked/reparse consumer ancestry, an existing or changed
destination occupant, an unsafe or colliding destination, an unsafe generated
region, unavailable real-link capability, or another unsafe mutation
precondition. Excluded source controls, links, special entries, and other
non-ordinary entries remain excluded from the inventory and are never projected.

Every error names `library attach`, the library ID or source root, the affected
path or boundary, the cause, and one useful next action when one is known.
Attach does not inspect Git, perform Git operations or diagnostics, mutate
remote or public state, create a root route, rewrite the Loader, or change any
source file.

## Scenarios

Attach one contained source root and preview the complete projection:

```text
open-forge library attach team-knowledge shared/team-knowledge --dry-run
```

Apply the same request with structured output:

```text
open-forge library attach team-knowledge shared/team-knowledge --json
```

Use `--workspace` from the shared global contract when the selected consumer
workspace is not the current workspace. The source-root value remains relative
to that selected workspace.

## Non-Goals

`library attach` does not:

- list or inspect libraries, sync an existing record, or detach any library;
- select a collection, subset, glob, remapping, dependency, or external
  destination;
- traverse, copy, write, move, delete, or follow source bytes;
- overwrite, adopt, rename, or release a consumer occupant or lifecycle claim;
- create route parents, materialize entrypoints, rewrite authored entrypoints,
  rewrite the Loader, or invent Framework roots;
- change the `.agents/open-forge.lifecycle.json` document or add library data to
  that lifecycle schema;
- perform Git fetch, pull, checkout, switch, stage, commit, or diagnostics;
- write through a projected file or make Route Update, Index, Route Move, or
  Route Remove follow or delete a library source target; their link-aware guard
  and real-filesystem regression are prerequisites to Attach dogfood;
- add compatibility, migration, remote publication, deployment, JavaScript,
  dependency injection, a runtime registry, a generic mutation dispatcher, or
  another callable design; or
- create a receipt, saved plan, journal, or automatic rollback history.

## EndToEnd Journeys

1. **Dry-run parity and no effect.** Run
   `open-forge library attach team-knowledge shared/team-knowledge --dry-run`.
   The result contains the same complete inventory, mappings, collisions,
   generated-region projection, and record plan that application would use,
   while no directory, link, record, generated region, lease, recovery
   artifact, or source byte changes.
2. **Successful attach preserves the source.** With an eligible source file at
   `shared/team-knowledge/.agents/directives/review.md` and an unoccupied
   consumer destination, apply
   `open-forge library attach team-knowledge shared/team-knowledge`. The
   consumer receives the exact relative file symlink and schema-v1 record path,
   any permitted existing generated region is updated, and the source file
   bytes remain unchanged.
3. **Invalid source or collision blocks all effects.** Run attach with a source
   root that is missing or lacks a real ordinary `.agents` directory, or with a
   destination already occupied. The request is `invalid` for the missing or
   non-ordinary mandatory source boundary and `blocked` for the collision, and
   it creates no link, directory, record, generated navigation, recovery
   artifact, or source change.

## Verification Requirements

Lower-tier and public proof must cover the following without adding another
public EndToEnd journey:

- exact command and flag parsing, singleton operands, shared global flags,
  terminal modes, JSON parity, stream assignment, and all seven statuses;
- library-ID length, ASCII grammar, duplicate IDs, namespace separation from
  source IDs, and malformed or extra-field record rejection;
- portable source-root spelling, strict lexical and physical containment,
  ordinary source and `.agents` directories, link/reparse ancestry, aliases,
  overlap with consumer `.agents`, and source-root availability;
- complete inventory coverage, excluded entrypoints and controls, ordinary
  file types, links and special entries, inaccessible subtrees, and source-byte
  preservation;
- identical source/destination path formation, relative raw-target identity,
  real parent directories, no copy fallback, local siblings, every collision,
  Extension/lifecycle ownership, and two-library destination collisions;
- existing consumer entrypoints and generated-region preservation, no route
  creation, no Loader rewrite, and pre-dogfood link-aware guards for Route
  Update, Index, Route Move, and Route Remove;
- schema-v1 exact serialization, sorted IDs and paths, record creation and
  publication last, malformed-record preservation, and derived rather than
  stored expected targets;
- complete plan/preflight parity, no dry-run lease or recovery probe, one
  workspace lease for application, under-lock revalidation, immediate
  no-follow checks, typed recovery, monotonic effects, verification, residual
  truth, and no automatic rollback;
- typed recovery for prior-missing ordinary record creation and relative-link
  creation/deletion, exact created-link removal, exact deleted or dangling-link
  recreation, and no source-byte storage or following; and
- invalid, incomplete, blocked, failed, interrupted, complete, and retained
  recovery `attention` results with useful next actions.

## Related Current Sources

- [library attach Contract Set](_attach.md)
- [library attach Behavior Contract](behavior.md)
