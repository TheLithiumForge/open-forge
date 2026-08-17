---
open-forge:
  description: Historical CLI-v2 source: Shared containment, identity, dispatch, persistence, verification, concurrency, and recovery boundaries for CLI filesystem effects
  responsibility: Define safe mechanical filesystem-effect semantics beneath Git-first workspace recovery and explicit Gitless backups
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Filesystem Effect Contract

## Scope

This document refines the filesystem portion of the [mutation execution contract](mutation-execution.md). It defines semantic guarantees rather than copying future TypeScript declarations or binding the implementation to one runtime API.

## Guarantees

### Typed And Navigable Dispatch

Effect kinds, target-boundary kinds, expected states, application states, verification states, and recovery states each have one named enum or readonly const object in production source.

Serializable discriminants describe data. They never resolve behavior through a string registry. Preflight, application, verification, and recovery dispatch each effect through an exhaustive typed branch to direct imported implementations.

Adding an effect kind must fail type checking in every dispatcher that has not intentionally handled it.

Each stage returns a discriminated result containing only its possible next states. A caller cannot manufacture a preflighted target, preflighted plan, or applied record through a boolean or workspace-owned type assertion.

### Target Boundaries

Every target belongs to one explicit typed boundary:

| Boundary            | Resolution                                                                                                     |
| ------------------- | -------------------------------------------------------------------------------------------------------------- |
| Workspace           | One portable workspace-relative logical path resolved beneath one selected logical and physical workspace root |
| Exact external file | One operation-authorized external identity, such as the profile returned by a supported shell resolver         |

An exact external target is not an arbitrary absolute path accepted from generic mutation input. Its owning operation resolves and authorizes it.

#### Logical Identity

A workspace target uses one non-empty, forward-slash path relative to the
selected workspace. It is already in Unicode NFC; the boundary does not
silently normalize, repair, or reinterpret input.

Unicode path segments are supported without transliteration or an ASCII-only
restriction. Each segment must still satisfy the portable logical rules and
enumerate with exact NFC spelling on the selected filesystem. A JavaScript
runtime's ability to represent a string does not override filesystem validity,
portable-name safety, or exact containment.

Reject absolute paths, drive changes, `.` or `..`, empty segments, control
characters, non-portable trailing characters, reserved device names, and
segments beyond the accepted portable limit.

At every existing parent, enumerate actual directory entries. The requested
segment must have one exact entry or be absent. A case or Unicode alias, two
entries with the same portable identity, or a path the filesystem resolves
without an exact entry blocks. Protected control paths remain
operation-authority policy rather than generic path syntax.

#### Physical Identity

Before planning accepts a target:

- Run a runtime-and-filesystem identity capability probe. Distinct files must
  produce distinct identifiers with usable precision. If that proof fails,
  mutation preflight blocks even when simpler identity tests happen to pass.
- Inspect the selected root without following a link, capture its real path and
  filesystem identity, and require an ordinary directory.
- Enumerate and inspect every existing target segment without following links.
  Symbolic links, junctions, special files, physical escapes, and unsupported
  device changes block even when they would resolve back inside the workspace.
- Use the filesystem device and file identifier as observed physical identity
  only after the capability probe succeeds. Zero, missing, colliding, rounded,
  changing, or otherwise unreliable identifiers block.
- For an absent target, capture the nearest existing parent identity and the
  complete missing suffix. Each missing directory remains its own later effect.
- For a present target, capture parent and target identities, real path, kind,
  link count, exact-byte fingerprint, and supported metadata.
- Require an ordinary mutation target to have one hard link. A typed
  executor-created link is allowed only in its known transient state and is
  removed as soon as its purpose ends.

Prepared sibling material and the target must share the filesystem device
needed by the accepted promotion primitive.

Revalidation repeats resolution and compares root, parent, and target
identities. Identity does not detect an in-place write, so target bytes are
fingerprinted independently. A changed identity, bytes, kind, link count,
metadata, or absent suffix blocks before the effect.

Portable APIs expose symbolic links and Windows junctions through `lstat`, but
they do not expose one complete cross-runtime classifier for every
platform-specific reparse, virtualization, network, or special filesystem
behavior. Support is limited to filesystems on which enumeration, `lstat`,
`realpath`, identity, hard-link, and metadata probes satisfy this contract.
Unknown semantics block when observable; the CLI is not a sandbox against a
hostile filesystem implementation.

Blocked evidence names whether the failure belongs to logical input, the
selected root, an ancestor parent, or the final target. Messages remain
legible; consumers do not infer location from a filesystem error string.

The exact source representation lives with the filesystem contract. Public results expose only the safe logical target and useful fingerprints.

### Mechanical Guarantees

#### File Creation

Creation never overwrites. The target transition is either:

```text
absent -> complete expected file
absent -> absent with a typed failure
```

No successful or failed invocation may expose a partially written final target. Temporary material is a contained sibling, created exclusively, verified before promotion, and linked to the absent target exclusively on a supported same filesystem. Unsupported link semantics block; there is no overwriting fallback. Temporary material is removed or reported as residual.

#### File Replacement

Replacement never truncates or writes the target in place. It prepares and verifies complete next bytes in a contained sibling, revalidates the expected original, moves the observed target into controlled recovery material, verifies the moved identity, promotes the next identity exclusively, verifies the result, and cleans only unambiguous temporary material.

The prepared sibling appends `.open-forge.next` to the complete target
filename. Controlled material preserving the observed target appends
`.open-forge.previous` to the complete target filename. For example,
replacement of `workflow.md` uses `workflow.md.open-forge.next` and
`workflow.md.open-forge.previous`.

Either existing sidecar blocks mutation before the target changes. Open Forge
never overwrites one or invents a numbered alternative. Successful application
or handled recovery removes each sidecar that is no longer needed. A hard stop
may leave either sidecar as visible residual evidence. These executor sidecars
are distinct from the user-owned Gitless `.bak` defined by the
[workspace recovery contract](workspace-recovery.md).

The target may be briefly absent between preservation and exclusive promotion.
A handled failure restores the preserved original in process. A hard stop in
that interval is recovered through Git or an already-created Gitless sibling
backup rather than a persistent Open Forge journal. This provisional visibility
tradeoff remains accepted because ordinary atomic rename can silently overwrite
a concurrent edit after final validation.

When promotion uses a hard link from prepared material, the executor removes
the executor-side link immediately after target verification. No temporary or
backup identity remains a live alias through which a later target edit could
mutate recovery material.

The final target is always a complete old, complete new, preserved concurrent, or explicitly reported residual identity. A target that appears after detachment is never overwritten; it and verified recovery material are retained as residuals. The executor never silently overwrites a target that it can prove diverged from the plan.

#### File Deletion

Deletion requires matching identity, exact bytes, and operation authority. The
executor captures the applied original for handled reverse recovery before the
target becomes absent. When the recovery policy requires a Gitless sibling
backup, that independent `.bak` must already exist and be verified. A handled
failure restores and verifies the original.

Deletion of divergent, unowned, linked, special, or ambiguously identified content is blocked.

#### Directory Creation

Directory creation is exact rather than recursive discovery. Every missing directory is its own planned effect. Recovery removes only the identity created by the plan and only while it remains empty.

Unchanged directories are observations.

### Metadata Boundary

Exact primary bytes and filesystem metadata are separate evidence.

For successful replacement:

- POSIX support preserves the observed owner, group, and permission mode or
  blocks before detachment when it cannot reproduce them.
- The portable Windows `node:fs` surface proves only the read-only versus
  writable state. Numeric owner and group values reported there are not treated
  as ownership evidence.
- Modification and change times describe the new content and are not restored
  to their before values. Rollback restores the original identity, which keeps
  its original metadata.
- Creation has operation-selected initial metadata because no prior target
  exists. Deletion preserves the detached original identity until commit.

Copying the original into prepared material may retain additional platform
metadata, but it is not a guarantee. On the initial Windows NTFS matrix, the
same `node:fs` copy retained a named alternate stream under Deno and dropped it
under Node.js and Bun.

The portable contract therefore does not claim preservation of ACLs, extended
attributes, alternate streams, encryption, compression, or other metadata that
the runtime cannot enumerate and verify consistently. Ordinary
workspace-contained Framework text may use the explicitly supported metadata
set. An exact-external mutation or another operation requiring richer metadata
must provide a platform capability that captures, reproduces, and verifies it;
otherwise preflight blocks.

### Applied Records

Application records one typed result for every started effect:

- Effect identity and kind.
- Exact observed transition.
- Applied target identity.
- Verification evidence.
- In-process recovery material or reconstructable recovery rule.
- Final effect state.

Recovery consumes applied records directly in reverse order. It never reconstructs rollback intent from public output or repeats planning.

### Concurrency Boundary

Preflight cannot authorize a later write permanently. The executor revalidates the complete plan immediately before application and revalidates volatile target facts immediately before every effect.

Cross-runtime promise filesystem operations are not synchronized with other
writers. Open Forge uses plan and per-effect revalidation rather than claiming
a process-wide lease. An editor, another Open Forge invocation, or another
filesystem actor may still change a target between observations.

Revalidation detects completed divergence at explicit boundaries; it is not a portable lease on an unrelated process's already-open file handle. A detected unknown identity is preserved. The CLI does not claim impossible isolation from undetectable writes after the last observation.

The archived filesystem mutation spike (`spike/cli-revamp-2026-07-31-155838`,
commit `7356ab5`) proves the candidate primitives on Windows NTFS under Node.js,
Bun, and Deno:

- Ordinary rename after validation overwrote a deterministic concurrent edit on all three runtimes.
- Preservation-first replacement restored an edit made before detachment.
- A target created after detachment was preserved with the verified before-state backup as an explicit residual.
- Exclusive creation, deletion, forced-stop recovery prototypes, and exact
  directory-lock contention completed as designed.
- Successful replacement exposed an observed absent-target interval of approximately 1.9–4.4 ms across the initial runs.
- The containment matrix rejected aliases, linked roots and ancestors,
  hard-linked targets, file-parent collisions, and swapped parents under every
  runtime. It also proved that identity revalidation does not replace byte
  fingerprinting.
- A repository-scale collision probe then found that Deno on Windows rounded
  distinct large NTFS identifiers to the same value. Deno remains supported,
  but an operation requiring exact identity blocks on that runtime/filesystem
  pair until an exact platform capability exists. Node.js and Bun passed the
  focused precision probe on the same host.

These measurements are evidence, not portable performance budgets. The current
matrix covers Windows NTFS only. Other supported operating systems,
representative filesystems, richer metadata capabilities, cancellation,
Gitless backup collisions, permission failure, hard-stop residual detection,
and packaged-artifact execution remain required implementation evidence. An operation requiring an unavailable
runtime or filesystem capability blocks through the ordinary typed result;
there is no unsafe fallback or alternate ABI.

### Recovery Levels

The accepted guarantees are intentionally visible:

| Level                  | Guarantee                                                                                             | Suitable use                                          |
| ---------------------- | ----------------------------------------------------------------------------------------------------- | ----------------------------------------------------- |
| In-process recovery    | A handled failure or cancellation reverts applied effects; a hard stop may leave complete mixed state | Every applied plan                                    |
| Deterministic rerun    | Complete derived state can be regenerated without private before-state                                | Generated route entries                               |
| Git-backed recovery    | The clean starting commit and resulting diff remain available for review or restoration               | Ordinary managed workspace mutations                  |
| Gitless sibling backup | Exact pre-effect bytes remain beside each replaced or deleted file                                    | Explicitly accepted Gitless or Git-bypassed mutations |

The [recovery contract](workspace-recovery.md) owns Git checks, backup naming,
hard-stop behavior, and residual evidence. The replacement initially creates no
persistent transaction journal or cooperative mutation lock.

## Boundaries

This contract does not authorize arbitrary external paths, claim isolation
from undetectable concurrent writes, treat runtime path representation as
filesystem validity, or promise metadata that the active platform cannot
enumerate and verify. Unknown identity, containment, link, filesystem, or
metadata semantics block before mutation. The initial replacement has no
persistent transaction journal or cooperative cross-process lock.

## Verification

Direct tests prove every typed transition, exhaustive dispatcher, logical path
rule, expected-state comparison, and recovery state. Real-filesystem tests
prove containment, identity precision, exact creation, preservation-first
replacement, deletion, directory recovery, metadata behavior, concurrent
divergence, and residual handling on each supported platform boundary.

The archived spike evidence establishes candidate behavior on Windows NTFS.
Release evidence must repeat the applicable cases against production code,
the packaged artifact, representative operating systems and filesystems, and
every supported runtime. An unavailable capability must produce the ordinary
typed blocked result rather than an alternate behavior.

## Related Current Sources

- [Mutation execution](mutation-execution.md)
- [Workspace recovery](workspace-recovery.md)
- [Runtime compatibility](runtime-compatibility.md)
