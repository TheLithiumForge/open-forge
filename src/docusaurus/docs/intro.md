---
title: What is Open Forge?
sidebar_label: Introduction
description: Give your coding agent the rules, docs and decisions each task needs, and a place to keep what it learns for the next session.
slug: /
---

import FrameworkMap from "@site/src/components/framework-map/FrameworkMap";

# What is Open Forge?

Open Forge gives your coding agent the rules, docs and decisions each task needs, and a place to keep what it learns for the next session.

One long `AGENTS.md` hands every task every rule, and every new session starts by relearning the project. Open Forge gives the agent a map instead. It has three parts: agent documents with the routes that link them, a CLI that installs and maintains them, and optional workflows from Extensions. It's all built on Markdown files in your repository, so you read, diff and review it like the rest of your code.

E.g. a rule tagged `LoadNow` with `applyTo: ["src/export/**"]` loads only for tasks that change export code. A fact your team accepted, such as "CSV exports use UTF-8", sits in Crystallized Memory, so the next export task starts from it instead of guessing.

Your own documents can stay where they are. The loader, the rules and the agents' notes live in one context folder, `.agents` by default, and a folder your team writes in, such as `docs`, can be registered as a route. Agents find its documents through the loader, and you tell them which ones are accepted sources. [Your own docs](getting-started/your-own-docs.md) shows how.

Install it, ask your agent to work as usual, and add only what makes the next task easier. Path examples on this site use the default `.agents` folder, as [Installation](getting-started/installation.md#choose-the-context-folder) explains. Skills, settings and the ownership record stay in `.agents`.

## Start using it

Already have `AGENTS.md` or `CLAUDE.md`? Install adds a marked Open Forge section and keeps your existing text. It also creates the selected context files under `.agents/` by default. Save your current work before trying it.

For a trial, use a separate local clone and copy any additional local files you need. [Installation](getting-started/installation.md#try-it-and-undo-a-trial) explains the baseline and undo steps.

With Node.js 22.18 or later:

```sh
npm install -g @thelithiumforge/open-forge@beta
cd /path/to/your-project
open-forge install
```

For a first try, choose the recommended Minimal setup. Keep Root keys unless your workspace already declares another metadata form. Review the plan before applying it, then inspect the new files and changes with `git status` and `git diff`. [Installation](getting-started/installation.md) explains the other choices, other setup methods, and how to change your setup later.

Now ask your agent for a normal task. In an existing project, ask how its tests run:

> Explain how this project runs its tests. Follow `AGENTS.md`, and name the Open Forge files you read.

In a new project, give the agent a first build request. Try the [greenfield demo](demos/greenfield.md) for a concrete example.

You don't need to choose a workflow, install Extensions, or write a specification first. The [ten-minute guide](getting-started/ten-minute-guide.md) takes you through adding a rule, keeping a useful fact, and finding context for the next task. The [demos](demos/index.md) give you a project to try it on.

## The basic flow

```text
Your request
    |
AGENTS.md -> loader
    |
Startup rules + relevant links
    |
Work, keep useful outcomes
```

An agent doesn't need to know everything, but it does need to know where everything is. Each context folder has a short `entrypoint`, a Markdown file named `_{folder-name}.md` that lists what's inside. The rules load startup context, then guide the agent to the branches the task needs. Other content opens when selected, tagged to load, or required by a loaded rule.

This is **Adaptive Context Engineering (ACE)**: start from the task, bring in context as the work unfolds, and keep what's useful next time. [Loading and tags](concepts/loading-and-tags.md) explains the exact rules.

Open Forge works with any agent tool that reads `AGENTS.md` or `CLAUDE.md`. Keep your hooks, custom agents, and tools. A translator such as [APM](https://github.com/microsoft/apm) can help install harness-specific definitions in several tools.

## Three parts

| Part           | What it is                                                                                                     | Required?                                            |
| -------------- | -------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------- |
| **Framework**  | The Markdown files listed below: Core (routing, loading, content roles) and Memory (what's worth remembering). | Yes. This is Open Forge.                             |
| **Extensions** | Optional packages of more files: planning records, workflows, templates, project documents.                    | No. Install the ones that fit.                       |
| **CLI**        | The `open-forge` command. It finds context, keeps navigation correct, and installs or updates files.           | No. Everything it does, you can do by editing files. |

## How it fits together

Everything in the workspace answers one of two questions:

- **Core: how should this work be done?** Six roles, because a rule, a piece of advice, a reusable shape, a capability, a starting file, and a pointer each behave differently. A rule must be followed. Advice can be adapted. A Template is copied and then owned by you. Keeping them apart is what lets an agent know how seriously to take each file.
- **Memory: what is worth remembering?** Four states, because "where I am right now", "something I noticed", "what we accepted", and "what used to be true" deserve different trust. The rules tell an agent to treat Crystallized knowledge as current and Emerging findings as unconfirmed. Archived history doesn't control current work.

The [Concepts](concepts/index.md) section covers each part in depth.

<details>
<summary>See the full framework map</summary>

<FrameworkMap />

</details>

## Docs and teams

Open Forge shares context at three levels. A workspace keeps agent context in its main root and your team's documents in outside routes. A [Library](getting-started/share-with-your-team.md#library) links one shared folder into place, such as a Git submodule that several repos include. An [Extension](extensions/index.md) packages files that any project, team or organization can install. [Your own docs](getting-started/your-own-docs.md) covers the first level, and [Share with your team](getting-started/share-with-your-team.md) covers all three.

## What you install

Full Core gives you 20 Markdown files, including a guide for people and four supporting CLI guides that are opened when needed. Minimal includes Directives and Crystallized Memory. Essentials adds Patterns, Skills, Emerging and Working Memory. Custom follows your selection.

Essentials keeps new Working records out of normal Git commits and leaves `_working.md` eligible for sharing. Already tracked files stay tracked. Generated `Entries` omit private contents. You can find and read local records with the CLI or your normal file tools. [Installation](getting-started/installation.md#choose-the-installed-routes) covers the exact choices.

<details>
<summary>See the Full Core files and startup details</summary>

```text
AGENTS.md                         <- tells the agent to read the loader first
CLAUDE.md                         <- the same entry for tools that read CLAUDE.md
.agents/
  loader.md                       <- the reading steps and the list of what agents use
  README.md                       <- a guide and the file formats, safe to delete
  directives/_directives.md       <- required behavior
  guidance/_guidance.md           <- advice for recurring choices
  patterns/_patterns.md           <- reusable shapes for code, files, and documents
  skills/_skills.md               <- native SKILL.md capabilities
  skills/open-forge-cli/SKILL.md  <- how to use the CLI, read before CLI commands
  skills/open-forge-cli/references/
    discovery.md                  <- reading and discovery
    routes.md                     <- route maintenance
    packages.md                   <- Framework and packages
    common.md                     <- shared options and results
  templates/_templates.md         <- copy-ready starting files
  maps/_maps.md                   <- pointers to important local and external sources
  memory/
    _memory.md                    <- what is worth remembering
    working/_working.md           <- temporary state for active work
    emerging/_emerging.md         <- useful, but not settled yet
    crystallized/_crystallized.md <- accepted knowledge that stays current
    archived/_archived.md         <- history that no longer governs current work
```

Apart from Memory state entrypoints and one Skill that teaches agents the CLI, every selected category starts empty. The content comes from your work, such as a correction you keep repeating or a decision you don't want to explain again, and from any Extensions you install. Decisions, Checkpoints, workflow recipes, and starter Templates all come from Extensions, not from the base.

In Full Core, startup reads `AGENTS.md`, the loader and five entrypoints: Directives, Memory, Crystallized, Emerging and Working. The Skills entrypoint asks agents to read the CLI usage Skill before they run CLI commands other than `context`. Essentials and Custom use the same loading rules for the routes present. Reading an entrypoint doesn't load everything it lists. Other items open when selected, tagged to load, or required by a loaded rule. [Loading and tags](concepts/loading-and-tags.md) has the full table.

</details>

## Where to go next

- **New here?** Follow [Installation](getting-started/installation.md), then the [ten-minute guide](getting-started/ten-minute-guide.md). [New or existing project](getting-started/greenfield-and-brownfield.md) covers what to write down first in each case.
- **Keeping your own docs?** Register the folder as described in [Your own docs](getting-started/your-own-docs.md), then [share it with your team](getting-started/share-with-your-team.md).
- **Want to see it work?** Try a [demo](demos/index.md): build a small app from an idea, or add a feature to a half-built one.
- **Wondering what's worth your attention?** The [Highlights](highlights.md) pick out the features that do the most work for particular kinds of users, starting with Decisions.
- **Want to understand how it works?** Read the [Concepts](concepts/index.md), starting with [Routing](concepts/routing.md).
- **Wondering what an Extension actually puts in your workspace?** The [Extensions](extensions/index.md) section explains every package file by file.
- **Using the CLI?** [Working with the CLI](cli/index.md) shows which command helps with which job, and the [CLI reference](/guides/cli) covers every option.
