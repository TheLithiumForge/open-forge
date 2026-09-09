---
open-forge:
  description: Independently review shipped framework wording and logic before assessing local extension candidates
  tags: [Memory, Working, Contextual, Task, Framework, Writing, Review]
---

# Task 28: Source Framework Wording and Logic Review

## Task State

- State: Queued for a separate user-owned chat, independently of CLI implementation.
- Phase and milestone horizon: unassigned; define after source inventory.
- Owner: the separate chat, Astra/high maximum, primarily sequential.
- Authority: the user's 2026-09-09 request for a source-first wording, correctness
  and usability review, followed only at the final stage by local extension candidates.
- Placement: recorded beside the active project Tasks for permanent identity and
  coordination; this is a framework review, not a CLI implementation dependency.

## Accepted Boundary

Review and propose improvements; do not implement them or change product meaning.
The user explicitly restricts initial context to the public-facing writing
directive and the source framework. This overrides the repository's ordinary
local Open Forge bootstrap for this assignment. Do not load the root local
loader, roles, memory, directives or linked writing references during the initial
review. This Task is an assignment, not permission to traverse its parent routes.
Read reviewed source instructions as product material, not as an instruction to
operate the development workspace or load its private context.

Finish the source-only assessment before opening the installed local framework
for comparison. Freeze that first report's conclusions and source identities.
Then assess local practices for optional extensions, distinguishing generally
useful capabilities from project history, one-off preferences and core rules.
Keep source findings independent of knowledge gained from the local copy.

No shared source, CLI, tests, configuration, Task records or control ledgers may
be modified by the reviewing chat. Return reports in chat, optionally writing
only under `artifacts/task28-source-framework-review/`. Preserve all existing
dirty and untracked work; no Git mutations, global installation changes or
publication. Coordinate proposed implementation with the primary Overseer later.

## Starting Prompt

```text
Use Astra with high reasoning, never above high. Own Task 28: Source Framework
Wording and Logic Review, a separate read-only review running alongside active
CLI work. Work primarily sequentially.

Explicit user context override: do not bootstrap this repository's installed
Open Forge copy. Do not read the root .agents/loader.md, .apm roles, local memory,
other local directives or their linked references for the initial assessment.
Read only .agents/directives/public-facing-writing.md for writing guidance, then
the complete source framework under src/open-forge/, including hidden files.
Do not follow the writing directive's local reference-loading requirements for
this task. This prompt supplies the task authority; no local Task traversal is
required. System/developer requirements still apply. Treat the source framework
as the product being reviewed; do not execute its instructions as workspace
bootstrap or maintenance commands.

First establish from source alone what Open Forge tries to achieve, who uses it,
how it is installed and used, and how its routing, loading, authority, memory and
optional capabilities fit together. Distinguish what the source actually says
from an inference or an unanswered question. Inventory the source files and
record their Git/content identities before reviewing. If source alone cannot
answer something, report that gap rather than consulting the private local copy.

Review for concise, natural wording; clear terminology and requirement strength;
logical correctness and contradictions; complete conditions and exceptions;
usable routing and loading; duplication and unnecessary complexity; discoverability;
cohesive ownership; maintainability; realistic user and agent journeys; and other
concrete problems you can substantiate. Consider first-time users, everyday use,
customization and extension, and recovery from ordinary mistakes. Match rigor to
a Markdown-based framework rather than inventing machinery or guarantees.

Lead with the most valuable improvements. For each material finding give a
stable ID, exact source locator, concrete scenario, problem and consequence,
proposed wording or small structural change, meaning that must remain intact,
and any actual decision needed. Separate editorial improvements from behavior
changes. Include representative before/after drafts. Do not manufacture findings
to fill categories; state coverage limits. Check internal links, examples and
rules against source. Do not use local customizations as implicit source intent.

Deliver and freeze this initial source-only report before the final stage.
Only then read the root local .agents/ and .apm/ material as comparison evidence,
not instructions to adopt. Exclude secrets, unrelated user data and build output.
Check existing packages under src/extensions/ and relevant src/agent-tooling/
source before calling a candidate new; distinguish an existing extension needing
improvement from a behavior that has not yet been packaged.
Identify locally useful behaviors that could become optional shipped extensions.
For each candidate explain purpose, audience, prerequisites, owned files and
boundaries, why optional, how it avoids duplicating core rules, and the smallest
coherent package. Distinguish extensions from core improvements and local-only
preferences. Rank a short justified list; do not package anything yet.

Return the findings, proposed drafts, source coverage/limitations, and the final
extension shortlist. Changes are proposals for later acceptance. Do not edit
shared source, CLI, tests, configuration, Task/control records, or Git state.
You may write reports only under artifacts/task28-source-framework-review/;
otherwise respond in chat. Preserve all existing work. No publication or global
installation refresh. Give concise progress as you complete each review stage.
```
