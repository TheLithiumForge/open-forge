---
title: Your first task
description: What happens when an agent starts work in an Open Forge workspace, and how to check what it loaded.
---

# Your first task

Give your agent a real task. You don't need a spec up front. Start with the request and add detail as the work calls for it.

## What the agent does

1. **It reads `AGENTS.md`.** The installed block tells it to read `.agents/loader.md` before starting.
2. **It reads the loader.** The loader defines the routing and loading rules, and lists the root routes: Directives, Guidance, Maps, Memory, Patterns, Skills, and Templates.
3. **It loads what's marked for startup.** Entries tagged `#LoadNow` are read right away. That's how the root entrypoints (all but Templates), your workspace-wide Directives, and the Working and Crystallized Memory states reach the agent before it touches your task. Emerging Memory is tagged `#KeepInMind`, so it's read at startup and refreshed at defined points.
4. **It selects the routes the task needs.** Each entrypoint lists its children under `Entries`, one line each, with a description and tags. The agent reads those lines and opens only the branches that matter.
5. **It works within the loaded rules** and saves what's worth keeping for the next task in Memory.

For a frontend task in a workspace with a frontend scope, the path looks like this:

```text
AGENTS.md
└─ .agents/loader.md                          read first
   ├─ directives/_directives.md               #LoadNow: read at startup
   │  ├─ testing.md                           #LoadNow: applies everywhere
   │  └─ frontend/_frontend.md                on demand: selected for this task
   │     └─ components.md                     #LoadNow inside the selected scope
   ├─ memory/_memory.md                       #LoadNow
   │  └─ crystallized/_crystallized.md        #LoadNow
   └─ templates/_templates.md                 on demand: not needed, never opened
```

A database scope next to `frontend/` would stay closed. Its entry line is visible, so the agent knows it exists, but none of its content loads.

## See what loads

With the CLI installed, you can read exactly what an agent would load at startup:

```sh
open-forge context
```

To see the route tree, or inspect how one file behaves:

```sh
open-forge route list
open-forge route inspect .agents/memory/_memory.md
```

`open-forge status` summarizes the workspace, and `open-forge doctor` checks its structure without changing anything.

## Keep what's worth keeping

As the work unfolds, the agent follows the Memory rules it loaded: temporary state goes in **Working**, unsettled findings in **Emerging**, accepted knowledge in **Crystallized**, and history in **Archived**. It saves outcomes deliberately rather than logging every conversation.

Read more in [Memory](../concepts/memory.md).

**Next:** [Grow your own framework](grow-your-framework.md).
