---
title: Extensions
description: Optional packages of workspace content, what each one is for, how they depend on each other, and how to choose.
---

# Extensions

Extensions are optional packages of workspace content: advice, reusable shapes, workflow recipes, templates, Memory categories, and native Skills. You install the ones that fit your work, and once installed, an Extension is just more files in your workspace that you can adapt like anything else.

An Extension is a way to ship files. Routed files take on the meaning of the route they're installed into. Native files, such as a `SKILL.md`, follow the tool that uses them. Packaging adds no authority, and installing a package doesn't make its methods mandatory.

## Choose by need

| Package                                                   | ID                          | Use it to                                                                                            | Depends on                                       |
| --------------------------------------------------------- | --------------------------- | ---------------------------------------------------------------------------------------------------- | ------------------------------------------------ |
| [Workflow Support](workflows.md)                          | `workflows`                 | Find and follow installed workflows, or write your own                                               | None                                             |
| [Core Templates](core-templates.md)                       | `core-templates`            | Start directives, guidance, patterns, skills, templates, maps, and memory records from starter files | None                                             |
| [Collaboration](collaboration.md)                         | `collaboration`             | Explore ideas, compare alternatives, and clarify decisions                                           | None                                             |
| [Planning](planning.md)                                   | `planning`                  | Record ideas, analysis, and decisions, and organize tasks, plans, backlogs, and checkpoints          | Workflow Support                                 |
| [Project Documents](project-documents.md)                 | `project-documents`         | Write and maintain vision, architecture, principles, and other current documents                     | Workflow Support                                 |
| [Flows and Scenarios](scenarios.md)                       | `scenarios`                 | Describe user journeys and expected outcomes, and record what happened when they were tried          | None                                             |
| [Observations and Handoffs](observations-and-handoffs.md) | `observations-and-handoffs` | Save useful observations and leave a clear snapshot for whoever resumes the work                     | None                                             |
| [Development](development.md)                             | `development`               | Implement, debug, and review using the project's own tools                                           | Workflow Support                                 |
| [Task Coordination](orchestration.md)                     | `orchestration`             | Coordinate related tasks, dependencies, and the combined result                                      | Development, Observations and Handoffs, Planning |
| [Development Toolkit](development-toolkit.md)             | `development-toolkit`       | Install Project Documents, Planning, Flows and Scenarios, and Development together                   | Those four                                       |

**Not sure where to start?** If you work on an existing codebase, start with [Planning](planning.md) for its Decisions: they keep the reasons behind changes, which the code alone rarely records. Core Templates is a good first pick if you want to write your own workspace content. Development Toolkit covers the common document, planning, scenario, and development set. Task Coordination stays outside the Toolkit. Choose it when you coordinate several related tasks and need to verify their combined result.

## Why Extensions exist

Full Core supplies 15 files: `AGENTS.md`, `CLAUDE.md`, the loader, an [entrypoint](../concepts/routing.md#entrypoints) for each of the six [Core categories](../concepts/core-categories.md), the [Memory](../concepts/memory.md) entrypoint with its four state entrypoints, and the `open-forge-cli` Skill. Essentials and Custom follow your [installation choices](../getting-started/installation.md#choose-the-installed-routes). Methods for planning, documenting, developing, or coordinating agents ship as optional packages you choose. Records such as Decisions and Checkpoints, workflow recipes, and starter Templates come from an Extension or from you.

Three rules shape the catalogue:

- **A package needs a clear use.** It ships because it helps an agent do something it wouldn't reliably do on its own: a method that changes the outcome, a record that keeps what would otherwise be lost, or a starter that makes the right shape obvious.
- **Packages follow how people choose.** Things you'd want on their own ship on their own, so you can have Decisions without Task Coordination, or scenario starters without Planning. The Development Toolkit exists so the common set is still one install.
- **Packaging adds no authority.** An installed file means what its route says. The package is only how it arrived, and you can edit or remove any file like your own.

Each package page below starts with why the package exists, then lists every file it installs.

## How they fit together

Arrows point from a package to what it needs:

```text
core-templates            -> (none)
collaboration             -> (none)
scenarios                 -> (none)
observations-and-handoffs -> (none)
workflows                 -> (none)
project-documents         -> workflows
planning                  -> workflows
development               -> workflows
orchestration             -> development, observations-and-handoffs, planning
development-toolkit       -> development, planning, project-documents, scenarios
```

Workflow Support installs once, however many packages need it.

Project Documents and Planning also work together. Decisions record changes, and Documents are kept top-down, from an overview of the whole to the detail of each part. [The document flow](document-flow.md) explains it.

## Where their files land

Every package installs into the same `.agents/` tree as the Framework. Here's where each kind of file goes, and what it means there:

| Kind of file    | Lands in                                            | What it is                                                                                                             |
| --------------- | --------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------- |
| Guidance        | `.agents/guidance/`                                 | Advice for a recurring situation, read on demand.                                                                      |
| Pattern         | `.agents/patterns/`                                 | A reusable shape, read on demand.                                                                                      |
| Template        | `.agents/templates/<folder>/`                       | A starter you copy and adapt, in its package's folder, such as `core/` or `planning/`. Never followed as instructions. |
| Memory category | `.agents/memory/<state>/<category>/`                | An entrypoint that defines a kind of record, such as Decisions or Handoffs. It starts empty.                           |
| Workflow recipe | `.agents/skills/use-workflow/references/<package>/` | A step-by-step method, reached through the `use-workflow` Skill.                                                       |
| Native Skill    | `.agents/skills/<name>/SKILL.md`                    | A capability in your harness's native Skill format.                                                                    |

No first-party Extension file carries a loading tag (`LoadNow` or `KeepInMind`). At startup, an agent sees at most a one-line entry for a package file in an entrypoint that's already loaded, such as Guidance, Patterns, or a Memory state. The file itself opens on demand, when a task selects it.

[Reading installed files](reading-installed-files.md) explains the conventions you'll see inside them, such as `{prompts}` in Templates and the `none` placeholder in empty categories.

## Install, update, remove

Preview everything first:

```sh
open-forge extension list --available
open-forge extension inspect planning
open-forge extension install planning --dry-run
```

The CLI has the first-party catalogue built in, so you don't need a checkout of the repository. It records what it installed in `.agents/open-forge.lock.json` so later updates can tell package files apart from your changes.

The [Extension guide](/guides/extensions) covers installing, updating, removing, installing by hand, and creating your own package.

## None of it is required

Ideas, Analysis, and Decisions are independent conventions, not a pipeline. A clear request can go straight to work. A workflow is a method you can choose, not a stage you must pass. Keep what helps, and remove the rest.
