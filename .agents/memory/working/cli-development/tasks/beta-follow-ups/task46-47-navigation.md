---
open-forge:
  description: Reproduced Skill-resource navigation behavior and the bounded Doctor corrections it exposes
  tags: [Memory, Working, Plan, CLI, Contextual]
---

# Tasks 46/47: Skill-resource navigation

## Current evidence

Rechecked on 2026-09-21 with the same-worktree Windows x64 Native AOT executable,
whose hash matches its clean build manifest. Every command used an isolated
scratch workspace and recovery home, not the repository workspace. Raw stdout,
stderr and scratch files are retained locally.

| Step                                                                                                           | Observed result                                                            |
| -------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------- |
| Install Core, then all bundled Extensions                                                                      | Both exit 0; seven Extensions installed                                    |
| Doctor immediately after installation                                                                          | Exit 0, no findings; 58 sources checked                                    |
| Default Index                                                                                                  | Exit 0; 24 files checked, no changes                                       |
| Explicit Index of `.agents/skills/use-workflow/references/_references.md`                                      | Exit 0; five files checked, no changes; selection is explicitly detached   |
| Add `planning/beta-probe.md` below the Skill reference catalogue, with valid description and workflow sections | New authored recipe exists; catalogue has no entry yet                     |
| Default Index again                                                                                            | Exit 0; same 24 files; recipe remains unlisted                             |
| Doctor                                                                                                         | Exit 2; `route.generated-region-stale` and `route.generated-entry-missing` |
| Follow Doctor's suggested `open-forge index`                                                                   | Already demonstrated above to leave this catalogue stale                   |
| Explicit Index of the reference catalogue                                                                      | Exit 0; one of five files updated; recipe is now listed                    |
| Doctor after explicit Index                                                                                    | Exit 0, no findings                                                        |

The old thirteen-warning fresh-install report in
[Task 46](../task46-routed-skill-resources.md) is superseded by this evidence.
The narrower navigation/action defect in
[Task 47](../task47-entrypoint-reachability.md) survives. Doctor also prints
`<catalogue> does not list absent.` rather than naming `beta-probe.md`.

## Why this happens

Repository-relative paths below are exact source owners:

- `src/cli/framework/OpenForge.Cli.Framework/Framework/Sources/Operational/RouteDoctorFactReader.cs`,
  `ReadMetadata`, `ReadShape`, `IsValidNativeSkill` and `IsNativeSkillResourcePath`:
  valid native Skill metadata and readable support resources no longer trigger
  the old false missing-metadata/unreachable findings. Malformed or unreadable
  resources are deliberately not hidden.
- `src/cli/framework/OpenForge.Cli.Framework/Framework/Sources/Routing/SourceRouteTopologyBuilder.cs`,
  `Build` and `ReadParentRelationship`: native Skills are admitted sources, but
  only recognized entrypoints provide parent relationships. A native Skill is
  not silently promoted into an entrypoint for its resource catalogue.
- `src/cli/operations/OpenForge.Cli.Operations/Commands/Doctor/Shared/Domains/RouteDoctorInspector.cs`,
  `CreateGeneratedFinding`, and sibling `RouteGeneratedEntryDoctorInspector.cs`,
  `Create`: generated-navigation advice is the bare `CommandLines.Index`.
- `src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Doctor/Shared/Wording/DoctorWording.cs`,
  `Actions` and `FallbackAction`: the renderer may additionally append a generic
  Index fallback, even after an explicit action has been supplied.
- `RouteGeneratedEntryDoctorInspector.Create` puts the actual comparison value
  into `Subject.Identifier`. Missing entries therefore use the sentinel
  `absent`, although `DoctorComparisonEvidence` already retains the expected
  destination. `DoctorWording.FindingIdentifier` renders that sentinel literally.

Current [Index behavior](../../../../crystallized/documents/cli/contracts/index-candidate/behavior.md)
explicitly preserves rooted default selection and explicit detached selection.
The accepted native-Skill journey treats the package as discoverable without
turning all its support files into ordinary routes. Neither is permission to
expand traversal over every file below `.agents`.

## Proposed outcome

Keep current native-Skill and Index selection behavior. Make Doctor identify
the missing destination and give an explicit Index target that can repair its
generated region. This is the smallest correction supported by the reproduction.
Automatically traversing Skill resources is a separate product decision and is
withheld from these correction jobs.

## N1: missing-entry identity

Luna/max. Ready to assign after the proposed output correction is selected.

Own exactly:

- `src/cli/operations/OpenForge.Cli.Operations/Commands/Doctor/Shared/Domains/RouteGeneratedEntryDoctorInspector.cs`
- `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Doctor/DoctorWorkspaceRouteMappingTests.cs`

For `RouteGeneratedEntryComparisonKind.Missing`, use its expected destination as
the finding identifier. Preserve actual/expected comparison evidence, including
the actual `absent` sentinel. For Extra and the other comparison kinds preserve
the current identifier rules. Do not alter finding kind, severity, resolution,
status, location, raw evidence or schema shape. A missing comparison without an
expected destination is a contract contradiction to return, not a value to guess.

The direct unit oracle supplies expected `beta-probe.md`, actual null and the
catalogue path. Assert identifier `beta-probe.md`, unchanged missing actual
evidence, and unchanged catalogue subject. Include a non-Missing comparison to
guard its existing identity. This test is pure and uses no disk.

Receipt: `.temp/beta-follow-ups/receipts/N1.md`. Immediate verification is an
owned diff and exact assertion inspection. Execution of the focused unit tests
waits for the coordinator's quiet-tree gate. N1 transfers its production file
to N2 only after acceptance; N1 and N2 cannot mutate it concurrently.

## N2: actionable Index advice

Luna/max. Depends on N1 acceptance and coordinator freeze of command-argument
rendering. The appended dispatch freeze now resolves this prerequisite and authorizes the correction.

Reserved production files, once that freeze is complete:

- The N1 production file.
- `src/cli/operations/OpenForge.Cli.Operations/Commands/Doctor/Shared/Domains/RouteDoctorInspector.cs`
- `src/cli/operations/OpenForge.Cli.Operations/Commands/Doctor/Shared/Domains/RouteShapeDoctorInspector.cs`
- `src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Doctor/Shared/Wording/DoctorWording.cs`

Generated-region, generated-entry and independent-overwrite findings must target
the known owning catalogue, rather than asking users to run default Index.
Use existing typed observation paths. Do not read the filesystem from Rendering,
parse the final sentence, or add route traversal. Prevent the renderer from
appending a contradictory generic Index fallback when an explicit targeted
Index action is already present. Preserve unrelated fallback behavior.

Before release, the coordinator must settle exact command construction for a
path with spaces or quotes. `CommandLines` currently owns constants only;
Extension Install has a private single-quote helper that must not be copied
blindly into cross-shell advice. Freeze the exact helper location, supported
argument spelling, test vectors and new test-file ownership. Do not claim that
a new helper exists or assign a worker to invent a generic shell escaper.

Expected ordinary-path example:
`open-forge index .agents/skills/use-workflow/references/planning/_planning.md`.
No change to automatic Index scope is needed for this command to work.

Receipt: `.temp/beta-follow-ups/receipts/N2.md`. Stop on a finding that does not
have an exact repairable target, an unsupported argument representation or a
required schema change. Return those cases rather than guessing a command.

## N3: connected evidence and task reconciliation

One Luna/max worker after N2; no overlap with an active N1/N2 owner.

Own `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedDoctorGeneratedNavigationProcessTests.cs`
for the connected journey: install the workflow dependency through Planning,
add one valid recipe, run default Index, diagnose, invoke the exact advertised
target, then diagnose again. Assert the missing recipe name, accurate command,
final catalogue entry, empty final findings and preserved authored recipe and
native `SKILL.md` bytes. Use existing process argument arrays and scratch fixtures.
Retain malformed-native-header, ordinary detached-route and healthy fresh-install
evidence. Do not change their expected results merely to close Task 46.

Coordinator ownership: Doctor interface/behavior contracts, any affected existing
snapshots, Task 46/47 status and task indexes. Bind the exact snapshot list from
the accepted output diff before permitting updates. No bulk snapshot rewrite.
N3 receipt: `.temp/beta-follow-ups/receipts/N3.md`.

## Integrated gate and remaining decision

After all mutation workers drain: focused Doctor mapping and published-navigation
checks, format/analyzers, all managed suites, Native AOT build and the native/public
gates. Verify the advertised action fixes the reported issue without changing
unrelated authored bytes. Preserve status 2 while stale and status 0 after repair.

The remaining product choice is whether default Index should ever traverse
Skill-owned reference trees automatically. Recommendation: keep its accepted
narrow behavior for this beta correction and make targeted guidance truthful.
The maintainer approved the bounded corrections, not this broader routing change.

## Divergences observed

Fresh-install diagnosis is already fixed; only the narrower stale-navigation
advice and missing-entry identity defects reproduced. No code or test changes
were made during this investigation.

## N2 dispatch freeze accepted during execution

The user approved the bounded corrections. Argument advice uses a deliberately
small portable spelling, not a shell-specific escape engine. For a nonempty
workspace-relative catalogue path made only of ASCII letters, digits, slash,
period, underscore, hyphen and ordinary space, emit `open-forge index <path>`.
Surround the entire path with double quotes when it contains spaces. A path
beginning with hyphen, containing another character, or containing controls
receives a plain-language instruction instead of a copyable command. This
preserves support for the actual file; it does not reject or rename the file.

The exact manual sentence is:
`Run open-forge index with <path> as its source argument, using your shell's quoting rules.`
The exact explanation for a copyable command is:
`Refresh the generated Entries in this catalogue.`
Keep statuses, finding resolution lanes, raw evidence and JSON schema unchanged.
No OS calls, filesystem reads, subprocesses or string parsing of causes are needed.

One helper owns this Doctor-only action construction:
`Commands/Doctor/Shared/Actions/DoctorIndexAction.cs` in Operations, internal
static `ForPath(string path)` returning `DoctorNextAction`. It validates a
nonblank path, uses AcceptedOperation/Index, and sets Command to the portable
command or null. Preserve the existing operation Reason as internal evidence;
Rendering selects the new OutputText explanation/manual sentence from typed
operation, subject and command-presence facts. Do not copy an escaping helper
from Extension Install or make a cross-command command builder.

All three inspector callers pass their observation catalogue path. For an
independently indexed overwrite, `RouteDoctorFactReader` already puts the owning
catalogue in `RouteShapeObservation.Path`; RelatedPaths names the overwrite and
must not be used as the Index target. Generated-entry and generated-region
observations likewise carry the catalogue path.

Rendering projects an explicit Index action without a command as a Sentence
using that finding's known catalogue subject. For the generated-navigation and
independent-overwrite finding kinds, an explicit Index action suppresses the
additional generic Index fallback, whether it is a command or a sentence.
Preserve fallback behavior for every other finding. The report's Next selection
still prefers a command, then uses this explicit Index sentence when necessary.
Never present a bare `open-forge index` as a repair for these targeted findings.

N2 owns exactly the following files after N1 transfers its production and unit
files. All paths below are repository-relative; files marked new do not exist
at the freeze:

- `src/cli/operations/OpenForge.Cli.Operations/Commands/Doctor/Shared/Domains/RouteGeneratedEntryDoctorInspector.cs`
- `src/cli/operations/OpenForge.Cli.Operations/Commands/Doctor/Shared/Domains/RouteDoctorInspector.cs`
- `src/cli/operations/OpenForge.Cli.Operations/Commands/Doctor/Shared/Domains/RouteShapeDoctorInspector.cs`
- New `src/cli/operations/OpenForge.Cli.Operations/Commands/Doctor/Shared/Actions/DoctorIndexAction.cs`
- `src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Doctor/Shared/Wording/DoctorWording.cs`
- `src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Doctor/Shared/Selection/DoctorReportSelector.cs`
- New `src/cli/output-text/OpenForge.Cli.OutputText/Doctor/DoctorNavigationText.cs`
- `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Doctor/DoctorWorkspaceRouteMappingTests.cs`
- New `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Doctor/Shared/Actions/DoctorIndexActionTests.cs`
- New `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Doctor/Shared/Rendering/DoctorTargetedIndexAdviceTests.cs`

Reserve IDs `doctor.navigation.index-reason` and
`doctor.navigation.index-manual`, with factories `IndexReason()` and
`IndexManual(string path)`. Preserve every existing ID.

Pure tests cover ordinary paths, a path with spaces, apostrophe, double quote,
dollar, backtick, percent, a control character and an initial hyphen; unsupported
command spellings retain the exact path in the manual instruction. Tests verify
one targeted action, no generic fallback, correct Next and unchanged resolution,
plus an unrelated fallback control. Mapping tests cover region, entry and
independent-overwrite observations with catalogue and child paths deliberately
different. No new filesystem setup is needed for these mappings. Shared build,
format and test execution remain deferred until the mutation wave drains.
