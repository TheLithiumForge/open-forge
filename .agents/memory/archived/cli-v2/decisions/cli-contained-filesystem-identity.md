---
open-forge:
  description: "Historical CLI-v2 source: Require portable logical identity, physical containment, byte evidence, and proven runtime capabilities for replacement CLI filesystem effects"
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Contained Filesystem Identity

## Context

Lexical path comparison cannot detect aliases, linked ancestors, mounted
devices, hard links, or a replaced parent. File identity alone cannot detect
an in-place content edit, and replacement may change both identity and
supported metadata.

## Decision

Mutation targets require separate portable logical identity, physical
containment and identity, exact-byte evidence, and a deliberately bounded
metadata guarantee. Volatile facts are revalidated immediately before use.
When the runtime or filesystem cannot prove a required capability, the
operation blocks through the same typed interface rather than using a heuristic
fallback.

The [Filesystem Effect
Contract](../../documents/cli/contracts/filesystem-effects.md) owns the exact
resolution, containment, metadata, revalidation, application, and recovery
semantics. The [Runtime Compatibility
Contract](../../documents/cli/contracts/runtime-compatibility.md) owns capability
parity and blocking.

## Rationale

The boundary protects the user's actual file rather than merely the spelling
of a requested path. Separating identity, bytes, and metadata prevents one
successful observation from being mistaken for a complete fidelity claim.
Blocking an unavailable guarantee is safer and more predictable than silently
changing behavior by runtime.

## Alternatives And Tradeoffs

- Lexical containment is fast but cannot prove physical containment.
- Size and timestamp heuristics are portable but insufficient for identity or byte safety.
- In-place writes preserve identity but risk truncation and hidden concurrent edits.
- Claiming complete platform metadata preservation would exceed the proven JavaScript boundary.

The accepted checks make some otherwise possible writes unavailable on weak
runtime/filesystem combinations. They do not claim sandbox isolation against a
hostile process after the final observation.

## Evidence

Windows NTFS spikes under Node.js, Bun, and Deno rejected the tested
containment hazards and proved that in-place byte edits retain identity. A
repository-scale probe then found Deno inode precision insufficient on the
tested host, validating the explicit capability gate. Copy experiments did
not establish richer portable metadata preservation.

## Consequences

- Paths receive no hidden correction.
- Physical identity, byte identity, and supported metadata remain separate evidence.
- Same-device support and identity precision are preflight requirements.
- Unsupported exact external replacement remains blocked until its guarantees are proven.
- Divergence is preserved and reported rather than overwritten under a false isolation claim.

## Related Sources

- [Filesystem Effect Contract](../../documents/cli/contracts/filesystem-effects.md)
- [Runtime Compatibility Contract](../../documents/cli/contracts/runtime-compatibility.md)
- [Contained Filesystem Target Pattern](../../../../patterns/open-forge/cli/filesystem/contained-filesystem-target.md)
