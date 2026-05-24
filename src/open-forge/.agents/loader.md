# Open Forge Loader

This file is the agent entrypoint. It tells agents what to read and when.

## Critical - Must Read

- `.agents/workspace.md` - workspace entry index.
- `.agents/patterns.md` - pattern index.

Read the relevant files listed by each index.

For every loaded markdown file, also load its sibling overwrite file when present:

- `{name}.overwrite.md`
- mode: `append`, `replace`, or `disable`

Use overwrites only when the combined behavior stays clear. If base and overwrite would conflict badly, prefer a local edit to the base file plus a small overwrite that states the replacement behavior.

## Read If Relevant

- `.agents/workspace/{entry}.md` - workspace paths, ownership, and local meaning.
- `.agents/patterns/{pattern}.md` - structural rules for placement, history, work, or handoff.
- `.agents/observations/` - candidate lessons and repeated friction.
- `.agents/sessions/` - session notes when continuity matters.
- `.agents/handoffs/` - fallback handoffs without a better local owner.

## Extras

- Archive, history, examples, and external methods are context, not active truth.
- Load extras only when the request needs them.

## Directions

Index entries use:

- `{file}` - `{description}` - `#{tag1} #{tag2}`

Authority order:

1. User instruction
2. Safety and destructive-action rules
3. Workspace active truth
4. Open Forge directions and patterns
5. Examples, history, archive, and external methods

Classify before acting:

- thought
- question
- rewrite
- document
- plan
- implementation
- review
- testing
- handoff
- cleanup
- workflow change

Use the smallest structure that makes the work clear, safe, and resumable.

For meaningful work:

1. Restate intent.
2. Name scope and non-scope.
3. Load relevant active truth.
4. Define evidence.
5. Make the change.
6. Review the result.
7. Run fitting validation.
8. Update nearby docs, evidence, handoff, or changelog when useful.

Do not create parallel truth when active truth already exists. Update the active truth and preserve history locally.
