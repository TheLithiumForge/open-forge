---
open-forge:
  description: Historical CLI-v2 source: Resolve a mutable filesystem target through canonical logical identity, exact ancestor enumeration, physical identity, and explicit revalidation
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# Contained Filesystem Target

Use this Pattern when a CLI operation turns a workspace-relative path into a
mutable filesystem target.

## Boundary

The accepted [Filesystem Effects contract](../../../../memory/crystallized/documents/cli/contracts/filesystem-effects.md) is authoritative for canonicalization, containment, supported identities, blocking conditions, target observations, revalidation, and persistence guarantees. This Pattern owns the reusable resolver and observation pipeline.

## Shape

```text
raw logical path
  -> canonical syntax result
  -> runtime/filesystem identity capability
  -> selected root observation
  -> exact segment enumeration
  -> physical ancestor observations
  -> absent or existing target observation
  -> operation-specific authority and byte checks
  -> preflighted target
  -> complete revalidation immediately before use
```

Each transition returns its actual discriminated alternatives. Import the next
function directly and branch exhaustively on named values. Do not connect
stages through string registries, unchecked casts, or boolean validation flags.

## Observations

Keep these concepts distinct:

- Logical identity: canonical workspace-relative name.
- Portable identity: conservative case and Unicode collision key.
- Physical identity: filesystem device and file identifier.
- Content identity: exact-byte fingerprint.
- Supported metadata: the explicit platform contract, not every stat field.

An absent target records its nearest existing parent plus missing suffix. An
existing target records root, parent, and target. Both retain only internal
physical paths; public results expose safe logical evidence.

## Shape Constraints

- Keep public grammar normalization in a focused syntax step before physical resolution.
- Enumerate exact ancestor names and produce immutable observations rather than passing an unchecked constructed absolute path.
- Put runtime and filesystem identity capability behind one explicit probe whose result travels with target evidence.
- Revalidation calls the same resolver and observation comparison with fresh facts immediately before the accepted effect stage.
- Keep generic containment and identity separate from operation-specific ownership, protected-path, and byte authority.

## Review

- Can definition lookup reach every named state and transition directly?
- Does absence bind to an observed parent identity?
- Does presence bind to parent, target, bytes, link count, and metadata?
- Has a capability probe proven that identity values are distinct and precise?
- Will an in-place edit be detected even though identity stays stable?
- Does a case, Unicode, filesystem, link, or device alias block?
- Does revalidation repeat all volatile facts immediately before mutation?
- Are exact blocking and fallback semantics obtained from the Filesystem Effects contract instead of being redefined here?
