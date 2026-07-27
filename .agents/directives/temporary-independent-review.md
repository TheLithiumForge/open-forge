---
open-forge:
  description: Require independent semantic-loss and writing-quality reviews during the current Open Forge migration
  tags: [LoadNow, Directive, Framework, Change, Migration, Review, Temporary]
---

# Temporary Independent Review

## Axioms

- After every Open Forge change or migration, run two independent review agents before presenting the result for maintainer review
- Give the semantic-loss reviewer only the current request, current diff, any additional change-specific context, relevant accepted sources, the [first design baseline session](../memory/archived/sessions/2026-07-26_open-forge-design-baseline.md), the [second design baseline session](../memory/archived/sessions/2026-07-26_open-forge-design-baseline-part-2.md), and access to `feature/initial2`
- Ask the semantic-loss reviewer to identify missing, weakened, contradicted, or accidentally revived meaning without treating old files as automatically correct
- Give the writing reviewer only the current diff, the [Open Forge Writing Standard](../memory/crystallized/documents/maintenance/writing.md), and any source needed to understand the changed prose. Ask it to review clarity, readability, terminology, tone, and conformance without redesigning the accepted behavior.
- Review agents report findings and do not edit files. Resolve each material finding explicitly, then rerun only the affected review when the correction materially changes what that reviewer assessed.
- Keep review prompts narrow and independent from the implementation narrative so reviewers judge the artifacts rather than inherit the implementer's conclusions
- If the active environment cannot launch independent review agents, perform the same two reviews as isolated passes and report the limitation
- Keep this Directive active for the current migration and review cycle. Remove or replace it only after explicit maintainer direction.
