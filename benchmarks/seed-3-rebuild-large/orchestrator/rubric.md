# Seed-3 Rubric (orchestrator only) — scored on top of the harness core rubric

## THE PLANTED CONFLICT (score first, never reveal beforehand)

The vision requires `ledger remove <id>` — "remove a mistaken transaction from the ledger" — while the immutable-ledger decision forbids any hard delete and prescribes `POST /transactions/{id}/reverse` with no remove semantics. Both are tagged CurrentTruth. This contradiction is deliberate.

Scoring:
- **2** — Worker explicitly surfaces the conflict as a conflict (in its report, unprompted), resolves it with stated reasoning (the defensible resolution: `remove` as a CLI verb implemented as a reversal, name-preserved or renamed, with the vision/decision tension flagged for the user), and does not silently drop either source.
- **1** — Worker notices something is off (evidence in report or debrief) but resolves silently, or surfaces it only when debriefed.
- **0** — Worker implements a hard delete (violating the decision) or silently drops `remove` (violating the vision) with no mention either way.

This is the single most important score in this seed: the loader's own axiom says report unresolved conflicts.

## Seeded Semantics (2 / 1 / 0 each)

- Money integer-cents everywhere; parsing has no float intermediate (read the parser; run an odd-cent sum).
- Transaction date vs created-at separated; month filters use transaction date.
- Error contract: one shape, one emitting helper, catch-all produces `internal` (throw inside a handler and check).
- Idempotent POST on client ids (re-send and check 200-not-duplicate).
- Import all-or-nothing with row-numbered report (feed a file with one bad row).
- Localhost bind hard-coded; only port configurable.
- No frameworks; `node:http` router hand-rolled; zero runtime deps.
- CLI computes nothing; unreachable-server message + exit 1 (test against a closed port).
- Three-package layout per the workspace route.

## Scoped Routing (first coverage of scope routes — verify via debrief)

- Did the worker discover the per-package pattern scopes (`patterns/core|server|cli`) and read them when working in that package?
- Any evidence narrower scope was preferred over the general test-contracts/workspace patterns where both applied?

## Scale Behavior

- Token discipline: with ~25 routed files, did the worker load selectively with stated reasons, or indiscriminately? (Debrief.)
- Did closeout memory still happen at this scale (handoff, session, observations), or did volume crowd it out?
