---
open-forge:
  description: "extension list: scenario collection"
  tags: [Memory, Document, CLI, Scenario, Evergreen]
---

# extension list: scenario collection

These are reviewed user outcomes. Selection does not mean execution passed. Improved targets remain subject to flow validation before new tests. Deferred cases retain only their identity and reason; they are not adopted specifications.

**Who:** A maintainer using Open Forge through the public CLI, without knowing its implementation or internal state model.

**Goal:** See what is installed and what is available, without confusing missing evidence with an empty catalogue.

Existing baseline: [Interface](../../../contracts/extension/list/interface.md), [Behavior](../../../contracts/extension/list/behavior.md). Exact interface and output differences must be reconciled before implementation.

See [scenario conventions](../_scenarios.md) and the [complete assessment](../../assessment.md).

## C18-01

**Situation:** None installed

**Disposition:** Added. Worth retaining as an independently checked user outcome: Distinguish Installed none from the available packages and describe available choices.

### Starting point

Use W1 with a known empty Extension ownership section and the pinned readable catalogue.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension list
```

### Expected result

Distinguish Installed none from the available packages and describe available choices.



### Verification

Compare installed and available sets independently; empty installed is not unknown ownership.

## C18-02

**Situation:** One installed

**Disposition:** Added. Worth retaining as an independently checked user outcome: List toolkit in installed and available contexts with the correct matching marker.

### Starting point

toolkit is installed with verified receipts and the matching pinned source is readable.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension list
```

### Expected result

List toolkit in installed and available contexts with the correct matching marker.



### Verification

Verify one row per package per block; this deliberate appearance in two blocks is not a duplicate defect.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C18-03

**Situation:** Installed only

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show only the installed selection without pretending available packages were absent.

### Starting point

Installed receipts are readable; the catalogue contains additional uninstalled packages.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension list --installed
```

### Expected result

Show only the installed selection without pretending available packages were absent.



### Verification

Compare requested sections and ensure availability enumeration does not leak into a hidden second list.

## C18-04

**Situation:** Available only

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show the available catalogue with useful descriptions.

### Starting point

A readable catalogue contains installed and uninstalled packages.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension list --available
```

### Expected result

Show the available catalogue with useful descriptions.



### Verification

Compare package identities, descriptive versions and dependency bundle sizes to the pinned manifests.

## C18-05

**Situation:** Explicit source

**Disposition:** Added. Worth retaining as an independently checked user outcome: Identify the selected source and list its packages, not the bundled defaults.

### Starting point

CAT is a readable external catalogue with package versions distinct from bundled ones.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension list --source "$CAT"
```

### Expected result

Identify the selected source and list its packages, not the bundled defaults.



### Verification

Verify actual source identity and displayed versions; custom-source continuation must retain the same source.

## C18-06

**Situation:** No ownership record

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain that installed packages cannot be established while still listing known available packages.

### Starting point

Remove only the ownership lock; leave the available catalogue readable.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension list
```

### Expected result

Explain that installed packages cannot be established while still listing known available packages.



### Verification

Verify no ownership reconstruction and no Installed none claim for unavailable ownership.

## C18-07

**Situation:** Source unreadable

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say available packages could not be listed from that source, preserving any independently known installed rows.

### Starting point

Explicit CAT exists but cannot be read by the invoking account.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension list --source "$CAT"
```

### Expected result

Say available packages could not be listed from that source, preserving any independently known installed rows.



### Verification

Confirm read denial and no fallback to bundled packages without explicit disclosure.

**Execution constraint:** Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure.

## C18-08

**Situation:** Installed source missing

**Disposition:** Added. Worth retaining as an independently checked user outcome: Keep the installed identity and say its recorded source is missing.

### Starting point

A valid installed receipt names a source directory that has been removed.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension list
```

### Expected result

Keep the installed identity and say its recorded source is missing.



### Verification

Verify installed content is not declared absent merely because its source disappeared.

## C18-09

**Situation:** Installed source unavailable

**Disposition:** Added. Worth retaining as an independently checked user outcome: Distinguish an unavailable source from a missing one and retain known installed facts.

### Starting point

The recorded source exists but access is denied; installed claims remain readable.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension list
```

### Expected result

Distinguish an unavailable source from a missing one and retain known installed facts.



### Verification

Verify the actual access state. The output Situations annotations explicitly classify this subtype as incomplete; do not collapse it into source-missing.

**Execution constraint:** Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure.

## C18-10

**Situation:** Installed source invalid

**Disposition:** Added. Worth retaining as an independently checked user outcome: Name the invalid manifest cause without describing it as an unreadable file.

### Starting point

The recorded source is readable but its manifest is malformed or lacks required fields.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension list
```

### Expected result

Name the invalid manifest cause without describing it as an unreadable file.



### Verification

Verify readable malformed bytes and preserve installed claims. The output Situations annotations explicitly classify this subtype as incomplete; do not collapse it into source-missing.

## C18-11

**Situation:** Installed source blocked

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the unsafe source boundary, without following it or presenting it as ordinary absence.

### Starting point

A recorded source now resolves through an unsafe or forbidden boundary.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension list
```

### Expected result

Explain the unsafe source boundary, without following it or presenting it as ordinary absence.



### Verification

Inspect links without following them and verify no external scan.

## C18-12

**Situation:** Installed files changed

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say the installed file changed and point to inspection of toolkit.

### Starting point

One file owned by toolkit differs semantically from the currently intended source.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension list
```

### Expected result

Say the installed file changed and point to inspection of toolkit.



### Verification

Compare current intended source and disk bytes; do not rely on a stale recorded baseline hash.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C18-13

**Situation:** Installed files missing

**Disposition:** Added. Worth retaining as an independently checked user outcome: Name the missing installed file without calling the whole package uninstalled.

### Starting point

One valid owned toolkit path is absent, while its source is readable.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension list
```

### Expected result

Name the missing installed file without calling the whole package uninstalled.



### Verification

Compare exact claimed paths to directory entries and preserve distinct changed/missing counts.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C18-14

**Situation:** Installed target blocked

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain that the target cannot be checked safely and do not follow it.

### Starting point

A claimed target has unsafe linked ancestry or another prohibited identity boundary.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension list
```

### Expected result

Explain that the target cannot be checked safely and do not follow it.



### Verification

Verify no unsafe target read and no fabricated file comparison.

## C18-15

**Situation:** Installed target unavailable

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say the installed target could not be read completely and preserve known package identity.

### Starting point

A claimed target is ordinary and safely located but genuinely unreadable.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension list
```

### Expected result

Say the installed target could not be read completely and preserve known package identity.



### Verification

Distinguish unavailable comparison from a changed-file result.

**Execution constraint:** Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure.

## C18-16

**Situation:** Installed files unavailable

**Disposition:** Not added. This abstract unavailable-comparison umbrella adds no distinct state beyond C18-09, C18-15 and X28; retain those concrete source/target coverage cases.

## C18-17

**Situation:** Source invalid

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the exact invalid source shape and required manifest information.

### Starting point

Explicit CAT is readable but is neither a valid package nor a valid package folder.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension list --source "$CAT_INVALID"
```

### Expected result

Explain the exact invalid source shape and required manifest information.



### Verification

Verify no fallback catalogue and no attempt to repair or invent the manifest.

## C18-18

**Situation:** Source blocked

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the source boundary and stop safely.

### Starting point

Explicit source overlaps a forbidden workspace or unsafe path boundary.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension list --source "$BLOCKED_CAT"
```

### Expected result

Explain the source boundary and stop safely.



### Verification

Verify no traversal into the forbidden path and no mutation.

## C18-19

**Situation:** Invalid input

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the invalid command form.

### Starting point

Pass an unsupported positional operand to this operand-free command.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension list toolkit
```

### Expected result

Explain the invalid command form.



### Verification

Check parser classification and no automatic interpretation as inspect or install.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C18-S07

**Situation:** Outcome 07

**Disposition:** Deferred; not selected yet. No concrete unexpected failure mechanism or reproducible fixture is supplied; a generic outcome row adds no executable user scenario yet.

**Required before reconsideration:** Supply a plausible public/OS failure mechanism and independently verified fixture; do not inject arbitrary private result states.

## C18-S08

**Situation:** Outcome 08

**Disposition:** Deferred; not selected yet. Repeated cancellation outcome without a concrete signal boundary; retain X09 as the shared invariant until a distinct fixture earns coverage.

**Required before reconsideration:** Use a real signal-capable process and deterministic observed boundary; a timeout or invalid argument is not cancellation. Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence.
