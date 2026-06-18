# Framework Map

This file maps maintainer descriptors to the installable Open Forge payload.

`docs/framework/` is not installed on user machines. It describes and governs the files that are installed from `src/open-forge/`.

## Mirror Rule

Descriptors for installable files live under `docs/framework/payload/`.

`docs/framework/payload/` mirrors `src/open-forge/` with one visible adjustment:

```text
docs/framework/payload/agents/  ->  src/open-forge/.agents/
```

The descriptor tree uses `agents/` instead of `.agents/` so maintainer docs stay visible to normal search tools.

Cross-cutting primitives that do not describe one installed file live under:

```text
docs/framework/concepts/
```

## Descriptor To Implementation Map

| Descriptor | Governs |
| --- | --- |
| `payload/agents/loader.md` | `src/open-forge/.agents/loader.md` |
| `payload/agents/workspace/_workspace.md` | `src/open-forge/.agents/workspace/_workspace.md` and workspace route folder behavior |
| `payload/agents/patterns/_patterns.md` | `src/open-forge/.agents/patterns/_patterns.md` and pattern folder behavior |
| `payload/agents/workflows/_workflows.md` | `src/open-forge/.agents/workflows/_workflows.md` and workflow folder behavior |
| `payload/agents/templates/_templates.md` | `src/open-forge/.agents/templates/_templates.md` and template folder behavior |
| `payload/agents/observations/_observations.md` | `src/open-forge/.agents/observations/_observations.md` and observation folder behavior |
| `payload/agents/sessions/_sessions.md` | `src/open-forge/.agents/sessions/_sessions.md` and session folder behavior |
| `payload/agents/handoffs/_handoffs.md` | `src/open-forge/.agents/handoffs/_handoffs.md` and handoff folder behavior |
| `payload/agents/skills/_skills.md` | `src/open-forge/.agents/skills/_skills.md` and skill folder behavior |
| `payload/docs/_docs.md` | `src/open-forge/docs/` human-facing docs anchors |
| `concepts/archive.md` | Archive behavior across the payload |
| `concepts/changelog.md` | Changelog behavior across the payload |
| `concepts/indexes.md` | Generated index behavior across the payload |
| `concepts/naming.md` | Naming behavior across the payload |
| `concepts/overwrites.md` | Overwrite behavior across the payload |

Descriptors that still need to be created as the file-by-file pass reaches them:

| Needed Descriptor | Will Govern |
| --- | --- |
| `payload/AGENTS.md` | `src/open-forge/AGENTS.md` |
| `payload/agents/constants.md` | `src/open-forge/.agents/constants.md` |
| `payload/agents/workspace/local.md` | `src/open-forge/.agents/workspace/local.md` |
| `payload/agents/workspace/open-forge.md` | `src/open-forge/.agents/workspace/open-forge.md` |
| `payload/agents/patterns/local.md` | `src/open-forge/.agents/patterns/local.md` |
| `payload/agents/patterns/open-forge.md` | `src/open-forge/.agents/patterns/open-forge.md` |

## Categories

- File primitive: a concrete file shape the installed framework or maintainer docs rely on.
- Concept primitive: a rule or behavior that applies across files.
- Route primitive: a file whose main job is to point agents at other files.
- Structural primitive: a rule for where material lives.
- Action primitive: a repeatable sequence of work.
- Artifact primitive: a reusable skeleton for creating a file.
- Runtime primitive: glue for a specific tool or agent runtime.
- Lifecycle primitive: a rule for how material matures, becomes inactive, or stays resumable.
- Support primitive: naming, docs, changelog, or other supporting behavior.

## Cross-Cutting Concepts

These concepts are not owned by only one file:

- Routing: agents should load indexes and route files before reading detailed material.
- Axioms: core framework files should contain only the smallest rules that define how Open Forge works and are not normally workspace-specific.
- Active truth: current human-reviewed material beats old sessions, observations, archive, and examples.
- Locality: related material should live near the thing it belongs to unless no useful local owner exists.
- Generated indexes: index files are navigation output, not a place for behavior.
- Customization layers: add local files first, use overwrites second, edit managed framework files only when needed for clarity.
- Phase 2 extension: workflows, templates, and skills are available as routes before Open Forge ships opinionated defaults.

## Axioms Versus Defaults

Core framework files should describe axioms, not preferred local process.

Axioms are rules that make the framework coherent:

- `AGENTS.md` routes agents into the loader.
- The loader routes instead of loading the whole workspace.
- Workspace files describe where material lives.
- Indexes are generated navigation output.
- Overwrites are for additive or lightly modifying behavior.
- Incompatible changes should edit the base file instead of stacking confusing overwrite behavior.
- Local active truth beats Open Forge defaults.
- Archive, observations, examples, sessions, and handoffs are contextual unless restored or promoted.

Defaults are opinionated behaviors a workspace may or may not want.

Default rules, review rituals, task shapes, testing loops, BMAD-like flows, and local working patterns should usually live in routed files. If Open Forge provides them later, prefer seeded user-owned files over managed framework files.

## Customization Ownership

The installable framework should avoid making users edit managed base files.

Default guidance:

1. Add local route, pattern, workflow, template, guide, or directive files.
2. Use overwrite files for small local adjustments.
3. Edit framework files only when the base file would mislead the workspace or when base plus overwrite would confuse agents.

Files that are advertised as customization points should either:

- be user-owned after creation and not overwritten by normal install/update, or
- be examples that clearly tell the user to add their own local file.

This matters for files such as `.agents/workspace/local.md`. If a file is the default place for local routes, normal install should not silently replace user edits.

## Phase 1 Boundary

Phase 1 should define primitives and routing behavior.

It should not include BMAD-like ceremony as default behavior. BMAD-like value belongs later as small routed workflows, templates, and optional skills.

Core framework files should avoid universal recommendations. When a behavior is opinionated or local, put it in a local route, pattern, workflow, guide, directive, or overwrite instead of hardcoding it into the loader.

## Descriptor Governance

`docs/framework/` files govern installable files in `src/open-forge/`.

A maintainer changing an installable framework file must first update or confirm the matching descriptor. The implementation change should have a clear reason traceable to that descriptor.

Framework descriptors should use contract language:

- `represents`
- `contains`
- `must`
- `requires`
- `is`

They should avoid vague preference language when defining core behavior.
