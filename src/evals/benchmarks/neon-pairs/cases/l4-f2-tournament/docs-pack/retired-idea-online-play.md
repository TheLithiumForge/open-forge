# Retired idea: connected play

This is a design note about online play considered for Neon Pairs and set aside in favor of the current local, offline game.

The game has no accounts, remote session, shared room, score service, or network synchronization. Sharing a reproducible deal does not require those services.

## Sharing a deal looked close to playing together

The proposal began after board codes made it easy for two people to load the same arrangement. If the starting cards could be shared as text, perhaps a small status panel could show another person's progress. That sounded like one step from the current game.

In fact, the current code solves only the local reproduction part. A code is interpreted by a fixed generator on each device. It does not identify a person, find another browser, carry messages, or establish which actions should be trusted. Two local attempts on the same code can be compared informally without any connection between the apps.

This distinction was useful because it preserved the value of board codes even after the connected feature was retired. The [board-code format](board-code-format.md) remains a compact way to repeat a starting arrangement, not a room address or invitation token.

## Three different proposals had been combined

One sketch was asynchronous sharing: send a code and later exchange results. Another was live observation, where one player saw someone else's pair count. A third let two people act on the same board. These have different failure modes and service needs.

Asynchronous sharing can happen outside the app using current local artifacts, although the app does not verify the exchanged result. Live observation requires transport, session identity, and a view of remote progress. A jointly controlled board requires agreement about action order and who may act when. Calling all three “online play” hid the design decisions that made them distinct.

The team did not choose a message protocol, room format, or hosting provider. Those would have been new implementation commitments for a feature the project had not approved. Retirement left the current app's simpler service boundary intact.

## A remote action is not just another click

Current `activateCard` processes one index against one state. In a single browser, the session controller synchronizes active time, applies that action, and commits the result. In a shared board, two remote actions may arrive in a different order from the order in which people performed them.

Suppose one client reveals a first card while another reveals a different one. Are those the two halves of one attempt, independent local attempts, or conflicting actions? During a mismatch, are remote activations ignored at the sender, receiver, or authoritative coordinator? The existing local transition function can be reused in a design, but it cannot answer who owns the state.

The review also considered what happens after disconnection. Showing an old board as if it were current would be misleading. Allowing local play and merging later would require a definition of conflict and replay. Blocking play would change the offline expectation. None of these was adopted as a hidden fallback behavior.

The [session module](session-module.md) explains the current single-browser controller. It has a generation number for invalidating old local callbacks, not a distributed revision or participant identity.

## Time would need another meaning

The current game counts active milliseconds while the page is shown and an unfinished imported state is not paused. A remote comparison introduces questions about different visibility periods, delayed messages, and whose clock decides when a miss ends.

One sketch used each client's local active time. That preserved familiar behavior but made a shared deadline or synchronized view difficult to explain. Another used a server timeline. That could order remote actions but would no longer match the existing pause model automatically. A third sent only final counters, which reduced synchronization work while removing any basis for verifying them.

These are product choices as well as technical ones. A local monotonic clock solves elapsed observation in one browser; it does not synchronize devices. The [clock design](clock-design.md) and [replay format](replay-format.md) document the current logical timeline without promising a network clock.

## Local data was not proof of a result

Board codes, snapshots, and replays are all supplied local data. Validation catches malformed or inconsistent states and actions, but it does not authenticate a person or establish that a human played the sequence unaided. A user can inspect the deterministic generator and compute the entire deck outside the game.

For the current product, that is acceptable. Reproduction helps debugging and local use. A remotely trusted score service would need a different threat and trust model, including decisions about what it could honestly claim to verify. Merely moving the same JSON to a server would not make the result reliable.

The review rejected a mock “verified” badge that did nothing beyond successful parsing. It would have implied more evidence than the format supplies. No replacement verification rule was adopted. The [import-safety page](import-safety-policy.md) describes current validation as validation, without elevating it to authentication.

## A service changed the delivery promise

The current distribution opens from a single HTML file without a network request. It uses local native controls, inline artwork, and system fonts. There is no account setup, API key, analytics, or remote dependency during play.

A connected mode could coexist with offline play in a future product, but it would add operational work: service availability, version compatibility between clients and server, error states, and decisions about retained data. Those costs are not represented by adding a dependency to the browser package alone.

The architecture review preferred not to create a partially implemented service shell around a game that already worked locally. The [no-backend decision](adr-004-no-backend.md) and [offline delivery page](offline-delivery.md) explain the present boundary.

## What survived the discussion

The useful surviving idea was better local evidence. A board code names the deal. A snapshot captures current progress. A replay can explain a timed sequence to an engineer. Those artifacts can be copied through whatever communication channel people already use, without making Neon Pairs responsible for that channel.

This is not described as an online feature in disguise. The app does not send the artifacts, list recipients, or track another player's progress. It simply makes local state reproducible. The [sharing FAQ](sharing-faq.md) covers these existing capabilities.

The idea was retired because live connection would have required a new product and service design, while the immediate need was understandable local play and reliable issue reproduction. [Roadmap notes](roadmap-notes.md) retain the distinction between discussion and commitment. No account screen, remote message type, or future compatibility promise was reserved in the current implementation.
