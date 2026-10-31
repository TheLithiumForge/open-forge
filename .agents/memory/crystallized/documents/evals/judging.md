---
open-forge:
  description: Who judges each Open Forge eval data point, why the headline numbers come only from checks and counts, how the model judge and blind pairwise picks work, and how anyone can rerun them
  tags: [Memory, Document, CurrentTruth, Evaluation, Judging, Measurement]
---

# Eval Judging

Eval Judging says who scores each data point of an Open Forge eval, and how the judging stays the same for anyone who reruns it.

Anyone who downloads Open Forge should be able to rerun an eval and check our results. So the headline numbers come only from checks and counts, which give the same kind of answer on every machine. A model judge answers yes or no questions about the code, and anyone can rerun it on the saved results. People pick the better of two games they play blind, which checks the model judge instead of setting the headline. For example, in the [Greenfield Baseline](greenfield-baseline.md) a hidden check plays a 4x4 game, the judge answers whether each component keeps its logic, markup and styles together, and the maintainer picks between two games labeled A and B.

Status: proposed on 2026-10-30, with the maintainer's decisions of the same day. A first draft is built into the [eval tool](../../../../../evals/README.md).

## Who judges what

| Judge         | Scores                                                                                                | Why                                         |
| ------------- | ----------------------------------------------------------------------------------------------------- | ------------------------------------------- |
| Harness       | Tokens, dollars, time, tool calls, context, compactions, build, performance, prompt words, questions | It can count them with no judgment          |
| Hidden checks | Functional success, requirement coverage, lost progress, the wishes a test can check                 | Tests repeat exactly and cost little        |
| Model judge   | Code quality, rules followed, the other wishes                                                        | They need reading the code at every run     |
| People        | Which game is better, and checks on the other judges                                                  | Taste and feel need people                  |

The [Eval Data Points](data-points.md) table gives each data point's unit.

## Model judge

The model judge follows these rules:

- It's the strongest model available at its highest effort, and at least as strong as the agents it judges. If an agent already uses the strongest model, the judge is that model at its highest effort.
- One judge, effort and rubric for every run of a batch. Same-family judges are fine, because we compare setups within one tool and model, so a taste for its own family lifts every setup alike.
- Every question has a yes or no answer, such as "Does each component keep its own logic, markup and styles together?" Unsure is allowed when the code doesn't tell.
- It cites the files and lines each answer is based on.
- It answers every question three times. The counts show how sure it is.
- It sees only the app's source. Open Forge files, instruction files and the agent's notes are removed first, so it can't tell the setup.
- It sees the prompt and the hidden wishes, so it can judge whether the wishes were met.

When models improve, a newer judge can score old runs again from their saved files. Old and new runs then share one judge, which is how we track over time whether Open Forge keeps its edge.

## People

People judge only what the others can't, and they judge blind:

- The tool shows two finished games side by side, labeled A and B, at desktop or phone width. They play both and pick the better one, or call it even.
- Pairs come only from runs of the same tool and model, in random order.
- Setups stay hidden, and so do the results, until they press Reveal.
- They can read the prompt and the hidden wishes. Blind hides the setup, not the task, and the wishes are the same for every setup.

Picking between two is far more consistent between people than a 1 to 5 score. The results show each setup's share of picks won, and how often people agree with the model judge.

## When people check

People look at a run in these cases:

- Every failed hidden check. The run's screenshots and checks show whether the game or the check broke.
- A sample of the model judge's answers, and every answer it gave as unsure or split.
- Any run whose numbers look wrong, such as zero tokens or a two-second build of a whole game.

## Reruns by others

Someone who reruns an eval gets different raw numbers, because models and dates differ. What should match is the difference between setups. Each batch records:

- A fingerprint of the prompt, the hidden wishes, the rubric and the checks. Editing the wishes changes it, so edited runs never mix with standard ones.
- The tool, Open Forge and Node versions, and the config.

## Not decided yet

- The rubrics beyond the first Greenfield Baseline questions.
- How large the sample of judge answers that people check is.
- Who else picks games besides the maintainer, and how picks from others are collected.

Back to [Evals](_evals.md).
