# Loader

## Essence

Loader defines what `.agents/loader.md` should do in the installed framework.

The loader is the agent entrypoint. It teaches an agent how to read the workspace without becoming the workspace's process.

The loader routes to the smallest useful set of files, explains authority at a high level, and keeps detailed behavior in routes, patterns, workflows, guides, directives, templates, or local files.

## Use When

ALWAys use, THIS IS THE STARTING POINT FOR ANY AGENT/CHAT/ETC

- Any agent starts planning, editing, reviewing, testing, or implementing.
- A wrapper, skill, or role needs to know how to load the workspace.
- The framework needs one common starting point.

## Default Rules

- Keep the loader small. the LOADER SHOULD BE THE ONLY FILE THAT SHOULD NOT BE NEEDED TO BE CHANGED, IT SHOULD CONTAIN ONLY THE NEEDED ESSENTIAL THINGS AND ROUTE TO THE REST. SO IT WILL HAVE ONLY THE RULES/WHERE TO LOAD TO/AND SOME EXPECTATIONS OR SOMETHING
- Split loading into first, relevant, and extra material.
- Load `.agents/constants.md` and `{forgePath}/workspace/_workspace.md` first.
- Load the default Open Forge route file and seeded local route file when present. lOCAL FILES HAVE PRECEDENCE OVER DEFAULTS, BUT THE DEFAULTS ARE STILL LOADED FOR CONTEXT.
- Load additional workspace route files by relevance before detailed material.
- Load `{forgePath}/patterns/_patterns.md` when placement, naming, history, archive, work, handoff, or structural consistency matters.
- For every loaded markdown file, also load `{name}.overwrite.md` when present.
- Treat local active truth as stronger than Open Forge defaults.
- Treat archive, history, examples, external methods, sessions, and observations as context unless explicitly promoted or restored.
- Prefer adding local files FIRST, then overwrite files, then direct framework-file edits.
- The loader should route. It should not become the whole framework.

## Useful Notes

If a rule becomes long, put it in a pattern, workflow, guide, directive, or template and route to it from the loader.

If a default would force users to edit the loader for normal use, the default is probably in the wrong place.

BMAD-like or SDD-like behavior should be distilled into routed workflows and templates later, not embedded in the loader.
