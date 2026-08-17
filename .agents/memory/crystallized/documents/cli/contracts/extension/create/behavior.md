---
open-forge:
  description: Accepted technology-neutral behavior for Extension catalogue scaffold planning, application, verification, and recovery
  responsibility: Define create's exact destination resolution, one scaffold plan, workspace no-op, safety, and typed result
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Extension, Create, Behavior, Catalogue, Mutation, Safety, Recovery, CurrentTruth]
---

# extension create Behavior Contract

## Status And Boundary

This is the accepted current Crystallized Behavior Contract for
`open-forge extension create`. It defines deterministic request resolution,
catalogue-parent and package-destination facts, one scaffold plan, dry-run and
application, Git and recovery policy, verification, result formation, and
technology-neutral conformance. It does not define package schema, parser,
storage, or workspace lifecycle authority.

## Typed Flow

```text
validated ID and catalogue parent
  -> exact catalogue and destination facts
  -> scaffold intended state
  -> one complete plan and preflight
  -> dry-run or application
  -> verification or reverse guarded recovery
  -> one typed result
```

The operation never forms a workspace lifecycle plan. Its `--workspace` value
may be parsed and reported as an accepted global no-op, but it does not affect
the catalogue destination. Create is the accepted no-workspace mutation
exception: the catalogue destination is the sole operation subject, so create
does not acquire `.agents/open-forge.lock` or mutate workspace state.

## Request Resolution

1. Resolve terminal help/version before any catalogue work.
2. Accept zero or one stable-ID operand and zero or one `--path` value. Require
   both after wizard/direct resolution; missing non-interactive semantic input is
   `invalid`.
3. Reject source, package-selection, force, prune, and other mutation flags.
4. Collapse repeated `--automatic`, `--dry-run`, and `--skip-git-check` presence
   idempotently. Repeated stable ID or `--path` is invalid.
5. Accept `--workspace` as the shared no-op defined by the global contract.

Argumentless prompt-capable human input enters the finite two-question wizard.
Explicit inputs populate the same request. Automatic mode suppresses interaction
only when ID and path are already explicit. No recommendation, current folder,
workspace, or source resemblance fills a missing value.

## Destination Facts And Plan

Resolve the exact catalogue parent from `--path` and prove its lexical and
physical identity and safe catalogue shape. No marker file is required. Resolve
the exact `<catalogue>/<id>/` package destination and retain its exact physical
identity when it exists. Prove that it is contained by the catalogue parent and
that it is either absent or contains the exact intended scaffold. An exact
matching scaffold is a verified no-op. Any divergent, partial, additional,
unknown, or colliding occupant blocks the complete plan. Existing package bytes
are never adopted or overwritten.

The intended scaffold has exactly these effects:

```text
<catalogue>/<id>/extension.json
<catalogue>/<id>/payload/.agents/
```

The plan does not include README, payload source files, dependency closure,
workspace files, generated navigation, or lifecycle-document effects.
It has no hidden source or target workspace.

If the exact scaffold already exists and matches the intended state, return a
verified no-op. Do not create timestamps or synthetic changes.

## Safety, Git, And Recovery

Preflight validates ID, destination, exact catalogue and destination physical
identity, parent containment, existing-state collision, affected paths, affected-
path Git cleanliness where applicable, backup readiness, expected state,
verification, and complete staging, recovery, and collision guards. One unsafe,
ambiguous, or unavailable selected fact blocks or incompletes the whole plan.

Dry-run and application share the same request, facts, plan, and preflight.
Dry-run writes no directory, scaffold file, backup, temporary artifact, or
lifecycle state and cannot claim application verification. It forms the same
pre-effect planning status as application but never produces an apply-time
`failed` or `interrupted` result because it performs no effects. A planning or
read failure and caller cancellation before effects retain their own event
meaning.

Application revalidates the exact parent and destination physical identities,
containment, expected state, and complete destination condition immediately
before effects. It creates the complete scaffold, verifies both scaffold results
and the complete destination condition, and retains/reports recovery evidence on
failure.
`--skip-git-check` bypasses only affected-path cleanliness and never grants
overwrite or adoption authority. If existing bytes require recovery, the
accepted adjacent-backup rules apply; unknown or colliding artifacts block.

On failure, stop new effects and reverse applied effects only while identity
guards match. Preserve concurrent changes and residual recovery evidence. A
recovery failure is `failed`; caller cancellation without stronger failure is
`interrupted`. A later invocation forms a fresh plan.

## Results And Conformance

Form one typed result containing exact catalogue/path, ID, mode, intended scaffold,
effects or no-op, workspace-lifecycle unchanged fact, Git/recovery, verification,
status, and at most one next action. Human and JSON renderers consume it once.
Use the shared seven statuses and streams; `attention` is currently unreachable
for create.

Create never mutates a workspace, package source, lifecycle document, generated
navigation, or Framework file. It does not acquire `.agents/open-forge.lock`.
Conformance must cover wizard and direct requests, automatic omission states,
absent destinations, exact-scaffold no-op, divergent, partial, additional,
unknown, and colliding occupants, exact catalogue and destination physical
identity, workspace no-op, dry-run no-effects, affected-path Git where
applicable, complete staging and recovery/collision guards, expected-state
revalidation immediately before effects, verification, reverse recovery,
repeated no-op, result parity, all statuses, and no implementation or shipping
claim. The shared CLI Architecture defines the exact JSON result schema and exit
mapping. Gate 5 must prove source-generated serialization, fixed Markdig where
used, real `System.IO`, Native AOT, isolated tests, and package journeys.
