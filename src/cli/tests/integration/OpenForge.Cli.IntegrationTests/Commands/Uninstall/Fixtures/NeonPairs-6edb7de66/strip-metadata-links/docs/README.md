# Neon Pairs
Neon Pairs is a browser memory game that asks you to find every pair of matching cards.

Status: spec version 2.2.

Reveal two cards at a time and finish in as few moves as you can. A board code repeats a deal, so you can report a problem with the same starting cards.

## Play

Choose a size, then reveal a card and find its partner. Matches stay face up. A miss closes both cards after 800 active milliseconds. Finding every pair completes the game with your moves and time. New Game gives you another deal. Restart repeats the current deal. Full details are in Rules.

## Controls

Use mouse, touch or keyboard for the same card action. Keyboard movement, focus, card names and the screen reader journey are in Input. The labels and behavior of game and reproduction actions are in Controls.

## Appearance

The game draws faces on a dark background. Shape, marker and fill distinguish each face without color. Layout, colors, motion, 16x16 behavior and zoom are in Visuals. The game stays silent as defined in Sound.

## Repeat a game

A board code describes a deal. A snapshot saves progress. A replay records timed actions. The algorithms and version 1 formats are in Reproducibility. Spec version 2.2 doesn't change those format versions.

## Build

Use React, TypeScript and native DOM controls. Package pins, commands and the offline HTML distribution are in Technology. Pure game functions, browser effects and behavior placement are in Architecture.

Use the journeys in Acceptance to check the finished game. Those examples add no rules beyond the requirement IDs in their defining documents.

## Out of scope

The game doesn't need to support these, so they're never tested:

- Keys and modifier combinations beyond the ones in Input, which stay with the browser
- Holding a key down
- Several tabs or windows of the game open at once, where each tab may simply overwrite the saved preferences
- How a pointer press behaves when it drifts or is dragged off a card
- Replay or snapshot files with encodings beyond the stated rules, such as a byte-order mark
- Spoken screen-reader output, which stays part of user acceptance as Input describes
- Confirming that a touch long press on a real device opens the context menu without revealing a card, which stays part of user acceptance like spoken output because automated browsers can't produce that gesture

Back to Play.
