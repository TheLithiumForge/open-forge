---
open-forge:
  description: Explore whether binding behavior needs explicit temporary or conditional activation beyond ordinary routed scope
  tags: [Memory, Idea, Contextual, Candidate, Directive, Scope, Lifecycle, Temporary]
---

# Temporary And Conditional Directives

## Opportunity

Some binding behavior applies only during a migration, release freeze, incident, experiment, or another bounded condition. Route scope answers where a Directive applies, but it does not by itself express when the behavior should begin, suspend, expire, or be removed.

The current [temporary independent review Directive](../../../directives/temporary-independent-review.md) tests the simplest available representation: an ordinary binding Directive with an explicit trigger, lifecycle statement, and removal condition.

This experiment does not establish a new Directive type, activation field, or routing semantic.

## Questions

- Is an explicit condition inside an Axiom sufficient, or do repeated cases need deterministic activation metadata?
- Which conditions are safe to evaluate mechanically, such as a date, branch, file, task state, or explicit user action?
- How should temporary behavior remain visible without leaving stale baseline instructions after its purpose ends?
- When is conditional behavior a Directive rather than Guidance, a Workflow step, Working Memory, or a scoped child route?
- How should route scope and time, event, or state conditions compose?
- Who may activate, suspend, extend, or remove the behavior?
- Should `doctor` warn about expired or unresolved temporary conditions?
- Can the Framework support this without reviving applicability gates that make loaded Directives silently optional?

## Evaluation

Use the independent-review Directive as a dogfood case. Record whether its trigger is clear, whether agents follow it, whether removal is forgotten, and whether ordinary routing plus explicit wording remains sufficient.

Formalize additional semantics only after repeated cases show that explicit Axioms and normal lifecycle maintenance are inadequate.

## Related Current Sources

- [Directive role](../../crystallized/documents/framework/primitives/directives.md)
- [Directive runtime maintenance](../../crystallized/documents/maintenance/payload/agents/directives.md)
- [Loading and continuity](../../crystallized/documents/framework/routing/loading.md)
- [Rejected mechanisms](../../crystallized/decisions/do-not-revive.md)
