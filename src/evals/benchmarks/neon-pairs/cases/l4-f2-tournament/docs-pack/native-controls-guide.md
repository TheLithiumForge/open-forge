# Native controls guide

This page explains the native HTML elements used by Neon Pairs and the behavior they supply to the interface.

The source combines React event wiring with browser controls. It does not draw input controls on canvas or implement a replacement select menu. The resulting code leaves common editing, activation and disclosure behavior with the browser while keeping game decisions in the session and core.

## The current element choices

| Element | Use in the game | Application responsibility around it |
| --- | --- | --- |
| `button` | Cards and actions | Decide what the activation changes |
| `select` with `option` | Grid size | Supply supported sizes and start a deal |
| `label` | Grid size and text fields | Associate visible names with field ids |
| `input` | Board-code draft | Read and validate the submitted code |
| `textarea` | Snapshot draft and export | Hold text and pass it to the snapshot decoder |
| `form` | Board-code submission | Prevent navigation and handle the result |
| `details` with `summary` | Reproduction disclosure | Supply content and layout |
| `dl` with `dt` and `dd` | Current game statistics | Derive the displayed values |

These choices describe the current DOM. Policies for other kinds of content are documented in [Dialog policy](dialog-policy.md) and [Tables policy](tables-policy.md), without implying that those elements already appear in Game.

## Buttons unify activation

Card renders a native button with `type="button"`. Its `onClick` sends an index to the shared activation path. Enter and Space can activate the same control through normal button behavior, so there is no separate keydown implementation of reveal.

The board's keydown handler only handles plain arrows with a navigation target. It does not call activation for Enter or Space and then also allow a native click. That avoids two paths processing one keyboard action. The game's pure activation guard still decides whether the card is eligible.

Action buttons outside the board use explicit types too. The Board code loader is `type="submit"` inside its form. New Game, Restart, Export snapshot, Load snapshot and Resume are `type="button"`. This makes submission intent visible in JSX rather than relying on where the browser happens to find an enclosing form.

A native button's semantics do not automatically implement ARIA-disabled behavior. Card uses `aria-disabled={matched}` while remaining focusable, and the game ignores matched activations. [Disabled state guide](disabled-state-guide.md) explains that deliberate split.

## The select keeps its native interaction

Controls renders a labeled select whose value comes from `game.size`. Each option comes from `GRID_SIZES` and has a numeric side length value with `NxN` display text. The browser supplies its dropdown interaction and option-selection behavior.

The component handles change by converting the selected value to a number and calling `newGame`. There is no local menu-open flag, active-option state or custom pointer hit testing. The controller still checks supported sizes, because native options are an interface aid rather than the only trust boundary.

The select inherits the shared font and panel colors through `.control`. Styling the border and background does not replace its role or its label relationship. The current CSS does not remove it in favor of an invisible field beneath a separately focusable drawn menu.

A successful board-code or snapshot load changes the actual game size, which flows back into this controlled select. The visible selected option therefore follows the model rather than a second UI-only size state. [Control bar](control-bar.md) covers that data flow.

## Text editing remains text editing

Board code is an input, and Snapshot JSON is a textarea. They retain selection, caret movement and browser editing operations. The textarea can resize vertically and receives readable export text without a clipboard permission dependency.

The card's `user-select: none` styling is scoped to `.card`. It does not disable selecting an exported snapshot. The code-field spell-check and autocomplete settings reflect its format-oriented use; the snapshot field disables spell checking as well. None of those attributes validates the entered data.

BoardCodeForm uses FormData and prevents the form's default navigation in `onSubmit`. Pressing Enter in the input reaches that same submission path. The snapshot tools instead use local React state and explicit action buttons, preserving multiline Enter behavior in the textarea.

Controlled and uncontrolled fields both appear here. Reproduction controls the textarea value with state. BoardCodeForm uses `defaultValue` on an input keyed by generation. Native element choice does not determine which React state pattern is appropriate. The intended draft lifecycle does; see [Reproduction panel](reproduction-panel.md).

## A disclosure is already an interaction

The reproduction panel uses details with a summary, starts collapsed and relies on native expansion. There is no handler toggling an `expanded` variable or emulating summary key activation. The CSS styles the panel and summary without changing their source order.

The summary's visible text supplies its purpose. The load notice and Resume live outside the collapsible body, so closing it does not hide the pause continuation action. This is a layout and component-boundary decision around the native disclosure, not a custom disclosure implementation.

A disclosure leaves the surrounding document operable and expands in flow. It is not equivalent to a modal overlay. That distinction matters when reading a feature description: choosing a different native element changes more than a border shape or animation.

## DOM cards and artwork

Board is a CSS grid of DOM buttons, and each visible face is an inline SVG child. Hidden cards omit the face component entirely. The button name comes from position, state and, when available, face description; the SVG is hidden from accessibility and cannot receive focus.

The face drawing is therefore not the input surface's semantic replacement. A user activates the button, not an independently focusable path inside the SVG. This keeps the artwork and interaction connected without requiring canvas coordinate mapping or an invisible parallel button list.

The matched check mark is also an aria-hidden, nonfocusable SVG. It reinforces state visually while the button name and disabled property express it through the control. [Card artwork](card-artwork.md) and [Screen reader notes](screen-reader-notes.md) describe those channels more fully.

## Native elements still need application work

The browser does not know that a pair matches, that a snapshot import is paused or that a game replacement invalidates an old timer. Those decisions remain in `game.ts`, `session.ts` and `use-game.ts`. A correct native element can still call the wrong controller action.

Nor does a button automatically implement the board's remembered entry point. `useBoardFocus` supplies the tab-index pattern and arrows, while the browser supplies basic focus and activation. The scroll helper addresses a specific Chromium centering behavior rather than replacing all browser scrolling.

Labels and descriptions also need correct associations. A visible label next to an input is not sufficient if its `htmlFor` points elsewhere. A load notice remains useful because the buttons reference its actual id. Native semantics reduce custom behavior, but they do not excuse checking the JSX relationships.

The same applies to CSS. Minimum height, shrinking fields, wrapping rows and visible outlines are explicit styles. A native input can still overflow a narrow page if its sizing constraints are wrong. [Form layout guide](form-layout-guide.md) and [Target size policy](target-size-policy.md) cover those aspects.

## Why alternatives would carry more work

Replacing the select with a drawn menu would move option navigation, expansion, selection and focus handling into application code. Replacing summary with a styled generic container would require recreating its activation and expanded-state exposure. Those are concrete costs even if the replacement initially resembles the current design.

A canvas board would also need a separate way to expose each card's position, name, state and focus behavior. The current DOM makes those properties inspectable directly. This is the benefit behind the existing choice, not a claim that native elements alone solve every accessibility issue.

The source does not contain a component library or a custom form abstraction that all new controls must use. Shared labels and CSS are small local tools. [ADR 006](adr-006-native-controls.md) is the decision reference, while [Architecture](architecture.md) explains the separation between browser effects and pure game behavior.

## Evidence in review

The controls tests locate fields and buttons by names and roles, inspect native option values and check selectable export text. Keyboard tests rely on normal Tab exit and native button activation. The accessibility journey captures the browser's exposed control roles and states.

These checks are more informative than asserting only that a div has the right class. They verify the interaction surface that the player receives. At the same time, they remain browser and tree evidence, with spoken confirmation assigned to user acceptance in the repository README.

For a refactor, compare element type, explicit button type, field association, key handling and callback as a connected path. If a visual wrapper is the only intended change, those details reveal whether it also changed submission, tab order or mobile flex behavior. The native structure makes the comparison concrete.
