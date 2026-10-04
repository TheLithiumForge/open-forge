---
open-forge:
  description: "Historical record: Review of the open loader question in Task 60 CLI Skill In Core, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation"
  tags: [Memory, Analysis, TaskReview, Contextual, Archived, Historical]
---

# Task 60 CLI Skill In Core Review

## Archive Status

Archived on 2026-10-04 from `.agents/memory/emerging/analysis/open-task-review/task60-cli-skill.md` after the maintainer selected Memory cleanup. Superseded analytical baseline; preserve original bodies and current residue owners.

This record preserves historical evidence. The [current CLI development route](../../../../working/cli-development/_cli-development.md) and its open Tasks define current work. Local qualification in this record does not establish later integration or publication.

## Question

Should the loader's CLI section shrink or move into the `open-forge-cli` Skill, and when should that be decided? This review covers only that open question.

**Task record:** [Task 60](../../tasks/task60-cli-skill.md)

## Current Conclusion

**Recommendation:** Fold into Task 53. Decide it in the loading audit, with a concrete proposal to shrink the section to a short pointer.

**Size:** Small. It is one loader section, two maintenance contracts, and two documentation passages.

On 2026-09-25 the maintainer kept the command list because not every harness activates native Skills. That reason is weaker than it looks. The Core Skills entrypoint is `#LoadNow`, so every agent that follows the loader sees the `open-forge-cli` entry and its description at startup. Its Axioms say to follow the selected `SKILL.md`. Open Forge routing reaches the Skill in any harness that reads the loader, whether or not the harness activates Skills natively. The [Task 62 analysis](../glob-scoped-loading.md) adds a second path. APM's targets page lists Copilot, Cursor, Codex, Gemini, OpenCode, and Windsurf as reading Skills natively from `.agents/skills/<name>/SKILL.md`, the folder Open Forge already uses. That claim is not yet confirmed against each vendor's documentation. So the loader's list duplicates a source that is already discoverable. It also keeps a second command list that must stay in step with the CLI. A one-sentence pointer keeps the useful part: the CLI is optional, the files stay complete, and the Skill explains the commands.

## What It Implies

- **Users** see no change in behavior. The agent learns the commands from the Skill instead of the loader.
- **Framework:** the loader loses about 900 bytes, roughly 225 tokens, from every session's startup. It keeps its rule that the plain files remain complete without the CLI.
- **Maintenance:** one command list instead of two. The two maintenance contracts that require the list both change.
- **CLI:** no change. Install and Status snapshots record startup size and must be refreshed.

## State Today

Verified against `815324f9`.

- The loader's `### CLI` section has 12 non-empty lines and 908 bytes. The whole loader has 92 non-empty authored lines, above the 35 to 80 review budget in the [loader maintenance contract](../../../../crystallized/documents/maintenance/payload/agents/loader.md). Removing most of the section brings it close to that budget.
- The loader contract requires a `CLI` group with `Applicable Commands` and names the nine commands. The [Skills contract](../../../../crystallized/documents/maintenance/payload/agents/skills.md) states the keep-the-list rationale.
- The README and the site's `cli/index.md` both say that harnesses without native Skill support still get the command list from the loader.
- The [Skill](../../../../../../src/open-forge/.agents/skills/open-forge-cli/SKILL.md) already covers every command in the loader list and more, and defers to `--help` for the installed version.
- The record's other open item is stale as a checkbox. The Install and Status integration snapshots were not refreshed after the Skill was added. Git shows no snapshot changes after `ebf50034`.

## Dependencies

- **Blocked by:** nothing technical. The record ties the decision to evidence about Skill activation. [Task 58](../../../../working/cli-development/tasks/task58-demo-evals.md)'s demo evaluations are the cheapest place to get it.
- **Blocks:** Task 44's loader rewrite, which should know whether this section stays.
- **Overlaps with:** [Task 53](../../../../working/cli-development/tasks/task53-loading-and-scoping-audit.md), which already lists this question. [Task 62](../../../../working/cli-development/tasks/task62-glob-scoped-loading.md) proposes `context --for`, which would grow the loader list if it stays.

## Remaining Work

1. Move the question into Task 53's audit table as a loader row, with this proposal: keep one sentence saying the CLI is optional and the files remain complete without it, and point to the `open-forge-cli` Skill and `open-forge --help`.
2. Optionally run one Task 58 demo with the shortened loader in a harness that does not activate native Skills. Check that the agent opens the Skill when the task needs the CLI.
3. After acceptance, update the loader in source and dogfood copies, both maintenance contracts, the README passage, and `cli/index.md`. Refresh snapshots and measure startup again.
4. Close Task 60 once this and its snapshot refresh are done.

## Pros And Cons

| Pros                                                                   | Cons                                                                                                           |
| ---------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| One command list, so less drift                                        | An agent must take one extra step to see command names                                                         |
| About 225 fewer startup tokens in every workspace                      | Reverses a decision the maintainer made three days ago                                                         |
| Keeps the loader about routing and loading, and nearer its line budget | Harnesses that skip `#LoadNow` routing would lose the list, though they would also miss the rest of the loader |

## Risks And Open Questions

- The maintainer's concern may be agents that read `AGENTS.md` and the loader but do not follow `Entries`. Those agents already miss every other Core rule, so the list does not protect them in any meaningful way. The maintainer should confirm that this is the case they had in mind.
- Keep the one-line principle, "the plain files remain complete without it", in the loader. It is a Framework rule, not CLI help.

## Next Check

**Action:** add the loader CLI row, with the pointer proposal, to Task 53's audit.

**Would change the conclusion:** demo evidence that agents given only the pointer fail to use the CLI where the list would have prompted them.

**Acceptance needed:** the maintainer, since this revises a decision recorded on 2026-09-25 and changes the shipped loader.
