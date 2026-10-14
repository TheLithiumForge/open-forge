---
title: Collaboration
description: Guidance for exploring ideas and converging on decisions, and a Brainstorming Template for comparisons worth keeping.
---

# Collaboration

Explore an uncertain direction with concrete alternatives, useful questions, and enough detail to make a decision.

- **[Package ID](../glossary.md#package-id):** `collaboration`
- **Depends on:** Nothing
- **Startup visibility:** This package adds one entry line in the Guidance [entrypoint](../glossary.md#entrypoint). The Guidance file and the Template stay on demand.

These counts describe this package's own entries when their parent routes expose them in `Entries` and are read. Required packages add their own entries. Linked files stay on demand unless selected or required.

## Install

Package files follow your installation choices. Essentials leaves out Templates, so Extension starters are skipped. [Restore the Templates category](index.md#if-your-setup-leaves-out-templates) if you want those starters.

Essentials also leaves out Guidance, so it skips this package's advice. If you want the advice too, preview `open-forge route init guidance --framework --dry-run`, then restore the category with `open-forge route init guidance --framework`.

From your installed Open Forge workspace, preview the package and its required packages:

```sh
open-forge extension install collaboration --dry-run
```

Review the plan. Then apply it:

```sh
open-forge extension install collaboration
```

The CLI installs required packages from the same [catalogue](../glossary.md#catalogue). See the [Extension guide](/guides/extensions#install-an-extension) for scripts using `--automatic`, updates, removal, or a local source.

## Why it exists

Correcting a direction before the work starts costs less than rebuilding afterward. This Guidance has the agent lead with its understanding and a recommendation, ask about one important choice at a time, and give you something concrete to react to. It steers the agent away from both starting on a guess and asking a long list of questions up front.

The Brainstorming Template is for the comparisons worth keeping. Collaboration stays separate from Planning because exploring isn't a required step before work.

## What it installs

```text
.agents/
  guidance/
    adaptive-collaboration.md      <- advice for exploring and converging
  templates/collaboration/
    _collaboration.md              <- Template category entrypoint
    brainstorming.md               <- starter for a saved comparison
```

## What each file is for

### `guidance/adaptive-collaboration.md`

**Kind:** Guidance. **Used when:** exploring an idea, resolving an important uncertainty, clarifying the desired outcome, or finishing broad work.

It shapes how the agent works with you while direction is unclear and when broad work finishes:

- Start from what you've already said and what accepted context settles.
- Build the best current understanding before asking for more.
- Unless you ask for deep analysis, open with the outcome as understood, the strongest recommendation, and at most one important open choice.
- Give you something concrete to react to instead of asking you to invent the solution.
- Match depth to the request: proceed when the outcome is clear, compare a few directions when it's still forming, and go deep only when needed.
- At convergence, summarize what's accepted, what stays open, and the smallest safe next step. Put each accepted outcome in the source that answers its question.
- After broad, important, or hard-to-reverse work, offer an independent review when a fresh perspective could catch omissions or risk. Say what it would check and that it uses extra model tokens, and ask before running it unless you've already authorized that cost.

### `templates/collaboration/brainstorming.md`

**Kind:** Template. **Used when:** a comparison needs to survive the conversation.

Sections: `Starting Point`, `Directions Worth Comparing`, `Recommendation And Next Check`, and `Outcome`. The outcome keeps three things separate: what was **accepted** (and by whom), what's **still open**, and the **destination** where an accepted result was recorded.

## How to use it

> Let's explore options for the caching layer before we pick one.

> Save this comparison as a brainstorm so we can come back to it.

## Good to know

- Collaboration clarifies a direction. [Planning](planning.md) organizes accepted work into a sequence. Either works on its own, and exploration isn't a required stage before action.
- Keep a saved brainstorm in an existing Emerging Memory scope until its outcomes are accepted. Then move accepted outcomes into the sources that define them.
- This package adds no brainstorming category, task lifecycle, or acceptance rule of its own.

Next: [Install this package](#install), or [update and remove Extensions](/guides/extensions#update-and-remove).
