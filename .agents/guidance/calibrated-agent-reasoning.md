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

## Local Development Routing

The selected local Development Workflow uses the following accepted routing:

| Stage             | Model and reasoning |
| ----------------- | ------------------- |
| Mastermind        | Sol/high            |
| Preflight         | Sol/xhigh           |
| Gray, Red, Green  | Terra/medium        |
| Blue, Purple      | Terra/high          |
| Whole-Task Review | Sol/xhigh           |

Do not substitute fast or nano models. Preflight may route Gray, Red, or Green
to Sol/high when security, concurrency, filesystem safety, migrations, external
integrations, public compatibility, or cross-domain architecture makes the
normal middle-phase routing insufficient. APM and runtime-profile implementation
remain separate work.

## Tradeoffs

Higher reasoning can improve cross-surface analysis, but it costs more time and
tokens and can overvalue speculative concerns when the scope is mechanical.
Lower reasoning is efficient for closed transformations, but it can miss
interactions when authority or evidence spans several sources. Match the level
to the bounded task and record deliberate deviations when they affect review
confidence.
