---
open-forge:
  description: Record what happened during one run without replacing the intended expectation
  tags: [Extension, Template, Planning, RunRecord]
---

# {Run name}

{Copy and adapt. Replace this metadata and remove prompts. Keep observed failures;
do not update the scenario or snapshot just to match them.}

## Basis

{Identify the scenarios or flow, expectation sources, tested product revision, and relevant conditions. Include executable identity and terminal mode for command-line work; omit details that do not affect this run.}

## Verified starting state

{Fixture identity and actual prepared content, permissions and ownership. State missing preparation.}

## Actions and observations

{Actions and choices in order. For a command-line run preserve stdout, stderr, exit, prompts, and each
real state transition. Name the actual capture files; normalize only documented dynamic values.}

## Independent state checks

{Before/after inventories, exact preserved bytes, affected identities, counts,
permissions, ownership and recovery facts. Do not infer these solely from printed output.}

## Verdict

| Dimension | Result | Evidence |
| --- | --- | --- |
| Outcome | {pass/fail/blocked/not-run} | {The user's goal or justified stop} |
| State | {pass/fail/blocked/not-run} | {Actual changes and preservation} |
| Communication | {pass/fail/blocked/not-run} | {Accurate contract-backed explanation} |

## Follow-up

{Implementation defect, wording defect, fixture defect or proposed contract change.
A proposal stays separate until reviewed. Keep unaffected passes and failed branches visible.}
