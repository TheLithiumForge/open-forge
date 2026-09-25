---
title: What is Open Forge?
sidebar_label: Introduction
description: A small Markdown framework that gives AI agents a map of your workspace instead of all of it at once.
slug: /
---

import FrameworkMap from "@site/src/components/framework-map/FrameworkMap";

# What is Open Forge?

Open Forge is a small Markdown framework for working with AI agents, built around your projects, your tools, and the way you like to work. It's agent- and harness-agnostic. Anything that reads `AGENTS.md` (or `CLAUDE.md`) can use it.

The idea behind it is **Adaptive Context Engineering (ACE)**: an agent doesn't need to know everything, but it does need to know where everything is. Open Forge gives your workspace a loader and one short entrypoint per folder. An agent starts from the task, follows the routes that matter, and skips the rest.

## How it fits together

<FrameworkMap />

Everything in the workspace answers one of two questions:

- **Core: how should this work be done?** Six roles, because a rule, a piece of advice, a reusable shape, a capability, a starting file, and a pointer each behave differently. A rule must be followed. Advice can be adapted. A Template is copied and then owned by you. Keeping them apart is what lets an agent know how seriously to take each file.
- **Memory: what is worth remembering?** Four states, because "where I am right now", "something I noticed", "what we decided", and "what used to be true" deserve different trust. An agent treats Crystallized knowledge as current and Emerging findings as unconfirmed, and Archived history never steers the work.

The [Concepts](concepts/index.md) section covers each part in depth.

## What you actually install

The base Framework is 15 plain Markdown files, about 7.9k tokens, of which about 5.8k load at startup ([how it's measured](/guides/development#measure-context-size)).

```text
AGENTS.md                         <- tells the agent to read the loader first
CLAUDE.md                         <- bridge for harnesses that read CLAUDE.md
.agents/
  loader.md                       <- routing and loading rules, and the root routes
  directives/_directives.md       <- required behavior
  guidance/_guidance.md           <- advice for recurring choices
  patterns/_patterns.md           <- reusable shapes for code, files, and documents
  skills/_skills.md               <- native SKILL.md capabilities
  skills/open-forge-cli/SKILL.md  <- how to use the CLI, loaded when needed
  templates/_templates.md         <- copy-ready starting files
  maps/_maps.md                   <- pointers to important local and external sources
  memory/
    _memory.md                    <- what is worth remembering
    working/_working.md           <- temporary state for active work
    emerging/_emerging.md         <- useful, but not settled yet
    crystallized/_crystallized.md <- accepted knowledge that stays current
    archived/_archived.md         <- history that no longer governs current work
```

Apart from Memory's four states and one Skill that teaches agents the CLI, every category starts empty. The content comes from your work: a correction you keep repeating, a decision you don't want to explain again, a workflow that keeps paying off.

## Three parts, one of them required

| Part           | What it is                                                                                              | Required?                                            |
| -------------- | ------------------------------------------------------------------------------------------------------- | ---------------------------------------------------- |
| **Framework**  | The Markdown files above: Core (routing, loading, content roles) and Memory (what's worth remembering). | Yes. This is Open Forge.                             |
| **Extensions** | Optional packages of more files: planning records, workflows, templates, project documents.             | No. Install the ones that fit.                       |
| **CLI**        | The `open-forge` command. It finds context, keeps navigation correct, and installs or updates files.    | No. Everything it does, you can do by editing files. |

## Where to go next

- **New here?** Start with [Installation](getting-started/installation.md), then read [New or existing project](getting-started/greenfield-and-brownfield.md).
- **Want to see it work?** Try a [demo](demos/index.md): build a small app from an idea, or add a feature to a half-built one.
- **Wondering what's worth your attention?** The [Highlights](highlights.md) pick out the features people find most useful, starting with Decisions.
- **Want to understand how it works?** Read the [Concepts](concepts/index.md), starting with [Routing](concepts/routing.md).
- **Wondering what an Extension actually puts in your workspace?** The [Extensions](extensions/index.md) section explains every package file by file.
- **Using the CLI?** [Working with the CLI](cli/index.md) shows which command helps with which job, and the [CLI reference](/guides/cli) covers every option.
