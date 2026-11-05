# Spring playtest notes

These are qualitative playtest notes about how first-time players learned the Neon Pairs loop, interpreted progress, and decided whether to try another board.

The sessions explored how players understood the game through qualitative observations rather than a statistically representative comparison.

## What we wanted to understand

The spring sessions focused on the first few minutes of play. Could someone infer what a pair meant, understand when a move was charged, and recognize the end of an attempt? The interest was in the transition from “I can click a card” to “I know what I am trying to improve.” Speed of completion was not treated as a useful comparative metric in a small exploratory group.

The group mixed people who played familiar matching games with people who rarely played browser games. Most used a laptop pointer for the first board. A few switched to the keyboard after seeing the help text. Each person began with the small default board, then could explore controls or choose another size.

The moderator avoided explaining the full rules in advance. A long verbal introduction would have tested recall of the moderator's instructions rather than the page's affordances. When someone hesitated, the notes recorded the hesitation and the visible state before offering help. That distinction mattered because a successful completion after coaching does not show that the interface explained itself.

## The first reveal was easy to find

In the sessions, most people tried a card without searching for a start button. The repeated neutral backs made the board look interactive, and the native button response confirmed the action. No one needed an explicit “begin attempt” step before opening the first face.

The current game aligns with that observation. `started` becomes true on the first valid reveal, and the active clock begins then. A player can read the controls or inspect the size selector without spending attempt time before acting. The page's short introduction describes revealing and matching rather than requiring a separate tutorial journey.

One participant opened a card, clicked it again, and waited. This was useful because it tested a plausible action rather than a mistake in the moderator's script. The current repeated-selection rule leaves the state unchanged. The interaction does not charge a move simply for uncertainty about how the card behaves.

## Moves needed a visible example

The strongest early confusion was about the counter. Some participants expected each click to add one. Others expected only misses to count. Once they completed one matching attempt and one failed attempt, the current second-reveal model became easier to infer.

This did not suggest adding another score label. Two competing counts would have increased the vocabulary a new player had to learn. Instead, the discussion favored a consistent move definition and examples in supporting documentation. The current core adds a move only on a different unmatched second reveal, whether it matches or misses.

The display puts moves alongside pair progress and time. In the study, people used the pair total to answer “how much is left?” and the move count to answer “how well am I doing?” Those are related but different questions. The [scoring page](scoring-and-moves.md) describes the present rules without claiming that every player interprets them immediately.

## A miss was an observation opportunity

Participants in the notes sometimes leaned closer after a miss because the faces would soon close. The interval created attention without requiring a separate dismissal action. A few tried a third card during that interval and found that nothing happened. When the two open faces closed together, they could continue.

The current 800 active milliseconds therefore serve two purposes: show the information and define when another attempt can begin. The research supplied qualitative support for a short, consistent observation window and against closing a miss immediately on the next click.

The moderator also noticed that people described misses spatially: “that one was upper left” or “the other was on the bottom row.” This reinforced the value of keeping matched cards in place. Removing solved cards and compacting the grid would change the coordinates that the player was trying to remember.

## Faces were remembered in different ways

Some participants named shapes first. Others remembered marker direction or color. This variety was useful because it exposed why color alone was too narrow a vocabulary. The current face identity combines shape, marker direction, and fill; color repeats the marker distinction.

At the default size, the early board uses only the first eight face IDs, so not every dimension of the full face catalogue is exercised. A successful small-board session therefore cannot prove that all 128 identities are distinct at the smallest rendering size. The later [autumn study](research-playtest-autumn.md) and the diagnostic face sheet address a broader set of conditions.

The notes did not interpret a participant's casual nickname as a replacement for the formal accessible name. Someone saying “the blue thing at the top” is using their own memory aid. The game still needs names that identify the same visible structure consistently. The [face catalogue](face-catalogue.md) and [card naming decision](adr-022-card-names.md) describe the current vocabulary.

## Completion needed to stay on screen

In the sessions, several people paused after the last pair and looked at their result before touching another control. An ephemeral message would have required them to catch the exact moment of completion. The current banner remains visible with moves and time after its optional single pulse.

The small local best also prompted different reactions. Experienced players wanted to beat it. Others used it mainly to confirm that a previous attempt had existed. The research did not support interpreting the best as a public ranking or a verified achievement. It remained a local reference for the current size.

One participant restarted immediately to test their memory of the same arrangement. Another selected New Game expecting a fresh challenge. That contrast made the distinction between repeat and fresh deal worth preserving. The current board code gives the distinction a concrete technical identity, even though a first-time player need not inspect the code to understand the buttons.

## Keyboard findings were directional

The keyboard exploration was informal. It revealed that an arrow route could be learned quickly after entering the board, and that leaving the board mattered as much as entering. It did not provide screen-reader evidence or a complete assessment of browser command interactions.

The study notes therefore fed questions into engineering review rather than claiming to close them. Does focus stay visible after pointer use? Does a matched card remain a spatial destination? Can Tab leave from an interior card? Those questions became more concrete in [keyboard release notes](release-notes-1-1.md), [the focus incident](incident-focus-trap.md), and the later accessibility discussion.

## What we took forward

The spring study supported keeping the first interaction small, the move definition consistent, solved positions stable, and the final result persistent. It also exposed evidence gaps: a small pointer board said little about large-board reachability, all face identities, restored progress, or spoken output.

The sessions were exploratory rather than a controlled comparison. The useful output is the set of observed misunderstandings and the code boundaries that address them. The [autumn notes](research-playtest-autumn.md) follow the next questions, while [phone usage](research-phone-usage.md) isolates the effects of a narrow, scrollable view.
