---
open-forge:
  description: Implement creation of one reviewable Extension package outside a workspace
  tags: [Memory, Working, CLI, Task, Extension, Create, Lifecycle, Contextual]
---

# Implement Extension Create

## Task State

- State: Planned after Mutation Foundation and Extension Inspect.
- Parent: [Lifecycle Commands](_lifecycle.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/extension/create/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/extension/create/behavior.md).

## Expected Outcome

`extension create` creates one exact reviewable Extension package at an explicit
catalogue destination without a workspace subject or workspace lock.

## Architecture

- Keep exact package identity, manifest model, Template/payload selection,
  destination observation, `ExtensionCreatePlan`, result, and presentation local.
- Reuse Extension manifest/source facts and strict path validation. Use a
  separate create-only destination writer with exact collision and revalidation
  checks; do not call the workspace `FileChangeApplier`, acquire a workspace
  lease, or prepare a recovery bundle.
- Replace workspace-lock and recovery safeguards with exact catalogue destination
  identity, expected-state revalidation, and collision refusal. This command has
  no workspace lease, no Replace/Delete, and no recovery bundle.

## Evidence

Cover valid and invalid package IDs, exact destination, existing/colliding content,
case/Unicode aliases, Template and payload formation, dry run, no workspace,
revalidation race, create-only partial failures, manifest/payload verification,
second run, no unrelated changes, process, and AOT.

## Stop Conditions

Stop before inferring a workspace, acquiring `.agents/open-forge.lock`, registering
the package automatically, installing it, or using a generic package generator.
