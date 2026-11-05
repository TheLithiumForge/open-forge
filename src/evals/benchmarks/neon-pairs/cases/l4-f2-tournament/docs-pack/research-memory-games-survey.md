# Memory-game survey notes

These are design-survey notes comparing common memory-game interaction patterns and explaining which tradeoffs are reflected in Neon Pairs.

The internal survey was organized by interaction patterns rather than named products. The comparisons informed the alternatives considered for Neon Pairs.

## What was compared

The review grouped examples by the work they asked a player to do. Some emphasized fast reactions, some emphasized position memory, and others wrapped a matching board in progression systems. The team was interested in the consequences of those patterns: what counts as an action, how long information stays available, whether solved positions remain stable, and what makes someone want another attempt.

This framing avoided a feature checklist in which every popular addition looked like a missing requirement. A small offline game can be coherent precisely because it chooses fewer surrounding systems. Neon Pairs currently concentrates on matching complete pairs with move count, elapsed time, local bests, and reproducible local artifacts.

The review did not claim that one pattern was best for every audience. A time-pressure game and a quiet memory exercise may both be successful while making incompatible choices about the same pause. The value of the comparison was clarifying the kind of interaction Neon Pairs already described.

## How a miss ends

The surveyed patterns included immediate closure, a fixed observation interval, and an explicit dismissal before continuing. Immediate closure makes failed guesses difficult to study. Explicit dismissal offers unlimited observation time but adds an extra action that is not a card choice. A fixed interval gives predictable pacing while leaving room to register both identities.

Neon Pairs uses an 800-active-millisecond interval. During it, card activation is blocked while focus movement remains possible. Both faces close together. The current game does not make that interval shorter as the board grows or as the player improves.

The survey treated active time as part of this choice. If a hidden tab consumes the interval, the person has not actually received the intended observation opportunity. The [mismatch interval page](mismatch-interval.md) gives the present behavior, and the [clock incident](incident-clock-drift.md) explains why browser scheduling alone cannot express it reliably.

## What success removes from the board

Some memory-game patterns remove solved cards or collapse the remaining positions inward. Others leave the board's geometry intact and mark solved locations. The first can make progress visually dramatic. The second preserves the spatial coordinates the player is trying to remember.

Neon Pairs keeps matched cards visible at full opacity, with a green border and check. They remain keyboard destinations, although their action has no game effect. This choice lets the board get more solved without getting rearranged. A remembered partner does not move because an unrelated pair was found.

The discussion noted a tradeoff: a large solved area still occupies space. In a different game, removing it might simplify the visual field. Here the stable geometry and consistent movement model were more valuable than compacting the remaining task. [Selection state](selection-state.md) and [card artwork](card-artwork.md) describe the current state and rendering separately.

## What the score rewards

The review compared per-click counts, pair-attempt counts, bonuses for streaks, and countdown results. Each rewards a different behavior. A per-click count penalizes harmless repeated activations unless special cases are added. Streak bonuses encourage risk-taking and can make a local result depend on a larger scoring formula.

Neon Pairs adds a move on the second eligible reveal. Correct and incorrect attempts each spend one. Matched pairs show progress, and the local best records lower completed move counts for each size. Elapsed time is visible but is not another weighted score or a tie-breaking mechanism in the local record.

This makes a perfect small-board result understandable: eight pairs require at least eight moves. The current storage validator uses the pair total as a lower bound for saved bests. It does not imply that a locally supplied result is verified as honest play. The [best-score records page](best-score-records.md) explains that limited purpose.

## How identity is recognized

The survey included photographic pairs, emoji, colored symbols, and generated geometric faces as categories. Photographs can be memorable but add asset management and may be hard to distinguish at small sizes. Emoji depend on platform rendering and can vary in shape. Color-only symbols give a narrow recognition channel.

Neon Pairs draws geometric SVG faces from shape, marker direction, and fill. A repeated palette reinforces marker distinctions. All 128 identities can be produced locally without external image files. Hidden cards do not mount those drawings, preserving the same information boundary in visual and accessible representations.

The tradeoff is that some generated near-neighbors require close review. A solid square and an outline square with diagonal markers are not as categorically different as a cat photograph and a bicycle. The current diagnostic face sheet makes those comparisons explicit at the minimum card size, including grayscale review. The [SVG-face decision](adr-007-svg-faces.md) describes the lasting choice.

## What surrounds the attempt

The patterns survey distinguished a single-page board from a sequence of title, level-select, play, and results screens. A separate flow can support progression and story. It also adds destinations, transitions, and focus handoffs to an otherwise compact game.

Neon Pairs currently stays in one page. Controls, progress, board, and final summary share that page. Reproduction tools are secondary, inside a collapsed native details element. There is no route change when the final pair is found, so the result remains near the board that produced it.

This aligns with offline distribution as one HTML file and with a small React composition. It does not mean that single-page layout is universally simpler for every future feature. The [routing decision](adr-044-no-routing.md) and [architecture page](architecture.md) explain the current project's scale and boundaries.

## Repeatability as a debugging feature

The survey treated replaying the same arrangement separately from starting a fresh challenge. Restart preserves a coded deal; New Game chooses another seed. Board codes make a starting arrangement shareable, snapshots make progress portable, and timed replay supports developer reproduction.

These formats are not presented as social infrastructure. They do not require a public feed, an account, or a service to resolve a code. They are local artifacts interpreted by a fixed versioned algorithm. The current generation version is therefore part of the meaning of the code, not merely a decorative prefix.

The survey considered this an unusual but useful investment for a small game: repeatability helps both a player revisiting a difficult arrangement and an engineer investigating a report. The [reproducibility page](reproducibility.md) explains which artifact answers which question.

## Why several attractive patterns were left aside

Hints, cosmetic theme packs, timed objectives, and connected play all appeared in the discussion. Each could be interesting, but none was necessary to complete the core memory loop. They also introduced questions that the existing state and local records did not answer by themselves.

The retired-idea pages preserve those questions more usefully than a vague backlog entry. [Hints](retired-idea-hints.md) concern information disclosure, [themes](retired-idea-themes.md) concern identity stability, [timed mode](retired-idea-timed-mode.md) concerns objective and active-time meaning, and [online play](retired-idea-online-play.md) concerns trust and service boundaries.

The survey's outcome was a clearer description of the current product rather than a list of additions to catch up with other games. It favored stable positions, explicit pair attempts, structurally distinct faces, local repeatability, and a short route from opening the document to playing. The comparisons remain historical rationale; the actual present contract lives in [Rules](rules.md) and the linked specifications.
