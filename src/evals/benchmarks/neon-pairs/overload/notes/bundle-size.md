# Bundle Size

Analysis of `dist/index.html` after the artwork landed.

The single file is about 210 KB before compression. React and React DOM are roughly two thirds of it. The card artwork is generated from a few shape and marker functions, so 128 faces cost only a few kilobytes of code instead of 128 separate SVG files.

We looked at Preact to save about 40 KB. The compatibility layer worked, but it added a second set of typings and one more package to pin, and the size never mattered for a game opened from disk.

Status: no change planned.
