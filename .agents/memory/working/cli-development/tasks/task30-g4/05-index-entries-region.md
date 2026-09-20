---
open-forge:
  description: Make index rewrite only the list of entries inside the Entries section and give status and update the same boundary
  tags: [Memory, Working, CLI, Task, Plan, G4, Index, Safety, Contextual, Active]
---

# 05 — Index rewrites only the list of entries

> Read [00 — G4 conventions](00-conventions.md) first.

## Goal

`index`, and every command that rewrites an Entries section, replaces only the
list of entries. Text a person writes inside the `## Entries` section before
or after that list survives every rewrite byte for byte. `status`, `update`,
`doctor` and the fingerprint comparison use the same boundary, so one file
state gets one verdict.

## Depends on / Blocks

- Depends on: [01](01-before-snapshots.md).
- Blocks: nothing. Runs in parallel with 03.

## The finding

Recorded in the [Task 30 open findings](../task30-cli-experience-remediation.md#open-findings):
a line appended after the entries list of `.agents/maps/_maps.md` was deleted
by `index`, because the heading-based section from B1 runs to the next
heading or end of file and the whole body is replaced. `status` had called
the file `changed` while `update --dry-run` called it `same`, because the
fingerprint excludes the whole section interior.

## Definition of the generated block

Inside the section that starts at the unique `## Entries` heading and ends
before the next heading of level 2 or higher, or at end of file:

1. Skip blank lines after the heading.
2. The generated block is the longest run of consecutive lines that begin
   with `- ` starting at the first such line. A blank line or any other line
   ends it.
3. Everything before the block (after the heading) and everything after it is
   authored text and is preserved byte for byte, including its line endings.
4. When the section has no list, the block is inserted after the heading and
   one blank line, before any authored text, followed by one blank line.
5. The empty placeholder `- none - No entries - #Empty` is a list of one item
   and is the block when present.
6. A second list later in the section is authored text and is preserved.
   Doctor may report it at `full` as `Info` (the [doctor catalogue](11-doctor.md)
   names the kind); nothing rewrites it.

The fingerprint comparison excludes exactly this block and nothing else.
`status` compares the block, not the section. Route init, create, update,
move and remove rewrite the block through the same owner.

## References

- `src/cli/core/OpenForge.Cli.Core/Framework/Documents/Markdown/MarkdownEntriesSectionReader.cs` — the single owner; add the block span beside the section span.
- `src/cli/core/OpenForge.Cli.Core/Framework/GeneratedNavigation/GeneratedNavigationRegionPlanner.cs:52` (`beforeBody`) and `:340` (`BuildExpectedBody`) — replace the block, not the body.
- `src/cli/core/OpenForge.Cli.Core/Framework/Documents/Markdown/MarkdownFingerprintReader.cs` — exclude the block.
- The B1 slice [07](../task30/07-b1-heading-entries.md) for the tests that must keep passing (`RetiredGuardsMigrateOnce`, LF and CRLF stability).
- Consumers found by `git grep -n "EntriesSection" src/cli/core`.
- Contract sentences to record in the ledger (do not edit): the index interface's "rewrites the body of one unique ## Entries section", the index help `Notes` section, the Loader's Entries wording if it says the section is generated.

## Preconditions

- [x] 01 committed. Three suites green.
- [x] Reproduce: seed a workspace, append `\nA note.\n` after the entries list
      of `.agents/maps/_maps.md`, run `index`, confirm the line is gone. Keep
      this as the regression test seed.

## Steps

1. [x] Add `EntriesBlock` (start, end, line ending) to
       `MarkdownEntriesSectionReader` with unit tests: list only; prose before;
       prose after; prose both; blank lines around; no list; placeholder only;
       two lists; CRLF; heading at level 3 inside the section ends nothing
       (only level 2 or higher ends the section). Verify: unit green.
2. [x] Switch `GeneratedNavigationRegionPlanner` to replace the block. Insert
       when absent per rule 4. Verify: unit green; the diff renderer for
       `--dry-run` shows only block lines.
3. [x] Switch `MarkdownFingerprintReader` to exclude the block. Verify: the
       appended-note workspace reports `changed` from `status` and `update`
       alike, and both say the same after `index`.
4. [x] Integration test: seed, append the note, run `index`, assert the note is
       present and the second run reports nothing to do. Repeat with prose
       before the list and with CRLF. Verify: integration green.
5. [x] Run `index` over `src/open-forge/` and `src/extensions/*/content/` and
       assert zero updates, so the shipped payload needs no change. If any
       file changes, stop and record it.
6. [x] Three suites green, counts recorded.

## Expected result

Before: the note is deleted. After: `index` prints `Updated the Entries
section in 1 of 21 files.` when the list was stale and leaves the note in
place; `status` and `update` agree about the file.

## Acceptance

- [x] The regression test passes.
- [x] `index` twice over this repository's `.agents` reports zero updates on
      the second run. (Run it only after the fix is green; it is a mutation
      of this repository and must be reviewed with `git diff` before commit.)
- [x] No consumer computes its own Entries boundary.

## Changes ledger

- Execution: isolated `codex/task30-g4-entries` begins at snapshot commit
  `5c388e7c5747a8a229591807f1b3693e4dd7b14e`. Its qualified baseline is Unit
  3,382 passed, Integration 2,180 passed with 17 expected platform skips, and
  EndToEnd 131 passed; the five documented formatter findings remain.
- Evidence preparation: `IndexEntriesPreservationTests` uses the real Index
  operation and owned root/child fixtures, with prose before and after the
  first list, a second authored list, a level-three heading, and LF/CRLF
  variants. Exact expected document bytes and second-run hashes are independent
  of the implementation. All four initial LF/CRLF prose cases failed before
  production changes because the authored suffix was deleted; all four pass
  after the block change (`artifacts/g4-05-red-integration.log` and
  `artifacts/g4-05-block-integration.log`).
- Generated boundary: the whole Entries section interior -> one shared
  `MarkdownEntriesBlock` carrying the first contiguous dash-space list span
  and its line ending. `ContentSpan` remains available for section structure;
  `OmissionSpan` now derives from the block. Existing list terminators and all
  surrounding authored bytes survive; an absent list receives the specified
  blank-line padding.
- Consumers: generated-content comparison, Loader declarations, generated-entry
  parsing, Find tags, Context expansion, References extraction and Route Move/Remove references now
  consume that same block. The authored fingerprint excludes only its bytes.
- Verification: comparing bounded generated-body strings before and after
  application -> comparing the fresh complete document bytes with the original
  complete expected bytes, while retaining fresh unchanged-state and exact
  selection/identity checks. Missing-list insertion and guard removal may
  legitimately change the replacement span; document verification remains exact.
- Tests: five frozen fingerprint vectors and generated-span expectations now
  retain authored heading padding instead of excluding it. Expected hashes were
  computed from literal retained strings independently of the production reader.
  Old malformed-declaration and stale-generated fixtures now start with `- `;
  bare prose has separate positive acceptance cases.
- Guard migration: the old whole-section rewrite normalized surrounding blank
  lines -> only the two exact obsolete guard comments are removed, preserving
  their line endings and all surrounding blank-line bytes. The LF/CRLF regression expects those retained
  bytes and continues to require a stable second run.
- Regression coverage: fourteen actual-operation cases cover LF/CRLF prose,
  missing-list insertion, and an installed maps file whose authored edits are
  reported as changed by both Status and Update before and after Index. The
  four installed-map cases pass; final suite qualification is recorded below.
- Snapshot review: the two Extension Inspect generated JSON expectations change
  only startByteOffset 21 -> 23, excludedInteriorByteLength 31 -> 29, and the
  matching excludedGeneratedBytes total. No human message changes. After this
  explicit recapture the full Unit suite passed 3,398 tests, and the first twelve
  preservation cases passed. Subsequent References and guard-separator changes
  passed the final qualification below.
- doc: `Commands/Index/Shared/Rendering/IndexHelpSections.cs:41` says "Index
  rewrites the body of one unique ## Entries section". The interface at
  `.agents/memory/crystallized/documents/cli/contracts/index-candidate/interface.md:334`
  and behavior at `behavior.md:201` also describe whole-body ownership. Task 41
  must distinguish section identification from list-only mutation/fingerprinting
  and describe missing-list insertion and preserved authored text.

- Canonical generated text: the final generated line still receives its local
  line ending, including at EOF. Preserving an unterminated generated list had
  changed Route Init's fixed scaffold bytes; restoring the existing final-line
  rule keeps those exact scaffolds while preserving all authored bytes outside
  the block.
- Continuity evidence: the old Route Inspect fixture inserted a separate list
  after the heading, with blank lines separating it from the baseline list.
  It now inserts the continuity entry beside the literal baseline entry inside
  the same list, preserving the original loading/count expectations and the
  fixture's platform line ending. All four loading cases pass.
- Generated-navigation, prospective-formation, Index projection, generic Route
  Init and published Index fixtures now identify their stale generated inputs
  with `- `. Exact generated-body and Route Remove bounded-byte expectations
  exclude the heading padding. Published Index's diff therefore shows
  `- - stale` for that literal list item; its three process cases pass.
- Reviewed snapshot delta: 45 files (28 JSON, 17 text). The 74 JSON leaf changes
  are only generated-body prefixes, the explicit stale-list fixture, and the
  two Extension Inspect byte-offset/count updates. Text changes are only those
  same bounded diff contents. Exact comparisons preserve all other members and
  sentences (`artifacts/g4-05-snapshot-review.json`, zero unexpected changes).
  The three explicitly regenerated Integration classes pass all 35 cases.
- Final qualification: Release build has zero warnings and errors. Unit passes
  3,398 tests; Integration passes 2,194 with 17 expected platform skips (2,211
  total); EndToEnd passes 131. Logs: `artifacts/g4-05-unit-third.log`,
  `artifacts/g4-05-integration-final.log`, `artifacts/g4-05-e2e-final.log`.
  Formatter verification reports only the five documented baseline findings;
  `git diff --check` passes. Focused semantic review passed after the retained
  guard-line separator correction.
- Shipped files: Index reports zero updates for all 20 Framework regions and
  the Memory Starters, Planning and Project Documents template entrypoints
  (one region each). SHA-256 hashes of all 47 files under the Framework and
  extension content roots are identical before and after. Other extension
  content roots contain no entrypoints. Receipts and hash inventories are in
  `artifacts/g4-05-payload-*.json`.
- Repository navigation: one selected Index application refreshes only three
  generated descriptions in `task30-g4/_task30-g4.md` to match existing child
  metadata. The full second run reports zero updates, 146 unchanged regions,
  one unestablished region and exit 3; all `.agents` file hashes are unchanged
  by that run. The existing missing proposal metadata is recorded below, so
  this receipt establishes stability without claiming complete inspection.

- G4 integration receipt: accepted commit `1eca17b2b50f7ad2c996921d3c7f40003c6627c` (parent `5c388e7c5747a8a229591807f1b3693e4dd7b14e`) was integrated into the G4 worktree through a clean 31-path patch (`artifacts/g4-05-integration-1/clean-paths.patch`, SHA-256 `ED05CF62F517BF068DC66C8E998991C343BB4601D42708AD34A1045523AB756A`). The final 05 non-snapshot set has 36 paths: 29 exact clean transfers and seven bounded reconciliations (including this ledger receipt) retaining the accepted 03/04 behavior in `PublishedIndexProcessTests`, `IndexApplicationIntegrationTests`, `IndexBeforeOutputSnapshotTests`, `RouteRemovePlanningIntegrationTests`, `ReferencesDomainModelTests`, and `IndexEntriesPreservationTests`. The existing 04 `UpdateInteractionTestSupport.Unavailable` seam was extended for the neutral Index preservation fixture.
- The 45 snapshot paths in the accepted 05 commit remain untouched and are reserved for one combined post-family recapture; no 05 snapshot assertions were claimed from this integration step. `_task30-g4.md` metadata was included only for its three generated Entries descriptions; authored task instructions and catalogue text remain unchanged. Main HEAD `e7415ceb9876635de6c0ccd53f60658bb0d9965b` and normal index tree `90968dcbf4a29e3f2da3b8b013d7ce344f75297c` remain preserved; no staging, commit, merge, or test/build was performed.

## Divergences observed

- The current fingerprint owner already consumes `OmissionSpan`; the change
  must make that span the generated list rather than introduce a second
  boundary calculation. `FrameworkContentIdentity.ReadGeneratedEntriesFingerprint`
  separately hashes generated content and must consume the same block.
- The shared Entries boundary also feeds Loader declarations, generated-entry
  parsing, Find body tags, Context link expansion, and Route Move/Remove authored
  reference scanning. Those consumers must all use the block so authored prose
  and later lists are treated consistently.
- The B1 guard-migration test uses the bare prose `stale` as its generated seed.
  Under the accepted list-only rule that text becomes authored. Preserve its
  migration purpose by changing the seed to `- stale`, while retaining the exact
  LF/CRLF and retired-guard removal assertions. Record other affected old
  whole-section expectations individually during implementation.
- Missing-list insertion exposed a pre-existing verification assumption that
  replacement spans remain identical across projections. Four new cases wrote
  the expected document but returned `VerificationFailed`. Exact whole-document
  byte comparison resolves that assumption without weakening verification or
  changing the public failure sentence. All final suites pass.
- Focused review G4-05-R1 found that removing an entire retired end-guard line
  could join the generated list to an authored second list. Remove the exact
  obsolete comment text but retain its original line ending, preserving a list
  separator. Two new LF/CRLF actual-operation cases require exact bytes and a
  stable second run; the existing migration case retains four rather than two
  line breaks on each side of the generated list. No catalogue wording changes.
- Repository dogfooding found that the existing
  `task30/phase-4b-g4-output-proposal-astra.md` has no frontmatter. Automatic
  Index therefore leaves the Task 30 parent unestablished and returns exit 3,
  preventing its first aggregate application. An explicit selection applies
  the independently valid G4 list; the next aggregate run reports zero updates
  and changes no files. The proposal is preserved. Task 41 can repair its
  navigation metadata; no source behavior or CLI sentence was changed to hide
  the incomplete observation.

## Rollback

Revert the branch. No document migration is involved.
