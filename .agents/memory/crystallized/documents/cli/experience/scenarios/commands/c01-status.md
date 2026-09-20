---
open-forge:
  description: "status: scenario collection"
  tags: [Memory, Document, CLI, Scenario, Evergreen]
---

# status: scenario collection

These are reviewed user outcomes. Selection does not mean execution passed. Improved targets remain subject to flow validation before new tests. Deferred cases retain only their identity and reason; they are not adopted specifications.

**Who:** A maintainer using Open Forge through the public CLI, without knowing its implementation or internal state model.

**Goal:** Know whether this workspace is ready, without changing it.

Existing baseline: [Interface](../../../contracts/status/interface.md), [Behavior](../../../contracts/status/behavior.md). Exact interface and output differences must be reconciled before implementation.

See [scenario conventions](../_scenarios.md) and the [complete assessment](../../assessment.md).

## C01-01

**Situation:** Not installed

**Disposition:** Improved and added. The starting state says uninstalled W0 but the fixture label says W1.

### Starting point

Use W0, with an unrelated README and no .agents/loader.md. Do not use the installed W1 fixture.

### Steps

```text
open-forge status
```

### Expected result

**Reviewed target:** With no local installation, explain that Open Forge is not installed in the selected workspace and preserve every file. Absence is an ordinary observed state.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.


**Execution constraint:** Start from a verified uninstalled workspace.

## C01-02

**Situation:** Healthy

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say the Framework is installed and current; show measured startup cost, not a diagnostic dump.

### Starting point

W1 has verified current Framework content, complete checks and no unrelated warning fixture.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge status
```

### Expected result

Say the Framework is installed and current; show measured startup cost, not a diagnostic dump.



### Verification

Compare each stated measure with its declared source set; inspect all emitted lines for irrelevant zero counts.




## C01-03

**Situation:** Healthy with extension

**Disposition:** Added. Worth retaining as an independently checked user outcome: Report a healthy installation and the actual installed Extension context, without treating the Extension as drift.

### Starting point

Install the pinned toolkit package and its declared dependencies into W1; verify their content before this check.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge status
```

### Expected result

Report a healthy installation and the actual installed Extension context, without treating the Extension as drift.



### Verification

Count distinct installed packages separately from dependency memberships and from files.




**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C01-04

**Situation:** Changed managed file

**Disposition:** Added. Worth retaining as an independently checked user outcome: Identify the file that differs from the current selected source.

### Starting point

Change one owned Framework file's meaning, preserving valid Markdown and route metadata.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge status
```

### Expected result

Identify the file that differs from the current selected source. Offer the relevant update action without claiming a historical user edit unless independently established.



### Verification

Compare hashes and known ownership; verify that status did not undo the edit.




## C01-05

**Situation:** Missing managed file

**Disposition:** Added. Worth retaining as an independently checked user outcome: Name the missing file and explain the consequence.

### Starting point

Remove one owned leaf file, leaving the rest of the installation readable.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge status
```

### Expected result

Name the missing file and explain the consequence. Count affected files, not repeated findings about one file.



### Verification

Compare unique affected paths with headline N; verify absence remains absence after this read-only command.




## C01-06

**Situation:** Stale entries

**Disposition:** Added. Worth retaining as an independently checked user outcome: Identify the stale navigation and the applicable indexing action.

### Starting point

Add one fully valid unowned route file without changing its parent's Entries list.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge status
```

### Expected result

Identify the stale navigation and the applicable indexing action. Do not describe the new file itself as a failed Framework update.



### Verification

Compare authored child inventory with the parent list; inspect any competing changed-file and stale-list diagnoses separately.




## C01-07

**Situation:** Recovery bundle present

**Disposition:** Added. Worth retaining as an independently checked user outcome: Name the leftover recovery item and the applicable cleanup action.

### Starting point

Use RECOVERY with one verified leftover bundle for this workspace and no other drift.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge status
```

### Expected result

Name the leftover recovery item and the applicable cleanup action. Do not call a recovery item an affected source file. The linked flow previews cleanup before applying it.



### Verification

Match the subject to the external recovery inventory; verify status does not remove it.

## C01-08

**Situation:** Library link missing

**Disposition:** Added. Worth retaining as an independently checked user outcome: Identify the missing Library link and the specific Library sync action.

### Starting point

Use LIB; unlink one registered destination without touching its source.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge status
```

### Expected result

Identify the missing Library link and the specific Library sync action.



### Verification

Compare the destination directory entry, registration and untouched source bytes.




## C01-09

**Situation:** No ownership record

**Disposition:** Improved and added. A forced healthy headline can imply ownership-dependent checks succeeded when they are unknown.

### Starting point

Keep W1 content but remove only the generated ownership lock in the disposable fixture.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge status
```

### Expected result

**Reviewed target:** Report independently known installation and content facts, explain that ownership-based comparisons are unavailable, and preserve unknown values. Continue other checks and leave all files unchanged.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.


## C01-10

**Situation:** Unreadable entry file

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say which checks could not finish and which source could not be read.

### Starting point

Use READ-DENIED on one required entrypoint, with no malformed-metadata fixture mixed in.

Fixture: `W1`. controlled fixture required.

### Steps

```text
open-forge status
```

### Expected result

Say which checks could not finish and which source could not be read. Do not equate unknown measures with zero.



### Verification

Independently confirm access denial under the running account; inspect each dependent measurement for unavailable status.




**Execution constraint:** Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure.

## C01-11

**Situation:** Blocked workspace

**Disposition:** Added. Worth retaining as an independently checked user outcome: Name the selected missing workspace and stop.

### Starting point

Supply an explicit nonexistent workspace path; keep a healthy parent workspace beside it.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge status --workspace "$MISSING_WS"
```

### Expected result

Name the selected missing workspace and stop. Do not search its parent or inspect a different workspace.



### Verification

Check the parent's hashes and captured output for accidental parent discovery.

## C01-12

**Situation:** Invalid input

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the invalid value before any workspace work.

### Starting point

W1 is healthy; the only defect is an unsupported global detail value.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge status --detail=verbose
```

### Expected result

Explain the invalid value before any workspace work. Parser-level text is not a schema-3 domain report.



### Verification

Verify nonzero invalid-input exit and no writes; do not demand a domain headline from a pre-binding parser error.

## C01-S06

**Situation:** Outcome 06

**Disposition:** Added. Worth retaining as an independently checked user outcome: Report both the known attention item and incomplete checks.

### Starting point

One owned file is independently known changed; a different required entry file is unreadable.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge status
```

### Expected result

Report both the known attention item and incomplete checks. Do not let either fact erase the other.



### Verification

Compare unique affected paths and per-measurement availability, not finding-row totals.

**Execution constraint:** Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure.

## C01-S09

**Situation:** Outcome 09

**Disposition:** Deferred; not selected yet. No concrete unexpected failure mechanism or reproducible fixture is supplied; a generic outcome row adds no executable user scenario yet.

**Required before reconsideration:** Supply a plausible public/OS failure mechanism and independently verified fixture; do not inject arbitrary private result states.

## C01-S10

**Situation:** Outcome 10

**Disposition:** Deferred; not selected yet. Repeated cancellation outcome without a concrete signal boundary; retain X09 as the shared invariant until a distinct fixture earns coverage.

**Required before reconsideration:** Use a real signal-capable process and deterministic observed boundary; a timeout or invalid argument is not cancellation. Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence.
