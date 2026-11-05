# Retired idea: hints

This is a design note about hint systems considered for Neon Pairs and set aside before implementation.

The current game has no hint control, hint counter, assisted-result category, or hint-related snapshot field.

## The original attraction

The suggestion came from watching someone lose track of a partner on a larger board. A small nudge seemed like a gentle way to reduce frustration without changing the matching rules. The initial sketch was deliberately vague: a hint would help someone continue rather than complete the game for them.

That vagueness concealed several different features. Highlighting a card the player had already seen is different from revealing an unseen face. Pointing to a partner of the current selection is different from briefly showing the entire board. Each changes how much information the player receives and whether it comes from memory or from the system.

The review therefore stopped treating “hint” as one small button. It examined the information each proposal would need and the result it would create. This was the point at which the idea ceased to fit comfortably inside the existing simple local score.

## Proposal A: remind the player of a seen card

One sketch would point toward a face that had previously been revealed. This sounded closest to ordinary remembering: the system would not disclose anything the player had never had an opportunity to see.

Current `GameState` does not retain a history of seen positions. It holds the immutable board, matched indices, current selection, moves, elapsed time, and whether play started. Once a miss closes, the core retains no separate record that those identities were observed. A reminder based on observation history would therefore need additional state and a definition of what counts as observed.

That definition becomes subtle around hidden pages and imported progress. Was a card observed if the page became hidden immediately after reveal? What should a snapshot preserve about earlier exposures? A snapshot contains the current state, not the complete history of visible identities. Inferring a seen-card set from moves would not recover it.

The discussion did not answer these questions with new rules. It used them to show that the proposal was not a free presentation change. The [game-state model](game-state-model.md) and [snapshot format](snapshot-format.md) describe the current, smaller set of facts.

## Proposal B: identify the partner

Another sketch would highlight the partner of the currently open card. The immutable deck makes that technically straightforward. It also changes the task directly: finding the second position is the core memory challenge, and the system would be supplying it.

The design question was not whether assistance is inherently undesirable. It was how to describe the resulting attempt honestly within a game whose local record compares completed moves. A partner hint could improve the count without demonstrating the same memory performance. Charging an arbitrary move penalty would introduce a new scoring formula, while storing a separate assisted category would expand persistence and display meaning.

The current best remains a simple minimum move count for each size. No assisted flag exists in `Preferences`, and the snapshot does not carry one. The review declined to add such a distinction just to make the initial button sketch seem complete. [Best-score records](best-score-records.md) documents the current scope.

## Proposal C: a brief whole-board reveal

The most visually appealing prototype idea flashed all faces for a short interval. It created an immediate sense of progress and a dramatic moment. It also raised the largest number of unanswered interaction questions.

Would the elapsed clock continue? Would a pending mismatch keep its remaining interval? Could someone activate a card during the preview? Would the preview last long enough to inspect the largest board, much of which is outside the viewport? A fixed visual flash would give different information to someone listening to card names than to someone seeing many faces at once.

Current `faceUpCards` derives visible identities from matched positions and the selection union. There is no preview selection case or board-wide reveal overlay. Adding one would require a real state transition and accessible interaction design, rather than a CSS class that simply exposes all SVGs.

The hidden-card boundary also matters. The current board passes `null` for a closed face and does not mount its SVG. A preview implemented by leaving identities in the DOM and hiding them visually would weaken the information model even outside hint use. The [card identity page](pair-identity.md) explains the present distinction.

## Why an automatic hint was harder to justify

The team briefly considered offering help after repeated misses. That would require defining a threshold and deciding what the game should infer from it. A player may be exploring deliberately, practicing a known code, or simply enjoying slow play. Repeated misses do not by themselves establish that an interruption is welcome.

An automatic highlight could also compete with the current turn result. The game already shows whether a pair matched and lets the player move focus during the mismatch interval. Adding an unsolicited suggestion would create another event at exactly the point where someone is trying to remember the two revealed positions.

No threshold or offer text was adopted. The retired status means the current app remains free of these branches.

## Alternatives that used existing tools

The discussion found several useful activities already supported by the current model. A player can Restart the same code and practice remembering its arrangement. They can choose a smaller board. They can export progress before leaving a difficult attempt and resume it later. None of these actions discloses a hidden partner through a new hint mechanic.

These actions provide different kinds of help from hints. They address some of the same frustration with existing controls and leave the memory task intact. The [new-game guide](new-game-and-restart.md), [board sizes](board-size-catalogue.md), and [snapshot pause decision](adr-037-snapshot-pause.md) explain their current behavior.

The research also suggested improving examples and the distinction between moves and pair progress. A person who understands the counter may be less likely to think a miss is an unexplained penalty. That is an explanation issue rather than a hint algorithm.

## Why the idea was retired

The team set the idea aside because the smallest useful hint still needed decisions about information history, score interpretation, serialization, and equal access to the disclosed information. Those decisions would be the feature, not finishing touches after adding a button.

Nothing in the current code reserves an extension point for hints. There is no placeholder action or inactive preference. If the idea returns, it would be new design work against the actual game contract. [Roadmap notes](roadmap-notes.md) retain that possibility as discussion, while the current product remains the unassisted two-reveal loop described in [Rules](rules.md).
