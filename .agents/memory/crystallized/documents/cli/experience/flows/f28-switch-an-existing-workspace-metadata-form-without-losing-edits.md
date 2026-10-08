---
open-forge:
  description: Switch an existing workspace's metadata form without losing edits during conversion
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F28: Switch an existing workspace's metadata form without losing edits

**Who:** A maintainer changing an earlier scoped installation who needs Configure to preserve local edits and user files.

**Execution:** This flow is implemented by the published journey class `F28SwitchFrontmatterFormJourneyTests` under `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/Journeys/`. This definition does not record a passing run.

## Starting point

An installed scoped workspace from an earlier release. Simulate it with `open-forge install --frontmatter scoped --automatic`, then remove only the `frontmatter` property from `.agents/open-forge.json`. The missing key still means scoped.

Install the bundled Collaboration Extension, create a scoped user note with Route Create, and append a distinctive paragraph to owned `.agents/patterns/_patterns.md`. Record the edited file and user note bytes before the flow. Canonical Framework and Extension sources remain available.

## Flow

Run the steps on the actual resulting state. A linked scenario supplies a verification boundary, not permission to reset the workspace between steps.

| Step | Action | Scenario | State passed forward |
| ---: | --- | --- | --- |
| 1 | open-forge status | [C01-13](../scenarios/commands/c01-status.md#c01-13) | The absent setting resolves to scoped, and Status identifies the appended owned edit without inventing a form mismatch. |
| 2 | open-forge install --frontmatter root --automatic | [C03-16](../scenarios/commands/c03-install.md#c03-16) | Invalid input explains that `--configure` is required, with content and settings unchanged. |
| 3 | open-forge install --configure --frontmatter root --dry-run | [C03-17](../scenarios/commands/c03-install.md#c03-17) | The preview shows the form transition, settings, eligible replacements, and kept edited file without writes. |
| 4 | open-forge install --configure --frontmatter root --automatic | [C03-18](../scenarios/commands/c03-install.md#c03-18) | Settings record root, eligible unedited owned files convert, and the edited file and user note remain unchanged. |
| 5 | Check converted Framework and Extension files against the payload rendered to root, and that the edited file and the user note are byte-identical. | [X31](../scenarios/experience.md#x31), [C03-18](../scenarios/commands/c03-install.md#c03-18) | Independent comparisons prove conversion and preservation, including projected navigation. |
| 6 | open-forge index | [C05-12](../scenarios/commands/c05-index.md#c05-12) | Navigation reads both forms while retained user content stays in its authored form. |
| 7 | open-forge status | [C01-13](../scenarios/commands/c01-status.md#c01-13) | The kept owned scoped file differs from the root-form intended payload and receives Update advice. |
| 8 | open-forge update --automatic | [C04-12](../scenarios/commands/c04-update.md#c04-12) | Explicit Update reconciles owned Framework differences to root, including the kept edited file, while preserving the user note. |
| 9 | open-forge install --configure --frontmatter scoped --automatic, then check canonical scoped bytes are restored exactly. | [C03-18](../scenarios/commands/c03-install.md#c03-18), [X31](../scenarios/experience.md#x31) | Eligible unedited Framework and Extension targets return to canonical scoped bytes with projected navigation, and settings record scoped. |
| 10 | Repeat step 9. | [C03-18](../scenarios/commands/c03-install.md#c03-18) | Identical scoped configuration is a verified no-op with no settings or content rewrite. |

## Alternatives and recovery

### Preference edited by hand

Edit the saved form preference by hand, then run Update to reconcile owned Framework files with that form. Update never asks for the form or writes the settings key. Check the same independent rendered-byte expectation and preserve files outside its scope.

Scenarios: [C04-12](../scenarios/commands/c04-update.md#c04-12).

## Final result

Configure preserves the edited owned file and user note during the form change. The later explicit Update replaces the owned edit with the intended Framework payload under its ordinary reconciliation contract. The final workspace records scoped, eligible unedited delivery matches scoped sources, the user note remains unchanged, and repeated configuration is a no-op.

## Verification

Check settings, ownership, rendered payload, generated navigation, and actual file bytes independently of reports. Preserve the pre-conversion copies through step 5 to prove retention, then record the distinct replacement authorized by Update at step 8. Verify the user note across the whole flow and compare scoped return bytes and the repeated no-op. Record actions, stdout, stderr, exit, and separate outcome, state, and communication verdicts. Carry actual state between all 10 steps and fork only for the hand-edited preference alternative.
