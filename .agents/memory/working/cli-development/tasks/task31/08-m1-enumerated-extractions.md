---
open-forge:
  description: Task 31 M1 execution plan enumerating every remaining safe extraction and permanently retiring the unsafe ones
  tags: [Memory, Working, CLI, Task, Plan, Contextual, Active]
---

# M1 — Enumerated extractions

> Read [00 — Slice conventions](../task30/00-conventions.md) first: verification
> commands, pass conditions, the recurring composition pattern, and the rules
> every slice shares. This plan does not repeat them.

> Historical M1 record: its dependency statements predate the completed G4
> packet. The M1 evidence remains valid; current Task 31 state is in the parent
> record, with M3 complete and M5 queued.

## Goal

Close the remaining within-family duplication as an explicit list. Every group is
already judged safe or unsafe here, so execution needs no judgement.

## Depends on / Blocks

- Depends on: A6, per the standing rule not to touch lifecycle-adjacent code
  while G1 is unresolved. Groups 2, 3, 4 and 6 below touch no lifecycle code and
  may proceed earlier if the maintainer accepts.
- Blocks: nothing. Independent of G4.

## Measurement

Method bodies in `Commands/{Route,Library,Extension,Cleanup,Repair,References}`,
normalized for whitespace, 6 to 80 lines, over 220 characters. Identical bodies
appearing more than once: **6 groups**. Measured 2026-09-12 at `19775bf4`.

No group spans two families, so no cross-family promotion is implied. `Cleanup`
and `Extension` produced no groups.

## Do

### [x] 1. Library `Validate()` — 3 copies, 25 lines

- `Library/Attach/Models/Result/LibraryAttachCompletionInput.cs`
- `Library/Detach/Models/Result/LibraryDetachCompletionInput.cs`
- `Library/Sync/Models/Result/LibrarySyncCompletionInput.cs`

**Check first**: confirm the three inputs expose the validated members through a
shared shape. If they do not, this is three similar bodies over three unrelated
types, which is not a duplicate — retire it and record that here.

If they do: extract to one validator under `Library/Shared/`. Owner is Library.
Verify: `npm run build`, integration suite green.

### [x] 2. Route `ToRouteDocument` — 2 copies, 10 lines

- `Route/List/Shared/Filesystem/RouteListSourceProjectionBuilder.cs`
- `Route/Inspect/Shared/Resolution/RouteInspectSourceProjectionBuilder.cs`

One mapping from a source read to a `RouteSourceDocument`, two consumers. Extract
to `Route/Shared/`. Owner is Route. Verify: `npm run build`, integration green.

### [x] 3. Route `ReadChain` — 2 copies, 35 lines

- `Route/Inspect/Shared/Profile/RouteInspectAxiomsProfileBuilder.cs`
- `Route/Inspect/Shared/Profile/RouteInspectTopologyProfileBuilder.cs`

Both already sit under `Route/Inspect/Shared/Profile/`, so the owner is not in
question. Largest single win. Extract to a shared profile helper in that folder.
Verify: `npm run build`, integration green.

### [x] 4. References `ToSource` — 2 copies, 11 lines

- `References/ReferencesOperation.cs`
- `References/Shared/Extraction/ReferencesLinkExtractor.cs`

Same family, same mapping. Extract to `References/Shared/`. Owner is References.
Verify: `npm run build`, integration green.

## Do not

### [x] 5. Route `Safety()` — identical, and must stay identical separately

- `Route/Inspect/Shared/Rendering/RouteInspectHumanValues.cs`
- `Route/Inspect/Shared/Rendering/RouteInspectJsonNames.cs`

**Retired, permanently. Do not merge.** The two bodies are byte-identical today
and semantically distinct: one is the human display vocabulary, the other is the
JSON wire vocabulary. The wire value is public contract; the human string is
prose the Writing Standard may reword at any time. Merging couples a contract to
a sentence.

This is the same trap as Route Move and Route Remove, whose finding-code enums
differ by destination behaviour and ship in JSON. Coincidental identity is not
duplication.

### [x] 6. Library `PlanState()` — needs a design change, not a move

- `Library/Sync/Shared/Planning/LibrarySyncPlanner.cs`
- `Library/Detach/Shared/Planning/LibraryDetachPlanner.cs`

The bodies are identical but the parameters are not: one takes
`LibrarySyncFinding`, the other `LibraryDetachFinding`. Extracting requires a
shared finding abstraction, which is a design change with its own contract
consequences, not a behaviour-preserving move.

**Deferred, not retired.** Revisit only if a shared library finding model is
accepted for another reason.

## Expected result

Four extractions land, roughly 70 lines of duplication removed, with two groups
permanently decided so nobody measures them again.

## Acceptance

- [x] Each extraction is behaviour-preserving: zero output diff and the directly
      affected tests pass unchanged.
- [x] No finding code, status, diagnostic wording, request shape, or JSON field
      changes. If one would, stop and record it as a contract decision.
- [x] Groups 5 and 6 are left alone, with their reasons recorded above.
- [x] Three suites green.

## Divergences observed

- **Order:** ran M1 after qualified P1 and before B1 because B1's repository
  precheck found a protected note without metadata. The active dependency plan
  allows the two independent slices in either order; no G4 work is included.
- **Group 1 retired:** Attach, Detach and Sync have distinct Request, Plan and
  PlanningInput types and no shared input shape. Only Execution is shared and
  already owns its validation. Followed the conditional instruction to retire
  the group; no interface, adapter or shared finding model was invented. The
  expected four extractions therefore becomes three.
- **Group 2 owner:** the existing RouteSourceProjector already owns Route source
  formation. Place the identical orphan-overwrite mapping there with its exact
  missing-read/cancellation fallback. Its existing general document mapping has
  different preconditions and remains separate.
- **Characterization:** captured 24 real published outputs before changes for
  Route List, Route Inspect (nested entrypoint, ordinary source and orphan
  overwrite), and References (entrypoint and paired source), in both formats
  and views. The fixture includes ancestor Axioms, an overwrite and a missing
  reference. `artifacts/m1-capture.py` owns the bounded capture;
  `artifacts/m1-characterization-before.json` owns exact inputs, exits and streams.
  These replace no historical harness and make no 297-output equivalence claim.
- **Format baseline:** ReferencesOperation is an explicitly enumerated M1 site,
  despite the convention saying no slice touches it. Preserve its two inherited
  whitespace diagnostics while extracting only the mapping below them.

- **Gate scope:** this leaf moves three identical bodies inside their existing
  command families, with no serialization, dependency, data-shape or runtime
  capability change. Run the four convention checks and unchanged affected
  evidence here; select the next full managed/Native AOT integration-wave gate
  after B1, before G4, as the implementation directive permits. The qualified P1
  native binary supplies the before-output baseline, not an M1 native claim.
  M1 does not need a separate repeat of the native integration wave.
- **Review scope:** a differently shaped ReadChain in the loading-facts builder
  is outside the enumerated group and stays local. Route Move/Remove, human/wire
  Safety vocabulary and Library finding/plan types are untouched. No new tests
  mirror these moved bodies; the existing tests run unchanged.

## Final Verification

Build `artifacts/m1-managed-build.log`: zero warnings/errors. Unit:
3349 succeeded, failed 0, skipped 0 (`artifacts/m1-managed-unit.log`). Integration:
1846 succeeded, failed 0, skipped 17 (`artifacts/m1-managed-integration.log`).
`artifacts/m1-check-dotnet.log` reports exactly the five inherited whitespace
errors, exit 2; its chained analyzer did not run. EndToEnd: 131 succeeded, failed 0, skipped 0
(`artifacts/m1-managed-e2e.log`). No cancellation-flake rerun was needed.

The 24 published outputs are byte-equal after line-ending normalization:
`artifacts/m1-characterization-managed-after.json` equals its before record,
including input bytes, command exits and both streams. This includes complete
nested topology/Axioms and paired layers, incomplete missing-reference evidence,
and a blocked orphan overwrite. Existing immutable snapshots all passed.

Managed source/artifact identity is recorded in
`artifacts/m1-accepted-managed-identity.json` before staging: nine affected Core
files, all qualified assembly hashes and identical before/after capture hashes.
The published Core assembly SHA-256 is `35b60fb8a48773a185beddd9e61e62eb541b3fcacf59b4181ab436a4360b358a`.
No source, test, fixture or configuration changed after the qualifying build.
The final source diff and new files were reviewed; all tests and snapshots are
unchanged. No push or integration was performed. At M1 closeout, the next boundary was B1's protected-note precondition,
followed by its native integration wave. Both are now closed below.

The requested [progression report](../task30/progression-report.md) is now routed.
Scoped Index successfully verified two generated regions while publishing its
entry (`artifacts/m1-report-index.json`); the diff contains that entry and the
current marker formatter's blank-line normalization only. This scoped success
does not resolve or claim a passing repository-wide B1 precondition.

## B1 Integration-Wave Closeout

[B1](../task30/07-b1-heading-entries.md#verification-receipt) subsequently completed
its managed and supported Windows native qualification, including these M1
extractions: 3366 Unit, 1849 Integration with 17 skips, and 131 EndToEnd succeeded
in each lane, with zero failures and no other skips. Counts are informational.
The same five inherited format errors remain; the chained analyzer did not run.
B1 owns the final source/artifact identities and exact reproduction commands.
The agreed stop is before G4; no renderer or M3 work has begun.

## Rollback

Each extraction is independent. Revert any one without touching the others.
