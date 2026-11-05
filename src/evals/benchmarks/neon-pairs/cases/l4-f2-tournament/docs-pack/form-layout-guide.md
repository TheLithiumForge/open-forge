# Form layout guide

This page explains the label, field and action arrangements used by the current controls.

The game has two kinds of control groups: the always-visible deal bar and the text tools inside Reproduce a game. They share native elements and the `.control` style, but their DOM and layout differ. This guide describes those differences so a markup refactor can preserve the actual interaction rather than just one screenshot.

## A map of the current groups

| Group | Markup and layout | Source |
| --- | --- | --- |
| Grid size | Label and select in a flex row | `Controls.tsx`, `controls.css` |
| Deal actions | Two buttons in the outer wrapped flex row | `Controls.tsx` |
| Board code | Form with label above a field/action row | `BoardCodeForm.tsx`, `reproduction.css` |
| Snapshot JSON | Label and textarea in the panel's body grid | `Reproduction.tsx` |
| Snapshot actions | Two buttons in a wrapped flex row | `Reproduction.tsx` |
| Load outcome | Paragraph and conditional Resume in a wrapped row | `Reproduction.tsx` |

Only Board code is wrapped in a form element. The snapshot field is controlled React state in Reproduction, and its buttons explicitly call callbacks. Enter in a multiline textarea remains text editing rather than submitting the snapshot. Enter in Board code invokes the form's submit path.

## Labels are visible and associated

Grid size uses `htmlFor="grid-size"` and a select with that id. Board code uses `htmlFor="board-code"`, and Snapshot JSON uses `htmlFor="snapshot-json"`. The visible text supplies the fields' names; placeholders do not stand in for labels.

Board code also has `name="boardCode"` because its form reads that field through FormData. Its id serves label association; its name serves native form data lookup. Those are different roles, and changing one does not automatically update the other.

The snapshot textarea has no need for a FormData name in its current path because its value is held in React state and passed directly to `loadSnapshot`. Adding a name attribute alone would not turn its surrounding buttons into a form submission. [Native controls guide](native-controls-guide.md) explains the behavioral value of the current element choices.

The labels come from `control-labels.ts`. Keeping visible text in those constants does not change the label/field relationship. A review can inspect both the imported value and the actual id association, because a correct label string next to the wrong id is still a broken field relationship.

## Deal bar: horizontal until it wraps

The outer `.controls` container uses `display: flex`, bottom alignment, wrapping and a `12px` gap. The nested `.size-control` is a centered flex row with its own `12px` gap. On a wide layout, label, select and action buttons appear in one compact band above the statistics.

At widths up to 599px, the outer container stretches its children. The size group takes the full line and distributes its label and select with space-between alignment. The direct child buttons grow equally. The DOM order stays Grid size, New Game, Restart while the layout moves into two lines.

That stability matters for keyboard order. The CSS changes wrapping, not the tab sequence or source order. A visual rearrangement implemented by moving DOM nodes could affect where Tab goes; the present media query does not need to do that.

The group is not a native form. Changing the select calls the controller immediately. There is no Apply button or submission that combines the selected size with one of the deal actions. [Control bar](control-bar.md) documents those callbacks and what they replace.

## Board-code form: one label, one submission

BoardCodeForm renders a grid form with a `6px` gap. Its first child is the label. The next is `.control-row`, a wrapping flex container with a `12px` gap and bottom alignment. Inside are the input and the Load board submit button.

The input has `flex: 1` and `width: 16ch`. The preferred width reflects code-like text, while flex growth gives it available space. The shared control stylesheet sets `min-width: 0` for inputs and textareas so their intrinsic width does not force the page wider than its container.

The button is the form's explicit submit action. Its ref supports successful keyboard submission when the input is replaced. The form prevents normal page navigation, reads `boardCode` and calls the loader. The label stays outside the row so a wrapped button does not detach it from the field's visual group.

On the phone breakpoint, direct child buttons of `.control-row` get `flex: 1`. This selector is shared with the snapshot action row. A wrapper inserted between the row and a button would change this rule's target; a DOM cleanup can therefore have a mobile layout consequence even when desktop placement looks unchanged.

## Snapshot field: a larger editing surface

Reproduction's body is a grid with a `12px` gap and `4px 16px 16px` padding. The Snapshot JSON label and textarea are adjacent children of this grid. The textarea uses five rows, fills its available width and can resize vertically.

Export replaces its value with readable JSON. Import reads the current value. There is no automatic serialization on every game render, so the field can hold a draft while the player inspects cards or changes a deal. Its controlled state is independent of the board-code input's generation-keyed default value.

The two snapshot buttons follow in a `.control-row`. Both are `type="button"`, which makes the action semantics explicit even if the surrounding layout later changes. The current component does not depend on a textarea Enter press to decide which snapshot action to perform.

A manual copy workflow uses native text selection. The card-specific `user-select: none` rule does not apply to this field. Vertical resize and long structured text can make the disclosure tall, but the field remains an ordinary part of document flow.

## Shared control styling

`.control` sets minimum height, padding, border, radius, panel background, text color and inherited font. It is imported by both Controls and the reproduction components. The selectors for button and select add a pointer cursor, while the input and textarea selector handles shrinking.

The shared class does not create a universal component with all semantics and validation built in. A select still has native options, an input still has its own maximum length, and a textarea still has multiline editing. Their association, callbacks and error descriptions remain in the JSX.

`game.css` sets global border-box sizing and a system sans-serif font at `16px` with a `24px` line height. Form controls inherit the font instead of taking a separate browser default. The focus outline is shared through `:focus-visible`, so a form layout also needs enough space for that outline to be seen.

## Outcome text is outside the disclosure

The load notice paragraph and Resume are in `.load-status`, following details rather than living inside `.reproduction-body`. The row has a `12px` gap, wraps and reserves at least `56px` of height. The paragraph grows and has `min-width: 0`.

This placement makes a paused-load outcome usable after the disclosure closes. It also means a selector that searches only inside details will not find Resume or the notice. Tests deliberately inspect the adjacent load-status location. [Error display guide](error-display-guide.md) follows its content and the load buttons' `aria-describedby` relationships.

Turn output is elsewhere, near the board instructions. The layout does not put every changing sentence inside the form body. The distinction is useful when testing a large phone board: result visibility near the board and load feedback near the text tools are separate observations.

## Reflow and review examples

At a narrow width, the page should be read as a vertical flow of labeled groups rather than a fixed matrix of coordinates. The existing wrapped rows and shrinking fields let content determine height. The default board fits the narrow layout, while a large board confines its own overflow to its viewport.

A longer draft can remain in an input even when not all of its characters are visible at once; native text editing can scroll within the field. That is different from making the whole outer page wider. A multiline snapshot can expand or scroll internally without turning its label into a floating overlay.

The equivalent zoom test at `640x400`, device scale 2, checks readable controls, statistics and completion output. It also uses the snapshot tools at that constrained layout. `tests/controls.e2e.test.ts` verifies labels, native limits and selectable export text. Together the sources cover geometry and editing behavior without defining a new generic form framework.

For a proposed name field, these label and native-control patterns are relevant background, but there is no implemented name form to copy. [Text entry policy](text-entry-policy.md), [Name length policy](name-length-policy.md) and [Duplicate names policy](duplicate-names-policy.md) own its separate acceptance rules. The current interface has no name-field placeholder, character counter, error precedence or field order.
