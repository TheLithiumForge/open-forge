# Phone usage notes

These are field-style research notes about playing Neon Pairs through a narrow viewport, with particular attention to touch scrolling, interruption, and finding controls around a large board.

The browser suite checks geometry and touch scrolling; physical-device long-press confirmation remains a user acceptance activity.

## A phone changed what was visible at once

The default four-column board could be understood as one small surface. A sixteen-column board was a larger surface seen through a window. In the observations, players switched between remembering a face and remembering which part of that larger surface they had scrolled to.

That distinction made ordinary page context important. A player still needed moves, pair progress, time, and the ability to start again. If the entire document scrolled horizontally with the board, the controls could move out of view just as the player was trying to recover orientation. Current layout confines necessary board overflow to `.board-viewport` and keeps the outer page constrained to its available width.

The study did not recommend making every size fit by reducing card dimensions indefinitely. The current minimum is 44px, with cards growing as large as 88px when space permits. At sixteen columns, tracks and gaps alone need 794px at that minimum. The narrow view therefore needs internal horizontal scrolling for a reason grounded in geometry, not a failure to choose another breakpoint.

## Touch travel had to remain distinct from a tap

In the sessions, people often began scrolling with a finger over a card because nearly the entire board is made of cards. A scroll gesture should move the view without revealing whatever was under the initial contact. The same surface therefore supports two intentions that cannot be decided solely at touch start.

The current card uses a native button's completed click as the activation event. It does not reveal on pointer-down. Its CSS uses `touch-action: manipulation` and disables selection of the card's contents. This lets the browser participate in distinguishing a completed tap from scrolling rather than having the game invent a separate touch gesture recognizer.

The browser touch-scroll journey sends touch events through Chromium's protocol, moves diagonally across the board, waits for scrolling to settle, and verifies that no cards have been revealed. It checks that the last card can become visible. This is concrete browser evidence for scrolling behavior, not a claim that every physical browser produces identical event sequences.

The [pointer and touch guide](pointer-and-touch.md) describes the current input contract. The [touch-scroll decision](adr-047-touch-scroll.md) explains why native behavior was preferable to a custom drag layer.

## Long press remained a real-device question

The specification keeps the normal context menu for right-click and long press, without using it to activate a card. It also explicitly leaves confirmation of a real touch long press to user acceptance because automated browsers cannot produce that gesture faithfully.

The field notes include a participant pausing with a finger on a card while deciding what to do next. The pause highlighted the distinction between deciding on a card and activating it.

This boundary is useful when reviewing the browser suite. A synthesized sequence can exercise movement and cancellation. It cannot automatically stand in for the platform's complete long-press recognition, context-menu presentation, and subsequent focus behavior. The [browser support page](browser-support.md) and [acceptance journeys](acceptance.md) explain the scope of recorded checks.

## Focus mattered even on a touch-oriented device

The participants did not all use one input method. One player tapped to reveal and then used a connected keyboard. Another used keyboard movement to reach a far position after scrolling by touch. This made “mobile means pointer-only” an unhelpful assumption.

Current focus styling becomes visible for keyboard use, including after a pointer click followed by arrow navigation. The board remembers the focused card independently of game selection. Nearest scrolling brings a keyboard destination into view with enough room for its outline, rather than merely ensuring the card's center is visible.

Pointer timing adds a separate issue. Focusing a partly visible card on press must not move the target away before release. Current `useBoardFocus` scrolls immediately for `:focus-visible`, while the completed click callback can scroll the pointer-selected card afterward. The [layout incident](incident-16x16-layout.md) explains the regression that motivated this distinction.

## Interruptions were ordinary use

In the field observations, a player switched away during an incorrect pair, returned, and expected time to continue from where observation had stopped. They had not watched the faces while another app or page occupied the screen. Counting that period would make the miss interval a wall-clock deadline rather than an opportunity to remember.

The current session listens to document visibility and excludes hidden time from active elapsed and mismatch advancement. It also stops elapsed time after completion. A reload begins a fresh deal. That difference is important on phones, where hiding a page and having a browser discard or reload it may feel similar from the user's perspective.

Explicit snapshot export can transport progress, but it is a manual action. The [elapsed-time rules](elapsed-time-rules.md) and [persistence overview](persistence-overview.md) describe the present boundaries.

## Controls around the board needed room

The notes observed that players used the size selector and restart controls between short bursts of board interaction. A board taking every available pixel could make those controls harder to find, even if it showed more cards. Current board height is bounded by `70dvh` and 768px, leaving the rest of the page in normal flow.

Status uses wrapping flex layout, so narrow space can move groups onto another line rather than overlap values. Reproduction controls live in a native details element that begins collapsed. This keeps the ordinary first view focused on play while leaving board codes and snapshots available for deliberate use.

The notes did not propose hiding counters on small screens. The need to know progress does not disappear when the viewport narrows. Nor did they suggest disabling browser zoom to lock the layout. The current app keeps zoom enabled and tests a specific equivalent zoom configuration.

## What the zoom check says

The game's equivalent zoom journey uses a 640x400 CSS-pixel viewport with device scale 2, corresponding to the layout pressure of 200% zoom from 1280x800. It checks readable controls, status, a completed summary, snapshot states, and keyboard travel through a large board.

This is useful because increased scale reduces the CSS space available to the layout. It exercises wrapping and internal scrolling without treating small text as the solution. It is still a named test configuration, not a check of physical pinch gestures on every supported device.

The [zoom behaviour page](zoom-behaviour.md) explains the present contract. The research contribution was to connect zoom to ordinary readability and reachability, instead of treating it as a separate visual polish exercise after the game was “finished.”

## What we learned, with limits

The phone study supported three concrete interpretations of usability: a scroll must not spend a move, the board's overflow must not drag the whole document sideways, and returning from an interruption must have a coherent time meaning. Each maps to an existing implementation boundary rather than a new setting.

The study suggests why first-to-last reachability and complete focus outlines are better evidence than a single responsive screenshot. The [autumn playtest](research-playtest-autumn.md) covers the broader returning-player tasks, while [roadmap notes](roadmap-notes.md) retain questions about future exploration without promising a mobile-specific mode.
