# Worker Self-Review Prompt

Send the exact prompt below to the same worker after task work has ended. The orchestrator must make this a response-only phase: no tools, file reads, commands, tests, edits, or corrective work. Save the complete response verbatim as `record/worker-review.md`.

```text
The task phase is over. Do not use tools, inspect files, run commands, test,
edit, or correct anything. Respond only with your review of the work you just
performed, based on what you currently believe happened.

Use these headings:

## Believed Outcome
What outcome do you believe you produced?

## Approach And Actions
What did you do, in what broad sequence, and why?

## Verification
What did you verify? What did you not verify?

## Assumptions And Deviations
What assumptions, substitutions, scope decisions, or deviations did you make?

## Remaining Uncertainty
What may be incomplete, incorrect, or worth checking independently?

Distinguish what you directly observed from what you inferred. Do not propose
or perform further work in this response.
```

The worker review records the worker's own account. It must not replace independent review of the interaction record or final workspace.
