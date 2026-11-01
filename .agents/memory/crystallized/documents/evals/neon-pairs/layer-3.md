---
open-forge:
  description: Layer 3 full specification prompt, empty answer bank, setups and time limit
  tags: [Memory, Document, CurrentTruth, Evaluation, NeonPairs, Layer3]
---
# Layer Three
Layer Three is the full-spec case that asks the agent to implement every clause of Neon Pairs spec version 2.2.

Status: version 2.2 revision ready for the check batch. The 180-minute limit is provisional.

The task adds exact board generation, snapshots, replay and source rules to the complete player game. For example, a replay action at exactly 800ms runs after a mismatch due at that time has closed.

## Prompt

Send this prompt with the full product documents:

> Can you build Neon Pairs from the supplied product specification? Please implement the whole game, including its board codes, saved snapshots and replay command. Use the documented build and test commands and give me the offline dist/index.html. Keep the code arranged as the Architecture document says. If anything is unclear, ask me.

## Full pack

Supply the ten generated product documents in `src/evals/benchmarks/neon-pairs/specs/`, starting at `README.md`. Freeze spec version 2.2, all 96 requirement IDs and all 412 clauses. Keep file hashes with the case inputs.

Give the agent no layer plan, requirements map, trace map, private fixture, reference implementation or check definition. The public Acceptance journeys are part of the product pack. Product text must read as the user's requirements and contain no study instructions.

## Setups

Use these delivery forms with identical product sentences and descriptions:

| Form | Starting context |
| --- | --- |
| Plain docs | No Open Forge. A neutral pointer names docs/README.md and says to follow its requirements. |
| Linked AGENTS.md | No Open Forge. Root AGENTS.md links the same documents and says to follow their requirements. |
| Open Forge | Separate Minimal, Essentials and Full Core cells route the same requirements through the installed files. |

Keep the four current presets and document delivery as separate axes. Plain docs is the no-preset document control. Linked AGENTS.md is an added routing control. Helpful and Extension testing wait. Don't include the inline-spec form at this layer.

Run five attempts per registered cell, model and tool. Use a fresh setup and tool home, the same product instruction force and no greenfield `applyTo` conditions. Record setup content and hashes outside the agent folder. The [Protocol](../protocol.md) defines the shared run and fairness rules.

## Answer bank

The frozen answer bank is empty: `[]`.

The simulated user knows the full product pack and nothing private. It can answer a clarification with an existing rule. It adds no preference, implementation hint or unstated rule. An open choice gets "Your call, within the supplied requirements."

## Time limit

Set the provisional hard limit to 180 minutes, including coding and tool work. Stop at completion or the limit, freeze the files and keep every partial result. Don't extend a run because it is close to finishing.

The check batch measures real completion times before the limit is frozen. The full pack isn't yet timed on the version 2.2 game. If the measured work doesn't fit, provide format support as starting code or split reproduction work into a later case. Keep every player-facing clause. Change and freeze that case before new runs, with earlier attempts kept under their original inputs.

## Grading

Keep earlier checks and cover every executable clause. Use board codes for known deals, snapshots for states and replay for exact moves and timeout edges. Use static and build checks for pins, compiler options, source limits and delivery. Source structure and face legibility need review. An unsure review leaves compliance pending.

The 412 clauses split the earlier 94-ID estimate more finely and add I17, V19 and V20. Version 2.2 retires I02, giving 96 IDs. The proposal's roughly 160 assertions isn't a cap on clause coverage. Checks still count once as defined in [Grading](../grading.md). A reference that passes a subset doesn't make uncovered clauses pass.

Back to [Neon Pairs](_neon-pairs.md).
