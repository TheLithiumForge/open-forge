---
title: Your first task
description: What happens when an agent starts work in an Open Forge workspace, and how to check what it loaded.
---

# Your first task

Give your agent a real task. You don't need a spec up front. Start with the request and add detail as the work calls for it.

For a first look at what the agent reads, try a small task in an existing project:

> Explain how this project runs its tests. Follow `AGENTS.md`, and name the Open Forge files you read.

The answer should describe the test setup and list the loader and the entrypoints it read.

## What the rules ask the agent to do

1. **It reads `AGENTS.md`.** The installed block tells it to read `.agents/loader.md` before starting.
2. **It reads the loader.** The loader defines the routing and loading rules, and lists the root routes: Directives, Guidance, Maps, Memory, Patterns, Skills, and Templates.
3. **It loads what's marked for startup.** Entries tagged `#LoadNow` are read right away. That's how the root entrypoints (all but Templates), your workspace-wide Directives, and the Working and Crystallized Memory entrypoints reach the agent before it touches your task. The Emerging Memory entrypoint is tagged `#KeepInMind`, so it's read at startup and read again at each refresh point: task start or resume, after context restoration, and before handoff or closeout.
4. **It stops at each entrypoint.** An entrypoint gives the agent its category's purpose, its rules, and one line per item under `Entries`, with a description and tags. The files and folders those lines point to stay closed unless their entry is tagged `#LoadNow` or `#KeepInMind`. For example, with the Planning Extension installed, the agent sees a one-line entry for Decisions in Crystallized Memory, but reads no Decision at startup.
5. **It selects the routes the task needs.** The agent reads the entry lines and opens only the branches that matter. [`applyTo` patterns](../concepts/loading-and-tags.md#file-conditions) keep an entry out of tasks that don't touch matching files.
6. **It works within the loaded rules** and saves what's worth keeping for the next task in Memory.

For a frontend task in a workspace with a frontend scope and the Planning Extension installed, the path looks like this:

```text
AGENTS.md                                     read at startup, first
└─ .agents/loader.md                          read in full, next
   ├─ directives/_directives.md               entrypoint at startup (#LoadNow)
   │  ├─ testing.md                           read with its entrypoint (#LoadNow)
   │  └─ frontend/_frontend.md                entrypoint on demand: selected for this task
   │     └─ components.md                     read once frontend/ is selected (#LoadNow)
   ├─ memory/_memory.md                       entrypoint at startup (#LoadNow)
   │  └─ crystallized/_crystallized.md        entrypoint at startup (#LoadNow)
   │     └─ decisions/_decisions.md           entrypoint on demand, from Planning: entry visible, not opened
   └─ templates/_templates.md                 entrypoint on demand: not needed, never opened
```

"Entrypoint at startup" means the entrypoint is read, and the items it lists open on demand unless their entry is tagged, like `testing.md`. "Entrypoint on demand" means the entrypoint itself waits until a task opens it. A database scope next to `frontend/` would stay closed. Its entry line is visible, so the agent knows it exists, but none of its content loads.

## See what loads

With the CLI installed, you can see what the rules load at startup:

```sh
open-forge context
```

When you know which files the task will touch, pass every path with `--for`. You can include planned files that do not exist yet. For example, if the task adds a C# file and updates its TypeScript caller, select the relevant routes and include both paths:

```sh
open-forge context --for src/orders/new-order.cs --for web/orders.ts
```

Context checks the conditions on entries those routes expose. Adding the TypeScript path lets its own rules apply without extending C# rules to that file. Context uses the paths you supply and does not infer related files. If the paths aren't known yet, startup works as usual for entries without conditions. Only conditioned `#LoadNow` and `#KeepInMind` entries that a loaded parent exposes remain pending. Other conditioned entries stay on demand.

To see the route tree, or inspect how one file behaves:

```sh
open-forge route list
open-forge route inspect .agents/memory/_memory.md
```

`open-forge status` summarizes the workspace, and `open-forge doctor` checks its structure without changing anything.

## Keep what's worth keeping

As the work unfolds, the agent follows the Memory rules it loaded: temporary state goes in **Working**, unsettled findings in **Emerging**, accepted knowledge in **Crystallized**, and history in **Archived**. The rules ask it to save outcomes deliberately rather than log every conversation.

Read more in [Memory](../concepts/memory.md).

**Next:** [Grow your own framework](grow-your-framework.md).
