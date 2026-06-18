# Archive

## Essence

Archive defines how installed Open Forge files should treat inactive material.

Archive is historical, superseded, consumed, rejected, or kept-for-context material. It is searchable context, not current truth.

## Use When

- A file, rule, route, workflow, template, or note was replaced.
- A handoff was consumed but should remain findable.
- An observation was reviewed and not promoted.
- Material is useful history but should not be loaded by default.

## Do Not Use When

- The material is still current.
- The agent should treat the material as active truth.
- The file is only messy but not actually inactive.

## Default Rules

- Prefer local archive: `{entry}/archive/`.
- Archive never overrides active truth.
- Restore archived material into an active location before using it as current truth.
- A shared archive is only for imported, orphaned, abandoned, or no-longer-local material.

## Useful Notes

`changelog.md` is active context, not archive. If the whole entry is retired, the changelog moves with it.
