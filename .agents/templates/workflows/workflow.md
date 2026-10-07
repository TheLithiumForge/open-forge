---
open-forge:
  description: "Write a scoped method that composes existing capabilities and has an honest completion boundary"
  tags: [Extension, Template, Workflow]
---

# {Workflow Name}

{
Use when a repeatable method adds value beyond ordinary work. Place the recipe in a relevant Use Workflow reference scope and update its catalogue.
Keep steps at the detail needed to follow the method. Replace {prompts}, then remove this guidance and unused optional sections. Preserve Goal, Steps, and Completion in that order.
}

## Goal

{State the outcome, and when this method is useful if that isn't obvious. State when to work directly or choose another method only if the distinction prevents confusion.}

**Required context or capabilities:** {Name genuine prerequisites, linking their sources or the workspace's configured capability. Remove this field when none exist.}

## Steps

1. **{Action}.** {Required input or condition, action, and observable result. Distinguish required, conditional, and optional work in plain language.}
2. **{Next action}.** {Add a step only for distinct work. Name dependencies and verification when needed. A step may invoke Skills, delegate bounded work, or call another recipe.}

{Describe a meaningful blocked, retry, or resume condition when needed. No arbitrary step or retry limit is required. Define what new evidence or changed conditions justify another attempt.}

## Completion

- {Observable outcome and the evidence that establishes it.}
- {What remains explicit when a capability, approval, or required check is unavailable.}

{
The recipe cannot grant permissions or invent runtime capabilities. Keep tool-specific invocation in the native capability that owns it.
Do not add a second SKILL.md merely because this recipe has several steps.
}
