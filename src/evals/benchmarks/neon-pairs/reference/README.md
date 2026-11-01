# Neon Pairs

Neon Pairs is a memory game that runs offline from one HTML file and keeps local best moves for each board size.

You can repeat a deal with its board code, save progress as a snapshot or run timed actions through the replay command.

Status: implements product spec version 2.

## Install

Use the named test combination, Node 26.12.1, npm 11.20.0 and Playwright 1.64.1 with Chromium 156.0.8078.4. From this folder, install the locked tools:

```powershell
npm ci --cache .npm-cache
$env:PLAYWRIGHT_BROWSERS_PATH = Join-Path $PWD '.playwright-browsers'
$env:TEMP = Join-Path $PWD '.browser-work'
$env:TMP = $env:TEMP
New-Item -ItemType Directory -Force -Path $env:TEMP | Out-Null
npm exec --cache .npm-cache -- playwright install chromium
```

These settings keep the browser download and temporary files inside the project. Browser tests use fresh contexts. They stop their browser and local HTTP server when they finish.

## Play

Run `npm run build`, then open `dist/index.html` from disk. The game makes no network requests. To keep saved sizes and scores on a stable HTTP origin, run `npm run dev`. Stop it when you finish.

Use Grid size to choose a board. Reveal two cards with a click, tap, Enter or Space. Arrow keys, Home and End move board focus. Ctrl+Home and Ctrl+End reach the board ends. Tab leaves the board. New Game gives another deal. Restart repeats the current deal.

Open Reproduce a game to load a board code or export progress. Loading a snapshot pauses it. Resume returns focus to the remembered card and continues the remaining mismatch interval. Elapsed time stops at 359999999ms while play continues. The minutes and seconds display reaches 5999:59. Moves stop at 999999. Matching and victory continue at that count. Saved bests use the same move limit.

## Replay

Run `npm run replay -- PATH` with a version 1 replay JSON file. The command prints its final version 1 snapshot. Invalid files produce a nonzero exit and no snapshot. Replay timestamps can reach the safe integer limit. Elapsed time uses the same cap as browser play.

## Check

Run the checks from this folder:

```powershell
npm run typecheck
npm test
npm run test:e2e
npm run format:check
```

`npm test` runs the unit and static checks with Vitest 5.0.5. `test:e2e` type checks and builds current source before opening the new HTML. It also builds the separate diagnostic face sheet. The game distribution contains no diagnostic entry.

The locked install, type check, Vitest unit tests, build from current source and Chromium journeys have passed on the named combination. The browser command asserts all four runtime versions. Other versions have no test record here.

## Accessibility

Keyboard, card names, atomic turn feedback, reduced motion, touch scrolling and actual 200% browser zoom have browser checks. The diagnostic face sheet draws all 128 faces at 44px.

Playwright checks Chromium's accessibility tree through a complete keyboard journey. It checks controls, card names, states, turn results, the final summary, Tab exit and New Game. A person still needs to confirm spoken output with a screen reader and record its version here.

Tree record: Chromium 156.0.8078.4 and Playwright 1.64.1. Screen reader: not run. Spoken journey: person confirmation pending.

Back to [Specification](../specs/README.md).
