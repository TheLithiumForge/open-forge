---
open-forge:
  description: "extension update: scenario collection"
  tags: [Memory, Document, CLI, Scenario, Evergreen]
---

# extension update: scenario collection

These are reviewed user outcomes. Selection does not mean execution passed. Improved targets remain subject to flow validation before new tests. Deferred cases retain only their identity and reason; they are not adopted specifications.

**Who:** A maintainer using Open Forge through the public CLI, without knowing its implementation or internal state model.

**Goal:** Update selected installed packages while seeing replacements, additions and deliberately kept old files.

Existing baseline: [Interface](../../../contracts/extension/update/interface.md), [Behavior](../../../contracts/extension/update/behavior.md). Exact interface and output differences must be reconciled before implementation.

See [scenario conventions](../_scenarios.md) and the [complete assessment](../../assessment.md).

## C22-01

**Situation:** Up to date

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say the package is up to date and there is nothing to do.

### Starting point

toolkit's current intended files and installed membership already agree.

Fixture: `PACKAGES-INSTALLED`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension update toolkit --source "$CAT" --automatic
```

### Expected result

Say the package is up to date and there is nothing to do.



### Verification

Verify no target or lock churn and no gratuitous recovery bundle.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C22-02

**Situation:** Files replaced

**Disposition:** Added. Worth retaining as an independently checked user outcome: Name the replacement and update to the selected source, without claiming all local edits are preserved.

### Starting point

The current pinned source differs from one owned installed file.

Fixture: `PACKAGES-INSTALLED`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension update toolkit --source "$CAT" --automatic
```

### Expected result

Name the replacement and update to the selected source, without claiming all local edits are preserved.



### Verification

Compare current source, prior target bytes and actual replacement; inspect recovery before relying on it.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C22-03

**Situation:** New version with new files

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show the version change and new file without treating the version string alone as proof of correctness.

### Starting point

Use VERSION-PAIR: toolkit has a changed descriptive version and one new source member.

Fixture: `PACKAGES-INSTALLED`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension update toolkit --source "$CAT" --automatic
```

### Expected result

Show the version change and new file without treating the version string alone as proof of correctness.



### Verification

Compare intended membership and each added/replaced file; no unrelated package should be refreshed.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C22-04

**Situation:** Retired kept

**Disposition:** Added. Worth retaining as an independently checked user outcome: Keep it by default and explain that it remains from the earlier package version.

### Starting point

A previously installed toolkit file is absent from the current source and remains on disk.

Fixture: `PACKAGES-INSTALLED`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension update toolkit --source "$CAT" --automatic
```

### Expected result

Keep it by default and explain that it remains from the earlier package version.



### Verification

Verify exact retained bytes and separate kept paths from updated paths.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C22-05

**Situation:** Retired pruned

**Disposition:** Added. Worth retaining as an independently checked user outcome: Remove only eligible retired owned content and list the deletion.

### Starting point

Use the same valid retired-file fixture with explicit pruning approval.

Fixture: `PACKAGES-INSTALLED`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension update toolkit --source "$CAT" --prune --automatic
```

### Expected result

Remove only eligible retired owned content and list the deletion.



### Verification

Verify shared or unowned files survive and source content is untouched.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C22-06

**Situation:** All packages

**Disposition:** Added. Worth retaining as an independently checked user outcome: Update the intended installed selection, not every available package.

### Starting point

Two installed packages have known updates in CAT; a third available package is not installed.

Fixture: `PACKAGES-INSTALLED`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension update --all --source "$CAT" --automatic
```

### Expected result

Update the intended installed selection, not every available package.



### Verification

Compare selected installed IDs and dependency effects; no installation of the unrelated available package.

## C22-07

**Situation:** Select prompt

**Disposition:** Added. Worth retaining as an independently checked user outcome: Present installed choices clearly and honor the selected package and its required dependency scope.

### Starting point

Several installed packages exist; use a real terminal and choose only toolkit.

Fixture: `PACKAGES-INSTALLED`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension update --source "$CAT"
```

### Expected result

Present installed choices clearly and honor the selected package and its required dependency scope.



### Verification

Record choices and verify unselected independent packages remain byte-identical.

**Execution constraint:** Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence. Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C22-08

**Situation:** No selection non interactive

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the missing selection and do not default to all installed packages.

### Starting point

Redirect execution with no IDs or --all.

Fixture: `PACKAGES-INSTALLED`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension update --source "$CAT"
```

### Expected result

Explain the missing selection and do not default to all installed packages.



### Verification

Verify no prompt hang and no effects.

## C22-09

**Situation:** Permission required

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the revoked or absent destination permission before updating content.

### Starting point

An owned external destination is no longer covered by permission, although the ownership claim remains valid.

Fixture: `PACKAGES-INSTALLED`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension update toolkit --source "$CAT" --automatic
```

### Expected result

Explain the revoked or absent destination permission before updating content.



### Verification

Verify ownership is not treated as permanent permission and --automatic does not supply the grant.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C22-10

**Situation:** Ownership unknown

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain that ownership cannot be established and do not infer it from matching paths.

### Starting point

The selected package has no usable installed claim because ownership evidence is unavailable or uninterpretable.

Fixture: `PACKAGES-INSTALLED`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension update toolkit --source "$CAT" --automatic
```

### Expected result

Explain that ownership cannot be established and do not infer it from matching paths.



### Verification

Compare unknown versus known-not-installed behavior against the command’s specific ownership and output rules.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C22-11

**Situation:** Dry run

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show proposed effects in future tense with no writes or prompts.

### Starting point

A valid update includes one replacement and one addition.

Fixture: `PACKAGES-INSTALLED`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension update toolkit --source "$CAT" --dry-run
```

### Expected result

Show proposed effects in future tense with no writes or prompts.



### Verification

Compare targets, ownership lock, settings and external recovery inventory.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C22-12

**Situation:** Source unreadable

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the missing comparison input and leave all selected package targets unchanged.

### Starting point

A source required for the selected package or dependency cannot be read.

Fixture: `PACKAGES-INSTALLED`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension update toolkit --source "$CAT" --automatic
```

### Expected result

Explain the missing comparison input and leave all selected package targets unchanged.



### Verification

Confirm the failure is a required source read, not an unrelated obsolete record.

**Execution constraint:** Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure. Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C22-13

**Situation:** Lock held

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain contention and preserve content.

### Starting point

LOCK holds this workspace during an otherwise valid update.

Fixture: `PACKAGES-INSTALLED`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension update toolkit --source "$CAT" --automatic
```

### Expected result

Explain contention and preserve content.



### Verification

Verify a real live lock rather than merely a lock file; no partial update.

**Execution constraint:** Demonstrate live same-workspace lock ownership and release; a lock file alone is insufficient. Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C22-14

**Situation:** Write failed partial

**Disposition:** Added. Worth retaining as an independently checked user outcome: Report the completed portion, unfinished changes and actual recovery/permission facts.

### Starting point

Fail after one actual update effect in a multi-effect plan.

Fixture: `PACKAGES-INSTALLED`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension update toolkit --source "$CAT" --automatic
```

### Expected result

Report the completed portion, unfinished changes and actual recovery/permission facts.



### Verification

Compare receipts with disk, including kept retired files and ownership publication state.

**Execution constraint:** Establish a deterministic public process or real filesystem boundary and independently verify before/after effects; no timer-only or mocked-result proof. Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C22-15

**Situation:** Cancelled

**Disposition:** Added. Worth retaining as an independently checked user outcome: Report cancellation without replacing, adding or pruning anything.

### Starting point

Reject the final concrete update plan in a real terminal.

Fixture: `PACKAGES-INSTALLED`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension update toolkit --source "$CAT"
```

### Expected result

Report cancellation without replacing, adding or pruning anything.



### Verification

Verify exact preservation and record that cancellation preceded effects.

**Execution constraint:** Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence. Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.
