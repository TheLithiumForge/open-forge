---
title: Extensions
description: Optional packages of workspace content, what each one is for, how they depend on each other, and how to choose.
---

# Extensions

Extensions are optional packages of workspace content: advice, reusable shapes, workflow recipes, templates, Memory categories, and native Skills. You install the ones that fit your work, and once installed, an Extension is just more files in your workspace that you can adapt like anything else.

An Extension is a way to ship files. Routed files take on the meaning of the route they're installed into. Native files, such as a `SKILL.md`, follow the tool that uses them. Packaging adds no authority, and installing a package doesn't make its methods mandatory.

## Why Extensions exist

The base Framework carries only what every workspace needs: routing, loading, the content roles, and a place to remember things. Methods for planning, documenting, developing, or coordinating agents are useful, but not everyone wants them, and everything in the base is something every workspace installs and every task has in view. So they ship as packages you choose.

Three rules shape the catalogue:

- **A package has to earn its place.** It ships because it helps an agent do something it wouldn't reliably do on its own: a method that changes the outcome, a record that keeps what would otherwise be lost, or a starter that makes the right shape obvious. Generic reminders a capable agent already follows don't make the cut, which is why an earlier Experience Design Skill was dropped from the catalogue.
- **Packages follow how people choose.** Things you'd want on their own ship on their own, so you can take Decisions without task coordination, or scenarios without a planning method. The Development Toolkit exists so the common set is still one install.
- **Packaging adds no authority.** An installed file means what its route says. The package is only how it arrived, and you can edit or remove any file like your own.

Each package page below starts with why the package exists, then lists every file it installs.

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

**Not sure where to start?** If you work on an existing codebase, start with [Planning](planning.md) for its Decisions: they keep the reasons behind changes, which is what gets lost first. Core Templates is a good first pick if you want to write your own workspace content. Development Toolkit covers the common document, planning, scenario, and development set. Task Coordination stays a separate choice, because coordinating many agents is a bigger commitment to one way of working.

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

| Kind of file    | Lands in                                            | What it is                                                                                   |
| --------------- | --------------------------------------------------- | -------------------------------------------------------------------------------------------- |
| Guidance        | `.agents/guidance/`                                 | Advice for a recurring situation, read on demand.                                            |
| Pattern         | `.agents/patterns/`                                 | A reusable shape, read on demand.                                                            |
| Template        | `.agents/templates/<package>/`                      | A starter you copy and adapt. Never loaded as instructions.                                  |
| Memory category | `.agents/memory/<state>/<category>/`                | An entrypoint that defines a kind of record, such as Decisions or Handoffs. It starts empty. |
| Workflow recipe | `.agents/skills/use-workflow/references/<package>/` | A step-by-step method, reached through the `use-workflow` Skill.                             |
| Native Skill    | `.agents/skills/<name>/SKILL.md`                    | A capability in your harness's native Skill format.                                          |

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
