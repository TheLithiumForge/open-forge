---
open-forge:
  description: "Historical CLI-v2 source: Prefer preservation-first file mutation over ordinary atomic replacement when concurrent unknown bytes could otherwise be lost"
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Preservation-First File Mutations

## Context

An ordinary complete-file rename gives readers continuous old-or-new visibility,
but no portable compare-and-swap guarantee exists between final validation and
promotion. A concurrent edit in that interval can be silently overwritten.

The focused Windows NTFS spike made that race deterministic. Node.js, Bun, and
Deno all accepted the rename and replaced the concurrent bytes.

## Decision

Use preservation-first transitions for replacement and deletion:

1. Prepare complete next bytes and every required Gitless sibling backup.
2. Revalidate the expected target.
3. Move the observed target to controlled recovery material.
4. Verify that moved identity before continuing.
5. Promote the prepared file exclusively, without overwriting a target that
   appeared after detachment.
6. Verify the result and retain ambiguous identities as residuals.

Creation promotes a complete sibling through an exclusive same-filesystem link.
Unsupported link or identity semantics block instead of falling back to
in-place writing or an overwriting rename.

After hard-link promotion and verification, remove the executor-side link
immediately. Keeping it would allow a later in-place target edit to mutate
temporary recovery material through their shared identity.

Replacement may expose a short absent-target interval. This is accepted
provisionally because transient unavailability is safer than silent loss of
unknown authored bytes. A handled failure restores the preserved identity;
hard-stop recovery uses Git or an already-created sibling backup.

## Boundary

Plan and per-effect revalidation detect completed divergence at the tested
boundaries, but no portable
primitive can guarantee preservation from a process that continues writing
through an already-open file handle after validation. Unknown or ambiguous
identities are preserved whenever detected.

## Evidence And Reversal

The archived filesystem mutation spike (`spike/cli-revamp-2026-07-31-155838`,
commit `7356ab5`)
executes real child processes under all three runtimes. On the initial Windows
NTFS runs, the preservation-first replacement detected early divergence,
preserved a late concurrent target plus before-state backup, and recovered
forced stops before and after promotion.

The observed absent-target interval was approximately 1.9–4.4 ms. This is
evidence, not a performance budget or universal probability.

Reconsider the primitive if supported-filesystem tests find unacceptable reader
behavior or a portable mechanism proves both continuous old-or-new visibility
and non-overwrite of a target changed after validation.

## Authoritative Sources

- [CLI filesystem effect contract](../../documents/cli/contracts/filesystem-effects.md)
- [CLI recovery contract](../../documents/cli/contracts/workspace-recovery.md)
