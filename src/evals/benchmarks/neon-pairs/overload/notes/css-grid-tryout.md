# Tryout: CSS Grid for the Board

What we tried while building the layout, and what happened.

A flexbox board with wrapping cards left ragged last rows on some widths and needed width math in script. A CSS Grid with a fixed column count per board size and cards sized with `clamp` gave square cards and even rows on every width we tried.

Worked: a grid with `aspect-ratio: 1` on cards. Didn't work: percentage widths in flexbox, which rounded to uneven gaps at 14 by 14.
