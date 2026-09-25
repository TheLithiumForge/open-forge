---
open-forge:
  description: Design and implement the interaction system with keyboard selection, dependency marks, plan review before confirmation, and non-interactive equivalents
  tags: [Memory, CLI, Task, Plan, G4, Interaction, Contextual, Archived, Historical]
---

# 04 — Interaction system

> Read [00 — G4 conventions](00-conventions.md) first.

## Goal

Every command that needs a choice asks for it in a terminal with keyboard
selection, shows the plan before asking for confirmation, never asks outside
a terminal, and has a flag for every question so scripts and agents can
answer in advance. Multi-select shows dependencies with marks and a legend.
Cancelling always leaves the workspace unchanged and says so.

## Depends on / Blocks

- Depends on: [03](03-rendering-system.md), because prompts render through the
  same text primitives and plan review renders the `minimal` report.
- Blocks: 12, 13, 15, 21, 24, 25, 26, 29, 30, 31, 32, 35, 36, 37.

## What existed before G4

> Historical baseline. The interaction composition and prompt primitives below
> describe the pre-qualification implementation; the merged system is recorded
> in [G4 closeout](#g4-closeout).

`Shell/Interaction/CliInteractiveSession` writes a prompt to stderr and reads
one line. Thirteen sites use it (listed under References). None reads a key.
`extension install` accepts one ID or `all`; a typo re-prompts silently.
`extension remove` accepts a space-separated list. `repair` asks one question
per proposal with typed words. `install` and `update` ask `[y/N]` before the
plan is shown. Permission prompts print the paths and ask for one of three
words.

## Capability

The host observes three facts and passes them into Core; Core never reads
`Console` itself:

| Fact          | How the host reads it                                                   | Effect                                                          |
| ------------- | ----------------------------------------------------------------------- | --------------------------------------------------------------- |
| `CanPrompt`   | stdin and stderr are not redirected                                     | Any prompt is allowed.                                          |
| `CanReadKeys` | `CanPrompt` and `TERM` is not `dumb` and the console supports key reads | Arrow, space and escape selection is used.                      |
| `CanRedraw`   | `CanReadKeys` and the console accepts ANSI cursor movement              | Lists redraw in place; otherwise each change reprints the list. |

When `CanPrompt` is false the command reports the shared family message for
the missing answer (`confirmation-required`, `selection-required`,
`permission-required`) with the flag that supplies it. `--format json` and
`--automatic` never prompt.

`Shell/Interaction/CliTerminal` replaces `CliInteractiveSession`: it exposes
`CanPrompt`, `CanReadKeys`, `CanRedraw`, `WriteAsync` (stderr),
`ReadKeyAsync`, `ReadLineAsync`. Tests supply a scripted terminal.

## Primitives

All prompt rendering lives in `Presentation/Shared/Prompts/`. Each primitive
has a key mode and a line mode (when keys cannot be read), identical
semantics, and a flag equivalent.

### Confirm

```text
Apply these changes? [y/N]
```

Keys: `y` yes, `n` or Enter no, Esc cancel. Line mode: `y`, `yes`, `n`, `no`,
empty is no. No and cancel both produce `cancelled` with `Nothing was
changed.` Flag: `--automatic`.

### Select (one of several)

```text
memory/notes matches 2 sources. Which one?

  > .agents/memory/notes.md
    .agents/memory/notes/_notes.md

  up/down: move   enter: choose   esc: cancel
```

Keys: up and down move, Enter chooses, a digit chooses that row, Esc cancels.
Line mode: numbered rows and `Choose a number (1-2), or press Enter to cancel:`.
Flag: the exact path or ID on the command line.

### Multi-select with dependencies

```text
Which Extensions do you want to install?

  [x] development           Debugging and review workflows
  [ ] development-toolkit   Documents, memory starters, planning        needs: memory-starters, planning, project-documents, development
  [+] memory-starters       Starter memory documents                    required by development-toolkit
  [ ] orchestration         Coordinate dependent tasks                  needs: planning
  [ ] planning              Planning workflow and templates
  [ ] project-documents     Vision and architecture workflows

  [x] chosen   [+] required by a chosen package   [ ] not chosen
  space: toggle   a: all   n: none   up/down: move   enter: continue   esc: cancel
```

Rules:

- Space toggles the row under the cursor. Choosing a package marks every
  package it needs, directly or through others, as `[+]`.
- A `[+]` row cannot be toggled off while a package that needs it is chosen.
  Trying prints one line under the list: `memory-starters is required by
development-toolkit. Unchoose that first.` The line clears on the next key.
- `a` chooses every row. `n` clears every row.
- Enter with nothing chosen prints `Choose at least one package, or press
esc to cancel.` and stays.
- The `needs:` column lists direct dependencies. `required by` names the
  chosen packages that pull the row in.
- Already installed packages, when the list is "what to install", are shown
  dimmed with `installed` in the last column and cannot be chosen.
- Line mode: numbered rows with the same marks; the prompt is `Choose numbers
separated by spaces, "all", or press Enter to cancel:`. Dependencies are
  added the same way and echoed: `Also installing memory-starters, required by
development-toolkit.`
- Flag: IDs on the command line, or `--all`.

The same primitive serves `extension update` (installed packages) and
`extension remove` (installed packages, where the `needs:` column becomes
`needed by:` and choosing a package that others need prints `orchestration is
needed by development-toolkit. Remove both?` with the dependent auto-marked
`[+]`).

### Text input

```text
Extension ID (lowercase, digits and hyphens): my-tools
```

Line mode only. A rejected value reprints the rule on the next line and asks
again: `'My Tools' is not a valid ID. Use lowercase letters, digits and
hyphens.` Empty input on a required value asks once more, then cancels. Flag:
the operand or option.

### Permission

```text
memory-starters writes outside .agents:

  templates/memory   (directory: everything under it)

  > Allow always   save these paths to .agents/open-forge.json
    Allow once     this run only
    Cancel

  up/down: move   enter: choose   esc: cancel
```

Select primitive with three fixed rows. Line mode: `always`, `once`, `cancel`.
Flag: `--allow-path <path>` (always).

### Plan review

Before any confirmation, the command renders its dry-run report at `minimal`
(the same report the `--dry-run` flag prints, through the same renderer) to
stderr, then asks. The report is never printed twice: after confirmation the
apply result is the only stdout output.

```text
Would install the Open Forge Framework into D:/work/myrepo.
  Would create 21 files and 20 directories under .agents, plus AGENTS.md and CLAUDE.md.
  Nothing that already exists would be changed.

Apply these changes? [y/N]
```

## Flows per command

| Command             | Questions, in order                                                                                                                                                                        | Flags that answer                                        |
| ------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | -------------------------------------------------------- |
| `install`           | Plan review, Confirm. When existing files are in the way: the plan names them and the Confirm reads `Replace the 2 existing files listed above? [y/N]`.                                    | `--automatic`, `--force`                                 |
| `update`            | Plan review, Confirm. Retired files are listed as kept; prune authority is never granted by a prompt.                                                                                      | `--automatic`, `--force`, `--prune`                      |
| `extension install` | Multi-select when no IDs and the source has several packages; Permission when paths outside `.agents`; existing-file Confirm (`--force`); Plan review; Confirm.                            | IDs or `--all`, `--allow-path`, `--force`, `--automatic` |
| `extension update`  | Multi-select of installed packages when no IDs; Permission; Plan review; Confirm.                                                                                                          | IDs or `--all`, `--allow-path`, `--automatic`            |
| `extension remove`  | Multi-select of installed packages when no IDs, with `needed by`; Plan review listing every deletion; Confirm reads `Delete the 3 files listed above? [y/N]`.                              | IDs, `--automatic`                                       |
| `extension create`  | Text input for the ID when missing; Text input for the package folder when missing; Plan review; Confirm.                                                                                  | operand, `--path`, `--automatic`                         |
| `repair`            | Summary line; Confirm `Apply the 6 repairs that are safe? [y/N]`; then for each link that needs a choice, Select among possible targets with `skip` as the last row; Plan review; Confirm. | `--automatic`, `--relink ...`                            |
| `route inspect`     | Select when an ID matches several files.                                                                                                                                                   | exact path                                               |
| `route update`      | Select when an ID matches several files.                                                                                                                                                   | exact path                                               |
| `route move`        | Select when the source ID matches several files.                                                                                                                                           | exact path                                               |
| `route remove`      | Select when the source ID matches several files; Plan review listing every deletion; Confirm.                                                                                              | exact path, `--automatic`                                |
| `library attach`    | Permission when the destination is outside `.agents`; Plan review; Confirm.                                                                                                                | `--allow-path`, `--automatic`                            |
| `library sync`      | Permission; Plan review; Confirm.                                                                                                                                                          | `--allow-path`, `--automatic`                            |
| `library detach`    | Permission (for link removal outside `.agents`); Plan review; Confirm.                                                                                                                     | `--allow-path`, `--automatic`                            |
| `cleanup`           | No prompt. Running the command is the explicit intent, as its contract says. Use `--dry-run` to see the list first.                                                                        | `--dry-run`                                              |

`route remove`, `library attach`, `library sync` and `library detach` gain
`--automatic` for scripts if they do not have it; record the addition in the
ledger. Read-only commands never confirm.

## Cancel semantics

Esc, Ctrl+C, end of input, or `no` at any question produces the `cancelled`
status with the sentence `<Command> was cancelled. Nothing was changed.` The
exit is 130. When cancellation arrives after effects started, the partial
template applies: `Stopped after <n> of <m> changes.` with the recovery path.

## References

- `src/cli/core/OpenForge.Cli.Core/Shell/Interaction/CliInteractiveSession.cs` — replaced by `CliTerminal`.
- `src/cli/root/OpenForge.Cli/Composition/CliCompositionRoot.cs` (`CreateInteractiveSession`) — capability construction.
- Prompt sites: `Commands/Install/InstallOperation.cs:195`, `Commands/Update/UpdateOperation.cs:98`, `Commands/Extension/Install/Shared/Planning/ExtensionInstallSelectionResolver.cs:115`, `.../ExtensionInstallTargetInspector.cs:227`, `Commands/Extension/Remove/Shared/Planning/ExtensionRemoveSelectionResolver.cs:43`, `Commands/Extension/Create/Shared/Resolution/ExtensionCreateRequestResolver.cs:126,149`, `Commands/Extension/Shared/Permissions/ExtensionPermissionOperation.cs:48`, `Commands/Library/Shared/Permissions/LibraryPermissionOperation.cs:42`, `Commands/Repair/Shared/Interaction/RepairWizard.cs:24,94`, `.../RepairLibraryRecoveryWizard.cs:29`, `Commands/Route/Inspect/Shared/Interaction/RouteInspectInteractiveSourceSelector.cs:42`, and the Route Update, Move and Remove subject selectors.
- Dependency facts: the Extension source's package manifests (`dependencies`).
- Contract text to record in the ledger: the shared operation contract's "Guided Leaves" section, each command's interaction paragraphs, `docs/cli.md`.

## Preconditions

- [ ] 03 merged.

## Steps

1. [x] Add `CliTerminal` with the three capabilities and a scripted test
       double. Host detection in `CliHost`. Verify: unit tests for each
       capability combination.
2. [x] Add the five primitives under `Presentation/Shared/Prompts/` with key
       mode, line mode and redraw fallback. Verify: unit tests with scripted
       keys for every rule in this file, including dependency marks, the
       refusal line, `a`, `n`, empty Enter, Esc.
3. [x] Replace each prompt site with the matching primitive, in the flow order
       above, rendering plan review through the `minimal` report. Verify:
       integration tests per flow with scripted answers, asserting effects,
       status and stream.
4. [x] Add `--automatic` where the table requires it. Verify: parser tests.
5. [x] Delete `CliInteractiveSession`, the typed-word loops, and the
       `interactive-wizard` selection kinds; rename remaining `wizard`
       identifiers to `prompt`. Verify: `git grep -i wizard src/cli` returns none.

## Expected result

Every prompt in the table works with keys in a terminal, with lines in a
terminal that cannot read keys, and not at all elsewhere, where the command
names the flag instead.

## Acceptance

- [x] Every flow has an integration test with scripted answers for choose,
      cancel and end-of-input.
- [x] No prompt is issued when stdin or stderr is redirected, under
      `--format json`, or under `--automatic`.
- [x] Plan review appears before every confirmation and is not repeated after it.

## Changes ledger

> This ledger retains the staged family receipts and preparation findings. Any
> statement that a family, legacy retirement, or permission change was still
> pending was true at that receipt boundary and is superseded by [G4
> closeout](#g4-closeout).

- Execution baseline: 03's accepted source is integrated in the same task
  worktree and preserved independently as staged Git tree
  `90968dcbf4a29e3f2da3b8b013d7ce344f75297c`. All six managed/native modes passed
  before interaction implementation began. Keep 04 changes outside that staged
  checkpoint so the rendering and interaction changes remain separately
  reviewable and committable. The Git integration marker above is recorded
  separately from this source qualification.
- Foundation implementation ownership is Luna/max with root review and
  coordination. The unstaged 04 source adds the neutral `CliTerminal` and
  typed interaction models, five shared prompt primitives, raw-value wording
  factories, cycle-safe multi-selection closure, shared report selection, and
  the neutral `CliPlanConfirmation<TResult,TQuestion>` delegate plus the
  Presentation `PlanConfirmation<TResult,TData,TQuestion>` factory over an
  existing concrete minimal report. Core prompt code checks policy and physical
  capabilities before any terminal I/O, preserves authored text spans, and
  keeps line and key answers semantically equivalent.
- The bounded foundation repair keeps all public interaction signatures stable:
  primitive execution now passes non-capturing typed operation delegates through
  the central policy/cancellation guard, and permission key selection reuses
  the shared cursor, redraw, digit, Enter, and Escape core with an explicit
  empty-heading frame. Focused evidence covers every primitive's policy and
  physical-capability no-I/O guard, key/line parity, redraw fallback, required
  dependency refusal clearing, `a`/`n`, select navigation and digits,
  permission line and digit choices including EOF/cancel, and plan preview
  ordering with exactly one confirmation read.
- The final shared-foundation gate completed after that repair: the Release
  build had zero warnings and zero errors, and the focused Unit 37/37 and
  Integration 16/16 runs had no failures, skips, or suite errors. This is
  foundation evidence only and does not qualify whole-04 acceptance.
- F04-10 keeps property-only prompt and test data models in their nearest
  `Prompts/Models` paths, while line-only select rendering uses the direct
  `SelectLine` wording after the key-mode early return with invariant
  interpolated formatting.
- The final Build6/focused3 gate confirms the shared foundation with zero
  warnings, errors, failures, skips, or suite errors: Unit 37/37 and
  Integration 16/16 passed. F04-07 through F04-10 are closed; this remains
  shared-foundation evidence rather than whole-04 acceptance.
- Five isolated family branches are based on `feature/render-improvements`
  at `5c388e7c5747a8a229591807f1b3693e4dd7b14e` plus the common staged
  foundation tree `0598e95e4ac2c741045de7ed918b1cd6a5c4a184`. Family changes
  remain unstaged and separately owned under root review and coordination.
  The operator receipt confirms the ten-file shared source delta across 50
  copies with raw-hash verification PASS; foundation tree `0598e95e4ac2c741045de7ed918b1cd6a5c4a184`
  and main index `90968dcbf4a29e3f2da3b8b013d7ce344f75297c` remain preserved.
- The host adapter owns one process-lifetime line-read task. Its invocation is
  moved to a worker so a synchronous `Console.In` wrapper cannot block the
  command waiter; cancellation only cancels that waiter, completed answers stay
  retained until consumed, and key reads are rejected while the line read is
  pending or retained. Host capability detection is monotonic and conservative:
  key reads require a supported non-`dumb` terminal and redraw requires a known
  ANSI host policy without native interop.
- Route and Library composition now receive `CliInteractionComposition` while
  preserving their existing `CliInteractiveSession` calls through
  `interaction.LegacySession`; command-family flows and old-session deletion
  remain out of this foundation change. Shared scripted terminal support and
  focused Unit/Integration cases cover capability denial, cancellation/EOF,
  line/key prompts, dependency cycles and disabled rows, text invariants,
  permissions, plan review, and retained host reads. Root must run the exact
  focused tests after each source freeze. The initial build3 gate completed
  with zero warnings and zero errors and recorded Unit 28 / Integration 16;
  that is an initial gate result, not whole-04 acceptance.
- Current main and the 05 branch descend from
  `feature/render-improvements` at `5c388e7c5747a8a229591807f1b3693e4dd7b14e`;
  all future branches base it and merge back there before the eventual G4
  squash to `develop`. Family migrations must preserve the exact cancellation
  sentence `<Command> was cancelled. Nothing was changed.` before effects and
  the existing partial/recovery wording after effects. Route Move/Remove
  selection removes only the blanket logical-identity uniqueness guard for the
  exact physical subject; alias, reference, topology, ownership, and
  revalidation checks remain. Task31's prune wording specializes the existing
  single final confirmation under `--prune` and grants no extra question or
  authority.
- The Install/Update family source is accepted for integration from its
  isolated branch, based on foundation tree
  `0598e95e4ac2c741045de7ed918b1cd6a5c4a184`. The bounded transfer contains
  36 family paths (26 modified and 10 new), with the ten already-propagated
  shared foundation files excluded. The artifact patch and blob receipt are
  recorded under `artifacts/g4-04-install-update-integration-1`; all 36
  family blobs and all 10 excluded shared blobs match their child sources.
  Child index `0598e95e4ac2c741045de7ed918b1cd6a5c4a184` and main index
  `90968dcbf4a29e3f2da3b8b013d7ce344f75297c` remain preserved. The family
  gate recorded Build4 with zero warnings/errors, Unit 25/25 and Integration
  149/149 with zero skips/errors, plus the reviewed final unit assertion.
  This qualifies the Install/Update family source only, not whole-04
  acceptance; its delta already includes the four global Install/Update
  fixture migrations identified during integration preparation.
- Install/Update confirmation wording: the direct prompts `Apply this Install
  plan? [y/N] ` and `Apply this Update plan? [y/N] ` are now the shared sentence
  `Apply these changes? [y/N]`. Install replacement uses `Replace the <N>
  existing file listed above? [y/N]` or its plural form, counting distinct
  replacement target files and excluding the ownership record. Update's prune
  deletion question uses `Delete the <N> file listed above? [y/N]` or its
  plural form, counting distinct physical delete paths and excluding lifecycle
  slots. Refusal, end of input, Escape and Ctrl+C before effects report
  `Install was cancelled. Nothing was changed.` or `Update was cancelled.
  Nothing was changed.`, exit 130, empty stdout, and no workspace effects.
- Install/Update plan review now renders the already-built minimal result
  through the same report renderer to stderr before confirmation; after an
  accepted answer, only the apply result is rendered to stdout. Dry-run and
  verified no-op paths do not prompt, and plan review does not rerun the
  operation. Their unavailable capability path reports the exact causes
  `Install requires explicit automatic mode when interactive confirmation is
  unavailable.` and `Update requires explicit automatic mode when interactive
  confirmation is unavailable.` with confirmation-required / invalid input,
  exit 4, and no prompt I/O or effects. `--automatic` bypasses confirmation
  while retaining the request flags; it grants neither Install `--force` nor
  Update `--force`/`--prune` authority.
- Install/Update operation construction changed from `CliInteractiveSession`
  to `CliPlanConfirmation<InstallResult, InstallConfirmationFacts>` and
  `CliPlanConfirmation<UpdateResult, UpdateConfirmationFacts>`. Install's
  `InstallConfirmationFacts.ReplacementCount` is distinct replacement-target
  count. Update's `UpdateConfirmationFacts.DeletionCount` is distinct planned
  physical-delete count and only reflects existing `--prune` authority.
  Install confirmation receives `InstallResultFactsFactory.DryRun(plan)`;
  Update confirmation receives `UpdateResult.ForPlanReview()`, a pure
  `Mode.DryRun` projection retaining source, comparisons, navigation, effects,
  lifecycle, recovery, verification, findings and Force/Prune/Automatic values.
- Install/Update integration tests moved from disconnected reader/writer
  assertions to real scripted-terminal composition for accepted, refusal and
  end-of-input paths, with preview ordering, remaining input, hashes, lock
  state and revalidation boundaries retained. Against the frozen 03 baseline,
  the exact 04 snapshot delta is eight cancellation cause-string substitutions:
  `Install confirmation was refused or reached end of input.` -> `Install was
  cancelled. Nothing was changed.` and the corresponding Update sentence,
  across the four Install and four Update cancellation snapshots. The 03
  prerequisite owns the schema/envelope and final-newline migrations; no other
  04 snapshot bytes changed.
- `doc:` propagation must replace the Install/Update `CliInteractiveSession`
  and old command-rendering references with the neutral Shell delegate, typed
  facts, and `Presentation/Install`/`Presentation/Update` wording. It must
  also correct the Update help and `docs/cli.md:438`: normal Update replaces
  changed owned content and restores missing owned content, while retired
  content is preserved unless `--prune`; `--automatic` remains independent.
- The Repair family source is accepted and integrated from its isolated branch,
  based on foundation tree `0598e95e4ac2c741045de7ed918b1cd6a5c4a184`. The
  bounded transfer contains 46 family paths (33 modified, 4 deleted, and 9
  added), with the ten already-propagated shared foundation files excluded.
  The prepared patch passed a fresh check and was applied to the main worktree
  without changing its normal index. All 45 non-composer path blobs and
  deletions match the child receipt, and the ten excluded shared blobs match
  the saved main receipt. The combined main `CliStandaloneComposer.cs` blob is
  `8c154b058c0604366733b7d4837ce007aedb0807`, distinct from the child
  `2cbf5f01caf2875f2165ed17c7c9a3458b6a1151`; it retains the accepted
  Install/Update wiring alongside the exact Repair interaction wiring. The
  artifact patch, path manifest, blob receipt, and preserved-index receipt are
  recorded under `artifacts/g4-04-repair-integration-prep-1`.
  Repair's snapshot audit found 11 exact changed paths with the four ordered
  substitution counts `4/4/3/3` and no missing or unexpected bytes. Child
  index `0598e95e4ac2c741045de7ed918b1cd6a5c4a184` and main index
  `90968dcbf4a29e3f2da3b8b013d7ce344f75297c` remain preserved. The accepted
  Repair family gate recorded Unit 127/127 and Integration 79/79 with zero
  skips or suite errors. This qualifies the Repair family source only, not
  whole-04 acceptance; global legacy retirement remains pending the remaining
  family integrations.

- The Library family source is accepted and integrated from its isolated
  branch, based on foundation tree `0598e95e4ac2c741045de7ed918b1cd6a5c4a184`.
  The bounded transfer contains 64 family paths (59 modified and 5 added),
  with the ten already-propagated shared foundation files excluded. The fresh
  patch check passed before application; all 64 child blobs match the prepared
  receipt, all 10 excluded shared blobs remain equal to the saved main
  receipt, and the main `CliStandaloneComposer.cs` retains the accepted
  Install/Update and Repair wiring outside this Library patch. The artifact
  patch, path manifest, blob receipt, and preserved-index receipt are recorded
  under `artifacts/g4-04-library-integration-prep-1`. Child index
  `0598e95e4ac2c741045de7ed918b1cd6a5c4a184` and main index
  `90968dcbf4a29e3f2da3b8b013d7ce344f75297c` remain preserved. Library's
  snapshot audit found 12 exact changed paths (Attach/Sync/Detach times four
  variants), with ordered rule counts `4/4/4` and no other snapshot path or
  byte changes. The accepted Library family gate recorded Unit 391/391 and
  Integration 288 passed with 7 accepted Windows skips out of 295 and no other
  errors. This qualifies the Library family source only, not whole-04
  acceptance; global legacy retirement remains pending the remaining family
  integrations.
- Library Attach/Sync/Detach request/help surfaces: no `Automatic` member or
  `--automatic` flag -> default-false `Automatic` request member, bound option,
  and matching help syntax. Propagate the three flags and request fields to
  the command contracts and `docs/cli.md` in 41.
- Missing final confirmation/default approval -> typed `Unavailable` and
  command findings `library-attach.confirmation-required`,
  `library-sync.confirmation-required`, or `library-detach.confirmation-required`;
  result `Invalid`/`invalid-input`, exit 4, and `Next` points to
  `--automatic`. Propagate codes, status, exit, and Next text in 41.
- Library permission or final-confirmation no/decline/EOF/Escape/Ctrl+C ->
  `cancelled`, exit 130, and exact `<Command> was cancelled. Nothing was
  changed.` before effects; missing permission remains blocked exit 5 and
  post-effect interruption retains partial/recovery wording. Propagate Library
  scenarios and transcripts in 41.
- Raw Library permission prompt with leaf rows, `future`/`descendants` prose,
  and `[a]`/`[o]` aliases -> typed scoped file/directory rows via
  `CliPrompts.PermissionAsync`, including `directory: everything under it`;
  nested leaf paths are omitted. Propagate scoped wording and retired aliases
  in 41.
- Early settings writes during Library planning -> explicit grants and
  interactive `Always` grants merged into one staged
  `WorkspaceSettingsChangePlanner.PlanGrant`, published only after final
  confirmation under lease/revalidation/recovery; unknown JSON, exact
  file/directory scopes, and every explicit grant (including empty targets)
  remain intact. Propagate timing and evidence in 41.
- Library permission tests constructing `CliInteractiveSession` and expecting
  abbreviated answers -> real scripted `CliPrompts.PermissionAsync` and typed
  `PlanConfirmation`; `[a]`/`[o]` and nested-leaf assertions were removed,
  while no-content grants, final confirmation, cancel/EOF, and no-prompt cases
  were added. Propagate test contract and deletion notes in 41.
- Library interaction ownership moved from
  `Commands/Library/Shared/Permissions/LibraryPermissionPresentation.RenderPrompt`
  to shared `Presentation/Shared/Prompts/CliPrompts.PermissionAsync` plus
  typed `Presentation/Library/{Attach,Sync,Detach}/Shared/Interaction/*`;
  legacy report/help bridges remain under
  `Presentation/Legacy/Library/{Attach,Sync,Detach}/Shared/Rendering/` and
  carry the `--automatic` help wording. Propagate these named paths in 41.
- Authorized 04 snapshot delta: the `attached-outside-with-flag` compact and expanded text/JSON variants now report `Permissions: granted; action create; outcome verified`; only the permission `action`/`outcome` fields changed from `none`/`not-requested`. No other snapshot fields changed.
- Authorized 04 snapshot delta: the `Synchronize/cancelled` compact and expanded text/JSON variants now report `Library sync was cancelled. Nothing was changed.` for the cancellation finding; summary headline, status, effects, and counts are unchanged.
- Authorized 04 snapshot delta: the `cancelled` compact and expanded text/JSON variants now report `Library detach was cancelled. Nothing was changed.` for the cancellation finding; summary headline, status, effects, and counts are unchanged.

- The Route family source is accepted and integrated from its isolated branch,
  based on foundation tree `0598e95e4ac2c741045de7ed918b1cd6a5c4a184`.
  Prep-2 transferred 75 reviewed Route paths (22 added, 51 modified, and 2
  deleted) while excluding the ten frozen shared-foundation files. The patch
  SHA-256 is `5a64ad153d8b21ea3cd958d53a4cef1e5f6d965d0f02852f7a8fac4bc30b3478`;
  fresh check and plain apply passed with the normal index preserved. The
  transferred blobs and deletions match the prep receipt except for the one
  accepted 05 overlap in `RouteMoveReferenceScanner.cs`, where the shared
  `EntriesBlock` boundary supersedes the Route prep's earlier `ContentSpan`
  line. The final Route namespace gate passed Unit 36/36 and Integration
  51/51 with no skips; the earlier full family gates passed Unit 584/584 and
  Integration 621/621 without snapshot updates. This qualifies Route source
  only, not whole-04 acceptance.
- Route's accepted interaction behavior uses neutral source-selection replies
  for Inspect, Update, Move, and Remove, preserves exact physical selection
  through planning and revalidation, keeps JSON/redirected/nonterminal paths
  prompt-free, and uses the exact lower-case cancellation sentence before
  effects. Remove renders its retained plan before one count-aware final
  confirmation and returns confirmation-required Invalid/exit 4 when
  interactive authority is unavailable. Move/Remove remove only the blanket
  logical-identity uniqueness rejection after exact physical selection;
  alias, reference, topology, ownership, category, destination, and
  revalidation guards remain active. The Route ledger receipt is
  `route04-changes-divergences.md` under the prep-2 artifact.
- The Extension family source is accepted and integrated from its isolated
  branch, based on the same foundation tree. Prep-1 transferred 132 reviewed
  Extension paths (110 modified and 22 added), excluding the ten frozen
  shared-foundation files. The patch SHA-256 is
  `c9cc1a965729d3d6760fddde071006fee6f07a370663ac0109909f5485c9340e`;
  fresh check and plain apply passed, and all 132 expected child blobs match
  the integrated main worktree. `CliExtensionComposer.cs` is part of this
  accepted family patch. The required Library permission consumer is a
  separate bounded main reconciliation: it now uses
  `LibraryPermissionTestPrompt.Create` for both Extension permission seams
  and has no legacy-session import. The accepted Extension gate recorded
  Unit 265/265 and Integration 403 passed with 2 expected Unix-permission
  skips, plus 27 composed/direct interaction cases and 57 reviewed snapshot
  captures. This qualifies Extension source only, not whole-04 acceptance.
- Extension's accepted flow keeps explicit Automatic/Force/Prune authority,
  typed selection and permission replies, staged `--allow-path` grants, one
  retained plan review before each confirmation, and the exact lower-case
  command cancellation sentence before effects. Unavailable confirmation is
  the command-specific confirmation-required Invalid/exit 4 result; decline,
  EOF, Escape, and Ctrl+C remain cancellation/exit 130. Installed or
  disabled multi-package candidates produce SelectionRequired when no
  explicit IDs or `--all` are supplied, without opening an impossible prompt.
  The corrected Extension receipt is
  `extension-04-changes-divergences.md` under the prep-1 artifact.
- Final legacy cleanup: `CliInteractionComposition` now carries only the
  concrete `CliTerminal` and typed `CliPrompts`; root composition no longer
  constructs `CliInteractiveSession`, and the session/response types,
  dedicated session test, `WorkspacePermissionPrompt`, and
  `WorkspacePermissionApprovalChoice` were removed after zero-source-reference
  confirmation. The obsolete Library `ReadEffectName` mapper and its two
  focused tests were removed while `ReadEffect`, permission evaluation,
  result/options, JSON projection, and receipts remain. The 15 remaining
  test-only `wizard` hits across six files were renamed to `prompt`; the Route
  help test's obsolete blanket `wizard` absence assertion was removed. No CLI
  output or snapshot bytes changed.
- Eight pre-existing published process cases now carry explicit apply
  authority after the interaction migration: Route Remove's successful leaf
  and category applications use `--automatic`, and Library SharedGrant's
  Attach setup for Sync/Detach plus its apply-mode grant cases use
  `--automatic`. Library dry-run cases remain `--dry-run` without
  `--automatic`; they now assert completed/exit 0 and planned permission
  metadata (`data.permissions.action=replace` and
  `data.permissions.outcome=planned`) while retaining the no-write and exact
  settings assertions. Help, invalid-input and repeated verified no-op cases
  remain unchanged. This is a bounded process-test correction following the
  first failed managed EndToEnd attempt; at that preparation boundary, the
  combined 04 qualification gate remained pending.
- Final qualification attempt 2: `npm run build:native -- --offline` exited 0;
  managed Unit 3520/3520, managed Integration 2304 passed with 17 accepted
  Windows skips, managed EndToEnd 163/163, native Integration 2304 passed with
  the same 17 accepted Windows skips, native EndToEnd 163/163, and managed
  EndToEnd against the native CLI 163/163. Every report had zero failures,
  pending, other results, and suite errors; the 17 Integration skip names and
  reasons matched the accepted G4-03 evidence exactly. Before and after source
  identity matched (`sha=e7415ceb9876635de6c0ccd53f60658bb0d9965b`,
  `dirty=true`, `changes=4269886913bc2ab7c4ce74d6bc16ccfb9eda7ce66059fcebc4022110b6f8f705`).
  Receipt: `artifacts/g4-04-six-mode-evidence-2.json`.

## Divergences observed

- Repair's existing Library recovery prompt is absent from the flow table.
  Preserve it as a separate selection with skip, after link choices and before
  final plan review. Keep the existing single-residual limit and automatic
  semantics; the safe-reference confirmation does not authorize every Library
  recovery proposal.
- The Framework Install catalogue in 12 explicitly limits the replacement
  question to plans already authorized with `--force`. Its occupied-target
  planner therefore retains the existing force requirement; no prompt grants
  that authority. This corrects the earlier preparation interpretation of the
  shorter flow table. Extension Install already has an existing-file approval
  prompt before effect planning. Preserve its exact eligible proposed paths
  and bytes as pending approval, preview that scope, and carry accepted
  authority in the frozen execution plan. Never fabricate Force or Automatic
  on the live request, expand eligible targets, or rerun an operation to render
  its preview. Unsafe and owned targets remain blocked. Pure preview factories
  serve both dry-run and plan review over the same established planning facts.
- New final-confirmation flows use command-specific `confirmation-required`
  finding codes with the existing shared family. Confirmation unavailable is
  `invalid-input`, exit 4, consistent with Install and Update and 12's explicit
  status table. This includes `library-attach.confirmation-required`,
  `library-sync.confirmation-required` and `library-detach.confirmation-required`.
  Missing permission keeps its established classification; an actual declined
  or ended permission question is cancellation, exit 130. Record these new
  interaction codes beside the later command catalogues during propagation.
- The six Extension Install/Update/Remove and Library Attach/Sync/Detach
  paths currently persist explicit `--allow-path` grants during apply-mode
  planning, before permission resolution and the new final confirmation.
  Interactive Allow always already stages a settings change correctly.
  Stage explicit grants through that same permission plan, evaluate required
  destinations against the prospective grants in memory, and publish only
  during confirmed application under the existing lease, revalidation and
  recovery. This is required to keep cancellation before application free of
  workspace changes. Preserve unknown settings, exact file/directory grant
  scopes and all explicit grants, including when no content permission is
  missing. The flag remains authority in automatic and noninteractive runs.
  Documentation propagation must replace the former before-proceeding
  persistence timing with the confirmed-application timing. This decision is
  accepted for implementation; no permission code has changed yet.
- Preparation found that Extension Update does not currently offer package
  selection: `ExtensionUpdatePlanner.BuildAsync` returns `SelectionRequired`
  for a multi-package source without IDs or `--all`, even when interaction is
  allowed. Its operation factory passes the session only to permissions. Add
  the specified selection flow here; the before snapshot must retain the
  current invalid result rather than pretend the prompt already exists.
- Extension Remove currently passes the original empty-ID request back into
  its planner during application revalidation and final verification. The
  planner asks for selection again, so one valid answer followed by end of
  input blocks before effects; after deletion, removed IDs are no longer
  offered by that selector. Resolve the user's choice once and carry the
  frozen IDs through revalidation and verification. These stages must validate
  the same chosen plan without asking again. Test the prompt count as well as
  effects and result; preserve the observed before behavior in 01.
- These are preparation findings only. No interaction implementation or
  acceptance result is claimed; 03 remains a prerequisite.
- The flow table places Extension Install's existing-file confirmation and
  Repair's safe-repair confirmation before their later complete-plan review,
  while acceptance requires a review before every confirmation. Each such
  confirmation must first render the already established plan for its exact
  scope at minimal detail. The later review still presents the complete plan
  after choices are resolved. This reconciles the two requirements without
  expanding the authority granted by either confirmation.
- Route Update, Move and Remove currently have no interactive source selector,
  and the three Library mutation request models have no Automatic member.
  These are new flows and flags, not replacements of existing prompt calls.
  Keep host capability observation and shared composition under one owner;
  command-family flows can proceed independently after the neutral terminal
  and prompt callable contracts are frozen. Command operations must consume
  those Shell contracts without importing Presentation.
- Plan-review composition must render the command's already established planned
  result through its concrete minimal report renderer to stderr. It must not
  rerun the operation or acquire additional mutation authority. Redraw support
  uses an explicit host policy and remains disabled when it cannot be
  established; no native interop probe is introduced.
- Install and Update's former prompt references at
  `Commands/Install/InstallOperation.cs:195` and
  `Commands/Update/UpdateOperation.cs:98` named `CliInteractiveSession`; those
  symbols no longer exist after the accepted family migration. The durable
  command references need propagation to the neutral Shell delegates and
  Presentation question factories; no product decision is pending.
- Update's catalogue sentence that normal mode “preserves changed” content was
  inconsistent with the accepted planner, which replaces changed current
  content and restores missing content without `--force`, while reserving
  retired deletion for `--prune`. Help and public documentation need the
  corrected wording during propagation; source behavior and focused planning
  tests agree.
- Documentation propagation required: after root selects these ledger entries,
  41 should carry the Library flag/help, status/exit, prompt, grant, test, and
  helper-path facts into the affected contracts and `docs/cli.md`; preserve
  00's authored wording and historical preparation notes unless a source
  change requires a surface update.

- First qualification attempt: the managed EndToEnd gate exposed eight process
  failures before the bounded test-only `--automatic` correction above. Attempt
  2 passed all six executable modes after that correction.
- Windows permission boundary: managed and native Integration each skipped the
  same 17 Unix-permission tests, with exact name and reason matches to accepted
  G4-03 evidence. Unit and all EndToEnd modes had zero skips.
- Git integration: implementation and qualification are complete in the
  reviewed worktree, but no commit or merge was made. Integration remains
  pending.

## G4 closeout

The planning divergences above retain the behavior and questions observed
before qualification. The merged interaction system now renders the
established minimal plan before every confirmation, stages explicit and
Allow-always grants, and publishes settings only during confirmed application
under lease, revalidation, and recovery. The Library confirmation-required
codes are `library-attach.confirmation-required`,
`library-sync.confirmation-required`, and
`library-detach.confirmation-required`; no maintainer question in the ledger is
resolved by this note.

## Rollback

Revert the branch; the primitives are additive until step 3.
