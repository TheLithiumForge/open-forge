---
title: Everyday flows
description: Common situations where the CLI helps, from setting up a workspace to finding out why an agent ignored a rule, with the commands for each and why they help.
---

# Everyday flows

Each flow starts from a common situation, then gives the commands in order and what to look for. The [command reference](/guides/cli) has every option, and the [glossary](../glossary.md) defines the terms.

Some flows use files that come from Extensions, not from the base install: Documents from the Project Documents Extension, and Decisions and their Template from the Planning Extension. The first flow installs both through `development-toolkit`.

## Set up a workspace

**When:** you're adding Open Forge to a project, new or existing.

```sh
open-forge install --dry-run
open-forge install
open-forge extension install development-toolkit --dry-run
open-forge extension install development-toolkit
open-forge status
```

`development-toolkit` is an optional Extension. It installs Project Documents, Planning, Flows and Scenarios, and Development, plus Workflow Support, which they depend on. Skip the two `extension install` commands if you want only the base.

**Look for:** what each dry run will create. The install preview gives a count, and `--detail standard` lists every file. If you already have an `AGENTS.md` or `CLAUDE.md`, the install adds an Open Forge section and keeps your content. `status` shows how much context loads at startup, which Extensions are installed, and whether anything needs attention. Review everything with `git status` and `git diff`, then commit.

**Why it helps:** you see what will be installed before any agent reads it. The CLI also records what it installed, so later updates can tell its files apart from yours.

## Find out why an agent ignored a rule

**When:** you wrote a rule and the agent behaves as if it never saw it.

```sh
open-forge context --for src/Order.cs --for web/order.ts
open-forge route inspect directives/frontend/components \
  --for src/Order.cs --for web/order.ts
```

**Look for:** whether the rule applies to either supplied working path. Include the
complete path set on each invocation, including planned files that do not exist
yet. One file must satisfy the source's whole inherited condition chain.
`applyTo` only filters, so a matching rule still needs a loading tag, or an
explicit request, to load.

If paths are unknown, `context` reports the conditions of tagged entries as
pending, returns an incomplete result, and exits `3`. `route inspect` can explain a
nonmatching source without activating its automatic children. Neither command
globally selects hidden ancestors or discovers code dependencies. If a related
caller or test is part of the work, add its path and load its context before
editing it.

The usual causes are a missing `LoadNow` tag or a rule placed in a scope that
the task never selects. `find --for src/Order.cs --tag=Directive` lists rules
that match the file wherever they live. Use `route update` to correct tags or
`applyTo`, then check `context` again.

**Why it helps:** "the model didn't listen" becomes something you can check.
Often the rule was never set to load, which is quick to fix. The command shows
what the rules select, not what a model actually read. A file condition
controls when context applies, not whether related files may be edited.

## Record something new

**When:** you made a decision, or noticed a convention worth writing down.

```sh
open-forge find --tag=Decision
open-forge route create memory/crystallized/decisions/money-in-cents \
  --template=templates/planning/decision \
  --description="Every amount is a whole number of cents" \
  --tag=Memory --tag=Decision \
  --dry-run
```

The Decisions category and the `decision` Template come from the Planning Extension.

**Look for:** whether `find` turns up a Decision that already covers it. If one does, update that one instead of adding a competing copy. Otherwise, `route create` makes the file with correct frontmatter, copies the Template's body into it, and adds it to its parent's `Entries`. You fill in the body.

**Why it helps:** you keep one current answer per question, and the new file is listed in `Entries` right away, so agents can find it.

## Reorganize without breaking links

**When:** a folder has grown, or a file belongs in a narrower scope.

```sh
open-forge references memory/crystallized/documents/billing --direction=in
open-forge route init memory/crystallized/documents/payments --dry-run
open-forge route init memory/crystallized/documents/payments
open-forge route move memory/crystallized/documents/billing \
  .agents/memory/crystallized/documents/payments/billing.md --dry-run
open-forge doctor
```

Here `billing` is a Document you wrote, inside the Documents category from the Project Documents Extension.

**Look for:** everything that links to the file before you move it. `route init` creates the new scope, which must exist before the move can be planned. The move plan shows which links and `Entries` it updates. Run the move again without `--dry-run` to apply it, then run `doctor` to confirm nothing still points at the old place. `route move` moves only files you created, not files installed by the Framework or an Extension. To delete a file instead, use `route remove`. It records the removal and turns links to the file into plain text.

**Why it helps:** moving files by hand breaks links without any warning, and an agent that follows a dead link finds nothing.

## Keep the workspace healthy

**When:** once in a while, after a big merge, or when `status` shows a warning.

```sh
open-forge status
open-forge doctor
open-forge repair --automatic --dry-run
open-forge index --dry-run
```

**Look for:** what `doctor` names and the next step it suggests. `repair --automatic` fixes broken local links that have one safe answer and reports the rest. To choose a target for those yourself, run `repair` without `--automatic` in a terminal. `index` rebuilds `Entries` that no longer match their files, usually after someone added or renamed files by hand.

**Why it helps:** agents find files through `Entries`. A file that's missing from its parent's `Entries` is hidden from them.

## Take a Framework update

**When:** a new Open Forge version is out. `update` installs the Framework bundled with your CLI, so update the CLI first.

```sh
git commit -am "Before Open Forge update"
open-forge update --dry-run
open-forge update
git diff
open-forge cleanup --dry-run
```

**Look for:** every shipped file the update would replace. That includes shipped files you edited: the update replaces them and keeps their previous content in a recovery bundle. Because you committed first, `git diff` shows those changes too. To keep a change through updates, move it into an [overwrite companion](../concepts/customizing.md#overwrite-companions) (`{name}.overwrite.md`), which updates never touch. After checking the result, `cleanup` removes the recovery copies the update kept.

**Why it helps:** you get Framework improvements through a plan you've reviewed, and nothing is replaced without being listed first.

## Drop what you don't use

**When:** a default category or an Extension doesn't fit how you work.

```sh
open-forge remove .agents/templates --dry-run
open-forge remove orchestration --kind extension --dry-run
```

**Look for:** the files that go, and the exclusion recorded in `.agents/open-forge.json`.

**Why it helps:** deleting a file by hand lets the next update bring it back. A removal through the CLI is recorded, so later updates leave it out.

## Share rules across repositories

**When:** several repositories should follow the same team rules or share one knowledge base.

Keep the shared files in one folder inside each repository, for example a Git submodule, laid out the way they should appear in the workspace. Files under `.agents` need an existing scope to land in. Create one with `route init` first, such as `guidance/team`, instead of linking straight into a category the Framework installed. Then attach the folder:

```sh
open-forge library attach team-rules vendor/team-rules --dry-run
open-forge library attach team-rules vendor/team-rules
```

**Look for:** the list of links the attach creates. Each one is a relative file link, so the files themselves stay in the shared folder. When the shared source changes, `library sync team-rules` brings the links up to date. `library detach` removes the links and leaves the source alone.

**Why it helps:** each repository links to the shared folder instead of keeping its own edited copy. When the shared source changes, `library sync` brings the links back in step.

## Automate it

**When:** you want CI, or another tool, to check the workspace.

```sh
open-forge doctor --format json
```

**Look for:** the exit code. `0` means completed. Any other code means the result needs attention, so a CI job fails when `doctor` finds a warning or an error. `--format json` returns one structured result for other tools to read.

**Why it helps:** a broken link or stale `Entries` fails the check before it reaches an agent.
