# Render Cards as DOM Buttons

Decided in week 2, after the canvas spike.

We render each card as a native button with inline SVG artwork, not as shapes on a canvas. A canvas board drew faster on the 16 by 16 board, but every card then needed hand-made focus, hit testing and accessible names. Native buttons gave us keyboard focus, screen reader names and pointer handling for free.

The cost is a larger DOM on big boards. Mounting a face only while its card is revealed keeps that in check, as the large board analysis shows.

Status: accepted. Revisit only if a board larger than 16 by 16 is ever planned.
