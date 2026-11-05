# ADR 022: Name the information a player can know

This record explains positional card names, revealed face names and the boundary around hidden identity.

## Context

A memory game depends on learning a face only after revealing it. That rule applies to screen-reader users as much as visual players. A button called only “card” would make positions hard to remember, while naming a hidden button with its face would expose information before the reveal.

The chosen accessible name combines position with the card's current state. A closed card exposes its one-based row and column and the word “closed.” A revealed or matched card adds the face description. `describeCard` in `card-state.ts` is the small function that keeps this naming logic together.

## Position and state

Internal positions are zero-based row-order indices. The label converts them to a row and column using division and remainder by the board size. The first closed card is “Row 1, column 1, closed.” This coordinate language stays stable when the board scrolls or the player changes input method.

Matched state takes precedence over revealed state in the descriptor. A matched button remains focusable and reports `aria-disabled`, allowing inspection while its game action is ignored. The state word therefore carries useful play information rather than merely echoing a visual class. [Screen reader notes](screen-reader-notes.md) describes the complete journey.

## Face language

`faceDesign` constructs the face label from palette name, fill, shape and marker direction. Face 0 is “cyan solid circle, marker top”; face 64 is the corresponding outline circle. The actual card label places the state before that face description. Both artwork and text come from the same ordered design data.

The marker direction and fill are essential parts of identity. Color repeats the marker distinction and can be helpful language, but it is not the only difference between faces. A label that said merely “cyan circle” would lose the solid/outline distinction; a numeric ID alone would be difficult to connect to the visual face. [Pair identity](pair-identity.md) and [Face catalogue](face-catalogue.md) explain the design vocabulary.

## Keeping closed information closed

`Board` passes `null` for the face of a closed card. `Card` then renders its neutral back and the descriptor omits the face label. The hidden SVG is absent rather than merely visually obscured. The implementation also avoids hidden face identifiers in attributes and diagnostic globals.

This boundary makes names part of the game model presented to a player, not an accessibility-only back door. Tests know the deal through its documented code and independent generation; the application does not need a solver hook to support testing.

## Alternatives

Short names such as “Card 7” would identify a button but give less spatial structure for board navigation. A separate accessible image description would duplicate the button's meaning and risk becoming an extra navigable object. Describing every card with its face from initial load would make the game a different task for different users.

The chosen labels are somewhat long, especially on large boards. That cost was accepted for precise position and identity. Movement itself does not activate cards, so a player can inspect known positions without changing a turn. The native button remains the interaction object; the SVG is hidden from accessibility APIs.

Unit tests check representative closed, revealed and matched names, and browser tests inspect the accessibility tree through a full keyboard game. Spoken output remains an acceptance check with the user's screen reader. [ADR 018](adr-018-playwright.md), [ADR 024](adr-024-roving-focus.md) and [ADR 043](adr-043-keyboard-parity.md) cover the neighboring evidence and input decisions.

## The name constructor

The function’s nullable face input makes the closed-state information boundary visible in both its type and string construction.

```ts
import { faceDesign } from './face-design.ts';

const CardState = { closed: 'closed', revealed: 'revealed', matched: 'matched' } as const;

export function describeCard(index: number, size: number, face: number | null, matched: boolean) {
  const state = matched ? CardState.matched : face === null ? CardState.closed : CardState.revealed;
  const position = `Row ${Math.floor(index / size) + 1}, column ${(index % size) + 1}`;
  const label = `${position}, ${state}${face === null ? '' : `, ${faceDesign(face).label}`}`;
  return { state, label };
}
```
