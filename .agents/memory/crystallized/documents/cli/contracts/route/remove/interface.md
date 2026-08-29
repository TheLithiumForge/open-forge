---
open-forge:
  description: Accepted current public interface for removing one eligible unmanaged routed leaf or complete routed category
  responsibility: Define what `route remove` accepts, removes, reports, rejects, and leaves unchanged
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Route, Remove, Interface, Mutation, Reference, Safety, CurrentTruth]
---

# route remove Interface Contract

## Status And Authority

This is the accepted current Crystallized authority for the caller-visible
Interface Contract for `route remove`. The command does not ship yet;
implementation and executable proof remain pending Gate 5. It is one explicit
mutation operation, not a generic batch or apply surface.

The sibling [Behavior Contract](behavior.md) defines the deterministic,
technology-neutral resolution, planning, effects, safety, recovery, and
conformance behind this public surface. The shared [Global CLI Flags](../../shared/global-flags/interface.md)
and [CLI Source References](../../shared/source-references/interface.md)
contracts retain their shared meanings.

The [Routed Markdown Representation](../../../../framework/markdown/routes.md),
[Routing Model](../../../../framework/routing/model.md),
[Routing Paths And Identity](../../../../framework/routing/paths.md),
[Overwrite Customization](../../../../framework/routing/overwrites.md),
and [Markdown Compatibility Boundary](../../../../framework/markdown/compatibility.md)
sources remain authoritative for the Framework meaning consumed by this
operation. The [Index Interface Contract](../../index-candidate/interface.md)
remains authoritative for generated-navigation projection and bounded generated
regions.

The [CLI Architecture](../../../architecture.md) defines the accepted shared
structured schema, process-status mapping, parser and serializer dependencies,
filesystem and physical-identity boundary, workspace lock, recovery boundary,
test evidence, runtime, Native AOT, and source-layout choices. This command
contract adds no competing implementation choice and preserves the observable
boundaries below.

## Purpose And Operation Boundary

`route remove` removes one eligible ordinary unmanaged logical leaf or one
eligible ordinary unmanaged category. It removes the complete selected physical
subject, updates affected generated navigation in the same operation, and
detaches every supported incoming authored Markdown link from outside the
removed subject by replacing that link with its visible label as plain authored
text.

A leaf is one ordinary routed Markdown base source and its valid adjacent
overwrite companion, when present. A category is one recognized entrypoint and
the complete physically contained folder tree below it. A category contains all
physically contained regular files and directories, including descendant
entrypoints, routed or unrouted Markdown, native or binary resources, ordinary
support files, and overwrite companions. Generated `Entries` interiors are
derived projections rather than authored authority.

The command performs one operation for one subject. A category plan may
enumerate many contained logical sources and resources, but it has one atomic
plan, one recovery boundary, and one final verification boundary. It is never a
series of independently committed leaf removals and never exposes a generic
batch or saved-plan surface.

The command is stateless and deterministic for unchanged workspace bytes and
explicit input. A repeated remove is a verified no-op only when exact intended
absence is independently established with complete trusted ownership, topology,
reference, and generated-projection evidence, with no orphan companion, residual
incoming reference, or stale generated region. Otherwise a missing, invalid,
incomplete, or blocked result is returned as applicable; absence alone does not
prove that an earlier remove succeeded. The operation creates no receipt,
tombstone, journal, saved plan, or history used to manufacture provenance.

## Syntax

```text
open-forge route remove <source-reference>
  [--dry-run]
  [global flags]
```

The command path selects the remove operation. It requires exactly one source
reference. The shared [Global CLI Flags](../../shared/global-flags/interface.md)
contract defines `--workspace`, `--json`, `--view`, `--verbose`, `--help`, and
`--version`; all six apply under that contract.

`--dry-run` is the only preview spelling. The command does not inspect or report
repository state. It does not select a subject, add authority,
or change the operation.

The command has no `--force`, `--automatic`, `--yes`, `--apply`, `--all`,
`--batch`, `--recursive`, root remove mode, alias, saved plan, receipt, or
generic mutation dispatcher. It has no operand-free wizard because the primary
subject cannot be safely selected by enumeration. JSON and other
non-interactive use therefore use the same explicit request and never prompt.

## Operand And Repetition

| Operand              | Role                                                                        | Accepted value                                                                                       | Omission and repetition              |
| -------------------- | --------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------- | ------------------------------------ |
| `<source-reference>` | Select one existing eligible logical leaf or recognized category entrypoint | One shared automatic source ID or exact `.agents/...` path, narrowed by this command's subject rules | Required; exactly one source subject |

The source operand follows the shared source-reference classification: `.agents/`
and `./.agents/` prefixes request exact paths, and every other value is an
automatic source ID. The CLI does not guess between the two forms. An ID that
does not resolve to exactly one eligible subject is not repaired by ranking,
basename matching, or route proximity.

`--dry-run` is a Boolean write-policy flag. Repeating it is accepted and
idempotent. Repetition does not multiply preview, consent, recovery, or mutation
authority. Shared global-flag
repetition, ordering, terminal behavior, and composition remain defined only by
the shared contract.

## Source Subject Selection

The source reference must resolve to one of these two subject forms:

1. One ordinary routed Markdown base source, optionally with its valid adjacent
   overwrite companion. This is the leaf form.
2. One recognized entrypoint that identifies one category folder. The source
   reference must resolve to that entrypoint itself, not merely to an arbitrary
   directory. This is the category form.

The Loader and the `.agents` workspace root are never remove subjects. A
Loader-exposed category may be eligible; when it is removed, the affected Loader
projection is part of the same plan. A recognized compatibility entrypoint can
be selected only when the route structure is otherwise unambiguous.

An automatic-ID collision is blocked in non-interactive and JSON use and does
not select a candidate. An exact path can identify the intended physical source,
but it cannot make an ambiguous route, overwrite pair, or category inventory
safe.

## Eligible Leaf

An eligible leaf is one ordinary routed Markdown logical source below an existing
valid entrypoint. Its base file and valid adjacent overwrite companion are one
logical subject and are removed together, preserving the pair's identity until
the complete effect is applied.

The selected leaf is not eligible when it is any of the following:

- an entrypoint, the Loader, or the `.agents` root;
- a routed native source such as `SKILL.md`, a non-Markdown resource, or an
  unsupported source kind;
- an orphan or ambiguous overwrite companion;
- a source with any trusted Framework or Extension lifecycle ownership claim;
- a source whose identity, route, containment, collision, reference, or recovery
  boundary cannot be established completely; or
- a source whose removal would leave a supported incoming reference that cannot
  be safely detached.

The command does not adopt an existing file, release lifecycle ownership, repair
route meaning, or reinterpret a generated entry as an authored source.

## Eligible Category

An eligible category is selected through one recognized entrypoint source
reference. Its subject is the complete physically contained folder tree rooted at
that entrypoint, including:

- the root entrypoint and its overwrite companion, when present;
- every descendant entrypoint and routed or unrouted Markdown source;
- every native, binary, ordinary, or support resource regardless of whether it
  is independently routable;
- every overwrite companion belonging to a contained base; and
- every other physically contained regular file and directory.

Derived generated regions are projected from the intended post-remove topology.
They are not treated as authored source authority or as independent category
items.

The category is eligible only when every contained item has complete safe
containment, physical identity, ownership, lifecycle, collision, reference, and
recovery classification. An item that is neither a regular file nor a directory,
or that is missing, malformed, conflicting, stale, unreadable, externally
resolving, ambiguous, incomplete, or unsafe, prevents the category operation as
a whole. An unfamiliar file extension does not exclude an otherwise safe regular
file from the category. The command does not skip an item, remove a safe subset,
or reinterpret a folder as a batch of leaf commands.

An entrypoint nested inside the selected category is part of the category, not a
second operation. A category's internal relative layout is removed as one
complete subject. The command does not flatten, reorder, or independently
commit descendants. The Loader itself and any item whose physical identity
cannot remain contained are outside this boundary.

## Positive Unmanaged Proof

Before a leaf or category can enter a mutation plan, the command must successfully
load a complete trusted lifecycle-ownership inventory for the selected workspace.
The inventory includes the Framework baseline and every applicable Extension
receipt or manager claim. It must establish that no trusted lifecycle source
claims any selected logical source or resource.

For a category, this proof covers every selected item, not only the root
entrypoint. A single claim, ownership conflict, stale claim, or unresolved item
blocks the complete category plan.

The following do not prove unmanaged status by themselves:

- failing to find one receipt or looking in one lifecycle source;
- a path, route placement, tag, generated entry, or familiar folder name;
- matching bytes, matching fingerprints, or an apparently initial file; or
- a previous command result, recommendation, or absence of a marker.

Missing, malformed, conflicting, stale, or incomplete lifecycle-ownership
inventory is a blocking authority condition. The command does not adopt content,
release ownership, repair lifecycle records, migrate receipts, or continue on a
partial inventory.

## Complete Reference Pass And Detachment

Remove performs one complete physically contained pass over the supported
Markdown catalogue in the selected workspace, both inside and outside `.agents`.
The catalogue includes every supported Markdown source and physical layer that
can contain an authored local reference, including base files and overwrite
companions. The command does not silently narrow this pass to routed files or to
the selected category.

Generated `Entries` interiors are derived navigation. The pass does not treat
their links as authored reference authority or detach them as ordinary prose;
affected generated regions are projected separately.

For every exact supported incoming local Markdown link from outside the selected
subject to the removed leaf, category member, or resource, the operation
replaces the complete link with its visible label as plain authored text. It
preserves the surrounding prose, whitespace, punctuation, and unrelated bytes.
Each detachment is a planned effect and is surfaced in dry-run and final human
and JSON results with its source location, original target, and visible label.

References originating inside the removed subject disappear with that subject.
They are not rewritten or detached in a surviving source. References that do
not target the removed subject remain unchanged.

External URLs are not local incoming references and are not changed. Unsupported
or ambiguous link forms that could target the removed subject, a transformation
that would lose surrounding prose, or an unsafe target identity blocks the
operation with no writes. If the supported Markdown catalogue cannot be
completely enumerated or inspected, the result is `incomplete` and no write
begins. The command never silently leaves a supported incoming link pointing at
the removed subject.

## Generated Navigation

Remove projects generated navigation against the hypothetical post-remove
workspace through the accepted [Index Behavior Contract](../../index-candidate/behavior.md).
It includes the old exposing parent projection and includes the Loader
projection when the removed subject is a Loader-exposed category. Any other
generated region is included only when its direct-child projection changes under
the removed topology.

The projection uses current authored topology and metadata, not current generated
lines, to derive entries. It preserves each valid marker pair and every byte
outside the bounded generated interior. It never starts a hidden `index`
subprocess. A generated boundary or required sibling projection that cannot be
established safely prevents the complete plan.

Generated effects, incoming-link detachments, and subject removal are part of the
same plan. The result reports them together, and final verification checks the
complete post-remove projection rather than treating navigation as a later
cleanup step.

## Planning And Effects

Remove follows one complete typed mutation flow:

```text
validated source
  -> complete leaf or category inventory
  -> trusted unmanaged proof
  -> complete reference catalogue and intended detachments
  -> post-remove route and generated projection
  -> one ordered complete mutation plan
  -> preflight
  -> dry-run or application
  -> verification and retained partial-state reporting
  -> one typed result
```

The plan includes every selected source/resource path, incoming-reference
detachment, affected generated region, expected state, verification condition,
and recovery requirement. Compatible changes to one physical file coalesce into
one complete-file effect. A category plan remains one operation even when it
contains many file effects; it is not independently committed per descendant.

One invalid, blocked, incomplete, conflicting, or unsafe required fact prevents
all effects. There is no partial, best-effort, silently narrowed, or
independently committed leaf application.

## Dry Run And Apply

`--dry-run` uses the same request, current facts, trusted ownership inventory,
complete category inventory, reference catalogue, intended detachments, generated
projection, plan, expected-state checks, and preflight as application. It shows
every removed, detached, and generated effect needed to review the complete
operation, then writes nothing. Planned changes alone do not create `attention`.

Omitting `--dry-run` selects application. The command path and the exact source
subject are sufficient consent in human, JSON, and other non-interactive use.
The command does not prompt for a second confirmation and does not accept
`--yes`.

The explicit consent covers only the selected leaf or complete category, its
incoming-link detachments, and generated projections in the complete plan. It
does not grant ownership, lifecycle, collision, containment, marker-repair, or
recovery bypass authority.

For an actual application, the complete plan checks every existing path it may
change or remove through ordinary workspace facts. It does not inspect or report
repository state.

Before the first target effect, orchestration uses only
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.Create)` and its application-owned
`OpenForge/recovery/v1` subtree. There is no temporary-directory, repository,
`HOME`, or custom-platform fallback; unavailable storage is a
pre-effect `incomplete` result. When the operation has one or more existing-target
effects (`Replace`, `ReplaceGeneratedRegion`, or `Delete`), it prepares exactly
one immutable ZIP bundle outside the workspace. An operation containing only Create effects or
no-ops creates no bundle. Its source-generated
schema-v1 `manifest.json` and streamed ordinal payload entries record
command/operation/workspace identity, ordered relative targets, change kinds,
exact prior bytes/lengths/hashes, and intended final absence or length/hash.
`Create` and semantic/byte no-op effects have no entry. A CreateNew draft is
closed and reopened for semantic manifest, exact ordered entry, length, hash,
and payload-byte validation, moved within the same directory to its deterministic
final name, and reopened and verified. Only the valid final ZIP forms the opaque
`RecoveryBundlePreparation`; the draft remains `Incomplete`.
`FileChangeApplier` requires the matching preparation for every existing-target effect
and performs one final effect per target. All preparation completes before the
first target effect; unknown, malformed, mismatched, or colliding bundles block.

After final verification, delete only the positively recognized bundle created
by this operation. `Deleted`/`Removed` permits normal completion.
`Failed`/positively observed `Retained` keeps target effects successful and
produces `attention`, the exact residual path, and
cleanup guidance. `Failed`/`Unknown` produces `failed` and reports an exact expected path only when the deletion result
provides one. Before post-verification deletion begins, a handled application,
verification, or cancellation outcome reports the actual residual draft or
final path; a valid final remains when preparation completed. A closed final ZIP may remain after
abrupt process termination, without an executable crash or power-loss guarantee.
Recovery provenance does not classify current target state, and no target is
restored automatically. Cleanup owns exact named final and draft deletion under
its separate lease-bound contract.

## Human Output

Human output comes from one typed result. The default expanded view includes the
workspace, selection method, selected subject kind, source identity, complete
effect summary, affected paths, reference coverage and every detachment,
generated projections, verification, bundle provenance, and retained partial-state
facts, and semantic status.
Compact view retains the identity, leaf/category kind, mode, status,
completeness and safety, every affected path, every detachment, every generated
effect, and at most one required `Next:` action.

Dry-run compact and expanded output still shows every exact planned effect or
bounded diff. A successful result does not name internal planning stages. The
primary human result for `complete`, `attention`, and `incomplete` goes to
stdout. The primary human result for `invalid`, `blocked`, `failed`, and
`interrupted` goes to stderr. Each primary result stays together on its assigned
stream. Bounded diagnostics use stderr.

### Successful leaf removal with detachment

```text
The routed leaf was removed.
Workspace: <workspace-path>
Selected by: current directory
Source: docs/old-guide
Detached references: 2
Updated generated regions: 1
```

Every detached occurrence is listed in the expanded result:

```text
Detached references
  .agents/README.md:18  [Old guide](docs/old-guide.md)  ->  Old guide
  docs/overview.md:7    [Guide](../.agents/docs/old-guide.md#intro)  ->  Guide
```

### Successful category removal

```text
The routed category was removed.
Category: .agents/guides/_guides.md
Removed items: 6
Detached references: 4
Updated generated regions: 2
```

### Dry run

```text
The routed category would be removed.
Category: .agents/guides/_guides.md

<complete removed-path, detachment, and generated-region effects>

No files changed (--dry-run).
```

The exact examples use illustrative paths. A complete result has no required
`Next:` action. Incomplete, invalid, and blocked results name the direct
correction when it is known. Failed and interrupted results identify retained
bundle/partial-state or retry guidance without inventing provenance.

## Structured Output

`--json` emits one complete structured result to stdout for every semantic status
from the same typed result used by human output. It never prompts and never
reruns resolution, planning, application, verification, or retained-state
reporting. Human text
is not mixed into JSON stdout; bounded diagnostics use stderr.

The structured result exposes the concrete command result under the exact shared
schema defined by the [CLI Architecture](../../../architecture.md):

- workspace and selection method;
- requested and resolved source identity and canonical path;
- selected subject kind, logical layers, and complete category item inventory;
- trusted Framework and Extension ownership-evidence state;
- reference catalogue coverage and every incoming detachment, including source
  location, original destination, visible label, and intended plain-text result;
- generated-region selection, projection, and bounded effect evidence;
- dry-run or application mode, completeness, safety, recovery-bundle facts;
- expected-state, changed, unchanged, and verified effect facts, plus any actual
  residual draft or final recovery path, without classifying current target state;
- application, verification, bundle provenance, and retained partial-state facts; and
- semantic status and at most one required `Next:` action.

The structured result retains every detachment even when a compact human view is
selected. It does not turn a category into an unexplained count or hide the
reference effect that makes surrounding prose safe.

## Semantic Results

| Result        | Meaning                                                                                                                                                                                                                                                                                                                               |
| ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `complete`    | A complete safe dry-run plan was established, or application and final verification completed, for a leaf or category removal. This includes every complete incoming-link detachment and generated effect. It also includes a verified no-op only when exact intended absence is independently proven with complete trusted evidence. |
| `attention`   | Post-verification recovery deletion returns `Failed` with positively observed disposition `Retained`; target effects remain successful with the exact residual path and cleanup guidance. Planned deletions and link detachments do not create it.                                                                                    |
| `incomplete`  | Safe identity and facts exist, but the complete supported-Markdown catalogue, reference pass, category inventory, or another required coverage boundary cannot be enumerated or inspected. No write begins.                                                                                                                           |
| `invalid`     | Command input, operand cardinality, source kind, flag use, or exact source reference does not follow this interface. A missing source without independent absence proof is not a verified no-op.                                                                                                                                      |
| `blocked`     | A valid request cannot establish one safe complete removal because ownership, lifecycle, route, identity, containment, collision, reference transformation, generated boundary, expected state, or recovery is unsafe or ambiguous. No write begins.                                                                                  |
| `failed`      | An unexpected application or verification failure occurs after a persistent effect begins, or recovery deletion returns `Failed`/`Unknown`; `Failed`/positively observed `Retained` recovery is the distinct `attention` case.                                                                                                        |
| `interrupted` | The caller cancels before completion; an unexpected application or verification failure remains `failed`.                                                                                                                                                                                                                             |

For ordinary conditions, status precedence is `blocked` > `incomplete` >
`attention` > `complete`. Invalid input stops before operation resolution.
Failed and interrupted preserve their event meanings. The shared numeric
process-status mapping is defined by the CLI Architecture.

## Verified No-Op And Honest Repeats

A repeated remove may be a verified no-op only when the operation independently
establishes all of the following for the exact requested subject:

- the intended subject is absent at its exact source identity and no replacement
  source has been adopted at that identity;
- the complete trusted ownership inventory establishes no selected ownership
  claim or orphan companion;
- current topology and affected generated projections establish the intended
  absence and contain no stale generated region;
- the complete supported-workspace-Markdown reference pass establishes no
  residual incoming reference to the absent subject.

The result is `complete` with verified no-op evidence only after all of those
facts are complete. A missing or invalid source reference without this evidence
is `invalid`; an orphan, ambiguous identity, or unsafe ownership fact is
`blocked`; and unavailable required coverage is `incomplete`. The command never
uses a receipt, tombstone, journal, or history record to claim that a previous
remove caused the absence.

## Errors And Boundaries

The command rejects or blocks:

- zero or several source operands;
- an unknown, missing, ambiguous, Loader, root, entrypoint, native, resource,
  orphan, or otherwise ineligible leaf subject;
- a category reference that is not exactly one recognized entrypoint or whose
  physically contained inventory is incomplete or unsafe;
- missing, malformed, stale, conflicting, or incomplete trusted ownership and
  lifecycle evidence;
- an incoming supported reference that cannot be safely detached, has an
  unsupported or ambiguous form, or would lose surrounding prose;
- incomplete supported-Markdown enumeration or inspection;
- an invalid or ambiguous generated boundary or required Index projection;
- unavailable recovery-bundle storage (`incomplete`); an unverified, malformed,
  mismatched, or colliding bundle (`blocked`); or
- a changed expected source, reference, generated region, ownership fact, or
  category item before application.

Every ordinary error names `route remove`, the affected subject or occurrence,
the direct cause, and one useful next action when one exists. The command does
not diagnose authoring quality, infer semantic intent, or silently leave a
supported broken link.

## Scenarios

Remove one ordinary leaf and detach exact incoming links:

```text
open-forge route remove \
  ".agents/docs/old-guide.md"
```

Remove one recognized category as one complete physical operation:

```text
open-forge route remove \
  ".agents/guides/_guides.md"
```

Preview all removal and detachment effects as structured output:

```text
open-forge route remove \
  guides \
  --dry-run \
  --json
```

The source operand in the last example must resolve to one eligible leaf or one
recognized category entrypoint. If the automatic ID is colliding, use the exact
recognized entrypoint path for the category or the exact source path for the
leaf.

## Non-Goals

`route remove` does not:

- remove or mutate the Loader or `.agents` workspace root;
- accept a generic directory operand, batch operands, repeated mutation
  requests, or independently committed descendant removals;
- remove a lifecycle-managed, generated-only, native-only, unsupported, or
  ambiguous subject;
- initialize or repair route parents, metadata, ownership, or lifecycle state;
- adopt, release, migrate, or repair Framework or Extension lifecycle state;
- rewrite references originating inside the removed subject, external URLs,
  unsupported or ambiguous link forms, or unrelated authored prose;
- delete an incoming link's surrounding prose or silently leave a supported
  incoming link broken;
- treat generated `Entries` as authored authority or run a hidden `index` command;
- create a receipt, tombstone, journal, saved plan, session, or automatic
  recovery history;
- create a Git commit; or
- redefine the shared libraries, parser boundary, physical identity, recovery
  bundle,
  lock, concurrency, test, Native AOT, or C# source-layout choices accepted by
  the CLI Architecture.

Use `index` for standalone generated navigation, `references` for read-only
direct reference facts, `doctor` for diagnosis, and the accepted lifecycle
operations for managed content. This remove contract owns only the exact
structural removal described here.

## Verification Requirements

Gate 5 executable proof must cover:

- exact source-ID and exact-path resolution, quoting, spaces, Unicode, collision,
  containment, and base/overwrite identity;
- one eligible ordinary leaf, one eligible recognized category, and rejection of
  Loader, workspace-root, entrypoint-as-leaf, native, unsupported, orphan, and
  ambiguous subjects;
- complete category physical inventory, descendant classification, and
  all-or-nothing planning;
- complete positive unmanaged proof from the Framework baseline and all
  applicable Extension claims, including every missing, malformed, conflicting,
  stale, incomplete, and source-claim case;
- complete supported Markdown catalogue coverage inside and outside `.agents`,
  base/overwrite layers, exact incoming-link resolution, visible-label
  detachment, surrounding-prose preservation, and external URL preservation;
- unsupported or ambiguous potentially applicable links, prose-loss
  transformations, unsafe identity, incomplete catalogue coverage, and their
  no-write results;
- old parent generated projection and Loader projection when applicable,
  generated-boundary preservation, and no hidden `index` invocation;
- one complete category plan and recovery boundary rather than independently
  committed leaf effects;
- exact dry-run/application parity, explicit-subject consent, no persistent
  dry-run effect, every detachment in human and JSON results, and complete
  effect visibility;
- recovery-bundle storage/readiness and collision handling, expected-state
  revalidation, all-effects verification, retained partial state without
  restoration, residual preservation, and fresh-plan rerun;
- complete verified no-op proof for exact intended absence and invalid,
  incomplete, or blocked results when that proof is unavailable;
- `complete`, reserved `attention`, `incomplete`, `invalid`, `blocked`, `failed`,
  and `interrupted` results; and
- human stream allocation, compact retention, every planned dry-run detachment,
  structured JSON parity, and no mixed human text in JSON stdout.

The proof must exercise the accepted CLI Architecture boundaries rather than
relying on source-level or managed-build claims. It must include the real
filesystem, workspace lock, recovery, Native AOT, and package/process evidence
required by that Architecture.

## Related Current Sources

- [route remove Interface Contract](interface.md)
- [route remove Command Contract Set](_remove.md)
- [Route group entrypoint](../_route.md)
- [Index Behavior Contract](../../index-candidate/behavior.md)
- [Index Interface Contract](../../index-candidate/interface.md)
- [CLI Architecture](../../../architecture.md)
- [Global CLI Flags Behavior Contract](../../shared/global-flags/behavior.md)
- [CLI Source References Behavior Contract](../../shared/source-references/behavior.md)
- [References Behavior Contract](../../references-candidate/behavior.md)
- [Doctor Behavior Contract](../../doctor/behavior.md)
- [Route Update Behavior Contract](../update/behavior.md)
- [CLI Command Contract Set — Interface Contract](../../../command-contract-set.md#interface-contract)
- [Historical CLI Decision Agenda](../../../../../../archived/cli-release/decision-agenda-2026-08-21.md)
- [Historical CLI Release Plan](../../../../../../archived/cli-release/release-plan-2026-08-21.md)
- [Shared CLI Operation Contract](../../../shared-operation-contract.md)
- [Routing Model](../../../../framework/routing/model.md)
- [Routing Paths And Identity](../../../../framework/routing/paths.md)
- [Overwrite Customization](../../../../framework/routing/overwrites.md)
- [Routed Markdown Representation](../../../../framework/markdown/routes.md)
- [Markdown Compatibility Boundary](../../../../framework/markdown/compatibility.md)
