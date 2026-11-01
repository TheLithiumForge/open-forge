---
open-forge:
  description: Editable Neon Pairs specification and frozen pack generation
  tags: [Memory, Document, CurrentTruth, Evaluation, Game, Specification]
---
# Neon Pairs
The Neon Pairs pack is a specification that defines the memory game used by Open Forge Evals.

The ten game documents describe play, controls, artwork, reproducible games, build commands and acceptance. For example, a board code lets you start a reported problem from the same cards. Start with the [game specification](README.md).

## Frozen pack

Edit the game documents here. From `src/evals`, run `node materialize/cli.ts specs` to strip their frontmatter and write the frozen pack under `benchmarks/neon-pairs/specs/`. Its manifest records the source hash and each file's SHA-256.

The import keeps the pilot's text with the equal-information fixes. The other spec gaps from council round 4 wait for the next packet.

Return to [Evals](../_evals.md) for the product documents.

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

