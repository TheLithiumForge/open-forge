---
open-forge:
  description: Compose multi-stage TypeScript behavior through direct references, named discriminants, exhaustive branches, and type-safe state transitions
  tags: [Pattern, TypeScript, Pipeline, StateMachine, DiscriminatedUnion, Navigation, Readability, TypeSafety]
---

# Typed Transition Pipeline

## Shape

Schematic coordinator fragments. Imports, result declarations, `assertNever` and stage implementations are omitted. Calls name directly imported symbols. Names, values and stage inventory are illustrative.

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

```text
plan         -> ready | blocked
preflight    -> ready | blocked
confirmation -> accepted | cancelled
application  -> complete | stopped
verification -> passed | failed
recovery     -> not-required | complete | incomplete
```

Schematic effect dispatcher with the same omissions:

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

| Part                                                                | Status                                 |
| ------------------------------------------------------------------- | -------------------------------------- |
| Operation-local coordinator and direct references                   | Required                               |
| Real typed choices at each meaningful stage                         | Required                               |
| Exhaustive branches, missing variant fails type checking            | Required                               |
| Exact state definitions beside owning contract                      | Required                               |
| Immutable facts as readonly data                                    | Required                               |
| Protected lifecycle transition as class or construction-gated value | Required when protection is needed     |
| Function-reference object at deliberate composition boundary        | Optional, typed and directly navigable |
| String-addressed behavior or boolean/assertion validation           | Invalid                                |

```text
raw plan -> validating module -> preflighted value -> downstream consumer
                                required input type
```

## Applicability And Sources

For meaningful stages or serializable variants. Serializable discriminants describe state. They do not discover behavior.

- [TypeScript Source Structure](../../../directives/open-forge/typescript/typescript-source-structure.md#typed-transitions) binds dispatch, navigation and lifecycle protection.
- [Named Values](named-values.md) shows discriminant definitions.
- [Nearest Shared Scope](../../software/source-locality/nearest-shared-scope.md) shows demonstrated shared transitions.
