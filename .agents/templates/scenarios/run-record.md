---
open-forge:
  description: Record what happened during one run without replacing the intended expectation
  tags: [Extension, Template, Scenario, RunRecord]
---

# {Run Name}

{Copy and adapt. Replace metadata, rebase links, and remove prompts and unused sections. Keep observed failures; do not rewrite the expected result merely to match them.}

## Basis

{Link to the scenarios or flow and the sources defining their expected outcomes. Identify the version or state tried and any conditions that affect the result.}

## Verified Starting State

{Record the relevant materials, inputs, people or roles, and conditions actually prepared. State missing preparation. Use a named fixture when one exists.}

## Actions And Observations

{Record actions and choices in order, what was observed, and what changed between steps. Link to useful captures or artifacts. For command-line work, relevant captures may include stdout, stderr, exit status, and prompts.}

## Independent Checks

{Check the resulting work or state independently of success messages. Record what changed, what stayed intact, and whether the intended outcome was reached. For file operations, this may include content, permissions, and recovery checks; use the equivalent evidence for other subjects.}

## Verdict

| Dimension | Result | Evidence |
| --- | --- | --- |
| Outcome | {pass/fail/blocked/not-run} | {The person's goal or justified stop} |
| Resulting state | {pass/fail/blocked/not-run} | {Observed effects and preservation} |
| Communication | {pass/fail/blocked/not-run} | {What the person understood and the expectation it was checked against} |

## Follow-Up

{Identify the defect, missing preparation, unclear expectation, or proposed change. Keep proposals separate from accepted expectations. Preserve relevant passes, failed branches, and limits on what this run establishes.}
