---
open-forge:
  description: "Historical CLI-v2 source: Use Git-first workspace recovery, in-process reversal, and adjacent Gitless backups without a persistent Open Forge transaction journal"
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Git-First Recovery Policy

## Context

The filesystem spikes proved that a durable journal can recover forced
termination around preservation-first mutations. They also showed the cost:
transaction state, synchronization, cleanup, process-liveness handling,
corruption policy, platform limitations, and a second persistent recovery
surface in every managed workspace.

Open Forge already optimizes installation and managed change around Git review.
Most users have Git, generated navigation is reconstructable, operations are
short, and Gitless users can receive visible adjacent backups. The stronger
journal therefore protects a low-probability tail by adding permanent
complexity to the ordinary path before production evidence justifies it.

## Decision

Use one explicit hybrid without a persistent Open Forge journal:

- Git is the primary durable workspace review and recovery mechanism.
- Relevant dirty paths block by default.
- `--skip-git-check` is a distinct explicit bypass, not confirmation or
  overwrite authority.
- Handled failure and cancellation recover applied effects in process.
- Reconstructable generated state recovers by deterministic rerun.
- Gitless or bypassed destructive changes recommend adjacent `.bak` files.
- `.agents/open-forge.json` is written atomically as the final persistent
  effect; it contains no transaction state.
- Exact external mutations need their own accepted recovery boundary.

The archived spike branch retains the journal evidence. Production scenarios
may be recreated from it later without promoting prototype architecture.

## Rationale

This policy makes safety visible through ordinary tools that users already
understand. It minimizes repository noise, avoids stale locks and hidden
cleanup state, and keeps recovery proportional to the operation.

It does not pretend Git makes atomic application unnecessary. Complete
planning, effect preflight, revalidation, complete-file writes, verification,
and reverse recovery remain mandatory. Git and backups cover the hard-stop and
review boundary that process-local mechanics cannot.

## Consequences

- Installation teaches Git, may initialize it, and asks for review before an
  optional explicit commit.
- Gitless use remains supported with a clear weaker guarantee.
- Existing `.bak` files are never overwritten or renumbered implicitly.
- A hard stop may leave complete mixed state; doctor reports evidence and the
  user recovers through Git, backups, deterministic rebuild, or review.
- The CLI initially ships no transaction directory, cooperative mutation lock,
  recovery daemon, or later-invocation rollback protocol.
- `--yes`, `--overwrite`, `--skip-git-check`, and formatter `RUN` approval
  remain separate authority; blocked formatter execution cannot be confirmed.
- A durable journal may return only after measured production failures show
  that Git-first recovery is materially insufficient.

## Authoritative Sources

- [CLI recovery contract](../../documents/cli/contracts/workspace-recovery.md)
- [CLI filesystem effect contract](../../documents/cli/contracts/filesystem-effects.md)
- [CLI architecture](../../documents/cli/architecture.md)
