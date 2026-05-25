# Open Forge Loader

This file is the agent entrypoint. It tells agents where to start.

## Critical - Must Read

- `.agents/constants.md` - root path constants.
- `{forgePath}/workspace/_workspace.md` - index of workspace route files.
- `{forgePath}/patterns/_patterns.md` - index of reusable pattern files.

Read the relevant files listed by each index.

For every loaded markdown file, also load its sibling overwrite file when present:

- `{name}.overwrite.md`

Use overwrites only when the combined behavior stays clear. If base and overwrite would conflict badly, prefer a local edit to the base file plus a small overwrite that states the replacement behavior.

## Read If Relevant

- `{forgePath}/workspace/{route}.md` - workspace paths, ownership, and local meaning.
- `{forgePath}/patterns/{pattern}.md` - structural rules for placement, history, work, or handoff.
- `{forgePath}/workflows/_workflows.md` - action sequences for design, implementation, testing, review, or handoff.
- `{forgePath}/templates/_templates.md` - reusable artifact skeletons.
- `{forgePath}/observations/_observations.md` - candidate lessons and repeated friction.
- `{forgePath}/sessions/_sessions.md` - user-approved saved chat summaries.
- `{forgePath}/handoffs/_handoffs.md` - temporary continuation notes.
- `{forgePath}/skills/_skills.md` - tool and agent runtime adapters.
- Any route declared by `{forgePath}/workspace/{route}.md` when the current request needs it.
- `{docsPath}/directives/` - human-facing rules when the request touches local authority.
- `{docsPath}/guides/` - human-facing guidance when the request needs local explanation.

## Extras

- Archive, history, examples, and external methods are context, not active truth.
- Load extras only when the request needs them.

## Work Rules

Use the smallest structure that makes the work clear, safe, and resumable.

Do not create parallel truth when active truth already exists. Update the active truth and preserve history locally.
