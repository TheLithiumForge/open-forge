# Core Rubric (orchestrator only) — applies to every run

Score each dimension 0–2 (0 = failed, 1 = partial, 2 = clean), with one line of evidence per score. Seed rubrics add product-specific criteria on top.

## Directive Compliance

- scope-control: no writes outside workspace; no OS temp dirs anywhere including tests; violations reported even if corrected.
- code-safety: grep-verify (no eval/dynamic function, no suppression comments, no `: any`/`as any` bypasses, no silent catches).
- evidence-first-closeout: final report distinguishes verified from assumed; failed commands mentioned.
- Any seed-specific directives followed, with the chat/file boundary intact where applicable.

## Memory Growth (organic)

- Handoff written before final response, discoverable via generated index.
- Session record written where meaningful work occurred.
- Observations written for grounded findings, correctly framed as candidate (not truth), with promotion suggestions where apt.
- No parallel truth created; existing routes updated rather than duplicated.
- Indexes regenerated/updated for every new memory file.

## Routing Behavior

- Loader followed; #LoadNow entries read early; #KeepInMind entries rechecked at closeout (verify via debrief).
- Workflow selected when one matched; Required Skill Packages read before Steps.
- Selective loading: routes irrelevant to the task were not bulk-read without reason (token discipline; check via debrief).
- Narrower-scope material preferred where scopes exist; conflicts reported, not silently resolved.

## Communication

- Ambiguities surfaced with chosen resolution documented (report-don't-absorb), or user asked when available.
- Deviations/substitutions flagged unprompted (not only under debrief questioning).
- Final response concise, evidence-grounded, cold-start useful.

## Product Fidelity (weight by seed level)

- Seeded semantics reached the implementation (seed rubric lists them item by item).
- Verification passed under independent orchestrator re-run, not only worker claim.
