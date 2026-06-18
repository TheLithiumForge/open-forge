# Workspace

## Essence

Workspace defines what `{forgePath}/workspace/_workspace.md` and `{forgePath}/workspace/` should do in the installed framework.

Workspace route files tell agents where important things live and what they mean.

## Use When

- A path should be discoverable by agents.
- A workspace has docs, directives, guides, tasks, repos, vault folders, or local material.
- Human-owned and agent-owned areas need to be visible.

## Do Not Use When

- The content is the full file being routed to.
- The content is detailed workflow instructions.
- The content is tool state.

## Default Rules

- Keep routes short: `` `{entry}` - {description} - #{tag1} #{tag2}``.
- Keep root-level constants in `{forgePath}/constants.md`.
- Use only these default constants: `{repoRoot}`, `{forgePath}`, `{docsPath}`.
- Seed `{forgePath}/workspace/local.md` for local user routes.
- Preserve seeded local route files on normal install once they exist.
- The route tells the agent where to look.
- The target file tells the agent what is true.
- Add route files when the workspace grows.
- Do not encode fixed workspace modes.

## Useful Notes

Workspace routes are the map, not the territory. The agent should follow them and then read the relevant destination.
