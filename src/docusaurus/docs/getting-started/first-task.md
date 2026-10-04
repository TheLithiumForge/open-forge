---
title: Your first task
description: What happens when an agent starts work in an Open Forge workspace, and how to check what it loaded.
---

# Your first task

Give your agent a real task. You don't need a spec up front. Start with the request and add detail as the work calls for it.

For one walkthrough from installation to a saved useful fact, follow the [ten-minute guide](ten-minute-guide.md). This page explains what the agent loads and how to check it.

For a first look at what the agent reads, try a small task in an existing project:

> Explain how this project runs its tests. Follow `AGENTS.md`, and name the Open Forge files you read.

The answer should describe the test setup and list the loader, entrypoints, and any CLI usage guide it read.

For everyday work, just ask for the result you want: fix a bug, explain a part of the code, or add a feature. You don't need to name every Open Forge file or command. Review the agent's work as usual, and keep useful corrections in the workspace when you want them to apply next time.

The rest of this page explains what happens underneath. The [ten-minute guide](ten-minute-guide.md#2-add-a-rule-and-a-reusable-shape) shows how to add your first rule and Pattern.

<details>
<summary>See the agent's loading sequence</summary>

## What the rules ask the agent to do

1. **It reads `AGENTS.md`.** The installed block tells it to read `.agents/loader.md` before starting.
2. **It reads the loader.** The loader defines the routing and loading rules, and lists the root routes present. Full Core has Directives, Guidance, Maps, Memory, Patterns, Skills, and Templates. Essentials and Custom follow your [installation choices](installation.md#choose-the-installed-routes).
3. **It loads what's marked for startup.** Entries tagged `#LoadNow` are read right away. That's how the installed root entrypoints (all but Templates), your workspace-wide Directives, and the installed Working and Crystallized Memory entrypoints reach the agent before it touches your task. The Emerging Memory entrypoint is tagged `#KeepInMind`, so when present it's read at startup and read again at each refresh point: task start or resume, after context restoration, and before handoff or closeout. Git-ignored routes load the same way.
4. **It reads each entrypoint's rules and map.** An entrypoint gives the agent its category's purpose, rules, and one line per item under `Entries`. Linked files open when selected, tagged to load, or explicitly required by a loaded rule. The Skills entrypoint requires the CLI usage guide at startup. With Planning installed, the agent sees an entry for Decisions in Crystallized Memory, but reads no Decision at startup.
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

"Entrypoint at startup" means the entrypoint is read. Linked items open when selected, tagged to load, or required by a loaded rule. "Entrypoint on demand" means the entrypoint itself waits until selected. A database scope next to `frontend/` would stay closed. Its entry line is visible, so the agent knows it exists, but none of its content loads.

</details>

## See what loads

With the CLI installed, you can see what the rules load at startup:

```sh
open-forge context skills/open-forge-cli
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
