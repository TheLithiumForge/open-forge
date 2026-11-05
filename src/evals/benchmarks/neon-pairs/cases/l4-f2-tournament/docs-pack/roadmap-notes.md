# Roadmap discussion notes

These are planning notes about possible directions for Neon Pairs, with ideas recorded as discussion rather than delivery commitments.

The present contract remains [Rules](rules.md), [Controls](controls.md), and [Architecture](architecture.md).

## Starting point

Spec 2.2 already covers a complete local game: seven sizes, reproducible deals, explicit progress transport, developer replay, and an offline HTML build. The planning discussion began with what could be learned from that game rather than assuming another menu was the next useful result.

No dates were attached. Several investigations could reasonably end with better evidence or explanation and no code change.

## Discussion board

| Topic | Useful question | Existing context |
| --- | --- | --- |
| Browser evidence | Which additional environment would stress a boundary we do not understand? | The README records one named runtime combination. |
| Explanation | Where do people confuse a pair attempt, a repeated deal, and saved progress? | Playtests surfaced these distinctions. |
| Dense artwork | Is there a specific pair of faces that needs closer human comparison? | The full face catalogue has a minimum-size diagnostic sheet. |
| Reproduction | Do examples help a reporter choose a code, snapshot, or replay? | The three artifacts answer different questions. |
| Local records | Do users understand why another origin can show different saved data? | Preferences are local and progress export is explicit. |

These questions are candidates for investigation, not approved feature scope. They are deliberately narrower than broad labels such as “mobile improvements” or “better sharing,” which can hide several unrelated changes.

## Evidence still worth understanding

The README records Node, npm, Playwright, and Chromium versions and says what passed on that combination. A supported install elsewhere does not automatically create another result record. The current suite already covers scrollbar differences, touch scrolling, equivalent zoom layout, and the keyboard accessibility tree in Chromium.

Physical long press and spoken screen-reader output remain separate acceptance activities. [Browser support](browser-support.md), [phone research](research-phone-usage.md), and [screen-reader notes from research](research-screen-reader-sessions.md) retain the actual limits.

A possible investigation would identify the value of another environment before treating its name as a compatibility promise. No additional support guarantee was adopted during the review.

## Explanation before interface expansion

Three recurring questions were easy to state: why does the first reveal add no move, which button repeats the arrangement, and why does a board code not retain matches? Better examples may help.

The discussion left interface wording unchanged. [Gameplay FAQ](gameplay-faq.md), [new-game behavior](new-game-and-restart.md), and [sharing FAQ](sharing-faq.md) already provide space for detail without expanding the primary play surface.

Likewise, reproduction convenience was left open. Automatic copying, file import, and a replay panel were discussed, not selected. Manual text has useful current properties: drafts stay visible, validation precedes replacement, and no clipboard permission is needed. Version 1 remains the format for all three reproduction artifacts under spec 2.2.

## Retired is a useful status

Hints raise questions about disclosed information and assisted results. Themes range from surface colors to replacing the recognition vocabulary. Timed mode adds a new objective and deadline ordering. Connected play adds service, trust, and synchronization questions.

The [hint](retired-idea-hints.md), [theme](retired-idea-themes.md), [timed-mode](retired-idea-timed-mode.md), and [connected-play](retired-idea-online-play.md) notes preserve those distinctions. Their presence does not reserve future state branches or dormant controls. Returning to one would be new design work against the actual current game.

The planning discussion also left automatic active-game restoration and device synchronization unselected. A reload currently starts a fresh deal; local preferences and manually copied progress have separate lifetimes. [Persistence overview](persistence-overview.md) explains that boundary.

## Maintenance notes left in the margin

- A demonstrated marker overlap is concrete artwork work; it does not require a theme system.
- A confusing report may need a better example rather than a new export feature.
- A local behavior that grows can be split locally; retired ideas do not justify a framework in advance.
- A new test record should describe what actually ran.

The [architecture review](meeting-notes-architecture.md) connects these observations to current source boundaries. The next task and delivery date remain undecided.
