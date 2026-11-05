# ADR 019: Use a custom confirmation overlay

This historical record explains the retired div-based confirmation overlay and its original interaction model.

Status: Superseded by [Dialog policy](dialog-policy.md)

## Original context

The early interface discussion included actions that might discard an in-progress choice. A confirmation was intended to keep the player from accidentally losing work while preserving the visible board underneath. At the time, the team chose a custom overlay because it appeared easy to style consistently with the dark panel and card surfaces.

The old decision used a div overlay with `role="alertdialog"`, a focus trap written by hand and two buttons labeled Cancel and Discard. The overlay covered the page while keeping the underlying state in memory. This record describes that historical design; the replacement policy governs current dialog decisions.

## Decision as originally made

Cancel dismissed the confirmation and returned the player to the interrupted activity. Discard accepted the pending destructive intent. The overlay did not itself own the game rules; it held the user's choice about whether the initiating action should proceed. That distinction was meant to avoid modifying the board and then attempting to undo it on cancellation.

The custom trap moved Tab and Shift+Tab among the confirmation's controls. The dialog role and accessible name were intended to make the interruption understandable to assistive technology. Returning focus to the initiating control was part of the original interaction sketch, because leaving focus on a removed overlay would make the next keyboard step unpredictable.

## Reasoning at the time

A div-based overlay allowed the same border, background and spacing choices as the rest of the page. It could be rendered conditionally with React without invoking a browser dialog method. The team expected that only two buttons and a short message would make the focus logic manageable.

An inline confirmation would have avoided trapping focus but would have competed with the board and controls for space. A browser-native confirmation prompt offered limited presentation control. The custom overlay seemed to give a clear pause in the interaction without introducing a separate screen or route.

## Costs that were easy to underestimate

A focus trap is more than cycling between the first and last button in a static example. Controls can be removed, disabled or focused programmatically. Background content must not remain an alternate interaction path. Escape and dismissal behavior need coherent ownership. Styling an overlay is straightforward; duplicating browser dialog behavior consistently is a larger maintenance burden.

The small number of visible controls did not remove these responsibilities. It merely made the first prototype look complete before keyboard, assistive-technology and interrupted-render cases had been explored. The prototype underestimated the maintenance burden of the custom trap.

## Relationship to the present game

`Game.tsx` composes the status, board, controls and reproduction panel directly. Its current new-deal and restart paths do not mount the historical overlay. Reproduction uses a native disclosure, and importing progress uses a session pause with Resume.

[Dialog policy](dialog-policy.md) is the replacement authority. This record supplies historical context for discussions about confirmation behavior and explains why a custom trap may appear in old notes. [Focus policy](focus-policy.md), [ADR 006](adr-006-native-controls.md) and [ADR 043](adr-043-keyboard-parity.md) cover the related concerns without reviving the retired approach.
