---
open-forge:
  description: Start a workspace in root form and keep working in it
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F27: Start a workspace in root form and keep working in it

**Who:** A first-time maintainer who wants root metadata and ordinary commands to work together.

**Execution:** This flow is implemented by the published journey class `F27RootFormJourneyTests` under `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/Journeys/`. This definition does not record a passing run.

## Starting point

W0, with a hand-authored README and no installation or declared frontmatter preference. Canonical Framework and bundled Extension sources are independently available for byte comparison.

## Flow

Run the steps on the actual resulting state. A linked scenario supplies a verification boundary, not permission to reset the workspace between steps.

| Step | Action | Scenario | State passed forward |
| ---: | --- | --- | --- |
| 1 | open-forge install --dry-run | [C03-02](../scenarios/commands/c03-install.md#c03-02), [C03-13](../scenarios/commands/c03-install.md#c03-13) | The preview selects root and shows intended delivery and settings, with every original byte preserved. |
| 2 | open-forge install --automatic | [C03-13](../scenarios/commands/c03-install.md#c03-13) | The Framework is delivered in root form and settings record `"frontmatter": "root"`. |
| 3 | Check every delivered file against the canonical payload rendered to root. | [X31](../scenarios/experience.md#x31) | An independent full delivery comparison verifies root bytes and unchanged ineligible assets. |
| 4 | open-forge status | [C01-02](../scenarios/commands/c01-status.md#c01-02) | Status describes the verified current root installation without changing it. |
| 5 | open-forge route create .agents/guidance/review-checklist.md --description "Review checklist for pull requests" --tag Guidance --tag Review | [C14-15](../scenarios/commands/c14-route-create.md#c14-15) | A user-owned checklist has root metadata and a projected parent entry. |
| 6 | Add another tool's root key, `sidebar_position: 3`, to that file by hand. | [C15-14](../scenarios/commands/c15-route-update.md#c15-14) | The recorded checklist bytes include the foreign key alongside root Open Forge metadata. |
| 7 | open-forge find --tag Review | [C09-13](../scenarios/commands/c09-find.md#c09-13) | Find returns the checklist from its root Open Forge tag without writing files. |
| 8 | open-forge context guidance/review-checklist | [C08-15](../scenarios/commands/c08-context.md#c08-15) | The admitted checklist and required context are readable, with authored content intact. |
| 9 | open-forge route update guidance/review-checklist --responsibility "Define the pull request review checklist" --tag Checklist | [C15-14](../scenarios/commands/c15-route-update.md#c15-14) | Root responsibility is set, tags are replaced with `[Checklist]`, and the foreign key and body survive. |
| 10 | open-forge extension install planning --automatic | [C21-19](../scenarios/commands/c21-extension-install.md#c21-19) | Planning and its declared dependencies are delivered in root form with verified ownership. |
| 11 | open-forge doctor | [C02-12](../scenarios/commands/c02-doctor.md#c02-12) | Applicable checks finish without managed changes caused by root form. |
| 12 | open-forge install --automatic, then open-forge index | [C03-03](../scenarios/commands/c03-install.md#c03-03), [C05-12](../scenarios/commands/c05-index.md#c05-12) | Repeated Install is a no-op, and Index confirms the resulting navigation without changing authored metadata. |

## Alternatives and recovery

### Interactive first installation

Run first Install in a prompt-capable terminal. Choose the preset, then the form with root preselected, review the complete plan, and confirm it. Verify prompt order and the resulting preference independently.

Scenarios: [C03-15](../scenarios/commands/c03-install.md#c03-15).

### Explicit scoped installation

Choose `--frontmatter scoped` on the first Install. Verify canonical scoped delivery and the recorded choice, then carry that real state through ordinary work.

Scenarios: [C03-14](../scenarios/commands/c03-install.md#c03-14).

### Preference declared before installation

Author a valid settings preference before first Install and omit the form option. Verify that Install honors the declaration rather than choosing the unattended default.

Scenarios: [C03-19](../scenarios/commands/c03-install.md#c03-19).

### File shared with another tool

Keep the other tool's root description and tags beside an explicit `open-forge` mapping. Index and Find use the scoped Open Forge values for that admitted source and preserve the other tool's fields.

Scenarios: [C05-12](../scenarios/commands/c05-index.md#c05-12), [C09-13](../scenarios/commands/c09-find.md#c09-13).

## Final result

The workspace records root form, delivers current Framework and Extension content in that form, and supports creation, discovery, reading, and authored-location editing. The README, user file body, and other tool's root key remain intact.

## Verification

Observe files, settings, ownership, and recovery state independently of printed output. Compare the entire canonical delivery with an independent root rendering and projected navigation. Record actions, stdout, stderr, exit, preserved bytes, and separate outcome, state, and communication verdicts. Carry actual state between all 12 steps and fork only for the four explicit alternatives.
