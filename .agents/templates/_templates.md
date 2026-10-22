---
open-forge:
  description: Starting files to copy when creating something new
  tags: [Core, Template]
---

# Templates

## What starting content can be copied, adapted, and maintained independently?

This folder holds starting files to copy, adapt and then maintain on their own.

## Axioms

### Using a Template

- Check these Entries when copy-ready starting content would help with a new file.
- Choose the most relevant Template. Keep only the sections and detail the copy needs, while preserving its required structure.
- Replace placeholders and metadata so they describe the copy's ownership, scope, state, authority and relationships, in the workspace's frontmatter form. A native file keeps its own metadata. Remove the #Template tag unless the copy is another Template, and remove package tags that do not describe it.
- Rebase relative links for the copy. Later Template changes do not update existing copies.
- If the copy needs continuing advice or requirements, link the source that defines them. A Template supplies starting content only.

### Writing a Template

- State the need and the main question each Template answers in its description and instructions.
- Ask for necessary content once, in a readable order, and mark optional sections clearly.
- Treat generic Templates as fallbacks. Add a specialized Template only when its starting content is meaningfully different.

## Entries

- [Copy-ready starting files for CLI documents, including command-local contract sets](cli/_cli.md) - #Template #CLI #Command #Contract #Interface #Behavior #TechnicalDesign
- [Explore a choice before committing to a direction](collaboration/_collaboration.md) - #Extension #Template #Collaboration
- [Start a Directive, Guidance, Pattern, Skill, Template, Map, or Memory record](core/_core.md) - #Extension #Template
- [Start one current explanation, Vision, Architecture, Principles, or Maintenance Contract](documents/_documents.md) - #Extension #Template #Document
- [Workspace-owned starting structures for this project's Memory records](memory/_memory.md) - #Template #Memory
- [Preserve useful execution evidence or prepare a fixed snapshot for an actual transfer](observations-and-handoffs/_observations-and-handoffs.md) - #Extension #Template #Memory #Observation #Handoff
- [Choose a starter for a possibility, investigation, accepted choice, or active work](planning/_planning.md) - #Extension #Template #Planning #Memory
- [Plan user flows and expected outcomes, group scenarios, and record actual runs](scenarios/_scenarios.md) - #Extension #Template #UserFlow #Scenario #ScenarioCollection #RunRecord
- [Create a method for the Use Workflow Skill without defining a new harness capability](workflows/_workflows.md) - #Extension #Template #Workflow
