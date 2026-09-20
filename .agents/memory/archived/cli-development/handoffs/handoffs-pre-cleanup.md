---
open-forge:
  description: Sealed transfer snapshots that preserve one boundary for resumption
  tags: [Memory, Archived, Contextual, Historical, Handoff, AgentCommunication]
---

# Handoffs

## What state must survive this transfer or planned resumption?

A Handoff is a sealed snapshot for another reader. It preserves the boundary-specific state needed to resume after an actual transfer or explicitly planned resumption after a context boundary.

## Axioms

### Creation And Scope

- Read `Entries` when work is resumed, transferred, delegated, interrupted, or reviewed after a context break.
- Treat Handoffs as contextual transfer notes, not complete history or accepted truth.
- Create and seal a Handoff when an actual transfer or explicitly planned resumption needs a stable boundary snapshot while the active Checkpoint may continue to change.
- Do not create one for a routine pause, ordinary closeout, or possible future interruption.
- Make the intended reader or resumed work clear from the route, description, or content.

### Content And Lifecycle

- Keep Handoffs short. Link to current state, durable sources, code, or other details instead of copying them.
- Record the boundary status, next action, blockers, and verification state in the Handoff itself. A live Checkpoint may supplement but not replace this snapshot.
- Keep the snapshot unchanged while it serves as a sealed Handoff. Record later state in the active Checkpoint or a new Handoff.
- When a Handoff no longer supports an active transfer, keep any useful result and archive it.

## Entries

Current transfer: [Workspace Libraries Sol implementation](2026-09-07_library-sol-implementation-handoff.md).
Read it first, then its linked frozen Gray/Red handoff and detailed preflight. Generated navigation refresh currently reports
`index.metadata-incomplete` for an existing direct child; preserve the sealed
historical files while resolving that separate metadata issue.

<!-- open-forge:generated-index:start -->

- [Historical CLI release Gate 2 handoff for queue item 31](2026-08-16_cli-release-gate-2-queue-31.md) - #Memory #Handoff #Archived #Historical
- [Historical CLI release Gate 2 handoff for queue item 32](2026-08-16_cli-release-gate-2-queue-32.md) - #Memory #Handoff #Archived #Historical
- [Historical CLI release Gate 2 handoff for queue item 33](2026-08-16_cli-release-gate-2-queue-33.md) - #Memory #Handoff #Archived #Historical
- [Historical CLI release Gate 3 start handoff](2026-08-16_cli-release-gate-3-start.md) - #Memory #Handoff #Archived #Historical
- [Historical CLI release Gate 4 completion handoff](2026-08-17_cli-release-gate-4-complete.md) - #Memory #Handoff #Archived #Historical
- [Sealed continuation state for Find Child 2 after Gray acceptance and during incomplete Red evidence authoring](2026-08-24_cli-find-query-red-start.md) - #Memory #Handoff #CLI #Find #Red #Contextual #Archived #Historical
- [Sealed continuation state for accepted Find before local integration and equality proof](2026-08-25_cli-find-accepted.md) - #Memory #Handoff #CLI #Find #Acceptance #Contextual #Archived #Historical
- [Handoff for resuming Route Create Green and later protected integration](2026-08-31_cli-route-create-green.md) - #Memory #Handoff #CLI #Route #Development #Evidence #Git #Archived #Historical
- [Sealed restart snapshot for the active CLI Doctor correction and the ordered replacement-CLI queue](2026-09-05_cli-doctor-restart.md) - #Memory #Handoff #CLI #Doctor #Restart #Contextual #Archived #Historical
- [Exact dirty path and content identities captured for the CLI Astra restart](2026-09-07_cli-astra-restart-inventory.md) - #Memory #Handoff #CLI #Evidence #Contextual #Archived #Historical
- [Resume the CLI program after the Astra transfer with re-enabled tasks and preserved unfinished worktrees](2026-09-07_cli-astra-restart.md) - #Memory #Handoff #CLI #Contextual #Archived #Historical
- [Resume Workspace Libraries Green from qualified frozen contracts and Red after the requested halt](2026-09-07_library-green-handoff.md) - #Memory #Handoff #Contextual #CLI #Library #Red #Archived #Historical
- [Transfer frozen Workspace Libraries Green implementation to Sol before returning to the Astra Overseer](2026-09-07_library-sol-implementation-handoff.md) - #Memory #Handoff #Contextual #CLI #Library #Implementation #Archived #Historical
- [State after the CLI experience audit and the first phase of fixes, with what is decided, what is open, and where the next session starts](2026-09-10_cli-experience-audit.md) - #Memory #Handoff #Contextual #CLI #Audit #Remediation #Archived #Historical

<!-- open-forge:generated-index:end -->
