---
open-forge:
  description: Context output catalogue with the payload stream and its framing
  tags: [Memory, Working, CLI, Task, Plan, G4, Context, Contextual, Active]
---

# 17 — context

> Read [00 — G4 conventions](00-conventions.md) first. Depends on
> [03](03-rendering-system.md).

## Goal

`context` prints the documents, in order, with one delimiter line per source
and nothing else between them. An agent that calls it at task start pays for
the documents only. Findings come before the stream. Nothing is ever
injected into a document.

## Depends on / Blocks

- Depends on: 03. Lane C, first.
- Blocks: 18 (shares the content-part renderer).

## Shape

Data. Exception to the headline rule: at `minimal` there is no headline; the
stream is the answer. `standard` and above print one summary line first.

## Situations

`startup`, `one-source`, `additions-only`, `paths`, `headings`, `section`,
`frontmatter-missing-host` (AGENTS.md; no finding), `section-missing`,
`follow-links`, `broken-followed-link`, `unknown-source` (invalid),
`ambiguous-source` (blocked), `unreadable-source` (incomplete), `invalid-content`.

## Statuses and headlines

| Status                  | When                                                                 | Text                                                                       | Exit | Stream |
| ----------------------- | -------------------------------------------------------------------- | -------------------------------------------------------------------------- | ---: | ------ |
| completed               | everything resolved                                                  | stream only at `minimal`; `standard`: `<N> sources, about <T> tokens.`     |    0 | stdout |
| completed               | empty additions                                                      | `No additional context. The selected sources are already read at startup.` |    0 | stdout |
| completed-with-warnings | a requested section is absent, a link's case differs, an ID collides | warning rows, blank line, stream                                           |    2 | stdout |
| incomplete              | a source or followed link could not be read or parsed                | warning rows, blank line, the safe stream                                  |    3 | stdout |
| invalid-input           | unknown source, bad `--content`, bad depth                           | `Cannot read context: <problem>.`                                          |    4 | stderr |
| blocked                 | ambiguous or unsafe source, overwrite or target                      | `Cannot read context: <reason>.`                                           |    5 | stderr |
| failed                  | unexpected error                                                     | `Context stopped because of an unexpected error: <reason>.`                |    1 | stderr |
| cancelled               | Ctrl+C                                                               | `Context was cancelled.`                                                   |  130 | stderr |

## The stream

```text
=== AGENTS.md ===
<body, byte-exact>

=== .agents/loader.md (loader) ===
<body, byte-exact>

=== .agents/memory/_memory.md (memory) ===
<body, byte-exact>
```

Rules:

- One delimiter per source layer: `=== <path> ===` when the source has no
  ID, `=== <path> (<id>) ===` otherwise, `=== <path> (<id>, overwrite) ===`
  for an overwrite file. One blank line before each delimiter except the
  first.
- The body is written through the authored span, byte-exact, including its
  line endings and any comment markers that belong to the file. A missing
  final newline is not added.
- `--content=paths`: one path per line, no delimiters.
- `--content=headings`: the delimiter, then one line per heading in Markdown
  form (`## Axioms`).
- `--content=metadata`: the delimiter, then `id:`, `route:`, `description:`,
  `tags:` lines from the CLI's view of the source.
- `--content=frontmatter`: the delimiter, then the frontmatter block
  byte-exact. A host file without frontmatter prints nothing under its
  delimiter and raises no finding.
- `--content=section:<name>`: the delimiter, then the section byte-exact. An
  absent section is a warning finding above the stream and nothing under the
  delimiter.
- Nothing else is printed between delimiters: no `Frontmatter: missing`, no
  `Body: available`, no `Order:`.

`standard` adds one summary line before the stream (`19 sources, about 8.0k
tokens.`) and one `included because <reason>` line under each delimiter.

`full` adds `route:`, `scope:`, `order:`, `layer:` lines under each
delimiter, headings with line numbers when headings are requested, and the
followed-link table after the stream.

## Findings catalogue

| Code                           | Severity | Family                | Message                                                                                                                     | Next                                |
| ------------------------------ | -------- | --------------------- | --------------------------------------------------------------------------------------------------------------------------- | ----------------------------------- |
| context.invalid-input          | error    | invalid-input         |                                                                                                                             |                                     |
| context.invalid-source         | error    | unknown-source        | `No source has the ID <ref>.` / `<path> is not under .agents.`                                                              | `open-forge route list --depth=all` |
| context.invalid-content        | error    | local                 | `--content <value> is not a known part. Use metadata, paths, frontmatter, headings, body, or section:<name>.`               | none                                |
| context.invalid-link-depth     | error    | local                 | `--follow-links must be a positive number or all.`                                                                          | none                                |
| context.workspace-unavailable  | error    | workspace-unavailable |                                                                                                                             |                                     |
| context.workspace-unsafe       | error    | workspace-unsafe      |                                                                                                                             |                                     |
| context.source-ambiguous       | error    | source-ambiguous      |                                                                                                                             |                                     |
| context.source-unsafe          | error    | source-unsafe         |                                                                                                                             |                                     |
| context.overwrite-ambiguous    | error    | local                 | `<name>.overwrite.md could belong to more than one base file.`                                                              | fix by hand                         |
| context.target-ambiguous       | error    | local                 | `The link at <path>:l:c could point to more than one file. It was not followed.`                                            | fix by hand                         |
| context.target-unsafe          | error    | local                 | `The link at <path>:l:c points outside the workspace. It was not followed.`                                                 | none                                |
| context.closure-unavailable    | warning  | local                 | `The startup files could not be resolved: <reason>.`                                                                        | `open-forge doctor`                 |
| context.layer-unavailable      | warning  | local                 | `<path> could not be read.`                                                                                                 | `open-forge doctor`                 |
| context.invalid-encoding       | warning  | local                 | `<path> is not valid UTF-8, so it was not included.`                                                                        | fix the file                        |
| context.markdown-unavailable   | warning  | local                 | `<path> could not be parsed as Markdown, so its <part> was not produced.`                                                   | `open-forge doctor`                 |
| context.target-missing         | warning  | local                 | `The link at <path>:l:c points to <destination>, which does not exist. It was not followed.`                                | `open-forge doctor`                 |
| context.fragment-missing       | warning  | local                 | `The link at <path>:l:c points to <file>, which has no heading <#fragment>.`                                                | `open-forge doctor`                 |
| context.link-encoding-invalid  | warning  | local                 | `The link at <path>:l:c has an encoding that cannot be resolved. It was not followed.`                                      | fix by hand                         |
| context.target-unreadable      | warning  | local                 | `The link at <path>:l:c points to <file>, which could not be read.`                                                         | none                                |
| context.section-ambiguous      | warning  | local                 | `<path> has more than one section named <name>. None was included.`                                                         | fix by hand                         |
| context.projection-unavailable | warning  | local                 | `The <part> of <path> could not be produced: <reason>.`                                                                     | `open-forge doctor`                 |
| context.identity-collision     | warning  | identity-collision    |                                                                                                                             |                                     |
| context.target-case-mismatch   | warning  | local                 | `The link at <path>:l:c is written <destination>, but the file is named <actual>.`                                          | `open-forge repair --automatic`     |
| context.frontmatter-missing    | warning  | local                 | `<path> has no frontmatter.` (only when `frontmatter` was requested for a routed source; never for AGENTS.md or the Loader) | none                                |
| context.section-missing        | warning  | local                 | `<path> has no section named <name>.`                                                                                       | none                                |
| context.operation-failed       | error    | operation-failed      |                                                                                                                             |                                     |
| context.interrupted            | error    | interrupted           |                                                                                                                             |                                     |

## Counts

`sources`, `tokens`, `bytes`, `linksFollowed`, `linksNotFollowed`.

## Next rules

Invalid source -> `open-forge route list --depth=all`; unreadable ->
`open-forge doctor`; case mismatch -> `open-forge repair --automatic`;
otherwise none.

## JSON data by level

| Level    | `data`                                                                                                                          |
| -------- | ------------------------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ sources: [ { path, id, layer: "base" \| "overwrite", parts: [ { part, text \| headings: [ ... ] \| paths: [ ... ] } ] } ] }` |
| standard | + per source `includedBecause: [ ... ]`, `route`, `scope`                                                                       |
| full     | + `order`, headings with `line`, `links: [ { from, location, destination, resolvedPath, resolution, followed } ]`               |

Text values are exact; JSON escaping is the serializer's.

## References

- `src/cli/core/OpenForge.Cli.Core/Commands/Context/Shared/Rendering/*` — replaced by `Presentation/Context/`, with `ContentPartsTextRenderer` shared with 18.
- Context interface, Human output, Structured output, Compact JSON Output (ledger only).

## Preconditions

- [ ] 03 merged.

## Steps

1. [ ] Write `ContextReportSelector` and `ContextDataTextRenderer` with the
       delimiter rules and authored spans.
2. [ ] Extract `ContentPartsTextRenderer` under `Presentation/Shared/Content/`
       for reuse by `find`.
3. [ ] Delete the old renderers.
4. [ ] Regenerate snapshots and review. Assert byte-exact bodies with CRLF
       and LF fixtures.
5. [ ] Three suites green.

## Acceptance

- [ ] `minimal` startup output contains nothing but delimiters, blank lines and bodies.
- [ ] A pristine install exits 0 with no findings.
- [ ] Bodies round-trip byte-exact.

## Changes ledger

- stream: legacy Context registration -> typed Presentation/Context report and neutral content blocks; minimal now emits only selected framing/body spans.
- JSON data: legacy result graph -> typed `data.sources` with standard/full metadata, order and link fields; authored values remain serializer-escaped only.
- counts: Context result did not carry aggregate counts -> typed nullable `ContextCounts` from the selected raw graph with strict UTF-8/scalar token accounting and link follow disposition.
- source metadata: Context projection dropped authored metadata -> carries existing metadata state, description and tags without a rescan.
- content fidelity: the new draft placeholder for absent authored text was removed; missing authored text emits no payload, and generated framing remains separate from authored spans.
- tests: added three focused Context presentation tests covering byte fidelity, count propagation and full JSON shape.
- evidence: `dotnet build src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj -c Release --no-restore -p:OpenForgeSkipDevelopmentPublish=true` exited 0 with 0 warnings and 0 errors (`artifacts/g4-17-context-review-unit-build.log`); `./artifacts/bin/OpenForge.Cli.Core.UnitTests/release/OpenForge.Cli.Core.UnitTests.exe --parallel none --no-ansi --progress off --minimum-expected-tests 1 --filter-class OpenForge.Cli.Core.UnitTests.Presentation.Context.ContextPresentationTests` exited 0 with 4 total, 4 passed, 0 failed and 0 skipped (`artifacts/g4-17-context-review-focused-unit.log`); `dotnet build OpenForge.Cli.slnx -c Release --no-restore` exited 0 with 0 warnings and 0 errors (`artifacts/g4-17-context-review-solution-build.log`).
- selection: `ContextReportSelector` now selects each physical layer's typed parts once, carries typed part kind into both text and JSON projections, and derives recovery commands from finding codes (`route list --depth=all`, `doctor`, or `repair --automatic`) without changing operation behavior.
- finding messages: invalid-source selection carries parser/resolver form and state through `ContextFinding.InvalidSourceKind`; only a resolver-confirmed unknown ID uses the catalogue `UnknownSource` wording, while malformed or unresolved exact paths retain their captured cause through the generic `CannotRead` factory. No current typed fact establishes an outside-.agents boundary, so the catalogue boundary wording is not inferred.
- headline filtering: a single warning finding remains visible at minimal/attention/incomplete output; only statuses with a concrete finding headline suppress the duplicate finding row.
- evidence: latest Unit build exited 0 with 0 warnings and 0 errors (`artifacts/g4-17-context-rctx10-unit-build-4.log`); focused Unit binding/presentation checks were 26/26 passing (`artifacts/g4-17-context-rctx10-focused-unit-3.log`); latest Integration build exited 0 with 0 warnings and 0 errors (`artifacts/g4-17-context-rctx10-integration-build-4.log`); focused Integration Context checks were 16/16 passing (`artifacts/g4-17-context-rctx10-focused-integration-3.log`).
- snapshots: `ContextBeforeOutputSnapshotTests.Selection` now gathers all 14 situations in one collector and writes 112 primary all-detail text/JSON snapshots plus 11 bounded Debug diagnostic snapshots under `src/cli/tests/integration/snapshots/ContextBeforeOutputSnapshotTests/Selection/`; capture passed (`artifacts/g4-17-context-snapshot-capture-rctx10-3.log`) and verify-only replay passed (`artifacts/g4-17-context-snapshot-verify-rctx10.log`).
- boundary fidelity: `ContentPartsTextRenderer` inserts a generated separator only between adjacent authored spans when the previous span has no line break and the next span does not begin one; CRLF/LF and missing-final-newline authored bytes remain unchanged. Link wording now uses the authored source path/location for the origin and the captured target path for the destination.
- evidence: R-CTX11/12 Unit build exited 0 with 0 warnings and 0 errors (`artifacts/g4-17-rctx11-12-unit-build.log`); Integration build exited 0 with 0 warnings and 0 errors (`artifacts/g4-17-rctx11-12-integration-build.log`); published E2E build exited 0 with 0 warnings and 0 errors (`artifacts/g4-17-rctx11-12-e2e-build.log`); `PublishedContextProcessTests` passed 3/3 (`artifacts/g4-17-rctx11-12-published-context-e2e.log`); affected Context snapshots recaptured and verify-only replay passed (`artifacts/g4-17-rctx11-12-context-snapshot-capture.log`, `artifacts/g4-17-rctx11-12-context-snapshot-verify.log`).

- final affected gate: `./artifacts/bin/OpenForge.Cli.Core.UnitTests/release/OpenForge.Cli.Core.UnitTests.exe --parallel none --no-ansi --progress off --minimum-expected-tests 1 --fail-warns on --fail-skips off` remained 26/26 passing (`artifacts/g4-17-rctx11-12-focused-unit.log`); the focused Context Integration command with `ContextOperationFindingTests`, `ContextOperationIntegrationTests`, `ContextGeneratedSerializationTests`, `ContextCliResultHelpTests` and `ContextBeforeOutputSnapshotTests` exited 0 with 17 total, 17 passed, 0 failed and 0 skipped (`artifacts/g4-17-rctx11-12-focused-integration-3.log`); `PublishedContextProcessTests` exited 0 with 3 total, 3 passed, 0 failed and 0 skipped (`artifacts/g4-17-rctx11-12-published-context-e2e.log`).
- R-CTX11/12 final review: generated boundary separators now keep adjacent no-final-newline authored layers distinct without rewriting authored bytes; link findings use the authored source path/location for the origin and the captured destination independently. Snapshot recapture and verify-only replay both passed (`artifacts/g4-17-rctx11-12-context-snapshot-capture.log`, `artifacts/g4-17-rctx11-12-context-snapshot-verify.log`).

- final command-scoped gates: Unit `./artifacts/bin/OpenForge.Cli.Core.UnitTests/release/OpenForge.Cli.Core.UnitTests.exe --parallel none --no-ansi --progress off --minimum-expected-tests 1 --fail-warns on --fail-skips off --filter-trait Feature=context` exited 0 with 32 total, 32 passed, 0 failed and 0 skipped (`artifacts/g4-17-context-command-unit.log`); Integration `./artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.exe --parallel none --no-ansi --progress off --minimum-expected-tests 1 --fail-warns on --fail-skips off --filter-trait Feature=context` exited 0 with 52 total, 52 passed, 0 failed and 0 skipped (`artifacts/g4-17-context-command-integration-3.log`).

## Divergences observed

- plan: replace/delete old Context renderers; reality: obsolete Context Legacy renderers and old command-owned presentation graph files are deleted, the typed composer is wired, and all-detail snapshots are now captured in the new snapshot root for root review.
- plan: old command result had no aggregate count model; implementation added optional `ContextCounts` to preserve existing constructor callsites while exposing final typed report counts.
- scope: no authoritative semantic scope exists in the current graph; JSON retains `scope: null` and text omits absent scope rather than inferring it.
- evidence: the existing `ContextApplicationIntegrationTests` now assert the native schema/detail shape, flattened layers, top-level findings/counts, direct links, raw repeat/detail invariants, authored order, no-write behavior and semantic exits.

## Rollback

Restore the bridge registration for context.
