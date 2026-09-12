# Development Extension Proposal

This proposal explores a reusable Development Extension based on methods used
in this repository. It is not an installable package. Validate the methods on
projects with different languages, delivery needs, and consequences of failure
before selecting its contents and dependencies.

## Intended Boundary

The package should help a workspace choose proportionate development rigor,
organize related architecture and implementation, and use standard platform
capabilities where they meet the need. Its methods should work across projects.

It must not contain Open Forge CLI product policy, one language's design rules,
repository-specific working state, or orchestration runtime configuration.

## Candidate Files After Validation

- `.agents/directives/proportional-development.md`
- `.agents/directives/program-architecture.md`
- `.agents/directives/source-locality.md`
- `.agents/guidance/adaptive-design-delivery.md`
- `.agents/workflows/adaptive-development.md`
- `.agents/patterns/software/_software.md`
- `.agents/patterns/software/source-locality/_source-locality.md`
- `.agents/patterns/software/source-locality/nearest-shared-scope.md`

The assured-development Workflow family is a candidate only if continued use
shows that it remains valuable after proportional profile selection. Do not add
it merely because it is more rigorous.

## Relationships To Resolve

The candidates rely on these repository rules and reusable shapes. They are
not all included in the base Framework, so extraction must establish which
generalized sources belong in this package and which require a dependency:

- `.agents/directives/decision-authority.md`
- `.agents/directives/execution-safety.md`
- `.agents/directives/review-evidence.md`
- `.agents/directives/public-facing-writing.md`
- `.agents/guidance/adaptive-collaboration.md`
- `.agents/guidance/calibrated-agent-reasoning.md`
- `.agents/patterns/testing/`

Installed links must make these relationships explicit. Keep each shared
source in one package and use dependencies to make it available. A reference
to a file in this repository does not establish that an installed workspace
will have it.

## Validation Before Extraction

- Exercise the repository's Direct, Standard, Assured, Derivative, and Batch
  profiles on projects with materially different failure consequences. Retain
  only distinctions that prove useful outside this repository.
- Confirm that project criticality and realistic threat models change rigor
  without permitting preventable user-data loss.
- Confirm that ordinary work remains direct and that exceptional machinery is
  surfaced before implementation.
- Confirm that shared-capability promotion follows real consumers rather than
  speculative reuse.
- Resolve overlap with the existing Development Toolkit and Orchestration
  packages before extracting another Workflow or dependency.
- Establish package identity, dependencies, installed routing, update behavior,
  and removal behavior before creating a manifest and `content/` files.
