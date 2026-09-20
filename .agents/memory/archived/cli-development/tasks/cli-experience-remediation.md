---
open-forge:
  description: Task 30 remediation of the CLI experience audit, with its execution order, phase state, decisions taken, and the findings each item traces to
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Remediation, Audit, Planning]
---

# CLI Experience Remediation

## Task State

- State: Active. Phases 0 to 3 complete; **phase 3.5 run** (see
  [Task 31](implementation-duplication.md)); **phase 4a G1 in progress** — its
  three judgment decisions are taken and recorded under G1, and the lock-file
  mechanism is proposed and awaiting validation. Worked **change by change with
  the maintainer**, not in bulk.
- Authority: The user's 2026-09-10 instruction to finish phase 0 and start
  phase 1, the 2026-09-11 decisions recorded under phase 1, and the 2026-09-11
  G1 decisions recorded under that group.
- Responsible role: Root, direct sequential implementation.
- Task source: Task 30 "CLI Experience Remediation", registered in
  [CLI Development Tasks](_tasks.md).
- Evidence: unit suite at 18 pre-existing failures of 3365, none newly failing
  and diffed by failing test name; a throwaway characterization harness over 297
  captures across three seeded workspaces shows byte-identical command output
  across phase 1 and every phase-3.5 commit.
- Findings and reasoning stay in
  [CLI Experience Audit](../../../emerging/analysis/cli-experience-audit/_cli-experience-audit.md). This record owns the work.
- Why the defects exist, as opposed to what they are, is in
  [CLI Design Retrospective](../../../emerging/analysis/cli-design-retrospective/_cli-design-retrospective.md).
  Consult it before changing a contract rather than a behaviour.
- Last updated: 2026-09-11.

## Environment

- `npm run test` is the suite. `dotnet test --project ...` reports zero tests.
  Run the built assembly directly to filter:
  `./artifacts/bin/OpenForge.Cli.Core.UnitTests/release/OpenForge.Cli.Core.UnitTests.exe --filter-class "*Name*"`.
- `npm run test` stops at the first failing suite. Because the unit suite has 18
  pre-existing failures, the integration and end-to-end suites never run under
  it; run their assemblies directly.
- The freshly built CLI is at
  `artifacts/publish/open-forge-dev/Release/open-forge-dev.exe`. A globally
  linked `open-forge` may be stale.
- `npm run cli:link` needs `vswhere.exe` on `PATH`
  (`C:\Program Files (x86)\Microsoft Visual Studio\Installer`), and a stale
  global shim can make it fail with `EEXIST`.
- Five `dotnet format` whitespace errors are pre-existing, in untouched files.
- `doctor` on this repository reports one error, "the lifecycle document is
  missing", because the repository is the Framework source rather than an
  installed workspace. That is expected.
- Test playgrounds are outside the repository under
  `<workspace>\open-forge-test\`.

## Working Agreement

**Phases 4 and 5 are worked change by change.** The maintainer is to be consulted
on each command-output change, class rename, layer move, and finding-model
decision before it is applied. The analysis in this scope reaches conclusions;
**none of them is accepted until the maintainer accepts it.** Do not batch these
phases, and do not treat a recorded recommendation as authority to proceed.

Phases 0 to 3 were mechanical or structural and were worked in bulk with
after-the-fact review. That mode ends here: from G1 onward every change is
visible to a user.

## Open Questions For Phase 4

Raised on 2026-09-11. Each carries a recommendation; **none is accepted.**

### Should `--projection` include `tsv`?

**Recommend dropping it. Ship `text` and `json`.**

TSV is tab-separated values: tabular output for shell pipelines, where `cut`,
`awk` and `sort` work without `jq`. The case for it is real, and it does not
apply here yet:

- **Most Open Forge output is not tabular.** Findings nest, routes are a tree,
  Index shows diffs, Doctor groups by domain. Only `route list`, `find`,
  `library list` and `extension list` are naturally rows.
- **A third projection multiplies the work G4 is already doing.** Every command
  must implement it or refuse it, and refusing raises a new question about what a
  refusal looks like.
- **Adding it later is backward compatible.** `--projection` takes a value, so a
  new value breaks nothing. Dropping one after release does.
- **The actual goal is better served another way.** Pipe-friendliness comes from
  `text` being line-oriented and stable with data on stdout and diagnostics on
  stderr — which G4 already commits to. A stable `text` projection pipes fine.

If tabular output is wanted later, add `tsv` to the commands that are genuinely
rows, as its own small task with real consumers.

### What replaces "human" in user-facing prose?

**Recommend deleting the sentence rather than rewording it.**

Every command's help currently opens its results block with _"Human complete,
attention, and incomplete results use stdout; invalid, blocked, failed, and
interrupted results use stderr."_ then repeats the mapping as rows. The
maintainer's objection is that "human-readable" is not how a person writes, and
that is right — but the deeper problem is that the sentence is **the tool talking
about itself**, the same class of prose G4 already deletes 98 repetitions of
elsewhere.

The rows carry the whole meaning:

```text
complete: exit 0, stdout
failed: exit 1, stderr
```

No format adjective is needed, because the exit and stream mapping is identical
for `text` and `json`. So the word disappears from the product rather than being
replaced. `text` remains in the flag and in type names, where it is naming a
format rather than describing a reader.

This is one edit: the block is single-sourced through `CliResultHelp` since phase

1. It belongs with G6, which owns making output useful to a person and an agent.

### Where does the folder restructure go?

**Recommend phase 7, replacing the old folder collapse, and not before G4.**

The target shape is `Shell/`, `Framework/` and `Presentation/` as top folders,
each holding its modules and its own shared support — a tree that can be cut into
one C# project per layer without further movement.

It has to follow G4. G4 makes renderers _"dumb, mostly shared formatters"_ behind
the selection stage, so most of the 310 presentation files either disappear or
merge. Moving them first is moving files that are about to be deleted, which is
the same mistake the naming pass was pulled out of phase 3 to avoid.

It also **replaces** the old "collapse 168 one-file folders" item. A restructure
by layer decides every folder's home on purpose; collapsing by file count decides
it by arithmetic.

### Should the layers become separate C# projects?

**Recommend yes, as a separate task after phase 7 — and note what it retires.**

One project per layer, with project references expressing the dependency
direction, gives at compile time what the phase 3 boundary tests give at test
time. That is strictly better: a violation stops being a failing test and becomes
a build error, and the four tests can be deleted.

It also makes the module-level integration tests the maintainer described
straightforward, because a module becomes a real assembly that can be referenced
and exercised on its own.

It is a separate task because it touches the build, the AOT configuration, the
package graph and CI, none of which this scope covers. Phase 7 is its
prerequisite: the folders must already be layer-shaped.

### What the test layers mean, once modules exist

Recorded for G7 rather than decided here:

- **Unit** — one type or one narrow behaviour, no filesystem.
- **Integration** — one module end to end, at its own boundary. This is what
  "integration" should always have meant; the reason it drifted is that no module
  boundary was defined until phase 2. Does not need Native AOT.
- **End to end** — the built executable, black box, real workspace, exit codes and
  streams. This is where AOT matters.

A snapshot library the maintainer has written will be offered as a local NuGet
package. It is the mechanism for the permanent snapshot suite, and the suite
belongs at the **start of G4's rewrites, not in G7** — see phase 4b.

## Phase Progress

| #       | Phase                                               | State                                                                                                                              |
| ------- | --------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| **0**   | Ship-blockers                                       | **Complete** — `1fd29dbc`, `42e9e360`                                                                                              |
| **1**   | Deduplication                                       | **Complete** — `d0b46476`, `76da3127`, `ed4cab7a`                                                                                  |
| **2**   | Architecture layers                                 | **Complete** — `3013962d`, `39cbad6e`                                                                                              |
| **3**   | Layer adherence                                     | **Complete** — four boundary tests and a placement clause                                                                          |
| **3.5** | [Task 31](implementation-duplication.md) M1, M2, M4 | **M2, M4 complete; M1 partial** — `14b85c48`, `d0f8e70d`, `c459e878`, `59d17e45`. Route Move/Remove stopped at a contract boundary |
| **4a**  | G1 → G2 → G3                                        | **In progress.** G1 decisions taken 2026-09-11; lock mechanism proposed, awaiting validation                                       |
| **4b**  | G4, contract → snapshots → rewrites                 | Planned. Change by change. Carries the naming, the escaper, the flag model                                                         |
| **5**   | G5 → G6 → G7                                        | Planned. G6 is the output-wording task; G7 gains the module test definition                                                        |
| **6**   | Oversized file splits                               | Planned. Smaller after G4                                                                                                          |
| **7**   | Folder restructure by layer                         | Planned. Replaces the old folder collapse                                                                                          |
| **8**   | Project split                                       | Candidate, separate task. Enabled by 7; retires the boundary tests                                                                 |

Every task traces to a finding in this scope. Sizes are relative: **S** one file
or one rule, **M** one command or subsystem, **L** cross-cutting. **judgment**
marks work where a wrong decision compounds into everything after it — specify
those before any implementation starts.

Tasks are grouped by theme (**G1–G7**) because that is how they load into a
session. **Groups are not the execution order.** The order is below.

## Execution order

Reasoned in [layers-and-sequencing.md](../../../emerging/analysis/cli-experience-audit/layers-and-sequencing.md). Each phase is
one session or a short run of them.

| #       | Phase                           | Why here                                                                                                                                                        | Session       |
| ------- | ------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------- |
| **0**   | **Ship-blockers**               | Release-gating, user-visible, individually tiny, independent of everything below                                                                                | done          |
| **1**   | **Deduplication**               | Depends on nothing; both shared owners already existed                                                                                                          | done          |
| **2**   | **Architecture layers**         | Needed by G4's contract, and the artefact worth carrying forward                                                                                                | done          |
| **3**   | **Layer adherence**             | The boundary tests guard every phase after them, and depend on nothing                                                                                          | done          |
| **3.5** | **Task 31 M1, M2, M4**          | 1,400 duplicated lines deleted before G4 rewrites the same files. Behaviour-preserving, so it needs no maintainer review per change                             | one           |
| **4a**  | **G1 → G2 → G3**                | State model, structural leniency, diagnosis truth. G1's decisions delete work in G2 and G3, so it goes first and alone                                          | one per group |
| **4b**  | **G4**                          | The presentation contract, then the permanent snapshot suite, then the renderer rewrites — in that order. The suite must exist _before_ the rewrites, not after | several       |
| **5**   | **G5 → G6 → G7**                | Post-release. G6 rewrites the output wording; G7 defines the test layers against the modules                                                                    | one per group |
| **6**   | **Oversized file splits**       | Seams are clearer once the selection layer exists and renderers have shrunk                                                                                     | one           |
| **7**   | **Folder restructure by layer** | `Shell/`, `Framework/`, `Presentation/` as top folders, each with its modules and its own shared. Must follow G4, which deletes most of what would be moved     | one           |
| **8**   | **Project split**               | One C# project per layer. Compile-time boundaries beat a boundary test. A separate task, not part of this one                                                   | separate      |

**Every phase has a `Decisions to make` block. Answer it before the session
starts, not during it.** Each is one or two lines of intent, and each sets a
precedent that is expensive to reverse once code depends on it. A session that
opens with the decisions unanswered will either stall or default them silently.

The decisions themselves are recorded with their phase below, along with a
recommendation and the reasoning behind it. A recommendation is a starting
position, not an answer — the point of recording it is that disagreeing is
cheap and fast.

**Boundary to respect in phase 1:** do not deduplicate inside the lifecycle
baseline or permissions subsystems — G1 deletes them. Everything in the phase-1
targets is rendering, help and folder structure, none of which G1 removes.

**Why dedup precedes the architecture document:** it depends on nothing.
`WorkspaceSelectionWireVocabulary` and `CliResultHelp` already exist, so the work
is deletion against known owners. Only the _naming_ pass needs the layer names,
which is why the architecture document sits between them rather than first.

### Phase 0 — ship-blockers

Pulled from the groups below by release impact, not by theme. Each is small on
its own and none needs its group's full redesign first. Locations and test blast
radius were measured, not estimated.

| #   | Fix                                              | Location                                                            | Size | Status          |
| --- | ------------------------------------------------ | ------------------------------------------------------------------- | ---- | --------------- |
| 1   | `SKILL.md` strict keys                           | `SourceAuthoredMetadataParser.cs`                                   | XS   | **done**        |
| 2   | `repair` crash                                   | `RepairOperation.cs`, `RepairCandidateMapper.cs`                    | XS   | **done**        |
| 3   | Absolute `workspacePath`                         | `LifecycleDocumentValidator.cs`                                     | XS   | **done**        |
| 4   | The 66 malformed files                           | `.agents/memory/archived/cli-v2`                                    | XS   | **done**        |
| 5   | `region: "entries"` baseline                     | `FrameworkLifecycleCurrentnessReader.cs`                            | S    | **done**        |
| 6   | Frontmatter BOM and fences                       | `MarkdownFrontmatterParser.cs`                                      | S    | **done**        |
| 7   | Upward workspace discovery                       | `CliWorkspaceSelector.cs`                                           | S    | **done**        |
| 8   | External Extension silent no-op + manifest error | `ExtensionManifestReader.cs`, `ExtensionInstallOperationFactory.cs` | S    | **done**        |
| 9   | `references` message reads as "no links exist"   | `ReferencesLinkExtractor.cs`                                        | S    | **moved to G4** |
| 10  | `context` exits 2 on a pristine install          | `SourceFormClassifier.cs`, `ContextProjectionBuilder.cs`            | S    | **done**        |
| 11  | CI check that this repo can install itself       | one CI step                                                         | XS   | **moved to G7** |

#### What the completed items actually were

Two turned out different from the sizing, and both are worth recording.

**The `repair` crash was two bugs.** Adding `exception.Message` to the catch
revealed _"Repair candidate targets must be unique within one candidate set"_.
`RepairCandidateMapper` mapped each candidate **basis** to its own candidate, so
one file found by both a filename match and a route-neighborhood match produced
two candidates with identical targets, and the model invariant threw. Candidates
are now grouped by target with their evidence merged — which is also the correct
shape, since the same file found two ways is one suggestion with two reasons.

**The entries baseline was recorded from the wrong bytes.** The instrumented
message showed `entries: recorded bdc56097, current a513fce8` — and `a513fce8`
is the value a pristine install has on disk. `CreateGeneratedTarget` fingerprints
the _payload_ bytes, but the file written to the workspace has its Entries region
generated, so the baseline never matched what install itself wrote. Every
workspace was one route change away from an unusable Extension install.

The fix skips generated regions in the currentness comparison rather than
removing the target: `Framework.GeneratedRegions` validation requires a matching
target, so removing it breaks install. Deleting the entry belongs with the
lifecycle record redesign in G1.

**Also found while fixing 4:** five files in `memory/working/handoffs` use
root-level `tags:` with no `open-forge:` block and no description, so `index`
still reports `incomplete` on this repository — down from `blocked`. Reading root
`description` and `tags` is the recorded interop policy and belongs with that
task, not with a mechanical Phase 0 fix.

**The `context` exit was narrower than "add frontmatter".** The decision was that
an entry file is not a routed source and needs no route metadata, so nothing was
added to the payload. `SourceFormClassifier.IsEntryDocument` names `AGENTS.md`
and `.agents/loader.md`, and Context suppresses `context.frontmatter-missing`
for exactly those two. Every other source still reports: stripping the
frontmatter from `.agents/guidance/_guidance.md` still returns attention and
exit 2.

**`Effects: 0` is not on its own a defect.** A re-install of an already-current
Extension also reports zero effects, and that is correct — it prints
`Installation record: preserve; already-current` and lists the planned paths, so
it explains itself. The silent case is a _package that delivers no files at all_,
which is what a payload outside `content/` produces.
`extension-install.package-content-missing` fires on that condition alone, names
the package and the `content/` layout, and reports attention on both apply and
dry-run. The idempotent re-install is untouched and still completes at exit 0.

**The manifest error was fixed by validating before deserializing.** Rather than
parsing the .NET unmapped-member message, `ExtensionManifestReader` now walks the
root property names itself and reports
_"The manifest key 'author' is not accepted. Accepted keys: id, name,
description, version, dependencies."_ `extension install --help` gained an
**External package layout** section, so the two undocumented requirements are now
documented where a user looks.

#### Verification

Baseline before any change: **18 unit failures of 3,347** (pre-existing, mostly
CRLF and Library projections). After the first seven: **18 of 3,355**. After the
last two: **18 of 3,359**, with **0 newly failing** and 12 new cases added. Four tests asserted the defects being fixed and were
rewritten to assert the intended behaviour instead — the "tests enshrine the
defect" pattern this audit predicted, met in practice.

#### Decisions taken

All three are settled. Recorded so a later session does not reopen them.

- **Where does the walk-up stop?** The first ancestor containing `.agents`,
  otherwise the starting directory unchanged. An explicit `--workspace` is never
  redirected. **Implemented.**
- **Should `references` count links inside generated Entries?** **No.**
  `references` reports _authored_ references — links someone wrote. The generated
  `Entries` tree is what `route list` shows, and duplicating it here would make
  the two commands answer the same question. **The current exclusion is correct;
  the message is not.** Saying `coverage complete` and `No direct links found`
  reads as "this file has no links" when it means "nobody wrote a link here by
  hand". This is a wording fix, not a behaviour change — see G4.
- **Frontmatter on `AGENTS.md` and `loader.md`?** **No.** `AGENTS.md`,
  `CLAUDE.md` and their siblings are the user's files and shared with other
  tools; Open Forge keeps its footprint in them to an absolute minimum. So the
  _rule_ changes: an entry file is not a routed source and does not need route
  metadata. Nothing is added to the payload.

#### Suggested model

**One Opus session for the whole phase**, not a split.

The usual advice — decide with high reasoning, delegate the volume — does not
apply here because **there is no volume**. Ten items across ten files, four of
them one-line changes, three carrying a small precedent-setting decision, and one
requiring run-read-fix iteration against a live CLI. Handing that off costs more
in specification and hand-back than it saves, and the three decisions are exactly
the kind that are cheap to make in-flight and expensive to specify in advance.

Delegation becomes worthwhile from Phase 1 onward, where the work is repetitive
against known targets.

### Phase 1 — deduplication and folder collapse

**Complete.** Items 1–3 landed in `d0b46476`, with a cross-command sweep
following in `ed4cab7a`. Item 4 landed much reduced in `76da3127`, and its
remainder is postponed to phase 7. See **Decisions taken** below for why it
shrank from 168 folders to three.

| #   | Work                                      | Outcome                                                                                                                                                                                                                                |
| --- | ----------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | Duplicate `WorkspaceSelection` mappings   | **Done, and wider than scoped.** 12 wire copies went to `WorkspaceSelectionWireVocabulary.Read`; a further 3 human copies in `FindHumanValues`, `RouteInspectHumanValues` and `RouteListHumanValues` went to `CliHumanText.Selection`. |
| 2   | `*HelpSections` through `CliResultHelp`   | **Done for the 5 that had a duplicate block**: `Doctor`, `Find`, `RouteInspect`, `RouteList`, `RouteRemove`. The other 5 named were not duplicates — see below.                                                                        |
| 3   | Shared enum mappings out of `*Vocabulary` | **Done.** `OperationalValueState`, `OperationalLifecycleState` and `OperationalSourceAvailability` moved from `DoctorWireVocabulary` and `StatusWireVocabulary` to a new `OperationalWireVocabulary`.                                  |
| 4   | Collapse one-file folders                 | **3 of 169 collapsed; the rest postponed to phase 7.** The other 166 are shapes the directives require, or oversized files misread as microfolders.                                                                                    |

26 duplicate implementations were deleted in the phase itself: 12 wire
selection, 3 human selection, 6 operational state, and 5 results-and-streams help
blocks. The cross-command sweep that followed removed 13 more, for **39 in
total**.

**Item 2 was smaller than the audit measured.** Of the 10 `*HelpSections` named,
only 5 carried a private copy of the shared block. `ExtensionHelpSections`,
`LibraryHelpSections` and `RouteHelpSections` are group help with a single
"Command help" line and no result to describe; `CleanupHelpSections` and
`RouteCreateHelpSections` have no results section at all, so routing them through
`CliResultHelp` would _add_ a section rather than deduplicate one. That is a
presentation-contract decision and belongs to G4.

**The 5 copies were each subtly different**, which is the cost the audit
predicted. `FindHelpSections` even re-declared the `CliSemanticStatus` order by
hand and got it wrong — it listed `failed` sixth where the enum has it second.
Converging on `CliResultHelp` fixed that silently-wrong second oracle.

#### Verification

A throwaway characterization baseline was captured first, as
**Coverage between phases** requires: every command in four views against three
seeded workspaces — pristine, populated, damaged — plus `--help` for every
command and subcommand. 224 captures, normalised for workspace paths,
fingerprints and timestamps, and proven self-stable by two independent runs
diffing to zero.

- **Command output: byte-identical across all 168 captures.** The behaviour
  preservation claim is now evidenced rather than asserted.
- **Help output: 5 files changed, each a reviewed convergence** onto the shared
  wording, with no other command's help touched.
- Unit suite: 18 pre-existing failures of 3357 before and after, **0 newly
  failing**. Eight tests asserted the duplication being removed and were
  rewritten or deleted.

The baseline is discarded with this phase. It is **not** the permanent snapshot
suite, which still waits for G4.

#### Decisions taken

- **Does the folder collapse include `Models/` subfolders?** The user's answer
  was _"included if we end up with better org"_. Measured against that test the
  answer is **no**, and the same measurement retires most of item 4. Of 169
  one-file folders:

  | Count | What it actually is                                   | Disposition                                                                                                                                                                        |
  | ----- | ----------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
  | 56    | `Shared/<Capability>/`                                | The C# directive _requires_ this shape below a command boundary. Keep.                                                                                                             |
  | 56    | `Models/<Layer>/` holding a small file                | `Binding`, `Request`, `Planning`, `Operation`, `Application`, `Result`, `Presentation` recur across roughly 28 commands. It is a uniform layer vocabulary, not task residue. Keep. |
  | 17    | `Models/<Layer>/` holding a large file                | One file with 10–27 types and 110–538 lines. These are **oversized files**, not microfolders. Splitting the file resolves the folder with no namespace churn. Phase 6.             |
  | 37    | `Models/<Topic>/` and feature-root capability folders | Mostly the same oversized-file pattern; a `Models/` folder is itself directive-required.                                                                                           |
  | **3** | Folders no clause asks for                            | **Collapsed.**                                                                                                                                                                     |

  Two further measurements settle it. Flattening the `Models/` roots that hold
  one-file children would produce single folders of 78 to 149 types — the exact
  "flat catalogue of many distinct responsibilities" the same directive clause
  forbids, and it would break the cross-command symmetry that makes the tree
  navigable. And **75 of the 169 one-file folders hold a file with several
  top-level types**: splitting those files in phase 6 resolves 44% of the count
  as a side effect, with zero namespace churn and no judgment call.

  The three collapsed were `Commands/Extension/Models/Permissions`,
  `Framework/Extensions/Models/Serialization`, and
  `Commands/References/Shared/Documents/Parsing` — each a lone child of a parent
  small enough that the directive never asked for grouping.

- **Keep or delete the `*WireVocabulary` types after item 3?** The user's answer
  was that proxy objects without substance are unwanted. **No type became a
  proxy**, so all are kept: `CleanupWireVocabulary` retains 22 mappings,
  `ExtensionInspectWireVocabulary` 26, `DoctorFindingWireVocabulary` 14,
  `StatusWireVocabulary` 6, `DoctorWireVocabulary` 5. The new
  `OperationalWireVocabulary` owns 3 and `WorkspaceSelectionWireVocabulary` owns
  1, but each _owns_ a mapping with many callers rather than forwarding to
  another owner.

#### The cross-command sweep, done after the phase closed

The maintainer asked whether everything that should have been global had been
checked and moved. It had not been — the audit's item 1 named one enum, and a
full scan found ten more crossing a command boundary. All are now moved, in
`ed4cab7a`.

| Family    | Enums                                                                                                                                                | Was duplicated across           | New owner                    |
| --------- | ---------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------- | ---------------------------- |
| Recovery  | `RecoveryBundleProducer`, `RecoveryBundleOperation`, `RecoveryBundleCandidateKind`, `RecoveryBundleIntegrity`, `RecoveryBundleTargetComparisonState` | Cleanup, Doctor, Repair, Status | `RecoveryWireVocabulary`     |
| Sources   | `SourceLayerKind`, `SourceReferenceKind`, `SourceReferenceResolutionState`                                                                           | Context, Find, References       | `SourceWireVocabulary`       |
| Libraries | `LibraryMappingObservationState`                                                                                                                     | Doctor, Status                  | `LibraryWireVocabulary`      |
| Shell     | `CliOutputTarget`                                                                                                                                    | Repair, `CliResultHelp`         | removed with the block below |

**An eleventh copy of the results-and-streams help block was found in
`RepairPresentation`.** The audit's list of ten missed it because it searched
`*HelpSections` files and this one lives in a `*Presentation` file. Routing it
through `CliResultHelp` removed both it and the last `CliOutputTarget`
duplicate. A search of the shared sentence now finds `CliResultHelp` and nothing
else.

**One duplicate is deliberately kept.** `FindHumanValues.Layer` produces the same
two strings as `SourceWireVocabulary.Layer`, but it answers the _human_ question,
not the wire one. Coupling them would make a human-wording change silently alter
the JSON contract. Human wording stays with `CliHumanText`.

The command tests restated these mappings, which would have made each command a
second oracle for a shared owner. They now defer to one
`SharedWireVocabularyTests`, and 30 unused using directives left by the moves
were removed.

Cross-boundary duplicates: **10 enums / 12 copies before, 0 after.** Command
output stayed byte-identical across all 168 characterization captures; the only
help change is Repair converging on the shared wording.

#### What else is duplicated, and what should have been promoted

Asked after the sweep above: is anything else duplicated or sitting lower than
it should. The enum sweep covered one shape of duplication; four further scans
covered the rest. Measured, not estimated.

**1. Type placement is sound — this is the good news.** A scan of every type
declared inside a command boundary and referenced from outside it returns **11
hits, and no leaf reaches into a sibling leaf at all**. Seven are Repair reading
`Doctor`'s diagnosis, which is the accepted design rather than a leak; the rest
are composition or name collisions with unrelated enum members. **Nothing needs
promoting.** The problem is not where types live.

**2. Identical method bodies: 103 groups, roughly 1,400 duplicated lines.**
Bodies were normalised for whitespace and for the method's own name, so the same
logic written twice under two names still collides.

| Span                        | Groups | Duplicated lines | Where it belongs                              |
| --------------------------- | ------ | ---------------- | --------------------------------------------- |
| Across command families     | 22     | ~348             | A shared owner, like the enum sweep above     |
| Across leaves of one family | 59     | ~793             | That family's `Shared/`, which already exists |
| Within one boundary         | 22     | ~270             | That command's own rendering work             |

The middle row is the largest and the most clearly wrong: `Route/Move` and
`Route/Remove` carry byte-identical `ApplyFileAsync`, `ApplyDeletionAsync`,
`DeleteCandidateAsync`, `ReadDeletionCatalogueAsync`, `ReadLayerAsync`,
`ExecuteHeldSafelyAsync` and `ReadStatus`; `Library/Attach`, `Detach` and `Sync`
carry identical `Validate` and `ApplyAsync`; `Extension/Install` and `Update`
carry identical `CreatePackageSource`. Each family already has a `Shared/`
parent, so these are deletions against an existing owner — the same shape as
this phase's item 1, at four times the size.

**3. Text escaping is a correctness problem, not just duplication.** Eight types
answer "how is text escaped for output", and they give **four different
answers**:

| Behaviour                                                | Implementations                                                          |
| -------------------------------------------------------- | ------------------------------------------------------------------------ |
| `\\`, `"`, control as `\uXXXX`                           | `CommandTextEscaping` (the shared owner), `Context`, `Find`, `RouteList` |
| ...plus `\n`, `\r`, `\t`, `\b`, `\f` short forms         | `Extension`, `References`                                                |
| control as `\uXXXX` only, no backslash or quote escaping | `RouteInspect`                                                           |
| `JsonEncodedText.Encode`, which also escapes `+ < > &`   | `Route`                                                                  |

So the same string emitted by `find --json` and by `extension list --json` is
escaped differently, and `route list --json` differs from both. The audit saw a
symptom of this — _"never emit JSON escaping in human text"_ in
[command-output-design.md](../../../emerging/analysis/cli-experience-audit/command-output-design.md)
— but not the cause. **Converging them changes output**, so this is a G4
presentation-contract decision, not a mechanical dedup. It should be decided
with the three detail levels, and it is a reason the permanent snapshot suite
matters.

**4. Duplicated constants.** `DiagnosticValueLimit = 240` is declared **seven**
times, once beside each escaper. The next-action command lines are declared
**eighteen** times across six `*Definitions` files — `"open-forge doctor"` six
times, `"open-forge cleanup"` five — while every command already owns a
`CommandIdentity` constant that could compose them. Both are mechanical and
behaviour-preserving.

`SchemaVersion = 1` appears 26 times and is **not** a duplicate: each command
versions its own envelope independently, which is the accepted contract.

#### Where the rest went

The maintainer accepted the recommended placement on 2026-09-11 and directed
that the work be written down. It now lives in
[Task 31: Implementation Duplication Removal](implementation-duplication.md),
with the measurements in
[Implementation Duplication](../../../emerging/analysis/cli-experience-audit/implementation-duplication.md).

Two decisions were taken at the same time:

- **Text escaping converges on one method.** Either `JsonEncodedText` is the
  implementation, or the single owner uses it internally. The other seven types
  are deleted. This changes `--json` output, so it carries a reviewed diff rather
  than an empty one — Task 31 M3.
- **Constants are placed by the scope that owns them.** Application-wide values
  in one global constants class, module or command values in
  `<CommandName>Constants`, class-local values at the top of the class. The rule
  is now a clause in [the C# style directive](../../../../directives/csharp/style.md)
  so it is inherited rather than re-derived — Task 31 M4.

Nothing moved into this task's remaining phases, and nothing belonged in phase 1.

#### Folder collapse is postponed to the end

The maintainer postponed the remaining folder work to the last phase on
2026-09-11. Nothing is carried forward as a plan, because the plan would be
wrong by then:

- **Phase 6 changes the input.** 75 of the 169 one-file folders hold a file with
  several top-level types. Splitting those files removes the folder from the
  count without touching a namespace, so the candidate list after phase 6 is a
  different list.
- **Phase 3 decides where anything left belongs.** A one-file topic folder inside
  a large `Models/` root should merge into a _sibling_ topic, not dissolve into
  the root. Which sibling is a naming judgment, and the naming pass owns it.
- **Do not re-derive the 168 figure.** It counts three different situations —
  directive-required shapes, oversized files, and genuine microfolders — and only
  the third is work. Re-measure before doing anything.

#### Left for a later phase

- **`CleanupWireVocabulary.Status` is a one-line forwarder** to
  `CliStatusDefinitions.Read(value).MachineName`. It is the only proxy method
  found. It is not in this phase's scope and belongs with G4's rendering pass.
- **The within-command duplication is left for G4.** A scan of every
  `Enum.Member => "wire-string"` arm found **35 enums mapped identically in two
  or more files, across 54 redundant copies**. Everything that crossed a command
  boundary has now been swept (see below). What remains is **20 enums across 24
  copies, all inside a single command** — `FindHumanValues` and
  `FindJsonProjection` re-answer nine enums between them, `References*` four,
  `Route*` five. Those are the same defect against a per-command owner, and they
  are cheaper to sweep alongside that command's rendering rewrite than as a
  separate pass.

### Phase 2 — architecture layers

**Complete.** The document was split rather than only edited, on the
maintainer's direction that the land record should summarise and link while each
big layer gets its own record.

| Deliverable                                                                             | Where                                                                           |
| --------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------- |
| The four layers, the question each answers, and the order a request passes through them | [CLI Layers](../../../crystallized/documents/cli/layers/_layers.md)             |
| Shell — parsing, binding selection, the pipeline, process completion                    | [shell.md](../../../crystallized/documents/cli/layers/shell.md)                 |
| Framework — Documents, Sources, Routing, and the state capabilities                     | [framework.md](../../../crystallized/documents/cli/layers/framework.md)         |
| Operations — the four stages a command moves through                                    | [operations.md](../../../crystallized/documents/cli/layers/operations.md)       |
| Presentation — the selection stage and rendering                                        | [presentation.md](../../../crystallized/documents/cli/layers/presentation.md)   |
| Land record — invariants, layer model, dependency direction, links                      | [architecture.md](../../../crystallized/documents/cli/architecture.md)          |
| Programme-era prose, extracted then archived                                            | [archive record](../../../archived/cli-release/architecture-programme-prose.md) |

The land record went from 824 lines to 523. Section bodies were **moved
verbatim** by script rather than retyped, so no accepted wording drifted during
the split.

#### The three required edits

- **The selection stage is inserted.** `CliViewSelection<TResult>` now sits
  between `CliOperationResult<TResult>` and `CliPresentation<TResult>` in the
  Execution Pipeline chain. Shell owns its position; Presentation defines what it
  selects. This is the layer G4's contract attaches to — the three detail levels,
  size budgets and severity ordering become properties of one stage rather than
  of twenty renderers.
- **`Sources, Routing, And Documents` is split into three**, each with the one
  question it answers. Documents is named as the base that depends on nothing,
  which is the clause a command needed in order to be told not to grow its own
  parser.
- **"A layer owns its question"** is stated in the land record as a structural
  property, with the two measured consequences of not having it.

#### Decisions taken

- **The layer names.** The maintainer accepted _Shell, Documents, Sources,
  Routing, State, Operations, Selection, Rendering, Composition_ but observed
  that a flat list does not convey order, and offered a four-step shape: Shell →
  Sources → transformation → Rendering. Both are now recorded, reconciled: **four
  layers** carry the order, and the nine names are capabilities inside them.

  One correction was needed. The maintainer's short form read _"Shell (arg
  parsing, command identification and routing)"_. **In this codebase Routing is
  the workspace's document route graph, not command dispatch.** Choosing which
  command to run is binding selection and belongs to Shell. Both senses are
  natural in a CLI and only one is a Framework capability, so the layers record
  states the distinction explicitly.

  Two smaller corrections: Documents sits _below_ Sources rather than after it —
  `Sources` has 29 references into `Documents` and `Documents` has none into
  anything — and Routing is not a pipeline step at all. It is a fact domain that
  is read, which is why it did not fit anywhere in the guessed order.

- **Edit in place or a document beside it?** **Both.** The land record is edited
  in place, keeping its acceptance, and now summarises and links. Each big layer
  has its own record.

- **How much reset-era prose survives?** **None.** Following the archive rules,
  what was still current was extracted first — the authority links, the
  dependency-order rule, the package boundary — and the rest was archived with
  its origin, why it was archived, and what replaced it. The Durable
  Implementation Sequence, the Planning and Delegation section, the greenfield
  reset framing, and two goal clauses left. Ideas and Observations were not
  touched, as directed.

#### Found while splitting

- **The documented source tree had drifted from the real one.** It listed a
  `Framework/Routing/` folder that has never existed and a `Shell/Output/` that
  does not exist, and omitted `Framework/OperationalContributors/`,
  `Framework/Permissions/` and `Framework/Serialization/`. Corrected, and both
  lists are now in dependency order rather than alphabetical.
- **Six pairs of Framework capabilities depend on each other in both
  directions.** `Sources`↔`Workspace`, `Sources`↔`OperationalContributors`,
  `Sources`↔`GeneratedNavigation`, `Extensions`↔`Lifecycle`,
  `Mutation`↔`Recovery`, `Libraries`↔`Permissions`. Each cycle means two
  capabilities are one capability that has not been named, or one reached for a
  fact it should have been given. The minority edge is the smaller inversion in
  every case. Recorded in the land record as **current state, not accepted as
  correct** — a layer order cannot be asserted while cycles exist.
- **Two incoming anchor links broke** when sections moved, and `references`
  caught both. One was retargeted; the other is a completed task record noting
  that the anchor had already gone, which is history and was left alone.

### Phase 3 — layer adherence

**Complete.** Four boundary tests in
`tests/unit/.../Architecture/LayerBoundaryTests.cs`, and one style-directive
clause. No production code changed; unit suite 18 pre-existing failures of 3365.

The tests read the `using` graph of the Core source tree and assert the four
invariants in
[CLI Layers](../../../crystallized/documents/cli/layers/_layers.md#boundary-invariants).
They name every offending file rather than reporting a count, and they were
proven by reintroducing the exact phase 1 regression — the test failed with the
file path, the rule, and why it exists.

Two things worth carrying forward:

- **One accepted exception is named, not tolerated silently.** `Commands.Repair`
  reaches into `Commands.Doctor.Shared.Rendering` for
  `DoctorLibraryRecoveryPresentation`, so both report a library residual
  identically. The accepted Architecture permits Repair to read Doctor. It sits
  in an `AcceptedReaches` table with a comment saying that if G1 or G3 moves that
  projection to a shared owner, deleting the entry tightens the test on its own.
- **The test fails loudly when it finds nothing.** It walks up from the test
  assembly to the repository root and asserts it got there, so a test that
  cannot see the source tree fails rather than passing vacuously.

The directive clause: _place a shared owner at the narrowest scope that covers
every consumer in the layer graph, not in the folder tree._ That is the rule the
phase 1 regression broke.

**The boundary tests are a bridge.** If the project split below happens, project
references enforce these four rules at compile time, which is strictly better,
and the tests can be deleted.

#### Why the naming pass left

Three findings, in the order they were made:

**One.** The audit's premise was wrong. It recorded 65 `*Human*` types as named
by negation — _"'Human' means_ not JSON" — and claimed two were
indistinguishable. `CliOutputFormat` has exactly two members, `Human` and
`Json`, and `CliRendererSet.Read(...Format)` selects between them, so "Human"
names a format. Stripping it from all 65 names produces **zero** collisions. The
style directive's clause does not cover the case.

**Two.** The maintainer's objection is nonetheless right, for a different
reason: _human_ is a poor word for a type. It names the audience rather than the
output. That is a real complaint even though the negation charge was not.

**Three — the decisive one.** **G4 has already chosen the replacement.** Its
accepted plan reads `--projection text|json|tsv` _(an option, default `text`)
replaces `--json`_. So:

- The word is **`text`**, not `Standard`, `Default` or `Friendly`. `Standard`
  collides with _standard output_ and _standard help_ in the same help block;
  `Default` names selection precedence rather than content, and stops being true
  the moment a default changes; `Friendly` pairs with nothing. `text` pairs with
  `json` the way every CLI does, already appears in this tree
  (`CommandTextEscaping`), and is the word G4 puts in front of users.
- A **third format arrives**. With `tsv` in the set, _human vs json vs tsv_ is
  incoherent and _text vs json vs tsv_ is coherent. Renaming now would be done
  against a two-format model that G4 replaces.
- **Most of these types stop existing.** G4 makes renderers _"dumb, mostly
  shared formatters"_ behind a selection stage. Renaming 65 types and 9 helpers
  before deleting or rewriting most of them is work done twice.

So all naming — the `Human` to `text` rename and the four-suffix consolidation
below — **moves into G4**, where the types are being rewritten anyway and the
rename costs nothing extra.

#### Carried into G4: the four suffixes

Nine types, four names for one role. Each is a static class of `string`-returning
helpers holding a command's shared human wording.

| Suffix                   | Types                                                                                         |
| ------------------------ | --------------------------------------------------------------------------------------------- |
| `*HumanValues`           | `FindHumanValues`, `ReferencesHumanValues`, `RouteInspectHumanValues`, `RouteListHumanValues` |
| `*HumanText`             | `CliHumanText`, `ExtensionHumanText`, `LibraryHumanText`                                      |
| `*HumanVocabulary`       | `DoctorHumanVocabulary`                                                                       |
| `*HumanRenderingSupport` | `ContextHumanRenderingSupport`                                                                |

Under the `text` vocabulary these become `*TextVocabulary`, pairing with the
already-uniform `*WireVocabulary` on the machine side. Decide it with G4's
contract, not before.

#### One user-visible consequence to settle in G4

"Human" appears eight times in **every command's help**, in the
results-and-streams block — _"human stdout"_, _"human stderr"_. That block is now
single-sourced through `CliResultHelp`, so it is one edit rather than twenty-two.
It does **not** appear anywhere in JSON output, so no wire contract depends on
the word.

Prose and type names need not use the same word. _"Human-readable results use
stdout"_ is better English than _"text results use stdout"_, and there is no
contradiction: the prose describes the reader, the type describes the format.
Recommend keeping _human-readable_ in prose and `text` in code and flags.

#### Suggested model

**Opus, short session.** The boundary test is the only design decision left in
the phase, and it is the guard every later phase relies on.

### Phases 4–6

Group detail is in the sections below, and the per-group _decide versus produce_
split is in **How to work each group**. Three notes that only make sense here:

- **G4 must not start before Phase 2.** Its contract _is_ the selection stage,
  and writing that contract twice is the expensive mistake in this plan.
- **G5 is unblocked by Phase 1 item 2**, not by anything in its own group.
- **Phase 6 should be re-measured, not planned now.** Three of the five largest
  files are in Extension Update and Repair, both of which G1 and G3 change
  substantially. The list will be different by then.

## Coverage between phases

The question this plan does not otherwise answer: after fixing Phase 0
mechanically, is there enough coverage to keep re-analysing and improving?

**Honestly, no — and the gap is narrow and cheap to close.**

Phase 0 itself is fine. Each fix is independently verifiable by the reproduction
that found it, and each touches 0–7 existing tests, so a regression test per fix
is small and should be written with the fix.

What is not covered is **Phase 1**, and it is the first phase whose entire claim
is _nothing changes_. Today **202 human-text assertions exist across 2,811
tests**, almost all `Assert.Contains`, which cannot fail on noise. Deleting 12
duplicate mappings and collapsing 168 folders would produce a green suite whether
or not the output changed.

Three things close it, in increasing cost:

- **A characterization baseline before Phase 1.** Capture full output for every
  command against two or three seeded workspaces into files, and diff after. A
  shell script over the linked CLI is enough. This is **not** the permanent
  snapshot suite, which still waits for G4 — it is a throwaway that proves one
  refactor changed nothing, then is discarded.

  This reconciles the earlier _"do not snapshot before G4"_ advice: permanent
  snapshots freeze output you intend to replace; a disposable characterization
  baseline for a behaviour-preserving refactor does not.

- **The property assertions**, which can be written now and survive everything
  because they express intent rather than current behaviour:
  `lines(brief) <= lines(standard) <= lines(verbose)`; severity ordering; valid
  UTF-8 with no escapes; a zero-finding result never containing `findings:`.
  Several fail today, which is the point. Detail in
  [model-level-snapshot-testing.md](../../../emerging/analysis/cli-experience-audit/model-level-snapshot-testing.md).
- **A regression test per Phase 0 fix**, written with the fix.

The first item is the one that answers the question: **without it, Phase 1 is
unverifiable and every phase after inherits that uncertainty.** It is perhaps an
hour of scripting and it makes the rest of the plan safe to execute.

## How to work each group

One session per group, not per task. The groups are dependency-ordered and
internally coherent; per-task sessions would re-derive the same context 119
times. Open each with that group's section below plus the one or two backing
documents it names — roughly 300 lines, against ~6,000 for the whole scope.

The split that matters is **decide versus produce**. A task marked
`judgment` bakes a choice into everything after it and should not be delegated
until the choice is written down. Everything else is spec-driven volume, and that
is where cost is saved.

| Group  | Decide (high reasoning, do not delegate)                                                                                                           | Produce (spec-driven, delegate)                                                                               |
| ------ | -------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------- |
| **G1** | Nearly all of it. One file or two? Baselines or git? What does `allow` look like? Every task is judgment and they cascade into G2 and G3           | almost nothing until decided                                                                                  |
| **G2** | Where the derivability boundary sits — what `index` may fix versus what stays a finding                                                            | the edits themselves: delete one clause, accept unknown keys, quote 66 files, swap markers for headings       |
| **G3** | The single-engine invariant — _if a command blocks, `doctor` has that finding; if `doctor` is empty, nothing blocks_                               | ~12 individual message and subject fixes, each self-contained                                                 |
| **G4** | The presentation contract: three detail levels, the selection rules, severity order, size budgets. **Write it before anything else in this group** | ~20 renderer rewrites against that contract, plus the small rendering defects. The largest volume in the plan |
| **G5** | What a good selector actually looks like — one design session                                                                                      | the TUI implementation, did-you-mean, all-errors-at-once, help restructuring                                  |
| **G6** | Which categories survive, what the payload teaches, the `#LoadNow` defaults                                                                        | template authoring once the shapes are decided                                                                |
| **G7** | The snapshot format, normalization scrubbing, and the three-layer split                                                                            | seeds, journeys, snapshot breadth, coverage matrix                                                            |

Notes that change the model choice:

- **G1 first, alone.** Its decisions delete tasks in G2 and G3 rather than
  informing them. Settle it in a short session before scheduling anything else.
- **G4 is the budget.** Renderer rewrites are high-token and mechanical once the
  contract exists. This is the one group where delegation saves real money, and
  the saving depends entirely on the contract being written first.
- **G6 is writing, not coding.** Templates, questions, axioms and shipped prose
  are the product's front door and carry the maintainer's voice. Reasoning effort
  matters less here than judgment about tone and intent; consider authoring the
  key text directly rather than delegating it.
- **G3's invariant is the one to state as a test**, not prose. Once written as
  an assertion it becomes G7 work and stops needing judgment.
- **Anything marked `—` is a decision already taken.** Do not reopen it in a
  working session; it is recorded so the session does not have to re-argue it.

---

## G1 — State model

One authored file, no per-file baselines, git for change detection. This group
contains two ship-blockers and the permission dead end.

### Decisions taken, 2026-09-11

The maintainer answered the three judgment questions. **These supersede the
bullets below wherever they differ**, and the bullets are kept so the reasoning
that led here stays readable.

- **Two files, npm-shaped: `open-forge.json` and `open-forge.lock.json`, both in
  `.agents/`.** This resolves a conflict between this record, which said collapse
  to _one_ file, and
  [repository-dogfood-and-configuration.md](../../../emerging/analysis/cli-experience-audit/repository-dogfood-and-configuration.md),
  which said split by nature. The split wins, in the npm spelling, because the
  convention already teaches which file a person edits. Root placement was
  considered for marker value and rejected: `.agents/` is already the marker,
  since `CliWorkspaceSelector`'s walk-up keys on the directory. **This is a
  structural change and needs its own decision record plus an architecture
  update before implementation.**
- **Change detection is neither a fingerprint nor a git dependency.** `update`
  overwrites, then says where the previous content went — pointing at `git diff`
  when a `.git` directory is present, and at the retained recovery bundle
  otherwise. No git binary is invoked; presence is a directory check. The
  fingerprint-chain alternative was raised and rejected by the maintainer as
  unmaintainable.
  - **The backup half already exists.** `install`, `update`, `index` and all
    three `extension` commands already write a per-operation zip of prior content
    to `%LOCALAPPDATA%/OpenForge/recovery/v1/<workspace-key>/`, and `cleanup`
    already deletes them. What is missing is that the CLI never _tells_ anyone.
    No `.bak` file is needed; the bundle is better, because it sits outside the
    workspace and never enters a diff.
- **Permissions become `allowInstallPaths` in `open-forge.json`,** globs
  accepted. In a TTY the CLI asks — allow always, allow once, cancel. Outside one
  it still needs a flag or the original defect survives, because the prompt
  requires `!StandardInputRedirected`. Six commands gate:
  `extension install/remove/update` and `library attach/detach/sync`.
- **A `removed` array of categories** in `open-forge.json`, so a category the
  user deleted is not reinstated. This is the enforcement point for the Loader
  axiom _"Removed defaults stay removed unless the user asks to restore them."_

**Open, proposed 2026-09-11, awaiting validation.** What the lock file holds once
change detection no longer needs it. Proposed: it answers **ownership only** —
what was installed, at what version, and which files each Extension and Library
owns, none of which is derivable — under one governing rule, **the lock file is
never a gate**. No command refuses because of it; missing or stale, it is rebuilt
best-effort and the command proceeds saying so. That rule is what makes it safe,
and it is affordable because integrity left with the baselines. Four things go:
per-file `baselineFingerprint`, the `region: "entries"` / `exact-bytes` twins,
`workspacePath`, and `fingerprintPolicy` — the entire ship-blocker surface. Named
risk: `extension remove` deletes by the lock's file list, so a wrong lock
over-deletes; contained by intersecting that list against what the payload claims,
and by the recovery bundle written before any deletion.

Backing: [lifecycle-baselines-and-architecture.md](../../../emerging/analysis/cli-experience-audit/lifecycle-baselines-and-architecture.md),
[repository-dogfood-and-configuration.md](../../../emerging/analysis/cli-experience-audit/repository-dogfood-and-configuration.md),
[structural-requirements-and-markers.md](../../../emerging/analysis/cli-experience-audit/structural-requirements-and-markers.md).

- **Collapse to one `open-forge.json`.** Holds authored config (`rules`,
  `thresholds`, `allow`) and a minimal install index (`installed`,
  `removedByUser`). Replaces `open-forge.lifecycle.json`,
  `open-forge.permissions.json` and `open-forge.libraries.json`. Today four files
  share one naming convention, so a 17 KB machine-owned record and a short human
  allow-list look identical. **M · judgment**
- **Delete every per-file baseline and `workspacePath`.** `framework.targets` is
  **9,588 of 11,337 bytes — 85%** of the record. Keep `version`, `files`, and one
  source `inventoryFingerprint`. **M · judgment**
- **Delegate change detection to git.** `update` asks git what is modified and
  says so plainly before overwriting. Formatting-tolerant, path-independent, and
  honest. Outside a git repository, overwrite and say so — a small stated gap
  rather than a mechanism that fails everywhere. **M · judgment**
- **Ship-blocker: the `region: "entries"` / `exact-bytes` baseline.** Every managed
  file has two baselines; the second fingerprints _generated_ content as exact
  bytes. Root cause of "`route init` poisons the baseline". **Measured: after one
  `route init`, `extension install` blocks on a file that is byte-identical to
  pristine, with an unchanged record (43 targets before and after, 0 differing);
  removing the route restores it.** So an Extension cannot be installed into any
  workspace that has ever had a route added. Resolved by the two tasks above. **—**
- **Ship-blocker: absolute `workspacePath` blocks every clone.** A byte-identical
  copy at a new path returns exit 5 from `status`, `update` and `extension
install`, and no command recovers it. Resolved by the deletion above. **—**
- **Fix CRLF and whitespace intolerance.** A trailing space, a missing final
  newline, and **CRLF** each block the workspace — CRLF despite
  `NormalizeLineEndings` existing, because it is not applied on the gating path.
  With Windows `core.autocrlf=true` a clone is unusable on arrival. Moot if change
  detection moves to git; otherwise widen normalization. **M**
- **Stop the silent `exact-bytes` fallback.** A malformed generated region makes
  the reader hash raw bytes, so a file already in trouble becomes maximally
  brittle. **S**
- **One verdict per state.** `update` says `attention` and `extension install` says
  `blocked` for the same change. **S**
- **Make permissions grantable without a TTY.** No `--allow`, `--grant`,
  `--approve` or `--permission` option exists **anywhere in the source**. The only
  non-interactive path is hand-writing an undocumented schema recovered from
  `WorkspacePermissionDocument.cs`. Outside a terminal, any destination outside
  `.agents` is permanently unreachable. Move grants to `allow` globs in
  `open-forge.json` and add `--allow <path>`. **M · judgment**
- **Make the permission block message state the fix**, naming the flag and the
  config entry rather than a file the user has never heard of. **S**
- **Add `rules` and `thresholds` to the config.** `rules` maps any finding code to
  `off|info|warn|error`; a rule set to `off` is not counted, not printed, and does
  not affect exit status, with one summary line saying how many are disabled.
  `thresholds` covers `startupTokens`, `outputLines`, `routeDepth`, `scopeTokens`,
  `brokenLinks`. Finding codes are already stable so the key space is free. **M**
- **Report the legacy `open-forge.extensions.json` once, as information.** It is an
  `open-forge-old` receipt; nothing in `src/cli` reads it and no command mentions
  it, so a workspace looks provisioned and is not. **S**
- **Do not add a git-cleanliness gate. Decided.** `install`, `update` and `repair`
  run on a dirty tree. One advisory line _after_ a write: _"Your working tree had
  N uncommitted changes before this ran."_ Never blocking. **S**

---

## G2 — Structural leniency

Make a custom scope cheap to create, and make the CLI fix what it can already see.
This group is the whole "annoying flow" complaint.

Backing: [structural-requirements-and-markers.md](../../../emerging/analysis/cli-experience-audit/structural-requirements-and-markers.md),
[interoperability-and-diagnosis.md](../../../emerging/analysis/cli-experience-audit/interoperability-and-diagnosis.md),
[taxonomy-and-adoption.md](../../../emerging/analysis/cli-experience-audit/taxonomy-and-adoption.md).

- **Treat absent or empty `## Axioms` as inherited.**
  [`RouteSourceStructureReader.cs:111-125`](../../../../../src/cli/core/OpenForge.Cli.Core/Framework/Sources/Operational/RouteSourceStructureReader.cs#L111-L125)
  requires exactly one heading _and_ non-whitespace content. Return `Valid` with an
  `Inherited` state instead, produce no finding, and stop `route init` emitting its
  placeholder bullet
  ([`RouteInitScaffoldComposer.cs:27-29`](../../../../../src/cli/core/OpenForge.Cli.Core/Commands/Route/Init/Shared/Planning/RouteInitScaffoldComposer.cs#L27-L29)).
  Measured: it does not block commands, but is a permanent `doctor` warning on
  every custom scope. Proof the rule is wrong — the scaffolder must emit
  boilerplate to pass its own check, and none of the 19 shipped entrypoints uses
  it. **S · judgment**
- **Locate generated regions by heading, not HTML comment markers.** The project
  already depends on Markdig
  ([`MarkdownDocumentParser.cs:1-3`](../../../../../src/cli/core/OpenForge.Cli.Core/Framework/Documents/Markdown/MarkdownDocumentParser.cs#L1-L3))
  and already finds `## Axioms` by heading
  ([`RouteSourceStructureReader.cs:111`](../../../../../src/cli/core/OpenForge.Cli.Core/Framework/Sources/Operational/RouteSourceStructureReader.cs#L111)),
  while `## Entries` needs a marker pair
  ([`MarkdownGeneratedRegionSyntax.cs`](../../../../../src/cli/core/OpenForge.Cli.Core/Framework/Documents/Markdown/MarkdownGeneratedRegionSyntax.cs),
  which already declares `EntriesHeadingText` and `EntriesHeadingLine`). Two
  mechanisms for one job; the marker one is what breaks. Removes an entire failure
  class. Accept and strip existing markers on read for one release. **M · judgment**
- **Replace the AGENTS.md comment markers with a visible `## Open Forge` heading**
  plus one human sentence, instead of
  [`FrameworkContentIdentity.cs:12`](../../../../../src/cli/core/OpenForge.Cli.Core/Framework/Lifecycle/FrameworkContentIdentity.cs#L12). **S**
- **Do not police frontmatter at all. `ParseSkill` is the anomaly — make it match.**
  Measured: ordinary Markdown frontmatter is already unpoliced — arbitrary foreign keys,
  nested objects, `license`, `allowed-tools` all parse to `Coverage: complete`. Only
  `SKILL.md` gets a stricter hand-rolled reader that returns `false` on any key but
  `name`/`description`. The correct policy already exists for every other file type.
  Open Forge cares about exactly three attributes under the `open-forge:` key —
  `description`, `tags`, `responsibility`
  ([`FrameworkMetadataYamlModels.cs:5-18`](../../../../../src/cli/core/OpenForge.Cli.Core/Framework/Documents/Metadata/Models/FrameworkMetadataYamlModels.cs#L5-L18))
  — plus root `name`/`description` for interop. Everything else is read past and left
  untouched: not validated, not rewritten, not round-tripped. The YAML reader already
  handles unknown keys; the serializer context never needs to know them because foreign
  keys are never written back. **S**
- **Accept unknown keys in `SKILL.md` frontmatter.**
  [`SourceAuthoredMetadataParser.cs:120`](../../../../../src/cli/core/OpenForge.Cli.Core/Framework/Sources/Metadata/SourceAuthoredMetadataParser.cs#L120)
  returns `false` on any key but `name`/`description`; `license` and
  `allowed-tools` are standard, so essentially every published skill hard-blocks
  the workspace. **S**
- **Fix the Skill tag contradiction.** `RouteSource.cs:93` requires a Skill to have
  zero tags while every other source requires at least one — both behind
  "Required route metadata is missing". **S · judgment**
- **`index` fixes what it can, in place, with no new flag.** It already rewrites
  these files and already computes the facts that identify the problem, yet is
  explicitly forbidden from repairing any of it
  ([`IndexHelpSections.cs:41`](../../../../../src/cli/core/OpenForge.Cli.Core/Commands/Index/Shared/Rendering/IndexHelpSections.cs#L41)).
  Safe derivable fixes: insert a missing `## Axioms` or `## Entries`, move a
  non-final Entries region, quote an unquoted `": "`, stub missing frontmatter as
  `#NeedsAuthoring`. `--dry-run` previews them. Anything not safely derivable stays
  a reported finding and is never guessed at. **M · judgment**
- **Reject a tag containing a space at write time, naming the tag.** Currently
  surfaces as "the source frontmatter or metadata shape is malformed". **S**
- **Never let one unreadable leaf block a whole command.** An unparseable source is
  `attention` on that source; `index` must still update the other regions and
  `install` must still run. **M · judgment**
- **Un-gate `install` from route validation.** It validates routes that cannot be
  valid until install has written the Loader — a circular dependency `--force` does
  not break. Validate only what it writes plus its collision targets; report
  pre-existing unroutable content afterwards as advisory. **M · judgment**
- **Replace `MarkdownFrontmatterParser` with Markdig's frontmatter extension, and strip
  the BOM.** It locates fences by exact string equality, so a **UTF-8 BOM**, a trailing
  space on either fence, or a `...` terminator all make valid frontmatter undetectable —
  reported as _"Required source metadata is missing"_ for a file whose frontmatter is
  visibly correct. On Windows, Visual Studio, Notepad and `Set-Content` write BOMs by
  default. Full inventory in [hand-rolled-parsing.md](../../../emerging/analysis/cli-experience-audit/hand-rolled-parsing.md). **S**
- **Delete the second `## Axioms` parser.**
  `RouteInspectAxiomsProfileBuilder.Parsing.cs` matches `lines[index] == "## Axioms"` by
  exact equality, while `RouteSourceStructureReader` uses Markdig. Measured divergence:
  a trailing space, two spaces after `##`, or closed ATX (`## Axioms ##`) each make
  `route inspect` report _"no local Axioms section"_ while `doctor` reports the file
  valid. Also removes `RouteInspectMarkdownStructure.cs`, a hand-rolled fence tracker
  that exists only to support it. **S**
- **Add a directive and lint rule: document structure is parsed only in
  `Framework/Documents`.** Outside it, no `StartsWith("#`, no `== "## `, no `"---"`, no
  `Split('
')` over document content. Two violations in the CLI today, two in
  `scripts/`. Cheap to fix now and cheap to keep enforced. **S**
- **Replace the regex frontmatter parsing in `scripts/agent-tooling/agent-projections/`.**
  `agent-model-policy.ts` and `patch-codex-agent-models.ts` parse YAML with regular
  expressions; there is no YAML library in the TypeScript dependency set at all. Same
  unquoted-colon class that broke 66 files on the C# side — and _more_ lenient about
  fences than the C# parser, so the two sides disagree about what frontmatter is. **S**
- **Fix the 66 malformed files in this repository.** `description: Historical CLI-v2
source: …` — unquoted `": "`. `open-forge index` cannot run on the Open Forge
  repo. **S**
- **Add a CI check that this repository can install itself.** Today `install`,
  `index` and `extension install` all fail here in a mutual deadlock. **S**

---

## G3 — Diagnosis truth

One engine, one answer, every finding actionable.

Backing: [interoperability-and-diagnosis.md](../../../emerging/analysis/cli-experience-audit/interoperability-and-diagnosis.md),
[presentation-field-audit.md](../../../emerging/analysis/cli-experience-audit/presentation-field-audit.md),
[repository-dogfood-and-configuration.md](../../../emerging/analysis/cli-experience-audit/repository-dogfood-and-configuration.md).

- **One diagnosis engine.** `doctor` becomes the single source; every other command
  renders a filtered view. Invariants: if a command blocks, `doctor` has that
  finding with the same code and subject; if `doctor` is empty, nothing blocks.
  Today `doctor` reports `Findings: none` on a workspace four commands refuse to
  operate on. **L · judgment**
- **Every blocking finding names a subject.** `target` becomes non-nullable for any
  finding that stops a mutation — `install.generated-region-unsafe` ships
  `"target": null` in every view including JSON and `--verbose`. **M**
- **Name the leaf, not the projection target.** `index` reports
  `.agents/skills/_skills.md`, a pristine Framework file, when the problem is a
  child. `route list` already computes the right answer and is never suggested. **S**
- **Name the key and position on malformed frontmatter.** "The source frontmatter
  or metadata shape is malformed" must become
  `file:3  description: … ^ unquoted ':' — wrap the value in quotes`. A YAML parse
  failure is not a "shape" problem. **M**
- **Fix the `repair` crash.** `repair --automatic --dry-run` in this repository
  exits 1 with `FAILED: Repair failed unexpectedly: ArgumentException` — a bare CLR
  type name as the whole diagnosis — and `--verbose` reveals nothing despite being
  named as the way to inspect it. **M**
- **`repair` must never report success it did not achieve.** No `Status: complete` /
  `Verification: … verified` / `Remaining: 0` while `doctor` has unresolved
  findings. **S**
- **Add upward workspace discovery.** `open-forge status` in `<workspace>/src/`
  reports "not installed", exit 0. **S**
- **Fix `context` exiting 2 on a pristine install.** The shipped `AGENTS.md` and
  `loader.md` trip the CLI's own frontmatter warning. **S**
- **Fix `references` to traverse generated Entries.** It reports zero links for
  every source in a stock workspace while asserting `coverage complete`. **M**
- **Report a deleted managed file as missing.** Deleting `_patterns.md` produces no
  such finding in 244 lines of `doctor`. **S**
- **Stop one appended line blocking the installation record.** Content after the
  Entries section should be `attention` on that file, not workspace-wide `blocked`.
  Largely resolved by G2's marker removal. **M · judgment**
- **Name the file and command in `extension-install.framework-unavailable`.** "The
  lifecycle document is missing" must say which file, where, and that
  `open-forge install` creates it. **S**
- **Reclassify "never installed" from `incomplete` to a terminal status.** Exit 3
  reads as "a check could not finish"; never-installed is a complete, final fact. **S · judgment**
- **Make the `index` count line balance.** `Regions: 137; updates: 16; already
current: 105; verified: 105`. **S**

---

## G4 — The View layer

The missing MVC selection stage, the flag model, and every output defect.
**Write the contract before delegating the volume.**

Backing: [command-output-design.md](../../../emerging/analysis/cli-experience-audit/command-output-design.md),
[lifecycle-baselines-and-architecture.md](../../../emerging/analysis/cli-experience-audit/lifecycle-baselines-and-architecture.md),
[repository-dogfood-and-configuration.md](../../../emerging/analysis/cli-experience-audit/repository-dogfood-and-configuration.md).

- **Rename the format vocabulary with the flag.** `--projection text|json|tsv`
  makes `Human` incoherent as a type name, so `*HumanRenderer` becomes
  `*TextRenderer` and the nine shared-wording helpers converge on
  `*TextVocabulary`, pairing with the existing `*WireVocabulary`. Phase 3
  deliberately did **not** do this: most of these types are rewritten here, and
  renaming them first is work done twice. The word "human" also appears eight
  times in every command's help, single-sourced through `CliResultHelp` — keep
  _human-readable_ in prose, use `text` in code and flags. Reasoning in phase 3.
  **S, with the rewrites**
- **Two presentation flags, not three.** `--projection text|json|tsv` (an option,
  default `text`) replaces `--json`; `--detail brief|standard|verbose` (default
  **`brief`**) replaces `--view` **and absorbs `--verbose`** — verbose is simply the
  highest detail level. Today `--view` selects density everywhere except `find`,
  where it switches format entirely. Every projection is pipeable by construction:
  data on stdout, diagnostics on stderr, no prose interleaved. **M · judgment**
- **Build the missing View-selection layer.** Model → _selection_ → renderers. Today
  324 renderer files each decide what to say and **nothing decides what to omit**;
  25,477 lines of presentation produce a 194-line "no problems" and an 8.8 MB
  `doctor`. Selection takes a result model plus a detail level and returns the
  subset worth showing; it owns severity ordering, suppression, zero-count
  collapsing, truncation and the size budget. Renderers become dumb, mostly shared
  formatters. Then adding a fact to the model changes no output until selection
  opts it in. **L · judgment**
- **End with fewer presentation lines than you started.** **—**
- **Fix the finding model before the view model.** Four kinds of statement share one
  channel: problems, proposals, evidence, and capability disclosures. Of 11,455
  findings here, **594 are problems**. A finding is a problem; a proposal nests inside
  the problem it resolves; evidence nests inside the proposal at `verbose`; coverage is
  a count; a disclosure is said once. Applying just the first two rules takes 11,455 to
  **696** before any presentation decision. Full analysis in
  [finding-model.md](../../../emerging/analysis/cli-experience-audit/finding-model.md). **M · judgment**
- **Delete `reference.target-valid`, `reference.cycle`, `reference.repeat` and
  `reference.external-unchecked` as findings.** 8,454 of 11,455. The last is a
  capability disclosure — _"Open Forge does not fetch external URLs"_ — repeated 98
  times, ~300 lines of the tool talking about itself. One clause per run, or the docs. **S**
- **Classify every finding code, using the three-question procedure.** Q1 removes tool
  statements, Q2 removes proposals and evidence, and only what survives reaches
  severity — which then has four levels: **error / warning / info / debug**. `debug` is
  new and is where most of the current informational set belongs, since its audience is
  someone diagnosing Open Forge rather than using it. Q1 and Q2 account for 10,861 of
  11,455 findings here, so do those passes first and the remaining severity calls are
  few. Mark each surviving code as correctness (fixed) or house style (configurable via
  `rules`). Procedure in
  [severity-and-command-division.md](../../../emerging/analysis/cli-experience-audit/severity-and-command-division.md). **M · judgment**
- **Fix external Extension sources — they fail silently.** With the payload at
  `<package>/.agents/…` instead of `<package>/content/.agents/…`, install reports
  `Status: complete`, `Effects: 0` and installs nothing, with no explanation. This is
  the "external extensions just don't work" experience. Report `Effects: 0` on an apply
  as `attention` with a cause, and document the layout in `--help`. **M**
- **Stop leaking .NET serializer errors.** A wrong manifest key produces _"The JSON
  property 'schema' could not be mapped to any .NET member contained in type
  'OpenForge.Cli.Core.Framework.Extensions.Models.Serialization.ExtensionManifestDocument'"_.
  Name the accepted keys instead. Same class as the bare `ArgumentException` from
  `repair`. **S**
- **Accept the hybrid ID/path form.** `guidance` and `.agents/guidance/_guidance.md`
  both resolve; `guidance/_guidance.md` is **rejected** — the natural middle, and
  exactly what a generated `Entries` link produces relative to its parent. One shared
  normalizer: strip leading `./`, normalize separators, strip trailing `/`, strip
  leading `.agents/`, strip trailing `.md`, resolve the remainder as an ID. Also fixes
  the `./` and backslash inconsistencies. **S**

  Re-measured 2026-09-11 against `index`, which adds three refused forms the
  original measurement did not cover, and confirms the implementation matches its
  contract — so this widens the
  [Source References contract](../../../crystallized/documents/cli/contracts/shared/source-references/interface.md)
  rather than fixing a bug against it:

  | Form                                          | Today       |
  | --------------------------------------------- | ----------- |
  | `.agents/memory/_memory.md` — exact file path | complete    |
  | `memory` — route ID                           | complete    |
  | `.agents/memory` — path to the folder         | **invalid** |
  | `memory/_memory` — ID plus entrypoint stem    | **invalid** |
  | `/memory/_memory.md` — leading slash          | **invalid** |

  So the normalizer also resolves a folder to its entrypoint, accepts an ID naming
  its own entrypoint with or without `.md`, and tolerates a leading `/`. The
  maintainer asked for one document listing every accepted case: it belongs as a
  table in that contract's `interface.md`, not as a new document beside it.

- **`index` fixes indexing-related problems in place; `doctor` reports everything
  wrong. Decided.** The boundary is _derivability_ — `index` fixes what its own parse
  uniquely determines (missing `## Entries`, non-final region, missing `## Axioms`,
  missing frontmatter, an unquoted `": "`, a missing parent entrypoint). A `license:`
  key in `SKILL.md` is not uniquely determined, so it stays a `doctor` finding. **M · judgment**
- **Let `index` create a missing parent entrypoint** — this resolves the skills
  `references/` question with no special case. `SKILL.md` is already its folder's
  entrypoint via `SourceIdentity`; a `references/` subfolder is an ordinary child
  folder, and its entrypoint is uniquely derivable from the folder name. The global
  rule stays "every folder in a route chain has one entrypoint"; the CLI just creates
  the obvious ones instead of refusing. Deeper nesting needs no new rule. **M**
- **Nest candidates inside the finding they resolve.** `candidates-several`,
  `candidate-route-neighborhood`, `candidate-literal-content`, `candidate-filename` and
  `candidate-title` total **2,305 WARNINGs — 75% of all warnings** — and are the
  proposed fixes and supporting evidence for the 594 broken links, emitted at the same
  severity as the problem. This is why warnings read as information. **M**
- **Never emit the same finding twice for one subject.** `reference.cycle` repeats
  six times per link with the explanation only on the first. **S**
- **Severity order, everywhere.** `doctor --view compact` here is 79,051 lines and
  the single ERROR is at line **79,038**. **S**
- **Add a hard output ceiling**, enforced by the selection layer and configured by
  `thresholds.outputLines`. **M · judgment**
- **Rewrite each command's renderer** to the per-command designs in
  [command-output-design.md](../../../emerging/analysis/cli-experience-audit/command-output-design.md). ~20 renderers; mechanical
  once the contract exists. **L**
- **Promote and document the `find` TSV format.** `--view compact` already emits the
  composable one-line-per-hit shape, but it is undocumented and prose findings are
  appended after the rows so it cannot be piped. **S**
- **Strip lifecycle bookkeeping from `status --json`** and collapse
  `{"state":"available","value":19}` to the bare value with `null` for unavailable.
  Both largely resolved by G1. **S**
- **Fix the `references` wording.** It correctly reports only _authored_ references —
  links someone wrote by hand — but says `coverage complete` and `No direct links
found`, which reads as "this file has no links" on a workspace whose links are
  almost all generated. Say what it means: `no hand-written links; see route list
for the generated tree`. Moved here from Phase 0; it is wording, not behaviour. **S**
- **Clean `context` output.** No status header, no per-source field labels in the
  stream, no markers, diagnostics to stderr. This payload is read by a model. **M**
- **Rendering defects**, each independently shippable: JSON escaping in human text
  (`<workspace>\\` in `route list`, `route inspect`, `find`; literal `\n` and
  `\u003C` in `route init`); the `0xFA` OEM byte in `route inspect`; every managed
  file listed twice in `status`; the `Unchanged:` roster in `route create`; a file
  in both `Effects` and `Unchanged`; `--description` echoed empty; the `route
--help` wrap hiding `move`'s second operand; `Written value:23:118:`; `1 files` /
  `1 sources`; `characters` duplicating `bytes`; `find`'s no-match block. **S each**
- **Rename the 65 `*Human*` types.** "Human" names the format by negation and says
  nothing about what the type does. Rename to what they render —
  `DoctorFindingLine`, `DoctorSummary`, `ExtensionPathList`. **M**
- **One shared path normalizer.** `route inspect` accepts `./.agents/…`;
  `library attach` rejects `./shared-src`. Backslashes are rejected everywhere, so
  on Windows tab-completion produces a path the CLI refuses. **M**

---

## G5 — Interaction

355 lines and 6 files against 18,309 for recovery, mutation and locking — a
**52:1** ratio, and the direct cause of every defect in this group.

Backing: [interaction-layer.md](../../../emerging/analysis/cli-experience-audit/interaction-layer.md),
[repository-dogfood-and-configuration.md](../../../emerging/analysis/cli-experience-audit/repository-dogfood-and-configuration.md).

- **Give interaction a real budget.** A keystroke reader, a redrawable selection
  list, a confirmation renderer that shows its plan, and a non-interactive
  equivalent for every prompt. The entire primitive today is `WriteAsync` +
  `ReadLineAsync`; `Console.ReadKey` appears nowhere in non-test source. **L · judgment**
- **Implement a real selector, or rename the concept.** If not building it now,
  rename `interactive-wizard` to `interactive-prompt` everywhere it appears. **M · judgment**
- **`extension install` needs multi-select**, and its `while(true)` must not loop
  forever silently on a typo. Fix the loop even if the selector waits. **M**
- **Render the plan before asking for confirmation.**
  `"Apply this Install plan? [y/N]"` is issued before anything is printed. **S**
- **Add did-you-mean.** `statuss` → `status`, `memries` → `memory`. **S**
- **Report all invalid inputs at once.** `route create` knew both `--description`
  and `--tag` were missing and reported one per round-trip. **S**
- **Make `Next:` answer the question asked.** `--help` cannot enumerate source IDs,
  and `Next: open-forge route update` is missing its required operand. **S**
- **Fix exit-code consistency.** `route init` exits 2 on its success path; bare
  `open-forge` exits 0; `status` in a non-workspace exits 0. **S · judgment**
- **Help**: separate `Options` from `Global options`; move the exit-code table and
  stream policy into `open-forge help exit-codes` instead of 20 identical lines on
  every command; stop printing `[default: Expanded]` when `Expanded` is rejected;
  delete the false "The equals form is required for `--depth`"; target ~20 lines per
  command (`install --help` is 71). **M**

---

## G6 — Content and taxonomy

What ships, how it reads, and why implementers skip it.

Backing: [taxonomy-and-adoption.md](../../../emerging/analysis/cli-experience-audit/taxonomy-and-adoption.md),
[loading-and-scope-discipline.md](../../../emerging/analysis/cli-experience-audit/loading-and-scope-discipline.md),
[lifecycle-baselines-and-architecture.md](../../../emerging/analysis/cli-experience-audit/lifecycle-baselines-and-architecture.md).

- **Ship a good template for every root category in core.** A bare install ships
  **zero** templates; even with `development-toolkit` there is nothing for
  `directives`, `guidance`, `maps`, `patterns`, `skills` or `workflows` — six of
  eight. One entrypoint template and one leaf template each. **M**
- **Scaffold `route init` / `route create` from the target category's template.**
  Correct by construction, and `#NeedsAuthoring` becomes a description to sharpen
  rather than a placeholder carrying no signal. **M**
- **Add a `## Context` section to every planning and task template**, naming the
  loader and the scopes relevant to the task. This is why an implementer skips
  Open Forge: a task record is a complete, self-contained work order that never
  mentions it, so skipping is the correct reading of what was handed over. Naming
  the relevant scopes also gives the plan its own selection. **M · judgment**
- **Make "a handoff must be self-sufficient" an axiom.** A record that assumes its
  reader already loaded the framework is a broken handoff. **S**
- **Rewrite shipped templates and workflows in user-facing language.** "How you
  record a decision", not "how Open Forge stores a Decision". This is the first
  thing every user reads. **M · judgment**
- **Make workstream a first-class shape under `working`** — a scope with a plan, a
  current state, and tasks — and ship a template for it. `working/cli-development`
  (100 task files) and `working/framework-review` both reinvented it. **M · judgment**
- **Demote `checkpoints` and `handoffs` to templates within a workstream.** 1 and 13
  files against 140 in `working`. **S · judgment**
- **Keep `crystallized/*`, `archived` and `emerging/*` unchanged.** 325 files across
  them is a clear earn. Reconsider only whether `emerging` needs three
  subcategories, since the placement decision is ambiguous and never corrected. **—**
- **Add a `doctor` finding for a `#LoadNow` scope with zero entries.** `maps` holds
  1 file and `skills` holds 0 here, and both are `#LoadNow`. **S**
- **Surface `responsibility` in `route inspect`, and make it required on entrypoints
  with a redirect half** ("…behavior that must be followed belongs in directives").
  The Loader already defines the field; **no command reads it**. This is the missing
  answer to "what belongs here?". Analysis in
  [memory-authority-boundary.md](../../../emerging/analysis/cli-design-retrospective/memory-authority-boundary.md). **M · judgment**
- **Add a `route.prescriptive-language` finding.** Memory carries **1,288** mandatory
  statements (`must`/`never`/`always`/`do not`) against **40** in `directives` — 32×
  more binding language in the category that describes than in the one that
  prescribes. Measure by density, exempt quoted material, default to `info`, and let
  the `rules` config disable it. Would have caught this on the first contract written. **M**
- **Print the scope's question when a route is created.** Every entrypoint already
  carries one (_"What behavior is required in this scope?"_), and nothing ever shows
  it to the author at the moment of placement. **S**
- **Fix the Directives loading axiom — it states half a rule.** _"Every sibling
  Directive listed under an entrypoint's `Entries` carries #LoadNow"_ is unconditional
  and contradicts the Loader's "on demand by default". The payload is correct — every
  leaf is `#LoadNow`, every **sub-entrypoint is on demand and acts as the scope gate**
  — but the gate half is nowhere written, so an agent applying the stated half tags the
  entrypoint too and collapses scoping. Observed in practice. **S**
- **Adopt the agent-authored Memory classification axiom**, first in its section:
  _"Memory supports the other categories with evidence, rationale, accepted context,
  and history. It does not define behavior. When an outcome belongs to another
  category, move it there and keep the supporting reasoning in Memory, linked in both
  directions."_ Add the redirect clause naming `directives` / `guidance` / `patterns` /
  `workflows`. Better than the current text on four counts; addresses the 1,288-vs-40
  imbalance. Analysis in
  [category-boundaries-and-wording.md](../../../emerging/analysis/cli-design-retrospective/category-boundaries-and-wording.md). **S**
- **Keep Workflows. Decided.** Runtime independence is premise-level: a Workflow is
  followable by any agent or human, a Skill needs a runtime implementing `SKILL.md`.
  Deleting Workflows would make the method layer depend on a vendor format. **—**
- **Sharpen the Workflow/Skill boundary instead.** Workflow = a method you follow;
  Skill = a capability your runtime activates. Add a redirect to each. Consider folding
  `skills` under `workflows` as a delivery variant — `SourceIdentity` already routes
  `SKILL.md` as its folder's entrypoint — but **only after checking whether target
  runtimes discover skills by path**, which would make the path load-bearing. **M · judgment**
- **Keep `maps`, sharpen its question, remove it from `#LoadNow`.** Revised: coarse
  repo orientation plus external references is a real need with no other owner —
  `Entries` is exhaustive, generated and one level deep, whereas a map is deliberately
  lossy. It is also the natural home for "where the non-Open-Forge things live". Change
  the question from _"Where is a useful source?"_ (which invites a link dump) to
  _"What shape is this repository, and what lives outside it?"_, ship a template to
  guard against granularity drift, and read it when orienting rather than every task. **S · judgment**
- **Treat interoperability as a premise obligation.** "Grow your own framework" is a
  promise about what the user already has. The CLI recognises exactly **one** foreign
  primitive — `FrameworkPayloadAsset.cs:10`, `RootClaudePath = "CLAUDE.md"` — while
  this repo alone carries `opencode.json`, `apm.yml`, `apm_modules/`, `.github/`, and
  the ecosystem adds `.claude/` skills, hooks and commands, `.cursor/rules`, `.mcp.json`
  and more. Three levels, in order: **do not break** (G2, required); **recognise and
  route** (generalise the `SKILL.md` mechanism — `SourceIdentity` + a native metadata
  reader — which was built for exactly one format); **reconcile** divergence between a
  Directive and a `CLAUDE.md` rule (not v1). **M · judgment**
- **Keep `skills` as a route — decided — and treat it as the first of a family.**
  Runtimes discover by path (`.claude/skills/`, `.claude/commands/`, `.cursor/rules/`),
  so the framework must meet them where they live and cannot relocate them for
  tidiness. Reframe the question from _"Which specialized capability would help?"_ to
  _"Which capability does your runtime already provide here?"_, with the redirect
  _a method with no runtime support belongs in `workflows`_. Decide the
  recognised-primitives shape before adding a second such route. **S · judgment**
- **Rewrite the `maps` question and description to force coarseness.** Question:
  _"What is the rough shape of this repository, its modules, and the external sources
  it depends on?"_ Description: _"A map gives the coarse shape of modules and external
  sources and how they roughly relate — enough to navigate, not enough to answer."_
  The current _"Where is a useful source?"_ invites a link dump, which is what caused
  the overlap with `Entries`. **S**
- **Do not add a `changes` category. Decided.** A change set is workstream state, and
  `working/cli-development/` already builds it by hand. Adding it would repeat the
  artifact-shaped mistake one level up. General rule: before adding a root category,
  check whether the need is _a new kind of thing_ or _a missing shape within an
  existing kind_. **—**
- **Change the shipped `#LoadNow` defaults.** 15 of 19 entrypoints and 7 of 8 root
  routes are `#LoadNow`; a bare install is 81% startup share. Ship it on
  `directives` only — _after_ G3 makes the on-demand path trustworthy. **S · judgment**
- **Move the `#LoadNow` policy next to the tag definition**, phrased as a test:
  _name the task shape that fails without it, or it is on demand_. **S**
- **Move the CLI command list out of the loader into an `open-forge-cli` Skill.**
  `loader.md` lines 82-96 are **1,049 of 9,860 bytes — 10.6%** of a file read on
  every task, and six of nine commands are maintenance a reading task never uses.
  Keep `context`, `find`, `references`. Also gives core its first real skill and a
  worked example of the format. **S**
- **Show loading cost at the moment of the decision.** `route create` / `update` /
  `init` print `Startup context: 8,254 → 9,102 tokens (+848, +10%)`. Highest-leverage
  single change for loading discipline. **M**
- **Attribute startup context in `status`**, sorted by cost, as continuity already
  is; and compute it from whatever parses, so one blocked route does not erase the
  whole measurement. **S**
- **Three install tiers, not a catalogue.** Core (loader + eight entrypoints + a
  template for each) → one recommended bundle offered by name at the end of install
  → the rest discoverable via `extension list`. Avoid the current failure where
  `templates` and `skills` are `#LoadNow` root routes and are empty: better to ship
  fewer routes fully populated than eight with two hollow. **M · judgment**
- **Make `find` composable and `route list` honest about truncation** — it reports
  `Coverage: complete` for a depth-1 listing and never mentions `--depth=all`. **S**
- **Require a real description before a route counts as authored.** **S · judgment**
- **Do not add conditional tags.** Task-shaped selection belongs in `rune`, which
  owns relevance judgment; the CLI stays contract-scoped. Recorded as a decision. **—**

---

## G7 — Scenarios

2,811 tests exist across three layers and **every layer asserts fragments; none
asserts a command's composed output.** Seven of nine audit findings survived
because of an _absent scenario_, not an absent assertion.

Backing: [test-strategy-and-scenarios.md](../../../emerging/analysis/cli-experience-audit/test-strategy-and-scenarios.md).

- **Write the cross-command property assertions now, before anything else.** They
  need no snapshots and no runner, they express the G4 contract rather than current
  behaviour, and their early failures are the point:
  `lines(brief) ≤ lines(standard) ≤ lines(verbose)`; no output exceeds its
  `outputLines` threshold; errors before warnings before informational; a zero-finding
  result never contains `findings:`; valid UTF-8 with no `\uXXXX`, `\\` or literal
  `\n`; rendering is idempotent. Cheapest high-value work in the plan. Full analysis in
  [model-level-snapshot-testing.md](../../../emerging/analysis/cli-experience-audit/model-level-snapshot-testing.md). **S**
- **Finish the string-snapshot tool, and prove it on two or three commands now.** Full
  rendered command output belongs in the **integration** layer: real workspace → real
  operation → real renderer already runs in-process there today. The integration
  assembly is published with `PublishAot` and run as a native binary
  (`native-integration` in `layout.ts:33`), so Verify and Snapshooter are the wrong
  shape — but a rendered command is already a `string`, so no serializer is needed and
  the AOT-safe mechanism is also the simplest. Use `[CallerFilePath]` to locate
  snapshot files under the native runner, and `OPENFORGE_SNAPSHOT_UPDATE=1` rather than
  a CLI flag. Scrub paths, separators, line endings, version and fingerprints —
  **never** scrub byte counts or non-UTF-8 bytes. **M · judgment**
- **Full snapshot coverage only after G4's selection layer.** One file per
  `(command, status, detail)`, named for situations
  (`doctor/healthy-workspace.txt`). Written before G4 they freeze the output being
  replaced — already true of `DoctorHumanSnapshots` and the verbatim e2e strings. **M**
- **Point the coverage-matrix meta-test at the snapshot set**: every
  `(command, status, detail)` triple must have a file, and the build fails when one is
  missing. **M**
- **Collapse to three layers, not four.** Integration and end-to-end are ~90%
  duplicative: `CliCoreApplication.RunAsync(argv, environment, writers, ct)` returns
  `CliProcessCompletion(Status, ExitCode, PrimaryOutputTarget)`, so argument parsing,
  exit codes, stream routing, cancellation and full rendered output are all available
  in-process. The scenarios layer collapses into it — a snapshot and a journey are the
  same mechanism with a different number of `RunAsync` calls. End-to-end shrinks to a
  process smoke suite. Full analysis in
  [test-layer-consolidation.md](../../../emerging/analysis/cli-experience-audit/test-layer-consolidation.md). **M · judgment**
- **Reduce end-to-end to its six real concerns**: the AOT binary runs at all, real OS
  signals, real `Console.IsInputRedirected`, the relocated executable and embedded
  payload, file-descriptor-level stream separation, and npm wrapper parity. Today it
  spends **172 process-spawn call sites across 106 tests**, with 87 of 91 runs going
  through `--json` to assert what `RunAsync` returns directly. **M**
- **Test the interaction layer in-process — it is unreachable any other way.**
  `CliCompositionInputs` exposes `StandardInputRedirected` / `PromptOutputRedirected`
  as injectable flags, and `canPrompt` derives from them. A spawned test process always
  has redirected stdin, so under end-to-end `CanPrompt` is permanently `false` and the
  `repair` wizard, `install` confirmation, `extension install` selection and permission
  prompt are all unreachable. All four are defective; none could have been caught.
  This is a precondition for G5. **M · judgment**
- **Move the write-freedom harness with the tests it guards**, and keep AOT execution
  for the in-process layer (`native-integration` already publishes and runs it
  natively) so it remains a real substitute rather than a managed-only approximation. **S**
- **Keep `Program.Main` deliberately thin** and list whatever remains in it — console
  encoding, culture, global exception handling — in the smoke suite, since `RunAsync`
  bypasses it. **S**
- **Prove the format** with `empty`, `fresh`, and J1 first-run before authoring the
  rest. **S**
- **Build the coverage matrix meta-test** early. Enumerate every
  `(command, semantic status)` pair from the same definitions the CLI uses and fail
  the build when a pair appears in no scenario. It immediately reports how much is
  missing. **M**
- **Author the 18 seeds**, one per state that produced a finding. **M**
- **Author the 12 journeys** so that together they touch every command. **L**
- **Add the three per-step invariants**: encoding (valid UTF-8, no `\uXXXX`, no
  `\\`, no literal `\n` — catches four G4 defects at once); size budget; and
  diagnosis agreement (after any `blocked`/`incomplete` step, `doctor --json` must
  carry a matching finding — encodes the G3 invariant as a property of every
  scenario). **M each**
- **Name scenarios after people and outcomes**, not properties:
  `install-into-existing-skill-workspace`, `repair-a-broken-link`,
  `wrong-package-name`. Every current test name describes the model — _preserves_,
  _retains_, _maps every_. Judge the layer by whether someone who did not write the
  CLI can read a transcript and say "yes, that is what should happen". **—**
- **Fix the tests that enshrine defects.** `PublishedDoctorProcessTests` asserts
  _"Typed Extension bridge-registration role and observed-state authority is
  unavailable"_ verbatim, so any copy pass fails the suite. `DoctorHumanSnapshots`
  contains the `Observed:` / `Read from:` detachment bug as its expected value. **S**
- **Fix the "no false errors" test**, which permits unlimited informational
  findings — why 20 × "Link target is valid" passes. **S**
- **Cover the compact-JSON schema**, or delete the second schema in G4. **S**
- **Stop growing composed-output snapshots in the unit layer**; that job moves to
  scenarios. Unit keeps fragment rendering, vocabulary closure, planner decisions. **—**
- **Keep the write-freedom harness and the closed-vocabulary tests.** Both are
  genuinely good and neither is replaced. **—**
