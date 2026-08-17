---
tags:
  - Memory
  - Working
  - Handoff
  - KeepInMind
description: Sealed continuation state for the CLI release program at the Queue 31 route move and remove review boundary.
---

# CLI Release Gate 2 Queue 31 Handoff

## Seal

Sealed on 2026-08-16 after Queue 29 integration, Queue 30 rejection, program and
workflow crystallization, focused review corrections, and final static
validation. Do not edit this record. The resumable mutable state remains in the
[CLI Release Checkpoint](../checkpoints/cli-release.md).

## Current authority and scope

- Gate 1 is complete. Gate 2 remains open only for the retained-command reviews
  in Queues 31–33.
- Queue 29 is accepted and integrated. Current Framework `install` and `update`,
  the Extension group and six Extension leaves, Status, Doctor, the Decision
  Agenda, and the shared CLI Pattern now carry its authority.
- Queue 30's partial first-release proposal is rejected and superseded history.
  Every retained command must be implemented before delivery.
- Queue 31 is the current review: grouped `route move` and `route remove`.
  Queue 32 root `cleanup` and Queue 33 completion disposition remain waiting in
  that order.
- Gate 3 Architecture cannot begin until Gate 2 closes. Implementation remains
  blocked until the maintainer accepts Gate 3 Architecture and Gate 4
  crystallization/readiness.
- Preserve the cumulative dirty Queue 22+ worktree. No branch, commit, merge,
  push, package, or implementation operation has been authorized or performed.

## Accepted program direction

1. Discuss and accept the complete architecture before implementation.
2. Build the C#/.NET 10+ `.slnx`, Native AOT, real-filesystem, xUnit v3
   foundation.
3. Implement every retained command in a fluid dependency order reanalyzed in
   Preflight.
4. Add thin package-manager wrappers, with npm first.
5. Add CI build/test evidence and main-only release/publish proof.
6. Finish public documentation, history, maps, packaging records, and release
   closeout only after implementation evidence exists.

Future implementation Tasks use read-only Preflight, the exact Gray/Red/Green
behavior-defect subcycle, one Blue production-improvement pass, one Purple
test-improvement pass, and at most one exceptional correction cycle. Mastermind
owns planning, phase transitions, integration, review, correction-cost
decisions, and repository actions; the maintainer retains architecture and final
acceptance authority. Tasks branch from `develop` through focused
`feature/<task>` branches, preserve coherent commits, squash-merge to `develop`,
and release and publish only from `main`.

## Queue 31 decision surface

The decision-ready candidate is in
[Route Move and Remove](../cli-release/review/route-move-remove.md). Present it
to the maintainer for explicit accept, revise, or reject disposition before
editing current contracts or advancing Queue 32.

The candidate keeps the shallow grouped surface
`open-forge route move <source-reference> <destination-reference>` and
`open-forge route remove <reference>`. Its load-bearing rules are:

- both source and destination stay at the unmanaged ordinary logical-leaf
  boundary; managed, generated, projection-owned, lifecycle-root, reserved, and
  ambiguous subjects remain out of scope;
- positive ownership/lifecycle proof is required rather than absence-based
  inference;
- move is one atomic logical mutation, rewrites the complete in-scope `.agents`
  incoming-reference set, and blocks when incoming references outside `.agents`
  would be left stale;
- remove blocks on any incoming reference and never silently cascades;
- generated parent navigation is regenerated rather than hand-authored;
- base checks, overwrite refusal, repository cleanliness, recovery behavior,
  consent, diagnostics, and result states follow existing CLI contracts;
- consent names the command and exact subject; no `--force`, `--automatic`,
  `--yes`, generic `apply`, or silent overwrite path is added; and
- repeat semantics distinguish a provable already-consumed source from an
  ambiguous or unsafe missing source.

Do not silently accept details from this summary if the packet differs. The
packet is the review authority until maintainer disposition; current command
contracts remain unchanged during review.

## Validation boundary

A fresh Queue 29 integration review found and corrected inconsistent manifest-ID
inference, omitted dry-run apply-event boundaries, and stale historical
authority. Fresh workflow/program writing reviews found and corrected seven
workflow/Directive clarity gaps and two program/public-documentation gaps.

Final focused validation reported:

- 63 files, 693 local references, and 41 anchors with no broken target;
- Queue 29 accepted rules: 8/8;
- workflow/Directive correction rules: 10/10 with no stale command vocabulary;
- historical/public corrections: 4/4 with no stale Queue 30 claim or
  drive-qualified local path in public CLI/Extension documentation;
- `open-forge-old doctor`: no problems;
- `open-forge-old load --bodies`: success; and
- `git diff --check`: success.

These are prose, routing, generated-navigation, formatting, and static contract
checks only. They are not executable, Architecture, dependency, Native AOT,
package, CI, or release evidence.

## Architecture evidence retained for Gate 3

- Local SDK evidence is .NET 10.0.101; no `global.json` currently pins it.
- `.slnx` can be created with `dotnet new sln --format slnx`; exact SDK-default
  behavior should be pinned and tested rather than assumed.
- `System.CommandLine` claims trimming and Native AOT support.
- System.Text.Json should use source generation for Native AOT-safe paths.
- YamlDotNet has .NET 10/AOT-related source evidence but still requires
  application-level publish proof.
- Markdig supports .NET 10 but has no accepted Native AOT guarantee yet.
- xUnit v3 supports explicit display metadata, traits/query filtering, parallel
  controls, and Microsoft Testing Platform integration.

This evidence informs Architecture; it does not select a dependency.

## Resume sequence

1. Load the CLI Release Checkpoint and this handoff.
2. Read the Queue 31 packet and linked current route/reference/recovery
   contracts.
3. Present a concise decision summary and checklist; obtain explicit accept,
   revise, or reject disposition.
4. If accepted or revised, integrate Queue 31 into current contracts, Agenda,
   public docs, review/program records, and generated navigation; run focused
   review and validation.
5. Only then advance Queue 32. Queue 33 follows Queue 32, then Gate 2 closeout and
   the full Gate 3 Architecture discussion.

## Key files

- [CLI Release Checkpoint](../checkpoints/cli-release.md)
- [CLI Release Program](../cli-release/_cli-release.md)
- [Review Queue](../cli-release/review/queue.md)
- [Queue 31 Route Move and Remove](../cli-release/review/route-move-remove.md)
- [Queue 32 Cleanup](../cli-release/review/cleanup.md)
- [Queue 33 Completion](../cli-release/review/completion.md)
- [Decision Agenda](../cli-release/decision-agenda.md)
- [Release Plan](../cli-release/release-plan.md)
- [CLI Implementation Directive](../../../directives/open-forge/cli/implementation.md)
- [Development Workflow](../../../workflows/development/_development.md)
