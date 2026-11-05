# Audio research notes

These notes record the design exploration behind Neon Pairs' silent feedback and explain why an audio treatment was set aside.

The resulting product decision is unambiguous in [Sound](sound.md): the game stays silent through every action and state. Version 2.2 includes no audio files, audio API calls or sound toggle. [Feedback without audio](feedback-without-audio.md) describes the implementation in detail. This page records the reasoning, including the useful parts of the discarded idea.

## The question that made sound worth trying

The early design question was small: after the player reveals a second card, how quickly can they tell whether the pair was secured? Memory games have a natural two-part rhythm, and an audible confirmation seemed to fit it. A short reveal tick could acknowledge input. A rising two-note cue could acknowledge a match. A softer falling cue could explain a miss. A final phrase could make completion feel like an occasion rather than just the absence of cards left to find.

In the sketch session, the attraction was especially clear when someone watched the cursor instead of the whole board. The second card appeared under the pointer, but the first card might be elsewhere. A cue could give a summary without requiring another glance across the grid. It could also add warmth to a visually restrained interface. These were sensible hypotheses to explore; silence was not assumed to be automatically clearer.

The sketch therefore treated audio as acknowledgment and mood rather than as a source of face identities. No face was assigned a melody. The identity space already had shape, marker and fill, and adding a new mnemonic system would have changed the nature of the memory task. The research question was whether a small set of outcome sounds improved confidence in ordinary play.

## Three passes through a disposable prototype

**First pass: a tick on every reveal.** The paper interaction was replaced with a rough clickable board and a brief, dry tick. The cue was intentionally modest. It helped the person demonstrating the prototype establish a rhythm: first reveal, second reveal, inspect the pair. It also made accidental extra activations feel more consequential than they were. The prototype raised a question the visual board already answered precisely: did the game accept this input, or did the button merely receive attention?

A matched card, a repeated first selection and an activation during a mismatch all need different interpretation from a valid reveal. A generic input tick blurred those distinctions. Restricting it to accepted reveals was better, but then the useful fact was the same one conveyed by the newly visible face. The tick was pleasant while watching the demo and much less valuable when it was absent from a real player's device.

**Second pass: outcome cues only.** Removing reveal ticks left a pair-found cue and a miss cue. This was easier to understand because each sound corresponded to a resolved turn. The match cue added a satisfying accent; the miss cue was harder to settle. A harsh cue made an ordinary part of memory play feel like a reprimand. A gentle cue was less intrusive but less distinct. Making it louder solved neither the tone nor the shared-room problem.

**Third pass: completion only.** A final cue avoided repetitive sound and preserved a reward at the end. It worked well as a demonstration flourish. It was also the cue most likely to occur after the room had forgotten the page could make noise. A person could play several quiet turns, settle into the task and then finish into an unexpected phrase. The very rarity that made it less repetitive made it harder to anticipate.

## The room mattered more than the cue

The review used three ordinary settings: a person sharing a room, a person with their own background audio, and a person playing with device output effectively unavailable. None makes sound inherently bad. Together they weaken the case for making sound part of the default interaction.

In the shared room, the game’s sounds were heard by someone who had not chosen to play. Lowering the level reduced the disturbance but also reduced the cue's utility to the player. A completion phrase could be short and still be conspicuous. A very quiet tick might be unobtrusive to one device's owner and still carry through another device's speakers. The team could not turn a volume choice into a dependable property of the interface.

With background audio, the cues competed with something the player had already chosen. A match sound could disappear into that background, or interrupt it perceptually without adding useful information. In either case, the player still needed the visual pair state. In the unavailable-output setting, that need became obvious: the whole turn had to make sense without the cues.

## A mute switch was considered, but it did not answer the design question

In the discussion, the obvious compromise was to retain the sounds and add a switch. That offered a real benefit: people who liked the cues could choose them, and everyone else could turn them off. It also raised the question of which experience the game should present before the player has made that choice.

Starting with sound enabled left the first unexpected cue unresolved. Starting with it disabled made the control an invitation to configure a feedback layer that the game had already demonstrated it did not need. Remembering the setting would introduce another preference to explain and verify. Forgetting it would make a returning player check the control again. Those are design costs, even when implementation is straightforward.

The decision was to remove the sound treatment from this version rather than ship the prototype's control. That is why the current absence of a switch is meaningful. A developer looking for the mute state will find no audio preference waiting to be wired up. `Controls.tsx` exposes Grid size, New Game and Restart. `Reproduction.tsx` handles board and snapshot operations. Neither presents a sound option.

## What was learned about acknowledgment

The strongest argument for the reveal tick was immediate confirmation. The review separated that into three cases.

For an accepted reveal, the face appears and the button name gains its face description. For an accepted second reveal, the result also changes the board or begins the mismatch interval. These are direct effects of the action. They are stronger evidence than a generic sound because they show which card and which outcome the game accepted.

For an ignored activation, there is no new reveal to acknowledge. The current rules ignore repeated activation of the first selected card, matched-card activation and activation during a mismatch. Playing a click anyway would risk suggesting that something had been queued or counted. The shipped implementation can simply leave the relevant game state and turn feedback unchanged.

For navigation, the useful confirmation is position. Arrow movement changes focus, and the white outline identifies the destination. It does not choose a card. A sound on movement would add an event but not explain row or column by itself. The button's name already supplies that position to assistive technology. This division gives a consistent account across pointer and keyboard input without needing a family of navigation tones.

## A miss is information, not a punishment

The miss cue exposed a tone problem. Failing to find a pair is a normal way to learn the board. An admonishing sound could make that ordinary learning step feel like an error in using the application. Making the cue cute did not entirely remove the interpretation; it merely changed its emotional style.

The current treatment stays descriptive. The second face appears, the move is counted, and the message says, “No match. Cards close after a short pause.” The player then has the 800ms active mismatch interval to look at the two faces. After that both close together. It is a bounded opportunity to remember, with no separate penalty state beyond the move already spent.

One review note described the desired change in emphasis as “study these two” rather than “that was wrong.” It captures why the visual information and neutral result sentence were a better fit than an error-like sound. The current game contains no audio verdict to interpret alongside the faces.

## The final pair needed a durable result

A victory sound has an obvious weakness as the sole completion signal: it is over. A player may be interrupted, have output unavailable or simply want to inspect the final result later. The current design solves that with state that remains in the document.

`GameStatus.tsx` displays a paragraph beginning “Every pair found in” with final moves and elapsed time. The completed board remains visible with checks. The normal status fields remain available. The session feedback for the final match combines the pair result and completion, rather than treating them as unrelated moments.

The brief visual emphasis lives in `status.css`. Its `victory-pulse` lasts 600ms and runs once when a played completion sets `celebration`. The reduced-motion branch disables that animation. Neither variation changes the final text. A loaded completed snapshot also displays the result without presenting it as a newly played celebration.

## Speech output is a different layer

One confusing phrase in early conversations was “silent accessibility.” It could imply that silence means the page should avoid providing content that a screen reader can speak. That would defeat the purpose of making the game usable through accessible names and results.

The current application does not synthesize speech itself. It supplies button names, states and status text to the browser. A user's assistive technology can present that information through the output they choose. The fact that a screen reader may speak a card's name does not make that name a game sound effect.

## Evidence in the source

The game paths described above change state and DOM without an audio service, media element or playback call. `Game.tsx` composes visual and semantic output directly. The following tests cover these paths.

The test coverage is broader than checking for sound files. In `tests/browser-environment.e2e.test.ts`, the audio journey installs throwing replacements for `AudioContext`, `webkitAudioContext` and `Audio`, replaces `HTMLMediaElement.prototype.play`, and replaces `speechSynthesis.speak`. It then loads a fixed board, completes it and restarts. The test also counts media requests and checks for audio or video elements. That combination addresses several ways a web page could make sound without shipping a conventional sound-file directory.

`scripts/source-audit.test.ts` also scans shipped source for known audio constructs. This covers unvisited code but is limited to the patterns in its expression. The browser test exercises actual game paths. Neither kind of evidence alone describes the whole feedback experience. [Source audit notes](source-audit-notes.md) and [Browser test guide](browser-test-guide.md) cover the broader checks.

The complementary feedback evidence includes status mutations in `tests/announcements.e2e.test.ts`, artwork overlap in `tests/matched-mark.e2e.test.ts`, and completed results in `tests/status.e2e.test.ts`. The keyboard journey in `tests/accessibility.e2e.test.ts` checks exposed names and states through Chromium accessibility trees. That test records no screen-reader use; spoken confirmation belongs to [Acceptance](acceptance.md).

## Questions the exploration did not settle

The exploration did not measure comparative task timings, recall or moves. The final game has a clear feedback model, but that does not retrospectively measure the benefit of removing sound.

The exploration did not produce a face-to-sound vocabulary. Reintroducing that idea would involve much more than restoring the sketched outcome cues. It would add another way to encode shape, marker and fill, with its own learning burden and information-equivalence questions. No such mapping is implied by the current face catalogue.

## Where the decision landed

The settled decision is recorded in [No sound](adr-012-no-sound.md) and the current [Sound](sound.md) specification. The present code has no switch or dormant playback path awaiting completion. For engineering work, an unexplained turn belongs with the board, status or session feedback that should explain it. Restoring the discarded prototype is not necessary to make that explanation complete.
