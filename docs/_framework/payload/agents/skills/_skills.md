# Skills

## Essence

Skills defines what `{forgePath}/skills/_skills.md` and `{forgePath}/skills/` should do in the installed framework.

Skills are tool or agent runtime adapters. They help a specific runtime invoke Open Forge behavior.

## Use When

- A tool supports local skill files.
- A wrapper should tell an agent to read `AGENTS.md` and `{forgePath}/loader.md`.
- A runtime-specific action needs a small adapter, such as saving a session.

## Do Not Use When

- The content is a workflow that any agent can read.
- The content is a structural placement rule.
- The content is tool-agnostic guidance better suited to patterns, workflows, directives, or guides.

## Default Rules

- Keep skills thin.
- Every skill should route back to the loader when the task touches workspace behavior.
- Put process in workflows, structure in patterns, and runtime glue in skills.

## Useful Notes

The default install ships the folder and index only. Actual skills should earn their place.
