---
open-forge:
  description: "extension install: scenario collection"
  tags: [Memory, Document, CLI, Scenario, Evergreen]
---

# extension install: scenario collection

These are reviewed user outcomes. Selection does not mean execution passed. Improved targets remain subject to flow validation before new tests. Deferred cases retain only their identity and reason; they are not adopted specifications.

**Who:** A maintainer using Open Forge through the public CLI, without knowing its implementation or internal state model.

**Goal:** Add a chosen package and its dependencies, with clear consent and no surprise overwrites.

Existing baseline: [Interface](../../../contracts/extension/install/interface.md), [Behavior](../../../contracts/extension/install/behavior.md). Exact interface and output differences must be reconciled before implementation.

See [scenario conventions](../_scenarios.md) and the [complete assessment](../../assessment.md).

## C21-01

**Situation:** Single package

**Disposition:** Added. Worth retaining as an independently checked user outcome: Install the selected package and name actual effects, without an unexplained success dump.

### Starting point

toolkit is a valid package with no dependency and only unoccupied .agents destinations.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension install toolkit --source "$CAT" --automatic
```

### Expected result

Install the selected package and name actual effects, without an unexplained success dump.



### Verification

Compare the manifest's intended paths and bytes with disk; unrelated files and source package remain unchanged.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C21-02

**Situation:** With dependencies

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the selected package and required dependency and install each once.

### Starting point

toolkit depends on base; both are valid, uniquely resolved and not installed.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension install toolkit --source "$CAT" --automatic
```

### Expected result

Explain the selected package and required dependency and install each once.



### Verification

Verify dependency closure, topological application requirements, distinct installed IDs and every target file.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C21-03

**Situation:** Select from source prompt

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show meaningful choices, honor the selected package and confirm the concrete plan.

### Starting point

CAT contains several packages; use a real terminal and choose toolkit from the displayed list.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension install --source "$CAT"
```

### Expected result

Show meaningful choices, honor the selected package and confirm the concrete plan.



### Verification

Record selection and verify no unselected package was installed except explicit dependencies.

**Execution constraint:** Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence. Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C21-04

**Situation:** No selection non interactive

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain that a selection is required; do not interpret omission as all packages.

### Starting point

Redirect the session and supply neither IDs nor --all.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension install --source "$CAT"
```

### Expected result

Explain that a selection is required; do not interpret omission as all packages.



### Verification

Verify bounded completion, no prompt hang and zero effects.

## C21-05

**Situation:** Permission required non interactive

**Disposition:** Added. Worth retaining as an independently checked user outcome: Name the uncovered destination and required consent, then stop without content effects.

### Starting point

toolkit has an external docs/team.md destination with no saved grant; .agents content is otherwise valid.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension install toolkit --source "$CAT" --automatic
```

### Expected result

Name the uncovered destination and required consent, then stop without content effects.



### Verification

Verify --automatic did not grant permission; settings and all target bytes remain unchanged.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C21-06

**Situation:** Permission prompt always

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain that the scope includes future descendants, save only the chosen grant, and install the package.

### Starting point

Use the uncovered docs destination in a real terminal; choose always for the displayed exact scope and approve the plan.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension install toolkit --source "$CAT"
```

### Expected result

Explain that the scope includes future descendants, save only the chosen grant, and install the package.



### Verification

Inspect .agents/open-forge.json and actual saved scope; re-run consent checks on a fresh later request to verify persistence.

**Execution constraint:** Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence. Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C21-07

**Situation:** Permission prompt once

**Disposition:** Added. Worth retaining as an independently checked user outcome: Apply this approved operation without persisting a grant.

### Starting point

Use the same uncovered destination but choose once in a real terminal.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension install toolkit --source "$CAT"
```

### Expected result

Apply this approved operation without persisting a grant.



### Verification

Compare settings byte-for-byte; a later uncovered operation must still require permission.

**Execution constraint:** Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence. Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C21-08

**Situation:** Allow path flag

**Disposition:** Added. Worth retaining as an independently checked user outcome: Save the explicit permitted scope and install only eligible planned targets.

### Starting point

toolkit needs docs/team.md and docs is an eligible ungranted scope.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension install toolkit --source "$CAT" --allow-path docs --automatic
```

### Expected result

Save the explicit permitted scope and install only eligible planned targets.



### Verification

Inspect the saved grant and effects separately; a later failure must not erase a verified saved-grant receipt from the report.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C21-09

**Situation:** Existing file without force

**Disposition:** Added. Worth retaining as an independently checked user outcome: Name the existing file and explain why it was not replaced.

### Starting point

A selected ordinary target has unowned different content, with no other-owner or reserved-path issue.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension install toolkit --source "$CAT" --automatic
```

### Expected result

Name the existing file and explain why it was not replaced.



### Verification

Verify automatic confirmation is not replacement authority and every target remains unchanged.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C21-10

**Situation:** With force

**Disposition:** Added. Worth retaining as an independently checked user outcome: Replace only eligible conflicts and say which content was replaced.

### Starting point

Review the eligible unowned conflict and deliberately authorize replacement.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension install toolkit --source "$CAT" --force --automatic
```

### Expected result

Replace only eligible conflicts and say which content was replaced.



### Verification

Compare bytes and prior-content recovery; force must not bypass other owners, source trees or protected controls.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C21-11

**Situation:** Already installed

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say toolkit is already installed with nothing to do.

### Starting point

toolkit's selected membership and current intended content match the installed state.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension install toolkit --source "$CAT" --automatic
```

### Expected result

Say toolkit is already installed with nothing to do.



### Verification

Verify no repeated file writes, duplicate ownership claims or navigation churn.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C21-12

**Situation:** Changed since install

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the changed-content boundary and the appropriate update action instead of silently reinstalling over it.

### Starting point

An installed selected file now differs from the currently intended source, with valid membership.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension install toolkit --source "$CAT" --automatic
```

### Expected result

Explain the changed-content boundary and the appropriate update action instead of silently reinstalling over it.



### Verification

Compare current source and target, not only stored version metadata; preserve the changed file.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C21-13

**Situation:** No content directory

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain that there was no content to install, without pretending files were created.

### Starting point

toolkit has a readable valid manifest but no installable content directory.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension install toolkit --source "$CAT" --automatic
```

### Expected result

Explain that there was no content to install, without pretending files were created.



### Verification

Verify zero target effects and the documented warning status; do not fabricate an empty content directory.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C21-14

**Situation:** Dry run

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show the complete proposed package and permission effects without saving consent or creating content.

### Starting point

Use a valid dependency plan, optionally with an explicit eligible --allow-path scope.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension install toolkit --source "$CAT" --allow-path docs --dry-run
```

### Expected result

Show the complete proposed package and permission effects without saving consent or creating content.



### Verification

Compare workspace, settings, source catalogue and recovery inventory; dry-run never prompts.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C21-15

**Situation:** Source unreadable

**Disposition:** Added. Worth retaining as an independently checked user outcome: Name the unavailable source and do not install a partial dependency closure.

### Starting point

CAT exists but a required package source cannot be read completely.

Fixture: `PACKAGES`. controlled fixture required.

### Steps

```text
open-forge extension install toolkit --source "$CAT" --automatic
```

### Expected result

Name the unavailable source and do not install a partial dependency closure.



### Verification

Independently confirm read failure and verify zero package-target effects.

**Execution constraint:** Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure. Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C21-16

**Situation:** Lock held

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the active operation and make no content changes.

### Starting point

LOCK holds this workspace while the plan is otherwise eligible and consent is already satisfied.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension install toolkit --source "$CAT" --automatic
```

### Expected result

Explain the active operation and make no content changes.



### Verification

Verify real contention and ensure a standing grant does not bypass the workspace lock.

**Execution constraint:** Demonstrate live same-workspace lock ownership and release; a lock file alone is insufficient. Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C21-17

**Situation:** Write failed partial

**Disposition:** Added. Worth retaining as an independently checked user outcome: Report the installed portion and unfinished operation, with actual recovery and permission-save facts.

### Starting point

Fail after one verified content effect in a multi-file package.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension install toolkit --source "$CAT" --automatic
```

### Expected result

Report the installed portion and unfinished operation, with actual recovery and permission-save facts.



### Verification

Compare all effects and ownership publication separately; do not equate written files with a fully installed package.

**Execution constraint:** Establish a deterministic public process or real filesystem boundary and independently verify before/after effects; no timer-only or mocked-result proof. Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C21-18

**Situation:** Cancelled

**Disposition:** Added. Worth retaining as an independently checked user outcome: Report cancellation and preserve all target and settings bytes.

### Starting point

In a real terminal, cancel package or final-plan selection before any effect.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension install --source "$CAT"
```

### Expected result

Report cancellation and preserve all target and settings bytes.



### Verification

Record the actual cancellation stage; a denied permission choice and cancellation may have different status semantics.

**Execution constraint:** Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence.

## C21-S03

**Situation:** Outcome 03

**Disposition:** Added. Worth retaining as an independently checked user outcome: Install both selected packages once and report each selected identity and actual effects.

### Starting point

Two explicitly selected independent packages, toolkit and base, have valid unoccupied destinations and no uncovered permission.

Fixture: `PACKAGES`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension install toolkit base --source "$CAT" --automatic
```

### Expected result

Install both selected packages once and report each selected identity and actual effects.



### Verification

Compare selected IDs, dependency closure and distinct paths; do not double-count a package also reached as a dependency.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.
