---
open-forge:
  description: The pre-release first-party catalogue is one deliberately small development toolkit containing six lean Workflows, one Experience Design Skill, and nine Templates
  tags: [Memory, Decision, CurrentTruth, Extension, Workflow, Skill, Template, Catalogue, DevelopmentToolkit]
---

# Development Toolkit Catalogue

## Context

The first-party Extension catalogue had grown into many capability, Workflow, pack, and support packages. Several repeated behavior that capable agents already perform, divided one coherent optional toolkit into maintenance-heavy package boundaries, or represented experimental integrations as current product choices.

The completed Workflow primitive migration made the runtime contract smaller, but it did not by itself decide which optional recipes and capabilities still earned distribution.

## Decision

Before a stable release, consolidate the complete first-party catalogue into one dependency-free Extension with the stable id `development-toolkit`.

The package contains:

- Six direct Workflows for Vision, Architecture, Planning, Development, Debugging, and Review
- One native Experience Design Skill with three focused references
- Nine copy-ready Templates for Vision, Principles, Architecture, Maintenance Contract, Decision, Idea, Analysis, Observation, and Handoff

Each Workflow uses only `Goal`, `Steps`, and `Completion` unless a genuine recipe-specific need earns another heading. The current six have no `Required Routes`, generated `Entries`, per-Workflow folders, or generic capability Skills.

The CLI installs the complete package. It does not select partial features within the package. Installed files remain user-owned, so routes that provide no local value may be removed deliberately after installation.

Earlier first-party package identities are retired without aliases. Existing installed files remain usable, and their receipts remain sufficient for explicit preview and removal without retaining the previous source packages.

## Rationale

Native agent competence should remain the default. A Workflow or Skill earns distribution only when its reusable content materially changes execution, preserves deliberate methodology, or improves reliability beyond ordinary capable-agent behavior.

Vision, Architecture, Planning, Development, Debugging, and Review preserve useful goal-specific judgment without turning normal work into ceremony. Experience Design remains a native Skill because its focused references provide specialized capability beyond a generic recipe.

The nine Templates are already useful, question-oriented starting artifacts. Packaging them with the toolkit keeps the complete catalogue small while preserving their independent Template semantics. Instantiated results never inherit package or Template authority.

One small package is easier to explain, install, test, review, and remove than a dependency graph whose boundaries do not provide independent value. It remains an Extension convenience boundary rather than a runtime abstraction.

## Alternatives And Tradeoffs

- Keeping separate packages would permit finer installation selection but preserve disproportionate catalogue, dependency, documentation, and test surface
- Keeping generic Skills would make familiar actions appear proprietary and duplicate native agent capability
- Keeping every previous Workflow would increase selection cost and maintain recipes without evidence that they improve outcomes
- A standalone Templates Extension would preserve type-based packaging but make the intentionally small catalogue less coherent without changing installed Template meaning
- Partial package selection could reduce installed files but would add another lifecycle and ownership surface to the MVP CLI

The complete package may install routes a particular workspace does not need. Its bounded size and user-owned files make deliberate post-install removal the accepted current tradeoff.

## Consequences

- The first-party catalogue advertises exactly one package
- Package payload and isolated assembly tests cover all advertised content
- The package source is canonical for the nine shipped Template leaves
- Selected repository dogfood Template leaves remain aligned through automated parity verification
- Active examples and benchmark scenarios use `development-toolkit`
- Historical archives and benchmark results may retain earlier package identities as history
- Continued distribution of every Workflow, Skill, and Template remains subject to real-use evidence

## Authoritative Sources

- [Development toolkit package](../../../../src/extensions/development-toolkit/README.md)
- [First-party catalogue](../../../../src/extensions/README.md)
- [Extensions MVP Architecture](../documents/extensions/architecture.md)
- [Workflow role](../documents/framework/primitives/workflows.md)
- [Template Maintenance contract](../documents/maintenance/payload/agents/templates.md)
- [Extension user guide](../../../../docs/extensions.md)

## Decision Relationships

- [Extension package boundary](extension-package-boundary.md)
- [Workflow shape](workflow-shape.md)
- [Templates as a Core primitive](template-primitive.md)
- [Core primitive roles](core-primitives.md)
