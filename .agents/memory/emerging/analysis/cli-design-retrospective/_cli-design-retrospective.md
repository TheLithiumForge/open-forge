---
open-forge:
  description: Retrospective separating the original Framework design, the accepted CLI contracts, and what agents actually built, to learn what to design differently
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Framework, Retrospective, Process, Design]
---

# CLI Design Retrospective

This scope answers a different question from
[CLI Experience Audit](../cli-experience-audit/_cli-experience-audit.md), and the
two must not be merged.

**The audit asks: what is broken and how do we fix it?** Its output is a
remediation backlog against current behaviour.

**This scope asks: how did we get here?** It separates three layers that the
audit deliberately treats as one, because a defect's _origin_ changes what you
learn from it but not what you do about it:

1. **Original Framework design** — decisions made before the CLI existed
   (2026-07-30, `a5dddf16`). The routing model, the Markdown conventions, the
   memory taxonomy, the loading tags. These are the maintainer's own design and
   are open to being judged wrong.
2. **Accepted CLI contracts** — written during the replacement CLI programme
   (from 2026-08-17, `768bd51a`). What the implementers were told to build.
3. **What was built** — the shipped code.

A finding can be a flaw at any of the three layers, and the corrective action
differs: layer 1 changes the Framework, layer 2 changes how contracts are
written, layer 3 is an ordinary bug fix. Only layer 3 belongs in the audit's
backlog.

The retrospective remains in Emerging while the related CLI Tasks are active
and its conclusions still require validation, acceptance, or explicit
supersession. It remains evidence for deciding what to design differently, not
current CLI behavior or execution state.

## Evidence Status

- Lifecycle: **Emerging analysis**; preserve the distinction between original
  Framework design, accepted CLI contracts, and implementation history while
  its actionable questions remain open.
- Authority: the active Tasks own current scope and implementation discoveries.
- Update rule: do not turn this retrospective into a second execution plan.
  Add a new Analysis record when materially new evidence needs a new
  conclusion, and let the owning Task record current implementation reality.
- Retention: keep this scope in Emerging until its open conclusions have a
  current owner and an explicit seal/archive or prune decision.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.
- This contextual analysis may inform active Tasks but does not define current
  CLI behavior or execution state.

## Entries

- [The Directives loading axiom that states half a rule, whether Workflows and Skills are redundant, whether Maps earns a root route, and an assessment of the agent-authored Memory classification axiom](category-boundaries-and-wording.md) - #Memory #Analysis #Contextual #Candidate #Framework #Design #Categories #Axioms #Loading
- [Where the shipped CLI diverges from its accepted contracts, which defects were specified rather than introduced, what the overseer model actually cost, and whether to rebuild](contract-versus-code.md) - #Memory #Analysis #Contextual #Candidate #CLI #Architecture #Contracts #Process #Assessment
- [Whether Memory holds behavior it should not, measured by prescriptive language density, and whether the fix is a negative axiom or a redirect clause on scope responsibility](memory-authority-boundary.md) - #Memory #Analysis #Contextual #Candidate #Framework #Design #Authority #Axioms #Boundary
- [Which painpoints trace to the original Framework design rather than to the contracts or the implementation, judged against how the workspace was actually used](original-design-assessment.md) - #Memory #Analysis #Contextual #Candidate #Framework #Design #Retrospective #Routing #Memory #Loading
