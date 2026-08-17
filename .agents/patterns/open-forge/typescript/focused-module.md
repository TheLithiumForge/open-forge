---
open-forge:
  description: Shape one strict ESM module around one readable responsibility and split it along behavioral boundaries
  tags: [Pattern, TypeScript, ESM, Module, Readability, Function, Class]
---

# Focused TypeScript Module

## Boundary

The active [TypeScript Source Structure
Directive](../../../directives/open-forge/typescript/typescript-source-structure.md) owns binding
language, type-safety, readability, and file-size policy. This Pattern owns the
reusable module, test, entrypoint, and behavioral-splitting shape.

## Shape

Start one behavior with one same-named module and one adjacent focused test:

```text
feature/
  feature.ts
  feature.test.ts
```

Export intentional named functions, classes, types, and constants. Import
dependencies directly from the module that owns them. Add a barrel only at a
deliberate public or composition boundary.

Keep pure transformations as functions. Use a class when cohesive state,
resource ownership, lifecycle, or side effects become clearer through
encapsulation. Choose the construct that makes the behavior easiest to read.

When the module gains a second responsibility or reaches the applicable size
boundary, split it by behavior. Use adjacency only when the subject is expected
to remain one production module with one test file. When a subject has or is
expected to need multiple test files, create its nearest `__tests__/` directory
from the first test and name the files by the behavior they prove:

```text
feature/
  feature.ts
  parse-feature.ts
  render-feature.ts
  __tests__/
    feature.test.ts
    feature-errors.test.ts
    parse-feature.integration.test.ts
```

The same-named module remains the feature's obvious entry or composition
point. A split file names the behavior it owns. It is not `helpers.ts`,
`common.ts`, or `utils.ts`.

Test tiers remain visible in filename suffixes such as `*.test.ts`,
`*.integration.test.ts`, and `*.e2e.test.ts`. Do not add `unit/`,
`integration/`, or equivalent tier directories beneath `__tests__/`; they
duplicate that identity and separate one subject's evidence by test mechanism.

Use the [Named Values](named-values.md) Pattern when the module defines
protocol-significant or otherwise magic strings and numbers. Use the [Nearest
Shared Scope](../../software/source-locality/nearest-shared-scope.md) Pattern only after support gains another
real consumer.

## Review Checks

- A reader can state the module's responsibility from its path, name, and exports.
- Pure behavior and stateful lifecycle behavior use the clearest fitting language construct.
- One focused test may sit beside a subject expected to remain a one-test subject; a subject expected to need multiple test files starts with one nearest `__tests__/` directory.
- Test filenames carry evidence depth without redundant tier directories.
- A split follows a responsibility boundary rather than merely moving lines.
- The same-named module remains the obvious entry or composition point.
- No generic file collects unrelated helpers.
- Shared support moves only after demonstrated reuse.
