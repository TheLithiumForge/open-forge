---
title: Collaboration
description: Guidance for exploring ideas and converging on decisions, and a Brainstorming Template for comparisons worth keeping.
---

# Collaboration

Explore an uncertain direction with concrete alternatives, useful questions, and enough detail to make a decision.

- **Package ID:** `collaboration`
- **Depends on:** Nothing
- **Loads at startup:** Only one entry line: the Guidance file's entry in the Guidance [entrypoint](../concepts/routing.md#entrypoints). The Guidance itself opens on demand. The Template stays on demand too, because the Templates root entrypoint isn't read at startup.

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
