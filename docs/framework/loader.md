# Loader

## Essence

Loader defines what `.agents/loader.md` should do in the installed framework.

The loader is the agent entrypoint. It routes the agent to the smallest useful set of files.

## Use When

- Any agent starts planning, editing, reviewing, testing, or implementing.
- A wrapper, skill, or role needs to know how to load the workspace.
- The framework needs one common starting point.

## Do Not Use When

- The content is project-specific architecture.
- The content is a detailed workflow.
- The content is a long example or tutorial.

## Default Rules

- Keep the loader small.
- Split loading into critical, relevant, and extra material.
- Load `.agents/constants.md` first.
- Load `{forgePath}/workspace/_workspace.md` and `{forgePath}/patterns/_patterns.md` as critical indexes.
- For every loaded markdown file, also load `{name}.overwrite.md` when present.
- The loader should route. It should not become the whole framework.

## Useful Notes

If a rule becomes long, put it in a pattern, workflow, guide, directive, or template and route to it from the loader.
