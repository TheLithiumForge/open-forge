---
open-forge:
  description: "Historical CLI-v2 source: Inspect third-party source as escaped positional facts and keep source trust separate from subject selection and mutation authority"
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# Reviewed Source Boundary

## Boundary

The accepted [Source Review contract](../../../../memory/crystallized/documents/cli/contracts/source-review.md) is authoritative for source closure, finding classes, inspected constructs, trust decisions, escaped presentation, blocking behavior, and fingerprint-bound authority. This Pattern owns the reusable inspection and transition arrangement.

## Shape

```text
selected subjects
  -> resolve immutable source closure
  -> inspect bytes without rendering or execution
  -> typed positional findings
  -> dedicated source-review transition
  -> ordinary mutation planning
```

Selection, source review, and mutation authority remain distinct typed transitions. A complete reviewed-source value contains the exact immutable source identities and fingerprints that were inspected; downstream planning accepts that value rather than an unrelated confirmation boolean.

Keep one focused finding representation containing its named class and kind, logical source identity, source range, escaped excerpt, and explanation. Scanner-specific state remains private behind that stable fact boundary.

The human presenter consumes the same findings as structured output. It renders already escaped positional evidence and returns only one permitted source-review decision. It does not grant overwrite, deletion, executable, or recovery authority.

## Placement

Keep source-closure resolution, document inspection, finding projection, safe excerpt rendering, and review-value construction as directly connected focused modules. Mutation planners depend on the reviewed value but do not own the source scanner or review presenter.

## Review Checks

- Selection, source trust, and mutation authority are separate types and calls.
- The reviewed value binds authority to the exact inspected source closure.
- Findings preserve stable positions and already escaped evidence.
- Rendering cannot execute or reinterpret source.
- Scanner implementation details do not leak into lifecycle planners.
- Exact finding and decision semantics are linked to the Source Review contract instead of restated here.
