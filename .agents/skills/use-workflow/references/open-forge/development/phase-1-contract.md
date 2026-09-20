---
open-forge:
  description: Define and freeze a callable contract only when an independent Gray boundary materially protects later work
  tags: [Workflow, Development, Phase, Contract, Interface]
---

# Phase 1 - Gray Contract

## Goal

Expose the smallest complete callable production surface required by the accepted task when freezing that surface independently reduces implementation or evidence risk.

## Steps

1. Read the accepted architecture, behavior obligations, baseline, expected paths, protected paths, direct integration neighborhood, and focused compile evidence.
2. Confirm that a new or changed callable contract exists and that a separate Gray boundary adds value. Otherwise record the accepted existing contract and skip mutation.
3. Define focused interfaces, named values, discriminated results, signatures, and dependency boundaries inside accepted placement. Reuse accepted shared contracts.
4. Add only the compilable skeleton needed to expose the surface. Missing behavior must fail explicitly rather than return plausible placeholder data.
5. Run focused compile and contract checks. Record the exact baseline or snapshot that freezes the accepted surface. A local commit may be used when authorized, but is not required by this generic workflow.
6. Return unresolved contract or architecture decisions before Red.

## Completion

- A separate Gray boundary was either justified and frozen or truthfully skipped.
- The callable surface is focused, strict, compilable, and inside accepted architecture.
- No domain behavior, tests, expectations, or unrelated surfaces changed.
- Red can rely on an exact accepted contract baseline.
