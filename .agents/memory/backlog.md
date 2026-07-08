# Backlog

This backlog is extracted from the archived sessions and ideas. Keep it current here until formal dogfooding replaces this cleanup memory.

## Alpha Sequence

1. Finish the alpha version.
2. Dogfood Open Forge by migrating this project's notes into its own workflow and memory.
3. Extract meaningful project-specific patterns, guidance, workflows, and extensions from dogfooding.
4. Restructure and re-review every maintained file.

## Near-Term Priorities

1. Finish the current uncommitted wording and CLI pass.
2. Reread every installable payload file and matching governance descriptor for wording, scope separation, tag usage, and route accuracy.
3. Recheck skills and workflows after route-template and extension terminology settle.
4. Define the user-documentation architecture: README responsibilities, short guide files, primitive glossary, layer glossary, route/scoping examples, and voice.
5. Rewrite human onboarding after dogfooding, not before it.
6. Run consistency and security review across routing, generated regions, prompt-injection boundaries, update behavior, and tests.
7. Document a human-run behavioral dogfood smoke checklist: install into a temp workspace, install selected extensions, run a representative agent task, and inspect whether routing, memory closeout, and workflow behavior happened.

## User Documentation To Write Later

- Explain recursive routing as the main scalability model.
- Explain `entrypoint`, `entry`, `framework route`, `scope route`, `scoped framework route`, `slug`, and child route.
- Explain #Core, #Memory, and #Extension.
- Explain directives, patterns, guidance, skills, workflows, and workspace routes in a short glossary.
- Show one-project, multi-project, monorepo, and shared-knowledge examples.
- Show the difference between `memory/crystallized/[scope]/documents/` and `memory/[scope]/crystallized/documents/`.
- Explain that folder slugs are concrete runtime paths while `[scope]` notation is only template/documentation notation.
- Explain that users can create external or distributed memory folders only when explicitly routed; no implicit filesystem search.

## CLI Work To Design Later

- Scaffold concrete routed paths from user intent.
- Preview trees before writing.
- Design `open-forge dump` as a first-party context loading helper before implementation. Explore `--index`, `--tags`, `--tag <Name>`, and `--path <file> --depth N`; keep it plain, deterministic, and readable.
- Design an adjacent route inventory command such as `open-forge give <route>` or `open-forge list <route>` that lists routed entries for things like workflows, memory, or crystallized memory without dumping all bodies.
- Generate missing ancestor entrypoints only with meaningful scaffold content.
- Detect collisions and ask before reusing or renaming paths.
- Support route templates with named slug parameters.
- Support extension manifests, extension metadata, provenance, compatibility, aliases, migrations, and preview.
- Support extension install, update, remove, list, and local testing.
- Define how first-party bundled extensions are named, documented, versioned, tested, and shown by `extend --list`.
- Let users select individual workflows while the CLI auto-selects required shared skills; also allow optional extra skills and related packs to be selected explicitly.
- Test extension installs and future workflow flows through real OS temp directories instead of mocked filesystem operations.
- Design workflow dependency metadata so selected workflows can install or require shared skills without duplicating extension payloads.
- Support extension-template authoring for maintainers.
- Consider `doctor` or validation commands for generated regions and routing health.
- Investigate an optional all-in-one generated index for agent cold starts; judge token cost, staleness risk, authority confusion, and whether recursive `entrypoints` already solve enough.
- Decide forceful versus softer upgrade modes:
  - forceful upgrade overwrites and re-adds all framework-owned files
  - softer upgrade updates existing framework-owned files but does not re-add optional/default files the user intentionally deleted

## Deferred Product Ideas

- Investigate how to word load-policy obligations honestly: they are mandatory framework instructions for compliant agents, not mechanically enforced tool behavior.
- Investigate whether observations should stay a separate candidate-learning route, become explicitly best-effort, or fold part of their closeout role into sessions.
- Investigate a small optional reliability/defaults extension for process directives such as memory closeout, language/runtime defaults, and safe technical practices without putting opinionated defaults in #Core.
- Document directive scope boundaries: directives govern work product, process, and technical/project behavior; agent persona or chat register should not rely on directives for deterministic behavior.
- Investigate whether route `description` metadata and the first heading/definition text should be identical, intentionally different, or partially deduplicated. If the description already carries the route meaning for generated `Entries`, some installed headings or first sentences may be redundant.
- Investigate whether the loader needs explicit Memory axioms beyond the generated Memory `entry`. The loader already sees the Memory path, description, tags, and load policy through `Entries`, so some Memory wording may belong only in `.agents/memory/_memory.md`.
- Archive metadata blocks with origin path, archived date, replacement, and reason.
- Optional typed grouping routes such as `projects/`, `packages/`, `domains/`, or `teams/` as CLI presets, not base defaults.
- Planning/task/backlog extension that can map to GitHub, Jira, GitLab, Linear, or local markdown.
- Technology pattern packs.
- Workflow packs for brainstorming, task creation, implementation, testing, review, architecture, UI/UX, and refactoring.
- Orchestration route or extension for workflows that can declare preferred tools, subagents, isolation rules, and CLI usage when multiple agent runtimes are available.
- Document that directives are reliable for work product, process, and technical defaults, but agent-to-user persona or communication-register directives may conflict with runtime safety behavior and should not be used for deterministic control.
- Skill packs that remain compatible with native agent/runtime skill concepts.
- Optional interactive wizard as a convenience over deterministic commands.
- External `.memory/` or distributed package-local memory as a documented user pattern, not default behavior.

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
