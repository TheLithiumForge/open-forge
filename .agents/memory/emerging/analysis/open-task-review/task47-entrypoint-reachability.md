---
open-forge:
  description: Review of Task 47 Entrypoint Reachability and its default Skill indexing follow-up, covering what they imply, dependencies, remaining work, pros and cons, and a recommendation
  tags: [Memory, Analysis, TaskReview, Contextual, Candidate]
---

# Task 47 Entrypoint Reachability Review

## Question

Is Task 47 still worth doing, what exactly remains, and when should it run? This review covers both records.

**Task record:** [Task 47](../../../working/cli-development/tasks/task47-entrypoint-reachability.md) and its [default Skill indexing follow-up](../../../working/cli-development/tasks/task47-default-skill-indexing.md)

## Current Conclusion

**Recommendation:** Do before 1.0. Close the main record as done, keep the follow-up as the single open task, and absorb Task 46 into it.

**Size:** Medium. Topology, Index selection, Doctor, and route inspection must agree on one bounded rule, with contracts and tests, even though the rule itself is small.

The targeted fix is done. When a Skill catalogue is stale, `doctor` names the missing recipe and prints an exact `open-forge index <catalogue>` command that repairs it. What remains is that plain `open-forge index` still reports success while that catalogue is stale. That is the one case where the CLI's own "current" claim is wrong. It sits on an advertised journey, since Workflow Support offers to let users "create your own" workflow. It also sits on the path the Core `open-forge-cli` Skill teaches agents: after editing routed files by hand, run `index`. It should not ship in 1.0 as a known inaccurate result.

## What It Implies

- **Users** who add a recipe run `index` once and get a listed recipe, with no Doctor round trip.
- **CLI:** default Index selection gains one bounded source of roots, and Doctor, `route list`, and `route inspect` report the same reachability.
- **Framework:** the Skills and Workflow Support maintenance contracts state one rule about resources inside a native Skill. Reachability for navigation does not make a resource load at startup.
- **Maintenance:** new contract text and tests in an area with detailed contracts.

## State Today

Verified against `815324f9`.

- **Done:** the N1 to N3 corrections from the [navigation packet](../../../working/cli-development/tasks/beta-follow-ups/task46-47-navigation.md). `DoctorIndexAction.cs` and `DoctorNavigationText.cs` exist. `PublishedDoctorGeneratedNavigationProcessTests.DetachedPlanningRecipeUsesOwningCatalogueIndexAction` covers the journey: add a recipe, see the finding, run the advertised command, and get a clean Doctor. The main record's questions about the useless bare `index` advice are answered.
- **Open:** `IndexSelectionResolver.cs` still starts only from loader roots and entrypoint closures. `SourceRouteTopologyBuilder.cs` lets only recognized entrypoints act as parents, so a native `SKILL.md` ends the chain. Neither file has changed since 2026-09-21. The follow-up records its traversal contract as not started.
- **Open:** the main record's other half is also unanswered. Plain `index` says nothing about detached catalogues it skipped.

The main record reads as still undecided. Its "Earlier questions" are resolved or moved to the follow-up, so it can close.

## Dependencies

- **Blocked by:** a frozen traversal model. That is the first step here, not an external task.
- **Blocks:** Task 60's idea of a routed command reference inside the CLI Skill, which is optional.
- **Overlaps with:**
  - [Task 46](task46-routed-skill-resources.md), to be folded in.
  - [Task 43](../../../archived/cli-development/tasks/task43-workflows-as-skill.md), whose recipe layout created the case.
  - A [Task 61](../../../working/cli-development/tasks/task61-documentation-accuracy-and-voice.md) finding: `extension update` reports the `use-workflow` catalogue's `Entries` as updated when the file doesn't change. Check whether it comes from the same detached topology.
  - [Task 55](../../../archived/cli-development/tasks/task55-alternative-root.md), because a different root may place Skills elsewhere.

## Remaining Work

1. Freeze the model. The smallest candidate is this: for each native Skill exposed through the Skills route or one of its scopes, recognized entrypoints inside that Skill's own folder join default Index as closures rooted at the Skill. `SKILL.md` is never rewritten. Nothing outside the package folder is scanned. Following Markdown links from `SKILL.md` is the alternative. It is more precise, but it depends on authored links.
2. State the metadata rule from Task 46: native `name` and `description` are the Skill's route metadata, and tags may be absent.
3. Decide whether a skipped detached catalogue should also be reported by plain `index`, as a fallback for Skills outside the Skills route.
4. Specify the journey from the follow-up: add a recipe, run plain `index`, see it listed, get a clean `doctor`, and change nothing on a second run. Include no-catalogue, nested, and malformed cases only where the model supports them.
5. Implement it, then update the Index, Doctor, and route contracts, the Skills and Workflow Support maintenance contracts, and the `index` help.
6. Close the main record and Task 46.

## Pros And Cons

| Pros                                                               | Cons                                                                      |
| ------------------------------------------------------------------ | ------------------------------------------------------------------------- |
| Removes the one case where plain `index` reports a false "current" | Changes Index's default write set, which has detailed contracts and tests |
| Makes the advertised own-workflow journey one step                 | A workaround already works, and Doctor gives an exact command             |
| Settles Tasks 46 and 47 with one model                             | The model must hold for Skills from other tools and nested Skills         |

## Risks And Open Questions

- The maintainer chooses between contained descendants and followed links. Contained descendants is simpler and bounded by package size. Links fit Skills whose resources are not entrypoints.
- The wording conflict noted in the Task 46 review must be resolved in the same change. The Workflow Support contract says the resources are not loader-reachable. The Skills contract mentions "loader-to-resource inheritance".
- If the maintainer rates this below 1.0, the interim fix is step 3 alone: plain `index` names the catalogues it skipped. That makes the output honest at a much smaller cost.

## Next Check

**Action:** propose the traversal model and its before-and-after `index` output for the `use-workflow` case, for the maintainer to accept.

**Would change the conclusion:** a decision that the own-workflow journey is not part of the 1.0 promise, which would make the interim reporting fix enough.

**Acceptance needed:** the maintainer, for the model and the CLI contract change.
