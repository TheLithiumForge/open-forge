# Neon Pairs
Neon Pairs is a browser memory game that asks you to find every pair of matching cards.

You reveal two cards at a time and try to finish in as few moves as possible. A board code reproduces the same deal, so a reported problem can start from the same cards.

## Play

Choose a board size, then reveal a card and look for its partner. Matches stay face up. After a miss, both cards turn back so you can try again. Finding every pair completes the game.

New Game gives you another deal. Restart repeats the current deal from the beginning. The complete turn, score and clock behavior is defined in [Rules](rules.md).

## Controls

Use a mouse, keyboard or touch screen. Cards keep the same game behavior on every device. Focus movement, activation, right-click and phone gestures are defined in [Input](input.md).

Game options, board codes and snapshot actions are defined in [Controls](controls.md).

## Appearance

The board uses code-drawn faces on a dark background. Card identity stays distinguishable without color. Layout, drawing, colors and motion are defined in [Visuals](visuals.md).

The base game is silent. Audio requirements are defined in [Sound](sound.md).

## Repeat a game

Use a board code for the initial deal, a snapshot for progress or a replay for timed actions. The generation algorithm, formats and validation rules are defined in [Reproducibility](reproducibility.md).

Version 1 replay runs through the CLI and game core.

## Build

Implement the game with React, TypeScript and DOM controls. Package versions, commands and the offline distribution are defined in [Technology](technology.md).

Keep game behavior with its source, checks and support. State changes, browser effects and module placement are defined in [Architecture](architecture.md).

## Check

Each requirement has an ID in its defining document. The required evidence and treatment of hidden checks are defined in [Acceptance](acceptance.md).
