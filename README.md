# Open Forge

Open Forge is a customizable, AI-agnostic framework built around your projects, your tools, and the way you like to work.

Inspired by **spec-driven development** and **progressive disclosure**, we built Open Forge around an idea we call **Adaptive Context Engineering (ACE)**.

Each task starts with a clear sense of what you want to achieve. Relevant details come into view as the work unfolds. What you learn along the way can then become part of the framework, ready to help with the next task.

A useful correction, a workflow that saves you time, or a decision you don't want to explain again can become part of your framework. As it grows, scopes keep the relevant pieces within reach without bringing the whole collection into every task.

The foundation is around 8k tokens of Markdown. From there, make it yours. Keep your favorite tools, add your own ideas, and change the parts that don't fit. We've included a CLI to make maintenance easier, but the files work on their own.

You begin with a useful foundation. What grows from it is yours.

## Get Started

The Framework is complete in [src/open-forge](src/open-forge/). For manual setup, copy its `.agents/` directory into your workspace and add the supplied `AGENTS.md` handoff to your existing agent instructions. Include the `CLAUDE.md` bridge when your harness uses it. Preserve existing files and review the combined result.

For CLI setup, follow the [local setup guide](docs/development.md#link-the-native-cli-locally). Then run these commands from the project you want to equip:

```sh
open-forge install --dry-run
open-forge install
```

Review the files with `git diff`, then commit the installation. Installation and updates are choices you make when maintaining the workspace; agents do not need to repeat them at startup.

Now describe the work you want done. Your agent starts from `AGENTS.md`, reads the loader, follows relevant routes, and works within their rules. Start with the request and add detail as the work calls for it.

## Grow Your Own Framework

Your project has its own tools, constraints, and habits. Open Forge gives them a place to live and a way to evolve:

- A recurring correction can become a Directive or useful Guidance.
- A structure that makes mistakes easier to spot can become a Pattern or Template.
- A method that repeatedly helps can become a Workflow.
- A specialized capability can become a Skill.
- A useful finding or accepted decision can become Memory that supports later work.

Add, adapt, replace, or remove the defaults as your needs change. Open Forge is model and harness agnostic, so you can work with a pool of agents and tools, connect external sources, or build your own capabilities. Each tool keeps its own interface; the Framework gives their shared context a place to live. Development is one use for it, alongside research, design, operations, or whatever your project calls for.

**Scoping is what makes this growth practical.** Put frontend conventions in a frontend scope and database rules in a database scope. A task selects the branches it needs; a task spanning both deliberately follows both. Each scope contains only what helps there.

The Framework has no fixed limit on the number or depth of routed scopes. Active context follows the selected paths and their relationships, so adding knowledge need not mean reading the whole workspace.

The complete base Framework is **about 8.3k tokens**, with **about 6.8k loaded at startup** before project-specific context or Extensions. These are measured source counts, not a context-window requirement for every model. See the [counting method](docs/development.md#measure-context-size).

## What Goes Where

The Framework combines **Core**, which defines routing, loading, and reusable content roles, with **Memory**, which preserves useful context as work evolves.

Core categories each answer a different question:

| Category                                                       | Question it answers                                                         |
| -------------------------------------------------------------- | --------------------------------------------------------------------------- |
| [Directives](src/open-forge/.agents/directives/_directives.md) | What behavior is required in this scope?                                    |
| [Guidance](src/open-forge/.agents/guidance/_guidance.md)       | What approach is recommended, and when does it fit?                         |
| [Patterns](src/open-forge/.agents/patterns/_patterns.md)       | What reusable shape makes related work easy to create and inspect?          |
| [Skills](src/open-forge/.agents/skills/_skills.md)             | Which specialized capability would help with this work?                     |
| [Templates](src/open-forge/.agents/templates/_templates.md)    | What starting content can be copied, adapted, and maintained independently? |
| [Workflows](src/open-forge/.agents/workflows/_workflows.md)    | What repeatable method can help reach this goal?                            |
| [Maps](src/open-forge/.agents/maps/_maps.md)                   | Where is a useful local or external source, and when should it be used?     |

A Pattern gives work a recognizable shape. A Workflow gives it a repeatable method. A Skill provides a specialized capability through its native `SKILL.md` package. Choose the combination that suits the work.

[Memory](src/open-forge/.agents/memory/_memory.md) asks: **What is worth remembering for current or future work?** Its default states separate temporary context, unsettled findings, accepted knowledge, and history:

| State                                                                       | Question it answers                                                         |
| --------------------------------------------------------------------------- | --------------------------------------------------------------------------- |
| [Working](src/open-forge/.agents/memory/working/_working.md)                | What temporary context is needed to continue or resume this work?           |
| [Emerging](src/open-forge/.agents/memory/emerging/_emerging.md)             | What is useful but still unsettled?                                         |
| [Crystallized](src/open-forge/.agents/memory/crystallized/_crystallized.md) | What accepted knowledge should remain current within this scope?            |
| [Archived](src/open-forge/.agents/memory/archived/_archived.md)             | What useful history should remain available without governing current work? |

Records can move as their usefulness changes, without passing through every state. Memory evolves as people and agents save useful findings, review them, bring related knowledge together, and retire what is no longer needed.

Each source defines only its part of the truth. A Decision records what was chosen and why; a current document explains its subject. Required behavior belongs in Directives or inherited Axioms. Recording an instruction in Memory does not turn that record into a Directive. Integrate accepted outcomes into the sources that define them, preserving useful reasoning in Memory.

## Small Metadata, Useful Context

Routed Markdown files use frontmatter to help readers choose and maintain them. For example, a document about a project's architecture might begin with:

```yaml
---
open-forge:
  description: Understand the service boundaries and how requests move through the system
  responsibility: Define the current service structure and dependency boundaries
  tags: [Memory, Document, Architecture]
---
```

A **description** helps a reader decide whether to open the file. An optional **responsibility** helps an editor decide what belongs in it. Tags support selection and search; a few also have defined behavior:

| Tag                           | Meaning                                                                               |
| ----------------------------- | ------------------------------------------------------------------------------------- |
| `LoadNow`                     | Read the entry when its parent route loads                                            |
| `KeepInMind`                  | Read exposed continuity context, then refresh it while its scope stays active         |
| `Contextual`                  | Treat useful context as unaccepted unless applicable authority establishes acceptance |
| `CurrentTruth`                | Identify accepted current state within its stated scope                               |
| `Evergreen`                   | Keep the content aligned with the current state it represents                         |
| `Core`, `Memory`, `Extension` | Identify the part of the Framework or package the content relates to                  |

Tags do not grant authority. Loading tags work through loaded parents and preserve scope. Other tags can describe your own subjects without introducing new loading behavior.

Each routed folder has an **entrypoint**, normally `_{folder-name}.md`, with short links under `Entries`. Those links expose the next useful choices. The [loader](src/open-forge/.agents/loader.md) contains the complete routing and tag rules; the [Markdown reference](.agents/memory/crystallized/documents/framework/markdown/syntax.md#frontmatter) explains the format.

## A CLI Worth Keeping Nearby

The files make Open Forge complete. The CLI makes repeated navigation and maintenance much easier.

| Command                                      | Use it to                                       |
| -------------------------------------------- | ----------------------------------------------- |
| `open-forge context`                         | Read startup context                            |
| `open-forge route list`                      | Explore available routes                        |
| `open-forge find --tag=Decision`             | Find decision records                           |
| `open-forge route init memory/project-alpha` | Create a routed scope                           |
| `open-forge index`                           | Rebuild generated navigation after edits        |
| `open-forge doctor`                          | Check workspace structure without changing it   |
| `open-forge status`                          | See the current workspace and maintenance state |
| `open-forge update --dry-run`                | Preview a Framework update                      |

Use `open-forge --help` or the [CLI guide](docs/cli.md) for more. Commands that support `--dry-run` let you inspect their planned changes before applying them.

### Working Without The CLI

Follow Markdown links, edit the files, and keep affected `Entries` aligned with their destinations. For a small local adjustment, an adjacent `{name}.overwrite.md` can supplement or replace the corresponding content of its base. It shares that file's role, scope, and loading behavior.

The Framework explains itself through its own files. This README and the documentation help you get acquainted with it; its own files contain the rules needed to understand and use it.

## Extensions

Extensions package optional content you can use and customize. The [first-party catalogue](src/extensions/README.md) includes:

- **Project Documents:** establish your project's direction and architecture with workflows and document starters.
- **Memory Starters:** keep useful analysis, decisions, ideas, observations, and handoffs in reusable starting formats.
- **Planning:** organize work with a Planning Workflow, a Work Records Pattern, and Task, Plan, Backlog, and Checkpoint starters. Use only the records your work needs.
- **Development:** follow adaptable workflows for development, debugging, and review.
- **Orchestration:** coordinate dependent tasks and isolated work through Managed Delivery. It brings in Planning and Development.

Choose the pieces that fit, or install **Development Toolkit** to get the first four together. Orchestration remains a separate choice.

Routed files keep their destination's meaning. Native capabilities and support files follow the tools that use them. Packaging does not add authority.

See the [Extension guide](docs/extensions.md) to install a package, copy its files manually, or create your own.

## Go Deeper

- [CLI guide](docs/cli.md): everyday commands and maintenance.
- [Extension guide](docs/extensions.md): packages, dependencies, and customization.
- [Development guide](docs/development.md): local setup, contributions, and verification.

This repository uses Open Forge to maintain Open Forge. Its current documents are also a working example: [Vision](.agents/memory/crystallized/documents/vision.md), [Principles](.agents/memory/crystallized/documents/principles.md), [Framework Architecture](.agents/memory/crystallized/documents/framework/architecture.md), [Memory](.agents/memory/crystallized/documents/framework/memory/model.md), and our [writing guides](.agents/memory/crystallized/documents/maintenance/writing.md#choose-the-voice).

[MIT license](LICENSE).
