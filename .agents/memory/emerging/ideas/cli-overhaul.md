---
open-forge:
  description: Rebuild the MVP CLI around the accepted ACE architecture, preservation-first lifecycle semantics, canonical Markdown help, and deterministic reasoning acceleration
  tags: [Memory, Idea, Contextual, Candidate, CLI, Architecture, Product, Refactor, Markdown]
---

# CLI Overhaul

The current CLI is a dogfooded MVP and a source of behavioral evidence, not the final product architecture. Reassess and recreate it after the vision, architecture, and installable source contracts converge instead of extending its present structure indefinitely.

The [CLI MVP Architecture](../../crystallized/documents/cli/architecture.md) owns proven current behavior, safety boundaries, and liabilities. This file owns candidate future design until enough of it is accepted to establish a replacement current architecture.

## Accepted Objectives

- Make correct framework behavior the cheapest path while keeping human-readable Markdown semantically complete without the CLI.
- Treat the CLI as a first-class `deterministic reasoning accelerator`: resolve explicit routes and inheritance, return ordered context with provenance, validate structure, and perform safe mechanical changes without privately owning meaning.
- Keep routing and helper behavior effective across deeply nested scopes, projects, repositories, and shared sources of truth, subject only to unavoidable context and resource costs.
- Preserve proven safety properties such as reviewable diffs, containment, collision detection, ownership checks, preview, and rollback without treating the current command or implementation shape as permanent.
- Replace the current monolithic MVP structure with an implementation whose command boundaries, contracts, and tests remain easy to evolve.
- Make every mutation start from one inspectable plan used by preview, application, structured output, and verification.
- Keep interactive wizards as presentations over the same deterministic operations available non-interactively.

## Candidate Architecture

The overhaul should center on one deterministic planning core used by thin human and machine interfaces:

```text
Human and machine interfaces
  -> application operations
    -> inspectable plans and results
      -> Framework and Extensions domain services
        -> filesystem, Git, package, clock, and terminal adapters
```

### Interfaces

Interfaces parse explicit intent, choose human or structured presentation, and map operation results to exit status. They do not own route, lifecycle, ownership, or transaction policy.

Interactive flows call the same application operations as scripts. A wizard must not become a second implementation path with different safety or lifecycle behavior.

### Application Operations

Each operation represents one clear intent. Candidate operations include:

- Inspect effective context
- Query explicit routes and relationships
- Validate a workspace
- Rebuild derived navigation
- Initialize the Framework
- Complete selected missing defaults
- Preview and apply an upgrade
- Restore selected distribution files
- Create a scope or routed artifact
- Install, update, or remove Extensions

Final command names and flags remain interface decisions. The semantic distinctions among operations are architectural.

### Domain Services

Pure or mostly pure services should own:

- Markdown metadata and structural parsing
- Route graph construction and resolution
- Context loading and inheritance
- Framework contract validation
- Portable path and ownership identity
- Distribution comparison
- Extension dependency resolution
- Change planning

These services return values and findings without printing, prompting, or mutating the filesystem.

### Plans, Application, And Adapters

Every write operation should produce a common plan that identifies:

- Operation, target, source, and declared lifecycle authority
- Preconditions and expected post-application verification
- Create, update, delete, preserve, and unchanged effects
- Authored, managed, and derived boundaries
- Reasons, provenance, conflicts, and required decisions
- Safety and review requirements

Application revalidates assumptions immediately before mutation, applies effects in a recoverable order, and verifies the result. Git remains the preferred durable checkpoint. A persistent journal or workspace lock is justified only if evidence shows that it closes a real recovery or concurrency gap.

Filesystem, Git, distribution discovery, terminal interaction, clocks, hashing, and packaging belong behind narrow adapters so process and platform behavior do not become domain policy.

## Lifecycle Direction To Design

- Separate first installation, non-destructive completion of missing framework material, intentional upgrade, and forceful restoration instead of treating every reinstall as the same operation.
- Preserve user-customized existing files by default. Require an explicit, previewable operation before replacing them with distribution defaults.
- Keep generated regions, explicitly managed blocks, user-owned files, scoped framework routes, extension ownership, and overwrite companions distinct during planning.
- Define how users receive framework fixes without silently destroying customization; Git recoverability is a safeguard, not permission to overwrite.
- Explore an update wizard that presents what changed and the exact diff before mutation, then offers at least apply all changes, review and select individual changes, keep local content, or cancel. The same interaction may cover managed Framework files, installed Templates, and other managed content while preserving their different ownership rules.
- Do not treat a Template update as an automatic update to artifacts previously instantiated from it. Instance propagation would require separate provenance, ownership, conflict, and opt-in semantics.

## Canonical Markdown Syntax Help

The CLI should provide `open-forge help syntax` as the easy entry to Open Forge's canonical Markdown authoring contract. The final command vocabulary may change during interface design, but a dedicated syntax reference is part of the intended product.

The help should make one preferred form obvious for every Markdown construct Open Forge generates, parses semantically, or asks users and agents to author. At minimum, it should cover:

- Scoped YAML frontmatter with `description`, optional `responsibility`, and tags
- ATX headings
- Paragraph and blank-line separation
- Hyphen unordered lists and the canonical ordered-list form
- Fenced code blocks
- Inline relative Markdown links
- Bare established tags
- Route `Entries` and workflow `Required Routes`
- Generated-region markers
- Category entrypoints and concrete scope slugs
- Overwrite companions
- Primitive-specific document shapes that deterministic validation recognizes

The command must surface the accepted [Open Forge Markdown scope](../../crystallized/documents/framework/markdown/_markdown.md) and the installed runtime sources it represents rather than make help text privately authoritative for syntax. It should distinguish:

- Canonical authoring that Open Forge emits, documents, and expects
- Explicit legacy or interoperability syntax accepted only for reading or migration
- Unsupported equivalents that Open Forge does not promise to interpret

This boundary should apply most strongly where syntax has Framework meaning. Ordinary prose remains ordinary Markdown unless Open Forge needs to parse it. `doctor` may reject ambiguous or noncanonical Framework structures and warn about selected authoring conventions, but it should not police unrelated prose merely to enforce personal style.

Scaffolding should use the same contract to create routed artifacts with canonical metadata and structure. Templates may supply starting content, while the CLI makes placement, frontmatter, placeholder replacement, and route maintenance cheap.

## Knowledge Role Help

The CLI should also make knowledge-role selection cheap through a command such as `open-forge help roles`. Users and agents should not need to memorize a governance table before ordinary placement feels natural.

The help should expose:

- The distinct current-document, Memory, and Core roles
- The primary question, authority, and lifecycle of each role
- The difference between current state and supporting rationale
- Natural selection examples and links to applicable Templates
- The authoritative route or source file for the human-readable definition

The command must derive from or point to human-readable authoritative Framework sources rather than make CLI output a private ontology. It may offer interactive placement or scaffolding assistance, but suggestions remain transparent and correctable.

The repository's temporary knowledge-role helper can inform this design during migration. It should not become a permanent prerequisite for users.

## Structured Interface Direction

Every read and write operation should return one versioned structured result suitable for scripts and future tools. It should distinguish:

- Data
- Findings
- Warnings
- Conflicts
- Planned or applied effects
- Provenance
- Suggested next actions

Human output should be rendered from the same result. This does not require a daemon or public network API.

## Migration Direction

The overhaul should:

1. Classify current tests as invariant protection, MVP compatibility, or obsolete-schema coverage
2. Extract pure domain behavior from the monolith without changing desired public behavior unnecessarily
3. Establish common plan and structured-result contracts
4. Separate read-only context operations from mutation operations
5. Split installation intents before changing overwrite behavior
6. Move extension-specific semantics behind the Extensions boundary
7. Replace scattered Framework constants with explicit owned contracts
8. Preserve closure tests around real filesystem, Git, package, and process boundaries
9. Remove MVP commands or flags only through an intentional migration surface

Compatibility is valuable only when it preserves a desired user contract. An unpublished MVP shape should not constrain the final architecture.

## Promotion Conditions

A replacement current CLI architecture is justified when the Framework contracts are accepted and the following choices are coherent enough to govern implementation:

- Command and structured interface responsibilities
- Canonical Markdown syntax ownership and compatibility policy
- Lifecycle intents and preservation boundaries
- Shared plan, transaction, and verification contracts
- Framework and Extensions domain boundaries
- Recovery, concurrency, packaging, and performance expectations

## Deferred

- Semantic or inferred relevance remains outside the current critical path. Explicit routes, descriptions, relative links, tags, and deterministic helpers are authoritative first.
- Rune or another future retrieval layer may exploit the same visible relationship surface without becoming Open Forge runtime truth.
