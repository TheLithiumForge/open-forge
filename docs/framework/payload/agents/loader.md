# Loader

## Description

This descriptor governs `src/open-forge/.agents/loader.md`.

The loader is the mandatory Open Forge entrypoint after `AGENTS.md`. It defines how an agent starts reading an installed workspace, how it discovers framework and user files, and how it decides which routed files are relevant to the current request.

The loader is intentionally stable. It contains Open Forge loading axioms and route categories only. Detailed behavior must live in the routed file or concept that owns that behavior.

## Represents

The loader represents the installed workspace loading contract.

It is a file primitive and routing primitive. It is not a workflow, template, guide, directive, project map, or task format.

## Contains

The installed loader must contain:

- the first files every agent loads
- the rule that the current request controls relevance
- the route categories available to the agent
- the authority posture for local active truth, defaults, context, and candidate learning
- the customization posture: add local files first, use overwrites for light changes, edit framework files for complete behavior changes

The installed loader must be short, concrete, and easy to diff. Target size is 40-90 non-empty lines. If the loader needs more than 200 non-empty lines, the design must be split into routed files or separate concepts before implementation.

## Load Contract

The installed loader must load these files first:

```text
.agents/constants.md
{forgePath}/workspace/_workspace.md
{forgePath}/workspace/_workspace-open-forge.md
```

The workspace index owns user route discovery. The workspace category contract owns installed Open Forge route meaning. The loader may name category contracts or route patterns as important route files, but it must not hardcode a large first-load set.

Local route files and local active truth have precedence over Open Forge defaults. Default files may still be loaded as context when useful.

The installed loader must route additional loading by relevance. It must not require loading every workspace, pattern, workflow, template, observation, session, handoff, skill, directive, or guide file upfront.

When an agent enters a routed category, it must load the generated category index named `_{category}.md` before exploring route files under that category. This keeps category discovery cheap and makes routing explicit.

## Route Contract

The installed loader must include one-line descriptions for the main route categories:

- workspace routes
- workspace category contract
- patterns
- workflows
- templates
- observations
- sessions
- handoffs
- skills
- directives
- guides

The installed loader must route to indexes, route files, or folders. It must not duplicate generated index entries.

## Authority Contract

The installed loader must state these authority axioms:

- user instructions apply when safe and allowed
- local active truth overrides Open Forge defaults
- generated indexes are navigation, not behavior
- archive, history, examples, external methods, sessions, handoffs, and observations are contextual unless restored or promoted
- observations are candidate learning, not authority
- detailed behavior belongs in the routed file or concept that owns it

## Customization Contract

The installed loader must state the customization order:

1. Add local files.
2. Use overwrite files for additive or lightly modifying behavior.
3. Edit framework files when a complete behavior change is required.

The loader must keep this as a posture only. The overwrite mechanism is governed by `docs/framework/concepts/overwrites.md`.

## Used By

The loader is used by every agent, chat, wrapper, role, skill, or runtime adapter that enters an installed Open Forge workspace.

## Why

The loader exists so agents do not guess how to enter the workspace.

It keeps Open Forge small by making the workspace discoverable through plain files instead of a hidden runtime, large config, fixed workflow pack, or complete upfront context load.

It protects local ownership by making Open Forge defaults route-based. The installed workspace decides its own structure and active truth.

## Alignment Checks

The implementation is aligned when it:

- stays small enough to read on every agent start
- loads constants, the workspace index, and the workspace category contract first
- loads a category index before exploring that category
- routes to categories instead of duplicating their contents
- states that local active truth overrides defaults
- treats indexes as navigation
- treats archive, sessions, handoffs, examples, and observations as contextual unless promoted or restored
- keeps detailed process behavior in routed files
- keeps framework customization guidance minimal and diff-friendly
