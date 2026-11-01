---
open-forge:
  description: The Neon Pairs spec's requirement ID syntax, its data maps, its measured size and the decisions behind version 2
  tags: [Memory, Document, CurrentTruth, Evaluation, Game, Specification, Requirements]
---

# Neon Pairs Spec Notes

The Neon Pairs Spec Notes record how the [product spec](spec/_spec.md) is mapped and measured, and why version 2 reads as it does.

Status: written with spec version 2 on 2026-11-01. These notes stay out of the spec agents receive, so the spec reads as a product spec a user wrote.

## Pack data

Requirement IDs use one capital family letter and two digits, for example `R08`. A clause adds a period and a positive decimal number, for example `R08.1`. A requirement keeps its ID when its wording changes. New requirements use new IDs.

The companion `requirements.json` uses schema version 1 and spec version 2. Its `requirements` array has one object per ID with `id`, `document`, `heading` and `clauses`. Each clause has `id`, `text`, `firstLayer` and `method`. Documents are relative to the game folder, for example `specs/rules.md`. Headings use the document's exact heading text. `firstLayer` is `L2` or `L3`. `method` is `static`, `build`, `browser`, `replay` or `review`. It records a planned method, not completed evidence. Clause text includes any table values or supporting rules needed to apply that clause.

The companion `trace.json` uses schema version 1 and spec version 2. Its `entries` array has exactly one object per clause with `clauseId`, `requirementId`, `checks` and `reference`. Both arrays start empty. A future check entry has `id`, `method` and `evidence`. A future reference entry has `path`, `symbol` and `evidence`. Paths are relative to the game folder. A review clause needs evidence from two reviewers before its reference entry is complete. Empty arrays mean evidence hasn't been added.

## Size

Counts use the ten product documents after frontmatter is stripped. Words are whitespace-separated tokens, including headings, tables and code. Requirement counts use defining IDs, counted once. The player subset counts each supplied clause's text from `requirements.json`, without JSON keys or source links. These scopes exclude layer plans and generated manifests.

| Scope | Words | Requirement IDs | Clauses |
| --- | --- | --- | --- |
| Full pack | 6929 | 97 | 391 |
| Player subset | 1426 | 60 | 180 |

The full pack isn't a reliable 30 to 60 minute implementation task. It adds exact generation, import validation, logical time and a replay command to the complete player game. Budget a 60-minute pilot and report unfinished clauses. A timed pilot hasn't measured completion yet. If it doesn't fit, give the board-code, snapshot and replay formats in a starting library or move that work to a separate task. Keep every player-facing rule, including accessibility, look and offline delivery.

## Decision log

Version 2 resolves the reported gaps and earlier build differences:

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

Back to [Neon Pairs](_neon-pairs.md).
