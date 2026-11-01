# Validation Verdicts
Validation Verdicts is the checker review record that explains the first Layer 2 Docker failures against the earlier reference.

For example, a CSS border rotated by 45 degrees draws a diamond even though its markup contains no shape name. The checker now measures that contour.

Status: checker corrections pass unit tests and container fakes. Docker validation against the reviewed reference is pending.

## Earlier failures

These verdicts compare the checker with the reviewed source and its audit. Runtime confirmation uses the commands under Validate:

| Failure | Verdict | Correction and evidence |
| --- | --- | --- |
| R06.6 | Checker bug fixed | Completion motion comes from the V18 journey after flushing style. Reading the animation after several summary queries could miss it. `reference/src/game/status/status.css` declares the 600ms pulse. |
| V09.1 | Checker bug fixed | Resolved auto margins reported the centered main's 80px offset as layout spacing at 1280px. CSS Typed OM keeps the auto keyword. Hidden content and inline artwork are excluded. |
| V14.1 | Checker bug fixed | Drawn CSS border and SVG contours replace shape words in markup. `reference/src/game/board/board.css` draws the neutral rotated square. |
| V07.1 | Checker bug fixed | Palette colors are collected while every face is revealed. Looking after the learning journey missed faces that had closed. |
| V15.2 | Checker bug fixed | The checker solves the learned deal, waits for border transitions and measures check contours. `reference/src/game/board/Card.tsx` draws the SVG check path. |
| V19.4 | Checker bug fixed | Native touch start, move and end events replace a synthetic scroll gesture. The final card must reach the browser viewport through touch, separately from keyboard reach. |
| V18.4 | Checker bug fixed | A wall-clock wait lets Chromium finish CSS motion. Advancing the controlled JavaScript clock alone didn't finish that animation timeline. |
| T07.4 | Checker bug fixed | Passing-count parsing accepts Vitest, Node and Playwright summaries, including colored output. Zero-test summaries fail. |
| I03.1 | Checker bug fixed | Right-click and held native touch exercise an uncancelled contextmenu path without activation. The operating-system menu itself isn't captured. |
| faces-distinct-16 | Checker bug fixed for L2 and L3 | Whole-page board samples included positions clipped by the internal scroll viewport. That could make the learning code mistake two unrelated cards for twins and copy a signature. Individual locator screenshots capture the cards after scrolling. L1-v2 and the duplicate thresholds stay unchanged. |

No reference defect was identified for these failures. This is source and unit evidence, not a new Docker result.

## Face limits

The old signature crops 20% from each edge. The reviewed grayscale capture has different top and right markers that this signature treats as alike. A unit test records that limit. It doesn't establish that colored faces are duplicates, and it doesn't change the L1 color or shape threshold. Grayscale legibility stays in visual review.

## Other corrections

V16.2 compares every computed card property and permits only border-color changes. An added transform, size or shadow fails. A11.1 follows runtime imports and reexports from the production HTML entry, so an unimported fixture doesn't count as shipped. T07.5 shares the runner-independent summary parser. T08.2 records additional local file requests as delivery failures.

All six fixture edits match the reviewed reference exactly once. Their replacement text and declared failed clauses still target the same rules. Their existing anchors needed no change.

The Layer 2 case's `specs/manifest.json` and `inputs.json` refresh only pinned source values. Its product sentences and clause selection stay unchanged. The current requirement pin is `ec07401dcc5fbc5e05e17d204db06a0e67f34624ee4926621fe55ee91646c04c` and its source pin is `66b339a53d509dd235b9843aa5efade12e3bb607df351bf1a096fe2a755bc583`.

## Validate

From `src/evals`, with the verifier image ready, run:

```powershell
node benchmarks/neon-pairs/checks/spec-layers/validate.ts --plan
node benchmarks/neon-pairs/checks/spec-layers/validate.ts --audit
node benchmarks/neon-pairs/checks/spec-layers/validate.ts --run L2
node benchmarks/neon-pairs/checks/spec-layers/validate.ts --run L3
```

V20.1 through V20.4 keep their native browser zoom errors. T03.4 needs the named toolchain. T07.9 stays a process review limit and emits no automatic assertion.

Back to [Later layers](README.md).
