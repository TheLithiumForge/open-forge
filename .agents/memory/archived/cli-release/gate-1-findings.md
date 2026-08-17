---
open-forge:
  description: Stable evidence-backed findings and required follow-up from the Gate 1 CLI audit
  tags: [Memory, Archived, CLI, Release, Gate, Audit, Finding, Evidence, Contextual, Historical]
---

# Gate 1 Finding Register

Finding states are `Open`, `Decision required`, `Deferred verification`,
`Historical`, or `Resolved`. None of these findings accepts a product design.

The maintainer has since frozen all MVP source, build, and test work, selected
.NET Native AOT as the implementation direction, excluded Rune, and archived
deleted CLI-v2 knowledge under `.agents/memory/archived/cli-v2/`. Findings about
the frozen implementation remain historical liabilities, not work items.

## Repository And Authority

- [x] **G1-F001 — Critical — Resolved:** Current source maps no longer claim `src/cli/` is the
      implemented final CLI build source, but only build helpers remain and
      `build.ts` builds `src/cli-mvp/cli.ts`. Evidence: `.agents/maps/sources-of-truth.md`,
      historical `.agents/memory/archived/cli-v2/documents/architecture.md`,
      `build.ts:11-20`.
- [ ] **G1-F002 — High — Open:** `package.json` exposes `open-forge` and
      `open-forge-old` through the same MVP artifact. Evidence: `package.json:11-14`,
      `src/cli-mvp/cli.ts:10`.
- [x] **G1-F003 — High — Resolved:** The contradicted direct-replacement Decision is historical under the CLI-v2 archive.
- [x] **G1-F004 — High — Resolved:** The replacement backlog is historical under the CLI-v2 archive, and the general backlog now follows the new CLI release program.
- [x] **G1-F005 — High — Resolved:** Deleted CLI-v2 Documents and contracts no longer act as current sources.
- [x] **G1-F006 — Medium — Resolved:** CLI-v2 Decisions no longer appear in current Decision routing.
- [x] **G1-F007 — Medium — Resolved:** CLI-v2 Directives and Patterns were removed from active governance and routing.
- [x] **G1-F008 — Medium — Resolved:** Public CLI and development documentation now describe the frozen legacy boundary and active native direction.

## Build, Package, And Evidence

- [x] **G1-F009 — Critical — Resolved for current scope:** The package now includes
      the adjacent Framework payload. Frozen Extension packaging is not a new-CLI work item.
- [x] **G1-F010 — High — Historical:** Generated embedded assets may remain unused.
      The frozen build is outside this effort.
- [ ] **G1-F011 — High — Open:** `check:fast` and `check` invoke a missing
      `typecheck` script.
- [x] **G1-F012 — High — Historical:** Retained MVP test suites import missing
      `tests/support/index.ts`.
- [ ] **G1-F013 — High — Open:** No current root CI or release workflow exists.
- [x] **G1-F014 — Medium — Historical:** TypeScript and ESLint projects still target
      the deleted replacement layout rather than the current MVP source.
- [ ] **G1-F015 — Medium — Open:** Commander/completion dependencies appear to
      be deleted-v2 residue and need later disposition.
- [x] **G1-F016 — Medium — Resolved:** Legacy documentation now matches the transitional Node floor.
- [ ] **G1-F017 — Medium — Open:** Generated `.temp` snapshots contain stale
      Framework contracts and must not be mistaken for current sources.

## Framework Compatibility

- [ ] **G1-F018 — High — Open:** The frozen MVP performs broad `#KeepInMind`
      loading rather than the current target-sensitive entrypoint contract.
- [ ] **G1-F019 — High — Open:** The MVP may fabricate temporary metadata and
      warn rather than fail closed for some overwrite liabilities.
- [ ] **G1-F020 — Medium — Open:** Detailed fail-closed metadata and generated
      region contracts are repository-only and not fully stated in the installable
      payload, despite semantic-completeness claims.
- [ ] **G1-F021 — Medium — Decision required:** Current loader command spellings
      are optimized for the MVP and should change only after the new interface is accepted.

## Evidence Quality

- [ ] **G1-F022 — High — Historical:** The master review and change log are
      derivative and must not be counted as independent support.
- [ ] **G1-F023 — High — Historical:** Most critique perspectives inherited
      earlier findings; apparent convergence is partly propagation.
- [ ] **G1-F024 — High — Decision required:** Existing measurements identify
      context-output cost but do not select a 3-, 16-, 17-, or 19-leaf surface.
- [ ] **G1-F025 — Medium — Decision required:** Receipt evidence is contested;
      `--since` introduces stale-context and debugging risk while paths projections
      capture much of the measured saving.

## Native Handoff And Prototype

- [ ] **G1-F026 — Critical — Deferred verification:** Restore, compile, tests,
      AOT publication, binary startup, wrapper parity, package size, and startup
      claims remain unverified.
- [ ] **G1-F027 — High — Open:** The prototype always emits empty context scope.
- [ ] **G1-F028 — High — Open:** Boot cost is calculated but not bounded.
- [ ] **G1-F029 — High — Open:** `check` uses a separate validation path rather
      than the context resolver/shared loading model.
- [ ] **G1-F030 — High — Open:** Source locations after frontmatter removal can
      be incorrect.
- [ ] **G1-F031 — High — Open:** JSON output does not consistently honor
      paths-only projections.
- [ ] **G1-F032 — High — Open:** The operation catalogue is manually synchronized
      rather than the actual command-composition source.
- [ ] **G1-F033 — High — Open:** Overlay Patterns, schemas, source, and fixtures
      disagree on result fields, snapshots, fixture structure, and compatibility.
- [ ] **G1-F034 — High — Open:** Placement instructions do not preserve links
      for all documented installation topologies.
- [ ] **G1-F035 — High — Open:** Supplied validation scripts do not perform all
      checks claimed by `VALIDATION.md`.
- [ ] **G1-F036 — High — Open:** Native CI smoke steps expect `rune status` to
      succeed even though it returns exit code 1.
- [ ] **G1-F037 — High — Open:** JSON-line size is checked only after allocating
      the full line, so hostile-input memory is not actually bounded.
- [ ] **G1-F038 — High — Open:** Mutation, locks, journal, backup, verification,
      and recovery behavior is specified but not implemented.
- [ ] **G1-F039 — Medium — Open:** npm packages are private/placeholders; PyPI,
      Homebrew, Scoop, and WinGet examples are incomplete; NuGet is absent.
- [x] **G1-F040 — Medium — Resolved by exclusion:** Rune is outside this effort.

## Program And Workspace Risk

- [x] **G1-F041 — High — Historical:** The pre-existing working tree had 55 changed or
      untracked entries, including staged/worktree divergence, deletions, and `NUL`.
      Future cleanup must preserve unrelated work and compare HEAD, index, and
      worktree where necessary.
- [ ] **G1-F042 — Medium — Open:** An empty stale `working/sessions/` directory
      remains after Checkpoints replaced Sessions.
- [ ] **G1-F043 — Medium — Open:** Several historical Decision links and removed
      benchmark/evaluation links are broken.
- [x] **G1-F044 — Medium — Resolved:** Former CLI-v2 knowledge is archived under
      `.agents/memory/archived/cli-v2/` as raw historical input.
- [x] **G1-F045 — High — Resolved:** Current source and exposed executable both
      validate direct Directive `## Instructions`.
- [x] **G1-F046 — High — Resolved for documentation:** Current development
      documentation no longer advertises absent replacement scripts. Transitional
      package scripts remain cleanup work after the native CLI and npm wrapper.
- [ ] **G1-F047 — High — Open:** `open-forge.extensions.json` declares 21
      development-toolkit files, but only 9 exist with matching hashes. Eleven
      files under `.agents/templates/{documents,memory}/` have digest mismatches,
      and receipt-owned `.agents/workflows/development.md` is missing. The current
      `.agents/workflows/development/` routed subtree is not the receipt-owned
      path and must be preserved as user-owned content until provenance and
      migration are accepted. `bun run cli:old -- extend development-toolkit .
--dry-run --pro` blocks on the first divergent owned file. Do not repair,
      restore, delete, or take ownership during Gate 1.

## Finding Completion Rule

A finding is checked only after its accepted disposition is applied and verified.
Discussion alone does not resolve a finding. Deferred verification remains open
until its stated evidence exists or the candidate is rejected.
