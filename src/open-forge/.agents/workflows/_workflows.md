---
open-forge:
    description: Repeatable markdown workflow recipes for reaching a defined goal
    tags: [LoadNow, Core, Workflow]
---

# Workflows

Workflows are markdown recipes for reaching a defined goal that takes more than one step or more than one skill.

## Axioms

- Before non-trivial work, infer the established development state from routed current truth and the transition requested by the user, then read `Entries` and load the workflow whose Goal best covers that transition. // Hn:This is also mentioned in the loader, as we mentioned earlier, if a thing is load now, we don't need to declare it above no? E need to focus on encapsulating behavior as much as possible, and making the agent's choices as cheap as possible, the agent should easily just read all the required things directly (let's have a cli command to provide all LoadNow files directly in order so it doesn't trigger multiple read calls, the loader should be minimal in the way of defining the essential things and not repeating itself with the subroutes, but being explicit enough to not throw into confusion anyone, i think we did the same rounded logic with memory too where it should hve been defined either more general or scoped to memory. )
- Recommend at most one prerequisite workflow only when a concrete missing or contradictory input would make the requested transition unreliable; otherwise start at the matching phase without ceremony.
- Default to a clear match and name the active workflow in commentary, handoff, or closeout.
- When several workflows apply, select one primary workflow and express the others as ordered handoffs instead of running ambiguous independent loops.
- When no workflow matches exactly, recommend the closest installed route or routes and direct execution as explicit choices; an explicit user opt-out proceeds directly without another prompt.
- Open Forge workflows are routed markdown recipes, not runtime orchestration objects.
- Every workflow recipe defines `Mode`, `Goal`, `Required Routes`, `Constraints`, `Steps`, `Loop`, `Outputs`, and `Completion`, in that order.
- Every complete workflow carries exactly one primary phase tag: #PhaseDiscovery, #PhaseDefinition, #PhasePlanning, #PhaseDelivery, or #PhaseVerification; other tags remain topical.
- Phases describe increasing commitment, not mandatory chronology. Work may start anywhere, skip, repeat, move backward, or use verification evidence to reopen an earlier phase.
- Every recipe contract section uses a level-2 Markdown heading so validation and Required Routes loading share one unambiguous structure.
- A child `entrypoint` used only to organize descendant workflows may contain category Axioms and Entries without recipe headings. Once an `entrypoint` declares any recipe heading, it declares the complete recipe contract; every non-entrypoint workflow file is a complete recipe.
- `Mode` is `linear` or `iterative`. Every workflow already seeks its Goal; goal-seeking is not a separate mode. //Hn: i think we mixed a bit too much building the workflow with whst the workflow.md file should contain as axioms to work, we can provide cli --workflow or cli new template --workflow or something thst would provide all the options and such, but as axioms here we can probably doscard or compress a lot of info. 
- `Constraints` always exists and states `- none` when no workflow-specific invariant applies.
- Generated `Entries` list what a workflow contains; `Required Routes` list what it needs from elsewhere.
- Read every `Required Routes` route before Step 1; a route that cannot be read is a blocker to report, not a step to skip. "none" is a valid value.
- A step may invoke a skill, consult guidance, delegate to a subagent, or hand off to another workflow by route; a sub-workflow's `Required Routes` are read at that activation.
- Delegation handoffs name the workflow route and the active step so the worker enters the same contract.
- A workflow may own workflow-local #Core routes; they apply only while it is active and are preferred over broader routes when safe and allowed, with unresolved conflicts reported.

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
