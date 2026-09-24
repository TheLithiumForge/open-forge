---
title: Grow your own framework
description: Turn a correction you keep repeating into a rule every task starts with, then scope it so it costs nothing elsewhere.
---

# Grow your own framework

The base Framework is deliberately small. It becomes useful as you add what your project keeps teaching you.

## Add your first rule

Say your agent keeps calling work done without running the tests. Create `.agents/directives/testing.md`:

```md title=".agents/directives/testing.md"
---
open-forge:
  description: Run the tests before calling a change done
  tags: [LoadNow, Directive, Testing]
---

# Testing

## Instructions

- Run the test suite before reporting a change as done, and include the result.
```

Then run `open-forge index`, or replace the `none` placeholder under `Entries` in `.agents/directives/_directives.md` with the line it would generate:

```md title=".agents/directives/_directives.md"
- [Run the tests before calling a change done](testing.md) - #LoadNow #Directive #Testing
```

Directives load at startup, so every task now begins with that rule in context.

## Put it in the right place

The same move works for everything your project teaches you. Pick the category by the question the content answers:

| You have...                                  | Make it a...                                    | Because it answers...                                   |
| -------------------------------------------- | ----------------------------------------------- | ------------------------------------------------------- |
| A correction you keep repeating              | Directive                                       | What behavior is required in this scope?                |
| Advice that depends on the situation         | Guidance                                        | What approach is recommended, and when does it fit?     |
| A structure that makes mistakes easy to spot | Pattern or Template                             | What reusable shape makes related work easy to inspect? |
| A method that keeps paying off               | Workflow recipe behind the `use-workflow` Skill | How do I reach this goal, step by step?                 |
| A specialized capability                     | Skill                                           | Which specialized capability would help with this work? |
| A finding or an accepted decision            | Memory                                          | What is worth remembering for later work?               |

The [Core categories](../concepts/core-categories.md) page explains each one.

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

A task selects the branches it needs, and a task that spans both follows both on purpose. There's no fixed limit on how many scopes you add or how deep they go. The CLI can create a scope for you:

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

Writing each file from scratch gets old. The [Core Templates](../extensions/core-templates.md) Extension gives you one starter for each category, and the more specialized Extensions add starters for tasks, decisions, scenarios, and project documents.

**Next:** learn how it all fits together in [Concepts](../concepts/index.md).
