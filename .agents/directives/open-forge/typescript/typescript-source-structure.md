---
open-forge:
  description: Keep authored TypeScript strict, readable, focused, modular, and reusable without accumulating oversized or generic source files
  tags: [LoadNow, Directive, TypeScript, Source, Structure, ESM, Strictness, Modularity, Readability]
---

# TypeScript Source Structure

## Instructions

### Language And Type Safety

- Write authored TypeScript as ESM with explicit `import` and `export` syntax. Do not add CommonJS modules, `require`, or `module.exports`.
- Use static imports for ordinary dependencies. When every binding in a static import declaration is type-only, write the declaration with `import type`. When one static declaration imports runtime values and types from the same module, mark its type-only specifiers with inline `type`.
- Reference imported types through named static imports. Do not use TypeScript import type expressions such as `import("module").Type`.
- Use runtime `import()` only at an explicit asynchronous lazy-loading boundary. Keep the reason visible at that boundary, and scope any lint exception to the exact import that requires it.
- Enable and preserve full TypeScript strictness. Do not use `any`, suppressed diagnostics, weakened compiler settings, or unchecked casts to bypass a type boundary that can be modeled explicitly.
- Separate compiler projects when their runtime environments or ambient globals differ. Repository scripts and tests that share the Node environment and import test APIs explicitly use one strict root no-emit configuration. Keep a focused emitting configuration for the shipped npm launcher so delivery helpers and tests stay outside its output.
- Use `const` unless the binding itself must be reassigned. Prefer declaration-side annotations when an authored contract benefits from one, and use `satisfies` selectively when preserving narrower inference is useful.
- Never use `as any`, a chained assertion, or `as Type` for a workspace-owned value. Isolate and explain an unavoidable assertion only at a focused untyped external boundary. Literal-preserving `as const` definitions remain valid.

### Readability And Size

- Aim to keep each authored TypeScript file at or below 100 lines and do not exceed 200 lines. Generated files are exempt.
- Reserve `*.types.ts` for modules that contain only type declarations and type-only exports. A module that exports any runtime value uses an ordinary `.ts` filename even when it also exports types.
- Keep ordinary code understandable at first glance through direct names, explicit control flow, and cohesive responsibilities rather than clever abstraction or compressed syntax.
- Use comments for intent, external constraints, and non-obvious contracts. Simplify code that needs a long comment to explain ordinary behavior, and fix a workaround at its root when the workspace controls that root.

### Named Values And Tests

- Do not leave magic strings, numbers, paths, suffixes, tokens, states, limits, or fixture controls at their use sites. Define each behavior-significant value once at its nearest owning scope and consume that name in production and tests.
- Keep a literal inline only when its meaning is self-evident and local and it does not couple control flow, protocol behavior, persisted data, or expectations. Repetition, semantic comparison, or change risk makes the value named.
- Import production named values for protocol and control concepts. Keep arbitrary reusable test data in deterministic named fixtures rather than restating raw values across setup and expectations.

### Required Patterns

- Apply the [Focused Module](../../../patterns/open-forge/typescript/focused-module.md) Pattern when creating or splitting source, tests, fixtures, or helpers.
- Apply the [Nearest Shared Scope](../../../patterns/software/source-locality/nearest-shared-scope.md) Pattern when support gains additional consumers.
- Apply the [Named Values](../../../patterns/open-forge/typescript/named-values.md) Pattern to protocol-significant or otherwise magic strings and numbers.
- Apply the [Typed Transition Pipeline](../../../patterns/open-forge/typescript/typed-transition-pipeline.md) Pattern when behavior has meaningful stages or serializable variants.
- Apply the [Contract Ownership](../../../patterns/software/contract-ownership.md) Pattern when semantic documentation, generated reference, and conformance evidence describe one contract; apply the TypeScript specialization when the exact source contract is TypeScript.

### Module And Test Shape

- Keep pure transformations as functions. Use a class when cohesive state, resource ownership, lifecycle or side effects become clearer through encapsulation. Choose the construct that makes the behavior easiest to read.
- Export intentional named functions, classes, types and constants. Import directly from the owning module. Add a barrel only at a deliberate public or composition boundary.
- When a module gains a second responsibility or reaches its applicable size boundary, split by behavior. Adjacency fits a subject expected to remain one production module with one test. A subject with, or expected to need, multiple test files starts with its nearest `__tests__/` directory. Name tests by the behavior they prove.
- Keep the same-named module as the obvious entry or composition point. Split files name their behavior, not `helpers.ts`, `common.ts` or `utils.ts`. Keep test tiers visible in suffixes such as `*.test.ts`, `*.integration.test.ts` and `*.e2e.test.ts`. Do not add redundant tier directories beneath `__tests__/`.

### Exact Contracts And Named Values

- Export exact types and named values from focused source modules. Do not maintain handwritten declaration mirrors for documentation. Keep runtime validation beside the untrusted boundary it protects. The [TypeScript Contract Ownership Pattern](../../../patterns/open-forge/typescript/contract-ownership.md) shows this specialization of the [generic ownership rule](../../software/contract-ownership.md).
- Give each semantic value set one runtime enum or readonly const object. Derive type unions from that runtime definition instead of restating members. Do not define an enum and const object repeating one concept's values. Build mappings from named keys and satisfy a complete typed record. Consumers use named values instead of raw protocol strings or policy numbers.
- Always name schema and format versions, command and operation identifiers, result statuses, message levels, diagnostic and error codes, exit values, lifecycle and persistence states, wire-format discriminants, and policy-bearing retry, timeout, size and count limits.
- Place value objects beside their contracts. CLI-wide concepts live at CLI scope, command or family diagnostics begin there, and domain states stay with their domain. Move definitions only to the nearest common ancestor of demonstrated consumers. Do not create a global constants catalogue. Ordinary one-off presentation copy, fixture content and self-explanatory local arithmetic are not automatically magic. Name them when repetition, domain meaning or change risk makes it useful.

### Typed Transitions

- Keep meaningful stage transitions visible in one operation-local coordinator through direct imported references. Each stage returns only its real typed choices. Serializable discriminants describe data and never discover behavior. Keep exact state values beside their contract as one enum or readonly const object and derive result-union discriminants from them.
- Dispatch through exhaustive branches calling direct references. Adding a variant must fail type checking in every incomplete dispatcher. Do not use string-addressed stages, keyed handler lookup or container resolution to discover behavior.
- A typed object of actual function references may serve a deliberate composition boundary. It uses `satisfies` or an explicit declaration-side type, remains directly navigable and does not hide materially different stage choices in a generic callback bag.
- Use ordinary readonly data for immutable facts. Use a class or construction-gated value when a lifecycle transition must be protected. The validating module creates the next-stage value without a workspace-owned assertion and downstream functions accept only that value. Do not simulate validation with a boolean or assertion.
- Promote reusable state values and mechanical transitions only to their nearest demonstrated common scope. Do not build a universal pipeline engine merely because operations have stages. Generic composition must not conceal operation-specific authority or recovery.
