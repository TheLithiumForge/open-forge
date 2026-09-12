# C# Development Extension Proposal

This proposal explores a focused C# Extension based on this repository's design
and style rules. It is not an installable package. Validate the rules outside
this repository and settle the shared [Development package](development.md)
boundary before selecting its contents and dependencies.

## Intended Boundary

The package should add focused modern C# design and style rules
to the shared Development methods. It should favor clear typed data flow,
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

The proposed dependency is the future Development Extension. Reuse its
proportionality, architecture, locality, review, and execution-profile sources
instead of copying them.
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
- Establish the dependency, package identity, installed route, update behavior,
  and removal behavior before creating a manifest and `content/` files.
