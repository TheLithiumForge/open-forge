---
open-forge:
  description: Compared category opening forms and the user choice that guided the applied presentation
  tags: [Memory, Archived, Contextual, Historical, Framework, Review]
---

# Category Openings Proposal

This is an earlier review snapshot retained for the active Task 28 Git review. Its status and paths describe the recorded stage. Use [Task 28](../../cli-development/tasks/source-framework-review.md) for current decisions, completion, and remaining work.

Prepared for the user's release-language discussion. The user accepted option A: each category presents its primary question in its own opening. The revised question-first form is now applied and reviewed after the separately committed language pass. This does not add a metadata field or change runtime behavior.

## Current Sources

The 19 public category entrypoints under `src/open-forge/.agents/` provide `description` and tags. None currently provides `responsibility`. They have short opening definitions rather than a named Description section. The loader is a separate entrypoint, not a category. Two packaged Template category entrypoints complete the 21-category change.

The current [Markdown syntax contract](../../../crystallized/documents/framework/markdown/syntax.md) distinguishes a description used to select a route before loading from an optional responsibility used to understand what an opened file defines. It says category entrypoints normally do not need responsibility because their route, definition, and entries already express it. The [routed Markdown contract](../../../crystallized/documents/framework/markdown/routes.md) includes a compact category definition in the entrypoint shape.

## Accepted Direction

The user likes the current category appearance, finds the primary questions insightful, and wants more self-explanatory meaning in as little space as clarity permits. They chose category openings rather than an overview as the primary home for these questions and delegated the exact presentation.

The user explicitly accepted this distinction. Keep `responsibility` optional and explain the two metadata uses in plain language:

- A description helps a reader decide whether to open the file.
- A responsibility helps an editor decide what belongs in the file.

Omit responsibility when it adds no distinct boundary beyond the description and route. Keep the existing name for now: `purpose` overlaps selection, `scope` already describes where meaning applies, and `defines` is less natural for sources that record or provide starting content.

The user subsequently proposed putting the question before its answer, either beneath the category title or as the title itself. Keep the category name as the level-1 title, use the primary question as a level-2 heading, and put the definition immediately below it. The title preserves the file's identity, while the question introduces its explanation. The heading needs no additional bold formatting:

```markdown
# Directives

## What behavior is required in this scope?

Directives contain required instructions.
```

This is an ordinary Markdown heading. It adds no metadata field or parser-recognized Framework behavior.

Keep any meaning the question cannot carry in a short explanation. In particular, preserve Memory's deliberate self-growing nature, Template independence, and the native Skill boundary. The question describes the category's role; it does not make the entrypoint the source of every answer beneath it.

Preserve the reader/editor distinction in the existing Markdown and maintenance sources, the README, and self-contained shipped context. No rename of `responsibility` is needed. The framework files must remain understandable without the README or repository-only maintenance documents.

The README serves as an introduction to what Open Forge is, provides, and does. The user initially described ACE as the meeting of progressive disclosure and spec-driven development, then requested a stronger definition of its own, inspired by or improving on those ideas. Use professional, friendly language with character to explain what deliberate organization and evolution of workspace context add. Preserve relevant context selection, explicit accepted expectations, applicability beyond software, and proportionate specification. Wording such as “builds on” expresses conceptual development without claiming measured superiority. Both diagram drafts remain deferred.

The earlier language rewrite preserved the existing category convention in commit `2b31f4e4`. Commit `28cac0fc` records the accepted presentation and introductory explanation as a separate follow-up after the current-content cleanup in `e91d2934`. The [focused review](../evidence/category-metadata-ace-review.json) passed after the canonical category-shape list was aligned with the new opening.

## Earlier Recommendation And Comparison

Keep the frontmatter description for route selection and a short declarative definition in the body. Remove repeated filler when the opening adds nothing useful, but preserve the category meaning and any boundary absent from the description. A compact definition at the body entry remains useful even when part of its meaning overlaps the route description.

Use the existing optional `responsibility` only when it adds a distinct boundary for what the file defines. Do not require it on every category entrypoint or add a new question field. Use questions while drafting and reviewing to test whether each category's purpose is clear.

| Element                   | Question it serves                          |
| ------------------------- | ------------------------------------------- |
| `description`             | Does this route matter to the current task? |
| Optional `responsibility` | What does this particular file define?      |
| Opening definition        | What does this category mean?               |

A category file and its contents answer different questions. The Directives entrypoint defines how Directives are selected, scoped, and written. The loaded Directives supply the required behavior for their respective subjects. A responsibility such as “Define all required behavior” would give the entrypoint an inaccurately broad responsibility.

## Earlier Concrete Example

Proposed Directives opening, with the existing metadata description retained:

```markdown
---
open-forge:
  description: Required instructions loaded through selected routes
  tags: [LoadNow, Core, Directive]
---

# Directives

Directives define required behavior within their loaded scope.

## Axioms
```

The existing Axioms and Entries would follow. This sample does not remove or replace them.

If a specialized entrypoint needs an explicit responsibility, use its actual scope, for example “Define how Directives are selected, scoped, and written.” That is different from repeating its description or claiming to define every instruction beneath it.

## Alternatives And Evidence Boundary

- A responsibility on every category file would make the same field visible everywhere, but often repeat what the route and definition already state. This would change the current authoring convention.
- A question followed by an answer in every opening would expose the drafting question, but also repeat a fixed structure whether or not the question helps the reader. An individual explanation can still use a question when it makes the subject clearer.
- Removing all opening definitions would reduce repeated words, but several current openings contain additional meaning. Memory explains deliberate growth, Templates explain independent maintenance, and Skills identify their native rules.

The accepted choice is based on source responsibility, readable structure, and the user's preference. No comparative agent evaluation establishes that question phrasing or an extra metadata field produces better AI outcomes. That remains a possible dogfood experiment, not a claimed result or a release prerequisite.
