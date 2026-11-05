# Spacing scale
This page maps the spacing values in the visual specification to the margins, padding and gaps in the current stylesheets.

V09 in [Visuals](visuals.md) gives the positive layout values 4px, 6px, 8px, 12px, 16px, 24px and 32px, with zero resets and automatic centering allowed. Native-control internals are outside that rule. Borders, radii, outline geometry, font metrics and SVG coordinates have their own values. The distinction is useful because the source contains many lengths that are not layout spacing.

## What the scale covers

The scale applies to authored gaps, padding and margins. It is not a statement that every CSS number is a multiple of four. The 6px board gap is an explicit value, and the 2px card border is not a gap. The 44px minimum card size and the 56px feedback minimum height are dimensions rather than margins or padding.

The implementation writes the lengths directly in component stylesheets. There is no shared spacing-token file. The board does define local variables for its gap and padding because the grid-width calculation uses those values too. [Theme tokens](theme-tokens.md) distinguishes those local variables from the literal spacing elsewhere.

A quick source audit usually starts with `game.css`, `controls.css`, `control.css`, `reproduction.css`, `status.css`, `board.css` and `card.css`. Looking only at the board misses the reproduction form, which has several nested spacing relationships and appears in expanded and paused-load states.

## Inventory by component

| Area | Declaration | Value |
| --- | --- | --- |
| Page | `main` padding | 16px on all sides |
| Header | Margin | 4px top, zero sides, 12px bottom |
| Title | Margin | 4px vertically, zero horizontally |
| Intro | Bottom margin | 24px |
| Help | Bottom margin | 12px |
| Top controls | Gap and bottom margin | 12px gap, 16px margin |
| Size group | Gap | 12px |
| Native control class | Padding | 8px vertically, 12px horizontally |
| Stats | Gap and bottom margin | 12px rows, 24px columns, 16px bottom |
| Feedback slot | Margin | 4px top, 8px bottom |
| Board viewport | Padding | 6px |
| Board grid | Gap | 6px on both axes |
| Card | Padding and scroll margin | Zero padding, 6px scroll margin |
| Victory paragraph | Padding and bottom margin | 12px by 16px, 12px bottom |
| Reproduction panel | Top margin | 24px |
| Reproduction summary | Padding | 12px by 16px |
| Reproduction body | Padding | 4px top, 16px sides and bottom |
| Reproduction body | Grid gap | 12px |
| Board-code form | Grid gap | 6px |
| Reproduction action row | Flex gap | 12px |
| Load-status row | Gap and margin | 12px gap, 4px top and 8px bottom |

The value 32px belongs to the allowed spacing set, but the production styles in this inventory do not use it as a positive layout margin, padding or gap. The title's 32px line height is a font metric, not evidence of a 32px layout gap. A scale can contain a value that the current screen does not need.

## Resets are part of the layout

The body margin is zero. Paragraphs have margin zero globally, the title receives explicit margins, and `dd` has margin zero. The stats list has an explicit `margin: 0 0 16px`. Without these declarations, browser defaults would add space that the component inventory does not explain.

The rules do not reset every browser-internal box. Options and optgroups remain native, and the spacing test explicitly excludes their internals. A select popup is therefore not a reason to alter the game's layout scale. [Native controls guide](native-controls-guide.md) describes why those elements retain browser behavior.

Automatic horizontal margins center `main`. They are not a fixed length to replace with a scale value. Similarly, normal gaps on elements that are not grid or flex containers can appear in computed styles without representing authored layout spacing.

## Vertical rhythm in the actual document

`Game.tsx` places the header before the intro, controls, stats, optional victory, help, feedback, board and reproduction content. That order explains why spacing should be examined in the completed and loaded states as well as the initial one. The victory paragraph is inserted into normal flow, so its padding and bottom margin increase the distance above the board.

The feedback area's 56px minimum height reserves room for short turn results. Its 4px and 8px margins are separate from that minimum. Text can increase the height when wrapping; there is no fixed-height clipping rule. This keeps a longer result from overlapping the board while maintaining a relatively stable short-result slot.

Margins do not always sum as independent strips. Block margins can collapse in ordinary flow, while flex and grid gaps do not collapse. A pixel estimate made by adding every listed margin can therefore differ from the actual bounding-box distance. The inventory tells where space is authored; browser measurement tells the resulting layout.

Inside the reproduction panel, the summary's bottom padding and body's 4px top padding are distinct surfaces. The body is a grid with a 12px gap, and the board-code form is a nested grid with a 6px gap. A form label-to-input distance is consequently not the same relationship as the distance between the textarea and the action row.

## Two-axis board spacing

Six-pixel grid gaps separate adjacent square cards. Six-pixel viewport padding separates the edge cards from the scroll boundary. These are different lengths with the same value. Removing one to compensate for the other would change either the grid's minimum width or the edge clearance.

The card has a 6px scroll margin. This property does not add space between cards or enlarge the grid. It influences where scrolling places the target. The focus ring extends 5px beyond the card box, from its 3px width plus 2px offset, so the chosen margin gives it room near a scroll edge.

At the beginning and end of the grid, physical viewport padding provides clearance. At an interior position reached through nearest scrolling, scroll margin is the relevant protection. `tests/layout.e2e.test.ts` deliberately navigates to an interior card for this reason. A test of only the first card would not demonstrate the interior scroll behavior.

The one-pixel viewport border, two-pixel card border and eight-pixel card radius affect visible geometry but are outside the spacing rule. [Layout grid](layout-grid.md) includes borders in width calculations instead of treating them as scale-controlled padding.

## Responsive rows

The top control bar wraps at its base style. At narrow widths, the size group occupies the full row and buttons flex into the remaining rows. The 12px gap remains the authored space between flex items. It does not need a special per-phone gap to make the row fit.

Stats similarly wrap with different row and column gaps. Using 24px horizontally separates the labeled values, while 12px vertically avoids an unnecessarily tall stack. These are two dimensions of a single `gap` declaration. A computed-style audit sees `row-gap` and `column-gap` separately.

Reproduction rows wrap, and the input can shrink because the shared control class gives inputs and textareas `min-width: 0`. The load notice paragraph has its own zero minimum width. These width declarations are not spacing exceptions; they let the existing gap and padding values work at reduced widths.

## The browser spacing audit

`tests/spacing.e2e.test.ts` defines `SCALE_PX = [0, 4, 6, 8, 12, 16, 24, 32]`. Its `offScale` helper walks the body and descendants, skips elements inside options and optgroups, and examines every side's margin and padding plus row and column gaps. It accepts `auto`, `normal` and numeric values in the set.

The test runs at the phone fixture and 1280x800. It finishes the board, opens the reproduction panel and checks that state, then loads an unfinished snapshot and checks again with the Resume button and notice present. That coverage is broader than auditing only the first visible screen.

The helper uses computed styles, so it can catch a browser default that survived the resets or a value introduced through a more specific selector. It does not inspect every conceivable geometric property. A passing scale audit cannot prove that a board fits horizontally, a focus ring is visible, or a text line avoids overlap. Those are covered by layout, focus and readability checks.

For a spacing report, the affected relationship is more useful than a proposed replacement number. “The Resume row pushes the panel beyond the phone width” points toward wrapping and minimum width; “the title is too close to the eyebrow” points toward header and title margins. The existing source and [Visual review checklist](visual-review-checklist.md) give concrete places to verify each observation.
