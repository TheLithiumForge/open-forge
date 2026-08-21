---
open-forge:
  description: Calibrate delegated agent reasoning to bounded work, concrete risk, complexity, and review independence
  tags: [Core, Guidance, Collaboration, Orchestration, Delegation, Reasoning, Review, Risk]
---

# Calibrated Agent Reasoning

## Scenario

Use this guidance when delegated work can run at different reasoning levels and
the mastermind must balance confidence, independence, time, and token cost.

## Preferred Approach

Bound the delegation first: state its outcome, authority, allowed and forbidden
surfaces, required evidence, and handoff. Then choose a default reasoning level
from the work itself.

Complete caller-owned decisions before delegation. Provide accepted meaning,
known facts, exact targets or search boundaries, non-goals, required validation,
return shape, and stop conditions with enough precision that the helper need not
rediscover the problem. Stop short of performing the assigned action in the
packet itself.

| Default  | Use when                                                                                                                                                                             |
| -------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `medium` | The work is mechanical inspection, state maintenance, or a tightly specified transformation with local failure effects.                                                              |
| `high`   | The work requires phase judgment, contract interpretation, test design, implementation decisions, or structural improvement across related surfaces.                                 |
| `xhigh`  | The work is a read-only Preflight or Whole-Task Review, or another bounded audit whose value depends on finding cross-surface gaps without inheriting the implementer's conclusions. |

Raise or lower the default only when concrete risk, complexity, reversibility,
or evidence cost warrants it. Preserve a fresh context and read-only authority
when independence matters.

## Reasoning

A clear boundary improves the value of additional reasoning because the agent
can spend attention on the actual decision surface. Calibrated defaults give
mechanical work enough rigor, phase work enough judgment, and acceptance gates
enough independent scrutiny without treating maximum reasoning as a universal
quality setting.

Reasoning levels describe the attention budget for one delegation. They do not
rank agent capability, seniority, or trustworthiness, and they do not replace
evidence or independent review.

## Delegation Packets

- Keep assignments non-overlapping unless independent perspectives are the
  explicit purpose. Do not ask several agents to repeat the same exploration,
  implementation, or review.
- Treat a writer packet as a prose specification, an explorer packet as one
  complete bounded evidence question, an implementer packet as a closed plan,
  and a reviewer packet as an exact diff plus accepted requirements and claimed
  evidence.
- Require compact results, validation, uncertainty, and the smallest useful
  escalation rather than raw search or hidden-reasoning transcripts.
- Keep provider, model, and runtime-profile choices in the local agent
  configuration and its package source rather than duplicating them in generic
  repository guidance.
- The primary agent must inspect the integrated artifacts and evidence before
  final acceptance within its delegated scope. Helper reviews add independent
  lenses but do not replace that final review.

## Tradeoffs

Higher reasoning can improve cross-surface analysis, but it costs more time and
tokens and can overvalue speculative concerns when the scope is mechanical.
Lower reasoning is efficient for closed transformations, but it can miss
interactions when authority or evidence spans several sources. Match the level
to the bounded task and record deliberate deviations when they affect review
confidence.
