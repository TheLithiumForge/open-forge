# Open Forge

**A framework that grows with your project. Plain Markdown. Any agent.**

Open Forge gives your agents a place to keep what they learn: the rules that matter here, the decisions you don't want to explain twice, the shape your code should take. It starts small, loads only what a task needs, and becomes more yours with every piece of work.

## The problem it solves

Capable agents still make avoidable mistakes when the right context is missing, stale, or buried. So you repeat yourself. You explain the same conventions to every new session. You review mistakes a single note would have prevented.

The usual answer is a bigger methodology: more prompts, more mandatory workflows, more ceremony. Most of it is irrelevant to the task in front of you, and all of it costs context.

## Adaptive Context Engineering

We built Open Forge around an idea we call Adaptive Context Engineering, or ACE. It borrows from two things that already work: spec-driven development, which makes goals explicit enough to act on, and progressive disclosure, which reveals detail as it becomes useful.

Applied to a workspace, that means three things:

- A task starts with what you want, in your own words.
- The agent follows routes to the context that applies, and only that context.
- What the work produces gets a home where the next task can find it. A correction. A decision. A finding you'd hate to rediscover.

Context here means the knowledge, rules, decisions, and working state relevant to a goal. It is not every file in the repository.

## Get started

You need a project and an agent that reads `AGENTS.md` or `CLAUDE.md`. Most do.

**Copy the files.** The whole Framework is the `.agents/` folder in [src/open-forge](src/open-forge/) plus a short `AGENTS.md`. Copy `.agents/` into your project, add the `AGENTS.md` handoff to your existing agent instructions, and include `CLAUDE.md` if your harness uses it. Keep your existing files and review the result.

**Or use the CLI.** Follow the [setup guide](docs/development.md#link-the-native-cli-locally), then run this from your project:

```sh
open-forge install --dry-run
open-forge install
```

Check the diff, commit it, and describe the work you want done. Your agent reads `AGENTS.md`, follows the loader, and works within the rules it finds. Installing is a maintenance step you take once. Agents don't repeat it.

## How it fits together

Everything lives under `.agents/`:

```text
.agents/
  loader.md        how to navigate, what wins, where things go
  directives/      what must be followed here
  guidance/        what we recommend, and when it fits
  patterns/        the shapes our work should take
  workflows/       repeatable recipes for defined goals
  templates/       starting files to copy and adapt
  skills/          native SKILL.md capabilities
  maps/            where to find important sources
  memory/          what is worth remembering
    working/       temporary state to resume from
    emerging/      useful, but not accepted yet
    crystallized/  accepted decisions and current documents
    archived/      history that no longer governs
```

Each folder has an entrypoint, `_folder.md`, that lists what's inside with a one-line description. An agent reads the entrypoint, opens what matters, and skips the rest. Rules from a parent stay in force inside its children.

Every category answers one question. Directives: what is required here? Guidance: what is recommended? Memory: what is worth remembering? The [loader](src/open-forge/.agents/loader.md) carries the complete rules, and the Framework explains itself through its own files. Nothing here needs a runtime.

## Grow your own framework

This is the part we care about most.

Your project has its own tools, constraints, and habits. Open Forge gives each of them a place to live and a way to get there:

- A correction you've made twice becomes a Directive, or Guidance when it's advice rather than a rule.
- A structure that makes mistakes easy to spot becomes a Pattern or a Template.
- A method that keeps helping becomes a Workflow.
- A finding worth keeping goes into Memory, where it can be reviewed, accepted, and kept current.

Scopes are what make this practical. Put frontend conventions in a frontend scope and database rules in a database scope. Insert a folder anywhere below a category and it narrows everything beneath it. A task touching both follows both. A task touching neither pays nothing for either.

There's no limit on how many scopes you add or how deep they go. What a task loads follows the routes it selects, not the size of the workspace.

The base is about 8k tokens of Markdown, under 7k at startup, before any of your own content. Those are [measured source counts](docs/development.md#measure-context-size), not a promise about any particular model. Add to it, replace it, or remove what you don't use. Removed defaults stay removed.

## Extensions

Optional packages for when you'd like a head start:

| Package           | Gives you                                                                                 |
| ----------------- | ----------------------------------------------------------------------------------------- |
| Project Documents | Vision and Architecture workflows, with document starters                                 |
| Memory Starters   | Templates for decisions, ideas, analyses, observations, and handoffs                      |
| Planning          | A planning workflow, a work-records pattern, and task, plan, backlog, and checkpoint starters |
| Development       | Development, debugging, and review workflows                                              |
| Orchestration     | Managed delivery across dependent tasks. Brings in Planning and Development               |

`development-toolkit` bundles the first four. Everything installs as ordinary files that follow the rules of wherever they land. The [Extension guide](docs/extensions.md) covers installing, customizing, and writing your own.

## The CLI

The files are complete on their own. The CLI makes the repetitive parts faster:

| Command                                      | Use it to                                        |
| -------------------------------------------- | ------------------------------------------------ |
| `open-forge context`                         | Read what an agent loads at startup              |
| `open-forge route list`                      | See the routes that exist                        |
| `open-forge find --tag=Decision`             | Find records by tag or heading                   |
| `open-forge route init memory/project-alpha` | Create a scoped route with its entrypoints       |
| `open-forge index`                           | Rebuild generated navigation after edits         |
| `open-forge doctor`                          | Check structure without changing anything        |
| `open-forge update --dry-run`                | Preview a Framework update                       |

Anything that changes files takes `--dry-run`. `open-forge --help` and the [CLI guide](docs/cli.md) have the rest.

## Learn more

- [CLI guide](docs/cli.md) for everyday commands and maintenance
- [Extension guide](docs/extensions.md) for packages, dependencies, and customization
- [Development guide](docs/development.md) for building, contributing, and verifying changes

This repository runs on Open Forge. Its own [Vision](.agents/memory/crystallized/documents/vision.md), [Principles](.agents/memory/crystallized/documents/principles.md), and [Architecture](.agents/memory/crystallized/documents/architecture.md) are worked examples of the Framework in use.

[MIT license](LICENSE)
