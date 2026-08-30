---
open-forge:
  description: Implement creation of one reviewable Extension package outside a workspace
  tags: [Memory, Working, CLI, Task, Extension, Create, Lifecycle, Contextual]
---

# Implement Extension Create

## Task State

- State: Active after integrated D0 and SF1. The command-local lane is isolated
  on branch `codex/extension-create`; all product decisions are closed.
- Parent: [Lifecycle Commands](_lifecycle.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/extension/create/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/extension/create/behavior.md).

## Expected Outcome

`extension create` creates one exact reviewable Extension package at an explicit
catalogue destination without a workspace subject or workspace lock.

## Architecture

- Keep exact package identity, manifest model, fixed empty payload scaffold,
  destination observation, `ExtensionCreatePlan`, result, and presentation local.
- Expose optional `--name`, `--description`, `--package-version`, and repeatable
  `--dependency`. Preserve accepted nonblank override text exactly and keep the
  version descriptive rather than adding SemVer or compatibility policy.
  Defaults are exact ID-derived display name,
  `Open Forge Extension package <stable-id>.`, `0.1.0`, and an empty dependency
  array. Sort explicit dependencies ordinally and reject invalid, duplicate, or
  self IDs without resolving availability.
- Keep the wizard command-local and ask only for missing required facts, currently
  stable ID and catalogue parent. Allow local correction of blank or invalid
  input without an attempt limit; EOF is no-write `invalid`, cancellation is
  no-write `interrupted`, and optional metadata never adds questions. Show
  resolved metadata in the plan; JSON, automatic, and redirected flows never
  prompt.
- Accept any existing safely resolved directory as the catalogue parent,
  including empty and marker-free directories. Do not create it or inspect
  unrelated siblings; classify only the exact `<catalogue>/<id>` destination.
- Emit the exact ordered command-local JSON result: catalogue, destination, ID,
  manifest, mode, intended/applied effects, verification, and
  `workspaceLifecycleChanged=false`, without duplicating shared envelope fields.
- Reuse Extension manifest/source facts and strict path validation. Use a
  separate create-only destination writer with exact collision and revalidation
  checks; do not call the workspace `FileChangeApplier`, acquire a workspace
  lease, or prepare a recovery bundle.
- Replace workspace-lock and recovery safeguards with exact catalogue destination
  identity, expected-state revalidation, and collision refusal. This command has
  no workspace lease, no Replace/Delete, and no recovery bundle.

## Evidence

Cover valid and invalid package IDs, exact destination, every manifest default
and override, native option value forms, singleton repetition, dependency order,
duplicate/self rejection, zero/one/all current missing required facts, local invalid
correction, EOF/cancellation, and direct equivalence, existing/colliding content,
empty/populated catalogue parents, unrelated siblings,
missing-parent refusal, case/Unicode aliases, ordered JSON, payload formation,
dry run, no workspace,
revalidation race, create-only partial failures, manifest/payload verification,
second run, no unrelated changes, process, and AOT.

## Stop Conditions

Stop before inferring a workspace, acquiring `.agents/open-forge.lock`, registering
the package automatically, resolving dependency availability, installing it,
adding optional-metadata questions, an attempt limit or generic retry framework,
or using a generic package generator.
