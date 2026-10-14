---
title: Extensions
description: Optional packages of workspace content, what each one is for, how they depend on each other, and how to choose.
---

# Extensions

Extensions are optional packages of workspace content: advice, reusable shapes, workflow recipes, templates, Memory categories, and native Skills. You install the ones that fit your work, and once installed, an Extension is just more files in your workspace that you can adapt like anything else.

An Extension is a way to ship files. Routed files take on the meaning of the [route](../glossary.md#route) they're installed into. Native files, such as a `SKILL.md`, follow the tool that uses them. Packaging adds no authority, and installing a package doesn't make its methods mandatory.

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

Package files follow your installation choices. Essentials leaves out Templates, so Extension starters are skipped. [Restore the Templates category](#if-your-setup-leaves-out-templates) before choosing a package for its starters.

**Not sure where to start?** If you work on an existing codebase, start with [Planning](planning.md) for its Decisions: they keep the reasons behind changes, which the code alone rarely records. Core Templates is a good first pick if you want to write your own workspace content. Development Toolkit covers the common document, planning, scenario, and development set. Task Coordination stays outside the Toolkit. Choose it when you coordinate several related tasks and need to verify their combined result.

## Why Extensions exist

Full Core supplies 19 Framework Markdown files. They include `AGENTS.md`, `CLAUDE.md`, the loader, six Core category entrypoints, five Memory entrypoints, and the `open-forge-cli` Skill with its four reference files. Essentials and Custom follow your [installation choices](../getting-started/installation.md#choose-the-installed-routes). The CLI also keeps settings and an [ownership record](../glossary.md#ownership-record) for maintenance. Methods for planning, documenting, developing, or coordinating agents ship as optional packages you choose. Records such as Decisions and Checkpoints, workflow recipes, and starter Templates come from an Extension or from you.

Three rules shape the [catalogue](../glossary.md#catalogue):

- **A package needs a clear use.** It ships because it helps an agent do something it wouldn't reliably do on its own: a method that changes the outcome, a record that keeps what would otherwise be lost, or a starter that makes the right shape obvious.
- **Packages follow how people choose.** Things you'd want on their own ship on their own, so you can have Decisions without Task Coordination, or scenario starters without Planning. The Development Toolkit exists so the common set is still one install.
- **Packaging adds no authority.** An installed file means what its route says. The package is only how it arrived, and you can edit or remove any file like your own.

Each package page has an installation step, explains why the package exists, and lists its files.

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

## If your setup leaves out Templates

Package files follow your installation choices. Essentials leaves out Templates, so it skips starter files from Extensions. Core Templates and Flows and Scenarios contain only starters. Planning installs its other enabled content, but skips its starters while Templates is left out.

A fully excluded package is recorded as installed without copying package files. The result names the skipped files and excluded category and suggests a restoration preview.

Plans and receipts also name packages with only some files excluded, including Planning and Workflow Support when their starters are skipped. The guidance previews restoring the category, then updating each affected package. Its commands keep the selected workspace and any local source.

Restore the Templates category first if you want those starters. Preview the restoration, review the plan, then apply:

```sh
open-forge route init templates --framework --dry-run
open-forge route init templates --framework
```

Then preview and install the package:

```sh
open-forge extension install core-templates --dry-run
open-forge extension install core-templates
```

For a script or another non-interactive package install, add `--automatic` to the package command.

If Planning is already installed without its starters, restore Templates using the preview and apply commands above. Then preview its update, review the plan, and apply:

```sh
open-forge extension update planning --dry-run
open-forge extension update planning --automatic
```

Use the affected package's ID and the same `--source` when updating from a local catalogue.

## Install, update, remove

Preview everything first:

```sh
open-forge extension list --available
open-forge extension inspect planning
open-forge extension install planning --dry-run
```

Review the plan. Then apply it:

```sh
open-forge extension install planning
```

For a script or another non-interactive apply, add `--automatic`.

The CLI has the first-party catalogue built in, so you don't need a checkout of the repository. Its [ownership record](../glossary.md#ownership-record), `.agents/open-forge.lock.json`, records what it installed so later updates can tell package files apart from your changes.

The [Extension guide](/guides/extensions) covers installing, updating, removing, installing by hand, and creating your own package.

## None of it is required

Ideas, Analysis, and Decisions are independent conventions, not a pipeline. A clear request can go straight to work. A workflow is a method you can choose, not a stage you must pass. Keep what helps, and remove the rest.

Next: [Read installed files](reading-installed-files.md), or [update and remove Extensions](/guides/extensions#update-and-remove).
