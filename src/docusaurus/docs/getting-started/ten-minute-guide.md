---
title: Ten-minute guide
description: Add one rule, keep a useful fact, and use the CLI to see what your agent can find and load.
---

import Tabs from "@theme/Tabs"
import TabItem from "@theme/TabItem"

# Ten-minute guide

Open Forge is Markdown, rules, and links. This guide takes you from installation to one project rule, a reusable shape, and a useful fact that survives the next conversation.

Use a project where your agent reads `AGENTS.md` or the `CLAUDE.md` bridge. Your harness, the tool that runs the agent, still supplies its tools and decides how Skills run.

## 1. Install and try a task

With Node.js 22.18 or later:

```sh
npm install -g @thelithiumforge/open-forge@beta
cd /path/to/your-project
open-forge install
```

Choose Essentials for a smaller start, or Full Core for all built-in routes. Custom lets you choose each route. Then choose root keys or scoped metadata, with root preselected. `--frontmatter root` or `--frontmatter scoped` supplies that choice without its question. Review the selected plan before confirming, then inspect the new files with `git status` and `git diff`. [Installation](installation.md) explains the choices, other setup methods, and `install --configure` for an existing workspace. If you've already installed Open Forge, start with the task:

> Explain how this project runs its tests. Follow `AGENTS.md`, and name the Open Forge files you read.

The agent starts with the loader and required context, then selects the branches that help answer your request. Each folder's `entrypoint`, named `_{folder-name}.md`, gives its rules and a list of links under `Entries`. Reading that list doesn't read every linked file. [Your first task](first-task.md) explains the full startup sequence.

## 2. Add a rule and a reusable shape

A **Directive** says what the agent must do. Suppose it keeps reporting a code change as done without checking the tests. Create this file:

Choose root fields or an `open-forge:` mapping for your workspace. Both are readable in every workspace. New files follow `.agents/open-forge.json`. A missing `frontmatter` setting means scoped.

<Tabs groupId="frontmatter-form">
<TabItem value="root" label="Root" default>

```md title=".agents/directives/testing.md"
---
description: Check code changes before calling them done
tags: [LoadNow, Directive, Testing]
---

# Testing

## Instructions

- Run the relevant tests before reporting a code change as done. Include the command and result. If you cannot run them, explain why.
```

</TabItem>
<TabItem value="scoped" label="Scoped">

```md title=".agents/directives/testing.md"
---
open-forge:
  description: Check code changes before calling them done
  tags: [LoadNow, Directive, Testing]
---

# Testing

## Instructions

- Run the relevant tests before reporting a code change as done. Include the command and result. If you cannot run them, explain why.
```

</TabItem>
</Tabs>

Make the new file discoverable:

```sh
open-forge index --dry-run
open-forge index
```

`index` updates the `Entries` lists. Without the CLI, add the link to `.agents/directives/_directives.md` yourself:

```md
- [Check code changes before calling them done](testing.md) - #LoadNow #Directive #Testing
```

The root Directives entrypoint loads at startup. `LoadNow` tells the agent to read this rule when that parent loads. Put specialized rules in a [scope](../concepts/scopes.md), such as `directives/frontend/`, when they apply only to that work.

A **Pattern** gives related work a reusable shape. For example, a test report can always show the command, result, and any checks left to run.

If your setup omitted Patterns, add its packaged files first. Review the preview, then apply it:

```sh
open-forge route init patterns --framework --dry-run
open-forge route init patterns --framework
```

The [CLI reference](/guides/cli#initialize-a-route-chain) defines restoration behavior, and [growing your framework](grow-your-framework.md#add-an-omitted-category) explains how the selection stays narrow.

Create this reusable shape:

Choose root fields or an `open-forge:` mapping for your workspace. Both are readable in every workspace. New files follow `.agents/open-forge.json`. A missing `frontmatter` setting means scoped.

<Tabs groupId="frontmatter-form">
<TabItem value="root" label="Root" default>

```md title=".agents/patterns/test-report.md"
---
description: Give test results a consistent shape
tags: [Pattern, Testing]
---

# Test report

## Shape

- Command: the command that was run
- Result: passed or failed, with useful details
- Not run: any remaining checks and why
```

</TabItem>
<TabItem value="scoped" label="Scoped">

```md title=".agents/patterns/test-report.md"
---
open-forge:
  description: Give test results a consistent shape
  tags: [Pattern, Testing]
---

# Test report

## Shape

- Command: the command that was run
- Result: passed or failed, with useful details
- Not run: any remaining checks and why
```

</TabItem>
</Tabs>

Run `index` after adding it too. A Pattern is the default shape when selected. A Directive can make a shape required. A justified adaptation of a Pattern is allowed.

A **Skill** packages reusable instructions and resources in `SKILL.md`. The bundled `open-forge-cli` Skill explains how to inspect and maintain these files. The agent follows those instructions and uses the tools its host provides. Reading a Skill doesn't install or run a tool.

The other categories can wait until you need them: **Guidance** offers advice, **Maps** point to sources you already have, and **Templates** are starting files you copy and maintain independently. [Core categories](../concepts/core-categories.md) explains their boundaries.

## 3. Keep a useful fact and resume later

**Memory** is self-growing Markdown state: you and your agent deliberately save useful records as work produces them. Choose the state by how the next task should treat the record:

| State        | Example from this task                                                    |
| ------------ | ------------------------------------------------------------------------- |
| Working      | "The test change is partly done. Run the integration checks next."        |
| Emerging     | "This test may be flaky. One failed run isn't enough to know why."        |
| Crystallized | "We've confirmed this project's test command and accepted it as current." |
| Archived     | "The old test command, kept with the reason we replaced it."              |

These aren't required stages. An accepted fact can go straight to Crystallized. An unconfirmed finding stays Emerging.

Once you've checked the test command, ask:

> Keep the confirmed test command in Crystallized Memory. Reuse an existing record if one fits. If the command is already documented, link to that source instead of copying its detail.

Records can live directly under a state, such as `.agents/memory/crystallized/testing.md`. Keep new records listed in `Entries`, using `index` as above when adding them by hand. Named records such as Decisions and Checkpoints come from optional Extensions. You don't need them to save a fact.

Before pausing unfinished work, ask the agent to save where it stands, what's next, and any blocker in Working Memory. Later, ask it to resume from that record and refresh the relevant context. An entrypoint makes records discoverable. A record's contents load when selected, tagged to load, or required by a loaded rule. [Memory](../concepts/memory.md) explains what to keep and when to update it.

Essentials Git-ignores the whole Working directory. Its records still load normally, but new local notes won't travel through Git. Preserve any notes you need elsewhere before changing machines or discarding the workspace.

## 4. Find context and check the workspace

Use these commands to inspect what you've added:

```sh
open-forge context skills/open-forge-cli
open-forge find --tag=Testing
open-forge context directives/testing patterns/test-report
open-forge status
open-forge doctor
```

`context` batches startup context and selected sources, including the parent entrypoints and child files their loading rules require. Plain `context` follows the loading tags. The Skills entrypoint also requires reading the CLI Skill. Explicitly naming `skills/open-forge-cli` includes that required read in the same batch. Follow explicit read instructions in the returned files too.

After startup is loaded, use `open-forge context directives/testing patterns/test-report --additions-only` to omit the startup set. This flag removes only that set. It doesn't track other files you read earlier. When the task's files are known, repeat `--for <path>` for every working file, including planned files. A matching path filters context but doesn't select a hidden scope.

`find` searches by tags and complete headings. For example, `open-forge find --heading=Instructions --within=body --content=body` returns sources with that heading in their body and shows their bodies. `--within` chooses where to look for matches, and `--content` chooses what to return. You can select frontmatter, body, or named sections. Use your editor's text search for arbitrary phrases.

`status` summarizes the workspace. `doctor` diagnoses broken links, stale navigation, and installation problems. They report what needs attention without changing files. Add `--detail standard` for more explanation, and use `--help` for your installed version's exact interface.

To read a saved fact, select its source too. For a record at the example path above, run `open-forge context memory/crystallized/testing --additions-only` after startup is loaded.

Keep working normally, then preserve the useful result. The [CLI overview](../cli/index.md) lists commands and flags, and the [full reference](/guides/cli) defines their exact behavior. [Grow your own framework](grow-your-framework.md) covers choosing categories and scopes as recurring needs appear.
