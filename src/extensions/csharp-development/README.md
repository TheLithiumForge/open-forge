# C# Development

This extension will be developed later.

For now, this directory intentionally contains only this planning file. Do not
add an `extension.json`, payload, catalogue entry, receipt entry, or install
behavior until the C# rules have been validated outside this repository and the
generic Development package boundary is accepted.

## Intended Boundary

The C# Development extension should add focused modern C# design and style rules
to the generic Development extension. It should favor clear typed data flow,
truthful nullability, cohesive call surfaces, source locality, source-generated
serialization, and the documented capabilities of the selected .NET platform.

It must not contain Open Forge CLI command policy, product-specific filesystem
rules, project topology, or generic development material copied from its
dependency.

## Candidate Files After Validation

- `.agents/directives/csharp/_csharp.md`
- `.agents/directives/csharp/design.md`
- `.agents/directives/csharp/style.md`

## Candidate Dependency

Depend on the future Development extension rather than duplicating its
proportionality, architecture, locality, review, or execution-profile sources.
The installed C# entrypoint should retain its current rule that the scope is
selected whenever C# source or tests are authored or reviewed.

## Validation Before Extraction

- Apply the rules to library, CLI, web, test, and Native AOT code without turning
  modern syntax preferences into mandatory novelty.
- Confirm that constructors, object initializers, cohesive inputs, nullability,
  constants, string templates, and model placement remain useful across project
  types.
- Confirm that standard .NET capabilities are checked before custom frameworks,
  reflection paths, native bridges, or compatibility workarounds are proposed.
- Freeze the dependency, package identity, installed route, update behavior, and
  removal behavior before creating the manifest and payload.
