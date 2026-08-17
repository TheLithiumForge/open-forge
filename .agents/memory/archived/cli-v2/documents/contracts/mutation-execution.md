---
open-forge:
  description: Historical CLI-v2 source: Shared request, planning, preflight, effect, application, verification, recovery, and evidence semantics for every replacement CLI mutation
  responsibility: Define the stable semantic mutation contract while leaving exact TypeScript declarations to production source
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Mutation Execution Contract

## Scope

This document owns shared mutation semantics. The [CLI interface](../interface.md) owns public commands, flags, statuses, and the result envelope. The [request construction contract](request-construction.md) owns how validated explicit or guided input becomes the complete request. The [planned-mutation Pattern](../../../../../patterns/open-forge/cli/filesystem/planned-mutation.md) owns the reusable implementation shape.

Once implementation exists, production TypeScript owns exact interfaces, discriminated unions, const objects, and import paths. Conformance tests prove that source behavior satisfies this contract. Documentation does not retain copied production declarations.

The [filesystem effect contract](filesystem-effects.md) owns the containment, identity, concurrency, and mechanical persistence boundary.

## Guarantees

### Common Flow

Every mutating leaf follows one flow:

```text
operation-specific request
  -> inspect current facts
  -> build complete ordered plan
  -> preflight complete plan and every effect
  -> preview or confirm
  -> revalidate complete plan
  -> apply and verify each effect
  -> verify complete operation
  -> recover on interruption or failure
  -> optional exact-file formatter post-processing
  -> typed operation result and post-processing evidence
```

A read-only command does not create an empty mutation plan.

`--dry-run` selects preview policy around the same request. It returns after successful complete preflight and never produces an authorization token that another invocation may apply.

Every transition exposes its real typed alternatives. Coordinators call direct imported functions and exhaustively branch on named discriminants. Raw strings never resolve stages, executors, or dependencies.

### Contract Values

#### Request And Policy

The request is operation-specific and expresses only selected intent. Application policy is separate:

- Preview or apply.
- Interactive confirmation or accepted documented defaults.
- Git readiness and explicit Git-check bypass policy.
- Cancellation signal and immutable execution facts.

Raw arguments, terminal state, presentation selection, and mutable global context are not request fields.

Plan confirmation follows complete preflight. It consumes a safe plan
projection and returns a named accepted, cancelled, or blocked outcome. It
cannot alter the request or effects. A changed semantic choice restarts request
construction and planning.

#### Plan

A plan is an immutable ordered sequence of every operation-owned persistent
effect plus useful unchanged observations. Optional formatter post-processing
is outside this sequence. The plan contains enough internal information to
apply, verify, and recover its own effects without rediscovery.

Every effect defines:

- A deterministic identity unique within the plan.
- One named mechanical effect kind.
- One operation-owned typed purpose.
- A safe logical target and an internal physical target.
- Exact expected current state.
- Exact intended next state.
- Authority and protected-boundary requirements.
- Verification expectations.
- In-process recovery, Git, Gitless backup, or reconstructable expectations.

Effect identity is invocation evidence, not a permanent cross-version identifier. Array order is execution order. Do not add a dependency graph until an operation cannot express safe ordering as a sequence.

Unchanged state is an observation, not an effect.

#### Mechanical Effect Kinds

The initial shared executor needs only:

| Kind               | Persistent transition                               |
| ------------------ | --------------------------------------------------- |
| File creation      | Absent target becomes one exact regular file        |
| File replacement   | One verified regular file becomes exact next bytes  |
| File deletion      | One verified authorized regular file becomes absent |
| Directory creation | Absent target becomes one exact directory           |

Route entries, receipts, and shell-profile markers are typed purposes and authority rules over those mechanical transitions. They do not require duplicate write implementations.

A new effect kind is accepted only after an operation proves that these transitions cannot preserve its semantics safely.

#### Preflighted Plan

Successful preflight produces a distinct preflighted value. Application cannot accept a raw plan or a plan carrying a boolean such as `preflightPassed`.

Preflight is read-only and all-or-nothing. It validates:

- Unique identities, ordering, portable aliases, and cross-effect collisions.
- Logical and physical containment.
- Target kind, identity, expected state, and ownership.
- Protected authored, managed, generated, and external boundaries.
- Permission, reviewed external-source authority, and source stability needed
  for application.
- Verification, Git readiness, and recovery readiness for every effect.

One failure blocks the complete plan before mutation.

#### Existing-Target Authorization

Preservation remains the default. An operation may classify an existing
ordinary-file target as eligible for replacement only through its explicit
ownership contract. The plan exposes the target, current ownership,
advisory checksums, intended next state, and material future lifecycle consequences
before authority is collected.

Interactive execution may collect approval for the complete eligible
replacement set. Deterministic execution uses `--overwrite`. Both authorize
the same already selected set. Neither mechanism broadens selection, makes an
unsafe or protected target eligible, bypasses containment, resolves
incompatible manager ownership, or selects another lifecycle operation.

One eligible collision without approval blocks complete preflight. Equal bytes
may remove the need for a content effect, but transferring future update or
removal authority remains explicit. Generated navigation is derived state and
never consumes overwrite authority merely because its expected bytes differ.

#### Applied State

Application records what actually happened for every started effect. Internal applied state retains exact identities and rollback material; it is not reconstructed from public output.

Before the first effect, application revalidates the complete plan. Immediately before each effect, it revalidates volatile facts for that effect.

Each effect uses its accepted atomic or exclusive primitive and is verified immediately. After all effects pass, the operation runs one final semantic verifier.

### Safe Failure And Recovery

The default is preservation:

- Ambiguous intent, authority, ownership, containment, or recovery readiness blocks before mutation.
- Divergent authored or unowned content remains unchanged.
- No implicit best-effort or partial-success mode exists.
- Application or verification failure stops new effects and automatically recovers applied effects in reverse order.
- Cancellation before application changes nothing.
- Cancellation during application stops new effects and recovers applied effects.
- Recovery restores only a target that still matches the applied identity. A concurrent unexpected edit is preserved and reported as residual state.
- Recovery completion is verified.

An application or verification failure remains `failed` even when recovery succeeds because the selected operation did not complete. Cancellation remains `cancelled` when recovery succeeds. Incomplete recovery is always `failed`.

Deterministic rerun is recovery only for explicitly reconstructable derived
state. Hard-stop recovery for authored, lifecycle, and destructive workspace
effects relies on the clean Git starting point or explicitly accepted Gitless
sibling backups. The CLI does not claim later-invocation journal recovery.

### Shared Outcome Vocabulary

Exact strings live once in source as named const objects or enums. The minimum shared meanings are:

| Dimension          | Values                                |
| ------------------ | ------------------------------------- |
| Mode               | Preview, apply                        |
| Preflight          | Passed, blocked                       |
| Application        | Not started, complete, stopped        |
| Verification       | Not run, passed, failed               |
| Recovery           | Not required, complete, incomplete    |
| Final effect state | Planned, verified, reverted, residual |

The operation result status remains the complete semantic outcome. These dimensions provide mutation evidence and do not independently choose the process exit value.

### Public Mutation Evidence

#### Projection Shape

After an operation produces a complete plan, its focused result data composes
one required mutation projection with these fields:

| Field          | Meaning                                                                            |
| -------------- | ---------------------------------------------------------------------------------- |
| `mode`         | Whether this invocation previewed or attempted application                         |
| `preflight`    | Whether the complete plan passed or was blocked before mutation                    |
| `application`  | Whether application never started, completed its effect sequence, or stopped early |
| `verification` | Whether final semantic verification did not run, passed, or failed                 |
| `recovery`     | Whether recovery was unnecessary, completed, or left residual state                |
| `effects`      | Every planned persistent effect in execution order with its final state            |

The exact serialized values are lowercase hyphenated forms of the named
vocabulary above. Production source defines them once through enums or
readonly const objects. Display code derives counts from `effects`; the result
does not store duplicate planned, applied, verified, reverted, or residual
counts that could disagree with the array.

#### Safe Target And Resource Evidence

Every public effect contains:

- `id`, unique and deterministic within the plan.
- `kind`, the named mechanical transition.
- `purpose`, the operation-owned reason for that transition.
- `target`, a safe logical identity.
- `before` and `after`, the expected logical resource states.
- `state`, the effect's final state for this invocation.

A workspace target uses its canonical workspace-relative path after removing
an optional input-only leading `./`. An exact external target uses a registered logical
identifier, never a physical machine path. File resource states expose a
fingerprint. Absent and directory states expose only their named kind.
Operation-specific effect projections may add focused evidence, such as
generated-entry counts, without adding universal optional fields.

#### Representative Structured Result

For example, a route rebuild preview exposes this focused data:

```json
{
  "workspace": {
    "root": "D:\\work\\example",
    "selectedBy": "current-directory"
  },
  "requestedPaths": [".agents/patterns/open-forge/cli/filesystem/planned-mutation.md"],
  "mutation": {
    "mode": "preview",
    "preflight": "passed",
    "application": "not-started",
    "verification": "not-run",
    "recovery": "not-required",
    "effects": [
      {
        "id": "route.generated-region:.agents/patterns/open-forge/cli/_cli.md",
        "kind": "file-replacement",
        "purpose": "route.generated-navigation",
        "target": {
          "kind": "workspace-path",
          "path": ".agents/patterns/open-forge/cli/_cli.md"
        },
        "before": {
          "kind": "file",
          "fingerprint": {
            "algorithm": "sha256",
            "value": "before"
          }
        },
        "after": {
          "kind": "file",
          "fingerprint": {
            "algorithm": "sha256",
            "value": "after"
          }
        },
        "state": "planned",
        "entries": {
          "before": 6,
          "after": 7
        }
      }
    ]
  }
}
```

#### Branch And Outcome Consistency

An invalid request or a prerequisite failure that prevents planning does not
manufacture an empty mutation projection. The operation-specific result uses
a typed non-plan branch instead. A blocked complete plan does include mutation
evidence with `preflight: "blocked"`, `application: "not-started"`, and every
effect still `planned`. Stable result messages explain the blocker and useful
next action.

The final combinations are consistent:

| Outcome                        | Preflight | Application         | Verification      | Recovery     | Effect states                                     |
| ------------------------------ | --------- | ------------------- | ----------------- | ------------ | ------------------------------------------------- |
| Successful dry run             | Passed    | Not started         | Not run           | Not required | Planned                                           |
| Blocked plan                   | Blocked   | Not started         | Not run           | Not required | Planned                                           |
| Successful application         | Passed    | Complete            | Passed            | Not required | Verified                                          |
| Successful apply no-op         | Passed    | Complete            | Passed            | Not required | Empty effect array                                |
| Cancelled before application   | Passed    | Not started         | Not run           | Not required | Planned                                           |
| Failure with complete recovery | Passed    | Complete or stopped | Failed or not run | Complete     | Reverted and unstarted planned effects            |
| Failure with residual state    | Passed    | Complete or stopped | Failed or not run | Incomplete   | Residual, reverted, and unstarted planned effects |

#### Final Effect And No-op Semantics

An effect never reports a transient `applied` final state. It either remains
planned, reaches verified intended state, returns to its original state, or
remains residual. Internal applied-state records retain the transition detail
needed for recovery.

A successful apply no-op still completes semantic verification. It reports an
empty effect array and any useful operation-specific unchanged observations.
It does not pretend application was skipped or create a synthetic unchanged
effect.

### Human Mutation Evidence

Human presentation leads with the outcome, names affected targets when useful,
and ends with preservation or next-action evidence. It does not dump the JSON
schema or rely on unexplained counts.

#### Preview And Success

Successful dry run:

```text
Route navigation is ready to rebuild.

Replace .agents/patterns/open-forge/cli/_cli.md
  Entries: 6 -> 7

Preflight passed.
No changes were applied (--dry-run).
```

Successful application:

```text
Rebuilt route navigation.

Verified 1 change:
  .agents/patterns/open-forge/cli/_cli.md
```

#### Blocked Plans

Blocked complete plan:

```text
Route navigation was not rebuilt.

Blocked: the source changed after planning.
  .agents/patterns/open-forge/cli/_cli.md

Nothing was changed.
Next: rerun the same command to build a fresh plan.
```

#### Recovery Outcomes

Failure with complete recovery:

```text
Route navigation failed and the applied change was reverted.

Reverted:
  .agents/patterns/open-forge/cli/_cli.md

No planned change remains applied.
```

Failure with residual state:

```text
Route navigation failed and recovery is incomplete.

Residual:
  .agents/patterns/open-forge/cli/_cli.md
  The unexpected current file was preserved.

No further changes were attempted.
Next: open-forge doctor
```

Operation-specific renderers may use more precise verbs, group large effect
sets, and show useful unchanged observations. They preserve the same evidence
and always state whether anything changed, whether verification passed, and
whether recovery left residual state. Human grouping never hides a blocker,
residual target, or replacement whose approval changes future ownership.
Structured output retains every effect.

### Evidence Boundary

The internal plan and applied state may retain physical paths, complete before and after bytes, file identities, temporary paths, and rollback material.

Public human and JSON projections may expose:

- Effect identity, named kind, typed purpose, and safe logical target.
- Expected transition and before or after fingerprints.
- Application, verification, recovery, and final effect states.
- Useful counts, residual identities, and deterministic next actions.

They never expose complete file contents, physical paths, temporary paths, or private rollback material.

Mutation evidence remains inside operation-specific result data. Do not add optional mutation fields to the five-field result envelope or force unrelated operation data into one universal payload.

### Representative Fit

| Operation            | Shared effects                                                                                                        | Operation-specific authority or recovery                                                                                                                                                                                                                                                                                                  |
| -------------------- | --------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `repair`             | Effects proposed by safe-repair findings                                                                              | Every selected repair remains mechanically proven; Git readiness and Gitless backup policy apply to its exact destructive effects                                                                                                                                                                                                         |
| `create`             | File creation and navigation replacement                                                                              | Destination must be absent and parent already routed; Git review records the complete authored result                                                                                                                                                                                                                                     |
| `route init`         | Directory creation, entrypoint file creation, and navigation replacement                                              | Existing valid entrypoints are preserved; the complete initialized chain is verified together                                                                                                                                                                                                                                             |
| `route rebuild`      | Generated-region file replacement                                                                                     | The route planner changes generated meaning only; handled failures recover in process and hard-stop recovery is deterministic rerun                                                                                                                                                                                                       |
| `install`            | File creation or authorized replacement, affected navigation replacement, and workspace-state persistence             | The complete embedded Framework minus retained persisted exclusions participates; exact `--restore` exclusions re-enter desired state without granting overwrite authority; missing and retired targets receive explicit restore, keep-removed, delete, or keep decisions; `.agents/open-forge.json` advances as the final primary effect |
| `extension add`      | Directory or file creation, authorized file replacement, navigation replacement, and workspace-state persistence      | Unowned takeover is explicit; external source is independently reviewed; affected destination navigation is automatically planned and verified; external source locations and review authority are not persisted                                                                                                                          |
| `extension update`   | Authorized file replacement, creation or deletion, navigation replacement, and workspace-state persistence            | External source is re-inspected; dropped content receives literal delete or keep decisions; shared owners and user-owned companions remain protected                                                                                                                                                                                      |
| `extension remove`   | File deletion or ownership release, navigation replacement, and workspace-state replacement                           | Each exclusive path is deleted or retained through visible policy; shared ownership and affected navigation are verified; workspace state advances as the final primary effect                                                                                                                                                            |
| `completion install` | Generated-asset creation or replacement followed by optional activation-profile creation or marker-region replacement | Asset-first ordering makes every hard-stop state dormant or usable; only the owned marker body may change, surrounding profile content is preserved, and divergent ownership requires an explicit decision                                                                                                                                |
| `completion remove`  | Activation-profile marker removal followed by generated-asset deletion                                                | Activation-first removal disables completion before deleting an intact owned asset; missing state is a verified no-op, divergent ownership requires an explicit decision, and malformed ownership blocks                                                                                                                                  |

These operations validate the common contract. Other commands define only their unique request, inspection, authority, plan construction, and final semantic verification.

## Boundaries

### Deliberate Non-Contracts

The following boundaries prevent the shared mutation pipeline from becoming a
general serialization, dependency injection, or universal result system.

The shared contract does not introduce:

- A serializable plan format or reusable plan token.
- A generic environmental service container.
- A dependency graph for sequential effects.
- One public result-data type for every mutation.

Git readiness, explicit bypass, Gitless backups, and hard-stop limitations are
owned by the accepted [recovery contract](workspace-recovery.md). Formatter
selection and resolved-invocation trust are owned by the
[workspace formatting contract](workspace-formatting.md). Formatting runs only
after primary mutation completion and is not serialized as a domain effect.
Neither concern becomes caller-authored plan serialization or hidden operation
intent.

## Verification

Direct conformance tests prove every typed transition, complete-preflight
requirement, outcome row, public projection, recovery branch, and no-op state.
Integration tests instantiate the shared contract through representative
authored, managed, derived, and external effects and prove that operation-local
authority cannot bypass shared preservation and recovery guarantees.

Built-process tests prove preview non-mutation, confirmation and overwrite
boundaries, successful application, handled failure, cancellation, complete
recovery, residual evidence, and clean human and structured output. Phase and
operation tests remain responsible for their unique request, plan, authority,
and final semantic verification.

## Related Current Sources

- [CLI interface](../interface.md)
- [Request construction](request-construction.md)
- [Operation prerequisites](operation-prerequisites.md)
- [Filesystem effects](filesystem-effects.md)
- [Workspace recovery](workspace-recovery.md)
- [Workspace formatting](workspace-formatting.md)
