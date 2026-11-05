# Typography
This page describes the system typography, text hierarchy and numeric layout currently used by Neon Pairs.

The game uses `system-ui, sans-serif` rather than a downloaded font. Body defaults, title sizing and supporting text styles are in `src/game/game.css`; controls inherit the font in `controls/control.css`, and status details live in `status/status.css`. The visual specification's type values are in [Visuals](visuals.md) V08.

## The root style

`:root` declares a 16px font size and a 24px line height. The family is `system-ui, sans-serif`. The root foreground is `#f4f6ff`, and the dark background is `#090b14`. There is no font-face declaration in these styles and no separately authored font family for numbers or controls.

The stack delegates the actual face to the platform. The same 16px text can have different glyph widths on two operating systems. That is expected from the declaration; a screenshot cannot establish a single universal width for every string. The source relies on wrapping and native layout rather than positioning text at hand-tuned coordinates.

The root line height is an absolute 24px length. Elements inheriting it keep that line height even if their own font size is changed to 14px. This is distinct from setting a unitless 1.5 line height, which would scale with a smaller font size. The distinction explains the relatively open leading in the supporting text.

## Type hierarchy in use

| Content | Font size | Line height source | Color |
| --- | --- | --- | --- |
| Main text and labels | 16px | Root 24px | Main text |
| `h1` title | 24px | Explicit 32px | Inherited main text |
| Eyebrow | 14px | Inherited 24px | Secondary |
| Intro | 14px | Inherited 24px | Secondary |
| Board help | 14px | Inherited 24px | Secondary |
| Stat labels, `dt` | 14px | Inherited 24px | Secondary |
| Stat values, `dd` | Inherited 16px | Inherited 24px | Main text |
| Native `.control` text | Inherited through `font: inherit` | Inherited | Main text |
| Completion paragraph | Inherited 16px | Inherited 24px | Main text |

The title rule sets size, line height and margins; it does not explicitly set a weight. The heading's browser-provided weight still participates. The eyebrow is a paragraph, not another heading or a small-caps font treatment.

The load notice and ordinary game feedback are paragraph text at the inherited main size. They are not styled with the smaller secondary-text classes. That makes them visually different from standing instructions even though all are short sentences near controls or the board.

## Header and instructions

`Game.tsx` starts with the eyebrow “A little focus. A perfect pair.” and the `Neon Pairs` title. The intro below gives the basic matching instruction. The help paragraph closer to the board explains pointer entry, keyboard movement and activation. These are separate reading tasks, and their layout positions reflect that separation.

The help class has `max-width: 70ch`. This limits its measure on wide layouts without fixing the width of the board. A `ch` is a font-relative unit related to the zero glyph's advance; 70ch is not a guarantee that every line contains exactly seventy characters. The active system font and the sentence's glyphs determine the actual break.

Paragraph margins are reset to zero. The intro and help receive explicit bottom margins, while the title has 4px vertical margins. Their typography and surrounding space are therefore authored through different declarations. [Spacing scale](spacing-scale.md) is the reference for the space rather than assuming the type sizes imply a default paragraph rhythm.

## Labels and values

`GameStatus.tsx` renders a definition list. Each stat is a wrapper containing an inline `dt`, a separating space and an inline `dd`. The label is smaller and secondary-colored; the value is main text. The CSS does not turn the list into a four-column table or absolutely position values under headers.

The wrapper has a minimum width of 8ch and the list uses wrapping flex layout. This gives each label/value group room while allowing groups to move to later rows. The smaller label and larger value still share a line because the elements are inline. Their baseline alignment follows normal inline text layout.

`dd` declares `font-variant-numeric: tabular-nums`. In fonts that implement it, digit advances line up consistently. This reduces width changes as displayed numbers change. It does not make all letters or punctuation fixed-width, and the property is not applied as a global monospace treatment.

The best-value fallback is the word `none`, so a status review needs both a numeric best and the initial fallback. Pair progress contains a slash between counts. The time value comes from `formatElapsed`; its behavioral details belong in [Elapsed-time rules](elapsed-time-rules.md). Typography only determines how that supplied text participates in the status line.

## Native controls inherit deliberately

`.control` uses `font: inherit`, together with main text color, panel background, border and padding. This shorthand carries the parent's font properties into the button, select, input and textarea instead of leaving a small browser default font. The summary element separately declares `font: inherit`.

The board-code input is 16ch wide before flex distribution and shrinkage. That width is a layout hint tied to font metrics, not an indication that the field uses a monospace font. The snapshot textarea likewise inherits the system font. Its five rows and full width come from component props and CSS; the game does not present it as a code editor with a separate type scale.

Browser-owned select options and internal control affordances can still differ by platform. The root dark color scheme informs their rendering. An enlarged operating-system font or a wider native select label is a reason to inspect flex wrapping, not evidence that the project bundles a different control font. [Native controls guide](native-controls-guide.md) covers those controls more broadly.

## Cards contain geometry, not visible labels

A game card's face uses SVG geometry. It has no printed number, emoji or visible face name inside the button. `aria-label` names the position, state and visible identity for assistive technology, while the inline artwork is hidden from the accessibility tree. [Card artwork](card-artwork.md) explains that markup boundary.

This choice removes font substitution from the identity drawing. A plus face is the authored plus path, not a font's plus glyph. The chevron and check likewise have fixed SVG paths. System-font changes can alter surrounding labels without changing the pair geometry. [Icon and glyph policy](icon-and-glyph-policy.md) discusses that distinction.

The development face sheet adds captions under the actual cards. `figcaption` is 14px secondary text with a 72px minimum height, and the figure layout has 124px columns. Those captions support inspection; their dimensions do not apply to the production board. The card itself remains the same component used in the game.

## Reflow and completion

No type rule sets a one-line clamp or text overflow ellipsis for the control, status or completion text. The completion paragraph has normal wrapping and grows with its content. The feedback and load-status containers have minimum heights rather than fixed heights, so a longer result can increase the occupied space.

At the equivalent zoom layout, the application has a 640px CSS viewport and device scale 2. Its typography keeps the same declared CSS sizes while the available line width is smaller. Controls wrap, stats groups wrap, and sentences can span more lines. The board can scroll separately. [Zoom behaviour](zoom-behaviour.md) documents the representative journey.

A 14px declaration does not guarantee readable output if the element is clipped, hidden behind another box or moved outside the viewport. Conversely, larger text does not require all content to fit in one screen height. The current acceptance evidence focuses on readable, non-overlapping text within the page width and reachable controls.

## Existing type checks

`tests/style.e2e.test.ts` checks the root size and line height, title size, help and intro sizes, and their colors. The product-contract test checks all four secondary roles: eyebrow, help, intro and stat labels. This catches a class that inherits the wrong size even when another secondary class is correct.

The zoom test checks the outer boxes of controls, stats and victory text after completion, and invokes `expectReadableText` in several load states. That helper measures text-node ranges, skips native option text, and detects out-of-width rectangles and overlap between separate text sources. It checks rendered text rather than relying on a container's nominal width.

These tests establish the declared hierarchy and representative rendered behavior. They do not claim visual equivalence among system fonts or measure how every reader experiences them. For a concrete regression, the useful evidence includes the exact text, viewport, platform font environment and open or loaded state. [Visual review checklist](visual-review-checklist.md) connects those details to the existing checks.
