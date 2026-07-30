---
open-forge:
  description: Historical record of the completed Markdown route-link migration and accepted complete-workspace validator contract
  tags: [Memory, Archived, Session, Contextual, Historical, Framework, Routing, CLI]
---

# Link Contract And Loader Review

## Goal

Align the maintainer's latest loader edits, migrate Open Forge route lines to standard relative Markdown links without weakening minimal plain-file operation, and resolve the isolated-extension validator contract.

## Accepted Direction Applied

- Keep one compact CLI group in the loader with exact conditions for `load`, `chain`, `index`, and `doctor`
- Use prose plus subheadings or true nested lists when one statement introduces a following list; never place an apparently subordinate list beside its introducer at the same list level
- Keep every CLI command equivalent to a complete manual plain-file path
- Preserve the directive wording change as explicit binding scope and the memory punctuation cleanup
- Leave automatic workspace formatting as a deferred idea
- Treat maintainer edits as intentional direction on clarity, readiness, logic, or tone rather than normalizing them back to an agent's earlier choice

## Link Decision Accepted

Use `[Description](relative/path.md) - #Tags` for generated `Entries` and authored `Required Routes`. Links resolve relative to the containing Markdown file. CLI route arguments remain workspace-relative because they are not Markdown links.

In extension source, a link to a framework or dependency file may remain unresolved until the payloads are assembled because the target will exist in the installed workspace. A link to a file contributed by the same pack must resolve inside that source payload.

## Link Shape Analysis

- `[Description](relative/path.md) - #Tags` is the strongest linked shape if the whole contract migrates: the description and path each occur once, rendered descriptions are clickable, raw Markdown still exposes the path, and graph-aware tools receive a standard edge
- `[relative/path.md](relative/path.md) - Description - #Tags` keeps the path visible when rendered but duplicates it and costs more context
- Keeping backtick paths preserves the current single shape and source/install path semantics but remains opaque to ordinary Markdown link and graph tools
- CLI `--route` arguments should remain workspace-relative regardless of the Markdown decision because command arguments are not document links

## Validator Decision Accepted

The maintainer accepted that `doctor` and `find --follow-required` validate their selected target as a complete workspace, remain manifest-agnostic, and treat every missing Required Route as a blocker. When either command is aimed directly at an isolated source payload, it therefore reports intentionally absent #Core or declared-dependency links. Same-package links still resolve in source, and cross-package links are validated after assembly without fake source stubs or duplicated payload files.

## Validation

- Reindexed all 29 maintained dogfood, installable source, extension, and benchmark route trees; a second complete pass was byte-idempotent
- `open-forge doctor --json .` reports zero errors and zero warnings
- `open-forge doctor --json ./src/open-forge` reports zero errors and zero warnings
- Audited 238 maintained Markdown files and 67 generated regions: all 128 generated links are canonical, document-relative, and resolvable; all 36 empty regions retain the exact sentinel
- All 15 Required Route links use the canonical shape; three same-package links resolve in isolated source and twelve declared cross-package links resolve through the assembled dependency closure
- Standard link parsing covers balanced labels and destinations, escaped punctuation, URI-encoded paths, legacy compatibility, and lexical plus physical containment; malformed Required Routes block `find --follow-required`
- Full verification passes: 168 tests and 1,147 assertions; the build passes
- Source/dogfood loader identity, maintainer wording preservation, changed-list hierarchy checks, both staged and unstaged `git diff --check`, and route-graph audits pass; the loader remains within its 35-to-70 non-empty authored-line contract at 42 lines

## Closeout

The complete-workspace validator contract is recorded in the current Extensions And CLI decision and governing CLI and extension documentation. No open follow-up remains for this checkpoint.
