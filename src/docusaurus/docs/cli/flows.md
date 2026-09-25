---
title: Everyday flows
description: The situations where the CLI earns its place, from setting up a workspace to finding out why an agent ignored a rule, with the commands for each and why they help.
---

# Everyday flows

Each flow starts from a situation you'll recognize, then gives the commands in order and what to look for. The [command reference](/guides/cli) has every option.

## Set up a workspace

**When:** you're adding Open Forge to a project, new or existing.

```sh
open-forge install --dry-run
open-forge install
open-forge extension install development-toolkit --dry-run
open-forge extension install development-toolkit
open-forge status
```

**Look for:** the dry run lists every file it will create. If you already have an `AGENTS.md` or `CLAUDE.md`, it adds an Open Forge section and keeps your content. `status` confirms how much context loads at startup and that navigation is complete. Review everything with `git diff`, then commit.

**Why it helps:** you know exactly what landed before your agent reads a word of it, and the CLI records what it installed so later updates can tell its files apart from yours.

## Find out why an agent ignored a rule

**When:** you wrote a rule and the agent behaves as if it never saw it.

```sh
open-forge context
open-forge route inspect directives/frontend/components
```

**Look for:** whether the file appears in the startup context at all. If it doesn't, `route inspect` says why: it may be on demand when you meant it to load at startup, or it may sit in a scope the task didn't select. A missing `LoadNow` tag or a rule placed in the wrong folder are the usual causes. Fix the tags with `route update`, then check `context` again.

**Why it helps:** "the model didn't listen" becomes a fact you can check. Often the agent never received the rule, and that's a five-minute fix.

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

**Look for:** whether `find` turns up a Decision that already covers it. If one does, update that one instead of adding a competing copy. Otherwise, `route create` makes the file with correct frontmatter and adds it to its parent's `Entries`. You fill in the body.

**Why it helps:** one current answer per question, and new files that agents can find immediately.

## Reorganize without breaking links

**When:** a folder has grown, or a file belongs in a narrower scope.

```sh
open-forge references memory/crystallized/documents/billing --direction=in
open-forge route init memory/crystallized/documents/payments --dry-run
open-forge route move memory/crystallized/documents/billing \
  .agents/memory/crystallized/documents/payments/billing.md --dry-run
open-forge doctor
```

**Look for:** everything that links to the file before you move it. `route init` creates the new scope, and the move plan shows which links and `Entries` it updates. `doctor` afterwards confirms nothing is left pointing at the old place. To delete instead, use `route remove`, which also remembers the removal.

**Why it helps:** moving files by hand breaks links silently, and an agent that follows a dead link just finds nothing.

## Keep the workspace healthy

**When:** once in a while, after a big merge, or when `status` shows a warning.

```sh
open-forge status
open-forge doctor
open-forge repair --dry-run
open-forge index --dry-run
```

**Look for:** what `doctor` names and the next step it suggests. `repair` fixes broken local links that have one safe answer and reports the rest for you to decide. `index` rebuilds `Entries` that no longer match their files, usually after someone added or renamed files by hand.

**Why it helps:** agents find knowledge through `Entries`. A stale list hides a file as surely as deleting it would.

## Take a Framework update

**When:** a new Open Forge version is out.

```sh
git commit -am "Before Open Forge update"
open-forge update --dry-run
open-forge update
git diff
open-forge cleanup --dry-run
```

**Look for:** every shipped file the update would replace. If you edited one of them, move your change into an [overwrite companion](../concepts/customizing.md#overwrite-companions) (`{name}.overwrite.md`), which updates never touch. After checking the result, `cleanup` removes the recovery copies the update kept.

**Why it helps:** you get Framework improvements without losing local changes, and nothing is replaced without being listed first.

## Drop what you don't use

**When:** a default category or an Extension doesn't fit how you work.

```sh
open-forge remove .agents/templates --dry-run
open-forge remove orchestration --kind extension --dry-run
```

**Look for:** the files that go, and the exclusion recorded in `.agents/open-forge.json`.

**Why it helps:** deleting a file by hand lets the next update bring it back. A removal through the CLI is remembered.

## Share rules across repositories

**When:** several repositories should follow the same team rules or share one knowledge base.

Keep the shared files in one folder inside each repository, for example a Git submodule, laid out the way they should appear in the workspace. Then attach it:

```sh
open-forge library attach team-rules vendor/team-rules --dry-run
open-forge library sync team-rules
```

**Look for:** the list of links the attach creates. When the shared source changes, `library sync` brings the links up to date. `library detach` removes the links and leaves the source alone.

**Why it helps:** one source of truth for many repositories, with nothing copied and nothing drifting apart.

## Automate it

**When:** you want CI, or another tool, to watch the workspace.

```sh
open-forge doctor --format json
```

**Look for:** the exit code. `0` means completed, and `2` or higher means the result needs attention, so a CI job fails when `doctor` finds a warning or worse. `--format json` returns one structured result for other tools to read.

**Why it helps:** broken navigation gets caught in review, before it quietly makes an agent worse.
