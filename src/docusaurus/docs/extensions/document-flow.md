---
title: The document flow
description: How Project Documents and Planning keep project knowledge top-down, with Decisions updating Documents and an overview at the top that links to every level of detail.
---

# The document flow

[Project Documents](project-documents.md) and [Planning](planning.md) ship an opinionated way to keep project knowledge. Decisions record changes. Documents are kept top-down, from an overview of the whole to the detail of each part, and each change is meant to reach every level it affects.

This flow belongs to those Extensions. Core only asks that each detail live in the narrowest source that defines it, and that an accepted change reaches that source. [Memory](../concepts/memory.md) works the same with or without the flow. If your project wants a different one, edit the Documents and Decisions categories, or leave them out.

## Decisions update Documents

A Decision and a Document answer different questions:

- A **Decision** records a change: what was chosen, and why.
- A **Document** explains its subject as it is now.

When you accept a Decision, the rules ask the agent to update the Documents it affects before other work depends on them, and no later than closeout at the end of the task. The Document states the result in its own words and links to the Decision when its reasoning helps. The Decision links forward to the Documents that now hold the result, in its `Current Sources` section.

```text
  a choice comes up in the work
               │
               ▼
  Decision ──────── updates ────────►  Document
  what changed, and why                what's true now
               ▲                             │
               └────── links for the why ────┘
```

Keeping them separate helps in both directions. Someone who needs the current answer reads one Document instead of replaying a history of Decisions. Someone who asks "why is it like this?" is one link away from the reason. When a later Decision replaces an earlier one, the Documents change once, and both Decisions keep their reasons.

## An overview on top, detail below

Top-level Documents give an overview of the whole. They summarize each narrower Document and link to it. Each narrower Document explains its own part in more detail and links back up. At any level, a Document links to the Decisions whose reasoning helps a reader. The deeper you go, the narrower the view and the more exact the detail.

```text
Vision                        why it exists, for whom, what the first version does
└─ Architecture               the parts, what each owns, the rules they all keep
   └─ component docs, scenarios   exact behavior and expected results
      └─ code and tests

Decisions sit beside the stack. Each one explains one choice, or a
tightly related group, and links to the level that now holds its result.
```

Take the [greenfield demo](../demos/greenfield.md)'s records. The Vision promises balances that are exact to the cent, and links to the Architecture for how. The Architecture says every amount is a whole number of cents, and links to the Decision that explains why. The scenarios give the exact expected results. Each level says only what a reader needs at that level.

The overview stays current when each change also reaches the levels above it. When a Decision changes a narrower Document, the Documents rules ask for the summaries above it to be updated wherever they no longer match. Kept this way, the top-level Documents give you the current picture of the whole, and every link from there leads to more detail.

The Templates prompt for links in both directions: a Vision links to its Architecture, an Architecture links back to its Vision and down to narrower views, and a Document links to the Decisions behind it.

## Keeping it current

- **Update the Documents as part of accepting the Decision.** Start with the Document that holds the detail, then fix the summaries above it. Tag a Document `Evergreen` when it must stay aligned with the current state. The loader then asks agents to update it before work depends on it, and no later than closeout.
- **Link to specifications instead of copying them.** A Decision that needs exact numbers links to the Document that holds them, so there's only one copy to keep current.
- **Replace, don't compete.** When a Decision is replaced, archive it or link the new one to it, and update the Documents once. That keeps one current version instead of two.
- **Skip what you don't need.** Ideas and Analysis can inform a Decision, but they aren't required steps. A clear request can go straight to a Decision and a Document update.

## Existing docs take part too

Project Documents doesn't ask you to move anything. Your README, a wiki, or a folder of ADRs can hold the current state, and a [Map](../concepts/core-categories.md#maps) tells agents where to find it. A Decision made while adding a feature then updates the README section that describes that feature, and links to it. The [brownfield demo](../demos/brownfield.md) works this way.

## Where the rules live

| Rule                                                                                                          | Where it lives                                                                                                                                  |
| ------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| Keep Documents top-down: top-level Documents summarize the narrower ones they link to, and those link back up | [Documents category](../../../extensions/project-documents/content/.agents/memory/crystallized/documents/_documents.md), from Project Documents |
| When a narrower Document changes, update the summaries above it that no longer match                          | [Documents category](../../../extensions/project-documents/content/.agents/memory/crystallized/documents/_documents.md), from Project Documents |
| Explain a subject so readers don't piece it together from Decisions, and link to a Decision for its reasoning | [Documents category](../../../extensions/project-documents/content/.agents/memory/crystallized/documents/_documents.md), from Project Documents |
| Link to the source that defines the current result, and keep exact specifications there                       | [Decisions category](../../../extensions/planning/content/.agents/memory/crystallized/decisions/_decisions.md), from Planning                   |
| Put detail in the narrowest source that defines it, and update that source when accepted direction changes it | [Loader](../../../open-forge/.agents/loader.md), Core. The flow builds on this general rule                                                     |
| Update affected sources before other work depends on them, and no later than closeout                         | [Memory entrypoint](../../../open-forge/.agents/memory/_memory.md), base Framework. The flow builds on this general rule too                    |
