---
open-forge:
  description: Unaccepted consolidated G4 output proposal drawn from the astra, opus and fable proposals, with the shared rules, the per-command matrix, the corrected transcripts, the decisions that remain, and an implementation handoff
  tags: [Memory, Working, CLI, Task, Proposal, Presentation, Contextual, Candidate]
---

# Task 30 G4 — Consolidated Output Proposal

## Status

Unaccepted. This proposal merges three independent proposals for the
[G4 view layer](../../../../archived/cli-development/tasks/task30/phase-4b-g4.md):

- [astra](phase-4b-g4-output-proposal-astra.md): a report-only specification
  from contracts and source, with a status coverage matrix and a 28-row
  selection matrix. No current output was captured.
- [opus](phase-4b-g4-output-proposal-opus.md): the shortest statement of the
  model, built on three content classes, with the finding-model question named
  as the first decision. Transcripts derived from renderer source and unit
  snapshots.
- [fable](phase-4b-g4-output-proposal-fable.md): the only proposal with
  transcripts captured from a build of the current tree, plus two
  implementation discoveries. It kept `--json` and `--verbose`, which the
  maintainer had already directed away from in the astra session.

Written 2026-09-14 against `d5824851`. Nothing here is approved. It changes no
renderer, escaper, contract or test.

## Which proposal this follows most

This consolidation follows **astra** most closely, for three reasons that
matter to the implementer more than style:

1. It was right where the other two were wrong about current behavior. Ordinary
   `update` replaces changed owned files and restores missing ones without
   `--force` (`UpdatePlanningPolicy` lines 59 and 60; the Update interface says
   `--force` grants no additional authority). `route move` rewrites references
   that would otherwise break, and `route remove` detaches incoming links. The
   fable transcripts for those three commands were built on the audit-era
   "kept your changes" story and are corrected below.
2. It preserves the safety facts the other two were willing to compress: every
   changed path in a mutation receipt at every level, in both formats, and no
   advice to rerun a mutation to see what it did.
3. It found the stale public documentation, the parser-failure stream
   exception, and the existing interactive paths, all of which change the
   handoff.

Two astra choices are not carried: its `--severity` and `--limit` options with
per-level caps (deferred, see [C6](#c6-caps-and-doctor-filters)), and its
natural-text `find` rows (see [C10](#c10-find-rows)). Its open naming section is
resolved in [C1](#c1-flags-and-level-names).

From **opus** the consolidation takes the three content classes, the framing
of the finding-model question as the first decision, the `Note` severity
word, and the rule that read-only commands never say `No files changed.`

From **fable** it takes the captured transcripts as the "before" evidence,
the visible-escape rule shared with astra, the per-command wording where the
three agree, and the two implementation discoveries.

## Part 1 — Shared rules (consolidated)

### Three content classes

Every printed line is one of these, and the class decides its rule (opus).

| Class    | Definition                                                                          | Rule at `minimal`                                              |
| -------- | ----------------------------------------------------------------------------------- | -------------------------------------------------------------- |
| Payload  | What the caller asked for: matches, routes, content, entries, the mutation receipt. | Complete. Never shortened by a detail level.                   |
| Findings | Things the command noticed that the caller did not ask about.                       | Errors listed, warnings and notes counted.                     |
| Framing  | Workspace echo, coverage, counts, selection echo, provenance, phases.               | Only when surprising: a limitation, an explicit `--workspace`. |

Four shapes decide what "payload" means per command (astra, fable):

| Shape         | Commands                                                                                                                                 | Payload                                                                                                                                      |
| ------------- | ---------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------- |
| Summary       | `status`, `doctor`                                                                                                                       | The outcome sentence and counts. A healthy summary is one to three lines.                                                                    |
| Data          | `context`, `find`, `references`, `route list`, `route inspect`, `extension list`, `extension inspect`, `library list`, `library inspect` | The requested identities, rows, content or comparison, complete.                                                                             |
| Change report | `install`, `update`, `index`, `repair`, `cleanup`, the `route`, `extension` and `library` mutations                                      | Every path that was replaced, deleted, restored, kept or rewritten, plus created paths (see [C11](#c11-created-paths-in-framework-install)). |
| Terminal      | help, version, bare groups                                                                                                               | Unchanged text on stdout.                                                                                                                    |

### C1. Flags and level names

One option selects how much of the result is shown:

```text
--detail <level>   minimal (default), standard, full, or debug
```

| Level      | Serves                      | Contains                                                                                                                                                                    |
| ---------- | --------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `minimal`  | Everyone by default         | Outcome sentence, complete payload, errors, counts of warnings and notes, coverage limitations, at most one `Next`.                                                         |
| `standard` | A person reviewing a result | Adds warnings with their subject, cause and code, the reason behind `Next`, per-finding actions, the workspace line, category grouping, kept and unchanged items as counts. |
| `full`     | Investigation               | Adds notes (informational findings), evidence, candidates and why each was included, provenance, hashes and fingerprints, rosters of unchanged or inspected paths.          |
| `debug`    | Debugging the CLI           | Everything in `full`, plus the bounded run diagnostics that `--verbose` writes today, on stderr.                                                                            |

Rules:

- `--view`, `--verbose` and `--json` are removed. No alias remains. The CLI is
  unreleased.
- Detail changes presentation only. Status, exit, findings, effects and
  coverage never change with it.
- `lines(minimal) <= lines(standard) <= lines(full)` for one result. `debug`
  adds stderr only.
- New intermediate levels (for example `summary` or `detailed`) may be
  inserted later by name without renumbering. None is defined now.

Recommendation: the four names above. The maintainer asked in the astra
session for a vocabulary resembling minimal, standard, verbose and debug with
room to grow. `full` is chosen over `verbose` for the third level because
`verbose` conventionally means "with diagnostics", which is exactly what
`debug` adds and `full` does not. Alternative: `minimal|standard|verbose|debug`
with the same meanings. Both are cosmetic; the meanings are the decision.

### C2. Format

```text
--format <format>   text (default) or json
```

`--format json` replaces `--json`. Format is a multi-value dimension, and the
shared operation contract prefers one typed flag over a Boolean per member. No
short alias: `-f` reads as `--force` to anyone who has used `install` or
`update`. TSV is not a format; see [C10](#c10-find-rows).

The selected facts are identical in both formats at the same level (astra's
parity rule). Changing only `--format` never changes finding, effect, row or
content membership, order, counts or coverage.

### C3. `--verbose` retires into `debug`

Run diagnostics keep their stderr stream, their 240-character value bound and
their redaction. They appear only at `debug`. The CLI implementation Directive
sentence "Verbose diagnostics remain separate" is satisfied by the separate
stream and the separate top level, and the Directive text is amended in the
same change to name `--detail debug`. This follows the maintainer's direction
recorded in the astra session.

### C4. Doctor severity ladder

| Severity | `minimal` | `standard` | `full`            |
| -------- | --------- | ---------- | ----------------- |
| Error    | listed    | listed     | listed + evidence |
| Warning  | counted   | listed     | listed + evidence |
| Note     | counted   | counted    | listed            |

Severity words in text are `Error`, `Warning`, `Note` (opus). A finding label
never carries a semantic status; the `COMPLETE:` prefix on informational
observations disappears.

When findings are hidden by the level, `minimal` prints one hint line naming
the command that lists them. Recommendation: keep the hint (opus, fable).
Alternative (astra): no hint, the flag belongs in help. The hint costs one
line only when something is hidden and saves an agent a help call.

### C5. Coverage-kind findings: the first decision

`reference.target-valid` is emitted once per valid link (6772 on this
repository by opus's count). `reference.cycle`, `reference.repeat`,
`reference.external-unchecked` and the three `*.ownership-observation` kinds
are statements about coverage or about the tool, not about a workspace fact.

| Option                                                                                             | Effect                                                                                             |
| -------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------- |
| A. Presentation only: keep per-occurrence findings; render as counts below `full`.                 | Text is fixed. `--format json --detail full` still carries thousands of objects.                   |
| B. Aggregate in the result: one finding per domain per kind carrying the checked and valid counts. | Text and JSON are fixed at every level. The kind stays in the public catalogue. A behavior change. |
| C. Both, with `full` re-expanding per occurrence.                                                  | The result depends on the presentation flag. Rejected by all three proposals.                      |

Recommendation: **B**, as opus recommends and G4 explicitly gates. It needs
the maintainer's word because the G4 packet says grouping is not permission to
remove a category. Under B no category is removed; its cardinality changes.
Fallback if B is declined: A, as fable specifies.

### C6. Caps and doctor filters

No list is capped at any level in G4. The severity ladder is the only filter.
Astra's `--severity` and `--limit` options and its 20-finding default at
`standard` are recorded as a candidate for phase 5, to be added only if real
use shows that the ladder is not enough. Opus's 10-per-severity cap at the
default level is not carried, because the default level does not list
warnings.

### C7. JSON

One schema, proposed as version 3:

```text
{ schemaVersion: 3, command, status, detail, workspace, result, next }
```

- Always minified.
- `result` membership per level is defined once per command (the selection
  matrix below). An omitted member is absent, never `null` or `[]`.
- Every level keeps identities, statuses, coverage and limitations, counts,
  listed findings with subject, code, cause, location and resolution, `next`,
  and for mutations every effect with path, action, outcome and residual.
- Scalars are plain: a number when available, `null` when not, with the reason
  carried by the finding or limitation that already exists. Never zero for
  unknown.
- The serializer owns escaping. Parser failures before binding keep their
  current path: stderr, exit 4, no envelope, even with `--format json` (astra).
- Schema versions 1 and 2 are deleted.

### C8. Escaping and line endings (Task 31 M3)

- One human text owner in Shell presentation. Printable characters pass
  through unchanged, including `\`, `"`, `<`, `>` and non-ASCII. `\n`, `\r`
  and `\t` become the visible two-character sequences. Other control
  characters and lone surrogates become `\uXXXX` (astra, fable). Opus's U+FFFD
  alternative loses the information and lets a tab break a row.
- Truncation exists only for `debug` diagnostics, counts Unicode scalars,
  never splits a pair, and marks the cut with ASCII `...`.
- JSON uses `System.Text.Json` with the relaxed encoder on every context.
- Generated framing is normalized to the platform newline once, at the
  framing writer. Authored content and diffs are byte-exact.
- The seven command-local escapers are deleted after the before-snapshot.

### C9. Statuses and exits

The seven statuses, their exits and streams are unchanged. Two corrections:

- `route init` that creates a scaffold needing authoring returns `complete`.
  Astra keeps `attention`; opus is silent; fable proposes `complete`. The
  scaffold is the command's purpose, and exit 2 fails scripts under `set -e`.
  Recommendation: `complete`, with the advisory in text and `needsAuthoring`
  in JSON.
- Every command's help states in one sentence that `attention` (exit 2) means
  the command finished and found something to look at (opus).

`status` outside a workspace stays `complete`. Read-only commands never print
`No files changed.`

### C10. `find` rows

`minimal` prints one tab-separated row per match, `id<TAB>path`, with no header
and nothing after the rows. Zero matches print one sentence. `standard` prints
an aligned table with descriptions and a count line. `full` adds match evidence
and the search details.

Recommendation: keep the tab rows (opus, fable). IDs may contain spaces, so a
tab is the only delimiter a script can rely on, and the rows are readable.
Alternative (astra): natural-text rows with a `Found N matching sources.`
header and no TSV promise. Choosing the alternative retires the TSV contract
the G4 gates list as an unaccepted recommendation.

### C11. Created paths in Framework `install`

Mutations list every created path at `minimal`, except Framework `install`,
which summarizes creations by count and directory because its roster is the
fixed payload and is recorded in `.agents/open-forge.lock.json`. Replaced,
restored, deleted, kept and rewritten paths are always listed. Recommendation:
this single exception. Alternative (astra): list all 44 paths at `minimal`.

### C12. Workspace echo

`Workspace: <path>` appears at `minimal` when `--workspace` was given or when
the status is `blocked`, `failed` or `interrupted`. It appears always at
`standard` and above. JSON always carries it. `Selected by:` is dropped from
text; JSON keeps `selectedBy`.

### C13. Finding codes

Codes such as `[reference.target-missing]` appear at `standard` and above and
in JSON at every level. `minimal` omits them.

### C14. Help in G4

In G4: the `--detail` and `--format` option lines, every `Global options`
paragraph, the `attention` sentence, and moving the repeated results-and-streams
block to root help. Phase 5: typo suggestions, grouped root help
(Read, Maintain, Groups), `repair` in Getting started, plan-before-confirm,
and prompt wording. Text that names a "wizard" says "prompt" now.

### C15. Where previous content went after `update`

`update` replaces changed owned files. The accepted state-files decision says
the receipt points at `git diff` when `.git` exists, otherwise at the recovery
bundle. Successful commands delete their bundle, so the second branch has
nothing to point at today. Recommendation: print `Previous content: git diff`
when `.git` exists and nothing otherwise, and record the gap against the
decision for the maintainer.

### C16. `references` and generated links

`references` counts authored links only. G4 fixes the wording so zero results
say `no authored links` and note that generated Entries links are not counted.
Whether to count them is a phase 5 behavior question.

### Shared rules carried unchanged from the three proposals

- First line is a complete sentence. No `Status:` line.
- Ordering: headline, workspace (when shown), errors, warnings, what changed,
  what was kept, what could not be checked, counts, `Next` last. Within a
  severity by path, line, column.
- Every listed finding names a path with `:line:column` when known, or an
  identifier. Nothing without either can be listed or block.
- One `Next:` that runs as typed, never `--help` for an identity error, never
  a command missing its operand. A manual action is a short sentence.
- No zero counts, no `not-applicable`, `not-requested`, `residual: none`,
  `none observed`, no phase names, no enum echoes in text. A missing fact is a
  sentence naming the cause.
- Dry runs say `Would ...` and end with `No files were changed.`
- Partial results list applied, not started and unknown paths and the recovery
  path. A headline never claims nothing changed when something may have.
- A no-op says `Nothing to do.` and never claims verification of nothing.
- Never advise rerunning a mutation to see its receipt (astra).
- ASCII framing. Two-space indentation. Paths, identifiers, commands and
  authored content are never truncated.
- Colour as today, plus bold on subject paths and dim on `Next` reasons; none
  in JSON, rows, authored content or diffs.

## Part 2 — Per-command matrix

The fable proposal's Part 2 holds the captured transcripts and the proposed
wording for all 28 commands. This matrix is the G4 checklist in one place and
records where the consolidation departs from that file. Level names map
`brief -> minimal`, `normal -> standard`.

| Command             | Question                                               | `minimal` keeps                                                                                      | `standard` adds                                                          | `full` adds                                                           | Consolidation notes                                                                                             |
| ------------------- | ------------------------------------------------------ | ---------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------ | --------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------- |
| `status`            | Installed, current, how much does startup cost?        | One sentence, startup tokens line, installed Extensions, attention items with paths, `Next`          | Context comparison block, routes, Framework, Libraries, recovery counts  | Every managed file and Entries section with state, continuity sources | As fable. Percentage stated as "N of M routed files".                                                           |
| `doctor`            | What is wrong and what do I do?                        | Counts line, errors, limitations, hint when hidden, `Next` from resolution lanes                     | Warnings, categories with content, codes, per-finding actions            | Notes, evidence, candidates with basis, provenance, resolution counts | C4, C5, C6.                                                                                                     |
| `install`           | What was created, was anything existing touched?       | Sentence, creation counts, every replaced or existing-touched path                                   | File effects roster, lock path                                           | Directories, asset paths, fingerprint, recovery in words              | C11.                                                                                                            |
| `update`            | What changed, what was kept, where is the old content? | Replaced, restored, deleted, kept-retired paths; `Previous content: git diff`; `Next` for `--prune`  | Every managed file with its relation                                     | Fingerprints, source                                                  | **Corrected**: ordinary update replaces changed files. See transcript below.                                    |
| `index`             | Which Entries sections were rewritten?                 | Sentence with N of M, changed paths with entry counts                                                | Diff per changed section                                                 | Selection facts, unchanged regions                                    | As fable; diff header is `--- path (Entries section)`.                                                          |
| `repair`            | What was fixed, what needs me?                         | Repaired occurrences with old -> new, unresolved count, `Next`                                       | Unselected findings with reasons, Library recovery selection             | Plan steps, preflight, verification, post-diagnosis                   | Never `verified` on zero effects.                                                                               |
| `cleanup`           | What recovery data was removed?                        | Every deleted path                                                                                   | Kind and integrity per candidate, ineligible candidates                  | Lease and revalidation facts                                          | As fable.                                                                                                       |
| `context`           | Give me the documents in order                         | `=== path (id) ===` delimiters and byte-exact content; findings above the stream                     | One summary line first, `included because` per source                    | Route, scope, order, headings with lines, link table                  | No stderr summary (astra). Managed markers stay in content.                                                     |
| `find`              | Which sources match?                                   | `id<TAB>path` rows; one sentence when empty                                                          | Aligned table with descriptions and count                                | Match evidence, search details                                        | C10.                                                                                                            |
| `references`        | What links in and out?                                 | `in`/`out` rows with `path:line:column`; authored-only sentence when empty                           | Written destination per occurrence                                       | Inspected sources, selectors                                          | C16.                                                                                                            |
| `route list`        | What routes exist?                                     | ID and description rows, indented, full IDs, depth trailer only when depth stopped the listing       | Path and tags per row                                                    | Parent, depth, kind, child count, provenance                          | Trailer never claims a count of hidden routes (astra).                                                          |
| `route inspect`     | Where is it, when is it read, what does it cost?       | Identity line and the three question blocks                                                          | Axioms block                                                             | Status explanation, closure measurements, selection facts             | `1 file` not `1 files`; commas not middle dots.                                                                 |
| `route init`        | Which entrypoints were created?                        | Created entrypoint paths, parent Entries updated, placeholder advisory                               | Every entrypoint in the chain with `created` or `present`, scaffold mode | Created content verbatim, hashes, recovery                            | C9: exit 0.                                                                                                     |
| `route create`      | Was the file created and listed?                       | Created path with ID, parent Entries updated                                                         | Metadata written, Template used                                          | Hashes, unchanged roster                                              | Invalid input names every missing value and a corrected command.                                                |
| `route update`      | Which metadata changed?                                | Field changes `old -> new`, Entries updated                                                          | Template eligibility explanation                                         | Planned comparisons, hashes                                           | Protected body keeps `attention`.                                                                               |
| `route move`        | What moved and what was rewritten?                     | Old -> new path, Entries updated, **references rewritten** with `path:line:column`                   | Each rewritten destination `old -> new`                                  | Occurrence coordinates, identity proof, hashes                        | **Corrected**: move rewrites references that would break.                                                       |
| `route remove`      | What was removed, what happened to incoming links?     | Removed paths, Entries updated, **incoming links detached** with `path:line:column`, recovery path   | The text each link became                                                | Guard, verification, recovery observations                            | **Corrected**: remove detaches incoming links, keeping their text.                                              |
| `extension list`    | What is installed, what is available?                  | Installed rows, available rows with descriptions, `Next`                                             | Source path, dependencies                                                | Managed path counts, lock coverage                                    | Descriptions at `minimal` (fable); astra moves them to `standard`. Kept at `minimal`: they are how one chooses. |
| `extension inspect` | Does the installed package match?                      | Sentence with versions, paths needing attention                                                      | Every managed path with relation                                         | Both SHA-256 per path, manifest, dependency order                     | No self-referential "use --detail" text.                                                                        |
| `extension create`  | Where is the scaffold?                                 | Created scaffold paths                                                                               | Effective metadata, validation summary                                   | Validation evidence                                                   | Edit reminder names the real manifest path from the result.                                                     |
| `extension install` | Which packages and files were installed?               | Packages incl. dependencies, created files, Entries updated, grant published, recovery               | Dependency order, per-file kinds, permission evaluation                  | Unchanged navigation observations, Framework fingerprint              | Empty `content/` is `attention`, never a silent success.                                                        |
| `extension update`  | What changed against the source?                       | Replaced, restored, kept-retired paths, recovery                                                     | Versions, dependency closure, relations                                  | Fingerprints, permission, verification                                | Same semantics as `update`: changed owned files are replaced.                                                   |
| `extension remove`  | Which claims and files were removed?                   | Deleted paths, kept shared paths with owners, Entries updated, retained bundle path, `Next: cleanup` | Owners per path, orphaned dependencies                                   | Effect plan, verification                                             | Retained bundle is ordinary success (astra, P1 help).                                                           |
| `library list`      | Which Libraries and links are registered?              | One row per Library with mapping, links needing attention, `Next`                                    | Every registered link with state                                         | Expected and observed targets                                         | Source inventory is never claimed by `list`.                                                                    |
| `library inspect`   | Does the source match the projection?                  | Sentence with counts, paths needing attention                                                        | Every source/destination relation                                        | Full inventory, link identity evidence                                | As astra.                                                                                                       |
| `library attach`    | What was registered and linked?                        | Sentence, created links, Entries updated, grant published, recovery                                  | Relative targets, inventory, verification                                | Admission and permission facts                                        | Duplicate ID is `blocked`.                                                                                      |
| `library sync`      | Which links were added or removed?                     | Added, removed, unchanged counts with paths for changes                                              | Relations and reasons                                                    | Inventory, comparisons                                                | As fable.                                                                                                       |
| `library detach`    | Which links were removed?                              | Removed links, source kept sentence                                                                  | Exact mappings and removability                                          | Identity proof                                                        | As fable.                                                                                                       |

Status coverage: astra's status coverage matrix (which of the seven statuses
each command can actually reach, and which have a partial-effects form) is
adopted as the test-planning input for phase 7. It is not repeated here.

## Part 3 — Corrected and representative transcripts

Blocks labelled **Captured** are from the fable capture of the current build.
Blocks labelled **Proposed** are new wording.

### `doctor`

Proposed, healthy, `minimal`:

```text
No problems found.
  6 checks complete. 21 links checked.
```

Proposed, warnings only, `minimal` (exit 2):

```text
No errors. 2 warnings and 3 notes were recorded.
  To list them: open-forge doctor --detail standard
```

Proposed, one error, `minimal`:

```text
1 error, 2 warnings and 3 notes.
  .agents/skills/pdf/SKILL.md:1:1    Frontmatter is invalid
                                     The frontmatter block is not closed.
Next: edit .agents/skills/pdf/SKILL.md, then rerun open-forge doctor
```

Proposed, `standard`:

```text
1 error, 2 warnings and 3 notes.
Workspace: <ws>

Routes and navigation
  Error    .agents/skills/pdf/SKILL.md:1:1     Frontmatter is invalid  [workspace.frontmatter-malformed]
                                               The frontmatter block is not closed. Edit the file by hand.

Links
  Warning  .agents/loader.md:105:3             Broken link  [reference.target-missing]
                                               The linked file was not found: patterns/_patterns.md
                                               Possible target (not chosen): .agents/patterns/_patterns.md
  Warning  .agents/maps/_maps.md:32:3          Broken link  [reference.target-missing]
                                               The linked file was not found: nowhere/_nope.md
                                               No possible target was found. Fix the link by hand.

  21 links checked, 3 external links not checked.
Next: open-forge repair  (choose a target for the first broken link)
```

Proposed, `minimal`, `--format json`, healthy (one line, shown wrapped):

```json
{
  "schemaVersion": 3,
  "command": "doctor",
  "status": "complete",
  "detail": "minimal",
  "workspace": { "path": "<ws>", "selectedBy": "current-directory" },
  "result": {
    "coverage": "complete",
    "counts": { "error": 0, "warning": 0, "information": 0 },
    "coverageCounts": { "linksChecked": 21, "externalLinksNotChecked": 0 },
    "limitations": [],
    "findings": []
  },
  "next": null
}
```

Under [C5](#c5-coverage-kind-findings-the-first-decision) option B the
`coverageCounts` member is the aggregated finding data; under option A it is
derived from the omitted per-occurrence findings. Either way `minimal` JSON
carries no per-link objects.

### `update`

Captured, clean workspace, dry run, current (16,371 bytes): every managed file
with two `Comparison:` lines and two SHA-256 pairs, then
`Lifecycle: trust=trusted / coverage=complete / action=preserve / outcome=already-current`.

Proposed, no change, `minimal`:

```text
The Framework is up to date. Nothing to do.
```

Proposed, changed and missing files, `minimal` (this replaces the fable
transcript, which wrongly said changed files are kept):

```text
Updated 3 managed files to the bundled Framework version.
  .agents/guidance/_guidance.md     replaced (you had changed it)
  .agents/patterns/_patterns.md     restored (it was missing)
  .agents/workflows/_workflows.md   replaced (new content in this release)
  Previous content: git diff
```

Proposed, retired file present, `minimal` (`attention`, exit 2):

```text
Updated 1 managed file. 1 retired file was kept.
  .agents/guidance/_guidance.md     replaced
  .agents/guidance/old-advice.md    kept; this release no longer ships it
Next: open-forge update --prune --dry-run  (preview deleting it)
```

Proposed, dry run, `minimal`:

```text
Would update 3 managed files.
  .agents/guidance/_guidance.md     replace (you have changed it; a recovery bundle is written first)
  .agents/patterns/_patterns.md     restore
  .agents/workflows/_workflows.md   replace
No files were changed.
```

`standard` adds every other managed file with `current`, and the Entries
sections rewritten. `full` adds current and intended SHA-256 per path.

### `route move`

Captured, dry run (26 lines):
`References: coverage=complete, scanned=28, inspected=28, occurrences=1` and
effects in `file:<hash> -> file:<hash>` form.

Proposed, `minimal`:

```text
Moved memory/emerging/ideas/pricing/tiers to .agents/memory/emerging/ideas/pricing/tier-options.md
  Entry updated in .agents/memory/emerging/ideas/pricing/_pricing.md
  Rewrote 1 link in .agents/maps/_maps.md:12:3
```

The third line appears only when the reference pass rewrote something. At
`standard` each rewritten link shows `old -> new`.

### `route remove`

Proposed, `minimal`:

```text
Removed .agents/memory/emerging/ideas/pricing/tiers.md
  Entry removed from .agents/memory/emerging/ideas/pricing/_pricing.md
  Detached 1 incoming link in .agents/maps/_maps.md:12:3; its text was kept
  The deleted file is kept in a recovery bundle at <recovery-path>.
Next: open-forge cleanup  (after reviewing the bundle)
```

### `install`

Proposed, fresh directory, `minimal`:

```text
Installed the Open Forge Framework into <ws>.
  Created 21 files and 20 directories under .agents (recorded in .agents/open-forge.lock.json).
  Created AGENTS.md and CLAUDE.md with an Open Forge section.
```

Proposed, existing `AGENTS.md`, `minimal`:

```text
Installed the Open Forge Framework into <ws>.
  Created 21 files and 20 directories under .agents (recorded in .agents/open-forge.lock.json).
  Added an Open Forge section to AGENTS.md. Your existing content was kept.
  Created CLAUDE.md.
```

Proposed, second run: `Open Forge is already installed and current. Nothing to do.`

### `status`

Captured, healthy, current default: 104 lines with every managed file twice.

Proposed, `minimal`:

```text
Open Forge is installed and current.
  Startup reads 19 of 22 routed files, about 8.0k tokens.
```

Captured, one line appended after `## Entries` in `_maps.md`: `incomplete`,
exit 3, six findings, every context measurement `unavailable`.

Proposed, same state, `minimal`:

```text
Open Forge is installed, but 1 file needs attention and startup context could not be measured.
  .agents/maps/_maps.md    changed after install; its Entries section is stale
  Startup context, continuity and root categories were not measured because that file could not be read completely.
Next: open-forge doctor
```

### `find`

Proposed, `minimal`:

```text
memory	.agents/memory/_memory.md
memory/archived	.agents/memory/archived/_archived.md
```

Proposed, empty: `No sources match --tag Zzz.`

### `context`

Proposed, `minimal`:

```text
=== AGENTS.md ===
<body, byte-exact>

=== .agents/loader.md (loader) ===
<body, byte-exact>
```

### Shared errors

```text
Unknown command 'statuss'. Run open-forge --help to list commands.
```

```text
'Expanded' is not a valid --detail value. Use minimal, standard, full, or debug.
```

```text
No source has the ID 'memries'.
Next: open-forge route list --depth=all  (list every ID)
```

The first is a parser failure and keeps its stderr-only path with no JSON
envelope.

## Part 4 — Decisions for the maintainer

| ID  | Decision                                       | Recommendation                                              | Source of the alternative                             |
| --- | ---------------------------------------------- | ----------------------------------------------------------- | ----------------------------------------------------- |
| C1  | Level names                                    | `minimal, standard, full, debug`                            | astra: `verbose` for level three                      |
| C2  | Format flag                                    | `--format text\|json`, no alias, no TSV format              | fable: keep `--json`; opus: `--projection` with `tsv` |
| C3  | `--verbose`                                    | Retire into `debug`; amend the Directive sentence           | fable: keep separate                                  |
| C4  | Doctor hint line when findings are hidden      | Yes                                                         | astra: no                                             |
| C5  | Coverage-kind findings                         | Option B, aggregate in the result                           | fable: presentation only                              |
| C6  | Caps and doctor filters                        | None in G4; `--severity`/`--limit` deferred to phase 5      | astra: add now; opus: cap 10                          |
| C7  | JSON schema 3, plain scalars, minified         | Yes                                                         | astra: `"unavailable"` strings for counts             |
| C8  | Escaper rule                                   | Visible escapes, STJ relaxed, ASCII `...`                   | opus: U+FFFD, `…`                                     |
| C9  | `route init` exit                              | `complete`                                                  | astra: keep `attention`                               |
| C10 | `find` rows                                    | Tab rows, no header                                         | astra: natural rows                                   |
| C11 | Framework install creation roster at `minimal` | Summarize with the lock as the roster                       | astra: list all                                       |
| C12 | Workspace echo                                 | `--workspace`, or blocked/failed/interrupted, or standard+  | opus: also incomplete                                 |
| C13 | Finding codes at `minimal`                     | Omit                                                        | keep everywhere                                       |
| C14 | Help scope in G4                               | Option text, attention sentence, move streams block         | astra: grouped root help now                          |
| C15 | Previous-content pointer                       | `git diff` when `.git` exists, else nothing; record the gap | retain the bundle after update                        |
| C16 | `references` generated links                   | Wording now, behavior in phase 5                            | count them now                                        |

## Part 5 — Implementation handoff

### Affected surfaces

All 28 commands. Shell: `CliSyntaxDefinitions`, `CliPresentationDefinitions`,
`CliPresentation`, `CliRendererSet`, `CliViewRenderers`, `CliHumanText`,
`CliHumanStyle`, `CliCompactJsonDocument` (deleted), `CliCompactJsonProjection`
(deleted), `CliResultHelp`, root help composition. Contracts: global flags,
result coordinates, the shared operation contract (`Next:` and stream
paragraphs), the CLI implementation Directive sentence on verbose, every
command interface's output and structured-output sections and its
`Compact JSON Output` section, `docs/cli.md`, README samples.

Public documentation corrections found by astra and confirmed: `docs/cli.md`
still names `.agents/open-forge.lifecycle.json`, still says ordinary update
preserves changed content, and still lists `extension remove --prune`, which
the command does not accept.

### Preserved behavior

The seven statuses and exits; stdout/stderr mapping; one operation, one
result, one primary output; every finding kind and code (cardinality changes
only under C5 option B); deterministic order; counts describing the whole
operation; byte-exact authored content; complete mutation receipts in JSON at
every level; every replaced, restored, deleted, kept or rewritten path in text
at every level; `--dry-run`, `--automatic`, `--force`, `--prune`,
`--allow-path`; colour capability detection; no truncation of paths,
identifiers or commands; the parser-failure stream exception.

### Selection stage

One function per command, `select(result, detail) -> selected model`, runs
before either renderer. It owns level membership, severity ordering,
suppression of healthy items, zero-count collapsing and coverage-count
grouping. Renderers format the selected model and decide nothing about
membership. Text and JSON of the same level are proved to carry the same
identities in the same order.

### Verification

- G4-0 captures and commits, alone, the current text and JSON output of every
  command at both current views for the seeded states in the fable capture and
  the unit fixtures, through the phase 7 AOT-safe in-process snapshot tool.
  The fable scratch capture is design evidence, not the baseline.
- Doctor is not run against this repository. The implementer may measure a
  fixture workspace. The 258 MB figure stays user-reported unless the
  maintainer lifts the restriction.
- Invariants run across all commands: level monotonicity, severity order, no
  `Status:` line, no JSON escapes in text, ASCII framing, valid UTF-8, one
  `Next` at most and last, every listed finding has a subject, text and JSON
  parity per level, no empty-array substitution for omitted members.
- M3 evidence: `"`, `\`, tab, CR, LF, CRLF, mixed endings, lone surrogate,
  astral pair, a value at and over the diagnostic bound, byte-exact content
  through a projection.
- Stream and exit tests for `route init`.
- Complete managed suite and the supported Native AOT gate at the end of the
  renderer wave.

### Slices

| Slice | Content                                                                                                                                                                | Output change |
| ----- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------- |
| G4-0  | Snapshot tool finished; before-capture committed alone.                                                                                                                | no            |
| G4-1  | Shell: `--detail` and `--format` replace `--view`, `--json`, `--verbose`; `CliDetail`; envelope v3 with `detail`; help option text; Directive sentence.                | yes           |
| G4-2  | Shared text: one escaper (M3), diagnostic bound moved, framing writer with platform newlines, headline, finding-row, counts and `Next` helpers; delete seven escapers. | yes           |
| G4-3  | Selection stage type and the C5 decision applied to Doctor's result; `doctor`, `status`.                                                                               | yes           |
| G4-4  | `install`, `update`, `index`, `repair`, `cleanup`.                                                                                                                     | yes           |
| G4-5  | `context`, `find`, `references`, `route` family.                                                                                                                       | yes           |
| G4-6  | `extension` and `library` families.                                                                                                                                    | yes           |
| G4-7  | Invariants, snapshot regeneration and review, `docs/cli.md` corrections, README samples, delete `DoctorHumanSnapshots` and verbatim copy assertions, full gate.        | no            |

Each output-changing slice commits its contract change with the behavior and
its reviewed snapshot diff.

## Part 6 — Discoveries carried forward

From fable, recorded in the Task 30 record and not presentation questions:

- `index` deleted authored prose written after `## Entries` in a scratch
  workspace, because the heading-based section runs to end of file. The
  recovery bundle was removed after verification.
- `status` and `update` disagree about that file: `changed` versus
  `current: same`.

From astra: interactive paths exist (`RepairWizard`,
`RepairLibraryRecoveryWizard`, Extension Create prompts, Extension selection,
Route Inspect disambiguation). "No wizard" means undiscoverable and unbounded,
not absent. Phase 5 owns their improvement.

From all three: both prepared executables predated G1. Only the fable
proposal rebuilt from the tree before capturing.
