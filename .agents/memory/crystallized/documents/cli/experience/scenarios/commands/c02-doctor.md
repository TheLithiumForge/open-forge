---
open-forge:
  description: "doctor: scenario collection"
  tags: [Memory, Document, CLI, Scenario, Evergreen]
---

# doctor: scenario collection

These are reviewed user outcomes. Selection does not mean execution passed. Improved targets remain subject to flow validation before new tests. Deferred cases retain only their identity and reason; they are not adopted specifications.

**Who:** A maintainer using Open Forge through the public CLI, without knowing its implementation or internal state model.

**Goal:** Understand real problems and their consequences, with an honest account of what was checked.

Current contracts: [Interface](../../../contracts/doctor/interface.md), [Behavior](../../../contracts/doctor/behavior.md). The contracts define exact current output and operation behavior.

See [scenario conventions](../_scenarios.md) and the [complete assessment](../../assessment.md).

## C02-01

**Situation:** Healthy

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say no problems were found.

### Starting point

W1 is independently checked and has no diagnostic issue.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge doctor
```

### Expected result

Say no problems were found. Do not append a no-files-changed mutation footer.

### Verification

Confirm all applicable diagnostic domains were inspected rather than skipped.

## C02-02

**Situation:** Info only

**Disposition:** Added. Worth retaining as an independently checked user outcome: Distinguish an informational observation from an error.

### Starting point

Remove only usable ownership claims; keep all ordinary content readable and valid.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge doctor
```

### Expected result

Distinguish an informational observation from an error. Never reconstruct ownership or rewrite the lock.

### Verification

Check informational severity, successful operation and complete observation coverage; retain unknown ownership distinctly.

## C02-03

**Situation:** Warnings only

**Disposition:** Improved and added. A default warning count without affected sources or a useful action leaves the person unable to act.

### Starting point

Seed the output fixture: two broken authored links, one with possible candidates, with no malformed source or unreadable domain.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge doctor
```

### Expected result

**Reviewed target:** Show a concise actionable warning summary in the default view, including the affected source and the useful next action. Richer views may expand occurrences and evidence without changing counts or diagnosis.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.

## C02-04

**Situation:** Error and warnings

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show the error's subject and action, with correct error and warning counts.

### Starting point

Seed the output fixture: one genuinely malformed frontmatter block plus independent broken authored links; all files remain readable.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge doctor
```

### Expected result

Show the error's subject and action, with correct error and warning counts. Diagnostic errors do not themselves mean the doctor process failed.

### Verification

Enumerate known defects independently and check the domain status remains completed-with-warnings when diagnosis finished.

## C02-05

**Situation:** Incomplete

**Disposition:** Added. Worth retaining as an independently checked user outcome: Identify checks that did not finish and retain findings from checks that did.

### Starting point

A source needed to inspect an installed Extension is genuinely unreadable; unrelated diagnostic domains remain testable.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge doctor
```

### Expected result

Identify checks that did not finish and retain findings from checks that did. Never say no problems found without qualification.

### Verification

Compare per-domain attempted, finished and unavailable coverage; do not add skipped routes to routes-checked totals.

**Execution constraint:** Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure.

## C02-06

**Situation:** Blocked workspace

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain why this workspace cannot be checked, without producing findings for another one.

### Starting point

Select an unsafe or nonexistent workspace explicitly.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge doctor --workspace "$MISSING_WS"
```

### Expected result

Explain why this workspace cannot be checked, without producing findings for another one.

### Verification

Verify no parent fallback, installation or repair occurred.

## C02-07

**Situation:** Invalid input

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the flag error and the valid help path.

### Starting point

W1 is valid; pass an unsupported flag.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge doctor --not-an-option
```

### Expected result

Explain the flag error and the valid help path. A parser failure need not use a domain report.

### Verification

Check no diagnosis or workspace effect occurred and no fake JSON envelope was printed.

## C02-08

**Situation:** Changed extension file

**Disposition:** Added. Worth retaining as an independently checked user outcome: Attribute the difference to the correct Extension and point to its inspection or update operation.

### Starting point

Change exactly one owned toolkit file; retain a readable pinned source for comparison.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge doctor
```

### Expected result

Attribute the difference to the correct Extension and point to its inspection or update operation.

### Verification

Verify the modified bytes remain; do not blame the Framework or invent missing source history.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C02-09

**Situation:** Stale entries

**Disposition:** Added. Worth retaining as an independently checked user outcome: Diagnose stale navigation, not a fabricated incomplete installation or update.

### Starting point

Add one valid route and leave only the parent's generated list stale.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge doctor
```

### Expected result

Diagnose stale navigation, not a fabricated incomplete installation or update.

### Verification

Inspect the actual list difference and verify doctor does not index automatically.

## C02-10

**Situation:** Library drift

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the missing Library link and its targeted action without silently recreating it.

### Starting point

In LIB, remove one registered destination link while all source inventories remain readable.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge doctor
```

### Expected result

Explain the missing Library link and its targeted action without silently recreating it.

### Verification

Verify the complete source inventory prerequisite before calling drift safe; a missing inventory must become a different scenario.

## C02-11

**Situation:** Recovery bundle

**Disposition:** Added. Worth retaining as an independently checked user outcome: Describe the item and its consequence, with cleanup preview rather than automatic deletion.

### Starting point

RECOVERY contains one verifiable retained item for this workspace.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge doctor
```

### Expected result

Describe the item and its consequence, with cleanup preview rather than automatic deletion.

### Verification

Confirm the item persists byte-for-byte and the diagnostic names the real recovery subject.

## C02-S08

**Situation:** Outcome 08

**Disposition:** Deferred; not selected yet. No concrete unexpected failure mechanism or reproducible fixture is supplied; a generic outcome row adds no executable user scenario yet.

**Required before reconsideration:** Supply a plausible public/OS failure mechanism and independently verified fixture; do not inject arbitrary private result states.

## C02-S09

**Situation:** Outcome 09

**Disposition:** Deferred; not selected yet. Repeated cancellation outcome without a concrete signal boundary; retain X09 as the shared invariant until a distinct fixture earns coverage.

**Required before reconsideration:** Use a real signal-capable process and deterministic observed boundary; a timeout or invalid argument is not cancellation. Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence.

## C02-12

**Situation:** Doctor on a root workspace finds no managed changes

**Disposition:** Added. Confirm that root delivery is healthy under the workspace's selected form.

### Starting point

W1 explicitly selects root and has independently verified Framework and installed Extension bytes. All applicable route, metadata, reference, ownership, and recovery inputs are readable and valid. User-owned root files may carry another tool's harmless root keys.

Fixture: `W1`, with current root-form content and no unrelated warning fixture.

### Steps

```text
open-forge doctor
```

### Expected result

Doctor compares the intended payload rendered to root and finds no managed-content changes. Root metadata uses the same validation as scoped metadata, and unrelated root keys are ignored. Applicable checks complete without a form warning, and Doctor performs no writes.

### Verification

**State:** Compare owned files with independently rendered canonical sources and retain the full pre-request tree. Verify that Doctor preserves content, settings, ownership, and recovery state.

**Output:** Check that applicable checks completed and no changed-target finding arose solely from root form. Keep informational reference observations separate from managed changes.

## Maintenance Review Boundaries

Doctor and Repair share current Entries identity records and occurrence-specific candidates. Protected sources and targets keep their managing or authoring actions. A Git timeout suppresses manual-removal advice for possibly renamed links and retries the complete request. Each next action keeps the selected workspace, and a clean Doctor prints no Next.

## C02-13

**Situation:** Missing metadata has an Index next step

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

Use [X02](../experience.md#x02) and its exact branch state. No fresh workspace is substituted between journey steps.

### Steps

1. Run `open-forge doctor` with the selected workspace.
2. For mutating commands preview first, then apply and run the printed continuation.

### Expected result

Follow the linked scenario and the command Interface's exact Task 79 text, finding codes and additive values. A complete result never hides an unproven target or skipped file.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state.

## C02-14

**Situation:** Deep missing chain has a working Index action

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

Use [X34](../experience.md#x34) and its exact branch state. No fresh workspace is substituted between journey steps.

### Steps

1. Run `open-forge doctor` with the selected workspace.
2. For mutating commands preview first, then apply and run the printed continuation.

### Expected result

Follow the linked scenario and the command Interface's exact Task 79 text, finding codes and additive values. A complete result never hides an unproven target or skipped file.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state.

## C02-15

**Situation:** Proven rename or move has an automatic Repair action

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

Use [X38](../experience.md#x38) and its exact branch state. No fresh workspace is substituted between journey steps.

### Steps

1. Run `open-forge doctor` with the selected workspace.
2. For mutating commands preview first, then apply and run the printed continuation.

### Expected result

Follow the linked scenario and the command Interface's exact Task 79 text, finding codes and additive values. A complete result never hides an unproven target or skipped file.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state.

## C02-16

**Situation:** Ambiguous or weak identity needs a choice

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

Use [X39](../experience.md#x39) and [X40](../experience.md#x40) for strong ambiguity and singleton weak evidence. No fresh workspace is substituted between journey steps.

### Steps

1. Run `open-forge doctor` with the selected workspace.
2. For mutating commands preview first, then apply and run the printed continuation.

### Expected result

Follow the linked scenario and the command Interface's exact Task 79 text, finding codes and additive values. A complete result never hides an unproven target or skipped file.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state.

## C02-17

**Situation:** No Git and already-indexed evidence limits stay truthful

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

Use [X41](../experience.md#x41) and [X42](../experience.md#x42), with and without stale identity. No fresh workspace is substituted between journey steps.

### Steps

1. Run `open-forge doctor` with the selected workspace.
2. For mutating commands preview first, then apply and run the printed continuation.

### Expected result

Follow the linked scenario and the command Interface's exact Task 79 text, finding codes and additive values. A complete result never hides an unproven target or skipped file.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state.

## C02-18

**Situation:** A mixed batch reaches clean Doctor by printed steps

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

Use [X47](../experience.md#x47) and its exact branch state. No fresh workspace is substituted between journey steps.

### Steps

1. Run `open-forge doctor` with the selected workspace.
2. For mutating commands preview first, then apply and run the printed continuation.

### Expected result

Follow the linked scenario and the command Interface's exact Task 79 text, finding codes and additive values. A complete result never hides an unproven target or skipped file.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state.

## C02-19

**Situation:** Malformed frontmatter and deletion need precise edits

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

Use [X44](../experience.md#x44) and [X45](../experience.md#x45) for malformed metadata and deleted targets. No fresh workspace is substituted between journey steps.

### Steps

1. Run `open-forge doctor` with the selected workspace.
2. For mutating commands preview first, then apply and run the printed continuation.

### Expected result

Follow the linked scenario and the command Interface's exact Task 79 text, finding codes and additive values. A complete result never hides an unproven target or skipped file.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state.

## C02-20

**Situation:** Unsafe or excluded routes never get futile Index advice

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

Use [X36](../experience.md#x36) and its exact branch state. No fresh workspace is substituted between journey steps.

### Steps

1. Run `open-forge doctor` with the selected workspace.
2. For mutating commands preview first, then apply and run the printed continuation.

### Expected result

Follow the linked scenario and the command Interface's exact Task 79 text, finding codes and additive values. A complete result never hides an unproven target or skipped file.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state.
