---
open-forge:
  description: "install: scenario collection"
  tags: [Memory, Document, CLI, Scenario, Evergreen]
---

# install: scenario collection

These are reviewed user outcomes. Selection does not mean execution passed. Improved targets remain subject to flow validation before new tests. Deferred cases retain only their identity and reason; they are not adopted specifications.

**Who:** A maintainer using Open Forge through the public CLI, without knowing its implementation or internal state model.

**Goal:** Add Open Forge without losing existing project content or making me interpret internal state.

Existing baseline: [Interface](../../../contracts/install/interface.md), [Behavior](../../../contracts/install/behavior.md). Exact interface and output differences must be reconciled before implementation.

See [scenario conventions](../_scenarios.md) and the [complete assessment](../../assessment.md).

## C03-01

**Situation:** Fresh directory

**Disposition:** Added. Worth retaining as an independently checked user outcome: Install the Framework, identify created host sections and summarize the managed content with truthful counts.

### Starting point

W0 contains an unrelated README and no Open Forge installation.

Fixture: `W0`. fixture recipe; not instantiated.

### Steps

```text
open-forge install --automatic
```

### Expected result

Install the Framework, identify created host sections and summarize the managed content with truthful counts.



### Verification

Compare the full pre/post tree, including hidden files and root directories; use COUNT rules, not the historical 21/20 literals.

## C03-02

**Situation:** Fresh directory dry run

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show what would be installed and finish with No files were changed.

### Starting point

Use an untouched copy of W0.

Fixture: `W0`. fixture recipe; not instantiated.

### Steps

```text
open-forge install --dry-run
```

### Expected result

Show what would be installed and finish with No files were changed.



### Verification

Compare bytes, directory entries, settings and external recovery inventory before and after; preview must not create consent state.

## C03-03

**Situation:** Already installed

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say Open Forge is already installed and current, with nothing to do.

### Starting point

W1 exactly matches the running bundle and its verified ownership receipts.

Fixture: `W0`. fixture recipe; not instantiated.

### Steps

```text
open-forge install --automatic
```

### Expected result

Say Open Forge is already installed and current, with nothing to do.



### Verification

Hash all files and inventory external recovery; a repeated install must not churn navigation or receipts.

## C03-04

**Situation:** Existing agents md

**Disposition:** Added. Worth retaining as an independently checked user outcome: Keep the authored instructions and describe the actual host-file treatment.

### Starting point

W0 has a hand-authored AGENTS.md with distinctive text and no conflicting managed region.

Fixture: `W0`. fixture recipe; not instantiated.

### Steps

```text
open-forge install --automatic
```

### Expected result

Keep the authored instructions and describe the actual host-file treatment. Do not report the whole file as newly created.



### Verification

Compare authored bytes and any inserted bounded region separately; count files below .agents independently of host files and the root directory.




## C03-05

**Situation:** Occupied without force

**Disposition:** Added. Worth retaining as an independently checked user outcome: Name the exact conflict before any installation effects and explain the available deliberate choice.

### Starting point

One install target outside category-entrypoint and native-Skill adoption is
occupied by an eligible unowned file with different bytes; other targets are
absent. Category entrypoints and native Skills instead follow the preservation
behavior in the [Install Behavior Contract](../../../contracts/install/behavior.md).

Fixture: `W0`. fixture recipe; not instantiated.

### Steps

```text
open-forge install --automatic
```

### Expected result

Name the exact conflict before any installation effects and explain the available deliberate choice.



### Verification

Verify every target and unrelated file remains unchanged; automatic confirmation is not force authority.

## C03-06

**Situation:** Occupied with force

**Disposition:** Added. Worth retaining as an independently checked user outcome: Replace only eligible conflicts and report which existing files were replaced.

### Starting point

Reuse the eligible unowned-occupant fixture, after reviewing the replacement preview.

Fixture: `W0`. fixture recipe; not instantiated.

### Steps

```text
open-forge install --force --automatic
```

### Expected result

Replace only eligible conflicts and report which existing files were replaced.



### Verification

Inspect before/after bytes and recovery evidence; force must not bypass reserved paths or another owner's claims.

## C03-07

**Situation:** Changed framework file

**Disposition:** Added. Worth retaining as an independently checked user outcome: Direct the user to update instead of pretending this is a fresh install conflict.

### Starting point

W1 contains one semantically changed Framework-owned file.

Fixture: `W0`. fixture recipe; not instantiated.

### Steps

```text
open-forge install --automatic
```

### Expected result

Direct the user to update instead of pretending this is a fresh install conflict.



### Verification

Verify the edit remains intact and install does not widen force or ownership authority.

## C03-08

**Situation:** Confirmation unavailable

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain that confirmation is required without waiting forever or printing an unanswered terminal prompt.

### Starting point

Run from a redirected, noninteractive session with an actual installation plan and no automatic flag.

Fixture: `W0`. fixture recipe; not instantiated.

### Steps

```text
open-forge install
```

### Expected result

Explain that confirmation is required without waiting forever or printing an unanswered terminal prompt.



### Verification

Capture stdin mode, stderr, exit and unchanged filesystem; compare against the valid --automatic counterpart.

## C03-09

**Situation:** Recovery store unavailable

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the required recovery limitation and make no target changes.

### Starting point

Use a plan that genuinely needs prior-byte recovery and make its required recovery store unreadable.

Fixture: `W0`. fixture recipe; not instantiated.

### Steps

```text
open-forge install --automatic
```

### Expected result

Explain the required recovery limitation and make no target changes. Do not substitute a corrupt unrelated old log.



### Verification

Prove the recovery dependency is actually needed, access really fails, and no target was touched.

**Execution constraint:** Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure.

## C03-10

**Situation:** Write failed partial

**Disposition:** Added. Worth retaining as an independently checked user outcome: State that install stopped, distinguish completed and unstarted effects, and name available recovery truthfully.

### Starting point

Use FAULT-AFTER-EFFECT with an externally reproducible write failure after a known completed effect.

Fixture: `W0`. controlled fixture required.

### Steps

```text
open-forge install --automatic
```

### Expected result

State that install stopped, distinguish completed and unstarted effects, and name available recovery truthfully.



### Verification

Compare receipts with actual completed effects; never use a successful-install headline or claim nothing changed.

**Execution constraint:** Establish a deterministic public process or real filesystem boundary and independently verify before/after effects; no timer-only or mocked-result proof.

## C03-11

**Situation:** Cancelled

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say installation was cancelled and nothing was changed.

### Starting point

Use a real terminal and reject final confirmation before any effect.

Fixture: `W0`. fixture recipe; not instantiated.

### Steps

```text
open-forge install
```

### Expected result

Say installation was cancelled and nothing was changed.



### Verification

Verify no target, settings or recovery effect; mid-write cancellation is separately required by X09.

**Execution constraint:** Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence.

## C03-12

**Situation:** Invalid input

**Disposition:** Added. Worth retaining as an independently checked user outcome: Reject the argument without attempting installation.

### Starting point

W0 is valid; supply an unsupported flag.

Fixture: `W0`. fixture recipe; not instantiated.

### Steps

```text
open-forge install --not-an-option
```

### Expected result

Reject the argument without attempting installation.



### Verification

Check parser-level diagnostics, invalid-input exit and preservation of W0.

## C03-S05

**Situation:** Outcome 05

**Disposition:** Deferred; not selected yet. Retained recovery matters, but this generic permutation has no demonstrated recovery-requiring plan or reliable post-success cleanup fault.

**Required before reconsideration:** Prove this exact plan requires recovery and synchronize successful effects with denied cleanup of its positively identified bundle.

## C03-S10

**Situation:** Outcome 10

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the exact unsafe boundary and stop before all target effects.

### Starting point

An intended install target crosses a protected or unsafe path boundary; no unrelated metadata problem is present.

Fixture: `W0`. fixture recipe; not instantiated.

### Steps

```text
open-forge install --automatic
```

### Expected result

Explain the exact unsafe boundary and stop before all target effects.



### Verification

Verify no traversal or modification beyond the safe workspace; force and automatic must not bypass it.

## C03-13

**Situation:** Fresh unattended Install defaults to root and records `"frontmatter": "root"`

**Disposition:** Added. Start an unattended workspace with root metadata and a saved choice.

### Starting point

W0 contains a hand-authored README, no installation, and no declared frontmatter preference. The canonical bundled payload is independently available.

Fixture: `W0`. Use the same workspace for preview and application.

### Steps

```text
open-forge install --dry-run
open-forge install --automatic
```

### Expected result

The preview selects root without asking and changes no files. Application delivers eligible Framework Markdown in root form and writes `"frontmatter": "root"` in `.agents/open-forge.json`, with `schemaVersion` still 1. Native `SKILL.md` files and other ineligible assets retain their canonical bytes. Both reports show `Frontmatter: root`. JSON exposes `data.frontmatter.form` as `root` and omits `previousForm` for this fresh installation.

### Verification

**State:** Compare the entire tree before and after preview. After application, read the settings independently and compare every delivered Markdown file with the canonical payload rendered to root, following [X31](../experience.md#x31). Preserve the README bytes.

**Output:** Record stdout, stderr, exit, effects, and the resolved form. Check that neither request asks a setup question and that preview describes planned effects rather than applied writes. Check the JSON form on an equivalent fresh fixture without accepting the report as proof of delivery.

## C03-14

**Situation:** Fresh Install with `--frontmatter scoped` delivers canonical scoped bytes and records the choice

**Disposition:** Added. Choose scoped metadata explicitly on first installation.

### Starting point

W0 has no installation or declared preference. The canonical scoped payload is independently available.

Fixture: `W0`.

### Steps

```text
open-forge install --frontmatter scoped --automatic
```

### Expected result

Install delivers eligible Markdown in canonical scoped form and writes `"frontmatter": "scoped"`, with `schemaVersion` still 1. The explicit option skips the form question. The report shows `Frontmatter: scoped`, and JSON resolves `data.frontmatter.form` to `scoped` without `previousForm`.

### Verification

**State:** Inspect the settings file and compare delivered bytes with the canonical scoped payload, following [X31](../experience.md#x31). Check the actual file inventory and preserved README independently.

**Output:** Record the resolved form, actual effect identities, stdout, stderr, and exit. Check the absence of a form question and the fresh-install JSON shape on an equivalent fixture.

## C03-15

**Situation:** First interactive Install asks for the form after the preset, with root first

**Disposition:** Added. Choose the metadata form while reviewing first-time setup.

### Starting point

W0 has no installation or declared preference. Use a real prompt-capable terminal without `--automatic`, `--frontmatter`, or JSON output.

Fixture: `W0`, with terminal input and prompt evidence captured.

### Steps

```text
open-forge install
```

Select a preset, retain the preselected root choice, review the complete plan, and confirm application.

### Expected result

After the preset choice, Install asks `How should Open Forge write file metadata?` with `Root keys` preselected. Its explanation is `description: and tags: at the top level`. The other choice is `Scoped under open-forge:` with `open-forge: holds description: and tags:`. Selection finishes before plan review and final confirmation. The applied files and saved preference use the selected form.

### Verification

**State:** Before final confirmation, verify that setup selection has written nothing. After confirmation, independently inspect settings and delivered bytes for the selected preset and form.

**Output:** Capture the preset question, form question, preselected choice, explanations, plan, and confirmation in their actual order. A redirected run without prompts does not prove this scenario.

## C03-16

**Situation:** `--frontmatter` on an installed workspace without `--configure` is invalid input with no writes

**Disposition:** Added. Explain how to change an installed workspace's form before making any change.

### Starting point

W1 is installed in scoped form. Its settings may omit `frontmatter`, as an earlier release did. Record content, settings, ownership, and recovery state before the request.

Fixture: `W1`, with the effective form independently verified as scoped.

### Steps

```text
open-forge install --frontmatter root --automatic
```

### Expected result

Install returns `invalid-input` at exit 4 and explains that an explicit form on an installed workspace requires `--configure`. It performs no settings, payload, ownership, or recovery writes.

### Verification

**State:** Compare the complete pre-request and post-request tree, settings bytes, ownership bytes, and recovery inventory. The effective form remains scoped.

**Output:** Check the invalid-input classification and the correction involving `--configure`. Do not infer no writes solely from the diagnostic.

## C03-17

**Situation:** Configure preview shows the form change, the replacements, and kept edited files, with no writes

**Disposition:** Added. Review a form conversion and its retained edits before applying it.

### Starting point

W1 is installed in scoped form with the Collaboration Extension and available canonical sources. Remove only the `frontmatter` property to represent an earlier release. Create a user-owned scoped note with Route Create and append a distinctive paragraph to owned `.agents/patterns/_patterns.md`. Record all fixture bytes.

Fixture: `W1`, with current unedited owned files, one edited owned file, and one user-owned note.

### Steps

```text
open-forge install --configure --frontmatter root --dry-run
```

### Expected result

The plan shows `Frontmatter: scoped -> root`, `settings would be updated`, and `metadata would move to root keys` for eligible owned targets that match the scoped payload. Replacement bytes include projected `Entries`. The edited owned file is kept unchanged and reported with reason `edited`. The user note is outside conversion. Form-only configuration leaves route choices unchanged and requires no preset or force. Preview writes nothing.

JSON exposes `data.frontmatter.form` as `root`, `previousForm` as `scoped`, and `kept` with the edited path and reason at every detail level. Text reports the kept count, with paths and reasons at `standard` detail and above. Kept files add no finding code.

### Verification

**State:** Compare content, settings, ownership, and recovery state before and after preview. Independently identify which owned targets match each rendered form and which edited bytes match neither. The settings still omit the key.

**Output:** Check the form transition, replacement set, and kept count independently of state. Check per-path reasons at standard detail and the JSON members on repeated read-only previews of the same fixture.

## C03-18

**Situation:** Configure converts unedited owned files, keeps edited and user files, writes the key, and repeats as a no-op

**Disposition:** Added. Switch forms while preserving edited owned files and independently authored content.

### Starting point

Use the scoped fixture from C03-17, with available Framework and Collaboration sources, an edited owned `.agents/patterns/_patterns.md`, and a user-owned scoped note. Save independent byte copies of the edited file and note before conversion. The settings omit `frontmatter`.

Fixture: `W1`. Carry the real converted state into the repeat.

### Steps

```text
open-forge install --configure --frontmatter root --automatic
open-forge install --configure --frontmatter root --automatic
```

### Expected result

The first request writes `"frontmatter": "root"` and converts eligible owned Framework and Extension files that match the other form. Files already matching the selected form need no replacement. The edited owned file and user note remain byte-identical. Excluded, Library, and overwrite files remain outside conversion. An Extension target whose source is unavailable is kept with reason `source-unavailable` when present in the selected workspace.

The result reports the form transition and kept owned paths without adding a finding code for retention. Settings and replacements share one reviewed plan with ordinary recovery. The identical second request changes no files and omits `previousForm` because the effective form did not change. A corresponding switch back to scoped uses the same bounded rule and restores canonical scoped bytes for eligible unedited targets, with projected `Entries`.

### Verification

**State:** Read settings independently, compare converted targets with [X31](../experience.md#x31), and compare the edited file, note, and other excluded content with saved bytes. Check route choices and unselected ownership remain intact. Compare all bytes and recovery state before and after the repeat. For the scoped return in F28, compare eligible converted bytes with canonical scoped sources and independently projected navigation.

**Output:** Check reported settings and replacement effects against the actual changes. Check kept paths and reasons separately. The repeat must describe a no-op rather than another conversion, even when it still reports a kept file.

## C03-19

**Situation:** A preference already declared in the settings file is honored by the first Install

**Disposition:** Added. Keep the metadata preference authored before installation.

### Starting point

W0 has no installation, but a valid `.agents/open-forge.json` declares `"frontmatter": "scoped"` with `schemaVersion` 1. Include an unrelated settings member and record it before installation.

Fixture: `W0`, with an explicit pre-install preference and no `--frontmatter` option.

### Steps

```text
open-forge install --automatic
```

### Expected result

First Install honors scoped instead of choosing the fresh unattended root default. It records the resolved choice, preserves unrelated settings members, and delivers eligible Markdown in scoped form. The report identifies scoped without a form question or a fresh-install `previousForm`.

### Verification

**State:** Read the saved settings and compare delivered bytes with the canonical scoped payload. Check that unrelated settings and the README remain intact.

**Output:** Check the resolved form and fresh-install JSON shape separately from delivery. Neither a printed scoped line nor an unchanged setting proves the file form by itself.

## C03-20

**Situation:** Interactive Custom shows one marked list, keeps `--route` rows locked, and finishes with one plan

**Disposition:** Added. Change several setup rows on one screen instead of answering two questions per row.

### Starting point

W1 is installed with Essentials. Use a real prompt-capable terminal without `--automatic` or JSON output. Record the settings, lock and `.gitignore` bytes before the run.

Fixture: `W1`, with terminal input and prompt evidence captured.

### Steps

```text
open-forge install --configure --preset custom --route templates=add
```

In the list, press `+` on `guidance`, press space on `memory/archived` until it shows `[~]`, try to change `templates`, then press Enter. Review the plan and confirm. Repeat on a fresh copy of the fixture in line mode by typing `2+ 10~`, an edit to the `templates` row, and an empty line.

### Expected result

Custom shows `Choose what Open Forge sets up`, the legend `+ add   ~ add, keep contents out of Git   - leave out`, and all ten rows with their current marks and summaries. The `templates` row is locked and explains that `--route templates=add` set it. Its mark does not change. Enter accepts the whole list. One plan follows, then one confirmation. The applied workspace adds Guidance, Templates and Git-ignored Archived Memory and keeps every other choice.

### Verification

**State:** Before confirmation, verify that nothing was written. After confirmation, inspect settings, the sharing policy in the lock, `.gitignore` and the delivered routes independently.

**Output:** Capture the list before and after each key, the locked-row explanation, the plan and the confirmation in their actual order. Check the line-mode rule after the invalid edit. A redirected run without prompts does not prove this scenario.

## C03-21

**Situation:** A Configure plan describes each change to an existing file instead of calling it a replacement

**Disposition:** Added. A user who only adds a route must not be told that their files will be replaced.

### Starting point

W1 is installed with Essentials, with user-authored content in `AGENTS.md` outside the Open Forge section and one user rule in `.gitignore`. Record the bytes of every existing file the plan lists.

Fixture: `W1`, with the user content above.

### Steps

```text
open-forge install --configure --preset custom --route guidance=add --route memory/archived=git-ignore --dry-run --detail standard
open-forge install --configure --preset custom --route guidance=add --route memory/archived=git-ignore
```

Confirm the second command in a prompt-capable terminal.

### Expected result

The preview headline is `Would change the Open Forge setup in <workspace>.` The loader and Memory entrypoint rows say `Entries would be updated`, `.agents/open-forge.json` says `settings would be updated`, the lock says `ownership record would be updated`, and `.gitignore` says `Open Forge Git-ignore rules would be updated`. No row says `replaced`. One summary line counts created files and directories and updated existing files. The confirmation asks `Apply these changes? [y/N]`. The applied result uses the done forms of the same labels.

### Verification

**State:** After application, compare each listed existing file with its recorded bytes. Only the Entries, settings, ownership record and Open Forge Git-ignore section changed. The user rule in `.gitignore` and the user content in `AGENTS.md` are unchanged.

**Output:** Capture the preview, the confirmation question and the applied result. Check that the headline has no replacement count and that the directory count appears once.
