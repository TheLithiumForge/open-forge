---
open-forge:
  description: Binding rules for the accepted new Open Forge CLI architecture and implementation scope
  tags: [Directive, CLI, Architecture, Implementation, DotNet, NativeAOT, Testing]
---

# Open Forge CLI Directives

## Axioms

- Select this scope only for the new Open Forge CLI architecture or implementation work. It does not govern the legacy CLI or unrelated Framework work.
- Architecture discussion and maintainer acceptance precede implementation. The Architecture covers libraries, dependencies, source and folder structure, boundaries, tests, physical filesystem behavior, Native AOT, solution, build, package, and CI shape, and the evidence needed to accept them.
- The maintainer superseded the former Gate 3 implementation topology and active Gate 5 route-list program with a greenfield reset under `src/cli/`. The accepted command contracts, current top-down Architecture, and active Plan govern the new program.
- Do not create a speculative source tree or make an unaccepted Architecture choice. Implementation proceeds only through the selected detailed Task after its parent foundation and contracts are ready.

## Entries

- [Capture aggregated sanitized CLI dogfooding evidence that can improve Framework agents, workflows, tools, and routing](dogfooding.md) - #LoadNow #Directive #CLI #Dogfooding #Observation #Review #AgentLearning #Evidence #Privacy
- [Implement the accepted greenfield replacement CLI below src/cli through architecture-owned foundations and closed Tasks](implementation.md) - #LoadNow #Directive #CLI #Implementation #Architecture #Task #CSharp #DotNet #NativeAOT #Filesystem #Testing
