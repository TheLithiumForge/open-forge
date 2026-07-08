---
open-forge:
  description: The Core layer installs directives, patterns, guidance, skills, workflows, and workspace with their accepted meanings
  tags: [Memory, Decision, CurrentTruth, Core, Primitive]
---

# Core Primitives

Accepted decisions extracted from the design sessions and idea notes on 2026-07-06.

- The #Core layer contains the required base routing and primitive routes.
- Core installs `directives/`, `guidance/`, `patterns/`, `skills/`, `workflows/`, and `workspace/`.
- Directives are mandatory instructions agents must follow when they apply.
- Patterns are concrete reusable shapes for code, files, APIs, documents, and other inspectable work.
- Guidance is contextual advice for recurring choices, tradeoffs, and scenarios.
- Skills are bounded reusable agent capability packages with clear use cases and expected results.
- Skills should align with native AI-tool skill shape: `.agents/skills/{skill-name}/SKILL.md` plus optional package resources such as `references/`, `scripts/`, and `assets/`.
- Workflows are repeatable markdown recipes for reaching defined goals, not runtime orchestration objects from an agent SDK.
- Workspace routes point to important project locations and explain when to use them.
- Workflows may own local `directives/`, `patterns/`, `guidance/`, and `skills/` categories when those routes are essential to that workflow.
