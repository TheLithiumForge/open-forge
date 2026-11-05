# Shared-device competition

This page explains the purpose, scope and implementation context of shared-device competition in Neon Pairs.

Competition gives a group gathered around one device a reason to return to the board together. It is intended for the small social setting: a family laptop, a tablet passed around a table, or a few people taking a break at the same desk. The interesting product question is how individual memory play becomes a shared occasion without losing the clarity of the existing game.

## Where the product stands

The game implements solo play. There is no competition entry form, bracket view, participant model or tournament controller in `src/game/Game.tsx`. That component composes the ordinary controls, status, board and reproduction panel around `useGame()`. Competition is a product extension that has not yet shipped.

This distinction matters during review. A prototype can show a convincing bracket around the existing board while still delegating all game actions to the ordinary session. That demonstrates a possible composition, but it says little about whether participant progress, handoffs and completion belong to the same coherent experience. The detailed pages identify the relevant behavior and the places where the current source offers support or creates a complication.

The existing specification is version 2.2. It describes a local browser memory game, its reproducibility tools and its browser session. It does not describe a competition flow. The additional decisions for this scope live in the linked competition pages; this overview introduces their purpose without reproducing them.

## Why share a device

A shared screen makes the activity visible. People can talk about the artwork, remember a near miss and recognize the effort behind another person's finish. There is no remote opponent waiting behind a network connection, and no account setup between the group and the game. The game already has a compact delivery model: the built game can be opened from one HTML file.

Sharing hardware also changes what people expect. On a personal device, a result feels like part of that person's practice. On a shared tablet, the person holding it may have changed several times since the page opened. A familiar status area can therefore carry a different social meaning even when its numbers are technically correct. The distinction between a person's current attempt, the group's progress and the device's ordinary records is especially easy to blur.

Competition is useful when it gives those existing actions a context people can follow. It is less useful when the group spends more attention administering the session than remembering cards. This is why the research discussion concentrates on interruptions, orientation and the physical act of passing the device, rather than treating the bracket drawing as the whole feature.

## Three things people are following

An individual concentrating on a board is following card positions and remembered faces. Someone waiting nearby is following the social event: who is occupied, whether a finish is close and when attention will return to the group. The person explaining the game is following the application itself, often watching for a control that might replace the current position.

These perspectives overlap, but they are not interchangeable. A board full of exposed faces is meaningful to the current player. It gives a waiting person relatively little information about where the group is in its event. A compact event view can answer that social question, while the ordinary board remains the place where the player thinks.

The competition documents separate these subjects so that readers can find the relevant decision without extracting it from a long screen-by-screen walkthrough. [Pairing](competition-pairing.md) covers the relationship between entrants and the bracket. [Rounds](competition-rounds.md) covers event stages. [Boards](competition-boards.md) and [deals](competition-deals.md) distinguish the dimensions of a board from its actual arrangement. [Ties](competition-ties.md) and [scores](competition-scores.md) address results from different angles.

## A local social activity, with local evidence

The current project has no backend or account service. Its board codes and exported progress are useful for reproducing a situation, not for establishing the identity of a person who played it. That is a comfortable fit for a group that is physically present and can see what happened. It is a much weaker basis for claims about distant strangers.

The same limitation applies to development evidence. A replay can help an engineer understand a sequence of card actions. A screenshot can help someone understand what the group saw. Neither explains why a participant handed the tablet back early, or whether someone had been watching another person's screen. Those are observations about the setting, and they belong in the research account rather than in an assertion about the game engine.

[Reproducibility](reproducibility.md) explains the existing technical tools. [Competition research](competition-research.md) describes local sessions and the limits of drawing conclusions from them. The two accounts answer different questions and are useful together.

## How the current code helps

The core already separates game data from presentation. `src/game/game.ts` contains card transitions and logical elapsed time. `src/game/deck.ts` contains board parsing and deterministic generation. Those files have no participant knowledge. That makes them a useful basis for discussing competition without embedding group administration into card rendering.

The browser session is a separate layer under `src/game/session`. It owns the bridge between that core and React, browser visibility, clock observations and preferences. This is the most consequential boundary for an extension: reusing a visual component is easy to notice, while inheriting a session side effect can remain hidden until much later.

The board itself lives under `src/game/board`. Its card buttons and navigation code operate on positions. The status view receives a game and a best value; it does not discover an event or a participant. [Architecture](architecture.md), [session module](session-module.md) and [React boundaries](react-boundaries.md) give the broader engineering context.

