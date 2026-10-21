---
title: New or existing project
description: How to start Open Forge on a greenfield project, how to adopt it in a brownfield codebase, and answers to the common concerns about each.
---

# New or existing project

Open Forge works the same way in a brand-new project and in a ten-year-old codebase. What differs is where the truth lives when you start, and so what you should write down first.

The base Framework gives you routing, loading, and your selected Core categories and Memory states. [Installation](installation.md#choose-the-installed-routes) explains Essentials, Full Core, and Custom. Several things on this page come from optional Extensions: Decisions from [Planning](../extensions/planning.md), and Vision and Architecture documents from [Project Documents](../extensions/project-documents.md). The [Development Toolkit](../extensions/development-toolkit.md) installs both, plus the Development and Flows and Scenarios packages.

|                                    | Greenfield: a new project                         | Brownfield: an existing codebase                         |
| ---------------------------------- | ------------------------------------------------- | -------------------------------------------------------- |
| **Where the truth lives at first** | In your head and in the request                   | In the code, and in the heads of the people who wrote it |
| **The biggest risk**               | Planning too much before anything works           | Breaking a rule that nobody wrote down                   |
| **Write first**                    | Nothing, or a short Vision if the goal is unclear | Loader links to the docs you already have                |
| **Extensions that help most**      | Project Documents, Planning                       | Planning, for Decisions, and Development                 |
| **Try it**                         | [Greenfield demo](../demos/greenfield.md)         | [Brownfield demo](../demos/brownfield.md)                |

## Starting a new project

1. **Install the Framework**, and the [Development Toolkit](../extensions/development-toolkit.md) Extension if you want the planning and development methods.
2. **Start with the request, not a spec.** Describe what you want to build. A good agent asks about the choices that matter instead of guessing. The [demo seed levels](../demos/index.md#seed-levels) show how much detail changes the result.
3. **Let direction settle as you go.** If the goal is still unclear, ask for a draft Vision and keep it in Emerging Memory until you accept it. A clear request can go straight to work.
4. **Record decisions as you make them.** The reasons are easiest to capture while the choice is fresh. With Planning installed, say "Record this as a Decision", and the Planning rules have the agent save the choice and its reasons as a Decision in Crystallized Memory.
5. **Turn the conversation into rules and specs.** When a convention or a requirement settles in conversation, ask the agent to write it down: a Directive for a rule the agent must follow or a Pattern for a shape worth repeating, with starters from the [Core Templates](../extensions/core-templates.md) Extension, or a Vision or Architecture for a spec, from [Project Documents](../extensions/project-documents.md).

What to avoid: writing Directives for problems you haven't had yet, and creating categories before anything goes in them. Core categories start almost empty on purpose: the only shipped content is a Skill that teaches agents the CLI.

## Adopting it in an existing codebase

Four principles guide adoption:

- **Existing code is evidence, not automatic authority.** It shows what the system does, not whether that's intended. Ask before treating an odd behavior as a rule, or as a bug.
- **Point to what exists instead of moving it.** Your README, docs folder, ADRs, runbooks and other tools' rule files stay where they are. `open-forge route include` lists each one in the loader where it is, with no frontmatter, and a [Map](../concepts/core-categories.md#maps) can describe a larger set.
- **Decisions start the day you adopt.** You can't decide what was already decided before you arrived. From now on, each important choice gets a [Decision](../highlights.md#decisions-keep-the-why) with its reasons, so the next person doesn't have to guess. The reasons behind older code stay wherever they live today, and your Map points there.
- **Rules follow real problems.** When an unwritten convention keeps getting broken, write it as a Directive, or as a Pattern if it's a shape rather than a rule, scoped to where it applies. Add a rule when a mistake repeats, not in advance.

### Step by step

Save your current work before adoption. For a trial, use a separate local clone and copy any additional local files you need. [Try it and undo a trial](installation.md#try-it-and-undo-a-trial) explains the saved baseline, how to restore changed tracked files, and how to remove only trial additions. Keep any work you want, and preserve an existing `.agents/` tree.

1. **Install the smallest setup with a dry run and review the diff.** Minimal
   writes the loader, Directives for your rules, Crystallized Memory for
   accepted knowledge, a short guide that also holds the file formats agents
   follow, which you may delete, and an Open Forge section in
   `AGENTS.md` and `CLAUDE.md` that keeps your content. The two folders start
   empty. See [With the CLI](installation.md#with-the-cli) for
   details, including how the CLI adopts an existing `.agents/` tree.

   ```sh
   open-forge install --preset minimal --dry-run
   open-forge install --preset minimal --automatic
   ```

2. **Include what you already have.** Run `open-forge route include` for
   each document agents should use. It stays where it is and needs no
   frontmatter. `--tag LoadNow` makes it a required read, and `--apply-to`
   limits it to matching work. Rule files for other tools, such as
   `.cursor/rules/*.mdc` or `.windsurfrules`, can be included one by one in
   the same way:

   ```sh
   open-forge route include CONTRIBUTING.md --description "Contribution rules to follow" --tag LoadNow
   open-forge route include docs/contributing/csharp.md --description "C# conventions" --tag LoadNow --apply-to "**/*.cs"
   open-forge route include docs/architecture.md --description "Architecture overview" --tag Project
   ```

   Each command writes one line under `## Entries` in `.agents/loader.md`,
   which you can also write by hand:

   ```markdown
   - [Contribution rules to follow](../CONTRIBUTING.md) - #LoadNow
   - [C# conventions](../docs/contributing/csharp.md) - #LoadNow - applies to `**/*.cs`
   - [Architecture overview](../docs/architecture.md) - #Project
   ```

   Then try a normal request and check that the agent names the files it used.
   New rules go in Directives and new accepted facts in Crystallized Memory.
   When work should survive a pause, add Working Memory with
   `open-forge install --configure --preset custom --route memory/working=git-ignore`,
   and install the [Development Toolkit](../extensions/development-toolkit.md)
   when you want Planning's Decisions.

3. **Work as usual, and record Decisions as you change things.** When a change alters behavior or picks between real alternatives, ask for its reasoning in the same change:

   > Add the export feature. If you make a choice with real alternatives, keep it in Analysis until I accept it, then record it as a Decision.

   Reviewers then see the code and the reason in one diff.

4. **Add a Directive when a convention keeps getting broken**, not in advance, and [scope it](../concepts/scopes.md) to the part of the codebase it applies to.

## Common concerns

**Do I have to document the whole codebase first?**
No. Start with Minimal and one link to a document you already trust. Decisions begin with your next change, and rules grow from the conventions that actually get broken.

**Will it clash with my existing `AGENTS.md` or `CLAUDE.md`?**
No. Installation adds a clearly marked Open Forge section and keeps everything you wrote. Other tools' rule files aren't touched. Read the combined instructions once, in case your rules and the loader's pull in different directions.

**How much context does it add in a large repository?**
Startup reads `AGENTS.md`, the loader, installed entrypoints tagged to load, and any files their rules require. Run `open-forge status` to estimate the tag-selected context in your workspace. Reading an entrypoint gives the agent its rules and one line per item. Linked files open when selected, tagged to load, or required by a loaded rule. Specialized rules live in [scopes](../concepts/scopes.md) that load when selected, and Memory records open when relevant. First-party Extensions add at most their entry lines at startup. Adding knowledge doesn't mean every task reads more of it.

**What if the agent writes something wrong into Memory?**
Everything is plain Markdown that you can inspect and correct. Tracked changes appear in `git diff`. Review new Git-ignored Working records directly with the CLI or your normal file tools. The rules keep unconfirmed findings in Emerging Memory and reserve Crystallized for what you accept. Tags alone never make something authoritative.

**We already keep ADRs. Do we switch to Decisions?**
You don't have to. Keep your ADRs where they are and link them from the loader, or add a Map that points to them. Decisions from the Planning Extension are a convention, not a requirement.

**We have a monorepo.**
Give each package or service its own scope, so its rules load only for work on it. A task that touches two packages selects both.

**Does it work for a team?**
Yes. Shared files live in the repository and get reviewed like code, and everyone's agents read the same rules. Essentials keeps new Working records out of ordinary Git commits and leaves `_working.md` eligible for sharing. Already tracked files stay tracked. Generated `Entries` omit private contents. You can find and read local records with the CLI or your normal file tools.

**Next:** [Try the ten-minute guide](ten-minute-guide.md).
