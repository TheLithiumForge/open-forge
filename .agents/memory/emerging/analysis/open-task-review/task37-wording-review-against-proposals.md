---
open-forge:
  description: Review of Task 37 Wording review against proposals, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation
  tags: [Memory, Analysis, TaskReview, Contextual, Candidate]
---

# Task 37 Wording Review Against Proposals Review

## Question

Is Task 37 still worth doing, what exactly remains, and when should it run?

**Task record:** [Task 37](../../../working/cli-development/tasks/task37-wording-review-against-proposals.md)

## Current Conclusion

**Recommendation:** Needs a maintainer decision first.

**Size:** Large as written, because it compares 2,605 shipped strings with about 5,800 lines of proposals. Medium if narrowed as recommended below.

The goal, better sentences for a stuck reader, is still right. The method has aged badly. The proposals predate the accepted detail names, the G4 implementation, and several rounds of later wording work, so most comparisons would end in "keep" or "not applicable". The [beta ordering](../../../archived/cli-development/tasks/ordering-2026-09-25.md#beta-ordering) says the maintainer holds this Task directly. My recommendation is to replace the proposal-by-proposal comparison with a targeted review of the text that readers hit most and that the beta review found wrong, using the proposals only as a lookup.

## What It Implies

A narrowed review would improve help text, shared message families, and minimal headlines, which are the lines people and agents read first. The full method would mostly produce a ledger of "keep" decisions. That ledger has value as a settled reference, but it would cost more review time than any other open CLI Task.

## State Today

Not started. Verified facts:

- **The inputs are the four sealed proposals** in the Task 30 folder: Astra (1,890 lines), Fable (1,859), Opus (1,369), and Consolidated (701). No fourth maintainer proposal exists in Memory.
- **The proposals are out of date.** Fable still proposes `--detail brief|normal|full`, which decision C1 replaced. They were written before slices 51 and 73 added consequence clauses, before E1 and E2 rewrote manifest errors, and before the Task 45 stage 6 behaviour changes. They cover per-command transcripts and shared rules, not every string.
- **The shipped side is now easy to enumerate.** All user-facing text lives in `src/cli/output-text` with stable `@OpenForgeText` identities: 614 titles, 526 phrases, 510 labels, 394 messages, 279 wording entries, and 217 help entries. The record's instruction to read sentences "from the code and the captures" is now one project.
- **Help text is the weakest area.** The [Task 61](../../../working/cli-development/tasks/task61-documentation-accuracy-and-voice.md) review found `update --help` and `extension update --help` describing `--force` against their own contract, and `extension create --help` claiming that supplying both values avoids prompts. OutputText still holds about 39 lines using words from the internal-vocabulary list, such as `lifecycle`, `payload`, and `projection`. Most are in help text.
- The acceptance's standing references are partly stale. The shared message families sit in the archived G4 conventions, not a current source.

## Dependencies

- **Blocked by:** the maintainer's decision on method, then [Task 32](../../../working/cli-development/tasks/task32-minimal-output-sweep.md) and [Task 34](../../../working/cli-development/tasks/task34-interpolated-value-markup.md), whose rulings the record says win.
- **Blocks:** nothing.
- **Overlaps with:** [Task 39](../../../working/cli-development/tasks/task39-output-audit.md) (truthful messages), the proposed Task for the Task 61 CLI findings (the two false help claims are defects, not taste), and [Task 60](../../../archived/cli-development/tasks/task60-cli-skill.md), whose Skill tells agents how to read results.

## Remaining Work

If the maintainer accepts the narrowed scope:

1. Fix the two false help claims in the Task 61 findings Task, not here, because they contradict a contract.
2. Review the 217 help entries against the command contracts. Remove internal vocabulary and check every claim.
3. Review the shared message families once, as the record already treats them as a higher-bar decision.
4. Review each command's minimal headline set at each status.
5. For each flagged sentence, look up the proposals' sentence for the same situation, then keep, adopt, or merge with a one-line reason.
6. Regenerate and review captures per command, updating contract rows and `ContractMessageTemplates.json`.

If the maintainer keeps the full method, add a first step that filters out proposal sentences whose situation or shape no longer exists.

## Pros And Cons

| Pros                                                    | Cons                                                               |
| ------------------------------------------------------- | ------------------------------------------------------------------ |
| Targets the text the beta review actually found wrong   | The full method mostly yields "keep" at high review cost           |
| Help text is cheap to change and read on every `--help` | Every adopted sentence regenerates captures                        |
| Turns three competing drafts into a settled reference   | Proposals written for an older output shape can mislead a reviewer |
| Stable text identities make each decision traceable     | Overlaps Tasks 34 and 39 on the same sentences                     |

## Risks And Open Questions

- Adopting a proposal sentence that described one of two situations sharing a code breaks "one code, one situation". The record already guards this.
- A wording pass over help text can drift into contract changes. Help must follow the contract, and a disagreement is a contract decision.
- The maintainer decides the method, whether the proposals stay as inputs, and whether this Task stays with the maintainer or goes to an agent.

## Next Check

**Action:** Ask the maintainer to choose between the full comparison and the narrowed review of help, shared families, and headlines.

**Would change the conclusion:** A fourth proposal written against the current output shape would make a full comparison cheaper and more useful.

**Acceptance needed:** The maintainer.
