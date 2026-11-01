# Later layers
Later layers is the Neon Pairs checker that grades the supplied Layer 2 clauses and full Layer 3 spec through public controls, replay files, source checks and isolated commands.

Layer 2 learns each deal by revealing cards. Layer 3 also uses documented board codes and snapshot controls, so a known replay can be compared with an independent oracle. Architecture and legibility use cited review.

## Check sets

Each layer keeps the 16 stated L1-v2 checks. Its additional assertions cover the supplied clauses from `requirements.json`. Every probe for a clause must pass before that clause passes. Every assertion in a check must pass before that check passes.

The frozen declarations contain these counts:

| Layer | Checks | Clause assertions | Review clauses | Questions |
| --- | --- | --- | --- | --- |
| L2-v1 | 48 | 169 | 11 | 10 |
| L3-v1 | 100 | 336 | 55 | 12 |

The assertion counts exclude the 16 inherited L1 checks. Each question takes three yes, no or unsure ballots with cited evidence. `layers/L2.json` and `layers/L3.json` list every clause and its method. Their `review` arrays list every review clause with its reason.

## Validate

From `src/evals`, run these read-only preparation checks:

```powershell
node benchmarks/neon-pairs/checks/spec-layers/validate.ts --plan
node benchmarks/neon-pairs/checks/spec-layers/validate.ts --audit
```

The audit checks every fixture search text matches exactly once. If the reference changes, update the fixture against the new source before running validation. A missing or repeated match stops validation.

When the new reference and verifier image are ready, run these commands:

```powershell
node benchmarks/neon-pairs/checks/spec-layers/validate.ts --run L2
node benchmarks/neon-pairs/checks/spec-layers/validate.ts --run L3
```

L2 validates the reference. L3 validates the reference and six broken copies. You can append a reference folder after the layer. The command writes the checks, clause outcomes, expectation differences and grading fingerprint into `fixtures/spec-layers/L2.validation.json` or `fixtures/spec-layers/L3.validation.json`.

Each broken copy declares failed assertions and any passing controls in its `expected` object. Other outcomes are reported. The reference requires all assertions and inherited checks to pass except the listed checker limits.

Candidate installs, builds, tests and replay commands run as the container build user. Chromium and the checker run as a separate user. The checker folder has a root-owned parent, so candidate processes can't replace it. Command results are copied there after each command. Container files reach the host through `copyOutPlain` and bounded report parsers.

## Integration

Case registration calls `runLayer2Checks` or `runLayer3Checks` from `container.ts`. Add `gradingFingerprint(layer)` from `load.ts` to that case's grading inputs. It hashes the frozen layer, rubric and requirement inventory. The packet leaves case registration and the runner to their separate worker.

The output includes the check sets, clause outcomes, review clauses and inherited board images. Review gets no automated credit. Judge callers use the matching frozen rubric and the same source, prompt and image evidence for both providers.

## Limits

Seven Layer 3 clauses need other evidence. Layer 2 includes six of them. Their outcomes stay `error` and their reasons appear in each layer's `uncheckable` array:

- I03.1 - Native long-press context menus need a headed touch browser. The checker exercises right-click behavior.
- V20.1, V20.2, V20.3 and V20.4 - Native 200% browser zoom needs a headed browser. Device scale and CSS zoom don't test that browser setting.
- T07.9 - Leftover authoring processes need a process check before the authoring container shuts down.
- T03.4 - The named Node 26.12.1, npm 11.20.0, Playwright 1.64.1 and Chromium 156.0.8078.4 combination needs its own toolchain. The shared verifier image uses Node 24.

The validation command reports these limits apart. Its preparation audit has run. Browser and mutation validation waits for the new reference and the overseer's container run.
