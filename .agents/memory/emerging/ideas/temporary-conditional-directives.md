---
open-forge:
  description: Explore whether binding behavior needs explicit temporary or conditional activation beyond ordinary routed scope
  tags: [Memory, Idea, Contextual, Candidate, Directive, Scope, Lifecycle, Temporary]
---

# Temporary And Conditional Directives

## Opportunity

Some binding behavior applies only during a migration, release freeze, incident, experiment, or another bounded condition. Route scope answers where a Directive applies, but it does not by itself express when the behavior should begin, suspend, expire, or be removed.

The Framework migration tested the simplest available representation through an ordinary binding Directive with an explicit trigger, lifecycle statement, and removal condition. The [migration closeout](../../archived/sessions/2026-07-29_open-forge-framework-migration-closeout.md) records the experiment and the proposed retirement of its Directive.

That single experiment does not establish a new Directive type, activation field, or routing semantic.

## Questions

- Is an explicit condition inside an Axiom sufficient, or do repeated cases need deterministic activation metadata?
- Which conditions are safe to evaluate mechanically, such as a date, branch, file, task state, or explicit user action?
- How should temporary behavior remain visible without leaving stale baseline instructions after its purpose ends?
- When is conditional behavior a Directive rather than Guidance, a Workflow step, Working Memory, or a scoped child route?
- How should route scope and time, event, or state conditions compose?
- Who may activate, suspend, extend, or remove the behavior?
- Should `doctor` warn about expired or unresolved temporary conditions?
- Can the Framework support this without reviving applicability gates that make loaded Directives silently optional?

## Evidence And Next Test

The migration Directive remained visible, governed its bounded change cycle, and required deliberate manual retirement. Ordinary routing plus explicit wording was sufficient for this case, but one case does not establish whether repeated or mechanically detectable conditions need stronger support.

For future cases, record whether the trigger is clear, whether agents follow it, whether removal is forgotten, and whether normal lifecycle maintenance remains sufficient. Formalize additional semantics only after repeated evidence shows that explicit Axioms are inadequate.

## Related Records And Current Sources

- [Directive role](../../crystallized/documents/framework/primitives/directives.md)
- [Directive runtime maintenance](../../crystallized/documents/maintenance/payload/agents/directives.md)
- [Loading and continuity](../../crystallized/documents/framework/routing/loading.md)
- [Rejected applicability gates and loading tradeoffs](../../crystallized/decisions/loading-reliability.md)
- [Framework migration closeout](../../archived/sessions/2026-07-29_open-forge-framework-migration-closeout.md)
