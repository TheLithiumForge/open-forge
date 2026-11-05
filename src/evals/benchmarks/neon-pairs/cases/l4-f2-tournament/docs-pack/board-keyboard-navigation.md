# Board keyboard navigation

This page traces keyboard movement from the pure target calculation through remembered focus and scrolling.

The board has many card buttons but only a single card in the page's tab sequence. Arrow keys explore the board after entry. Enter and Space use the native button activation path. The useful distinction is between choosing where focus goes and choosing a card for a turn: movement can happen while play is temporarily blocked.

## Files and responsibilities

`src/game/board/keyboard.ts` exports `BoardKey` and `navigationTarget`. It knows an index, side length, key and modifier flags. It returns a destination index or null. It does not know a card's face, whether it is matched, whether the session is paused or where the board is scrolled.

`use-board-focus.ts` turns that calculation into DOM focus. It owns refs for the board viewport and inner board, a `focused` state initialized to zero, and handlers for movement and focus receipt. `nearest-scroll.ts` handles the extra work needed to maintain nearest alignment in the scroll area. Board wires those handlers to Card and exposes `focusRemembered()` to Game through its imperative ref.

This split makes a navigation change inspectable in layers. A wrong destination can be reproduced without a browser by calling `navigationTarget`. A right destination with clipped focus belongs in the focus and scroll layer. An Enter key that changes Moves twice belongs in activation handling rather than the geometry function.

## Index geometry

Positions are zero-based in row order. For side length `size`, the current row begins at `Math.floor(index / size) * size`; the column is `index % size`. The target calculation clamps within the appropriate row or column.

| Key | Expression in `navigationTarget` | Effect at an edge |
| --- | --- | --- |
| ArrowLeft | `Math.max(rowStart, index - 1)` | Stays in the first column |
| ArrowRight | `Math.min(rowStart + size - 1, index + 1)` | Stays in the last column |
| ArrowUp | `Math.max(index % size, index - size)` | Stays in the top row |
| ArrowDown | `Math.min(size * (size - 1) + (index % size), index + size)` | Stays in the bottom row |

On a four-column board, index 5 is row 2, column 2. Left reaches 4, right reaches 6, up reaches 1 and down reaches 9. At index 3, right remains at 3; it does not continue at index 4 in the next row. At index 14, down remains at 14. The apparent rectangular geometry and keyboard geometry agree.

The function returns the current index for an arrow at an edge rather than returning null. That still counts as a handled arrow. The hook prevents its default behavior, then attempts to focus the same card. A key with no movement target is a different case: null means the board leaves it to the browser.

## Keys the board leaves alone

Before the switch, the function checks `ctrlKey`, `altKey`, `shiftKey` and `metaKey`. If any is true, it returns null. The switch recognizes only the four plain arrow key strings. Home, End, Tab and other keys also return null.

The event handler only calls `preventDefault()` when the result is non-null. This is the mechanism that preserves browser shortcuts such as Alt+ArrowLeft and Ctrl+End. It also leaves Shift+Tab to ordinary reverse traversal. The board does not intercept all keydowns and then try to recreate the browser's behavior for a selected subset.

Enter and Space are not returned as movement destinations. Card is a native button, so keyboard activation becomes a click through the element's normal behavior. Card's `onClick` invokes the same `activate(index)` callback used by a pointer click. There is no second keydown activation call that could double a move or reveal two cards from one press.

The input specification describes that division in [Input](input.md). [Native controls guide](native-controls-guide.md) explains why retaining the browser's activation behavior is useful beyond this board.

## The remembered entry point

The hook's `focused` state determines the card with `tabIndex={0}`. Board gives all other cards `tabIndex={-1}`. Receiving focus calls `cardFocused(index, card)`, which updates `focused`. The state therefore remembers the last card actually focused, including focus gained by clicking or tapping where the browser focuses a button.

This is a memory of focus, not of a selected game card. A player can reveal index 0, arrow to index 5 without revealing it, leave with Tab and return to index 5. The game may still have index 0 as its only unmatched revealed card. Collapsing those two kinds of state would make exploratory movement change play.

Tab and Shift+Tab are not trapped. When focus is on a card, Tab proceeds to the next page control after the board, which in the collapsed layout is the reproduction summary. Shift+Tab proceeds back toward Restart. Re-entering finds the remembered card because it is the board's current native tab stop.

Matched cards remain candidates for this remembered position. A match does not evict focus from its second card or make arrow movement skip it. That keeps coordinates predictable when part of the board has been solved. The matched state is communicated separately; see [Disabled state guide](disabled-state-guide.md).

## A replacement resets the memory

Game renders `<Board key={session.generation} ... />`. New Game, Restart and successful loads increment generation through `replaceGame`. React mounts a new Board and the hook initializes its focus memory to index 0. The code does not attempt to clamp an old remembered index into a smaller replacement board.

A regular turn, mismatch closure or clock render does not replace that key. The remembered location survives those updates. So does opening or closing the reproduction details element, which is outside the Board. A failed load leaves the generation unchanged and does not reset navigation memory.

Resetting memory is separate from moving current focus. The start and load buttons retain focus after those actions, and initial page load does not force focus to the first card. The first card is ready for the next board entry. [Focus policy](focus-policy.md) describes the action-specific transfers.

## Moving actual DOM focus

The hook's `navigate` handler receives the React keyboard event and the current card index. It calls `navigationTarget(index, size, event.key, event)`, then looks up the target in `element.current?.querySelectorAll('button')`. The returned NodeList follows the card DOM order, which Board creates by mapping the deck in row order.

The target card receives `.focus({ preventScroll: true })`. Preventing default focus scrolling here avoids the browser centering it before the board's focus handler can apply the intended alignment. When focus arrives, `cardFocused` remembers its index and, for keyboard-visible focus, calls the nearest-scroll helper.

The lookup assumes the inner board's button order is the card order. An extra button inserted inside `.board` would affect that assumption even if it looked like a harmless helper. This is a concrete review concern for changes to Board's children. Additional controls elsewhere in the board viewport do not belong to this particular NodeList if they are outside the inner element.

## Scrolling is part of keyboard usability

Large boards can scroll horizontally and vertically. A correct active element is insufficient when the player cannot see its focus outline. Cards have `scroll-margin: 6px`; the shared `:focus-visible` outline is `3px` wide with a `2px` offset. Nearest alignment has enough clearance for that ring.

`useNearestScroll` remembers the viewport's last reported `scrollLeft` and `scrollTop` through a passive scroll listener. Before calling `card.scrollIntoView({ block: 'nearest', inline: 'nearest' })`, it restores that stored board position. The source comment explains the Chromium case: native Tab entry may center a card before focus handlers run, with the resulting scroll event arriving later. Restoring the prior position lets nearest alignment begin from where the player was looking.

The helper leaves page scrolling to the browser. It does not freeze the outer document or always force the board to its top-left corner. Nearest means making the card visible with a small necessary movement, rather than centering every visited card. Tests check interior cards as well as corners because padding alone can hide a mid-board outline problem.

## Resume and paused exploration

Board exposes `focusRemembered()` using `useImperativeHandle`. That function looks up the remembered card, focuses it with `preventScroll: true` and explicitly applies nearest scrolling. Game uses it before continuing an imported unfinished snapshot, since the Resume button is about to disappear.

A paused import does not disable the navigation hook. A player may move focus to another part of the board while no card action changes the session. Resume then returns to that newly remembered position. The same independence applies during an ordinary mismatch: the pair waits to close, but arrows still explore positions.

This separation is visible in `tests/focus-entry.e2e.test.ts`: it imports a large paused board, remembers a far card, scrolls back to the start, then resumes. The expected focus and right/bottom clearance refer to the far remembered card. It is stronger evidence than merely checking that some card received focus.

## Evidence and limits

`keyboard.test.ts` checks ordinary destinations, row and column edges, modifiers and unlisted keys. `tests/keyboard.e2e.test.ts` checks native exit, remembered re-entry and matched-card behavior. `tests/focus-entry.e2e.test.ts` covers nearest alignment on Tab, Shift+Tab and Resume, plus visible focus when switching from pointer input to arrows. `tests/layout.e2e.test.ts` and `tests/zoom.e2e.test.ts` exercise full outline visibility at constrained sizes.

Those tests establish geometry and browser behavior for the project's recorded runtime. They do not constitute a report of spoken screen reader output. [Screen reader notes](screen-reader-notes.md) separates the accessibility-tree evidence from the user acceptance listening journey.

For diagnosis, record the focused index, expected row and column, pressed key, modifier state and board scroll position. That information can distinguish a clamping error from a scroll issue in a few lines. Reporting only that an arrow “did nothing” is ambiguous at a valid board edge, on a modified shortcut and when focus moved outside the visible area.
