---
open-forge:
  description: Accepted read-only Interface for inspecting one Extension ID across the installed record, the workspace, and the package
  responsibility: Define inspect's stable-ID syntax, exact source handling, result graph, finding vocabulary, fingerprint compatibility, and conformance
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Extension, Inspect, Interface, ReadOnly, Source, Lifecycle, CurrentTruth]
---

# extension inspect Interface Contract

## Status And Authority

This is the current Crystallized Interface Contract for read-only
`open-forge extension inspect`. It owns the exact syntax, stable-ID subject,
source selection, installed and available projections, dependency facts,
comparison result, generated-navigation boundary, finding vocabulary, status,
`next` values, structured result graph, examples, and public conformance. The
command does not ship yet; this contract does not claim implementation or
Gate-5 evidence.

The sibling [Behavior Contract](behavior.md) defines technology-neutral
resolution, fingerprint formation, comparison, retention, and result
formation. The [Extension group entrypoint](../_extension.md) defines group help
only. [Global CLI Flags](../../shared/global-flags/interface.md) defines shared
flags and terminal behavior. The [Shared Result
Coordinates](../../shared/result-coordinates/interface.md) define the shared JSON
envelope, source-location primitive, and status/process coordinates. The [CLI
Architecture](../../../architecture.md) defines parser, filesystem, concrete
serialization, and evidence boundaries.

## Purpose And Boundary

`inspect` gives package-specific read-only detail before an install, update, or
remove decision. It reports one exact stable package ID from installed
lifecycle facts, one selected package source, or a comparison of both when the
required facts are trusted and complete.

It may report installed-only, available-only, or current/intended
facts. It never installs, updates, removes, creates, repairs, adopts, indexes,
locks, writes, downloads, executes package content, or turns a recommendation
into authority. An installed fact can remain visible without a healthy current
Framework, but that fact does not grant mutation trust.

## Syntax

```text
open-forge extension inspect <stable-id> [--source <package-or-catalogue-path>] [global flags]
```

Exactly one stable-ID operand is required. `--source` is one exact external
package or catalogue read location. There is no `--all`, `--automatic`,
`--dry-run`, `--force`, `--prune`, wizard, or mutation flag.

The shared flags are `--workspace <path>`, `--format json`,
`--detail <minimal|standard|full|debug>`, `--detail debug`, `--help`, and `--version`. Their grammar,
defaults, repetition, terminal behavior, and no-op rules remain in [Global CLI
Flags](../../shared/global-flags/interface.md).

The stable ID uses the accepted Extension identity grammar: lowercase ASCII
letters and digits in non-empty segments separated by single hyphens, with a
maximum length of 128 characters. It is an exact package identity, not a folder
name, path spelling, version selector, fuzzy query, or semantic
recommendation. A malformed, missing, duplicated, or conflicting manifest ID
never supplies an inferred subject.

## Subject And Source

The subject is the exact supplied stable ID. Inspect resolves it only against
the selected source universe and the selected workspace's installed lifecycle
facts. No path, folder, version, proximity, registry, cache, network, ambient
search, glob, resemblance, or embedded fallback supplies a different identity.

Without `--source`, Inspect reads installed facts from the exact workspace and
available facts from the embedded catalogue. With `--source`, it reads only
that exact package or catalogue and proves that source is lexically and
physically disjoint from the target workspace. An explicit source remains the
only source for that invocation, even when it is missing, malformed, or
unreadable.

The selected source may contain one requested package or a catalogue containing
it. Dependency closure is resolved only inside that one source universe. A
multi-package source never selects another package by folder spelling, source
order, version, or proximity.

The CLI distribution embeds Framework and first-party Extension assets with
deterministic inventory and hash proof. That proof identifies distributed
source assets; it is not evidence of a selected workspace's current
installation or of a proven runtime implementation.

## Installed, Available, And Comparison Views

Installed-only retains receipt membership and current path observations when
the selected package source is unavailable. Available-only retains the selected
source without inventing installation. Installed-and-available compares current
workspace bytes with intended package bytes; the mode name identifies package
sources and remains unchanged. No stored baseline participates.

Missing, malformed, unreadable or uninterpretable ownership is informational.
The result retains incomplete ownership coverage, unavailable installed facts,
empty installed arrays and null installed counts. A complete lock with no
matching receipt means no recorded installation. Neither case establishes that
manually copied files are absent. Source and physical-path failures retain their
independent meanings. Old state files are unrelated user content.

External destinations retain exact-byte comparison and the existing
semantic-only update recommendation boundary. Known empty counts are `0`;
unavailable counts are `null`. Arrays remain present in partial results.

## Structured Result Data

Inspect's command-owned data is part of the shared native result. With
`--format json`, the report is one schema-3 envelope on stdout. The envelope
carries `schemaVersion: 3`, `command: "extension inspect"`, the current result
`status`, `workspace`, `detail`, `filter`, `data`, `findings`,
`effects`, `counts`, `limitations`, `recovery`, and `next`; applicable
members remain present with `null` where the catalogue says the fact is
unavailable. The envelope does not use the retired `result` member or the
retired earlier status set. Text and JSON carry the same command facts.

The `data` member retains the Inspect result graph: subject and source
selection, installed and available package facts, dependency closure, path
observations, comparison relations, fingerprints, and generated-region
observations. Unknown and unavailable values remain explicit. Counts are
`0` only for known empty collections and `null` when coverage is
unavailable. The command's current finding vocabulary and next-action rules are
defined in the propagated output contract below.

## Fingerprint Policy: `open-forge-markdown-v1`

`open-forge-markdown-v1` is one immutable fingerprint policy. The policy string
is its version. Any change to byte admission, UTF-8 handling, BOM or NUL
handling, line-ending rules, CommonMark parser validation, generated-region
recognition, byte omission, hash algorithm, hash encoding, or fallback changes
the policy value and requires a new policy version.

### Admitted Markdown and fallback boundary

For a Markdown package path, the policy first receives the exact original byte
sequence. It does not pre-normalize bytes. Strict UTF-8 decoding is required.
A leading UTF-8 BOM (`EF BB BF`) is retained as the decoded U+FEFF at its exact
position; BOM presence and absence are distinct bytes and are not normalized.
Any NUL byte is a binary boundary. Invalid UTF-8, NUL, unsupported package kind,
binary input, unparseable input, or an invalid generated-region boundary uses
the exact-byte fallback.

The exact-byte fallback hashes the original bytes exactly. It does not normalize
line endings, strip a BOM, remove NUL, parse Markdown, omit generated content,
or add a final newline. Its result has `kind: "exact-bytes"`, its hash is the
lowercase SHA-256 of the original bytes, and its policy remains the selected
`open-forge-markdown-v1` policy when the fallback is reported as an Inspect
observation. A fallback never proves semantic equivalence or mutation
authority.

Admitted Markdown is parsed with one fixed CommonMark parser configuration.
Parser extensions, permissive decoding, renderer output, YAML reserialization,
Unicode normalization, whitespace trimming, frontmatter rewriting, heading
rewriting, tag rewriting, link rewriting, code rewriting, and final-newline
insertion are not part of this policy.

### Byte normalization and generated region

After strict decoding and fixed parser validation, the only ordinary byte
normalization is line-ending normalization: every CRLF pair and every lone CR
becomes one LF. LF remains LF. No other Unicode scalar, whitespace byte,
frontmatter byte, heading byte, tag byte, link or destination byte, code byte,
or final-newline byte changes.

One top-level canonical ATX `## Entries` heading owns the body from the end of
its heading span to the next top-level heading of level 1 or 2, or EOF. Fenced,
indented-code, quoted, nested-list, Setext, differently cased and differently
leveled lookalikes do not establish this semantic section. Trailing horizontal
heading whitespace and an initial BOM are accepted. Duplicate `## Entries`
headings are diagnosed; no arbitrary first section is selected.

Absent sections receive semantic hashing of the complete normalized document.
Duplicate sections produce invalid-region exact-byte fallback. A valid section
omits exactly `[end-of-Entries-heading-span, next-same-or-higher-heading-or-EOF)`
in normalized UTF-8 coordinates. Its heading and every outside byte remain.
The interval may be empty. Old guard comments inside this body have no special
fingerprint meaning. Encode UTF-8 without adding a preamble, retaining admitted
U+FEFF content, then render SHA-256 as 64 lowercase hexadecimal characters.

### Equivalence and persistence

Two fingerprints are equivalent only when their policy values, kinds, and
lowercase SHA-256 values are equal. A semantic fingerprint and an exact-byte
fingerprint are never equivalent, even if their digest strings happen to match.
An unavailable fingerprint is not equal to anything. Equal exact-byte hashes
prove equal bytes for that operation; they do not prove semantic equivalence,
ownership, or a safe mutation.

Only ownership receipts are persisted in the lock. Current and intended hashes,
including fallback hashes, are operation-time facts and are never written by
Inspect. Public fingerprints retain only `sha256`; kind, policy and origin are
internal comparison mechanics. The accepted [Workspace State Files](../../../../../decisions/framework/workspace-state-files.md)
decision removes the stored third side entirely. Equality never grants mutation
authority. The Markdown policy and golden byte vectors below are unchanged.

## Human Output

The command uses the shared native report. The default detail is `minimal`; `standard`, `full` and `debug` add the catalogue-defined facts. `--detail-filter <error|warning|info|all>` is repeatable and changes only the rendered detail. Use `--format text` for this text report. Primary result text for `completed`, `completed-with-warnings` and `incomplete` is on stdout; primary errors for `invalid-input`, `blocked`, `failed` and `cancelled` are on stderr. There is no `Status:` line.

### Statuses and headlines

| Status                  | When                                                          | Headline                                                                                            | Exit | Stream |
| ----------------------- | ------------------------------------------------------------- | --------------------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | installed and equal                                           | `<id> <version> is installed and matches the package.`                                              |    0 | stdout |
| completed               | available, not installed                                      | `<id> <version> is available and not installed.`                                                    |    0 | stdout |
| completed               | no ownership record                                           | `<id> <version> is available. No ownership record exists, so installation cannot be checked.`       |    0 | stdout |
| completed-with-warnings | files changed, missing, new or retired                        | `<id> <installed version> is installed. <N> files need attention.` (+ ` Version <v> is available.`) |    2 | stdout |
| incomplete              | source, manifest, dependency or file facts unreadable         | `<id> is installed, but the comparison could not finish: <limitation>.`                             |    3 | stdout |
| invalid-input           | bad ID, bad source, extra operand                             | `Cannot inspect <ref>: <problem>.`                                                                  |    4 | stderr |
| blocked                 | ambiguous identity, source overlap, cycle, ownership conflict | `Cannot inspect <id>: <reason>.`                                                                    |    5 | stderr |
| failed                  | unexpected error                                              | `Extension inspect stopped because of an unexpected error: <reason>.`                               |    1 | stderr |
| cancelled               | Ctrl+C                                                        | `Extension inspect was cancelled.`                                                                  |  130 | stderr |

### Text by level

`minimal`, equal:

```text
development 0.1.0 is installed and matches the package.
  3 files under .agents/workflows
```

`minimal`, differences:

```text
toolkit 1.0.0 is installed. 2 files need attention. Version 2.0.0 is available.
  Warning  .agents/changed.md   changed since it was installed
  Warning  .agents/retired.md   no longer part of the package
Next: open-forge extension update toolkit --dry-run
```

`standard` adds `Workspace:`, the source (`bundled with this CLI` or the
path), dependencies with their state, and every file with `unchanged`,
`changed`, `missing`, `new in the package` or `no longer part of the package`.

`full` adds both SHA-256 values per path, the manifest path, the resolution
order, the Entries sections the package registered in, and the record
coverage.

### Representative transcripts by status

### Transcript — completed

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-inspect-completed). [Matching reviewed capture](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Inspect/__snapshots__/ExtensionInspectBeforeOutputSnapshotTests/PackageRelationship/installed-matches.minimal.txt).

### Transcript — completed-with-warnings

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-inspect-completed-with-warnings). [Matching reviewed capture](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Inspect/__snapshots__/ExtensionInspectBeforeOutputSnapshotTests/PackageRelationship/installed-changed-and-retired.minimal.txt).

### Transcript — incomplete

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-inspect-incomplete). [Matching reviewed capture](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Inspect/__snapshots__/ExtensionInspectBeforeOutputSnapshotTests/PackageRelationship/installed-source-missing.minimal.txt).

### Transcript — invalid-input

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-inspect-invalid-input). [Matching reviewed capture](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Inspect/__snapshots__/ExtensionInspectBeforeOutputSnapshotTests/PackageRelationship/invalid-input.standard.txt).

### Transcript — blocked

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-inspect-blocked). [Matching reviewed capture](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Inspect/__snapshots__/ExtensionInspectBeforeOutputSnapshotTests/PackageRelationship/ambiguous-source.minimal.txt).

### Transcript — failed

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-inspect-failed).

### Transcript — cancelled

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-inspect-cancelled).

## Structured Output

`--format json` writes one schema-3 envelope to stdout for every report status. It contains the command, status, workspace when applicable, detail, filter, command data, findings, effects, counts, limitations, recovery facts and next action as applicable. It is the same typed result as the text report; no ordinary text is mixed into the JSON document. If parsing fails before binding, the raw parser diagnostic remains text on stderr and no report envelope exists.

### JSON data by level

| Level    | `data`                                                                                                                                      |
| -------- | ------------------------------------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ id, installed { version } \| null, available { version } \| null, source { kind, path }, matches: bool, files: [ { path, relation } ] }` |
| standard | + `dependencies: [ { id, version, state } ]`, `name`, `description`                                                                         |
| full     | + per file `installedSha256`, `packageSha256`, `manifestPath`, `resolutionOrder`, `registeredIn: [ path ]`, `recordCoverage`                |

## Semantic Results

The status and exit mapping above are unchanged by detail or format. Root effects and recovery receipts retain their complete result facts at every detail level; command-owned data follows the catalogue's level rows.

### Counts and limitations

`filesUnchanged`, `filesChanged`, `filesMissing`, `filesNew`, `filesRetired`,
`dependencies`.

## Errors And Boundaries

The findings catalogue below is the command's finite error and warning vocabulary. Findings keep their code, severity, family, subject and cause; detail filtering affects display only. A blocked, failed or cancelled result prevents further effects according to the catalogue.

### Findings catalogue

| Code                                         | Severity | Family                  | Message                                                                                     | Next                                                 |
| -------------------------------------------- | -------- | ----------------------- | ------------------------------------------------------------------------------------------- | ---------------------------------------------------- |
| extension-inspect.invalid-input              | error    | invalid-input           |                                                                                             |                                                      |
| extension-inspect.invalid-stable-id          | error    | local                   | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Inspect/Shared/Wording/ExtensionInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-inspect.invalid-stable-id`).           | `open-forge extension list`                          |
| extension-inspect.package-unavailable        | error    | unknown-id              | [`extension.inspect.phrase.no-extension-has-the-id-in`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/Inspect/ExtensionInspectPhrases.cs); [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Inspect/Shared/Wording/ExtensionInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-inspect.package-unavailable`).                                                 | `open-forge extension list`                          |
| extension-inspect.package-invalid            | error    | local                   | [`extension.inspect.phrase.the-package-at-is-invalid`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/Inspect/ExtensionInspectPhrases.cs); [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Inspect/Shared/Wording/ExtensionInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-inspect.package-invalid`).                                          | fix by hand                                          |
| extension-inspect.source-invalid             | error    | local                   | [`extension.inspect.phrase.is-not-an-extension-package-or-package-folder`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/Inspect/ExtensionInspectPhrases.cs); [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Inspect/Shared/Wording/ExtensionInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-inspect.source-invalid`).                           | none                                                 |
| extension-inspect.source-unavailable         | warning  | local                   | [`extension.inspect.phrase.the-source-could-not-be-read-so-the-comparison-could-not-finish`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/Inspect/ExtensionInspectPhrases.cs); [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Inspect/Shared/Wording/ExtensionInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-inspect.source-unavailable`).                  | none                                                 |
| extension-inspect.source-ambiguous           | error    | local                   | [`extension.inspect.phrase.is-found-more-than-once-in`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/Inspect/ExtensionInspectPhrases.cs); [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Inspect/Shared/Wording/ExtensionInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-inspect.source-ambiguous`).                                                 | fix by hand                                          |
| extension-inspect.source-overlap             | error    | local                   | [`extension.shared.phrase.is-inside-the-workspace-and-cannot-be-used-as-a-source`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/Shared/CanonicalPhrases.cs); [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Inspect/Shared/Wording/ExtensionInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-inspect.source-overlap`).                            | none                                                 |
| extension-inspect.workspace-unavailable      | error    | workspace-unavailable   |                                                                                             |                                                      |
| extension-inspect.workspace-unsafe           | error    | workspace-unsafe        |                                                                                             |                                                      |
| extension-inspect.identity-ambiguous         | error    | local                   | [`extension.inspect.phrase.the-ownership-record-names-in-a-way-that-cannot-be-matched-to-one-package`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/Inspect/ExtensionInspectPhrases.cs); [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Inspect/Shared/Wording/ExtensionInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-inspect.identity-ambiguous`).           | `open-forge doctor`                                  |
| extension-inspect.ownership-conflict         | error    | ownership-conflict      |                                                                                             |                                                      |
| extension-inspect.ownership-observation      | info     | ownership-observation   |                                                                                             |                                                      |
| extension-inspect.path-changed               | warning  | local                   | [`extension.inspect.phrase.changed-since-it-was-installed`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/Inspect/ExtensionInspectPhrases.cs); [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Inspect/Shared/Wording/ExtensionInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-inspect.path-changed`).                                                    | `open-forge extension update <id> --dry-run`         |
| extension-inspect.path-missing               | warning  | local                   | [`extension.inspect.phrase.missing-it-was-installed-by`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/Inspect/ExtensionInspectPhrases.cs); [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Inspect/Shared/Wording/ExtensionInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-inspect.path-missing`).                                                 | `open-forge extension update <id> --dry-run`         |
| extension-inspect.path-new                   | warning  | local                   | [`extension.inspect.phrase.new-in-the-package-not-installed-yet`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/Inspect/ExtensionInspectPhrases.cs); [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Inspect/Shared/Wording/ExtensionInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-inspect.path-new`).                                             | `open-forge extension update <id> --dry-run`         |
| extension-inspect.path-retired               | warning  | local                   | [`extension.inspect.phrase.no-longer-part-of-the-package`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/Inspect/ExtensionInspectPhrases.cs); [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Inspect/Shared/Wording/ExtensionInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-inspect.path-retired`).                                                     | `open-forge extension update <id> --prune --dry-run` |
| extension-inspect.path-invalid               | error    | local                   | [`extension.inspect.phrase.in-the-package-is-not-a-valid-workspace-path`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/Inspect/ExtensionInspectPhrases.cs); [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Inspect/Shared/Wording/ExtensionInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-inspect.path-invalid`).                                      | fix by hand                                          |
| extension-inspect.path-unavailable           | warning  | local                   | [`extension.inspect.phrase.could-not-be-read`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/Inspect/ExtensionInspectPhrases.cs); [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Inspect/Shared/Wording/ExtensionInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-inspect.path-unavailable`).                                                                 | `open-forge doctor`                                  |
| extension-inspect.fingerprint-unavailable    | warning  | local                   | [`extension.inspect.phrase.could-not-be-compared`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/Inspect/ExtensionInspectPhrases.cs); [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Inspect/Shared/Wording/ExtensionInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-inspect.fingerprint-unavailable`).                                                             | `open-forge doctor`                                  |
| extension-inspect.fingerprint-fallback       | info     | local                   | [`extension.inspect.phrase.was-compared-byte-for-byte-because-it-is-not-markdown`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/Inspect/ExtensionInspectPhrases.cs); [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Inspect/Shared/Wording/ExtensionInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-inspect.fingerprint-fallback`).                             | none                                                 |
| extension-inspect.generated-boundary-invalid | error    | generated-region-unsafe |                                                                                             |                                                      |
| extension-inspect.dependency-changed         | warning  | local                   | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Inspect/Shared/Wording/ExtensionInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-inspect.dependency-changed`).                      | `open-forge extension update <id> --dry-run`         |
| extension-inspect.dependency-conflict        | error    | local                   | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Inspect/Shared/Wording/ExtensionInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-inspect.dependency-conflict`).                           | fix by hand                                          |
| extension-inspect.dependency-cycle           | error    | local                   | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Inspect/Shared/Wording/ExtensionInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-inspect.dependency-cycle`).                                                     | fix by hand                                          |
| extension-inspect.dependency-incomplete      | warning  | local                   | [`extension.inspect.phrase.the-dependency-of-could-not-be-resolved-it-was-not-compared`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/Inspect/ExtensionInspectPhrases.cs); [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Inspect/Shared/Wording/ExtensionInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-inspect.dependency-incomplete`). | `open-forge doctor`                                  |
| extension-inspect.operation-failed           | error    | operation-failed        |                                                                                             |                                                      |
| extension-inspect.interrupted                | error    | cancelled               |                                                                                             |                                                      |

## Scenarios

### Catalogue situations

`installed-matches`, `installed-changed-and-retired`, `available-not-installed`,
`installed-source-missing` (incomplete), `newer-available`, `dependency-cycle`
(blocked), `unknown-id` (invalid-input), `no-ownership-record` (info),
`ambiguous-source` (blocked), `invalid-input`.

Each status has one representative native text transcript above. JSON uses the same status and command facts under the schema-3 envelope.

### Open maintainer questions

The `ambiguous-source` situation currently produces the `identity-ambiguous` finding. The contract records that current mapping without deciding whether the situation should use a different finding. **Maintainer decision remains open.**

## Fingerprint Golden Byte Vectors

These vectors use one exact notation: printable ASCII is shown literally and
every non-ASCII or control byte is shown as `\xNN` in byte order. The hash is
the lowercase SHA-256 of the normalized or exact-fallback bytes identified in
the `Output bytes` column. They are calculated vectors, not implementation
evidence.

| Vector                            | Input bytes                         | Treatment                        | Output bytes            | SHA-256                                                            |
| --------------------------------- | ----------------------------------- | -------------------------------- | ----------------------- | ------------------------------------------------------------------ |
| LF baseline                       | `alpha\x0A`                         | admitted Markdown                | `alpha\x0A`             | `b6a98d9ce9a2d9149288fa3df42d377c3e42737afdcdaf714e33c0a100b51060` |
| CRLF equals LF                    | `alpha\x0D\x0A`                     | CRLF to LF                       | `alpha\x0A`             | `b6a98d9ce9a2d9149288fa3df42d377c3e42737afdcdaf714e33c0a100b51060` |
| Lone CR equals LF                 | `alpha\x0D`                         | lone CR to LF                    | `alpha\x0A`             | `b6a98d9ce9a2d9149288fa3df42d377c3e42737afdcdaf714e33c0a100b51060` |
| Authored whitespace               | `alpha\x20\x20\x0A`                 | whitespace retained              | `alpha\x20\x20\x0A`     | `a1d36921b09507031f6a0d2ecbda13dac0d41b20318f6138ccbf3f01907deb5c` |
| Authored final-newline difference | `alpha`                             | final newline not added          | `alpha`                 | `8ed3f6ad685b959ead7022518e1af76cd816f8e8ec7ccdda1ed4018e8f2223f8` |
| Unicode                           | `caf\xC3\xA9\x0A`                   | Unicode retained                 | `caf\xC3\xA9\x0A`       | `7b49b9e063bd91a4f9252b413261f5557b9c570aa61516989499f64a62dbcdd6` |
| Leading BOM retained              | `\xEF\xBB\xBFalpha\x0A`             | BOM retained; no preamble added  | `\xEF\xBB\xBFalpha\x0A` | `9eff3bdb19b9bef9372b96a1244c0f0f939acdb8f539551e0a099a5b20a3862e` |
| Valid generated interior          | `## Entries\x0A\x0A- generated\x0A` | omit heading body                | `## Entries`            | `2a38f622e9c1d12f9d062fa1b6062b6f3c2ceaa0de30b6687c77a9ee66039b71` |
| Duplicate sections                | `## Entries\x0A\x0A## Entries\x0A`  | invalid region; exact fallback   | same bytes              | `7ab5dc3f8dbe26ecf1a0580e88774bf5e5c175bf05276a5033de2ea9d14aa1f7` |
| Unsupported bytes                 | `{}\x0D`                            | unsupported kind; exact fallback | `{}\x0D`                | `f545623b541a21d6b8b415ee1793b91001a50ca985d26fad253c3c68aba5ffe9` |
| Binary NUL                        | `\x00a\x0D\x0A`                     | binary/NUL; exact fallback       | `\x00a\x0D\x0A`         | `c4cbb7cbfda0feb8dde8cd2e8abfb0771fc2c04fe50720acc3b83971937b8ac3` |
| Invalid UTF-8                     | `\xFF\x0D\x0A`                      | invalid UTF-8; exact fallback    | `\xFF\x0D\x0A`          | `1320b5dc13aa91dbac6eabc346cb655592aef8244a8ed04b8c4b3bdd59b8af4c` |
| Deterministic repeat, first run   | `repeat\x0Avalue`                   | admitted Markdown                | `repeat\x0Avalue`       | `d9884573b6ea5e967e594532570e613d871fe38fc980df9ac05423e0c2559f38` |
| Deterministic repeat, second run  | `repeat\x0Avalue`                   | admitted Markdown                | `repeat\x0Avalue`       | `d9884573b6ea5e967e594532570e613d871fe38fc980df9ac05423e0c2559f38` |

The LF, CRLF, and lone-CR rows have equal output bytes and hashes. The
whitespace, Unicode, BOM, and final-newline rows demonstrate that no other
source bytes are normalized. The generated row retains its heading. The
duplicate-section, unsupported, binary, and invalid-UTF-8 rows retain exact bytes,
including CR and NUL where present. Repeated input yields the same bytes and
hash.

## Non-Goals And Public Conformance

Inspect does not select a package from multiple IDs, resolve a semver update,
infer trust, reconstruct lifecycle state from files outside the exact lifecycle
document, invoke another public command, or write package, workspace,
lifecycle, generated-navigation, lock, backup, temporary, recovery, or
diagnostic state. It does not execute package content or access network,
registry, cache, ambient search, or legacy lifecycle input.

Conformance must demonstrate, with the same typed result for human and JSON:

- exact stable-ID grammar, one operand, singleton-source repetition, terminal
  modes, and all global-flag rules;
- exact CWD and `--workspace` selection, source package/catalogue
  classification, lexical and physical disjointness, aliases, containment, and
  no explicit-source fallback;
- installed-only, available-only, trusted installed-and-available, valid empty-section,
  source-unavailable, malformed, ambiguous, unsupported, and event states;
- forgiving lock reads, informational unavailable ownership, duplicate and unsafe
  claim handling, receipt membership, and untouched leftover state files;
- offline one-source dependency closure, duplicate and cycle rejection, stable
  dependency-first order, package metadata, payload inventory, target paths,
  current physical identity, and exact-byte observations;
- every command-local member and nested member in frozen order, nullability,
  empty-array, known-zero, and unavailable-null rules;
- all 28 finding codes, one status per code, duplicate and unknown-value
  rejection, global ordering, within-code tie-breaks, aggregate precedence,
  safe fact retention, and the exhaustive `next` table;
- `open-forge-markdown-v1` admission, BOM/NUL/UTF-8 boundaries, fixed
  CommonMark validation, line-ending-only normalization, exact heading and
  generated-region treatment, fallback, fail-closed equivalence, persistence
  boundaries, and every golden byte vector;
- trusted complete actionable recommendation gating, with no update
  recommendation for fallback, unavailable, blocked, invalid-input, or event states;
- human minimal and standard output, JSON parity, one JSON document per status,
  stream and process-exit mapping, bounded diagnostics, no prompts, repeat
  determinism, and recursive unchanged-state/no-write checks; and
- supported source-generated serialization and Native-AOT/process evidence at
  Gate 5, without treating this contract or its examples as proof.

The [Shared Result Coordinates](../../shared/result-coordinates/interface.md) own
the envelope, source-location shape, and status exit map. The [CLI
Architecture](../../../architecture.md) owns parser/runtime boundaries and
system-level evidence. This Interface owns the Inspect result, finding,
fingerprint, scenario, and `next` meanings above.



## Executable Wording References

Exact wording is owned by the linked typed factories. Selection, output coordinates and behavioral requirements remain in this contract and its existing semantic owners. The independent fixture preserves the original reviewed message forms.

CLI help syntax: [`extension.inspect.help.syntax`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/Inspect/ExtensionInspectText.cs).

<!-- @OpenForgeTextRef extension.inspect.help.syntax -->
<!-- @OpenForgeTextRef extension.inspect.phrase.changed-since-it-was-installed -->
<!-- @OpenForgeTextRef extension.inspect.phrase.could-not-be-compared -->
<!-- @OpenForgeTextRef extension.inspect.phrase.could-not-be-read -->
<!-- @OpenForgeTextRef extension.inspect.phrase.in-the-package-is-not-a-valid-workspace-path -->
<!-- @OpenForgeTextRef extension.inspect.phrase.is-found-more-than-once-in -->
<!-- @OpenForgeTextRef extension.shared.phrase.is-inside-the-workspace-and-cannot-be-used-as-a-source -->
<!-- @OpenForgeTextRef extension.inspect.phrase.is-not-an-extension-package-or-package-folder -->
<!-- @OpenForgeTextRef extension.inspect.phrase.missing-it-was-installed-by -->
<!-- @OpenForgeTextRef extension.inspect.phrase.new-in-the-package-not-installed-yet -->
<!-- @OpenForgeTextRef extension.inspect.phrase.no-extension-has-the-id-in -->
<!-- @OpenForgeTextRef extension.inspect.phrase.no-longer-part-of-the-package -->
<!-- @OpenForgeTextRef extension.inspect.phrase.the-dependency-of-could-not-be-resolved-it-was-not-compared -->
<!-- @OpenForgeTextRef extension.inspect.phrase.the-ownership-record-names-in-a-way-that-cannot-be-matched-to-one-package -->
<!-- @OpenForgeTextRef extension.inspect.phrase.the-package-at-is-invalid -->
<!-- @OpenForgeTextRef extension.inspect.phrase.the-source-could-not-be-read-so-the-comparison-could-not-finish -->
<!-- @OpenForgeTextRef extension.inspect.phrase.was-compared-byte-for-byte-because-it-is-not-markdown -->
