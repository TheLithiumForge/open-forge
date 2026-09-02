---
open-forge:
  description: Review a bounded change or design with stable finding IDs, evidence-backed consequences, and no automatic duplicate review
  tags: [Extension, Workflow, Quality, Review, Evidence, Efficiency]
---

# Review

## Goal

Produce prioritized actionable findings, or a clear no-findings result, without modifying the target or making conclusions stronger than the evidence.

## Steps

1. Establish the accepted outcome, review horizon, protected surfaces, direct integration neighborhood, workspace rules, claimed evidence, maximum review budget, and consumed review IDs. Stop before invocation when no unit remains.
2. Choose one review path before requiring its intake. For the ordinary path, consume one stable review-budget unit, record the accepted baseline plus the current changed and untracked target, and invoke one bounded reviewer. For the explicitly selected coordinated path, provide a bounded list of independently owned snapshot records, allocate one stable named unit to each relevant topic and none to the read-only Review Mastermind, then let that coordinator validate and route each record and join returns by snapshot and writer. Each coordinated record names its semantic owner and return writer, display and lane, actual ancestor, candidate, candidate parent, and any separate authority commits and trees, exact changed and formerly untracked paths, contracts, protected paths, non-goals, receipts, and topic units, prefixes, and routing. Validate ancestry or explicit candidate-parent tree equivalence and confirm relevant formerly untracked artifacts are committed before launching that record. Workflow and repository conformance stays with coordinator intake and process validation rather than becoming a fifth topic.
3. For an independent first pass, withhold earlier reviewer conclusions and inspect only enough surrounding context to understand responsibilities and interactions. An ordinary reviewer inspects the recorded baseline and explicit current target. A coordinated topic inspects only its named Git objects and does not rely on a mutable branch, worktree, or language-server view.
4. Trace high-risk paths, contracts, consumers, failure modes, compatibility, source and test locality, durable-source transitions, and evidence gaps within the selected review scope.
5. Run or inspect verification matched to a suspected issue and known to be read-only. Challenge each candidate finding against existing safeguards and plausible false positives.
6. Give every blocking or material finding a stable ID, severity, category, location, evidence, consequence, smallest credible correction, and earliest invalidated boundary. An ordinary finding may use an ID such as `R1`. A coordinated topic finding uses its unit-derived prefix and also records its snapshot key, inspected commit and tree, confidence, and missing verification.
7. The Review Mastermind may link likely duplicates and must preserve material dissent. It does not disposition findings, repair artifacts, or change task state. Separate residual risk, optional material improvements, preferences, duplicate candidates, and missing verification.
8. The original writer revalidates each finding against current relevant content, records `accepted`, `rejected`, `duplicate`, `preference`, `false-positive`, `fixed`, or `deferred`, and groups accepted findings into one repair packet.
9. After correction, run fresh affected evidence and recheck changed finding IDs and affected context. Create a new coherent immutable snapshot when a coordinated topic recheck is needed. Rerun the complete review only when the correction materially changes the artifact or review horizon.

## Completion

- Every material finding has stable identity, evidence, consequence, and correction guidance.
- False positives and existing safeguards were considered.
- The target remained unchanged.
- Verification gaps, residual risk, and uncertainty are explicit.
- Duplicate review and repeated rationale were avoided.
- Ordinary review remained compatible with an explicit changed and untracked target. Coordinated topic passes, when selected, stayed read-only, used at most one wave per task snapshot, grouped returns by snapshot and writer, and did not replace a required fresh holistic review.
