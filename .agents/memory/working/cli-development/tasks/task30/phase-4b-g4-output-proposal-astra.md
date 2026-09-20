---
open-forge:
  description: Unaccepted G4 output proposal replacing the view model with detail levels, with per-command transcripts, JSON packet, naming alternatives, and an implementation handoff
  tags: [Memory, Working, CLI, Task, Proposal, Presentation, Contextual, Candidate]
---

# astra

CLI output design review and proposed specification for Task 30 G4, including Task 31 M3.

Status: revised proposal for discussion. Your direction is settled: replace the view model with detail levels that apply to every renderer, defaulting to the smallest useful level. The exact flags, schemas, examples, and implementation packet below still await a later specification freeze. This task created and revised only this report and did not modify production code, contracts, task records, configuration, or historical analysis. Doctor was not run.

The report distinguishes current contracts, source observations, historical measurements, and proposed output. All example transcripts outside the captured-evidence appendix are proposed wording for illustrative fixtures. Their paths, counts, versions, and failure conditions are examples, not measurements of this repository. Commands shown in transcripts were not executed.

Naming follow-up: the user is considering shorter alternatives to --projection and a more extensible vocabulary than brief/normal/full. The Naming alternatives section records the latest recommendation: --format and minimal/standard/verbose, with clearly defined room for additional levels. These names remain proposals. Earlier transcripts retain their prior spellings so an exploratory naming discussion does not silently become an accepted wholesale interface change; their shared-selection/default principles still apply.

## Start here: the corrected model

I did fall into the trap you identified. The first draft kept --view compact|expanded, retained a separate --verbose flag, and recommended the existing large Doctor JSON graph as the default. That contradicted the intended change. This revision supersedes those recommendations throughout the proposed interface, examples, JSON packet, and handoff.

Your recollection has a source basis. [Lifecycle and architecture analysis](.agents/memory/emerging/analysis/cli-experience-audit/lifecycle-baselines-and-architecture.md) proposes --detail brief|normal|full and the smallest useful default. [The later revised presentation note](.agents/memory/emerging/analysis/cli-experience-audit/structural-requirements-and-markers.md) records --detail absorbing --verbose and defaulting to brief. [The layering analysis](.agents/memory/emerging/analysis/cli-experience-audit/layers-and-sequencing.md) places selection before the format-specific presentation. Those records explain the direction; your clarification now makes that direction explicit for this report. They do not establish that the current CLI already implements it.

Recommend two independent choices: --detail brief|normal|full chooses information, and --projection text|json chooses its format. Defaults are brief and text. Changing the renderer must never silently increase detail. These are proposed flags; they are not runnable against today's parser yet.

Doctor's ordinary default is one line, including relevant nonzero counts. No healthy-domain list, finding rows, candidates, evidence, or automatic detail hint:

    > open-forge doctor
    No errors found. 10 warnings; 100 information items.

The same brief selection in JSON is also small:

    > open-forge doctor --projection json
    {"schemaVersion":3,"command":"doctor","status":"attention","detail":"brief","workspace":{"path":"D:/work/demo","selectedBy":"current-directory"},"result":{"coverage":"complete","counts":{"error":0,"warning":10,"information":100}},"next":null}

Normal detail requests the actionable findings. It defaults to errors and warnings and shows at most 20 finding records:

    > open-forge doctor --detail normal
    No errors found. 10 warnings; 100 information items.
    Workspace: D:/work/demo
    Showing all 10 matching findings. 100 information items omitted.

    Warning: A linked file was not found.
      .agents/guidance/team.md:12:8 -> ../policies/review.md

    [The remaining 9 warnings appear here in the complete fixture.]

The bracketed line is a schematic omission in this report, not literal CLI text. All 10 actual rows must appear in a complete snapshot. --detail normal --projection json selects the same 10 finding identities and excludes the same information items. It does not serialize the whole result behind the short summary.

Full detail requests every finding, including information, with available evidence, candidates, provenance, and bounded diagnostic records. It has no default finding cap. Use open-forge doctor --detail full, or append --projection json for the same information in JSON. Full may be large because the reader explicitly requested it. Selected-content and mutation-receipt rules below prevent brief output from discarding the actual answer to other commands.

All these renderings retain attention, exit 2, and complete diagnosis coverage for the same example result. Zero errors does not imply exit 0 when actionable warnings remain. The exact schema version, new flag grammar, normal-detail limit, and command field selections are concrete proposals for a later freeze, not claims of current support.

## Findings from this review

| ID | Finding and evidence | Consequence for this design |
| --- | --- | --- |
| ASTRA-01 | You report a 258 MB Doctor result. I did not reproduce it. The historical audit contains smaller measurements on other fixtures, including an 8.8 MB example. | Treat the scale as a serious reported problem, but do not claim a measured reduction or a proven cause for the particular 258 MB result. |
| ASTRA-02 | The current global contract and CliSyntaxDefinitions default to expanded. DoctorHumanRenderer traverses every domain, and DoctorFindingHumanRenderer prints every finding group and every candidate even in compact mode. Expanded also prints evidence and provenance. | Replacing that default requires a real detail-selection stage before either text or JSON is built. A new flag name or minification alone cannot bound the output. |
| ASTRA-03 | Both existing local executables inspected here report the removed Libraries state file and require retired Entries markers. The current source and reconciled contracts use the ownership lock and heading-based Entries. | Neither executable can establish a trustworthy before baseline for current G4 source. Do not snapshot them as current behavior. |
| ASTRA-04 | Current source contains RepairWizard, RepairLibraryRecoveryWizard, Extension creation prompts, Extension selection prompts, and Route Inspect disambiguation. | The blanket statement that no wizard exists is stale for this checkout. The quality and completeness of each interactive journey still need verification against a matching executable. |
| ASTRA-05 | Public docs still name the retired lifecycle file, describe old Update preservation behavior, and advertise Extension Remove --prune. Current narrower contracts and source disagree. | Examples must follow current command-local contracts and source. Include these public-doc corrections in a later accepted implementation packet; they were not edited here. |
| ASTRA-06 | Current compact JSON already has schemaVersion 2. It deliberately retains all mutation receipts, and Doctor retains its findings and candidate sets. | That existing projection is unsuitable as the new brief default. Propose one new detail-aware schema; preserve essential original mutation effects without forcing the complete diagnostic graph into brief results. |
| ASTRA-07 | Shared result contracts describe JSON operation envelopes, but CliCoreApplication writes shell/parser failures directly to stderr. Route Move explicitly documents this exception. | Do not promise JSON for every rejected argv combination without separately changing the shell contract and implementation. |
| ASTRA-08 | The handover requires maintaining G4 records and obtaining approval. Your test request instead permits only a report and asks for all analysis at once. | This report carries proposals, evidence, and pending decisions. Existing G4 records remain untouched, and no approval is inferred. |
| ASTRA-09 | The first draft anchored its recommendations to current view names and current JSON membership despite the earlier detail-selection analysis. Your follow-up exposed that error. | Corrected to brief/normal/full selection shared by renderers, brief by default in both formats, and full detail absorbing diagnostic verbosity. Current interfaces remain evidence of the starting point, not a reason to preserve the rejected model. |

These are design findings, not a new implementation task or accepted defect disposition. ASTRA-03 and ASTRA-05 are material reasons to strengthen the prompt's evidence requirements.

## Shared output proposal

### Default, detail, and diagnostics

Use --detail brief|normal|full for every domain command. The default is brief. Use --projection text|json to choose the renderer, defaulting to text. The proposed interface replaces --view, --verbose, and --json with those two options. Do not retain the old concepts internally as two independent content-selection paths. A temporary compatibility alias, if actually needed by an identified consumer, must resolve to these same choices and must never restore a larger default.

| Form | Selected information | Format |
| --- | --- | --- |
| open-forge COMMAND | Brief: outcome and the smallest complete useful answer | Text |
| open-forge COMMAND --detail normal | Brief facts plus actionable explanation and relevant comparisons | Text |
| open-forge COMMAND --detail full | Complete applicable result facts and supporting evidence; bounded diagnostics when available | Text |
| open-forge COMMAND --projection json | Exactly the brief information selection | Minified JSON |
| open-forge COMMAND --detail normal --projection json | Exactly the normal information selection | Minified JSON |
| open-forge COMMAND --detail full --projection json | Exactly the full information selection | Minified JSON |

The pipeline is: complete typed operation result -> detail selection -> chosen renderer -> output. Selection owns inclusion, finding ordering, limits, and omitted counts. Renderers own syntax, human wording, escaping, and structural line endings. They must not independently decide which findings or effects to include. JSON carries stable codes and typed coordinates; text expresses those same selected facts naturally. Format metadata is allowed to differ; substantive result selection is not.

Full detail absorbs the diagnostic information currently selected by --verbose. Keep diagnostics separate from operation data on stderr: human records for text, structured records for JSON, using the same selected diagnostic facts. The full operation result stays on its established primary stream; JSON operation envelopes stay on stdout. No independent verbosity option remains in the proposed interface. Diagnostic availability must not change status, trigger a retry, or rerun the operation. Bounded diagnostic values follow M3; logs are not an excuse to print a second copy of the result graph.

All three levels use one new JSON schema, proposed as version 3, with a detail discriminator. Schemas 1 and 2 describe the old interface and are not the new defaults. Full detail is not a separate schema version, and pretty-printing is not another detail level. The JSON packet below defines the new selections and concrete examples.

The historical note also considers TSV. This report settles text and JSON. TSV is a separate format decision, not a detail tier; the Find section identifies the existing TSV compatibility impact. Do not silently advertise a universal TSV renderer that the command catalogue does not yet define. This is an explicit narrowing of that historical proposal, with the detail model preserved.

### What the default contains

Use one of three shapes according to what the command actually returns:

| Shape | Commands | Default content |
| --- | --- | --- |
| Summary | status, doctor | Outcome; a few useful counts; the reason for incomplete or unsafe coverage; a next action only when necessary to act on that condition |
| Data | context, find, references, route list/inspect, extension list/inspect, library list/inspect | The requested identities, content, rows, or comparison, plus honest coverage and actionable findings |
| Change report | install, update, index, repair, cleanup, Route mutations, Extension mutations, Library mutations | Outcome; every selected changed path once; retained or unresolved paths; partial effects and recovery when present. Omit internal planning/verification detail until requested. |

An inventory is not replaced by “found 40 items”: the identities are its answer. Context is not summarized: its selected authored content is its answer. A write receipt is not replaced by “updated 40 files”: the affected paths may be impossible to reconstruct by rerunning after the change.

These exceptions apply equally to text and JSON. “Brief” is the smallest useful answer to that command, not a promise that every command fits in one line. Doctor can be one line because its default question is a diagnosis summary. Find must still return requested matches; a mutation must still return its essential original receipt. Preserving that receipt does not require serializing every planned step, fingerprint, evidence record, or healthy check at brief detail.

For successful mutations, use one short physical-path row per changed file and list multiple logical edits together on that row. A file changed for both metadata and Entries is one changed file. Name ownership/settings publication separately when it occurs. Empty internal phases, unchanged-file rosters, fingerprints, and repeated source paths do not belong in the default.

### Outcome wording and process meaning

Retain the seven accepted semantic statuses and numeric exits. A warning is a finding severity; attention is an operation result. A no-op is a successful outcome when the command can prove it, not an eighth status.

| Result | Exit | Human primary stream | Proposed wording pattern |
| --- | ---: | --- | --- |
| complete | 0 | stdout | “Updated 2 files.” / “Already current. No changes.” |
| attention | 2 | stdout | Say what succeeded, then what remains: “Updated 2 files. 1 retired file was kept.” |
| incomplete | 3 | stdout | “The result is incomplete.” Identify the unavailable fact and which returned facts remain usable. |
| invalid | 4 | stderr | “The request is invalid: …” Identify the missing operand or invalid value. |
| blocked | 5 | stderr | “Cannot …” Identify the exact safety, identity, collision, or permission boundary. |
| failed | 1 | stderr | “The update failed …” State verified changes and uncertain or remaining effects. |
| interrupted | 130 | stderr | “The update was cancelled …” State whether effects started and what is known. |

Typed domain JSON results go to stdout for every one of these statuses, with selected diagnostics on stderr. Preserve the current exception for shell/parser failures before command binding: stderr, exit 4, and no operation envelope. Help and version remain text-only terminal modes with stdout and exit 0 even when JSON is selected. They are CLI instructions and version text, not command-result renderers. The current --json spelling and proposed --projection json spelling do not change that terminal boundary.

Use the actual command-local result policy. Do not convert all findings into exit 2, all errors into exit 1, all partial outcomes into incomplete, or every retained bundle into attention. In particular:

- Doctor may finish diagnosis with errors and return attention, because diagnosis succeeded and findings remain.
- A missing ownership lock is informational and does not itself block the operation.
- Normal Update replaces/restores owned current content under the current two-way policy. Do not resurrect baseline-based “your edits were kept” examples.
- Update and Extension Update/Remove can intentionally retain recovery after success. That is different from a failed attempt to remove recovery.
- Route Init currently returns attention for a new NeedsAuthoring entrypoint. Changing that to exit 0 would be a separate behavior decision.
- Cleanup and Extension Create have no currently accepted attention condition. Do not invent one for symmetry.

### Ordering, subjects, and next actions

Put the outcome first. Put conditions that limit trust before ordinary data. At normal/full detail, order diagnostic findings by error, warning, then requested information; within a severity preserve the current domain sequence and stable finding order. Select this order once for both text and JSON. This deliberately changes any old JSON presentation order that differs, without altering finding identities or the operation's original typed facts.

Show a file as a workspace-relative path, with line and column when available. A package uses its stable ID. A Library uses its Library ID. A whole-workspace issue may legitimately have only the workspace as its subject; lack of a line number does not make it non-blocking.

Display the workspace in text when --workspace was explicit or when a result failed or its boundary is uncertain. At normal/full detail, also show it where it is useful for interpreting source/target relationships or evidence; it is not compulsory repeated framing for every current-directory success. This changes the current always-echo contract and is a proposal. The selected workspace remains envelope metadata in JSON; that formatting metadata does not opt additional result facts into brief detail.

Use at most one operation-level Next action, only when it materially helps resolve the returned condition. Doctor brief does not print a routine suggestion to request more detail; the detail option belongs in help. A normal result that is explicitly limited may show how to retrieve the remainder. Recovery uncertainty takes precedence over a generic retry. Choose a direct, useful inspection or preview before a broad mutation.

Proposed next-action corrections must be coordinated with structured next values; a human sentence cannot quietly promise a different action. Include required operands and preserve the selected workspace when generating a runnable recommendation. Use the CLI's existing shell-quoting boundary, or show an explicit instruction to run in the printed workspace when a portable command cannot be formed. Do not copy raw user text into a shell command.

Never advise rerunning a mutation merely to see its original receipt at greater detail. A fresh dry-run can inspect current state, but it is not the record of the earlier operation. Request full detail on the original mutation when its complete supporting evidence is needed; brief still carries essential original effects and recovery facts. Do not automatically retry on a renderer failure. Cleanup has no single-bundle selector: a suggested cleanup --dry-run previews the entire recognized catalogue for that workspace.

### Empty, unknown, and not applicable

| Actual fact | Human presentation |
| --- | --- |
| A complete search found no matches | “No sources matched.” |
| An incomplete search returned no safe matches | “No matches could be confirmed. The search is incomplete.” |
| No Library claims in a readable lock | “No Libraries are registered.” |
| Ownership unavailable or absent | “No Library registrations could be read. Ownership information is unavailable.” Keep its actual informational status. |
| A field genuinely does not apply | Omit it from the brief selection; explain it at normal/full detail only when that distinction matters |
| A requested section is absent | Name the source and missing section; preserve the command's attention result |
| Recovery is not required and no artifact exists | Omit the empty recovery section |
| Recovery remains or its disposition is unknown | Show the exact known path and disposition at every detail level, in both formats |
| A source or package was not inspected | Say “not checked” or “unavailable”; never print an empty inventory instead |

Define JSON presence and nullability for each new detail selection. A field excluded by brief detail is not an assertion that its value is empty, zero, or unavailable. For selected facts, preserve the difference between known empty and unknown. Suppress irrelevant healthy sections before either renderer; within selected counts, JSON can retain explicit zero values while text omits the redundant phrase. Do not populate a selected array with [] when its inspection did not finish.

### Doctor severity and size controls

The three detail levels are sufficient for ordinary use. Propose two additional Doctor-only selection options for targeted investigation. They affect both renderers identically and do not change diagnosis:

| Option | Exact proposed behavior |
| --- | --- |
| --severity error / warning / info | Repeatable union of exact severities; repeats deduplicate. Requires an explicit normal or full detail level. Brief plus this option is invalid, including when brief was defaulted. It never silently increases detail. |
| --limit positive-integer / all | Maximum returned finding records after severity selection and shared ordering. Normal defaults to 20; full defaults to all. Requires normal or full detail. Zero, negative, malformed, and repeated values are invalid. |

Normal without --severity selects errors and warnings; full without --severity selects every severity. --severity info selects only information; repeating it with --severity warning selects those two sets. “Warning” is an exact selection, not a minimum threshold. These defaults and overrides mean the same thing with text or JSON. Reject brief plus a finding-control option because brief does not return a finding list; explain that normal/full detail is required.

Counts and status describe all findings actually established by diagnosis, independent of the display filter. If diagnosis coverage is incomplete, label those counts as observed, not a complete workspace total. A display limit cannot make diagnosis coverage incomplete, and a severity filter cannot hide the operation's attention status. If no findings match, say so alongside the full observed counts. Coverage limitations and indispensable failure reasons survive every selection.

If 20 of 78 matching findings are shown, text says “Showing 20 of 78 matching findings”; JSON records matching=78, shown=20, and omittedByLimit=58 in selection metadata. Both report the separate number omitted by severity. --limit all obtains the remainder at the same detail level. This is intentional reduced JSON in a new schema, never a silent change to schema 1 or 2.

The limit counts finding records, not grouped headings. Normal includes finding identity, severity, subject/location, message, and applicable direct action; candidate rosters, evidence excerpts, and provenance require full. Full with no explicit limit includes all selected findings and their complete available support. With a finite limit at full detail, cap each displayed candidate set at 3 identities and one bounded excerpt per displayed candidate, preserving order and explicit shown/total counts in both formats. --limit all removes those display caps. Never label a filtered or capped payload as the complete diagnosis. Repeated supporting sets can be represented once with stable references, provided the information selected by each level is identical in both renderers.

| Output class | Proposed budget for named synthetic fixtures | Qualification |
| --- | --- | --- |
| Doctor brief text, complete coverage | 1 line, at most 160 UTF-8 bytes | Outcome and relevant counts; no finding rows or routine Next hint |
| Doctor brief JSON, complete coverage | One minified envelope, at most 512 UTF-8 bytes | Counts and coverage; no findings, domains, candidates, evidence, or diagnostic payload |
| Doctor brief, incomplete/blocked/failed | Normally 2–4 text lines; selected reasons in JSON | Every indispensable trust boundary survives; exact necessary paths may exceed fixture budgets |
| Doctor normal, either format | At most 20 finding records by default; target 12 KiB in the bounded fixture | No supporting evidence graph; --limit all explicitly requests more rows |
| Doctor full, either format | No default finding cap | Explicit request for all available detail; bounded diagnostic values still apply |
| Status, healthy fixture | At most 6 lines, 512 bytes of generated framing | No full list of healthy managed files |
| Other empty/no-op results | Normally 1–3 lines | Preserve an applicable attention or uncertainty condition |
| Lists | At most 2 framing lines plus one brief row per returned item | No unaccepted item cap; requested identities remain complete |
| Mutation reports | At most 4 framing lines plus path/effect and necessary finding rows | No cap that loses the original receipt |
| Context/content projections | Small framing plus the complete selected content | Selected content is exempt from display budgets |

These are proposed acceptance budgets, not measured performance claims or universal hard byte cutoffs. Measure UTF-8 bytes, lines, elapsed time, and peak memory separately. Suppressing printed evidence does not prove that the operation stopped allocating a large finding graph. Preserve that distinction when investigating the reported 258 MB case.

### Escaping and line endings: the M3 decision

Use one human scalar escaper at the presentation boundary for generated messages, findings, prompts, and diagnostic scalar values. Ordinary paths retain ordinary backslashes; do not render Windows paths as JSON string literals. Make embedded control characters visible, for example newline as `\n`, tab as `\t`, carriage return as `\r`, and escape as `\u001B`. The same actual value should look the same across commands. This human display need not be a reversible wire format; JSON supplies exact values.

Use System.Text.Json once for JSON string encoding. Do not human-escape a value before serializing it. A source string with a newline must deserialize to the same newline, not a literal backslash followed by n. Preserve valid Unicode, quotes, backslashes, and supplementary characters. Changes to existing pre-escaped JSON values are intentional M3 output changes requiring reviewed before/after evidence.

Keep the existing 240-character diagnostic-value bound with the accepted owner; define truncation by Unicode scalar values and add a visible omission marker. Do not apply it to required target identities, selected authored content, or machine result fields. Retain exact values in the typed result. The specific limit unit and marker are proposals to freeze with M3.

Normalize generated human structure to host line endings, once. Cover LF, CRLF, lone CR, and mixed input. Preserve selected authored frontmatter/body text and persisted bytes exactly. A detail level must not strip empty Axioms, Entries sentinels, comments, or other source content from a requested content projection. Do not use a JSON encoder to normalize human multiline output.

Keep existing automatic colour capabilities: labels carry meaning without colour; JSON, authored content, paths, preview diffs, prompts, help, and version remain plain. Do not quietly add Windows colour support, a new flag, or a new dependency as part of M3.

## Command catalogue and reading key

The command surface has 28 leaves: 10 root operations, 7 Route operations, 6 Extension operations, and 5 Library operations. Help, version, and the three bare groups are terminal surfaces in addition to those leaves.

The following command sections specify the question, proposed brief and normal-detail output, special states, and preservation boundaries. The 28-row selection matrix in the JSON packet defines what full detail adds for each command; that matrix governs text and JSON equally. Shared invalid-input, unexpected-failure, cancellation, JSON, and stream templates apply to every supported state. The status matrix records exceptions so that “every state” does not mean inventing impossible states.

Each section links to the current interface for existing result facts and finite conditions. Its old rendering prescriptions are the baseline to replace, not the new selection contract. Supporting behavior contracts live beside those interfaces. Normal-detail examples illustrate the same fixture as brief unless explicitly stated otherwise. For mutations, these are alternative renderings of one original result, never instructions to apply the same operation again to obtain more detail. Full adds available support to that result, without rerunning it.

## Root commands

### 01. status

Question: Is this workspace usable, and what deserves attention? [Current interface](.agents/memory/crystallized/documents/cli/contracts/status/interface.md).

Proposed brief success:

    > open-forge status
    Open Forge is installed.
    Startup: 12 files, about 4,200 tokens.
    Routes: 8 roots. Generated Entries are current.

Normal detail adds the measured startup comparison, continuity size, total available context, root additions/removals, observed Framework/Extension/Library state, and recovery coverage. For example:

    > open-forge status --detail normal
    Open Forge is installed.
    Workspace: D:/work/demo

    Context
      Startup: 12 files, about 4,200 tokens.
      Continuity: 3 files, about 900 tokens.
      Total available: 40 files, about 12,000 tokens.
      Startup uses 35% of the available context.

    Routes
      8 root routes. Generated Entries are current.
    Extensions
      No installed packages are recorded.
    Libraries
      No Libraries are registered.
    Recovery
      No recognized recovery bundles or drafts.

This normal-detail example illustrates the major sections; the complete fixture must also include any applicable nonempty baseline comparison, root changes, and detailed observations. Do not claim exact token counts: label estimates. Characters and UTF-8 bytes are different measurements, even if they happen to agree on ASCII fixtures.

| Scenario | Proposed default output |
| --- | --- |
| Framework absent | “Open Forge is not installed.” Preserve the actual installation and status facts. |
| Attention | “Open Forge needs attention. 2 generated Entries sections are stale.” Name their paths; Next: open-forge doctor. |
| Incomplete | “Workspace status is incomplete. Could not read .agents/guidance/_guidance.md.” Include the safe measurements that did finish. |
| Blocked | “Cannot inspect this workspace. The .agents path resolves outside the selected workspace.” |
| Recovery present | Show each recognized residual path and its actual integrity. A malformed final is not a verified bundle. |

Read-only “no change” is simply another observation, not a verification that all files are unchanged. Never make a missing ownership lock mean the Framework is broken or absent.

### 02. doctor

Question: Which diagnosed conditions need action? [Current interface](.agents/memory/crystallized/documents/cli/contracts/doctor/interface.md).

Use the representative detail levels at the beginning of this report and these additional states:

    > open-forge doctor
    No errors found.

    > open-forge doctor
    No errors found. 100 information items.

    > open-forge doctor
    Found 2 errors and 10 warnings.

    > open-forge doctor
    Diagnosis is incomplete. 1 domain could not be checked.
    Could not read .agents/guidance/team.md.

    > open-forge doctor
    Cannot diagnose this workspace.
    The .agents directory resolves outside the workspace.

The first two cases are complete/0 only when every required domain finished. The third is attention/2 only when coverage is complete. Incomplete is exit 3; an unsafe required boundary is blocked/5. An unexpected failure and cancellation use the shared event templates.

Proposed exact severity selection, not supported today:

    > open-forge doctor --detail normal --severity info --limit 2
    No errors found. 10 warnings; 100 information items.
    Workspace: D:/work/demo
    Showing 2 of 100 matching information items. 10 warnings omitted by severity.

    Information: The local reference target is valid.
      .agents/guidance/team.md:6:3
      Target: review.md

    Information: The external link was not checked.
      .agents/guidance/team.md:9:3
      Target: https://example.com/guide

    Next: open-forge doctor --detail normal --severity info --limit all

The tool does not fetch external links. The displayed URL is an illustrative authored value.

Do not silently equate warning count with number of broken links. Current candidate-basis and candidate-cardinality records can themselves be warnings. Preserve every kind and count until the separate finding-model decision changes them. Grouping can remove repeated supporting text while retaining all underlying finding records.

### 03. install

Question: What Framework content was established? [Current interface](.agents/memory/crystallized/documents/cli/contracts/install/interface.md).

Use one row for every actual physical effect. The following is an excerpt of a successful result, not a complete Framework asset inventory:

    > open-forge install --automatic
    Installed the Open Forge Framework.
      Created .agents/loader.md
      Updated the Open Forge section in AGENTS.md
      Updated the Open Forge section in CLAUDE.md
      Updated .agents/open-forge.lock.json

Every other changed path must appear in a complete result. Normal detail adds embedded source version, selected authority, precise generated-region changes, verification, and applicable recovery evidence. It does not repeat a source asset path identical to its destination.

    > open-forge install --dry-run
    Preview: install the Open Forge Framework.
      Create .agents/loader.md
      Update the Open Forge section in AGENTS.md
      Update the Open Forge section in CLAUDE.md
      Update .agents/open-forge.lock.json
    No files changed.

This preview is the matching excerpt. The complete plan includes every effect before any confirmation.

| Scenario | Proposed default output |
| --- | --- |
| Exact managed no-op | “The Open Forge Framework is already installed. No changes.” |
| Existing managed divergence | “Cannot install over changed managed Framework content. Review an update.” Name the target; Next: open-forge update --dry-run. |
| Initial collision requiring authority | “Cannot replace .agents/loader.md without explicit permission.” State the actual initial --force boundary; do not add it automatically. |
| Noninteractive confirmation required | “Installation needs an explicit noninteractive choice. Use --automatic to apply the reviewed request, or --dry-run to preview it.” Preserve the current invalid status for this condition. |
| Recovery unavailable before writing | “Installation is incomplete. Recovery storage could not be prepared. No files changed.” |
| Recovery cleanup failed, positively retained | “Installed the Framework. Recovery data could not be removed.” Show the exact path and Next: open-forge cleanup --dry-run. Status attention. |
| Failure after effects | “Installation failed after 3 verified file changes.” List verified, failed/uncertain, and not-started paths. Never claim nothing changed. |

An unavailable ownership receipt does not create the old missing-lock installation gate.

### 04. update

Question: What managed Framework content changed, and what was retained? [Current interface](.agents/memory/crystallized/documents/cli/contracts/update/interface.md).

In this illustrative fixture, only two content paths need changing:

    > open-forge update --automatic
    Updated 2 Framework files.
      Replaced .agents/guidance/review.md
      Restored .agents/patterns/checklist.md
    Recovery saved: D:/recovery/demo/update-001.zip

The recovery path is a placeholder for the actual external path returned by the operation, not an Open Forge path convention. Include ownership publication when it changes; the example assumes it was already current.

Normal detail adds the same paths' old/current-to-intended comparison, source version, normal/force/prune mode, verification, and bundle protection details. It must explain that normal current Update can replace changed owned current files and restore missing owned current files. Current UpdatePlanningPolicy directly selects Replace and Restore for those states.

    > open-forge update --prune --dry-run --detail normal
    Preview: update the Framework.
      Replace .agents/guidance/review.md
      Restore .agents/patterns/checklist.md
      Delete retired file .agents/guidance/old-review.md
    A recovery bundle will protect the changed existing content.
    No files changed.

| Scenario | Proposed default output |
| --- | --- |
| No-op | “The Framework is current. No changes.” |
| Retired content kept | “Updated 2 Framework files. Kept 1 retired file: .agents/guidance/old-review.md.” Next: open-forge update --prune --dry-run. Status attention. |
| No safe ownership information | “No owned Framework targets could be selected. Ownership information is unavailable.” Preserve the actual informational/no-effect result, not a fabricated repair gate. |
| Incomplete source or recovery | Name the unavailable fact and say no files changed when pre-effect evidence proves that. |
| Blocked | Name the cross-owner collision, unsafe target, or invalid generated boundary. |
| Partial failure/cancellation | Name verified replacements/restorations/deletions, ownership-publication state, remaining work, and recovery. |

Do not classify an intentionally retained verified recovery bundle as a failed cleanup. The public documentation's old baseline-preservation explanation needs reconciliation with this source policy.

### 05. index

Question: Which generated Entries sections were rebuilt? [Current interface](.agents/memory/crystallized/documents/cli/contracts/index-candidate/interface.md).

    > open-forge index
    Updated 2 of 12 generated Entries sections.
      .agents/guidance/_guidance.md
      .agents/patterns/_patterns.md

    > open-forge index --detail normal
    Updated 2 of 12 generated Entries sections.
      .agents/guidance/_guidance.md: 1 entry added.
      .agents/patterns/_patterns.md: 1 entry removed.
    The other 10 sections were already current.
    Verified both changed files.

Added/removed entry counts may be shown only if available from the accepted result or an approved, precisely defined diff calculation; otherwise show “Entries changed.” Do not invent counts by subtracting unrelated counters.

    > open-forge index guidance --dry-run
    Preview: rebuild 1 generated Entries section.
      .agents/guidance/_guidance.md
    No files changed.

| Scenario | Proposed default output |
| --- | --- |
| All current | “Generated Entries are current. 12 sections checked.” |
| No selected regions | “No generated Entries sections were selected.” State whether this is a valid empty selection or incomplete discovery. |
| Bad child metadata | “Cannot rebuild .agents/guidance/_guidance.md. Invalid metadata in .agents/guidance/team.md.” Include the actual parse location/cause and “No files changed” if pre-effect. |
| Incomplete discovery | “Entries could not be rebuilt completely. Could not read … No files changed.” |
| Recovery retained after cleanup failure | The rebuild remains successful; show residual path and attention/2. |
| Apply failure | Identify already verified sections and the exact failed/uncertain target. |

Heading-based Entries and automatic retirement of old generated guards already exist. This design does not introduce them again. Keep Index's current all-plan safety boundary; do not silently update the other 11 regions after one required region blocks.

### 06. repair

Question: Which selected repairs were applied, and which still need a choice? [Current interface](.agents/memory/crystallized/documents/cli/contracts/repair/interface.md).

    > open-forge repair --automatic --dry-run
    Preview: repair 1 link.
      .agents/guidance/team.md:12:8
      review.md#Review -> review.md#review
    No files changed.

    > open-forge repair --automatic --dry-run --detail normal
    Preview: repair 1 link.
      .agents/guidance/team.md:12:8
      Current destination: review.md#Review
      Intended destination: review.md#review
      The current target and corrected fragment were verified.
    A recovery bundle will protect the changed file.
    No files changed.

The fragment example assumes an actual safe-exact canonical correction in the fixture, not a guessed heading.

| Scenario | Proposed default output |
| --- | --- |
| Complete selected no-op | “No repairs are needed in the selected scope.” |
| Applied | “Repaired 1 link.” Show the occurrence and destination change. |
| Safe selected work with unresolved choices | “Repaired 1 link. 2 findings still need a choice.” Show the unresolved subjects. Preserve attention when coverage is complete. |
| No automatic repair available | “No automatic repairs were selected. 2 findings need a choice.” Do not say the workspace is healthy or fixed. |
| Incomplete required diagnosis | “Repair could not establish a complete plan. Could not inspect … No files changed.” |
| Stale explicit relink | “Cannot repair .agents/guidance/team.md:12:8. The destination no longer matches the supplied old value.” |
| Post-effect failure or cancellation | Report actual verified edits, uncertain effects, remaining findings, and recovery. A failed post-diagnosis does not erase applied edits. |

Correct explicit-relink grammar uses three values:

    open-forge repair --relink ".agents/guidance/team.md@12:8" "../old.md#Old" ".agents/guidance/review.md#New" --dry-run

The target and fragment must exist and be valid. Never present the historical two-value --relink or repair --rebind. Repair's complete result covers its selected repair scope; it does not promise every Doctor finding is gone. Preserve Library recovery steps as distinct from ordinary link rewrites.

### 07. cleanup

Question: Which recognized recovery artifacts were removed? [Current interface](.agents/memory/crystallized/documents/cli/contracts/cleanup/interface.md).

    > open-forge cleanup --dry-run
    Preview: remove 2 recovery artifacts.
      D:/recovery/demo/recovery-001.zip
      D:/recovery/demo/recovery-002.draft
    No recovery artifacts were removed.

These are illustrative result paths, not an assertion about generated naming.

    > open-forge cleanup --detail normal
    Removed 2 recovery artifacts.
      Removed verified bundle: D:/recovery/demo/recovery-001.zip
      Removed incomplete draft: D:/recovery/demo/recovery-002.draft
    Both deletions were verified.

Brief application retains those exact paths with shorter labels. Do not invent freed-byte totals unless the command actually measures them.

| Scenario | Proposed default output |
| --- | --- |
| Empty complete catalogue | “No recovery data to remove.” No lease or new recovery state is needed. |
| Incomplete catalogue | “Could not inspect all recovery data. Nothing was removed.” |
| Malformed recognized candidate | “Cannot remove the recovery catalogue safely. This candidate is malformed: …” Do not silently skip a blocking exact-name candidate. |
| Failure after one deletion | “Cleanup failed after removing 1 of 2 artifacts.” Show removed and remaining paths. |
| Cancellation after one deletion | “Cleanup was cancelled after removing 1 of 2 artifacts.” Show the same residual truth. |

Cleanup has no current attention case, no ID/path selector, no rollback, and no replacement recovery bundle. Unknown unrelated files remain outside its recognized catalogue. Do not suggest that cleanup will restore anything.

### 08. context

Question: What exact source content should be read for this request? [Current interface](.agents/memory/crystallized/documents/cli/contracts/context/interface.md).

Assume the selected fixture body is exactly the heading and sentence below, with its original line endings:

    > open-forge context guidance/team --additions-only --content body
    --- .agents/guidance/team.md ---
    # Team guidance

    Review the affected contract before changing a command.

The delimiter is generated framing. It is not part of the source. Normal detail adds a brief source explanation outside the authored span:

    > open-forge context guidance/team --additions-only --content body --detail normal
    Reading 1 additional source.
    Source: guidance/team
    Path: .agents/guidance/team.md
    Included because: explicitly requested.
    --- .agents/guidance/team.md ---
    # Team guidance

    Review the affected contract before changing a command.

Actual closure may include other required sources; this fixture assumes the selected additional closure contains just that one source. With no --content, preserve the existing frontmatter/body default. With no operands, return startup context. Do not remove source comments, empty sections, loading rules, or frontmatter to reduce output.

| Scenario | Proposed default output |
| --- | --- |
| Complete empty additions | “No additional context. The selected sources are already included at startup.” Only when that is the actual reason. |
| Requested section absent | “Section 'Decisions' was not found in .agents/guidance/team.md.” Retain attention and any other selected content. |
| Incomplete closure/content | “Context is incomplete. Could not read …” Then emit safe selected content without pretending the omitted source was empty. |
| Invalid additions-only | “--additions-only requires at least one source.” |
| Unsafe source | “Cannot read this context safely. The selected path resolves outside the workspace.” |

Keep the primary stream rules. Moving operation findings or a summary to stderr would be a separate stream-contract change, not an incidental way to clean up the body stream. JSON remains the reliable structured way to separate content, source identity, coverage, and findings. Selected content is never subject to the Doctor detail cap.

### 09. find

Question: Which sources match the supplied tag/heading predicates? [Current interface](.agents/memory/crystallized/documents/cli/contracts/find/interface.md).

    > open-forge find --tag Guidance
    Found 2 matching sources.
      guidance/review  .agents/guidance/review.md
      guidance/team    .agents/guidance/team.md

    > open-forge find --tag Guidance --detail normal
    Found 2 matching sources.
      guidance/review
        Path: .agents/guidance/review.md
        Description: Review steps for local changes.
        Matched tag: Guidance
      guidance/team
        Path: .agents/guidance/team.md
        Description: How the team reviews changes.
        Matched tag: Guidance
    Searched the default source set. All supplied predicates were required.

Preserve the current ordinal match ordering by source ID and its existing tie-breakers. --content adds the complete requested projection, with exact content and projection state.

| Scenario | Proposed default output |
| --- | --- |
| No matches, complete search | “No sources matched.” |
| Bare inventory empty | “No Markdown sources were found.” Only for a complete valid inventory. |
| Known missing projection | “Found 1 matching source. Section 'Decisions' is absent.” Preserve attention and the source identity. |
| Partial search | “Found 2 matching sources. The search is incomplete.” Name the unavailable source or projection. |
| No safe matches in incomplete search | “No matches could be confirmed. The search is incomplete.” |
| Invalid predicate | Name the exact invalid tag, heading, region, or repeated singleton option. |
| Blocked universe | Name the unsafe selector/source boundary; do not silently search a broader universe. |

This natural-text brief proposal replaces the current tab-separated result header and ID/path rows. That is an explicit G4 decision, not merely punctuation. I recommend natural human output and JSON for machines, but do not retire the TSV contract until approved. Do not add --paths, invent a tag catalogue from unmeasured data, or change default inventory/filter semantics in this output packet.

### 10. references

Question: Which direct authored links point into or out of this source? [Current interface](.agents/memory/crystallized/documents/cli/contracts/references-candidate/interface.md).

    > open-forge references guidance/team --direction out
    Found 2 outgoing authored references.
      .agents/guidance/team.md:6:3 -> .agents/guidance/review.md
      .agents/guidance/team.md:9:3 -> https://example.com/guide

    > open-forge references guidance/team --direction out --detail normal
    Found 2 outgoing authored references.
      .agents/guidance/team.md:6:3
        Written destination: review.md
        Resolved target: .agents/guidance/review.md
      .agents/guidance/team.md:9:3
        Written destination: https://example.com/guide
        External link; not fetched.
    Outgoing inspection is complete.

The default direction remains both. Show incoming and outgoing sections separately. Filtering the incoming source universe does not affect outgoing links.

| Scenario | Proposed default output |
| --- | --- |
| Complete empty both-directions scan | “No direct authored references found in the requested scope.” |
| Outgoing-only empty result | “No outgoing authored references found.” Do not imply incoming coverage. |
| Incomplete incoming scan | “Found 1 incoming reference. Incoming inspection is incomplete.” Name the unavailable scan source. |
| Safe non-blocking authored-form finding | Show the occurrence and cause; preserve attention. An external link not fetched is not automatically attention. |
| Outgoing-only with --include | “--include requires incoming inspection. Use --direction in or both.” Invalid. |
| Unsafe/ambiguous target | Name the source occurrence, raw destination, and unsafe or ambiguous boundary. |

Do not change “authored” into “all links.” Generated navigation is deliberately excluded by the current reference contract. Including it or adding --generated would be a separate behavior/interface change. Counts are occurrence counts; duplicate authored occurrences are not collapsed into one link.

## Route commands

### 11. route list

Question: Which routed sources exist at the requested structural depth? [Current interface](.agents/memory/crystallized/documents/cli/contracts/route/list/interface.md).

    > open-forge route list guidance --depth 1
    guidance         .agents/guidance/_guidance.md
      guidance/team  .agents/guidance/team.md
    Listed 2 routes at depth 1.

    > open-forge route list guidance --depth 1 --detail normal
    Routes under guidance:
      guidance
        Path: .agents/guidance/_guidance.md
        Description: Advice for recurring choices.
        Direct children: 1
      guidance/team
        Path: .agents/guidance/team.md
        Description: How the team reviews changes.
        Tags: Guidance, Team
    Listed 2 routes at depth 1. Requested coverage is complete.

Default depth remains 1. --depth all requests the full descendant closure; --depth 0 remains valid. Retain actual hierarchy and order, using complete IDs where shortening would be ambiguous.

| Scenario | Proposed default output |
| --- | --- |
| Complete empty roots | “No root routes are exposed by the Loader.” Only when that fact was established. |
| Complete selected leaf | Show the one selected leaf; do not say “no routes” merely because it has no children. |
| Partial topology | “Listed 2 confirmed routes. The requested route listing is incomplete.” Name the unresolved boundary. |
| Non-blocking identity/authored-form finding | Keep rows and the specific finding; preserve attention. |
| Bad depth | “Depth must be a non-negative integer or all.” |
| Unsafe/ambiguous root | Name the exact root/entrypoint and boundary. |

Do not print “2 of 100 routes” unless the unshown total was actually measured. A depth-limited complete result is complete for that request, not an incomplete scan. A useful optional footer is “Use --depth all to include all descendants”; it must not imply that hidden descendants definitely exist without evidence.

### 12. route inspect

Question: Where does one source belong, and when is it loaded? [Current interface](.agents/memory/crystallized/documents/cli/contracts/route/inspect/interface.md).

    > open-forge route inspect guidance/team
    guidance/team
    Path: .agents/guidance/team.md
    Route: guidance
    Loading: on demand.

    > open-forge route inspect guidance/team --detail normal
    guidance/team
    Path: .agents/guidance/team.md

    Where this source belongs
      Parent: guidance
      Ordinary routed Markdown source.

    When it is read
      On demand. No loading tag is present.
      Inherits the rules of the selected ancestor routes.

    Context size
      1 physical file; 420 UTF-8 bytes; about 105 tokens.

Use the current result's actual measurement method and mark estimates. These numbers describe an illustrative measured fixture, not this repository. Retain overwrite layers, loading conditions, source kind, supported unrouted/detached states, and uncertainty when applicable.

| Scenario | Proposed default output |
| --- | --- |
| Read-only repeated request | Repeat the observed profile; do not claim a no-op verification. |
| Exact path resolves a non-unique ID | “This file was selected by exact path. Its source ID also identifies another file.” Attention; show both identities when available. |
| Unreadable layer or incomplete route chain | Show the established identity and “The route profile is incomplete.” |
| Loader selected | “Route Inspect requires a source other than the Loader.” Invalid under the current interface. |
| Unknown source | “Source not found: guidance/missing.” Invalid; a bounded route list is a useful next inspection. |
| Unsafe overwrite/route boundary | Name the conflicting files and cause. Blocked. |

Keep the existing explanatory section names where useful. Avoid adding authored body output to this command.

### 13. route init

Question: Which missing entrypoints were created along this exact route chain? [Current interface](.agents/memory/crystallized/documents/cli/contracts/route/init/interface.md).

Assume the parent route already exists:

    > open-forge route init guidance/team --description "Team guidance" --tag Guidance
    Created the guidance/team route.
      Created .agents/guidance/team/_team.md
      Updated Entries in .agents/guidance/_guidance.md

    > open-forge route init guidance/team --description "Team guidance" --tag Guidance --detail normal
    Created the guidance/team route.
      .agents/guidance/team/_team.md
        Description: Team guidance
        Tags: Guidance
      .agents/guidance/_guidance.md
        Added the new route to Entries.
    Both file changes were verified.

| Scenario | Proposed default output |
| --- | --- |
| Matching route already initialized | “The requested route is already initialized. No changes.” |
| Default placeholder created | “Created the route. Its description and tags still need authoring.” Show every NeedsAuthoring entrypoint and preserve attention/2. |
| Preview | “Preview: create 1 route entrypoint.” List created entrypoints and affected ancestor Entries, then “No files changed.” |
| Framework scaffold mode | Name --framework and the selected scoped route; show any actual ownership publication. Do not imply it installs the root Framework. |
| Invalid mixed scaffold options | Explain that --framework conflicts with explicit description/responsibility/tags. |
| Incomplete/blocked chain | Name the first unresolved or unsafe route fact; keep any planned safe rows clearly unapplied. |
| Post-effect failure | Distinguish created entrypoints from ancestor navigation or ownership effects that did not finish. |

This is an explicit non-wizard leaf. Do not add --automatic or silently accept an invented current scope.

### 14. route create

Question: Was this ordinary Markdown source created and listed? [Current interface](.agents/memory/crystallized/documents/cli/contracts/route/create/interface.md).

    > open-forge route create guidance/team --description "Team review guidance" --tag Guidance
    Created .agents/guidance/team.md.
      Updated Entries in .agents/guidance/_guidance.md

    > open-forge route create guidance/team --description "Team review guidance" --tag Guidance --detail normal
    Created .agents/guidance/team.md.
      Description: Team review guidance
      Tags: Guidance
      Updated Entries in .agents/guidance/_guidance.md
    Both file changes were verified.

With --template, show which Template supplied the body and that the destination has its own metadata. The exact resulting body/diff, when the current result represents it, belongs at full detail in either format. Normal explains the relevant intended change. If authored content is explicitly requested by a command option, its complete selected content is required at every detail level.

| Scenario | Proposed default output |
| --- | --- |
| Verified identical intended target | “The requested file is already present with the intended content. No changes.” Preserve the contract's exact no-op boundary. |
| Different existing content | “Cannot create .agents/guidance/team.md. A different file already exists.” Blocked, not an overwrite prompt. |
| Missing metadata | “A nonblank description and at least one tag are required.” Invalid. |
| Invalid target kind | Name why the destination is an entrypoint, overwrite companion, directory, or otherwise ineligible. |
| Incomplete Template/source facts | Explain what cannot be read; do not substitute an empty body. |
| Recovery cleanup problem | Keep the successful creation and exact retained recovery path; attention. |
| Partial failure | Show whether the target file was created and whether parent Entries were updated. |

Do not add force, arbitrary content replacement, or a wizard. Repeated creation is a no-op only when the exact intended-state rule is satisfied.

### 15. route update

Question: Which explicit metadata or eligible Template changes were applied? [Current interface](.agents/memory/crystallized/documents/cli/contracts/route/update/interface.md).

    > open-forge route update guidance/team --description "How the team reviews changes"
    Updated .agents/guidance/team.md.
      Changed the description.
      Updated Entries in .agents/guidance/_guidance.md

    > open-forge route update guidance/team --description "How the team reviews changes" --detail normal
    Updated .agents/guidance/team.md.
      Description: Team guidance -> How the team reviews changes
      Updated Entries in .agents/guidance/_guidance.md
    Both file changes were verified.

| Scenario | Proposed default output |
| --- | --- |
| Exact metadata no-op | “The requested metadata is already current. No changes.” |
| Empty responsibility value | “Removed the responsibility from .agents/guidance/team.md.” Only when the key was actually removed. |
| Protected Template body | “The existing body was kept. The Template was not copied.” Name the source; preserve attention even when independent metadata changes succeeded. |
| No patch supplied | “Provide a metadata change or --template.” Invalid. |
| Incomplete source/Template | Name the unreadable or ambiguous content; no write begins. |
| Unsafe/ambiguous target | Name the boundary and keep the target unchanged. |
| Partial failure | Report metadata/body/navigation effects separately on their actual physical paths. |

Repeated tags replace the complete ordered tag list; output must not call them an append. Do not widen Template eligibility or add whole-body replacement as an output improvement.

### 16. route move

Question: What moved, and which references/navigation changed with it? [Current interface](.agents/memory/crystallized/documents/cli/contracts/route/move/interface.md).

    > open-forge route move guidance/team .agents/guidance/review-team.md
    Moved .agents/guidance/team.md to .agents/guidance/review-team.md.
      Updated references in .agents/guidance/start.md
      Updated Entries in .agents/guidance/_guidance.md

    > open-forge route move guidance/team .agents/guidance/review-team.md --dry-run --detail normal
    Preview: move .agents/guidance/team.md to .agents/guidance/review-team.md.
      .agents/guidance/start.md:8:3
        team.md -> review-team.md
      .agents/guidance/_guidance.md
        Replace the old Entries destination with the new destination.
    No files changed.

The two invocations represent alternatives from the same initial fixture, not a sequence in which the old source still exists after the first move.

| Scenario | Proposed default output |
| --- | --- |
| Complete category move | Name old and new category roots and list every changed member/companion/reference/navigation path. |
| Repeating the consumed old source | “Source not found: guidance/team.” Invalid, not “already moved.” |
| Destination exists | “Cannot move to .agents/guidance/review-team.md. The destination already exists.” Blocked. |
| Managed subject | Explain its actual owner and that Route Move cannot move it under this contract. |
| Incomplete reference scan | “Cannot prepare the move because reference inspection is incomplete.” No writes. |
| Failure/cancellation after effects | State source/destination existence and verification as actually observed, plus rewritten/unwritten references and recovery. |

There is no generic no-change result for rerunning a move. An extra positional operand can fail at the shell before a Route Move envelope exists. Preserve base/overwrite identity and category semantics; do not turn a category into independent partial leaf moves.

### 17. route remove

Question: What was removed, and what happened to incoming links? [Current interface](.agents/memory/crystallized/documents/cli/contracts/route/remove/interface.md).

    > open-forge route remove guidance/team
    Removed .agents/guidance/team.md.
      Removed the incoming link in .agents/guidance/start.md; kept its text.
      Updated Entries in .agents/guidance/_guidance.md

    > open-forge route remove guidance/team --dry-run --detail normal
    Preview: remove .agents/guidance/team.md.
      .agents/guidance/start.md:8:3
        Change [team guidance](team.md) to team guidance.
      .agents/guidance/_guidance.md
        Remove the source from Entries.
    No files changed.

| Scenario | Proposed default output |
| --- | --- |
| Category removal | Name the category and show all selected member, companion, incoming-link, and Entries changes. |
| Proven intended absence | “The requested target is already absent. No changes.” Only with independent complete proof accepted by the current contract. |
| Merely missing source | “Source not found: guidance/team.” Invalid; do not infer earlier successful removal. |
| Managed/unsafe source | Name the actual ownership or safety boundary. Blocked. |
| Unsupported incoming transformation | Name the reference occurrence that cannot safely be detached. Blocked, not a blind text rewrite. |
| Incomplete catalogue/reference scan | No deletion begins; explain the missing evidence. |
| Failure/cancellation after effects | List deleted, retained, uncertain, and untouched paths plus recovery; do not claim rollback. |

Removing link markup must preserve the link text only where the accepted reference transformation permits it. A source result that supports another bounded transformation needs its own faithful wording.

## Extension commands

### 18. extension list

Question: Which packages are installed, and which are available from this source? [Current interface](.agents/memory/crystallized/documents/cli/contracts/extension/list/interface.md).

    > open-forge extension list
    Installed
      team-guidance  1.0
    Available
      review-tools   1.0
      team-guidance  1.1

    > open-forge extension list --detail normal
    Installed
      team-guidance  1.0
        Recorded for this workspace.
    Available from the selected catalogue
      review-tools   1.0  Review templates.
      team-guidance  1.1  Team review guidance.

These are hypothetical package IDs and descriptive versions in an illustrative catalogue. They are not claimed bundled packages, and versions do not imply SemVer upgrade policy.

| Scenario | Proposed default output |
| --- | --- |
| Both sections known empty | “No installed or available Extensions were found.” |
| Installed section requested and empty | “No installed Extensions are recorded.” |
| Ownership unavailable | “Installed Extensions could not be read from ownership information.” Preserve the informational/no-gate result and independently available packages. |
| Available source unavailable | Name the selected source and retain available installed facts; do not print “no packages.” Use the actual attention/incomplete condition. |
| Invalid source or flags | Name the exact input problem. |
| Unsafe overlapping source/target | Name both boundaries; blocked. |

Both sections are intentional. Do not merge identical IDs across Installed and Available or hide an available package because it is installed. --installed and --available retain their current meanings.

### 19. extension inspect

Question: What does this package provide, and how does it compare with recorded installation facts? [Current interface](.agents/memory/crystallized/documents/cli/contracts/extension/inspect/interface.md).

    > open-forge extension inspect team-guidance --source ../catalogue
    team-guidance
    Installed version: 1.0. Available version: 1.1.
    1 content path differs from the selected package.
      .agents/guidance/team.md

    > open-forge extension inspect team-guidance --source ../catalogue --detail normal
    team-guidance
    Installed version: 1.0. Available version: 1.1.
    Source: ../catalogue
    Dependencies: review-tools
    Content
      .agents/guidance/team.md: current content differs from intended content.
    Comparison completed.

Show the actual path relations and ownership information. Supporting current/intended fingerprints and provenance require full detail in both formats. Neither brief nor normal should add opaque fingerprints to an otherwise useful comparison.

| Scenario | Proposed default output |
| --- | --- |
| Equal complete comparison | “team-guidance matches the selected package.” |
| Available but not recorded installed | “team-guidance is available. No installed ownership is recorded.” |
| Installed facts but unavailable source | State the installed facts and “The package comparison is unavailable.” |
| Finite complete divergence | Show changed/missing/retired relations and preserve attention. |
| Incomplete dependency/source facts | State exactly which comparison could not finish. |
| Invalid/malformed ID | Name the ID grammar or cardinality problem. |
| Ambiguous or colliding ownership | Name the actual identity conflict; blocked. |

Unknown/unavailable ownership is not evidence that a path is safe to adopt or delete. “Not recorded” and “not present” are different claims.

### 20. extension create

Question: Where was the package scaffold created? [Current interface](.agents/memory/crystallized/documents/cli/contracts/extension/create/interface.md).

    > open-forge extension create team-guidance --path ../catalogue --automatic
    Created the team-guidance Extension scaffold.
    Package: ../catalogue/team-guidance

This brief transcript is a heading excerpt: the complete receipt must also enumerate every actual created scaffold path once. Normal detail adds the effective name, description, package-version, dependencies, and validation summary. Full adds available schema/validation evidence. Display an edit reminder for the scaffold's real authoring entry, using its actual path from the result; do not invent a manifest filename.

    > open-forge extension create team-guidance --path ../catalogue --dry-run --detail normal
    Preview: create the team-guidance Extension scaffold.
    Catalogue: ../catalogue
    Package: ../catalogue/team-guidance
    The complete scaffold path list appears below.

The final sentence marks this as an excerpt. Actual normal-detail output must print the path list, then “No files changed.”

| Scenario | Proposed default output |
| --- | --- |
| Identical intended scaffold | “The team-guidance scaffold already matches the request. No changes.” |
| Different existing destination | “Cannot create team-guidance. Its destination contains different content.” Blocked. |
| Missing ID/path in noninteractive mode | Name whichever required value is missing and provide a valid explicit example. Invalid. |
| Unreadable catalogue fact | “The scaffold could not be planned completely. Could not inspect …” |
| Failure halfway through create | Name created, failed, and remaining scaffold paths. Do not imply a workspace recovery bundle exists. |
| User cancels | Describe whether input ended before planning or creation stopped after effects; preserve the actual result code. |

No attention state is currently defined. --workspace is a valid no-op here; the catalogue is the subject. This operation creates content/ package layout and does not install the package into a workspace.

### 21. extension install

Question: Which selected packages and dependencies were installed? [Current interface](.agents/memory/crystallized/documents/cli/contracts/extension/install/interface.md).

    > open-forge extension install team-guidance --source ../catalogue --automatic
    Installed team-guidance and 1 dependency.
      review-tools: dependency
      Created .agents/guidance/team.md
      Created .agents/templates/review.md
      Updated Entries in .agents/guidance/_guidance.md
      Updated Entries in .agents/templates/_templates.md
      Updated .agents/open-forge.lock.json

The fixture assumes destination admission is already granted. When it is not, show the missing scope and require the current --allow-path or interactive permission boundary.

Normal detail retains these effects and adds source catalogue, root selection versus dependency closure, versions, current/intended comparison, permission facts, verification, and recovery when applicable. Do not repeat a common dependency's files under every selected package.

| Scenario | Proposed default output |
| --- | --- |
| Exact installed no-op | “team-guidance is already installed with the intended content. No changes.” |
| Current managed divergence | “Cannot install over the changed team-guidance installation.” Name the path; Next: open-forge extension update team-guidance --source ../catalogue --dry-run. |
| Explicit preview | Prefix “Preview: install …” and show the whole dependency/effect plan. No writes. |
| No selection from a multi-package catalogue | Show that exact IDs or --all are required, and suggest extension list with the same source. Preserve the current typed selection-required status. |
| Missing dependency | Name required package and requesting package, within the selected offline source universe. |
| Missing permission | Name the exact destination scope. Do not silently add --force or --all. |
| Partial application | Show package/file effects and ownership-publication state; a root package headline must not imply all dependencies installed. |

Keep current single-package inference where the resolver legitimately supports it. Do not invent an extra selection prompt for a request whose subject is already exact.

### 22. extension update

Question: Which selected installed packages changed against the selected source? [Current interface](.agents/memory/crystallized/documents/cli/contracts/extension/update/interface.md).

    > open-forge extension update team-guidance --source ../catalogue --automatic
    Updated team-guidance.
      Replaced .agents/guidance/team.md
      Updated .agents/open-forge.lock.json
    Recovery saved: D:/recovery/demo/extension-update-001.zip

Normal detail adds available/installed descriptive versions, selected dependency closure, exact current/intended/retired path relations, grants, verification, and recovery protection. A differing version string is not itself proof that content changed.

| Scenario | Proposed default output |
| --- | --- |
| Verified no-op | “team-guidance already matches the selected package. No changes.” |
| Retired content preserved | “Updated team-guidance. Kept 1 retired file: …” Status attention; suggest the same source/ID request with --prune --dry-run. |
| Installed ownership cannot be established | “team-guidance could not be matched to installed ownership. No update effects were inferred.” Preserve the command's attention condition. |
| Complete --all with no selected managed packages | Report the actual complete empty selection; do not infer packages from disk. |
| Incomplete source/dependencies | Name the missing fact and show no writes for pre-effect failure. |
| Cross-owner collision or unsafe projection | Name package, target, and conflicting owner. Blocked. |
| Failure/cancellation | Retain exact verified paths, not-started paths, ownership publication, and recovery. |

--all is explicit and conflicts with explicit IDs. Keep source selection offline and exact. Do not revive baseline fingerprints or silently change force/prune semantics.

### 23. extension remove

Question: Which ownership claims and final-owner files were removed? [Current interface](.agents/memory/crystallized/documents/cli/contracts/extension/remove/interface.md).

    > open-forge extension remove team-guidance --automatic
    Removed team-guidance.
      Deleted .agents/guidance/team.md
      Updated Entries in .agents/guidance/_guidance.md
      Updated .agents/open-forge.lock.json
    Recovery saved: D:/recovery/demo/extension-remove-001.zip

    > open-forge extension remove team-guidance --dry-run --detail normal
    Preview: remove team-guidance.
      Delete final-owner file .agents/guidance/team.md
      Keep .agents/templates/review.md; review-tools also owns it.
      Update Entries in .agents/guidance/_guidance.md
      Update .agents/open-forge.lock.json
    A recovery bundle will protect deleted content.
    No files changed.

The preview uses a fixture with one shared file; it is a different result from the first example. It makes the shared-owner distinction concrete.

| Scenario | Proposed default output |
| --- | --- |
| No selected claims | “No ownership is recorded for team-guidance. No files were removed.” |
| Ownership unavailable | Say ownership could not be read and no deletion was inferred. Do not call the package safely absent. |
| Orphaned dependency remains | “Removed team-guidance. review-tools remains installed and is no longer required by it.” Preserve attention only under the actual orphan condition. |
| Retained dependent blocks removal | Name the installed dependent that still needs the selected package. |
| Missing permission | Name the denied destination scope; preserve claim/effect facts. |
| Incomplete route/recovery facts | No mutation begins; identify the unavailable boundary. |
| Partial removal | Show removed claims, deleted files, retained shared files, failed/unknown effects, and bundle path exactly as observed. |

Current removal does not distinguish changed versus unchanged final-owner content through stored baselines. There is no --prune, --force, --all, or --source for this leaf. Its retained verified recovery is ordinary success, not cleanup failure.

## Library commands

### 24. library list

Question: Which Library registrations and registered links can be observed? [Current interface](.agents/memory/crystallized/documents/cli/contracts/library/list/interface.md).

    > open-forge library list
    team-knowledge  vendor/team-knowledge -> .agents/guidance
      4 registered links are current.

    > open-forge library list --detail normal
    team-knowledge
      Source: vendor/team-knowledge
      Destination: .agents/guidance
      Registered links: 4 current.
      Source root is available.
    Source contents were not inventoried.

Normal detail should list actual abnormal registered-link observations and supporting record facts. A normal result must not imply that new or retired source files were checked; that belongs to inspect.

| Scenario | Proposed default output |
| --- | --- |
| Known empty registration set | “No Libraries are registered.” |
| Ownership missing/unusable | “No Library registrations could be read. Ownership information is unavailable.” Preserve complete plus informational finding where current policy does. |
| Missing/changed registered link | “team-knowledge needs attention: 1 registered link is missing.” Show its destination. |
| Unreadable registered-link fact | “Library observations are incomplete.” Name the ID/path. |
| Invalid recorded source shape | Name the malformed source-root/claim condition; use the command's invalid result. |
| Unsafe identity or containment | Name the boundary; blocked. |

Do not print the removed Libraries record path. Registration evidence comes from the libraries section of .agents/open-forge.lock.json.

### 25. library inspect

Question: How does the complete source inventory compare with this Library's projection? [Current interface](.agents/memory/crystallized/documents/cli/contracts/library/inspect/interface.md).

    > open-forge library inspect team-knowledge
    team-knowledge is current.
    4 source files match 4 registered links.

    > open-forge library inspect team-knowledge --detail normal
    team-knowledge is current.
    Source: vendor/team-knowledge
    Destination: .agents/guidance
    Source inventory and projection comparison are complete.
    4 source files match 4 registered links.

The complete normal-detail result adds each ordered source/destination relation and actual link evidence. Do not show invented fingerprints for a link identity.

| Scenario | Proposed default output |
| --- | --- |
| Complete empty source/registration | “team-knowledge is current. No eligible source files or registered links.” |
| New source files | “team-knowledge has 2 source files that are not projected.” Show the mappings; attention, with a sync --dry-run recommendation when safe. |
| Retired source files | “team-knowledge has 1 retired projection.” Show the exact registered destination. |
| Missing/changed links | Show each actual relation; do not suggest sync can overwrite an ordinary replacement file. |
| Source unavailable | “team-knowledge could not be fully inspected. Source unavailable: vendor/team-knowledge.” Incomplete, never an empty inventory. |
| Unknown ID with established records | “Library not found: team-knowledge.” Invalid. |
| Unsafe mapping/source/link | Name the exact boundary and preserve blocked. |

Unlike list, inspect claims a complete source inventory only when that inventory actually finished.

### 26. library attach

Question: Which new Library registration and file links were created? [Current interface](.agents/memory/crystallized/documents/cli/contracts/library/attach/interface.md).

Assume this fixture's source has one file and the destination parent is already routed:

    > open-forge library attach team-knowledge vendor/team-knowledge --to .agents/guidance --allow-path .agents/guidance
    Attached team-knowledge.
      Linked .agents/guidance/team.md to vendor/team-knowledge/team.md
      Updated Entries in .agents/guidance/_guidance.md
      Updated .agents/open-forge.lock.json
      Updated allowed destinations in .agents/open-forge.json

The grant-publication line is present only when this explicit grant changes settings. Normal detail adds the mapping's actual relative link target, complete inventory result, grant/effect verification, ownership publication, and recovery facts. The first line must not claim registration succeeded when only a link was created.

    > open-forge library attach team-knowledge vendor/team-knowledge --to .agents/guidance --dry-run
    Preview: attach team-knowledge.
      Link .agents/guidance/team.md to vendor/team-knowledge/team.md
      Update Entries in .agents/guidance/_guidance.md
      Record the Library in .agents/open-forge.lock.json
    No files changed.

That preview assumes the required destination grant is already present; otherwise report the missing grant rather than a ready-to-apply plan.

| Scenario | Proposed default output |
| --- | --- |
| Empty eligible source | “Attached team-knowledge. The source contains no eligible files.” A verified registration can still be a change. |
| Repeated registered ID | “Cannot attach team-knowledge. That Library ID is already registered.” Blocked, not an idempotent attach no-op. |
| Missing source directory | “Source directory not found: vendor/team-knowledge.” Invalid. |
| Incomplete source inventory | “The Library could not be attached because its source inventory is incomplete.” No writes. |
| Destination collision | “Cannot create .agents/guidance/team.md. An unrelated file already exists.” |
| Link capability unavailable | State the observed capability boundary; do not silently copy instead. |
| Partial failure | State which links verified, whether settings/ownership published, which mappings remain, and exact recovery facts. |

The source root is a portable relative directory strictly inside the workspace. Do not use an external ../ source root or claim Library attach is the same input model as Extension --source. Preserve source contents.

### 27. library sync

Question: Which added or retired mappings were reconciled against the current source? [Current interface](.agents/memory/crystallized/documents/cli/contracts/library/sync/interface.md).

    > open-forge library sync team-knowledge
    Synced team-knowledge.
      Added link .agents/guidance/new.md
      Removed retired link .agents/guidance/old.md
      Updated Entries in .agents/guidance/_guidance.md
      Updated .agents/open-forge.lock.json

    > open-forge library sync team-knowledge --dry-run --detail normal
    Preview: sync team-knowledge.
    Source inventory is complete.
      Add .agents/guidance/new.md -> vendor/team-knowledge/new.md
      Remove the exact registered link .agents/guidance/old.md
      Update Entries in .agents/guidance/_guidance.md
      Update .agents/open-forge.lock.json
    No files changed.

| Scenario | Proposed default output |
| --- | --- |
| Verified no-op | “team-knowledge is current. No changes.” |
| Ownership unavailable | “No Library changes were inferred. Ownership information is unavailable.” Current planner can return an informational complete no-effect result. |
| Established record, unknown ID | “Library not found: team-knowledge.” Preserve invalid, distinct from missing ownership information. |
| Missing/unreadable source | “Cannot complete the sync. Source unavailable: vendor/team-knowledge.” Incomplete; no retirements are inferred. |
| Changed destination occupant | “Cannot replace .agents/guidance/team.md. It no longer matches the registered link.” Blocked. |
| Recovery cleanup retained | Show successful links and exact residual path; attention. |
| Partial failure/cancellation | Show actual link states and whether final registration publication occurred. |

There is no --force, --automatic, or --prune for sync. Do not promise that a missing or changed registered link can always be repaired by sync; its exact preconditions control that result.

### 28. library detach

Question: Which exact registered projections and claims were removed? [Current interface](.agents/memory/crystallized/documents/cli/contracts/library/detach/interface.md).

    > open-forge library detach team-knowledge
    Detached team-knowledge.
      Removed link .agents/guidance/team.md
      Updated Entries in .agents/guidance/_guidance.md
      Updated .agents/open-forge.lock.json
    Source files were kept.

    > open-forge library detach team-knowledge --dry-run --detail normal
    Preview: detach team-knowledge.
      Remove the exact registered link .agents/guidance/team.md
      Update Entries in .agents/guidance/_guidance.md
      Remove the Library claim from .agents/open-forge.lock.json
    Source: vendor/team-knowledge; unchanged.
    No files changed.

| Scenario | Proposed default output |
| --- | --- |
| Registration with no links | Report removal of the registration and any actual generated effect. Do not call it “nothing changed.” |
| Ownership unavailable | “No Library files were removed. Ownership information is unavailable.” Preserve the current informational complete no-effect result. |
| Unknown ID in established records | “Library not found: team-knowledge.” Invalid; a second detach is not automatically a no-op. |
| Source absent but exact links provable | Detach can still complete because it is source-independent. |
| Changed/missing/unproven registered occupant | “Cannot detach team-knowledge safely. The registered destination no longer has the expected link.” Name the path; no partial preflight deletion. |
| Incomplete consumer/recovery facts | Explain which required fact cannot be established. |
| Failure/cancellation after effects | State removed links, remaining/uncertain links, claim-publication state, and recovery. |

Detach does not remove source files. It does not ignore changed occupants or silently implement partial detach. Preserve other Framework, Extension, and Library claims when updating the shared lock.

## Shared exceptional-state transcripts

These templates complete the event cases for every command in the catalogue. Substitute the exact command, actual typed cause, and observed paths. They do not add new statuses to a command.

### Invalid domain input

    > open-forge route update guidance/team
    The request is invalid: no update was supplied.
    Provide --description, --responsibility, --tag, or --template.

Expected: invalid, exit 4, stderr. If a command-local result exists, its JSON form preserves the error as one stdout envelope. Missing semantic input must be distinguished from a complete request lacking write permission.

### Rejection before a domain result exists

    > open-forge route move guidance/team .agents/guidance/new.md extra
    Unrecognized command or argument 'extra'.

This wording is illustrative. Preserve the actual parser's diagnostic and documented grammar. The current pre-binding failure is exit 4, stderr, no Route Move result even when the existing --json flag is supplied. The proposed --projection json preserves that exception. An invalid global combination, unknown command, unknown option, or invalid terminal combination may take this shell-owned path. There is no fabricated workspace or command result.

### An incomplete read

    > open-forge find --tag Guidance
    Found 1 matching source. The search is incomplete.
      guidance/team  .agents/guidance/team.md
    Could not read .agents/guidance/review.md.

Expected: incomplete, exit 3, stdout. The safe match remains useful. The output does not claim the unreadable file was not a match. Read-only unexpected failure and cancellation follow the same truth principle but retain failed/1 or interrupted/130.

### A blocked preflight

    > open-forge route create guidance/team --description "Team guidance" --tag Guidance
    Cannot create .agents/guidance/team.md.
    A different file already exists at that path.
    No files changed.

Expected: blocked, exit 5, stderr. The last line is valid only because this scenario stopped before effects. Show known planned effects as planned, never as completed.

### An apply failure with partial effects

    > open-forge index
    Indexing failed after 1 verified file change.
      Updated .agents/guidance/_guidance.md
      Could not verify .agents/patterns/_patterns.md; its final state is unknown.
      Not started: .agents/maps/_maps.md
    Recovery saved: D:/recovery/demo/index-001.zip
    Review the reported file states before starting another operation.

Expected: failed, exit 1, stderr. “Unknown” is not “unchanged,” “restored,” or “retained.” If no valid recovery bundle is known, do not print “Recovery saved.” State only an exact expected/observed path the result actually supplies.

The equivalent JSON result contains all these effects and their existing outcome/residual coordinates on stdout. Changing human wording does not change those enums.

### Cancellation

Before effects:

    The operation was cancelled. No files changed.

After effects:

    The operation was cancelled after 1 verified change.
      Updated .agents/guidance/_guidance.md
      Not started: .agents/patterns/_patterns.md
    Recovery saved: D:/recovery/demo/operation-001.zip

Expected: interrupted/130 unless an actual failure or stronger command-local residual rule forms a different result. Do not promise this precise friendly text for unhandled abrupt termination or power loss.

### Successful work with cleanup attention

    Updated .agents/guidance/team.md.
    Recovery data could not be removed: D:/recovery/demo/operation-001.zip
    Next: open-forge cleanup --dry-run

Expected: attention/2 only for commands whose post-verification cleanup returned the accepted Failed/positively-Retained condition. The path is still known. An unknown disposition can be failed/1. Intentionally retained recovery after Update or Extension removal uses normal success wording instead.

### Output failure

If formatting or writing fails after effects, never rerun the operation to reconstruct a prettier result. Use the existing pipeline's bounded failure path and preserve its completion rules. Broken pipes, writer cancellation, renderer exceptions, and fallback unavailability require distinct verification cases. A secondary output failure must not falsely assert that the requested operation made no changes.

## Status coverage matrix

All 28 leaves retain invalid, failed, and interrupted in their current contracts. Every leaf can report a required-coverage incomplete result or an unsafe-boundary blocked result. Their exact producers and precedence remain command-local; these generic rows are not permission to broaden them.

| Command | Complete empty or no-change case | Attention condition to retain | Partial effects when application fails/cancels |
| --- | --- | --- | --- |
| status | Complete observation, including accurately absent components | Established drift, relevant lifecycle observation, or recognized recovery | No writes |
| doctor | Complete diagnosis without actionable warnings/errors | Actionable warnings/errors with complete coverage | No writes |
| install | Exact managed installation | Positively retained recovery after cleanup failure | Yes |
| update | Verified no selected changes | Kept retired content | Yes; intended recovery retention can still be complete |
| index | All selected regions current | Positively retained recovery | Yes |
| repair | No needed edits within the selected complete scope | Remaining selected-scope findings or retained cleanup residual | Yes; post-diagnosis also matters |
| cleanup | Complete empty catalogue | Not currently reachable | Yes; verified deletions stay deleted |
| context | Complete empty additions/projection | Known absent section or safe case mismatch | No writes |
| find | Complete zero matches | Known projection omission or identity collision | No writes |
| references | No authored occurrences in requested directions | Safe authored-form/identity findings | No writes |
| route list | Complete empty roots or requested topology | Safe authored-form/identity findings | No writes |
| route inspect | Complete observed profile | Resolved exact source with non-unique automatic ID | No writes |
| route init | Already initialized, verified equivalent state | NeedsAuthoring or retained cleanup residual | Yes |
| route create | Exactly matching intended target | Retained cleanup residual | Yes |
| route update | Requested patch is already satisfied | Protected Template body or retained cleanup residual | Yes |
| route move | Repeating a consumed source is invalid, not no-op | Retained cleanup residual | Yes |
| route remove | Intended absence only if independently proven | Retained cleanup residual | Yes |
| extension list | Known empty requested sections | Finite source/installed observations | No writes |
| extension inspect | Complete equal or valid empty comparison | Finite complete comparison divergence | No writes |
| extension create | Exact intended scaffold exists | Not currently reachable | Yes; no workspace bundle is implied |
| extension install | Exact installed package content | Finite accepted observation or retained cleanup residual | Yes |
| extension update | Exact complete selected source comparison | Retired content or ownership cannot be established | Yes |
| extension remove | No selected claims; no inferred deletion | Finite orphaned-dependency observation | Yes |
| library list | Known empty registrations or informational ownership absence | Missing/changed registered links | No writes |
| library inspect | Complete empty or matching inventory/projection | Safe additions, retirements, missing/changed links | No writes |
| library attach | Empty source can still require a new registration; duplicate ID is blocked | Retained cleanup residual | Yes |
| library sync | Verified current projection or informational ownership absence | Retained cleanup residual | Yes |
| library detach | Empty registration can still be removed; unknown ID is invalid | Retained cleanup residual | Yes |

For each incomplete/blocked row, the corresponding command section names representative actual boundaries. The shared invalid/failure/cancellation templates supply the wording form, while snapshots must use that command's real result fields and finite findings. Do not create 196 nearly identical tests by multiplying 28 commands by seven statuses without checking reachability.

## JSON packet

### One schema for detail selection in every format

Propose schemaVersion 3 for the replacement interface, with this ordered envelope:

    schemaVersion, command, status, detail, workspace, result, next

All seven members are present. Detail is brief, normal, or full. Workspace and next retain their existing nullable meanings; result is a non-null object. Command identities remain exact, such as "library list". Existing finding kinds, resolution lanes, subject identities, and source-coordinate meanings are preserved. The exact version number is a proposal; changing detail never changes it.

The new schema explicitly defines different result membership by detail. A brief Doctor result contains coverage and severity counts, plus indispensable limitations/reasons if applicable. It contains no findings array, candidateSets, domain catalogue, evidence, or diagnostic graph. An omitted collection means this detail level did not request it; it does not mean the collection was inspected and found empty.

This is an intentional interface/schema change. Current schema 1 is the expanded contract, and current schema 2 is the compact contract with substantial retained data. Neither is an adequate label for these new reduced selections. There is no recommendation to ship a short text default while leaving JSON at the old full membership.

Keep JSON minified at every level. Indentation of multiline examples below is for report readability only. Formatting metadata such as command, detail, workspace selection, and typed status makes JSON somewhat longer than the equivalent sentence. It must not introduce extra substantive result detail.

Counts in the new brief Doctor object use error, warning, and information keys. Each is a nonnegative integer when available, or the exact state string "unavailable" or "not-applicable" when that count has that state. This is a proposed compact representation of the existing typed count distinction, not permission to replace unknown with zero. In incomplete diagnoses, counts remain the observed counts and coverage/limitations state why they are not a complete workspace inventory. The --severity info option selects the existing information severity; it does not rename finding severity on the wire.

### Complete small examples

Known-readable empty Library registrations, with source inventory intentionally not requested:

    > open-forge library list --projection json
    {"schemaVersion":3,"command":"library list","status":"complete","detail":"brief","workspace":{"path":"D:/work/demo","selectedBy":"current-directory"},"result":{"coverage":"complete","registrations":"known","libraries":[]},"next":null}

Equivalent brief text:

    > open-forge library list
    No Libraries are registered.

Full detail of that same result preserves the actual record observation and inspection boundary:

    > open-forge library list --detail full --projection json
    {
      "schemaVersion": 3,
      "command": "library list",
      "status": "complete",
      "detail": "full",
      "workspace": {
        "path": "D:/work/demo",
        "selectedBy": "current-directory"
      },
      "result": {
        "coverage": "complete",
        "registrations": "known",
        "libraries": [],
        "record": {
          "path": ".agents/open-forge.lock.json",
          "state": "complete",
          "libraryCount": 0
        },
        "inventory": "not-requested",
        "findings": []
      },
      "next": null
    }

Equivalent full text:

    > open-forge library list --detail full
    No Libraries are registered.
    Workspace: D:/work/demo
    Ownership record: .agents/open-forge.lock.json; readable.
    Registered-link checks completed.
    Source inventory was not requested.
    No findings.

These complete examples have exit 0 and no stderr because the fixture has no diagnostic records. A missing or unreadable ownership lock is a different result. At brief detail, use registrations="unavailable", libraries=null, and an applicable reason; retain independently known facts and the actual informational result. Do not call that state a known-empty registration set.

Doctor's brief JSON appears at the start of the report. A targeted normal selection of that same diagnosis can request one information item:

    > open-forge doctor --detail normal --severity info --limit 1 --projection json
    {
      "schemaVersion": 3,
      "command": "doctor",
      "status": "attention",
      "detail": "normal",
      "workspace": {
        "path": "D:/work/demo",
        "selectedBy": "current-directory"
      },
      "result": {
        "coverage": "complete",
        "counts": {
          "error": 0,
          "warning": 10,
          "information": 100
        },
        "selection": {
          "severities": ["information"],
          "limit": 1,
          "matching": 100,
          "shown": 1,
          "omittedBySeverity": 10,
          "omittedByLimit": 99
        },
        "findings": [
          {
            "domain": "local-references",
            "kind": "reference.target-valid",
            "severity": "information",
            "message": "The local reference target is valid.",
            "subject": {
              "kind": "source-occurrence",
              "path": ".agents/guidance/team.md",
              "id": null,
              "location": {
                "line": 6,
                "column": 3
              }
            },
            "target": "review.md",
            "resolution": "informational",
            "actions": []
          }
        ]
      },
      "next": {
        "command": "open-forge doctor --detail normal --severity info --limit all --projection json",
        "reason": "Show the remaining matching findings."
      }
    }

Equivalent text, changing only projection:

    > open-forge doctor --detail normal --severity info --limit 1
    No errors found. 10 warnings; 100 information items.
    Workspace: D:/work/demo
    Showing 1 of 100 matching information items. 10 warnings omitted by severity.
    Information: The local reference target is valid.
      .agents/guidance/team.md:6:3 -> review.md
    Next: open-forge doctor --detail normal --severity info --limit all

The original diagnosis still has 110 findings and returns attention/2. Each renderer receives the same one selected finding, and both expose the 10 excluded warnings and 99 remaining information items. The normal JSON finding shape is deliberately selected: a stable domain/kind, severity, message, identifying subject/location, applicable target, resolution, and direct actions. Supporting evidence, provenance, candidates, and proposals that require detailed explanation belong to full; an indispensable boundary or actionable exact replacement must not disappear merely because its current model stores it in an evidence object.

At full detail, Doctor retains this selected-finding structure and adds each finding's available typed evidence, full coordinates, provenance, candidate records, proposals, and action support. It also adds all six domain observations, boundaries, coverage, lifecycle/source availability, limitations, severity/resolution counts, and diagnosis-level actions. Domain metadata references the selected findings rather than duplicating their full payloads. Unfiltered full selects all findings; explicit severity/limit selections are recorded just as at normal detail. This is the complete fact-preservation rule; a field-by-field schema and complete full Doctor fixture still belong in the later reviewed freeze. The report does not fabricate a captured full result or run Doctor to create one.

### Selection matrix for all 28 commands

This matrix defines information selection for both renderers. Brief and normal correspond to the command transcripts above. Full adds the available supporting facts named here; it does not merely add whitespace. The current typed command result remains the source of truth about which facts exist. Do not invent a field or perform new inspection just to fill a detail level.

For every command, preserve its actual status, relevant coverage limits, exact requested payloads, indispensable subjects/actions, and essential mutation receipt. A full field list is a schema-freeze deliverable, not permission for the JSON renderer to serialize unselected internal objects. The following memberships are proposed semantics; the complete current wire graphs are evidence for mapping full detail, not the default payloads to keep unchanged.

| Command | Brief: smallest useful answer | Normal adds | Full adds |
| --- | --- | --- | --- |
| status | Installed/usable state, startup estimate, route freshness summary, actionable boundaries and residuals | Context comparisons, continuity and available totals, meaningful Framework/Extension/Library summaries | Source-level measurements, full lifecycle/target observations, detailed coverage and supporting findings |
| doctor | Diagnosis coverage, severity counts, indispensable inability-to-diagnose reasons | Selected error/warning finding identities, subjects, messages and direct actions; default limit 20 | Information findings by default; all domain facts, resolution counts, evidence, candidates, proposals and provenance; no default finding cap |
| install | Outcome, every created/updated path and publication, unresolved effects/recovery | Source version/authority, relevant generated-region changes, verification summary | Full plan/comparison and verification evidence, exact protection/recovery observations |
| update | Outcome, replaced/restored/deleted/retained paths, publication and recovery | Current-to-intended relations, mode and source version, why retired paths remain | Supporting comparison/fingerprint facts, complete plan/verification and recovery evidence |
| index | Changed Entries hosts once, no-op when proved, unresolved host paths | Which entries changed and why a host was skipped or blocked | Complete per-host projection/comparison and verification evidence |
| repair | Exact selected occurrence/replacement, effected files, unresolved choices and receipt | Applicable rule/choice explanation, admissibility and verification summary | Selected proposal evidence, candidate basis, precise input/output spans, full verification/recovery facts |
| cleanup | Every deleted/retained/failed bundle or draft path and final state | Why each artifact was eligible or retained | Full catalogue admission/integrity facts, lease/removal verification, uncertain observations |
| context | Selection identity and coverage, every requested source/layer and exact selected authored content | Why those sources/layers were included and applicable scope relationships | Complete selection provenance, inclusion reasoning and coordinate support; never add unrequested source body |
| find | Every matching source identity/path and any explicitly requested content projections | Descriptions, tags, headings and matching explanation relevant to the query | Complete matching evidence and provenance; identical match membership and authored content |
| references | Every selected occurrence, source location, written/resolved target and relevant state | Direction, target-resolution explanation and actionable findings | Full byte/layer coordinates, destination locations and provenance; no occurrence deduplication |
| route list | Every row in the requested depth, preserving ID/path and hierarchy | Kind, description, tags, parent/depth and observed direct-child facts | Route/source provenance and complete supporting row observations; no depth expansion |
| route inspect | Selected identity, route chain, effective layer identities and important ambiguity | Effective metadata, scope/overwrite explanation and applicable navigation relationships | Complete metadata/layer provenance and route explanations; no unsolicited authored body |
| route init | Every created entrypoint and updated Entries/ownership path; NeedsAuthoring condition | Effective metadata/scaffold mode and verification summary | Complete chain/ancestor plan and validation/comparison/recovery evidence |
| route create | Created subject and every changed parent/publication path | Effective metadata, Template identity and verification summary | Actual intended body/diff when represented, complete Template/plan/verification/recovery evidence |
| route update | Subject, changed field names, all other affected paths, preserved-body condition | Before/after metadata values and explanation of eligible/protected Template body | Complete planned field/body comparisons, provenance, verification and recovery support |
| route move | Exact old/new identity, all changed link/navigation/publication paths and unresolved effects | Each rewritten destination and explanation of applicability | Complete occurrence coordinates, planned edits, identity proof and verification/recovery evidence |
| route remove | Removed subject, link/navigation effects, preserved authored labels and recovery | Exact affected occurrences and detached destination explanation | Complete occurrence/effect plan, guard/verification and recovery observations |
| extension list | Every selected Installed/Available ID and descriptive version, independently honest availability | Package descriptions, source identities and abnormal observations | Full ownership/catalogue/dependency coverage and supporting findings; no new source scan |
| extension inspect | Package identity/versions, meaningful path differences and unavailable comparison | Source, dependencies, current/intended/retired relations and owners | Supporting comparison fingerprints, package/source/ownership evidence and provenance |
| extension create | Catalogue/package identity and every created scaffold path | Effective package metadata/dependencies, authoring reminder and validation summary | Full schema/content validation and operation evidence actually present in the result |
| extension install | Selected roots/dependencies, every changed physical path, grants/publication and recovery | Source/version, why dependencies were selected, comparison and verification summary | Full dependency/permission/comparison plan, file verification and recovery evidence |
| extension update | Selected packages, changed/retained paths, grants/publication and recovery | Versions, dependency closure and current/intended/retired relations | Full comparison/fingerprint, permission, verification and protection evidence |
| extension remove | Removed packages, deleted/shared-kept paths, ownership publication, orphan condition and recovery | Why shared owners/dependencies require retained files or packages | Complete owner/dependency relationships, effect plan and verification/recovery evidence |
| library list | Every registered Library ID and mapping, abnormal links or unavailable registration knowledge | Registered-link states and ownership-record/source boundary explanation | Full record and link observations/provenance; source inventory remains not-requested |
| library inspect | Mapping, changed/new/retired/unsafe source-link relations and inventory coverage | Every source/destination relation and link-state explanation | Full source inventory and exact relative-target/link identity evidence; no invented fingerprints |
| library attach | Library mapping, every created link, grants/ownership publication and unresolved recovery | Exact relative link targets, inventory and verification summary | Complete admission/permission/inventory facts, link verification and recovery evidence |
| library sync | Added/removed/retained link paths, record publication, unchanged source boundary and recovery | Source/destination relations and reasons for differences | Complete source inventory, link/ownership comparisons, verification and recovery evidence |
| library detach | Every removed link, record publication, source left in place and residuals | Exact registered mappings and why each link is removable | Complete no-follow identity/admission proof, verification and recovery evidence |

A mutation's brief receipt is complete about user-relevant effects, not about every internal planning field. It must retain physical paths, effect kind, verified/failed/unknown/not-started distinctions when relevant, exact move/link identities, ownership/settings publication, retained subjects, and recovery location/disposition. Normal/full add explanation and support. Both brief text and brief JSON use this same minimum. Do not justify a huge default JSON graph by calling all diagnostic support an indispensable receipt.

The selection matrix does not promise that full can recover facts discarded after an earlier process exited. These are choices for the original invocation. Rerun read-only inspection for more detail when allowed; do not rerun a mutation to reconstruct its first receipt.

### Schema and parity checks

- The selected finding/effect/source identities and order must agree between text and JSON for each detail level and option combination. Compare semantic selections before snapshotting syntax.
- Both Doctor brief renderers must avoid materializing finding/candidate/evidence output, even for a huge existing typed diagnosis. This proves bounded presentation work, not bounded cost of gathering the underlying diagnosis.
- Brief JSON must omit unselected graphs rather than replace them with empty arrays, nulls, or prose saying they are hidden.
- Normal/full JSON selection metadata distinguishes matching, shown, omittedBySeverity and omittedByLimit. Use limit=null to represent all. When matching totals are unavailable, preserve that state instead of inventing a total.
- Full detail may still select fewer findings when an explicit severity/limit was supplied; that limitation is visible in both formats. Unfiltered full preserves every available diagnosis fact, without duplicate rendered copies of shared evidence.
- Preserve source-location meaning: 1-based Unicode-scalar line/column and, when selected, 0-based UTF-8 physical-layer byte offsets/lengths. Never substitute UTF-16 indices, terminal columns or normalized-newline offsets.
- The new schema's mapping, nullable selected fields, full-detail additions, and diagnostic-record encoding must be reviewed explicitly. Do not relabel schema-1/2 documents as schema 3 without implementing the new membership.
- JSON diagnostics at full detail are separately framed records on stderr, never prose interleaved into the stdout result. Define their exact bounded record schema in the shared contract. Brief/normal do not enable the old verbose diagnostics.

## Help, version, groups, and interaction

### Terminal output

Keep help derived from the composed command tree. Use the public executable name in examples. The inspected old development help shows “OpenForge.Cli” in Usage; a future matching-host check should establish whether this is still present before changing it.

Proposed root help excerpt:

    Open Forge helps you read context and maintain its supporting files.

    Usage:
      open-forge <command> [options]

    Read:
      status       Summarize workspace state.
      context      Read startup or selected context.
      find         Find sources by tags or headings.
      references   Inspect direct authored links.
      doctor       Diagnose problems without changing files.

    Maintain:
      install      Establish the Framework.
      update       Update managed Framework content.
      index        Rebuild generated Entries.
      repair       Apply selected local repairs.
      cleanup      Remove recognized recovery artifacts.

    Groups:
      route        Inspect and maintain routed sources.
      extension    Inspect and manage Extension packages.
      library      Inspect and manage Library projections.

    Output:
      --detail brief|normal|full   How much to show. Default: brief.
      --projection text|json       Output format. Default: text.

This excerpt does not replace parser-generated operand/option detail. Retain --workspace, --help, --version, command-local options, examples, and errors in complete help. The proposed --detail and --projection values are case-insensitive on input, canonical lowercase on output; a missing/unknown value or repeated value option is invalid. Support the parser's existing space and equals value forms. They change presentation only and do not create new operation modes. Grouping the help list is a proposed presentation change; do not maintain a second hard-coded catalogue or reorder operation semantics.

The bare route group lists inspect, list, init, create, update, move, remove. Extension lists list, inspect, create, install, update, remove. Library lists list, inspect, attach, sync, detach. Bare groups show help, not a wizard or an operation result.

Version emits the canonical distributed version and one terminating newline. It needs no banner, workspace inspection, update check, or JSON wrapper. A valid --help/--version terminal mode does not run Doctor or any other domain operation.

### Wizard findings and proposed wording

Current source already has:

- Repair proposal selection and a final plan/confirmation prompt.
- Library residual choices inside Repair.
- Extension Create prompts for missing stable ID and catalogue path.
- Extension Install and Remove selection paths.
- Route Inspect identity selection when interactive resolution is allowed.
- Framework Install/Update confirmation and shared destination-permission prompts.

That source inventory does not prove every live journey works in this environment. The existing executables are stale, and no interactive mutation was attempted.

The remaining issue is the quality and ordering of these interactions. RepairWizard.ConfirmAsync currently shows expected/intended SHA-256 and repeated verification/recovery prose. UpdateOperation.ConfirmAsync asks a fixed confirmation question after planning without itself showing the plan. Verify its enclosing host path before asserting a current live plan-before-confirmation failure. Improve these sites through one approved interaction contract rather than claiming a wizard subsystem must be invented from scratch.

Proposed Extension selection:

    > open-forge extension install
    Available Extensions:
      team-guidance  Team review guidance.
      review-tools   Review templates.

    Enter one Extension ID or all:
    team-guidance

    Preview: install team-guidance.
      Create .agents/guidance/team.md
      Update Entries in .agents/guidance/_guidance.md
      Record ownership in .agents/open-forge.lock.json

    Apply these changes? yes/no (default: no):

The complete plan and required grants must be known before the final application question. This proposal does not silently add multi-selection syntax beyond what the actual selection resolver accepts.

Proposed guided Repair:

    .agents/guidance/team.md:12:8 links to a missing file: review.md

    Possible targets:
      1. .agents/guidance/review-guide.md
      2. .agents/guidance/review-process.md

    Choose a number, skip, back, or cancel (default: skip):

Keep “possible” and “none selected” honest. Recommended evidence is not user selection. After choices, show the complete relevant plan and ask once. A fully explicit authorized request uses existing noninteractive rules; presentation must not add redundant permission.

Interaction and cancellation have command-specific current differences. For example, current Extension Create can classify end-of-input while collecting required values as invalid. Do not silently normalize every input ending to interrupted as a wording change. Record any accepted change to those event semantics separately.

No prompts when --projection json is selected, in redirected/non-prompt-capable invocation, or in existing automatic modes. Missing input/selection must produce the current invalid or blocked result promptly. Do not hang waiting for input. Detail must never suppress a required permission boundary or implicitly consent. Do not add a global --automatic, wizard, --yes, or force alias.

## G4 checklist, handoff, and verification

### Fields that every command must settle

| G4 item | Decision in this report |
| --- | --- |
| User question | Stated in each of the 28 command sections |
| Default tier | Brief for every domain command and renderer; normal/full add information |
| Selected fields | One selection stage; summary/data/change-report rules and all 28 rows of the shared selection matrix |
| Healthy/zero/not-applicable suppression | Exclude irrelevant sections before rendering; retain explicit unknown-versus-empty distinctions for selected facts |
| Ordering | Shared selected order for both formats; Doctor severity-first detail is an explicit presentation change |
| Subject | Exact source path/location, package ID, Library ID, or workspace boundary |
| Next | At most one useful action; preserve subject/source/workspace and avoid unsafe or irrecoverable replays |
| Context echo | Explicit workspace or uncertain/failed boundary; normal/full text adds useful source/target context; JSON retains workspace metadata |
| Size budget | Named fixture budgets with content/identity/receipt exceptions and Doctor detail limits |
| JSON projection | New schema 3 for all levels; brief is actually small, normal is selected detail, full supplies support; parser exception retained |
| Exit/stream meaning | Seven current results and terminal/shell exceptions retained |
| Escaping/line endings | One human scalar rule, serializer-owned JSON, host structure normalization, exact authored content |
| Acceptance | The user has clarified the shared detail model and lightweight default; exact flags/schemas/transcripts still need the later explicit freeze |

### Decisions to approve before freeze

| ID | Recommendation | What remains to approve |
| --- | --- | --- |
| D1 | --detail brief/normal/full; default brief; full absorbs --verbose | Exact grammar and per-command membership, following the clarified direction |
| D2 | --projection text/json; same detail selection in both; one new schema 3 | Exact schema mappings, diagnostic-record format, and any evidence-based compatibility migration |
| D3 | One-line Doctor brief, small brief JSON; normal error/warning rows capped at 20; full includes information and support | Proposed severity/limit options applying identically to both formats and exact normal/full budgets |
| D4 | Keep exact command-specific status, exit, safety, ownership, and recovery behavior | Any separately identified behavior change; none is implied by wording |
| D5 | Natural brief Find rows and a sentence summary; format is independent of detail | Retirement of current implicit TSV behavior, or a separately specified explicit TSV projection if required |
| D6 | Useful Next actions with original subject/workspace/format; no routine Doctor brief hint | Exact computed next values for limited detail and actionable conditions, and the accepted quoting policy |
| D7 | Human control-character escaping and serializer-owned JSON | Exact M3 before/after values, diagnostic truncation unit/marker, and line-ending fixtures |
| D8 | Keep current finding categories and counts during G4 | Separate finding-model/category changes, including candidate-warning classification |
| D9 | Keep essential original mutation receipts at every level; move supporting evidence to full; no replay to recover an earlier receipt | Exact selected effect/recovery fields shared by brief text and JSON, without retaining the whole diagnostic graph |
| D10 | Improve existing wizard text and plan-before-confirmation presentation | Which changes fit G4 versus the later interaction phase; event semantics remain separate |
| D11 | Preserve the shell/parser JSON exception for now | A separately scoped shell-error JSON normalization, if desired |

Your instruction establishes the shared detail model, its effect on every renderer, and the lightweight default. This revision applies that direction now. The table records remaining concrete interface/schema details for a later freeze; it is not a request to confirm the direction again, and silence does not approve implementation.

### Proposed implementation slices after approval

1. Establish the exact current source baseline and a matching executable without changing behavior. Preserve and review existing output before the first output change. G4 requires a separate before-snapshot commit before later behavior changes; this report performs no such work.
2. Freeze the two presentation axes, stream/status rules, three detail selections, new schema mapping, Doctor filters/limits, and M3 escaping expectations. Keep the operation/finding boundary explicit.
3. Implement one selection boundary before either renderer, with Doctor brief text and brief JSON as the first representative slice. Implement normal/full from the same selection policy. Do not deliver a text-only volume fix as completion of this slice. Execute the operation once. Measure bytes and runtime allocation separately.
4. Apply summary and data layouts to Status, Context, Find, References, and Route inspection/listing. Preserve exact authored content, occurrence identity, selection, depth, and coverage.
5. Apply change-report layouts to Index, Install, Update, Repair, Cleanup, and Route mutations. Preserve every actual effect, permission change, ownership publication, and recovery outcome.
6. Apply the accepted patterns to Extension and Library commands, keeping each command's selection, dependency, collision, and no-op policy local.
7. Complete any approved prompt/confirmation presentation changes. Use current wizard/selection paths; do not absorb the whole later interaction/taxonomy program into G4.
8. Update current shared/command contracts, help, and affected public documentation alongside the corresponding behavior. Run the required focused and shared-boundary qualification and review the integrated output packet.

Expected production surfaces include Shell/Presentation and its new shared selection boundary, both text and JSON command-local rendering/projection scopes, global flag binding, root composition/help, and explicitly approved interaction sites. Replace the view discriminator with detail, unify diagnostic selection under full, and remove old schema selection by view. M3 should remove duplicate escaping only after the expected output changes are frozen. No new generic command engine, state store, plugin, dependency, or project split is justified by this report.

Expected contract surfaces include shared/global-flags, shared/result-coordinates where affected, all 28 command Interface/Behavior pairs as needed, and docs/cli.md. Record accepted execution state in the existing Task 30 G4 and Task 31 M3 records after the user opens that implementation work. Historical analysis stays intact.

### Required evidence for the later implementation

- Use source-owned small fixtures and actual current typed results. Exercise all reachable normal, attention, incomplete, invalid, blocked, failed, and interrupted cases without inventing unreachable attention states.
- Separate assertions for semantic result, exit, stdout, stderr, prompt behavior, effects, and recovery from exact output snapshots. A snapshot alone cannot prove no writes or at-most-once execution.
- Test all three detail levels against both renderers for the same typed result. Detail may add selected facts; changing only projection must preserve selected identities, order, authored content, counts, coverage, limitations and effect facts. Verify the default equals explicit brief for both formats.
- Test Doctor brief against a large synthetic finding model with 258 MB or more of representable detail if practical; measure actual fixture size rather than assuming it. Distinguish many distinct occurrences from repeated supporting candidate sets. Prove neither brief renderer constructs the full finding/candidate/evidence output, and assert both text and JSON byte budgets.
- Verify normal defaults to errors/warnings and 20 findings, while full defaults to every severity with no finding cap. Verify severity union, positive/all limit grammar, invalid brief/filter combinations, zero matching findings, explicit omitted counts, identical ordering across formats, and unchanged operation status. Severity/limit with JSON must work; do not retain the old text-only rejection.
- Preserve essential original mutation receipts in all six detail/format combinations. Test failure after one verified effect, failed verification, not-started effects, unknown final state, retained/unknown bundle disposition, and ownership/settings publication separately. Assert that supporting evidence does not leak into brief JSON.
- Test Context and Find content with LF, CRLF, CR, mixed line endings, empty sections, comments, quotes, tabs, backslashes, Unicode, and control characters. Authored text round-trips exactly in the supported JSON text contract.
- Test human scalar escaping and JSON decoding at the actual serializer/render boundary. Never normalize away encoding defects, finding counts, ordering, or diagnostic content.
- Verify terminal help/version/group behavior, typed invalid input versus pre-binding parser failure, redirected streams, NO_COLOR, actual supported colour capability, and real process cancellation where needed.
- Check a representative full in-process scenario at the accepted AOT-safe snapshot boundary. Use process/AOT evidence for the boundaries it alone can prove, without moving the test architecture as an incidental output cleanup.
- Validate every approved command example against the matching composed CLI. For a mutation, use isolated owned fixtures and preview/application evidence as appropriate.

Doctor was prohibited for this report. Any continuing prohibition must remain in force: baseline its renderer from complete synthetic typed-result fixtures without invoking diagnosis, and clearly record that no live Doctor end-to-end/large-repository measurement was obtained. A fresh real-workspace Doctor run needs an explicit relaxation of that constraint; this report does not authorize one.

### Material deviations from the pasted handover

| Planned in the handover | Found during this task | Treatment and reason |
| --- | --- | --- |
| Maintain the active G4/task records | This test permits report-only writing | Saved proposals here; left active records and contracts unchanged |
| Capture fresh representative output as current | Both prepared executables are stale | Kept limited captures as negative provenance evidence, used source/contracts for the design, and did not rebuild |
| Assume no wizard exists | Current source includes several concrete interactive paths | Reframed the issue as quality/coverage verification of existing journeys |
| End with an approved specification | User clarified the design direction, but no exact specification freeze was supplied | Delivered the corrected proposal and conditional implementation handoff, preserving the distinction between direction and a frozen specification |
| Start a sequential interactive decision session | User requested all analysis/examples in one saved report | Supplied the representative first decision and the full catalogue without a questionnaire |
| Historical proposals supply examples | Several examples are obsolete or change behavior | Preserved history and replaced them only in this report with current-boundary proposals |
| First draft preserved current views and full default JSON | User clarified the earlier shared-detail direction; relevant analyses support it | Replaced the recommendation with brief/normal/full across renderers, small default JSON, and full-level diagnostics; revised examples, schema, tests, and prompt together |

## Naming alternatives

The user's follow-up asks for a shorter format option and a detail scale resembling minimal, standard, verbose/debug, with room for intermediate levels. This is a naming and selection-design discussion. No spelling has been accepted and no CLI implementation changes are implied.

### A shorter format option

| Candidate | Example | Assessment |
| --- | --- | --- |
| --format | open-forge doctor --format json | Recommended: directly names the choice between text and JSON |
| --as | open-forge doctor --as json | Shortest readable alternative; the command reads like a sentence |
| --output | open-forge doctor --output json | Understandable, but may be read as an output destination rather than a format |
| --out | open-forge doctor --out json | Short, with the same destination ambiguity |

Recommend --format, optionally with -f as its short spelling. A source search found no literal -f/--format/--as option declaration in the inspected C# tree; a later composed-parser check must still confirm that the chosen alias is available. --as is a reasonable alternative if shorter typing matters more than naming the format explicitly. The choice affects spelling only: every format still receives the same detail selection.

### Detail-name families

| Family | Smallest to largest | Tradeoff |
| --- | --- | --- |
| Familiar amount labels | minimal -> standard -> verbose | Closest to the user's preference; standard must not be mistaken for the configured default |
| Describe the returned information | summary -> details -> all | Clear for diagnosis; summary can misdescribe commands whose actual answer is a list or authored content |
| Describe granularity | coarse -> medium -> fine -> full | Expresses the scale, but gives less guidance about which facts each level contains |

Recommend starting the naming discussion with minimal -> standard -> verbose, defaulting to minimal. These levels mean the smallest useful answer, actionable explanation, and complete result detail respectively. Retain --detail as the option name because it says what is being increased.

The full potential ladder can be:

    minimal -> summary -> standard -> detailed -> verbose -> debug

This illustrates extension points, not a requirement to expose six levels immediately. Each added level must earn its place by selecting a useful intermediate amount of information. For Doctor, a possible distinction is:

| Level | Selected information | Place in the proposed initial surface |
| --- | --- | --- |
| minimal | Outcome and aggregate counts, plus indispensable coverage limitations | Initial default |
| summary | The same outcome with a short breakdown by category/domain | Possible intermediate level |
| standard | Actionable findings and direct next actions, with the existing proposed bounded selection | Initial ordinary detail |
| detailed | More explanation and evidence for those selected actionable findings | Possible intermediate level |
| verbose | All result findings and their available evidence, including informational findings | Initial complete result detail |
| debug | Verbose result detail plus execution diagnostics such as existing decision/phase observations | Optional highest diagnostic level |

Default is a setting, not a level name. Calling the middle value default would contradict the requested minimal default; calling it standard is workable if help makes minimal's default status explicit. Debug describes execution diagnostics, while verbose describes how much result information is shown. Keep both under the same --detail option if both are offered; this does not revive independent --verbose/--debug controls or renderer-specific selection.

The verbose/debug distinction is a proposed refinement of the earlier three-level packet: its full tier included both complete result evidence and execution diagnostics. Under this alternative, that old full behavior corresponds to debug, while verbose is a new useful point below it. Do not silently remove diagnostic facts from a supposedly equivalent rename. If only three levels are wanted and the highest must include diagnostics immediately, minimal -> standard -> debug is another coherent set, although its top name emphasizes troubleshooting rather than completeness.

New named levels can be inserted without renumbering existing public levels. Give each a stable semantic definition and explicit order; do not derive ordering alphabetically. Adding a level must not silently change existing selections, the minimal default, status/exits, safety boundaries, or format parity. Update the accepted enum/schema/help and coverage when a level is introduced. Names are preferable here to an exposed 1/2/3 scale whose existing values might later need renumbering. An all endpoint can also have levels inserted below it; it is not inherently unextensible, but its completeness promise needs a precise scope.

Examples under the latest naming candidate, all illustrative and not executed:

    open-forge doctor
    open-forge doctor --detail standard
    open-forge doctor --detail verbose
    open-forge doctor --detail standard --format json

These examples preserve the central requirement: changing only --format must not change the selected facts. Once the names and the diagnostic-level distinction are chosen, apply them consistently to all 28 command examples, shared help, JSON detail values, the handoff, and the reusable prompt. Until then, keep this alternative clearly identified rather than treating exploratory preferences as a freeze.

## Prompt improvements

The prompt's useful core is clear: short natural output, concrete examples, current behavior as the baseline, explicit approval before implementation. It will produce more reliable results if it resolves these conflicts up front:

1. Choose a mode: report-only proposal or interactive specification. “All commands at once,” “ask one question at a time,” and “deliver an approved specification” cannot all be completed autonomously in one response.
2. Make report-only the explicit override: creating the report is allowed, while editing G4 records/contracts is not. Specify the output filename.
3. Treat 258 MB as user-reported until independently measured. Prohibit Doctor explicitly, including indirect invocation through a tool/test/helper. Reading its source is allowed.
4. Require binary/source provenance. An old local executable is not current evidence merely because it runs from this repository.
5. Require the actual composed command inventory and command-local status coverage, including help/version, bare groups, parser rejection, unknown data, no-ops that are not supported, and partial writes.
6. State the intended replacement model explicitly: one --detail brief/normal/full selection before every renderer, default brief for text and JSON, with diagnostic verbosity folded into full. Current --view contracts are evidence of the starting point, not a constraint to preserve. Require a compatibility ledger for replaced flags, TSV, the new JSON schema, next actions, filters/limits, escaping, and prompts.
7. Preserve requested payloads and essential original receipts in both formats: source content cannot be summarized, and mutations cannot be rerun to obtain omitted evidence. Do not use receipt preservation to justify retaining the entire internal graph in brief JSON.
8. Treat wizard absence, obsolete baselines, generated-link inclusion, and global Doctor/Repair equivalence as claims to check, not accepted conclusions.
9. Define completion honestly: a report can be complete while its proposed product specification remains unapproved.
10. Ask for concise evidence and rationale, not an unbounded narration of the investigation. “All analysis” is better specified as conclusions, evidence, assumptions, alternatives that affect UX, and unresolved decisions.
11. Add a concrete rejection check: fail the proposal if changing text to JSON increases detail, if brief Doctor JSON contains findings/candidates, if normal/full are only formatting changes, or if any proposed command still depends on the retired view model. This directly catches the error in my first draft.

### Copy-ready report-mode prompt

    Work with me on Open Forge Task 30 G4: propose a complete user-facing CLI
    output specification, including Task 31 M3 escaping.

    Mode: report-only design review.
    Create or revise only the Markdown report astra.md in the repository root.
    Do not modify other existing files, task records, contracts, source, tests,
    configuration, or historical analysis. Do not commit, merge, install,
    publish, or change external state.

    Do not run Doctor, directly or indirectly, including through tests or
    helpers. You may read its source, contracts, and existing evidence.
    The reported 258 MB Doctor result is a user observation, not a measurement
    you may claim to have reproduced.

    Read .agents/loader.md and select its relevant scopes. Then read:
    - .agents/memory/working/cli-development/plan.md
    - The parent Task 30 and Task 31 records.
    - .agents/memory/working/cli-development/tasks/task30/phase-4b-g4.md
    - .agents/memory/working/cli-development/tasks/task30/review-second-gate-pre-g4.md
    - .agents/memory/working/cli-development/tasks/task31/phase-escaper.md

    Follow relevant G4 references, especially these analyses:
    - .agents/memory/emerging/analysis/cli-experience-audit/lifecycle-baselines-and-architecture.md
    - .agents/memory/emerging/analysis/cli-experience-audit/structural-requirements-and-markers.md
    - .agents/memory/emerging/analysis/cli-experience-audit/layers-and-sequencing.md

    Current command contracts define existing operation meaning; current source
    establishes implementation reality. Investigate disagreements and report
    them without editing either source. Historical proposals are provenance,
    not proof that the current CLI implements them or a substitute for approval.

    Verify the current command inventory from source. Before using an existing
    executable as evidence, check that it matches the relevant current behavior.
    Do not rebuild if that would violate report-only scope. Label stale or
    unverified captures and do not use them as the current baseline.

    The direction is fixed: discard --view as the output model. Use one detail
    selection that affects every renderer, defaulting to the smallest useful
    level in both text and JSON. Recommend --detail brief|normal|full, with
    diagnostic verbosity absorbed into full. Use --projection text|json to
    select format independently. Identify any naming or TSV alternative as an
    explicit design choice, never as another detail tier.

    Select information before formatting it. Switching projection must not
    change finding/effect/source membership, order, limits or coverage. Doctor
    brief should normally be one sentence with relevant counts. Its brief JSON
    must also be a small summary without findings, candidates or evidence.
    Do not preserve old compact/full JSON membership as the new default.
    Specify a new schema when the existing schemas cannot express this model.

    Preserve exact requested authored content, inventory identities, and
    essential original mutation effects/recovery at every level and in both
    formats. Move supporting evidence into greater detail. Do not advise
    rerunning a mutation to recover its earlier receipt.

    Start with Doctor at brief, normal and full detail, including a genuinely
    small brief JSON example and a text/JSON parity example. Explain the first
    remaining concrete design decision. Then cover all command leaves
    and terminal surfaces in one coherent report. Do not stop for approval
    during this report-only run.

    For each command, record:
    - The user's question and valid example invocation.
    - Proposed brief and normal transcripts and what full detail adds.
    - Success, supported no-op/empty states, actionable findings, incomplete
      coverage, invalid input, blocked operation, failure, and cancellation.
    - Partial effects and recovery where supported.
    - Required identities, paths, ordering, next action, workspace echo,
      suppressed fields, and an appropriate output-size budget.
    - Shared selection at each detail level, JSON membership/schema,
      stdout/stderr, exit codes, and prompt behavior.
    Explicitly mark unsupported states instead of inventing them.

    Separate current behavior, observed evidence, proposed changes, and open
    decisions. Label constructed transcripts; do not present them as captures.
    Identify every new flag or changed default/schema/status/selection rule.

    Do not restore baseline fingerprints, workspace rebind, missing-lock gates,
    the retired legacy-receipt notice, or resolved SKILL diagnostics. Check the
    actual wizard implementation before claiming that one is missing. Preserve
    current authored-reference and repair-scope boundaries.

    Define human escaping, JSON string encoding, line endings, selected-content
    preservation, truncation, and limits as distinct concerns.

    Before finishing, reject and correct any recommendation that keeps the
    old view model, makes JSON more detailed than text at the same level,
    leaves Doctor brief JSON carrying the full graph, or treats minification
    as a reduction in detail. Check all command examples, help, schema, handoff
    and the suggested prompt for the same mistake.

    Finish the report with:
    - A decision table and meaningful alternatives.
    - A conditional implementation handoff with affected surfaces and slices.
    - Verification and before-snapshot requirements.
    - Evidence limitations and prompt improvements.

    The direction above is part of this request. Exact flags, schemas,
    transcripts and implementation are not frozen until I approve them later.
    Do not claim an implementation-ready freeze when decisions or exact
    snapshots remain open, and do not ask me to reconfirm the stated direction.

    Write short natural CLI sentences. State what happened, the affected item,
    and the next useful action. Avoid unexplained internal terms and decorative
    text. Keep the chat reply short and link to the report.

For an interactive version, replace the mode paragraph with: “Discuss one consequential decision at a time. Prepare concrete alternatives before asking. Record only my explicitly approved decisions in the existing G4 record; keep proposals separate.” Remove the instruction to complete the whole report without questions. Do not conflate that later interaction with this completed report-only test.

## Evidence and limitations

### Source basis

The inspected checkout was on feature/development-2, with latest short commit ed5824851 (“Merged the pre-G4 contract corrections”). The initial tracked/untracked status was clean. The report does not claim a new build or qualification of that commit.

Primary design sources:

- [Task 30 G4](.agents/memory/working/cli-development/tasks/task30/phase-4b-g4.md)
- [Task 31 M3](.agents/memory/working/cli-development/tasks/task31/phase-escaper.md)
- [Current plan](.agents/memory/working/cli-development/plan.md)
- [Pre-G4 review dispositions](.agents/memory/working/cli-development/tasks/task30/review-second-gate-pre-g4.md)
- [Global flag contract](.agents/memory/crystallized/documents/cli/contracts/shared/global-flags/interface.md)
- [Result coordinates](.agents/memory/crystallized/documents/cli/contracts/shared/result-coordinates/interface.md)
- [Shared operation contract](.agents/memory/crystallized/documents/cli/shared-operation-contract.md)
- [All command contracts](.agents/memory/crystallized/documents/cli/contracts/_contracts.md)
- [G4 scenario/snapshot boundary](.agents/memory/working/cli-development/tasks/task30/phase-7-scenarios.md)
- [Historical output proposals](.agents/memory/emerging/analysis/cli-experience-audit/command-output-design.md), used as unaccepted provenance.
- [Earlier detail/default proposal](.agents/memory/emerging/analysis/cli-experience-audit/lifecycle-baselines-and-architecture.md), [revised detail/projection note](.agents/memory/emerging/analysis/cli-experience-audit/structural-requirements-and-markers.md), and [selection-stage analysis](.agents/memory/emerging/analysis/cli-experience-audit/layers-and-sequencing.md), rechecked after your correction. These establish the history of the direction without changing today's source/contract facts.

Focused current-source evidence:

- [Doctor human renderer](src/cli/core/OpenForge.Cli.Core/Commands/Doctor/Shared/Rendering/DoctorHumanRenderer.cs) and [finding renderer](src/cli/core/OpenForge.Cli.Core/Commands/Doctor/Shared/Rendering/DoctorFindingHumanRenderer.cs): domain/finding/candidate traversal and optional expanded evidence.
- [Doctor candidate finding formation](src/cli/core/OpenForge.Cli.Core/Commands/Doctor/Shared/Domains/LocalReferenceDoctorCandidateInspector.cs): candidate-basis/cardinality warnings remain separate findings.
- [Doctor result formation](src/cli/core/OpenForge.Cli.Core/Commands/Doctor/Shared/Aggregation/DoctorResultBuilder.cs): current domain aggregation and null envelope-level Next.
- [Current human text utility](src/cli/core/OpenForge.Cli.Core/Shell/Presentation/Shared/Rendering/CliHumanText.cs): existing headers, control-character replacement, and line-ending normalization.
- [Find compact renderer](src/cli/core/OpenForge.Cli.Core/Commands/Find/Shared/Rendering/FindCompactRenderer.cs): current TSV summary/rows.
- [Update policy](src/cli/core/OpenForge.Cli.Core/Commands/Update/Shared/Planning/UpdatePlanningPolicy.cs): ordinary Replace/Restore and explicit prune decisions.
- [Repair wizard](src/cli/core/OpenForge.Cli.Core/Commands/Repair/Shared/Interaction/RepairWizard.cs), [Extension selection](src/cli/core/OpenForge.Cli.Core/Commands/Extension/Install/Shared/Planning/ExtensionInstallSelectionResolver.cs), and [Extension creation input](src/cli/core/OpenForge.Cli.Core/Commands/Extension/Create/Shared/Resolution/ExtensionCreateRequestResolver.cs): existing interactive paths.
- [Update confirmation](src/cli/core/OpenForge.Cli.Core/Commands/Update/UpdateOperation.cs): current confirmation and noninteractive failure path.
- [Library Attach planner](src/cli/core/OpenForge.Cli.Core/Commands/Library/Attach/Shared/Planning/LibraryAttachPlanner.cs): duplicate ID blocks; missing ownership observation does not itself gate.
- [Library Sync planner](src/cli/core/OpenForge.Cli.Core/Commands/Library/Sync/Shared/Planning/LibrarySyncPlanner.cs) and [Detach planner](src/cli/core/OpenForge.Cli.Core/Commands/Library/Detach/Shared/Planning/LibraryDetachPlanner.cs): informational no-effect outcome when ownership information is unavailable.
- [Shell application](src/cli/core/OpenForge.Cli.Core/Shell/Composition/CliCoreApplication.cs): parser/terminal handling and direct stderr failure path.

The existing [CLI checkpoint](.agents/memory/working/checkpoints/cli-development.md) still describes earlier G1/M1 progress. The active plan and task-owned G4 preparation state are newer and were used for the current phase boundary. This contextual drift was recorded here, not repaired under report-only authority.

### Captured evidence from stale binaries

These are deliberately short captures. The workspace line was omitted from the excerpts to keep them portable. They are not proposed output and are not current-source baseline snapshots.

Development executable:

    artifacts/publish/open-forge-dev/Release/open-forge-dev.exe

Native executable:

    artifacts/publish/win-x64/open-forge/OpenForge.Cli.exe

Both returned the following obsolete Library state for library list --view compact:

    No Libraries are registered.
    Status: complete
    Record: missing (.agents/open-forge.libraries.json)
    Registered Libraries: 0
    Source inventory: not scanned (library list checks registered links only)
    Checks: complete

Both returned this obsolete requirement for route list patterns/open-forge/cli --depth 1 --view compact:

    The Loader must contain exactly one final ## Entries section with one ordered marker pair.

The development executable's --help displayed:

    --view <view>       Result detail: compact or expanded. [default: Expanded]

Its bounded unmatched Find invocation returned:

    result=complete    coverage=complete    universe=filtered    matches=0
    No matches.

The spaces in that last excerpt represent tab separators for readability. It demonstrates the old prepared executable's output, not proof that it is up to date.

Executed domain invocations were limited to those bounded read-only Library List, Route List, and unmatched Find requests. Root help and read-only Git/filesystem/source searches were also used. Neither build/test commands nor mutation/preview commands were run. No Doctor invocation, direct or indirect, was used.

### What remains unproven

- No 258 MB reproduction, fresh Doctor measurement, or measured output/memory reduction.
- No matching-current-binary process baseline; the two prepared executables were disqualified.
- No live interactive wizard or mutation behavior verification.
- No assertion that an illustrative count, package, body, or path occurs in this repository.
- No approved exact schema/flag/status change or frozen snapshot suite. Your clarification does establish the intended shared detail model and lightweight default used by this revised proposal.

Final artifact checks: all 28 unique command sections have matching rows in the detail-selection matrix; all 60 local prose links resolve; all four complete JSON examples parse and use schemaVersion 3. The constructed brief Doctor envelope is 242 UTF-8 bytes before its terminating newline and its result contains only coverage and counts. The normal Doctor example's selected/omitted counts reconcile. No proposed invocation uses --view, --verbose, or --json; remaining mentions identify the replaced interface, historical evidence, or rejection checks. These checks validate this report, not live CLI behavior or a measured reduction from the reported 258 MB. The illustrative Markdown link inside the Route Remove transcript is source text, not report navigation.

An unrelated untracked report appeared during the session at .agents/memory/working/cli-development/tasks/task30/phase-4b-g4-output-proposal.md. This task did not create, edit, read for authority, or remove it. The report-only boundary here applies to this task's actions.

This report completes the requested design test. A later specification freeze still needs explicit decisions and complete reviewed fixtures for the examples marked as excerpts.
