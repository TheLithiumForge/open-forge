---
description: Phase 1 primitives, phase 2 workflow goodies, and edit/overwrite direction
tags: [OpenForge, FrameworkDesign, Customization]
---

# Framework Primitives And Customization

Date: 2026-05-27

## Status

This is a working design note. The public `README.md` remains the current source of truth until these ideas are promoted into framework docs or installable files.

## Direction

Open Forge should keep phase 1 focused on good primitives and architecture.

BMAD-like benefits are still desired, but they belong in phase 2 as routed workflows, templates, and runtime adapters. BMAD is an input library and comparison point, not the base architecture.

Phase 1 should make the framework easy to understand, install, route through, and customize without forcing a complete methodology pack.

## Primitive Categories

The framework is mostly built from these kinds of things:

- Files: concrete installed or maintainer files, such as loader, route indexes, and pattern files.
- Concepts: rules or mechanisms that can apply across files, such as overwrites, active truth, archive, locality, observations, and generated indexes.
- Routes: files that tell agents where to look and what a place means.
- Patterns: reusable structure and placement rules.
- Workflows: action sequences. These are phase 2 unless a minimal placeholder is needed for routing.
- Templates: artifact skeletons. These are phase 2 unless a minimal placeholder is needed for routing.
- Skills: runtime adapters. These should stay thin and route back to the loader.
- Specs: maintainer-facing source descriptions under `docs/framework/`.
- Payload: installable files under `src/open-forge/`.

Example classification:

- Loader is a file and an entrypoint primitive.
- Overwrite is a concept and customization mechanism.
- Index is both a file shape and generated routing mechanism.
- Workspace route is a file category and a routing concept.
- Pattern is a file category and structural-rule concept.

## Customization Direction

The framework files should be written so users usually do not need to edit them.

Preferred customization order:

1. Add new local files.
2. Add overwrite files when a small local adjustment is clearer than a new file.
3. Edit framework files only when the base file would mislead the workspace or when base plus overwrite would confuse agents.

Editing framework files is allowed because installed Open Forge files become the user's files. It should be the rare escape hatch, not the normal path.

The docs should say this plainly:

- Added local files survive updates best.
- Overwrite files are good for small local adjustments.
- Generated indexes should not have overwrite files.
- Managed base files may be overwritten by install/update and should be reviewed through git diffs.
- If an agent would need to reconcile incompatible truths, edit the base file instead of piling on overwrites.

## Axioms Versus Routed Behavior

Framework files should contain only the smallest set of axioms: rules that define how Open Forge works and are not normally contested by a workspace.

Examples of axioms:

- the loader is the entrypoint after `AGENTS.md`
- workspace loading is route-based
- local active truth beats Open Forge defaults
- overwrite companions are additive or lightly modifying behavior, not a place for incompatible replacement systems
- generated indexes are navigation output
- archive and observations are not active truth unless restored or promoted

Most behavior should not be axiomatic. It should be routed into local files, seeded files, patterns, workflows, templates, guides, directives, or skills.

Default rules should be pushed out of core framework files as much as possible. If Open Forge later offers default rules, they should usually be extra seeded files that become user-owned after install, not hard managed framework behavior.

Users may edit framework files when they want a complete change. At that point the file is theirs. Open Forge should still keep managed files minimal and easy to diff so future updates are reviewable in git.

## Non-Imposing Framework Files

Default framework files should avoid imposing local working patterns.

They should say:

- what this primitive is
- when to load it
- what it routes to
- what not to put there
- how to customize it safely

They should avoid saying:

- this is the recommended way all users should work
- every project should use this workflow
- every workspace should use these task files
- every user should adopt this review process

If a behavior is opinionated, it should usually live in an editable local file, a pattern file, a workflow file, a guide, or a directive, not in the core loader.

## File-By-File Refinement Plan

Work from the maintainer specs to the installable payload.

For each `docs/framework/*.md` file:

1. Classify it as file, concept, route, pattern, workflow, template, skill, or support mechanic.
2. Refine the essence.
3. Decide what belongs in the installable payload, if anything.
4. Rewrite the matching `src/open-forge/` file so it is small, non-imposing, and route-oriented.
5. Keep phase 2 workflow content out unless it is only an empty route/index.

## Immediate Candidates

- Add a framework concept map or classification doc for maintainer use.
- Tighten edit versus overwrite language in README and CLI docs.
- Consider adding one minimal authority concept so Open Forge defaults do not overpower local truth.
- Fix index generation newline behavior or add repository line-ending policy.
- Keep BMAD distillation for phase 2 after primitives are stable.
- Explore scoping framework-owned files under a dedicated `forge/` or `open-forge/` folder so user-owned files are visually easy to distinguish.

## Idea To Explore: Scoped Framework Folder

Current install shape mixes Open Forge framework files and user-owned extension points directly under `.agents/`:

```text
.agents/
  loader.md
  workspace/
  patterns/
  workflows/
  templates/
```

An alternative is to scope framework-owned files under a dedicated namespace:

```text
.agents/
  open-forge/
    loader.md
    workspace/
    patterns/
    workflows/
    templates/
  workspace/
    local.md
  patterns/
    local.md
```

or:

```text
.agents/
  forge/
    loader.md
    workspace/
    patterns/
    workflows/
    templates/
```

Potential benefits:

- Framework-owned files are visually recognizable.
- User-owned files and framework-owned files are harder to confuse.
- Install/update behavior can be simpler: namespace files are managed, user files live outside the namespace.
- It reduces pressure on edit versus overwrite because users know where local additions belong.

Potential costs:

- Paths get heavier.
- `AGENTS.md` and docs need one more path segment.
- Existing current design and README would need a structural rewrite.
- The mental model may shift from "small files installed into `.agents`" to "a framework inside `.agents`", which could feel more tool-like.

Open questions:

- Should `{forgePath}` mean `.agents` or `.agents/open-forge`?
- Should local routes live beside the framework namespace or inside it?
- Is `forge/` too generic, and is `open-forge/` too branded or long?
- Does namespacing make updates safer enough to justify changing the install shape now?

## First Refinement Decisions

- Add `docs/framework/_framework.md` as a maintainer-facing primitive map.
- Treat `.agents/workspace/local.md` and `.agents/patterns/local.md` as seeded local files: created when missing, preserved when already present.
- Keep the loader as route and authority posture, not local process.
- Move opinionated local structure toward local pattern files instead of default Open Forge pattern files.
- Add a repository line-ending policy so generated indexes do not create platform noise.

## Maintainer Descriptor Model

`docs/framework/` is maintainer and AI-facing descriptor material.

Files under `docs/framework/` define what the installable files under `src/open-forge/` are supposed to represent, contain, and preserve. They are not installed as user framework files and should not read like user-facing methodology instructions.

Mental model:

```text
docs/framework/*  -> descriptors/specs for maintainers and agents
src/open-forge/*  -> actual files that end up on the user's machine
```

Each framework descriptor should help maintainers answer:

- What src file or primitive does this describe?
- What does that src file represent?
- What should that src file contain?
- Where is that src file used in the installed framework?
- Why does that src file exist?
- What keeps the implementation aligned with the descriptor?

Descriptors should use positive, bounded definitions instead of broad "do not use when" lists. Negation-heavy sections can become vague without context.

Descriptors should use firm contract language such as `must`, `contains`, `represents`, and `requires` where the behavior is not optional. They should not read like loose advice.

Maintainer rule: changes to an installable `src/open-forge/` framework file should start by updating or confirming the matching `docs/framework/` descriptor. The implementation change should have an explicit reason tied to the descriptor.

## Descriptor Mirror Layout

The framework descriptors should mirror the installable payload shape.

Descriptor side:

```text
docs/framework/payload/
```

Implementation side:

```text
src/open-forge/
```

Visible mirror convention:

```text
docs/framework/payload/agents/  ->  src/open-forge/.agents/
```

The descriptor tree uses `agents/` instead of `.agents/` so maintainer specs are not hidden from normal file search.

Cross-cutting concepts that do not govern one exact installed file live in:

```text
docs/framework/concepts/
```

This keeps the two worlds clear:

- `payload/` describes concrete installable files and folders.
- `concepts/` describes behavior that cuts across multiple installable files.
