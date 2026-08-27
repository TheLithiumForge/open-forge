# Development

This extension will be developed later.

For now, this directory intentionally contains only this planning file. Do not
add an `extension.json`, payload, catalogue entry, receipt entry, or install
behavior until the generic development sources have been validated across
projects with different criticality, languages, and delivery profiles.

## Intended Boundary

The Development extension should provide project-independent development
judgment and delivery structure. It should help a workspace choose proportionate
rigor, keep architecture and implementation local, and avoid exceptional
machinery when standard platform capabilities satisfy the real need.

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

## Core Relationships To Preserve

The candidate sources currently rely on Core rules that should remain available
through the base Framework or an explicit package dependency:

- `.agents/directives/decision-authority.md`
- `.agents/directives/execution-safety.md`
- `.agents/directives/review-evidence.md`
- `.agents/directives/writing.md`
- `.agents/guidance/adaptive-collaboration.md`
- `.agents/guidance/calibrated-agent-reasoning.md`
- `.agents/patterns/testing/`

Links express these relationships after installation. Do not copy Core files
into this package merely to make the package source self-contained.

## Validation Before Extraction

- Exercise the Direct, Standard, Assured, Derivative, and Batch profiles on
  projects with materially different failure consequences.
- Confirm that project criticality and realistic threat models change rigor
  without permitting preventable user-data loss.
- Confirm that ordinary work remains direct and that exceptional machinery is
  surfaced before implementation.
- Confirm that shared-capability promotion follows real consumers rather than
  speculative reuse.
- Freeze package identity, dependencies, installed routing, update behavior, and
  removal behavior before creating the manifest and payload.
