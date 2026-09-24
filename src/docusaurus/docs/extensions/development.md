---
title: Development
description: Development, Debugging, and Review workflow recipes that work with your project's own tools and conventions.
---

# Development

Implement changes, investigate defects, and review results using your project's technologies, rules, and verification tools. No task tracker or record taxonomy is required.

- **Package ID:** `development`
- **Depends on:** [Workflow Support](workflows.md)
- **Needed by:** [Task Coordination](orchestration.md), [Development Toolkit](development-toolkit.md)
- **Loads at startup:** Nothing. Recipes load when the `use-workflow` Skill selects them.

## What it installs

```text
.agents/skills/use-workflow/references/development/
  _development.md       <- recipe scope entrypoint
  development.md        <- implement and verify an accepted change
  debugging.md          <- reproduce, explain, and fix a defect
  review.md             <- return prioritized, evidence-backed findings
```

## What each file is for

### `development.md`

**Goal:** deliver one accepted change that fits the current system, with evidence matched to its behavior and risk.

| Step                               | What the agent does                                                                                                                                          |
| ---------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 1. Establish the boundary          | Carries forward the accepted outcome, scope, non-goals, authorization, and completion evidence. Asks only for a missing consequential decision.              |
| 2. Resolve blocking design choices | Checks whether an unsettled product or architecture question could change the work.                                                                          |
| 3. Inspect the actual system       | Uses your project's tools, commands, and conventions rather than familiar defaults.                                                                          |
| 4. Define a coherent slice         | States the behavior change, what must be preserved, and how it will be verified. Uses test-first ordering when it helps.                                     |
| 5. Implement and check             | Makes the smallest change that fits, then runs the narrowest distinguishing checks.                                                                          |
| 6. Diagnose before repairing       | Classifies a failure (implementation, expectation, setup, environment, unrelated) before changing anything. Never weakens a valid expectation to get a pass. |
| 7. Improve and verify              | Refactors when worth it, then reruns affected and neighboring checks.                                                                                        |
| 8. Close the boundary              | Reports pre-existing failures, residual risk, and gaps.                                                                                                      |

Completion doesn't imply permission to publish or integrate.

### `debugging.md`

**Goal:** connect an observed symptom to an evidenced cause and, when changes are authorized, a minimal verified fix.

The steps: capture the discrepancy, reproduce reliably, locate where behavior first diverges, distinguish competing hypotheses with the cheapest separating check, explain the cause, correct it when authorized, and verify the changed state. An **inconclusive** diagnosis is a valid result. The agent then reports the checks performed, the remaining hypotheses, and the next discriminating check, rather than claiming a cause.

### `review.md`

**Goal:** actionable findings in priority order, or a clear no-findings result, without claiming more than the evidence supports. **Review is read-only by default.**

The agent fixes the review target (baseline, changed and untracked files, claimed evidence), reads enough context, traces consequential failures first (correctness, security, data loss, regressions, broken contracts) over style, checks changes to durable knowledge, challenges each finding against counterevidence, and reports each finding with a stable ID, priority, location, evidence, consequence, and smallest credible fix.

## How to use it

> Use the development workflow for this change. Follow the repository conventions and verify the final state.

> Debug why the export job times out. Don't change anything until we know the cause.

> Review this branch against main.

## Good to know

- Development depends only on Workflow Support. It doesn't install Planning, Project Documents, Observations, or an agent hierarchy.
- The three recipes stay separate methods within the shared selector Skill. They may use several configured Skills and tools, and they don't replace your harness's native behavior.
