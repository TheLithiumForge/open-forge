# Ship One HTML File

Decided before the first release.

The build produces one `dist/index.html` with the script, styles and artwork inlined. Players open it straight from disk, and testers can mail it around without a server or a zip.

We tried a normal multi-file Vite build first. Opening it from `file:` broke module loading in two browsers, and the asset folder kept getting lost when people copied the game.

Status: accepted. The single-file plugin is pinned with the other packages.
