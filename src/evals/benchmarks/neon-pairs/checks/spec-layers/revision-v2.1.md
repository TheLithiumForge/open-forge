# Council checks
Council checks are the Layer 2 and Layer 3 changes that grade the revised Neon Pairs spec through public controls, source checks, replay output and cited review.

For example, loading a completed snapshot now passes only when it stays completed, keeps its clock fixed and shows no Resume button. A later win from unfinished imported progress must save its best across reload.

Status: checker changes implemented. Product qualification needs the protected player pack refreshed. Container validation is pending.

## Clause map

Both layer and rubric versions are `L2-v4` and `L3-v4`. The 416 clauses have check or review slots in the trace. This table maps the council changes, including the checks kept from part 1:

| Clauses | Check | Probe |
| --- | --- | --- |
| I17.1 through I17.5 | `l2-screen-reader` | `keyboardTree` discovers and completes 4x4 through keyboard controls and saves tree names, states and turn payloads. |
| I17.6 | `l2-screen-reader` | `staticChecks` reads the README through `toolRecord`. It requires browser and tool versions and a screen-reader-use record. |
| R05.1, R05.4, R05.9 | `l2-moves` | `turns` checks normal move counting and `Moves: N` before, during and after play. |
| R06.2 | `l2-completion` | `turns` checks `Pairs: M/T` throughout play. |
| R07.12 | `l2-clock` | `publishedClock` checks floor rounding, padded seconds and minutes past 59. |
| R10.6, R10.7 | `l2-best` | `best` checks `Best: N` and `Best: none` for the current size during play, completion, restart and reload. |
| I16.6, I16.7, I16.8 | `l2-announcements` | `announcements` checks singular matches, plural matches, misses and the combined win text. |
| I16.9 | `l3-i16-browser` | `loadAnnouncements` checks successful and rejected snapshot loads and rejected board loads, including repeated statuses. |
| I13.2 | `l2-scroll` | `scroll` checks every card's four scroll margins and interior outline bounds. |
| C14.3, C14.4, C14.5 | `l3-c14-replay` | The published silent npm command reads each replay. `compareReplay` checks snapshot-only success and stderr-only nonzero rejection. |
| T07.7 | `l3-t07-replay` | The same CLI vectors check the published command and replay result. |
| R05.5 through R05.8 | `l3-r05-browser` | `snapshotCaps` checks capped moves, continued play, matching and completion. Layer 2 omits these clauses. |
| R07.9 | `l3-r07-browser` | `snapshotCaps` checks the exact elapsed limit. Layer 2 omits it. |
| R07.10, R07.11 | `l2-clock` | `visibleTimeCap` permits an uncapped Layer 2 clock. Layer 3 also checks exact capped state. |
| C09.3, C09.5 | `l3-c09-browser` | `importedProgress` pauses unfinished progress and loads completed progress without Resume, with its exact status. |
| C10.1, C10.7 | `l3-c10-browser` | `importedProgress` checks Resume presence and completed state after controlled time. |
| D07.6 | `l3-d07-browser` | Both unfinished pause and completed import rules must pass. |
| C12.4, A06.3 | `l3-c12-browser`, `l3-a06-browser` | A win from unfinished imported progress saves eight moves and keeps that best after reload. |
| C08.6, C08.7 | `l3-c08-replay` | `snapshotRounding` exports through the UI at fractional monotonic times. It floors elapsed time and ceils mismatch time, including 0.25ms to 1ms. |
| D01.4 | `l3-d01-replay` | `boardControls` trims whitespace and replaces nonzero progress. Replay vectors keep whitespace invalid in the core parser. |
| C01.4 | `l3-c01-browser` | `loadedFocusAndSize` checks the selected size immediately after both load types. |
| V10.9 | `l2-faces` | `faces` compares rendered colors and named markers in both layers. |
| V06.2, V06.6 | `l2-colors` | `namedTargets` checks Panel controls and Secondary help, status labels and supporting notes. |
| V08.4 | `l2-typography` | The same secondary targets need 14px text. |
| V12.8 | `l3-v12-browser` | `svgGeometry` checks solid fills, outlined strokes, marker fills, empty outline fills and Face-background marker strokes. |
| V15.2 | `l2-faces` | `faces` keeps the Green border and drawn check assertions. |
| V16.2 | `l2-outline` | `appearance` checks the Text hover border and other computed changes. `faces` also checks matched hover stays Green. |
| V17.1, V17.2 | `l2-motion` | `appearance` checks border-color transitions and reduced motion. Opacity transitions fail. |
| V01.4 | `l3-v01-browser` | `svgGeometry` checks canvas and component-library markers. |
| T01.4 | `l3-t01-static` | `staticChecks` checks component-library dependencies and shipped source. |
| A12.3 | `tests-and-change` in the L3 rubric | The question treats the global store, event bus, router and generic utility folder prohibition as a rule. |
| I14.1 | `l2-names` | `accessibleNames` keeps the closed row and column names. |
| V09.1 | `l2-typography` | Positive spacing, reset margins and native control exceptions keep the part 1 tests. |
| V20.1 through V20.4 | `l2-zoom` | The part 1 equivalent viewport and device-scale journey stays. |
| T03.4 | `l3-t03-build` | The part 1 verifier-version probe stays. |
| T06.3 | `l3-t06-static` | The 200-line limit still covers authored TypeScript files. |

C08.6 and C08.7 keep their planned replay method in the inventory. Browser exports provide their fractional-time assertions because replay timestamps are integers. The clock adapter is installed before application code, so cached calls to `performance.now` see the same controlled fractions.

Removed acceptance bookkeeping and the changed Layer 3 prompt add no product clause. The case metadata now names the Architecture document and uses the provisional 180-minute limit.

## Verification

The last complete product run has 252 passing tests and five failures. Typecheck passes before the final parser extension. Positive and negative synthetic browser tests cover tree play, text formats, announcements, loaded state, saved bests, fractional clocks, trimming, target colors and hover behavior. Unit tests cover replay output, component libraries and trace coverage. Source collection skips installed caches. Every existing mutation still matches the revised reference exactly once. The fixture manifest now names spec version 2.1.

The final README-parser extension permits other named browsers, test tools and screen readers. Its nine positive and negative parsing cases pass in the tool runtime. A repository test and typecheck rerun can't start because Windows returns `CreateProcessWithLogonW failed: 1909`. This final extension still needs those commands on the repository files.

The full product suite has five failures outside the checker. The player pack source fingerprint differs from the current full pack and requirement input. Its document bodies match. The strict comparison stays in place. The failing tests are:

- `experiments/document-plans.test.ts` - The saved document plan matrix.
- `materialize/delivery.test.ts` - Three tests that load or install the document cases.
- `materialize/subset.test.ts` - Exact regeneration of the player pack.

The packet protects packs. Their maintainer needs to regenerate the player pack, then refresh its case input pin. No Layer 4 case test currently fails.

The saved player source hash is `7c03b21e8648bdec0e181dc7f33c210a7096ffaa91d1c4597302588595837862`. Regeneration from the current full pack and requirements gives `6d9e56d54c2d9fce5d38804fb1bf8b66e73ddd05c3465cc05c9a5cbb5ffe01b1`.

## Checker files

New files under `checks/spec-layers/` are:

- `announcements.ts`, `published-text.ts`, `browser-tree.ts`, `browser-rounding.ts` and `browser-targets.ts`.
- `browser-game.fixture.ts`, `reproduction.fixture.ts`, `reproduction.test.ts` and `revision.test.ts`.
- `revision-v2.1.md`.

Changed files under that folder are:

- `browser-access.ts`, `browser-geometry.ts`, `browser-input.ts`, `browser-play.ts`, `browser-session.ts`, `browser-storage.ts`, `browser-view.ts` and `browser.ts`.
- `browser-caps.ts`, `browser-formats.ts`, `browser-import.ts`, `browser-layout.ts` and `browser-layout.test.ts`.
- `cap-policy.ts`, `catalog.ts`, `catalog.test.ts`, `commands.ts`, `commands.test.ts`, `fixtures.ts` and `freeze.test.ts`.
- `numeric.ts`, `requirements.ts`, `review-policy.ts`, `rubrics.ts`, `source.ts`, `source.test.ts` and `vectors.ts`.
- `README.md` and `validation-verdicts.md`.

Generated changes cover `layers/L2.json`, `layers/L3.json`, `rubrics/L2.json`, `rubrics/L3.json` and only check slots in `trace.json`. The fixture version changes in `fixtures/spec-layers/manifest.json`. These paths are relative to the Neon Pairs benchmark. No source file is deleted.

## Integration files

Minimal edits outside the checker and frozen declarations bind the document cases to their supplied manifests and apply the accepted Layer 3 prompt and limit. The changed files are:

- `cases/l2-defined-idea/inputs.json` - Existing player manifest pin.
- `cases/l3-full-spec/inputs.json` - Current full-pack pin.
- `cases/l3-full-spec/prompt.md` - Architecture document wording.
- `cases/l3-full-spec/suite.json` - 180-minute limit.
- `src/evals/experiments/neon-pairs-l3-check.json` and `neon-pairs-l3-full.json` - Matching limits.
- `src/evals/experiments/document-plans.test.ts` - Compare limits with each case.
- `src/evals/materialize/delivery.test.ts` - Expect the accepted Layer 3 limit.

Case paths above are relative to the Neon Pairs benchmark. The spec, clause text, packs, reference, runner and Layer 4 folders stay unchanged.

## Container checks

From `src/evals`, the overseer runs:

```powershell
node benchmarks/neon-pairs/checks/spec-layers/validate.ts --plan
node benchmarks/neon-pairs/checks/spec-layers/validate.ts --audit
node benchmarks/neon-pairs/checks/spec-layers/validate.ts --run L2
node benchmarks/neon-pairs/checks/spec-layers/validate.ts --run L3
```

No container or spoken-output validation ran in this packet. Back to [Later layers](README.md).
