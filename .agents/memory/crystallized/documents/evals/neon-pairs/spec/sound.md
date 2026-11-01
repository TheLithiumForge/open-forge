---
open-forge:
  description: "Game sound behavior"
  tags: ["LoadNow", "Game", "Sound"]
---
# Sound
Sound is a game requirement that keeps Neon Pairs silent.

Status: spec version 2.1.

You can play in a shared room without starting audio. Card feedback and completion are conveyed by the game display and accessible announcements.

## Playback

- S01 - Keep the game silent for every action and state, including a reveal, a match, a mismatch, a restart and a win.

- S02 - Include no audio files, audio API calls or sound toggle in this version. Silence is the required behavior, not an unfinished feature.

## Feedback

The game communicates card states and completion through its [visual design](visuals.md). Turn results also use the [input announcements](input.md#announcements), so playing doesn't depend on hearing an event.

The complete game pack starts in [Neon Pairs](README.md).
