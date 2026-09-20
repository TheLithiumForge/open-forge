---
open-forge:
  description: "Create a custom package, then install from that source"
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F12: Create a custom package, then install from that source

**Who:** A maintainer publishing local reusable guidance.

**Execution:** See the separate [run record](../../../../../working/cli-experience-run.md). Inclusion records a reviewed user outcome, not a passing implementation.

## Starting point

PACKAGE-SCAFFOLD with an external ordinary CAT directory and a separate verified consumer W1.

## Flow

Run the steps on the actual resulting state. A linked scenario supplies a verification boundary, not permission to reset the workspace between steps.

| Step | Action | Scenario | State passed forward |
| ---: | --- | --- | --- |
| 1 | open-forge extension create toolkit --path "$CAT" --name "Team toolkit" --description "Shared team guidance" --package-version=preview --automatic | [C20-02](../scenarios/commands/c20-extension-create.md#c20-02) | The scaffold exists at CAT/toolkit, not at a guessed nested or consumer location. |
| 2 | Author valid ordinary content under CAT/toolkit/content/.agents/guidance and keep its route requirements explicit. | [X27](../scenarios/experience.md#x27) | The package now has actual eligible content, not only a manifest. |
| 3 | open-forge extension list --source "$CAT" | [C18-05](../scenarios/commands/c18-extension-list.md#c18-05) | The custom package is listed from the intended catalogue. |
| 4 | open-forge extension inspect toolkit --source "$CAT" | [C19-03](../scenarios/commands/c19-extension-inspect.md#c19-03) | The consumer sees this exact source and its proposed content. |
| 5 | open-forge extension install toolkit --source "$CAT" --automatic | [C21-01](../scenarios/commands/c21-extension-install.md#c21-01) | The explicit custom package is installed into the consumer, leaving CAT unchanged. |

## Alternatives and recovery

### Scaffold has no content yet

Explain that nothing was installable; do not invent a success with files.

Scenarios: [C21-13](../scenarios/commands/c21-extension-install.md#c21-13).

### Partial scaffold creation

Report the actual manifest/directory effects; a recovery action must retain ID and catalogue selection.

Scenarios: [C20-10](../scenarios/commands/c20-extension-create.md#c20-10).

## Final result

The package source and consumer installation are separate and every continuation uses the selected source.

## Verification

Observe the resulting files and links independently of printed output. Record actions, stdout, stderr, exit, preserved bytes, and separate outcome/state/communication verdicts in a run record. Carry real state between steps; fork only at an explicit alternative.
