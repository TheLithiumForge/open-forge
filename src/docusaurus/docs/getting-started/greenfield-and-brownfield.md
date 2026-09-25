---
title: New or existing project
description: How to start Open Forge on a greenfield project, how to adopt it in a brownfield codebase, and answers to the common concerns about each.
---

# New or existing project

Open Forge works the same way in a brand-new project and in a ten-year-old codebase. What differs is where the truth lives when you start, and so what you should write down first.

|                                    | Greenfield: a new project                       | Brownfield: an existing codebase                         |
| ---------------------------------- | ----------------------------------------------- | -------------------------------------------------------- |
| **Where the truth lives at first** | In your head and in the request                 | In the code, and in the heads of the people who wrote it |
| **The biggest risk**               | Planning too much before anything works         | Breaking a rule that nobody wrote down                   |
| **Write first**                    | Nothing, or a short Vision if the goal is fuzzy | A Map to the docs you already have                       |
| **Extensions that help most**      | Project Documents, Planning                     | Planning, for Decisions, and Development                 |
| **Try it**                         | [Greenfield demo](../demos/greenfield.md)       | [Brownfield demo](../demos/brownfield.md)                |

## Starting a new project

1. **Install the Framework**, and the [Development Toolkit](../extensions/development-toolkit.md) if you want the planning and development methods.
2. **Start with the request, not a spec.** Describe what you want to build. A good agent asks about the choices that matter instead of guessing. The [demo seed levels](../demos/index.md#seed-levels) show how much detail changes the result.
3. **Let direction settle as you go.** If the goal is still fuzzy, ask for a Vision and keep it proposed until you accept it. A clear request can go straight to work.
4. **Record decisions as you make them.** The reasons are never fresher than now. Say "Record this as a Decision", and the choice and its reasons land in Crystallized Memory.
5. **Turn the conversation into rules and specs.** When a convention or a requirement settles in conversation, ask the agent to write it down: a Directive or Pattern for a rule, with starters from [Core Templates](../extensions/core-templates.md), or a Vision or Architecture for a spec, from [Project Documents](../extensions/project-documents.md).

What to avoid: writing Directives for problems you haven't had yet, and creating categories before anything goes in them. Core categories start almost empty on purpose: the only shipped content is a Skill that teaches agents the CLI.

## Adopting it in an existing codebase

Four principles keep adoption painless:

- **Existing code is evidence, not automatic authority.** It shows what the system does, not whether that's intended. Ask before treating an odd behavior as a rule, or as a bug.
- **Point to what exists instead of moving it.** Your README, docs folder, ADRs, and runbooks stay where they are. A [Map](../concepts/core-categories.md#maps) tells the agent where they are and when to read them.
- **Decisions start the day you adopt.** You can't decide what was already decided before you arrived. From now on, every change that alters behavior records its [Decision](../highlights.md#decisions-keep-the-why) with it, so the next person doesn't have to guess. The reasons behind older code stay wherever they live today, and your Map points there.
- **Rules follow pain.** When an unwritten convention keeps getting broken, write it as a Directive or a Pattern, scoped to where it applies. One rule for the problem you hit this week beats a rulebook nobody reads.

### Step by step

1. **Install with a dry run and review the diff.** If you already have an `AGENTS.md` or `CLAUDE.md`, installation adds an Open Forge section and keeps your content.

   ```sh
   open-forge install --dry-run
   open-forge install
   open-forge extension install development-toolkit --dry-run
   ```

2. **Map what you already have.** Ask your agent to add a Map of the important docs, specs, notes, and runbooks, with a line on when each matters:

   > Add a Map of this project's important sources, such as the README, the docs folder, and any design notes, with a line on when to read each one.

3. **Work as usual, and record Decisions as you change things.** When a change alters behavior or picks between real alternatives, ask for the Decision in the same change:

   > Add the export feature. If you make a choice with real alternatives, record it as a Decision and keep it proposed until I accept it.

   Reviewers then see the code and the reason in one diff.

4. **Add Directives only for the conventions that have bitten you**, and [scope them](../concepts/scopes.md) to the part of the codebase they apply to.

## Common concerns

**Do I have to document the whole codebase first?**
No. Start with a Map. Decisions begin with your next change, and rules grow from the conventions that actually get broken.

**Will it clash with my existing `AGENTS.md` or `CLAUDE.md`?**
No. Installation adds a clearly marked Open Forge section and keeps everything you wrote. Other tools' rule files aren't touched.

**How much context does it add in a large repository?**
The base adds about 5.8k tokens at startup. Specialized rules live in [scopes](../concepts/scopes.md) that load only when a task selects them, and Memory records load when they're relevant. Adding knowledge doesn't mean every task reads more of it.

**What if the agent writes something wrong into Memory?**
Everything is plain Markdown in your repository, so it shows up in `git diff` like any change. Unconfirmed findings stay in Emerging Memory. Only what you accept becomes Crystallized, and tags alone never make something authoritative.

**We already keep ADRs. Do we switch to Decisions?**
You don't have to. Keep your ADRs and add a Map that points to them. Decisions from the Planning Extension are a convention, not a requirement.

**We have a monorepo.**
Give each package or service its own scope, so its rules load only for work on it. A task that touches two packages selects both.

**Does it work for a team?**
Yes. The files live in the repository and get reviewed like code, and everyone's agents read the same rules.
