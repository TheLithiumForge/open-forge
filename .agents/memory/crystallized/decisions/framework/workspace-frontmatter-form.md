---
open-forge:
  description: Why each workspace chooses root or scoped Open Forge frontmatter
  tags: [Memory, Decision, CurrentTruth, Framework, Frontmatter]
---

# Per-Workspace Frontmatter Form

## Context

Routed Markdown may share metadata with other tools. Root fields offer a familiar layout, while scoped fields keep Open Forge descriptions and tags separate.

## Decision

On 2026-10-08, the maintainer accepted a workspace choice between root and scoped frontmatter. Every workspace reads both forms. Writers follow the workspace setting, and existing authored metadata is edited in place.

A missing setting means scoped. Fresh interactive installation offers a choice with root preselected. Fresh unattended installation uses an explicit preference already in settings, otherwise root. Changing the form through `install --configure` converts only owned delivered files that still match the payload rendered in the other form. The settings write and conversions share one reviewed plan and ordinary recovery. Edited files and files whose source is unavailable remain unchanged and are reported.

## Rationale

The choice supports familiar root metadata without converting existing workspaces or removing scoped coexistence. Reading remains independent of output selection.

## Consequences

Writers and managed-content comparisons share the selected rendering. Repository payload sources remain scoped. Excluded, user-authored, Library, and overwrite files are not converted. Native Skills retain their own metadata contract. Defined tags and their loading meanings remain unchanged.

## Authoritative Sources

- [Markdown Syntax](../../documents/framework/markdown/syntax.md#frontmatter)
- [Shared CLI Operation Contract](../../documents/cli/shared-operation-contract.md#frontmatter-form)
- [Install Interface](../../documents/cli/contracts/install/interface.md#setup-selection)

## Provenance

- [Accepted Task 75 decisions](../../../archived/cli-development/tasks/task75-workspace-frontmatter-form.md#accepted-decisions)
- [Frontmatter research Analysis](../../../emerging/analysis/frontmatter-root-keys-and-okf.md#choose-the-form-per-workspace)
