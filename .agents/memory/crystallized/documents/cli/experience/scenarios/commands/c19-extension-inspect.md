---
open-forge:
  description: "extension inspect: scenario collection"
  tags: [Memory, Document, CLI, Scenario, Evergreen]
---

# extension inspect: scenario collection

These are reviewed user outcomes. Selection does not mean execution passed. Improved targets remain subject to flow validation before new tests. Deferred cases retain only their identity and reason; they are not adopted specifications.

**Who:** A maintainer using Open Forge through the public CLI, without knowing its implementation or internal state model.

**Goal:** Understand one package and the differences that matter before deciding to install, update or remove it.

Existing baseline: [Interface](../../../contracts/extension/inspect/interface.md), [Behavior](../../../contracts/extension/inspect/behavior.md). Exact interface and output differences must be reconciled before implementation.

See [scenario conventions](../_scenarios.md) and the [complete assessment](../../assessment.md).

## C19-01

**Situation:** Installed matches

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain that the installed package matches the available content, with useful file and dependency facts.

### Starting point

toolkit is installed and all selected files equal its pinned available source.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension inspect toolkit --source "$CAT"
```

### Expected result

Explain that the installed package matches the available content, with useful file and dependency facts.



### Verification

Compare current source content and target membership independently; a matching version string alone proves nothing.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C19-02

**Situation:** Installed changed and retired

**Disposition:** Added. Worth retaining as an independently checked user outcome: Distinguish changed current content from retired content and preserve both during inspection.

### Starting point

One installed file changed locally and another is retired from the selected available package.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension inspect toolkit --source "$CAT"
```

### Expected result

Distinguish changed current content from retired content and preserve both during inspection.



### Verification

Compare the two distinct sets; do not merge them into an unexplained generic difference count.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C19-03

**Situation:** Available not installed

**Disposition:** Added. Worth retaining as an independently checked user outcome: Describe the available package and clearly say it is not installed.

### Starting point

toolkit exists in CAT but has no installed claim in a readable lock.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension inspect toolkit --source "$CAT"
```

### Expected result

Describe the available package and clearly say it is not installed.



### Verification

Verify no install effects and no ownership inferred from coincidentally matching files.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C19-04

**Situation:** Installed source missing

**Disposition:** Added. Worth retaining as an independently checked user outcome: Preserve known installed identity and explain that full comparison could not finish.

### Starting point

toolkit has valid installed claims but the required comparison source is missing.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension inspect toolkit --source "$CAT"
```

### Expected result

Preserve known installed identity and explain that full comparison could not finish.



### Verification

Check missing source separately from missing installed files; do not claim the package is current.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C19-05

**Situation:** Newer available

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show installed and available versions and the actual content differences without assuming semantic-version ordering proves compatibility.

### Starting point

Use a verified package version pair with distinct descriptive version values and known file differences.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension inspect toolkit --source "$CAT"
```

### Expected result

Show installed and available versions and the actual content differences without assuming semantic-version ordering proves compatibility.



### Verification

Compare manifests and file sets; no package download or update occurs.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C19-06

**Situation:** Dependency cycle

**Disposition:** Improved and added. A dependency cycle prevents a complete dependency closure but should not erase readable package facts.

### Starting point

CAT contains a deliberate readable dependency cycle involving toolkit.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension inspect toolkit --source "$CAT"
```

### Expected result

**Reviewed target:** Identify the cycle and involved packages, preserve readable identity and manifest facts, and mark dependency comparison incomplete. Do not traverse indefinitely, invent a valid closure or mutate anything.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.


**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C19-07

**Situation:** Unknown id

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain that this ID is unknown in the selected source and installed set.

### Starting point

The explicit ID is absent from readable installed and selected available sets.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension inspect does-not-exist --source "$CAT"
```

### Expected result

Explain that this ID is unknown in the selected source and installed set.



### Verification

Verify no fuzzy match or interpretation as a filesystem path.

## C19-08

**Situation:** No ownership record

**Disposition:** Improved and added. Available-package information remains useful even when installed ownership is unknown.

### Starting point

The ownership lock supplies no usable claims, but toolkit remains available in CAT.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension inspect toolkit --source "$CAT"
```

### Expected result

**Reviewed target:** Describe the independently readable available package and its files/dependencies while explaining that installed state cannot be established. Do not label it installed or not installed from absent ownership, reconstruct claims or suppress safe available facts.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.


**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C19-09

**Situation:** Ambiguous source

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the ambiguous identity instead of selecting the first manifest.

### Starting point

The selected catalogue contains conflicting definitions for the same stable package ID.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension inspect toolkit --source "$CAT"
```

### Expected result

Explain the ambiguous identity instead of selecting the first manifest.



### Verification

Reverse discovery order and verify stable blocking meaning and exact conflicting subjects.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C19-10

**Situation:** Invalid input

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the invalid ID without silently lowercasing or renaming it.

### Starting point

Supply an ID that violates the documented lowercase, hyphen-separated grammar.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension inspect Bad_ID --source "$CAT"
```

### Expected result

Explain the invalid ID without silently lowercasing or renaming it.



### Verification

Verify no source mutation and no guessed identity.

## C19-S08

**Situation:** Outcome 08

**Disposition:** Deferred; not selected yet. No concrete unexpected failure mechanism or reproducible fixture is supplied; a generic outcome row adds no executable user scenario yet.

**Required before reconsideration:** Supply a plausible public/OS failure mechanism and independently verified fixture; do not inject arbitrary private result states. Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C19-S09

**Situation:** Outcome 09

**Disposition:** Deferred; not selected yet. Repeated cancellation outcome without a concrete signal boundary; retain X09 as the shared invariant until a distinct fixture earns coverage.

**Required before reconsideration:** Use a real signal-capable process and deterministic observed boundary; a timeout or invalid argument is not cancellation. Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence. Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.
