---
open-forge:
  description: Review C's frozen records and read-command candidate before Root integration
  tags: [Memory, Working, CLI, Tool, Contextual, Evidence, Review]
---

# Records review

Records review is a focused integration review that checks C's records, native inventory and deferred read connections.
For example, a changed native home must keep an earlier previous value attached to its recorded home.

Status: REVIEW_READY. Four actionable findings need C's grouped correction before integration.

## Review target

F and C both have base `6d007254d5ca2937f4874ba5a93161f585814d8c`.
C's candidate has 77 changed or untracked files and stays uncommitted.
All 77 file hashes match `artifacts/tool-lane-c/continuation/source-sha256.txt` in C's tree.
The changed-file inventory matches its Git status.
This is a frozen working candidate for this review, not a candidate commit.

The review reads C's complete packet, the coordinator's nine-fix continuation, accepted Tool and record contracts, the read leaves, workspace Status and Doctor, production consumers, focused tests and six synthetic baselines.
E's clean catalogue commit is `1ec6ad921943b881f819f3a8eb66b4796eeabacb`, with parent equal to this review base.
Its catalogue and client-binding evidence define Root inputs.
A, B and D's mutable candidates aren't reviewed as finished here.
This pass isn't the fresh final adherence review.

Paths below are repository-relative to C unless another lane is named.
Framework paths start at `src/cli/framework/OpenForge.Cli.Framework/Framework/`.
Operations paths start at `src/cli/operations/OpenForge.Cli.Operations/Commands/`.
Rendering paths start at `src/cli/rendering/OpenForge.Cli.Rendering/Presentation/`.

## Findings

### C-R1

Severity: P1, correctness. Location: Framework `Ownership/Shared/Serialization/WorkspaceOwnershipPreservation.cs:23`, with recursive merge at line 26.

The new merge starts with every original member and only overwrites members emitted by the serializer.
Known optional members omitted from the generated result therefore keep their old values.
For example, reading a Library row with `gitIgnore=true`, changing its typed `GitIgnore` to false and writing it leaves `gitIgnore=true` in the file.
`LibraryOwnershipEntry.GitIgnore` omits false at `Ownership/Models/Document/WorkspaceOwnershipEnvelope.cs:81`.
Clearing `FrameworkOwnership.GitIgnoredRoutes` has the same problem because its empty value is omitted at line 53 of that model.
Nullable source and version fields can also keep a value that the typed result cleared.
The special removal of the top-level Framework member at preservation line 15 covers only that one case.

Keep unknown members while treating the generated known fields, including their absence, as the replacement for that known object.
Use the current typed schema to remove omitted known members before merging unknown fields.
Add focused round-trip cases for true to false, nonempty routes to empty and present optional value to absent, with an unknown sibling kept.
The earliest affected boundary is the shared ownership codec used by Framework, Extension and Library writes.
Existing C evidence tests only removal of the whole Framework object, so its green result doesn't cover these transitions.

### C-R2

Severity: P2, contract preservation. Locations: Framework `Tools/Shared/Serialization/ToolRecordJson.cs:37` and `Ownership/Shared/Serialization/WorkspaceOwnershipPreservation.cs:34`.

Unknown nested fields survive only when an array entry matches one of the hard-coded identity keys.
`ToolPathChangeRecord` has `Kind`, `Target` and optional markers, but none of `id`, `changeId`, `launcherPath` or `source`.
Rewriting an unchanged PATH row therefore drops unknown fields on that row and inside its `Prior` or `Applied` objects.
The ownership merge matches only `id`, so unchanged region rows identified by path and region, and Git-ignored route rows identified by directory and entrypoint, also lose their unknown fields.
The record contract requires unknown keys and unrelated data to survive writes.

Match these existing array shapes by their complete typed identity before merging.
Keep exact replacement for opaque receipt JSON values and added items.
Add semantic preservation cases for PATH history, an owned region and a Git-ignored route, including reordered rows and an unmatched identity.
The earliest affected boundary is JSON merge during record publication or a shared ownership rewrite.
`ToolRecordJsonTests` covers native-setting identity, while `UnknownMembersSurvivePublication` covers root and machine members. Neither covers these arrays.

### C-R3

Severity: P2, C# architecture rule. Location: Rendering `Tool/Inspect/ToolInspectPresentation.cs:74`, with further labels at lines 80, 98, 110, 116 and 149 through 170.

Inspect adds authored wording directly in Rendering, including detail labels, unavailable-state descriptions and command headings.
Passing those literals through `ToolInspectText.Detail` or `Setup` leaves that wording outside OutputText's typed factories.
The accepted CLI architecture assigns authored wording to OutputText.

Move these new labels and phrases into the leaf's existing OutputText factories without changing the report facts or wording.
Apply the same correction to the two override-state phrases in `Tool/Status/ToolStatusPresentation.cs:77`.
Recheck only the affected synthetic presentation selection and required formatting checks.
The earliest affected boundary is authored text presentation.
The six reviewed synthetic baselines can stay byte-identical for this move.

### C-R4

Severity: P2, malformed-record handling. Locations: Framework `Tools/Shared/Serialization/ToolMachineValidation.cs:25,38,59`.

Machine JSON can contain a null element in `launchers`, `nativeSettings` or PATH `changes`.
Source-generated deserialization doesn't reject those collection elements under the current options.
The validator then dereferences that element before validating it.
The resulting `NullReferenceException` escapes `ToolRecordReader.ReadAsync`, whose malformed-input catch at line 130 handles JSON and argument errors.
It can also escape writer validation before the result with failure handles is formed.
This turns a malformed record into an unexpected command failure instead of an Invalid record and an Incomplete read result.

Reject null elements once at the machine-record ingress before reading their members, with a cause naming the collection.
Add focused malformed-record cases for those three existing arrays.
Keep compiler-trusted internal values free of repeated checks after that validation.
The earliest affected boundary is deserialization and validation of machine state.
The existing wrong-shape and missing-home cases don't include null array elements.

## Grouped fixes

These checks trace each of the coordinator's nine fixes to source and existing focused evidence:

| Fix                    | Source check                                                                                                                                                                                                                                                                        | Existing evidence and result                                                                                                                                                                                                     |
| ---------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1, deferred backups    | `Tools/Shared/Records/ToolRecordWriter.cs:59-81` prechecks all writes, returns every prepared handle, marks verified replacements and keeps failure handles. It never calls backup completion                                                                                       | `ToolRecordServiceTests.PortableAndMachineRoundTrip`, `ExpectedStateMismatchWritesNothing` and `FailedWriteKeepsCompletedPrefix`. The failure case keeps a verified prefix and an unverified second handle                       |
| 2, native inventory    | `Tools/Shared/Observation/ToolNativePlanReader.cs:18-47` rejects unsuccessful, timed-out, cancelled or failed processes. Required source-generated fields cover all five file arrays, every detected client and hook paths. Advice isn't executed and stderr isn't parsed           | `ToolNativePlanReaderTests`, 15 cases. The process host supplies the output bound. B checks returned paths against reviewed destinations before effects                                                                          |
| 3, native home         | `Tools/Shared/Serialization/ToolMachineValidation.cs:33-48` validates absolute resolved physical homes. `ToolObservationReader.cs:44-64` matches current-home facts for missing-fact checks and passes the complete tool-specific list to B                                         | `NativeHomeBindingIsRequired`, two cases, and `CompleteContextAndRetiredChanges`. Missing and relative bindings are invalid, and other-home facts stay in the input set                                                          |
| 4, Open Forge home     | `Tools/Shared/Records/ToolRecordPaths.cs:22-26,63-75` resolves existing ancestors and puts state directly under the supplied Open Forge home. Read paths create no directories. `ToolRecordWriter.cs:115-139` creates parents only during effects, rechecks and replaces atomically | `MissingRecordsAreReadOnly` checks that the Open Forge home itself stays absent. `PortableAndMachineRoundTrip` checks that another `.open-forge` isn't appended                                                                  |
| 5, native identity     | `Tools/Shared/Serialization/ToolRecordJson.cs:39-65` matches tool ID, change ID and physical-path-comparer home before copying unknown native fields                                                                                                                                | `ToolRecordJsonTests`, four cases, separates same identity, different tool, different home and missing binding. Other array preservation still needs C-R2                                                                        |
| 6, complete reads      | `ToolObservationReader.cs:25,47-69` passes recorded detection, calls the injected context factory, sets installed invocation, keeps native settings and checks retired removals for drift                                                                                           | `SelfUpdateFacts` checks recorded detection. `CompleteContextAndRetiredChanges` checks context, client roots, native environment, installed invocation, kept other-home facts and retired drift                                  |
| 7, Inspect             | Operations `Tool/Shared/Observation/ToolReadProjection.cs:64-140` projects Linux floors, asset bundled runtime, native home policy, inventory, possible outputs, stored reader and command-read declarations. Inspect consumes those typed facts                                    | `ToolReadPresentationTests.ReadReports`, six reviewed synthetic baselines plus semantic assertions. New fact coverage is present. Wording placement needs C-R3                                                                   |
| 8, unavailable records | Operations `Tool/Shared/Observation/ToolReadObserver.cs:36-45` includes invalid and unavailable runtime and PATH records in Incomplete. Missing stays separate. Workspace reader applies the same availability check                                                                | `ToolReadObserverTests`, six cases. Runtime and PATH each cover invalid, unavailable and missing                                                                                                                                 |
| 9, deferred entry      | Three leaf bindings invoke `Func<Tool*Operation>` only from their `Operation` delegate. Invalid List calls the pure catalogue projection. Workspace Status and Doctor accept `Func<ToolWorkspaceObservationReader>` and call it after normal observations                           | `ToolReadBindingTests.DeferredConstruction` checks zero construction during binding and invalid results. `WorkspaceContributorsAreDeferred` checks zero construction before execution and one factory call per workspace command |

The nine requested mechanisms are present.
The findings above prevent treating the complete preservation and placement work as finished.

## Read safety

C's read path calls record reads, bounded detection, manager detection, PATH observation, digest formation and config planning.
It calls no install, latest lookup, apply, remove, backup preparation or record publication method.
C reads no ambient process environment or real known folder.
Its injected recipe-context delegate keeps home and environment resolution in Root.
The Integration doubles throw on effectful channel and config methods, and read tests compare record hashes and absent state folders.
Actual A and B service behavior still needs the later integrated checks.

`ToolReadProjection` reports stored current and desired values separately from `NativeEnvironmentOverrides`.
The six baselines include stored `off` with a deciding synthetic variable whose effective value is `on`.
They print that effective-on value and don't call it effective telemetry off.
The raw `ToolRecipeContext.NativeEnvironment` map isn't projected or persisted by C.
Null and empty normalized maps stay distinct in JSON and text.
B handles precedence, and Root passes only the declared native variables.

## Root connections

The refreshed [Root map](root-integration.md#c-connections) records the exact final constructors and typed binding returns.
The old home mismatch and eager-construction prerequisite are closed in this candidate.
Root can pass factories directly through C's binders and the existing report closure.
It needs neither a second binder nor a privileged service created on help, parser failure or an unrelated command.
F still implements Root after the coordinator integrates checked branches.

E's committed CBM declarations require exact file-valued `<id>-config` bindings as well as client directories.
The map lists those keys and their source locations.
Native inventory validates planned destinations. It doesn't grant an arbitrary returned path permission or choose clients for Root.
The shipping version stays with the maintainer.

## Existing evidence

C's final build log `artifacts/tool-lane-c/continuation/build-accepted.txt` reports zero warnings and errors.
Its exact command is `dotnet build OpenForge.Cli.slnx --configuration Release --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1`.
The packet reports SDK `10.0.101`, managed Windows x64, exit zero and passing `npm ci`.
The local NuGet override is invocation-only.

Each selection uses `dotnet run --project <project> --configuration Release --no-build --no-restore -- --filter-class <class> --minimum-expected-tests <count> --fail-warns on --fail-skips on --no-ansi --progress off`.
Unit uses `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj`.
Integration uses `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj`.
The following logs are under `artifacts/tool-lane-c/continuation/` in C's tree:

| Tier        | Qualified class                                                                                  | Count | Final log                             |
| ----------- | ------------------------------------------------------------------------------------------------ | ----- | ------------------------------------- |
| Unit        | `OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Serialization.ToolPortableCodecTests`       | 10    | `ToolPortableCodecTests.txt`          |
| Unit        | `OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Serialization.ToolRecordJsonTests`          | 4     | `ToolRecordJsonTests.txt`             |
| Unit        | `OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Observation.ToolNativePlanReaderTests`      | 15    | `ToolNativePlanReaderTests.txt`       |
| Unit        | `OpenForge.Cli.Core.UnitTests.Commands.Status.Shared.Aggregation.StatusToolFindingAppenderTests` | 5     | `StatusToolFindingAppenderTests.txt`  |
| Unit        | `OpenForge.Cli.Core.UnitTests.Commands.Doctor.Shared.Domains.DoctorToolFindingAppenderTests`     | 4     | `DoctorToolFindingAppenderTests.txt`  |
| Unit        | `OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Observation.ToolReadBindingTests`             | 4     | `ToolReadBindingTests.txt`            |
| Unit        | `OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Observation.ToolReadObserverTests`            | 6     | `ToolReadObserverTests.txt`           |
| Unit        | `OpenForge.Cli.Core.UnitTests.Commands.Tool.Models.Read.ToolReadPresentationTests`               | 2     | `ToolReadPresentationTests-final.txt` |
| Integration | `OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Records.ToolRecordServiceTests`           | 13    | `ToolRecordServiceTests.txt`          |
| Integration | `OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Observation.ToolReadCommandTests`           | 9     | `ToolReadCommandTests-final.txt`      |

These logs report 50 Unit and 22 Integration cases passed, zero failures and zero skips.
C's packet records warning gates and zero warnings.
The earlier presentation failure is the recorded baseline review, followed by the passing final selection.
Both final dotnet formatting logs contain the required verify-only commands with no diagnostics.
Exit zero comes from C's execution receipt.

Both managed artifact hashes match `test-artifact-sha256.txt`:

- Unit - `artifacts/bin/OpenForge.Cli.Core.UnitTests/release/OpenForge.Cli.Core.UnitTests.dll`, SHA-256 `c8148c41a70b25b4f0b503d13d2d7505c17870fe605953d39fae5d36314a7cb0`.
- Integration - `artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll`, SHA-256 `ac7ff05fae82652ce8e21d34b74a834d517a35c79a3285a9b911fd792e47f91b`.

F consumes this evidence without rerunning a build, test or native process.
Only F's packet, Root map and this new report change.
Final payload fixtures stay for the one integration pass after E's wording.
The coordinator's later Index pass needs to include this new report.
