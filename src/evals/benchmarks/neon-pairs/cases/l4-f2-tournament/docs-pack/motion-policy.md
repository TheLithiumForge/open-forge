# Motion policy
This page describes the motion currently used by Neon Pairs, its reduced-motion behavior, and the source and tests behind it.

The game has two authored motion effects: a card border transition and a completion pulse. Neither moves a card, changes its dimensions, or delays a game-state update. The relevant declarations live with their components in `src/game/board/card.css` and `src/game/status/status.css`. The broader visual specification is [Visuals](visuals.md); turn timing belongs in [Turn resolution](turn-resolution.md).

## Card borders

The `.card` rule declares `transition: border-color 120ms ease`. On hover, `.card:hover` changes the border to `var(--text)`. A matched card instead uses `var(--green)`. The transition is limited to the border color: background, outline, size, opacity and SVG geometry have no transition in this rule.

This distinction matters when reading a recording. A reveal replaces the back with the face and changes the card background immediately. The border can still be between colors if the pointer has just entered or left the card. A screenshot taken during that interval does not show the final border value. In `tests/art.e2e.test.ts`, the comparison helper waits for the card's animations to finish before collecting computed styles for this reason.

There is no flip animation. `Card.tsx` conditionally renders either the neutral back span or the `Face` component. The face is absent while the card is closed. Adding a visual flip would therefore be more than a cosmetic adjustment to an existing transform: it would introduce an effect where the current renderer has none. The present markup and identity boundary are described in [Card artwork](card-artwork.md).

Hover also has no gameplay effect. `tests/style.e2e.test.ts` records all computed properties before and after hover, checks that only border-color properties differ, and compares exported game progress. That test is useful when an apparently small visual edit adds a shadow, scale transform or other hover change. Such a difference would depart from the existing V16 behavior even if the card still looked recognizable.

## Reduced motion

Both component stylesheets contain a `prefers-reduced-motion: reduce` media query. The card query sets `transition: none`; the status query sets `animation: none` on `.celebrate`. Under prefers-reduced-motion: reduce, new elements run no animation and no transition. This keeps the static visual states available immediately when the preference is active. The existing cards still change their border and artwork when their state changes; the change simply has no interpolated border effect.

The current implementation uses CSS media queries rather than a stored game preference or a React branch. `Card.tsx` does not need to know the preference, and the session does not select an alternative board. The same matched mark, face and victory text appear in both modes. The media query affects presentation rather than game state.

A useful review includes setting the preference before the page loads and completing a board in that mode. `tests/motion.e2e.test.ts` does this through browser media emulation, then checks the banner's computed animation name and a card's transition duration. Those assertions check the existing effects directly. They do not infer reduced-motion support from a screenshot that happens to have been taken after an animation finished.

Changing the preference while the page is open is also a useful manual observation because the implementation is a live CSS media query. There is no application restart step in the code. This is an observation of how the declarations are applied, rather than a separate game lifecycle. It should not be confused with restarting a deal.

## The completion pulse

`GameStatus.tsx` renders the completion paragraph when `isWon(game)` is true. Its class is `victory`, with `celebrate` appended only when the `celebration` prop is true. In `status.css`, `.celebrate` declares `animation: victory-pulse 600ms ease-out 1`.

The keyframes specify a midpoint only. At 50%, the paragraph background is `var(--panel)` and opacity is `0.75`; the start and end come from its underlying styles. There is no `animation-fill-mode` that preserves the midpoint appearance. After the one iteration, the banner remains present in its normal static styling. Its border, padding and text are not removed when the animation finishes.

This is a pulse of the paragraph as a whole, so the opacity also affects its text and border during the effect. It is not a highlight painted behind otherwise opaque text. [Contrast audit](contrast-audit.md) separates opaque token ratios from a composited estimate for this midpoint. That distinction is helpful when evaluating a captured frame halfway through completion.

The effect neither loops nor emits particles. V18 describes one pulse, a static reduced-motion result, and the absence of flashing and confetti. There are no pulse keyframes on individual cards. A finished board continues to show its matched artwork at full card opacity, as specified in V15.

## Where the celebration flag comes from

The boolean is session data, not a CSS guess based on how many green borders appear. `initialSession` initializes `celebration` to false. `activateSession` sets it from the result of `isWon(game)` after an effective activation that resolves a turn. `replaceGame` clears it when the session changes deals or loads a snapshot.

A completed snapshot can consequently display the completion paragraph with no `celebrate` class. The board is complete, but that load is not a fresh completion event. An unfinished import can later finish through play and receive the normal completion effect. These cases are particularly useful when checking whether an edit has accidentally tied animation to every render of a completed state.

The flag can remain true after the CSS animation has finished. That does not make the effect repeat on every timer update: an existing element retaining the same animation class is different from mounting a new element or changing its animation name. The final game also stops advancing its active elapsed time. The relevant session behavior is covered more fully in [Session module](session-module.md).

## Scrolling is separate

The board's scrolling code uses nearest alignment. `Board.tsx` calls `scrollIntoView` after a click, and the focus hook uses the same alignment for keyboard focus. Neither call specifies smooth behavior. The source CSS has no `scroll-behavior: smooth` declaration. Ordinary touch inertia belongs to the browser and is not the authored completion pulse or border transition.

A card can move on screen because its scroll container moves. That movement does not mean the card has a transform animation. This distinction helps reproduce reports about cards near the edge of a large board: the important evidence is the scroll position, focus state and press timing. See [Large-board rendering](large-board-rendering.md) and [Focus policy](focus-policy.md).

The pointer path also deliberately postpones its own scroll adjustment until the click handler runs. Moving a partly clipped card during pointer focus could shift the release target and lose the click. A motion review that only activates fully visible cards misses this interaction. The existing pointer tests include clipped-edge presses and touch taps at the phone, desktop and equivalent zoom layouts.

## Evidence and limits

The normal-motion test checks a `0.12` second card transition whose property list contains only `border-color`. After finishing a board, it checks a `0.6` second banner animation with one iteration. The reduced-motion test checks `animationName === 'none'` on the banner and a parsed card transition duration of zero. These are source-level and computed-style checks.

For a visual change, the existing evidence is strongest when the computed checks and a completion journey agree. A class accidentally applied to the wrong element can produce a plausible screenshot while changing the actual animated surface. Conversely, a screenshot made after 600ms can look static even when an unintended repeated animation is present elsewhere. Reading the declaration and observing the transition answer different questions.

No motion duration here controls whether two cards match or when a mismatch closes. Those decisions come from the game rules and active time. The presentation stays synchronized by rendering their result, rather than waiting for a CSS event to authorize the next action. That separation is the main maintenance benefit of the small current motion surface.
