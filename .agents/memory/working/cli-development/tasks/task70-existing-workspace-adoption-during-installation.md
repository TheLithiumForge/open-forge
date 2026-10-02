---
open-forge:
  description: Define bounded adoption of compatible existing workspace metadata and route entrypoints during ordinary Framework installation and ordinary managed Update
  tags: [Memory, Working, Task, CLI, Install, Metadata, Contextual, Active]
---

# Task 70 — Existing workspace adoption during installation

## Accepted first-install correction on 2026-10-02

The maintainer clarified that existing `.agents` content must be adopted during
ordinary first installation. A filename collision with a shipped category
entrypoint is preserved in its adjacent overwrite companion before the
Framework base is established; an existing companion retains final precedence.
Recognized generated navigation is rebuilt in the base. Other entrypoints stay
in place. Native Skills, including an unowned Skill at a bundled destination,
remain user owned and receive only missing required metadata. Main `SKILL.md`
files never receive `Entries`.

The correction is implemented on `ww/a_6ecaa48917f3`. Shared migration
result vocabulary now has its own `Models.Result` namespace, with the narrow
Install/Update Presentation dependency explicitly documented and checked.
The earlier B5-Q1 choice is resolved by this user direction; B5-Q2 is corrected
in source. The exact-byte category case also records whole-file ownership of
the installed base when no base-byte replacement is needed; its preserved
companion remains user owned. Final managed evidence passes 4,070 unit tests,
2,737 Integration tests with 17 platform exclusions, and 272 public journeys.
Native qualification, installed-package qualification, and the actual published
beta4-to-beta5 upgrade journey also pass. Root accepts the combined local
candidate for integration; hosted qualification and publication remain.
The maintainer authorized squash integration, develop push and beta5 release
only from 12:00 through 13:00 Europe/Zurich today. Tasks 34 and 39 stay paused;
48 remains deferred. The prior failed candidate results below are history.

## Final local qualification on 2026-10-02

The final candidate is branch `ww/a_6ecaa48917f3`, based on actual HEAD
`f61988442228d76c0ccbbebd1d88b5c7cf8d9b5f`, with source-change fingerprint
`6d738cd7d801eb371e3a85bf51451f4ff61674659e630dc720d56d7b1efe715f`.
It remains uncommitted until the authorized noon window. Its complete tree and
integration patch are frozen separately from the normal Git index.

| Gate | Final result |
|---|---|
| Managed unit | 4,070 passed |
| Managed Integration | 2,737 passed; 17 platform exclusions |
| Managed public | 272 passed |
| Native Integration | 2,737 passed; 17 platform exclusions |
| Native public | 272 passed |
| Managed public against native | 272 passed |
| Installed npm package | Passed against the qualified native bytes |
| Published beta4 upgrade | One complete journey passed; 13 process invocations |
| Website | Typecheck and build passed |

The six runtime modes total 10,360 passes, 34 platform exclusions, and zero
failures. The final native build has zero warnings/errors. The existing full
format/analyzer check passed, followed by a whitespace check of the final
ownership correction. No individual Task 32/47/54/64 suites were repeated.
Their already-integrated behavior was checked empirically in the combined CLI.

Delivery manifest SHA-256:
`C3BD40EC0AADE28637112FAB32DC839F83C64D4F1549E1F9CA0DDD80DB6B49CC`.
Native SHA-256:
`B5334780C200338D2C0546400E9D36254E000BE2231E217560D51A5231164DB9`.
The current `readBuilt(..., true)` validates the source identity, tested reports,
and native inventory. Logs are under `artifacts/root-beta5-qualification/`
(`final-preservation-*`); runtime reports are under
`artifacts/delivery/win-x64/reports/`.

The actual upgrade receipt is retained in the qualification worktree
`a_a5158a5d7118`, under
`artifacts/beta5-upgrade-qualification/evidence/runs/b5-verified/receipts/native-upgrade.json`,
SHA-256 `AFD4E32265C994796FB1E7F4AD625EAAE9435DD5D3AAC1316CBE94CF3D4F3091`.
The pinned Skill, custom directive and companion, native Skill fields/body,
other-manager state, category exclusion, and retained recovery bytes survived.
The interactive confirmation succeeded; dry-run and repeat snapshots remained
unchanged. Repeat Update produced zero effects and migrations. Index reported
only the four exact optional-metadata warnings, with no writes.

The retained upgrade harness needed corrections to its setup order, Windows
shim arguments, beta4 route-removal semantics, planned dry-run effects, minimal
output rows, generated-region versus whole-file ownership, scoped Context
arguments, and a companion without frontmatter. These are evidence-harness
corrections; the qualified product source and binaries did not change. Maps
exclusion is exercised through route removal; a separate ordinary path removal
provides the retained recovery bundle. Failed harness attempts remain retained.

Root's separate first-install probes preserve an existing category body and
companion, a custom Skill at a bundled path, partial native metadata, and nested
reference navigation. The exact-byte base gains Framework ownership while its
companion remains user owned. Repeat Install has zero effects. Two optional
metadata warnings on pre-existing plain reference files remain visible.

Remaining: exact-tree squash into develop during the authorized window, push,
green exact-commit six-host hosted Build, normal release integration,
publication, and public postchecks. No beta5 publication has happened yet.

## Historical initial combined qualification on 2026-10-02

Root corrected the missing `managed-adoption` expectation in
`InstallDefinitionsContractTests`. Candidate `f61988442228d76c0ccbbebd1d88b5c7cf8d9b5f`
then passed all 4,066 unit tests. The Windows native build passed with zero
warnings or errors. Documentation typecheck and site build also passed.
The existing successful build and repository verification receipts were reused.

Managed and native Integration each executed 2,749 cases: 2,727 passed,
five failed, and 17 were recognized platform exclusions. These results block
Task 70 integration and beta5 publication. The task's earlier focused v9
qualification remains valid historical evidence, not a combined release pass.

- **B5-Q1, P2 contract conflict:** `OccupiedGeneratedRegion` and
  `EligibleForceReportsBoundedGeneratedRegionReplacement` expect the previous
  force boundary for an unowned exact standard entrypoint. Adoption instead
  reuses that entrypoint and reports `SafeAbsence`. The related
  `RecoveryStoreUnavailable` and `PartialWriteFailure` snapshots differ in
  adoption, effect, and footprint facts. Root reproduced the behavior through
  the native CLI. This was resolved by the first-install direction recorded above; the old
  force expectations are superseded for preserved category entrypoints.
- **B5-Q2, P2 architecture evidence:** `PresentationDependenciesFollowAcceptedDirections`
  rejects the shared WorkspaceAdoption model imports in Install's wire
  vocabulary and Update's report selector. The accepted migration result
  records expose that shared action and derivation vocabulary. Reconcile the
  result-model dependency boundary explicitly, without broadly allowing
  presentation access to adoption planning behavior.

The ordinary fresh Install, Planning installation, nested Skill reference
indexing, repeat Index, no-op Extension Update, trimmed installed tags, and
partial native Skill metadata adoption were checked empirically. Scratch
Doctor reported no problems after indexing and adoption. Tasks 32, 47, 54,
and 64 retain their existing individual squash commits on local `develop`.
No test suite was repeated during this qualification attempt. The managed
public mode subsequently ran 272 cases: 271 passed and one failed, with no
skips. `F01InstallReadinessJourneyTests.OccupiedTargetRequiresDeliberateForce`
reproduced B5-Q1 through the delivered CLI, expecting exit 5 and receiving 0.
The two later public/native combinations were not run after that failure.
Package and actual beta4 upgrade acceptance remain blocked by these results.
No beta5 push or release has occurred. The only source correction committed
in this continuation is the unit classification expectation. This combined
qualification record remains saved in the candidate worktree.

Reproduction uses the existing delivery `runSuites` selections in
`scripts/delivery/layout.ts`: unit, integration, public, native-integration,
native-public, and public-native. Logs and CTRF results are retained under
`artifacts/root-beta5-qualification/*-modes-resume.log` and
`artifacts/delivery/win-x64/reports/`. The ignored orchestration scripts are
not product or tracked repository files.

## Historical Execution Capsule — v9 verified

**Status: QUALIFIED — READY FOR ROOT ACCEPTANCE.** Task70 implementation and the final v9 qualification are complete for the bounded ordinary-workspace adoption behavior below. This is a Root handoff, not Root semantic acceptance, a merge, or a release. The task title remains **Task 70 — Existing workspace adoption during installation**.

**Current scope.** Ordinary root Install and ordinary managed Update in a complete, known, managed Framework workspace. Install adopts only compatible missing required metadata and route entrypoints under the accepted selection rules. Update adopts only compatible sources under selected standard local routes when Framework is present and management is complete; it does not establish absent or unknown management. Existing Install managed-divergence protection and Update managed-payload reconciliation remain strict.

**Preservation and effects.** Native Skills retain native semantics: valid fields and optional metadata are left alone, no `open-forge:` wrapper or Entries section is added, and only missing required fields are completed. A missing `name` derives from the containing directory basename. A missing description uses an existing usable description/title, then the first top-level H1, then the canonical workspace-relative path. Ordinary Markdown optional metadata remains unchanged. Bodies, unknown YAML, encoding, newline style, references, companion bytes, and user whole-file ownership are preserved. Frozen exclusions and malformed, foreign, ambiguous, or unsafe facts remain visible to strict diagnostics. User files are never assigned Framework whole-file ownership; only a verified generated Entries region may have a separate receipt. Dry-run, cancellation, lease, revalidation, recovery, application, and exact-byte verification remain in force. Repeat operations yield zero effects and rows; a complete managed no-effect state is `TrustedExact`, while actual migration effects remain `ManagedAdoption`.

**Grouped correction dispositions.** F1 now carries original authoritative `ProjectionTargetBytes` for exact lease comparison separately from final rebased physical `TargetBytes`, only after strict managed admission; fingerprints, snapshots, other comparisons, and admission remain unchanged. F2 selects the first top-level H1 fallback. F3 publishes migration rows only for backed physical file effects. F4 uses actual `IsUserOwnedSource` provenance, including local adopted metadata/catalogues while excluding payload or Framework whole-file claims; wire shape is unchanged. P3 help text now describes ordinary managed Update limits. Parent verified the source and affected tests. The initial `style.md` example was corrected to the actual shipped `open-forge-cli/SKILL.md` payload/native source form; no counts or assertions were waived.

**Final v9 qualification.** One shared full Release v9 build exited 0 with zero warnings/errors. All six isolated read-only lanes passed with numeric exit 0 and zero failures/skips: CORE 128, REGRESSION 16, TYPES 65, APPLICATION 77, PRESENTATION 32, PROCESS 15 (**333/333 hermetic**). The separate pinned local Skill Creator journey passed **10/10**. Bounded Extension regression coverage passed 19 tests; this is not a broader Extension feature claim.

| Lane | Tests | Receipt SHA256 |
|---|---:|---|
| CORE | 128/128 | `118B5CF01362C326B94005E4D2AAE5D093EEC267B2B8FCA47503A1C20B538101` |
| REGRESSION | 16/16 | `6759272C186C4E7FE9D779FD53C86C47D97A0461749E59ED526B8E78D7D8E14E` |
| TYPES | 65/65 | `00C252E813D787B513EC2F33FC21AAFA131CAA6B5E84E2DB4E3DC29A712FF8B0` |
| APPLICATION | 77/77 | `5244F61B08EF127AAD6E1D4A4A8E4688A614FD57723CDFA1556076EE3FF192F4` |
| PRESENTATION | 32/32 | `5679C87471E82C4A78FE890F3F1CDB1E5E5F8FB716EDB6ED423E835235C92FF0` |
| PROCESS | 15/15 | `415E7B29EB410AC456EE8AFAF5DD5C6A758E716FABE896CFAF7C42CB5C7097E0` |

**Candidate bindings.** Full build receipt `artifacts/task70/shared-build-receipt-v9.json` SHA `3C6424D48641799E6B46DE9E65F969EEC5E4AED1CBD765BE065542F47DBEED49`; assembly receipt `artifacts/task70/single-assembly-receipt-v9.json` SHA `7F5096812883362EAB33F7B8FF628BBB35FBC669B6D760C441EF14BB8A47E090`; source inventory `artifacts/task70/final-candidate-source-inventory-v9.json` SHA `3659A8CE09764C7F2D0C35BB90891CEE25A8321373DCF045F8E834E8B69CDDA8`. The inventory has 79 rows: 76 Task70 paths, two separately owned Task47 prerequisites, and a record row frozen at SHA `297AD8F4F825D4E856E41B5D40602C519E9BC31BCB084F5D5452E748274AD425`. The final 77-item a6e handoff comprises 74 unchanged tested Task70 files, two documentation-only Install contract files from the original INSTALL owner, and this separately reconciled Working record. Those three documentation-only closeout inputs do not change the qualified v9 runtime; the tested 79-row inventory and candidate remain immutable. The verified runtime identities are Unit DLL `A9BAC71D132B54DAE5798B45BCC1E42F9CEACA2BD8F34A17FD12449C0F07CE9C`, Integration DLL `AE8B8282473CC4F811903D30DB662FF5BE904912BE27F9949DD5866A832F71B1`, EndToEnd DLL `B3B27E8B23D66CFF2DA2EC2CE0246885A74736621D26BEDE4DF58FE8A6A4B0B4`, Operations DLL `3BE4C0EEF2035B259457835BE7D734CC3BBE7733044C411500A71880FE194002`, and published CLI DLL `BA8E7D8E022144F1AFE07AF5845E1B159F3E3D07BEBE25BEDF7926DE1AF9F002` with a 33-file closure. Parent QA verified 79 source rows, 328 test-output files, and runtime bindings with zero mismatches. Install contract wording was clarified after qualification only: the original INSTALL owner changed exactly four fallback-phrase occurrences (two in each document) from “first top-level heading” to “first top-level H1 heading”; no other bytes, code, tests, or policy changed. The source-ready receipt C:/Users/Tedy/.worker-watch/worktrees/a_0f9f108d71df/artifacts/task70/contracts-h1-closeout-v9.json has SHA 2003675D10378A610483CB98F3CE8E272EC9FBB896854215B04F94E2E52DEAF7. Final Install Interface SHA 6882EA7DF003B043538F4AC9593FB1072AAA809D75DFEBCA2B53B450E2973014 differs only by those two H1 words from tested SHA 578D6CCDDCD139F9EAF7361B469346E2789E6207E8AD7DC7CDF18129DFFE7166; final Install Behavior SHA 0D136B495E50C2261FC0809878AF0344CD58A18DC471267DD96B1EDE76BF8C03 differs only by those two H1 words from tested SHA 3631016F2AFFF97F747E75809E95A2B0B76A216D0C8B5C152C40181C55A02FD4.

PROCESS receipts are `artifacts/task70/process/v9-runs/20261002043331904-ad522ccf9458407e8e1bbc00ba3a4ed2/PROCESS-v9-qualification.json` (SHA `415E7B29EB410AC456EE8AFAF5DD5C6A758E716FABE896CFAF7C42CB5C7097E0`) and pinned `artifacts/task70/process/runs/20261002043429778-d25f59b6302a48d988f1c7d9db2de5f5/qualification.json` (SHA `9E2E44BD1AB5F77C8A95BF3AA60B7130D7F607363BBDD783B71A8AE0BEC794D2`). The canonical aggregate PROCESS receipt is C:/Users/Tedy/.worker-watch/worktrees/a_0f9f108d71df/artifacts/task70/LANE-shared-qualification-v9.json (SHA 0FEB8C59BEEFFEC1FEFEF2788BB71397E746CF5BB689CAE3CF9E635052FB7971); the detailed PROCESS receipt and pinned receipt above remain unchanged. The pinned manifest is 18 blobs / 224,992 bytes, SHA `DEE669A0BE312D756B0F0E7EC00294F4D7FB71557C99B1EDA93F53086333C064`; 45 process captures and 405 runtime hashes matched, TTY confirmation exited 0 and decline 130. The upstream root LICENSE is absent while the subtree LICENSE.txt is present; downloaded scripts and instructions were not executed. Default Index required no additional catalogue repair; four optional-metadata warnings were observed and are not described as warning-free. Repeat Update had zero effects/rows, valid upstream bytes were preserved, and the labeled missing/partial metadata mutations passed.

**Review and history.** The single fresh whole-task GPT-6.1 high review `r_b0d032f1336f` is consumed: four P2 and one P3 source-derived findings, with reproductions not run by the reviewer. The same single grouped correction is complete and verified; this is not a claim that the original review passed or that a new review approved the result. Zero councils and no second review. Historical v7 apply exit 1 verification failure and v8 apply exit 5 lease-time block diagnostic `A8A3FF5D6C09BB4F7C7B4A8B5F9C0CD02E46654FCD1508D0BDDA1C15B09D9F3D` remain preserved. V8 CORE 108/109 and PRESENTATION 7/8 results are stale-candidate history; the v8 ChangedFiles failure was a managed-host fixture oracle, corrected in v9. The public beta 4 apply exit 5 remains historically inferred; Root independently observed dry-run exit 5. The local pinned journey is not an actual beta 4-to-beta 5 upgrade.

**Root-owned pending gates.** Native and complete managed combined gate, package/site proof, actual published beta 4 to final combined beta 5 upgrade, Task47 prerequisite semantics, shared ledger/routeEntry integration, semantic acceptance, and develop/release integration remain with Root. Candidate version remains beta 4. Task70 is qualified and ready for that handoff; it is not Root-accepted or released.

**Contracts and source context:** [Install interface](../../../crystallized/documents/cli/contracts/install/interface.md), [Install behavior](../../../crystallized/documents/cli/contracts/install/behavior.md), [Update interface](../../../crystallized/documents/cli/contracts/update/interface.md), [Update behavior](../../../crystallized/documents/cli/contracts/update/behavior.md), [lifecycle provenance](../../../crystallized/documents/cli/technical-designs/lifecycle-provenance.md), [Markdown syntax](../../../crystallized/documents/framework/markdown/syntax.md), and [Markdown overview](../../../crystallized/documents/framework/markdown/_markdown.md).

## Historical initial Install outcome and beta 4 reproductions

The defect concerns ordinary root Install in a workspace with a pre-existing
`.agents` tree. Install can block when a routed source needed for routability
lacks required metadata or an entrypoint. Ordinary Install automatically
completes compatible missing fields and routes under the frozen policy below.
This task preserves existing user content and the current Install safety model.
It does not make Index or general metadata parsing more permissive.

The public beta 4 `missingSkill` and `name/license-no-description` reproductions
both blocked and preserved their files. Their apply evidence and text streams
are recorded in the linked artifact. The numeric apply exit 5 is inferred after
the runner lost capture and is not directly persisted; Root independently
confirmed actual dry-run exit 5 for both. Neither original beta 4 reproduction passed, and no fix is claimed from this historical reproduction.

## Historical Execution Capsule — policy frozen 2026-10-01 (superseded by v9)

**Status (reconciled 2026-10-02 from v6 and closed review):** The pre-review candidate had 321 hermetic passes across CORE (125), REGRESSION (15), TYPES (63), APPLICATION (75), PRESENTATION (28), and PROCESS (15), plus 10 pinned local Skill Creator journeys reported separately. The single fresh whole-task GPT-6.1 Sol high review, run r_b0d032f1336f, completed with four P2 blockers and one P3 finding. Findings are source-derived; reproductions were not executed and the candidate was not changed. Those receipts qualify only the pre-correction bytes. One grouped correction packet is frozen below and awaits implementation and qualification. Root's actual beta 4 to combined beta 5 native, package, site, development, and release gates remain pending.

**Historical v2/pre-v3 stage:** The v2 candidate had 78 verified rows (75 Task70 changes, two separately identified Task47 prerequisites, and its Working record separately) and a full Release build with exit 0, zero warnings, and zero errors. Its recorded checks were CORE 125/125, TYPES 63/63, and PRESENTATION 28/28. The prepared presentation expectation of 39 was incorrect and the historical receipt remains preserved. PROCESS v2 reported four PublishedUpdateAdoption failures with CLI exit 1 and unexpected planning because relative UpdateAdoptionTarget.Path was compared with absolute Snapshot.LogicalPath. The APPLICATION correction removed that comparison while preserving kind, bytes, coalescing, upstream physical planning, and lease guards. REGRESSION's first four cases reported three failures and one pass because its fixture used invalid SKILL.md.overwrite.md; its correction used SourceOverwritePath.ReadAdjacentPath for SKILL.overwrite.md and preserved the existing assertions. Those v2/pre-v3 facts are historical; later stage outcomes are retained below.

Seven disjoint source-ownership lanes were authorized, with the user later permitting maximum useful disjoint concurrency. Execution uses one shared candidate and independent read-only filter runners with isolated working, data, temporary, and report directories. The final v6 Integration test assembly was compiled test-only against the sealed v5 production baseline; there was no full-solution v6 build. No source replicas were used for the read-only tests. The pinned local A/B process acceptance has since passed; the synthetic earlier-payload proof remains diagnostic and is not an actual older-binary installation. The earlier 102-test / stage Install result is historical, not final Update qualification. Root owns final acceptance and actual beta 4 upgrade gates.

**Scope:** Complete Task70 across the Install contracts, shared document and topology helpers, root Install integration, schema-3 migration presentation, process qualification, and combined acceptance. Ordinary managed Update adoption and every frozen exclusion unit remain within the accepted scope. The bounded Extension coherence regression closure has 19 passing tests; no broader Extension feature expansion is claimed here.

**Authority:** The maintainer froze the policy below. Root owns integration,
combined qualification, and final acceptance. Root owns the shared task route
entry and project-control record.

## Frozen Policy

- Ordinary root Install automatically adds missing compatible required
  metadata and route entrypoints in the selected standard route subtree when
  they are needed for routability. It requires no new flag or `--force`.
- Existing payload collisions and the current explicit `--force` boundary
  remain. This policy does not authorize replacement, managed update, or
  recovery of divergent authored managed payload. A verified managed authored
  base may receive only the bounded user adoption and generated navigation
  effects required by its newly observed selected-route sources, without
  `--force`. This is `ManagedAdoption` internally and `managed-adoption`
  publicly. An exact managed state with no effects remains `TrustedExact`.
  User-owned targets are excluded only from managed-base admission; all planned
  targets still require exact applied-byte verification. A generated Entries
  fingerprint may differ only when authored identity remains unchanged, its
  navigation migration is planned, and the existing bounded region-span check
  succeeds. The narrow occupancy exception applies only to verified managed
  targets and planned `UserOwnedPaths`; fresh-establishment force rules remain.
- Install reuses unique recognized entrypoints, honors `removedFiles` and
  removed defaults, and never follows outside junction targets. Ordinary
  catalogue observation may read other safe sources under its existing
  selection rules; unrelated content is not adopted or normalized.
- A native `SKILL.md` keeps native semantics. Install completes only missing
  required native fields, leaves complete native fields and optional
  `license` unchanged, adds no `open-forge:` wrapper, and adds no `Entries`
  section to the Skill. Resource catalogues under `references` stay within
  the accepted Task 47 selection boundary.
- Existing compatible `description` or `title` wins, followed by the first
  top-level H1 heading and then the workspace-relative path. A missing native
  Skill `name` comes from its directory basename. A required tag is added
  only when a new completion needs ordinary classification. In that case,
  `Workspace` is a search tag. Install never invents loading, behavior,
  authority, or state tags.
- Existing bodies, unknown YAML, encoding, newline style, valid metadata,
  optional metadata, overwrite companions, binaries, and user ownership are
  preserved. Only missing compatible required fields or route structures are
  added.
- Ownership is observed before adoption. An absent ownership file is known
  empty. Malformed, unreadable, or unsupported ownership does not license
  migration when a competing claim cannot be ruled out, but does not gate
  unrelated safe effects. Migrated sources and new local resource
  entrypoints do not receive whole-file Framework ownership. A verified
  generated `Entries` region may be recorded separately.
- Install forms prospective adoption bytes and route sources before its
  existing navigation projection. Existing parser spans, confirmation,
  workspace lease, revalidation, recovery preparation and retention,
  application, and post-verification remain in force. Install adds no
  automatic rollback.
- Payload-owned targets and user-adoption targets stay separate. There is no
  global Index, parser, or projector relaxation. Optional metadata that is
  already allowed to be absent remains unchanged. A later Extension change
  may cover only its bounded affected projection closure and is not claimed as
  implemented by Task70.
- Malformed, ambiguous, conflicting, or unsafe inputs block precisely before
  effects. No ownership inference follows from matching bytes, names, tags, or
  paths.

## Historical implementation packets — plan frozen before execution

1. **Contracts and shared helpers:** update the Install Interface and Behavior,
   Markdown syntax, lifecycle provenance, and this Working task record; author
   the bounded document and topology helper APIs under the frozen contracts.
2. **Root Install integration:** form prospective adoption bytes and sources,
  distinguish user-owned targets from payload targets, plan and verify exact
  effects, preserve ownership boundaries, independently verify an existing
  managed authored base, classify non-empty bounded adoption as
  `ManagedAdoption`, and produce migration facts without changing global Index,
  parser, or projector behavior. Generated navigation updates may have a
  migration row on an existing managed host while retaining that host's
  current ownership. `UserOwnedPaths` is not synonymous with migration rows.
3. **Presentation and process qualification:** conditionally expose schema-3
   migration rows, preserve existing no-op JSON snapshots, verify text streams,
   and qualify dry-run/apply and repeat-install behavior.
4. **Extension follow-up:** after root qualification, make a separate bounded
   decision over only Extension's affected projection closure. Do not claim
   broader Extension normalization as implemented.
5. **Root qualification and acceptance:** verify the Task47 prerequisite and
   combined install/index/install sequence below. Root owns the result and
   final acceptance.

## Migration Result

Install keeps schema number 3. Its internal migrations collection is empty on
no-op. Public `data.migrations` is optional and omitted when there are no rows,
preserving existing no-op JSON snapshots. When rows exist, the array is present
at every detail level. Each row has `path`, `actions`, `fields`, `derivation`,
and `outcome`. The path is canonical and workspace-relative.
Actions use `metadata-completed`, `entrypoint-created`,
`entries-section-added`, and `navigation-updated` as applicable. Fields
list only the actual metadata fields changed. Derivation rows use
`{ field, source }`, with sources `existing-description`,
`existing-title`, `heading`, `relative-path`, `directory-name`, and
`required-tag`. Outcome is `planned` or `applied`, and `applied` requires
verified effects. On no-op, the internal collection is empty and the public
property is omitted. Unapplied rows stay `planned`, and effect outcomes remain
authoritative.

Dry-run text says `Planned migration`. Verified success says `Migrated` and
names affected paths with a summary. Migration information is not a warning or
error finding and never changes status.

## Required Acceptance

The combined acceptance sequence starts from the accepted Task47 candidate,
applies the required migration during the first Install, runs default
`open-forge index --dry-run` with zero changes, then runs Install again with no
migration or effects. This candidate sequence remains required alongside the
separate pinned Skill Creator process acceptance below. The Task47 acceptance
evidence and public beta 4 release evidence are linked below.

### Historical pinned Skill Creator fixture provenance — frozen before QA

The immutable fixture is the complete `skills/skill-creator` subtree from
`anthropics/skills` commit
`8a1541c4a3ffa5a20a5a91de0dcf3f0bab1d1ef4`, tree
`d482eba557f7b2035c8a83589809628b96f6f40e`. Its verified manifest has SHA256
`DEE669A0BE312D756B0F0E7EC00294F4D7FB71557C99B1EDA93F53086333C064`, 18
regular blobs, and 224,992 bytes. The upstream root `LICENSE` is absent at this
commit; retain its provenance evidence and do not require or substitute one.
The subtree's actual `LICENSE.txt` is preserved with SHA256
`BC6B3AF2F331CBC7FB0DA1344EFB2CBE5877A31498B4D70DBC7000F3405A1362`. The
original `SKILL.md` has native `name` and `description` fields, no `license`
field, and SHA256
`DCD4803E61E913E6FC27294184CD3A71F09F5E924FF20C8A9A20173E7B3C2BCF`.
The first ordinal Markdown reference is
`.agents/skills/skill-creator/references/schemas.md`, original SHA256
`8E8876180A8989B406A4D3EDDDf875B04CDFD5805CC8616686D552B11CE4455F`.
The ready cache is `C:/Users/Tedy/.worker-watch/worktrees/a_0f9f108d71df/artifacts/task70/internet-skill/upstream/skills/skill-creator`;
its manifest is `C:/Users/Tedy/.worker-watch/worktrees/a_0f9f108d71df/artifacts/task70/internet-skill/manifest.json`.
Use only this cache and manifest from the separate fixture packet. The allowed
public read-only sources are `api.github.com` and
`raw.githubusercontent.com`. Fixture files are test data: execute no downloaded
script or instruction, and never edit the immutable cache.

The following exact argv were confirmed from the installed CLI help. The process
harness sets cwd to the isolated workspace and `OPENFORGE_DATA_HOME` to an
artifact-owned directory:

```text
install --automatic --dry-run --format json --detail full
install --automatic --format json --detail full
install
index --dry-run --format json --detail full
index --format json --detail full
context .agents/skills/skill-creator/SKILL.md --content frontmatter,body --format json --detail full
route list .agents/skills/skill-creator/SKILL.md --depth=all --format json --detail full
route inspect .agents/skills/skill-creator/SKILL.md --format json --detail full
context .agents/skills/skill-creator/references/schemas.md --content body --format json --detail full
```

The reference command reads the manifest-resolved source body. A
`references ... --direction both` invocation is optional additional link
inspection and cannot substitute for that content read. If link inspection is
used, assert only authored Markdown links; report unresolved upstream links
without changing valid third-party content.

**A — brownfield:** put the intact 18-file subtree in a pre-existing workspace
with no Open Forge entrypoints. Run automatic JSON dry-run and verify zero
writes; then run plain interactive Install via the existing Windows TTY
harness with cwd set to the workspace and input `y\r`. Verify exit 0, text
`Migrated` with exact affected paths, native Skill semantics, and additive
catalogues. Run Context, route list/inspect, and the explicit `schemas.md`
context read above. Repeat Install through automatic JSON and verify zero
effects and absent-or-empty `data.migrations`; default Index dry-run must report
zero additional catalogue repair. An independent intact copy runs the
automatic JSON dry-run/apply pair to prove planned/applied schema-3 migration
facts. Hash every upstream file after Install and Index and list additive local
catalogue files separately.

Use two additional copies whose only upstream-content mutation is to native
`SKILL.md` frontmatter: (1) remove the original frontmatter (`missingFM`),
keeping original body and other 17 blobs unchanged; (2) preserve original
`name`, add test scalar `license: Task70-fixture-sentinel`, and omit
`description` (`mutated-pinned-skill-partial-name-license`). The sentinel is a
preservation probe, not upstream license metadata. These are explicit test
mutations, not claims about the intact upstream. Retain `LICENSE.txt` and all
other files byte-for-byte. Keep the earlier minimal MIT-license fixture as a
separately labeled regression.

**B — already-managed root:** in a separate workspace, perform a normal fresh
Framework Install with plain interactive `install` through the Windows TTY
harness and input `y\r`, then copy the intact 18-file subtree to
`.agents/skills/skill-creator`. Before adoption, run and capture the following
in order, including stdout, stderr, exit, and source hashes for every command:

```text
index --dry-run --format json --detail full
index --format json --detail full
context .agents/skills/skill-creator/SKILL.md --content frontmatter,body --format json --detail full
route inspect .agents/skills/skill-creator/SKILL.md --format json --detail full
route list .agents/skills/skill-creator/SKILL.md --depth=all --format json --detail full
context .agents/skills/skill-creator/references/schemas.md --content body --format json --detail full
```

Index apply is the command without `--dry-run`; Index has no `--automatic`.
Promptly report any failed or incomplete pre-Install command to the Task70
owner with exact argv, exit, and findings. Do not silently waive it, add
entrypoints or metadata manually, or edit Index. Continue with evidence-only
adoption when safe. Then run Install automatic JSON dry-run and plain
interactive `install` with `y\r` through the Windows TTY harness. The plain
Install must follow the verified managed-base adoption path without force.
After adoption, rerun Skill Context, route inspect/list, and explicit
`schemas.md` context. Verify source preservation, references/catalogues, and
schema-3 `data.classification: managed-adoption`. Repeat Install through
automatic JSON and verify zero effects and no migration rows; default Index
dry-run must report zero additional catalogue repair. Capture every raw stream,
numeric exit, TTY mode, cwd, before/after hash, and candidate runtime/source
hash. Candidate provenance includes the selected executable, every published
`OpenForge.Cli*.dll`, runtime configuration and dependency files, version
marker, test assembly, and full candidate source inventory. At least one plain
Install must use a real TTY.

Preserve the historical `r_b714963066ce` ambiguity. Do not rerun the historical
public beta 4 baseline in this packet.

The original public beta 4 defect reproductions remain historical evidence.
Both dry-runs were blocked with exit 5, reported zero effects, and left input
hashes unchanged. Apply evidence and text streams show both applies blocked and
the files preserved. The numeric apply exit 5 is inferred after the runner lost
capture and was not directly persisted. Root independently confirmed actual
dry-run exit 5 on both. Neither original beta 4 reproduction passed, and no fix is inferred from this historical reproduction.

## Process qualification and Root acceptance boundary

The required pinned real-fixture Skill Creator A/B process acceptance has passed in the ignored local harness: 10 journeys, 45 process captures, 405 runtime hash checks, TTY confirmation exit 0, and decline exit 130. The fixture manifest and exact provenance remain recorded in the pinned acceptance section below. This local process qualification does not complete Root's actual beta 4 to final combined beta 5 acceptance.

Keep checked-in PublishedInstallAdoptionProcessTests.cs limited to hermetic minimal process regressions. It must not contain personal absolute paths, depend on the ignored internet fixture cache, download sources over the network, or skip because the local fixture is unavailable. The complete pinned harness and its outputs remain under ignored artifacts; no committed build or fixture files are added. This CI boundary does not waive the frozen A/B acceptance or its evidence requirements.

## Evidence

- Task47 follow-up and acceptance record: [default Skill indexing](task47-default-skill-indexing.md).
- Task47 qualification receipt: [absolute artifact](C:/Users/Tedy/.codex/worktrees/beta-stabilization/open-forge/artifacts/task47/qualification-receipt.json), SHA256 `54B7F4A42D051C0591F2FC1D3F6A341BDEDA061A50E3D6A1E2E5173E17B9F001`.
- Task47 root physical qualification: [absolute artifact](C:/Users/Tedy/.worker-watch/worktrees/a_390948de2166/artifacts/task47-physical-qualification/run-04a606154cc641c496be26a9e750dc9c/qualification-receipt.json).
- Public beta 4 baseline: [Task69 release receipt](task69-next-beta-stabilization-release.md) and [absolute final receipt](C:/Users/Tedy/.codex/worktrees/beta-stabilization/open-forge/artifacts/beta4-public/final-receipt.json).
- Task70 historical apply evidence and text streams: [absolute artifact](C:/Users/Tedy/.worker-watch/worktrees/a_0f9f108d71df/artifacts/task70-repro/apply-evidence.json).

## Historical Review Budget Snapshot — before final v9 qualification

- The one fresh whole-task GPT-6.1 Sol high review, r_b0d032f1336f, is complete: four P2 blockers and one P3 finding, source-derived and not executed.
- The single grouped post-review correction is frozen below and remains pending implementation and qualification; do not start another review.
- Zero councils.
- The closed-review JSON and grouped-correction freeze receipt are under ignored `artifacts/task70/review/`; this record update does not alter the candidate.
- Return to Root if the frozen policy conflicts with a payload collision, ownership claim, route boundary, parser or projector boundary, recovery invariant, or accepted Task47 behavior.

## Accepted Update Adoption Follow-up — 2026-10-01

Root approved bounded ordinary managed Update adoption after reviewing the
synthetic earlier-payload proof. The proof seeded ownership with a normal
current-payload Install, then replaced only `.agents/loader.md` with bytes from
a cloned payload containing an earlier authored sentence. Its source
fingerprint differs from the current payload. This is a synthetic fixture, not
an installation by an older binary. The evidence is
`artifacts/task70/upgrade-proof/install-upgrade-adoption-boundary-proof.json`
(SHA256 `90FDF837789A79A843CB6AEEAB216EDF8AF3059D14D7E539ACBC97FE9614A589`)
from proof source `InstallUpgradeAdoptionBoundaryProofTests.cs` (SHA256
`07E006B4EE697B2D37BAEAF45707491BF9BD743C2F722538B438B90F83A2E8BB`).
That run observed Install `ManagedDivergence` for all three cases. Update planned
the loader replacement in the no-Skill and complete-Skill cases, planned the
local catalogue replacement for the complete-Skill case, and blocked the
partial-Skill case on `GeneratedRegionUnsafe`. The isolated automatic Install
Apply repros also returned `ManagedDivergence` with no effects. All observed
workspace snapshots were preserved. Root owns the interpretation of this
diagnostic result and final acceptance.

### Frozen Update policy

- Update may adopt only compatible sources under selected standard local
  routes, and only when ownership is complete and Framework state is present.
  Adoption does not establish an absent or unknown Framework state.
- Use `FrameworkPayloadSelection.IncludesPath` with its full exclusions to
  filter adoption candidates. Keep the complete source catalogue and strict
  projection inputs. Do not relax Index, the global parser, or the global
  projector.
- Before projection, form intended bytes for missing required native Skill
  fields and required resource catalogue documents. Preserve optional
  frontmatter, valid and unknown fields, bodies, newlines, encodings, and
  companion files. Combine metadata completion and generated `Entries` into
  one exact physical effect per path.
- Keep typed adoption targets separate from owned Framework comparisons. Do
  not assign Framework asset provenance, payload counts, or whole-file
  ownership to user-authored files. A generated `Entries` receipt may be
  recorded on its own. Excluded, malformed, foreign, ambiguous, and unsafe
  facts remain visible to strict block diagnostics, with no guessed effects.
- Preserve Update's existing lease, revalidation, recovery, application, and
  verification pipeline. Install's managed-divergence behavior remains
  unchanged. Preserve user bytes on confirmation cancellation and on repeat
  Update; successful adoption adds no warning or incomplete status.

### Frozen consumer API and presentation

CORE provides `OpenForge.Cli.Core.Commands.Update.Models.Result`:

- `UpdateMigrationOutcome` has `Planned` and `Applied`.
- Internal sealed record `UpdateMigration` accepts `path`, `actions`, `fields`,
  `derivation`, and `outcome`, with the same constructor and validation shape
  as Install's `InstallMigration`.
- `UpdateResult.Migrations` is an `IReadOnlyList<UpdateMigration>`.
  `UpdateResultFormation.Migrations` defaults to an empty collection so
  existing callers remain valid. `ForPlanReview` keeps planned rows truthful.
- `UpdateDefinitions` provides the machine-name overload for
  `UpdateMigrationOutcome`. Existing shared action and derivation vocabulary
  remains unchanged.

Presentation keeps schema version 3. When migrations are empty,
`data.migrations` is omitted. When rows exist, every detail level emits the
same deterministic, path-ordered array and row shape as Install:
`path`, `actions`, `fields`, `derivation`, and `outcome`. Outcomes are
`planned` or `applied`. Only the matching verified non-directory physical
effect permits `applied`; preview, cancellation, unstarted work, and unverified
effects stay `planned`. Physical user-adoption file effects have no Framework
`SourceAssetPath`. CORE owns their logical representation and fact linkage.
Text names each path and uses `Planned migration` or `Migrated` with a
consistent summary. Migration facts do not create findings or warnings.

### Ownership and acceptance boundary

CORE owns Update operation code, models, result factories and definitions, the
new `UpdateWorkspaceAdoptionBuilder` and typed plans, Install exclusion
consumption, and its integration regressions. This packet owns only Update
presentation models, selector, required existing renderer changes,
Update-specific output text, focused presentation tests, Update behavior and
interface contracts, this Task record, and the necessary lifecycle-provenance
clauses. It does not own Install rendering or shared adoption helpers.

Acceptance covers the older-loader no-Skill and complete-Skill controls; missing and partial native Skill metadata through Update dry-run and automatic Apply; confirmation and cancellation without writes; preservation of body, optional license, references, and unrelated catalogue bytes; local catalogues remaining user-owned; excluded paths and categories remaining unchanged or causing a strict block; and a second Update with no effects or migrations. Malformed, foreign, or incomplete-management cases block. Existing Update and Install suites remain required, as do pinned real Skill A/B and hermetic published-process checks. The pinned local A/B run has passed in v5; Root's actual beta 4 to combined candidate gate remains separate and pending.The frozen consumer API and presentation remain within the v6 source inventory. An earlier presentation report at artifacts/task70/PRESENTATION-shared-qualification.json (SHA256 D21A83416EB218C8BC1BC63402ED12B846D4922BB2EA818C609B6B0AD85EA1A9) is retained as stage history. The later claim under run r_d7ac2f82ae43 was rejected as unrun: no v5 receipt or command_execution events existed, and the claimed FFBEE hash was the unchanged v4 receipt. Do not count that claim. The actual v5 direct-DLL run r901 passed the four presentation classes 28/28 with zero failures or skips; its receipt is artifacts/task70/LANE-shared-qualification-v5.json, SHA256 68E7BD03D7789A8A258C38D9CD2F4C1D11262BEE7969D4C51E21710C1694D69B. The correct breakdown is 6 Update migration, 3 Update wording, 8 Install migration, and 11 Install presentation contract tests. This lane result is not whole-task acceptance.
### Historical seven-lane source freeze — 2026-10-01

The user explicitly authorized seven disjoint Luna/max source-ownership lanes
for the bounded Task70 continuation, and later authorized the maximum useful
disjoint concurrency. This supersedes the earlier live concurrency cap;
historical receipts and completed-run counts remain unchanged. The source
owners stay disjoint while test execution uses one shared frozen candidate.
This section records the source/API freeze, not a completion claim for every
lane or qualification gate.

1. **CORE** (`a_aed2fa9ed36c`) owns Install exclusion consumption, shared
   topology helpers and their regressions, plus Update
   `UpdateWorkspaceAdoptionBuilder`, `UpdateGeneratedNavigationPlanner`,
   `UpdateIntendedStateBuilder`, `UpdatePlanBuilder`, and private consumer
   models as needed. It does not own Update model/result/effect/application
   files or Update integration tests.
2. **TYPES** (`a_aa115dab04da`) owns minimal Update model additions,
   `UpdateDefinitions`, `UpdatePlanResultFactory`, `UpdateResultBuilder`, new
   `UpdateMigrationFacts`, `UpdateMigrationResultTests`, and only necessary
   `UpdateResultContractTests` adjustments.
3. **APPLICATION** (`a_be46715e1b33`) owns `UpdatePhysicalEffectPlanner`,
   `UpdatePlanRevalidator`, narrow generic-pipeline edits to
   `UpdateAppliedVerifier` or `UpdateEffectApplication` only if required, and
   `UpdateAdoptionEffectPlanningTests`.
4. **REGRESSION** (`a_4497d8f827b6`) owns only new
   `UpdateWorkspaceAdoptionIntegrationTests`, reusing the existing integration
   workspace and synthetic earlier-loader fixture pattern.
5. **PROCESS** (`a_0f9f108d71df`) owns existing published Install adoption
   process coverage and new hermetic `PublishedUpdateAdoptionProcessTests`.
   It does not duplicate the Root-owned beta4-to-candidate journey.
6. **PRESENTATION** (`a_6e3644443722`, this lane) owns Update rendering,
   output text, focused presentation tests, Update behavior/interface
   contracts, lifecycle-provenance clauses, and this working record. It does
   not author typed model or result-factory source.
7. **TRANSFER/EXECUTOR** (`a_025cfc30c08d`) owns ignored artifacts, literal
   hash-guarded dependency transfers, final candidate assembly in its own tree
   and MAIN after receipts, and exact build/test execution. It does not author
   product source.

Frozen cross-lane API and invariants:

- `UpdateMigrationPlan` in Update planning models carries required `Path`,
  `Actions`, `Fields`, and `Derivation` with the Install-plan shape.
- `UpdateAdoptionTarget` is the positional record
  `(string Path, FileStateSnapshot Snapshot, byte[] IntendedDocumentBytes,
  bool OwnsGeneratedEntries)`. Its snapshot is the exact original state,
  including missing state; bytes are the final coalesced document; the boolean
  is true only for a verified complete generated-Entries receipt candidate.
  It never represents whole-file ownership.
- `UpdateGeneratedNavigationBuild`, `UpdateIntendedStateBuild`, and
  `UpdatePlanExecution` preserve their positional constructors and add
  initialized-empty `AdoptionTargets` and `Migrations` properties. Completion
  uses its existing intended state.
- CORE passes final adoption targets and migrations separately from Framework
  comparisons and projection inputs. The plan builder calls the three-argument
  physical-effect planner and records ownership only for eligible generated
  `Entries`. Complete ownership and present Framework state are prerequisites;
  strict complete-catalogue projection and canonical exclusions remain.
- APPLICATION exposes the three-argument physical-effect planner while the
  existing two-argument overload forwards an empty target list. Identical
  target paths coalesce snapshot and final bytes into one physical file effect.
  Adoption file changes have no region and no Framework source asset. Existing
  Framework provenance checks stay unchanged.
- TYPES narrowly permits file create/replace logical changes with no source
  asset only when same-path nonempty migration facts back them. Migration
  outcomes are `Planned` or `Applied`; only the matching verified,
  non-directory effect makes a row `Applied`. Formation defaults migrations
  to empty, and plan review/cancel/unstarted work stays planned. Migration
  facts do not inflate Framework payload counts or asset labels.
- Schema remains version 3. Nonempty migrations serialize at every detail level
  in deterministic path order with the Install row shape; empty migrations are
  omitted. Text identifies each path and reports planned versus verified
  migration without adding findings or warnings. Physical user-authored file
  effects do not imply Framework ownership.
- Frozen acceptance retains old-loader no-Skill and valid-Skill controls,
  missing/partial metadata dry-run and Apply, confirmation/cancellation byte
  preservation, repeat Update with zero effects/migrations, exclusions,
  malformed/foreign/incomplete-management blocks, affected Update and Install
  suites, pinned A/B and hermetic process checks. The real beta4 combined
  candidate remains Root-owned.

### Historical source-freeze milestone — 2026-10-01

At that source freeze, the diagnostic proof and its evidence were archived
with hashes recorded above; seven presentation/document files were partially
authored in this worktree and had not yet been compiled against the frozen
Update types. That dependency and execution topology are superseded by the v2
candidate and PRESENTATION results recorded above. This historical milestone
does not establish final Task70 qualification. The fresh whole-task review and
grouped repair remain unused.

## Historical v2 qualification snapshot — 2026-10-02 (superseded by v6 below)

| Area | State at that snapshot | Remaining at that snapshot |
|---|---|---|
| Candidate | v2 inventory has 78 verified rows, SHA256 `E415A577749C003FB4BB78A004C8B895E51E8A8839E47AB34C0D374DB9390394`; full Release build exit 0, warnings 0, errors 0, receipt SHA256 `BC4951DF919E7F9410206AF5CD83C8BD357956FE8DC4A6109322C3258CF7AF7B`. | Guarded v3 assembly and a fresh nonincremental Release build. |
| v2 test lanes | CORE 125/125; TYPES 63/63; PRESENTATION 28/28. | Requalify affected filters against v3. |
| PROCESS and REGRESSION | Their v2 evidence exposed the unexpected planning and invalid fixture described above. | APPLICATION correction, REGRESSION fixture correction, and affected v3 filters; REGRESSION's nine boundary cases have not run. |
| Fixed v3 filters | The six-lane filter set remains CORE 13 classes/125, TYPES 4/63, PRESENTATION 4/28, REGRESSION 2/13, APPLICATION 6 classes (shared topology 19 + document 25 + Update 4), and PROCESS 4 hermetic classes/15 plus 10 real-pinned archive journeys. | Execute against the final v3 candidate with zero Windows skips and preserve required streams, status, hashes, TTY, and archive evidence. |
| Final gates | The source freeze and accepted policy are recorded here. | One fresh whole-task GPT-6.1 Sol high review, one grouped repair if findings require it, and Root's combined beta 5 native/package and real beta 4 upgrade acceptance. |

The v2 source inventory and build receipts are in the executor worktree. The
APPLICATION correction receipt is `C:/Users/Tedy/.worker-watch/worktrees/a_be46715e1b33/artifacts/task70/application-path-correction.json`
(SHA256 `9A59BE8212E91CF05088495869B2F618F236BB02EE026635683E575E2B09C599`).
The REGRESSION fixture correction receipt is
`C:/Users/Tedy/.worker-watch/worktrees/a_4497d8f827b6/artifacts/task70/regression-fixture-correction.json`
(SHA256 `28822F31CCC9BCA745DE8CCC75E19F6B94026C7CBA50AF865BC229BCCCABAE90`).
These two corrected files are among the three queued for the sole guarded v3
assembler run `r_3739bc8fce15`; its packet is documented in MAIN at
`artifacts/task70/orchestration/assembly-v3.txt`. At that snapshot, no v3 build or qualification receipt was available; later v3-v6 evidence below supersedes that status. The earlier public beta 4 Apply
numeric exit limitation documented above remains unchanged.

## Historical v3 recovery qualification correction — 2026-10-02

This appended status supersedes the preceding v3-pending statements only for
the evidence and recovery issue recorded here; the earlier phase capsule and
its receipts remain historical. The v3 shared Release build is recorded at
`C:/Users/Tedy/.worker-watch/worktrees/a_025cfc30c08d/artifacts/task70/shared-build-receipt-v3.json`
(SHA256 `85DAA3F31B84F7C048CD4B08B59AE69440E6F5AAC9C53F6636E4565727056C1B`),
with source inventory SHA256
`039BED077302FB062908498499A972F1EF3E38C651E22D159FBD97092FD2763A`.
PRESENTATION v3 passed its four assigned direct-DLL filters, 28/28 with zero
failures and skips; its receipt is
`artifacts/task70/LANE-shared-qualification-v3.json` (SHA256
`918E8A05277E16734220B5DE421E559ED4624B7327A001D9CC2D05F3BC9F8B99`).

PROCESS v3's first four `PublishedUpdateAdoption` cases produced three
failures and one pass, zero skips, runner exit 2. The original v2 physical
planning failure is fixed in v3; missing/partial automatic Update previews
now plan four effects, but Apply fails during recovery preparation with
`update.operation-failed` (“Update recovery preparation failed unexpectedly”)
and zero effects. The TTY confirmation path exited 1; decline passed with
exit 130. Raw stdout is at
`C:/Users/Tedy/.worker-watch/worktrees/a_0f9f108d71df/artifacts/task70/shared-checks/PROCESS-V3-RUN/20261001221521880-d6d36cd1df3142d1a037f5979a60af9e/01-PublishedUpdateAdoptionProcessTests/stdout.bin`.
The lane receipt is
`C:/Users/Tedy/.worker-watch/worktrees/a_0f9f108d71df/artifacts/task70/LANE-shared-qualification-v3.json`
(SHA256 `8FAAEC9671D39BAF41D6486F8BA072D60AE4030821917FE926AC7DC0769CE9F5`).
The remaining 11 hermetic cases and all 10 pinned archive journeys were not
run under the stop rule.

The APPLICATION first filter completed before the stop: 8 cases, 7 passed,
1 failed, zero skips, runner exit 2. Its fixture helper attempted to create
an empty parent-directory segment for root-level `AGENTS`/`CLAUDE` payloads:
`Path.GetDirectoryName` returns an empty string for those targets, which the
helper retained and `CreateDirectory` rejected. The bounded fixture correction
skips empty or whitespace parent paths because the workspace root already
exists; assertions remain unchanged. The raw stdout is at
`C:/Users/Tedy/.worker-watch/worktrees/a_be46715e1b33/artifacts/task70/shared-checks/APPLICATION-v3-RUN/01-UpdateAdoptionEffectPlanningTests/stdout.bin`.
The original APPLICATION test run was confirmed stopped; a brief interim
author run stopped without edits, and the same owner resumed as
`r_027151aa1981` with this fixture evidence. The remaining five APPLICATION
filters and all REGRESSION filters are unrun. The focused APPLICATION class
remains expected to have 11 cases (eight existing plus the three frozen cases
below); existing assertions remain intact.

CORE v3 completed 13 classes, 125 passed, zero failed or skipped; its receipt
is `C:/Users/Tedy/.worker-watch/worktrees/a_aed2fa9ed36c/artifacts/task70/LANE-shared-qualification-v3.json`
(SHA256 `88F68F7AFDDFFC388A382296F4350E6E3593FFF77163E5415CB6307EED3D825E`).
TYPES v3 completed four classes, 63 passed, zero failed or skipped; its receipt
is `C:/Users/Tedy/.worker-watch/worktrees/a_aa115dab04da/artifacts/task70/LANE-shared-qualification-v3.json`
(SHA256 `EEE1C9DC57C95CC0CD931CCCACEA5A00F6609C8FA31AA43BC6098A66A29F40D9`).
Together with PRESENTATION's 28/28, the three completed lanes total 216
passed, zero failed, zero skipped. This is staged lane evidence, not complete
Task70 qualification. These are qualification observations, not completion
claims.

### Frozen APPLICATION recovery correction

APPLICATION (`a_be46715e1b33`) owns only
`src/cli/operations/OpenForge.Cli.Operations/Commands/Update/Shared/Recovery/UpdateRecoveryOperation.cs`
and
`src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Update/UpdateAdoptionEffectPlanningTests.cs`.
Keep the prior physical-planner correction unchanged. Recovery preparation
currently selects a snapshot by matching `effect.Path` against comparison
observations; adoption-only effects have no such observation. Preserve the
ordinary comparison selection as primary and use the exact captured
`UpdatePlanExecution.AdoptionTargets` snapshot as the bounded fallback. Do not
reread current state, invent a nested command or new model, or admit unowned
paths. Existing `RecoveryBundleTarget.Create` validation continues to enforce
the planned expectation, missing-versus-file kind, and exact bytes; missing
or ambiguous captured sources fail closed. Ownership snapshot handling,
store/attribution, leases, publication, and cleanup remain unchanged.

Extract the existing target formation into the internal pure
`ReadTargets(UpdatePlanExecution) -> IReadOnlyList<RecoveryBundleTarget>` and
have `Prepare` consume it. Add three focused cases: adoption-only existing
replacement retains the exact original snapshot and bytes; adoption-only
missing catalogue retains the exact missing expectation and normal create
recovery semantics; and absent captured observation/adoption target fails
closed. Keep current coalescing, revalidation, byte, and guard assertions.
Expected focused class count is 11 from its current eight plus these three,
unless source fact discovery shows otherwise; report any count discrepancy
before changing the expectation.

After the exact two-file source-ready receipt, the sole assembler adds one
production delta for v4: expected inventory 79 rows (76 Task70 delta, two
separate Task47 prerequisites, and this record separate). Then run one full
cache-only Release build and the affected six fixed lanes against the shared
read-only v4 candidate; retain v3 results as historical. PROCESS remains 15
hermetic cases plus 10 pinned archive journeys, and the new Update integration
class has 13 cases. Require zero Windows skips. The fresh GPT-6.1 Sol high
whole-task review and grouped post-review repair remain unused; this bounded
pre-review correction does not consume them. Root's real beta 4 to combined
beta 5 acceptance remains a separate gate. No v4 build, test, or final Task70
acceptance is claimed here.

## Historical v4 stage results and v5 correction freeze — 2026-10-02

The v4 shared Release build completed with exit 0, zero warnings, and zero
errors. Its 79-row inventory is 76 Task70 changes, two separate Task47
prerequisites, and this record separately. Build receipt SHA256 is
`2B433545BB3F789F5E736806B7B40415E70EBC7A3F42E28B9EFD56FC2622215E`;
inventory SHA256 is
`22D93F60735D1136CBC1C7FA4DD0238A3D2E81276A340B3224C833C4ECFE5601`.
CORE (13 classes), TYPES (four), APPLICATION, and PRESENTATION completed with
125, 63, 75, and 28 passes respectively, with zero failures or skips: 291
passes total. PRESENTATION's v4 receipt is
`artifacts/task70/LANE-shared-qualification-v4.json` (SHA256
`FFBEE3D35A0CFC3DB9B662EB056E100E0202089ABF4B022B223D36913FA9BC8D`). These
are staged results, not final Task70 qualification.

PROCESS v4's first four cases had two passes and two failures; both repeat
checks failed because each repeat Update had zero effects but reported one
planned `NavigationUpdated` migration for `.agents/skills/_skills.md`. TTY
confirmation and decline passed. Its
remaining 11 hermetic cases and 10 pinned archive journeys were not run.
REGRESSION v4's first four cases had two passes and two failures: one repeat
phantom migration row and one incorrect managed-host ownership helper. Its
nine boundary cases were not run. All v4 readers have completed and are no
longer running. The Root-owned beta 4 to final beta 5 upgrade remains
unexecuted and separate. No fresh whole-task review has been used; one grouped
repair remains.

### Frozen v5 source packets

These bounded corrections preserve the accepted policy, frozen recovery API,
and ordinary metadata behavior. They are source packets for the next shared
candidate, not a qualification or completion claim.

- **CORE** (`a_aed2fa9ed36c`) owns only
  `src/cli/operations/OpenForge.Cli.Operations/Commands/Update/Shared/Planning/UpdatePlanBuilder.cs`.
  After `UpdatePhysicalEffectPlanner.Plan`, filter `intended.Migrations` to
  actual planned non-directory file effects using exact canonical
  workspace-relative `ResultEffect.Path`. Put the same filtered migrations
  into the intended state passed to preview and into execution, preserving
  observations, projection, adoption targets, and every other field. Preview
  and execution agree: no effects means no migrations; planned/cancel rows
  exist only for actual effects. Verification and applied-fact behavior stay
  unchanged. Do not relax parsing, Index, planning policy, or ordinary
  metadata behavior. Keep repeat assertions in
  `PublishedUpdateAdoptionProcessTests` and
  `UpdateWorkspaceAdoptionIntegrationTests` unchanged. The assembler may copy
  the precise existing init-property model only if needed; do not author its
  shared schema.
- **REGRESSION** (`a_4497d8f827b6`) owns only
  `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Update/UpdateWorkspaceAdoptionIntegrationTests.cs`.
  Correct `AssertFrameworkDoesNotOwnAdoptedFilesOrCatalogues`: the existing
  managed `.agents/skills/_skills.md` remains Framework-owned, while adopted
  native Skill originals and new local resource catalogues are not whole-file
  Framework-owned. Keep the generated-region Entries-only check over all
  relevant paths and preserve all byte, support, overwrite, status, effect,
  migration, and repeat assertions, including
  `Assert.Empty(result.Migrations)`. Add no cases, waive no assertions, and do
  not change expected totals.

The paragraph above records the frozen v5 handoff plan as it stood before the
v5 build and lane runs. Verified v5 results and the remaining exclusion-boundary
test correction are recorded below; no whole-task acceptance is claimed.

## Historical v5 qualification status and exclusion-boundary regression freeze — 2026-10-02

This current-phase update preserves the earlier v2-v5 failures and
interruptions, the rejected/unrun PRESENTATION claim, and the 265-character
TMP prelaunch correction as history. The shared v5 build completed with exit
0, zero warnings, and zero errors. Its inventory is 79 rows: 76 Task70
changes, two separate Task47 prerequisites, and this record separately. Build
receipt SHA256 is
C0762AE7AB2BF3E819527CC6571CA600A6A1A98BCEE50E13E5FCAEFAF7628813;
inventory SHA256 is
2ADA041B7C01843B62152B1D215F47D0E2D13492614E0B6241F6F15AC9108405.

Verified v5 lanes:

- CORE: 125 passed, zero failed/skipped; receipt
  1F067874898407829488C7116B7F29210605C70DE949C8349A251B7CC3DB1C3A.
- TYPES: 63 passed, zero failed/skipped; receipt
  EF3AD5895B6054C9EFC847E5976FF343EE492E894C896E2734456D5BBB2A05CD.
- APPLICATION: 75 passed, zero failed/skipped; receipt
  9AEFBD5673A16E5740C54EED86905D99639571F1306855681874F1794816A420.
- PRESENTATION: 28 passed, zero failed/skipped; actual direct-DLL v5 receipt
  68E7BD03D7789A8A258C38D9CD2F4C1D11262BEE7969D4C51E21710C1694D69B.
- PROCESS: 15 hermetic and 10 pinned cases passed, zero failures/skips; receipt
  5D4D233DAB4E81C88068BEBE85A983BFC74BA8905FF7A2578D001D0B10018D75.
  The pinned archive run recorded 45 captures and 405 runtime hash checks.
  The five lanes above have 306 hermetic passes total; pinned archive cases
  are separate.
- REGRESSION: the adoption cases passed (4); the boundary cases produced
  eight passes and one failure, 12/13 overall. Receipt
  B3FE44EAC644DF28DB098FECDCC5040EF0941E4AC1B71A43F72E4B53FE77E080.
  The failing test over-broadly rejected effects anywhere under the Skills
  root when its fixture excluded only one Skill directory. It did not
  demonstrate a mutation inside that excluded directory.

The separate diagnostic at
C:/Users/Tedy/.worker-watch/worktrees/a_4497d8f827b6/artifacts/t70/exclusion-proof-6ec5cef9/exclusion-proof.json (SHA256
53E2CB28716A61005F0D306E2356A06842A73F99BAF3616F08EAD640EAFE2142)
used a v5 Install followed by an authored loader-comment edit. It is not an
actual beta 4 binary upgrade. Install, Update dry-run, and Update apply exited
0. Update planned/applied exactly .agents/loader.md and the unexcluded
parent .agents/skills/_skills.md; verification succeeded with no
creations/deletions and no migrations collection. The excluded Skill subtree
(native Skill, guide, and directory bytes) remained unchanged, and no local
catalogue was created. The readonly parent Entries projection retained valid
Skill metadata; retaining the strict full catalogue/projection inputs remains
required. This diagnostic demonstrates no exclusion failure inside the
excluded unit and does not justify filtering the catalogue or relaxing strict
projection.

### Historical REGRESSION-only test freeze before v6

Before REGRESSION edits, the bounded packet is frozen to
src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Update/UpdateWorkspaceAdoptionIntegrationTests.cs
(current pre-edit SHA256
6C419A9E67E112766DBF0EFC5AA74B9F9FE9CBF47BE117CE4AF558EEE076FCCF).
No production, contract, or shared API files are part of this packet. The
accepted Update policy remains unchanged: keep the complete catalogue and
strict projection inputs; filter only adoption candidates by the exact
excluded unit.

1. Correct the existing
   RemovedDirectoryOrCategoryPreservesValidSkillAndCatalogues assertion
   helper. For a removed directory, the excluded unit is exactly
   skill.DirectoryPath; for a removed category, it is exactly
   .agents/skills. Reject effect paths equal to the unit or below it
   (unit + "/"). Keep Assert.Empty(allMigrations) and every existing
   byte, support, settings, user-ownership, status, loader, apply, repeat,
   overwrite, and effect assertion. Preserve the complete .agents/skills
   parent bytes when the category root is excluded; the parent may be
   projected when only a child directory is excluded. Add an explicit
   reference-byte preservation assertion if the existing test lacks one.
   Keep native Skill metadata visible through the strict readonly projection.
2. Add one meaningful theory with two cases beneath a removed Skills
   directory: a native Skill with missing frontmatter, and partial frontmatter
   with name and license but missing description. Use the existing
   fixture helper and ordinary old-payload ownership. Both cases must retain
   the existing strictBlockedGeneratedRegionUnsafe result, have zero effects
   and zero migrations, preserve all workspace bytes/hashes and support files,
   and create no local catalogue. Do not silently complete or adopt excluded
   source documents and do not suppress their projection facts.

The corrected REGRESSION class has 15 cases: its existing four adoption cases
plus 11 boundary cases. The v5 class had 13 cases; the two added theory cases
raise the whole hermetic total from 319 to 321 without removing or waiving an
assertion. The pinned 10-case PROCESS archive run remains unchanged. After the
test-only change is source-ready, the sole assembler freezes a candidate and
qualifies the resulting test binary; receipts bound to unchanged v5 binaries
remain valid for those exact bytes. Do not rerun unchanged filters
gratuitously.

At the v5 freeze, the fresh whole-task review and grouped repair had not been
used, and Root's actual beta 4 to combined beta 5 gates remained pending. The
v6 qualification and current review state are recorded below. No final Task70
acceptance was claimed at the v5 stage.

## Historical v6 qualification reconciliation and review handoff — superseded by v9

**Current state:** the v6 candidate source inventory is E7687744E6469BC15A3E0337FC766D826664E33A13623D0B5E086E31B088A53B, 79 rows: 76 Task70 delta, two separate Task47 prerequisites, and one Working record. Its source hash check reported zero mismatches. The candidate freeze included this record at SHA256 8F1F7BC69A507405CE488C06F9340F6C63BD28FCFD68893075BDDEF162C25A0B; this subsequent record reconciliation is not in the tested candidate and does not change its production source set.

The v5 full Release production build remains the sealed baseline, receipt C0762AE7AB2BF3E819527CC6571CA600A6A1A98BCEE50E13E5FCAEFAF7628813. The v6 shared test build was the exact Integration test project compile with BuildProjectReferences=false, exit 0, zero warnings, zero errors, elapsed 56.85 seconds; it was not a full-solution v6 build. Receipt: C:/Users/Tedy/.worker-watch/worktrees/a_025cfc30c08d/artifacts/task70/shared-test-build-receipt-v6.json, SHA256 6358104D93CA25092F6B5ACEB989D2C8D5F51127815BD0570D5E3D49EFC6FA87. There are no production-source changes versus sealed v5. The only Task70 source delta is the test-only regression file, with hash F4000972991B60E3D75FC15667107D1870C6ED19D2768E9EE84EA894384EA3B4; this Working record is separate. The new Integration test DLL is CFF29275F7E78CD04357F939CF75705BA3A3FF5B14D4AE75848316CDEF782C08; CLI DLL remains 786DF4C49D2D179262FF92D088F44E13DC115AB48F8FF0F2093F40E63565F693. The 33 published CLI files, Unit DLL, and EndToEnd DLL retain their v5 identities: 35 unchanged binary identities total.

The final hermetic result is 321 passed, zero failed, zero skipped: CORE 125, REGRESSION 15, TYPES 63, APPLICATION 75, PRESENTATION 28, and PROCESS 15. The separate pinned local process run adds 10 passed, zero failed or skipped. Unit 166, EndToEnd 15, and pinned 10 checks retain identical v5 bytes; Integration 140 checks ran against the fresh v6 Integration assembly. Do not claim all 321 were freshly run against a v6 binary. All raw class-filter numeric exits were zero.

- CORE: 13 filters, 125 passed. Receipt C:/Users/Tedy/.worker-watch/worktrees/a_aed2fa9ed36c/artifacts/task70/LANE-shared-qualification-v6.json, SHA256 C72E7BDE5BD6BE5C830B1426DB842EC9FE2031BD0EB3851B1ABB42267FFFE8F6.
- REGRESSION: two filters, four adoption cases plus 11 boundary cases, 15 passed. Receipt C:/Users/Tedy/.worker-watch/worktrees/a_4497d8f827b6/artifacts/task70/shared-checks/REGRESSION-v6-2026-10-02T01-12-56-875Z-31672/LANE-shared-qualification-v6.json, SHA256 7CFE6B34C27956718BE342B8A091A49F2906DE442D3F6DFE02FE14252A0DFA45.
- TYPES: four filters, 63 passed. Receipt SHA256 EF3AD5895B6054C9EFC847E5976FF343EE492E894C896E2734456D5BBB2A05CD.
- APPLICATION: six filters, 75 passed. Receipt SHA256 9AEFBD5673A16E5740C54EED86905D99639571F1306855681874F1794816A420.
- PRESENTATION: four filters, 28 passed. Actual direct-DLL v5 receipt SHA256 68E7BD03D7789A8A258C38D9CD2F4C1D11262BEE7969D4C51E21710C1694D69B.
- PROCESS: 15 hermetic plus 10 pinned cases passed. Receipt SHA256 5D4D233DAB4E81C88068BEBE85A983BFC74BA8905FF7A2578D001D0B10018D75.

REGRESSION corrected the exclusion-unit assertion to reject effect paths only when equal to the exact excluded unit or below it. Its two new missing/partial native Skill metadata cases under a removed Skills directory retained strictBlockedGeneratedRegionUnsafe, zero effects, zero migrations, unchanged workspace bytes, and no local catalogue. This test-only correction increased the suite from 319 to 321; no assertions were waived. The v5 12/13 diagnostic remains historical. The preliminary B302 second-class attempt was caught for a missing private constant before copy or compile; run r_40 completed before the attempted stop, and same-owner follow-up r7e produced the final F400 source. There was no failed compile or production change. The earlier premature source-ready report is not qualification evidence.

The Windows PROCESS runner initially hit the 265-character temporary-path limit before launching CLI; shortening only ignored runner environment paths allowed the original 15 hermetic cases to replay and pass. No checked-in test was weakened or skipped. The pinned fixture remains the 18-blob Skill Creator subtree from commit 8a1541c4a3ffa5a20a5a91de0dcf3f0bab1d1ef4, tree d482eba557f7b2035c8a83589809628b96f6f40e, manifest SHA256 DEE669A0BE312D756B0F0E7EC00294F4D7FB71557C99B1EDA93F53086333C064. The upstream root LICENSE is absent and the subtree LICENSE.txt is present; no downloaded code was executed. The pinned process recorded TTY confirmation exit 0, decline exit 130, 45 captures, and 405 runtime hash checks.

The exclusion diagnostic is at C:/Users/Tedy/.worker-watch/worktrees/a_4497d8f827b6/artifacts/t70/exclusion-proof-6ec5cef9/exclusion-proof.json, SHA256 53E2CB28716A61005F0D306E2356A06842A73F99BAF3616F08EAD640EAFE2142. It used a v5 Install plus an authored loader-comment edit, not an actual beta 4 binary. Keep its limited finding as recorded above; it does not broaden adoption or alter the accepted full-catalogue projection policy.

The MAIN transfer preflight is C:/Users/Tedy/.worker-watch/worktrees/a_025cfc30c08d/artifacts/task70/main-final-preflight-v6.json, SHA256 134BC5666912573C550E38243455DCFC8B30FD35440186C330FC40B995F83A07. It checked 77 paths with zero unknowns: 40 destination preimages already matched, 31 were baseline or absent, five matched named earlier stages, and one matched the earlier record stage. MAIN HEAD and real index were unchanged. This is preflight evidence only: no final transfer, patch, or prompt-history move has occurred. The 82 untracked root prompt paths remain inventoried for a later ignored-history move. The planned final transfer covers 77 paths with literal byte-copy evidence and a Git-normalized binary patch, excluding the two separate Task47 prerequisites and prompts; Root performs integration.

**Closed review and handoff:** the whole-task GPT-6.1 Sol high review r_b0d032f1336f completed with four P2 blockers and one P3 finding. It was source-derived, did not execute reproductions, and did not change the candidate. The grouped correction below is frozen but not implemented or qualified. Do not claim a review pass, Root acceptance, or release. Root still owns Task47 prerequisite acceptance, integration of the 77-path transfer, shared ledger and routeEntry integration, public documentation, and the actual beta 4 to final combined beta 5 native, package, site, development, and release gates. These external acceptance gates remain pending. The v6 receipts qualify only the pre-correction candidate bytes.

## Historical closed review and grouped correction specification — completed on v9

The closed review result is preserved at
`artifacts/task70/review/closed-review-r_b0d032f1336f.json` (SHA256
`64FA42EBAE96C32438512CC2F692A6C008F98CFF39397783687976C53BC77B12`). It
reports four P2 blockers and one P3 finding. Findings are source-derived; the
reviewer did not run the reproductions or change the candidate. The reviewer
could not independently verify the earlier exclusion-proof JSON at the
supplied REGRESSION path. This limitation does not alter the final v6 lane
receipts. No second review or council is authorized by this packet.

The following is one bounded grouped correction. Each lane owns only the
listed files; exact preimage hashes are the frozen transfer/edit guards. The
accepted Install/Update policy and shared APIs do not change.

### F1 — preserve managed Install host bytes outside Entries

- **INSTALL** (`a_0f9f108d71df`) owns only
  `src/cli/operations/OpenForge.Cli.Operations/Commands/Install/Shared/Planning/InstallEstablishmentPlanner.cs`
  (v6 preimage SHA256
  `0D4A63AAEE67BDED93F1803D2416ADA690990F98C1532BB981CE2D97FE65AB01`).
  After strict authored-payload admission, project intended generated Entries
  changes onto the captured current host bytes with existing parsed spans and
  mutation primitives. Preserve every byte outside Entries, including the
  existing heading, BOM, and newline style; a semantically unchanged region
  must remain an exact byte no-op. The embedded intended payload remains
  authoritative for managed-base admission. Do not compare the current host
  to itself as running-payload authority or weaken divergence checks. Final
  verification fingerprints and expected effect bytes remain truthful.
- INSTALL may create an optional ignored diagnostic driver that exercises the
  frozen v6 F1–F4 numeric failures without mutating the candidate. It is not a
  substitute for focused tests and must not delay the bounded source repair.
- **CORE** adds the managed-host CRLF regression to its existing owned
  `InstallWorkspaceAdoptionIntegrationTests.cs`. No change to
  InstallIntendedStateBuilder or InstallContentIdentity is planned. If the
  frozen API requires another file, report the exact minimal addition before
  editing outside this ownership.

### F2 — use H1 for native Skill heading fallback

- **APPLICATION** (`a_be46715e1b33`) guard-copies only these two files from the
  idle INSTALL owner, preserving history and checking the exact v6 preimages:
  `src/cli/operations/OpenForge.Cli.Operations/Commands/Shared/WorkspaceAdoption/Shared/WorkspaceAdoptionSkillMetadataPlanner.cs`
  (`7D8BAE06489889CF5351C07FCD8EEC4606B63B482FDC131E7819846026C0A5C4`) and
  `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Shared/WorkspaceAdoption/WorkspaceAdoptionDocumentPlannerTests.cs`
  (`7D6EC34E2CB348262E7CB336D0320FD2EA0D2554AE8DA19673D5002B7331828C`).
  Description/title precedence remains unchanged. Body fallback accepts only
  a top-level level-1 heading with usable visible text. Add two meaningful
  cases: H2 before H1 chooses H1; H2 only falls back to the relative path.
  Preserve body and license bytes. The focused unit class grows from 25 to 27.

### F3 — publish Install migration facts only for planned file effects

- **TYPES** (`a_aa115dab04da`) receives only the following guarded copies from
  idle INSTALL: `src/cli/operations/OpenForge.Cli.Operations/Commands/Install/Shared/Result/InstallResultFactsFactory.cs`
  (`28E90379123B9AE14331E1FFBD23E44C31A89DCD00E135BC38FCBF16D5CFF3E8`) and
  `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Install/InstallMigrationPresentationTests.cs`
  (`C3C0874427763BCDCE0067C596A1643146F354BC80D71E5382C9B12F00363984`).
  A build result without a plan/effects returns no migration rows. PlanStage
  materializes actual file effects once and filters migrations to matching
  non-directory effect paths. Applied remains limited to verified matching
  effects; planned preview/cancel/unstarted rows remain only when backed by
  actual effects. Empty, incomplete, and directory-only outcomes expose none.
  Add two cases; the Install migration presentation class grows from 8 to 10.

### F4 — preserve actual user-owned source provenance in Update migrations

- **TYPES** owns its existing
  `Update/Models/Planning/UpdateMigrationPlan.cs`,
  `Update/Models/Result/UpdateMigration.cs`,
  `Update/Shared/Result/UpdateMigrationFacts.cs`, and
  `Commands/Update/UpdateMigrationResultTests.cs`. Add
  `IsUserOwnedSource` with public init and default false to the plan, and
  internal init/default false to the migration while keeping its existing
  five-parameter constructor; preserve the plan value through facts. Do not
  add a JSON/wire field: the migration row remains
  `path/actions/fields/derivation/outcome`. Add true/false preservation cases
  with outcomes unchanged; the result test class grows from 13 to 15.
- **CORE** (`a_aed2fa9ed36c`) owns only
  `src/cli/operations/OpenForge.Cli.Operations/Commands/Update/Shared/Planning/UpdateGeneratedNavigationPlanner.cs`
  and its existing
  `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Install/InstallWorkspaceAdoptionIntegrationTests.cs`.
  Annotate final migrations using actual whole-file management facts: false
  for selected payload-source paths or Framework whole-file receipts, true
  only for compatible local adopted/unowned generated-region hosts in the
  existing authorized selection. Never infer ownership from actions alone;
  the managed `.agents/skills/_skills.md` remains Framework-owned. Add three
  Install cases: managed CRLF host preservation; managed loader divergence
  with partial Skill blocks at zero effects/migrations; and an occupied exact
  ordinary payload destination with partial Skill blocks at zero effects and
  migrations without force. Preserve diagnostics and existing assertions.
  The class grows from 15 to 18.
- **REGRESSION** (`a_4497d8f827b6`) owns only
  `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Update/UpdateWorkspaceAdoptionIntegrationTests.cs`
  (v6 preimage SHA256
  `F4000972991B60E3D75FC15667107D1870C6ED19D2768E9EE84EA894384EA3B4`).
  Add one normal managed-workspace case: after Install adopts a user
  references catalogue, add a second reference and valid Skill; Update
  dry-run/apply succeeds, the navigation-only user catalogue migration is
  marked user-owned, and managed `_skills.md` navigation remains Framework
  owned. Preserve source/support bytes, whole-file and region ownership
  receipts, and zero-effect/zero-row repeat behavior. Do not fake the flag.
  Adoption grows from 4 to 5; the 11 boundary cases remain unchanged.
- **PRESENTATION** (`a_6e3644443722`) owns only
  `src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Update/Shared/Selection/UpdateReportSelector.cs`,
  `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Presentation/Update/UpdateMigrationPresentationTests.cs`,
  and `src/cli/output-text/OpenForge.Cli.OutputText/Update/UpdateText.cs`.
  Use `migration.IsUserOwnedSource`, not action-derived ownership. Preserve
  Framework attribution for false and suppress Framework source/payload
  counts for true. Mark existing fake user fixtures true. Add two navigation-
  only user/Framework cases; the presentation class grows from 6 to 8.
  Clarify that Update requires complete Framework management and can adopt
  compatible missing metadata/entrypoints in selected standard local routes;
  it does not establish absent/unknown management or claim whole-file user
  ownership. Add no wire field or new framework.

### Historical v6 qualification handoff plan — superseded by v9

Expected fixed filters are CORE 128, REGRESSION 16, TYPES 65, APPLICATION 77,
PRESENTATION 32, and PROCESS 15: 333 hermetic passes total, plus 10 separate
pinned journeys, all with zero failures or skips. No new tracked source files
are planned; the candidate remains the existing 76 Task70 paths, two separate
Task47 prerequisites, and this record separately (79 inventory rows). The
pre-review v5/v6 receipts remain historical and do not qualify corrected
bytes. After all source-ready handoffs, the sole assembler must create one
shared full Release candidate/build, run the six read-only qualification
lanes, and replay the 10 pinned journeys. Root's actual beta 4 to final
combined beta 5 native/package acceptance and release gates remain pending.
This record freezes the repair scope; it does not claim implementation,
qualification, Root acceptance, or release.

### Historical PRESENTATION v7 fixture correction — 2026-10-02

The sealed v7 full Release candidate remains unchanged (build receipt `CFCD453CB9FA3A78D70007B5CE5BF3CEE3BBA39951DEE0494051E35F4AF14BB4`; inventory `374A8F018A7FEA04F20AF5A4FD1997A02F584A13DE3B862E94D414F0F13C358C`). The actual PRESENTATION v7 receipt is `artifacts/task70/LANE-shared-qualification-v7.json` (`3264F4294C31E6E69281EE93C16270174F06D7E04C230EEA41F18E0EAE9DFE72`): 8 cases, 6 passed, 2 failed, 0 skipped, runner exit 2; three other lane filters were unrun under the stop rule. Both failures were fixture-construction errors in `NavigationOnlyFrameworkSkillsCatalogueKeepsPayloadAttribution` and `NavigationMigrationDoesNotReclassifyItsFrameworkHostAsUserOwned`: the generated-region logical-change contract requires `sourceAssetPath: null` (`UpdateLogicalChange.ValidateCoordinates`, `UpdatePhysicalEffectModels.cs:133`). The preserved raw stdout SHA is `1E62C4104A698E47524C40998F003884075EFC10C74423C148380B55DEF6312D`.

The bounded correction changes only those two test fixtures to pass null source provenance and assert `SourceAssetPath` is null because their result comparisons are empty. Framework source identity, section kind, managed-host `IsUserOwnedSource == false`, action/count/JSON assertions, and the existing user-owned navigation case remain intact. The PRESENTATION class stays at 8 cases; no product policy, API, production code, or assertion coverage is relaxed. Qualification remains pending.

REGRESSION v7 is separate historical evidence: receipt `CB9C28AD5B53DFE2FD480042F07F5A276F404B215889498DD4FFFF57369E743B`, 5 cases with 4 passed and 1 failed, exit 2; its boundary cases were unrun. Its author is correcting only the expected ordered actions for newly added catalogues (`EntrypointCreated`, then `NavigationUpdated`); no REGRESSION file is changed here.

After all v7 readers stop, the sole assembler may guarded-copy the two corrected test files and this record, keep the 79-row inventory shape (76 Task70 paths, two separate Task47 prerequisites, and the record), compile only the affected test projects with `BuildProjectReferences=false`, Release, `--no-restore`, `--no-incremental`, and disabled build servers, while preserving the v7 production CLI closure and Operations identity. Existing green v7 CORE/TYPES/APPLICATION/PROCESS results remain bound to their unchanged binaries; only corrected PRESENTATION 32 and REGRESSION 16 filters are to be rerun. This is not a full solution v8 build or a claim that all 333 filters ran against one v8 test-binary set. Final qualification remains pending.
### Historical INSTALL F1 expected-state freeze — superseded by v9 lifecycle packet

The v7 CORE result exposed a required production correction, superseding the earlier test-only v8 assembly plan above. Preserve the prior v7 receipts and failures as history. The v7 CORE execution reported 109 total, 108 passed, 1 failed, 0 skipped, exit 2; 18-case Install class was 17 passed and 1 failed, and the 19 Extension cases were unrun. The failure was `CrLfHostOutsideEntriesSurvivesDryRunApplyAndRepeat`: apply reached `Failed` after a completed dry run. The sealed v7 candidate and all prior stage evidence remain unchanged; PROCESS run `r_440027419c3f` was stopped before further tests and has no owned process remaining.

Frozen production ownership remains only `src/cli/operations/OpenForge.Cli.Operations/Commands/Install/Shared/Planning/InstallEstablishmentPlanner.cs` in the original INSTALL lane (v7 source SHA `FA6CB4BB27B928DB9097F3F5835AE337E1E1D4F0B8167ABF30CAD27DB55D5012`). After existing strict `VerifiedManagedTargetPaths` admission, form final expected bytes for verified managed generated-navigation hosts using the existing parsed, bounded Entries rebase. Ensure `InstallPlanContext.IntendedState.TargetBytes` holds exactly the same final bytes planned for those physical effects, including unchanged managed Entries. Rebase before exact-byte comparison so an unchanged CRLF Entries region remains a no-op. Do not alter initial unverified-force behavior or relax content identity, fingerprints, parser, Index, verifier, or trust admission; preserve snapshots, leases, recovery, application, and verification.

The source-derived cause is that `FrameworkContentIdentity.ReadGeneratedEntriesFingerprint` hashes raw Entries bytes. Establishment planning rebases the physical effect for a CRLF host, but the completion context still carries embedded LF-derived target bytes; `InstallAppliedVerifier` then compares the current CRLF fingerprint against that stale target. The fix must coalesce those final bytes across planning and completion without changing admission authority. Existing CRLF prefix/suffix, native/reference bytes, ownership and region receipts, dry-run/apply/repeat-zero-effect assertions remain required. The INSTALL author will first reproduce the failure using the frozen v7 CLI in a disposable artifact fixture and record numeric exit/status/hash evidence; this is not a beta 4 baseline run.

The PRESENTATION test fixture is source-ready at `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Presentation/Update/UpdateMigrationPresentationTests.cs` SHA `1CBC6244BEDC700A235D8B2768A7DB7BC96D0F4CF5355DFB6F2B1E9C9A23527D`; the separate REGRESSION fixture is reported source-ready by its owner; its exact SHA remains in that owner's receipt. Together with the one INSTALL production file and this record, the corrected inventory remains 79 rows (76 Task70 paths, two separate Task47 prerequisites, one record). The sole assembler must wait for all readers, then make one full Release v8 build with `--no-restore`, `--no-incremental`, and disabled build servers. All six read-only lanes (333 hermetic cases) and the 10 pinned journeys must run against that final candidate/runtime. Earlier green or failed v7 results remain stage history and do not qualify v8. Final Task70 qualification, Root acceptance, and actual beta 4 to combined beta 5 acceptance remain pending.
### Historical F1 lifecycle completion freeze — v8 findings, superseded by v9 qualification

The sealed v8 candidate is stage evidence, not final qualification. Its full build receipt is `97B18D550E00B20F04C6CC62FC42B6714172883505325F6E47B31185A744208D`; inventory is `7940ADDCC2F317FC6D419FF5DAC4579CDEDA414326643A5C0263CBD886AE4E94`. Reported v8 stage outcomes: TYPES 65, APPLICATION 77, REGRESSION 16, and PROCESS 15 plus 10 pinned journeys passed. CORE reported 109 total, 108 passed, 1 failed, 0 skipped, exit 2; the Install class was 17/18 and Extension’s 19 cases were unrun. PRESENTATION’s actual v8 receipt is `artifacts/task70/LANE-shared-qualification-v8.json` SHA `D4D94FF04A700AECA1A56AD2D7EC46A138A64CCBA79E7711D4DCBA8C14D16203`: its first filter ran 8 cases, 7 passed, 1 failed, 0 skipped, exit 2; the remaining 24 expected cases were not run under the stop rule. `NavigationOnlyFrameworkSkillsCatalogueKeepsPayloadAttribution` now fails because the Framework-owned generated host first fails at line 362: `ChangedFiles` was expected 0 but actual was 1. The later `filesReplaced` assertion was not reached; the corrected fixture expects 1 because the selector counts this Framework-owned non-directory file effect. The v7 failures and stops remain historical and are preserved.

The final bounded F1 correction requires three existing production paths, all owned by original INSTALL lane `a_0f9f108d71df`; this supersedes the earlier one-file F1 freeze. The exact v8 preimages are:

- `src/cli/operations/OpenForge.Cli.Operations/Commands/Install/Shared/Planning/InstallEstablishmentPlanner.cs` — `345B53F5F7574D0E219D8F6BC2357277E118C08E8C3EE048BEF66095E6DEA5A7`
- `src/cli/operations/OpenForge.Cli.Operations/Commands/Install/Models/Planning/InstallPlan.cs` — `426EC6FE42DA700B9CC3BA2D07B27F425BDCAE0D07851D9DC54F2C2B26F98B00`
- `src/cli/operations/OpenForge.Cli.Operations/Commands/Install/Shared/Operation/InstallApplicationPreconditionValidator.cs` — `99A0E02C8A784B570AB6662A3721015B8B8A2A57FF9E3DA1ABA1A05014A62691`

The v8 source cause is a split between physical expected bytes and lease-time projection equality. `InstallPlanContext.IntendedState.TargetBytes` now contains the correct final CRLF bytes for physical verification, but the lease validator rebuilds the authoritative projection as LF and compares it to those CRLF bytes with `IntendedStateEquals`; this rejects unchanged state. Preserve both roles with only one nullable command-scoped immutable planning fact: optional `InstallIntendedState.ProjectionTargetBytes` of type `IReadOnlyDictionary<string, byte[]>?`, default null. Keep `TargetBytes` as final physical expected bytes. Only after existing strict `VerifiedManagedTargetPaths` admission, retain the original authoritative projection map in `ProjectionTargetBytes` when bounded generated-host rebasing creates a distinction; otherwise leave it null. Lease equality compares `planned.ProjectionTargetBytes ?? planned.TargetBytes` with the freshly rebuilt `current.ProjectionTargetBytes ?? current.TargetBytes` by exact bytes. Every other projection, ownership, path, migration, adoption, snapshot, lease, recovery, and mutation-revalidation comparison remains unchanged. Do not normalize strings, skip checks, derive trust from current bytes, relax fingerprints/admission, or change parser, Index, verifier, wire schema, or metadata authority. For a complete verified managed plan with zero physical effects (including ownership/directory-only work), the planner returns `TrustedExact`; actual migration effects remain `ManagedAdoption`. Initial unverified-force behavior stays unchanged.

Two existing test files complete the six-existing-path packet; no tracked files are added:

- CORE owns only `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Install/InstallWorkspaceAdoptionIntegrationTests.cs`, v8 preimage `E8541BAC861EF71D5F198B7EFDB6DCAF0BBD1BAAC215EE45BF8837D3A97DC41C`. In the existing CRLF case, add dry-run/apply/repeat failure diagnostics without weakening its expected-complete checks, and assert repeat management classification is `TrustedExact` alongside existing zero-effect/zero-row and byte-preservation assertions. Keep 18 cases.
- PRESENTATION owns only `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Presentation/Update/UpdateMigrationPresentationTests.cs`, preimage `1CBC6244BEDC700A235D8B2768A7DB7BC96D0F4CF5355DFB6F2B1E9C9A23527D`. Correct the two Framework-host navigation assertions to `ChangedFiles == 1` and `filesReplaced == 1`; retain one section update, Framework source identity, null generated-region asset provenance, action and wire assertions. The user-owned catalogue remains 0 files changed/replaced with no Framework provenance. Keep 8 cases and do not change selector/counting production code.

After source-ready handoffs, the sole assembler guard-copies the six existing paths (three INSTALL production files, the CORE and PRESENTATION tests, and this record), preserving the 79-row inventory shape: 76 Task70 paths, two separate Task47 prerequisites, and the record. Build one cached full Release v9 candidate with no restore, no incremental compilation, and disabled build servers. Requalify all six lanes (333 hermetic cases) and all 10 pinned journeys on that final production runtime; the v8 PROCESS result is invalidated by this production change and must be replayed. Prior v8 lane results remain stage history. Final qualification, Root acceptance, and actual beta 4 to combined beta 5 acceptance remain pending. This freeze adds no policy, review, or council and makes no acceptance claim.
