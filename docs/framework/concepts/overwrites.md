# Overwrites

## Description

This descriptor governs the overwrite concept across the installable Open Forge payload.

An overwrite is a markdown companion file named `{name}.overwrite.md`. It is read after `{name}.md` when the base file is loaded.

Overwrites are a customization mechanism, not a separate workflow system. They let a workspace make small local adjustments while keeping upstream framework changes visible in git.

## Represents

Overwrites represent the middle customization layer between adding local files and directly editing framework files.

The customization order is:

1. Add local files when the behavior can be expressed as a new route, pattern, workflow, template, guide, directive, or skill.
2. Use overwrite files when the base file is mostly right and needs a small local addition, narrowing, exception, or disable.
3. Edit the base framework file when the desired behavior is a complete replacement or when base plus overwrite would confuse an agent.

## Contains

An overwrite file must contain only the local adjustment.

It must make its relationship to the base file clear. It must not require an agent to reconcile competing models.

An overwrite may contain:

- an additive local rule
- a local example
- a local exception
- a narrowed interpretation
- an explicit disable of a small behavior with a reason

## Scope

Overwrite files apply to markdown source files that agents read as behavior, routes, patterns, workflows, templates, guides, directives, observations, sessions, handoffs, or skills.

Generated index regions are output, not behavior. They must be changed by adding, removing, or editing files in the indexed folder, then regenerating the index.

The authored portion of a category `entrypoint` may define category behavior and follows the normal customization order. Its generated region never has an overwrite.

## Implementation Requirements

Agents must read `{name}.overwrite.md` after `{name}.md` when both files exist and the base markdown file is loaded.

Index generation must ignore overwrite files as index `entries`.

Install/update behavior must keep overwrite files visible as local files. An Open Forge update must not silently merge overwrite content into the base file.

## Why

Overwrites exist to keep local adjustments reviewable.

They reduce the need to edit managed framework files while still allowing a workspace to adapt installed behavior. They also keep future updates diffable: upstream changes appear in the base file, local changes remain in the overwrite file.

## Alignment Checks

The overwrite concept is aligned when:

- local files are preferred before overwrites
- overwrites are used only for additive or lightly modifying behavior
- direct edits are used for complete behavior changes
- generated index regions are changed only through indexed files
- agents are not asked to reconcile contradictory base and overwrite behavior
- update behavior keeps local overwrite files visible and reviewable
