---
open-forge:
  description: Define the exact public syntax, source-independent link checks, all-or-nothing effects, record, and results for `library detach`
  responsibility: Define what detach accepts, removes, preserves, rejects, and reports for one complete registered Workspace Library
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Library, Detach, Interface, Mutation, Recovery, Safety, CurrentTruth]
---

# library detach Interface Contract

## Status And Authority

This is the current Crystallized Interface Contract for
`open-forge library detach`. It owns the public purpose, exact syntax, operand
grammar, source-independent link boundary, observable effects, record changes,
statuses, output, errors, examples, non-goals, and public verification.

The sibling [Behavior Contract](behavior.md) defines deterministic
technology-neutral record and mapping facts, all-or-nothing planning,
preflight, application, verification, and recovery. The shared [Global CLI Flags](../../shared/global-flags/interface.md),
[Shared Result Coordinates](../../shared/result-coordinates/interface.md), and
[Shared CLI Operation Contract](../../../shared-operation-contract.md) retain
their shared meanings.

The [Workspace Libraries Technical Design](../../../technical-designs/workspace-libraries.md)
and [Mutation And Recovery Technical Design](../../../technical-designs/mutation-and-recovery.md)
define accepted shared realization boundaries. The [Index Behavior Contract](../../index-candidate/behavior.md)
defines existing generated-navigation projection. This Interface Contract adds
no callable or implementation choice. The active Task records implementation and executable evidence.

## Purpose And Operation Boundary

`library detach` removes the complete consumer projection for one registered
library and removes that library from the separate consumer record. It removes
only exact registered relative file symlinks and any permitted existing
generated `Entries` effects. It never removes, copies, moves, reads for
recovery, or writes through source bytes.

Detach is source-independent in the first release. It does not require the
recorded source root, source file, or link target to exist. An exact registered
relative file symlink may be removed even when it is dangling, provided its
no-follow parent and leaf identity and raw relative target still match the
recorded mapping. Strong typed recovery can recreate that same dangling link.

Detach is one whole-library, all-or-nothing mutation. It has no selector-based
partial form. A changed, unsafe, separately owned, missing, or otherwise
unverifiable occupant blocks every link and record effect. Only an exact
registered link, including an exact dangling link, supplies a deletable
projection identity. A missing destination is not silently converted into
record removal.

## Syntax

```text
open-forge library detach <library-id> [--dry-run] [global flags]
```

The command path selects the detach operation. The shared [Global CLI Flags](../../shared/global-flags/interface.md)
contract defines `--workspace`, `--json`, `--view`, `--verbose`, `--help`, and
`--version`; all six retain their shared spelling, composition, repetition,
terminal behavior, and output meaning.

`--dry-run` is the only preview spelling. `--help` and `--version` are terminal
forms and stop before workspace, record, mapping, or projection work.

Detach has no source-root operand, aliases, extra positional operands, `--force`,
`--automatic`, `--yes`, `--apply`, collection selector, remapping flag, glob,
copy mode, saved plan, partial selector, or generic mutation dispatcher.

## Operand And Repetition

| Operand or flag     | Role                                      | Accepted value                                  | Omission and repetition                                                                                        |
| ------------------- | ----------------------------------------- | ----------------------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| `<library-id>`      | Select one registered management identity | One value matching the library-ID grammar below | Required and singleton. An unknown ID is invalid; a successful prior detach does not establish a repeat no-op. |
| `--dry-run`         | Write policy                              | Boolean flag with no value                      | Application is selected when omitted. Repetition is accepted and idempotent.                                   |
| Shared global flags | Workspace and presentation                | Defined by the shared global contract           | Shared defaults and repetition rules apply.                                                                    |

The library ID supplies record selection only. It does not select a source
reference, route, or destination outside the selected workspace.

## Library-ID Grammar And Record Selection

`<library-id>` must contain 1–128 characters and match exactly:

```text
[a-z0-9]+(-[a-z0-9]+)*
```

The grammar uses lowercase ASCII letters and digits in non-empty segments
separated by one hyphen. Uppercase, empty segments, leading or trailing
hyphens, repeated hyphens, whitespace, slashes, backslashes, and other
characters are invalid. The ID is a management identity in the consumer
library-record namespace. It is not derived from the source root and is not an
automatic source ID or source-reference operand.

Detach reads the separate consumer-owned record:

```text
.agents/open-forge.libraries.json
```

The record must have exactly schema-v1 shape:

```json
{
  "schemaVersion": 1,
  "libraries": [
    {
      "id": "team-knowledge",
      "sourceRoot": "shared/team-knowledge",
      "destinationRoot": ".",
      "paths": [".agents/directives/review.md"]
    }
  ]
}
```

The only properties are `schemaVersion`, `libraries`, `id`, `sourceRoot`, `destinationRoot`, and
`paths` at their declared levels. `schemaVersion` is exactly numeric `1`.
Library records are sorted by ID, and each `paths` array is sorted by portable
path spelling. `paths` contains unique eligible source-relative
path strings mapped below the recorded `destinationRoot`. The record
stores no expected link target; detach derives the expected relative target
from recorded `sourceRoot`, `destinationRoot` and each source-relative suffix,
measured from the actual destination parent to the source leaf. Extra fields,
duplicates, malformed values, unsafe paths, or a missing record block or make
the request invalid under the shared result boundary. Detach never migrates or
repairs the record.

An unknown ID is `invalid`. After a successful detach, a repeat therefore
reports an unknown ID rather than claiming a no-op from absence alone. This is
a lower-tier repeat detail; the public journey remains the changed-occupant
blocking journey below.

## Source Independence And Mapping Boundary

Detach validates recorded roots and source-relative paths without resolving or
enumerating a source root. It derives the recorded destinations and raw relative
links from those strings. Missing sources and exact dangling links are supported.
Current destination permission is still required, bound to the recorded source
identity, and cannot be supplied by recovery bytes or an old grant for another
source.

Each record keeps `sourceRoot`, `destinationRoot` and source-relative `paths`.
For a path `p`, its source is `sourceRoot/p`. Its consumer destination is `p`
when `destinationRoot` is `.`, otherwise `destinationRoot/p`. Derive the exact
raw relative file-link target from the destination parent to that source.
Root-level leaf destinations use the workspace root as their parent.

Detach removes only exact registered relative file symlinks. Every existing
parent must be a real ordinary directory with no linked or reparse ancestry;
a missing parent blocks the request. Detach creates no parents or links. Local
siblings and destination directories remain untouched.

Validate recorded path grammar and destination protection without resolving or
enumerating source content. Protect
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

## Exact Registered Occupants

For each recorded mapping, detach distinguishes these current destination
facts:

| Destination fact                                                                                                                                  | Detach treatment                                                                        |
| ------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------- |
| Exact registered relative file symlink with the derived raw target                                                                                | Plan one link deletion. The target may exist or be dangling; detach does not follow it. |
| Positively missing leaf with safe no-follow parents                                                                                               | Block the complete request because the registered link identity cannot be verified.     |
| Ordinary file, directory, different link, junction, special entry, changed raw target, unsafe parent, unknown state, or separately owned occupant | Block the complete request and preserve every occupant and the record.                  |

An exact registered dangling link is still an exact registered link. Detach may
remove it even when both source and target are absent because raw target
identity, parent identity, and leaf identity are enough to prove the intended
consumer effect without following source bytes.

Detach does not sweep an unregistered link or any unlisted consumer path. A
matching link without the selected record's ownership evidence is not adopted.
Local sibling files remain untouched.

## Generated Navigation And Record Effects

When removing a projected child changes an already established consumer route,
detach may update only the existing consumer-owned generated `Entries` region
that the [Index Behavior Contract](../../index-candidate/behavior.md) permits.
The ordinary route chain and entrypoint must already exist. Authored bytes
outside the bounded generated region remain unchanged. If no route exposes a
projection, detach does not create one.

Detach never projects or materializes source entrypoints, creates route
parents, repairs a missing route chain, rewrites authored entrypoints, rewrites
the Loader, or invents Framework semantics. A required generated region that
is missing, ambiguous, changed, or unsafe blocks the complete request.

If other library records remain, the intended schema-v1 record removes only the
selected library and retains all other records in sorted ID order. If the
selected library is the last record, detach removes
`.agents/open-forge.libraries.json` only after every planned link and generated
effect verifies. Record publication is last. An unsafe, changed, or missing
occupant never permits record publication.

The record remains separate from `.agents/open-forge.lifecycle.json`. It has no
expected-link, source-byte, timestamp, Git, collection, per-file remapping, glob,
dependency, or source metadata field.

## Consumer Permission

This command selects [Workspace Permissions](../../shared/workspace-permissions/interface.md)
for every registered destination selected for exact-link deletion. `.agents/**` leaves remain implicit.
Requirements bind the selected Library ID and source root. Permission remains
necessary even for existing owned links; recorded identity makes removal
source-independent, without exempting it from revocation.

Every missing permission proposal is an exact file grant. Detach does not request future-folder authority or revoke saved grants.
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

Permission findings use the `library-detach.` prefix and suffixes
`permission-required`, `permission-declined`, `permission-invalid`,
`permission-unavailable`, `permission-changed` and `permission-write-failed`.
Changed lease-bound permission facts block; failed permission publication is
`failed` with its actual receipt; cancellation uses the existing `interrupted`
finding. No content effect proceeds after an unverified permission write.

## Dry Run And Application

`--dry-run` forms the same typed request, record facts, source-independent
derived targets, destination classifications, generated projection, intended
record, ordered plan, expected-state facts, and effect-free preflight as
application. It reports every exact link deletion, missing-link blocker,
generated-region effect, record effect, and blocker, then writes nothing. It
creates no directory, link, record, generated navigation, recovery artifact,
or lock, and performs no lease or recovery capability probe.

Omitting `--dry-run` selects application. Application completes all mapping
checks and all-or-nothing preflight before acquiring one same-workspace lease
for the effectful plan. Under that lease it revalidates the record, selected
ID, destination and parent states, generated regions, and expected record.
Every link, generated-region, and record effect receives an immediate
no-follow final-component check immediately before its effect.

The operation removes only exact registered relative file symlinks. It never
resolves a target to delete a source file, follows source bytes, copies a
source, or uses a copy fallback. For an effectful application, typed recovery
is prepared and verified before the first effect; an effect-free plan has no
recovery bundle. Effects are monotonic and are not rolled back or
compensated after verification. Link and generated effects verify before the
record is published last. When the selected ID is the last library, record
removal is the final publication effect.

## Human Output

Human output comes from one typed result. Expanded output includes the selected
workspace, library ID, record source root, source-independent mode, every
registered path, exact raw-target and no-follow classification, dangling or
missing target facts, generated-region effects, record effect, dry-run or
application mode, verification, recovery disposition, residual paths, and
semantic status.

Compact output retains the library ID, mode, status, completeness, safety,
every removed or safely absent path, every blocker, and at most one required
`Next:` action. The primary human result for `complete`, `attention`, and
`incomplete` goes to stdout. The primary human result for `invalid`, `blocked`,
`failed`, and `interrupted` goes to stderr. Bounded diagnostics use stderr.

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
Library detach preview completed.
Status: complete
```

Identity includes the recorded source and destination roots and the statement
`Source files are not scanned for this operation.` The plan lists exact links:

```text
Plan: complete
  Remove link: docs/guide.md -> ../shared/team-knowledge/guide.md
  Record: delete (.agents/open-forge.libraries.json)
```

The preview also prints `No files changed (--dry-run).` A completed apply reports
actual application and verification states. A failed or interrupted apply keeps
its recovery disposition and residual paths visible beside those states.

## Structured Output

`--json` emits one complete structured result to stdout for every semantic
status from the same typed result used by human output. It never prompts and
never reruns resolution, mapping checks, planning, application, verification,
or residual reporting. Human text is not mixed into JSON stdout; bounded
diagnostics use stderr.

The result exposes the concrete detach facts under the exact shared result
envelope, including:

- selected workspace, library ID, record validity, and source-independent mode;
- recorded source root and mapped consumer-relative paths without source
  bytes or a stored expected-link field;
- every exact, dangling, missing, changed, unsafe, unknown, or separately
  owned destination fact;
- generated-region effects, record effect, remaining library records, and
  record-publication-last evidence;
- dry-run or application mode, expected-state, verification, recovery, and
  residual facts; and
- semantic status and at most one required `Next:` action.

The result does not follow, disclose, or materialize source bytes.

## Semantic Results

| Result        | Meaning                                                                                                                                                                                                                                                                                     |
| ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `complete`    | A complete safe dry-run plan was established, or all exact registered links and permitted generated effects verified and the resulting record was published or removed. Every registered destination had an exact link identity.                                                            |
| `attention`   | Target effects verified, but post-verification recovery cleanup has a positively observed retained residual under the shared recovery boundary. Planned detach effects do not create `attention`.                                                                                           |
| `incomplete`  | A valid record or consumer boundary has a required coverage or application fact that cannot be completely inspected or prepared. No effect begins. Source unavailability alone is not incomplete because detach is source-independent.                                                      |
| `invalid`     | Command input, operand cardinality, library-ID grammar, unknown ID, malformed selection, source-root operand, or terminal-mode use is outside this interface.                                                                                                                               |
| `blocked`     | The request is syntactically valid but a malformed record, unsafe recorded path, changed or unsafe occupant, unproven raw target, collision, unsafe generated region, unavailable real-link capability, or another mutation precondition prevents a safe complete detach. No effect begins. |
| `failed`      | An unexpected application, verification, or unknown recovery-disposition failure occurs after a persistent effect begins.                                                                                                                                                                   |
| `interrupted` | The caller cancels before completion. Effects already verified remain residual truth; an unexpected post-effect failure remains `failed`.                                                                                                                                                   |

For ordinary conditions, status precedence is `blocked` > `incomplete` >
`attention` > `complete`. Invalid input stops before operation resolution. The
shared [Result Coordinates](../../shared/result-coordinates/interface.md)
define numeric exits, stream coordinates, and the structured envelope.

## Errors And Boundaries

Detach rejects or blocks:

- zero or several positional operands;
- an ID outside the exact grammar, an unknown ID, or a source-root operand;
- a missing, malformed, extra-field, duplicate, unsorted, or unsafe
  `.agents/open-forge.libraries.json` record;
- an absolute, backslash, dot-traversing, escaping, aliased, outside, or
  otherwise unsafe recorded source-root or path form;
- a missing or non-ordinary consumer `.agents` root or linked/reparse consumer
  ancestry;
- a consumer destination parent that crosses a link, reparse point, special
  entry, external transition, alias, or unknown state;
- a missing destination leaf, because the registered link identity cannot be
  proven for all-or-nothing detachment;
- any changed raw target, ordinary file, directory, different link, special
  entry, unsafe occupant, unknown state, or separately owned path;
- a missing, ambiguous, changed, or unsafe existing generated region when the
  route projection is required; or
- unavailable real-link capability or unavailable required application
  recovery preparation.

Every error names `library detach`, the library ID or affected path, the cause,
and one useful next action when one is known. Detach does not inspect Git,
perform Git operations or diagnostics, mutate remote or public state, create a
route, rewrite the Loader, or change any source file.

## Scenarios

Preview complete detachment as structured output:

```text
open-forge library detach team-knowledge --dry-run --json
```

Apply whole-library detachment:

```text
open-forge library detach team-knowledge
```

The request remains valid when the recorded source root or a registered link's
target is absent, provided every consumer destination is an exact registered
link with the derived raw target. A missing destination leaf is not an exact
registered link and blocks whole-library detachment.

## Non-Goals

`library detach` does not:

- detach one selected path, a collection, subset, glob, or partial mapping;
- attach or sync a library, choose another source root, or select an unrecorded
  link;
- require source availability, enumerate source inventory, resolve source
  targets, follow source bytes, copy a source, or use a copy fallback;
- overwrite, adopt, rename, or release a changed consumer occupant or
  lifecycle claim;
- remove any source file, source directory, source link, or source record;
- create route parents, materialize entrypoints, repair a missing route chain,
  rewrite authored entrypoints, rewrite the Loader, or invent Framework roots;
- change the `.agents/open-forge.lifecycle.json` document or add library data to
  that lifecycle schema;
- perform Git fetch, pull, checkout, switch, stage, commit, or diagnostics;
- write through a projected file or make Route Update, Index, Route Move, or
  Route Remove follow or delete a library source target; their link-aware guard
  and real-filesystem regression are prerequisites to library dogfood;
- add compatibility, migration, remote publication, deployment, JavaScript,
  dependency injection, a runtime registry, a generic mutation dispatcher, or
  another callable design; or
- create a receipt, saved plan, journal, or automatic rollback history.

## EndToEnd Journeys

1. **Dry-run has no effect.** Run
   `open-forge library detach team-knowledge --dry-run` for a valid record. The
   result lists the same complete exact-link, dangling-link, generated-region,
   and record plan as application, while no link, record, generated region,
   lease, recovery artifact, or source byte changes.
2. **Apply removes the exact projection and preserves the source.** Apply
   `open-forge library detach team-knowledge` when each registered destination
   is an exact relative link, including an exact dangling link. Detach removes
   only exact registered consumer links, removes or updates the record after
   link verification, and leaves every source path and source byte unchanged
   even when a source or link target is missing.
3. **A changed occupant blocks all effects.** Replace one registered
   destination with an ordinary file, directory, different link, or unsafe
   occupant and run detach. The whole request is `blocked`, no link or record
   effect occurs.

## Verification Requirements

Lower-tier and public proof must cover the following without adding another
public EndToEnd journey:

- exact command and flag parsing, singleton ID, shared global flags, terminal
  modes, JSON parity, stream assignment, and all seven statuses;
- library-ID length, ASCII grammar, unknown-ID invalidity, successful-repeat
  unknown-ID behavior, namespace separation, exact schema-v1 properties,
  sorting, duplicates, and malformed-record preservation;
- source-independent operation with missing source roots, missing source files,
  missing link targets, exact dangling links, valid recorded path derivation,
  unsafe recorded paths, and no source-byte read/write/follow effect;
- consumer containment, real ordinary no-follow parents, missing-leaf blocking,
  exact raw-target and leaf identity, different-link detection, ordinary and
  special occupants, aliases, Extension/lifecycle/library ownership, and
  whole-request blocking;
- existing generated-region projection, authored-byte preservation, no route
  creation, no Loader rewrite, and pre-dogfood link-aware guards for Route
  Update, Index, Route Move, and Route Remove;
- immutable dry-run/application plan parity, no dry-run lease or recovery
  capability probe, complete all-or-nothing preflight, one lease, under-lock
  revalidation, immediate no-follow checks, record-publication-last ordering,
  typed recovery, monotonic effects, verification, residual truth, and no
  automatic rollback;
- typed recovery for prior-missing ordinary record creation and relative-link
  creation/deletion, exact created-link removal, exact deleted-link recreation
  including dangling links, and no source-byte storage or following; and
- complete, `attention`, `incomplete`, `invalid`, `blocked`, `failed`, and
  `interrupted` result mapping with human/JSON parity.

The three public EndToEnd journeys are defined only by the [Interface
Contract](interface.md#endtoend-journeys). Lower-tier evidence may exercise
additional record, occupant, dangling-link, failure, and residual branches
without creating additional public journeys.

## Related Current Sources

- [library detach Contract Set](_detach.md)
- [library detach Interface Contract](interface.md)

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
