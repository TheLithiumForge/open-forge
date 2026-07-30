---
open-forge:
  description: Migration record for consolidating the pre-release first-party Extension catalogue into one lean development toolkit
  tags: [Memory, Archived, Session, Contextual, Historical, Extension, Workflow, Skill, Template, Catalogue, Migration]
---

# Development Toolkit Migration

Status: completed on 2026-07-29.

## Accepted Result

The first-party Extension catalogue was reset from many capability, Workflow, pack, and support packages to one dependency-free `development-toolkit`.

The package contains:

- Six direct Workflows for Vision, Architecture, Planning, Development, Debugging, and Review
- One native Experience Design Skill with three focused references
- Nine Templates for Vision, Principles, Architecture, Maintenance Contract, Decision, Idea, Analysis, Observation, and Handoff

The package is one install and ownership unit. The CLI does not provide partial feature selection. Installed files remain ordinary user-owned workspace content.

## Catalogue Reduction

Generic Skills that repeated native agent capability were removed. Workflows without sufficient distinct reusable value were removed. Design recipes were replaced by one native Experience Design Skill. Experimental reliability, CLI-testing, and tool-integration packages were removed from the current catalogue.

Earlier package identities were retired without aliases because the catalogue reset occurred before a stable release. Historical archives and benchmark results retain old identities where they explain past evidence. Current examples, active benchmark scenarios, and CLI messages use `development-toolkit`.

## Workflow Contract Alignment

Selection now begins from visible `description`, tags, `route` meaning, and clear user direction. A loaded `Goal` confirms fit after selection.

A new Workflow earns distribution only when its repeatable recipe materially changes execution, preserves deliberate methodology, or improves reliability beyond ordinary capable-agent behavior. Direct execution remains valid.

Every shipped Workflow contains only:

- `Goal`
- `Steps`
- `Completion`

No shipped Workflow requires another `route`, contains generated `Entries`, or uses a per-Workflow folder.

## Template Boundary

The package payload is canonical for its nine Template leaves. The corresponding repository dogfood leaves are compared after normalizing line endings and removing only the package-specific #Extension tag.

The Maintenance Contract Template no longer links to a repository-only Pattern. Continuing structure belongs to applicable Patterns, Directives, `Axioms`, or another authoritative source. A Template provides starting content only.

## CLI And Lifecycle

Human `chain --heading` output now reports `[heading absent]` instead of `[absent]`. Structured JSON retains the stable internal status.

Extension integration coverage verifies:

- Exactly one advertised package
- Idempotent source indexes
- Complete isolated installation
- One-owner receipt state
- Direct Workflow structure
- Native Skill resources
- Template route installation and parity
- Dry-run behavior
- Default Git checkpoint behavior
- Complete removal while preserving Core route hosts
- Assembled Framework health through `doctor`

## Inputs And Current Sources

- [Workflow rework handoff](../handoffs/2026-07-29_workflow-rework-input.md)
- [Earlier Workflow overhaul inputs](../ideas/2026-07-29_workflow-overhaul-inputs.md)
- [Development toolkit decision](../../crystallized/decisions/development-toolkit.md)
- [Development toolkit package](../../../../src/extensions/development-toolkit/README.md)
- [Extensions MVP Architecture](../../crystallized/documents/extensions/architecture.md)
- [Workflow role](../../crystallized/documents/framework/primitives/workflows.md)
- [Template Maintenance contract](../../crystallized/documents/maintenance/payload/agents/templates.md)

## Verification

- The focused first-party Extension integration suite passed with 9 tests and 183 assertions.
- `bun run test:ci` passed with 25 unit tests and 162 closure tests.
- Root and installable-source `doctor` passed with no errors or warnings.
- The build succeeded.
- The npm package dry-run contained 53 files, including only the consolidated first-party package.
- Generated indexes were rebuilt for the repository and installable source.
- Active retired-id, source punctuation, and shipped Workflow-shape scans passed.
- `git diff --check` passed.
