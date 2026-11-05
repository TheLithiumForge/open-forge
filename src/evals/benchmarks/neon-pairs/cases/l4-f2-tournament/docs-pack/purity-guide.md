# Purity guide

This page explains the data-oriented core of Neon Pairs and the practical value of keeping rules independent of rendering and browser effects.

The core receives a board code, state, activation index, logical time amount or serialized text. Its results depend on those inputs. React, storage, event listeners and scheduling enter through adapters. This separation supports browser play and command-line replay with the same transition functions.

## Purity is about observable decisions

`createGame(code)` reconstructs the deck and initializes progress. `activateCard(state, index)` decides whether an activation is ignored, opens one card, matches a pair or creates a mismatch. `advanceGame(state, milliseconds)` applies supplied logical time. None needs to know which button was pressed or when a browser timeout was requested.

`isWon(state)` derives victory from matched and deck lengths. `faceUpCards` derives presentation positions from matched positions and selection. These functions avoid a second mutable record of facts that are already available in the state.

The session also contains pure operations. `activateSession` adds feedback and completion preferences to a core transition, while `tickSession` gates a logical advance by generation and pause. Pure does not mean “must be in the top-level game directory”; it means the decision can be evaluated from explicit data without performing an external effect.

## A concrete activation sequence

For `np1-4-1`, positions 2 and 6 both contain face 0. Starting from `createGame`, activating 2 returns a game with `started: true` and a one-card selection. Moves remain zero. Activating 6 returns a game with both positions appended to `matched`, selection closed and moves equal to one.

Activating position 0 and then 1 instead opens a mismatch because the example deck has faces 6 and 7 there. The second activation adds one move and sets `remainingMs` to 800. Activating another position while that mismatch is pending returns the same object.

The same sequence can be used by a pointer journey, a keyboard journey or replay. Their input acquisition differs, but the rule decision does not. A UI-only correction to the move text would leave these other clients inconsistent; a correction in the transition reaches them all.

## Local mutation can produce a pure result

`generateBoard` constructs a local array and swaps elements while generating the seeded deck. That local mutation does not mutate an input deck or expose intermediate state. The completed board description and deck are frozen before return.

Similarly, `readMatched` constructs a local array and map while validating input. Its decision depends on supplied data, and callers do not observe the intermediate accumulation. Purity here does not require expressing every loop as a chain of array combinators.

What matters for shared game state is that transitions do not mutate the state given to them. A new match creates a new matched array; a new selection creates a new selection object. An ignored activation returns the old state. These reference relationships are meaningful to the session commit function and board memoization.

## Readonly types and runtime behavior

The `Board` and `GameState` interfaces expose readonly fields, and arrays are typed readonly. This prevents ordinary TypeScript callers from assigning into them. The generator adds runtime freezing for the board description and deck.

Not every returned progress object is recursively frozen. The code's immutability discipline is expressed through readonly types and functional transitions, with explicit freeze at the stable board boundary. Describing the whole state as deeply frozen at runtime would overstate the implementation.

`faceUpCards` returns a `ReadonlySet<number>` but constructs an ordinary Set internally. The type communicates the consumer contract. `Board` memoizes that derived set rather than storing it in the serialized game. This distinction keeps a convenient rendering index from becoming another authoritative state field.

## Logical time is an argument

`advanceGame` ignores nonfinite or nonpositive deltas and returns unchanged data before start or after victory. For active unfinished games it caps elapsed total and decreases mismatch remainder. An advance that expires a mismatch closes selection in the same call.

Replay uses this property by advancing from the previous timestamp to the next action before activating. Equal timestamps produce a zero advance and preserve array order. A timeout boundary is therefore a numerical question that can be tested without waiting for browser scheduling.

The browser controller supplies measured active deltas through the session clock. Its effect lifecycle determines which intervals are active. The core does not infer hidden periods from a timestamp history or attempt to compensate for browser throttling itself. [Clock design](clock-design.md) follows the adapter side.

## Validation is a pure boundary too

Serialized text is untrusted input even when produced by another copy of the game. `parseJson` returns unknown data or an undefined malformed marker. `isJsonObject`, integer checks and exact-key checks narrow it before any state is installed.

The snapshot decoder checks the code, bounds, matched positions and selection against a newly reconstructed deck. It checks a minimum move count and whether an unstarted state contains impossible progress. A valid result is a new `GameState`; an invalid result is null. A failed decode does not partially modify the current browser game.

This validation is structural and domain-specific, not a reconstruction of every possible prior activation history. For example, the decoder checks lower bounds on moves and relationships between start state and progress; it does not replay a missing action log to prove that a particular elapsed value occurred. The format contains a state, not that history.

`saved-format.ts` uses `TextEncoder` to measure UTF-8 bytes. This is a standard facility available in both the browser and the Node environments used here. It does not read browser state, storage or the DOM. Treating every standard host-provided constructor as a browser effect would obscure the actual independence these modules need.

## Invalid arguments have explicit outcomes

Different APIs serve different callers. `boardCode` and `generateBoard` throw on invalid board descriptions because their constructors expect a valid input. `parseBoardCode` returns null, providing a nonthrowing validation boundary. `decodeSnapshot` and `replayGame` return null for invalid text, while `readReplay` exposes a structured problem.

`activateCard` instead ignores invalid indices and illegal attempts by returning its input state. That is suitable for the activation path, where stale or repeated input should not corrupt progress. `advanceGame` similarly ignores unsuitable amounts and inactive states.

These are existing contracts, not a universal rule that pure functions never throw. A function can be deterministic and still reject an argument with an exception. The important review question is whether its caller expects an exception, a null result, a problem value or unchanged state.

## Small shared helpers have real clients

`saved-format.ts` holds object narrowing, safe nonnegative integer checks, exact-key matching, byte measurement and JSON parsing. Snapshot, replay and preference validation actually share those operations. Their domain checks remain in their own modules.

A helper such as `isNonnegativeSafeInteger` cannot by itself decide whether a number is a legal move count. Snapshot adds the move cap and consistency lower bound; preferences add the board's minimum completed moves; replay uses it for timestamps and positions before applying its own range checks.

Keeping those checks explicit prevents a superficially reusable “validate number” abstraction from hiding different meanings. The architecture specification asks for shared support when two real behaviors need the same meaning, and the current helper file is small enough to inspect against its callers.

## Rendering consumes outcomes

Components derive visible state from the returned game rather than reading classes back into the rules. Focus stays in the board, and storage writes follow preference decisions through a hook. [React boundaries](react-boundaries.md) and [Session module](session-module.md) explain those adapters.

## Evidence made possible by this shape

Literal decks, direct transition tests, malformed-format cases and numerical clock steps can be checked without rendering. Browser journeys then address native input, focus, storage and delivery. [Test policy](test-policy.md) explains why these are complementary forms of evidence.

## Reviewing an accidental effect

A rule that imports a component, reads a DOM element or performs storage access would bring environment state into a computation currently driven by arguments. The source audit scans selected core filenames for known effect markers, but its list and regular expressions are limited. Reading the imports and operations remains part of review.

A less obvious problem is mutating an input array while returning a new outer object. That can preserve or corrupt references in ways memoized consumers do not expect. The existing transitions instead create changed collections where their meanings change and keep stable collections where they do not.

A useful explanation of a core change can therefore state its input, output, ignored cases and retained references. If that explanation requires a mounted component to define the result, it is probably describing an adapter concern or an incomplete rule boundary. [Module layout policy](module-layout-policy.md) connects that distinction to file placement.
