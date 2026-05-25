# Changelog

## Essence

Changelog defines how installed Open Forge files should record meaningful change.

A changelog is active context for the thing it describes. It helps future humans and agents understand why the current shape changed.

## Use When

- Active truth changed in a way that matters later.
- A route, pattern, workflow, template, directive, or guide changed meaning.
- Material moved to archive and the reason should stay visible.
- A future reviewer would ask "why is it like this now?"

## Do Not Use When

- The change is trivial formatting.
- The note is a raw session transcript.
- The content is an old copy that belongs in archive.

## Default Rules

- Keep `changelog.md` beside the entry it describes.
- Keep entries short and useful.
- If the whole entry is retired, move its changelog with it.
- If the changelog becomes too large, move old sections into local archive.

## Useful Notes

Changelog is not a replacement for decisions. If the reason is important enough to guide future behavior, create or update a decision/directive/guide where the workspace says that belongs.
