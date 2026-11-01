---
open-forge:
  description: The shared run steps, simulated user, judge evidence, fair comparison and pre-registration rules
  tags: [Memory, Document, CurrentTruth, Evaluation, Protocol, Conversation]
---

# Eval Protocol

The Eval Protocol is the sequence that gives every attempt the same run stages and keeps grading away from the coding agent.

Status: approved on 2026-11-01. The complete protocol is Not built yet.

A Layer 1 attempt gets a fresh setup, one prompt and answers only when it asks. Its files freeze before checks and judging start. The [proposal's run protocol](../../../emerging/analysis/eval-product-proposal.md#run-protocol) defines this flow at every layer.

## Run steps

Every attempt follows these steps:

1. Restore frozen inputs into a fresh agent container and tool home.
2. Send one task prompt to start the coding session.
3. Answer questions through the simulated user in that same session.
4. Stop at completion or the registered limit, then freeze the files.
5. Run the verifier on those files, then give the judge its evidence.
6. Save the stage outcomes, usage, artifacts and assessments.

The agent never receives grading. Failure and timeout accounting follows [Grading](grading.md#counting). Record storage follows [Results](results.md).

## Simulated user

The simulated user is available at every layer in the assessor container. A fixed classifier finds what each question asks about. Its answer comes from the case's frozen answer bank. It gives no coding advice and volunteers nothing.

For a choice outside the bank, it answers exactly "Your call, within the supplied requirements."

The conversation allows three question rounds. Each decision in a bundled message counts as its own question. A question after the third answer gets the fallback answer and is recorded as unanswered. A simulated user failure records an infrastructure error, never "no question".

The bank has these limits:

| Layer | What the simulated user knows | Limit |
| --- | --- | --- |
| 1 | Prompt and frozen private preferences | Answers only from those inputs |
| 2 | Provided spec subset and answers for declared open choices | Never reveals the rest of Layer 3 |
| 3 | Full spec | Knows nothing private |
| 4 | Project rules, request and reported symptoms | Knows neither the defect location, its fix nor private checks |

For example, a color choice absent from the answer bank gets the fallback instead of a new requirement. The [proposal's simulated user rules](../../../emerging/analysis/eval-product-proposal.md#run-protocol) define the answer bank and its limits.

## Judge evidence

Both providers' judges get the same evidence:

- Filtered source without setup files, generated output or test reports.
- Neutral spec for the layer.
- Change request at Layer 4.
- The same screenshots and face sheets for questions about the look.

The pinned judge runs after every attempt, including usable partial ones. Collaboration is judged from the transcript apart from code, because the transcript can reveal the setup. The [proposal's judge contract](../../../emerging/analysis/eval-product-proposal.md#run-protocol) defines these inputs. Ballots and audits follow [Grading](grading.md#judge).

## Fair comparison

Within a comparison, every setup gets the same prompt, project facts, document descriptions, instruction force, starting code and answer bank. Answers differ only because agents ask different things.

Generate setups from one source and register every extra instruction. Before scored runs, `qualify` checks entry files outside the scored sample. Current comparison setups and the later Extension scope follow [Vision](vision.md#current-work).

The setup must satisfy these rules:

- Keep the setup manifest outside the agent's folder.
- Keep eval wording out of the spec.
- Give linked `AGENTS.md` controls the same descriptions as Open Forge Entries when routing comparisons are run.
- Leave `applyTo` out of greenfield setups.
- Start every setup as a Git repository with one commit.

Use balanced random run order with the same model, effort, tools, network, resources and limits. Record reads without reminders. An agent that doesn't use Open Forge stays in the main result. The [proposal's fair comparison rules](../../../emerging/analysis/eval-product-proposal.md#fair-comparison) define these controls.

## Pre-registration

Before scored runs, freeze these items:

- Inputs, grading and setup content.
- Answer banks and question opportunities.
- Judge model, effort, rubric and ballot settings.
- Limits, balanced order and sample size.
- Comparison groups, margins, methods and result wording.
- Infrastructure replacement rule and human audit sample.

Each current case runs five times per setup, model and tool. Publish inconclusive results. Never add runs because a result nearly passes. The [approval](../../../emerging/analysis/eval-product-proposal.md#approved) fixes the current sample size, and the [proposal's registration rules](../../../emerging/analysis/eval-product-proposal.md#how-many-runs) define what freezes before the run.

## Not decided yet

- The pinned classifier and simulated user model. Choose and register them before scored runs.

Back to [Evals](_evals.md).
