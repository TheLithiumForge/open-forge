# Neon Pairs
Neon Pairs is a browser memory game that asks you to find every pair of matching cards.

Status: spec version 3.

Reveal two cards at a time and finish in as few moves as you can. A board code repeats a deal, so you can report a problem with the same starting cards.

## Play

Choose a size, then reveal a card and find its partner. Matches stay face up. A miss closes both cards after 800 active milliseconds. Finding every pair completes the game with your moves and time. New Game gives you another deal. Restart repeats the current deal. Full details are in [Rules](rules.md).

## Controls

Use mouse, touch or keyboard for the same card action. Keyboard movement, focus, card names and the screen reader journey are in [Input](input.md). The labels and behavior of game and reproduction actions are in [Controls](controls.md).

## Appearance

The game draws faces on a dark background. Shape, marker and fill distinguish each face without color. Layout, colors, motion, 16x16 behavior and zoom are in [Visuals](visuals.md). The game stays silent as defined in [Sound](sound.md).

## Repeat a game

A board code describes a deal. A snapshot saves progress. A replay records timed actions. Unnamed snapshots, board codes and replays keep format version 1. Named snapshots use version 2. The algorithms and formats are in [Reproducibility](reproducibility.md).

## Build

Use React, TypeScript and native DOM controls. Package pins, commands and the offline HTML distribution are in [Technology](technology.md). Pure game functions, browser effects and behavior placement are in [Architecture](architecture.md).

Use the journeys in [Acceptance](acceptance.md) to check the finished game. Those examples add no rules beyond the requirement IDs in their defining documents.

## Out of scope

The game doesn't need to support these, so they're never tested:

- Keys and modifier combinations beyond the ones in [Input](input.md#keyboard-movement), which stay with the browser
- Holding a key down
- Several tabs or windows of the game open at once, where each tab may simply overwrite the saved preferences
- How a pointer press behaves when it drifts or is dragged off a card
- Replay or snapshot files with encodings beyond the stated rules, such as a byte-order mark
- Spoken screen-reader output, which stays part of user acceptance as [Input](input.md#screen-reader) describes
- Confirming that a touch long press on a real device opens the context menu without revealing a card, which stays part of user acceptance like spoken output because automated browsers can't produce that gesture

Back to [Play](#play).
