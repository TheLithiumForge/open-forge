---
open-forge:
  description: Observed CLI flow results and reproducible gaps from the 2026-09-19 review
  tags: [Memory, CLI, RunRecord, Evidence, Contextual, Archived, Historical]
---

# CLI Experience Run

## Basis

This manual run supports [Task 49](extension-experience-review.md) and the [reviewed flow collection](../../crystallized/documents/cli/experience/flows/_flows.md). All 438 supplied scenarios were assessed; this run exercises selected connected paths, not all 438 cases. No new automated test or CLI behavior change is included. The maintainer must validate the flow selection before new tests.

The Windows managed executable was built from checkpoint `7b4745f7` plus the four Planning templates. `OpenForge.Cli.Core.dll` SHA256: `B1703CDE8647D49580AFE8B4907899B741D3D7F1A255660811159DA172AE781D`. The apphost SHA256 is `966CCAAF5A326468264808055386BAD7F1B67963EDD25CD7A6EBD5EC9EC10278`; the apphost alone does not identify embedded content. The original workspace remains separate from this worktree.

## Evidence Boundary

The fresh execution agent used isolated workspaces, catalogues, and `OPENFORGE_DATA_HOME` under `.temp/astra-open-forge-2026-09-19/flow-runs/`. Captures include actual argv, stdout, stderr, exit and inventories with file hashes and raw symlink targets. Fixture actions are separate from CLI calls. A state description written before execution is an expected check, not proof that it passed.

The first harness attempt reused the F01/F02 baseline on retry. Those claims remain invalid; the canonical fresh-root F01/F02 evidence was independently audited instead. Initial F12 content creation failed in the fixture; its empty-package result is not a product defect. Initial Library attachment failed on missing metadata before links existed; downstream commands from that attempt do not prove Library synchronization or detachment behavior. Corrected fixture continuations remain separate from the original failures.

Inventory comparisons and content inspection take precedence over provisional harness verdicts. In particular, exit 3 with unchanged navigation is not a passing plain-note index. Not-run branches are neither passes nor product failures. Real interactive consent, live writer contention, access-denial variants and selected fault branches are not established merely by describing them.

## Confirmed Gaps

| Flow / case | Observed disagreement |
| --- | --- |
| F03 / X02 | A plain Markdown note causes index to report incomplete work and change no navigation. The authored note survives, but enrichment is required before the journey works. |
| F03, F08, F21 / X05, C02-03 | Doctor can describe an unfinished Framework update after ordinary note or link changes. Default output hides warning subjects behind a request for `--detail standard`. |
| F05 / C14-09 | Create-first nested guidance stops on the absent parent entrypoint. Explicit init and retry work, but the desired direct path does not. |
| F06 / X11 | With an empty uninstalled child recorded as cwd, default status reports installed/current. Explicit workspace and missing-path selection work. The captured inventories omit A/B, so their preservation is not proved. |
| F09 / C16-04, C16-02 | The supplied shorthand destination is rejected: move requires a Markdown file path beneath `.agents`. This is an invocation/interface gap, not evidence of corrupted links. |
| F10 / C17-05 | The first removal preserves surrounding authored text; repeating the already completed removal returns invalid input instead of a harmless no-op. |
| F11 / C22-05, C23-04 | Installation and update work, but retained recovery data prevents the subsequent prune/remove sequence from completing. |
| F11 / C19-03, C22-03 | The report counts two dependencies for a fixture declaring only `base`; update describes an untouched installed file as user-changed, and preview uses completed tense. |
| F09 / C16-04 | The corrected physical-path preview says entries and links were updated despite no changes, and calls an outgoing-link rewrite an incoming old-path link. The actual corrected move preserves the intended links and labels. |
| F12 / C19-03 | Inspect reports one dependency for a manifest and installed record with an empty dependency list. Installation itself succeeds. |
| F13 / C21-09, C21-14, C21-10, X15 | Protection works, but blocked/preview output claims navigation or backup effects that the independent inventory does not show. Suggested follow-up commands omit the custom source. |
| F15 / X12 | A malformed file in an unrelated routed branch blocks the selected valid package install; no selected effect occurs. |
| F17 / C27-07 | A replacement user file blocks the whole sync, including an independently addable new member. Backing up the replacement lets the new member and restored link synchronize. |
| F17, F18 / C28-05, C28-06 | Detach stops for an already absent link or an ordinary replacement file instead of removing the remaining exact link and releasing registration. The replacement and source bytes are preserved. |
| F18 / X17 | Detachment after source disappearance succeeds, but the report says source files were kept even though the source folder was already absent. |
| F22 / C14-07 | Creating an unambiguous destination without description/tags is rejected. The explicit-metadata recovery succeeds. |
| F26 / native Skill diagnosis | Planning and its Workflow dependency install, but Doctor treats the native `use-workflow` Skill and its support resources as missing metadata or unreachable ordinary routes. All actual local package link targets exist. |

These are observations against the reviewed targets. Current contracts and desired UX must be reconciled before changing implementation or approving exact output snapshots. Preserve the successful effects and file-protection evidence while fixing each failed boundary.

## Controlled Partial Update

The parent ran an additional deterministic Windows fixture under `.temp/astra-open-forge-2026-09-19/parent-fault-run/679ff302e3f04138b453bbbe96da6302/`. After a verified Core install, it appended distinct text to Guidance's entrypoint and the loader. An ordinary read handle held the loader with `FileShare.Read`: reads remained possible, replacement did not. All mutation calls explicitly selected the scratch workspace.

`update --automatic` exited 1 after replacing Guidance, before replacing the loader. Guidance exactly matched the original installed bytes afterward; loader and ownership hashes remained unchanged. A real recovery ZIP survived with a manifest and both prior payloads. Doctor identified the mixed prior/intended state. This establishes F19/X09's actual partial-effect boundary without timers, fabricated archives, or product hooks.

The update correctly reported one of two changes, but named `.agents/open-forge.lock.json` as the failed write although the controlled obstruction was the loader. This is an apparent error-attribution defect requiring reconciliation with the real failure boundary. No generic repair or rollback was invented, and recovery evidence was retained. The remaining recovery-choice and cleanup steps were not claimed as executed.

## Missing And Changed Link Detachment

The supplementary fixture under `.temp/astra-open-forge-2026-09-19/parent-library-run/bf0ddc34764142d8b2a68f2d4106565b/` establishes a valid destination, two described source files, a successful public attach and two verified relative file symlinks. It deletes only the first destination link, preserving the source, then attempts detach. Exit 5 leaves the second link and registration in place. It then writes an ordinary replacement at the absent destination and retries: exit 5 again preserves both the replacement and the remaining link. Both source hashes remain unchanged.

This is direct evidence for the unmet C28-05/C28-06 continuation targets, not a symlink-capability limitation. The desired behavior must still preserve the replacement; the gap is unnecessary blockage of independently known detach effects.

## Successful State Evidence

- F04: copied Scenario content under Working Memory stays independent when its original Template changes; metadata updates preserve the authored body.
- F07: the executed intersection, union and heading searches return the independently chosen match sets and leave files unchanged. The outside-scope include-filter decoy remains untested.
- F13: occupied and edited user content survives blocked operations; force replaces the eligible content, and the next identical install is unchanged. Reporting defects remain separate.
- F14: absence of an external grant produces no changes; explicit `--allow-path docs/team.md` creates exactly that file and persists the exact grant. Real once/always prompts and revocation were not exercised.
- F16: attach creates two real relative file symlinks; additions and retirement change the independently counted link set; detach removes only the registered links and retains the source files.
- F18: source disappearance leaves real dangling links. Sync preserves registration instead of inferring retirement; detach can remove those exact dangling links and release registration.
- F23: a real package removal produces a recognized recovery bundle. Preview preserves it, cleanup removes it without changing source files, and repetition reports no recovery data. Damaged candidates and deletion-failure branches remain unrun.
- F24: the CRLF input remains byte-identical except for the explicitly requested description replacement; reading and repeated indexing do not change it.
- F25: context emits the base then its overwrite, the overwrite has no separate route entry, and both authored files survive repeated indexing unchanged. Context emits a metadata warning but still supplies the content.
- F26: the four installed Planning starters match the repository copies exactly; removing Collaboration retains Planning and Workflow content. No Development Toolkit was installed.

## Canonical Execution Receipt

The fresh main run is .temp/astra-open-forge-2026-09-19/flow-runs/evidence-20260919-172325-f03671cf/. Its results.json contains 26 flow records, 191 recorded actions and 175 CLI calls. These counts include setup and repeated presentation views; they are not counts of complete scenarios or passing tests. The two parent supplements add nine CLI calls for a real partial update and two detach boundaries. F20's live-writer contention path remains unrun. Other unrun alternatives remain listed in the raw record.

Raw runner verdicts are provisional and do not replace the audited observations below. In particular, its unavailable F19 is supplemented by the real controlled fault, and its partial labels do not conceal the identified failures. Routine successful state comparisons were checked independently, rather than inferred from the absence of a failure entry.

| Flow | Audited observation |
| --- | --- |
| F01 | Fresh install, preview, readiness and repeat preserve the README and expected state. Count wording omits the generated ownership file from the reported payload count. |
| F02 | All three native Skill files match fixture hashes; discovery/context and repeated indexing preserve them. |
| F03 | Plain-note target fails; optional-metadata enrichment is currently required for recovery. |
| F04 | Copy independence and body preservation pass on the executed path. |
| F05 | Create-first target fails; explicit parent initialization recovers the journey. |
| F06 | Default status from the empty child says installed/current, contrary to the selected target. Explicit A and invalid-path selection work. A/B preservation is not proved by the captured inventory. |
| F07 | Executed discovery queries and read-only preservation pass; not every filtering branch is covered. |
| F08 | Exact relink succeeds; diagnosis and the final unresolved choice remain gaps. |
| F09 | Corrected physical-path move preserves labels and rewrites incoming/outgoing links; shorthand is rejected and preview wording/count classification misleads. |
| F10 | First removal preserves authored meaning; repeated removal fails the no-op target. |
| F11 | Update succeeds; retained recovery blocks subsequent lifecycle work, with additional report defects. |
| F12 | Scaffold authoring and installation pass with exact source bytes. Inspect falsely reports one dependency for a dependency-free manifest. |
| F13 | Protection and force effects pass; claims about navigation, recovery and next commands are misleading. |
| F14 | Exact persistent grant branch passes; interactive once/always and revocation are not run. |
| F15 | Unrelated malformed content blocks the useful selected install. |
| F16 | Real link creation, membership synchronization and ordinary detach pass. |
| F17 | Missing-link restoration passes; changed-file sync and missing/changed detach block independent work. |
| F18 | Detaching dangling links passes; already-absent link handling and source-preservation wording have gaps. |
| F19 | A real partial update and diagnosis are exercised; error attribution is suspect and recovery choice remains explicit. |
| F20 | Not run: no live writer-held mutation boundary was established. |
| F21 | Views preserve observed facts, but default warnings are not actionable and diagnosis can be misleading. |
| F22 | Optional-metadata creation fails; explicit-metadata recovery succeeds. |
| F23 | Real recovery cleanup and repetition pass; damaged and inaccessible candidates remain unrun. |
| F24 | Exact authorized metadata replacement preserves remaining CRLF bytes. |
| F25 | Base/overwrite order, one route entry and no-churn indexing pass, with a metadata warning. |
| F26 | Optional package composition, template bytes and removal isolation pass; native Skill diagnosis has gaps. |

## Next Boundary

Validate the 26 concise flow definitions and the revised scenario targets. Then reconcile affected contracts, implement useful CLI changes, and add focused end-to-end tests with semantic state assertions and named output snapshots. Do not turn current failure output into an accepted snapshot merely because it was observed.
