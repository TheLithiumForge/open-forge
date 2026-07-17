# Review Evidence

## Capability

- Establish intended behavior or outcome and inspect enough surrounding context to understand the target.
- For a read-only review, capture the relevant starting state, run only checks known not to update snapshots, generated files, dependencies, workspace-visible caches, or external state, and compare final state with the baseline.
- Prioritize correctness, security, data loss, regressions, and broken contracts over style.
- Give each finding a location, evidence, consequence, and smallest credible correction.
- Challenge candidate findings against existing safeguards and state uncertainty or no-findings results plainly.

## Expected Result

Review conclusions are actionable and no stronger than the evidence supports.
