---
open-forge:
  description: Define the exact public syntax, complete-inventory requirement, reconciliation effects, record, and results for `library sync`
  responsibility: Define what sync accepts, creates, retires, preserves, rejects, and reports for one registered Workspace Library
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Library, Sync, Interface, Mutation, Recovery, Safety, CurrentTruth]
---

# library sync Interface Contract

## Status And Authority

This is the current Crystallized Interface Contract for
`open-forge library sync`. It owns the public purpose, exact syntax, operand
grammar, record and source-availability boundary, reconciliation effects,
statuses, output, errors, examples, non-goals, and public verification.

The sibling [Behavior Contract](behavior.md) defines deterministic
technology-neutral resolution, complete source inventory, set reconciliation,
planning, preflight, application, verification, and recovery. The shared
[Global CLI Flags](../../shared/global-flags/interface.md),
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

`library sync` reconciles one registered consumer-local library with the
complete current eligible ordinary-file inventory below its recorded source
root's `.agents/` directory. It creates missing links for current registered
or newly discovered eligible paths, removes retired projections only when the
registered relative-link identity is proven, updates permitted existing
generated `Entries` regions, and publishes the sorted consumer record last.

Sync is one complete mutation for one library. It must establish the complete
source inventory before planning any retirement. A source that is unavailable
or incompletely enumerable prevents every addition, retirement, generated
change, and record effect. A changed or unsafe destination occupant blocks the
whole request; Sync does not apply unaffected mappings as a safe subset.

Sync changes projection links and the consumer record only. It never copies,
moves, deletes, or writes through source bytes. Changes to the bytes of an
ordinary source file are visible through its existing link and do not create a
copy or a source mutation.

## Syntax

```text
open-forge library sync <library-id> [--dry-run] [global flags]
```

The command path selects the sync operation. The shared [Global CLI Flags](../../shared/global-flags/interface.md)
contract defines `--workspace`, `--json`, `--view`, `--verbose`, `--help`, and
`--version`; all six retain their shared spelling, composition, repetition,
terminal behavior, and output meaning.

`--dry-run` is the only preview spelling. `--help` and `--version` are terminal
forms and stop before workspace, record, source, or projection work.

Sync has no source-root operand, aliases, extra positional operands, `--force`,
`--automatic`, `--yes`, `--apply`, `--prune`, collection selector, remapping
flag, glob, copy mode, saved plan, partial-detach selector, or generic mutation
dispatcher. The recorded `sourceRoot` is the only source selection for the
requested library.

## Operand And Repetition

| Operand or flag | Role | Accepted value | Omission and repetition |
| --- | --- | --- | --- |
| `<library-id>` | Select one registered management identity | One value matching the library-ID grammar below | Required and singleton. An unknown ID is invalid; a malformed record is blocked. |
| `--dry-run` | Write policy | Boolean flag with no value | Application is selected when omitted. Repetition is accepted and idempotent. |
| Shared global flags | Workspace and presentation | Defined by the shared global contract | Shared defaults and repetition rules apply. |

No flag adds retirement authority, bypasses expected-link proof, chooses a
source root, or changes record ownership. Sync never treats a recommendation
or a matching unregistered link as permission to adopt it.

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

Sync reads the separate consumer-owned record:

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
      "paths": [
        ".agents/directives/review.md"
      ]
    }
  ]
}
```

The only properties are `schemaVersion`, `libraries`, `id`, `sourceRoot`, and
`paths` at their declared levels. `schemaVersion` is exactly numeric `1`.
Library records are sorted by ID, and each `paths` array is sorted by portable
path spelling. `paths` contains unique eligible `.agents/...` source-relative
path strings that are also their identical consumer destinations. The record
stores no expected link target; Sync derives the expected relative target from
`sourceRoot` and each destination path. Extra fields, duplicates, malformed
values, absolute paths, and unsafe paths block the operation. The record is not
part of `.agents/open-forge.lifecycle.json`.

An unknown ID has no accepted no-op interpretation and returns `invalid`.
Sync does not select an ID by prefix, source-root similarity, destination
basename, or route position. A record's source-root spelling is the only source
selection and is validated again before mutation.

## Source Availability And Complete Inventory

The recorded `sourceRoot` must remain a normalized portable workspace-relative
path strictly contained by the selected workspace both lexically and
physically. Its resolved root and `.agents` child must be real ordinary
directories with no link or reparse ancestry. The source-root tree must remain
physically disjoint from the consumer workspace's `.agents` tree. Sync does
not reinterpret a changed or aliased record path, and it does not search for a
replacement root.

Sync inventories the entire eligible source `.agents/` subtree before any
retirement is considered. Eligible entries are ordinary regular files whose
portable source-relative paths are not exclusions. The inventory excludes the
Loader, recognized entrypoints, adjacent overwrite companions, lifecycle,
library, and other manager controls, symlinks, junctions, reparse points, and
special or non-ordinary entries. Links are never traversed.

An inaccessible directory or subtree, enumeration failure, unavailable source
root, or other coverage failure makes the complete source inventory
`incomplete`. Sync then performs no link creation, link deletion, generated
projection, or record publication. An excluded link is not followed and does
not silently contribute descendants. Source bytes are not copied or stored by
Sync and remain untouched.

The consumer `.agents` root must already be a real ordinary directory without
link or reparse ancestry; Sync never creates or replaces it. Each destination
parent is checked without following links or reparse points. Missing parents
below that root may be created only as real ordinary directories required by a
declared mapping. Existing parent links, reparse points, special entries,
aliases, external transitions, or unknown states block. Existing local files
beside a declared destination remain untouched.

## Reconciliation Effects

Let the complete current eligible path set be `P` and the selected record's
registered path set be `R`. Both sets use the same portable `.agents/...`
strings, so no source-to-destination remapping is inferred.

| Relationship | Destination fact | Sync effect |
| --- | --- | --- |
| `P ∩ R` | Exact registered relative file symlink remains at the identical destination and its raw target is the derived target | Preserve the link and record path. |
| `P ∩ R` | Destination leaf is missing and its parent path is safe | Create the exact relative file symlink. |
| `P ∩ R` | Destination is an ordinary file, directory, different link, special entry, unsafe path, unknown state, or separately owned path | Block the whole Sync. |
| `P \ R` | Destination leaf is exactly missing and its parent path is safe | Create a new exact relative file symlink and add the path to the record. |
| `P \ R` | Any destination occupant exists or is unsafe, including an unregistered matching link | Block the whole Sync. |
| `R \ P` | Destination is the exact registered relative symlink with the derived raw target, whether its source target is present or dangling | Delete only that exact link and remove the path from the record. |
| `R \ P` | Destination is positively missing, even with safe no-follow parents | Block the whole Sync because the registered link cannot be proven for retirement. |
| `R \ P` | Destination is an ordinary file, directory, different link, special entry, unsafe path, unknown state, or separately owned path | Block the whole Sync. |

The source inventory must be complete before the `R \ P` set is formed. A
source file becoming excluded is absent from `P` and follows the same exact
retirement proof. A missing current link in `P ∩ R` is safe drift that Sync may
repair after complete preflight; a missing retired link in `R \ P` is
unverifiable and blocks. A changed occupant is unsafe mutation drift and
blocks. Read-only observation may expose complete safe drift as `attention`,
but Sync never uses that status to bypass a mutation precondition.

Sync does not sweep unregistered consumer files or adopt a matching symlink.
Local sibling files remain untouched. Source file byte changes do not create a
retirement or replacement effect because links project live source content.

## Generated Navigation And Record

When a source addition or retirement changes an already established consumer
route, Sync may update only the existing consumer-owned generated `Entries`
region that the [Index Behavior Contract](../../index-candidate/behavior.md)
allows. The ordinary route chain and entrypoint must already exist. Authored
bytes outside the bounded generated region remain unchanged.

Sync never projects a source entrypoint, creates a route parent, materializes a
missing route chain, rewrites the Loader, or invents Framework semantics. If a
required existing generated region is missing, ambiguous, or unsafe, the
complete request blocks or is incomplete according to the established fact.

The intended record preserves the selected library's source root and replaces
its `paths` with the sorted current eligible set `P`, while retaining other
library records in sorted ID order. It has no extra field, expected target,
source bytes, absolute path, Git fact, dependency, collection, remapping, glob,
or exclusion metadata. The record is published after all link and generated
effects verify.

## Dry Run And Application

`--dry-run` forms the same typed request, record and source facts, complete
inventory, current-versus-registered reconciliation, collision checks,
generated projection, intended record, ordered plan, expected-state facts, and
effect-free preflight as application. It reports every create, retirement,
generated-region change, record change, preserved drift, and blocker, then
writes nothing. It creates no directory, link, record, generated navigation,
recovery artifact, or lock, and performs no lease or recovery capability probe.

Omitting `--dry-run` selects application. Application completes source
inventory, all collision checks, and preflight before acquiring one
same-workspace lease for the effectful plan. Under that lease it revalidates
the record, source-root boundary, complete inventory, set relationship,
destination and parent states, generated regions, and expected record. Every
link, generated-region, and record effect receives an immediate no-follow
final-component check immediately before its effect.

Sync creates only missing exact relative file symlinks and removes only exact
registered raw-target links. It never resolves a link to delete its target and
never copies or writes source bytes. For an effectful application, typed
recovery is prepared and verified before the first effect; an effect-free plan
has no recovery bundle. Effects are monotonic and are not rolled back or
compensated after verification. Link and generated effects verify before the
record is published last.

## Human Output

Human output comes from one typed result. Expanded output includes the selected
workspace, library ID, recorded source root, inventory completeness, current
and registered path sets, created and retired paths, exact raw-target proof,
preserved local occupants, generated-region effects, record effect, dry-run or
application mode, verification, recovery disposition, residual paths, and
semantic status.

Compact output retains the library ID, source root, mode, status, completeness,
safety, every created or retired path, every blocker, and at most one required
`Next:` action. The primary human result for `complete`, `attention`, and
`incomplete` goes to stdout. The primary human result for `invalid`, `blocked`,
`failed`, and `interrupted` goes to stderr. Bounded diagnostics use stderr.

### Unchanged no-op example

```text
The library is already synchronized.
Library: team-knowledge
Source root: shared/team-knowledge
Created links: 0
Retired links: 0
Record: unchanged
```

### Reconciliation dry-run example

```text
The library would be synchronized.
Library: team-knowledge
Create: .agents/guidance/new.md
Retire: .agents/guidance/old.md
Record paths: 1

No files changed (--dry-run).
```

### Changed occupant example

```text
The library was not synchronized.
Library: team-knowledge
Blocked: .agents/guidance/new.md has a changed consumer occupant.
Next: restore the expected missing destination, then run sync again.
```

The paths in these examples are repository-relative illustrative values. A
complete no-op and a successful application have no required `Next:` action.

## Structured Output

`--json` emits one complete structured result to stdout for every semantic
status from the same typed result used by human output. It never prompts and
never reruns resolution, inventory, planning, application, verification, or
residual reporting. Human text is not mixed into JSON stdout; bounded
diagnostics use stderr.

The result exposes the concrete sync facts under the exact shared result
envelope, including:

- selected workspace, library ID, record source root, and record validity;
- complete inventory coverage and excluded or unavailable source facts;
- registered, current, added, preserved, missing, retired, and blocked path
  facts;
- exact identical source/destination paths and derived raw-link identity,
  without storing an expected-link field in the record;
- generated-region effects, consumer ownership and collision facts, and the
  sorted intended record;
- dry-run or application mode, expected-state, verification, recovery, and
  residual facts without source bytes; and
- semantic status and at most one required `Next:` action.

## Semantic Results

| Result | Meaning |
| --- | --- |
| `complete` | A complete safe no-op or dry-run reconciliation was established, or all planned additions, exact retirements, generated-region changes, and final record publication verified. |
| `attention` | Target effects verified, but post-verification recovery cleanup has a positively observed retained residual under the shared recovery boundary. Planned additions or retirements do not create `attention`. |
| `incomplete` | The selected record or source has a required coverage or application fact that cannot be completely inspected or prepared, most importantly an unavailable or incomplete source inventory. No effect begins. |
| `invalid` | Command input, operand cardinality, library-ID grammar, unknown ID, source selection through an operand, or terminal-mode use is outside this interface. |
| `blocked` | The request is syntactically valid but malformed record data, unsafe containment or aliasing, a changed occupant, an unproven expected raw target, a collision, an unsafe generated region, unavailable real-link capability, or another mutation precondition prevents a safe complete reconciliation. No effect begins. |
| `failed` | An unexpected application, verification, or unknown recovery-disposition failure occurs after a persistent effect begins. |
| `interrupted` | The caller cancels before completion. Effects already verified remain residual truth; an unexpected post-effect failure remains `failed`. |

For ordinary conditions, status precedence is `blocked` > `incomplete` >
`attention` > `complete`. Invalid input stops before operation resolution. The
shared [Result Coordinates](../../shared/result-coordinates/interface.md)
define numeric exits, stream coordinates, and the structured envelope.

## Errors And Boundaries

Sync rejects or blocks:

- zero or several positional operands;
- an ID outside the exact grammar, an unknown ID, or an attempted source-root
  operand;
- a missing, malformed, extra-field, duplicate, unsorted, or unsafe
  `.agents/open-forge.libraries.json` record;
- a missing, absolute, backslash, dot-traversing, escaping, aliased, outside,
  or physically overlapping recorded source root;
- a non-ordinary source root or `.agents` child, link or reparse ancestry, or
  source and consumer `.agents` physical overlap;
- a missing or non-ordinary consumer `.agents` root or linked/reparse consumer
  ancestry;
- an unavailable or incomplete source inventory, inaccessible subtree, or
  incomplete exclusion pass;
- a missing retired destination where exact registered raw-target proof is
  required;
- any ordinary, directory, different-link, special, unsafe, unknown, or
  separately owned destination occupant where an exact missing destination or
  exact registered raw target is required;
- an unregistered matching link, Extension or lifecycle-owned path, library
  control, two-library destination collision, or unsafe parent directory;
- a missing, ambiguous, or changed existing generated region when the route
  projection is required; or
- missing real-link capability or unavailable required application recovery
  preparation.

Every error names `library sync`, the library ID or affected path, the cause,
and one useful next action when one is known. Sync does not inspect Git,
perform Git operations or diagnostics, mutate remote or public state, create a
route, rewrite the Loader, or change any source file.

## Scenarios

Verify an unchanged library without creating effects:

```text
open-forge library sync team-knowledge
```

Preview source additions and retirements with structured output:

```text
open-forge library sync team-knowledge --dry-run --json
```

The source root always comes from the selected record. The command does not
accept a second path to override it.

## Non-Goals

`library sync` does not:

- attach a new ID, choose a source root, detach a library, list or inspect
  records, or perform a selector-based partial detach;
- infer a source inventory from a partial read, remove a retired link before
  complete inventory, or apply an unaffected subset after any changed
  occupant;
- sweep or adopt unregistered files or matching links;
- select a collection, subset, glob, remapping, dependency, or external
  destination;
- traverse, copy, write, move, delete, or follow source bytes;
- overwrite, adopt, rename, or release a consumer occupant or lifecycle claim;
- create route parents, materialize entrypoints, rewrite authored entrypoints,
  rewrite the Loader, or invent Framework roots;
- change the lifecycle record or add library data to that schema;
- perform Git fetch, pull, checkout, switch, stage, commit, or diagnostics;
- write through a projected file or make Route Update, Index, Route Move, or
  Route Remove follow or delete a library source target; their link-aware guard
  and real-filesystem regression are prerequisites to library dogfood;
- add compatibility, migration, remote publication, deployment, JavaScript,
  dependency injection, a runtime registry, a generic mutation dispatcher, or
  another callable design; or
- create a receipt, saved plan, journal, or automatic rollback history.

## EndToEnd Journeys

1. **Unchanged complete no-op.** Run
   `open-forge library sync team-knowledge` for a valid complete source
   inventory whose registered paths and exact relative links already match. The
   result is `complete`, reports no link or record effect, and leaves consumer
   and source bytes unchanged.
2. **Source addition and retirement reconcile exactly.** Add one eligible
   ordinary source path and retire one previously eligible source path, then run
   `open-forge library sync team-knowledge --dry-run` followed by the same apply
   request. Dry-run lists the exact create and raw-target delete plus any
   permitted generated-region and record changes without effects; application
   creates and retires only those links, publishes the sorted record last, and
   leaves source bytes unchanged.
3. **Incomplete source or changed occupant blocks all effects.** Run Sync when
   the source inventory is incomplete or when an intended destination has a
   changed or unsafe occupant. The result is `incomplete` or `blocked` as
   applicable, and it creates no link, deletes no link, changes no generated
   region, and publishes no record.

## Verification Requirements

Lower-tier and public proof must cover the following without adding another
public EndToEnd journey:

- exact command and flag parsing, singleton ID, shared global flags, terminal
  modes, JSON parity, stream assignment, and all seven statuses;
- library-ID length, ASCII grammar, unknown-ID invalidity, namespace
  separation, exact schema-v1 record properties, sorting, duplicates, and
  malformed-record preservation;
- recorded source-root normalization, strict lexical and physical containment,
  ordinary source and `.agents` directories, link/reparse ancestry, aliases,
  physical disjointness, source availability, and complete inventory before
  retirement;
- complete inventory coverage, inaccessible-subtree `incomplete`, every
  exclusion, link non-traversal, ordinary-file coverage, and source-byte
  preservation;
- `P ∩ R`, `P \\ R`, and `R \\ P` reconciliation, missing-link recreation,
  exact raw-target deletion including dangling targets, missing-retired-link
  blocking, changed-occupant whole-request blocking, local-sibling
  preservation, and no adoption;
- relative-link identity, real parent directories, no copy fallback,
  Extension/lifecycle/library ownership, two-library collisions, and no
  external destinations;
- existing generated-region projection, authored-byte preservation, no route
  creation, no Loader rewrite, and pre-dogfood link-aware guards for Route
  Update, Index, Route Move, and Route Remove;
- immutable dry-run/application plan parity, no dry-run lease or recovery
  capability probe, complete preflight, one lease, under-lock revalidation,
  immediate no-follow checks, record publication last, typed recovery,
  monotonic effects, verification, residual truth, and no automatic rollback;
- typed recovery for prior-missing ordinary record creation and relative-link
  creation/deletion, exact created-link removal, exact deleted or
  dangling-link recreation, and no source-byte storage or following; and
- complete no-op, `attention`, `incomplete`, `invalid`, `blocked`, `failed`,
  and `interrupted` result mapping with human/JSON parity.

The three public EndToEnd journeys are defined only by the [Interface
Contract](interface.md#endtoend-journeys). Lower-tier evidence may exercise
additional inventory, retirement, collision, failure, and residual branches
without creating additional public journeys.

## Related Current Sources

- [library sync Contract Set](_sync.md)
- [library sync Interface Contract](interface.md)
