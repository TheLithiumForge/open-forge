---
open-forge:
  description: "Historical record: Deliver the Library attachment Git-ignore choice and its documented lifecycle"
  tags: [Memory, Task, CLI, Library, Installation, GitIgnore, Documentation, Contextual, Complete, Archived, Historical]
---

# Task 74: Library attachment Git-ignore choice

## Archive Status

Archived on 2026-10-04 after local acceptance at phase 3/3, milestone 4/4. Production, documentation, fresh review and all six managed and supported Windows Native AOT modes passed. The combined candidate awaited integration at that point. It subsequently shipped in beta6 after all six hosted platforms passed; [Task 70](task70-existing-workspace-adoption-during-installation.md#beta-6-release-complete-2026-10-04) retains the completed integration and public release receipt. The [current CLI ledger](../../../working/cli-development/project-control.md#completed-onboarding-release) defines the remaining work.

## Outcome

When attaching a Library, ask whether its projected links should be added to
the consumer workspace's `.gitignore`. Give unattended use an explicit way to
select the same behavior. Update the CLI documentation, help, Library user
flow and CLI Skill with the accepted option and its best use cases.

The maintainer requested this as a new Library improvement on 2026-10-04.
`library attach` is the Library installation operation. There is no separate
`library install` command. This Task does not reopen completed Framework
setup work in [Task 73](task73-layered-adoption-and-installation-choices.md).

## Behavior before Task 74

- Attach accepts a Library ID, a contained source root, an optional `--to`
  destination, `--allow-path`, `--dry-run` and `--automatic`. It has no ignore
  choice or planned `.gitignore` effect.
- A Library tree consists of individual relative file symlinks inside ordinary
  directories. The destination defaults to the workspace root. A directory
  may contain unrelated files or links from several Libraries.
- Interactive Attach resolves destination permissions, reviews the plan and
  asks for final confirmation. Permissions and Git-ignore intent are separate
  choices. Automatic, JSON, redirected and dry-run modes must remain usable
  without an interactive prompt.
- The existing Install-owned ignore section accepts only Framework route
  patterns. Library patterns need their own ownership and must preserve that
  section and surrounding authored text.

The [Attach contract](../../../crystallized/documents/cli/contracts/library/attach/_attach.md)
and [Workspace Libraries design](../../../crystallized/documents/cli/technical-designs/workspace-libraries.md)
define the existing operation. [Detach](../../../crystallized/documents/cli/contracts/library/detach/_detach.md)
and root [Remove](../../../crystallized/documents/cli/contracts/remove/_remove.md)
define its removal consumers.

## Accepted implementation decisions, 2026-10-04

The maintainer selected implementation in this session. Root freezes these
routine choices within the requested Library improvement:

- Attach accepts singleton `--git-ignore <true|false>`. Explicit false skips
  the choice. Omission asks only during prompt-capable interactive application;
  automatic, JSON, redirected and dry-run omission means false. Cancellation
  before the final confirmation applies nothing.
- If the source would project a root `.gitignore`, explicit true blocks before
  prompts. Prompt-capable interactive omission also blocks with an explicit-false
  next step; explicit false and unattended omission remain eligible. The normal
  choice uses existing typed No/Yes selection so No differs from cancellation.
- Ignore exact projected leaves, anchored to the consumer workspace. Escape
  Git pattern metacharacters literally. Never ignore a whole shared directory,
  workspace root or source tree. No Git executable or index operation is added.
- Persist optional `gitIgnore: true` in the Library ownership claim. Missing
  or false means no ignore intent and retains the existing serialized shape.
  Preserve the flag when projecting or releasing paths from a surviving claim.
- Maintain one separate `# BEGIN OPEN FORGE LIBRARIES` / `# END OPEN FORGE LIBRARIES`
  section containing the ordered union of opted-in registered projected paths.
  Preserve authored equivalent rules outside that section without adopting
  them, along with authored bytes, line endings and the Install section.
  Malformed, ambiguous or unsupported owned sections block the required change.
- Resolve the choice after structural Library admission and before one complete
  permission plan and final review. Include a planned `.gitignore` write in
  Library permission targets. Unattended writes require its existing grant or
  explicit `--allow-path .gitignore`; the choice is not itself a permission grant.
- Sync reconciles additions and retirements using recorded intent. Detach and
  whole-Library Remove release only owned patterns. Individual-link Remove
  reconciles the same section as companion metadata within its existing
  confirmed root Remove plan; it adds no separate permission flow or flag.
  Surviving Library entries and unrelated rules remain intact.
- An explicit `.gitignore` Remove never also rewrites that target. Respect saved
  path-removal exclusions instead of restoring an intentionally removed ignore
  file. Required opt-in Attach/Sync effects blocked by that exclusion report it;
  removal can release claims while an already absent ignore file stays absent.
- Carry a distinct ordinary-file change plus prior snapshot through planning,
  lease revalidation, recovery, application, receipts and reporting. Apply it
  after verified content/navigation effects and before ownership publication.
  Reuse ordinary Create/Replace and existing recovery kinds. No after-hook,
  source write, automatic rollback or new recovery format is permitted.

Git pattern meaning follows the [Git ignore manual](https://git-scm.com/docs/gitignore).
The CLI manages rules; later user rules and Git's own precedence still apply.

Ignoring changes Git's treatment of untracked paths. It does not remove already
tracked links from Git, change Framework loading, or authorize writes to the
source tree. No automatic Git index operation is requested.

## Plan

1. Freeze the accepted decisions above in the affected command contracts and
   Workspace Libraries design. Inspect current implementation and consumers
   before selecting callables or ownership fields.
2. Add the choice after safe structural planning and before one complete
   permission stage, preview and final confirmation. Show the actual paths
   affected. Include ignore changes in the existing expected-state validation,
   recovery, application, verification and partial-effect reporting.
3. Implement the accepted Sync/removal lifecycle in the same owned boundary.
   Keep source contents, authored rules, unrelated bytes and Install behavior
   intact.
4. Update [CLI reference](../../../../../docs/cli.md),
   [website CLI overview](../../../../../src/docusaurus/docs/cli/index.md),
   [website flows](../../../../../src/docusaurus/docs/cli/flows.md),
   [shipped CLI Skill](../../../../../src/open-forge/.agents/skills/open-forge-cli/SKILL.md)
   and the maintained [workspace Skill](../../../../skills/open-forge-cli/SKILL.md).
   Explain when to opt in, which paths are ignored and how cleanup works.
5. Run focused Library planning/application tests and published CLI journeys,
   then the required managed and supported Native AOT checks for the accepted
   public mutation change. Validate documentation against executable help.

## Completion evidence

- [x] Interactive yes and no have the accepted effects. No preserves ignore
      bytes while still attaching the Library.
- [x] Explicit unattended input selects the same behavior. Omission and
      noninteractive modes follow the accepted contract without prompting.
- [x] Dry-run reports exact ignore changes and performs no writes.
- [x] Root destinations, shared directories and unrelated siblings prove the
      selected path scope.
- [x] Authored rules, line endings and the Install-owned section remain intact.
- [x] Sync, Detach and both Library removal paths reconcile only owned entries.
- [x] A changed or unsafe ignore file and interrupted application use the
      existing guarded mutation/recovery path and report actual effects.
- [x] Sources remain unchanged and ownership is published after required effects
      verify.
- [x] Help, reference, flows and Skill agree with the delivered behavior and
      show a useful example.

## Completion state

Task 74 “Library attachment Git-ignore choice” (phase 3/3): milestone 4/4. Root accepts the integrated implementation and documentation locally. The completed execution record is archived; integration remains recorded in the current CLI ledger.

## Execution capsule

- Phases: 1 preflight and contracts; 2 implementation and focused evidence;
  3 documentation, fresh review and complete qualification.
- Fixed milestones: M1 accepted semantics and mutation seams; M2 coherent
  production with focused evidence; M3 aligned documentation/help/Skill;
  M4 fresh review and complete managed plus supported Windows Native AOT gate.
- Profile: one continuous implementer for tests, production and grouped fixes;
  Root owns shared meaning, documentation, integration and acceptance. Maximum
  two fresh reviews and one grouped correction pass; a real divergence returns
  to Root before widening scope.
- Consequence: local consumer links, ignore bytes, settings and ownership can
  change. Recovery retains exact prior ordinary bytes/absence and link identity.
  Git or backups and explicit/manual recovery remain available. The supported
  boundary covers defects, malformed input, interruptions, crashes and
  cooperating processes, not malicious same-user namespace races.
- Ordinary .NET 10 BCL, pinned native parser, source-generated JSON, real-file
  snapshots, same-workspace lease and existing recovery/application capabilities
  suffice. Exceptional machinery: none. No new project, package, reflective
  serializer, Git adapter, native bridge or compatibility reader is required.
- Shared reuse: existing mapping, ownership codec/path release, settings grants,
  no-follow ordinary-file observation, mutation/revalidation, recovery and report
  coordinates. Library section semantics are one neutral Library capability
  shared by its mutators and root path Remove because both reconcile the same
  accepted ownership facts; Install's private section remains independent.
- Cheapest decisive evidence: Unit for parser, pattern rewriting, ownership
  projection and deterministic plans; real-filesystem Integration for ignore
  Create/Replace, drift, source protection, permission, interrupted effects and
  removal consumers; published-process evidence for explicit inputs, help,
  noninteractive modes and complete public Library lifecycle.
- Full gates: triggered by public composition, persisted optional ownership
  shape and guarded mutation changes. Historical exact predecessor has 10,559
  passes and 34 declared platform exclusions across six Windows modes under
  `artifacts/qualification/configure-format-final/`; it is a starting baseline,
  not qualification for Task 74.
- Protected: unrelated onboarding/wizard changes, paused/deferred Tasks,
  publication hold, source contents, authored ignore rules and Install behavior.
  No commit, merge, push or publication is included in this execution boundary.
- Isolation: Root owns contracts, public docs, integration and acceptance in
  `C:/Users/Tedy/.codex/worktrees/onboarding-and-presets/open-forge` on
  `docs/onboarding-and-presets`. `/root/library_ignore_implementer` owns coherent
  production/tests in `C:/Users/Tedy/.codex/worktrees/library-gitignore/open-forge`
  on `feature/library-gitignore`, seeded from exact base
  `de54c1d32f37e25e7b2da9f699edc22b35664549` plus the protected predecessor delta.
  Its complete source baseline is `artifacts/task74/source-baseline.json`.
  Transfer only verified child differences against that baseline with an exact
  destination-before hash check. Runtime: `gpt-6.1-sol` / `xhigh`, role Brilliant
  Implementer. The read-only Library mutation-map helper has completed.
- Known recovery limit: an ignore-only Sync with several registrations may
  retain valid recovery evidence without unique Library repair attribution.
  Task 74 does not claim universal automatic repair or change that format.

Done: M1 accepted semantics, M2 coherent production and focused evidence, M3 aligned documentation/help/Skill, and M4 fresh review plus complete qualification. All 10,718 test cases passed across six modes; 34 declared platform exclusions were accepted. No review findings remain.
Now: Task 74 is locally complete and archived.
Next: Retain the exact qualified delta for integration under the current CLI ledger.
Blocker: None within Task 74.

## Final receipts

- Exact first intake: `artifacts/task74/integration-receipt.json`; final two-path title correction: `artifacts/task74/r3-title-integration-receipt.json`.
- Complete frozen-source gate: `artifacts/task74/qualification-receipt.json`, with source, executable, delivery-manifest and six report hashes.
- Focused implementation and Red/Green title evidence: `artifacts/task74/worker-evidence/`.
- Fresh whole-task review: `artifacts/task74/whole-task-review.md`; prose and actual-help confirmations: `artifacts/task74/writing-review-*.md`. The native Attach help exactly matches the reviewed capture.
- Public site build: `artifacts/task74/site-build.log`. Only the larger shipped Skill's numerical Status budget changed: `artifacts/task74/status-budget-approved.json`; four-case recheck passed.
- Final archive, formatting, navigation and Doctor comparison: `artifacts/task74/closeout-receipt.json`. Doctor's existing repository findings remain a separate baseline.
