---
open-forge:
  description: Task 39 typed failure facts, representative Extension slice and parallel migration boundaries
  tags: [Memory, Working, Plan, CLI, Contextual]
---

# Task 39: actionable errors

## Outcome

When an operation knows the failing file or source, keep that identity through
to the user-facing error. Explain the known reason and an applicable next step.
Do not infer the subject or failure classification by parsing an operating-system
message. Preserve raw causes in the existing full/debug evidence.

[Task 39](../task39-output-audit.md) already records this direction. The maintainer approved E1–E2 implementation and its representative wording on 2026-09-21. The execution record owns acceptance evidence; E3–E9 remain forecast lanes.

## Verified mechanism and reproduction

`src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Shared/Wording/CliFindingWording.cs`
still contains `PlainCause`, `CauseSentence`, `QuotedPath` and marker-based
classification. It recognizes exception names, HRESULT text and English phrases,
and attempts to recover a path from apostrophes. Its callers include semantic
messages as well as filesystem failures; replacing every caller identically
would lose meaning.

A fresh direct-reference inventory found 36 qualified call sites in 27 Rendering
files across 19 command leaves: 21 `PlainCause` adapters and 15
`CauseSentence` calls. This count excludes helper declarations and
Doctor's unrelated local trim-only method. It is not a count of 36 defects.

The existing Framework already has typed failure facts:

- `Framework/Filesystem/Models/FilesystemFailure.cs`: finite failure kinds and
  bounded diagnostic cause.
- `Framework/Filesystem/TypedReads/Models/FileReadResult.cs`: logical path,
  typed read state and failure.
- `Framework/Extensions/Models/ExtensionSourceModels.cs`: source state, identity,
  packages, cause and domain failure kind.

These paths are below `src/cli/framework/OpenForge.Cli.Framework/`. Reuse their
meaning where present. A missing fact at a result boundary is not justification
for a filesystem abstraction or a new parser.

A concrete Extension example was reproduced with the current Windows Native AOT
binary in an isolated workspace. A local package with `content/` and malformed
`extension.json` makes `extension list --source <package>` return exit 4. The
headline is `Cannot list Extensions: the content is not valid JSON.` The detailed
finding identifies the selected package directory, while its raw cause contains
parser detail. It does not preserve the exact manifest as a structured failure
subject, and it supplies no next action.

The loss is visible in source: `ExtensionManifestFileReader.ReadAsync` knows
`physicalManifest`; `ExtensionSourceReader.ReadPackageAsync` and its catalogue
loop replace the failed read's identity with the selected package/catalogue
identity. Both identities matter and must remain separate. Do not replace the
selected source identity with the child manifest identity.

## Current snapshot distinctions

The bounded inventory identifies twelve generic or locally subjectless fields,
not twelve wholly pathless output frames. Format/detail variants are not counted
as separate situations:

- Two Extension List source-invalid fields lose the precise failing manifest;
  the primary finding still names the selected source directory.
- Five Library List record-invalid aggregate reasons omit the lock-record
  subject, which is already present in the primary finding.
- Three Extension Install/Update/Remove partial-write messages are generic,
  but their finding subjects already name the affected file.
- Two Library Detach/Sync partial-write findings omit the failed file path.
  The Library ID is visible and the lock path appears in effect evidence,
  but the finding itself has `Path = null`.

These are checked-in `standard.txt` situations under the matching command's
`__snapshots__` directory in the integration project. The coordinator inspected
the representative Library List and Detach frames against the worker receipt.
Do not duplicate an adjacent subject merely to make a string-count metric zero.

The Library seam is concrete: `LibraryUnexpectedFailureFact` in Operations
`Commands/Library/Models/Application/LibraryExecutionFacts.cs` retains Stage and
Cause only; `Shared/Application/LibraryMutationApplicationRunner.cs` reduces the
failed receipt before `Detach/Shared/Completion/LibraryDetachCompletion.cs` and
`Sync/Shared/Completion/LibrarySyncCompletion.cs` project a null finding path.
Freeze which receipt identifies the actual failure before assigning E5; do not
choose the first path in a mixed link/directory/generated-region/record plan.

File mutation receipts also retain causes after dropping `FilesystemFailureKind`.
Their shared producers need a single foundation owner before family expansion.
Some non-snapshot Route/Inspect/Attach wrapper chains remain untraced; the direct
caller count is complete, not an assertion that every deeper producer is mapped.

## Proposed internal contract

Keep source selection, semantic status, finding code, cause, effects and public
JSON shape unchanged. Add a narrow optional failure detail at the Extension
source boundary, rather than a universal error engine:

- `ExtensionSourceFailureDetail` contains the exact failing path and a finite
  reason: invalid manifest, invalid UTF-8, access denied, file in use or other
  input/output failure.
- `ExtensionSourceReadResult` retains this optional detail alongside its existing
  selected `Identity` and raw `Cause`. Successful and domain-only outcomes leave
  it absent. Existing dependency-conflict handling keeps its specific meaning.
- Classify while the exception and attempted file are available. A manifest
  `JsonException` is an invalid manifest; do not claim every such exception proves
  syntactically invalid JSON, since required-field validation uses that exception
  too. `DecoderFallbackException` is invalid encoding. Access and I/O remain
  distinct. Do not infer “another process” merely from an arbitrary I/O failure.
- Copy the detail explicitly when the source reader wraps a failed child result.
  No change to traversal, early-exit behavior, cancellation or collected packages.
- Rendering selects wording from these facts. OutputText owns literal wording
  factories. Neither layer reads the filesystem or parses raw causes.

The proposed callable shape is an internal sealed record
`ExtensionSourceFailureDetail(string path, ExtensionSourceFailureDetailKind kind)`
with internal get-only `Path` and `Kind`. Its constructor rejects a blank path or
undefined enum value. The enum has exactly `InvalidManifest`, `InvalidEncoding`,
`AccessDenied`, `FileInUse` and `InputOutput`. Add internal nullable init-only `FailureDetail`
to the existing `ExtensionSourceReadResult`; keep its current constructor and
all existing properties unchanged. This optional fact is populated only by the
specified failed manifest reads, using the attempted `physicalManifest` path.
Wrapping readers copy the same detail while retaining their current selected
source `Identity`. No new reads or path normalization are introduced.

Preserve known sharing failures at the read boundary. For an `IOException` on
Windows, low-word HRESULT 32 or 33 maps to `FileInUse`, following the existing
`LibraryInventoryCollector.IsWindowsSharingDenial` classification. Other I/O
failures remain `InputOutput`; do not parse exception text or infer this Windows
meaning on other platforms. Keep this narrow classification in the manifest
reader; do not change the shared filesystem enum in E1. Real Windows sharing
evidence must prove this branch before acceptance. Report unavailable OS evidence
as unavailable, not a pass.

Before release, the coordinator accepts this internal shape, validates its
direct consumers, and confirms source-generated serialization cannot expose the
new internal detail accidentally. No worker may invent a public schema addition.

## E1: preserve Extension manifest failure facts

One Luna/max worker after that contract freeze. Own exactly:

- `src/cli/framework/OpenForge.Cli.Framework/Framework/Extensions/Models/ExtensionSourceModels.cs`
- New `src/cli/framework/OpenForge.Cli.Framework/Framework/Extensions/Models/ExtensionSourceFailureDetail.cs`
- `src/cli/framework/OpenForge.Cli.Framework/Framework/Extensions/Shared/Manifest/ExtensionManifestFileReader.cs`
- `src/cli/framework/OpenForge.Cli.Framework/Framework/Extensions/ExtensionSourceReader.cs`
- `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Extensions/Shared/Manifest/ExtensionManifestFileReaderIntegrationTests.cs`
- `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Extensions/ExtensionSourceIntegrationTests.cs`

Preserve the exact manifest identity at its read boundary and retain it through
both single-package and catalogue wrappers. Extend the existing malformed
manifest scenarios to assert the detail without weakening the original state,
failure-kind, cause, package and byte-preservation assertions. Add the catalogue
case proving that selected catalogue identity and failed child path differ.
Retain dependency-conflict, cancellation and success behavior. Permission or
sharing cases use existing real-OS fixtures, not a filesystem mock.

E1 edits no Rendering, OutputText, command result, snapshot or contract file.
Its immediate evidence is the precise callsite/diff audit and assertions; the
coordinator builds and runs the focused tests after the mutation wave drains.
Receipt: `.temp/beta-follow-ups/receipts/E1.md`.

## E2: representative Extension List output

One Luna/max worker after E1 acceptance. Assign Operations and Rendering together.
Own exactly:

- `src/cli/operations/OpenForge.Cli.Operations/Commands/Extension/List/Models/ExtensionListResult.cs`
- `src/cli/operations/OpenForge.Cli.Operations/Commands/Extension/List/Shared/Result/ExtensionListResultBuilder.cs`
- `src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/List/Shared/Selection/ExtensionListReportSelector.cs`
- `src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/List/Shared/Wording/ExtensionListWording.cs`
- `src/cli/output-text/OpenForge.Cli.OutputText/Extension/List/ExtensionListWording.cs`
- `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Extension/List/ExtensionListBindingAndPresentationTests.cs`
- `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/List/ExtensionListApplicationIntegrationTests.cs`

Add the same internal nullable init-only `FailureDetail` property to
`ExtensionListFinding`. Copy it only for the selected-source findings built from
`ExtensionSourceReadResult`, preserving `Subject`, `Path`, source selection and
all other fields. Use it for the complete headline and finding sentence. Keep raw cause
evidence at its current detail levels. Avoid composing a whole sentence inside
another whole sentence. Preserve unaffected semantic-cause branches until their
own mapping is specified; do not delete the shared legacy helper yet.

Proposed wording expectations, with the actual path substituted:

| Known reason      | Error meaning                                                     | Useful advice                                               |
| ----------------- | ----------------------------------------------------------------- | ----------------------------------------------------------- |
| Invalid manifest  | `<path> is not a valid Extension manifest.`                       | Check the manifest's required fields and values, then retry |
| Invalid UTF-8     | `<path> is not valid UTF-8.`                                      | Save the manifest as UTF-8, then retry                      |
| Access denied     | `<path> could not be read: permission was denied.`                | Check read access to the named file, then retry             |
| File in use       | `<path> could not be read because it is in use.`                  | Close the program holding the file, then retry              |
| Other I/O failure | `<path> could not be read because a filesystem operation failed.` | Check that the file is accessible, then retry               |

For a selected-source failure carrying the detail, freeze the proposed headline
as `Available Extensions could not be listed from <path>.` Keep its existing
headline kind and status. The finding message is exactly the Error meaning in
the table. The available-packages explanation uses the same typed meaning,
without changing its surrounding section or inventory. Other findings and
installed-source cases retain their current projection in this first slice.

The next action uses the existing `CliNextActionKind.Sentence`; its text is the
corresponding Useful advice in the table, with a final period. Set its Command
and Reason to that sentence, following the existing sentence-action convention.
Do not manufacture a shell command for a manual edit. A later installed-package
inspection action must not be replaced when the selected-source detail does not
apply.

Reserve these new OutputText IDs in the assigned `ExtensionListWording.cs`:
`extension.list.wording.source-failure-headline`,
`extension.list.wording.manifest-invalid`,
`extension.list.wording.manifest-encoding-invalid`,
`extension.list.wording.manifest-access-denied`,
`extension.list.wording.manifest-in-use`,
`extension.list.wording.manifest-io-failed`, and five matching IDs ending in
`.next` for the advice. Use one explicit typed switch in the Rendering wording
owner to choose the factories; undefined enum values throw. Existing IDs and
factories remain for unaffected branches. The coordinator verifies that these
reserved IDs are unused and accepts the complete output frames before release.

Pure result-to-output tests require no disk. The integration test verifies a
real invalid package and unchanged source bytes. Compare text and JSON at
minimal, standard, full and debug: same facts and exit, with raw details confined
to their existing evidence surfaces. Coordinator-owned contract changes and
reviewed snapshot updates land with E2 before its gate is accepted.
Receipt: `.temp/beta-follow-ups/receipts/E2.md`.

## Parallel family expansion after the representative gate

The coordinator must inspect the connected E1/E2 result before repeating its
pattern. Reuse existing typed states in each family; the Extension detail is
not a new type every family must consume. Reserve these non-overlapping logical
lanes, but do not dispatch their mutation work from folder names alone:

| Lane | Scope                               | Shared boundary requiring prior freeze                 |
| ---- | ----------------------------------- | ------------------------------------------------------ |
| E3   | Remaining Extension commands        | E1 source facts, per-command finding maps              |
| E4   | Library List and Inspect            | Existing source/inventory typed failures               |
| E5   | Library Attach, Sync and Detach     | Library write and recovery facts shared with E4        |
| E6   | Framework Install and Update        | Ownership, write-failure and recovery facts            |
| E7   | Route Init, Create, Update and List | Existing source/projected-region failures              |
| E8   | Route Move and Remove               | Exact source/destination and partial-write facts       |
| E9   | Cleanup                             | Recovery-store identity and per-artifact failure facts |

Each lane's release requires an exact file manifest across Operations, Rendering,
OutputText and mirrored tests; every replacement maps a named finding code to
already available facts and reviewed expected output. Pure authored semantic
causes must stay distinct from native exception evidence. Freeze missing facts
at their owning boundary before delegating a consumer. Shared Library, ownership
and recovery files stay with one coordinator-assigned foundation owner, not
multiple family workers. Reserve one receipt per lane using its slice ID.

These are forecast lanes, not falsely complete mutation packets. Their unresolved
producer-to-finding maps and exact expectation sets are the next specification
boundary if the full Task 39 sweep is selected. The first two packets establish
the proposed pattern against an observed defect before that larger commitment.

## Shared retirement and gates

The coordinator owns `CliFindingWording.cs`, its focused unit test, shared
OutputText factories and IDs, shared contracts and the output corpus acceptance.
Doctor navigation files belong to N1–N3 and are excluded here.

Only after every real caller is migrated may the coordinator remove `PlainCause`,
`CauseSentence`, `QuotedPath` and their marker logic. An `rg` result of zero
production references is necessary, not sufficient: inspect callers for newly
copied string heuristics and preserve meaningful operation-authored causes.
Do not rename the old parser or move it into another layer.

At each connected checkpoint, in a quiet tree: build, run focused affected
tests with a positive expected-test minimum, inspect contract/output diffs,
and check all managed suites. At the final gate run `npm run check:dotnet`,
`npm run build:native -- --no-restore` and `npm run test:built` using prepared
dependencies. Record unavailable OS prerequisites separately; never count
platform exclusions as passes. No hosted run is authorized by this plan.

## Stop conditions and divergence

Stop on unknown producer identity, changed status/schema/side effects, missing
ownership, a new shared abstraction, exception-string interpretation, or a
snapshot change without a previously approved situation expectation. Return
those decisions to the coordinator. This investigation establishes a concrete
lost-manifest-subject seam; it does not prove every old pathless-error count is
still current or that all cause-helper callers are erroneous.

## Accepted integration amendment

The representative build verified that Rendering deliberately has no Framework
reference. E2 therefore projects the Framework detail into Operations-owned
`ExtensionListSourceFailureDetail` and `ExtensionListSourceFailureDetailKind`,
with the same path and five meanings, in the result builder. The coordinator owns
`src/cli/operations/OpenForge.Cli.Operations/Commands/Extension/List/Models/ExtensionListSourceFailureDetail.cs`.
Rendering consumes that result fact. This supersedes the proposed shared runtime
type at the E2 boundary; it does not add a project reference or change JSON.
