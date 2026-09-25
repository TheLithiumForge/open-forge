---
title: Collaboration
description: Guidance for exploring ideas and converging on decisions, and a Brainstorming Template for comparisons worth keeping.
---

# Collaboration

Explore an uncertain direction with concrete alternatives, useful questions, and enough detail to make a decision.

- **Package ID:** `collaboration`
- **Depends on:** Nothing
- **Loads at startup:** Nothing. The Guidance appears in the Guidance entries and is read when relevant.

## Why it exists

The most expensive time to find out you built the wrong thing is after building it. This Guidance has the agent lead with its understanding and a recommendation, ask about one important choice at a time, and give you something concrete to react to, instead of charging ahead or burying you in questions.

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

It shapes how the agent talks with you when direction is unclear:

- Start from what you've already said and what accepted context settles.
- Build the best current understanding before asking for more.
- Unless you ask for deep analysis, open with the outcome as understood, the strongest recommendation, and at most one important open choice.
- Give you something concrete to react to instead of asking you to invent the solution.
- Match depth to the request: proceed when the outcome is clear, compare a few directions when it's still forming, and go deep only when needed.

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
