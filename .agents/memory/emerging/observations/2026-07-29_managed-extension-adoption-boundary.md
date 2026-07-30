---
open-forge:
  description: Installing the development toolkit over equivalent unowned dogfood Templates exposed a missing explicit transition into managed Extension ownership
  tags: [Memory, Observation, AgentLearning, Contextual, Candidate, CLI, Extension, Ownership, Lifecycle, Dogfood]
---

# Managed Extension Adoption Boundary

## Observation

The current CLI can install a managed Extension into new paths and reconcile paths it already owns. It deliberately refuses to claim a pre-existing unowned path, even when that path represents content already aligned with the package.

This leaves no ordinary one-operation transition for a workspace that wants to move existing local content into managed Extension ownership.

## Evidence And Source

The first local `development-toolkit` dry run stopped at `.agents/templates/documents/_documents.md` with:

> Managed Extension may not claim or replace unowned existing file.

The repository already contained the package's nine selected Template leaves and two supporting Template `entrypoints`. Installing the complete managed package required removing those eleven exact target paths and using `extend development-toolkit --pro`, because the temporary deletions made the Git worktree intentionally dirty.

The resulting plan created twenty-one managed files, regenerated forty-five index regions, and passed `doctor`. The receipt now records one owner for every installed package file.

## Scope

This occurred in the current CLI MVP while installing a first-party Extension into the Open Forge repository, which already dogfooded part of the package as local content.

The refusal to claim unowned files is an existing safety property. This observation does not establish that automatic or same-byte adoption would be safe in every workspace.

## Uncertainty

The missing transition is clear. The correct interface and lifecycle semantics are not accepted.

Possible directions include an explicit adoption operation, an upgrade plan that can transfer ownership after review, or retaining the current refusal and documenting a bounded migration procedure. Byte equality alone may be insufficient when generated regions, local blocks, provenance, or future package versions matter.

## Relevance

The CLI overhaul already needs to separate installation, completion, upgrade, restoration, and Extension lifecycle intents. Ownership adoption is another distinct intent that should be considered before the new planning model is accepted.

Without an explicit design, users may need artificial commits, manual receipt edits, or expert-mode deletion and reinstallation when turning proven local content into a managed package.

## Occurrences

- 2026-07-29: Local installation of `development-toolkit` over existing dogfood Templates.

## Follow-Up And Promotion Signals

Evaluate this transition during the CLI overhaul Architecture Workflow. Promote the result only after the accepted plan defines authorization, preview, provenance, collision handling, generated regions, rollback, and post-application verification.

## Related Records And Sources

- [CLI overhaul candidate](../ideas/cli-overhaul.md)
- [CLI MVP Architecture](../../crystallized/documents/cli/architecture.md)
- [Extensions MVP Architecture](../../crystallized/documents/extensions/architecture.md)
- [Development toolkit catalogue decision](../../crystallized/decisions/development-toolkit.md)
- [Installed ownership receipt](../../../../open-forge.extensions.json)
- [Development toolkit package](../../../../src/extensions/development-toolkit/README.md)
