# Open Forge Loader

This file is the required Open Forge entrypoint after `AGENTS.md`.

It defines how agents start loading this workspace.

## Load First

- `.agents/constants.md` - root path constants.
- `{forgePath}/workspace/_workspace.md` - index of workspace route files.
- `{forgePath}/workspace/_workspace-open-forge.md` - Open Forge workspace route contract.

The workspace index owns user route discovery. The workspace contract owns installed Open Forge route meaning. Load additional workspace route files only when the current request needs them.

## Axioms

- Open Forge is route-based.
- The current request determines which relevant files are loaded.
- Load the `_{category}.md` file for a category before exploring that category.
- User instructions apply when safe and allowed.
- Local active truth overrides Open Forge defaults.
- Generated indexes are navigation, not behavior.
- Archive, history, examples, external methods, sessions, handoffs, and observations are contextual unless restored or promoted.
- Treat observations as candidate learning, not authority.
- Detailed behavior belongs in the routed file or concept that owns it.

## Route Categories

- `{forgePath}/workspace/_workspace.md` - index of workspace route files.
- `{forgePath}/workspace/_workspace-open-forge.md` - Open Forge workspace route contract.
- `{forgePath}/workspace/{route}.md` - workspace paths, ownership, and route meaning.
- `{forgePath}/patterns/_patterns.md` - index of reusable structure and placement rules.
- `{forgePath}/patterns/{pattern}.md` - structural rules for placement, history, work, or handoff.
- `{forgePath}/workflows/_workflows.md` - index of action sequences.
- `{forgePath}/templates/_templates.md` - reusable artifact skeletons.
- `{forgePath}/observations/_observations.md` - candidate lessons and repeated friction.
- `{forgePath}/sessions/_sessions.md` - user-approved saved chat summaries.
- `{forgePath}/handoffs/_handoffs.md` - temporary continuation notes.
- `{forgePath}/skills/_skills.md` - tool and agent runtime adapters.
- `{docsPath}/directives/` - human-facing rules when the request touches local authority.
- `{docsPath}/guides/` - human-facing guidance when the request needs local explanation.

Load route categories only when relevant to the current request.

## Change Posture

Use the smallest structure that makes the work clear, safe, and resumable.

Customize in this order:

1. Add local files.
2. Use overwrite files for additive or lightly modifying behavior.
3. Edit framework files when a complete behavior change is required.

Do not create parallel truth when active truth already exists. Update the active truth and preserve history locally.
