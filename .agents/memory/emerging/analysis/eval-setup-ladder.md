---
open-forge:
  description: The maintainer's five-setup ladder from November 2026, from everything in the prompt to Open Forge with proper categories, and how each setup is built from the same spec with the CLI's strip recipes
  tags: [Memory, Analysis, Contextual, Candidate, Evaluation, Setups, Documents, Routing, Proposal]
---

# Eval Setup Ladder

## Which setups should the evals compare, from the prompt alone to Open Forge?

Status: the maintainer's direction on 2026-11-17. It isn't applied to the harness or the published docs yet. It replaces the published [setup ladder](../../../../src/docusaurus/docs/demos/index.md#setup-ladder) once the harness builds it.

| Step | Setup | What the agent starts with |
| --- | --- | --- |
| 1 | Prompt only | One prompt holds every requirement. Code only belongs here too, since its requirements live in the prompt and the existing code |
| 2 | Agent file and prompt | One `AGENTS.md` holds the requirements, plus the prompt |
| 3 | Agent file, docs and prompt | One `AGENTS.md` that points to the docs, plus unorganized docs: one flat folder with no links between documents |
| 4 | Agent file index and linked docs | One `AGENTS.md` that indexes each document with a short description, plus organized docs: their folders and the links between them |
| 5 | Open Forge | Open Forge with proper categories and subcategories, routed and indexed |

## What changes from the published ladder

- Linked `AGENTS.md` isn't a separate setup any more. It folds into step 4, where the index sits beside organized docs.
- Spec in prompt and Code only stop being side variants. They are step 1.
- Open Forge is one step. Whether the Minimal, Essentials and Full Core presets stay as variants of step 5 is still open.
- Whether No setup, the request without any spec, stays as a control below step 1 is still open.

## How each setup is built

Every setup starts from the same frozen spec, so the steps differ only in where the requirements live and how they're organized.

- **Step 5:** the harness materializes the scoped Open Forge setup, as `materialize/setup.ts` and `scopes.ts` do today.
- **Step 4:** strip step 5 with `open-forge uninstall --strip metadata,entrypoints`, which keeps folders and links, then add the `AGENTS.md` index. The harness writes the index from the documents' descriptions before stripping. The CLI doesn't, because nothing outside the evals needs it.
- **Step 3:** strip step 5 with `open-forge uninstall --strip metadata,entrypoints,links --flatten docs`, then add an `AGENTS.md` that points to `docs/`.
- **Steps 1 and 2:** the harness concatenates the spec into the prompt or into one `AGENTS.md`, in document and clause order, as `setupPrompt()` does for the player pack today.

The strip recipes come from the uninstall and strip work in [PR #34](https://github.com/TheLithiumForge/open-forge/pull/34), whose parity tests already compare the CLI's output with this harness's, byte for byte.

## Related

- [Evals at documentation scale](eval-docs-at-scale.md) asks whether these setups can differ at all on a small project.
- [Eval program plan](eval-program-plan.md) holds the scenarios, phases and context steps.
