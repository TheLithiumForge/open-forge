---
open-forge:
  description: The Neon Pairs spec's requirement ID syntax, its data maps, its measured size and the decisions behind version 2.2
  tags: [Memory, Document, CurrentTruth, Evaluation, Game, Specification, Requirements]
---

# Neon Pairs Spec Notes

The Neon Pairs Spec Notes record how the [product spec](spec/_spec.md) is mapped and measured, and why version 2.2 reads as it does.

Status: revised for spec version 2.2 on 2026-11-01. These notes stay out of the spec agents receive, so the spec reads as a product spec a user wrote.

## Pack data

Requirement IDs use one capital family letter and two digits, for example `R08`. A clause adds a period and a positive decimal number, for example `R08.1`. A requirement keeps its ID when its wording changes. New requirements use new IDs. A retired clause leaves the spec, `requirements.json` and `trace.json`, and its ID is never reused. A requirement whose clauses all retire leaves with them. The decision log names every retired ID.

The companion `requirements.json` uses schema version 1 and spec version 2.2. Its `requirements` array has one object per ID with `id`, `document`, `heading` and `clauses`. Each clause has `id`, `text`, `firstLayer` and `method`. Documents are relative to the game folder, for example `specs/rules.md`. Headings use the document's exact heading text. `firstLayer` is `L2` or `L3`. `method` is `static`, `build`, `browser`, `replay` or `review`. It records a planned method, not completed evidence. Clause text includes any table values or supporting rules needed to apply that clause.

The companion `trace.json` uses schema version 1 and spec version 2.2. Its `entries` array has exactly one object per clause with `clauseId`, `requirementId`, `checks` and `reference`. The `checks` array contains registered check references. Each `reference` entry has `path`, `symbol` and `evidence`. Paths are relative to the game folder. A review clause needs evidence from two reviewers before its reference entry is complete. Empty arrays mean evidence hasn't been added.

## Size

Counts use the ten product documents after frontmatter is stripped. Words are whitespace-separated tokens, including headings, tables and code. Requirement counts use defining IDs, counted once. The player subset counts each supplied clause's text from `requirements.json`, without JSON keys or source links. These scopes exclude layer plans and generated manifests.

| Scope | Words | Requirement IDs | Clauses |
| --- | --- | --- | --- |
| Full pack | 6434 | 96 | 412 |
| Player subset | 1713 | 59 | 185 |

The full pack isn't a reliable 30 to 60 minute implementation task. It adds exact generation, import validation, logical time and a replay command to the complete player game. Layer 3 uses a provisional 180-minute limit. The check batch measures real times before that limit is frozen. If it doesn't fit, give the board-code, snapshot and replay formats in a starting library or move that work to a separate task. Keep every player-facing rule, including accessibility, look and offline delivery.

## Decision log

Version 2 resolved the reported gaps and earlier build differences. Later decisions below replace its screen-reader deliverable, zoom layout and pilot limit:

| Finding | Decision | Reason |
| --- | --- | --- |
| C10 and C13 focus | Resume moves focus to the remembered board tab stop before its button disappears. Other action buttons keep focus. | A removed button can't keep focus. |
| Paused input | C10 blocks card activation while keeping focus movement. | Paused progress must stay unchanged. |
| Current best | R10 shows the current size's best moves or Best: none. | A saved record needs to be readable during play. |
| Repeated announcements | I16 announces each result once, including identical successive results and a combined final match and win. | Renders and focus changes mustn't repeat results. |
| D10 consistency | Validate only D01 and D05 through D09 for their respective formats. Don't reconstruct history. | Input validation needs a finite stated scope. |
| Loaded size | R08 saves the size of a valid board or snapshot load. Failed loads keep the saved size. | Reload should use the size most recently put into play. |
| V12 marker | Shrink the main shape to 25..75. Use a radius-5 marker with a 3-unit background stroke above it. Review all 128 faces at 44px in grayscale. | Diagonal square markers need separation from the square. |
| Stale browser build | T07 builds current source before browser tests and stops on build failure. | An older distribution can't show a current change. |
| Runtime combination | T03 names exact Node, npm, Playwright and Chromium versions and requires a complete test record. | Package pins alone don't show the runtime used. The version 2 application record is still needed. |
| Largest board | V19 checks all 256 cards, both viewport sizes, scroll reach and full focus outlines. | A default-board check misses large-board failures. |
| Screen reader | I17 requires a complete named journey with tool versions recorded. | Accessible names alone don't show usable play. |
| Browser zoom | V20 requires 200% zoom, readable controls and playable small and large boards. | Zoom must keep the whole game usable. |
| A03 gathered constants | Keep A03. Rebuild with states, versions, timing and size limits beside the behavior they control. | A shared rules file separates constants from their behavior. |
| R06 win summary | Keep R06. Rebuild the banner with final moves and time. | The result needs both values. |
| V08 text sizes | Keep V08 and change off-scale text in the rebuild. | The stated type sizes are the product choice. |
| V09 spacing | Keep the spacing scale for layout and state its scope. Change off-scale layout in the rebuild. | Borders, type and SVG geometry already have separate values. |
| V03 and V16 clipping | Change board padding from 4px to 6px and keep the 3px outline with 2px offset. | The outline extends 5px beyond a card. The extra pixel prevents clipping. |
| Requirement syntax | Use R01-style requirement IDs and R01.1-style clause IDs everywhere. | One syntax connects a rule to its evidence. |
| Product wording | Remove process instructions from Controls, Architecture and Acceptance. | The documents describe the requested game. |
| Coverage | Map every ID and every clause before adding evidence. | Missing evidence stays empty instead of being implied. |
| Pack size | Count the full pack and player subset. Keep the full task at a 60-minute pilot limit until measured. | A larger contract needs a measured time budget. |
| Elapsed counter limit | R07 caps elapsed time at 359999999ms while play continues. D07 rejects larger imported values. D09 caps replayed elapsed time while D08 timestamps stay nonnegative safe integers. | The cap is 99 hours, 59 minutes and 59.999 seconds. Every exported elapsed value fits version 1 without an overflow policy. C08.2 and C10.3 need no change. R05.1 is independent of elapsed time. |
| Move counter limit | R05 caps moves at 999999. Play, matching and completion continue at that count. D07 rejects larger imported moves and saved best scores. | The maintainer chose a finite count so a valid loaded game can always continue and export version 1 data. |
| Screen reader evidence | I17.1 through I17.6 use Chromium accessibility-tree captures for automated checks. A person confirms the spoken journey as a review item. Record Chromium and Playwright versions and state that no actual screen reader was run. | Tree names, roles, states and live-region payloads can be checked unattended. Those checks don't establish spoken output. |

## Council revision

The maintainer chose these version 2.1 changes after the Claude Opus and Codex Sol council reports. Finding IDs refer to those reports. Requirement IDs stay fixed and new clauses follow the last existing number:

| Finding | Decision | Changed clauses |
| --- | --- | --- |
| Opus F1, Sol NP-S1 | I17 requires a supported keyboard journey, automated tree evidence and a tool record. Spoken confirmation is user acceptance, never a developer deliverable. | I17.1, I17.3, I17.6 |
| Opus F4, Sol NP-S3 | Publish Moves: N, Pairs: M/T, Time: m:ss with continuing minutes and Best: N or Best: none as product wording. | R05.9, R06.2, R07.12, R10.6, R10.7 |
| Opus F14 | Publish the existing singular match, plural match, miss and combined win announcements. Load statuses use the same single region once per event. | I16.6 through I16.9 |
| Opus F5 | Cards have at least 6px scroll margin, including interior rows. | I13.2 |
| Opus F6, Sol NP-S5 | Remove the unmapped ledger and grading instructions. Keep Acceptance as journeys under existing rules. | No clause added. Acceptance only. |
| Opus F7 | Publish `npm run --silent replay -- PATH`. Valid replay prints only final snapshot JSON to stdout and exits 0. Invalid input prints only an error to stderr and exits nonzero. The command flag suppresses npm's banner for replay while other npm commands keep their errors and warnings. T07 points to C14 for output behavior. | C14.3, C14.4, C14.5, T07.7 |
| Opus F8 | Move the requested cap clauses to Layer 3. Keep ordinary move counting in Layer 2 without an undefined cap reference. Existing R07.10 and R07.11 still require play and display to continue if a clock has a cap, without supplying its numeric value to Layer 2. | R05.1, R05.4, R05.5 through R05.8, R07.9 |
| Opus F9 | Define the equivalent zoom layout as 640x400 CSS pixels with device scale 2. Pinch zoom stays enabled. | V20.1 through V20.4. V20.5 unchanged. |
| Opus F10, Sol NP-S6 | The scale covers positive spacing set by the developer. Allow zero and automatic centering, reset browser margins and exclude native control internals. | V09.1 |
| Opus F3 | Keep the named Node 26.12.1, npm 11.20.0, Playwright 1.64.1 and Chromium 156.0.8078.4 combination. The images follow this choice. | T03.4 unchanged. |
| Sol NP-S4 | Publish the prohibition already used by the map and rubric. Don't add a global store, event bus, router or generic utility folder. | A12.3 |
| Opus F11 | T06's 200-line limit covers authored TypeScript files, as its map already says. | T06.3 unchanged. Document aligned. |
| Opus F12 | Later wins from unfinished imports record a best under R10. The Board code control trims surrounding whitespace. The core parser still requires the exact code. | C12.4, A06.3, D01.4 |
| Opus F13 | Export rounds elapsed milliseconds down and pending mismatch milliseconds up. A pending mismatch exports at least 1ms. | C08.6, C08.7. D06.6 unchanged. |
| Opus F14 | Name Panel and secondary-text targets, the Green matched border and check, the Text hover border and the face parts that take the face color. Matched hover keeps Green. Drop unused opacity transitions. Successful loads select their board size. | V06.2, V06.6, V08.4, V12.8, V15.2, V16.2, V17.1, V17.2, C01.4 |
| Opus F15 | Add palette-at-marker color to Layer 2 and keep the closed-card name example. Describe the generated subset as a checklist. | V10.9, I14.1 |
| Opus F16 | Use no component library. Keep the existing native DOM and canvas choices. | V01.4, T01.4 |
| Opus F17 | The Layer 3 prompt names the Architecture document. | No product clause changed. |
| Opus F18 | Completed snapshots load completed without a pause or Resume. Their load status says Loaded completed snapshot. The banner stays static and bests stay unchanged. | C09.3, C09.5, C10.1, C10.7, D07.6 |
| Opus F2, Sol NP-S7 | Use 180 minutes provisionally and measure the check batch before freezing the limit. No model or container run belongs to this revision packet. | Layer 3 plan only. |
| Sol NP-S2 | Keep unspecified markup and Layer 2 layout choices open. Fix any checker assumptions in the separate checks packet. | No product clause changed. |

The architecture rubric already asks for the absence of those four mechanisms. Its next version and the new executable check mappings belong to the workers following this revision. New trace check slots stay empty until those checks exist. Reference evidence is recorded separately.

## Maintainer trim

Version 2.2 drops rules for corners that reasonable players and agents never reach, so checks grade the behavior that matters. Strict judges kept finding reference defects in such corners, such as Ctrl+End on a card that already has focus. Requirement IDs stay fixed. The new clause follows the last existing number, and retired clauses leave the spec and both maps. The rounding decision replaces Opus F13 above:

| Source | Decision | Changed clauses |
| --- | --- | --- |
| Maintainer trim, 2026-11-01 | Retire Home, End, Ctrl+Home, Ctrl+End and the autorepeat rule. The board handles only ArrowLeft, ArrowRight, ArrowUp and ArrowDown without modifiers, plus Enter and Space. Every other key and modifier combination is left to the browser and doesn't move focus or activate a card. | I06.5 through I06.8 and I02.1 retired, with I02. I06.10 added. |
| Maintainer trim, 2026-11-01 | Export `elapsedMs` and a pending mismatch's `remainingMs` as whole milliseconds. A pending mismatch exports at least 1ms. Drop the floor and ceiling rules. | C08.6, C08.7. D06.6 unchanged. |
| Maintainer trim, 2026-11-01 | Add an Out of scope section to the spec README. The game needn't support keys and modifier combinations beyond the listed ones, holding a key down, several tabs or windows open at once, a pointer press that drifts or is dragged off a card, replay or snapshot files with exotic encodings such as a byte-order mark, or spoken screen-reader output. These are never tested or graded. Each tab may simply overwrite saved preferences. Spoken output stays user acceptance. | No clause added. README only. |
| Maintainer trim, 2026-11-01 | Keep everything else, including the 640x400 equivalent zoom layout, the visible focus outline with its scroll margin, the board scrolling inside its frame, right-click and long press not activating a card and the stated size limits. | V20, V16.1, I13.2, V19, I03 and D10 unchanged. |

The README states the Out of scope list in product words, so it says "never tested" without grading terms. The I06.10 check slots stay empty until the checks packet adds them.

Back to [Neon Pairs](_neon-pairs.md).
