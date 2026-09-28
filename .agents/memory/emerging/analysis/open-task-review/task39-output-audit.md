---
open-forge:
  description: Review of Task 39 Output audit, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation
  tags: [Memory, Analysis, TaskReview, Contextual, Candidate]
---

# Task 39 Output Audit Review

## Question

Is Task 39 still worth doing, what exactly remains, and when should it run?

**Task record:** [Task 39](../../../working/cli-development/tasks/task39-output-audit.md)

## Current Conclusion

**Recommendation:** Do before 1.0.

**Size:** Large, because seven error-fact lanes (E3 to E9) remain and the per-command audit covers all 28 commands.

This is the Task that asks whether each output is true and useful, and the beta review showed that question is still open. The representative manifest fix (E1 and E2) landed, but the shared marker-based cause parser still serves about 40 production call lines. The per-command audit never ran. Run the audit first, using the Run 1 lesson that a claim must be checked against the workspace, not against an earlier capture. Then run the E lanes in order of visible damage.

## What It Implies

Users get errors that name what failed and a next step that works. Agents get findings they can act on without parsing prose. For maintenance, retiring `PlainCause`, `CauseSentence`, and `QuotedPath` removes a classifier that guesses where the useful half of an operating-system string ends.

## State Today

Verified against the tree at `815324f9`:

- **Item 1 is partly done.** E1 and E2 passed their gates, as the [error packet](../../../working/cli-development/tasks/beta-follow-ups/task39-errors.md) and the archived execution record show. E3 to E9 are still forecast lanes. A direct search finds about 41 production lines calling `PlainCause` or `CauseSentence` across 29 files, close to the packet's 36 call sites in 27 files. The marker parser is still in `rendering/.../Presentation/Shared/Wording/CliFindingWording.cs`.
- **Item 2, the per-command verdict, has not started.**
- **Item 3 is still live.** `Presentation/Legacy/` still holds the three `*HelpSections.cs` files. OutputText still has about 39 lines using words from the internal-vocabulary list, mostly in help text.
- **A Run 1 defect survives.** B-5, the doubled resolution line, is still frozen in the Doctor renderer captures at standard, full, and debug. `error-and-warnings.standard.txt` prints "Fix it by hand." twice and "Fix the link by hand." twice.
- **Item 4 belongs to [Task 40](../../../working/cli-development/tasks/task40-capture-coverage.md).** `repair.plan-conflict` is still uncaptured, and 17 integration tests still skip on Windows.
- **Item 5 is closed.** Slice 54 finished the snapshot layout.
- The header is accurate. The mechanism description refers to code that now lives under `src/cli/rendering`, not the old core project.

The [Task 61](../../../working/cli-development/tasks/task61-documentation-accuracy-and-voice.md) review found more output that is false rather than unclear. Dry runs print "Removed" and "created". A Library dry run prints "Would the Entries section of", because `LibraryDetachWording.cs` passes "Would" where it needs "Would update". `extension list` says "no ownership record" on a fresh install. Each of these is what item 2 exists to catch.

## Dependencies

- **Blocked by:** nothing for the audit. Each E lane needs its producer-to-finding map frozen before dispatch, as the packet requires.
- **Blocks:** [Task 37](../../../working/cli-development/tasks/task37-wording-review-against-proposals.md) should not reword sentences that this Task rebuilds from facts.
- **Overlaps with:** [Task 34](../../../working/cli-development/tasks/task34-interpolated-value-markup.md) (new messages should carry the markup), Task 40 (evidence), [Task 41](../../../archived/cli-development/tasks/task41-beta-journey-scenarios.md) (its H1 to H7 checks), and the proposed Task for the Task 61 CLI findings.

## Remaining Work

1. Hand the concrete Task 61 wording defects to the proposed findings Task, so they ship without waiting for this audit.
2. Adopt Task 41's H1 to H7 checks as the per-command checklist. Run each command's main situations in a scratch workspace and check each claim against the files: counts, dry-run writes, subjects, and whether `Next:` works as typed.
3. Record a verdict per command, naming anything that is merely different rather than better.
4. Fix B-5 in the Doctor renderer and regenerate its captures.
5. Re-measure the pathless-cause lines, then freeze and run E3 to E9 in order of visible damage. E5 (Library detach and sync findings with a null path) goes first.
6. Remove the marker parser once no production caller remains.
7. Re-sweep the Legacy help sections, the internal vocabulary, and dead code kept alive only by its own test.
8. Rule on the one-code questions that Task 30 left: `references.invalid-encoding` and `context.closure-unavailable`.

## Pros And Cons

| Pros                                                       | Cons                                                                                |
| ---------------------------------------------------------- | ----------------------------------------------------------------------------------- |
| Catches false output that captures cannot catch            | Large, and most of the cost is careful manual checking                              |
| Retires a fragile string classifier                        | E lanes may need result models to carry facts they lack, which is a contract change |
| Gives every command an explicit quality verdict before 1.0 | Every corrected message regenerates captures and contract rows                      |

## Risks And Open Questions

- A lane that cannot find the failing path in its result model must stop and ask, not fall back to parsing strings. That can add contract decisions to the maintainer's queue.
- The audit may find more defects than fit before 1.0. The maintainer should decide which verdicts block 1.0.
- Whether the audit runs by hand or by agents depends on tooling. Run 1 was manual because the agent fleet was unavailable.

## Next Check

**Action:** Re-count the pathless-cause lines and the remaining callers at the current commit, then write the per-command checklist from H1 to H7.

**Would change the conclusion:** If the re-count finds almost no pathless output left, E3 to E9 become cleanup that can wait until after 1.0, and only the audit stays before it.

**Acceptance needed:** The maintainer, for which verdicts block 1.0 and any result-model changes.
