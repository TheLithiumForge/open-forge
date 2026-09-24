---
title: Concepts
description: How Open Forge works, from the idea behind it to the files that carry it.
---

# Concepts

Open Forge is built on one idea: give agents a map of the workspace, not the whole workspace. Everything else follows from making that map cheap to read and easy to grow.

## Adaptive Context Engineering

ACE combines two ideas that already work well:

- **Progressive disclosure:** show the next useful level of detail, not everything at once.
- **Spec-driven development:** make goals, constraints, and expected results explicit enough to guide the work and check the result.

ACE applies both to the workspace itself. A task starts from what you want to achieve. As the work unfolds, the agent pulls in what it needs, such as the relevant architecture, your local conventions, or the decision you made last month, and unrelated history stays out. What you learn along the way can become part of the framework, so the next task starts from a better place.

## The pieces

| Concept             | In one sentence                                                                                           | Read more                               |
| ------------------- | --------------------------------------------------------------------------------------------------------- | --------------------------------------- |
| **Routing**         | Every folder has one short entrypoint that lists its children, so an agent navigates instead of scanning. | [Routing](routing.md)                   |
| **Scopes**          | A folder can narrow where its content applies, so specialized rules load only for matching work.          | [Scopes](scopes.md)                     |
| **Loading tags**    | `#LoadNow` and `#KeepInMind` decide what is read at startup. Everything else waits until selected.        | [Loading and tags](loading-and-tags.md) |
| **Core categories** | Six roles for content: Directives, Guidance, Patterns, Skills, Templates, and Maps.                       | [Core categories](core-categories.md)   |
| **Memory**          | Four states for what's worth remembering: Working, Emerging, Crystallized, and Archived.                  | [Memory](memory.md)                     |
| **Customizing**     | Every file is yours. Edit it, replace it, or add an overwrite companion that survives updates.            | [Customizing](customizing.md)           |

## Two parts of the Framework

The Framework has two parts:

- **Core** defines routing, loading, and a few reusable content roles.
- **Memory** keeps useful context as the work evolves.

Methods for planning, development, project documents, or coordinating several agents are optional [Extensions](../extensions/index.md). You install the ones that fit your work and skip the rest.

## Why plain Markdown

AI made code cheap to write, and review became the bottleneck. The framework you work in should be easy to read, diff, and version with the rest of your project. There's no hidden database, no agent runtime, and nothing specific to one vendor.

It's small on purpose too. Big predefined methodologies assume everyone works the same way, and nobody does. Open Forge gives you a handful of rules and a structure that scales, then leaves the rest to you.

:::info The files are the source of truth

This site helps you get acquainted. The rules an agent follows live in the installed files themselves, starting with [`loader.md`](../../../open-forge/.agents/loader.md). When this site and your installed files disagree, the files win.

:::
