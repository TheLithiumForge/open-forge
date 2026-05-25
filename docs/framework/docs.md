# Docs

## Essence

Docs defines how installed Open Forge files should treat the default human-facing documentation root.

`docs/` is where human-readable workspace material begins. The default install creates only:

- `docs/directives/`
- `docs/guides/`

## Use When

- A rule should be human-reviewed and authoritative.
- A guide should explain how to do or understand something.
- Human-facing docs should sit outside `.agents/`.

## Do Not Use When

- The material is agent loader mechanics.
- The material is a temporary handoff.
- The material is tool state, cache, or generated runtime data.

## Default Rules

- `docs/directives/` is for human-reviewed rules and local authority.
- `docs/guides/` is for explanations, guidance, and teaching material.
- Docs become active truth only when the workspace treats them as human-reviewed current material.
- If a workspace uses a different docs root, declare it in `.agents/workspace/`.

## Useful Notes

The installed docs folders are intentionally empty. They are anchors, not a forced documentation system.
