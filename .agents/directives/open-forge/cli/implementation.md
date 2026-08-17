---
open-forge:
  description: Implement the accepted new Open Forge CLI in C# on .NET 10 or newer with real filesystem boundaries and Native AOT-safe evidence
  tags: [LoadNow, Directive, CLI, Implementation, CSharp, DotNet, NativeAOT, Filesystem, Testing, AOT]
---

# New Open Forge CLI Implementation

## Instructions

### Scope And Acceptance

- Apply these instructions only to the new Open Forge CLI after the parent CLI scope is selected. Do not use them for the legacy CLI or unrelated Framework work.
- Gate 3 Architecture and Gate 4’s current source set are accepted. Gate 5 is authorized and active through bounded implementation Tasks. Keep the replacement non-shipping until the complete Gate 5 evidence and release boundary are accepted.
- Do not make an unaccepted Architecture or library choice during implementation. If implementation exposes a material choice outside the accepted Architecture, stop and reopen Architecture for that narrow choice only.
- The new CLI has no legacy compatibility or migration layer. Do not carry legacy commands, paths, schemas, or behavior into it.

### Runtime, Solution, And Build

- Implement the CLI in C# on .NET 10 or newer only. The accepted design specifies one future production executable.
- Use the modern `.slnx` solution format. Its project and folder structure must mirror real physical folders; do not create solution-only virtual folders.
- Managed tests mirror the production feature and capability paths one-to-one under the test root. Put end-to-end and package journeys at system scope rather than under a production feature folder.
- Treat Native AOT and trimming compatibility as requirements for every runtime feature, dependency, and serialization path. Prove those requirements with actual Native AOT publish evidence rather than source inspection or a nominal setting.
- Format authored C# with `dotnet format`. Prefer one line for a declaration, invocation, or object construction when it stays at or below 200 characters and remains readable; lines in the 180–200 range are acceptable. Treat 200 lines per class as a review heuristic rather than a hard limit. A materially larger class prompts a locality or architecture review before it is accepted.

### Filesystem, Platform, And Locality

- Use real `System.IO` filesystem boundaries and real isolated OS temporary directories for integration and end-to-end tests. Do not introduce a virtual filesystem abstraction, fake filesystem, or fake filesystem hierarchy. Pure path and value logic may remain directly testable without pretending that filesystem effects occurred.
- Use cross-platform .NET BCL APIs first. Add platform-specific code only when a critical guarantee is proved impossible otherwise, and reopen Architecture for that narrow choice before adding it.
- `.agents/open-forge.lock` coordinates operations that mutate the selected workspace. `extension create` has no workspace subject and therefore does not acquire that lock; it uses exact catalogue-destination identity, expected-state revalidation, Git/recovery, and collision guards. This is the only current no-workspace mutation exception.
- Keep command behavior, source, unit and integration tests, fixtures, and one-use support at the narrowest useful command or capability scope. Keep fixtures and support at the nearest mirrored scope in the separate test root.
- Do not link test files into production folders merely to simulate locality. Do not create a remote `utils` folder, universal engine, or speculative shared abstraction. Promote support only after demonstrated reuse, and promote it to the nearest common scope of the real consumers.

### Construction And Composition

- Prefer direct construction and pure, capability-named static functions or extensions when they provide the required behavior clearly.
- Dependency injection is allowed only when concrete lifecycle or composition value earns it. Any dependency-injection path must be source-generated and Native AOT-safe.
- Do not use generic `Utils` helpers or folders, global mutable service state, reflective scanning, or a service locator.
- Choose classes, functions, records, or direct procedural composition according to state, lifecycle, resource ownership, clarity, and local simplicity. No one form is mandatory by dogma.

### Dependencies And Libraries

- Use the accepted package and boundary choices: explicit/manual `System.CommandLine`, a fixed Markdig pipeline, the source-generated YamlDotNet path, and `System.Text.Json` (STJ) source generation.
- Require every dependency and accepted boundary to earn its Native AOT and trimming compatibility, binary-size, complexity, maintenance, and security cost. A focused local implementation is allowed when it is simpler and sufficiently verified.
- Actual Native AOT publish evidence remains required for every accepted package, runtime feature, dependency, parser, and serialization path. Do not replace an accepted choice without reopening Architecture.

### Lifecycle And Packaging

- Store Open Forge lifecycle state at `.agents/open-forge.lifecycle.json` and the mutation lock at `.agents/open-forge.lock`.
- The npm package is `@thelithiumforge/open-forge`. Platform packages use the same `@thelithiumforge` scope.
- Package wrappers install or invoke the canonical executable and never implement CLI behavior. Package end-to-end evidence must exercise the packed wrappers.

### Test Evidence

- Use xUnit v3. Every `Fact` and `Theory` must declare an explicit, readable `DisplayName`.
- Give every test durable, independently selectable `Feature` and `Evidence` traits. Use `Unit`, `Integration`, `EndToEnd`, and `PackageEndToEnd` as the `Evidence` values.
- Unit tests cover cheap methods, classes, functions, and steps. Integration tests prove modules or commands through real boundaries. End-to-end tests invoke the whole built or native CLI. Package end-to-end tests prove packed wrappers.
- Each test owns every mutable workspace, home, temporary directory, cache, build, Git, and package state it can affect. Use isolated state rather than shared mutable test state.
- Prove asset identity once through inventories and hashes. Do not assert random embedded prose. Use snapshots only for focused stable projections, while keeping critical invariants and effects as direct assertions.

### Public Artifact Hygiene

- Public artifacts, packages, release notes, logs, and documentation must not expose AI, provider, model, or runtime-orchestration identifiers; internal task, review, or handoff identifiers; hidden prompt or system metadata; local user, machine, or path identifiers; secrets; or tokens.
- Public product, command, schema, version, and artifact identifiers remain allowed when they are part of the accepted public contract, provided they are not identifiers in the prohibited classes above.
