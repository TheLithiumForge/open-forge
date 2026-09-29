---
title: New or existing project
description: How to start Open Forge on a greenfield project, how to adopt it in a brownfield codebase, and answers to the common concerns about each.
---

# New or existing project

Open Forge works the same way in a brand-new project and in a ten-year-old codebase. What differs is where the truth lives when you start, and so what you should write down first.

The base Framework gives you routing, loading, the Core categories, and Memory's four states. Several things on this page come from optional Extensions: Decisions from [Planning](../extensions/planning.md), and Vision and Architecture documents from [Project Documents](../extensions/project-documents.md). The [Development Toolkit](../extensions/development-toolkit.md) installs both, plus the Development and Flows and Scenarios packages.

|                                    | Greenfield: a new project                         | Brownfield: an existing codebase                         |
| ---------------------------------- | ------------------------------------------------- | -------------------------------------------------------- |
| **Where the truth lives at first** | In your head and in the request                   | In the code, and in the heads of the people who wrote it |
| **The biggest risk**               | Planning too much before anything works           | Breaking a rule that nobody wrote down                   |
| **Write first**                    | Nothing, or a short Vision if the goal is unclear | A Map to the docs you already have                       |
| **Extensions that help most**      | Project Documents, Planning                       | Planning, for Decisions, and Development                 |
| **Try it**                         | [Greenfield demo](../demos/greenfield.md)         | [Brownfield demo](../demos/brownfield.md)                |

## Starting a new project

1. **Install the Framework**, and the [Development Toolkit](../extensions/development-toolkit.md) Extension if you want the planning and development methods.
2. **Start with the request, not a spec.** Describe what you want to build. A good agent asks about the choices that matter instead of guessing. The [demo seed levels](../demos/index.md#seed-levels) show how much detail changes the result.
3. **Let direction settle as you go.** If the goal is still unclear, ask for a Vision and keep it proposed until you accept it. A clear request can go straight to work.
4. **Record decisions as you make them.** The reasons are easiest to capture while the choice is fresh. With Planning installed, say "Record this as a Decision", and the Planning rules have the agent save the choice and its reasons as a Decision in Crystallized Memory.
5. **Turn the conversation into rules and specs.** When a convention or a requirement settles in conversation, ask the agent to write it down: a Directive for a rule the agent must follow or a Pattern for a shape worth repeating, with starters from the [Core Templates](../extensions/core-templates.md) Extension, or a Vision or Architecture for a spec, from [Project Documents](../extensions/project-documents.md).

What to avoid: writing Directives for problems you haven't had yet, and creating categories before anything goes in them. Core categories start almost empty on purpose: the only shipped content is a Skill that teaches agents the CLI.

## Adopting it in an existing codebase

Four principles guide adoption:

- **Existing code is evidence, not automatic authority.** It shows what the system does, not whether that's intended. Ask before treating an odd behavior as a rule, or as a bug.
- **Point to what exists instead of moving it.** Your README, docs folder, ADRs, and runbooks stay where they are. A [Map](../concepts/core-categories.md#maps) tells the agent where they are and when to read them.
- **Decisions start the day you adopt.** You can't decide what was already decided before you arrived. From now on, each important choice gets a [Decision](../highlights.md#decisions-keep-the-why) with its reasons, so the next person doesn't have to guess. The reasons behind older code stay wherever they live today, and your Map points there.
- **Rules follow real problems.** When an unwritten convention keeps getting broken, write it as a Directive, or as a Pattern if it's a shape rather than a rule, scoped to where it applies. Add a rule when a mistake repeats, not in advance.

### Step by step

1. **Install with a dry run and review the diff.** If you already have an `AGENTS.md` or `CLAUDE.md`, installation adds an Open Forge section and keeps your content. The last two commands preview and install the Development Toolkit, which brings Planning for Decisions. Skip them if you don't want it yet.

   ```sh
   open-forge install --dry-run
   open-forge install
   open-forge extension install development-toolkit --dry-run
   open-forge extension install development-toolkit
   ```

2. **Map what you already have.** Ask your agent to add a Map of the important docs, specs, notes, and runbooks, with a line on when each matters:

   > Add a Map of this project's important sources, such as the README, the docs folder, and any design notes, with a line on when to read each one.

3. **Work as usual, and record Decisions as you change things.** When a change alters behavior or picks between real alternatives, ask for the Decision in the same change:

   > Add the export feature. If you make a choice with real alternatives, record it as a Decision and keep it proposed until I accept it.

   Reviewers then see the code and the reason in one diff.

4. **Add a Directive when a convention keeps getting broken**, not in advance, and [scope it](../concepts/scopes.md) to the part of the codebase it applies to.

## Common concerns

**Do I have to document the whole codebase first?**
No. Start with a Map. Decisions begin with your next change, and rules grow from the conventions that actually get broken.

**Will it clash with my existing `AGENTS.md` or `CLAUDE.md`?**
No. Installation adds a clearly marked Open Forge section and keeps everything you wrote. Other tools' rule files aren't touched. Read the combined instructions once, in case your rules and the loader's pull in different directions.

**How much context does it add in a large repository?**
The base adds about 6.1k tokens at startup: `AGENTS.md`, the loader, and the entrypoints tagged to load at startup. Reading an entrypoint gives the agent its rules and one line per item, and the items themselves stay closed until a task needs them. Specialized rules live in [scopes](../concepts/scopes.md) that load only when a task selects them, and Memory records open when they're relevant. Installed Extensions add only their one-line entries at startup. Adding knowledge doesn't mean every task reads more of it.

**What if the agent writes something wrong into Memory?**
Everything is plain Markdown in your repository, so it shows up in `git diff` like any change. The rules keep unconfirmed findings in Emerging Memory and reserve Crystallized for what you accept. Tags alone never make something authoritative.

**We already keep ADRs. Do we switch to Decisions?**
You don't have to. Keep your ADRs and add a Map that points to them. Decisions from the Planning Extension are a convention, not a requirement.

**We have a monorepo.**
Give each package or service its own scope, so its rules load only for work on it. A task that touches two packages selects both.

**Does it work for a team?**
Yes. The files live in the repository and get reviewed like code, and everyone's agents read the same rules.
