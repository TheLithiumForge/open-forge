---
open-forge:
  description: Historical proposal to package the proven repository Templates separately before they were accepted inside the consolidated development toolkit
  tags: [Memory, Archived, Idea, Contextual, Historical, Template, Extension, Product, Dogfood]
---

# Starter Templates Extension Proposal

## Outcome

The useful selection and admission reasoning from this proposal was accepted, but the proposed standalone package boundary was not.

The nine ready Templates now ship inside the single `development-toolkit` Extension:

- Vision
- Principles
- Architecture
- Maintenance Contract
- Decision
- Idea
- Analysis
- Observation
- Handoff

Operating Context, Strategy, Roadmap, and Project Status remain repository dogfood. Other suggested artifact roles remain unproven rather than automatic gaps.

## Preserved Reasoning

Templates provide copy-ready starting content without imposing a mandatory document suite. An instantiated result belongs to its destination and does not inherit authority or updates from its Template.

The accepted package:

- Removes the Maintenance Contract Template's repository-only Pattern dependency
- Treats the package payload as canonical for shipped Template leaves
- Verifies selected repository dogfood copies against that payload after removing only #Extension provenance
- Tests isolated links, generated indexing, installation, ownership, and removal
- Keeps admission independent for every later Template

The package was consolidated with the lean Workflows and Experience Design Skill because the complete first-party catalogue is intentionally small. The CLI installs that whole package rather than selecting partial components.

## Current Sources

- [Development toolkit decision](../../crystallized/decisions/extensions/development-toolkit.md)
- [Template role](../../crystallized/documents/framework/primitives/templates.md)
- [Template Maintenance contract](../../crystallized/documents/maintenance/payload/agents/templates.md)
- [Extensions MVP Architecture](../../crystallized/documents/extensions/architecture.md)
- [Development toolkit source](../../../../src/extensions/development-toolkit/README.md)
