---
open-forge:
  description: Compose multi-stage TypeScript behavior through direct references, named discriminants, exhaustive branches, and type-safe state transitions
  tags: [Pattern, TypeScript, Pipeline, StateMachine, DiscriminatedUnion, Navigation, Readability, TypeSafety]
---

# Typed Transition Pipeline

## Shape

Keep stage transitions visible in one coordinator:

```ts
const planResult: PlanResult = planMutation(request);

switch (planResult.state) {
  case PlanState.ready:
    return preflightPlan(planResult.plan);
  case PlanState.blocked:
    return createBlockedResult(planResult.findings);
  default:
    return assertNever(planResult);
}
```

`PlanState`, `planMutation`, `preflightPlan`, and `createBlockedResult` are direct imported symbols. Editor navigation reaches their definitions immediately.

Each stage returns only its real choices:

```text
plan        -> ready | blocked
preflight   -> ready | blocked
confirmation-> accepted | cancelled
application -> complete | stopped
verification-> passed | failed
recovery    -> not-required | complete | incomplete
```

Exact state values live beside the contract that owns them as one enum or readonly const object. Result unions derive their discriminants from those values.

## Behavior Dispatch

Serializable discriminants describe data; they do not discover behavior. Dispatch through an exhaustive branch that calls direct references:

```ts
switch (effect.kind) {
  case EffectKind.fileCreate:
    return applyFileCreate(effect);
  case EffectKind.fileReplace:
    return applyFileReplace(effect);
  case EffectKind.fileDelete:
    return applyFileDelete(effect);
  case EffectKind.directoryCreate:
    return applyDirectoryCreate(effect);
  default:
    return assertNever(effect);
}
```

Adding an effect kind must make every incomplete dispatcher fail type checking.

Do not use:

```text
pipeline.run("apply")
handlers[effect.kind](effect)
container.resolve("filesystem")
```

A typed object containing actual function references may be useful at a deliberate composition boundary. It must use `satisfies` or an explicit declaration-side type, remain directly navigable, and not hide materially different stage choices behind one generic callback bag.

## Lifecycle Values

Use ordinary readonly data for immutable facts. Use a class or construction-gated value when a lifecycle transition must be protected, such as turning a raw plan into a preflighted plan.

Do not simulate validation through:

```text
preflightPassed: true
state: "ready" as ReadyState
```

The validating module creates the next-stage value without a workspace-owned type assertion. A downstream function accepts only that value.

## Placement

The coordinator stays with the operation it makes legible. Reusable state values and mechanical transitions move to their nearest demonstrated common scope. Do not create a universal pipeline engine merely because several operations use stages.

## Review Checks

- Each behavior is connected through a direct imported reference.
- Every meaningful stage exposes its real typed choices.
- Branches are exhaustive and fail compilation when a variant is missing.
- String discriminants describe serializable state rather than resolve behavior.
- Invalid lifecycle transitions cannot be created through a boolean or assertion.
- A reader can navigate from coordinator to stage implementation in one editor action.
- Generic composition does not conceal operation-specific authority or recovery.
