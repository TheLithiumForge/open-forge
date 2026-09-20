---
open-forge:
  description: Task 30 structural slice B1 execution plan for replacing the generated-index comment guards with heading-based region lookup and consolidating the second Markdown parser
  tags: [Memory, Working, CLI, Task, Plan, Contextual, Active, Structural, Parsing]
---

# B1 — Heading-based Entries, guards deleted

> Read [00 — Slice conventions](00-conventions.md) first: verification
> commands, pass conditions, the recurring composition pattern, and the rules
> every slice shares. This plan does not repeat them.

> Historical B1 receipt: the qualification commands later in this record used
> the then-current serial runner setting. The maintained delivery path now
> runs all six suites with `--parallel collections`.

## Goal

The generated `Entries` region is found by its `## Entries` heading through the
Markdig AST. The `<!-- open-forge:generated-index:start -->` and `:end` comment
guards are retired from the active contract, from every shipped document, and
from the old parser. A minimal input-only detector supports automatic removal
inside heading-owned bodies, as accepted on continuation. The Prettier containment that exists only for the guards is removed.

## Depends on / Blocks

- Depends on: A6, so the lock work is not fighting a document migration. B1 may
  start earlier only if the maintainer accepts the overlap.
- Blocks: nothing before G4. G4 may proceed in parallel once B1 lands.

## References

Authorizing evidence: [Structural requirements and markers](../../../../emerging/analysis/cli-experience-audit/structural-requirements-and-markers.md),
section 2 — the same file that requires markers already locates `## Axioms` by
heading with no markers at all, so two mechanisms exist for one job and the
marker-based one is the one that breaks.

Maintainer direction, 2026-09-12: the guards are going away entirely.

Marker definition and consumers:

- `Framework/Documents/Markdown/MarkdownGeneratedRegionSyntax.cs:8` — `MarkerPrefix`.
- `Framework/Documents/Markdown/MarkdownGeneratedRegionParser.cs`
- `Framework/Sources/Loading/SourceGeneratedEntriesParser.cs`
- `Framework/Sources/Routing/SourceLoaderEntriesParser.cs`
- `Framework/GeneratedNavigation/GeneratedNavigationRegionPlanner.cs`
- `Framework/Extensions/Operational/ExtensionBridgeRegistrationObservationReader.cs:29`
- `Framework/Documents/Markdown/MarkdownFingerprintReader.cs`
- `Commands/Find/Shared/Matching/FindBodyTagScanner.cs`
- `Commands/Route/Init/Shared/Planning/RouteInitScaffoldComposer.cs:33` — the
  scaffold that emits the guards for new documents.

Second parser to consolidate in the same slice (structural item 3): the Route
Inspect Axioms parser under `Commands/Route/Inspect/Models/Profile/`. Both walk
Markdown headings; after B1 they answer the same question and should share one
owner.

Recorded symptom this fixes: Prettier inserts a blank line after the start marker
and before the end marker; `index` writes the region without them. A workspace
formatted and then indexed churns forever. Contained today by `.prettierignore`
fences on `src/open-forge/` and `src/extensions/`.

## Preconditions

- [x] Three suites green.
- [x] `open-forge index` is byte-stable today: run it twice on `.agents`, second
      run reports zero updates. Record the number of regions.

## Steps

1. [x] Add heading-based region location to the Markdig-owned reader: the region is
       the content between the `## Entries` heading and the next heading of the same
       or higher level, or end of document. Keep it beside the existing Axioms
       heading lookup so one owner answers both. Verify: `npm run build`.

2. [x] Add unit tests for the locator before using it: heading present and empty,
       heading absent, heading at a different level, BOM, CRLF and LF, trailing
       whitespace, a fenced code block containing a `## Entries` line that must not
       match, and two `## Entries` headings, which must be a reported diagnostic and
       not a silent first-match. Verify: unit suite green.

3. [x] Switch `GeneratedNavigationRegionPlanner` to the heading locator, writing the
       region without markers. Verify: `npm run build`.

4. [x] **Migration.** `index` must rewrite an existing marked document to the
       unmarked form once, removing both marker lines, and be byte-stable on the
       second run. Add an integration test that runs `index` twice over a workspace
       seeded with marked documents and asserts the second run reports zero updates.
       Verify: integration suite green.

5. [x] Switch the remaining readers: `SourceGeneratedEntriesParser`,
       `SourceLoaderEntriesParser`, `FindBodyTagScanner`,
       `ExtensionBridgeRegistrationObservationReader`, `MarkdownFingerprintReader`.
       Verify: `npm run build`, three suites green.

6. [x] Stop emitting guards in `RouteInitScaffoldComposer.cs:33`. Verify: e2e green.

7. [x] Consolidate the Route Inspect Axioms parser onto the same Markdig owner, or
       record in Divergences why it must stay separate. Compare headings, fences,
       BOMs and trailing whitespace with focused tests **before** deleting either
       helper. Verify: three suites green.

8. [x] Migrate the shipped payload: run `index` over `src/open-forge/` and
       `src/extensions/*/content/` so the shipped documents carry no markers.
       Verify: payload parity tests green — these compare embedded bytes against the
       staged source copy, so **delete the stale staged copies first**
       (`artifacts/bin/OpenForge.Cli.IntegrationTests/release/FrameworkPayloadSource`
       and `ExtensionCatalogue`) or `PreserveNewest` will serve yesterday's bytes.

9. [x] Migrate this repository's own `.agents` tree. Verify: `index` twice, second
       run zero updates.

10. [x] Delete `MarkdownGeneratedRegionSyntax` and the marker parser. Verify:
        `git grep -n "generated-index" src/cli` returns nothing.

11. [x] Remove the `src/open-forge/` and `src/extensions/` fences from
        `.prettierignore`, then run Prettier over the payload and confirm `index` is
        still byte-stable afterwards. This is the whole point of the slice: prove the
        two tools no longer fight. Verify: integration suite green, `index` twice
        clean after a Prettier run.

12. [x] Update the contracts describing the region: search
        `contracts/` and `documents/framework/markdown/` for the marker syntax.
        Verify: `git grep -l "generated-index" .agents/memory/crystallized` returns
        nothing.

## Expected result

`## Entries` sections carry a generated list with no comment markers. Running
Prettier and then `index`, in either order, any number of times, produces stable
bytes. `.prettierignore` no longer fences the payload.

## Acceptance

- [x] Shipped and local generated Entries use heading boundaries only; old guard
      comments are removed automatically during Index migration. Explicit migration
      detection and focused old-input fixtures may name the retired tokens.
- [x] `index` is idempotent after a Prettier run — asserted, not assumed.
- [x] Two `## Entries` headings produce a diagnostic, not a silent first match.
- [x] One Markdown owner answers both Entries and Axioms, or the separation is
      recorded with its reason.
- [x] Payload parity tests green with freshly staged copies.
- [x] Three suites green.

## Divergences observed

- **Precondition: repository metadata blocker.** The plan expected two successful
  pre-change Index runs. Both returned incomplete (exit 3): 146 regions,
  144 planned updates, zero applied. `.agents/memory/emerging/authors-findings/claude.md`
  lacks metadata; its route explicitly protects maintainer notes. Evidence:
  `artifacts/b1-before-repository-index-{first,second}.json`. No finding text was
  changed or deleted. Asked whether to add only frontmatter while preserving
  the note exactly. The maintainer explicitly authorized that metadata-only
  correction on continuation; its body was preserved byte-for-byte. Both reruns
  completed: first 143 updates, then zero updates across 146 verified regions
  (`artifacts/b1-unblocked-index-{first,second}.json`).
- **Execution order.** B1 and M1 are independent after G1. While the protected
  note's treatment was pending, executed M1 first. The active plan allowed either
  order; B1 production had not started at that point.

- **Additional acceptance boundary; stopped before behavior.** The maintainer
  authorized the note metadata and continuation to G4 only if no further blocker
  appears. B1 Acceptance literally requires that no shipped or local document
  contain the retired marker text; Steps 10/12 additionally propose zero textual
  matches. The authorizing sealed analysis itself quotes `MarkerPrefix` in
  `structural-requirements-and-markers.md`, and the required migration evidence
  needs marked input. The Loader requires sealed analysis to remain provenance,
  while this task's Acceptance is explicitly binding. Do not silently turn the
  literal requirement into an active-syntax-only check or rewrite historical
  evidence to make a grep pass.
  Resolution proposed at that stop, before the acceptance below: remove all active guards,
  runtime marker support and current/shipped contract requirements; retain literal
  text only as clearly historical evidence and focused migration fixtures.
  Record and test those explicit exclusions. No B1 parser, scaffold, payload,
  public-output or contract behavior change had started at that stop.

### Maintainer Resolution On Continuation

The maintainer directed a complete migration to headings, retaining a small
old-comment detection/removal path so existing projects migrate automatically.
Current tests and contracts are updated to the new form. This supersedes the
literal zero-text-match acceptance and Steps 10/12 where they would prevent
explicit migration support or its focused fixtures. Old guards do not establish
ownership or section boundaries; headings do. Historical quoted examples remain
provenance rather than an active format, and all actual generated sections in
the repository and shipped payload migrate. No further approval is needed for
this accepted implementation. The former stop-before-G4 boundary is superseded
by the completed G4 packet.

### Implementation Discoveries

- **Additional reader.** The plan lists the Framework readers but omits
  `RouteInspectGeneratedEntriesReader`'s own marker-adjacency check. It rejected
  heading-only bodies. Removed that check and consumed shared Entries facts so
  every reader follows the accepted boundary.
- **Axioms consolidation.** Six focused before cases passed against the original
  helper (`artifacts/b1-axioms-before.log`): LF/CRLF and fenced headings behaved
  as expected; initial BOM and trailing heading spaces were missed, and a later
  level-1 heading remained in Axioms. The shared Markdig section owner corrects
  those structural boundaries. Missing, empty, inherited-sentinel and substantive
  classifications remain distinct; their deferred meaning decision is untouched.
  The focused cases were run before deleting the old helper. Its physical path
  was `Shared/Profile`, rather than the plan's older `Models/Profile` location.
- **Additional Axioms consumer.** `RouteSourceStructureReader` already used Markdig
  sections but independently selected visible-text Axioms headings, including
  nested or noncanonical lookalikes. Its structural selection now uses the same
  semantic-section owner as Route Inspect; its existing required/missing/empty
  validity rules are preserved. This closes the structural duplication without
  implementing the deferred Axioms meaning decision.
- **Fixture migration.** Old grammar tests asserting ordered comment pairs,
  final-only regions or retained marker output are replaced with heading-boundary
  and migration evidence. Shared document seeds emit canonical blank-separated
  heading bodies; production marker mechanics leave Inspect output. Historical
  before snapshots remain in Git, and old-input fixtures explicitly test removal.
- **Formatter exception.** B1 Step 11 expressly retires the conventions' temporary
  payload formatting fence. Prettier runs only after the heading migration, as
  the slice requires; the broader prohibition was containment for the old bug.

- **Execution batching.** Steps 1–6 prescribe building and proving each consumer
  switch separately. The shared reader, writer, consumers and replacement grammar
  evidence were developed as one uncommitted behavior packet, then rebuilt and
  tested together. This avoids maintaining an interim dual grammar. Before-output
  evidence was still captured and committed before any production change.
- **Payload selection.** Framework Index selected its Loader and all 20 reachable
  regions. Extension content trees have no Loader, so explicit detached entrypoint
  operands selected one region each in memory-starters, planning and
  project-documents. Development and orchestration have no entrypoint targets;
  development-toolkit is manifest-only. Their remaining payload files were still
  included in the formatting/stability inventory. The repository migrated all 146
  regions; its second Index run had zero updates. The local CLI entrypoint Template
  is an authoring example rather than an indexed region and was edited explicitly.
- **Staged payload refresh.** Step 8 requested deleting the two stale staged
  directories. Automatic approval review rejected that recursive removal before
  execution, so it was not retried. The project-declared inventories were checked
  exactly instead, then existing file bytes were refreshed directly: 23 Framework
  files (20 changed) and 36 Extension files (13 changed). The project explicitly
  excludes the catalogue README. All staged bytes then matched source. No files
  were deleted; the parity gate remains mandatory after rebuilding.
- **Formatting scope.** Removing the payload fence also exposes existing Markdown
  table alignment and JSON array formatting. Prettier changed those layouts only;
  package identities, dependencies and authored wording were preserved. Hash
  assertions covered all 60 payload files through Prettier→Index→Index→Prettier
  and a repeated cycle (`artifacts/b1-payload-stability.json`): every Index pass
  reported zero updates and neither tool changed the stabilized bytes.

### Fixture Audit Correction

- **B1-R1 — material evidence defect, fixed.** Before behavior commit, reviewed
  the changed tests against `e3d6777f`. Mechanical comment removal had made the
  ambiguous inputs valid in `ExtensionInstallTargetPolicyIntegrationTests` and
  `ExtensionRemoveRegressionIntegrationTests`; the latter could still block
  because its metadata was absent. `ExtensionPermissionDestinationIntegrationTests`
  had also lost its syntax-like external input. These passing tests no longer
  proved their named boundaries. Replaced their input with duplicate Entries
  headings, supplied valid metadata to isolate the intended failure, and renamed
  the affected scenarios. Assertions for refusal, unchanged bytes and opaque
  external content remain. The earliest invalidated boundary was these three
  Integration fixtures; rebuilt and repeated managed/native qualification after
  correction. No production behavior changed in this correction.

- **B1-R2 — material contract drift, fixed.** The final contract audit found
  generated-marker wording without literal retired tokens in Route Init/Create/Move/Remove,
  References and Install, plus a duplicated Index section heading from editing.
  Replaced those generated-boundary claims with the shared heading-owned body
  and corrected the heading. Kept the separate managed workspace-block markers
  and frozen old-CLI compatibility prose. The initial token search was too narrow;
  reviewed surrounding marker/guard/Entries language across current contracts.
  This correction changes prose only and accompanies B1 behavior in its commit.

## Execution Applicability

This is a shared Markdown and public-output integration wave in the non-shipping
CLI. It reuses Markdig, existing document facts, bounded mutation/recovery, and
current fingerprint normalization; no dependency or platform workaround was
added. The expressly accepted old-input line detector is limited to migration
inside heading-owned sections. Markdown parsing and fingerprint vectors use Unit
evidence; real Index migration, refusal, unchanged authored bytes and payload
parity use Integration; published help/scaffold and command journeys use EndToEnd.
Full managed and supported Windows Native AOT qualification is required because
shared parsing, embedded payload and Inspect output changed. Git retains before
bytes; Index's existing recovery and bounded write behavior remain the recovery
boundary for existing workspaces. Historical quotations are not active syntax.

## Preparation Evidence

The completed M1 managed baseline supplies the beginning three-suite gate:
3349 Unit, 1846 Integration (17 skipped), 131 EndToEnd; failed 0 throughout.
Its build had zero warnings/errors and its format check retained the specified
five inherited diagnostics. B1's final full managed/native wave is recorded below.

Reviewed every precheck-generated document diff: all bytes outside the current
generated boundaries remain unchanged. The only authored edit is the expressly
authorized note metadata. No runtime parser or payload change is included.

The requested automatic-metadata issue belongs to [Task 30 Phase 5 diagnosis
and interoperability](phase-5-diagnosis-and-interoperability.md), scheduled after
G4 with no calendar date. Its source analysis names missing frontmatter under
Index's safe structural fixes and records the no-new-flag division of labor.
That is the later behavior packet; this preparation does not implement it.

Before behavior, reviewed and captured four real generated-boundary Inspect
projections (compact/expanded human/JSON), one Route Init scaffold byte snapshot,
and published Index/Inspect help. The preexisting four Inspect snapshots remained
unchanged. The fresh managed build passed with zero warnings/errors; targeted
capture passed 9 tests and immutable full Unit verification passed 3354, failed 0,
skipped 0 (`artifacts/b1-before-{final-build,capture,unit}.log`). These baselines
and their harness are committed separately before changing production.

### Migration Review

Verified exact authored prefixes for all 169 changed entrypoints: 146 repository,
20 Framework and 3 Extension sections (`artifacts/b1-authored-prefix-review.json`).
No shipped payload contains the retired guard tokens. Runtime literal tokens are
confined to the minimal input detector; three focused test sources retain old
input for bootstrap, fingerprint and real Index migration evidence. Current
contracts and the local entrypoint Template use the new form. Historical Analysis,
archived evidence and committed before snapshots retain their provenance.

Reviewed published Index and Inspect help before/after: only Index's unique
heading-body rewrite and automatic guard removal note, and Inspect's heading
exclusion description, change. Arguments, options, streams and exits are unchanged
(`artifacts/b1-{index,inspect}-help.diff`). The original checkout remains clean.
Fresh native and managed help are byte-equal after LF normalization. The stored
review copies also trim line-end spaces introduced by help wrapping, so comparing
raw output directly with those copies initially differed by one wrapped-line
space; comparing fresh executions and applying the documented capture formatting
both passed (`artifacts/b1-native-help-equality.json`).

### Reproducing The Formatter Stability Assertion

After a fresh build, run this Python check from the worktree. It uses the actual
CLI and repository Prettier, asserts zero Index updates after formatting, and
compares all payload bytes after each tool in both orders. Markdown meaning is
selected and validated by the CLI; the script supplies canonical entrypoint paths.

```python
from pathlib import Path
import hashlib
import json
import subprocess

cli = Path("artifacts/bin/OpenForge.Cli/release/OpenForge.Cli.exe").resolve()
payloads = [Path("src/open-forge"), Path("src/extensions")]
roots = [payloads[0], *sorted(payloads[1].glob("*/content"))]

def hashes():
    return {
        p.as_posix(): hashlib.sha256(p.read_bytes()).hexdigest()
        for root in payloads for p in root.rglob("*") if p.is_file()
    }

def prettier():
    subprocess.run([
        "node", "node_modules/prettier/bin/prettier.cjs", "--write",
        *map(str, payloads)
    ], check=True, capture_output=True)

prettier()
stable = hashes()
for _ in range(2):
    for root in roots:
        operands = []
        if root.name == "content":
            operands = sorted(
                p.relative_to(root).as_posix()
                for p in (root / ".agents").rglob("*.md")
                if p.name == "_" + p.parent.name + ".md"
            )
            if not operands:
                continue
        run = subprocess.run([
            str(cli), "index", *operands, "--workspace", str(root.resolve()), "--json"
        ], check=True, capture_output=True, text=True, encoding="utf-8")
        assert json.loads(run.stdout)["result"]["counts"]["updates"] == 0
    assert hashes() == stable
    prettier()
    assert hashes() == stable
```

### Reviewed Snapshot Delta

Before commit: `e3d6777f`. The final reviewed snapshot delta is five files,
9 added lines and 21 removed lines. The four preexisting Inspect output snapshots
and generated compact human output are unchanged. Generated JSON removes
`startMarker`, `endMarker` and `markerLinesRetained`; the migrated fixture's
normalized body offsets change from 65–94 to 21–52 and excluded bytes from 29 to 31. The expanded human output loses only marker rows. Inspect help names heading
exclusion. Route Init scaffold loses its two guards and their extra blank lines.
Statuses, findings, path relations and computed comparison hashes do not change
in these projections. Frozen fingerprint vectors separately qualify the new
heading-owned omission and preserve ordinary authored-byte normalization.

## Verification Receipt

Working root: `<workspace>/open-forge-worktrees/task30-a4-extension-readers`;
branch: `task30-a4-extension-readers`; before commit: `e3d6777f`. Toolchain:
.NET SDK `10.0.101`, Node `v26.3.1`, Windows `win-x64`. The managed build command
was `npm run build -- --no-restore`; it completed with zero warnings/errors.
The three immutable suites used the exact assembly commands in the conventions,
with `CI=true`, no snapshot updates, sequential Integration/EndToEnd execution,
and all tests selected. No cancellation flake occurred.

| Check                  | Managed result                                                                                         | Native result                                     |
| ---------------------- | ------------------------------------------------------------------------------------------------------ | ------------------------------------------------- |
| Unit                   | 3366 succeeded, failed 0, skipped 0                                                                    | 3366 succeeded, failed 0, skipped 0               |
| Integration            | 1849 succeeded, failed 0, skipped 17 (1866 total)                                                      | 1849 succeeded, failed 0, skipped 17 (1866 total) |
| EndToEnd               | 131 succeeded, failed 0, skipped 0                                                                     | 131 succeeded, failed 0, skipped 0                |
| `npm run check:dotnet` | Exit 2, exactly the five inherited whitespace errors; no new errors. The chained analyzer did not run. | Not a native suite                                |

The format locations remain `ReferencesOperation.cs:459,460` and
`ExtensionListApplicationIntegrationTests.cs:83,84,85`; neither file was changed.
Counts describe discovered/executed evidence and are never pass conditions.
Managed logs: `artifacts/b1-managed-{build,unit,integration,e2e}.log` and
`artifacts/b1-dotnet-check.log`.

Native build command: `npm run build:native -- --rid win-x64 --offline`.
Native qualification uses these independent test projects with `CI=true`:

```text
dotnet artifacts/delivery/win-x64/build/unit/OpenForge.Cli.Core.UnitTests.dll --parallel none --no-ansi --progress off --minimum-expected-tests 1
./artifacts/publish/win-x64/integration/OpenForge.Cli.IntegrationTests.exe --parallel none --no-ansi --progress off --minimum-expected-tests 1
./artifacts/publish/win-x64/end-to-end/OpenForge.Cli.EndToEndTests.exe --parallel none --no-ansi --progress off --minimum-expected-tests 1
```

The Unit assembly remains managed in this supported native qualification lane;
Integration and EndToEnd are published native runners, with EndToEnd targeting
the freshly published native CLI. This is an integration-wave qualification,
including M1's previously deferred native evidence, not a packaging or release
claim. No push or merge is authorized or performed. G4 is now complete.

### Final Qualification And Handoff

After the final fixture correction, rebuilt and reran all three complete managed
and native suites. All six executions passed, with zero failures, the specified
17 Integration skips and no other skips. No flake rerun was needed. Managed
build: zero warnings/errors; native build: exit 0 with no reported warnings or
errors. The five inherited format diagnostics remain the only check exception;
the chained analyzer did not run. No production, test, fixture, payload or build
configuration changed after these qualifying builds.

The build manifest was validated against current source and artifact closures
before staging (`artifacts/b1-accepted-evidence-identity.json`). Source parent:
`e3d6777f5e7eef78f532bfca7357e6a21522801f`; dirty source/configuration identity:
`10f6f901e960654f8e43859f6214f496c6c8ee5ce398733ec56a1b8d25f47645`. Qualified native SHA-256 identities:

| Artifact                           | SHA-256                                                            |
| ---------------------------------- | ------------------------------------------------------------------ |
| OpenForge.Cli.exe                  | `cdbf2b08893e2914193d78a71b44cec237f0711d08b5adc15696f3066e5deeb0` |
| OpenForge.Cli.IntegrationTests.exe | `4d85a66ead5b69fb799c5c825ee505c84d544dba9126bc9bc8e54d80a94be0c5` |
| OpenForge.Cli.EndToEndTests.exe    | `83235b4d35cd11ccd58f89c8af077fd7beba12301e6d94f5b81ace493e41d677` |

The supported Unit lane uses its managed assembly from the native build closure.
The validated closure identities are:

| Closure                                    | SHA-256                                                            |
| ------------------------------------------ | ------------------------------------------------------------------ |
| `artifacts/delivery/win-x64/build`         | `6391417a386000cb187aba531eca92f1675aed3145d2cdab51d3887bcefdf88b` |
| `artifacts/publish/open-forge-dev/Release` | `74ed6421022a84d30211a111f3954f85d3f67dba194dd12e9b75dcfed35d06c3` |
| `artifacts/publish/win-x64`                | `7a0fe0a5b19f7c119b532b6c604ff1ec9f72916a6e484e682fc49b9cbefed1ea` |

Final repository Index, after task-state and contract formatting, updated one
route description and verified 146 regions. Its repeat verified all 146 with
zero updates (`artifacts/b1-final-repository-index-{first,second}.json`). The
runtime/payload inputs and qualified binaries did not change.

The manifest's `tested` flag remains false because these suites were invoked directly; this is recorded
execution evidence, not a release-manifest promotion. Native logs are
`artifacts/b1-native-{build,unit,integration,e2e}.log`.

Steps are checked against their accepted outcomes and the divergences above.
Not performed as originally written: individual builds after every consumer
switch, recursive removal of staged directories, and literal zero-token searches
that would remove migration fixtures or historical provenance. Their replacements
are recorded and verified. The obsolete Doctor malformed/misplaced vocabulary is
left for its catalogue/presentation boundary; it does not restore the removed
parser or define ownership. No legacy state-file migration, Axioms meaning change,
TypeScript grammar change, or loader/AGENTS relocation was added.

B1 is complete. M1's deferred native integration-wave evidence is also complete.
Stop before opening G4; the maintainer owns its next contract and evidence gate.

## Rollback

Revert before step 8. After the payload migration a rollback must also revert the
migrated documents, so treat steps 8 to 12 as one unit.
