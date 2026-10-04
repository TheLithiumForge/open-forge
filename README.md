# Open Forge

Open Forge is a small Markdown framework for working with AI agents, built around your projects, your tools, and the way you like to work. It gives an agent a map to your rules and project knowledge, and a place to keep useful outcomes for the next task.

At its core, it's Markdown, rules, and links. Install it in your project, then ask your agent to work as usual. Add a rule when a correction keeps coming up, link to knowledge you already have, and keep useful outcomes for later. You can add optional Extensions when you need them.

## Get started

With Node.js 22.18 or later, install the CLI and run it from your project:

```sh
npm install -g @thelithiumforge/open-forge@beta
cd /path/to/your-project
open-forge install
```

Choose Essentials for a smaller start, Full Core for every built-in route, or Custom to choose each one. The installer shows your selected plan and asks before applying it. Review the new files with `git status` and `git diff`, then commit the result.

Essentials includes Directives, Patterns, Skills, Emerging and Crystallized Memory, plus Working Memory with its whole directory Git-ignored. Working still loads normally, but new local records stay out of ordinary Git commits. Use `open-forge install --configure` to revisit an existing setup. The [installation guide](https://thelithiumforge.github.io/open-forge/docs/getting-started/installation) explains all choices and unattended commands.

Now give your agent a normal task. For a small first try in an existing project:

> Explain how this project runs its tests. Follow `AGENTS.md`, and name the Open Forge files you read.

That's enough to start. The [ten-minute guide](https://thelithiumforge.github.io/open-forge/docs/getting-started/ten-minute-guide) walks through adding a rule, keeping a useful fact, and finding the context for your next task. You don't need to choose a workflow, install Extensions, or write a specification first. The [demos](demos/) give you a project to try it on.

<details>
<summary>Other installation methods and setup details</summary>

The CLI is optional. Use a native release without Node.js, build from source, or copy the Framework files manually.

### Install the CLI

#### From npm

The quick start above uses the npm package. It needs Node.js 22.18 or later.

#### From a GitHub release

No Node.js needed. Download `open-forge-<version>-<platform>.tar.gz` from the [releases page](https://github.com/TheLithiumForge/open-forge/releases), where the platform is `linux-x64`, `linux-arm64`, `osx-x64`, `osx-arm64`, `win-x64`, or `win-arm64`. Check it against the release's `SHA256SUMS` and extract it. For example, on Linux:

```sh
sha256sum --check --ignore-missing SHA256SUMS
tar -xzf open-forge-<version>-linux-x64.tar.gz
```

The archive holds a single `open-forge` executable (`open-forge.exe` on Windows) and its license. Put it on your `PATH`, or keep it in your repository, for example as `tools/open-forge`, and run it from there as `./tools/open-forge`.

#### From source

Clone this repository and follow the [local setup guide](docs/development.md#link-the-native-cli-locally) to build and link the CLI.

### Install the Framework

The CLI selects built-in routes and records what it manages, so it can update those files later. A manual copy supplies Full Core, which you can adapt by hand.

#### With the CLI

From your project (use `./tools/open-forge` instead if you kept the binary in your repository):

```sh
open-forge install --preset essentials --dry-run
open-forge install --preset essentials
```

The dry run previews that exact selection without asking setup questions. Use `full-core` for every built-in route. An explicit preset on an installed workspace also needs `--configure`. For Custom rows and noninteractive configuration, see the [CLI reference](docs/cli.md#setup-choices).

#### Manually, from a clone

No CLI needed. The Framework is complete in [src/open-forge](src/open-forge/). Clone this repository and copy it into your project:

```sh
git clone https://github.com/TheLithiumForge/open-forge.git
cp -R open-forge/src/open-forge/.agents /path/to/your-project/
cat open-forge/src/open-forge/AGENTS.md >> /path/to/your-project/AGENTS.md
```

If your harness reads `CLAUDE.md`, append `open-forge/src/open-forge/CLAUDE.md` to your project's `CLAUDE.md` as well. If the project already has a `.agents/` folder, merge the two by hand instead of copying over it.

### After installing

Whichever route you took, review the result with `git status` and `git diff`, then commit it. A fresh install adds new files, which `git status` lists and `git diff` alone doesn't show. That's the whole setup. Installing and updating are things you do when maintaining the workspace. Agents don't repeat them at startup.

Now give your agent a real task. It starts at `AGENTS.md`, reads the loader and the entrypoints marked for startup, then opens only the routes relevant to the task and works within their rules. You don't need a spec up front. Start with the request and add detail as the work calls for it.

Adding it to an existing codebase? Point a Map at the docs you already have, and record Decisions (from the Planning Extension) starting with your next change. Choices made before you adopted Open Forge don't need to become Decisions after the fact, but every change from now on can keep its reasons. The [new or existing project guide](https://thelithiumforge.github.io/open-forge/docs/getting-started/greenfield-and-brownfield) covers both cases, and the [demos](demos/) let you try each one.

</details>

## How it works

```text
You ask for a task
        |
AGENTS.md -> loader
        |
Startup rules + relevant links
        |
Work, keep useful outcomes
```

An agent doesn't need to know everything, but it does need to know where everything is. Each context folder has an `entrypoint`, a short Markdown file that lists what's inside. Startup reads the loader and the entrypoints tagged to load, plus the files their tags or rules require. The rest opens when a task needs it. Scopes keep specialized context with the work it applies to.

I call this **Adaptive Context Engineering (ACE)**. It takes progressive disclosure and spec-driven development and applies them to the workspace itself. Start with what you want to achieve, add detail as the work unfolds, and keep what's worth knowing next time.

Full Core supplies 15 Markdown files. Essentials selects fewer routes, and Custom lets you choose. Startup reads the installed entrypoints tagged to load and the files their rules require. The files work on their own. The optional CLI speeds up maintenance.

I built Open Forge to work beside your harness, the tool that runs your agent. Anything that reads `AGENTS.md` (or the `CLAUDE.md` bridge) can use it. Keep your hooks, custom agents, and tools. If you want to define them once for several harnesses, a translator such as [APM](https://github.com/microsoft/apm) can install them where each harness looks for them.

## What you get

This is the Full Core inventory. Essentials omits Guidance, Maps, Templates, and Archived Memory, and Custom follows your selection. Neither preset installs optional Extensions.

```text
AGENTS.md                         <- tells the agent to read the loader first
CLAUDE.md                         <- bridge for harnesses that read CLAUDE.md
.agents/
  loader.md                       <- routing and loading rules, and the root routes
  directives/_directives.md       <- required behavior
  guidance/_guidance.md           <- advice for recurring choices
  patterns/_patterns.md           <- reusable shapes for code, files, and documents
  skills/_skills.md               <- native SKILL.md capabilities
  skills/open-forge-cli/SKILL.md  <- how to use the CLI, read at startup
  templates/_templates.md         <- copy-ready starting files
  maps/_maps.md                   <- pointers to important local and external sources
  memory/
    _memory.md                    <- what is worth remembering
    working/_working.md           <- temporary state for active work
    emerging/_emerging.md         <- useful, but not settled yet
    crystallized/_crystallized.md <- accepted knowledge that stays current
    archived/_archived.md         <- history that no longer governs current work
```

Each installed entrypoint says what its category is for and the rules for using it, then lists what's inside under `Entries`. Apart from Memory state entrypoints and the `open-forge-cli` Skill, every selected category starts empty. The content comes from your work, or from Extensions you choose.

In Full Core, startup reads `AGENTS.md`, the loader, every entrypoint above except Templates and Archived, and the CLI usage guide required by Skills. Other linked files open when selected, tagged to load, or explicitly required by a loaded rule. Essentials and Custom use the same loading rules for the routes present. Git-ignored content remains readable and indexed.

Read these files once now, and again after each update. They're short, and they become the instructions your agents are asked to follow.

They're also yours. Add, adapt, replace, or remove the defaults as your needs change. A removed default stays removed unless you ask to restore it.

Use `install --configure` to change the selected defaults later. Custom's Remove choice keeps existing files and notes routable while releasing their Framework management. Explicit configuration can restore eligible missing packaged scaffolding after a checkout, but missing private records need your own copy or backup. The [growth guide](https://thelithiumforge.github.io/open-forge/docs/getting-started/grow-your-framework) covers changing choices and adding one omitted category.

Edit a shipped file directly when you want a different version. Keep in mind that `open-forge update` brings changed Framework files back to the current version and reports each one it replaces. For a local change that should survive updates, put it in an adjacent `{name}.overwrite.md` instead. It loads right after its base file and shares that file's role, scope, and loading behavior. Where the two answer the same question, the overwrite wins, and updates leave it alone.

## Grow your own framework

Say your agent keeps calling work done without running the tests. Create `.agents/directives/testing.md`:

```md
---
open-forge:
  description: Run the tests before calling a change done
  tags: [LoadNow, Directive, Testing]
---

# Testing

## Instructions

- Run the test suite before reporting a change as done, and include the result.
```

Then run `open-forge index`, or replace the `none` placeholder under `Entries` in `.agents/directives/_directives.md` with the line it would generate:

```md
- [Run the tests before calling a change done](testing.md) - #LoadNow #Directive #Testing
```

A Directive at the root of `directives/` must carry `LoadNow`, which tells the agent to read it at the start of every task.

The same move works for everything your project keeps teaching you:

- A correction you keep repeating can become a Directive, or Guidance if it's advice rather than a rule.
- A structure that makes mistakes easy to spot can become a Pattern or a Template.
- A method worth repeating can become a workflow recipe behind the `use-workflow` Skill, from the Workflow Support Extension.
- A specialized capability can become a Skill.
- A useful finding or an accepted decision can become Memory that later work builds on.

**Scopes keep this growth cheap.** Put frontend conventions in a frontend scope and database rules in a database scope. A task selects the branches it needs, and a task that spans both follows both on purpose. Each scope holds only what helps there. There's no fixed limit on how many scopes you add or how deep they go. Active context follows the selected routes and their links, so adding knowledge doesn't mean every task reads more of it.

## Why it works this way

ACE combines two ideas that already work well:

- **Progressive disclosure:** show the next useful level of detail, not everything at once.
- **Spec-driven development:** make goals, constraints, and expected results explicit enough to guide the work and check the result.

ACE applies both to the workspace itself. A task starts from what you want to achieve. As the work unfolds, the agent pulls in what it needs, such as the relevant architecture, your local conventions, or the decision you made last month, and unrelated history stays out. What you learn along the way can become part of the framework, so the next task starts from a better place. Context isn't every file in the repository. It's the knowledge, rules, and current state a task actually needs.

It's plain Markdown on purpose. AI made code cheap to write, and review became the bottleneck, so the framework you work in should be easy to read, diff, and version with the rest of your project. There's no hidden database, no agent runtime, and nothing specific to one vendor.

It's small on purpose too. Big predefined methodologies assume everyone works the same way, and nobody does. My use cases differ from yours, and yours differ from the next person's. Open Forge gives you a handful of rules and a structure that scales, then leaves the rest to you.

That's also why the base stays small and specialized capabilities live in Extensions. Core and Memory cover what every workspace needs: routing, loading, a few content roles, and a place to remember things. Methods for planning, development, project documents, or coordinating several agents are optional packages. You install the ones that fit your work and skip the rest. Once installed, an Extension is just more files in your workspace, and you can adapt it like anything else.

It also works with a mix of agents and tools. Each tool keeps its own interface, and the Framework gives their shared context a place to live. Development is its proving ground, not its boundary. The same structure can support research, design, operations, a personal knowledge vault, or several projects managed together.

## What goes where

The Framework has two parts. **Core** defines routing, loading, and a few reusable content roles. **Memory** keeps useful context as the work evolves.

<details>
<summary>See the full framework map</summary>

<p align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="src/docusaurus/static/img/framework-map-dark.svg">
    <img alt="Core holds six categories for how work gets done. Memory holds four states for useful context. The map distinguishes startup from on-demand reading. Skills requires its CLI usage guide at startup. Other linked files open when selected, tagged to load, or required by a loaded rule. Dashed examples come from optional Extensions." src="src/docusaurus/static/img/framework-map-light.svg" width="900">
  </picture>
</p>

The [loading guide](https://thelithiumforge.github.io/open-forge/docs/concepts/loading-and-tags) explains the loading tags and file conditions step by step.

</details>

Each Core category answers one question:

| Category                                                       | Question it answers                                                         |
| -------------------------------------------------------------- | --------------------------------------------------------------------------- |
| [Directives](src/open-forge/.agents/directives/_directives.md) | What behavior is required in this scope?                                    |
| [Guidance](src/open-forge/.agents/guidance/_guidance.md)       | What approach is recommended, and when does it fit?                         |
| [Patterns](src/open-forge/.agents/patterns/_patterns.md)       | What reusable shape makes related work easy to create and inspect?          |
| [Skills](src/open-forge/.agents/skills/_skills.md)             | Which specialized capability would help with this work?                     |
| [Templates](src/open-forge/.agents/templates/_templates.md)    | What starting content can be copied, adapted, and maintained independently? |
| [Maps](src/open-forge/.agents/maps/_maps.md)                   | Where is a useful local or external source, and when should it be used?     |

Patterns do more than they seem to. Work that follows the local Pattern is easier to review, and work that departs from it stands out.

A Skill provides a specialized capability through its native `SKILL.md` package. Repeatable methods aren't a Core category. The optional Workflow Support Extension adds a `use-workflow` Skill whose catalogue holds them. Combine whatever suits the work.

[Memory](src/open-forge/.agents/memory/_memory.md) asks: **What is worth remembering for current or future work?** Its default states separate temporary context, unsettled findings, accepted knowledge, and history:

| State                                                                       | Question it answers                                                         |
| --------------------------------------------------------------------------- | --------------------------------------------------------------------------- |
| [Working](src/open-forge/.agents/memory/working/_working.md)                | What temporary context is needed to continue or resume this work?           |
| [Emerging](src/open-forge/.agents/memory/emerging/_emerging.md)             | What is useful but still unsettled?                                         |
| [Crystallized](src/open-forge/.agents/memory/crystallized/_crystallized.md) | What accepted knowledge should remain current within this scope?            |
| [Archived](src/open-forge/.agents/memory/archived/_archived.md)             | What useful history should remain available without governing current work? |

Records move between states as their usefulness changes, and they don't have to pass through every state. People and agents save findings, review them, merge related knowledge, and retire what's no longer needed.

The base ships only the four state entrypoints. Extensions add named categories inside them, for example Checkpoints and Decisions from Planning, Documents from Project Documents, and Handoffs from Observations and Handoffs. Without them, records can live directly under a state.

One rule keeps this from turning into a pile of notes: each source defines only its own part of the truth. A Decision records what was chosen and why. A current document explains its subject as it is now. Required behavior belongs in Directives or inherited Axioms, and writing an instruction into Memory doesn't make it a Directive. When something is accepted, update the source that defines it and keep the useful reasoning in Memory.

## Frontmatter and tags

Routed Markdown files carry a little frontmatter to help readers, human or agent, decide what to open and what belongs where. An architecture document might begin like this:

```yaml
---
open-forge:
  description: Understand the service boundaries and how requests move through the system
  responsibility: Define the current service structure and dependency boundaries
  tags: [Memory, Document, Architecture]
---
```

The **description** helps a reader decide whether to open the file. The optional **responsibility** helps an editor decide what belongs in it. Tags help with selection and search, and a few of them have defined behavior:

| Tag                           | Meaning                                                                                                                      |
| ----------------------------- | ---------------------------------------------------------------------------------------------------------------------------- |
| `LoadNow`                     | Read the linked file when its loaded parent lists it                                                                         |
| `KeepInMind`                  | Read it when its parent loads, then again at task start or resume, after context restoration, and before handoff or closeout |
| `Contextual`                  | Treat useful context as unaccepted unless applicable authority establishes acceptance                                        |
| `CurrentTruth`                | Identify accepted current state within its stated scope                                                                      |
| `Evergreen`                   | Keep the content aligned with the current state it represents                                                                |
| `Core`, `Memory`, `Extension` | Identify the part of the Framework or package the content relates to                                                         |

Tags don't grant authority. Loading tags act only through a parent that is already loaded, so they can't pull in a scope nobody selected. Any other tag is yours to describe your own subjects, and it adds no loading behavior.

A file can also list `applyTo` patterns, such as `["**/*.cs"]`. The patterns only filter: a tagged or selected file applies only while the task works on a matching file. [Loading and tags](src/docusaurus/docs/concepts/loading-and-tags.md#file-conditions) has the details.

Each routed folder has an **entrypoint**, normally `_{folder-name}.md`, with short links under `Entries`. Those links are the next choices an agent can make. Reading an entrypoint doesn't open the files it links to. The [loader](src/open-forge/.agents/loader.md) has the complete routing and tag rules, and the [Markdown reference](.agents/memory/crystallized/documents/framework/markdown/syntax.md#frontmatter) covers the format.

## The CLI

The CLI is an accelerant, not a requirement. Everything it does, you can do by editing files. It speeds up the repetitive parts, such as finding context, keeping navigation correct, and updating the Framework, and it shows planned changes before it writes them.

| When you want to                       | Run                                                | Because                                                                           |
| -------------------------------------- | -------------------------------------------------- | --------------------------------------------------------------------------------- |
| See the tag-selected startup routes    | `open-forge context`                               | You can confirm a new rule is set to load before blaming the model                |
| Find what the workspace already knows  | `open-forge find --tag=Decision`                   | An existing Decision is better reused than made again                             |
| Add a scope or a record                | `open-forge route init`, `open-forge route create` | Frontmatter and navigation are right from the start                               |
| Rebuild navigation after editing files | `open-forge index`                                 | Entries are how agents find things, so stale ones hide knowledge                  |
| Check that everything still fits       | `open-forge status`, `open-forge doctor`           | Broken links and stale navigation show up before they mislead an agent            |
| Pick up a new Framework version        | `open-forge update --dry-run`                      | Every file it would replace is listed, so local edits are never replaced silently |
| Remove a default for good              | `open-forge remove <path> --dry-run`               | Later updates respect the removal                                                 |

Commands that change files offer `--dry-run`, so you see the plan before anything is written.

The flags you'll reach for most often are:

| Flag                                   | Use it to                                                                           |
| -------------------------------------- | ----------------------------------------------------------------------------------- |
| `--dry-run`                            | Preview a change                                                                    |
| `--workspace <path>`                   | Choose a project without changing directories                                       |
| `--detail standard` or `--detail full` | See more reasons and evidence                                                       |
| `--format json`                        | Return structured output for a script or agent                                      |
| `--automatic`                          | Skip confirmation on commands that support it, after reviewing the plan             |
| `--for <path>`                         | Supply working files to `context`, `find`, or `route inspect`. Repeat for each file |
| `--help`                               | See the selected command's arguments and flags                                      |

`--automatic` adds no permission to overwrite or delete files. The [CLI overview](https://thelithiumforge.github.io/open-forge/docs/cli#which-command-for-which-job) lists every command's flags in one place.

Your agent can use it too. The Skills entrypoint requires the `open-forge-cli` usage guide at startup. It lists each command's flags and useful cases, including batching initial reads with `open-forge context skills/open-forge-cli`. The loader and Skill are plain instructions a harness can read with its usual file tools.

[Working with the CLI](https://thelithiumforge.github.io/open-forge/docs/cli) walks through these flows and when each one helps, and the [CLI reference](docs/cli.md) covers every command and option.

### Without the CLI

Follow the links, edit the files, and keep each `Entries` list in step with the files it points to. That's all the Framework itself needs.

The Framework explains itself through its own files. This README and the guides are here to help you get acquainted. The rules you need to understand and use Open Forge live in the files themselves.

## Extensions

Extensions are optional packages of content you can install and then adapt. The [first-party catalogue](src/extensions/README.md) has the full details and dependencies:

| Package                   | What it's for                                                                                           |
| ------------------------- | ------------------------------------------------------------------------------------------------------- |
| Core Templates            | Starting files for your own Directives, Guidance, Patterns, Skills, Templates, Maps, and Memory records |
| Collaboration             | Exploring ideas, comparing alternatives, and clarifying decisions through discussion                    |
| Project Documents         | Vision, architecture, principles, and other current project documentation                               |
| Planning                  | Ideas, analysis, decisions, tasks, plans, backlogs, and checkpoints                                     |
| Flows and Scenarios       | User journeys, expected outcomes, and records of what happened when they were tried                     |
| Observations and Handoffs | Saving observations and leaving a clear snapshot for whoever resumes the work                           |
| Development               | Implementing, debugging, and reviewing with the project's existing tools                                |
| Task Coordination         | Coordinating related tasks, their dependencies, and the combined result                                 |
| Workflow Support          | Finding and following installed workflows through the `use-workflow` Skill                              |
| Development Toolkit       | Installing Project Documents, Planning, Flows and Scenarios, and Development together                   |

Each package is included because it helps an agent do something it wouldn't reliably do on its own: a method that changes the outcome, a record that keeps what would otherwise be lost, or a starter that makes the right shape obvious. The packages are split the way people choose them, so you install what helps and nothing else.

Not sure where to start? On an existing codebase, start with **Planning** for its Decisions, which keep the reasons behind changes. To write your own rules, start with **Core Templates**. **Development Toolkit** installs Project Documents, Planning, Flows and Scenarios, and Development together. Task Coordination stays outside the Toolkit. Choose it when you coordinate several related tasks and need to verify their combined result.

An Extension is a way to ship files. Routed files take on the meaning of the route they're installed into. Native files, such as a `SKILL.md`, follow the tool that uses them. Packaging adds no authority.

The [Extensions section of the site](https://thelithiumforge.github.io/open-forge/docs/extensions) explains what every package installs and why, and the [Extension guide](docs/extensions.md) covers installing a package, copying its files by hand, and creating your own.

### The document flow

Project Documents and Planning together add an opinionated flow for project knowledge. A Decision records a change: what was chosen, and why. When you accept it, the rules ask for the documents it affects to be updated to say what's true now. The document links to the Decision for the reason, and the Decision links to the document for the result.

Documents are kept top-down:

```text
Vision                        why it exists, for whom, what the first version does
└─ Architecture               the parts, what each owns, the rules they all keep
   └─ component docs, scenarios   exact behavior and expected results
      └─ code and tests

Decisions sit beside the stack and link to the level that holds their result.
```

Top-level documents give an overview of the whole and summarize the narrower documents they link to. Each narrower document explains its own part in more detail and links back up. When a change lands in the detail, the rules ask for the summaries above it to be updated too, so the top keeps giving the current picture. The current answer is one read away, and the reason behind it is one link away.

This flow is the Extensions' opinion, not Core's. Core only asks that detail live in the narrowest source that defines it, and Memory's states work the same without it. [The document flow](https://thelithiumforge.github.io/open-forge/docs/extensions/document-flow) walks through an example.

## What Open Forge doesn't do

Some limits to know before you adopt it:

- **It doesn't make a model deterministic.** Open Forge makes the right context cheap to find and the rules explicit. An agent can still misread or skip them, so review stays with you.
- **It depends on your harness reading `AGENTS.md`** (or `CLAUDE.md` through the bridge). Everything starts there.
- **It isn't a methodology.** There's no mandatory workflow, role, or ceremony. Extensions offer methods, but installing one doesn't make its methods required.
- **It doesn't record everything.** Memory holds what's worth keeping, and the rules ask agents to save outcomes deliberately rather than log every conversation.

## Go deeper

- [Documentation site](https://thelithiumforge.github.io/open-forge/): getting started, concepts, and a file-by-file tour of every Extension.
- [Demos](demos/): try Open Forge on a new project and on a half-built one, with checks for the result.
- [CLI reference](docs/cli.md): every command, option, status, and exit code.
- [Extension guide](docs/extensions.md): packages, dependencies, and customization.
- [Development guide](docs/development.md): local setup, contributions, and verification.

This repository uses Open Forge to build Open Forge, so its own documents double as a working example: [Vision](.agents/memory/crystallized/documents/vision.md), [Principles](.agents/memory/crystallized/documents/principles.md), [Framework Architecture](.agents/memory/crystallized/documents/framework/architecture.md), [Memory](.agents/memory/crystallized/documents/framework/memory/model.md), and the [writing guides](.agents/memory/crystallized/documents/maintenance/writing.md#choose-the-voice).

[MIT license](LICENSE).
