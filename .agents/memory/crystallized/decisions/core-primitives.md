---
open-forge:
  description: The Core layer installs directives, guidance, patterns, skills, templates, workflows, and workspace with their accepted meanings
  tags: [Memory, Decision, CurrentTruth, Core, Primitive]
---

# Core Primitives

Accepted choices about the distinct reusable roles shipped by Core.

- The #Core layer contains the required base routing and primitive routes.
- Core installs `directives/`, `guidance/`, `patterns/`, `skills/`, `templates/`, `workflows/`, and `workspace/`.
- Directives are mandatory instructions whose scope is selected before their bodies are opened. Every direct directive file carries #LoadNow. Loading the root route reads workspace-wide direct files; loading a selected child route first establishes its positive narrower scope, then reads its direct files through the same generic rule. A loaded directive has no second applicability gate.
- Patterns are concrete reusable shapes for code, files, APIs, documents, and other inspectable work.
- Guidance is contextual advice for recurring choices, tradeoffs, and scenarios.
- Skills are ordinary `SKILL.md` capabilities that Open Forge makes routable without redefining their activation or internal navigation.
- Skills use `.agents/skills/{skill-name}/SKILL.md` plus any resources the skill itself references, such as `references/`, `scripts/`, and `assets/`.
- Templates are copy-ready source artifacts intended to be instantiated into independently owned workspace content. They provide useful starting content rather than continuing conformance.
- An instantiated result belongs to its destination and does not inherit authority or updates from its template. Patterns own reusable shapes that should continue guiding related results, while Directives or Axioms own binding requirements.
- Generic templates are fallbacks. Specialized templates exist only when their copy-ready contents differ materially, and users may edit, scope, replace, or remove them.
- Workflows are repeatable markdown recipes for reaching defined goals, not runtime orchestration objects from an agent SDK. Visible descriptions and tags support selection, complete recipes declare one non-waterfall development phase for wayfinding, and the routed Goal owns execution and completion. Optional `- helpful before: ...` prior work is advisory and never blocks the selected workflow.
- Workspace routes are coarse maps to important project locations and explain where, when, and why to use them without replacing the destinations or memory.
- Workflows may own local `directives/`, `patterns/`, `guidance/`, `skills/`, and `templates/` categories when those routes are essential to that workflow. A selected workflow-local directive route adds binding scope and never creates a silent override over loaded ancestor directives.
- Framework contracts refer to #Core collectively when any suitable Core owner may satisfy the requirement. They name a specific primitive when its distinct semantics matter and enumerate concrete routes when the exact shipped defaults are the subject.

The [Framework Architecture](../documents/framework/architecture.md#core-primitives) owns the complete current relationships among these primitives. Their installed entrypoints own exact runtime wording.
