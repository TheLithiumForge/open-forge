---
open-forge:
  description: Open Task 36 to design partial file merging by Extensions and to replace the comment guards in authored Markdown with a boundary an agent still reads as an instruction
  tags: [Memory, Working, CLI, Task, Extensions, Markers, Authoring, Contextual, Active]
---

# Task 36 — Extension Partial Merge And Guard Replacement

**Reviewed on 2026-09-28:** [review](../../../emerging/analysis/open-task-review/task36-extension-merge-and-guards.md). Recommendation:
Needs the maintainer's decision first. The review names any details in this record that are out of date.

## Accepted execution on 2026-09-30

The maintainer approved the visible heading and named closing paragraph (rank 1),
legacy HTML-comment compatibility, automatic conversion by Index or Update,
parallel Worker Watch execution in an isolated development worktree, squash
integration into `develop`, then push and beta 3 publication after green checks.
This execution covers Question 2 only. Partial Extension merging remains deferred.

### Frozen behavior

- Canonical hosts use the top-level ATX heading `# Open Forge` and the standalone
  strong paragraph `**End of Open Forge managed section.**`. Both AGENTS.md and
  CLAUDE.md use these boundaries. Instructions and Claude imports remain ordinary
  Markdown between them.
- Continue reading the exact legacy `<!-- open-forge:start -->` and
  `<!-- open-forge:end -->` pair indefinitely as compatibility input. A heading
  inside that legacy pair is content, not a second opening boundary.
- Root `update` performs conversion under its existing trusted-ownership,
  preview, confirmation, exclusion, recovery and expected-state rules. No new
  migration flag or force requirement. `index` retains its navigation scope.
- New installs emit canonical hosts. Install keeps its existing managed-state
  policy, directing managed divergence to Update.
- Only root-level parsed Markdown blocks establish boundaries. Fenced examples,
  inline text, quoted blocks and nested lists cannot claim host ownership.
- Missing, reversed, duplicated or mixed boundaries block writes. A heading
  without its named end never extends ownership to end-of-file. Preserve the
  exact bytes before and after the recognized span, including Unicode and BOM.
- Recognize formatting through Markdig structure, retain source spans, and keep
  one parser owner. Formatters must not change the intended region. Repeated
  Update after conversion is a no-op.

### Execution capsule

Method: Use Workflow / Development, with the workspace Adaptive Development
recipe for parallel ownership. Profile: Standard with a full integration gate.
Review budget: one independent holistic review including prose. Correction
budget: one grouped review pass, plus focused implementation diagnosis.

The product modifies local Markdown owned by users. The consequential failure
is loss of adjacent instructions. Existing recovery and expected-state checks,
plus strict parsed spans, protect ordinary malformed input and concurrent edits.
This does not add an adversarial same-user security boundary. Standard Markdig,
UTF-8 and existing managed mutation primitives suffice. Exceptional machinery:
none. No dependencies, project graph, wire envelope or ownership schema changes.

The shared parser remains in Framework/Documents/Markdown. Distribution's
FrameworkContentIdentity consumes its typed facts. Preserve the existing
ReadManagedBlock signatures and recognition result used by Install and Update.
An immutable managed-host fact attached to MarkdownDocumentFacts exposes absent,
present or invalid state, the complete span, legacy/canonical form and cause.
The parser builds it from the existing Markdig document; downstream code never
reparses host syntax. Any new support and models stay beside this capability.

### Parallel packets and dependencies

1. Boundary parser and focused unit evidence: Markdown parser/facts, its local
   support, FrameworkContentIdentity and focused mirrored unit tests.
2. Update migration and preservation evidence: UpdateManagedHostIntegrationTests
   and only its directly needed command-local support. Keep legacy fixtures.
3. Installable and repository host files: the four AGENTS.md/CLAUDE.md payload
   and root files. Preserve all instructions outside the old pair.
4. Public guidance: docs/cli.md and affected Docusaurus pages, describing the
   canonical form and Update conversion. Do not rewrite historical material.
5. Accepted contracts: Framework Markdown syntax/compatibility and Install/Update
   command contracts, plus directly affected maintenance constraints.
6. Integration owner: inspect all patches, handle directly affected assertions,
   run gates, update task state, prepare version and integrate/release.

Writers use separate Worker Watch worktrees from the plan commit. Their output
is an uncommitted scoped diff. No worker commits, pushes or publishes. Foundation
signatures are frozen above; tests that need the new payload run after integration.
Workers are not alone and must preserve changes outside their ownership.

### Evidence and completion

Unit: canonical/legacy recognition, false markers inside examples, incomplete,
duplicate and mixed pairs, Unicode spans, LF/CRLF and formatter output.
Integration: new install; normal and dry-run legacy Update for both hosts;
unchanged prefix/suffix, including a following level-2 section; excluded hosts;
invalid boundaries unchanged; canonical repeat no-op and existing ownership forms.
Public journey: installed package update from legacy hosts using the delivered CLI.
Run the complete managed suite and supported Windows Native AOT gate at integration
because shared parsing, embedded payload and release behavior change. Run delivery,
format and docs checks. Record exact commands, runtime targets, counts and outcomes.

Current status: the guard replacement is implemented and locally accepted.
The final local evidence and authorized remote delivery gate are recorded below.
Partial Extension merging remains a separate deferred question.

### Implementation and qualification record

The isolated branch is `codex/task36-visible-host-boundaries`, based on local
`develop` at `f5f95f2aa`. The accepted plan was committed as `441c2c700` before
workers started. Eight implementation packets ran through Worker Watch in
separate worktrees: parser, migration evidence, payload, public docs, contracts,
install evidence, public journey and exclusions. One additional read-only
worker performed the holistic code and prose review.

The Worker Watch catalogue did not advertise GPT-6 Luna. Seven precisely
specified packets used the workspace-permitted `gpt-5.6-luna` at `max`;
parser ownership and independent review used `gpt-6-astra` at `medium`.
All patches were inspected before integration. Workers left uncommitted diffs.

The shared Markdig parse now produces a typed managed-host fact. Distribution
consumes that fact through its existing recognition surface. Install and Update
retain their ownership and mutation policies. Changed payload identity selects
legacy conversion through normal Update planning. The four source/dogfood
hosts and affected public and accepted contract documents use the visible form.

Initial evidence from Windows x64:

- Baseline Release build: zero warnings/errors; 18 focused existing
  Install/Update tests passed before implementation.
- Implemented parser/identity evidence: 46 cases passed. Unicode character and
  UTF-8 byte offsets, BOM, LF/CRLF, examples, invalid pairs and legacy input are
  covered.
- Initial combined managed run: 3,735 unit cases passed. Integration found
  two setup failures in the new exclusion fixture and four Status snapshot
  failures. The fixture now creates settings before installation, respecting
  TemporaryWorkspace ownership. The 64 affected snapshot files change only
  payload byte/character/token measurements and derived startup shares. Their
  explicit, class-scoped refresh passed all four cases; normal verification
  runs with snapshot updates disabled.
- All 260 public journey cases passed, including the new migration journey for
  both hosts, dry-run, exact surrounding bytes and repeated no-op behavior.
- Delivery type checking/lint/format passed; delivery tests passed 55 cases and
  package-layout tests passed seven. C# format checks passed before the review
  correction. Docusaurus type checking and production build passed.
- A real CLI/Prettier probe confirmed a formatting-only no-op with BOM,
  Unicode, CRLF and a following level-2 user section. The independent review
  identified one additional case: a narrow formatter width wraps the strong
  footer across a soft line break. The failing CLI probe reproduced R1. The
  grouped correction accepts only literal text and soft breaks inside the
  single strong footer, with parser and Update regression evidence.
  The existing `open-forge-markdown-v1` fingerprint normalizes line endings,
  not arbitrary prose reflow. Reflowed owned content is therefore restored to
  the canonical payload by ordinary Update; its surrounding bytes remain
  unchanged and the next Update is a no-op. The real CLI probe with Prettier
  `proseWrap: always` and `printWidth: 30` passed this preservation and repeat
  check. R1 does not expand fingerprint equivalence or change receipt policy.
- The Claude Code import contract was rechecked against the linked official
  [memory documentation](https://code.claude.com/docs/en/memory#import-additional-files).
  Imports remain ordinary Markdown, outside code spans and fences.

Repository-wide Doctor remains blocked by the twelve pre-existing
`reference.target-alias` errors around the beta-follow-ups routes, plus existing
workspace warnings. These are not release regressions. Changed documentation
links are checked separately; the obsolete InstallOperation links in the two
host maintenance contracts were corrected. Shared task ledgers have unrelated
uncommitted edits in the original checkout and are preserved.

Reproduction and intermediate logs are under ignored `artifacts/task36` in the
development worktree. The final local receipt follows. GitHub Actions and the
versioned release own remote CI and publication outcomes.

### Final local acceptance

Question 2 is implemented and locally accepted on 2026-09-30. The canonical
heading/footer and indefinite legacy input compatibility are complete. Question
1, partial Extension merging, remains deferred.

The clean development candidate `510545fc17167a39e19d3635119caf326d191808`
passed the Windows x64 Native AOT delivery gate at version `0.9.0-beta.3`:

| Test mode | Passed | Platform exclusions | Failed |
| --- | ---: | ---: | ---: |
| Unit | 3,747 | 0 | 0 |
| Managed integration | 2,541 | 17 | 0 |
| Managed public CLI | 260 | 0 | 0 |
| Native integration | 2,541 | 17 | 0 |
| Native public CLI | 260 | 0 | 0 |
| Managed tests against native CLI | 260 | 0 | 0 |

Reproduce with `npm run build:native -- --rid win-x64 --no-restore`,
`npm run test:built -- --rid win-x64`, and `npm run pack -- --rid win-x64`.
The build had zero warnings/errors, all six test modes passed, and the generated
npm tarballs passed the installed native package journey. The artifact manifest
recorded the exact candidate SHA, version and RID with `dirty: false` and
`tested: true`. Snapshot updates were disabled throughout this final gate.

An additional real upgrade began with the published
`@thelithiumforge/open-forge@0.9.0-beta.2` package and its actual ownership record.
The beta 3 native candidate preview changed no files, ordinary automatic Update
converted both legacy hosts, BOM/Unicode/CRLF prefix and level-2 suffix bytes
were preserved exactly, and repeated Update changed nothing.

The independent holistic review found one formatter issue, R1, and no other
actionable findings. The grouped correction recognizes soft-wrapped strong
footers without broadening the existing fingerprint policy. Parser evidence
and both-host Update regression coverage pass in managed and native modes.
C# formatting/analyzers, delivery checks/tests, package layout and Docusaurus
type checking/production build also passed.

All eleven unrelated changed or untracked files in the original checkout were
verified byte-identical before integration and are excluded from the squash.
Only this Task's acceptance record is added during integration; shipped source,
tests and snapshots remain the qualified candidate.

### Remote delivery gate

The maintainer authorizes squash integration into `develop`, then push. Release
requires the complete [Build workflow](https://github.com/TheLithiumForge/open-forge/actions/workflows/build.yml)
to pass for that exact merged commit on all six runtime targets. The existing
[Release workflow](https://github.com/TheLithiumForge/open-forge/actions/workflows/release.yml)
owns native package ordering, wrapper publication, beta-channel policy and
GitHub release assets. Its successful exact-commit artifacts may be reused.

The [beta 3 release](https://github.com/TheLithiumForge/open-forge/releases/tag/v0.9.0-beta.3)
and its linked Actions runs are authoritative for remote publication status and
the final release commit. Promote the verified released commit to `main` for
the documentation deployment, without pushing the development feature branch.

## Original task state

- State: **Open, not started.** Raised by the maintainer on 2026-09-16.
- Owner: Root.
- Two questions in one Task because they share a mechanism: both are about how
  an Extension's content and a user's content coexist in one file.

## Question 1 — partial merging of files by Extensions

Today an Extension owns whole files and whole regions. The maintainer wants the
option of an Extension contributing **part** of a file — the example given was
adding headers — so that one file can carry both authored and Extension-supplied
content without the Extension owning the whole thing.

What has to be decided:

- **What the unit of contribution is.** A heading and its body? A named block?
  A list under a known heading? The Entries region already proves one shape
  works, and its history is the cautionary tale: see the `index` finding where a
  section that extended to end-of-file swallowed authored prose.
- **How ownership is recorded.** The ownership document already distinguishes
  `paths` from `regions`, so a partial contribution is closer to a region claim
  than a path claim. Decide whether regions generalise or whether this needs its
  own concept.
- **What happens on update.** If the user edits inside an Extension's
  contribution, does the update overwrite, skip, or report a divergence? This is
  the same question [Task 33](../../../archived/cli-development/tasks/task33-managed-content-removal.md) and
  [Task 35](../../../archived/cli-development/tasks/task35-removal-and-suppression-model.md) face for removal, and the
  answers should agree.
- **What happens on removal.** Removing an Extension must remove its
  contribution without taking the authored text around it.

## Question 2 — replace the comment guards

The generated-region comment guards are going away; that is already recorded as
accepted in Task 30's findings, where the Prettier churn was closed as "resolved
by deletion, not by a fix". B1 migrated the Entries region to **heading-based**
location. This Task decides what replaces guards everywhere else, including in
`AGENTS.md`-style authored instruction files.

Candidate boundaries, none chosen:

1. **A heading**, as B1 already did for Entries. Proven, parses with Markdig,
   survives formatters. Its weakness is the end-of-section boundary problem
   already seen once.
2. **A fenced block with an info string**, for example a fence tagged
   `open-forge` with the instructions inside.
3. **A pair of thematic breaks (`---`)**, proposed by the maintainer on
   2026-09-25 for `AGENTS.md` and `CLAUDE.md`. The Open Forge section sits
   between two `---` lines. It reads as a clearly separated section in any
   Markdown preview, where HTML comments look like noise, and the text between
   stays plain instructions. Check before choosing it: a `---` directly under a
   line of text makes that line a heading, so each break needs a blank line
   above it. A `---` on the first line of a file starts YAML frontmatter. Other
   content may use `---` too, so the pair probably needs a heading inside, such
   as `# Open Forge`, to identify the region. Frontmatter was also considered,
   but a file has only one frontmatter block and harnesses don't reliably treat
   it as instructions.
4. Something else — the Task should propose alternatives rather than pick from
   these.

**The risk the maintainer flagged is the one to test first, and it is not a
parsing question.** A fenced code block is valid Markdown and easy to locate
with the existing parser — but an agent reading the file may treat fenced
content as an *example* rather than as an instruction it must follow. That would
silently weaken every directive it wraps.

So this cannot be decided on parser convenience. It needs evidence about how
the content is *read*, not just how it is located:

- Write the same instruction in each candidate form and check whether an agent
  actually follows it. More than one model, since this is a behavioural claim
  about readers, not a property of the syntax.
- Note that Open Forge's own `.agents` tree is the test corpus — the workspace
  dogfoods itself, so a form that reads badly will degrade this repository's own
  instructions first.

## Actionable boundary

- Decide question 2 before question 1 if they conflict: the contribution
  mechanism may want the same boundary the guards are replaced with, and picking
  two different ones would be the worst outcome.
- Any boundary must be locatable with the Markdig parser already in use. Do not
  hand-roll a scanner; that is the defect `hand-rolled parsing` recorded.
- A formatter must not be able to break the boundary. Prettier inserting blank
  lines is what killed the comment guards; test the candidate against it.
- Preserve authored text absolutely. The `index` finding — a section boundary
  that ran to end-of-file and deleted a user's appended prose — is the standing
  example of what failure looks like here.

## Acceptance

- A recorded decision on the boundary form, with the agent-readability evidence
  that justified it, including what was tried and rejected.
- A recorded decision on the partial-contribution unit and its ownership
  representation, agreeing with Tasks 33 and 35 on update and removal
  behaviour.
- Guards removed wherever the new boundary replaces them, with the Prettier
  containment in `.prettierignore` reduced accordingly.
- No authored text can be lost by any boundary the decision introduces, proven
  by a test that appends prose in every position around a boundary.

## Folded in on 2026-09-28

- **From [Task 30](../../../archived/cli-development/tasks/task30-cli-experience-remediation.md):** the "Managed Host
  Heading Direction" in [phase-4a-structural](../../../archived/cli-development/tasks/task30/phase-4a-structural.md)
  asks the same question as Question 2 here. Treat them as one.
- **Evidence against a heading-only boundary:** a `# Open Forge` region in this
  repository's `AGENTS.md` would absorb the `## Exact Mechanical Execution
  Exception` section that follows it, and the next update would overwrite it.
  See the [review](../../../emerging/analysis/open-task-review/task36-extension-merge-and-guards.md).

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.
