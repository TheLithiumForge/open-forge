---
open-forge:
  description: Design cohesive C# call surfaces with truthful nullability, readable construction, and no speculative frameworks
  tags: [LoadNow, Directive, CSharp, Design, Nullability, Initialization, Parameters, Records, Strategy, Efficiency, Readability]
---

# C# Callable Design

## Instructions

- Prefer one or two explicit parameters for a behavioral method, constructor, or delegate. Treat four or five as the normal maximum. Use more only at an exceptional framework, interop, serialization, recursive traversal, or data-contract boundary where the separate values are genuinely clearer than a cohesive input.
- Pass an existing cohesive immutable record or stage context directly when the callee operates within that record's semantic boundary. Do not unpack a record merely to forward or repack many of its members. Let the receiving capability select the facts it owns.
- Treat repeated member forwarding as a design signal. When one call would pass several facts from the same cohesive immutable source, such as `combined.Sources`, `combined.Links`, `combined.Findings`, and `combined.Complete`, pass that source once and let the receiving constructor, factory, or capability select or project the facts it legitimately owns. Apply this together with the normal parameter limit; do not preserve a long call merely by naming several extractions from one object. Do not pass a broad source object when the callee owns only one fact or when doing so would grant unrelated authority or couple distant layers; use a narrow typed view instead.
- Introduce a context only when its values share one stage, lifecycle, invariant, or ownership boundary. Do not hide unrelated dependencies in an `Args`, `Options`, `Context`, dictionary, or service bag merely to satisfy a parameter count. Prefer a narrower typed view when passing a broad record would grant unrelated authority or couple distant layers.
- Distinguish behavior call surfaces from genuine data shapes. A concrete serialized result, immutable fact record, or source-generated model may contain more than five fields when that is its accepted schema. Keep construction readable with named members, focused factories, or cohesive nested values rather than long positional plumbing through behavioral methods.
- When one finite typed key repeatedly selects stable data or behavior, prefer one centralized immutable static map or table of small strategy values or non-capturing delegates over duplicated switches, condition chains, strategy class hierarchies, dependency injection, or reflective dispatch. Use enum or other typed keys, validate unknown values exhaustively, and keep the table at the nearest scope that owns the shared meaning.
- Do not promote a local branch merely to claim reuse. A static strategy earns a wider scope when an accepted shared contract already owns the policy or multiple real consumers demonstrate identical meaning. Keep command-specific payload formation and rendering local even when shared process presentation, status, or stream policy is centralized.
- Optimize for readable data flow and avoid redundant extraction, allocation, rendering, and dispatch. Do not evaluate unselected strategies or create per-call capturing delegates. Treat runtime-efficiency claims as boundary-specific: passing a reference-type record is cheap, while large value types and measured hot paths still require evidence.
- Do not nest or chain conditional (`?:`) expressions. Use ordered `if` returns for precedence and guard flow, or one clear exhaustive switch when it reads better. A single non-nested conditional expression is acceptable only for one obvious two-way value; never use indentation to make a conditional chain appear simpler than it is.
- Do not declare custom operator overloads or user-defined implicit or explicit conversion operators. Prefer visible named construction and conversion methods such as `Parse`, `TryParse`, `From`, `Create`, or `Read`. If an exceptional external boundary truly requires implicit framework invocation, obtain maintainer acceptance and annotate/register that behavior explicitly at the boundary; do not rely on hidden conversion or convention as ordinary application flow.

### Nullability And Runtime Validation

- Declare nullable reference intent accurately and keep nullable compiler analysis
  warning-clean. Fix a possible-null warning by correcting initialization, control
  flow, the declared `T` or `T?` contract, or a truthful nullable-analysis
  attribute. Do not silence ordinary production warnings with `!` or preserve an
  inaccurate non-nullable declaration.
- Rely on compiler-proven non-null state for trusted internal values. Do not add a
  routine `ArgumentNullException.ThrowIfNull` check when a value was created and
  retained inside typed warning-clean code and has not crossed an untrusted or
  nullable-oblivious boundary.
- Validate unsupported `null` at public, protected, explicit-interface, library,
  deserialization, reflection, interop, `dynamic`, legacy, nullable-oblivious, and
  other external boundaries, or whenever the invariant cannot be established
  statically. Validate once at ingress and pass a proper non-nullable value into
  the trusted domain. Prefer `ArgumentNullException.ThrowIfNull` when that runtime
  guard is required.
- Use `required` to express a construction-site presence requirement, not a
  runtime non-null guarantee. `required`, `init`, nullable annotations, nullable-
  analysis attributes, and `[SetsRequiredMembers]` provide compiler information;
  none substitutes for boundary validation. Keep attributes truthful and use
  `[NotNull]`, `[NotNullWhen]`, `[NotNullIfNotNull]`, or `[MemberNotNull]` only when
  the implementation establishes the stated postcondition.

### Construction And Modern Syntax

- Prefer the shortest idiomatic syntax that improves readability without hiding
  type, ownership, mutation, validation, API evolution, serializer/source-
  generation/Native-AOT compatibility, allocation, or collection semantics.
  Modern syntax is selected for fitness, not novelty or maximum usage.
- Use constructors or focused factories for behavioral objects, required
  dependencies, authorization or resource ownership, cross-member invariants, and
  types that must be valid atomically. Treat a long constructor as a design signal:
  keep jointly constrained values together, group a genuine cohesive value, or
  split unrelated behavior instead of hiding dependencies in a property/options
  bag.
- Use named object initializers with `required init` for genuine data-shaped
  contracts whose members are independently supplied and intentionally writable
  during construction. On an `internal` containing type, `public required init`
  members do not widen the type beyond its containing assembly; prefer that clear
  construction surface when no member-specific access restriction is required.
  Do not make the containing type or an external API public only to enable an
  initializer. Keep a constructor or factory when non-public members, temporary
  invalidity, inherited required members, runtime validation, cross-member
  invariants, resource ownership, or serializer behavior would weaken the type.
  Adding a required member to an evolving public type is a compatibility decision.
- When a constructor or factory remains, use named arguments at an ambiguous call
  site. Named arguments are required when four or five values would otherwise be
  difficult to identify locally, when adjacent values have the same or compatible
  types, or when swapping them could compile. Prefer passing one cohesive source
  record over naming several repeated member extractions when the receiving
  capability legitimately owns that record's semantic boundary.
- Use primary constructors when they remove boilerplate and the parameter scope,
  storage, inheritance, and lifetime remain obvious. They do not turn parameters
  into properties on ordinary classes or replace invariant validation.
- Use target-typed `new()` when the target type is immediately visible. Use
  `new { Prop = value }` only for a local anonymous projection; named public or
  reusable data uses a named type.
- Prefer collection expressions such as `[]` and `[first, second]` when the target
  type and eager materialization are clear and concrete type, collection identity,
  custom `Add`/indexer behavior, overload resolution, and allocation strategy do
  not carry meaning. Keep an explicit array/list construction or collection
  initializer when those semantics matter.

### Rationale And Evidence

- Nullable reference types, `required`, `init`, nullable-analysis attributes, and
  `!` are compile-time features. They do not create CLR-level runtime null safety.
  Defensive checks therefore belong at uncertain ingress, while typed internal
  flow should prevent invalid assignment rather than repeatedly rediscover it.
- Object initializers run after a constructor. They improve long independent data
  shapes but cannot enforce atomic cross-property invariants. `required` enforces
  assignment presence for participating C# callers, not value validity, and can
  be bypassed by reflection or other nullable-oblivious construction paths.
- Collection expressions are target-typed eager materialization and can select a
  different concrete representation or allocation strategy. They are not a
  mechanical replacement when identity, laziness, overloads, or custom collection
  behavior matters.
- Authoritative language and runtime references:
  - [Nullable reference types](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/null-safety/nullable-reference-types)
  - [`required` modifier](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/required)
  - [Nullable static-analysis attributes](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/attributes/nullable-analysis)
  - [Object and collection initializers](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/object-and-collection-initializers)
  - [Primary constructors](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/instance-constructors#primary-constructors)
  - [Collection expressions](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/collection-expressions)
  - [System.Text.Json required properties](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/required-properties)
  - [Reflection versus source generation](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/reflection-vs-source-generation)
  - [CA1062: Validate arguments of public methods](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1062)
  - [CA2264: Do not pass a non-nullable value to ThrowIfNull](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2264)
  - [IDE0028: Collection initialization](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0028)
