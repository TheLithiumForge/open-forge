---
open-forge:
  description: Preserve material review findings with stable identity while avoiding duplicate rationale, repeated review, and observation noise
  tags: [LoadNow, Core, Directive, Review, Evidence, Reasoning, Tradeoff, Observation, Dogfooding]
---

# Review Evidence

## Instructions

- Give every blocking or material review finding a stable identifier, severity, category, exact location, evidence, consequence, smallest credible correction, and earliest invalidated boundary when applicable.
- For ordinary review, record the accepted baseline plus the current changed and untracked target and give findings stable IDs such as `R1`. Ordinary review may inspect that explicit mutable target and its language-server view; do not describe it as an immutable snapshot or reuse its findings after the target changes without revalidation.
- For explicitly selected coordinated topic review, bind each snapshot record to immutable Git objects. Record its working root, semantic owner and return writer, actual ancestor commit and tree, candidate commit and tree, candidate parent commit and tree, and any separate accepted authority commit and tree. Validate ancestry before using an ordinary range. When a non-ancestor authority is accepted only because its tree equals the candidate parent's tree, record both proven tree identities and do not describe that equivalence as ancestry.
- For coordinated topic review, confirm that every relevant formerly untracked artifact is committed. Review the named commits, trees, and ranges rather than a mutable branch, worktree, or language-server view.
- Keep an independent first pass independent. Do not prime a fresh reviewer with earlier conclusions merely to measure agreement. After return, compare only the material findings with prior evidence.
- Group accepted findings into one correction packet. Prefer the original implementation owner for local corrections. Recheck the changed finding identifiers and affected neighborhood rather than rerunning the complete review when the rest of the artifact is unchanged.
- A coordinated topic finding uses a stable prefix derived from its named review-budget unit and also records the inspected commit and tree, confidence, and missing verification. The read-only coordinator validates intake, routes relevant topics, joins returns, links likely duplicates, and preserves material dissent. It does not decide dispositions, repair artifacts, or change task state.
- The original writer alone records `accepted`, `rejected`, `duplicate`, `preference`, `false-positive`, `fixed`, or `deferred`, performs grouped repair, and changes task state. Before acting on any finding, the writer revalidates it against the current relevant content.
- A routine pass or no-finding result needs only a concise conclusion, evidence coverage, and residual risk. Require a fuller rationale, strongest alternative, tradeoffs, and change conditions only when the review changes a consequential decision, exposes a surprising failure mode, resolves material disagreement, or produces reusable evidence.
- Record finding dispositions when review yield matters. Do not treat raw finding count as quality.
- Preserve reusable review evidence in the active task or matching Emerging Observation only when recurrence, cost, surprise, or decision value justifies future discovery. Do not create one Observation per invocation or duplicate the same rationale across task, review, and memory records.
- Sanitize reusable evidence. Keep repository-relative locations and behavior-level facts. Omit provider, model, runtime profile, session, hidden orchestration, personal, machine, secret, token, local absolute-path, and incidental environment identifiers.
- Repeated agreement is promotion evidence, not automatic authority. Preserve counterexamples and propose the smallest supported Directive, Pattern, Workflow, agent change, or implementation change only after impact and recurrence justify it.
