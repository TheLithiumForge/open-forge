# ADR 024: Give the board one remembered tab stop

This record explains roving tabindex, edge-stopping navigation and focus memory independent of play.

## Context

A 16x16 board has 256 card buttons. Putting all of them into the page's ordinary Tab order would make reaching the reproduction controls or leaving the board laborious. Removing the board from the keyboard path would be worse. The selected compromise is one remembered tab stop inside the board, with arrow keys for spatial movement.

Before any card focus has been remembered, the tab stop is the first card. After focus moves, that card has `tabIndex=0` and the others have `-1`. Native Tab and Shift+Tab enter or leave the board rather than stepping through every position. [Board keyboard navigation](board-keyboard-navigation.md) gives the interaction details.

## Decision

`useBoardFocus` owns the remembered index. `Board` passes each `Card` the corresponding tab index and the hook's focus and navigation callbacks. The game state does not contain the focus index, because focusing a card changes neither its face nor the move count.

The pure `navigationTarget` function computes destinations for plain arrow keys. Left and right stop at the current row's edges; up and down stop at the top and bottom without wrapping to another column. Modifier combinations and unlisted keys return `null`, leaving those keys to the browser. Native button behavior handles Enter and Space.

Matched cards remain focusable. During a mismatch, movement is still available although activation is blocked by the game. These choices let a player inspect and traverse the board without interpreting “cannot reveal now” as “cannot focus this position.” [ADR 006](adr-006-native-controls.md) explains the underlying button model.

## Remembering and resetting

`Game.tsx` keys the board with the session generation. A successful new deal, Restart or load replaces the board's focus memory and starts again at the first position. That reset does not itself move the browser's focus away from the action control. It changes where a later Tab entry goes.

Resume is the special path because its button disappears. `Game` calls the board's `focusRemembered()` before the controller removes the pause. This prevents focus being left on an element about to be unmounted and takes the player back to a useful position.

## Visibility and scrolling

The focus hook moves cards with `focus({preventScroll: true})`, then applies nearest scrolling. It remembers the board viewport's previous scroll offsets to counter Chromium's eager centering on native Tab entry. Cards have scroll margin so the focus outline fits, including at positions away from the outer board edge.

Pointer focus is treated carefully. Scrolling a button beneath a pressed pointer can cause release to land elsewhere and lose the click. The hook therefore applies its focus-time scrolling for keyboard-visible focus, while a completed click receives a nearest-scroll adjustment afterward. [ADR 047](adr-047-touch-scroll.md) records that detail.

## Alternatives and evidence

A fully tabbable board was easy to author but expensive to navigate. A focus trap would have made exiting the board dependent on a special escape instruction. Wrapping arrows could have been convenient for some users but would have contradicted the spatial edge model selected for this board.

The chosen behavior has both focused navigation tests and browser journeys. The latter check Tab entry and exit, remembered positions, resets and complete focus-outline visibility. That combination verifies arithmetic and browser behavior separately. [Focus policy](focus-policy.md), [ADR 035](adr-035-scroll-large-boards.md) and [ADR 043](adr-043-keyboard-parity.md) cover the related decisions.

## Spatial destination calculation

The navigation function returns a destination or declines the key. It never reveals a card or changes progress.

```ts
export function navigationTarget(
  index: number,
  size: number,
  key: string,
  modifiers: KeyModifiers,
): number | null {
  if (modifiers.ctrlKey || modifiers.altKey || modifiers.shiftKey || modifiers.metaKey) return null;
  const rowStart = Math.floor(index / size) * size;
  switch (key) {
    case BoardKey.left:
      return Math.max(rowStart, index - 1);
    case BoardKey.right:
      return Math.min(rowStart + size - 1, index + 1);
    case BoardKey.up:
      return Math.max(index % size, index - size);
    case BoardKey.down:
      return Math.min(size * (size - 1) + (index % size), index + size);
    default:
      return null;
  }
}
```
