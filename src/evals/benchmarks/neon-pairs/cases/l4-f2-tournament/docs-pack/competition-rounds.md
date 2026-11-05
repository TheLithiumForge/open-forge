# Competition rounds

This page explains the stages of a shared-device bracket and the round text associated with play.

A round provides context that the card board cannot provide on its own. A participant may recognize the game immediately and still be unsure where the group has reached. This page concerns that event context; the arrangement of opponents is in [competition pairing](competition-pairing.md), and the dimensions of the playing board are in [competition boards](competition-boards.md).

## Names during play

The round name describes a stage of the event rather than the current card selection or the number of turns taken. Name each round Quarterfinal, Semifinal or Final, and show it as "Round: NAME" while that round is played. A bracket of four has no Quarterfinal. The resulting vocabulary is small enough for a waiting participant to recognize without interpreting a bracket diagram.

For an eight-person event the names progress through Quarterfinal, Semifinal and Final. For a four-person event the names begin with Semifinal and lead to Final. These follow the naming rule.

The capital letters in the quoted pattern indicate where the round's name appears. Substituting Semifinal gives `Round: Semifinal`; substituting Final gives `Round: Final`. The pattern does not imply that the screen literally presents the placeholder. It also does not define the name of the participant who is currently using the device.

## Round, match and board are different contexts

A round can contain several matches. A match concerns a particular pair of people. The board currently rendered is the puzzle someone is working on. These relationships matter when a screen is refreshed after a handoff: a new game state does not, by itself, establish that the event has moved to another stage.

Consider the early part of an eight-person example. The group can still be in its first round while several people have already finished individual attempts. A mockup that advances a round label whenever the completion banner appears would conflate an individual game result with event progress. The existing completion banner reports a board finish; it contains no event interpretation.

This is a useful distinction even in static designs. A review can ask what the round text is describing at the exact moment represented by a screenshot. If the answer depends on which person happened to finish last, the design may be mixing stage context with attempt history. The labels above identify the event stage, while the remaining competition pages explain the other subjects.

## What the current status component knows

`src/game/status/GameStatus.tsx` accepts a `game`, a `best` value and a `celebration` flag. It renders moves, matched pairs, elapsed time, the current best and a completion paragraph when `isWon(game)` is true. It has no round prop and does not query an outer event.

The labels in `src/game/status/status-labels.ts` are the four ordinary game statistics. Searching there for the competition round text will not reveal an unconnected implementation waiting to be enabled. The present application simply lacks that event concept.

Similarly, `src/game/Game.tsx` is the composition of a solo session. It passes `session.game` to `GameStatus` and uses `session.generation` as the board's React key. Neither operation establishes which stage a group has reached. [Status bar](status-bar.md) explains the existing statistics, and [React boundaries](react-boundaries.md) explains the surrounding composition.

This source boundary is valuable in review. Someone can demonstrate the round text in a prototype without having changed the underlying game mechanics. Conversely, reusing the existing status component does not supply a round label automatically. The product concept and its rendered location are separate matters.

## A generation is not a round counter

The `generation` member of `Session` begins at zero in `initialSession`. `replaceGame` increments it when a new game replaces the previous one. The increment lets timer work associated with an earlier game be recognized as stale. It also causes `Game.tsx` to mount a fresh board instance through the keyed component.

That counter changes during Restart and successful board or snapshot replacement. A person can perform those actions without any group event existing. Reading generation 2 as the second round would therefore make an event interpretation out of an implementation lifetime counter.

The example becomes more concrete during a miss. An ordinary restart clears the previous game and its pending selection. The replacement game has a later generation even though it can have exactly the same board code. The generation's meaning is about callback validity and component lifetime. The board code's meaning is about the deal. Neither is a name for an event stage.

`tickSession(session, generation, milliseconds)` checks the generation before advancing logical time. This check is covered in `src/game/session/session.test.ts`. The test is evidence for game replacement behavior, not evidence for bracket progression. [Mismatch interval](mismatch-interval.md) and [new game and restart](new-game-and-restart.md) cover the underlying behavior.

## How a sequence can be reviewed

A paper review can show several moments from an event instead of only its opening and final screens. One moment might show a board in progress, another an individual finish, and another a later match within the same round. The round names provide a stable point of reference across those images.

The interesting question in that exercise is what changed between images. Card progress can change while the round name remains the same. A different participant can be present while the stage remains the same. Eventually the event context changes as well. Separating those changes makes the review more informative than a single screenshot with every field different.

For the four-person example, a screen sequence containing a Quarterfinal would contradict the naming decision above. For the eight-person example, a sequence that jumps straight from the first stage to the Final would be missing context. These are straightforward consequences of the defined stages, and they can be assessed without a complete game implementation.

Animations, transition screens, navigation controls and focus destinations are separate interface questions. [Focus policy](focus-policy.md) and [dialog policy](dialog-policy.md) describe the existing interactions.

