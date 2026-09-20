---
open-forge:
  description: What was chosen, why, and what follows from the choice
  tags: [Extension, Memory, Decision, Rationale, CurrentTruth]
---

# Decisions

## What was chosen, why, and what follows from the choice?

A Decision records an important accepted choice and why it was made. It may also preserve consequences and context that help future work.

## Axioms

- Check `Entries` when the work needs the reason behind an important choice.
- Record what was chosen, why, and which applicable authority accepted it. Keep unaccepted proposals in a candidate route. Link to the source that defines the current result when one exists.
- Keep alternatives, tradeoffs, constraints, and consequences only when they help future work.
- Keep each Decision focused on one choice or a tightly related group. Split unrelated choices. Keep exact current specifications in the sources that define them.
- Consolidate, reshape, or link overlapping Decisions when their reasoning agrees. Archive or link the reasoning behind a replaced choice. Surface important disagreement instead of merging it silently.

## Entries

- [Why replacement CLI dependency roles are narrow, centrally pinned, and constrained by Native AOT and trimming](cli-dependency-policy.md) - #Memory #Decision #CurrentTruth #CLI #Dependency #DotNet #NativeAOT #Security
- [Accepted rationale for optional Extension packaging, ownership, and catalogue choices](extensions/_extensions.md) - #Memory #Decision #CurrentTruth #Extension #Rationale
- [Accepted Framework rationale for routing, authority, Core roles, Memory, Markdown, packaging, and user-facing writing](framework/_framework.md) - #Memory #Decision #CurrentTruth #Framework #Rationale
- [Accepted product-direction rationale, distinct from Framework and implementation decisions](product/_product.md) - #Memory #Decision #CurrentTruth #Product #Rationale
- [The replacement CLI uses repository-root .NET tooling and an automatically built local development publication for ordinary tests](repository-root-cli-tooling.md) - #Memory #Decision #CurrentTruth #CLI #DotNet #Testing #DeveloperExperience
