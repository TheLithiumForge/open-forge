# Indexes

## Essence

Indexes define what installed `_*.md` files should do.

An index file helps agents see what a folder contains without reading every file.

## Use When

- A folder contains multiple markdown files agents may need to choose between.
- The folder is part of the Open Forge routing model.
- A generated list of entries helps navigation.

## Do Not Use When

- The file needs to contain rules, recommendations, or process.
- The folder has no agent-facing routing value.
- A local route file would be clearer.

## Default Rules

- Place the index inside the folder it indexes.
- Name it `_{folder-name}.md`.
- Keep index files to a short description plus `## Entries`.
- Do not put rules or recommendations in index files.
- Do not use overwrite files for indexes.
- The CLI regenerates entries under `## Entries`.

## Useful Notes

Index files are mostly for agents. They should be dull, scannable, and hard to misunderstand.
