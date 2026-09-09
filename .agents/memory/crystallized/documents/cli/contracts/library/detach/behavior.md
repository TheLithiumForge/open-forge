---
open-forge:
  description: Define the technology-neutral record checks, all-or-nothing planning, application, verification, and recovery behavior for `library detach`
  responsibility: Define how one complete detach plan is formed, guarded, applied, verified, or refused without source availability
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Library, Detach, Behavior, Mutation, Recovery, Safety, Determinism, CurrentTruth]
---

# library detach Behavior Contract

## Status And Boundary

This is the current Crystallized Behavior Contract for
`open-forge library detach`. It defines deterministic request and workspace
resolution, schema-v1 record and mapping facts, source-independent destination
checks, all-or-nothing planning, generated-region projection, preflight,
dry-run, lease-bound application, verification, typed recovery, residual truth,
and technology-neutral conformance.

The [Interface Contract](interface.md) owns public syntax, exact input grammar,
observable effects, statuses, output, examples, non-goals, and exactly three
public EndToEnd journeys. This file does not add operands, flags, aliases,
record properties, status names, output fields, or implementation callables.

The [Workspace Libraries Technical Design](../../../technical-designs/workspace-libraries.md)
and [Mutation And Recovery Technical Design](../../../technical-designs/mutation-and-recovery.md)
define shared realization boundaries. The [Index Behavior Contract](../../index-candidate/behavior.md)
defines generated-navigation facts when an existing consumer region is eligible.
The shared [Global Flags Behavior Contract](../../shared/global-flags/behavior.md),
[Shared Result Coordinates](../../shared/result-coordinates/behavior.md), and
[Shared CLI Operation Contract](../../../shared-operation-contract.md) retain
their cross-command meaning.

## Operation Invariants

Detach performs one complete whole-library operation for one selected record
identity. A conforming implementation follows this conceptual flow:

```text
validated library ID, workspace, record, and flags
  -> exact selected library record and mapped destination path set
  -> source-independent derived relative targets
  -> complete no-follow destination and generated-region facts
  -> one all-or-nothing ordered mutation plan
  -> effect-free preflight
  -> dry-run or lease-bound application
  -> under-lock revalidation and no-follow final checks
  -> typed recovery preparation, monotonic effects, and verification
  -> record publication or last-record removal
  -> one typed result with residual truth
```

No persistent effect begins until the record, every mapping, every destination
parent and leaf, any generated region, intended remaining record, ordered plan,
and preflight are complete. One changed or unsafe occupant blocks every effect.
Detach never applies a safe subset and never treats a matching unregistered
link as owned.

Source availability is not a precondition. Detach does not enumerate or resolve
the source root or link target. It uses the valid recorded path strings only to
derive the expected relative raw target and to classify the consumer-side link
without following it.

For unchanged consumer bytes and explicit input, resolution, mapping facts,
generated projection, record bytes, plan, and result are deterministic. A
successful repeated detach has no inferred no-op: once the record is absent,
the unknown ID is `invalid`.

## Request, Workspace, And Record Resolution

Request resolution validates the exact command path, one required singleton ID,
the library-ID grammar, `--dry-run`, shared global flags, and terminal behavior.
It does not accept a source-root operand, infer a library from a path, or rescan
raw arguments.

The selected workspace is the exact current workspace or exact shared
`--workspace` value. The consumer `.agents` path and each recorded destination
path are resolved within that workspace. The resolver never searches for a
source root, substitutes a Git root, or accepts an external destination.

The record is read from the exact consumer path
`.agents/open-forge.libraries.json`. It must have schema discriminator exactly
`1`, exactly `schemaVersion` and `libraries` at the top level, exactly `id`,
`sourceRoot`, `destinationRoot`, and `paths` for each library, no extra properties, no duplicate
IDs, source paths within a Library, or mapped destination leaves, and sorted IDs and path lists. The selected ID must resolve to
exactly one record. A missing record or unknown ID is `invalid`; a malformed,
unsafe, or unreadable required record is blocked or incomplete according to the
shared result boundary.

The selected `sourceRoot` is validated as a normalized portable
workspace-relative source-origin string and each `paths` value as a portable
source-relative leaf path. Detach does not require the source root to exist and does not perform physical source containment
resolution. A path that cannot safely derive a contained consumer destination
is blocked. The expected relative target is derived from the recorded source
root, destination root and source suffix; no expected target is read from the
record because the schema has no such field.

## Source-Independent Destination Facts

For each recorded source suffix, detach derives the destination under the recorded destination root below
the selected workspace. It checks destination parents one component at a time
without following links or reparse points. The consumer `.agents` root must
already be a real ordinary directory without link or reparse ancestry; detach
never creates or replaces it. A missing root, parent, or leaf makes the
registered link unverifiable and blocks; a parent link, reparse point, special
entry, external transition, alias, or unknown state is unsafe.

At the destination leaf, the operation distinguishes:

- an exact registered relative file symlink whose raw target equals the derived
  target;
- a missing leaf with otherwise safely observable parents;
- an ordinary file, directory, different link, junction, special entry,
  changed raw target, unsafe path, unknown state, or separately owned occupant.

Only the first case receives a link `Delete` effect. The target is not resolved,
so an exact dangling relative link is eligible. The second case blocks the
complete operation because a missing leaf does not prove the registered link
identity needed for all-or-nothing detachment. Every third case also blocks the
complete operation. A matching link outside the selected record remains
untouched.

The operation never reads source bytes to decide link identity. A source file,
source root, or raw target may be absent, while the exact consumer link still
has enough no-follow identity for safe detachment. Source bytes remain at their
original paths.

## Generated Projection And Intended Record

If a removed destination is part of an already established consumer route,
detach may form a bounded post-detach generated `Entries` projection under the
[Index Behavior Contract](../../index-candidate/behavior.md). The entrypoint,
route chain, generated markers, and authored bytes outside the region must
already be safely established. Generated lines are derived navigation, not
ownership evidence.

No source entrypoint is projected, and no route parent, Loader, authored
entrypoint, or missing generated region is created. A missing, ambiguous,
changed, or unsafe required region blocks the complete plan. When the consumer
path is not exposed by an existing route chain, there is no generated effect.

The intended record removes the selected library. If another library remains,
their records and paths stay sorted and unchanged. If the selected library is
last, the intended final state is positive absence of
`.agents/open-forge.libraries.json`. In either case, record publication occurs
after every link and generated effect verifies.

## Complete Plan And Preflight

The ordered plan contains only:

- an explicitly approved permission create/replace before link deletion;
- exact relative-file-link `Delete` effects for registered links whose raw
  target matches, including dangling links;
- permitted bounded generated-region replacements; and
- an ordinary record replacement or record `Delete` as the final publication
  effect.

A missing destination has no eligible link effect and blocks the plan. The plan
includes expected states and verification conditions for every path. It
contains no source deletion, source copy, source bytes, broad directory
removal, unregistered-link adoption, Loader rewrite, route creation, or
unselected path.

Effect-free preflight validates the record, destination containment, no-follow
parent and leaf states, raw-target equality, generated-region boundaries,
record expected state, duplicate logical targets, prospective physical aliases,
and cancellation. The plan is all-or-nothing. A changed occupant or an
unavailable required fact rejects the complete plan before effects.

`--dry-run` uses the same immutable request, record facts, source-independent
mapping classification, generated projection, intended record, ordered plan,
and preflight as application. It reports every exact link delete, dangling
link, missing-link blocker, generated effect, record effect, and other blocker, then
stops before lease acquisition or recovery capability probing. It writes no
consumer or application-data state. Planned effects alone do not create
`attention`.

## Consumer Permission

Derive required external leaves from every registered destination selected for exact-link deletion, using the recorded
Library/source identity. The [Interface](interface.md#consumer-permission)
selects scope proposals and the [Workspace Permissions Behavior](../../shared/workspace-permissions/behavior.md)
defines exact admission, future-descendant approval, source rebinding, revocation,
prompt grammar and receipt truth.

Complete source/record, ownership, mapping and structural preflight before the
question. Preserve immutable required/missing leaves, proposed/approved scopes
and any old/new source binding. Missing or declined permission blocks the whole
request; dry-run and noninteractive execution do not write approval.

Under the same workspace lease, compare exact permission bytes or prior absence
and every other volatile plan fact. Drift invalidates approval without merging
new grants. Prepare one bundle including prior permission bytes or absence, then
verify its ordinary create/replace before directories, links or generated effects.
Publish the Library record last. Failure preserves actual verified permission
outcome and residual evidence. Library repair never applies the permission entry;
explicit recovery checks current grants without widening or restoring them.

## Lease-Bound Application

For application, all mapping checks and preflight complete before one
same-workspace lease is acquired. The lease is held through under-lock
revalidation, all link and generated effects, verification, and record
publication. Under that lease, detach revalidates the selected record, every
destination parent and leaf, every raw target, generated regions, expected
record, and cancellation.

Immediately before each effect, the operation performs an immediate no-follow
final-component check and repeats its expected-state validation. It removes a
link only by its exact registered leaf and raw relative target. It never
follows the target or deletes its source, even when the target is present.

Effects are deterministic and monotonic. Exact link deletes and permitted
generated-region updates verify before the record is replaced or removed last.
Record publication is last.
If the selected ID is the last library, record absence is verified only after
those link effects. A record write cannot authorize a link effect retroactively.

## Typed Recovery And Residual Truth

For an effectful application, typed recovery is prepared and verified before
the first effect under the shared mutation boundary; an effect-free plan has no
recovery bundle. Library recovery distinguishes:

- a prior-missing ordinary consumer-record `Create`;
- a relative-file-link `Create` at an exact missing destination; and
- a relative-file-link `Delete` carrying the exact registered raw relative
  target.

It stores consumer-side record and permission bytes and raw link identity only. It never
stores, opens, follows, restores, or deletes source bytes. Strong no-follow
recovery can remove an exact created link or recreate an exact deleted link
only when its destination is safely missing and the recorded raw target is
unchanged. Recreated links may remain dangling because recovery does not need
the source target to exist.

Detach never automatically recovers, rolls back, compensates, or claims that a
source changed. If application, verification, cancellation, or final record
publication fails after an effect, no new effect begins. Already verified link
and generated effects remain true residual state, the prior record remains when
the final publication did not verify, and exact residual and retained recovery
facts are reported.

## Verification And Result Formation

Verification checks every planned link deletion for positive absence, every
generated region for its intended bounded bytes, and the final record for exact
schema-v1 properties, sorted IDs, sorted paths, and selected-library absence or
retention. It also verifies that source paths and bytes were not changed by the
operation without following them.

An application is `complete` only after all planned link and generated effects
and final record publication verify. A dry run is `complete` when its complete
plan and preflight verify. A missing destination is an unverifiable registered
link and is `blocked`, as are changed or unsafe occupants. A safe read-side
drift may be exposed as `attention`, but it never bypasses a mutation
precondition.
Recovery-cleanup retention maps to the shared `attention` case; unknown
post-effect state remains `failed`.

The operation forms one typed result and does not let human or JSON rendering
rerun the operation. The Interface Contract's status precedence, stream rules,
structured envelope, and public output remain authoritative.

## Conformance Evidence

Conformance must prove, at the cheapest boundary that directly owns each fact:

- exact parser and shared-flag behavior, ID grammar, unknown-ID invalidity,
  and no source-root operand;
- exact schema-v1 record properties, sorting, duplicate and malformed-record
  handling, selected-record resolution, and last-record removal;
- source-independent behavior with missing source roots, files, and link
  targets, exact dangling links, missing-link blocking, raw-target derivation, and no source-byte
  storage, read, write, or follow effect;
- no-follow consumer containment, real ordinary parents, missing-leaf blocking,
  exact raw-target and leaf identity, different-link detection, ordinary and
  special occupants, aliases, Extension/lifecycle/library ownership, and
  all-or-nothing blocking;
- existing generated-region projection, authored-byte preservation, no route
  creation, no Loader rewrite, and pre-dogfood link-aware guards for Route
  Update, Index, Route Move, and Route Remove;
- immutable dry-run/application plan parity, no dry-run lease or recovery
  capability probe, complete preflight, one lease, under-lock revalidation,
  immediate no-follow checks, typed recovery, monotonic effects, record-last
  publication, verification, residual truth, and no automatic rollback; and
- all shared semantic statuses, stream and JSON parity, retained recovery
  `attention`, exact source preservation, and useful error actions.

The three public EndToEnd journeys are defined only by the [Interface
Contract](interface.md#endtoend-journeys). Lower-tier evidence may exercise
additional record, occupant, dangling-link, failure, and residual branches
without creating additional public journeys.

## Related Current Sources

- [library detach Contract Set](_detach.md)
- [library detach Interface Contract](interface.md)
