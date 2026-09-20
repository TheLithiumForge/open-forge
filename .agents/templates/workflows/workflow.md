---
open-forge:
  description: "Write a scoped method that composes existing capabilities and has an honest completion boundary"
  tags: [Extension, Template, Workflow]
---

# {Workflow Name}

<!-- TEMPLATE: Use when a repeatable method adds value beyond ordinary work. Place the recipe in a relevant Use Workflow reference scope and update its catalogue.
Replace {prompts}; remove this comment and sections that add no value.
Set metadata for the destination, not this Template. Rebase links after copying. -->

## Goal

{State the outcome and when this method is useful. State when to work directly or choose another method only if the distinction prevents confusion.}

**Required context or capabilities:** {Name only genuine prerequisites. Link to defining sources or use the workspace's configured capability for a stated job. Omit when no extra prerequisites exist.}

## Steps

1. **{Action}.** {Required input or condition, action, and observable result. Distinguish required, conditional, and optional work in plain language.}
2. **{Next action}.** {Name dependencies and a verification point when they matter. A step may invoke several Skills, delegate bounded work, or call another recipe.}

{Describe a meaningful blocked, retry, or resume condition when needed. No arbitrary step or retry limit is required; define what new evidence or changed conditions justify another attempt.}

## Completion

- {Observable outcome and the evidence that establishes it.}
- {What remains explicit when a capability, approval, or required check is unavailable.}

<!-- The recipe cannot grant permissions or invent runtime capabilities. Keep tool-specific invocation in the native capability that owns it.
Do not add a second SKILL.md merely because this recipe has several steps. -->
