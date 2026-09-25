---
open-forge:
  description: Track accepted mapped Library leaf projections, scoped permissions, recovery and complete managed/native evidence
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Workspace, Library, Destination, Symlink, Complete]
---

# Task 25: Workspace Library Destination Projections

## Task State

- State: Complete and locally integrated at `3b4aba9d` after Tasks 23
  and 24. The user accepted the final
  folder-scoped Library proposal and optional destination mapping on 2026-09-08.
- Permanent mapping: Task 25 “Workspace Library Destination Projections” in the
  [project control ledger](../project-control.md).
- Sources: [Workspace Libraries idea](../../ideas/workspace-libraries.md),
  [Extensions Evolution idea](../../../emerging/ideas/extensions-overhaul.md),
  and the accepted first-release design at the immutable contract tip recorded
  in [Task 23](workspace-libraries.md).
- Parent: [Complete The Replacement CLI](00-cli-development.md).
- Phase and milestone: phase 5/5, milestone 8/8.

## Accepted Functional Direction

The user's explicit approval supersedes the pending and candidate wording in
the historical discussion below. The accepted behavior is:

- The attached source directory scopes the Library's recursively discovered
  eligible ordinary files. No mandatory `content/` or `.agents/` child exists.
- Attach creates relative symlinks for individual leaf files. Destination
  directories are ordinary directories, never directory symlinks.
- Optional `--to <workspace-relative-directory>` selects the destination root.
  Omission selects the workspace root. Source-relative suffixes are preserved
  below that root. Record the mapping once for Inspect, Sync, Detach and recovery.
- Library permissions may grant a destination folder and all future descendants.
  Prompt-capable mutations show uncovered destinations and explicitly describe
  the future scope before remembering approval. Exact file grants cover leaves
  for which a folder grant would be too broad. Never offer the workspace root
  as a recursive grant. JSON, redirected and dry-run operation remains prompt-free.
- Existing implicit `.agents/` permission, protected paths, collision refusal,
  no-follow ancestry, and source-preserving behavior remain. Permission grants
  do not confer ownership or authorize replacing unrelated occupants.
- Multiple Libraries may share ordinary destination directories but cannot own
  the same leaf. Sync reconciles links for additions, retirements and missing
  projections. It never synchronizes file contents or performs Git operations.
- Extensions retain `content/` and their existing exact-file permission policy.
  Library folder grants cannot widen Extension permission.

## Execution Capsule

- Outcome: implement the accepted flow across all five Library commands and
  their shared record, permissions, recovery, CLI composition and public prose.
- Profile: streamlined assured. This local developer tool modifies consumer
  links and management records; it must preserve unrelated files and source
  bytes under ordinary defects, interruption and cooperating processes. Existing
  bounded recovery evidence and manual repair apply. No malicious same-user
  guarantee is added.
- Architecture: use the existing typed BCL filesystem, mutation, permissions,
  source-generated serialization and interaction foundations. Library mapping
  and grant-selection policy remain Library-owned. No new dependency, registry,
  DI, reflection, compatibility reader, JS implementation or native bridge.
- Baseline: `de1e1b7a`, accepted Task 24 runtime evidence. Working branch:
  `codex/library-destination-projections`. The two prior discussion edits are
  preserved and incorporated. Other worktrees remain untouched.
- Ownership: root owns acceptance, contracts and coordination. One bounded
  read-only architecture inspection precedes a continuous implementation owner.
  Shared/public implementation, artifacts and integration remain sequential.
- Expected neighborhood: Library commands and mirrored tests; Framework Library,
  permission, filesystem/recovery models and direct consumers; root composition;
  Library/shared contracts and current public documentation.
- Protected scope: frozen MVP, packages and dependencies, publication/global
  installation, unrelated worktrees, Extension behavior and pure Task 26 work.
- Evidence: pure grammar, mapping, grants and records at Unit tier; real leaf
  links, directories, prompts, races, recovery and preservation at Integration;
  exactly three public journeys per Library command, fifteen in total. Retain
  eighteen Extension journeys. Full managed and supported linux-x64 Native AOT
  gates are triggered by shared serialization, mutation and public composition.
  Formatting, static, callable-shape, prohibited-pattern, line-length and
  protected-path gates precede acceptance; refreeze every changed/untracked file.
- Budgets: one architecture inspection; one fresh holistic review including
  ordinary structure/evidence concerns; one coherent writing review; one grouped
  implementation correction cycle plus one bounded ownership follow-up; council
  zero. Consumed: architecture
  inspection T25-A1; coherent contract writing review T25-WR1; fresh holistic
  review T25-R1; grouped implementation correction T25-C1; bounded ownership
  follow-up T25-C2. Root added this two-file follow-up after the
  affected-finding recheck identified one remaining portable-key comparison
  gap; product meaning and external scope do not expand. Ordinary
  prose corrections from T25-WR1 stayed within the contract boundary before Red.
- Stop conditions: unresolved product meaning, exceptional machinery, source
  mutation, unsafe grant broadening, or unapproved external effect. Routine
  implementation decisions remain with the root within this accepted design.
- Next: activate Task 26 on the accepted integrated baseline. All executable
  gates, finding rechecks and exact-tree integration passed.

| Phase | Boundary                     | Milestones completed at boundary                   |
| ----- | ---------------------------- | -------------------------------------------------- |
| 1/5   | Preflight and architecture   | M1 architecture and applicability accepted         |
| 2/5   | Contract and evidence freeze | M2 contracts; M3 qualified Red                     |
| 3/5   | Implementation               | M4 mapped leaf inventory; M5 permission lifecycle  |
| 4/5   | Verification and correction  | M6 focused integration; M7 review and correction   |
| 5/5   | Acceptance                   | M8 full gates, final freeze and accepted candidate |

Root personally read all three current C# directives. SHA-256 fingerprints:
`_csharp.md`: `31045ebcb02d5bfeee8ba9f3112d307b1f72a2186cda618fbf7d22e7d1d90b53`;
`design.md`: `76aa8fc7aaaa79d9535998f5557150f3754e3d80520a7a06864b61659373c1a9`;
`style.md`: `c3fa9d31575e77fedb103ca397f0ccf10ab7236e6edbef1658c7fe36138457cb`.

## M4 And M5 Green Implementation

Root implemented recursive selected-root inventory, mapped destination and
ancestor admission, portable ownership, opaque external content, Library-only
directory grants, exact retired-leaf grants, explicit source rebinding, and
permission-aware Attach, Sync, Detach and explicit recovery. Permission writes
join the complete recovery preparation, are verified before directory/link
application, and retain their actual receipt after later failure. The Library
record is published last. The source guard includes every registered source.

The corrected connected run in `artifacts/task25-m5/corrected-connected/`
passed 736 Unit, 422 Integration and 33 public cases with zero failures/skips.
The public selection remains exactly fifteen Library and eighteen Extension
journeys. Source and executable hashes stayed fixed across that run. Its
Release build had zero warnings/errors. Earlier failures and their receipts
remain in `artifacts/task25-m5/`; they are not acceptance evidence.

The connected evidence exposed a missing-parent preflight mismatch: an
explicitly planned real directory must be admissible before creation, while
actual link application still requires ordinary parents. Library-local
preflight now permits only declared missing parents, checks every ancestor,
and leaves strict effect-time validation unchanged. New direct Integration
cases cover declared, undeclared and redirected parents. Two older fixtures
were corrected to remove the selected source itself rather than its obsolete
mandatory `.agents` child. New mutation journeys gained exact-output teardown;
this does not activate the deferred general environment-cleanup idea.

Two internal callable corrections preserve the accepted public meaning:
completion receives the actual workspace so nested external paths remain
workspace-relative, and plan comparison consumes two producer-owned immutable
effect views rather than eight unpacked values. Permission revalidation and
publication are static operations because they do not own prompt state.
The temporary untracked publication helper was folded into its owning
permission operation before freeze; no forwarding or compatibility type remains.
The new shared values stay in their nearest Library-owned Models scopes.

## M6 Focused Verification And Review Freeze

M6 passed on the current Green source. The decisive renewed run in
`artifacts/task25-m6/renewed/` passed 747 Unit, 427 Integration and 33 public
cases: 1,207 tests, zero failures and zero skips. The Release build had zero
warnings/errors. Source and executable hashes remained fixed across the run.
The exact source inventory and per-tier discovery, runtime and execution
receipts are retained beside the logs. The Green runner now fails when any
selected case fails or skips rather than inheriting Red's permissive exit.

Informational formatting passed for the full Task selection in
`artifacts/task25-m6/final/format.json` and for every subsequently changed C#
file in `artifacts/task25-m6/renewed/format.json`, both with zero diagnostics.
The final renewal also verified the complete focused source freeze unchanged.
Earlier formatter diagnostics and the failed intermediate compiler/test runs
remain preserved; they are not acceptance receipts.

The rejection regressions required an explicit failure fact for an unknown
parent observation. The complete changed-file static audit also found and
removed thirteen pre-existing null suppressions in Doctor presentation and
Library fixtures; guards and asserted fixture prerequisites now establish
non-null state. One intermediate fixture edit placed a local declaration in
the wrong method; compilation caught it and the corrected source passed the
renewed run. Current whole-Task static scanning reports no prohibited patterns,
no lines above 200 characters, no protected build/package/JavaScript changes,
and all 24 unrelated dirty-file bytes and modes preserved.

Current recovery permission checking uses the exact selected bundle target
for a non-record entry even if it is no longer listed in the current Library
record. A record restoration checks the selected Library's mapped leaves.
Four source-absent recovery cases prove current grant admission with and
without current path membership, without restoring permission or source bytes.

The immutable M6 candidate and all changed/new path hashes are recorded in
`artifacts/task25-m6/freeze-receipt.json` after commit. This is the fresh-review
input, not full M8 acceptance. The subsequent review and correction state is
recorded below. No Task 25 integration, global refresh or publication occurred.

## M7 Review And Grouped Correction

Fresh holistic review T25-R1 inspected immutable candidate
`7d0df8d9bf673cef25bf59a9c1a01b449767989c`, tree
`03341fc15c658781728f0352653845d805f3dea0`, against the accepted Task baseline.
The reviewer independently matched all 201 changed-source hashes, including
29 additions and ten formerly untracked paths, and the focused 747/427/33
receipts. The verdict was `CHANGES_REQUIRED`. Root inspected the affected
consumers and accepted all four findings within existing product meaning:

| Finding      | Boundary and consequence                                                                                                                                                                       | Required correction and evidence                                                                                                                                                                  |
| ------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| T25-R1-1, P2 | Doctor and Status associate mappings by source-relative suffix and assume source-root uniqueness. Valid multiple registrations can throw or misattribute counts; remapped ownership is missed. | Use centrally derived destination identity and non-unique source association. Cover multiple Libraries, repeated source roots and remapped ownership.                                             |
| T25-R1-2, P1 | Explicit recovery checks grants but omits current registered source-tree exclusion. A retained target can become another Library's source after bundle creation.                               | Exclude every selected recovery target from current recorded source roots before application and under lease, without source enumeration. Exercise real Repair after a later source registration. |
| T25-R1-3, P2 | Structural preview omits record and permission effects from source exclusion. An empty `.agents` source can appear admissible until the late application guard rejects it.                     | Include all proposed mutation targets before a complete preview or permission question. Cover empty and externally mapped `.agents` sources and preserve the late guard.                          |
| T25-R1-4, P2 | Permission questions show paths and scopes without the per-path planned effect required by the shared question contract.                                                                       | Carry typed plan effects into disclosure and cover mixed Sync creation, retained membership and retirement.                                                                                       |

The review invalidates the named M4/M5 implementation qualifications and their
M6 coverage, not the accepted product direction. Task progress remains phase
4/5, milestone 6/8 while correction is active. No new user decision is needed.
T25-C1 was one grouped pass owned by `/root/library_review_corrections`, assuming
`.apm/agents/brilliant-implementer.agent.md`, explicitly invoked as
`gpt-6-astra` / `high`. Root retains Task/control prose and final acceptance.
The child owned affected production, regression evidence and focused runtime
mutation. It returned ownership after verification; execution remains serialized. Its receipts belong in
`artifacts/task25-c1/`. The existing reviewer will recheck affected findings
against a newly frozen candidate; no second fresh review is commissioned.

The completed pre-correction full managed run in `artifacts/task25-m8/`
passed 2,803 Unit, 1,545 Integration and 188 public cases with zero failures or
skips. Its Release build had zero warnings/errors and whole-Task informational
formatting reported zero diagnostics. All five Library and six Extension
commands retain exactly three public journeys each. These are baseline
receipts, not evidence that the four missing scenarios work. Full gates must
be renewed on the corrected source; Native AOT has not started for Task 25.

Reviewer `/root/library_projection_review` assumed
`.apm/agents/reviewer.agent.md`, explicitly invoked as `gpt-6-astra` / `high`,
and personally read the complete C# directive trio. Its reported fingerprints
match the root fingerprints above. This review establishes concrete findings,
not comparative model quality or cost against a matched Sol implementation.

### T25-C1 Verified Correction

The grouped correction changes twenty tracked C# files: eleven production and
nine tests. There are no newly untracked C# paths. Root independently matched
the returned source hashes and verified that its six paused Markdown files
remained unchanged during the child run. Canonical correction accounting is
`artifacts/task25-c1/verified-candidate-renewed.json`.

Doctor and Status associate observations through complete centrally derived
mapping identity. The producer observes each distinct source root and complete
mapping once, retaining separate destination observations. This also closes
foreign attribution from newly eligible files and duplicate mappings where
registrations share source/destination roots but own disjoint registered leaves.
Dangling lookup uses the same complete identity. Consumer ownership matching
uses mapped destinations.

Recovery now rereads current Library records, checks the selected target against
current registered and selected recorded source roots, and rejects unsafe record
observation without enumerating source content. Held-lease admission precedes
downstream diagnosis/revalidation. Structural Library admission includes record
and permission targets before preview or questioning. Permission requests carry
required typed create/retain/remove effects; questions disclose each missing
path's actual planned link effect. Public JSON shapes and existing technical
contract meaning remain unchanged.

Initial Red reported four Unit and seven Integration failures. R1, R3 and R4
were direct reproductions; strict record ordering limited the initial R2
fixture's qualification. A later bounded run against the exact baseline recovery
reader qualified four valid source-admission failures among five failures in
37 Integration cases. The fifth was a fixture setup failure and is excluded
from behavioral evidence. Both reader variants and exact byte reinstatement
are pinned in `artifacts/task25-c1/recovery-red/receipt.json`. Corrected fixtures
now pass. Initial compiler and fixture failures remain retained as failed
attempts, not acceptance evidence.

Final inspection widened R1 coverage within the same pass: three Unit failures
and one Integration failure demonstrated foreign attribution, ambiguous dangling
lookup and duplicate mappings. These qualified failures are retained under
`artifacts/task25-c1/mapping-red/`. The follow-up used existing mapping values
and a local set, without new services or generic execution machinery.

The decisive `artifacts/task25-c1/final-renewed/` run passed 762 Unit, 441
Integration and 33 public cases with zero failures/skips. The Release build had
zero warnings/errors and informational formatting had zero diagnostics. Source
and runtime hashes remained fixed. Whitespace, 200-column, and added-code
prohibited-pattern checks passed. The earlier 759/440/33 `final/` run remains
preserved but predates the final four attribution regressions. Full M8 and the
existing reviewer's affected-finding recheck remain pending.

The correction author personally read the complete C# trio and reported the
same fingerprints recorded above. Source/runtime ownership returned to root;
no child process remains active. No staging, commit or integration was performed
by the child.

### Remaining Portable Ownership Comparison

The affected-finding recheck on `a7c410cb` closed T25-R1-2, T25-R1-3 and
T25-R1-4. It confirmed complete mapping attribution and distinct observations
but retained T25-R1-1 for raw ordinal ownership-key comparison: mapped
`docs/README.md` and a lifecycle claim for `docs/readme.md` share portable
identity and must collide. Root accepted this bounded P2 gap and revised the
internal correction budget for T25-C2, limited to the existing Doctor consumer
and its focused ownership test. Both sides will use the existing portable-key
capability; original presentation facts remain unchanged.

The first corrected M8 run passed whole-Task formatting with zero diagnostics
and a warning-free Release build, then root stopped its exact owned pipeline
before acceptance. Receipts and the interruption reason remain under
`artifacts/task25-m8-corrected/`. This run does not establish final acceptance.
T25-C1's immutable source and all earlier evidence remain preserved. Phase and
milestone remain 4/5 and 6/8 until the final finding closes.

T25-C2 extended the existing ownership theory with two case-alias assertions.
Both failed at the missing collision while all sixteen existing cases passed.
The correction normalizes both registration and claim keys with the existing
`PortableWorkspacePath.CreatePortableKey`, uses one dictionary lookup per claim,
and keeps the original Library registration in the finding. All eighteen cases
then passed with zero failures/skips after a warning-free Unit-project Release
build. Exact source/runtime hashes stayed fixed; receipts are retained in
`artifacts/task25-c2/`. Informational formatting passed with zero diagnostics.
The same-reviewer ownership recheck remains before acceptance. Root personally read the complete C# directives and
recomputed the unchanged fingerprints recorded above before this authoring.

### M7 Complete And M8 Managed Gate

The existing reviewer closed the final ownership gap at immutable `4e675424`,
tree `d8e93b06649aaf04b2ab20d26bd0b34de35c1296`. T25-R1-1 through T25-R1-4
are closed and M7 is `PASS`. The same review context checked only the affected
boundary, with unchanged personally read C# fingerprints; no second fresh
holistic review was commissioned.

The final complete managed gate in `artifacts/task25-m8-final/` passed 2,820
Unit, 1,559 Integration and 188 public cases, zero failures/skips. Whole-Task
informational formatting reported zero diagnostics and Release build reported
zero warnings/errors. Exactly fifteen Library and eighteen Extension public
journeys remain. The source and runtime manifests stayed fixed across the run.
Toolchain identity is retained alongside the source, discovery and execution
receipts. This is M8 managed qualification, not completed native acceptance.

Task progress advances to phase 5/5, milestone 7/8. Subsequent state-only prose
updates do not invalidate the verified executable source. Supported linux-x64
native publication/execution, final source accounting and acceptance remain.

## M8 Final Acceptance

Task 25 is accepted at phase 5/5, milestone 8/8. The final implementation
candidate is `4e67542458903449ffabf7e2b47ec82dd028db84`. Native execution used
`af45136e7194d40fd19142759161b330e28ec5ab`, tree
`36abf3c349f62af9571da822c26d281633cdb5b3`; its five changed Markdown files
are the only overlay on the managed candidate. All other source bytes and
modes match. The final acceptance commit adds only closeout prose/navigation.

| Gate                                     | Passed | Failed / skipped |
| ---------------------------------------- | -----: | ---------------- |
| Complete managed Unit                    |   2820 | 0 / 0            |
| Complete managed Integration             |   1559 | 0 / 0            |
| Complete managed public                  |    188 | 0 / 0            |
| linux-x64 Native AOT Integration         |   1559 | 0 / 0            |
| linux-x64 Native AOT public              |    188 | 0 / 0            |
| Managed public runner against native CLI |    188 | 0 / 0            |

These 6,502 executions retain exactly three public journeys for each of five
Library and six Extension commands. Every discovered type/method executed;
nine existing Find theories expand from one discovery entry to multiple rows,
accounting for 22 additional Unit executions. All completed rows passed.
Informational formatting has zero diagnostics. The Release build has zero
warnings/errors. All three native publications succeeded; artifact inspection
confirmed ELF x86-64 executables and root version/help smokes passed using this
worktree's `OpenForge.Cli` executable. SDK identity is `10.0.111`.

Canonical receipts are `artifacts/task25-m8-final/runtime-acceptance.json`,
`acceptance.json`, the source/runtime manifests and each gate's discovery and
execution records. Final static evidence checks complete Task paths, prohibited
patterns, 200-column limits, protected paths and preservation of all 24 unrelated
dirty-file bytes/modes. Final source accounting includes formerly untracked
files. T25-R1 and both affected-finding rechecks close all four findings.

Two root smoke-harness interruptions remain recorded separately: an incorrect
published filename, then an overly broad architecture-string count. Both were
corrected without source changes or republishing; exact native hashes were
verified before the successful resumed run. Neither is production acceptance
evidence. The later receipt assembly also corrected an overly strict discovery
count equality after directly reconciling all theory methods and executed rows.

No global installation refresh or external publication occurred. The accepted
scope is mapped leaf symlinks, remembered Library folder/file permissions,
source preservation and recovery. Task 26 begins only on the accepted locally
integrated baseline and preserves these behaviors.

## Accepted Architecture Packet

M1 is complete. Root accepted T25-A1 with the following concrete choices and
one correction to its removal recommendation:

- Record one required `destinationRoot`, using `.` for the workspace root, and
  retain source-relative `paths`. A Library-owned destination-root value admits
  `.` without broadening the contained source-root grammar. One central mapping
  derives final destinations and exact relative link targets. All collision,
  ownership, inspection, operational and recovery consumers use destinations.
- Keep one strict current schema-v1 shape. Add required `directories` alongside
  `paths` only for Library permission entries. No legacy reader or migration.
  Directory matching uses canonical portable segment prefixes and authorizes
  descendant leaves only. Extension grants and wire results stay exact-file-only.
- Keep concrete required/missing leaves in results and add Library-local proposed
  and approved grant scopes with source identity. Live uncovered leaves propose
  their immediate parent; deduplicate and remove covered child proposals. Root
  leaves receive exact grants. Prompts state coverage of future descendants.
- Source rebinding requires explicit disclosure of the old and new roots and
  replacement of that Library's old grants with only the newly approved scopes.
  Never reuse previous-source grants implicitly. Revalidate the exact permission
  observation under the lease before effects.
- Preserve accepted revocation behavior for selected external paths, including
  retirement, Detach and explicit recovery application. The inspection's proposal
  to exempt removals is not accepted. Retirement-only and Detach requests may
  restore missing exact grants rather than request unnecessary folder scopes.
  Source-independent removal uses recorded source identity, without source bytes.
- Permit destination-root ancestry of contained sources when `--to` is `.`.
  Reject actual destination leaves and mutation targets inside any registered or
  selected source tree. Check both original source controls and final destination
  protection; a remap cannot bypass either. Inspect every real destination parent,
  including first-level directories. Root-leaf mappings must work explicitly.
- Place Library permission selection/interaction below
  `Commands/Library/Shared/Permissions`; reuse neutral Framework permissions,
  mutation and interaction foundations without importing Extension-private policy.
  Prepare one complete bundle, verify permission writes before directories/links,
  and publish the Library record last. Retain truthful partial effects.
- Extend direct consumers: Extension Library ownership checks; Status/Doctor and
  Library observations/results; generated navigation using only mapped `.agents`
  leaves; and exact recovery attribution. Exclude the permission control entry
  from automatic Library repair attribution. Explicit recovery must check current
  grants without restoring or widening them.

The existing BCL and typed foundations cover this design. No exceptional
machinery is needed. The architecture specialist independently read the current
C# directives and returned the same three fingerprints as root.

## M2 Semantic And Callable Freeze

M2 is complete on the Task branch. The continuous implementation owner froze
current Library and shared-permission contracts, their technical designs, and
necessary Status/Doctor/Repair and architecture descriptions. Callable C# now
requires recorded destination roots, mapped source/destination identities,
Library-only directory grants, explicit prompt capability, scope-visible
permission results and recovery admission. Source observations no longer carry
mandatory `.agents`-child or whole-root-disjointness properties.

Pure identity, mapping, record and schema data construction remains usable for
independent Red fixtures. Twenty explicit Task 25 `NotImplementedException`
boundaries mark the pending source/inventory observation, mapped navigation,
permission policy/interaction/publication, mutation orchestration and recovery
permission behavior. This is compiler-green Gray, not implemented or tested
Library behavior. Existing private implementation bodies remain behind those
boundaries for the next phase; no compatibility overload or old wire default
was introduced. Extension content and exact-file wire behavior are unchanged.

The final Release solution build completed with zero warnings and zero errors.
Informational `dotnet format --verify-no-changes` covered all 126 changed/new C#
paths: the complete 126-path receipt plus a one-file Inspect forwarding renewal
have zero diagnostics, zero changed bytes and exact final hash coverage. Git
whitespace, changed C# line-length, prohibited-pattern and protected-path checks
passed. No Unit, Integration, EndToEnd or Native AOT execution occurred in M2.
The existing public journey files are unchanged.

Evidence is retained under ignored `artifacts/task25-m2/`: `build-final.log`,
`build-final-receipt.json`, `format-final-receipt.json`,
`format-inspect-renewal-receipt.json`, `format-coverage.json`, `static-audit.json`,
`test-compile-adaptation.diff` and the complete source-accounting manifest.
The author personally reread the complete current C# trio and verified the same
three SHA-256 fingerprints recorded above.

Only four obsolete assertion expectations were removed from
`LibrarySourceRootReaderIntegrationTests` as the authorized compile adaptation:
`ReadsContainedOrdinarySource` no longer compares `PhysicalAgentsDirectory` to
`agents` or asserts `PhysicallyDisjoint`; `RejectsMissingAndNonordinaryBoundaries`
and `ReportsInaccessibleSource` no longer assert null `PhysicalAgentsDirectory`.
Other test edits supply required members or call the new explicit signatures;
existing behavior assertions were not weakened. M3 must replace obsolete
mandatory-child and `.agents`-only meaning with positive root/no-child coverage,
use literal independent mapping/link/record oracles, and distinguish target-stage
Red from prerequisite failures. Root inspects this freeze before M3 resumes.

## M2 Forward Correction And Writing Review Disposition

Root accepted T25-WR1 against the immutable M2 freeze. WR1-001 removes stale
Sync mandatory-child/disjointness and external-destination conformance wording;
it now requires ordinary ancestry, per-effect source exclusion and contained
workspace destinations. WR1-002 makes Detach's mapping boundary removal-only,
with existing ordinary parents, missing-parent blocking and recorded grammar
validation independent of source content. WR1-003 consistently derives raw link
identity from both roots and the source-relative suffix, measured from the
actual destination parent. All three findings are corrected; WR1 is complete
and no second writing review is commissioned.

The small C# correction reuses the existing typed shared permission vocabulary
for `LibraryPermissionView.NotEvaluated`, without changing its API or result.
This forward correction preserves the M2 commit and the root's review-budget
update. Evidence is in `artifacts/task25-m2/correction/`. M3 proceeds after its
focused informational format and post-format Release build have passed.

## Red Continuation Boundary

On 2026-09-09 the implementation session stopped during M3 authoring. Root
confirmed the runtime failure and absence of active build/test processes before
taking over sequentially. The corrected M2 commit remains `eb3cc6dd`.
Thirty modified test paths and the new Library permission test directory are
preserved, with a byte inventory in `artifacts/task25-m3/interrupted-draft.json`.
These are uncompiled drafts, not qualified Red. No Green runtime bodies changed.
Root will inspect the draft, finish the focused evidence, and freeze it before
Green. The prior author remains stopped; mutable ownership is now with root.

## M3 Qualified Red

M3 is complete. Root continued the preserved draft, corrected obsolete record
member expectations and mandatory-child fixtures, and froze 45 changed C# test
paths, including seven formerly untracked files. Production and composition
bytes remain unchanged from the corrected M2 freeze. The actual candidate and
complete source inventory are in `artifacts/task25-m3/freeze-receipt.json` and
`source-accounting.json`.

Final focused results are 675 passed / 47 failed Unit, 252 passed / 170 failed
Integration, and 20 passed / 13 failed public journeys, with no skips. All
fifteen Library and eighteen Extension journeys are independently discovered,
three per command. All eighteen Extension journeys pass. Of 230 failures,
214 reach explicit Gray placeholders, two reach missing-policy assertions
(portable mapped-leaf ownership and permission-file recovery exclusion), and
fourteen consumer assertions remain blocked by earlier unfinished observation
or recovery stages. These fourteen do not qualify their downstream branches.
Green must reach and pass every retained assertion; a placeholder failure does
not prove behavior beyond that boundary.

The expanded fixtures cover root and nested mapping, ordinary source roots,
external opaque content, recursive directory grants, uncovered siblings, exact
root and retired-leaf grants, explicit source rebinding, lease revalidation,
permission retention after link failure, source-independent Detach and recovery,
source protection, and the existing direct consumers. Expected paths, raw links
and wire members use independent literal oracles. Strict schema and pure-data
passes are controls, not evidence for unimplemented mutation behavior.

The final Release build has zero warnings and errors. The full changed-test
informational format receipt and one-file fixture renewal have zero diagnostics
and no source changes. Whitespace, line-length and added-code prohibited-pattern
checks pass. Required source and runtime hashes remain exact across each run.
The initial incomplete run and later fixture corrections remain under
`artifacts/task25-m3/initial-focused/` and `pre-fixture-correction/`. No full
managed or Native AOT acceptance was attempted at this boundary.

## Current User Direction

On 2026-09-08 the user required a consumer allowlist and a CLI question when
an entry is missing, alongside the accepted Task 24 `content/` rename. The
[joint draft](extensions-destination-proposal.md) now includes remembered exact
file grants, prompt-free automation/dry-run, and explicit permission effects.
This supersedes its earlier manual-edit-only recommendation. Task 25 still
requires the accepted Task 23 baseline and frozen Task 24 shared boundaries.

## Pending Source Selection And Recovery Boundary

The consumer must be able to discover or nominate a never-approved external
file before the CLI can ask for permission. Inventorying only `.agents/` and
already-approved paths cannot do that. The root asked the user to choose
explicit external paths or all eligible files in the contained source root;
Task 24 proceeds independently while this Task 25 choice remains open.

Permission changes use the existing bundle as recoverable evidence but are not
automatic Library repairs. Exclude the permission control-file entry from
`LibraryResidualAttributionReader.IsAttributedEntry`; link repair must not
restore or revoke consumer grants. Add focused regression evidence at Task 25.

## Candidate Boundary

This task prepares a proposal for consumer-approved relative-symlink projections to
exact workspace-relative destinations outside `.agents/`. The first Task 23
release remains `.agents/**`-only and is not changed by this queue record.

Any later design must preserve contained sources, complete eligible inventory,
exact relative-link identity, collision refusal, source-preserving Sync and
Detach, recovery, and the separate Library record. Source content cannot widen
consumer permission. Library links never become Extension copies or Extension
lifecycle ownership.

Task 24 may prove a neutral destination-admission primitive covering canonical
relative paths, exact allowlist membership, containment, and no-follow identity.
Task 25 may reuse it only when the meaning is identical; permission and
ownership remain Library-local.

## Implementation Conditions

Activation requires completed Task 23 evidence, an explicit permission carrier
and allowlist grammar, protected-root and alias rules, proportionate real-filesystem
evidence, and maintainer acceptance. Read-only design may overlap Task 24 now,
but shared/public implementation and integration are serialized.

The final functional draft must reach the user for comments before any
implementation. Preparation changes no command, contract, record schema,
source, or accepted destination. Compatibility machinery, remote action, and
publication remain outside this task.

## Functional Draft

The [joint functional proposal](extensions-destination-proposal.md) presents
recommended package vocabulary, consumer permission, concrete copy and link
examples, lifecycle behavior, and the remaining user decision. It is contextual
and must reach the user before implementation.

## Ready Dependency Baseline

Task 24 is accepted and integrated at `2eedaf87`; all original CLI commands,
including the five Library commands, were already integrated before that
Extension enhancement. Shared permission observation, evaluation, publication
and result contracts now have full managed and linux-x64 Native AOT evidence.
The current Task 25 product discussion concerns the content-root model below,
including how external source files are discovered before offering missing
grants. No Library projection implementation has begun.

## Content Root Discussion

On 2026-09-08 the user asked to analyze folder-scoped sharing in both Libraries
and Extensions. The desired journey is to choose a folder, add shared content,
and sync, with multiple Libraries available when useful. A mandatory `content/`
folder is under reconsideration, not accepted or rejected by that question.
The project ledger records the accepted preference for streamlined workflows.

Current recommendation, pending user acceptance:

- A Library uses its explicitly attached source root as its content root.
  Discover all eligible files recursively rather than requiring a second
  manually maintained selection list. No specially named child is mandatory.
  Authors may choose a dedicated subfolder when their repository also contains
  material that should not be projected.
- An Extension retains `content/` as its installable tree. Its sibling
  `extension.json` and package README remain package metadata. The stable ID
  identifies the package but does not distinguish documentation from files
  intended for installation. Removing this boundary would require reserved
  exclusions or another explicit way to identify installable content.
- Both models retain matching source-relative and workspace-relative paths.
  Attaching a narrower folder strips that folder prefix; it does not infer a
  destination such as `.agents/skills`. Arbitrary destination remapping remains
  a separate potential feature, not an implication of folder selection.
- Newly discovered external files require exact consumer grants through the
  accepted question flow. Discovery does not grant permission. Existing
  collision and protected-file rules still apply, and multiple Libraries do
  not acquire competing ownership of the same destination.
- Library links expose edits to existing source files directly. Sync reconciles
  link membership, including additions and retirements; it does not copy
  consumer edits back or publish changes to a shared Git repository.

This recommendation supersedes the root's earlier preference for explicit
per-file selection. It does not change accepted contracts, implementation, or
Task 24 completion. A Library containing only external destinations would also
need the current mandatory source `.agents` child requirement removed as part
of the Task 25 contract change.

## Local Integration

Accepted feature `564545cb7e357cd1b2167422c60d8dd4894b3340` was squash-integrated
onto current local develop `de1e1b7a` as
`3b4aba9d1c6b939e867eabda8421eaf216bb3bbf`. Both trees are exactly
`7bdbe449e85c5e15659bd7104506a4e2bd20fa4d`. All 203 changed-path bytes and
modes match the acceptance inventory, including 29 Task additions and all ten
formerly untracked M6 paths. The feature branch and prior evidence remain.
The subsequent integration receipt changes only coordination prose.
