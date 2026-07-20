---
open-forge:
  description: The Core layer installs directives, patterns, guidance, skills, workflows, and workspace with their accepted meanings
  tags: [Memory, Decision, CurrentTruth, Core, Primitive]
---

# Core Primitives

Accepted decisions extracted from the design sessions and idea notes on 2026-07-06.

- The #Core layer contains the required base routing and primitive routes.
- Core installs `directives/`, `guidance/`, `patterns/`, `skills/`, `workflows/`, and `workspace/`.
- Directives are mandatory instructions whose scope is selected before their bodies are opened. Every direct directive file carries #LoadNow. Loading the root route reads workspace-wide direct files; loading a selected child route first establishes its positive narrower scope, then reads its direct files through the same generic rule. A loaded directive has no second applicability gate.
- Patterns are concrete reusable shapes for code, files, APIs, documents, and other inspectable work.
- Guidance is contextual advice for recurring choices, tradeoffs, and scenarios.
- Skills are ordinary `SKILL.md` capabilities that Open Forge makes routable without redefining their activation or internal navigation.
- Skills use `.agents/skills/{skill-name}/SKILL.md` plus any resources the skill itself references, such as `references/`, `scripts/`, and `assets/`.
- Workflows are repeatable markdown recipes for reaching defined goals, not runtime orchestration objects from an agent SDK. Visible descriptions and tags support selection, complete recipes declare one non-waterfall development phase for wayfinding, and the routed Goal owns execution and completion. Optional `- helpful before: ...` prior work is advisory and never blocks the selected workflow.
- Workspace routes are coarse maps to important project locations and explain where, when, and why to use them without replacing the destinations or memory.
- Workflows may own local `directives/`, `patterns/`, `guidance/`, and `skills/` categories when those routes are essential to that workflow. A selected workflow-local directive route adds binding scope and never creates a silent override over loaded ancestor directives.
