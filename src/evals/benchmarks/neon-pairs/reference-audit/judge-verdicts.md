# Judge Verdicts

The Judge Verdicts record how an independent Claude Opus judge scored the Neon Pairs reference against spec v2 before it became the Layer 4 seed.

Status: 10 out of 10 on 2026-11-01, after four rounds. The spoken screen reader journey, I17.6, still needs a person, following [screen-reader-journey.md](screen-reader-journey.md).

The judge ran the reference itself in every round: the type check, the unit tests, the build, the browser tests three times, the replay command on valid and invalid files, and hands-on checks of the built page in Playwright. It changed no files. Three Sol 6.1 auditors also checked every clause by reading after the first build.

| Round | Overall | Rules | Controls | Reproducibility | Look and accessibility | Delivery | Code | Tests |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 8 | 9 | 9 | 9.5 | 7.5 | 7 | 7.5 | 7.5 |
| 2 | 9 | 10 | 9 | 10 | 9 | 10 | 8.5 | 9 |
| 3 | 9.5 | 10 | 10 | 10 | 9 | 10 | 10 | 9.5 |
| 4 | 10 | 10 | 10 | 10 | 10 | 10 | 10 | 10 |

## What each round fixed

- **Round 1** found a flaky zoom test, an unused Vitest pin, polish gaps in type size, the matched check mark and the best label, two clocks where the spec implies one, misplaced constants and labels, tests that couldn't fail, and audit material inside the seed. The builder fixed all of it and moved the audit material here. [review-round-1.md](review-round-1.md) lists each change.
- **Round 2** confirmed those fixes and passed V12.7: all 128 faces are distinguishable in grayscale at 44px. It found Enter in the Board code box losing focus and the turn-result live region in the wrong component. [review-round-2.md](review-round-2.md) lists each change.
- **Round 3** confirmed those and found the load notices shown but no longer announced. [review-round-3.md](review-round-3.md) lists the change.
- **Round 4** confirmed each load notice is announced exactly once through the single live region, with nothing else announced and nothing regressed, and found no remaining defect.

Back to the [audit ledger](AUDIT.md).
