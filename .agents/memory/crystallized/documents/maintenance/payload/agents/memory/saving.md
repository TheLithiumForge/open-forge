---
open-forge:
  description: Current maintenance contract for the optional progress-saving policy beside the Memory states
  responsibility: Preserve unfinished-work capture, Working dependency, shared placement and independent removal
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Working, Policy]
---

# Saving Policy Maintenance Contract

## Source

[`src/open-forge/.agents/memory/saving.md`](../../../../../../../../src/open-forge/.agents/memory/saving.md) is the optional installed progress-saving policy. It lives beside the state folders so Git can share it when Working records are private. This repository does not carry a dogfood copy.

The [Memory contract](_memory.md) defines durable capture and state placement. The [Working contract](working/_working.md) defines continuation records. The [lossless council](../../../../../../emerging/analysis/lossless-council/record.md#chair-decisions) records why the policy moved out of Working.

## Contract

- The policy carries #LoadNow, #Memory, #Working and #Policy and puts its two saving rules under a non-empty Instructions section.
- Memory lists it as a required leaf only when the policy is installed. It requires Working and is omitted when Working is left out. It must not return to the Git-ignored Working folder.
- Before unfinished work pauses or transfers, its Working record is updated or created with the goal, current state, next step and open questions.
- Capture stays limited to temporary state needed by a later session. Finished work needs no continuation record unless its result still matters.
- The opening explains how to stop automatic saving while retaining Working records: delete this file, remove its Memory entry and record the managed removal in exclusions before updating.
- Durable outcome integration stays in Memory's Axioms. Emerging candidate capture stays independent of this policy. Neither becomes a mandatory sealed Handoff.

## Verification

- Use the [payload integrity checks](../../_payload.md#verification) for the shipped policy and metadata rendering.
- Inspect its Memory row, Working dependency and sibling location. Review Install preset selection for omission without Working and shared Git visibility with private Working records.
- In an isolated fixture, remove the policy and its row, retain Working records and check the recorded exclusion survives Update. Compare its pause and transfer triggers with Working and Emerging so removing it does not disable candidate capture.
