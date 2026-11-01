---
open-forge:
  description: "Public game acceptance examples"
  tags: ["Game", "Acceptance"]
---
# Acceptance
The acceptance specification is a check map that connects Neon Pairs requirements to evidence of correct behavior.

The full game needs checks for the stated rules and for regressions after a change. For example, adding a game option still needs to keep restart, keyboard play and replay working.

## Required checks

Each defining document gives its requirements an ID. These checks cover the full specification:

| Requirement family | Evidence |
| --- | --- |
| Rules `R01-R06` in [Rules](rules.md) | Every size has complete pairs. Reveals, matches, misses, ignored activation, move counts and completion follow the state rules. |
| Rules `R07-R12` | Controlled time checks cover hidden-page pause, resume, restart during mismatch and stale updates. Browser checks cover saved size, per-size bests, invalid input and unavailable storage. |
| Reproducibility `D01-D10` in [Reproducibility](reproducibility.md) | Independent golden vectors check generation. Round trips check snapshots. CLI and game-core replay checks cover action order and exact timeout edges. Invalid board codes and snapshots keep the current game. Invalid replays are rejected. |
| Input `I` in [Input](input.md) | Pointer, keyboard and touch checks cover the same game actions. Focus, accessible names, announcements, right-click, scrolling and blocked actions follow their rules. |
| Controls `C` in [Controls](controls.md) | Browser checks cover each game control, validation feedback and focus. |
| Visuals `V` in [Visuals](visuals.md) | Browser measurements check layout and target sizes. Screenshots and a face sheet support review of states, colors, reduced motion and all 128 face identities. |
| Sound `S` in [Sound](sound.md) | Page load and complete play produce no sound or audio requests. |
| Technology `T` in [Technology](technology.md) | A locked install, type check, tests and build complete. The distribution opens offline without runtime network requests. |
| Architecture `A` in [Architecture](architecture.md) | Source review checks local behavior, pure state changes, browser effects and focused tests. Executable checks cover the specified state and lifecycle contracts. |

Use fixed small deals for most rule checks. Check the largest board separately for complete pair generation, layout and navigation. Browser checks activate cards through their documented controls.

## Independent expectations

Expected decks and replay results come from fixed specification examples or independently calculated fixtures. A check can't use the game implementation to generate its expected answer.

Keep executable requirements separate from source review. A passing browser check doesn't answer whether behavior stays local. A source review doesn't replace a failed turn check.

## Results

Keep a result for every required check with its requirement ID, outcome and supporting artifact. Keep errors, timeouts and missing evidence distinct from passes. Functional success requires every required executable check to pass.

Report judgment answers and uncertainty alongside executable results. Keep feature checks, repair checks and regressions separate in brownfield tasks. Freeze the checks before comparing attempts.

Return to [Neon Pairs](README.md) for the full specification.
