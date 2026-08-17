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
subjects, finite conditions, effects, and result facts. This Contract does not
replace or duplicate those local definitions. It is a Crystallized
Document/Contract, not a Pattern, Directive, Architecture, or implementation
design. The replacement does not ship yet. The accepted [CLI
Architecture](architecture.md) controls high-level implementation choices while
this Contract remains technology-neutral. It does not choose libraries, modules,
filesystem mechanics, or other implementation details, and it does not weaken
the Architecture's fixed result, lifecycle, recovery, or AOT boundaries.

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
ownership, bypasses safety, Git checks, or conflicts, or makes a fuzzy choice.
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
| Write policy | Controls preview or one named safety boundary                                            | `--dry-run`, `--skip-git-check`                                     |
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

An Authority flag must be idempotent. It must not imply Git bypass, deletion
outside its exact boundary, adoption or ownership, conflict or identity bypass,
containment or marker bypass, confirmation unrelated to its named authority, or
weaker verification or recovery. If it changes the job, subject, validation,
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

### `--skip-git-check`

When a mutating command exposes `--skip-git-check`, it is an optional Boolean
write-policy flag. Omission keeps the relevant or affected-path Git cleanliness
check. Presence bypasses only that check for actual application. Repetition is
accepted and idempotent. It does not multiply a bypass or grant overwrite,
delete, force, adoption, ownership, containment, marker, expected-state,
verification, or recovery authority. The command's accepted recovery
requirements remain in force. It composes with `--dry-run`; the combination
still writes nothing. Read-only and unrelated commands reject it.

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
- An applicable command-specific Boolean such as `--dry-run` or
  `--skip-git-check` may repeat idempotently. Repetition does not create another
  operation, multiply authority, or widen a bypass.

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
needed.

The [Global CLI Flags contract](contracts/shared/global-flags/interface.md)
remains the detailed owner for JSON, `--view`, and `--verbose`. Each command
contract remains the detailed owner for its local finite conditions and result
facts. This shared status and stream rule does not create a second command
schema.

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
  -> verification or recovery
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

### Cleanup's Narrow Monotonic Exception

The direct root [`cleanup` contract](contracts/cleanup/_cleanup.md) is a narrow
exception to the normal reverse-recovery shape. Cleanup still forms one complete
catalogue and plan, runs preflight, and revalidates each selected artifact
immediately before deletion. It does not create an adjacent backup, staging
copy, receipt, journal, or tombstone merely to delete eligible cleanup
artifacts, and it does not reverse a deletion that it has verified.

Already verified deletions remain desired effects when a later deletion fails or
the caller interrupts. Remaining and residual facts stay visible for a fresh
plan. This exception applies only to cleanup. Other mutating operations retain
their accepted backup, reverse-recovery, and residual-preservation rules.

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
  lifecycle. It is idempotent and does not imply a Git bypass or unrelated
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
- `--skip-git-check` bypasses only the relevant or affected-path Git cleanliness
  check and does not weaken other safety or recovery requirements.
- Read-only and mutating authority remain separate.
- Important stages return typed results and can be tested directly.
- Human and structured output use one operation result. The seven statuses,
  ordinary precedence, stream assignment, bounded diagnostics, and one-result
  JSON rule are uniform; finite `attention` conditions remain command-local.
- Shared code reflects demonstrated reuse without hiding command meaning.
- Repeating unchanged input produces the same semantic result and, when the
  local contract permits proof, a verified no-op.
- Errors remain useful without verbose mode.
