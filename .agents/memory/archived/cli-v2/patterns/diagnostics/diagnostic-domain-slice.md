---
open-forge:
  description: "Historical CLI-v2 source: Keep one domain's facts, deterministic findings, safe repair proposals, planner, and tests together behind explicit doctor composition"
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# Diagnostic Domain Slice

Use this Pattern when one established domain contributes findings and safe
repairs to doctor.

## Boundary

The accepted [Diagnosis And Repair contract](../../../../memory/crystallized/documents/cli/contracts/diagnosis-and-repair.md) is authoritative for finding meaning, completeness, ordering, suggestions, safe proposals, conflict handling, repair, and rediagnosis. This Pattern owns the reusable domain slice and root-composition shape.

## Shape

```text
shared accepted facts
  -> diagnoseDomain()
  -> DomainReport
       findings
       completeness
       internal safe proposals
  -> planDomainRepairs(report)
  -> typed mutation purposes
```

The root coordinator directly imports both functions. Codes never locate them.

## Locality

Keep together:

- Domain finding-code const object and types.
- Typed subject and evidence.
- Pure or read-only diagnostic functions.
- Safe repair proposal variants.
- Proposal-to-plan function.
- Colocated direct tests and focused fixtures.

Move shared severity, completeness, resolution, action, and aggregation values
only to the CLI diagnostic common scope. Move a helper only after multiple
domains use the same behavior.

## Shape Constraints

- Return one typed domain report containing facts, findings, completeness, and directly related safe proposals.
- Keep diagnosis read-only and presentation-free. A separate proposal-to-plan function performs the typed transition into mutation purposes.
- Use a focused typed subject and evidence shape rather than a cross-domain bag of optional fields.
- Let the root coordinator aggregate reports and the root mutation planner detect cross-domain effect conflicts.

## Root Composition

```text
diagnoseWorkspace()
diagnoseRecovery()
diagnoseRoutes()
diagnoseReferences()
diagnoseFramework()
diagnoseExtensions()
```

Prefer this visible sequence over a string-keyed registry or reflection-based
discovery.

## Review

- Can a code string accidentally choose behavior?
- Is every safe proposal reachable directly from the report that created it?
- Can a manual suggestion become an effect without new authority?
- Does a malformed input remain a normal typed finding?
- Is partial coverage explicit?
- Are subject, evidence, and next action useful to both humans and agents?
- Can cross-domain conflicts block before the first write?
- Do exact finding and repair semantics come from the Diagnosis And Repair contract rather than this source layout Pattern?
