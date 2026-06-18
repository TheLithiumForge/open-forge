# Overwrites

## Essence

Overwrites define how installed markdown files can be locally adjusted without losing update visibility.

An overwrite is a `{name}.overwrite.md` companion file loaded after `{name}.md`.

## Use When

- The base file is mostly right.
- The workspace needs a local addition, narrowing, exception, or explicit disable.
- Base plus overwrite remains clear to an agent reading both files.

## Do Not Use When

- The base file would actively mislead the agent.
- Base plus overwrite would create two incompatible truths.
- The target is an index file.

## Default Rules

- Edit the base file when the base behavior is wrong for the workspace.
- Use `{name}.overwrite.md` when the base behavior needs a clear local adjustment.
- Do not ask agents to reconcile contradictions.
- Do not use overwrites for generated index files.
- Prefer adding local files over overwriting shared defaults when a new file is clearer.

## Useful Notes

Edit versus overwrite is a clarity decision, not a purity test. If the agent would be confused, edit.
