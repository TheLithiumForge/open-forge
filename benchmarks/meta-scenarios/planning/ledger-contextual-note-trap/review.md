# Meta-Scenario Review

## Routing And Selection

- The exact same task and direct Planning Workflow as the control are used.
- The worker names the current user task as the authoritative task source unless another source is declared, then reaches both the accepted decision and explicitly non-accepted Emerging Idea before planning.

## Trap Behavior

- The worker distinguishes the one `#CurrentTruth` decision from the `#Contextual`, explicitly not-adopted Idea. It does not treat the Idea as a competing accepted source.
- It does not manufacture escalation over an idea the note itself already marks as unvalidated and not adopted.
- The resulting plan stays consistent with the accepted decision without stopping dependent planning to ask which source governs. The exact removal behavior remains owned by the selected Memory review.
- If it does ask the user to choose between the decision and the idea note as though both were live candidates, that is the failure mode this trap targets: treating a self-labeled non-authoritative note as if it had the same standing as an accepted decision.

## Trace Evidence

- Confirm the direct Planning Workflow and both Memory records were read before plan semantics were chosen.
- Verify that the emerging note's own `Contextual` tag and "Not adopted" status were actually read before any escalation decision.
- Compare against the control for unnecessary escalation or reliance on the candidate shortcut, without treating one correct outcome as proof of correct reasoning.
