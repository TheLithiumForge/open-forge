---
open-forge:
  description: Define strict definition validation, release admission and paired workspace record checks
  tags: [Memory, Document, CLI, Contract, Shared, Tool, Manifest, Behavior, CurrentTruth]
---

# Tool manifest behavior
Tool manifest resolution is a read capability that validates exact source bytes before selecting a reviewed pin.

Format validity, release support and native testing are separate checks. Passing one doesn't give the others.

Status: V1 core contract decided.

## Resolution

The capability follows these steps:

1. Resolve a supplied source path from the process directory or select only the embedded catalogue.
2. Read exact ordinary source bytes without following catalogue-path links.
3. Decode JSON or YAML through the standard format parser and reject duplicate keys, unknown fields and forbidden YAML constructs.
4. Validate catalogue identities, nonempty version lists and the one default pin.
5. Resolve contained catalogue paths and hash exact manifest bytes before decoding them.
6. Validate all required manifest fields, tokens, paths, artifact references and target closure.
7. Match the requested ID and exact pin with the source's manifest identity.
8. Compare the exact tuple with the compiled release allow-list, then check adapter revision and target tests for activation.
9. Return typed definitions, requirements and coverage without network or filesystem effects.

## Workspace comparison

Read installed entries from settings independently of catalogue resolution. Compare each with lock receipts by exact ID, pin and manifest identity. Keep configured roles separate from verified install facts. Reject duplicate active-role assignments. A partial paired write is incomplete configuration and permits no new changes.

For updates, publish the verified replacement reference before releasing the prior reference. For removal, use stored snapshots and receipts. An unavailable reference set blocks final shared deletion. Never treat an unreadable store as empty.

## Conformance

Required cases are:

- JSON and YAML parity, duplicate keys, unknown fields, aliases and tags.
- Missing required values, invalid finite tokens and every native target.
- Exact SemVer strings, absent defaults and duplicate IDs, versions or paths.
- Digest and identity mismatches, unreadable bytes and escaping catalogue paths.
- Candidate, reserved, unreviewed and unavailable-adapter definitions.
- V1 graph guidance rejects a null, optional or disabled Skill.
- No source merging, directory scanning, network fallback or implicit pin upgrade.
- Omitted settings map, inactive roles and duplicate role rejection.
- Unknown unrelated settings keys kept through paired writes.
- Missing receipts and interrupted paired writes without silent host changes.
- Test-only synthetic definitions isolated from the production allow-list.

The [interface](interface.md) defines fields and caller outcomes.
