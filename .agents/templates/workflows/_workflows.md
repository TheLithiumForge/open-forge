---
open-forge:
  description: "Create a method for the Use Workflow Skill without defining a new harness capability"
  tags: [Extension, Template, Workflow]
---

# Workflow Templates

## Axioms

- This Template belongs to the optional Workflows package. It does not establish a root Workflow primitive.
- Follow the [workflow catalogue's recipe convention](../../skills/use-workflow/references/_references.md). Keep one complete method in each recipe, with explicit dependencies and completion conditions.
- Source tags describe a Template. Replace them with truthful recipe metadata and update the selected reference scope's Entries after copying.
- Keep a recipe on demand unless every use of its parent scope genuinely requires it. Add only the capability dependencies the method needs.

## Entries

- [Write a scoped method that composes existing capabilities and has an honest completion boundary](workflow.md) - #Extension #Template #Workflow
