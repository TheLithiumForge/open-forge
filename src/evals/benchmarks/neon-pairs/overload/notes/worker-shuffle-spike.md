# Spike: Shuffling in a Web Worker

A half-day spike to see whether deck generation should leave the main thread.

Generating and shuffling the 256-card deck takes well under a millisecond, even on the old phone. A worker added message passing, a second build entry and trouble with the single-file build, for no visible gain.

Result: not worth it. Deck generation stays a pure function on the main thread.
