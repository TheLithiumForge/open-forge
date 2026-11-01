---
open-forge:
  description: Layer 2 player clauses, equal delivery forms, prompt and declared open choices
  tags: [Memory, Document, CurrentTruth, Evaluation, NeonPairs, Layer2]
---
# Layer Two
Layer Two is the defined-idea case that gives the agent the player-facing Neon Pairs rules before it builds the game.

Status: case documents ready for setup and check implementation. Freeze their generated inputs before running this layer.

The task states how sizes, turns, moves, scores, controls, accessibility, appearance and delivery work. For example, a miss closes after 800 active milliseconds and the current size's best is shown. Board codes, snapshots, replay, face-ID calculations and exact SVG construction belong to Layer 3.

## Prompt

Send this short prompt before the supplied requirements:

> Can you build me Neon Pairs, a neon memory matching game for my phone and keyboard? Follow the supplied product requirements. Use React and TypeScript and give me one offline dist/index.html. Keep each component's logic, markup and styles together. If you need a choice I've left open, ask me.

Use the same task text in every setup. Only the registered delivery of the supplied requirements changes.

## Supplied clauses

Use spec version 2.2 and only clauses whose `firstLayer` is `L2` in `src/evals/benchmarks/neon-pairs/requirements.json`. That is 185 clauses across 59 requirement IDs. The following list freezes the exact subset. All means every clause of that requirement. A mixed requirement lists its supplied clauses:

| Requirement | Supplied clauses |
| --- | --- |
| R01 | R01.1, R01.2, R01.3 |
| R02 | All |
| R03 | All |
| R04 | R04.1, R04.2, R04.3, R04.4 |
| R05 | R05.1, R05.2, R05.3, R05.4, R05.9 |
| R06 | All |
| R07 | R07.1, R07.2, R07.3, R07.5, R07.6, R07.7, R07.8, R07.10, R07.11, R07.12 |
| R08 | R08.1, R08.2, R08.3, R08.4 |
| R09 | R09.1, R09.2, R09.3, R09.4 |
| R10 | All |
| R11 | R11.3, R11.4 |
| R12 | All |
| C01 | C01.1, C01.2 |
| C02 | C02.1, C02.2 |
| C03 | C03.1, C03.2 |
| C13 | C13.1, C13.3, C13.5 |
| I01 | All |
| I03 | All |
| I04 | All |
| I05 | All |
| I06 | All |
| I07 | All |
| I08 | All |
| I09 | All |
| I10 | All |
| I11 | All |
| I12 | I12.1 |
| I13 | All |
| I14 | All |
| I15 | All |
| I16 | I16.1, I16.2, I16.3, I16.4, I16.5, I16.6, I16.7, I16.8 |
| I17 | All |
| V01 | V01.1, V01.3 |
| V02 | All |
| V03 | All |
| V04 | All |
| V05 | All |
| V06 | All |
| V07 | All |
| V08 | All |
| V09 | All |
| V10 | V10.1, V10.3, V10.4, V10.5, V10.9 |
| V11 | All |
| V12 | V12.7 |
| V13 | All |
| V14 | All |
| V15 | All |
| V16 | All |
| V17 | All |
| V18 | All |
| V19 | All |
| V20 | All |
| S01 | All |
| S02 | S02.3 |
| T01 | T01.1, T01.2 |
| T07 | T07.4, T07.6, T07.9 |
| T08 | All |
| T09 | T09.2 |
| T10 | T10.2, T10.3, T10.4 |

Deliver each included clause's complete text and its defining heading. Include the table values and exact labels it needs. Resolve parent references to included clauses only. Use neutral local document links with the same descriptions in every form. Don't give the agent full source documents with omitted clauses still in them. Don't send layer plans, data maps, the decision log, trace entries or grading instructions.

The generated subset must carry only the same 185 clauses in each form. It reads as a checklist, with examples and exact labels in the clause text. Check its clause IDs and product sentences before scored runs. Unprovided rules can't become hidden correctness checks. Source locality stays in the prompt and in the existing judge result, while the stricter architecture and file-length rules wait for Layer 3.

## Delivery setups

Keep the preset and delivery form as separate axes. No setup, Minimal, Essentials and Full Core are the current preset comparison. Helpful and Extensions wait. Use these delivery forms for the defined-idea comparison:

| Form | How the agent gets the subset |
| --- | --- |
| Plain docs | No Open Forge. A neutral pointer sends it to the player-only `docs/README.md` and its linked documents. |
| Linked AGENTS.md | No Open Forge. Root AGENTS.md says to follow the same docs, with the same descriptions as the other forms. |
| Open Forge | Run Minimal, Essentials and Full Core as separate cells. Route the same player-only sentences through the installed files. |
| Spec in prompt | No Open Forge. Append the same product sentences to the prompt in document and clause order. |

The neutral pointer and all entry files give the same product requirements the same force. Every form has the same files needed to build, no starting game and no greenfield `applyTo` conditions. Keep metadata and setup names out of product sentences. Keep manifests outside the agent folder. Register the extra linked and inline controls separately from the preset comparison.

Run five attempts per registered cell, model and tool. Randomize the balanced order. A delivery-form result and a preset result answer different questions, so report their groups apart. The [Protocol](../protocol.md#fair-comparison) defines the equal-information checks.

## Open choices

Only these choices have answers beyond the provided rules:

| Choice | Answer |
| --- | --- |
| Supporting title copy | Just use Neon Pairs as the title. I don't need a subtitle. |
| Control arrangement | Put the size and game buttons above the board. They can wrap on my phone. |

These choices don't change timing, type sizes, spacing or required labels. The simulated user knows only the supplied subset and this bank. It can quote a supplied rule to answer a clarification. It never reveals a Layer 3 rule or a private preference. Every other open choice gets the shared fallback answer.

## Run and grade

Use a 60-minute limit for this case, including coding and tool work. Freeze files at completion or timeout and keep partial attempts. Register the fixed limit with the case before its first batch.

Keep Layer 1's 16 stated checks and add checks for every supplied clause. The proposal's roughly 48 checks is a planning estimate, not this clause count. One check can test several clauses, but each clause needs all its assertions to pass. Controlled clocks check the stated intervals. Find cards through play and accessible controls without an undocumented hook.

The developer provides the I17 tree evidence and tool record. Spoken output belongs to user acceptance. Report face-legibility reviews separately from executable outcomes. The [Grading](../grading.md) defines statuses, review uncertainty and counting.

Back to [Neon Pairs](_neon-pairs.md).
