---
open-forge:
  description: User-facing scalability, configurability, and a low-friction CLI experience
  tags: [OpenForge, Documentation, Scalability, Customization, CLI, UX]
---

# Scalability And CLI Experience

## User-Facing Positioning

Future user documentation must make the recursive category model one of Open Forge's clearest benefits.

One small convention scales to any useful depth:

- a folder opts into routing through its category entrypoint
- the category entrypoint contains its stable meaning and generated direct entries
- direct markdown files become routes in that category
- child categories repeat the same contract
- the loader exposes only direct root categories

This gives users an arbitrarily deep knowledge tree without a central registry, fixed taxonomy, or framework-defined project layout.

The user owns the organization. Categories can represent technologies, projects, repositories, teams, domains, patterns, workflows, private knowledge, or any other useful grouping. Users can add, rename, move, split, or nest categories as their work evolves.

The documentation should show that the same structure works for:

- a small personal project
- technology-specific material such as React patterns
- a monorepo with multiple projects
- many repositories sharing one knowledge source
- a large personal or organizational knowledge tree

Scalability comes from recursive composition and selective loading. Agents see the next useful routing layer instead of loading or flattening the whole tree.

## Documentation Principle

Describe this as user-owned configurability, not as a large configuration system.

Users configure Open Forge primarily by creating folders and markdown files. The CLI maintains navigation derived from that structure. The framework must not imply that users need to understand every possible category before starting.

## Primitive Glossary

After the framework primitives are approved, user documentation needs one concise section that extracts their essential meanings and relationships.

The glossary must distinguish directives, patterns, guidelines, skills, and workflows without redefining established AI terms. It should explain which primitives constrain behavior, provide judgment, shape outputs, perform reusable capabilities, and orchestrate larger agent goals.

This glossary must be derived from the approved framework governors rather than developed independently in user-facing documentation.

## Low-Priority CLI Experience

The CLI should eventually make common actions obvious for new users while remaining predictable for scripts and experienced users.

Candidate command responsibilities include:

- `install` for the initial core installation
- `update` for reviewing and applying core framework updates
- `index` for rebuilding generated navigation
- `list` for discovering available extensions
- `preview` for showing what an operation would change
- `add` for installing selected skills, workflows, patterns, or other extension payloads
- `remove` for removing a managed extension
- `doctor` for checking structure, generated regions, and installation health
- category scaffolding for creating routed folders with required description and tags
- extension scaffolding for creating shareable installable payloads

Exact command names remain undecided. Similar verbs such as patch or modify must not be added as aliases unless they represent a meaningfully different operation.

## Interactive Selection

An optional interactive interface or wizard may support:

- initial setup
- selecting optional skills, workflows, patterns, and technology packs
- previewing files and destinations before installation
- choosing between compatible variants
- reviewing updates and conflicts

Interactive use must remain a convenience layer over stable non-interactive commands. Installation, updates, extension selection, and CI usage must not require a terminal UI.

## Constraints

- Keep the default installation small.
- Require explicit selection before installing extensions.
- Preview managed file changes before destructive or conflict-prone operations.
- Clearly distinguish #Core updates from extension updates.
- Preserve user-added files and make ownership visible.
- Keep automation possible through deterministic commands and flags.
- Do not design the wizard until extension ownership, provenance, trust, updates, and removal are defined.
- Do not auto-create missing category entrypoints without description and tags from a user command or extension manifest.

## Priority

The CLI experience and extension selector are low-priority work. They follow the #Core primitives, user documentation, file-by-file framework refinement, and consistency and security review.
