---
open-forge:
  description: Implement explicit repair planning, dry run, application, verification, and recovery
  tags: [Memory, Working, CLI, Task, Repair, Mutation, Recovery, Contextual]
---

# Task 19: Repair

## Task State

- State: Complete at phase 5 of 5, milestone 8 of 8. The Overseer accepted
  the grouped correction and all final evidence, then squash-integrated final
  candidate `de315968` into `develop` at `11e7a5ed`, with the exact same tree
  `b2bd951fde4e3afdad87f64a692597e3d167f609`. All 81 C# files retain the
  fully tested `fe1ae684` source identity; the final candidate changed only
  this Task receipt. Cleanup may refreeze against the integrated baseline.
- Permanent mapping: Task 19 “Repair” in the
  [project control ledger](../../project-control.md).
- Queue relation: Task 18 “Extension Remove” is complete and integrated. Task
  20 “Cleanup” has accepted Red and may begin its baseline refreeze and Green.
- Parent: [Operational Commands](_operations.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/repair/interface.md)
  and [Behavior](../../../../crystallized/documents/cli/contracts/repair/behavior.md).

The exact immutable preparation base is commit
`76e8e5f1f58e60a9de159318d10e7b3e9f8fc9c9`, tree
`ed698b992b68f76b037b4567d154eadbc013239b`, with direct parent
`5cabb10de31cc522e80e82f1ac2ada49e60929c0`. Relative to its parent, the
candidate changes only non-executable planning and authority records. It
includes Task 18 activation and contains no Task 19 implementation. At that
base, Task 19 was queued/prepared and had no implementation or activation
mutation.

## Expected Outcome

`repair` converts accepted repairable Doctor findings into an explicit
reviewable plan, applies only authorized repairs under mutation safeguards,
verifies results, and preserves exact recovery for incomplete work.

## Outcome And Profile

The command applies only fresh diagnosis-backed, conflict-free,
meaning-preserving local-reference corrections and explicitly selected
contained relinks. It does not promise to eliminate every Doctor finding.

The selected profile is streamlined assured. Its five phases are:

1. Preflight and activation.
2. Gray callable and public shape.
3. Red evidence.
4. One coherent Green and focused verification under one Brilliant
   Implementer.
5. Fresh whole-task review, at most one grouped correction, final acceptance,
   and integration.

The eight milestones are: 1 Preflight and preparation; 2 Gray; 3 Red; 4
coherent Green; 5 focused, public, full managed, and supported `linux-x64`
Native AOT verification; 6 fresh whole-task review (`T19-R1`); 7 grouped
correction or documented no-op (`T19-C1`); and 8 acceptance. There is no
council. Exactly one holistic review ID, `T19-R1`, and one correction ID,
`T19-C1`, are the maxima. Both IDs are consumed: review returned four
material findings, and the single grouped correction is complete. The current owner is Task
Mastermind Ampere Repair. Hypatia II owned only the accepted Gray implementation
root. Emmy II and Faraday II owned the accepted disjoint Red boundaries.
Brilliant Implementer Curie Repair Green owns coherent Green, focused
verification, and the later grouped correction pass if `T19-C1` is required.

### Astra Restart

On 2026-09-07, the new Task Mastermind personally verified branch
`codex/repair-implementation`, HEAD `eae8eb366e8ad50d8c18cca6a4e08e9d3b6d22bb`,
an empty index, and all 28 preserved dirty paths against the sealed restart
manifest `556c919e52611d85ed88bf36c7cd39dd5cde690fdd23cf2e0518545f005fa54b`.
No former compiler, test, or CLI process remained. The complete restart handoff
and inventory were read from the main worktree before ownership transferred.

The active Task Mastermind is `/root/ampere_repair`, assuming
`task-mastermind.agent.md`. The sole Green author is
`/root/ampere_repair/curie_repair_green`, assuming
`brilliant-implementer.agent.md`. Both use the user-selected GPT-6 Astra with
high reasoning, the maximum Astra reasoning level the user authorized. The
Green author independently read the complete current C#
directive, design, and style sources and reported the same three fingerprints
recorded below. No phase or budget was restarted.

The first prescribed diagnostic command was:

```text
dotnet build src/cli/core/OpenForge.Cli.Core/OpenForge.Cli.Core.csproj -c Release --no-restore -p:OpenForgeSkipDevelopmentPublish=true
```

It exited 0 with zero warnings and errors over the preserved draft. This proved
compilation only. At that boundary, interactive selection and final confirmation,
static Repair composition, focused verification, and full acceptance gates
remained incomplete.
The accepted Doctor reader seam retains its six observations and its operation's
existing catches. Shared Recovery contract changes remain outside this lane.

The initial GPT-6 Astra loading pass emitted oversized historical receipt tables
and truncated several tool results, requiring smaller follow-up reads. This is
an observed context-loading cost, not evidence of model speed or cost differences.
Required complete reads remain complete; other parent continuity uses relevant
authority and current-state sections. A model comparison requires comparable
completed evidence, and none is claimed at this boundary.

### Required Evidence Supplement

`T19-E1` is an authorized evidence-only completion under the existing Repair
Interface and Behavior contracts. The frozen six Integration cases do not cover
the wizard or all application-integrity requirements. The continuous Green
author may add new Repair-local Unit and Integration files for select, skip,
back, cancel, default-No confirmation, interactive dry-run, exact occurrence and
fragment verification, source or target drift during final confirmation, and
handled post-diagnosis cancellation with retained prepared recovery. Preserve a
distinct supplemental Red boundary: author these cases against the current
incomplete implementation and capture their actual selected failures before
implementing the corresponding behavior. Independent already-covered Green work
may continue. The earliest invalidated boundary is incomplete required Red
coverage, not accepted product meaning.

The original twelve Red paths and the three Repair and three Doctor public
journeys remain unchanged. This supplement grants no shared TestSupport,
contract, failure-injection, or architecture change. It neither restarts a
phase nor consumes `T19-R1` or `T19-C1`. A case that cannot be proved through
ordinary accepted real boundaries returns to the Task Mastermind for evidence
disposition.

The Task Mastermind accepted the supplemental Red boundary after personal source
and receipt inspection. `RepairWizardTests` selected, discovered, and executed
nine cases, all failing at the intentional selection or confirmation scaffold.
`RepairCompletionIntegrationTests` selected, discovered, and executed six cases:
default-No and two confirmation-drift cases incorrectly completed, exact
occurrence and required-fragment verification incorrectly succeeded, and
cancellation at the real prepared application-completion stage escaped instead
of retaining known recovery. All fifteen failures were intended, with zero
skips. Both fresh project builds had zero warnings and errors.

The supplemental test SHA-256 values at this freeze are:

- `RepairWizardTests.cs`:
  `6f2d68dd480a2749326cdff3001c4da1a840d9cabccd3486a13bcdeac4009ebb`.
- `RepairCompletionIntegrationTests.cs`:
  `55a5d84d462a89a5d8f931f2fbefa51cb6ddf6a20b7179f484bd73eab53272d0`.

Same-worktree receipts and full logs are under
`artifacts/repair-astra-gates/e1-red-{unit,integration}{,-build}.*`.
Integration retained source identity
`7a351abf9d18ce694211d4d87c78f344f645241f15848c92fcc29b39b31c3093`
through build and execution. Only Integration test additions changed between
the Unit build and its execution; Core, Unit, TestSupport, and configuration
inputs were unchanged. The original twelve Red paths and both three-journey
public sets have no changed tracked bytes. Corresponding Green is authorized.

### Supplemental Fixture Cleanup

`T19-E1-CLEANUP-001` was accepted by the Overseer after the first fresh Green
Integration selection passed eleven of twelve cases. In the remaining
target-drift case, all behavior assertions passed, then disposal rejected the
unowned `moved.md` created by the test's simulated external move. The bounded
correction preserves that move and every assertion. It uses existing fixture
ownership if available, otherwise restores the move in `finally` after the
assertions. The original twelve Red files and both public sets remain frozen.

The stable failing receipt is `green-integration-01`, with unchanged source
identity `68dab4e38f047c76b76d5b06bf860c211859cb0c5d9434dad70fb2edfc0fedb6`
through build and execution. Its exit code was 2, with eleven successes, one
cleanup failure, and zero skips. This correction retains the historical
supplemental Red fingerprint and receipt above; it does not invalidate the
intentional Red failures or consume `T19-R1` or `T19-C1`. Fresh affected
Integration evidence is required after the cleanup correction.

The accepted cleanup wraps the unchanged execution and three assertions in
`try` and restores the moved target from `finally`, because the frozen fixture
does not expose its existing temporary-workspace owner. The new supplemental
Integration SHA-256 is
`4755b83cd8b9ee6f777bf22b09a67ac0a2fd56699ddf875031b9632142a501c1`.
The exact diff is retained at
`artifacts/repair-astra-gates/e1-cleanup-001.diff`; reversing just that diff
reproduces the accepted Red fingerprint. The supplemental Unit file remains
byte-identical.

### Supplemental Formatting Correction

`T19-E1-FORMAT-001` was accepted after severity-info verification identified
four analyzer diagnostics in the frozen supplemental Integration file. The
exact correction uses the effect returned by the existing `Assert.Single`,
replaces two `ToArray` calls with collection expressions, and makes the
state-free confirmation-drift theory method static. Every action, assertion,
and cleanup is preserved. The original Red and public test bytes are unchanged.

The pre-format SHA-256 was
`4755b83cd8b9ee6f777bf22b09a67ac0a2fd56699ddf875031b9632142a501c1`;
the current accepted SHA-256 is
`71871a136ff84658ac7e5c9a37c631898171729b2b89b87a1fb67d4d3be5a56e`.
The literal diff is `artifacts/repair-astra-gates/e1-format-001.diff`.
Fresh affected verification is required. Future supplemental Red freezes
include severity-info formatting before freezing. This mechanical correction
consumes neither `T19-R1` nor `T19-C1`.

### Fresh Boundary Diagnosis Evidence

The final Green contract audit identified that handled pre-effect refusal and
failure also require fresh post-diagnosis. The existing default-No supplement
asserts status and absence of effects, but does not prove observation freshness.
The Overseer required one separate lower-tier supplemental oracle, preserving
the original and `T19-E1` tests and both three-journey public sets. Its ordinary
confirmation reader changes source bytes during final No; the result must be
Interrupted and report the new missing-reference occurrence in post-diagnosis.

A three-file command-private correction had been authored but not built when
the stronger Red sequencing instruction arrived. The correction and prior
source identities are preserved before reproducing the pre-correction failure.
The earlier provisional final source fingerprint
`8e48e1e29ae7144155abd349550455e8badffdf148af9e889f6e2e2d8a56b3d0`
and its passing focused Unit evidence are diagnostic history only. At that boundary, Green had
not been accepted and the milestone remained three of eight. This evidence
completion does not consume `T19-R1` or `T19-C1`.

The Overseer authorized the exact three-file controlled literal edit cycle for
`T19-E2`: preserve both variants and hashes under ignored artifacts, freeze all
other source and test inputs, run a fresh pre-correction build and selected
Red failure, then reapply corrected bytes from `finally` and prove byte
equality with no unrelated drift. This uses no Git reset, restore, clean,
stash, or rebase, and no second worktree. Provisional inputs are never staged,
committed, or used for acceptance.

The Task Mastermind accepted `T19-E2` after personal test and receipt
inspection. The fresh Integration build had zero warnings and errors. The
exact class selection discovered and executed one case, with one intended
failure and zero skips: after the Interrupted and unchanged-source assertions
passed, post-diagnosis was `NotRequested` instead of `Complete`. The stable Red
source identity was
`b368235fb30ebc858df8ef09585eba4bf481af203a3520a63dc6c8da34081d57`.
The frozen new `RepairBoundaryDiagnosisIntegrationTests.cs` SHA-256 is
`5969c46974b372417a33c884ed1e3312a6956bb12cc971c302f872be9d5f1f81`.

Receipts `e2-red-integration-build`, `e2-red-integration`, and
`e2-red-cycle.json` retain the variant and restoration evidence. Only the three
authorized compiler inputs differed during Red. All 2,643 Git-visible inputs
before and after the cycle were byte-identical, and the corrected three files
matched their preserved bytes. The corresponding Green is authorized. The
supplement increases focused Repair Integration from twelve to thirteen cases;
public counts remain unchanged.

A provisional Doctor Unit selection using `Feature=doctor` correctly failed
the minimum-count policy with zero tests. The established source trait is
`Feature=doctor-command`; final Doctor Unit and Integration receipts must use
that exact selection. The zero-selection receipt is retained and proves no
behavior gate.

### Storage Interruption And Resume

The user paused CLI work for disk pressure, then authorized deletion of build
artifacts and explicitly resumed the original tasks. Suite-end temporary-test
cleanup was deferred as an idea. Root removed selected `bin`, `obj`, and
`publish` directories while preserving task receipts, recovery, and source.
The Task Mastermind personally verified unchanged HEAD `eae8eb36`, empty index,
no remaining compiler or CLI process, and source identity
`c964ae943ff00a1337a85370312e92a8fb967a300282975ad60bda84874ab50e`.

The same GPT-6 Astra/high Green author resumed; no phase or budget restarted.
Historical Red receipts remain valid, but deleted binaries and restore assets
invalidate any use of `--no-build` or `--no-restore` until fresh restoration and
build. Restoration prefers the existing local NuGet cache, retains
`NuGetAudit=true` with mode `all`, and must report missing capability before
broadening. Only the current lane and its required gates are rebuilt.

After a successful root experiment, the Overseer explicitly selected locked
restoration with the existing package-cache directory supplied by `--source`.
No configuration, dependency version, or audit property changes are authorized.
This establishes cached dependency restoration and does not claim a new online
vulnerability audit.

During restart, `dotnet test --help` unexpectedly triggered default-source
solution restoration while loading dynamic runner help. The author stopped
that process and rejected the overlapping restart build as acceptance
evidence. Source, configuration, versions, and audit properties were unchanged.
The authorized local-source restore and fresh build must be re-established;
subsequent test selections explicitly use `--no-build --no-restore`.

### Accepted Green

The Task Mastermind accepted coherent Green at source identity
`c964ae943ff00a1337a85370312e92a8fb967a300282975ad60bda84874ab50e`.
The exact 47-path candidate contains one Task record, the accepted three-file
Doctor seam, 37 Repair-local production paths, three static root composition
paths, and three supplemental test files. No protected shared contract or
original Red path changed. The Doctor reader still performs the same six
reads, with existing catches retained only in `DoctorOperation`.

Repair now forms the finite diagnosis-backed catalogue and explicit plan,
preserves wizard intent and final default-No confirmation, revalidates source
and target facts, prepares external recovery before effects, applies through
accepted mutation primitives, verifies the addressed occurrence and fragment,
and reports fresh post-diagnosis and truthful recovery disposition. Static root
composition exposes the existing public syntax.

Fresh evidence after the storage interruption passed all six selections:

| Command | Unit | Integration | Published |
| ------- | ---: | ----------: | --------: |
| Repair  |   60 |          13 |         3 |
| Doctor  |   21 |           3 |         3 |

All 103 tests passed with zero failures or skips. The three fresh Release
project builds had zero warnings and errors. Four no-restore, severity-info
format verifications covered exactly all 46 changed C# files. Static,
protected-path, frozen-test, line-length, and diff checks passed. All sixteen
frozen test-file fingerprints match their accepted boundaries, including the
two explicitly recorded E1 mechanical corrections. The published inventory
remains exactly three Repair and three Doctor journeys.

`artifacts/repair-astra-gates/green-ready-packet.json` records exact commands,
source identities, logs, test counts, and ten fresh managed artifact identities.
The Task Mastermind independently checked their hashes, complete formatting
coverage, and absence of active compiler or test processes. The artifact
manifest SHA-256 is
`4940b79bd0922d1c0eb693a84bc923e14c8bc0a37688594202d58086468e2eb3`.
Only the receipts after `restart-cache-restore-02` are claimed for this final
focused boundary; the discarded restart build remains explicitly excluded.

GPT-6 Astra/high completed the two recorded supplemental Red boundaries and
this focused Green boundary. The late evidence completion, mechanical test
corrections, and discarded restart build are observable execution overhead
recorded above. These observations do not establish a model speed or cost
ranking. Full managed and Native AOT gates, `T19-R1`, and `T19-C1` remain open;
no acceptance or integration is claimed at this boundary.

### Current Baseline And Conformance

Green was committed as `fd0ce55b98bd51b97d381232405a9eb34433de6d`, tree
`e2f4ab16bf1c0ced831262cee2836a87676b67a6`. After an empty-index and full-input
refreeze, the Overseer authorized merging accepted develop
`06e0b5198d1971aa355c46ba359ca09920a3db8b`. The merge commit is
`9e66e9f329e5a12a3f88241b2f7f572fd048fd57`, tree
`3b9255cf3cd5c2654313a56771b93cd43bc27f58`.

The only conflict was this Task record's current state and owner versus stale
incoming text. It was resolved by preserving the exact Green Task blob; every
other staged blob matched accepted develop. No production or test conflict
occurred. The only added CLI source delta outside Task 19 was the accepted
Context help-example literal. All sixteen frozen test files and the three C#
Directive fingerprints remained exact. Whole-task review compares against the
accepted develop commit above.

The broader whole-task static check found `invalid!` in the inherited Gray
binder, outside the earlier Green-only changed-file selection.
`T19-STATIC-001` closes that C# conformance gap with a truthful
`[NotNullWhen(false)]` annotation on the private helper and removal of the
null-forgiving operator. All false branches already produce a non-null finding;
no behavior, public callable shape, or frozen test changed. Its new file
SHA-256 is
`96b70d0c9e445066fbd2d0916f63b51feb31b379883c0d774126715971a06413`.

The fresh Unit build had zero warnings and errors, Repair Unit passed 60/60
with zero skips, and scoped severity-info formatting and whole-task static
checks passed. Receipts are recorded in
`artifacts/repair-astra-gates/static-correction-ready.json`. The resulting
source identity is
`1c4e774498e0e9921894bc23f5a0db2b0b365ff9c9acfc674f5bece63dc66138`.
The earliest affected evidence was Gray C# conformance. At that boundary,
`T19-R1` and `T19-C1` remained unconsumed; full gates followed the clean commit
and refreeze.

### Accepted Full Verification

The full evidence ran against immutable commit
`f33f5ebbb3327a1155d812cb1de5c650f905db78`, tree
`ecadc319c6c1086b06f2a0768b35598950f00bcd`, and source manifest
`1c4e774498e0e9921894bc23f5a0db2b0b365ff9c9acfc674f5bece63dc66138`.
The accepted comparison baseline is `06e0b5198d1971aa355c46ba359ca09920a3db8b`.
All twelve gates passed; the owner independently checked exact commands,
source freshness, counts, log hashes, all thirteen artifact identities, and a
clean index/worktree. No gate reported warnings, failed tests, or skipped tests.

| Execution                               | Passed tests |
| --------------------------------------- | -----------: |
| Managed Unit                            |         1969 |
| Managed Integration                     |         1039 |
| Managed EndToEnd                        |          190 |
| Native Integration                      |         1039 |
| Native EndToEnd                         |          190 |
| Managed EndToEnd against the native CLI |          190 |

The sequence also passed the cache-only locked restore, Release solution
build, three supported `linux-x64` Native AOT publishes, and the managed
EndToEnd rebuild targeting the native CLI. NuGet audit settings were preserved;
local-source restoration is not evidence of a new online vulnerability audit.
Fresh focused evidence and severity-info formatting preceded this sequence;
the whole-task static check covered inherited Gray as well as Green.

The frozen exact-command capsule is
`artifacts/repair-astra-gates/full-command-capsule.json`, SHA-256
`f5ece0b741ce1a112cb3f0d67a9ad55916c133c516f05ba7ddf9756aeda91453`.
The full summary is `artifacts/repair-astra-gates/full-green-summary.json`,
SHA-256 `313e92fd21718fc7aba143df55501c500e8192212de8038c1b5603fb89b7bb7b`.
The native root SHA-256 is
`82202a458ed06aca1e230ab2c0545c4810563938c1bda2d3dca406cb135586de`;
the summary retains the other binary, assembly, marker, and log identities.
All artifacts came from this worktree.

The mechanical execution owner was
`/root/ampere_repair/kelvin_full_gates`, `gpt-5.6-luna` with `max` reasoning,
under the exact mechanical execution exception. The `gpt-6-astra`/`high` Task
Mastermind selected and froze commands and retained evidence interpretation
and milestone acceptance. This division completed the supplied sequence
without source edits; it supports no comparative speed or cost conclusion.

### Whole-Task Review And Grouped Correction

The single independent review `T19-R1` examined the complete accepted-task
delta from `06e0b5198d1971aa355c46ba359ca09920a3db8b` to
`f33f5ebbb3327a1155d812cb1de5c650f905db78`, including inherited Gray.
The reviewer `/root/ampere_repair/maxwell_repair_r1` assumed
`reviewer.agent.md` using `gpt-6-astra` with `high` reasoning. It personally
read all three complete current C# Directives and reported the matching
fingerprints below. It independently corroborated the full receipts and
artifact identities, preserved Doctor seam, and static composition, but
returned `CHANGES_REQUIRED` for four material gaps not covered by passing tests.

| Finding        | Accepted correction                                                                                                | Earliest affected boundary                                 |
| -------------- | ------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------- |
| T19-R1-001, P1 | Honor every explicit tuple's expected literal and target before forming its union with automatic selection.        | Missing Red union coverage and Green selection.            |
| T19-R1-002, P2 | Deduplicate independently parsed identical relinks by normalized tuple fields in request and binding paths.        | Gray request normalization.                                |
| T19-R1-003, P2 | Start effects as Planned and derive terminal step outcomes from actual application and verification.               | Original Red planner oracle, then Green lifecycle/results. |
| T19-R1-004, P2 | Preserve actual applied/verified subsets and terminal interruption without manufactured blocking or pending facts. | Gray status formation and Green completion.                |

The Task Mastermind revalidated these findings against current immutable source
and accepted contracts, then released the single `T19-C1` packet to the same
Brilliant Implementer. Its first boundary is evidence-only: new lower-tier
contract cases must reproduce the defects before production changes. The
Overseer authorized `T19-R1-RED`, the exact original
`RepairPlannerTests.cs` assertion change from `Verified` to `Planned`, because
Dry-Run Parity forbids claiming unwritten bytes verified. Preserve its original
hash/history, capture the corrected assertion failing against the reviewed
production, and retain every other original and E1/E2 frozen oracle. Both
public three-journey sets remain unchanged. No public or shared contract changed.

The immutable review inventory is
`artifacts/repair-astra-gates/review-scope.json`; the report is
`artifacts/repair-astra-gates/r1-report.md`, SHA-256
`9f58a99cb1ed1ae2c2eba27952fd1015e787ebee9e14c58feac338076bd06cc4`.
The grouped packet is `artifacts/repair-astra-gates/c1-packet.md`.
This one `gpt-6-astra`/`high` review found four accepted behavior gaps after
passing full gates; it supports an evidence-quality observation, not an
unsupported speed or cost comparison. Acceptance remains open until correction
and fresh invalidated evidence pass. There is no second holistic review budget.

### Accepted C1 Failure Evidence

Before production correction, the owner inspected the full new Unit and
Integration files, exact original assertion diff, all 2,651 frozen inputs,
seven canonical log receipts, and current compiled artifact hashes. Production
remained exact `f33f5ebb`; source manifest with the new evidence was
`abe2bd7414bd8a1782f576dd980c0d3a5430af9400f0d6c0b1de2745b4da396e`.
Both fresh builds had zero warnings/errors; both new test files passed
severity-info formatting before their freeze. The selected runs produced:

| Selected evidence                             | Failed as intended | Passing controls | Skipped |
| --------------------------------------------- | -----------------: | ---------------: | ------: |
| New selection and normalization Unit cases, 9 |                  4 |                5 |       0 |
| Original planner cases, 2                     |                  1 |                1 |       0 |
| New lifecycle Integration cases, 6            |                  5 |                1 |       0 |

The selection failures proved stale/unadmitted explicit authority bypass and
independent duplicate request/binder rejection. Controls preserved a matching
union, admitted nonrecommended target provenance, and contradictory-tuple
refusal. Lifecycle failures proved typed cancellation's Blocked status,
prepared zero/partial effects being overcounted as verified, and coalesced
preview steps claiming verification. Actual target bytes and retained bundle
assertions passed before the prepared-count failures; later terminal-status
and step assertions remained to execute in Green. Successful application and
no-op controls passed.

`T19-R1-RED` changed only the authorized original planner assertion from
`Verified` to `Planned`. Its original SHA-256 remains
`f4588e880b3f1a06990e55be82b1e234cf563bd90a8b5aef6c67def5c348690f`;
the accepted corrected hash is
`a21e1856cdb39e0108c598f06536ddc9cbc01a6481e45fce9f537d630d62b05a`.
The corrected assertion failed against unchanged reviewed production.
The new frozen `RepairC1SelectionTests.cs` hash is
`87c0d37252fce9cef0ef55da586ddb425ad51dfbe52594ad4c35a8266de0ed6f`;
`RepairC1LifecycleIntegrationTests.cs` is
`02972f51077240c169a25fe0c7471a30b4405fa3b356fc6108f6b63278d4c1fd`.
Every other original/E1/E2 oracle and both public three-journey sets stayed exact.
The ordinary seams used real lock acquisition, recovery preparation,
revalidation, application, completion, and complete result formation without a
test hook. Evidence resides in `artifacts/repair-astra-gates/c1-red-ready.json`,
`c1-red-complete.diff`, and `c1-red-inputs.json`. The owner accepted these actual
failures before releasing production correction under the same consumed C1.

### Authorized C1 Conformance Closure

Final severity-info inspection expanded to the complete Task C# delta against
`06e0b519`, including inherited Gray. It exposed inherited diagnostics beyond
the earlier Green-only formatting receipts. The owner kept the same C1 and
obtained bounded conformance dispositions before touching frozen evidence.
No suppression, contrived state, additional review, or behavior redefinition
was introduced.

The stateless `RepairPlanner` became static, and its redundant factory,
components, and revalidator instance plumbing was removed. The Overseer
explicitly authorized the exact caller substitutions in five frozen files,
including the two necessary Planning imports. A separate approved literal
patch addressed nine original-test diagnostics and two public-fixture
diagnostics across seven files. Each `Assert.Single` returned the same member
previously indexed; the UTF-8 sample remained `61 C3 A9 7A`; removed unused
workspace arguments were only variable reads; static helpers and the concrete
snapshot return retained the same values. Every action, assertion, and all
three Repair public journeys remained. Doctor's three public journeys were
unchanged. These are explicit exceptions to the earlier byte freezes, with
complete predecessor histories retained.

The owner reconstructed both accepted patch files from preserved preimages
and current bytes, verified all twelve predecessor/current test hashes and
every other test against the C1 Red freeze, and found no unrelated drift.
`artifacts/repair-astra-gates/c1-conformance-test-hash-chain.json` records the
exact twelve-file history, SHA-256
`ba1d28eaf2a6cede43f16f8aff7ef1c45bb706690e225bbc683c81d6b39ce461`.
The literal patches are `c1-frozen-format-proposal.diff` and
`c1-static-planner-test-proposal.diff`; the owner receipt is
`c1-conformance-owner-check.json`. Earlier Red receipts and fingerprints
remain historical evidence of their exact source variants.

The Overseer also authorized nine private return-type narrowings in
`CliStandaloneComposer.cs`, with required type imports. Owner comparison
proved all builder bodies, registration order, and composition interfaces
unchanged. The proof is `c1-root-conformance-owner-check.json`. Other ordinary
conformance corrections stayed in Repair production. The final static helper
`check_c1_final_static.py` pins the accepted hash-chain identity and predecessor
hashes, preserving the prior static helpers and manifests.

This whole-task inspection caused additional formatting and verification work
under `gpt-6-astra`/`high` ownership. The directly observed issue was incomplete
scope in earlier formatting receipts; it does not establish a comparative
model speed or cost ranking.

### C1 Source Closure

The four accepted findings were corrected together. Explicit tuple validation
now precedes automatic union selection while preserving admitted alternate
targets and all matching origins/provenance. Both request construction and
binding use structural normalized tuple identity. Planned effects begin as
`Planned`; command-owned per-effect facts retain the actual mutation receipt
and fresh exact target/byte observations. Coalesced step outcomes and verified
counts derive from those facts. Unapplied expected bytes receive no verification
credit. Partial application shifts only the actually applied occurrence offsets.
Unavailable verification retains the known receipt subset and stronger failure.
Terminal interruption suppresses only synthetic pending-work checks; genuine
failure, incomplete coverage and residual recovery precedence remain.

Owner revalidation of `T19-R1-004` identified one further preparation-producer
path within that finding: `Cancelled` preparation had retained incomplete
recovery facts but still formed `Application.NotRequested`, yielding `Incomplete`
where accepted Behavior requires `Failed`. A separate Unit oracle first failed
with exactly those statuses against source
`2c820062418817fa7292d4c5b7dd2de46c2133c043522525f3b3bf438b8dc652`.
Its fresh build and pre-freeze severity-info verification were clean; the owner
rehashed all 2,655 frozen inputs and logs, confirmed the unchanged factory
preimage, and accepted the selected one failure with zero skips before the fix.
`RepairC1PreparationCancellationTests.cs` is frozen at
`f6ff491b8d2a08167d068c5fb1b1610f2c724a6fb0672696bd4872048d8ba7f9`.
The exact packet is `artifacts/repair-astra-gates/c1-preparation-red-ready.json`.
Cancelled preparation now forms `Interrupted` application with zero effects;
its unchanged incomplete residual facts correctly produce terminal `Failed`.
This extends the evidence of the same open finding and C1, without another
formal review or a new behavior rule.

A final bounded visual pass covered all 81 Task C# files and initially 92
conditional-expression sites. Three direct nested-conditional violations in
post-verification, validation cancellation, and recovery-deletion finding
formation were replaced with guards and simple local choices. Two formatter
comma artifacts were normalized during the same source boundary. The owner
inspected all changed production, exact approved test/root patches, and final
guards before releasing the final execution freeze. No material source finding
remained. The source-acceptance packet is
`artifacts/repair-astra-gates/c1-owner-source-acceptance.json`, SHA-256
`eb5607f736565465dc4e56e05ea22128c6338b1204a42b0929d4db19c685bd0e`.
Earlier C1 ready packets and provisional final03 receipts remain historical;
final acceptance uses the later fully frozen final04 evidence and fresh full gates.

### Final C1 Focused Verification

The distinct `artifacts/repair-astra-gates/c1-final-04-immutable-ready.json`
packet supersedes the historical `c1-immutable-ready.json` (118 focused cases,
source `8614a…`) for current acceptance. Its SHA-256 is
`cc0eed23c3df8287c63fdbe6f2898ac494bf7fa8795c4af26050fc72e7a75fa3`;
its production/test source manifest is
`a159ad5355098950f325e2ef6b0bf894a4430dd08c245571fa86f9d96fdf0032`.
All 15 primary gates exited zero: five severity-info formatting checks covered
all 81 Task C# files, the protected-scope static check passed, three fresh
builds had zero warnings/errors, and six focused selections passed all 119
cases with zero failures/skips. Repair passed 70 Unit, 19 Integration and three
public cases; Doctor retained 21 Unit, three Integration and three public cases.

The owner personally rehashed all 2,655 frozen inputs and nine compiled
artifacts, checked exact formatting membership and all receipts/logs, and
confirmed no source drift. The input-manifest SHA-256 is
`5aa9fc0686ab91f66279a587bef1dee8cfd4b5070c5b767147dd54a332079f7b`.
`c1-final-04-owner-evidence-check.json` records that closure. This Task-only
receipt update requires a refreshed all-input freeze before exact staging;
source identity remains unchanged. The correction source and focused evidence
are accepted, while milestone 7 awaits fresh complete managed and Native AOT
gates on the clean correction commit. Historical packets remain preserved.

### Accepted C1 Full Verification And Acceptance Recommendation

The coherent correction was committed as
`fe1ae684807d754ae8f0747737567e663359a425`, tree
`f788778a82f31ca7e831579f0e27fb041ff814e3`, after exact staging of 43 paths
from all 2,655 refrozen Git-visible inputs. Six formerly untracked C# files
were explicitly inventoried and staged. The clean candidate includes accepted
baseline `06e0b5198d1971aa355c46ba359ca09920a3db8b` and preserves the final04
source identity `a159ad5355098950f325e2ef6b0bf894a4430dd08c245571fa86f9d96fdf0032`.

The same exact-command worker `/root/ampere_repair/kelvin_full_gates`, actual
`gpt-5.6-luna`/`max`, executed the frozen twelve-command capsule under the
explicit no Open Forge context mechanical exception. The Task Mastermind,
actual `gpt-6-astra`/`high`, retained source acceptance and personally checked
every command/receipt/log, all 2,655 unchanged inputs, thirteen compiled
artifact hashes, the three native-code generation markers and the clean Git
state. This is evidence of that division's completed work, not a comparative
speed or cost claim. No additional formal review or correction cycle occurred.

| Complete corrected evidence         | Passed | Failed | Skipped |
| ----------------------------------- | -----: | -----: | ------: |
| Managed Unit                        |  1,979 |      0 |       0 |
| Managed Integration                 |  1,045 |      0 |       0 |
| Managed EndToEnd                    |    190 |      0 |       0 |
| Native Integration, linux-x64       |  1,045 |      0 |       0 |
| Native EndToEnd, linux-x64          |    190 |      0 |       0 |
| Managed EndToEnd against native CLI |    190 |      0 |       0 |

All twelve gates exited zero without warnings. Cache-only locked restore
retained the existing audit configuration; it proves cached dependency
restoration, not a new online vulnerability audit. Release solution build,
three native publishes, and the managed-on-native build used only this
worktree's artifacts. Final04 supplies the complete-task severity-info/static
and 119-case focused evidence for the identical production/test source.

`artifacts/repair-astra-gates/full-c1-summary.json` has SHA-256
`e2748d3b949a092d7dc99c2a76798517a30a866fbf139e0e2bc49cc29588087b`.
The owner's `full-c1-owner-acceptance.json` has SHA-256
`070e0621d8d684cf2c92538384ea89f28344a6d0c0f476baaa54af3d1e8906d2`
and records no unresolved evidence errors. The exact capsule SHA-256 is
`9149a254b7834e30cb01445aec37eaa31526ada386316c48f8731d431c3b9761`.
The baseline-to-source-candidate manifest is
`c1-baseline-candidate-manifest.json`, SHA-256
`d80a57a8805f6033a656033435523f242d497b3624292497ef449c6e9848978f`:
82 paths comprising 63 production C# files, 18 test C# files and this record,
with baseline/candidate modes and blobs. The final handover refreshes that
manifest for this Task-only closeout without changing tested source.

Milestone 7 is accepted. Each of `T19-R1-001` through `004` is closed by the
bounded correction and its failure-before-fix evidence described above; R1
and C1 remain consumed exactly once. The owner recommends milestone 8 and
local integration of this accepted capability before Task 20 Green. Repair's
Interface/Behavior and shared Recovery contracts are unchanged. The Doctor
three-file seam retains six observations and operation-only catches. Both
public three-journey sets remain, subject only to the expressly recorded
Repair-fixture formatting exception. Historical Red bytes and receipts are
preserved with exact authorized exception chains. No unresolved Task 19
product, source, evidence or integration dependency remains.

The Overseer must record acceptance and the eventual integration identity in
project control, then release Task 20 under its own accepted packet. The later
Task 23 recovery exception and the deferred suite-cleanup idea remain outside
this candidate. No remote publication, package installation or develop
integration occurred in this lane. This final record update changes no
production/test input and does not require repeating the full execution.

## Activation And Accepted Architecture

The accepted authority fingerprints are:

- Repair Interface: `92ea7c6a888149f22a4e473aac492ba042fd249368f16651704f6968239b453b`.
- Repair Behavior: `7a33b333f78a822092fdbe39ef0e9c00c76b18daa4814130e7996021cb08bb89`.
- C# Directive `.agents/directives/csharp/_csharp.md`:
  `31045ebcb02d5bfeee8ba9f3112d307b1f72a2186cda618fbf7d22e7d1d90b53`.
- C# design Directive `.agents/directives/csharp/design.md`:
  `76aa8fc7aaaa79d9535998f5557150f3754e3d80520a7a06864b61659373c1a9`.
- C# style Directive `.agents/directives/csharp/style.md`:
  `c3fa9d31575e77fedb103ca397f0ccf10ab7236e6edbef1658c7fe36138457cb`.

The public syntax is exactly:

```text
open-forge repair [--automatic] [--relink <source-location> <expected-destination> <target-path>]... [--dry-run] [global flags]
```

The first-release catalogue contains only same-target canonical path, case,
and encoding corrections; a unique canonical fragment correction; and an
explicitly selected missing-target relink from bounded Doctor candidates.
Repair never repairs generated navigation, route topology, metadata,
Framework or Extension lifecycle, ownership, recovery artifacts, prose,
external references, or arbitrary links. Automatic selection admits only
safe-exact effects. A guided candidate remains unselected until the wizard or
an exact `--relink` supplies user intent.

Repair consumes fresh producer-owned Doctor and local-reference views,
including canonicalizations and bounded candidates. It reuses source sessions,
exact source locations, snapshots, destination resolution,
`PlannedFileChange.Replace`, preflight and revalidation,
`WorkspaceLockManager`, `RecoveryBundleStore`, `FileChangeApplier`, native
interaction, and command-local source-generated JSON. All Repair semantics stay
local: request normalization, selection and catalogue formation, exact relink
resolution, effect coalescing and conflict handling, plan formation, byte
edits, recovery mapping, application orchestration, post-diagnosis, result
formation, and rendering.

Do not import another command's private `Shared/**` implementation or add a
generic repair engine, dependency injection, a service locator, a runtime
registry, reflection, JavaScript/MJS/CJS, or compatibility machinery. Prohibit
unauthorized destructive repository, worktree, or source operations and
out-of-plan destructive effects. Accepted in-plan `Replace` effects remain
allowed when their recovery and safety gates pass. Remote, release, and
publication actions remain prohibited. Every planned `Replace` has external
recovery. A Doctor canonicalization proposal marked `NoPersistentState` does
not waive Repair's recovery requirement.

The provisional `codex/repair-provisional` commits `e6b906fd`, `a90a8007`, and
`0521b278` are read-only historical evidence from stale base `0d269b7a`.
They contain useful finite-catalogue and deduplication ideas, but duplicate
`FileStateSnapshot` as `RepairFileState` and omit the required diagnosis,
recovery, result, composition, public, and Native AOT boundaries. They are not
transplant units.

## Architecture

- Doctor findings remain observation input. `RepairPlanner` maps only recognized
  repair codes and complete provenance to command-local `RepairPlan` steps.
- Each repair step names target, expected state, intended effect, verification,
  dependency, and recovery boundary.
- Reuse shared mutation primitives and producer-owned repair capabilities. Do not
  duplicate install, update, index, or Extension behavior inside Repair.
- Dry run forms the complete plan and stops before lock/effects.
- Keep request normalization, catalogue and selection policy, relink resolution,
  coalescing and conflicts, plan/effect formation, recovery attribution,
  application orchestration, post-diagnosis, result, and rendering under Repair.
- Consume fresh producer-owned Doctor/local-reference views and reuse the
  accepted source-session, snapshot, destination, preflight, lock, recovery,
  applier, interaction, and source-generated JSON capabilities.

## Accepted Gray Freeze

The cumulative Gray boundary is accepted at immutable tip commit
`140920d3116fe0744bac933c77041f22107c8ce7`, tree
`4060c945011b41f17822baf1c4f2aeb535069112`. Its immutable sequence is
initial Gray `2f660f52`, followed by corrections `2597c413`, `9ebbc064`,
`4d25c287`, and `140920d3`.

The accepted Gray root and inventory contain exactly 20 paths under
`src/cli/core/OpenForge.Cli.Core/Commands/Repair/**`. Its exact root/inventory
hash is `b00ec856f633289a1a916472e93844eeccb15b20c01ab29d69e5463493a758a3`.
The boundary freezes the command-local definitions and options, binding and
request/relink grammar, selection and finite catalogue, plan/effect/no-op/
conflict models, result, command-local JSON and presentation, and the
explicitly failing operation and binding skeleton. It does not change root or
static composition, Doctor, Framework, Shell, projects or configuration,
TestSupport, shared JSON, tests, or another command.

The accepted corrections tighten option metadata, selected-proposal occurrence
identity and non-overlap, step-to-selection coverage, outcome/recovery
coherence, and blocked/no-op projection. They also enforce typed result-status
and recovery/residual coherence, normalized residual paths, cleanup next-action
selection for retained recovery, and lifecycle precedence that places
`Interrupted` after `Incomplete`.

Red opens only in the already-frozen disjoint Unit and combined
Integration/EndToEnd lanes described in the Execution Capsule below.

## Accepted Red Freeze

The cumulative Red boundary is accepted at immutable tip commit
`af59c957e5b74550731554f6195e1330de98ece9`, tree
`903ddc56e487e2976ed0019b8a5973c09c3242a6`. Unit evidence is commit
`5bf5f690c4ce3ebfbc40f18a9cdd9fef5d4f2379`, tree
`9e3041563ca1e2481835c1cae49b9eca1ea88a7d`, over Red activation parent
`cd4ce0899040c06836a6061b3c65afc55d4ad61d`. Integration and EndToEnd
evidence are the accepted tip over that Unit commit.

The exact 12-path Red inventory contains eight Unit files under
`src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Repair/**`, three
Integration files under
`src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Repair/**`,
and
`src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRepairProcessTests.cs`.
Its sorted path-manifest SHA-256 is
`1a058269d30e95c7e405d1fb8c8f7baf2c16397bb21d3ad91d565c64ae4ed638`, and
its path-plus-content manifest SHA-256 is
`e36ea642fe03c0effb16567e1c9c3da5e568005a2e9d8c1408c8839374878741`.

Fresh warning-free Release compiles preceded the exact no-build selections.
Unit selected and executed 51 cases: 49 passed and exactly two failed only at
the deferred `RepairPlanner.Build` seam. Integration selected and executed six
cases, all failing only at the deferred `RepairOperation.ExecuteAsync` seam.
EndToEnd selected and executed exactly three Repair journeys, all reaching the
expected missing-composition boundary. Every selection had zero skips. The
three existing Doctor EndToEnd journeys remain unchanged. Formatting,
targeted analyzers, diff, expected/protected-path, callable/count,
prohibited-pattern, machine-path, changed-line, and 200-character line checks
passed.

Fresh combined C#, behavior-contract, and test-evidence acceptance review
`T19-RED-ACC-01` passed without findings on the immutable tip. It confirmed
that the evidence is Open Forge-owned, non-tautological, and free of setup
failures at the intended Red boundaries. This phase-boundary review consumes
neither `T19-R1` nor `T19-C1`. Milestone 3 is complete. The accepted upstream
refreeze below satisfied the remaining Green gate, and the Overseer explicitly
authorized coherent Green.

## Accepted Upstream Refreeze

The integrated Task 18 baseline was merged without rebasing at commit
`f043751247d356c458ccef8efddd4cc3c7d6f528`, tree
`25d88102be8456c0004b2a039f690145960e7c3a`, with parents `fb8ce672` and
`2c62f59a`. The 20-path Gray manifest remains
`b00ec856f633289a1a916472e93844eeccb15b20c01ab29d69e5463493a758a3`, the
12-path Red manifest remains
`1a058269d30e95c7e405d1fb8c8f7baf2c16397bb21d3ad91d565c64ae4ed638`, and
the combined 33-path manifest remains
`10a3afd96c085cbb8da6b7b608a29f59d3d4a29aaebaac14d319518f06a3fd56`.
Every frozen Repair blob is unchanged.

Locked restore and a fresh non-incremental Release solution build passed with
zero warnings and errors. Exact no-build refreeze selected 51 Unit cases with
49 passing and two failing only at `RepairPlanner.Build`; six Integration cases
failed only at `RepairOperation.ExecuteAsync`; and exactly three Repair public
cases failed only at the missing-composition boundary. The retained three
Doctor public cases passed. Every selection had zero skips. Formatting,
static, protected-path, callable-shape, prohibited-pattern, machine-path,
changed-line, and line-length checks passed. The public-help invalidation from
Task 18 is therefore discharged for the frozen Red boundary.

## Execution Capsule

This capsule is frozen at Preflight. It records the accepted boundaries for
activation, Gray, Red, Green, evidence, review, correction, and acceptance.

### Gray Boundary

The exact isolated Gray mutation root from the preparation base is only
`src/cli/core/OpenForge.Cli.Core/Commands/Repair/**`. Gray freezes definitions
and options, binding/request/relink grammar, selection, the finite catalogue,
plan/effect/no-op/conflict models, result, command-local JSON and presentation,
and an explicitly failing operation and binding skeleton.

Gray must not touch root or static composition, Doctor, Framework, Shell,
projects or configuration, TestSupport, shared JSON, task or control records,
or any other command. Overseer acceptance of this frozen Preflight packet is
required before Gray.

### Red Boundary

After accepted immutable Gray, Red opens only in the already-frozen disjoint
Unit and combined Integration/EndToEnd lanes from that Gray snapshot:

- Unit only:
  `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Repair/**`.
- Combined Integration/EndToEnd only:
  - Integration:
    `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Repair/**`.
  - EndToEnd: the new exact file
    `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRepairProcessTests.cs`.

Fixtures remain lane-local. No shared TestSupport change is allowed unless a
later accepted finding proves it necessary.

The public Repair evidence retains exactly three simple journeys:

1. `repair --help` succeeds without workspace inspection or writes.
2. `repair --automatic --dry-run --json` previews one safe-exact correction,
   leaves a guided candidate unselected, and creates no workspace, lock,
   recovery, or temporary effect.
3. One explicit `--relink` applies one contained missing-target correction,
   preserves the label and unrelated bytes, deletes the successfully handled
   recovery bundle, and an automatic rerun converges to a verified no-op.

Retain exactly three existing `PublishedDoctorProcessTests` journeys.

### Focused Evidence

After Red is frozen, Unit and Integration use exact project selection with a
nonzero Red-frozen count:

```text
dotnet test --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj -c Release --no-build --filter-trait "Feature=repair" --minimum-expected-tests <Red-frozen-count>
dotnet test --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj -c Release --no-build --filter-trait "Feature=repair" --minimum-expected-tests <Red-frozen-count>
```

Repair and retained Doctor public evidence use the EndToEnd project and exact
class filters:

```text
dotnet test --project src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj -c Release --no-build --filter-class "*PublishedRepairProcessTests" --minimum-expected-tests 3
dotnet test --project src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj -c Release --no-build --filter-class "*PublishedDoctorProcessTests" --minimum-expected-tests 3
```

Zero-test, stale `--no-build`, skipped, warning-bearing, partially loaded, or
wrong-scope receipts do not pass a gate. Focused evidence proves Repair-owned
grammar, selection, catalogue, planning, conflicts, dry-run, byte preservation,
revalidation, recovery, application, verification, no-op convergence, results,
presentation, and public reachability at the cheapest decisive boundary.

### Full Acceptance Gate

After Green, and again whenever that evidence is invalidated, acceptance
requires locked restore; a warning-free Release solution build; direct managed
Unit, Integration, and EndToEnd execution; supported `linux-x64` native root,
Integration, and EndToEnd publish and execution; managed EndToEnd execution
against the same native root; and formatting, diff, static, protected-path,
callable-shape, prohibited-pattern, machine-path, changed-line and line-length,
no-JavaScript/MJS/CJS, no-public-command-subprocess, and no-direct-file-effect
checks. Use only artifacts from the same worktree. Do not publish packages.

The full project paths are:

- `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj`.
- `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj`.
- `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj`.

### Upstream-Green Gate

Task 18 is accepted and integrated into `develop`. Its baseline was merged, not
rebased, into the Repair lane and refrozen at the accepted commit above. The
completed gate required:

1. Prove that the preparation base commit
   `76e8e5f1f58e60a9de159318d10e7b3e9f8fc9c9` is an ancestor.
2. Prove zero intersection between the final Task 18 delta and the frozen Gray
   and Red paths.
3. Reconcile the integrated static root while leaving only Repair composition
   deferred to coherent Green.
4. Rebuild Gray, rerun Red, and prove the intended missing-behavior failures.
5. Freeze the new base before coherent Green.

All five requirements passed. Root/static Repair composition and any necessary
narrow directly callable Doctor diagnosis reader now belong to Curie Repair Green's
authorized Green boundary. The same Brilliant Implementer owns focused
verification and the later grouped correction pass.

### Invalidation Rules

Return to the earliest affected Gray or Red boundary if Task 18 changes any of
these paths or meanings:

- `src/cli/core/OpenForge.Cli.Core/Framework/Sources/Operational/**`.
- `src/cli/core/OpenForge.Cli.Core/Framework/Sources/References/**`.
- `src/cli/core/OpenForge.Cli.Core/Framework/Documents/Markdown/**`.
- `src/cli/core/OpenForge.Cli.Core/Framework/Mutation/**`.
- `src/cli/core/OpenForge.Cli.Core/Framework/Recovery/**`.
- `src/cli/core/OpenForge.Cli.Core/Shell/Interaction/**`.
- `src/cli/core/OpenForge.Cli.Core/Shell/Parsing/**`.
- `src/cli/core/OpenForge.Cli.Core/Shell/Pipeline/**`.

Also invalidate on a shared result or serialization contract change; Repair
contracts; C# or CLI Directives; test-project or TestSupport change;
public help or parser convention change; `repair/repair/workspace` recovery
attribution change; Doctor change beyond the agreed one-kind removal; a
non-ancestor final baseline; any frozen-path overlap; or a merge conflict in a
frozen path. The repeated
`src/cli/core/OpenForge.Cli.Core/` prefix is written explicitly above so each
protected path is unambiguous.

## Expected, Protected, And Integration Paths

Expected production mutation is limited to
`src/cli/core/OpenForge.Cli.Core/Commands/Repair/**` during Gray. Later
directly required composition and producer-reader neighbors remain deferred to
the accepted upstream-Green gate. Red is limited to the exact Unit,
Integration, and EndToEnd paths above.

Protected meaning includes all Task 18 mutable authority, other commands'
private `Shared/**`, Framework lifecycle semantics and files, public shared
result coordinates, dependency/platform/project/build/package/release and
generated-source authority, legacy CLI and npm paths, and every target outside
a complete accepted Repair plan. No neighboring expansion is allowed without
Task Mastermind acceptance. Shared or public contract changes require an
Overseer decision.

## Evidence

Cover no repairs, one/many independent and dependent repairs, unrepairable and
unavailable findings, stale Doctor facts, lock/revalidation race, dry run,
confirmation/write policy, partial failure at each step, recovery, post-repair
Doctor outcome, idempotence, preservation, process, and AOT.

The accepted recovery contract requires every existing-target `Replace` to have
one immutable, externally prepared and verified recovery bundle before the
first effect. A verified no-op and a dry run create no recovery or temporary
effect. Recovery disposition, residual paths, exact byte preservation, and
fresh relevant-domain post-diagnosis remain visible in the typed result.

Run the focused commands only after Red freezes nonzero counts. Full acceptance
then repeats locked restore, warning-free Release build, direct managed and
supported `linux-x64` Native AOT execution, managed-on-native EndToEnd, and all
formatting, static, protected-path, callable-shape, prohibited-pattern,
machine-path, line-length, and no-direct-effect checks from the frozen capsule.

## Stop Conditions

Stop before repairing a finding without complete provenance, executing free-form
instructions, hiding producer behavior, claiming all findings repairable, or
continuing after a dependency-invalidating failure.

Also stop before Task 18 integration for semantic Green and before semantic
Green until the upstream-Green gate has passed; stop on upstream invalidation;
on any product, architecture, shared-contract,
or safety change; before repairing incomplete-provenance findings; before any
unauthorized destructive repository, worktree, or source operation or
out-of-plan destructive effect; an accepted in-plan `Replace` remains allowed
only after its recovery, preflight, revalidation, and verification gates; stop
before any remote, release, or publication effect; or when accepted evidence
would require an unaccepted seam, workaround, shared abstraction, or
protected-path expansion.
