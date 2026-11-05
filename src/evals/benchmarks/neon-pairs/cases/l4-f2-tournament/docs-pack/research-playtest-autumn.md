# Autumn playtest notes

These are follow-up playtest notes about larger boards, reproducible attempts, and restoring progress after players had already learned the basic Neon Pairs loop.

The follow-up study explored questions that became visible after the introductory playtest.

## The task changed from learning to returning

The spring study asked whether a person could begin and finish. The autumn study asked what happened after that first success: choosing a larger grid, repeating a known arrangement, leaving a difficult attempt, and returning to saved progress. These tasks introduced navigation and data lifetime questions that a single short board rarely reveals.

The sessions began with a familiar small board, then offered a larger size and the reproduction panel. Participants were told that one artifact described a starting arrangement and another described progress. The moderator watched which artifact they chose for a concrete goal rather than asking them to memorize terminology.

This setup was deliberately exploratory. There was no attempt to rank memory ability or to compare people by completion time. A participant struggling with a sixteen-column board might be experiencing the intended memory difficulty, a scrolling problem, or a face-recognition problem. The notes kept those explanations separate until the interaction provided evidence.

## Larger did not mean a different game

The participants generally expected the same turn rules after changing size. They did not look for a new score formula or a shorter mismatch interval. That expectation matches the current implementation: size determines the deck's length and pair count, while `activateCard` and `advanceGame` apply the same transitions.

The useful friction was remembering where a face had appeared beyond the visible window. People sometimes described regions rather than exact positions: upper-right, lower rows, or near the middle. Stable row and column geometry helped, but scrolling made the board feel like a larger surface than the small default board.

This observation supported keeping the visible window separate from the full board. Shrinking all positions to fit might reduce scrolling while increasing face and target difficulty. The current internal overflow preserves the card minimum and leaves the page controls reachable. The [large-board incident](incident-16x16-layout.md) explains how the alternative failed in the engineering history.

## Restart and New Game were tested by expectation

One task asked participants to try the same arrangement again. People who chose Restart used their previous memory intentionally. A few chose New Game and were surprised that the arrangement changed, then corrected their interpretation from the label and result.

The current difference is exact: Restart passes the active board code to `createGame`, while New Game asks for a fresh code at the current size. Both clear progress. The controls do not need separate result screens to express this difference, and an exported code can make it inspectable during a report.

The research discussion did not recommend renaming the buttons or adding a confirmation step before each one. The errors were recoverable through another start, and the session purpose was to understand behavior, not to invent friction around every possible misclick. The [new-game guide](new-game-and-restart.md) supplies the precise present explanation.

## A board code was easier after a concrete failure

Participants understood a code more readily when given a reason to share a particular arrangement. “Show someone the deal you are looking at” was more effective than an abstract introduction to deterministic generation. The short text made sense as the name of a starting layout once it produced the same positions again.

The notes also recorded a predictable misunderstanding: someone expected a board code to preserve matched cards. Loading it reset the attempt because it describes the starting deck. This was not a parser error. It exposed the need to distinguish deal identity from progress in explanation and examples.

Current reproduction controls reflect that distinction with a Board code field and a separate Snapshot JSON textarea. The details panel starts collapsed so the primary game does not begin as a data-entry form. The [reproduction panel page](reproduction-panel.md) covers the present controls, and [sharing FAQ](sharing-faq.md) addresses which artifact fits which goal.

## Restored progress needed time to inspect

In the snapshot task, participants first looked for familiar matched positions, then the temporary selection, then the counters. An automatically running imported mismatch could have closed before that inspection finished. The current paused import lets the person orient themselves and then select Resume.

This pause also made the feature easier to explain. The board was visibly restored but waiting for an explicit continuation. Focus movement remained available, so a keyboard user could inspect positions while card actions stayed blocked. Resume returned to the remembered board position rather than leaving focus on a disappearing control.

The current load notice supplies the explanation in text, and the button supplies an action. The [snapshot pause decision](adr-037-snapshot-pause.md) and [snapshot release](release-notes-1-4.md) explain the engineering outcome.

## Invalid drafts were part of the task

Copying data during the sessions produced truncated or mistyped drafts. The useful behavior was retaining both the existing game and the entered text after a failed load. People could compare, correct, and retry. Erasing a draft would have turned a small correction into another transfer task.

Current `loadBoard` and `loadSnapshot` validate before replacement. Their error notices are distinct, while successful loads update the active size and reset board entry memory. A visible, correctable rejection was more understandable than silently loading a position whose meaning had been changed.

A related finding was that a completed snapshot looked like evidence of a prior result, not a newly achieved win. The current static banner and absence of Resume fit that interpretation. Import does not itself establish a local best; a later winning action on resumed unfinished progress can. This is a session distinction that casual visual inspection alone would not reveal.

## Faces at the dense end of the range

The larger boards used more of the face catalogue, including outline fills and diagonal marker positions. The hardest comparisons involved nearby geometric features rather than entirely different colors. Participants who relied mostly on color noticed repeated palette values as the face catalogue expanded.

The current identity scheme anticipates that: shape, marker, and fill define a face, while color reinforces marker direction. A minimum-size grayscale sheet is therefore a useful review artifact. It tests whether structural distinctions survive without the palette. It does not prove that a full game places the same perceptual demand on every player.

The autumn notes led to questions about marker separation and matched check placement, which are concrete in `Face.tsx`, `face-design.ts`, and `card.css`. The current check occupies a small corner area to avoid obscuring the top-right marker. [Card artwork](card-artwork.md) describes the final rendering.

## What remained unresolved by this study

The study offered directional support for internal board scrolling, explicit progress transport, paused restoration, and correctable drafts. It did not supply real-device long-press evidence, spoken screen-reader confirmation, or a broad browser compatibility sample. Those require different methods from a facilitated general playtest.

The [phone notes](research-phone-usage.md) and [screen-reader session notes](research-screen-reader-sessions.md) keep those evidence boundaries visible. The [roadmap](roadmap-notes.md) records questions still open for discussion. The autumn study's main contribution was recognizing that returning to a game is a sequence of navigation and data choices, not simply the basic reveal loop repeated later.
