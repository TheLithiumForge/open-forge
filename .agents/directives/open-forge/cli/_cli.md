---
open-forge:
  description: Binding rules for the accepted new Open Forge CLI architecture and implementation scope
  tags: [Directive, CLI, Architecture, Implementation, DotNet, NativeAOT, Testing]
---

# Open Forge CLI Directives

## Axioms

- Select this scope only for the new Open Forge CLI architecture or implementation work. It does not govern the legacy CLI or unrelated Framework work.
- Architecture discussion and maintainer acceptance precede implementation. The accepted Architecture covers libraries, dependencies, source and folder structure, boundaries, tests, physical filesystem behavior, Native AOT, solution, build, package, and CI shape, and the evidence needed to accept them.
- Gate 3 Architecture is accepted, and Gate 4’s current source set is available. Implementation still waits for Gate 5 acceptance. Do not describe this Architecture as open or begin implementation before Gate 5.
- Do not create a speculative source tree or make an unaccepted Architecture choice. The implementation sibling adds the narrower rules that apply after the required acceptance boundary.

## Entries

<!-- open-forge:generated-index:start -->
- [Implement the accepted new Open Forge CLI in C# on .NET 10 or newer with real filesystem boundaries and Native AOT-safe evidence](implementation.md) - #LoadNow #Directive #CLI #Implementation #CSharp #DotNet #NativeAOT #Filesystem #Testing #AOT
<!-- open-forge:generated-index:end -->
