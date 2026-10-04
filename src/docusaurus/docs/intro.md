---
title: What is Open Forge?
sidebar_label: Introduction
description: A small Markdown framework that gives AI agents a map of your workspace instead of all of it at once.
slug: /
---

import FrameworkMap from "@site/src/components/framework-map/FrameworkMap";

# What is Open Forge?

Open Forge is a small Markdown framework for working with AI agents, built around your projects, your tools, and the way you like to work. It gives an agent a map to your rules and project knowledge, and a place to keep useful outcomes for the next task.

At its core, it's Markdown, rules, and links. Install it, ask your agent to work as usual, and add only what makes the next task easier.

## Start using it

With Node.js 22.18 or later:

```sh
npm install -g @thelithiumforge/open-forge@beta
cd /path/to/your-project
open-forge install
```

Choose Essentials for a smaller start, Full Core for every built-in route, or Custom to choose each one. Review the selected plan before confirming, then check the new files with `git status` and `git diff` before committing. [Installation](getting-started/installation.md) explains the choices, other setup methods, and `install --configure` for an existing workspace.

Now ask your agent for a normal task. For a first try in an existing project:

> Explain how this project runs its tests. Follow `AGENTS.md`, and name the Open Forge files you read.

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

An agent doesn't need to know everything, but it does need to know where everything is. Each context folder has a short `entrypoint`, a Markdown file named `_{folder-name}.md` that lists what's inside. The rules load startup context, then guide the agent to the branches the task needs. Other content opens when selected, tagged to load, or explicitly required by a loaded rule.

This is **Adaptive Context Engineering (ACE)**: start from the task, bring in context as the work unfolds, and keep what's useful next time. [Loading and tags](concepts/loading-and-tags.md) explains the exact rules.

Open Forge works with any harness, the tool that runs your agent, that reads `AGENTS.md` or the `CLAUDE.md` bridge. Keep your hooks, custom agents, and tools. A translator such as [APM](https://github.com/microsoft/apm) can help install harness-specific definitions in several tools.

## Three parts, one of them required

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

## What you actually install

Full Core supplies 15 Markdown files. Essentials includes Directives, Patterns, Skills, Emerging and Crystallized Memory, plus Working Memory with its whole directory Git-ignored. Working still loads normally, but new local records stay out of ordinary Git commits. Custom follows your selection. [Installation](getting-started/installation.md#choose-the-installed-routes) covers the exact choices.

<details>
<summary>See the Full Core files and startup details</summary>

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

Apart from Memory state entrypoints and one Skill that teaches agents the CLI, every selected category starts empty. The content comes from your work, such as a correction you keep repeating or a decision you don't want to explain again, and from any Extensions you install. Decisions, Checkpoints, workflow recipes, and starter Templates all come from Extensions, not from the base.

In Full Core, startup reads `AGENTS.md`, the loader, nine entrypoints, and the CLI usage Skill required by Skills. Essentials and Custom use the same loading rules for the routes present. Reading an entrypoint doesn't load everything it lists. Other items open when selected, tagged to load, or required by a loaded rule. [Loading and tags](concepts/loading-and-tags.md) has the full table.

</details>

## Where to go next

- **New here?** Follow [Installation](getting-started/installation.md), then the [ten-minute guide](getting-started/ten-minute-guide.md). [New or existing project](getting-started/greenfield-and-brownfield.md) covers what to write down first in each case.
- **Want to see it work?** Try a [demo](demos/index.md): build a small app from an idea, or add a feature to a half-built one.
- **Wondering what's worth your attention?** The [Highlights](highlights.md) pick out the features that do the most work for particular kinds of users, starting with Decisions.
- **Want to understand how it works?** Read the [Concepts](concepts/index.md), starting with [Routing](concepts/routing.md).
- **Wondering what an Extension actually puts in your workspace?** The [Extensions](extensions/index.md) section explains every package file by file.
- **Using the CLI?** [Working with the CLI](cli/index.md) shows which command helps with which job, and the [CLI reference](/guides/cli) covers every option.
