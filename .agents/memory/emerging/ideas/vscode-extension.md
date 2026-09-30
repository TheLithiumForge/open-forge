---
open-forge:
  description: Explore a thin VS Code extension over the CLI for @open-forge code references, hover previews, applyTo match previews, and two-way links between code and docs
  tags: [Memory, Idea, Contextual, Candidate, CLI, Editor, VSCode, References, Navigation, Glob]
---

# VS Code Extension

## The idea

Recorded on 2026-09-30 from the maintainer. It builds on
[Code To Context References](code-to-context-references.md), which defines the
`@open-forge` comment convention:

```csharp
// @open-forge ./.agents/memory/crystallized/documents/cli/architecture.md#threat-boundary
```

A VS Code extension would make those references and other Open Forge facts
usable from the editor:

- **Ctrl-click** a reference to open the target file at its heading. A
  workspace-root path reads well and resolves without tooling. An ID, written
  the way CLI commands accept it, is shorter.
- **Hover** a reference to see the target heading and a short excerpt of the
  surrounding section.
- **Preview `applyTo` matches**: hover or run a command on an `applyTo` line to
  see which current files the condition matches.
- **Two-way links.** One comment is the edge. Code points to docs through the
  comment, and a document shows which code references it through a reverse
  lookup derived from the same comments. No second, hand-maintained backlink is
  needed.
- **CLI-backed commands** for upkeep and debugging, such as "which rules apply
  to this file" through `context --for <current file>`, `index`, and `doctor`.

## Assessment

An independent council review on 2026-09-30 and the Overseer agree that the
idea makes sense as a small navigation tool once the reference convention is
settled.

**Why it is useful.** Someone meets a non-obvious constraint in code, follows
its reference, and reads the explanation without searching the documentation
tree. "Which rules apply to this file" is the other strong journey, and the CLI
already answers it.

**What VS Code does not give for free.** VS Code links URLs in source files and
resolves relative links in Markdown, but it does not make a root-relative path
inside a code comment clickable. `DocumentLinkProvider` and `HoverProvider` are
the integration points. The extension must recognize the marker and resolve the
target itself.

**Keep the logic in the CLI.** The extension should be a thin interface over CLI
JSON output, so resolution, validation and matching have one implementation.
Agents, which gain nothing from Ctrl-click or hover, then get the same
capabilities from the CLI directly.

## Prerequisites and corrections

- `context` resolves an ID, but `--content section:<name>` matches visible
  heading text, not `#slug` fragments. Heading resolution needs a defined rule,
  including duplicate headings.
- `references` scans links in `.agents` Markdown today. Reading `@open-forge`
  comments in code is a new input format and scan scope, not an existing
  tracked-file scan.
- The `applyTo` preview can call `route inspect <source> --matching-files`,
  added on 2026-09-30. It reuses the applicability evaluator and reports the
  matching files, a count, the scan scope, and completeness. A source with no
  restrictive condition answers "all files" without scanning.

## Minimum useful version

1. The `@open-forge` convention, validated by `doctor`.
2. An extension with Ctrl-click, a bounded hover, and a visible missing-target
   or missing-heading marker for references in open documents.
3. Later: the match preview, the reverse lookup view, and CLI commands. Keep
   any command that deletes data behind an explicit action.

## Risks

- Duplicating CLI semantics in the extension. Avoid it by calling the CLI.
- Heading drift. Validation is what keeps heading anchors trustworthy.
- Ambiguous IDs or workspace roots in multi-root workspaces.
- Slow scans or subprocess calls while typing. Cache per session and support
  cancellation.
- VS Code Workspace Trust. Run the CLI only in trusted workspaces.
- Maintaining an editor-specific package. A language server could reach other
  editors later, but it is heavier than a first prototype needs.

## Priority

Not now. Revisit after the CLI is releasable and the hybrid ID normalization and
heading-resolution rule exist.
