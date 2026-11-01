---
open-forge:
  description: Editable Neon Pairs specification and frozen pack generation
  tags: [Memory, Document, CurrentTruth, Evaluation, Game, Specification]
---
# Neon Pairs
The Neon Pairs pack is a set of product specifications and layer plans that defines the memory game used by Open Forge Evals.

Status: spec version 2 and layer plans written. The reference rebuild and later-layer execution are still needed.

The [Neon Pairs Spec](spec/_spec.md) holds the ten game documents an agent receives: play, controls, artwork, reproducible games, build commands and acceptance. For example, a board code lets you start a reported problem from the same cards. The [Spec Notes](spec-notes.md) keep the requirement IDs, the data maps, the measured size and the version 2 decisions.

## Frozen pack

Edit the game documents in the spec folder. From `src/evals`, `node materialize/cli.ts specs` strips their frontmatter and writes the frozen pack under `benchmarks/neon-pairs/specs/`, with a manifest of the source hash and each file's SHA-256. The spec has 97 requirement IDs, mapped in `benchmarks/neon-pairs/requirements.json` and `trace.json`.

## Layer plans

The idea case keeps the baseline prompt and private preferences in [Layer 1](layer-1.md). The player-facing subset and its delivery forms are in [Layer 2](layer-2.md). The full pack, empty answer bank and time limit are in [Layer 3](layer-3.md). The council seed, change cases and regression gates are in [Layer 4](layer-4.md).

Back to [Evals](../_evals.md).

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [Layer 1 idea prompt, current setups, answer bank and L1-v2 checks](layer-1.md) - #Memory #Document #CurrentTruth #Evaluation #NeonPairs #Layer1
- [Layer 2 player clauses, equal delivery forms, prompt and declared open choices](layer-2.md) - #Memory #Document #CurrentTruth #Evaluation #NeonPairs #Layer2
- [Layer 3 full specification prompt, empty answer bank, setups and time limit](layer-3.md) - #Memory #Document #CurrentTruth #Evaluation #NeonPairs #Layer3
- [Layer 4 council seed, feature and repair cases, regression guards and richer context](layer-4.md) - #Memory #Document #CurrentTruth #Evaluation #NeonPairs #Layer4 #Brownfield
- [The Neon Pairs spec's requirement ID syntax, its data maps, its measured size and the decisions behind version 2](spec-notes.md) - #Memory #Document #CurrentTruth #Evaluation #Game #Specification #Requirements
- [The Neon Pairs product spec, the ten documents an agent receives as the game's requirements](spec/_spec.md) - #Memory #Document #CurrentTruth #Evaluation #Game #Specification

