# Later layers
The spec-layer checks are the Neon Pairs checker that grades the supplied Layer 2 clauses and full Layer 3 spec through public controls, replay files, source checks and isolated commands.

Layer 2 learns each deal by revealing cards. Layer 3 also uses documented board codes and snapshot controls, so a known replay can be compared with an independent oracle. Architecture and legibility use cited review.

## Check sets

Each layer keeps the 16 stated L1-v2 checks. Its additional assertions cover the supplied clauses from `requirements.json`. Every probe for a clause must pass before that clause passes. Every assertion in a check must pass before that check passes.

The frozen declarations contain these counts:

| Layer | Checks | Clause assertions | Review clauses | Questions |
| --- | --- | --- | --- | --- |
| L2-v3 | 48 | 170 | 17 | 11 |
| L3-v3 | 100 | 345 | 56 | 12 |

The assertion counts exclude the 16 inherited L1 checks. Each question takes three yes, no or unsure ballots with cited evidence. `layers/L2.json` and `layers/L3.json` list every clause and its method. Their `review` arrays list every review clause with its reason.

Layer 2 reaches the displayed elapsed cap with a controlled clock and continues matching, closing a miss and winning. R07.9 stays in source review because a seconds-only display can't show the exact stored millisecond limit. R05.5 through R05.8 stay in source review because nearly two million card activations would be needed without snapshot import. Those five clauses keep their planned browser method in the requirement inventory.

Layer 3 imports progress just below both caps, reaches the exact limits and continues through a miss, matches, victory and snapshot export. It also rejects above-cap imports and saved best scores. Replay timestamps keep their safe-integer range while exported elapsed time stays capped.

I17 keeps its spoken-output review under the spec notes. Its planned browser method covers tree evidence, which doesn't establish spoken output or a screen reader version. Existing names, focus and announcement checks support that review.

T07.9 uses cited process review. It gives no automatic credit because a later verifier can't reconstruct processes left during authoring.

To regenerate these definitions and their checks slots, run `node benchmarks/neon-pairs/checks/spec-layers/freeze.ts` from `src/evals`. Add `--caps` to update only the ten cap trace slots.

## Validate

From `src/evals`, run these read-only preparation checks:

```powershell
node benchmarks/neon-pairs/checks/spec-layers/validate.ts --plan
node benchmarks/neon-pairs/checks/spec-layers/validate.ts --audit
```

The audit checks every fixture search text matches exactly once. If the reference changes, update the fixture against the new source before running validation. A missing or repeated match stops validation.

With the rebuilt reference and verifier image ready, run these commands:

```powershell
node benchmarks/neon-pairs/checks/spec-layers/validate.ts --run L2
node benchmarks/neon-pairs/checks/spec-layers/validate.ts --run L3
```

L2 validates the reference. L3 validates the reference and six broken copies. You can append a reference folder after the layer. The command writes the checks, clause outcomes, expectation differences and grading fingerprint into `fixtures/spec-layers/L2.validation.json` or `fixtures/spec-layers/L3.validation.json`.

Each broken copy declares failed assertions and any passing controls in its `expected` object. Other outcomes are reported. The reference requires all assertions and inherited checks to pass except the listed checker limits.

Candidate installs, builds, tests and replay commands run as the container build user. Chromium and the checker run as a separate user. The checker folder has a root-owned parent, so candidate processes can't replace it, and the replay inputs and the stale-build fault file sit inside it. Each command's exit code, output and errors come straight from docker exec, never from a file the candidate could write, and every process the build user started is stopped before the next command runs. Container files reach the host through `copyOutPlain` and bounded report parsers.

## Integration

Case registration calls `runLayer2Checks` or `runLayer3Checks` from `container.ts`. Add `gradingFingerprint(layer)` from `load.ts` to that case's grading inputs. It hashes the frozen layer, rubric and requirement inventory. The packet leaves case registration and the runner to their separate worker.

The output includes the check sets, clause outcomes, review clauses and inherited board images. Review gets no automated credit. Judge callers use the matching frozen rubric and the same source, prompt and image evidence for both providers.

L2 and L3 capture every revealed face through its card locator, which scrolls it into view before taking the screenshot. This corrects the inherited whole-page sampling of clipped large boards. The duplicate thresholds and L1-v2 files stay unchanged. The [validation verdicts](validation-verdicts.md) record the earlier failures and their checker corrections.

## Limits

Five Layer 3 assertions need other evidence. Layer 2 includes four of them. Their outcomes stay `error` and their reasons appear in each layer's `uncheckable` array. T07.9 also appears there as a review limit:

- V20.1, V20.2, V20.3 and V20.4 - Native 200% browser zoom needs a headed browser. Device scale and CSS zoom don't test that browser setting.
- T07.9 - Leftover authoring processes need a process check before the authoring container shuts down.
- T03.4 - The named Node 26.12.1, npm 11.20.0, Playwright 1.64.1 and Chromium 156.0.8078.4 combination needs its own toolchain. The shared verifier image uses Node 24.

I03.1 checks an uncancelled contextmenu path after right-click and a held native touch. It doesn't capture an operating-system menu.

The validation command reports these limits apart. Its preparation audit has run. Browser and mutation validation against the reviewed reference waits for the overseer's container run.
