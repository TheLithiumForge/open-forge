# Incident review: the board would not let go of focus

This is an incident review about keyboard focus becoming trapped in the Neon Pairs board and about a related disappearing-control handoff found during the review.

The current implementation leaves native Tab traversal intact.

## The report was about leaving, not entering

The player could enter the board, move between positions, and reveal cards. The failure appeared only when they wanted to reach the controls after playing. Tab continued through cards instead of leaving the board. Shift+Tab took them through the same internal route in reverse. Reloading escaped the situation, but that also discarded the active attempt.

A pointer reviewer did not see a blocker because clicking New Game worked. The first keyboard demonstration also looked successful because it ended after revealing a pair. The trapped player had completed the part the demonstration exercised and then attempted the ordinary next step: leave the board without a mouse.

The impact was a complete interruption to keyboard-only navigation within that session. It was not merely an inefficient route across many targets. The handler consumed the key that should have moved focus beyond the board, so page controls had no normal traversal path. The [board navigation guide](board-keyboard-navigation.md) describes the current intended route.

## How the trap was introduced

The faulty prototype used a single key handler for “move to another card.” It treated arrows and Tab as variations of the same action and called `preventDefault` before deciding what destination to choose. The code was originally meant to avoid hundreds of page tab stops. It instead replaced the browser's exit mechanism with a circular card route.

This was a case where a good local intention hid a bad page-level result. A board with one tab stop is useful. A board that owns Tab after entry is a different behavior. The implementation had conflated reducing tab stops with capturing traversal.

The initial patch attempt during the investigation added special cases for the first and last positions. That made some routes leave the board, but only at particular edges and with direction-dependent behavior. It also made the remembered position affect how far someone had to travel before reaching an exit. The review discarded that approach because native Tab already expresses leaving a group of spatially navigable controls.

## The current boundary is smaller

Today's `navigationTarget` returns a card destination only for unmodified ArrowLeft, ArrowRight, ArrowUp, and ArrowDown. It returns `null` for other keys or modifier combinations. `useBoardFocus.navigate` calls `preventDefault` only when it receives a destination. Tab therefore reaches the browser untouched.

Enter and Space activate native card buttons. They do not need the board to synthesize a second click action. This keeps the activation path shared with a pointer click and limits the board's custom keyboard code to spatial movement. Other browser commands remain available under the scope defined by the input specification.

One card has `tabIndex={0}`, chosen by the remembered focused index. The rest have `-1`. Tab enters at that remembered position; arrows can visit any card; Tab leaves from wherever the player is. There is no special exit card to discover. The [native-controls decision](adr-006-native-controls.md) gives the broader reason for retaining platform behavior where it already fits.

## The second failure had a different cause

During the fix review, an unfinished snapshot exposed a related focus loss. Loading paused progress displayed Resume. Selecting it removed the button while it still owned focus. The browser then had to choose what to do with an active element that no longer existed, and subsequent Tab entry could feel disconnected from where the player had just been inspecting the board.

That was not the original trap: Tab was no longer being captured. It was a handoff problem at a lifecycle boundary. Treating both as “keyboard broken” would have hidden two independent causes and encouraged a single large focus manager to paper over them.

The current `Game.tsx` makes the order explicit. Its resume handler invokes `board.current?.focusRemembered()` before `controller.resume()`. The board focuses with `preventScroll` and performs its own nearest alignment. Only then does the controller clear the imported pause and remove Resume. This retains a meaningful focused element through the transition.

Ordinary New Game, Restart, load, and export actions do not need that same handoff because their control remains present. They keep focus on the action used. The board's remembered position is separately reset on replacement. This is why a general “focus the first card after every action” repair would have caused a new interruption while fixing the disappearing button case.

## Scrolling complicated the apparent result

Once focus was correct, large-board review found another misleading symptom: Tab could return to the correct card but center the board more than expected. A developer inspecting `document.activeElement` would call the result correct; a player looking at a stable part of the board would see a disorienting jump.

Current `useNearestScroll` records the board scroll position. When focus needs nearest alignment, it restores that recorded position before asking the card to scroll into view. This accounts for Chromium's centering during native Tab entry, which can happen before the focus handler runs. The outer page's own scrolling remains the browser's responsibility.

The distinction between owning focus and being visibly located was important to the review. A focus outline clipped by a scroll edge can feel like lost focus even when the active element is right. Current CSS and browser checks cover the full outline with scroll margin rather than assuming button bounds alone are sufficient.

## Evidence that would have caught the original issue

The first missing journey was short: focus Restart, Tab into the board, move a few positions, press Tab, and verify arrival at the next page control. Repeat in reverse with Shift+Tab. It specifically tests an exit from an interior position, where an edge-only workaround cannot succeed by accident.

The second was to import unfinished progress, move board focus while paused, select Resume, and verify that the remembered card owns focus before another action. The current snapshot and focus-entry browser tests contain this shape of check. They also inspect nearest alignment for a far position on a phone-sized board.

Pure keyboard tests remain useful for edge stops and modifier handling, but they cannot by themselves prove native traversal through rendered controls. Conversely, a browser test that only presses arrow keys can miss a trap in a key it never sends. The [browser test guide](browser-test-guide.md) and [regression catalogue](regression-catalogue.md) describe the broader evidence.

## Outcome

The fix removed custom ownership of Tab and added a specific handoff for the disappearing Resume control. It did not make keyboard navigation a second play engine. Movement stayed in board focus state, while card activation stayed in the shared session action.

The incident's useful conclusion is that keyboard completion includes leaving the thing a user has just operated. A board can be reachable, navigable, and still block the surrounding page. The current accessibility journey includes leaving the board and starting another game for that reason. The [accessibility meeting notes](meeting-notes-accessibility.md) revisit that complete path and the limits of automated evidence.
