---
open-forge:
  description: CLI capabilities to design later - scaffolding, bulk dump, route inventory, extension lifecycle, upgrade modes
  tags: [Memory, Idea, Contextual, Candidate, CLI, Extension]
---

# CLI Design Backlog

Deferred CLI design work carried from the cleanup backlog. Candidate designs, not commitments.

- Scaffold concrete routed paths from user intent.
- Preview trees before writing.
- Design `open-forge dump` as a first-party context loading helper before implementation. Explore `--index`, `--tags`, `--tag <Name>`, and `--path <file> --depth N`; keep it plain, deterministic, and readable. The dogfood v4 Experiment B prototype validated bulk dumping as the highest-leverage, lowest-risk tooling investment.
- Design an adjacent route inventory command such as `open-forge give <route>` or `open-forge list <route>` that lists routed entries for things like workflows, memory, or crystallized memory without dumping all bodies.
- Generate missing ancestor entrypoints only with meaningful scaffold content.
- Detect collisions and ask before reusing or renaming paths.
- Support route templates with named slug parameters.
- Support extension manifests, extension metadata, provenance, compatibility, aliases, migrations, and preview.
- Support extension install, update, remove, list, and local testing.
- Define how first-party bundled extensions are named, documented, versioned, tested, and shown by `extend --list`.
- Let users select individual workflows while the CLI auto-selects required shared skills; also allow optional extra skills and related packs to be selected explicitly.
- Test extension installs and future workflow flows through real OS temp directories instead of mocked filesystem operations.
- Design workflow dependency metadata so selected workflows can install or require shared skills without duplicating extension payloads; see `extension-skill-sharing.md`.
- Support extension-template authoring for maintainers.
- `doctor` or validation commands for generated regions and routing health are currently judged unlikely to help; revisit only with a demonstrated need.
- Investigate an optional all-in-one generated index for agent cold starts; judge token cost, staleness risk, authority confusion, and whether recursive `entrypoints` already solve enough.
- Decide forceful versus softer upgrade modes:
  - forceful upgrade overwrites and re-adds all framework-owned files
  - softer upgrade updates existing framework-owned files but does not re-add optional or default files the user intentionally deleted
