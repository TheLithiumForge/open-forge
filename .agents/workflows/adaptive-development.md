---
open-forge:
  description: Deliver development work through one persistent primary owner with proportionate planning, implementation, testing, cleanup, and optional bounded delegation instead of mandatory phase separation
  tags: [Workflow, Development, Adaptive, Implementation, Testing, Review, Delegation]
---

# Adaptive Development

## Goal

Deliver authorized development work with one primary owner retaining intent, architecture, planning, integration, and acceptance while using bounded helper agents only where a separate context materially helps.

This is a secondary development workflow. It does not replace the stricter Development workflow or its phase recipes. Use either workflow directly, or compose a relevant strict phase into this workflow when that phase adds value.

## Steps

1. **Orient.** Establish the requested outcome, applicable workspace rules, source authority, affected surfaces, and success evidence. Delegate bounded repository discovery to `explorer` or external evidence gathering to `researcher` when that is cheaper than loading broad evidence into the primary context.
2. **Plan.** Keep planning in the primary context. Use a micro-plan for small reversible work and an executable plan for nontrivial work. When relevant, state canonical source, dogfood or local counterpart, generated surfaces, public or installable projection, scope, non-goals, ordered work, validation, and stop conditions.
3. **Harden proportionately.** Challenge the plan internally first. Use `advisor` or the Council workflow when diverse perspectives matter. Use `challenger` only for consequential architecture, security, migration, public compatibility, hard-to-reverse work, or material unresolved disagreement.
4. **Implement and test.** Implement directly while substantial contextual judgment remains. Delegate to `executor` only when the plan is closed enough that no material product, architecture, authority, or compatibility decision remains. Keep required tests with the implementation rather than creating a separate testing phase by default.
5. **Clean up when earned.** Refactor only when the change reveals a material structural improvement or the accepted result would otherwise remain unnecessarily difficult to maintain. Keep behavior protected by evidence.
6. **Review proportionately.** The primary owner inspects the actual result and verification. Use one fresh `reviewer` when an independent bounded read is likely to catch omissions cheaply. Use `challenger` as a final falsification only when consequence warrants it.
7. **Correct and close.** Route a material finding to the earliest invalidated assumption, plan step, contract, or implementation surface. Avoid repeated review loops. Finish when the requested outcome, relevant authority relationships, and proportionate evidence agree.

### Using the strict Development workflow in tandem

Use the existing Development workflow, or one of its phase recipes, when the task specifically benefits from stronger separation such as:

- freezing a public callable contract before implementation;
- constructing an explicit expectation matrix before production changes;
- high-risk refactoring where a separate structural pass is useful;
- independent test-structure work;
- a deliberately formal whole-task review and acceptance gate.

Do not invoke those phases merely because they exist. Escalate from this workflow when the risk they control becomes material.

## Completion

- One primary owner retained user intent, architecture, plan, integration, and final judgment.
- Delegated work was bounded and did not invent material decisions.
- Implementation and tests satisfy the accepted outcome.
- Refactoring and independent review were used only where they added value.
- Relevant canonical, dogfood, generated, and public relationships are coherent without accidental mirroring.
- Verification is proportionate to the behavior and consequence of the change.
