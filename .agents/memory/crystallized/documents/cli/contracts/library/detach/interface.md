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
no callable or implementation choice. Implementation and executable proof
remain pending the Task 23 contract freeze and later acceptance gates.

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

| Operand or flag | Role | Accepted value | Omission and repetition |
| --- | --- | --- | --- |
| `<library-id>` | Select one registered management identity | One value matching the library-ID grammar below | Required and singleton. An unknown ID is invalid; a successful prior detach does not establish a repeat no-op. |
| `--dry-run` | Write policy | Boolean flag with no value | Application is selected when omitted. Repetition is accepted and idempotent. |
| Shared global flags | Workspace and presentation | Defined by the shared global contract | Shared defaults and repetition rules apply. |

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
stores no expected link target; detach derives the expected relative target
from the recorded `sourceRoot` and each destination path. Extra fields,
duplicates, malformed values, unsafe paths, or a missing record block or make
the request invalid under the shared result boundary. Detach never migrates or
repairs the record.

An unknown ID is `invalid`. After a successful detach, a repeat therefore
reports an unknown ID rather than claiming a no-op from absence alone. This is
a lower-tier repeat detail; the public journey remains the changed-occupant
blocking journey below.

## Source Independence And Mapping Boundary

Detach validates the recorded `sourceRoot` and each recorded path as the
portable, workspace-relative forms required by the schema. It uses those
recorded strings only to derive the expected relative raw link target. It does
not require the source root to be present, enumerate its `.agents` directory,
resolve its physical target, read its bytes, or prove its current source
inventory.

The destination for every recorded path is the identical consumer-relative
`.agents/...` path below the selected workspace. Its expected relationship is a
relative file symlink from the destination's parent to the recorded source
path. The relationship is derived, not stored. Detach never accepts a record
path that escapes the consumer workspace, has an absolute or backslash form,
uses dot traversal, or cannot form a safe contained destination.

The consumer `.agents` root and every traversed destination parent must be a
real ordinary directory without link or reparse ancestry. A missing root,
parent, or leaf makes the registered link unverifiable and blocks the complete
request. A parent link, reparse point, special entry, alias, external
transition, or unknown state is unsafe.

## Exact Registered Occupants

For each recorded mapping, detach distinguishes these current destination
facts:

| Destination fact | Detach treatment |
| --- | --- |
| Exact registered relative file symlink with the derived raw target | Plan one link deletion. The target may exist or be dangling; detach does not follow it. |
| Positively missing leaf with safe no-follow parents | Block the complete request because the registered link identity cannot be verified. |
| Ordinary file, directory, different link, junction, special entry, changed raw target, unsafe parent, unknown state, or separately owned occupant | Block the complete request and preserve every occupant and the record. |

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
expected-link, source-byte, timestamp, Git, collection, remapping, glob,
dependency, or source metadata field.

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

### Dry-run example

```text
The library would be detached.
Library: team-knowledge
Registered paths: 1
Exact links to remove: 1

No files changed (--dry-run).
```

### Applied dangling-link example

```text
The library was detached.
Library: team-knowledge
Removed exact links: 1
Dangling exact links removed: 1
Record: removed
Source: preserved
```

### Changed occupant example

```text
The library was not detached.
Library: team-knowledge
Blocked: .agents/directives/review.md no longer has its registered link.
Next: restore the exact registered link or move the changed occupant, then run detach again.
```

The paths in these examples are repository-relative illustrative values. A
complete result has no required `Next:` action.

## Structured Output

`--json` emits one complete structured result to stdout for every semantic
status from the same typed result used by human output. It never prompts and
never reruns resolution, mapping checks, planning, application, verification,
or residual reporting. Human text is not mixed into JSON stdout; bounded
diagnostics use stderr.

The result exposes the concrete detach facts under the exact shared result
envelope, including:

- selected workspace, library ID, record validity, and source-independent mode;
- recorded source root and identical consumer-relative paths without source
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

| Result | Meaning |
| --- | --- |
| `complete` | A complete safe dry-run plan was established, or all exact registered links and permitted generated effects verified and the resulting record was published or removed. Every registered destination had an exact link identity. |
| `attention` | Target effects verified, but post-verification recovery cleanup has a positively observed retained residual under the shared recovery boundary. Planned detach effects do not create `attention`. |
| `incomplete` | A valid record or consumer boundary has a required coverage or application fact that cannot be completely inspected or prepared. No effect begins. Source unavailability alone is not incomplete because detach is source-independent. |
| `invalid` | Command input, operand cardinality, library-ID grammar, unknown ID, malformed selection, source-root operand, or terminal-mode use is outside this interface. |
| `blocked` | The request is syntactically valid but a malformed record, unsafe recorded path, changed or unsafe occupant, unproven raw target, collision, unsafe generated region, unavailable real-link capability, or another mutation precondition prevents a safe complete detach. No effect begins. |
| `failed` | An unexpected application, verification, or unknown recovery-disposition failure occurs after a persistent effect begins. |
| `interrupted` | The caller cancels before completion. Effects already verified remain residual truth; an unexpected post-effect failure remains `failed`. |

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
