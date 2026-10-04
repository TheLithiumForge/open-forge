---
open-forge:
  description: "Historical record: Review of Task 40 Capture coverage, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation"
  tags: [Memory, Analysis, TaskReview, Contextual, Archived, Historical]
---

# Task 40 Capture Coverage Review

## Archive Status

Archived on 2026-10-04 from `.agents/memory/emerging/analysis/open-task-review/task40-capture-coverage.md` after the maintainer selected Memory cleanup. Superseded analytical baseline; preserve original bodies and current residue owners.

This record preserves historical evidence. The [current CLI development route](../../../../working/cli-development/_cli-development.md) and its open Tasks define current work. Local qualification in this record does not establish later integration or publication.

## Question

Is Task 40 still worth doing, what exactly remains, and when should it run?

**Task record:** [Task 40](../../../../working/cli-development/tasks/task40-capture-coverage.md)

## Current Conclusion

**Recommendation:** Do after 1.0.

**Size:** Large, because about 400 error-level finding codes still have no capture and many need a seeded workspace.

The gap is real and has not shrunk. It is maintainer insurance, not user polish. The beta review's defects show the limit of this Task. Most were behaviour or truth problems that a capture would have frozen rather than caught. Before 1.0, keep one rule: every defect fix lands with a capture of its corrected situation. Run the systematic sweep after 1.0, starting with Doctor's blocking findings.

## What It Implies

No output changes. The corpus invariants, such as `CapturedMessagesMatchTheirContractRow`, `AssertSubject`, `AssertNextRule`, and `AssertBlockingFindingHasConcreteNextAction`, would start guarding situations they cannot see today. A later wording or rendering change would then show its effect on those situations in review.

## State Today

Not started. I re-measured roughly on 2026-09-28 by matching code-shaped rows in the contract `interface.md` files against code-shaped tokens in every `__snapshots__` file:

- 867 distinct codes appear in contract rows, and about 650 never appear in a capture.
- By severity: 410 of 540 error rows, 195 of 259 warning rows, and 18 of 41 info rows are uncaptured.
- Doctor's own codes are almost entirely uncaptured: 102 of 113.

The method is approximate. Some shared family rows may be exercised under another code, so treat the numbers as the same order as the record's 604 of 813, not as exact. The vocabulary grew since 2026-09-17, and the uncaptured share stayed near 75%.

Two facts arrived after the record:

- [Task 45](../../tasks/task45-end-to-end-observability.md) added 82 journey cases. They assert live JSON and workspace state, not captures, so they do not reduce this gap.
- Slice 72 of [Task 30](../../tasks/task30-cli-experience-remediation.md) found four ways the corpus proves less than it appears to. F-01 rewrites every backslash in text captures, including non-path evidence. F-02 replaces any 64-hex token. F-03 is an unbounded root replacement in two unit helpers. F-04 is four debug captures that leak a temporary path. These are about capture quality, so they belong here.

## Dependencies

- **Blocked by:** nothing. A ruling on F-02 and F-04 is needed before those captures change.
- **Blocks:** nothing directly. [Task 39](../../../../working/cli-development/tasks/task39-output-audit.md) and the invariants gain reach once this lands.
- **Overlaps with:** Task 39 (whether output is good), [Task 41](../../tasks/task41-beta-journey-scenarios.md) (Run 1 showed a reviewed capture can still be false), and the proposed Task for the [Task 61](../../../../working/cli-development/tasks/task61-documentation-accuracy-and-voice.md) CLI findings, whose fixes should each add a capture.

## Remaining Work

1. Before 1.0: require every defect fix to add a capture of the corrected situation, with the expected output written before the run.
2. Turn the count into a checked-in report or test that lists uncaptured contract codes, so the residue stays visible without re-measuring.
3. Take over slice 72's findings. Narrow F-01 to real path separators, fix F-03's boundary, and bring F-02 and F-04 to the maintainer.
4. Capture Doctor's blocking findings first. Slice 51 listed 39 `blocked-repair` rows with no capture.
5. Then capture error rows that carry a next action, command by command, extending existing fixtures.
6. Record each genuinely uncapturable situation, such as a Linux permission path on the Windows host, with its reason.
7. Treat any capture that shows a false claim as a Task 39 finding, not a pass.

## Pros And Cons

| Pros                                                                | Cons                                                        |
| ------------------------------------------------------------------- | ----------------------------------------------------------- |
| The output invariants guard up to four times more of the vocabulary | Large fixture work with no user-visible change              |
| Regressions in rare error paths become visible in review            | Captures freeze whatever the code prints, true or not       |
| Makes the uncaptured residue a known, published list                | Some situations need expensive or platform-specific seeding |
| Fixes normalization rules that can hide real differences            | Raises capture churn for Tasks 34 and 37                    |

## Risks And Open Questions

- Capturing current output without an independent expectation turns defects into contracts. That is Run 1's third failure mode.
- Doing this before Task 34 means regenerating every new capture again. That argues for running it after the markup pass.
- The maintainer decides whether F-02's hash scrub may be narrowed and how F-04's leaked path should be carried or bounded.

## Next Check

**Action:** Add the "every fix lands with its capture" rule to the proposed Task 61 findings Task and to Task 39.

**Would change the conclusion:** A regression in an uncaptured situation reaching users before 1.0 would move the Doctor blocking subset ahead of 1.0.

**Acceptance needed:** The maintainer, for the timing and the F-02 and F-04 rulings.
