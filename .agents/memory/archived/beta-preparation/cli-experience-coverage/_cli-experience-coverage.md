---
open-forge:
  description: Static audit of existing tests against all 26 flows and 438 scenario identities
  tags: [Memory, CLI, Testing, Evidence, Contextual, Archived, Historical]
---

# CLI Experience Coverage Review

Audited on **2026-09-19** in branch `codex/extensions-experience-review`, checkpoint `7b4745f7`, including the uncommitted reviewed experience corpus. **None of the 26 complete reviewed flows is established by the existing suite.** There is substantial command-level and shorter-sequence evidence; it is worth reusing and must not be mistaken for complete journey coverage.

The C# E2E suite has **112 methods in 35 classes: 100 Facts plus 12 Theories containing 63 rows, or 163 declared executions**. This audit read test bodies and relevant fixture/harness code. It did not execute tests, measure line/branch coverage, modify tests or CLI behavior, or approve the proposed outcomes. The earlier manual runs remain [separate evidence](../cli-experience-run.md).

## Scenario Accounting

Each of the 438 supplied identities appears once in the detailed matrices. Selection and evidence are independent: 399 retained/improved scenarios are under review, 38 are deferred and one is omitted. “Situation” in their body is simply the scenario title field.

| E2E evidence for the complete individual scenario | Selected 399 | All 438 identities | Meaning |
| --- | ---: | ---: | --- |
| direct | 9 | 9 | Asserts the reviewed individual outcome and meaningful state; does not imply its containing flow. |
| partial | 115 | 115 | Exercises that same situation but omits a material outcome, state, communication or setup check. |
| adjacent | 60 | 60 | Tests a related incident, different setup, different command or narrower boundary. |
| opposite | 7 | 7 | Explicitly asserts behavior incompatible with the reviewed target. |
| none | 208 | 247 | No corresponding published-process assertion identified; lower-tier evidence may still be listed. |

These are conservative, overlapping-domain **scenario classifications**, not a product coverage percentage. A single method can inform several cases; multiple unrelated tests do not automatically combine into one fully covered case. Essential communication and fixture provenance count. Deferred/omitted rows are accounting only, not demands to implement them.

## Most Useful Existing Evidence

- Real preview/apply/repeat sequences exist for several mutations, including install, route creation/init/update, extension update/removal and Library sync preview→apply.
- Published-process tests check stream/exit boundaries, semantic JSON, exact file bytes, selected navigation edits, ownership/link facts and no-op preservation.
- The shared schema theory covers all 28 command bindings in both formats at four explicit detail levels, **on unavailable-workspace outcomes**. Healthy Route List separately compares full/debug JSON.
- Library tests use real symlinks and validate important preservation boundaries, but most start from manually seeded registrations.
- Integration tests supply additional real locking and operation-fault evidence. They are identified separately and never counted as full published-process journeys.

## Expectations That Need Reconciliation

Existing tests should not silently be repurposed as proof of the proposed forgiving behavior. These E2E scenarios explicitly differ:

- [X15](cross-command-scenarios.md#x15) — Keep a copyable next action complete and relevant: Some missing-input guidance is useful, but Extension Inspect explicitly requires a suggested update that omits its custom source and workspace; it is not a complete identity-preserving replay command. Missing optional metadata also triggers a required correction in Route Create.
- [X18](cross-command-scenarios.md#x18) — Preserve a changed Library destination while continuing safe work: Reviewed target requires preserving the changed user file while applying independent safe sync/detach work and releasing appropriate claims.
- [X19](cross-command-scenarios.md#x19) — Make a second identical operation genuinely a no-op: Useful no-op sequences exist for other commands, but Route Remove explicitly rejects the already-absent category after successful removal, contrary to the reviewed harmless repeated-removal target. Library repeated-absence detach remains untested.
- [C14-05](authoring-scenarios.md#c14-05) — Missing description: The current EndToEnd test asserts missing description is invalid and required, opposite the reviewed optional-metadata target.
- [C17-05](authoring-scenarios.md#c17-05) — Source not found: EndToEnd repeat asserts exit 4/source-not-found for verified absence, opposite the revised harmless no-op target.
- [C27-07](library-scenarios.md#c27-07) — Changed occupant: Explicit whole-sync block and no independent addition contradict continuation.
- [C28-06](library-scenarios.md#c28-06) — Changed occupant: Whole registration and other links retained, contrary to independent detach.

Lower-tier contradictions are also recorded: Index folder operands are rejected (C05-05), damaged selected-bucket Cleanup candidates block planning (C07-04), References outgoing-only with an incoming include filter is invalid (C10-09), and missing intermediate Route Create parents block planning (C14-09). These are Integration expectations, not E2E coverage or a new runtime observation.

## Gaps To Prioritize After Flow Validation

1. **Everyday permissive authoring:** native Skill import, plain/partial-metadata notes, deterministic nested creation, optional metadata enrichment and harmless repeats (F02–F05, F10, F22).
2. **Useful independent continuation:** unrelated corruption, changed/absent Library destinations and damaged cleanup candidates (F15, F17–F18, F23). Preserve user data while checking which independent effects can proceed.
3. **Complete ordinary lifecycles:** first adoption, search→read, rename/repair, move, dependency packages, custom-source packages and selective extensions (F01, F07–F13, F16, F26).
4. **Genuine hard boundaries:** terminal once/always consent, revoked scope, two live writers, after-effect failure, cancellation and ownership-publication failure (F14, F19–F20). Use real synchronized boundaries rather than artificial status injection.
5. **Trustworthy communication and content:** explicit workspace choice, default warning usefulness, full view comparisons, literal authored bytes and overwrites (F06, F21, F24–F25). Use named snapshots for whole reports and semantic assertions for independent state.

No new tests are implemented here. The pending maintainer flow review still gates that work.

## Entries

- [Individual scenario coverage with exact existing evidence and missing outcomes](authoring-scenarios.md) - #Memory #CLI #Testing #Evidence #Contextual #Archived #Historical
- [Individual scenario coverage with exact existing evidence and missing outcomes](core-scenarios.md) - #Memory #CLI #Testing #Evidence #Contextual #Archived #Historical
- [Individual scenario coverage with exact existing evidence and missing outcomes](cross-command-scenarios.md) - #Memory #CLI #Testing #Evidence #Contextual #Archived #Historical
- [Fixture setup that supports existing tests and branches that remain unexercised](fixtures.md) - #Memory #CLI #Testing #Evidence #Contextual #Archived #Historical
- [Flow summaries and every main-path step mapped to existing scenario evidence](flows.md) - #Memory #CLI #Testing #Evidence #Contextual #Archived #Historical
- [What the existing published-process and package tests can actually prove](harness.md) - #Memory #CLI #Testing #Evidence #Contextual #Archived #Historical
- [Individual scenario coverage with exact existing evidence and missing outcomes](library-scenarios.md) - #Memory #CLI #Testing #Evidence #Contextual #Archived #Historical
- [All 112 published-process methods with what they assert and what they leave open](tests.md) - #Memory #CLI #Testing #Evidence #Contextual #Archived #Historical
