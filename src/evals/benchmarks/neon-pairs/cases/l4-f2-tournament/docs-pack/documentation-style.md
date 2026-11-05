# Documentation style

This guide explains how to write and maintain Neon Pairs documentation so that a reader can connect an explanation to the implemented behavior.

The documentation serves engineers and coding agents working on a small browser game. Its useful detail comes from actual transitions, source boundaries and examples, rather than a formal tone. A reader should be able to find the relevant behavior, understand its limits and decide what evidence would support a change.

## Start with the page's job

The existing specification pages open with a title and a sentence identifying the document. The rules page says it is a game contract; architecture explains the separation between rules and browser effects; acceptance identifies itself as journeys that check requirements. Those opening sentences help a reader decide whether to continue or follow a related page.

A useful explanation then starts with the behavior a person can observe. For a mismatch, that is two different faces staying visible before they close. The next paragraph can explain active time and the session adapter. Starting with the adapter's hook names would make the reader reconstruct the product behavior from implementation details.

Different page types need different shapes. A glossary works as entries, a format page benefits from tables and literal examples, and an incident needs a sequence of cause and consequence. A short decision can be shorter than a reference page. Uniform headings across all of them would not make their contents more useful.

[Rules](rules.md), [Controls](controls.md), [Input](input.md) and the other subject specifications define the product. A sentence in an explanatory guide can clarify an existing constraint without quietly expanding it.

## Give each claim the right status

There are three common kinds of statement: what the current game does, what the specification defines, and what someone proposes. They can appear together, but their wording should let a reader distinguish them. “The component renders” identifies an implementation observation. “R04 defines” identifies a requirement. “A possible follow-up” identifies a proposal.

For example, `src/game/snapshot.ts` exports `SNAPSHOT_VERSION = 1`, while the specification identifies itself as version 2.2. An accurate sentence names both subjects. “The current version is 2.2” is ambiguous on a page about JSON because a reader might change an exported format field on that basis.

History can explain a decision, but it should not replace a current behavior description. A release discussion about clock drift is useful context for the measured-delta design. A reader still needs the present boundary: pure game functions receive durations, while the browser session supplies them. [Clock design](clock-design.md) is a better destination for that explanation than an unrelated release note.

## Trace a behavior through its layers

Source references are most useful when they explain responsibility. `src/game/game.ts` owns card transitions and logical time. `src/game/session/session.ts` wraps them with feedback and preferences. `src/game/session/use-game.ts` connects loading, exporting and React state. `src/game/Game.tsx` composes the screen. A page about a turn can begin in the core and follow only the layers relevant to its question.

The same discipline helps avoid false shortcuts. Saying “React counts a move on click” combines rendering, input and rules into one inaccurate sentence. A second eligible activation increments the counter in `activateCard`; browser controls route input to the shared action. That description also explains why keyboard and pointer input have the same move behavior.

## Examples answer a particular question

The code `np1-4-1` is a good common example because the specification publishes its deck. Positions 0 and 11 form one pair, while positions 0 and 1 differ. Those facts let a writer explain a match or a miss without depending on an unknown fresh deal. A precise example identifies whether its numbers are positions, face IDs or human rows and columns.

A snapshot example answers a state question: what progress will load? A replay example answers a sequence question: which activations and timing lead to a result? Substituting one for the other can omit the very detail a report needs. [Sharing FAQ](sharing-faq.md) explains the choice, while the format pages define acceptable data.

Complete format examples can be copied and checked. Fragments can be clearer when discussing just one branch, provided the prose identifies them as fragments. An object showing only `kind` and `index` is a selection example, not an entire snapshot. Calling it a snapshot would imply that the decoder accepts missing top-level fields.

Placeholders also need context. In a sentence explaining a completion message, `N` can stand for a move count. Inside a JSON block intended as valid input, an unquoted `N` is not a valid number. The difference is about the example's purpose, rather than a preference for more or less code on the page.

## Explain the boundary that changes the answer

“The cards close after a delay” is enough in a quick introduction. An engineering explanation needs the active-time distinction because a hidden page or paused import changes when the interval finishes. “The game saves locally” needs a similar boundary: preferences and best moves persist, while ongoing progress is restored through explicit snapshot tools.

There is no need to repeat the complete timing or persistence model every time those subjects appear. A sentence can name the relevant condition and link to [Mismatch interval](mismatch-interval.md) or [Persistence overview](persistence-overview.md). Repetition is particularly risky for numeric limits and exact strings, which can become inconsistent when copied into many pages.

The best edge case is one that clarifies the rule being explained. Repeating the first revealed card demonstrates why activation is not the same as a move. A stale timer after Restart demonstrates why session generation exists. An unsupported side length demonstrates board-code validation. Adding an unrelated browser limitation to each of those examples would distract from the point.

## Evidence has a scope

Reading `formatElapsed` establishes how the formatter computes text. A browser test establishes how the status component exposes that value in a built page. A screen-reader journey involves another layer of evidence. These observations support different claims and do not automatically stand in for one another.

The repository README records a named runtime combination and says that spoken screen-reader output was not run. That is a useful model of a bounded evidence statement: it reports what was checked and leaves the unperformed part visible. A documentation edit alone does not supply a new test run or expand the tested browser matrix.

For command instructions, `package.json` is the source for the available scripts. `npm run test:e2e` builds before executing the browser journeys; `npm run build` type checks before Vite emits the HTML. Describing the command's defined behavior is different from claiming it passed in the writer's environment. [Test strategy](test-strategy.md) covers the larger evidence plan.
