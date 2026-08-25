---
open-forge:
  description: Preserve material review findings with stable identity while avoiding duplicate rationale, repeated review, and observation noise
  tags: [LoadNow, Core, Directive, Review, Evidence, Reasoning, Tradeoff, Observation, Dogfooding]
---

# Review Evidence

## Instructions

- Give every blocking or material review finding a stable identifier, severity, category, exact location, evidence, consequence, smallest credible correction, and earliest invalidated boundary when applicable.
- Keep an independent first pass independent. Do not prime a fresh reviewer with earlier conclusions merely to measure agreement. After return, compare only the material findings with prior evidence.
- Group accepted findings into one correction packet. Prefer the original implementation owner for local corrections. Recheck the changed finding identifiers and affected neighborhood rather than rerunning the complete review when the rest of the artifact is unchanged.
- A routine pass or no-finding result needs only a concise conclusion, evidence coverage, and residual risk. Require a fuller rationale, strongest alternative, tradeoffs, and change conditions only when the review changes a consequential decision, exposes a surprising failure mode, resolves material disagreement, or produces reusable evidence.
- Record accepted, rejected, duplicate, preference-only, and false-positive dispositions when review yield matters. Do not treat raw finding count as quality.
- Preserve reusable review evidence in the active task or matching Emerging Observation only when recurrence, cost, surprise, or decision value justifies future discovery. Do not create one Observation per invocation or duplicate the same rationale across task, review, and memory records.
- Sanitize reusable evidence. Keep repository-relative locations and behavior-level facts. Omit provider, model, runtime profile, session, hidden orchestration, personal, machine, secret, token, local absolute-path, and incidental environment identifiers.
- Repeated agreement is promotion evidence, not automatic authority. Preserve counterexamples and propose the smallest supported Directive, Pattern, Workflow, agent change, or implementation change only after impact and recurrence justify it.
