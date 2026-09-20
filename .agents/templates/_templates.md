---
open-forge:
  description: Copy-ready files for starting independently maintained workspace content
  tags: [Core, Template]
---

# Templates

## What starting content can be copied, adapted, and maintained independently?

Templates are reusable starting files. Copy and adapt a Template, then maintain the result independently.

## Axioms

### Selection And Use

- Check `Entries` when copy-ready starting content would help with a new artifact.
- Choose the most relevant Template. Copy and adapt only what the destination needs.
- Replace metadata and placeholders so they describe the destination's ownership, scope, state, authority, and relationships. Remove Template and package-provenance tags from independently maintained copies. Rebase relative links for the destination.
- Later Template changes do not update existing copies.
- If the new artifact needs continuing guidance or requirements, link the source that defines them. The Template provides only starting content.

### Catalog Maintenance

- State the need and primary question each Template answers in its description and source instructions.
- Treat generic Templates as fallbacks. Add a specialized Template only when it provides meaningfully different starting content.
- Users may edit, replace, scope, or remove Templates. Removed defaults stay removed unless the user asks to restore them.

## Entries

- [Explore a choice before committing to a direction](collaboration/_collaboration.md) - #Extension #Template #Collaboration

- [Copy-ready starting files for CLI documents, including command-local contract sets](cli/_cli.md) - #Template #CLI #Command #Contract #Interface #Behavior #TechnicalDesign
- [Start one current explanation, Vision, Architecture, Principles, or Maintenance Contract](documents/_documents.md) - #Extension #Template #Document
- [Workspace-owned starting structures for this project's Memory records](memory/_memory.md) - #Template #Memory
- [Preserve useful execution evidence or prepare a fixed snapshot for an actual transfer](orchestration/_orchestration.md) - #Extension #Template #Orchestration #Memory
- [Choose a starter for a possibility, investigation, accepted choice, or active work](planning/_planning.md) - #Extension #Template #Planning #Memory
- [Create a method for the Use Workflow Skill without defining a new harness capability](workflows/_workflows.md) - #Extension #Template #Workflow
