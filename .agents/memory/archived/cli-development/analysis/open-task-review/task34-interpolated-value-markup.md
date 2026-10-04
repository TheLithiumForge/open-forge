---
open-forge:
  description: "Historical record: Review of Task 34 Interpolated value markup, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation"
  tags: [Memory, Analysis, TaskReview, Contextual, Archived, Historical]
---

# Task 34 Interpolated Value Markup Review

## Archive Status

Archived on 2026-10-04 from `.agents/memory/emerging/analysis/open-task-review/task34-interpolated-value-markup.md` after the maintainer selected Memory cleanup. Superseded analytical baseline; preserve original bodies and current residue owners.

This record preserves historical evidence. The [current CLI development route](../../../../working/cli-development/_cli-development.md) and its open Tasks define current work. Local qualification in this record does not establish later integration or publication.

## Question

Is Task 34 still worth doing, what exactly remains, and when should it run?

**Task record:** [Task 34](../../../../working/cli-development/tasks/task34-interpolated-value-markup.md)

## Current Conclusion

**Recommendation:** Do before 1.0.

**Size:** Large, because 947 parameterized text factories and most of the 3,400 captures are affected, even though each edit is mechanical.

The maintainer already accepted the rule, and it fixes a real reading problem. Since the record was written, every user-facing string moved into one project, which makes the pass far cheaper than the record assumes. Record the rule now, so Task 39 and the Task 61 fixes write new messages correctly, then run the sweep after [Task 32](../../tasks/task32-minimal-output-sweep.md) and before [Task 37](../../../../working/cli-development/tasks/task37-wording-review-against-proposals.md).

## What It Implies

Readers can see where a path, identifier, or command ends and the sentence resumes. That matters most for identifiers with spaces, such as `Cannot remove guidance/old guide: ...` in the Route Remove captures. JSON `message` and `headline` fields carry the backticks too, which the ruling intends. For maintenance, the rule becomes one more authoring check for every new message.

## State Today

Not started. The record is accurate about the decision but stale about the code:

- All user-facing text now lives in `src/cli/output-text/OpenForge.Cli.OutputText`, with 2,605 `@OpenForgeText` entries checked by `OutputTextIdentityTests`. 947 factories take parameters. Their parameters are mostly paths (about 355), identifiers (about 125), and generic values (77). Only 12 take a command.
- The `*Wording` classes the record names are now thin adapters in `src/cli/rendering` that call OutputText. `CliTextStyle` moved to `rendering/.../Presentation/Shared/Text/`.
- Some words are still assembled in Rendering. `LibraryDetachWording.cs` passes `"Would"` or `"Updated"` into an OutputText phrase, which produces the "Would the Entries section of" defect that the [Task 61](../../../../working/cli-development/tasks/task61-documentation-accuracy-and-voice.md) review found.
- The record's example is still live: `ConfirmationRequired` renders "Install needs confirmation, and this session cannot ask." The interpolated value is a display name, not the command. Marking `Install` as code would be wrong, because the command is `open-forge install`.
- The acceptance says to record the rule in `00-conventions.md`, which is now archived. A current home is needed.
- 262 contract rows now point at `ContractMessageTemplates.json`, so contract churn lands mostly in that fixture rather than in prose tables.

## Dependencies

- **Blocked by:** a short scope ruling (step 1 below).
- **Blocks:** Task 37, which already says to sequence against this Task.
- **Overlaps with:** [Task 39](../../../../working/cli-development/tasks/task39-output-audit.md), whose E3 to E9 lanes rewrite about 36 cause messages, and the proposed Task for the Task 61 CLI findings. Both write new sentences that should carry the markup from the start.

## Remaining Work

1. Rule on the open scope: paths, identifiers, and echoed input (the record leans yes), listing cells and table columns, `Next:` lines, help syntax lines, and whether a display name such as "Install" becomes the literal command.
2. Record the rule where authors meet it: the [CLI design guidance](../../../../../guidance/cli-design.md) or the Writing Standard's CLI text section, with a pointer from the CLI directive.
3. Decide escaping for a value that contains a backtick. `CliText.Escape` handles control characters only.
4. Move any sentence fragments still composed in Rendering into whole OutputText sentences before marking them.
5. Apply the rule one command family at a time. Regenerate its captures, review each situation, and update `ContractMessageTemplates.json` and the contract rows.
6. Run the four gates.

## Pros And Cons

| Pros                                                                | Cons                                                                |
| ------------------------------------------------------------------- | ------------------------------------------------------------------- |
| Sentences with multi-word identifiers and paths parse on first read | Nearly every capture changes, so review load is high                |
| Agents and people both read backticks as literal values             | Backticks in plain terminal text look like Markdown to some readers |
| One project now holds every string, so the pass is mechanical       | JSON consumers that display `message` will show the markers         |
| Settles an authoring rule before more messages are written          | Churns the same sentences Tasks 37 and 39 also touch                |

## Risks And Open Questions

- Bulk-accepting regenerated captures is the main risk. The record requires per-situation review, and Run 1 showed a reviewed capture can still freeze a false line.
- A sentence that starts with a code span changes capitalization and rhythm. Some sentences will need rewording, not only markup, which edges into Task 37.
- If the scope includes every path in listing rows, token cost rises slightly on large `doctor` and `find` results. The maintainer decides that tradeoff.

## Next Check

**Action:** Classify the 947 parameterized factories by parameter kind and position (sentence or field), and bring the resulting scope table to the maintainer.

**Would change the conclusion:** If the maintainer limits the rule to command names and flags, the Task shrinks to about a dozen factories and becomes Small.

**Acceptance needed:** The maintainer for the scope ruling. The rule itself is already accepted.
