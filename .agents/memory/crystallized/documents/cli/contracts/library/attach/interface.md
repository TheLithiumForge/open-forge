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
no callable or implementation choice. The active Task records implementation and executable evidence.

## Purpose And Operation Boundary

`library attach` registers one new consumer-local library ID and projects the
complete current eligible ordinary-file inventory below one contained source
root into the selected consumer workspace. Each source
path preserves its suffix below the chosen destination root as a
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
open-forge library attach <library-id> <source-root> [--to <workspace-relative-directory>] [--dry-run] [global flags]
```

The command path selects the attach operation. The shared [Global CLI Flags](../../shared/global-flags/interface.md)
contract defines `--workspace`, `--json`, `--view`, `--verbose`, `--help`, and
`--version`; all six retain their shared spelling, composition, repetition,
terminal behavior, and output meaning.

`--dry-run` is the only preview spelling. `--help` and `--version` are terminal
forms and stop before workspace, source-root, record, or projection work.

Attach has no aliases, extra operands, `--force`, `--automatic`, `--yes`,
`--apply`, collection selector, per-file remapping flag, glob, copy mode, saved plan,
or generic mutation dispatcher.

## Operands And Repetition

| Operand or flag                       | Role                                                           | Accepted value                                                                 | Omission and repetition                                                                                                         |
| ------------------------------------- | -------------------------------------------------------------- | ------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------- |
| `<library-id>`                        | Select the new management identity                             | One value matching the library-ID grammar below                                | Required and singleton. A repeated positional value is invalid. An already registered ID is blocked, not last-wins.             |
| `<source-root>`                       | Select the source directory relative to the selected workspace | One portable workspace-relative path satisfying the source-root boundary below | Required and singleton. A repeated positional value is invalid.                                                                 |
| `--to <workspace-relative-directory>` | Destination root                                               | `.` or a canonical portable child directory                                    | Defaults to `.`. Singleton; repetition is invalid. Native spaced, equals and colon option-value forms follow the pinned parser. |
| `--dry-run`                           | Write policy                                                   | Boolean flag with no value                                                     | Application is selected when omitted. Repetition is accepted and idempotent.                                                    |
| Shared global flags                   | Workspace and presentation                                     | Defined by the shared global contract                                          | Shared defaults and repetition rules apply.                                                                                     |

`--to` selects the recorded destination root. No flag changes source selection, ownership, collision,
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

The source root is a non-empty canonical portable workspace-relative directory,
strictly contained by the selected workspace lexically and physically. It has
no absolute, empty, backslash, `.` or `..` segment and no portable alias. Every
ancestor and the selected root must be a real ordinary directory, without
symlink, junction or reparse ancestry. No specially named child is required.
The selected directory itself scopes the recursively discovered eligible files.

An absent or non-directory source root is `invalid` for Attach. For an existing
registration, unavailable or missing source facts make Inspect or Sync
`incomplete`; a readable non-directory root is `invalid`. Unsafe containment,
linked ancestry or ambiguous identity is `blocked`. List reports only bounded
root availability and does not enumerate descendants. An incomplete source is
never an empty source inventory.

The consumer workspace and its existing ordinary `.agents` control directory
remain consumer-owned. The operation does not create or replace either root.
The destination root may be an ancestor of a contained source root, including
`.`. Actual destination leaves and every mutation target must remain outside
all selected and registered source trees. This per-leaf check preserves source
contents without forbidding workspace-root projection.

## Complete Eligible Inventory

Recursively enumerate the selected real source root, recording each eligible
ordinary file by its canonical portable path relative to that root. A source
containing no `.agents` or `content` child is valid. Empty eligible inventory is
complete when the entire selected tree was safely observed.

Exclude Git metadata at any path segment, known manager controls, symlinks,
junctions, reparse points and special entries. Recognize Open Forge Loader,
entrypoint and overwrite controls at their original `.agents` source coordinates
before remapping. A remap cannot make those controls eligible. External
`_name.md`, `*.overwrite.md` and README remain ordinary opaque Library content. Inspect excluded entries without
following them and never descend into excluded metadata or linked directories.
Existing source classification and protected-control rules remain applicable.

An inaccessible directory, enumeration failure, unavailable eligible ordinary
file or unsafe required boundary prevents complete inventory. Retain known safe
facts as partial evidence, never as permission to delete retired links. Source
bytes are never copied, rewritten or deleted. Only eligible leaf membership and
physical path facts feed projection planning.

## Destination Mapping And Projection

Each record keeps `sourceRoot`, `destinationRoot` and source-relative `paths`.
For a path `p`, its source is `sourceRoot/p`. Its consumer destination is `p`
when `destinationRoot` is `.`, otherwise `destinationRoot/p`. Derive the exact
raw relative file-link target from the destination parent to that source.
Root-level leaf destinations use the workspace root as their parent.

Projection creates individual relative file symlinks. Required missing parents
are separate real ordinary directory effects, including first-level parents.
Existing parents must be real ordinary directories with no linked or reparse
ancestry. Local sibling files remain untouched; no directory symlink, copied
file fallback or directory ownership is introduced.

Validate source eligibility and final destination protection separately. Protect
Git metadata, Framework and recognized manager controls, `.agents` Loader,
entrypoint and overwrite controls, lifecycle/Library/permission/lock controls, recovery and temporary
storage, and every selected or registered Library source tree. A grant covering
a containing directory never overrides these leaf checks. Compare portable
identity and physical containment. Different source-relative paths and different
Libraries may share ordinary directories but never the same destination leaf.
An unregistered link, including an exact-looking link, is an existing occupant
and is never adopted.

Only mapped `.agents/**` leaves may participate in an existing consumer route
chain and its bounded generated `Entries` projection under the Index contract.
The region and route chain must already exist and authored bytes remain intact.
External Markdown remains opaque content. No source entrypoint, Loader, missing
route or generated region is created.

## Consumer Record

The consumer record is `.agents/open-forge.libraries.json`, separate from
lifecycle ownership and consumer permissions. Its exact current schema is:

```json
{
  "schemaVersion": 1,
  "libraries": [
    {
      "id": "team-knowledge",
      "sourceRoot": "shared/team-knowledge",
      "destinationRoot": ".apm/agents/team",
      "paths": ["checks/security.md", "review.md"]
    }
  ]
}
```

Require exactly `schemaVersion` and `libraries` at the top level, and exactly
`id`, `sourceRoot`, `destinationRoot` and `paths` per Library. Require integer
`1`, existing Library-ID grammar, canonical portable roots and source-relative
eligible paths. The destination root is `.` or a normal relative directory;
source roots do not admit `.`. Unknown, missing, null, duplicate and wrongly
typed members are malformed. No previous schema shape, migration or alternate
reader is accepted.

IDs and each source-relative path array use ordinal order. Paths are unique
within a Library. Derived destinations must be unique across Libraries under
portable identity; equal source-relative paths at different destinations are
valid. Empty path arrays and an empty Library array are valid. The record stores
no expected-link text, contents, hashes, timestamps, Git facts, dependencies,
globs or per-file remapping. Link identity derives from both recorded roots and
the source-relative path. Permission is separate from ownership and may be
revoked independently.

A missing record is a valid prior-absence fact for Attach and a complete empty
List result. Inspect, Sync and Detach require the requested ID in a valid record.
Malformed, unavailable and unsafe records retain their existing invalid,
incomplete and blocked classification; none becomes an empty valid record.

## Consumer Permission

This command selects [Workspace Permissions](../../shared/workspace-permissions/interface.md)
for the complete eligible mapped inventory. `.agents/**` leaves remain implicit.
Requirements bind the selected Library ID and source root. Permission remains
necessary even for existing owned links; recorded identity makes removal
source-independent, without exempting it from revocation.

Live uncovered leaves propose their immediate parent folder; root leaves use exact grants.
Directory proposals explicitly include future descendants and never cover the
workspace root. A conflicting saved source binding requires disclosed old/new
source replacement approval under the shared contract. Protected paths,
source trees, ancestry, ownership and collision checks still apply per leaf.

Only human prompt-capable application can approve the displayed scopes. JSON,
redirected execution and dry-run never prompt; missing or declined approval is
`blocked` and cancellation is `interrupted`, without effects. Malformed or unsafe
permission observations are `blocked`; unavailable observations are `incomplete`.

`result.permissions` appears after `plan` and before `application`. It uses the
shared Library leaf, scope, rebinding and receipt coordinates exactly. Required
and missing arrays are concrete destinations; proposed/approved scopes expose
remembered authority. A proposed rebind is not an applied one. Only a verified
outcome says permission was saved; later content failure retains that outcome.

Permission findings use the `library-attach.` prefix and suffixes
`permission-required`, `permission-declined`, `permission-invalid`,
`permission-unavailable`, `permission-changed` and `permission-write-failed`.
Changed lease-bound permission facts block; failed permission publication is
`failed` with its actual receipt; cancellation uses the existing `interrupted`
finding. No content effect proceeds after an unverified permission write.
Malformed `--to` uses `library-attach.destination-root-invalid` and `invalid`.

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
workspace, library ID, normalized source and destination roots, complete eligible path set,
excluded and incomplete inventory facts, exact destination mappings, collision
facts, generated-region effects, record effect, dry-run or application mode,
verification, recovery disposition, residual paths, and semantic status.

Compact output retains the library ID, source and destination roots, mode, status, completeness,
safety, every affected path or blocker, and at most one required `Next:` action.
The primary human result for `complete`, `attention`, and `incomplete` goes to
stdout. The primary human result for `invalid`, `blocked`, `failed`, and
`interrupted` goes to stderr. Bounded diagnostics use stderr.

Both views lead with the operation outcome or preview, status, exact workspace
and selection method, then Library identity and the recorded roots. Human
`requires attention` represents the typed `attention` status. Findings and
blockers remain prominent before a plan can be mistaken for completed work.

The observed record and links are labelled as before-change facts. Comparison
labels describe membership in the intended Library and never imply a source scan
for Detach. Mappings and effects are grouped beside their exact paths. Both views retain
all affected or preserved paths, permission decisions, application and
verification state, recovery disposition and residual paths. Expanded adds
supporting source, ownership, expected-state and hash details. Plan rows remain
labelled as planned when application is incomplete, failed or interrupted;
rendering does not infer that an individual planned effect was applied. A
source file and the relative link exposing it remain distinct identities.
No path is truncated. At most one required Next action comes from the result.

### Dry-run excerpt

```text
Library attach preview completed.
Status: complete
```

The identity and checks precede the planned changes:

```text
Plan: complete
  Create link: docs/guide.md -> ../shared/team-knowledge/guide.md
  Record: create (.agents/open-forge.libraries.json)
```

The dry run also prints `No files changed (--dry-run).` Application remains
`not started`. A completed apply reports its actual application, verification
and record-publication states; planned paths alone never establish success.

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
- distinct source-relative and mapped workspace-relative leaf paths and derived-link identity;
- every collision, Extension or manager ownership fact, and generated-region
  effect;
- the exact schema-v1 record effect and sorted resulting record projection;
- dry-run or application mode, expected-state, verification, recovery, and
  residual facts without source bytes; and
- semantic status and at most one required `Next:` action.

The result does not add an expected-link field to the persisted record and does
not disclose or materialize source content.

## Semantic Results

| Result        | Meaning                                                                                                                                                                                                                                                                                                          |
| ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `complete`    | A complete safe dry-run plan was established, or application and final verification completed. An empty eligible source inventory is complete when its record and any permitted generated projection are verified.                                                                                               |
| `attention`   | Target effects verified, but post-verification recovery cleanup has a positively observed retained residual under the shared recovery boundary. Planned changes alone do not create `attention`.                                                                                                                 |
| `incomplete`  | A valid request has an unavailable or inaccessible existing record or source fact, incomplete source inventory, incomplete generated projection, or unavailable required application recovery preparation. No effect begins.                                                                                     |
| `invalid`     | Command input, operand cardinality, library-ID grammar, source-root spelling, a malformed strict record, a missing or non-ordinary mandatory source root, or terminal-mode use is outside this interface.                                                                                                        |
| `blocked`     | The request is syntactically valid but duplicate or ambiguous identity, unsafe containment or physical aliasing, an unsafe or colliding record or destination, an unsafe generated region, unavailable real-link capability, or another mutation precondition prevents a safe complete attach. No effect begins. |
| `failed`      | An unexpected application, verification, or unknown recovery-disposition failure occurs after a persistent effect begins.                                                                                                                                                                                        |
| `interrupted` | The caller cancels before completion. Effects already verified remain residual truth; an unexpected post-effect failure remains `failed`.                                                                                                                                                                        |

For ordinary conditions, status precedence is `blocked` > `incomplete` >
`attention` > `complete`. Invalid input stops before operation resolution. The
shared [Result Coordinates](../../shared/result-coordinates/interface.md)
define numeric exits, stream coordinates, and the structured envelope.

## Errors And Boundaries

Attach reports `invalid` for zero or several positional operands, an ID outside
the exact grammar, a repeated positional operand, a malformed or extra-field
strict record, an invalid source-root spelling, a missing or non-ordinary
mandatory source root, or invalid terminal-mode use.

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
- select a collection, per-file subset, glob, per-file remapping or dependency;
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
   root that is missing or is not a real ordinary directory, or with a
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
  ordinary source and destination directories, link/reparse ancestry, aliases,
  actual destination overlap with protected source trees, and source-root availability;
- complete inventory coverage, excluded entrypoints and controls, ordinary
  file types, links and special entries, inaccessible subtrees, and source-byte
  preservation;
- derived source/destination path formation, relative raw-target identity,
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

## Compact JSON Output

Normal `--json` uses expanded output and the full schema-v1 document. Explicit
`--json --view=compact` uses the [shared compact envelope](../../shared/result-coordinates/interface.md#compact-json-envelope):
`schemaVersion: 2`, `view: "compact"`, then `command`, `status`, `workspace`,
`result` and `next`.
It is minified through the serializer. The command/status/workspace/next values
and process exit remain unchanged; expanded remains the default.

The compact result retains the complete command-owned result graph defined by
its structured schema, including every nullable value and ordered collection.
Its core already carries the facts needed to use the result. For mutation
commands this includes plans, exact previews, effects, permissions when
applicable, verification, findings and recovery. Rendering never asks a caller
to rerun a mutation to recover an omitted receipt.

No collection is truncated and no finding is filtered. Counts describe the
original operation. Both JSON views retain the same result facts.
The complete structured schema and examples elsewhere in this contract describe
expanded output unless explicitly labelled compact.
