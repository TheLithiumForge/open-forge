---
open-forge:
  description: Review of Task 32 Minimal output sweep, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation
  tags: [Memory, Analysis, TaskReview, Contextual, Candidate]
---

# Task 32 Minimal Output Sweep Review

## Question

Is Task 32 still worth doing, what exactly remains, and when should it run?

**Task record:** [Task 32](../../../working/cli-development/tasks/task32-minimal-output-sweep.md)

## Current Conclusion

**Recommendation:** Do before 1.0.

**Size:** Medium, because the `Next:` survey covers 163 lines in 353 minimal text captures and changes regenerate captures.

Half of the question is already answered by the code, and the record misreads it. The `Workspace:` echo is not printed on every minimal result. The other half, whether each `Next:` line earns its place and is correct, is real and has fresh evidence from the beta. Run the narrowed sweep before [Task 34](../../../working/cli-development/tasks/task34-interpolated-value-markup.md) and [Task 37](../../../working/cli-development/tasks/task37-wording-review-against-proposals.md), because both review the lines this Task decides to keep.

## What It Implies

`minimal` is the default for people and for agents, so the last line decides where most readers go next. A wrong `Next:` costs a person a wasted command and costs an agent a wasted tool call. For maintenance, the sweep would give the shared presentation rules a current home. Today they live only in the archived G4 conventions and in code.

## State Today

- **The echo rule already works as intended.** `CliReportTrimmer` shows `Workspace:` at `minimal` only when `--workspace` was given or the status is blocked, failed, or interrupted. That matches accepted decision C12. The captures show the echo because the test harness passes `--workspace`, as the index `all-current` JSON capture confirms with `selectedBy: explicit-workspace`. A user running `open-forge index` in the workspace sees one line.
- **The record's premise is stale.** "The shared rule makes it three lines" describes the capture harness, not a normal run. The [index contract](../../../crystallized/documents/cli/contracts/index-candidate/interface.md) repeats the same misreading as an "open maintainer question".
- **The acceptance points at an archived file.** Both acceptance paths amend `00-conventions.md`, which moved to Archived Memory on 2026-09-25. Neither [docs/cli.md](../../../../../docs/cli.md) nor any current contract states the echo or `Next:` rule.
- **`Next:` quality is a live problem.** 163 minimal text captures end in a `Next:` line, with 74 distinct lines. The most common is `Next: open-forge doctor` (38 captures). Run 1 of [Task 41](../../../archived/cli-development/tasks/task41-beta-journey-scenarios.md) found a `doctor` suggestion that could not help (B-3). The [Task 61](../../../working/cli-development/tasks/task61-documentation-accuracy-and-voice.md) review found commands blocked by a kept recovery bundle pointing to `doctor`, which reports no problems, instead of `cleanup`. Extension List prints `Next: open-forge extension install <id>` with a placeholder in five captures.
- The Extension Install continuation line after `Next:` still exists in four captures.

## Dependencies

- **Blocked by:** nothing.
- **Blocks:** Task 34 and Task 37 should follow it, since Task 37 already says an existing Task 32 ruling wins over a proposal.
- **Overlaps with:** [Task 39](../../../working/cli-development/tasks/task39-output-audit.md) on whether an action is correct, and with the proposed Task for the Task 61 CLI findings, which owns the behaviour of a kept recovery bundle blocking route commands.

## Remaining Work

1. Record that the echo rule stands as implemented. Correct the Task 32 premise and remove the false open question from the index contract.
2. Confirm that the explicit-workspace echo prints the resolved path. If it does, keep it, because it confirms how a relative path resolved.
3. Put the echo and `Next:` rules in a current source, either a shared contract under `contracts/shared/` or the output section of `docs/cli.md`.
4. List every distinct minimal `Next:` line with its situation. Classify each as a genuine next step, a restatement, a placeholder, or wrong.
5. Fix the wrong ones first. A result blocked by a kept recovery bundle should point to `cleanup`, which the slice 50 ownership matrix names as the owner of admissible deletion.
6. Decide the `<id>` placeholder in Extension List and the Install continuation line.
7. Regenerate changed captures, review each situation, and update the matching contract rows and `ContractMessageTemplates.json`.

## Pros And Cons

| Pros                                                                   | Cons                                                                       |
| ---------------------------------------------------------------------- | -------------------------------------------------------------------------- |
| Fixes the line that steers the next action at the default detail level | Every changed `Next:` regenerates text and JSON captures at several levels |
| Settles a false open question in a current contract                    | The echo half yields no user-visible change                                |
| Gives the presentation rules a current, public home                    | Some `Next:` corrections depend on behaviour fixes owned elsewhere         |

## Risks And Open Questions

- Dropping `Next:` where a person would not need it may remove the one line an agent follows. The rule should favour a correct next action over a shorter frame.
- Correcting a `Next:` without fixing the underlying behaviour can produce advice that is right but still unhelpful. Sequence the recovery-bundle case with the Task 61 behaviour fix.
- The maintainer decides where shared presentation rules live, and whether placeholders are acceptable in a listing's `Next:`.

## Next Check

**Action:** Extract the 74 distinct minimal `Next:` lines with their capture situations into one table, and mark the ones that are wrong.

**Would change the conclusion:** If nearly every line is correct and useful, fold the few wrong ones into the Task 61 findings Task and close Task 32 after steps 1 to 3.

**Acceptance needed:** The maintainer, since the presentation rule is a product choice.
