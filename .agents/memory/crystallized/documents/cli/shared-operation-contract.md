---
open-forge:
  description: Accepted current shared operation contract for the non-shipping Open Forge CLI
  responsibility: Define cross-command input, modifier, execution, result, safety, and locality boundaries without choosing implementation
  tags: [Memory, Crystallized, CLI, Release, Document, Evergreen, CurrentTruth, Contract, Operation, Shared, Interface, Behavior, Determinism, Output, Safety, Locality]
---

# Shared CLI Operation Contract

## Status And Authority

This is the accepted current `#Evergreen` Crystallized Document/Contract for the
shared operation shape of the replacement Open Forge CLI. It is the authoritative
current source for the cross-command conventions stated here: command and group
shape, input roles, modifier composition, typed operation flow, semantic
results, output streams, deterministic no-ops, recovery boundaries, and source
locality.

The [Command Contract Set](command-contract-set.md) defines the roles, topology,
and authority boundaries for those local contracts. The command-local Interface
and Behavior Contracts remain authoritative for each command's exact syntax,
subjects, finite conditions, effects, and result facts. The shared [Result
Coordinates](contracts/shared/result-coordinates/_result-coordinates.md) define
the exact public envelope, source locations, statuses, exits, streams, and
compatibility used by those results.

This Contract does not replace or duplicate those definitions. It is a
Crystallized Document/Contract, not a Pattern, Directive, Architecture, or
implementation design. The replacement does not ship yet. The accepted [CLI
Architecture](architecture.md) controls high-level implementation choices while
the routed [Technical Designs](technical-designs/_technical-designs.md) define
exact shared-capability realization. This Contract remains technology-neutral
and chooses no library, module, storage path, archive format, lock API, or file
application mechanism.

The [generic CLI Operation Pattern](../../../../patterns/open-forge/cli/composable-operation.md)
is optional reusable shape and context. It is not authority for current Open
Forge command semantics. This Contract and the command-local contracts control
the accepted Open Forge behavior.

## Command And Group Shape

The current Open Forge command forms are:

```text
open-forge <direct-command> [operands] [flags]
open-forge <group> <operation> [operands] [flags]
```

Use a direct root command for one stable operation. Use a group only when
several actual operations share a coherent subject and lifecycle. A group with
one actual child is ceremonial and adds no useful operation boundary. Keep
ordinary command paths shallow, normally one or two words after `open-forge`.

The current direct roots are `find`, `index`, `status`, `context`,
`references`, `doctor`, `repair`, `install`, `update`, and `cleanup`. The
current grouped families are:

- `route` with `inspect`, `list`, `init`, `create`, `update`, `move`, and
  `remove` operations.
- `extension` with `list`, `inspect`, `create`, `install`, `update`, and
  `remove` operations.

The [Command Contract Set](command-contract-set.md) records the current roles,
placement, authority boundaries, and detailed local contract links. A group
performs no domain operation or wizard by itself. Its bare form shows help for
its child operations. Every leaf command performs one complete operation. A flag
may select input, add compatible behavior, choose output, or set write policy,
but it must not turn a leaf into another operation.

## Command Input Roles

The command path selects the operation. Positional operands provide its primary
subjects or query values:

```text
open-forge context [source-reference...]
open-forge route move <source-reference> <destination-target>
```

A leaf may require one or more operands when its complete operation needs a
subject or query. Do not require a value flag merely to choose what the command
does. When a leaf already selects one complete filtering operation, its typed
predicate flags narrow that operation rather than selecting another operation.

Typed predicate flags are different when the leaf already performs one complete
filtering operation and its operand-free form has a documented result:

```text
open-forge find
open-forge find --tag=Architecture --heading=Axioms
```

Here `find` selects one source-discovery operation. The first form returns its
unfiltered source inventory. Each typed predicate flag narrows that same
inventory by a named field, and compatible predicate flags compose without
changing the operation. Use a named value flag when positional ordering would
otherwise make different operand roles ambiguous, and document that exception.

Treat typed predicate flags as Selection flags that narrow results within the
same operation. Value flags remain appropriate for non-operation choices,
including workspace selection, optional authored metadata, typed predicates,
output projection, input location, combination behavior, and write policy. A
typed value keeps its default, composition, and future compatible values
explicit.

A required `--mode`, `--kind`, `--action`, `--by`, or similar discriminator is a
review signal that the leaf may be underspecified. Move materially different
operations into child operations when they have distinct intent, validation,
effects, or result meaning. Do not split one operation merely because an
optional flag changes one compatible dimension of it.

## Guided Leaves And Automatic Selection

A wizard-capable leaf normally exposes its wizard from its simplest useful
interactive invocation, usually without operands or operation-specific flags.
If its primary subject cannot be safely and finitely enumerated, the subject
remains required instead of being invented by a wizard.

Wizard answers, positional operands, and operation-specific flags populate the
same typed request. Explicit inputs add to or narrow that request
incrementally. Conflicting explicit inputs are invalid. The operation does not
create hidden precedence, disjunctive modes, or a recommendation that is
selected merely because it was displayed. A recommendation may be emphasized
for review, but the user or an explicit input must supply its authority.

JSON and other non-interactive modes never prompt. Missing semantic input is
invalid. Missing authority or an unresolved choice is blocked.

An operation-specific `--automatic` flag may suppress a wizard and select only
the documented explicit inputs plus deterministic automatic selections and
defaults for that operation. An unresolved semantic choice remains blocked.
`--automatic` is not global. It never accepts a recommendation, replaces
divergent content, adds replacement or deletion authority, adopts or takes
ownership, bypasses safety checks or conflicts, or makes a fuzzy choice.
It may execute ordinary safe effects already authorized by the explicit
operation and subjects, including a safe unchanged final-owner deletion when
that operation defines it. It never selects deletion of changed content on its
own. Repeating it is idempotent. A leaf must explicitly apply this flag;
read-only and unrelated commands reject it.

## Modifier Roles And Stable Meanings

Classify each flag before adding it:

| Role         | Purpose                                                                                  | Current examples                                                    |
| ------------ | ---------------------------------------------------------------------------------------- | ------------------------------------------------------------------- |
| Global       | Same meaning across the CLI                                                              | `--workspace`, `--json`, `--view`, `--verbose`                      |
| Selection    | Adds or narrows explicit input or results within the same operation                      | `--tag`, `--follow-links`, `--additions-only`                       |
| Guided input | Uses explicit input plus documented deterministic automatic selections without prompting | `--automatic`                                                       |
| Projection   | Chooses which parts of one result are shown                                              | `--content`                                                         |
| Write policy | Controls preview                                                                         | `--dry-run`                                                         |
| Authority    | Widens one explicitly named owned or replacement boundary within the same operation      | `--force` only where its local contract defines the complete effect |

Flags in one role compose when their meanings do not conflict. Prefer one
typed projection flag with named parts over several mutually exclusive Boolean
flags. A typed projection may accept named parts, for example:

```text
--content=frontmatter,body,section:Axioms
```

Use separate operands for repeated primary subjects:

```text
open-forge context route-a route-b
```

Do not hide several subjects in a comma-separated flag when each value needs
independent parsing, completion, validation, and errors. Do not add a Boolean
flag for one member when the underlying dimension has several compatible named
values.

When two flags represent incompatible operations rather than compatible choices,
split the operation into separate commands. Do not resolve that conflict through
precedence, terminal state, or workspace state. Prefer one typed composable flag
for a multi-value dimension rather than one-off Boolean flags for individual
members.

An Authority flag is distinct from write policy and from a safety bypass. It may
remain on one leaf only when the subject, intended operation, current-fact
resolution, plan, safety boundaries, result vocabulary, and verification and
recovery lifecycle remain the same while the flag widens exactly one explicit
owned or replacement boundary. The command's local contract must define that
boundary completely. `--force` is an example, not a generic meaning.

An Authority flag must be idempotent. It must not imply a safety bypass, deletion
outside its exact boundary, adoption or ownership, conflict or identity bypass,
containment or marker bypass, confirmation unrelated to its named authority, or
weaker verification or recovery-bundle handling. If it changes the job, subject, validation,
effect family, result meaning, or recovery lifecycle, split it into another
operation.

Define each global flag once. Every valid command path accepts a well-formed
global flag with the same meaning. A global flag is an explicit no-op when its
meaning does not apply to the selected operation. Reuse a flag name only when
its full meaning stays the same. Keep authority-bearing flags fully spelled and
separate from ordinary confirmation.

A safety bypass must not grant permission to overwrite, delete, force an
operation, or take ownership. A bounded Authority flag must not grant any
safety bypass or unrelated replacement, deletion, adoption, ownership,
conflict, identity, containment, marker, verification, or recovery authority.

Omitted input may use one documented deterministic default. It must not be
inferred from filesystem coincidence. Explicit workspace selection uses the
exact current directory or exact `--workspace` value; it never searches for
another root.

### `--dry-run`

`--dry-run` is the sole preview spelling. It uses the same typed request,
current facts, planner, and preflight as application, includes every selected
automatic and explicit effect, and writes nothing. Human output may call the
mode Preview. Do not add `--preview`, `--suggestions`, a `plan` operation, a
saved plan, or a generic `apply` operation.

Dry-run forms the same pre-effect planning status as application. Because it
performs no effects, it never produces an apply-time `failed` or `interrupted`
result. Planning or read failures and caller cancellation before effects retain
their own event meaning. Dry-run and application use the same status
conditions; planned changes alone do not create `attention`.

## Repetition, Results, And Streams

Keep repetition rules structural and explicit:

- A singleton one-value input rejects repetition, including repetition with an
  equal value, unless the command explicitly declares that input multi-value.
- An explicitly multi-value flag may repeat and combines under its command-local
  order, duplicate, and deduplication rules. Do not add last-wins behavior by
  convention.
- An applicable command-specific Boolean such as `--dry-run` may repeat
  idempotently. Repetition does not create another operation or multiply
  authority.

Use the same seven semantic statuses across operations:
`complete`, `attention`, `incomplete`, `invalid`, `blocked`, `failed`, and
`interrupted`. A command defines whether `attention` applies and what finite
condition forms it. For ordinary safety and coverage conditions, use
`blocked` > `incomplete` > `attention` > `complete`. Invalid input stops before
operation resolution. Failed and interrupted preserve their event meaning.

Primary human `complete`, `attention`, and `incomplete` results go to stdout.
Primary human `invalid`, `blocked`, `failed`, and `interrupted` results go to
stderr. JSON renders one complete result from the same typed result to stdout
for every semantic status. Bounded diagnostics use stderr. Compact and
structured results retain at most one required `Next:` action when one is
needed. When `Failed`/positively observed `Retained` recovery attention coexists
with another command-local attention condition, exact recovery cleanup guidance
owns that single action; the other attention facts remain visible evidence.

The [Global CLI Flags contract](contracts/shared/global-flags/interface.md)
remains the detailed owner for JSON, `--view`, and `--verbose`. Each command
contract remains the detailed owner for its local finite conditions and result
facts. This shared status and stream rule does not create a second command
schema. The [Result Coordinates Interface
Contract](contracts/shared/result-coordinates/interface.md) defines the exact
numeric exits, envelope, source-location coordinates, primary streams, and
compatibility.

## Typed Operation Flow

Keep the accepted operation stages visible and directly testable:

```text
validated command input
  -> complete operation request
  -> current facts
  -> read result or complete mutation plan
  -> preflight when mutating
  -> preview or application consent
  -> revalidation and apply
  -> verification and recovery-disposition reporting
  -> optional bounded post-processing
  -> typed operation result
  -> human or structured rendering
  -> process completion
```

Read-only operations stop after producing their result. They do not create an
empty mutation plan. Dry-run and application use the same request, fact
inspection, planner, and preflight. Dry-run stops before effects. Changing
intent discards the old plan and starts planning again.

Handlers return typed values. They do not write streams, select presentation,
or set process status. Human and JSON renderers consume the same result and do
not rerun the operation.

### Recovery And Cleanup Boundaries

The direct root [`cleanup` contract](contracts/cleanup/_cleanup.md) is a narrow
exception to the normal recovery shape. Other mutating operations prepare one
external recovery bundle for the complete operation before the first existing-
target effect. Cleanup still forms one complete catalogue and plan, runs
preflight, and revalidates selected artifacts immediately before deletion. It
may return a verified empty no-op without a lease. Before any deletion, it
acquires the same-workspace lease and repeats final catalogue and expected-state
validation under that lease. It creates no replacement bundle, staging copy,
journal, or tombstone merely to delete eligible cleanup artifacts, and it does
not reverse a deletion that it has verified.

Already verified deletions remain desired effects when a later deletion fails or
the caller interrupts. Remaining and residual facts stay visible for a fresh
plan. This exception applies only to cleanup. Other mutating operations retain
their accepted bundle-preparation and post-verification disposition rules.

Recovery writing and observation remain distinct. Only a mutation bundle writer
may create application-owned recovery storage during pre-effect preparation.
Status, Doctor, and Cleanup observe without creating storage. An absent store is
zero recognized bundles or drafts for Status and Doctor and a verified Cleanup
no-op. An unreadable selected-workspace catalogue remains unavailable. A final
bundle that cannot be validated retains its exact malformed, unsupported, or
unavailable condition. Neither condition is absence.

Status and Doctor do not acquire the workspace lease, report activity, or infer
activity from bundle contents, a filename, age, PID, marker, journal, or the
persistent external lock file.

Before applying an existing-target effect (`Replace`, `ReplaceGeneratedRegion`,
or `Delete`), the complete operation prepares exactly one immutable verified
bundle outside the workspace. The bundle contains the exact prior bytes and
static prior and intended identity for every existing-target effect. Create and
semantic or byte no-op targets have no bundle entry. An operation containing
only those effects creates no bundle. Unavailable required storage forms a
pre-effect incomplete result. Unknown, malformed, mismatched, or colliding
artifacts do not authorize an effect.

All bundle preparation and verification complete before the first target effect.
A draft never authorizes an effect. Every existing-target effect requires the
matching verified final preparation. Create requires no preparation. Exact
store, ZIP, manifest, draft/final, bounded-validation, and callable mechanics
live in the [Mutation And Recovery Technical
Design](technical-designs/mutation-and-recovery.md).

Before post-verification deletion begins, handled application, verification, or
cancellation outcomes stop new effects and report the actual residual draft or
final path; a valid final bundle remains when preparation completed.

After whole-command verification succeeds, the command deletes only its
positively recognized bundle. `Deleted` with disposition `Removed` permits
normal completion. `Failed` with positively observed disposition `Retained`
keeps target effects successful and produces `attention` with the exact residual
path and cleanup guidance. `Failed` with disposition `Unknown` produces `failed`
and reports an exact expected path only when the deletion result provides one.
`Blocked` and `Cancelled`, with either `Retained` or `Unknown`, remain neutral
typed event facts for command-local mapping; disposition alone never selects a
command status. A closed final artifact may remain after an abrupt process
termination, but the CLI provides no executable crash or power-loss durability
guarantee. Shared support never
automatically restores a target, rolls back an effect, or compensates for target
effects, classifies current target state from recovery provenance, or saves a
journal, progress receipt, history, or replayable plan. A fresh invocation plans
from current facts.

Bundles are immutable after preparation and are never extracted by the CLI.
Status, Doctor, and Cleanup may inspect payload data only through bounded
validation. They never extract, disclose, render, log, return, retain, or
materialize payload bytes. Exact archive, hashing, and bounded-reading mechanics
belong to the [Mutation And Recovery Technical
Design](technical-designs/mutation-and-recovery.md).

Cleanup mechanically deletes only exact named final or draft candidates for the
selected workspace after it acquires the same-workspace lease and repeats the
complete catalogue and ordinary path/kind checks under that lease, then verifies
absence. The held lease provides cooperating-process exclusion only. If Cleanup
cannot acquire it, Cleanup performs no deletion. Unknown,
malformed, mismatched, differently keyed, or lookalike artifacts remain
untouched. Same-path normalized physical workspace identity is required for
deterministic rediscovery; workspace moves are outside the automatic guarantee,
though Doctor or Cleanup may report orphaned original-root bundles and never
auto-binds or restores them. Cleanup writes no marker, PID, journal, lock
metadata, or other lifecycle record and makes no activity inference.

The mutation lock is a persistent reusable external coordination artifact with
no activity metadata. Its existence does not demonstrate ownership or activity,
and read-only observation never acquires it. Composition and terminal no-effect
flows do not create recovery or lock infrastructure. Exact location, identity,
handle, and lifetime mechanics belong to the [Mutation And Recovery Technical
Design](technical-designs/mutation-and-recovery.md).

Standalone Extension Create uses a separate exact-destination, collision, and
revalidation path with no workspace lease, none of `Replace`,
`ReplaceGeneratedRegion`, or `Delete`, and no recovery bundle.

Cleanup's operand-free catalogue is explicit command intent, not an automatic
interaction policy. Invoking the dedicated command explicitly selects deletion
of all positively recognized eligible cleanup artifacts under its local
contract. An `--automatic` interaction policy or inferred recommendation never
adds authority to delete or replace content.

## Source Locality And Contract Boundaries

Keep one command's syntax, request resolver, handler, planner, renderer, and
direct tests near that command. Move parsing, graph, mutation, result, or
display support upward only after real sibling commands share it. The permanent
shared contract route is [`contracts/shared/`](contracts/shared/_shared.md), but
a shared location does not make a command-local meaning global.

Keep operation meaning visible and directly testable. Shared mechanisms must not
hide the operation's subjects, authority, stages, effects, or result semantics.
This is a visibility and locality boundary, not a choice of modules,
dependencies, libraries, dispatch, or runtime. Prefer direct typed relationships
and exhaustive handling of finite cases where they keep the contract visible.
Do not use string-keyed behavior registries, service locators, reflective
dispatch, or a universal operation engine that hides command meaning. The
accepted Architecture controls concrete implementation choices; Gate 5 still
owes implementation and evidence.

## Errors And No-Ops

Every error states the operation, affected subject, cause, and useful next
action. Verbose output may add diagnostic evidence but must not be required to
understand an ordinary failure.

An idempotent write converges on one state. Repeating it against that state
returns a verified no-op. Do not invent an effect to represent unchanged state.
An operation may report a verified no-op only after its local contract has
established the complete facts needed to prove that no effect is required.

## Current Examples And Related Authority

The active [`context` contract set](contracts/context/_context.md) applies this
Contract through explicit route operands, composable selection and content
projection, one typed result, and no session state.

The active [`find` contract set](contracts/find/_find.md) applies the
typed-predicate exception through one meaningful bare inventory, composable tag
and heading filters, one flat requirement, independent region and content
selection, and one typed result.

The accepted [`install` Interface Contract](contracts/install/interface.md) and
root [`update` Interface Contract](contracts/update/interface.md) demonstrate
bounded composition:

```text
open-forge install --force --dry-run
```

Here `--force` is an Authority flag and `--dry-run` is a Write policy flag for
the same leaf. On `install`, force is limited to an eligible exact initial
occupant. It does not reconcile managed divergence. The Install Interface owns
that complete local meaning; this example does not define a generic effect for
either flag.

The separate managed-reconciliation composition is:

```text
open-forge update --force --prune --automatic --dry-run
```

Here `--force` widens current expected-footprint replacement, `--prune` widens
retired-content deletion, `--automatic` suppresses interaction without adding
authority, and `--dry-run` previews the same complete plan. The Update Interface
owns these boundaries; the flags do not form a generic lifecycle mode.

The accepted [`extension` contract set](contracts/extension/_extension.md) applies
the same request shape while keeping source, ownership, and package boundaries
explicit:

```text
open-forge extension install development-toolkit --automatic --dry-run
open-forge extension update development-toolkit --force --prune --dry-run
open-forge extension remove development-toolkit --prune --automatic --dry-run
```

The Extension contracts own the exact source universe, package identity,
dependency, ownership-release, and same-request prune meaning. `--automatic`
does not select packages, replace divergence, adopt content, or delete changed
content on its own. Read-only `extension list` and `extension inspect` reject
`--automatic` and mutation flags. `extension create` treats global
`--workspace` as a no-op because `--path` names its catalogue destination.

## Review Checks

- The command performs one complete operation.
- A direct root represents one stable operation. A group represents several
  actual coherent operations and is not ceremonial.
- The command path is shallow, and the current direct-root and grouped-family
  conventions remain visible in help.
- Primary subjects are explicit operands unless a command owns a complete,
  safely bounded operand-free catalogue. Cleanup's default-all catalogue is the
  accepted mutation exception; it never becomes arbitrary path discovery or a
  generic batch surface.
- Primary subjects and query values that do not name a predicate field are
  operands rather than required discriminator flags.
- A typed predicate narrows one complete filtering operation whose form without
  operands has a documented result. It does not supply a missing operation kind.
- Every flag has one named role and stable meaning. Compatible modifiers compose
  without hidden precedence; incompatible operations are separate commands.
- An Authority flag widens exactly one explicit owned or replacement boundary
  without changing the subject, plan, safety, result, verification, or recovery
  lifecycle. It is idempotent and does not imply a safety bypass or unrelated
  authority.
- Singleton and explicitly multi-value repetition rules are explicit, and
  applicable Boolean repetition is idempotent.
- A group performs no operation or wizard, and its bare form shows help.
- A wizard-capable leaf exposes its simplest useful interactive invocation when
  its primary subject can be safely and finitely enumerated; otherwise the
  subject remains explicit.
- Wizard answers and explicit inputs populate one typed request without hidden
  precedence, disjunctive modes, or automatic recommendation selection.
- JSON and other non-interactive modes never prompt.
- Operation-specific `--automatic` is explicit, deterministic, idempotent, and
  cannot grant replacement, deletion, ownership, bypass, or fuzzy-choice
  authority. Automatic selection never deletes or replaces content on its own.
- `--dry-run` is the sole preview spelling and shares planning and preflight
  with application while writing nothing. It shares status conditions, and
  planned changes alone do not create `attention`.
- One verified immutable external recovery bundle covers every
  existing-target effect (`Replace`, `ReplaceGeneratedRegion`, or `Delete`) in
  the complete operation before the first target effect; creates and semantic or
  byte no-ops receive none.
- Preparation produces one verified final bundle before any existing-target
  effect. A draft never authorizes an effect, and every existing-target effect
  requires the matching verified preparation; Create requires none. Exact
  archive, draft/final, atomic-file, callable, and validation mechanics belong
  to the [Mutation And Recovery Technical
  Design](technical-designs/mutation-and-recovery.md).
- Successful commands delete their command-owned bundle only after whole-command
  verification. `Deleted`/`Removed` permits normal completion;
  `Failed`/positively observed `Retained` preserves successful target effects
  with `attention`, the exact residual path, and cleanup guidance; and
  `Failed`/`Unknown` produces `failed` and reports an exact path only when the
  deletion result supplies one.
  `Blocked` and `Cancelled`, with either `Retained` or `Unknown`, remain neutral
  typed event facts for command-local mapping.
  Before post-verification deletion, handled application, verification, or
  cancellation outcomes report the actual residual draft or final path. A closed
  final may remain after abrupt process termination without a crash- or
  power-loss-durability guarantee. The foundation never automatically restores a
  target, rolls back an effect, or compensates for target effects, and it does
  not save a journal, progress receipt, history, or replayable plan.
- Cleanup deletes only exact named selected-workspace final bundles or drafts
  under its nonrecursive support-artifact exception. Before deletion it holds
  the same-workspace lease, repeats catalogue and expected-state validation, and
  revalidates each candidate immediately before deletion; contention prevents
  all deletion. Unknown,
  malformed, mismatched, or differently keyed artifacts remain untouched. The
  persistent external lock carries no activity metadata.
- Status, Doctor, and Cleanup use bounded validation and never extract, disclose,
  retain, or materialize payload bytes. Status and Doctor neither report nor
  infer activity.
- Read-only and mutating authority remain separate.
- Important stages return typed results and can be tested directly.
- Human and structured output use one operation result. The seven statuses,
  ordinary precedence, stream assignment, bounded diagnostics, and one-result
  JSON rule are uniform; finite `attention` conditions remain command-local.
- Shared code reflects demonstrated reuse without hiding command meaning.
- Repeating unchanged input produces the same semantic result and, when the
  local contract permits proof, a verified no-op.
- Errors remain useful without verbose mode.
