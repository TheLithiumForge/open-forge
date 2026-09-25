---
open-forge:
  description: Unaccepted G4 proposal for the shared output rules and per-command transcripts, with the measured current behavior behind each change
  tags: [Memory, Working, CLI, Task, Subtask, Proposal, Presentation, Contextual]
---

# Task 30 G4 — Output Design Proposal

## Status

**Nothing here is accepted.** This is the design packet the
[G4 subtask](../../../../archived/cli-development/tasks/task30/phase-4b-g4.md) asks for, written so the maintainer can approve or
reject each rule and transcript individually. No production file, contract, or
test was changed while writing it. `doctor` was not run.

Three labels appear throughout and never mix:

| Label        | Meaning                                                                                          |
| ------------ | ------------------------------------------------------------------------------------------------ |
| **Captured** | Verbatim bytes from a committed snapshot under `src/cli/tests/unit/.../__snapshots__/`.          |
| **Derived**  | Constructed by reading the renderer source. Faithful to the code, but not produced by executing it. |
| **Proposed** | New wording. Not current behavior.                                                                |

Freeze means the maintainer says so explicitly. A recommendation here is not
acceptance, and silence is not acceptance.

## What was read

Loader and its `#LoadNow` Directives; the
[global flags](../../../../crystallized/documents/cli/contracts/shared/global-flags/interface.md)
and [result coordinates](../../../../crystallized/documents/cli/contracts/shared/result-coordinates/interface.md)
contracts; the [Doctor interface contract](../../../../crystallized/documents/cli/contracts/doctor/interface.md);
the [G4 subtask](../../../../archived/cli-development/tasks/task30/phase-4b-g4.md), [second-gate review](../../../../archived/cli-development/tasks/task30/review-second-gate-pre-g4.md)
and [Task 31 M3](../../../../archived/cli-development/tasks/task31/phase-escaper.md); the shell presentation layer; every
human renderer named below; the eight escapers; and the committed output
snapshots.

Three Emerging records already settled the shape of the presentation surface, and
this proposal restates rather than reinvents it. Their obsolete premises listed in
[G4 preparation](../../../../archived/cli-development/tasks/task30/phase-4b-g4.md#preparation-reconciliation) are not reused.

| Source                                                                                                                                   | What it already established                                                                                                       |
| ---------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------- |
| [lifecycle-baselines-and-architecture.md](../../../../emerging/analysis/cli-experience-audit/lifecycle-baselines-and-architecture.md)     | `--projection` is an option, not a mode; `--view` is granularity and should be `--detail`; default to the smallest useful level; the missing view-selection layer. |
| [structural-requirements-and-markers.md](../../../../emerging/analysis/cli-experience-audit/structural-requirements-and-markers.md)       | `--detail` absorbs `--verbose`; default `--detail brief`; two presentation flags where there are three.                            |
| [contract-versus-code.md](../../../../emerging/analysis/cli-design-retrospective/contract-versus-code.md)                                 | Records the above as the accepted narrowing; retire the 20 interface contracts as rendering specifications and write one presentation contract. |
| [command-output-design.md](../../../../emerging/analysis/cli-experience-audit/command-output-design.md)                                   | The per-command proposals and the six rules that generate them. Its own three-tier naming is superseded by the narrowing above.    |

S1, S1a, S14 and S15 are those conclusions carried into G4 as decisions. The rest
of Part 2 and all of Part 4 are new work built on top of them.

---

## Part 1 — What is actually wrong

### 1.1 The default view is the largest view

`CliSyntaxDefinitions.View` sets `DefaultValue = CliView.Expanded`, and the
global-flags contract states "`expanded` is the default. It includes the
explanations, evidence, provenance, locations, and next actions that make the
result understandable without another call."

Every invocation therefore pays for the evidence-bearing view. The token-cheap
renderers exist and are unreachable without typing a flag. This one default is
the largest single cause of the volume problem.

### 1.2 Four unbounded renderings

Each of these grows with workspace size and is in the default view.

**Doctor emits one informational finding per valid link.**
`LocalReferenceDoctorDescriptorReader.Read` maps
`SourceLinkTargetResolution.Complete` to an `information` finding of kind
`reference.target-valid`. `LocalReferenceDoctorInspector.Inspect` calls it for
every observed reference.

`DoctorFindingHumanRenderer.Append` groups by `finding.Subject`. A source
occurrence carries its own location, so each valid link is its own group and
renders, in the default expanded view, as a subject line, a destination line, a
severity line, a resolution line, and a provenance line.

Measured in this repository:

```
$ find .agents -name "*.md" | wc -l
762
$ grep -roh "\]([^)h][^)]*)" --include=*.md .agents | wc -l
6772
```

That is roughly 6772 finding groups of about five lines each from the link
domain alone, before the route, workspace, Framework, and Extension domains add
theirs, and before candidate sets are expanded for any missing link. Candidate
sets are the second multiplier: a missing link admits candidates found by
filename, title, literal content, and route neighborhood, and expanded view
prints each candidate's subject, every basis match, and its provenance.

The 258 MB figure reported by the maintainer was not reproduced in this session,
because the instruction was not to run `doctor`. The mechanism above is
sufficient to produce output of that order and should be measured once, by the
implementer, as the reviewed before snapshot.

**`references --view=expanded` prints one `Inspected:` line per inspected
layer.** `ReferencesHumanRenderer.AppendSelection` iterates
`selection.InspectedSources` with no cap. An incoming scan over this workspace
inspects the whole source universe.

**`status --view=expanded` prints one line per generated navigation target even
when the target is current.** `StatusHumanRenderer.AppendStructure`:

```csharp
if (view == CliView.Expanded || target.State != OperationalGeneratedNavigationState.Current)
{
    builder.AppendLine($"  {Text(target.Path)}: {NavigationState(target.State)}");
}
```

The second-gate review measured 120 regions in this workspace, so `status`
prints 120 lines saying nothing is wrong, immediately after printing the grouped
counts that already said so.

**`find --view=expanded` echoes the query back on every search.**
`FindExpandedRenderer.Render` appends a `Search details:` block with filters,
`Require:`, `Within:`, source universe mode, every include and exclude selector
with its form, resolution, identity, source kind, expansion and candidates, and
the inspected-of-candidate counts. A caller who just typed the query pays for it
again in the answer.

### 1.3 Framing costs more than the answer on small results

Every workspace-aware command opens with four lines from
`CliHumanText.AppendHeader`:

```text
<outcome sentence>
Status: <status>
Workspace: <path>
Selected by: <current directory | --workspace>
```

The outcome sentence already carries the status in words, and the process exit
code carries it a third time. On a successful `index` run with nothing to do,
three of the eight printed lines are this header.

### 1.4 Facts that are fine are printed anyway

**Captured** — `LibrarySyncOutputSnapshotTests/ExpandedHuman`:

```text
Record before change: not checked (.agents/open-forge.lock.json)
Source: not checked; scan not scanned
Source location: unavailable
Resolved source: unavailable
Inside workspace: path unavailable; resolved path unavailable
```

Five consecutive lines whose entire content is "there is nothing here". The same
pattern appears as `residual: none`, `Verification: not-requested`,
`Recovery: not-required`, `Findings: 0`, and `Resolution: 0 exact repairs, 0
choices, 0 targeted commands`.

**Captured** — `DoctorLifecycleOutputSnapshotTests/MissingStateExpandedHuman`
spends six lines on counts before any finding, and repeats the same six-line
shape inside each domain. With six domains that is 21 lines of counts on a
workspace where everything is fine.

### 1.5 Internal vocabulary reaches the user

**Captured** — `UpdateOutputSnapshotTests/ExpandedHuman`:

```text
Lifecycle: trust=trusted / coverage=complete / action=publish / outcome=planned
    Comparison: managed-region / region=entries-  -end; current: changed; intended: changed; retirement: not-applicable
```

**Captured** — `ExtensionListOutputSnapshotTests/ExpandedHuman`:

```text
COMPLETE: No Extension ownership is recorded; installed packages cannot be established. [extension-list.ownership-observation]
```

A finding prefixed `COMPLETE` is the result's semantic status printed where a
severity word belongs. `style.Finding(...)` upper-cases the semantic status and
uses it as the finding label, so an informational observation on a successful
run announces itself as `COMPLETE:`.

### 1.6 Human output is JSON-escaped

**Captured** — `UpdateOutputSnapshotTests/ExpandedHuman`, rendering a version
string containing a tab, a quote, a backslash, an emoji, and a lone surrogate:

```text
  ID: framework; version: v1\u0009\"x\"\\😀\ud800  -end; inventory fingerprint: aaaa...
```

That is JSON string syntax printed to a terminal. The user sees `\"` where the
file contains `"`.

This is [Task 31 M3](../../../../archived/cli-development/tasks/task31/phase-escaper.md). Eight escapers exist and give
five different answers:

| Owner                                                                      | Behavior                                                          |
| -------------------------------------------------------------------------- | ------------------------------------------------------------------ |
| `CommandTextEscaping`, `ContextTextEscaping`, `FindTextEscaping`, `RouteListTextEscaping`, `RouteInspectTextEscaping` | `\\`, `\"`, `\uXXXX` for controls and lone surrogates             |
| `RouteTextEscaping`                                                        | `JsonEncodedText.Encode`, which also escapes `<`, `>`, `&`, `'`, `+` |
| `ExtensionTextEscaping`                                                    | adds `\n`, `\r`, `\b`, `\f`, and spells tab `\u0009`              |
| `ReferencesTextEscaping`                                                   | same, but spells tab `\t`                                         |
| `CliHumanText.Text`                                                        | replaces control characters with U+FFFD and escapes nothing        |

`CliHumanText.Text` is the only one that is right for a terminal, and it is used
only by the shared header, Doctor, and Status. Truncation disagrees as well:
`RouteListTextEscaping.Clamp` counts raw characters against a limit measured on
escaped output, `ExtensionTextEscaping.Clamp` can split a surrogate pair, and
`RouteInspectTextEscaping` defaults to an 8192-character limit that no other
owner has. `DiagnosticValueLimit = 240` is declared independently in six files.

### 1.7 The wizard exists but is hard to reach and unbounded

Interactive selection is implemented: `RepairWizard`,
`RepairLibraryRecoveryWizard`, `RouteInspectInteractiveSourceSelector`, and the
Extension Create wizard. `CliCompositionRoot.CreateInteractiveSession` enables
prompting only when neither standard input nor prompt output is redirected, and
`RepairRequest.ReadSelectionMode` selects `InteractiveWizard` when `--automatic`
is absent, no `--relink` is supplied, and interaction is allowed. So bare
`open-forge repair` in a real terminal does enter the wizard.

Two problems remain, and they match a report that "there is no wizard":

- Root help's `Getting started` lists `install --dry-run`, `context`,
  `route list` and `doctor`. It never names `repair`, so nothing tells a new
  user the interactive path exists.
- `RepairWizard.SelectAsync` asks one question per proposal, starting at the
  first. On a workspace with hundreds of guided findings, the wizard is a
  hundreds-question interrogation with no summary, no "apply all safe repairs",
  and no way to filter.

**Open question O-1** records this so the maintainer can say which of the two
they meant.

---

## Part 2 — Proposed shared rules

Each rule has an identifier so a review can accept or reject it individually.

### The three content classes

Most of the argument disappears once output is split into three classes with
three different rules. Every line a command prints belongs to exactly one.

| Class       | Definition                                                                      | Rule                                                     |
| ----------- | -------------------------------------------------------------------------------- | ---------------------------------------------------------- |
| **Payload** | The thing the caller asked for: matches, routes, content, entries, effects.      | Never abbreviated by view. A list command prints its list. |
| **Finding** | Something the command noticed that the caller did not ask about.                 | Filtered by severity in the default view.                  |
| **Framing** | Everything else: workspace echo, counts, coverage, selection echo, provenance.   | Shown only when surprising, or when a larger view is asked for. |

This is why the default view is not simply "less output". `find` in the default
view prints every match; `doctor` in the default view prints no informational
finding at all. Both follow the same rule.

### S1 — Retire `--view`. Two axes: detail and projection

This rule restates the narrowing already recorded in
[lifecycle-baselines-and-architecture.md](../../../../emerging/analysis/cli-experience-audit/lifecycle-baselines-and-architecture.md)
and confirmed in
[contract-versus-code.md](../../../../emerging/analysis/cli-design-retrospective/contract-versus-code.md).
It is repeated here because G4 is where it becomes a decision rather than a
proposal, not because it is new.

`--view` is retired. `compact` and `expanded` describe ink, not information, and
`--view` already means two different things: format in `find`, density
everywhere else. Three presentation flags become two, each meaning exactly one
thing in every command.

**`--detail <brief|normal|full>`, default `brief`.**

| Level              | For                          | Contains                                                                                         |
| ------------------ | ---------------------------- | -------------------------------------------------------------------------------------------------- |
| `brief` (default)  | the answer, and nothing else | Complete payload. Findings as counts. No framing at all.                                          |
| `normal`           | acting on what was found     | Adds each error and warning with its subject, explanation and machine code, and one `Next`.       |
| `full`             | proving what the CLI did     | Adds informational findings, evidence, provenance, candidate basis, echoed selection, comparison detail, and today's `--verbose` diagnostics. |

**`--detail` absorbs `--verbose`.** There is no separate verbosity flag. Verbose
is simply `full`, and the diagnostics that go to stderr under `--verbose` today
become part of that level. This deletes a flag rather than adding one.

**`--projection <text|json|tsv>`, default `text`.**

`--json` is retired as a flag and becomes `--projection json`. `tsv` gives Find's
existing row shape a name and makes it available wherever rows are the answer,
instead of hiding it inside one command's `compact` view.

**Detail applies to every projection.** `--projection json --detail brief`
is a valid and useful request: the same selection, serialised. This is the point
the two-axis model exists to make, and it is what makes S14 trivial.

Every projection is pipeable by construction: data on stdout, diagnostics on
stderr, no prose interleaved.

**Why `brief` is the default.** Most invocations are a person checking state or
an agent reading a result, and both are better served by one line. A user who
wants more asks for it. A user drowning in output cannot ask for less without
already knowing the flag exists. For an agent the argument is arithmetic: a
`doctor` run that returns one line has spent nothing and can escalate on the rare
occasion it needs to.

### S1a — One presentation contract, and a selection layer that does not exist

Two structural consequences, both recorded in the same sources.

**Retire the 20 interface contracts as rendering specifications.** Keep them as
data-model specifications: which facts exist and which states they hold. Delete
their prescriptions of rendered vocabulary and layout, and write one presentation
contract for every command that owns the detail levels, the selection rules,
severity ordering, and the per-command size budget.

**Add the layer that decides what to omit.** Today every renderer independently
decides what to say, so nothing decides what not to say. The proposed shape:

- **Model** — the existing typed result records. Already correct, already what
  the JSON projection serialises. Unchanged.
- **Selection** — takes a result model and a detail level, returns the subset
  worth showing. Owns severity ordering, suppression of healthy findings,
  zero-count collapsing, the `… and N more` line, and the size budget. **This
  layer does not exist.**
- **Renderers** — formatters over an already-selected subset. Small, uniform,
  mostly shared: a finding line, a key/value block, a tree, a table.

Without the selection layer, S4 through S9 have to be reimplemented in every
renderer, which is how the current output was arrived at.

### S2 — Lead with a sentence, drop the status line

Human output opens with one complete sentence a reader can act on. The separate
`Status:` line is removed from human output. Status survives in the exit code
and in the JSON envelope.

### S3 — Echo the workspace only when it is surprising

`Workspace:` and `Selected by:` appear when `--workspace` was supplied, when the
status is not `complete` or `attention`, or when `--view` is `normal` or
`full`. Otherwise they are omitted.

### S4 — Never print a fact that is fine

Omit from `brief`: zero counts, `none`, `not-applicable`, `not-requested`,
`not-required`, coverage that is complete, comparison results that match, and
any item whose state is the expected one. Replace a per-item list of fine things
with one count.

`normal` may state them, in plain words rather than internal spellings:
"nothing was retained" rather than `residual: none`.

### S5 — Order by severity, then location

Findings render errors first, then warnings, then notes. Within a severity, by
path, then line, then column. Domain and category grouping returns in
`normal`; a person scanning a `brief` wants the worst thing first, not the
first domain's first check.

### S6 — Every finding names its subject on the same line

Path, and line and column when they exist, on the same line as the message. The
current shape puts the subject on one line, the destination on a second, the
severity and title on a third, and the resolution on a fourth.

### S7 — Severity words are `Error`, `Warning`, `Note`

A finding label never carries a semantic status. `COMPLETE:` as a finding prefix
is removed. Machine codes such as `[reference.target-missing]` move to
`normal` and JSON.

### S8 — Cap the summary, and say what was capped

`brief` lists at most ten items per severity and then one line naming the
remainder and the flag that shows it. Payload is never capped; this rule applies
to findings only.

**Decision needed (D-3):** ten is a proposal, not a measured number.

### S9 — One `Next`, and it has to run

At most one `Next:` line in `brief`, carrying a command that can be pasted and
that can answer the question it is offered for. The reason line moves to
`normal`. `--help` is not a valid `Next` for a question about workspace
contents.

### S10 — Mutations state what changed; read-only commands say nothing about changing

A mutation reports the count of changed files and lists the paths. A dry run
says nothing was written. A mutation with nothing to do says so in the opening
sentence. Read-only commands drop `No files changed.` entirely: `doctor` saying
it on every run is answering a question nobody asked.

### S11 — Human text is never escaped as JSON

One owner for human text: `CliHumanText`. Its rules:

- Control characters, including tab, and unpaired surrogates render as U+FFFD in
  single-line fields.
- Every other character renders exactly. `"` is `"`. `\` is `\`.
- Authored content selected by a content projection passes through byte-exact
  and is never escaped, clamped, or line-ending-normalized.
- `PlatformLineEndings` stays where it is and applies to generated framing only.

JSON escaping belongs to `System.Text.Json` and to nothing else. The eight
command-local escapers are deleted. `DiagnosticValueLimit` moves with the owner,
and the move is recorded as creating a shared policy rather than as a silent
relocation, per the M3 boundary.

### S12 — One truncation helper

Truncation counts Unicode scalars, never splits a surrogate pair, measures
against the rendered string rather than the raw one, and marks the cut once.
JSON values are never truncated.

**Decision needed (D-4):** `…` or `...` as the marker.

### S13 — Streams and exits are unchanged

The accepted table stands: `complete` 0, `attention` 2, `incomplete` 3, `failed`
1, `invalid` 4, `blocked` 5, `interrupted` 130; `complete`, `attention` and
`incomplete` on stdout, the rest on stderr; JSON always one complete stdout
document for every status.

One addition: every command's help states that `attention` exits 2 and is not a
failure, because a caller that treats nonzero as failure will mishandle the most
common non-clean result.

### S14 — One JSON schema, `detail` as the discriminator

Today `--json --view=compact` emits `schemaVersion: 2` and `--json` emits
`schemaVersion: 1`, so a presentation choice moves the schema version. Under S1
that stops being a question worth asking: detail is one axis, projection is
another, and the selection layer produces the same subset for both.

**One `schemaVersion`. `detail` present in the envelope as the discriminator.**
The presentation contract defines membership per detail level once, for every
command, rather than each command contract defining its own compact shape.
`schemaVersion` increments only for a breaking change to the shared coordinates,
which is what it was for.

`schemaVersion: 2` and the separate compact envelope are deleted rather than kept
as an alias. Nothing ships yet.

### S15 — There is no `--verbose`

`--verbose` is deleted. Diagnostics about the run belong to `--detail full` and
keep their stderr stream, so they still never contaminate a piped projection.
This removes the ambiguity the current contract creates by insisting `--verbose`
"remains a separate diagnostic dimension and does not select the expanded view",
which required every command to answer the same question twice.

---

## Part 3 — The first decision

The handover asks for one command in both views and the first decision named.

### Doctor today, default view, this workspace

**Derived** from `DoctorHumanRenderer`, `DoctorCountsHumanRenderer` and
`DoctorFindingHumanRenderer`. Counts are illustrative; the link count is the
measured 6772.

```text
The workspace needs attention. No files changed.
Status: requires attention
Workspace: <workspace>/open-forge
Selected by: current directory
Checks: complete
Findings: 2 errors, 14 warnings, 6784 informational findings
Resolution: 0 exact repairs, 11 choices, 3 targeted commands
  2 manual decisions, 0 blocked repairs, 6784 informational findings

Workspace: checks complete
  Findings: 0 errors, 0 warnings, 0 informational findings
  Resolution: 0 exact repairs, 0 choices, 0 targeted commands
    0 manual decisions, 0 blocked repairs, 0 informational findings

Recovery: checks complete
  Findings: 0 errors, 0 warnings, 0 informational findings
  Resolution: 0 exact repairs, 0 choices, 0 targeted commands
    0 manual decisions, 0 blocked repairs, 0 informational findings

Routes and navigation: checks complete
  Findings: 0 errors, 1 warning, 0 informational findings
  ...

Links: checks complete
  Findings: 0 errors, 13 warnings, 6772 informational findings
  Resolution: 0 exact repairs, 11 choices, 2 targeted commands
    0 manual decisions, 0 blocked repairs, 6772 informational findings

  Source occurrence: .agents/AGENTS.md:9:3
    Destination: .agents/loader.md
  INFO  Link target is valid [reference.target-valid]
    Resolution: information only
    Read from: local reference inspection

  Source occurrence: .agents/directives/_directives.md:23:3
    Destination: csharp/_csharp.md
  INFO  Link target is valid [reference.target-valid]
    Resolution: information only
    Read from: local reference inspection

  ... 6770 more of these, and the broken links are somewhere inside them ...
```

### Doctor proposed, `--detail brief`, same workspace

**Proposed.** Doctor has no payload. Its entire output is findings and framing,
so at `brief` it collapses to counts. That is the whole result:

```text
2 errors, 14 warnings, 6758 notes. Run open-forge doctor --detail normal to see them.
```

Clean workspace:

```text
No problems found.
```

Blocked workspace, stderr, exit 5:

```text
Cannot check D:/work/missing: the directory does not exist.
```

This is the ladder working as intended. `find` at `brief` still prints every
match, because matches are its payload; `doctor` at `brief` prints one line,
because it has none. One rule, two outcomes.

### Doctor proposed, `--detail normal`

**Proposed.**

```text
2 errors and 14 warnings in this workspace.

Errors
  .agents/maps/_maps.md:41:7   Link target does not exist: ../guidance/testing.md
  .agents/skills/_skills.md    Two entrypoints claim this folder.

Warnings
  .agents/memory/_memory.md    Generated Entries are out of date.
  .agents/patterns/_patterns.md:112:5  Link target does not exist: ../work-plan.md
  .agents/templates/_templates.md:18:3 Heading fragment not found: #entries
  ... 11 more warnings in 9 files

Checked 812 sources, 6772 links, 120 generated sections and 5 Extensions.

Next: open-forge repair          Choose a target for each broken link.
```

### Doctor proposed, `--detail full`

**Proposed.** The six areas return as sections, informational findings appear as
one line per kind rather than one per occurrence, machine codes return, and
today's `--verbose` diagnostics join on stderr.

```text
2 errors and 14 warnings in this workspace.
Workspace: <workspace>/open-forge
Selected by: current directory

Workspace and entry — 812 sources checked, no problems
  Error  .agents/skills/_skills.md
         Two entrypoints claim this folder. [workspace.entry-ambiguous]
         Resolve the ambiguity by hand; Repair cannot choose.

Recovery — nothing retained

Routes and navigation — 120 generated sections checked
  Warning  .agents/memory/_memory.md
           Generated Entries are out of date. [route.generated-entries-changed]
           Next: open-forge index .agents/memory/_memory.md

Links — 6772 checked, 6758 valid
  Error    .agents/maps/_maps.md:41:7
           Link target does not exist: ../guidance/testing.md [reference.target-missing]
           3 possible targets, none selected.
  Warning  .agents/templates/_templates.md:18:3
           Heading fragment not found: #entries [reference.fragment-missing]
  ...
  Note     6758 link targets are valid. [reference.target-valid]

Framework — installed, payload matches
Extensions — 5 installed, all match their source

Next: open-forge repair          Choose a target for each broken link.
```

### The first decision: what happens to `reference.target-valid`

**D-1.** Everything above depends on one choice, and it is not a presentation
choice.

The result model currently carries one `DoctorFinding` object per valid link.
Rendering it or not changes the human view; it does not change the fact that the
typed result, and therefore `--json`, carries 6772 objects each with a subject,
evidence, provenance and a resolution lane. Suppressing them in the renderer
fixes the terminal and leaves `--json` unusable on a real workspace.

| Option                                                                                                                          | `brief` | `normal` | `--projection json`         | Contract impact                                                                                  |
| -------------------------------------------------------------------------------------------------------------------------------- | ------------- | --------------- | --------------------------- | -------------------------------------------------------------------------------------------------- |
| **A. Presentation only.** Keep emitting per-occurrence findings; suppress them below `full`.                                     | one line      | one line        | unchanged, still 6772 items | Doctor's "No diagnostic kind or severity is filtered out" changes.                                 |
| **B. Aggregate in the result.** Emit one `reference.target-valid` finding per domain carrying the checked and valid counts.      | one line      | one line        | one item with counts        | Same contract line changes, plus the finding's subject becomes the domain rather than an occurrence. |
| **C. Both.** Aggregate by default; `--detail full` re-expands per occurrence.                                                      | one line      | one line        | depends on `--detail`       | As B, plus a detail level that changes which findings the result contains.                        |

**Recommendation: B.** It is the only option that makes the JSON projection
usable, it keeps
the diagnostic kind in the public catalogue rather than deleting it, and it does
not make the typed result depend on the presentation flag. Option C's
view-dependent result contradicts the G4 requirement that selection and
rendering stay separate.

Option B is a behavior change, not a rendering change. The G4 packet is explicit
that "presentation grouping is not permission to remove a public finding
category", so this needs the maintainer's word before any code moves.

The same question applies, with the same three options, to
`reference.image`, `reference.external-unchecked`, and
`reference.target-unsupported`.

---

## Part 4 — Per-command specification

Each entry gives the question the command answers, how its output divides into
payload, finding and framing, and the transcripts for the states it can reach.
Every command inherits S1 through S15; only differences are stated.

### 4.1 Read-only workspace commands

#### `status`

**Question:** Is Open Forge installed here, is anything out of date, and what
does this workspace cost to load?

Payload: the installation state and the context measurements. Finding: anything
out of date. Framing: everything else.

Current default output includes the four-line header, five context measurement
lines, a `Startup share:` percentage, root category added and removed lists,
grouped navigation counts, **one line per generated navigation target**, a
recovery block, and lifecycle and library blocks.

**Proposed, healthy:**

```text
Open Forge is installed and up to date.
812 sources, 1.2 MB, about 310,000 tokens at startup — 41,000 of them loaded every session.
```

**Proposed, something to do:**

```text
Open Forge is installed. 3 generated sections are out of date.
  .agents/memory/_memory.md
  .agents/patterns/_patterns.md
  .agents/skills/_skills.md
812 sources, about 310,000 tokens at startup — 41,000 of them loaded every session.

Next: open-forge index
```

**Proposed, not installed:**

```text
Open Forge is not installed in this workspace.
Workspace: D:/work/example
Selected by: current directory

Next: open-forge install --dry-run    Preview what installing would write.
```

**Proposed, partial:** when a boundary could not be inspected, name it and keep
the facts that were established.

```text
Open Forge is installed. Some checks could not run.
  .agents/memory/private/ could not be read.
812 sources counted; the context estimate excludes the unreadable folder.
```

`--detail normal` restores the per-measurement breakdown, the largest continuity
sources, and root category changes. `--detail full` restores the per-target
navigation list.

**S4 applies hard here:** `Added: none` and `Removed: none` disappear.

#### `doctor`

Covered in Part 3. Remaining per-command points:

- The six domain identities stay in the result in every view. `brief` prints a
  domain heading only when that domain has an error or warning.
- Coverage that is `complete` is never stated in `brief`. Coverage that is
  `incomplete` or `blocked` is stated as a sentence naming what could not be
  checked.
- `blocked` on an unavailable workspace, **proposed**:

  ```text
  Cannot check this workspace: D:/work/missing does not exist.
  ```

  stderr, exit 5, and no domain skeleton printed, because none of it was
  inspected.
- `incomplete`, **proposed**:

  ```text
  1 error and 4 warnings. Some checks could not run.
  Could not check: Extension sources — the catalogue at .agents/extensions/ could not be read.

  Errors
    ...
  ```

  stdout, exit 3.
- Doctor keeps no wizard and no filters. A severity filter is unnecessary once
  `brief` lists only errors and warnings.

#### `context`

**Question:** What does an agent load when it enters this workspace?

Payload is the authored content itself, and it is the whole point of the
command. S11's byte-exactness rule matters more here than anywhere else.

Current default is `ContextExpandedHumanRenderer`, which precedes every layer
with `Path:`, `ID:`, `Route:`, `Scope:`, `Order:`, `Layer:` and one
`Included because:` line per reason. On a 40-source startup that is at least 240
framing lines wrapped around the content the caller wanted.

**Proposed default:** the content, with a one-line separator naming each source,
and nothing else.

```text
=== .agents/loader.md ===
<authored bytes, exactly>

=== .agents/directives/_directives.md ===
<authored bytes, exactly>
```

**Proposed, paths only:** one path per line, no numbering, no `ID:`/`Layer:`
block.

**Proposed `--detail normal`:** restores `ID:`, `Route:`, `Scope:`, `Order:` and
`Layer:` per source. `--detail full` restores `Included because:`.

**Proposed, a projection is unavailable:** keep the existing bracketed marker,
because it sits inside content and must not be mistaken for content.

```text
[frontmatter: unavailable]
```

#### `find`

**Question:** Which sources match?

Payload is the match list. The current `compact` renderer is already the right
answer and is not the default.

**Proposed default** — the current compact shape, promoted, with the summary
line made readable:

```text
14 matches.
directives/csharp	.agents/directives/csharp/_csharp.md
directives/style	.agents/directives/csharp/style.md
...
```

**Proposed, no matches:**

```text
No matches.
```

**Proposed, no matches and incomplete coverage:**

```text
No matches, and the search did not finish.
  .agents/memory/private/ could not be read.
```

**Decision needed (D-2):** the tab-separated `id<TAB>path` row. It is the
cheapest thing an agent can parse and the G4 gates record dropping TSV as an
unaccepted recommendation. Recommendation: **keep it**, because it is the single
most token-efficient output in the CLI and this product exists to save tokens.

`--detail normal` adds each match's description and matched evidence.
`--detail full` restores the `Search details:` echo, which is where it belongs.

#### `references`

**Question:** What links to this source, and what does it link to?

**Proposed default:**

```text
Direct links for directives/csharp
  In   .agents/directives/_directives.md:31:3
  In   .agents/memory/crystallized/documents/cli/architecture.md:88:1
  Out  .agents/directives/csharp/design.md
  Out  .agents/directives/csharp/style.md
```

**Proposed, none:**

```text
No direct links for directives/csharp.
```

The `Incoming scan:` line and the per-layer `Inspected:` list move to
`--detail full`. They describe how the answer was obtained, not the answer.

#### `route list`

**Question:** What routes exist under here?

Payload is the rows. **Proposed default** keeps the rows and drops `Coverage:`,
`Selection:`, `Requested depth:` and every `Confirmed:` line.

```text
12 routes under memory.
memory                     .agents/memory/_memory.md
memory/crystallized        .agents/memory/crystallized/_crystallized.md
...
```

**Proposed, none:**

```text
No routes under memory.
```

**Proposed, incomplete:**

```text
7 routes under memory; the listing did not finish.
  .agents/memory/private/ could not be read.
memory                     .agents/memory/_memory.md
...
```

`Unresolved:` boundaries stay in `brief` because they change what the list
means. `Confirmed:` evidence moves to `full`.

#### `route inspect`

**Question:** What is this one route?

Payload is the route's facts, so `brief` keeps them and drops only the
provenance. Interactive collision selection stays as it is; it asks one bounded
question.

#### `extension list`, `extension inspect`, `library list`, `library inspect`

**Question:** What is installed, and what is available?

**Proposed `extension list` default:**

```text
2 Extensions installed, 6 available.

Installed
  planning        1.4.0
  orchestration   2.0.0

Available
  development     3.1.0
  memory-starters 1.0.0
  ...
```

**Proposed, ownership unavailable** — replacing the `COMPLETE:` finding:

```text
6 Extensions available. Installed packages cannot be listed.
  No Extension ownership is recorded in .agents/open-forge.lock.json.

Available
  ...
```

`Source kind:`, `Packages: 1; dependencies: 0`, and per-package descriptions
move to `normal`.

**Proposed `library list` with a broken registration:**

```text
1 Library registered. 1 registered destination is missing.

team-knowledge  shared/team -> .
  Warning  .agents/directives/review.md is registered but absent.

Next: open-forge library sync team-knowledge
```

### 4.2 Mutation commands

Every mutation uses one template, stated once here. `install`, `update`,
`index`, `repair`, `cleanup`, `route create|init|update|move|remove`,
`extension create|install|update|remove`, and `library attach|detach|sync` all
follow it.

**Proposed template — applied:**

```text
<What happened>. <n> files changed.
  <path>   <what happened to it>
  ...
[Next: <command>    <why>]
```

**Proposed template — nothing to do:**

```text
<Subject> is already up to date. Nothing changed.
```

**Proposed template — preview:**

```text
<What would happen>. Nothing was written.
  <path>   <what would happen to it>
  ...
```

**Proposed template — partial:**

```text
<What happened>, then stopped. <n> files changed before it stopped.
  <path>   changed
  <path>   unknown — check this file
<Why it stopped>
[Next: <command>]
```

`Mode:`, `Preflight:`, `Workspace lock:`, `Final workspace check:`,
`Verification:`, `Lifecycle:`, `Comparison:`, `Fingerprints:`, `Source kind:`
and `residual:` move to `normal` or `full`. The opening sentence already says
whether it was a preview.

#### `index`

The closest command to right today. It already has good no-change wording and
already suppresses per-region lines that did not change.

**Proposed, no change:**

```text
Generated Entries are up to date. 120 sections checked.
```

**Proposed, applied:**

```text
Updated 3 generated sections.
  .agents/memory/_memory.md          18 -> 19 entries
  .agents/patterns/_patterns.md      7 -> 7 entries
  .agents/skills/_skills.md          4 -> 5 entries
```

**Proposed, preview:** the same list with `would change`, then
`Nothing was written.` The diff belongs in `normal`, not in the default.
Today `--dry-run` renders a full diff per region in every view.

**Proposed, partial:**

```text
Updated 2 generated sections, then stopped. 1 file has an unknown outcome.
  .agents/memory/_memory.md          updated
  .agents/patterns/_patterns.md      updated
  .agents/skills/_skills.md          unknown — check this file
The operation was interrupted.
```

#### `install`

**Proposed, first install:**

```text
Installed Open Forge. 47 files written.
  .agents/loader.md
  .agents/directives/_directives.md
  ... 44 more
  .agents/open-forge.lock.json

Next: open-forge context    See what an agent now loads here.
```

**Proposed, already installed and matching:**

```text
Open Forge is already installed and matches this version. Nothing changed.
```

**Proposed, existing content in the way:**

```text
Cannot install: 3 files already exist at installation paths.
  .agents/loader.md
  .agents/directives/_directives.md
  .agents/maps/_maps.md

Next: open-forge install --force    Replace them. Existing content is recovered first.
```

stderr, exit 5.

The captured `Source: embedded Framework; 1 assets`,
`Inventory fingerprint:`, `Footprint:`, `Lifecycle:`, `Recovery:` and
`Verification:` lines move to `normal`.

#### `update`

**Proposed, nothing to reconcile:**

```text
Managed files already match this version. Nothing changed.
```

**Proposed, applied:**

```text
Updated 4 managed files.
  .agents/loader.md                   replaced
  .agents/memory/_memory.md           generated section replaced
  ...
```

**Proposed, attention** — replacing the captured transcript in section 1.5:

```text
Update finished, and one recovery archive was kept.
  recovery-2026-09-13T14-02.zip

Next: open-forge cleanup    Remove it once you have confirmed the result.
```

#### `repair`

**Question:** Fix what can be fixed safely, and let me choose the rest.

**Proposed, nothing to repair:**

```text
Nothing to repair.
```

**Proposed, `--automatic`:**

```text
Repaired 6 links. 6 files changed.
  .agents/maps/_maps.md:41:7        -> ../guidance/testing.md
  ...
11 findings need a decision and were left alone.

Next: open-forge repair    Choose a target for each of them.
```

**Proposed, non-interactive with nothing selected:**

```text
17 findings can be repaired, but none were selected.
6 are safe and exact; 11 need you to choose a target.

Next: open-forge repair --automatic --dry-run    Preview the 6 safe repairs.
```

stderr, exit 5. This is the current `NonInteractiveBlocked` path and its message
should name both numbers, because the caller's next move depends on which
findings they care about.

**Proposed wizard entry** — replacing the current "question 1 of 400" opening:

```text
17 findings can be repaired.
  6 are safe and exact.
  11 need you to choose among possible targets.

Apply the 6 safe repairs now? [yes / choose each / cancel]
```

**Proposed, interrupted mid-apply:** the partial template, naming every file
whose outcome is unknown.

#### `cleanup`

**Proposed, nothing to remove:**

```text
No recovery data to remove.
```

**Proposed, applied:**

```text
Removed 3 recovery archives. 41 MB freed.
  recovery-2026-09-01T09-12.zip
  ...
```

**Proposed, preview:**

```text
3 recovery archives would be removed. Nothing was written.
  ...
```

`Candidate check:`, `plan:`, `Preflight:`, `Workspace lock:` and
`Final workspace check:` move to `normal`. The dry-run sentence about leases
and final validation moves with them.

#### `route create`, `route init`, `route update`, `route move`, `route remove`

One shape, per the template.

```text
Created route memory/decisions.
  .agents/memory/decisions/_decisions.md
  .agents/memory/_memory.md    entry added

Next: open-forge index memory    Rebuild the parent Entries.
```

`route move` additionally reports rewritten references as payload, because they
are files the command changed:

```text
Moved memory/decisions to memory/crystallized/decisions. 9 files changed.
  .agents/memory/crystallized/decisions/_decisions.md   moved
  8 references updated in 6 files
```

`--detail normal` lists the eight references. In `brief` the count is enough.

#### `extension create`, `extension install`, `extension update`, `extension remove`

The mutation template. Extension Create's wizard keeps its current bounded shape,
which asks only for the missing required facts.

**Proposed `extension install`, applied:**

```text
Installed Extension planning 1.4.0. 12 files written.
  .agents/workflows/planning/_planning.md
  ... 11 more

Next: open-forge context    See what an agent now loads here.
```

**Proposed, already current:**

```text
Extension planning 1.4.0 is already installed and matches its source. Nothing changed.
```

**Proposed, path not permitted:**

```text
Cannot install Extension planning: a package targets a reserved path.
  .agents/open-forge.lock.json
```

stderr, exit 5. The reserved set is `.agents/open-forge.json`,
`.agents/open-forge.lock.json`, `.agents/open-forge.lock` and their descendants,
per `ExtensionDestinationPolicy.IsAllowed` and the SG-R1 correction.

#### `library attach`, `library detach`, `library sync`

The mutation template. `library sync` replaces its captured block from section
1.4 with:

```text
Synchronised team-knowledge. 2 links created.
  docs/first.md    -> ../shared/first.md
  docs/second.md   -> ../shared/second.md
```

**Proposed, interrupted:**

```text
Library sync was interrupted. Recovery data was kept.
  .agents/recovery/pending.json
Nothing is confirmed as written. Run the command again to finish.
```

stderr, exit 130.

**Proposed, permission needed:**

```text
Cannot sync team-knowledge: writing outside .agents is not permitted.
  docs/second.md

Next: open-forge library sync team-knowledge --allow-path docs
```

### 4.3 Terminal modes and input errors

#### `--help`

Help stays plain text on stdout, exit 0, and keeps its product sections.

Two changes are proposed:

- Root help's `Getting started` names the interactive path:

  ```text
  Getting started
    open-forge install --dry-run
    open-forge context
    open-forge doctor
    open-forge repair          Fix what doctor found, interactively.
  ```

- Every command's `Results and streams` section keeps its exit table and gains
  one sentence: `attention` means the command finished and found something worth
  looking at; it is not a failure.

The `[default: Expanded]` fragment in the generated options block becomes
`[default: summary]` once S1 is accepted.

#### `--version`

Unchanged. Plain text, stdout, exit 0.

#### Invalid input

**Proposed:**

```text
open-forge doctor does not accept the operand "memory".
Run open-forge doctor --help for the accepted form.
```

stderr, exit 4. Name the command, the exact rejected input, and one way
forward. No status line, no workspace echo, and no envelope framing, because
nothing was inspected.

---

## Part 5 — Verification this change needs

The G4 packet and the M3 boundary both require the before snapshot first. These
are the conditions the implementation must meet.

1. **Capture before changing anything.** Run every command in every reachable
   state against a fixture workspace and against this repository, in all views
   and both formats, and commit those outputs as reviewed evidence in their own
   commit. A snapshot taken after a renderer change proves nothing.
2. **Measure `doctor` once.** Record the actual byte count on this repository, so
   the reduction claim has a number behind it and the 258 MB report is either
   confirmed or corrected.
3. **Use the accepted normalization boundary.** Normalize only named
   environment-dependent paths, versions and fingerprints, and line endings, per
   [Phase 7 scenarios](phase-7-scenarios.md). Never normalize counts, ordering,
   encoding or diagnostic content.
4. **Prove the escaping contract.** One owner, with tests for `"`, `\`, tab, CR,
   LF, CRLF, mixed endings, a lone surrogate, an astral pair, and a value at and
   just over the truncation limit. Authored content must be proved byte-exact
   through a content projection.
5. **Prove the payload rule.** A test that `--detail brief` never drops a match,
   a row, an entry, or an effect, on a fixture large enough to exceed any cap.
6. **Prove the JSON rule.** One `schemaVersion`, `detail` present in every
   envelope, and the presentation contract's per-level membership asserted for
   every command. Also prove that `--projection json --detail brief` and
   `--projection text --detail brief` select the same subset.
7. **Keep the suites' known conditions.** Integration `skipped: 17`;
   `npm run check:dotnet` reporting exactly 5 whitespace errors in the two files
   no slice touches, per [conventions](00-conventions.md).

---

## Part 6 — Implementation slices

Ordered. Each is separately committable and green.

| Slice  | Content                                                                                                            | Changes output |
| ------ | -------------------------------------------------------------------------------------------------------------------- | -------------- |
| **G4-0** | Capture and commit the complete before snapshot, plus the `doctor` byte measurement. No production change.        | no             |
| **G4-1** | One text owner in `CliHumanText`: human escaping, one truncation helper, `DiagnosticValueLimit`. Delete the eight escapers. This is Task 31 M3. | **yes**        |
| **G4-2** | Shared framing: S2, S3, S4, S10. Header, workspace echo, zero-fact suppression, the read-only `No files changed.` removal. | **yes**        |
| **G4-3** | S1 and S1a: `--detail` and `--projection` replace `--view`, `--json` and `--verbose`; add the selection layer; write the one presentation contract and strip rendering prescriptions from the 20 interface contracts. | **yes**        |
| **G4-4** | Finding presentation: S5, S6, S7, S8, S9 across Doctor, Status, Index, Find, Route list, References.               | **yes**        |
| **G4-5** | D-1, whichever option is accepted. Doctor result model and the Doctor interface contract's filtering sentence.     | **yes**        |
| **G4-6** | S14: one schema version, `detail` as discriminator, delete the compact envelope.                                 | **yes**        |
| **G4-7** | Help text, `Getting started`, the `attention` sentence, and the repair wizard's opening summary.                   | **yes**        |

Every slice from G4-1 onward updates its contracts in the same commit as the
behavior, per the plan's rule for output-changing slices.

---

## Part 7 — Decisions and open questions

### Decisions the maintainer owns

| ID      | Decision                                                                                 | Recommendation                                |
| ------- | ------------------------------------------------------------------------------------------ | ----------------------------------------------- |
| **D-1** | What happens to `reference.target-valid` and the other per-occurrence informational kinds | B — aggregate in the result                    |
| **D-2** | Keep or drop Find's tab-separated rows                                                    | Keep                                           |
| **D-3** | The `brief` cap per severity                                                            | 10, pending a look at real finding counts      |
| **D-4** | Truncation marker                                                                         | `…`                                            |
| **D-5** | S1: retire `--view` and `--verbose` for `--detail brief\|normal\|full` and `--projection text\|json\|tsv` | As proposed; it restates the recorded narrowing |
| **D-6** | S14, one JSON schema with `detail` as discriminator                                       | As proposed                                    |
| **D-7** | Whether machine codes leave the default human view                                        | Yes, they move to `normal`                   |

### Open questions

**O-1 — What "no wizard" means.** Repair, Extension Create and Route Inspect all
have working interactive paths, gated on both stdin and prompt output being
unredirected. Two candidate readings are in section 1.7: undiscoverable, or
unusable at scale. Both are worth fixing and G4-7 covers both, but the maintainer
should confirm whether a third thing was meant — for example an install or
onboarding wizard that does not exist at all.

**O-2 — Whether `status` should keep the context estimate in the default view.**
It is the one number that justifies the product's existence, which argues for
keeping it. It is also framing rather than an answer to "is anything wrong",
which argues for `normal`. Recommendation: keep it, as one line.

**O-3 — Whether `attention` should stay exit 2.** It is accepted and this
proposal does not reopen it. Recorded only because every wrapper script that
treats nonzero as failure will mishandle the commonest non-clean result, and
S13's help sentence is a mitigation rather than a fix.

**O-4 — What `full` owes the reader once it absorbs `--verbose`.** `full` carries
two different things: the command's evidence about the workspace, and diagnostics
about the run itself. S1 deliberately merges them, on the argument that a user at
`full` wants both and should not have to know which flag holds which. The cost is
that `full` becomes large. Recommendation: merge as proposed, keep run
diagnostics on stderr so a piped projection stays clean, and revisit only if a
real case needs evidence without diagnostics.

---

## Deviations from the G4 packet

- The packet asks for a per-command review checklist covering user question,
  default tier, selected fields, suppression, ordering, subject, `Next`, context
  echo, size budget, JSON projection, and exit and stream meaning. Part 4 covers
  the question, default contents, suppression, ordering, subject, `Next` and
  context echo per command, and covers size budget, JSON projection, and exit
  and stream meaning once in Part 2 rather than repeating them 28 times. That
  follows the Loader's rule to put detail in the narrowest source that owns the
  question, and it is a deliberate departure from the packet's wording.
- The packet lists `--view=compact|expanded` as current. S1 retires `--view` and
  `--verbose` entirely, which is larger than a rename. The global-flags contract
  states the flags do not ship yet, so this is a design choice rather than a
  compatibility break.
- **Correction made during drafting.** The first version of this record proposed
  `--view=summary|detailed|full`, keeping `--view` and keeping `--verbose`
  separate. That was a weaker restatement of a narrowing the analysis had already
  reached. The maintainer pointed at it, the recorded sources were re-read, and
  S1, S1a, S14, S15 and every transcript were rewritten to match. The earlier
  wording is not preserved here because it was never accepted and adds nothing;
  this note is the record that it happened.
- The packet treats the output budget as presentation. D-1 shows that the
  largest single contributor is a result-model decision, not a rendering one.
  This is the material deviation in the packet and it is why D-1 is the first
  decision rather than a detail inside Doctor's section.
