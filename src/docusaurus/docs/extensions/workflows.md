---
title: Workflow Support
description: The use-workflow Skill, its recipe catalogue, and a Template for writing your own methods.
---

# Workflow Support

Find and follow installed workflows, or write your own. This package supplies the `use-workflow` Skill, which is the one entry point for every repeatable method other packages install.

- **Package ID:** `workflows`
- **Depends on:** Nothing
- **Needed by:** [Planning](planning.md), [Project Documents](project-documents.md), [Development](development.md), and through them [Task Coordination](orchestration.md)
- **Loads at startup:** Nothing. The Skill is discovered by your harness, and recipes load when selected.

## What it installs

```text
.agents/
  skills/use-workflow/
    SKILL.md                      <- native Skill: how to pick and follow a recipe
    references/
      _references.md              <- the recipe catalogue (starts empty)
  templates/workflows/
    _workflows.md                 <- Template category entrypoint
    workflow.md                   <- starter for writing your own recipe
```

## What each file is for

### `skills/use-workflow/SKILL.md`

**Kind:** native Skill. **Used when:** you ask for a workflow, or an installed recipe would clearly improve the task.

This is the selector. It tells the agent to read the catalogue, load only the relevant scope and recipe, check the recipe's `Goal` before proceeding, and prefer the smallest method that fits. If no recipe adds value, the agent works directly.

Once a recipe is selected, the Skill makes the agent follow its required steps, take conditional branches only when their conditions hold, and check `Completion` against the actual result. Finishing a recipe never authorizes new scope or permission to commit, merge, or publish.

### `skills/use-workflow/references/_references.md`

**Kind:** recipe catalogue entrypoint. **Used when:** the Skill is choosing a recipe.

The catalogue defines the recipe convention: every recipe has one non-empty `## Goal`, `## Steps`, and `## Completion`, in that order. Other packages add their recipes as scoped folders below it (`development/`, `planning/`, `project-documents/`, `orchestration/`). Installed alone, it contains no recipes.

The catalogue is reached through the Skill, not through the loader's root routes. After adding or removing recipes, rebuild its `Entries` by naming it explicitly:

```sh
open-forge index .agents/skills/use-workflow/references/_references.md
```

### `templates/workflows/workflow.md`

**Kind:** Template. **Used when:** you want to capture a method of your own.

A starter recipe with `Goal`, `Steps`, and `Completion` sections, plus guidance on placing it in a catalogue scope and keeping tool-specific invocation in the native capability that owns it.

## How to use it

> Use the installed workflow that fits this task. Load only its relevant scope and required context.

> Use the review workflow on this branch.

## Good to know

- Open Forge routing and native harness discovery are separate. Make sure your harness can see `.agents/skills/use-workflow/SKILL.md` through its own Skill discovery, and that relative resource links resolve.
- The package installs no hooks, execution engine, tool grants, or provider-specific agents.
- A missing or deliberately removed recipe isn't permission to reinstall it.
