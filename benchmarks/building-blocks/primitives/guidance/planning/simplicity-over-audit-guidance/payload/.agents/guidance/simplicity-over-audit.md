---
open-forge:
  description: Trade-off guidance favoring simple irreversible deletes over reversal bookkeeping
  tags: [Guidance, Tradeoff, Simplicity]
---

# Simplicity Versus Audit Trail

Prefer the simpler irreversible delete over reversal bookkeeping when an audit trail
is not an explicit product requirement.

## Reasoning

Irreversible deletion reduces implementation and storage overhead for removal-style
commands whose history has no accepted product value.

## Tradeoffs

Reversal bookkeeping preserves evidence for compliance, dispute resolution, and
reproducible reporting. Revisit the simpler approach when any of those needs becomes
material.

## Boundary

This is advisory guidance for a recurring trade-off, not a decision. When accepted
project information already requires an audit trail for a specific resource, the
condition for preferring irreversible deletion is not met.
