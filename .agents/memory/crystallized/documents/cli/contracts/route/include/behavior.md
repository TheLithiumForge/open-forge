---
open-forge:
  description: Accepted behavior for route include planning, preservation and recovery
  responsibility: Define deterministic route include resolution, mutation and verification
  tags: [Memory, Crystallized, CLI, Release, Command, Behavior, Route, Include, Mutation, CurrentTruth]
---

# route include Behavior Contract

## Workspace Layout

Resolve categories through the layout rather than a fixed path segment. Distinguish native Skills from external includes, retain literal registered paths and revalidate the layout before effects.

See the [shared layout contract](../../../shared-operation-contract.md#workspace-layout).

## Status And Authority

The [Interface](interface.md) defines observable meaning.
This document defines its technology-neutral operation.
The [route create behavior](../create/behavior.md) supplies the existing route
preflight and mutation pattern. Include and Exclude preserve that boundary.

## Request Resolution

Resolve one explicit workspace and canonical operand. Validate the full lexical
and physical path before reading a target. Reject unsupported targets with an
invalid-input cause that names the operand and gives the supported alternative.
Require an existing UTF-8 file and never infer loading metadata from it.
Read the loader and authored settings safely. A malformed control document blocks
the complete operation before effects.

## Planning

Read rows through the shared Entries grammar. Match destinations by their
canonical workspace path, including percent-encoded authored destinations.
Keep omitted metadata and replace only supplied fields. Preserve an identical
authored row exactly. Append a new external row after existing authored rows.
Turn a primary-category catalogue on through loader membership and use its own metadata.
Plan external registration through the settings root-registration planner.

Build loader bytes through the generated navigation planner. Preserve authored
links and membership through the same machinery used by Index.
Settings `roots` records registration only. It never restores a removed loader row.
Keep target bytes and all writes outside the two control files out of the plan.
Identical loader and settings bytes form a verified no-op.

## Application And Recovery

Dry-run stops with the complete plan. Application takes the workspace mutation
lock, rebuilds and compares the plan, and revalidates loader, settings, target and
catalogue observations. A changed observation stops application before effects.
Prepare the recovery bundle before replacing either control file.
Apply only planned changes with expected-state checks and truthful receipts.

Verify every changed file and unchanged source observation. Replan the request
and require convergence to a no-op. Only then remove the exact recovery bundle.
Retain partial effects and recovery evidence on failure or interruption.
A known retained artifact after verified changes is a warning. Unknown recovery
disposition is a failure.

## Evidence And Scope

Unit evidence covers binding, result projection and source-generated JSON.
Integration evidence covers real files, row merging, invalid targets and planner
preservation. Published-process journeys cover application and no reactivation
through Index, Update, Configure and Extension installation.

The implementation reuses managed filesystem APIs, the shared Markdown parser,
generated navigation, settings planners, mutation lock, recovery store and file
applier. There is no custom parser, native bridge or new dependency.
The realistic failure boundary is malformed input, interruption, stale plans and
cooperating workspace operations. Recovery bundles and source control provide
repair. A malicious same-user process is outside the accepted CLI threat model.
The overseer owns the complete managed and Native AOT integration gate.

## Recovery History

Drafts, damaged or invalid bundles, and unavailable recovery coverage produce warnings. Verified final bundles from earlier runs are normal retained evidence and produce no warning in writing commands. Historical artifacts never veto unrelated work. Current target, ownership, permission, plan and expected-state checks still apply. The current operation must prepare and verify its own required recovery bundle before effects. Historical paths remain separate from that preparation and remain unchanged.
