---
title: Scopes
description: How folder placement narrows where content applies, and how that keeps growth from inflating every task's context.
---

# Scopes

A **scope** is a folder in a route that narrows where the content after it applies. Scopes are how Open Forge grows without every task reading more.

## A scope is just a folder with an entrypoint

There's no separate scope syntax. Any routed folder below a root route can act as a scope. Its entrypoint says what the scope means, and everything after it in the path applies only within that subject.

```text
.agents/directives/
  _directives.md
  testing.md                   <- applies to every task
  frontend/
    _frontend.md               <- "Rules for frontend work"
    components.md              <- applies only to frontend work
```

Open Forge doesn't reserve names like `projects`, `teams`, or `platforms`. Use whatever names make your own scopes understandable.

## Placement changes meaning

Put a scope immediately before the first route segment it should narrow. The same subject name means different things in different positions:

| Path                                        | Meaning                                                                               |
| ------------------------------------------- | ------------------------------------------------------------------------------------- |
| `memory/mobile-app/crystallized/documents/` | `mobile-app` has its own Memory, which can hold any of the Memory states.             |
| `memory/crystallized/mobile-app/documents/` | `mobile-app` narrows accepted knowledge only. It may hold several Crystallized roles. |
| `memory/crystallized/documents/mobile-app/` | `mobile-app` narrows Documents only.                                                  |

Scopes nest. `memory/platform/crystallized/payments/documents/` is Documents for payments, inside the accepted knowledge of the platform.

A scope only needs the routes useful there. It doesn't have to mirror another scope or the installed defaults.

## Selecting scopes

An agent selects every scope that's relevant to the task and follows each one as its own chain. A task that touches the frontend and the database follows both on purpose. A task that touches neither opens neither.

Selecting several scopes doesn't merge them or rank them. If two selected scopes disagree about one result, path depth and load order don't decide it. The agent follows clear user direction or the source declared authoritative for that question, and reports anything still unresolved.

## Loading inside a scope

A scope's own entrypoint decides what loads once the scope is selected:

- Entries are on demand by default.
- `#LoadNow` inside a scope means "read this whenever the scope is selected", not "read this at startup".
- A loading tag inside an unselected scope has no effect.

For example, a C# scope may contain a `#LoadNow` design file and an on-demand Windows scope. Selecting C# loads its design rules, not Windows. A `#LoadNow` file under Windows loads only after Windows is selected.

:::tip[Narrow first, then make it mandatory]

Don't mark a broad parent `#LoadNow` just to expose something important deep inside it. Put the content in the narrowest scope that fully describes where it applies, then make it `#LoadNow` there.

:::

## Creating a scope

With the CLI:

```sh
open-forge route init memory/crystallized/documents/project-alpha \
  --description="Project Alpha documents" \
  --tag=Document \
  --tag=ProjectAlpha \
  --dry-run
```

By hand: create the folder, add `_{folder-name}.md` with a description, tags, a title, `## Axioms`, and `## Entries`, then add an entry for it in the parent entrypoint.

Next: [Loading and tags](loading-and-tags.md).
