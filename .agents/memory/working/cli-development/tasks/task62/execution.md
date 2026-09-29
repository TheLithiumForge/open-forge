---
open-forge:
  description: Execute Task 62 through frozen shared contracts, isolated parallel slices, integration, and managed and Native AOT evidence
  tags: [Memory, Working, Task, Plan, CLI, Contextual, Active]
---

# Task 62 execution

## Accepted outcome and authority

The maintainer accepted all recommendations in the [analysis](../../../../emerging/analysis/glob-scoped-loading.md)
on 2026-09-29 and authorized detailed parallel planning followed by parallel
implementation. [Task 62](../task62-glob-scoped-loading.md) retains the outcome.
Task 55 remains separate. The accepted analysis is frozen as input evidence.
Current contracts and this packet own subsequent implementation decisions.

## Execution capsule

- Profile: Standard with a shared-foundation integration gate.
- Horizon: phase 1/3 planning and contracts, phase 2/3 implementation, phase 3/3
  integration and qualification. Six milestones: packet freeze, foundation,
  authoring/navigation, retrieval commands, documentation, full qualification.
- Current status: Task 62 “Glob-scoped loading” (phase 3/3): milestone 6/6.
  Implementation, documentation, independent review and full qualification are
  complete in the isolated worktree. Ready for maintainer review and integration.
- Review budget: one independent whole-change review after integration, one
  grouped correction pass, focused rechecks of corrections. Council budget zero.
- Base: d7e8a6d261dd85ac4086584121758ff143640226.
- Integration branch: task62-applyto, isolated from the original develop checkout.
- Scope: optional applyTo metadata, matching, Entries, selected commands and docs.
- Protected: alternative roots/APM integration, unrelated local work, release,
  external effects, dependency versions, project graph, existing mutation safety.
- No worker commits, pushes, merges, installs dependencies from the network, or
  changes another slice's files. Preserve changed/untracked artifacts.
- Workers use separate branch/worktrees from the named base. Foundation outputs
  are copied by exact declared paths into dependent worktrees before dispatch.
  Record the copied source identities and inspect diffs before integration.
- Root integrates exact owned file changes into the task worktree. This is
  reversible local assembly, not an authorized merge into develop.
- Consequence: wrong context selection or damaged authored metadata. Existing
  mutation planning, expected-state checks, locking and recovery remain intact.
- Standard capabilities: YAML/Markdown parsing stays in Documents through current
  libraries. Matching uses BCL FileSystemName.MatchesSimpleExpression for segments
  and bounded product traversal for standalone **. No new dependency or general
  glob engine is required.
- Tests: pure parsing/matching at Unit, file preservation/traversal at Integration,
  a small set of CLI arguments/output journeys at EndToEnd. Shared/public changes
  trigger final complete managed and supported host Native AOT gates.

## Frozen behavioral decisions

1. Optional applyTo at frontmatter root or open-forge scope has identical meaning.
   Both locations are read even when a scoped block exists. Existing requirements
   for description/tags and native Skill exceptions remain unchanged.
2. Accept quoted scalar or quoted-string list. Missing is unconditioned, empty
   and non-string values invalid. Equivalent normalized dual declarations count
   once. Conflicting declarations are invalid, never silently unioned/preferred.
3. New fields use a scoped quoted list. Updates preserve authored placement and
   unrelated bytes; equivalent dual declarations update/clear together.
4. Pattern syntax: slash-separated workspace-relative paths; * and ? within a
   segment; ** only as an entire segment and matching zero or more segments.
   Case-sensitive everywhere. Reject absolute/drive paths, backslash, dot or
   parent segments, empty segments, controls, braces, brackets, leading negation,
   and embedded consecutive stars. No comma splitting or escape syntax.
5. OR within a list; AND across ancestor conditions on the SAME working file;
   OR across supplied working files. No condition inherits ancestor condition.
6. Glob changes applicability, never edit permission. Related in-scope work adds
   its file path and loads its own context. Merely reading a context source does
   not make its Markdown path a working file or expand the source's applicability.
7. Visible matching entries load before working on matched files. Hidden ancestors
   remain unselected. Explicit selection establishes its chain. Reference/explicit
   inspection can retrieve a nonmatch without activating its automatic children.
8. LoadNow retains first-load meaning behind the condition. KeepInMind adds normal
   refresh while paths remain in task scope. Unknown working files defer conditions
   and are reported as pending, not mismatched or universally complete.
9. Entries append: link, existing tags, then ` - applies to` with comma-separated
   code spans. Project declared patterns only. Unconditioned lines retain bytes.
10. Context, Find and Route Inspect receive repeatable --for concrete paths.
    CLI is stateless, supports planned nonexistent paths, rejects paths outside
    selected workspace, and does not infer Git changes or a dependency graph.
11. Context --additions-only still requires explicit source operands. Use the same
    complete working-path set to calculate filtered startup and filtered combined.
12. Find --for adds file compatibility filtering, independent of tag/heading
    --require; inherited conditions require ancestor facts without widening the
    returned source universe. Sources with no effective condition are compatible.
13. Create/Init/Update use repeatable --apply-to. Update uses --clear-apply-to,
    mutually exclusive with --apply-to. Init retains its existing restrictions
    for metadata flags, including --framework and existing final targets.
14. Index never filters its inventory by the task's working set. Doctor checks all
    normal metadata regardless of file matches. Valid globs matching no current
    disk file are not errors. Status has no new public option or required schema.
15. No applyTo source should break existing behavior on unconditioned routes.
    Overwrite companions share base applicability and load immediately after base.
16. Documentation keeps Directive LoadNow mandatory while allowing applyTo as
    its pre-load file condition. Reconcile obsolete no-second-gate wording.

## Work graph

- P1–P4: parallel Astra investigation of foundation, retrieval, authoring, docs.
- F1: pattern model/parser/matcher and focused tests, independent.
- F2: typed applyTo YAML facts and source projections, depends on F1 API.
- F3: Entries parser/model/emitter and parity, depends on F1/F2 API.
- D1: accepted Framework/Loader contracts and local/shipped rules, parallel with F.
- A1–A3: Create, Init and Update commands after foundation.
- V1: Doctor validation and focused tests after foundation.
- C1: shared route condition evaluation and Context after foundation.
- C2: Find and C3: Route Inspect after shared route condition API freeze.
- D2: command contracts, D3: public docs/diagram alongside command slices.
- I1: integrate exact outputs in dependency order and run coherent build.
- Q1: focused new scenarios and affected regressions, then full managed and AOT.
- R1: one independent review, grouped owner repairs, affected rechecks.
- D4: reconcile final docs, index/doctor, receipts and task state.

Each detailed slice packet must name exact owned paths, direct consumers, callable
dependencies, test commands, stop conditions and receipt. Workers must independently
read AGENTS/loader/applicable routes and complete C# Directives when touching C#.
No worker may infer another slice's API or copy its policy locally.

## Initial evidence and limitations

- Main checkout had seven planning changes from prior discussion and one unrelated
  untracked installed-beta-flow-run.md. Only planning inputs copied to task tree.
- Worker Watch launcher fails because its installed program is missing. Native
  agent tools provide isolated planning/implementation instead.
- Offline .NET restore succeeded in the integration worktree. First delivery
  build invocation requires the already-installed local semver dependency.
- Existing workspace Doctor reports reference.target-alias defects before this
  task. Preserve this baseline; targeted new route/link checks must still pass.
- Verification receipts and remaining actions are appended here as slices finish.

## Proportionality and evidence applicability

This changes local context selection and authored Markdown for Open Forge users.
An incorrect filter can omit instructions; an incorrect Update can damage source
metadata. Changes are recoverable through the isolated Git worktree, existing
mutation recovery, source backups and regenerated Entries. Existing planning,
expected-state checks and cooperating-process locks remain in force. Malformed
input, ordinary defects, interruption, dependency failure and accidental
concurrency are supported concerns. A hostile same-user process is outside the
existing threat boundary; this task adds no security or authorization guarantee.

The pinned runtime, BCL, Markdig, YamlDotNet and test platform provide the required
capabilities. Shared YAML syntax facts and serialization stay in Documents;
Framework owns identical pattern and condition meaning needed by the six command
consumers. Command options, selection policy, statuses and presentation remain
with their existing owners. Exceptional machinery: none. Segment matching plus
bounded traversal for the accepted standalone `**` grammar is ordinary product
logic, not a replacement filesystem glob engine.

Unit evidence covers parsing, exact condition algebra and binding. Integration
evidence covers authored-byte preservation, route traversal and generated Entries.
A small set of process journeys proves public options and output. Shared metadata,
serialization, loading and public composition trigger the complete managed and
supported Windows Native AOT gates after coherent integration. The complete gate passed. Focused receipts below explain the development
evidence, while the final qualification table records the coherent result.

## Active ownership and receipts

- Maintainer requested Luna max during execution. All active implementation and
  writing lanes were checkpointed and resumed with `gpt-6-luna`, reasoning `max`.
  Their existing worktrees and owned edits were preserved.
- F1: four pattern source files and one test file integrated. Focused Unit result:
  29 passed. Owned whitespace checks passed.
- F2: fourteen metadata/YAML/source files and four test files integrated and
  copied to dependent lanes. Focused metadata/YAML/source result: 123 passed,
  build zero warnings/errors. Duplicate containing mappings and independent
  optional-field state were checked and corrected before the final source freeze.
- F3 Entries, F4 evaluation, six command lanes and V1 Doctor are integrated
  from separate `task62-*` worktrees. Root distributed shared dependencies.
- Runtime instructions, routing, syntax, command contracts, public guides,
  diagrams and templates are integrated. Generated navigation converged and
  the documentation site type check and production build passed.
- Review budget consumed: R1–R4 were rechecked in one independent review.
  Correction budget consumed: one grouped correction, now integrated.
  Both corrected findings passed the focused reviewer recheck. Final gate
  receipts below complete the qualification evidence.

### Completed foundation and diagram evidence

F3 is integrated after its Framework Release build passed with zero warnings or
errors. Seven focused Unit filters passed 139 tests; Index Entries preservation
passed 15 Integration tests. The row parser source hash after its final nullable
flow correction is
`1950E6ED41D302F4A092644ECE54490A82A18880F3421C5B375A8FD01BD7A485`.
Literal edge-backtick patterns now round-trip through generated code spans.

F4 production needed no changes after its first dependency copy. Its Unit build
passed with zero warnings/errors; evaluator tests passed 7/7 and lexical path
normalizer tests passed 5/5 through direct MTP execution. Both test files are
integrated.

The diagram data and generated light/dark SVGs are integrated. Both were rendered
with the bundled Sharp runtime and visually inspected at 900 by 1362 pixels:
changed text remained readable without clipping or overlap. The canonical
Directive template lives in the Core Templates Extension, not the base Framework
payload; its authored content matches the dogfood template.

The final F2 reader correction passes 2 reader, 43 document-parser and 71
source-parser tests. Its SHA-256 is
`4370F8EDF0AB0574DBDF2ED20FE61290AE90806B488375C5FE0D12FE8DE14D7E`.
Duplicate scoped declarations cannot disappear behind duplicate containing maps.

V1 Doctor validation is integrated in the source-structure reader and two test
files. Focused selections passed: 10 source-structure Unit tests, 107 Doctor Unit
tests, 25 Doctor Integration tests including the new validation scenario, and
3 published Doctor process tests. Invalid Skill metadata is reported even when
ordinary metadata is missing; valid future-file patterns remain valid.

Root's coherent foundation build ran
`node scripts/delivery/cli.ts build --no-restore` in the integration worktree:
exit zero, zero warnings/errors, 2 minutes 8 seconds. The subsequent V1 copy and
command integration still require the final gate. Site dependency restoration
completed without lockfile changes, and `npm --prefix src/docusaurus run typecheck`
passed. The full site build follows public documentation integration.

## Integration clarifications

- Inspect compares task-start and selected loading with the same complete
  normalized working-path set. A pending condition produces an unavailable
  task-start fact with a clear reason and `read.atStart: null`; existing
  completeness propagation applies when required behavioral facts are unknown.
  An unmatched source has `atStart: false` and may still have a complete
  structural inspection. Matched and unconditioned sources use actual filtered
  startup membership. Structural topology counts retain their existing meaning.
- Retrieval applicability uses `state`, `conditions` and `matchingPaths`.
  No additional per-path array is introduced. Pending conditions retain their
  provenance and patterns with an empty `matchingPaths` list.
- Offline restore from the installed cache is authorized in every isolated
  worktree. Use `node scripts/delivery/cli.ts restore --offline`; never copy
  another worktree's intermediate build assets.
- If SDK test-project discovery rejects the prepared MTP projects, use the
  existing delivery path: build Release, then execute the built test DLL with
  `dotnet`, `--filter-class` and `--minimum-expected-tests 1`. This changes no
  test runner configuration and preserves the same test implementation.
- Site dependencies were absent from the existing checkout and npm cache. Root
  is restoring only the versions in `src/docusaurus/package-lock.json` with
  `npm ci --ignore-scripts --no-audit --no-fund` to complete the authorized site
  build. This is ordinary setup of accepted dependencies, not a version or
  Architecture change. Workers retain their no-network boundary.

### Integrated documentation and authoring progress

The five public CLI guides, eight concepts pages, diagram and templates, routing
rules, and sixteen authoring/retrieval Interface and Behavior contracts are now
integrated. Docusaurus type checking and the complete production build passed.
The existing lockfile is unchanged. No site was published.

Create is integrated after 25 Unit, 17 Integration and 3 published EndToEnd
tests passed. Pattern-set equivalence compares ordinal text across independently
parsed values. Rendering receives command-owned string projections.

A targeted Index preview showed only Task 62 inventories and the changed Route
Update technical-design description. Applying that reviewed plan completed with
exit zero. Final navigation and Doctor reconciliation follow command integration.

### Coherent implementation qualification

The integrated Release solution build passed with zero warnings/errors. The first
complete Unit run passed 3527 of 3529 tests; two exact Find grammar assertions
still named the previous seven options and are being updated for --for.
Complete managed Integration and public suites are running.

Context's independent Integration class passed nine scenarios, and Find's passed
eleven. Root's disposable-workspace smoke journey passed all fifteen steps,
including Create, Init, Update, clear, pending/matched/unmatched Context, mixed
working paths, Find, Inspect and a converged Index preview. Receipts are under
artifacts/task62-smoke-*.json.

The one independent Luna max review is active. Update's final self-check found
three additional cases: block-style list edits, preservation of interior comments,
and clear-absent requests with stale navigation. The Update owner is correcting
those before final qualification. A read-only review can examine stable slices
while that bounded correction finishes; final acceptance waits for both.

The shipped payload Doctor completed with exit zero and only three ownership
observations. Full-workspace Doctor remains blocked by existing repository
findings. Running the same new binary against the original checkout produced
the same findings: no newly introduced diagnostics or broken references were
found in the integration worktree. This is not a clean-workspace claim.

The first complete managed Integration run reported 2503 passed, ten failures,
and seventeen expected Unix-only exclusions out of 2530. The failures split
into two Inspect fixture/output cases, three Context ancestor expectations, one
Find projection compatibility regression, and four Status snapshot groups.
Context's exact ancestor-reason reconciliation now passes all 62 Integration
tests. Status changes are limited to shipped-payload size/token figures.

The first complete public run passed 250 of 253: two new Inspect assertions used
the wrong public property path, and one unchanged workspace-lock journey timed
out waiting for its helper handshake. The final quiet qualification gate will
recheck that journey without changing its expectations or timeouts.

### Independent review correction group

Accepted corrections from the one independent review are bounded to:

- Keep required metadata completeness independent from an invalid optional
  applyTo declaration. Preserve its Invalid facts and explicitly reject them in
  generated-navigation projection, so Index does not silently erase a condition.
- A provisional ancestry concern was withdrawn after tracing real topology
  construction. No extra route-state guard or impossible-graph test is retained.
- Complete Create process evidence with real repeated-pattern application,
  parent suffix, convergence and an invalid-glob no-write scenario.

Owners are implementing these in their existing isolated worktrees. The grouped
correction budget is now active; no second independent review is authorized.

Review refinement: the existing topology includes every actual structural
ancestor. A non-admitted ordinary leaf has no missing entrypoint ancestor, and
unresolved parent chains already return unknown. The reviewer withdrew the
ancestry candidate; owners removed the provisional extra guards and tests.
Find's separate selected-universe projection correction remains required.

### Final correction integration

The metadata independence correction is integrated with 159 focused tests
passing. Invalid optional conditions remain visible without discarding valid
ordinary metadata. Generated navigation explicitly blocks invalid conditions.
Create's expanded process evidence passes all four selected tests. Final Find,
Inspect and Update slices are integrated, including compatibility and metadata
preservation corrections. The .NET whitespace and warning-level formatting
checks pass.

The withdrawn missing-node ancestry candidate remains withdrawn. A distinct
workspace with two physical parent entrypoints is being checked because a
Loader-exposed child must not activate automatic descendants through an unknown
condition chain. This check is part of the same review correction group.

The independent whole-change scan is complete. Stable correction dispositions:

| Finding | Disposition | Evidence |
| --- | --- | --- |
| R1 Optional metadata independence | Corrected and rechecked | Ordinary and Skill metadata retain their own state, invalid conditions remain typed, Index rejects them |
| R2 Create public application evidence | Corrected and rechecked | Repeated patterns, generated parent suffix, convergence and invalid no-write process tests |
| R3 Loader root with ambiguous structural ancestry | Corrected and rechecked | Direct nested Loader targets are reachable, and competing parent entrypoints cannot be treated as no parent |
| R4 Find route-facts read failure | Corrected and rechecked | Standard-detail applicability must not disappear behind a successful result when the required facts read throws |

The earlier missing-node candidate was withdrawn and is not R3. Remaining review
work is limited to rechecking R3/R4 corrections. No second whole-change review
will run. The unchanged workspace-lock journey that timed out during concurrent
builds passed all three focused tests in 15 seconds without changing its code,
expectations or timeout. The complete final public suites will still include it.

R3 reproduction used two physical parent entrypoints with different file
conditions and a directly exposed nested Loader root. Before correction, the
real CLI returned completed and loaded the nested source plus its inherited
child. After correction, the focused workspace test returns incomplete and
selects neither. The shared startup resolver treats ambiguous route ancestry
as invalid applicability only when applicability evaluation is enabled. Status
retains its legacy mode. Context diagnostics now name both invalid metadata and
unresolved ancestry as possible causes.
R3 and R4 are now integrated. R3's shared-resolver Unit selection passed 11,
its real CLI regression passed 1, and affected Context operation, link seed and
snapshot selections passed 21 combined. R4's two-case throwing-reader regression
passed and the complete Find Unit filter passed 182. Unexpected exceptions use
the existing bounded failed-operation event with or without --for. Successfully
returned unavailable facts retain the existing incomplete path.

The final source is frozen for qualification. The canonical native command is
`npm run build:native -- --no-restore`, followed by `npm run test:built`. Invoking
the native delivery script directly without npm stopped at launcher compilation
because that step requires npm's execution environment. The npm invocation uses
the same existing script and needs no source or dependency change.
### Final gate receipts

- Final `npm run check:dotnet`: exit zero for whitespace and warning-level
  analyzer verification after every correction.
- `npm run build:native -- --no-restore`: exit zero, no warnings or errors.
  Managed solution, native CLI, native Integration, native EndToEnd, and managed
  EndToEnd targeting the native CLI all built successfully for win-x64.
- Build source identity: base `d7e8a6d261dd85ac4086584121758ff143640226`,
  changes `054d9b8c45410126a4488dd71951f7b0ae0af26895bced0ac46fd62c2cd355f6`.
- Native CLI smoke journey: all 15 steps passed, including preview/apply and
  converged Index. Inspect echoed the supplied normalized working path.
- Final native payload Doctor: completed, exit zero, three informational
  ownership observations. Repository Doctor: blocked by 1060 existing findings
  (12 errors, 930 warnings, 118 information). The same native binary against the
  original checkout reported 1062 findings. Comparing diagnostic code, subject
  path and message found zero new findings in this worktree.
- Full six-mode gate: exit zero. The manifest has `tested: true`, and its source
  identity matches the current implementation. All failures from the earlier
  qualification attempt are resolved. No test expectations or timeouts were
  weakened to hide the earlier workspace-lock timeout.
| Final execution mode | Passed | Failed | Expected platform exclusions |
| --- | ---: | ---: | ---: |
| Managed Unit | 3539 | 0 | 0 |
| Managed Integration | 2515 | 0 | 17 |
| Managed public CLI | 254 | 0 | 0 |
| Native Integration | 2515 | 0 | 17 |
| Native public CLI | 254 | 0 | 0 |
| Managed public tests targeting native CLI | 254 | 0 | 0 |

This is 9331 passing executions across six modes on Windows, with 34 expected
platform exclusions. It is not a claim about six operating systems. The runner
rejects warnings, and no unexpected skipped evidence was accepted.

Canonical receipts are `artifacts/task62-native-build.log`,
`artifacts/task62-test-built.log`, `artifacts/task62-dotnet-check-final.log`,
`artifacts/task62-site-build.log`, `artifacts/task62-native-smoke.log`,
`artifacts/task62-doctor-final.json`, and
`artifacts/delivery/win-x64/reports/*/results.json`. These ignored outputs are
reproducible with the commands and source identity above. The maintained tests
and fixtures are under `src/cli/tests/`.

## Completion and handoff

Task 62 “Glob-scoped loading” (phase 3/3): milestone 6/6.

The accepted implementation horizon is complete in branch `task62-applyto` at
`C:/Users/Tedy/.codex/worktrees/task62-applyto/open-forge`. Loader rules, optional
metadata, Entries, authoring, filtering, inspection, validation, public guides,
contracts and diagrams agree with the verified result. The documentation site
type check and production build also passed. The one independent review and its
single grouped correction are complete. No implementation findings remain open.

The worktree is ready for maintainer review and local integration. No commit,
merge, release or publication was performed in this implementation run. Existing
repository Doctor findings remain outside this task. Task 55 remains separately
open and unstarted. Alternative roots, multi-root behavior and APM integration
were not added to Task 62.

Final review disposition from `/root/luna_max_final_review`: PASS after
correction recheck and all six gate receipts. Final targeted Index preview
completed with zero changes. Final `git diff --check` passed.
