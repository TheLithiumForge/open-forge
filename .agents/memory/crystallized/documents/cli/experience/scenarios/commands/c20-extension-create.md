---
open-forge:
  description: "extension create: scenario collection"
  tags: [Memory, Document, CLI, Scenario, Evergreen]
---

# extension create: scenario collection

These are reviewed user outcomes. Selection does not mean execution passed. Improved targets remain subject to flow validation before new tests. Deferred cases retain only their identity and reason; they are not adopted specifications.

**Who:** A maintainer using Open Forge through the public CLI, without knowing its implementation or internal state model.

**Goal:** Create a package scaffold in the location I chose, without installing it into my project.

Existing baseline: [Interface](../../../contracts/extension/create/interface.md), [Behavior](../../../contracts/extension/create/behavior.md). Exact interface and output differences must be reconciled before implementation.

See [scenario conventions](../_scenarios.md) and the [complete assessment](../../assessment.md).

## C20-01

**Situation:** Created

**Disposition:** Added. Worth retaining as an independently checked user outcome: Create the manifest and content scaffold under CAT/toolkit and explain the actual location.

### Starting point

CAT is a writable ordinary catalogue parent; CAT/toolkit does not exist.

Fixture: `PACKAGE-SCAFFOLD`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension create toolkit --path "$CAT" --automatic
```

### Expected result

Create the manifest and content scaffold under CAT/toolkit and explain the actual location.



### Verification

Distinguish file effects from created directories; no package installation or workspace mutation.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C20-02

**Situation:** Created with metadata

**Disposition:** Added. Worth retaining as an independently checked user outcome: Preserve supplied descriptive metadata and dependency identity in the manifest.

### Starting point

The package destination is absent and all metadata inputs are valid.

Fixture: `PACKAGE-SCAFFOLD`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension create toolkit --path "$CAT" --name "Team toolkit" --description "Shared team guidance" --package-version=preview --dependency=base --automatic
```

### Expected result

Preserve supplied descriptive metadata and dependency identity in the manifest.



### Verification

Compare exact manifest fields; do not impose unrequested semantic-version validation or copy metadata from another package.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C20-03

**Situation:** Dry run

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show the exact package directory and files that would be created, with no writes.

### Starting point

Use the absent-package fixture.

Fixture: `PACKAGE-SCAFFOLD`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension create toolkit --path "$CAT" --dry-run
```

### Expected result

Show the exact package directory and files that would be created, with no writes.



### Verification

Verify CAT/toolkit remains absent and no workspace lock or installation content was created.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C20-04

**Situation:** Already present

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say the package scaffold is already present and nothing changed.

### Starting point

The exact requested supported scaffold already exists unchanged.

Fixture: `PACKAGE-SCAFFOLD`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension create toolkit --path "$CAT" --automatic
```

### Expected result

Say the package scaffold is already present and nothing changed.



### Verification

Compare manifest and content tree bytes; no duplicate nested toolkit/toolkit folder.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C20-05

**Situation:** Destination has other content

**Disposition:** Added. Worth retaining as an independently checked user outcome: Name the occupied destination and preserve it.

### Starting point

CAT/toolkit contains unrelated user content instead of an identical scaffold.

Fixture: `PACKAGE-SCAFFOLD`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension create toolkit --path "$CAT" --automatic
```

### Expected result

Name the occupied destination and preserve it.



### Verification

Verify no overwrite, cleanup or implicit adoption.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C20-06

**Situation:** Missing id non interactive

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain that the package ID is required without hanging on an unavailable prompt.

### Starting point

Run without an ID in a redirected session; CAT is otherwise valid.

Fixture: `PACKAGE-SCAFFOLD`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension create --path "$CAT"
```

### Expected result

Explain that the package ID is required without hanging on an unavailable prompt.



### Verification

Verify no guessed ID from folder names and no scaffold effects.

## C20-07

**Situation:** Prompted id and path

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show prompts whose folder meaning matches the created location, then create only after confirmation.

### Starting point

Use a genuine terminal with no ID or path supplied; provide toolkit and the actual catalogue parent when asked.

Fixture: `PACKAGE-SCAFFOLD`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension create
```

### Expected result

Show prompts whose folder meaning matches the created location, then create only after confirmation.



### Verification

Compare prompt wording with CAT/toolkit placement; a prompt labeled package folder must not silently mean its parent without explanation.

**Execution constraint:** Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence. Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C20-08

**Situation:** Invalid id

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the ID grammar and preserve the destination.

### Starting point

Supply Bad_ID, with an otherwise valid catalogue parent.

Fixture: `PACKAGE-SCAFFOLD`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension create Bad_ID --path "$CAT" --automatic
```

### Expected result

Explain the ID grammar and preserve the destination.



### Verification

Verify no automatic lowercasing or filename sanitization.

## C20-09

**Situation:** Catalogue unreadable

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain that the selected folder could not be read, not that it is empty.

### Starting point

CAT exists but cannot be safely enumerated by the invoking account.

Fixture: `PACKAGE-SCAFFOLD`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension create toolkit --path "$CAT" --automatic
```

### Expected result

Explain that the selected folder could not be read, not that it is empty.



### Verification

Confirm access denial and no partial scaffold.

**Execution constraint:** Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure. Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C20-10

**Situation:** Write failed partial

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say what was created and what remains unfinished; do not call a directory a completed file.

### Starting point

Fail after manifest creation but before completing the content scaffold.

Fixture: `PACKAGE-SCAFFOLD`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension create toolkit --path "$CAT" --automatic
```

### Expected result

Say what was created and what remains unfinished; do not call a directory a completed file.



### Verification

Compare actual path kinds and manifest bytes; a retry suggestion must retain toolkit and CAT instead of dropping required inputs.




**Execution constraint:** Establish a deterministic public process or real filesystem boundary and independently verify before/after effects; no timer-only or mocked-result proof. Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C20-11

**Situation:** Cancelled

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say package creation was cancelled without creating the destination.

### Starting point

In the real terminal wizard, cancel before final approval.

Fixture: `PACKAGE-SCAFFOLD`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension create
```

### Expected result

Say package creation was cancelled without creating the destination.



### Verification

Verify no manifest, content directory or workspace effect.

**Execution constraint:** Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence.

## C20-12

**Situation:** End of input before a required answer

**Disposition:** Added. Worth retaining as an independently checked user outcome: Return invalid-input with the missing fact, not a cancellation claim or a partially guessed request.

### Starting point

Use a real terminal session; the wizard requests an ID or catalogue parent, and input ends while that required fact is still missing. No operation was approved.

### Steps

```text
open-forge extension create
[end input before completing the required answers]
```

### Expected result

Return invalid-input with the missing fact, not a cancellation claim or a partially guessed request.



### Verification

Verify no package directory or manifest was created. This is not the Ctrl+C event in C20-11.

**Execution constraint:** Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence.

## C20-13

**Situation:** Descriptive metadata does not trigger dependency lookup

**Disposition:** Added. Worth retaining as an independently checked user outcome: Create the scaffold with the exact descriptive version and dependency declaration.

### Starting point

An ordinary empty catalogue parent exists. Use a descriptive version and a syntactically valid dependency ID that is unavailable in every installed catalogue.

### Steps

```text
open-forge extension create toolkit --path "$CAT" --package-version=preview --dependency=not-installed --automatic
```

### Expected result

Create the scaffold with the exact descriptive version and dependency declaration. Do not require package availability or SemVer.



### Verification

Verify exact manifest field values and that no dependency content or workspace ownership was written.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.
