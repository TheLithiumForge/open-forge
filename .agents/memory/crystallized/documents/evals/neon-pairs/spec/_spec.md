---
open-forge:
  description: The Neon Pairs product spec, the ten documents an agent receives as the game's requirements
  tags: [Memory, Document, CurrentTruth, Evaluation, Game, Specification]
---

# Neon Pairs Spec

The Neon Pairs Spec is the product specification of the memory game, written the way a user would hand it to a developer.

Status: spec version 2.2. Edit these documents, then refreeze them with `node materialize/cli.ts specs` from `src/evals`. Only these documents go into the frozen pack under `src/evals/benchmarks/neon-pairs/specs/`.

Start with the [game overview](README.md). Back to [Neon Pairs](../_neon-pairs.md).

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [Neon Pairs game specification and document map](README.md) - #LoadNow #Game #Specification
- [Public game acceptance examples](acceptance.md) - #Game #Acceptance
- [Game code structure and module responsibilities](architecture.md) - #LoadNow #Game #Architecture
- [Game options and reproduction controls](controls.md) - #LoadNow #Game #Controls
- [Pointer, touch, keyboard and focus behavior](input.md) - #LoadNow #Game #Input
- [Board codes, snapshots and replay formats](reproducibility.md) - #LoadNow #Game #Reproducibility
- [Card matching, moves, timing and victory rules](rules.md) - #LoadNow #Game #Rules
- [Game sound behavior](sound.md) - #LoadNow #Game #Sound
- [Language, package versions, build and distribution](technology.md) - #LoadNow #Game #Technology
- [Layout, card artwork, colors and motion](visuals.md) - #LoadNow #Game #Visuals

