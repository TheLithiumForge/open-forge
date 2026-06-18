---
description: Default Open Forge structure, locality, history, work, handoff, and learning patterns
tags: [OpenForge, Pattern]
---

# Open Forge Patterns

This file describes generic structure primitives used by Open Forge.

Local workspace structure belongs in `local.md` or another local pattern file.

## Entry Shape

Use this shape:

- `{entry}` - {description} - #{tag1} #{tag2}

Keep entries terse. Avoid tables unless the shape of the data truly needs them.

## Naming

Use names that sort well and say what the file is.

Use numbering when ordering matters. Skip numbering when it adds nothing.

## Locality

Keep related material near the thing it belongs to when a useful local owner exists.

Common local-owner shapes:

- {entry}/archive/ - Old or superseded material for that entry - #History
- {entry}/changelog.md - Notable changes to that entry - #History
- {entry}/decisions/ - Decisions that explain current direction - #HumanReviewed #ActiveTruth
- {entry}/work/ - Reviewable work material for that entry when a workflow needs it - #Work

Use a shared archive only for imported, orphaned, abandoned, or no-longer-local material.

## Archive

Archive is for historical, superseded, consumed, rejected, or kept-for-context material.

Archive does not override active truth. Restore archived material explicitly before treating it as current.

- {entry}/archive/ - Archive beside the thing it belongs to - #Archive

## Changelog

A changelog is active context for the thing it describes.

Prefer:

- {entry}/changelog.md - Notable changes to that entry - #History

If an entire entry is retired, its changelog moves with it. If a changelog becomes too large, older sections can be moved into local archive.

## Active Truth

Active truth is the current human-reviewed source for a thing.

When active truth changes:

1. Update the active file.
2. Move or link superseded material to local history when useful.
3. Add a changelog note when the change would matter later.

Do not create parallel truth.

## Work

Work belongs to workflows first.

Use local work folders only when a workflow needs a reviewable, testable, documented slice of work.

- {entry}/work/{work-package}/ - Work package beside its local owner - #Work

If a workspace needs a specific work package shape, define it in a local pattern, workflow, or template.

- {entry}/work/{work-package}/scope.md - Intent, scope, and non-scope - #Work
- {entry}/work/{work-package}/evidence.md - Required proof and validation results - #Work #Evidence
- {entry}/work/{work-package}/handoff.md - Continuation notes and next safe action - #Work #Handoff

## Handoff

Create a handoff when context, ownership, or session continuity matters.

- {entry}/work/{work-package}/handoff.md - Local handoff for a work package - #Handoff

Use `{forgePath}/handoffs/` only when there is no useful local owner. Handoffs are temporary; delete, archive, or summarize them into a session after they are consumed.

## Learning

Agent observations are candidates, not truth.

Put observations in `{forgePath}/observations/` unless a more specific route exists.

Promote observations only after human review or explicit instruction.
