---
open-forge:
  description: Pending priorities and planned passes for Open Forge before and during formal dogfooding
  tags: [Memory, Working, Backlog, Contextual]
---

# Backlog

Actionable priorities carried forward from the pre-dogfood cleanup. Deferred designs and product ideas live in `.agents/memory/emerging/ideas/`.

## Alpha Sequence

1. Finish the alpha version.
2. Dogfood Open Forge by migrating this project's notes into its own workflow and memory. Done 2026-07-09; see `.agents/memory/working/sessions/2026-07-09_dogfood-migration.md`.
3. Extract meaningful project-specific patterns, guidance, workflows, and extensions from dogfooding.
4. Restructure and re-review every maintained file.

## Near-Term Priorities

1. Reread every installable payload file and matching governance descriptor for wording, scope separation, tag usage, and route accuracy. The 2026-07-08 wording and CLI pass is committed; verify nothing from it is left dangling.
2. Replace tool-implying load wording such as "must be loaded" with agent-imperative wording such as "read X before Y" across payload and docs; keep the imperative mandatory.
3. Deduplicate framework descriptors against concept docs without losing information; concepts and file descriptors stay conceptually separate.
4. Recheck skills and workflows after route-template and extension terminology settle; apply the workflow redesign idea once accepted.
5. Define the user-documentation architecture: README responsibilities, short guide files, primitive glossary, layer glossary, route/scoping examples, and voice.
6. Rewrite human onboarding after dogfooding, not before it.
7. Run consistency and security review across routing, generated regions, prompt-injection boundaries, update behavior, and tests.
8. Document a human-run behavioral dogfood smoke checklist: install into a temp workspace, install selected extensions, run a representative agent task, and inspect whether routing, memory closeout, and workflow behavior happened.

## User Documentation To Write Later

- Explain recursive routing as the main scalability model.
- Explain `entrypoint`, `entry`, `framework route`, `scope route`, `scoped framework route`, `slug`, and child route.
- Explain #Core, #Memory, and #Extension.
- Explain directives, patterns, guidance, skills, workflows, and workspace routes in a short glossary.
- Show one-project, multi-project, monorepo, and shared-knowledge examples.
- Show the difference between `memory/crystallized/[scope]/documents/` and `memory/[scope]/crystallized/documents/`.
- Explain that folder slugs are concrete runtime paths while `[scope]` notation is only template/documentation notation.
- Explain that users can create external or distributed memory folders only when explicitly routed; no implicit filesystem search.

## Terminology Pass

After alpha dogfooding, define stable terms for:

- current truth
- accepted memory
- durable memory
- historical memory
- contextual memory
- transfer notes
- candidate learning
- organic growth

Use the vocabulary to clean route descriptions, loader tag meanings, user docs, and maintainer governors.

## Low-Priority Review

- Check whether repeated Memory axioms should remain local for clarity or be moved to shared Memory-level wording.
- Reorder axioms across installed files only if it improves readability without creating a large noisy diff.
- Review old archived ideas only when reconstructing why a decision was made.
