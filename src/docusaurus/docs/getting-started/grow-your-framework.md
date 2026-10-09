---
title: Grow your own framework
description: Turn a correction you keep repeating into a rule every task starts with, and use scopes to keep specialized rules out of unrelated work.
---

# Grow your own framework

The base Framework is deliberately small. The selected categories start empty apart from Memory state entrypoints and the CLI Skill. It becomes useful as you add what your project keeps teaching you.

The [ten-minute guide](ten-minute-guide.md) covers the first rule, Pattern, and saved fact together. This page explains how to place later additions and keep them relevant as the workspace grows.

## Make the next task simpler

Start with one recurring problem. If you keep repeating a correction, write one rule. If an answer already lives in your README, wiki, or API documentation, link to it and say when it matters. If a task produces a useful finding, keep it where the next relevant task can find it.

That is the core: Markdown, rules, and links. You can edit them with your usual editor and review them in Git. Leave unused categories empty, and add a scope or Extension when an actual need appears.

When the workspace feels cumbersome, look for repeated content and rules that load for unrelated work. Keep each detailed answer in one place, replace copies with links, and move specialized material into a scope. A small relevant set is easier to understand and maintain.

## Add your first rule

Use the [first rule in the ten-minute guide](ten-minute-guide.md#2-add-a-rule-and-a-reusable-shape) as a starting point. A Directive file keeps required behavior under `## Instructions` and carries `LoadNow`, so the agent reads it when its parent entrypoint loads. Root Directives apply throughout the workspace. Scoped Directives apply once their scope is selected.

After adding a file by hand, preview `open-forge index --dry-run`, then run `open-forge index` to update its parent's `Entries`. Without the CLI, maintain that link yourself. The entry makes the file discoverable and repeats its loading tags.

## Put it in the right place

The same move works for everything your project teaches you. Pick the category by the question the content answers:

| You have...                                  | Make it a...                                    | Because it answers...                                                       |
| -------------------------------------------- | ----------------------------------------------- | --------------------------------------------------------------------------- |
| A correction you keep repeating              | Directive                                       | What behavior is required in this scope?                                    |
| Advice that depends on the situation         | Guidance                                        | What approach is recommended, and when does it fit?                         |
| A structure that makes mistakes easy to spot | Pattern                                         | What reusable shape makes related work easy to create and inspect?          |
| A starting file you keep recreating          | Template                                        | What starting content can be copied, adapted, and maintained independently? |
| A source the agent should know about         | Map                                             | Where is a useful local or external source, and when should it be used?     |
| A specialized capability                     | Skill                                           | Which specialized capability would help with this work?                     |
| A method that keeps saving you time          | Workflow recipe behind the `use-workflow` Skill | What repeatable method would help?                                          |
| A finding or an accepted decision            | Memory                                          | What is worth remembering for current or future work?                       |

The six Core categories and Memory are built-in routes that you can select during [installation](installation.md#choose-the-installed-routes) or add later. Workflow recipes and the `use-workflow` Skill come from the optional [Workflow Support](../extensions/workflows.md) Extension. The [Core categories](../concepts/core-categories.md) page explains each one.

## Change your setup choices

Run `open-forge install --configure` to revisit Essentials, Full Core, or Custom. Custom shows one list that starts with the current concrete selection. Mark an omitted category `+` when it becomes useful, or `~` to keep new files throughout its directory out of ordinary Git commits. Already tracked files stay tracked. Ignored content is still indexed and read.

Marking a category `-` omits supplied defaults and releases their Framework management. It keeps existing files, notes, and overwrite companions in place and routable. For actual deletion, review the separate [`remove` command](/guides/cli#remove-and-keep-removed).

Explicit configuration can restore eligible missing packaged defaults while preserving authored files and narrower omissions. After a checkout, it can restore an ignored route's scaffolding with or without the lock file. It cannot recover private notes that weren't shared. [Installation](installation.md#configure-an-existing-workspace) covers the wizard and unattended setup, and the [CLI reference](/guides/cli#setup-choices) lists Custom row flags.

## Add an omitted category

You can add an omitted Core category later. Name its canonical root with `open-forge route init <category> --framework --dry-run`, review the plan, then run the same command without `--dry-run`. The [ten-minute guide](ten-minute-guide.md#2-add-a-rule-and-a-reusable-shape) shows the commands for Patterns.

Selecting a Core root restores its packaged contents. For example, `skills` includes the bundled CLI Skill. Selecting a Memory state such as `memory/archived` restores only that state and necessary missing ancestors, leaving the other states as they are.

If a broader ancestor was excluded, add it explicitly first. The plan clears only the selected root or state's exact exclusion. It preserves unrelated descendant and file omissions, existing authored files, and overwrite companions.

This restoration applies to unscoped canonical Core roots and Memory states. A scoped target still creates its sparse entrypoint chain. See the [CLI reference](/guides/cli#initialize-a-route-chain) for the complete rules.

## Keep it cheap with scopes

Adding knowledge doesn't have to mean every task reads more. Put frontend conventions in a frontend scope and database rules in a database scope:

```text
.agents/directives/
  _directives.md
  testing.md                  <- #LoadNow: applies to every task
  frontend/
    _frontend.md              <- on demand: opened only for frontend work
    components.md             <- #LoadNow once frontend/ is selected
  database/
    _database.md
    migrations.md
```

Scopes narrow rules by subject, while `applyTo` can narrow a source to matching files within a selected route. A matching path doesn't open a hidden scope.

At startup the agent sees each scope only as one entry line in `_directives.md`. A task selects the branches it needs, and a task that spans both follows both on purpose. There's no fixed limit on how many scopes you add or how deep they go. The CLI can create a scope for you:

```sh
open-forge route init directives/frontend \
  --description="Rules for frontend work" \
  --tag=Directive \
  --tag=Frontend \
  --dry-run
```

Run it again without `--dry-run` once the preview looks right.

Read more in [Scopes](../concepts/scopes.md).

## Start from a template

The built-in Templates category starts empty when selected. The [Core Templates](../extensions/core-templates.md) Extension adds a starter for each Core category and one for Memory records. The more specialized Extensions add starters for tasks, decisions, scenarios, and project documents.

**Next:** learn how it all fits together in [Concepts](../concepts/index.md).
