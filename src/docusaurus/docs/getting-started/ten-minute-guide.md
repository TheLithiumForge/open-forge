---
title: Ten-minute guide
description: Add one rule, one report shape, and one saved fact, then check that your agent uses them on a small task.
---

# Ten-minute guide

Use Open Forge to give your agent one testing rule, a reusable report shape, and a useful fact for the next task.

This guide uses the default `.agents` context folder. If you chose a custom folder at [installation](installation.md#choose-the-context-folder), use that folder for the rule and Memory paths below.

This test example needs a project with tests and a harness that reads `AGENTS.md` or the `CLAUDE.md` bridge. Starting from an empty folder? Try the [greenfield demo](../demos/greenfield.md) first, or use the [brownfield demo](../demos/brownfield.md) for an app with tests. Your harness still supplies the agent's tools and decides how Skills run.

## 1. Install and try a task

Already installed? Start with the task below. Otherwise, save your current work. Install adds a marked Open Forge section to existing `AGENTS.md` and `CLAUDE.md` files and keeps your text. It creates the selected context files under `.agents/` by default. The [installation guide](installation.md#try-it-and-undo-a-trial) explains how to try it in a clone and undo a trial from a saved baseline.

With Node.js 22.18 or later:

```sh
npm install -g @thelithiumforge/open-forge@beta
cd /path/to/your-project
open-forge install
```

For a first try, choose Essentials. Keep Root keys unless your workspace already declares another metadata form. Review the plan before confirming, then inspect the new files with `git status` and `git diff`. Full Core, Custom, and both metadata forms remain supported. [Installation](installation.md) explains those choices.

Ask your agent:

> Explain how this project runs its tests. Follow `AGENTS.md`, and name the Open Forge files you read.

The answer should name the test command and the files it read. Check the command against the project's own instructions before keeping it as a fact.

Each context folder has an `entrypoint`, normally named `_{folder-name}.md`, with its rules and links under `Entries`. Reading that list doesn't open every linked file. [How agents load context](first-task.md) explains the sequence.

## 2. Add a testing rule

A **Directive** says what the agent must do. This step needs Directives, which every preset includes, Minimal too. If your Custom setup left it out, restore its packaged files first. Review the preview, then apply it:

```sh
open-forge route init directives --framework --dry-run
open-forge route init directives --framework
```

If an ancestor has an explicit exclusion, restore it first as the [CLI reference](/guides/cli#route-init) explains.

Create `.agents/directives/testing.md` using Root keys:

```md title=".agents/directives/testing.md"
---
description: Check code changes before calling them done
tags: [LoadNow, Directive, Testing]
---

# Testing

## Instructions

- Run the relevant tests before reporting a code change as done. Include the command and result. If you cannot run them, explain why.

Use the [test report shape](../patterns/test-report.md) when reporting the result.
```

<details>
<summary>If your workspace uses scoped metadata</summary>

Use an `open-forge:` mapping for the metadata in both files in this guide. Both forms are readable. New files follow `.agents/open-forge.json`, and a missing `frontmatter` setting means scoped. Keep the body of each example unchanged.

For the testing rule:

```yaml
---
open-forge:
  description: Check code changes before calling them done
  tags: [LoadNow, Directive, Testing]
---
```

For the report Pattern in the next step:

```yaml
---
open-forge:
  description: Give test results a consistent shape
  tags: [Pattern, Testing]
---
```

[Installation](installation.md#choose-the-frontmatter-form) explains the two forms.

</details>

Make the new rule discoverable:

```sh
open-forge index --dry-run
open-forge index
```

`index` updates the `Entries` lists. Without the CLI, add the link to `.agents/directives/_directives.md` yourself:

```md
- [Check code changes before calling them done](testing.md) - #LoadNow #Directive #Testing
```

You can also start with plain Markdown and let Index add the missing description and category tags. Your content stays intact, and Index creates missing navigation files for nested folders. See [After you edit files by hand](/guides/cli#after-you-edit-files-by-hand) for new files, renames, and moves.

The root Directives entrypoint loads at startup. `LoadNow` tells the agent to read this rule when that parent loads. Put specialized rules in a [scope](../concepts/scopes.md) when they apply only to that work.

## 3. Add a report shape

A **Pattern** gives related work a reusable shape. This report keeps the test command, result, and remaining checks together.

If your setup omitted Patterns, restore its packaged files first. Review the preview, then apply it:

```sh
open-forge route init patterns --framework --dry-run
open-forge route init patterns --framework
```

If an ancestor has an explicit exclusion, restore it first. The [CLI reference](/guides/cli#route-init) defines restoration behavior, and [growing your framework](grow-your-framework.md#add-an-omitted-category) explains how the selection stays narrow.

Create this file, using the scoped metadata above if your workspace needs it:

```md title=".agents/patterns/test-report.md"
---
description: Give test results a consistent shape
tags: [Pattern, Testing]
---

# Test report

Use this shape for a test-run receipt. The [testing rule](../directives/testing.md) binds when tests must run and what their report must include.

## Example receipt

- Command: `npm test`
- Result: passed, 12 tests
- Not run: integration tests, because the test service was unavailable

The default shape has three required labels in this fixed order, all at the same level with no nested parts. Each appears once, with no optional labels. Commands, counts, and reasons are illustrative. Use this project's actual command and observed result. For a failed run, put the failure in Result. When nothing remains, write `Not run: none`. When no test ran, put `not run` in Result and the reason in Not run.
```

Preview and run `index` again after adding it. A Pattern is the default shape when selected. A justified adaptation is allowed, and a review reports it as a departure separately from conformance. A Directive can make a shape required. [Core categories](../concepts/core-categories.md) explains Patterns and the other roles.

## 4. Keep a useful fact

**Memory** is self-growing Markdown state. You and your agent deliberately save useful records as work produces them. Choose the state by how the next task should treat the record:

| State        | Example from this task                                                    |
| ------------ | ------------------------------------------------------------------------- |
| Working      | "The test change is partly done. Run the integration checks next."        |
| Emerging     | "This test may be flaky. One failed run isn't enough to know why."        |
| Crystallized | "We've confirmed this project's test command and accepted it as current." |
| Archived     | "The old test command, kept with the reason we replaced it."              |

These aren't required stages. An accepted fact can go straight to Crystallized. An unconfirmed finding stays Emerging.

Once you've checked the test command, ask:

> Keep the confirmed test command in Crystallized Memory. Reuse an existing record if one fits. If the command is already documented, link to that source instead of copying its detail.

Records can live directly under a state, such as `.agents/memory/crystallized/testing.md`. Keep shared records listed in `Entries`, using `index` as above when adding them by hand. Named records such as Decisions and Checkpoints come from optional Extensions. You don't need them to save a fact.

Before pausing unfinished work, ask the agent to save where it stands, what's next, and any blocker in Working Memory. Keep the record's path for resuming:

> Resume from `.agents/memory/working/resume.md` and refresh the relevant context.

Use the actual saved path. Essentials keeps new Working records out of ordinary Git commits and leaves `_working.md` eligible for sharing. Already tracked files stay tracked. Generated `Entries` omit private contents. You can find and read local records with the CLI or your normal file tools. Copy any local notes you need before changing machines or discarding the workspace. [Memory](../concepts/memory.md) explains what to keep.

## 5. Check the files and use them on a task

Inspect what you've added:

```sh
open-forge find --tag=Testing
open-forge context directives/testing patterns/test-report
open-forge status
open-forge doctor
```

`find` should locate the rule and Pattern. `context` should return both, along with startup context. `status` summarizes your setup, and `doctor` reports structural problems without changing files. Read the [CLI guide](../cli/index.md) for other inspection tasks and the [command reference](/guides/cli#context) for filtering and search flags.

Now ask your agent to use the rule and report shape:

> Add one regression test for an existing behavior in this project. Keep the behavior unchanged. Follow `AGENTS.md` and use `.agents/patterns/test-report.md` to report the test command and result.

Check that the reply names the command, result, and any checks left to run. Review the changed test. If the agent could not run it, its reply should explain why.

You have one rule, one report shape, and one saved fact. Ask a later task to read the fact, then check that it follows the source it links to. Review the agent's work as usual.

**Next:** [Grow your own framework](grow-your-framework.md).
