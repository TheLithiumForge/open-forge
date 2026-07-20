# Overwrites

## Description

This descriptor governs the overwrite concept across the installable Open Forge payload.

An overwrite is a user-owned Markdown companion named `{name}.overwrite.md`. Agents read it immediately after `{name}.md` whenever the base is loaded.

Overwrites are a customization mechanism, not a separate route or workflow system.

## Represents

Overwrites represent the middle customization layer between adding local files and directly editing framework files.

The customization order is:

1. Add a local routed file when the behavior can stand on its own.
2. Use an overwrite when the base is mostly right and needs a small local addition, narrowing, exception, or disable.
3. Edit the base framework file when the desired behavior is a complete replacement or base plus overwrite would confuse an agent.

Extensions contribute whole files through ordinary routes. They do not own overwrites or use a companion mechanism to mutate shared Markdown.

## Contains

An overwrite contains only the local adjustment and makes its relationship to the base clear.

An overwrite may contain:

- an additive local rule
- a local example
- a local exception
- a narrowed interpretation
- an explicit disable of a small behavior with a reason

It must not leave competing models that an agent has to reconcile.

## Scope

An overwrite inherits the base file's route, scope, and load behavior. It is never indexed or selected independently.

Generated index regions are output, not behavior. Change them by adding, removing, or editing routed files, then regenerate the index manually or with the CLI.

The authored portion of a category `entrypoint` follows the normal customization order. Its generated region never has a separate overwrite.

## Implementation Requirements

Agents read `{name}.md` and then `{name}.overwrite.md` when the overwrite exists. The overwrite has final precedence within that file's scope.

Index generation ignores `.overwrite.md` companions as generated entries.

Install and update behavior keeps overwrite files visible and user-owned. Open Forge must not silently merge them into a base, and a managed extension must not claim them as payload.

## Why

Overwrites keep small local adjustments reviewable and preserve a clear diff between upstream behavior and workspace choices.

## Alignment Checks

The overwrite concept is aligned when:

- local routed files are preferred before overwrites
- overwrites are used only for small local adjustments
- direct edits are used for complete behavior changes
- generated indexes change through routed sources rather than overwrite content
- agents are not asked to reconcile contradictory base and overwrite behavior
- overwrites remain visible, user-owned, and last within their base file's scope
- extensions add whole routed files instead of mutating shared Markdown
