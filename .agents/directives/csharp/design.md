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
- A switch over every named member of a C# enum is not closed over the enum's runtime value space: callers can still supply an unnamed underlying numeric value, and the compiler can warn that the switch is non-exhaustive. Use a clear switch expression with an explicit discard arm that throws `ArgumentOutOfRangeException`, and test both every supported named mapping and one undefined runtime value. Do not add analyzer, source-generator, discriminated-union, reflection, or warning-suppression machinery merely to claim compiler-enforced exhaustiveness when the native enum boundary and direct evidence are sufficient. Follow the [Exhaustive C# Enum Switch Pattern](../../patterns/software/exhaustive-csharp-enum-switch.md) when this shape applies.
- Give syntax tokens, schema versions, marker text, policy names, stable identifiers, limits, and other values with constant-like meaning one symbolic definition at their nearest owning scope. Use an enum for a finite concept, `const` for a true compile-time constant, and `static readonly` or an immutable value when construction or runtime type semantics require it. Do not scatter a shared contract as repeated string or numeric literals, and do not create a distant constants catalogue that loses ownership. A test may keep an independent literal oracle when importing the production constant would make the assertion tautological; otherwise reuse the owned contract value or a focused test-fixture constant.
- Do not promote a local branch merely to claim reuse. A static strategy earns a wider scope when an accepted shared contract already owns the policy or multiple real consumers demonstrate identical meaning. Keep command-specific payload formation and rendering local even when shared process presentation, status, or stream policy is centralized.
- Build a capability shared from the start when an accepted document already establishes it as generic. A backing contract, architecture, or pattern that defines one meaning for a policy is sufficient justification; a second consumer need not exist yet. Cite the accepted source in the shared type so a later reader can see why the scope was chosen. This is the only case where anticipating reuse is warranted, and it does not license speculative frameworks, extension points, or configuration that no accepted source requires.
- Default to the shared owner. When an accepted shared capability exists for a policy, use it; do not restate its mapping, formatting, or decision locally because a local copy is shorter to write. A shared type with surviving duplicates is worse than either alternative, because a change must then be found in several places and the copies drift silently.
- Promotion is not finished until the duplicates are removed. When a policy moves to a wider scope, migrate every existing consumer and delete the local implementations in the same coherent phase. Leaving both is an incomplete change, not a gradual one.
- A structural question has one implementation. Two correct answers to the same question — how a section is located, how a status is named, how a path is normalized — are a defect even while they agree, because nothing keeps them agreeing. Find the existing owner before writing the second answer.
- Optimize for readable data flow and avoid redundant extraction, allocation, rendering, and dispatch. Do not evaluate unselected strategies or create per-call capturing delegates. Treat runtime-efficiency claims as boundary-specific: passing a reference-type record is cheap, while large value types and measured hot paths still require evidence.
- Do not nest or chain conditional (`?:`) expressions. Use ordered `if` returns for precedence and guard flow, or one clear exhaustive switch when it reads better. A single non-nested conditional expression is acceptable only for one obvious two-way value; never use indentation to make a conditional chain appear simpler than it is.
- Do not declare custom operator overloads or user-defined implicit or explicit conversion operators. Prefer visible named construction and conversion methods such as `Parse`, `TryParse`, `From`, `Create`, or `Read`. If an exceptional external boundary truly requires implicit framework invocation, obtain maintainer acceptance and annotate/register that behavior explicitly at the boundary; do not rely on hidden conversion or convention as ordinary application flow.

### Standard Capabilities And Authored Data

- Use the pinned runtime, BCL and accepted dependencies before writing replacement
  mechanics. Read their supported call surface and consume typed results. A
  product validation rule is not a reason to recreate tokenization, option
  aggregation, format parsing or serialization.
- Let the accepted YAML, Markdown and JSON libraries interpret their formats.
  Use their models, tokens, syntax trees and source spans for the facts the
  application needs. Keep product-specific grammar and validation explicit and
  local. Do not rescan raw text with splits or regular expressions to duplicate
  syntax the library already handles. Exact source edits may retain original
  bytes and spans without reparsing the surrounding language.
- Documents enter the system through one parsing layer and leave it as the typed
  document model. Every other layer consumes that model. A command, planner,
  renderer, test, or delivery script never re-derives document structure from
  raw text, and never introduces a second reader for a format the parsing layer
  already owns.
- When shipping embedded data, derive its inventory from declared resources and
  manifests. Use ordinary project resource inclusion and resource access instead
  of maintaining a second list of payload files, IDs or dependencies in C#.
  Keep resource names deterministic and validate the discovered data through its
  typed contract. Resource enumeration does not imply reflective type discovery
  or dynamic command registration.
- Keep formatting and terminal state outside domain behavior. Pass the narrow
  immutable presentation facts a renderer needs, and style generated labels from
  typed values. Do not parse finished human output or mutate ambient console
  state inside reusable domain capabilities.
- Give behavior tests small fixtures that express the scenario independently of
  unrelated documentation wording. Use reviewed snapshots for deliberate text
  projections. Assert semantic facts and effects independently of those snapshots.
  Tests of packaged-data parity must still compare the actual shipped inputs.
- Keep custom substitutes and compatibility shims tied to an accepted requirement
  and a demonstrated standard-capability gap. Follow the
  [Proportionate Development Directive](../proportional-development.md) for that
  decision. Remove a solved exception from active instructions instead of
  preserving obsolete workaround advice.

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
