---
open-forge:
  description: Shape one strict ESM module around one readable responsibility and split it along behavioral boundaries
  tags: [Pattern, TypeScript, ESM, Module, Readability, Function, Class]
---

# Focused TypeScript Module

## Shape

```text
feature/
  feature.ts                        same-named entry, one responsibility
  feature.test.ts                   adjacent focused test
```

Default for a subject expected to remain one production module and one test. For a subject with, or expected to need, multiple test files:

```text
feature/
  feature.ts                        entry or composition point
  parse-feature.ts                  separate behavior
  render-feature.ts                 separate behavior
  __tests__/                        nearest test directory from first test
    feature.test.ts
    feature-errors.test.ts
    parse-feature.integration.test.ts
```

| Part                                           | Status                                                |
| ---------------------------------------------- | ----------------------------------------------------- |
| Behavior-based split and obvious entry module  | Required when splitting                               |
| Named exports and direct owning-module imports | Required                                              |
| Evidence tier in filename suffix               | Required, no tier directories beneath `__tests__/`    |
| Barrel                                         | Optional at deliberate public or composition boundary |
| `feature`, `parse`, `render`, file order       | Illustrative                                          |

## Applicability And Sources

For strict ESM modules. Pure transformations fit functions. A class fits when state, resource ownership, lifecycle or side effects read more clearly through encapsulation. The clearest construct fits the behavior.

- [TypeScript Source Structure](../../../directives/open-forge/typescript/typescript-source-structure.md#module-and-test-shape) binds language, safety, size, splitting and test layout.
- [Named Values](named-values.md) shows protocol and policy values.
- [Nearest Shared Scope](../../software/source-locality/nearest-shared-scope.md) shows support placement when reuse is real.
