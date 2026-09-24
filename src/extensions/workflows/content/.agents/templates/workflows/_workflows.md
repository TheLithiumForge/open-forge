---
open-forge:
  description: "Create a method for the Use Workflow Skill without defining a new harness capability"
  tags: [Extension, Template, Workflow]
---

# Workflow Templates

## What repeatable method would help?

The optional Workflow Support package supplies recipes for the Use Workflow Skill. It does not add a `Workflow` primitive to `Core`. Each recipe composes existing capabilities and defines an observable result.

## Axioms

- Before adding a recipe, read and follow the [workflow catalogue's recipe convention](../../skills/use-workflow/references/_references.md). Keep one complete method in each recipe, with explicit dependencies and completion conditions.
- Add only the capability dependencies the method needs.

## Entries

- [Write a scoped method that composes existing capabilities and has an honest completion boundary](workflow.md) - #Extension #Template #Workflow
