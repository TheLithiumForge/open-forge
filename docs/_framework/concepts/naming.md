# Naming

## Essence

Naming defines how installed Open Forge files should name generated or user-created artifacts.

Names should be sortable, readable, and easy for agents to reference.

## Use When

- Creating sessions, handoffs, observations, templates, workflows, or route files.
- Deciding whether a folder needs numbered files.
- Creating overwrite companions.

## Do Not Use When

- A local workspace has a stronger naming convention.
- Numbering adds ceremony without improving ordering.
- The name hides meaning behind cleverness.

## Default Rules

- Use `{name}.overwrite.md` for overwrite companions.
- Use `_{folder-name}.md` for index files inside indexed folders.
- Use `{yyyy-MM-dd}_{HHmm}_{slug}.md` for saved sessions.
- Use clear slugs over clever names.
- Use numbering when ordering matters.
- Skip numbering when order does not matter.

## Useful Notes

The filename should help the agent before the file is opened. Cute can wait outside the critical path.
