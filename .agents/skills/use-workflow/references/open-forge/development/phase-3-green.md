---
open-forge:
  description: Implement the smallest correct behavior against frozen evidence while protecting expectations
  tags: [Workflow, Development, Phase, Green, Implementation, Testing]
---

# Phase 3 - Green

## Goal

Make the frozen evidence pass with the smallest correct implementation of the accepted contract and behavior.

## Steps

1. Read the accepted architecture, callable contract, frozen behavior matrix and evidence, baseline, expected production paths, protected test paths, direct integration neighborhood, and focused commands.
2. Implement only the accepted production behavior. Treat expected paths as forecasts; report a directly required neighboring production path inside accepted meaning. Never cross a protected surface.
3. Run the narrowest failing evidence after each meaningful change. Classify failures before changing artifacts.
4. Keep tests, fixtures, snapshots, and frozen contracts unchanged. Return expectation or contract defects to the earlier boundary instead of correcting them in Green.
5. Apply necessary implementation-local refactoring while evidence remains green. Do not start a broad structural pass.
6. Run focused and direct integration evidence, then record the exact Green baseline and residual risk.

## Completion

- Frozen evidence passes without changed expectation meaning.
- The implementation is the smallest clear correct result inside accepted architecture.
- Direct integration evidence passes and protected surfaces remained unchanged.
- Any unresolved architecture, contract, or evidence defect was returned to its owning boundary.
