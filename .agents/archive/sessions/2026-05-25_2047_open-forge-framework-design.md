---
description: Open Forge framework design session covering original issues, goals, current structure, and useful files
tags: [OpenForge, Session, FrameworkDesign]
---

# Open Forge Framework Design Session

Date: 2026-05-25

## Original Issues

The work started from clutter and friction in the previous private `.github-private` setup. The old setup had useful ideas, but too much inherited structure, BMAD terminology, old debates, and mixed active/history material.

The main problems were:

- Agents needed too much manual routing and too much context loaded up front.
- Important docs, directives, scripts, tasks, and handoffs were mixed with old or abandoned material.
- Existing workflow systems felt too heavy, too absolute, or too tool-specific.
- The user wanted AI help for thinking, planning, implementation, review, testing, and handoff without letting AI become the main driver.
- Review became the real bottleneck, so work needs to be split into complete, reviewable, documented, testable chunks.
- The system needed to grow organically without shipping a giant default methodology.

## What We Want To Achieve

Open Forge should become a tiny, agnostic, markdown-first workflow system for agentic work.

The goal is to help turn:

```text
idea
-> docs
-> tasks or workflows
-> implementation
-> review
-> handoff
-> learning
```

into plain repo files that humans and agents can read, diff, update, and carry forward.

The framework should:

- Keep the person at the forefront.
- Treat AI as a tool in the developer toolbox, not the main driver.
- Be plain markdown first, not dependent on a CLI, server, database, plugin, or vendor.
- Let each workspace define its own shape.
- Prefer routing and patterns over large static defaults.
- Make active truth, archive, observations, sessions, handoffs, workflows, templates, and skills clearly different.
- Be a little more work up front so it can avoid fake complete defaults.
- Support one repo, many repos, monorepos, vault-style documentation, or whatever shape the user's work actually has.

## Current Shape

The installable framework payload lives in:

```text
src/open-forge/
```

The current installable shape is:

```text
AGENTS.md
.agents/
  constants.md
  loader.md
  workspace/
    _workspace.md
    local.md
    open-forge.md
  patterns/
    _patterns.md
    open-forge.md
  workflows/
    _workflows.md
  templates/
    _templates.md
  observations/
    _observations.md
    archive/
  sessions/
    _sessions.md
    archive/
  handoffs/
    _handoffs.md
    archive/
  skills/
    _skills.md
docs/
  directives/
  guides/
```

Index files now live inside the folder they index and are named with a leading underscore:

```text
{folder}/_{folder}.md
```

Generated index entries use:

```md
- `{entry}` - {description} - #{tag1} #{tag2}
```

The CLI now regenerates entries for `_*.md` files by reading markdown siblings in the same folder. Index files should stay boring: short description plus entries, no rules or recommendations.

## Important Decisions

- `AGENTS.md` is patch-managed through Open Forge markers.
- Constants live at `{forgePath}/constants.md`.
- Default constants are intentionally shallow:
  - `{repoRoot}`
  - `{forgePath}`
  - `{docsPath}`
- The loader should stay minimal and route agents to indexes and relevant files.
- Workspace files are route files. `local.md` is the default place for local user routes.
- Patterns describe structure and placement rules.
- Workflows describe action sequences.
- Work packages and tasks are folded into workflows plus locality patterns. There is no default `.agents/tasks/` or `.agents/work/` folder.
- Skills are runtime/tool adapters, not workflows.
- Observations are pseudo-truth and should be reviewed periodically for promotion to directives, guides, patterns, or workflows.
- Sessions are durable user-approved saved chat summaries.
- Handoffs are temporary continuation notes and should be deleted, archived, or summarized into sessions after use.
- Archive is historical or inactive material and never overrides active truth.
- `docs/framework/` contains source specs for installable framework behavior and should not be packaged or installed directly for users.

## Useful Files

User-facing docs:

- `README.md` - public explanation, install instructions, growth model, indexes, overwrites.
- `docs/cli.md` - CLI command behavior.
- `docs/dev.md` - maintainer build, release, and package notes.

Internal framework specs:

- `docs/framework/loader.md`
- `docs/framework/workspace.md`
- `docs/framework/indexes.md`
- `docs/framework/overwrites.md`
- `docs/framework/patterns.md`
- `docs/framework/workflows.md`
- `docs/framework/templates.md`
- `docs/framework/observations.md`
- `docs/framework/sessions.md`
- `docs/framework/handoffs.md`
- `docs/framework/skills.md`
- `docs/framework/archive.md`
- `docs/framework/changelog.md`
- `docs/framework/docs.md`
- `docs/framework/naming.md`

Installable framework files:

- `src/open-forge/AGENTS.md`
- `src/open-forge/.agents/constants.md`
- `src/open-forge/.agents/loader.md`
- `src/open-forge/.agents/workspace/_workspace.md`
- `src/open-forge/.agents/workspace/local.md`
- `src/open-forge/.agents/workspace/open-forge.md`
- `src/open-forge/.agents/patterns/_patterns.md`
- `src/open-forge/.agents/patterns/open-forge.md`

Build and packaging:

- `src/cli/cli.ts` - Node-targeted CLI source.
- `build.ts` - Bun build script for CLI and release artifacts.
- `package.json` - npm package metadata and file allowlist.

Private scratch:

- `NOTES.MD` - internal candidates and reminders.
- `.agents/sessions/` - repo-local saved sessions for framework design memory.

## Next Useful Work

- Keep streamlining installable files against the `docs/framework/*.md` specs.
- Decide which default workflows, templates, or skills deserve real content.
- Define the save-session skill.
- Continue checking that installable payload files are useful but not overfilled.
- Keep `docs/framework/` out of package/install output.
