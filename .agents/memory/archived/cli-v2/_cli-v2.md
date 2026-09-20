---
open-forge:
  description: Historical CLI-v2 designs, decisions, governance, plans, and implementation records kept only as raw input for the new CLI
  responsibility: Preserve deleted CLI-v2 knowledge in one inspectable non-current route without letting it govern the new CLI
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI v2 Archive

CLI v2 was deleted. Everything below this route is historical raw input. It is
not current design, accepted direction, binding behavior, a reusable default, or
an implementation plan for the new CLI.

Inspect these files when a current CLI question may benefit from earlier ideas.
Validate each idea against the current Framework and discuss it with the
maintainer before carrying it forward.

The frozen TypeScript MVP is separate. Its source remains under
`src/cli-mvp/`, and its current legacy description remains in
[`../../crystallized/documents/cli/mvp-architecture.md`](../../crystallized/documents/cli/mvp-architecture.md).

The active new-CLI program remains under
[`../../working/cli-release/_cli-release.md`](../../working/cli-release/_cli-release.md).

## Archive Integrity

The files retain original CLI-v2 prose for evidence. Relative links inside moved
descendants may still express their former locations and are not guaranteed to
resolve after quarantine. Use the archive tree and current source map rather than
treating a broken historical link as current navigation.

## Archive Rules

- Treat every descendant as historical and #Contextual regardless of labels retained inside the original file.
- Do not restore a command, job model, library, architecture, safety rule, test strategy, or implementation pattern without current evidence and maintainer acceptance.
- Do not use this route as active governance or implementation authority.

## Entries

- [Historical CLI-v2 rationale kept only as raw input for the new CLI](decisions/_decisions.md) - #Memory #Archived #Contextual #Historical #CLI #CLIv2 #RawData
- [Historical CLI-v2 Directive sources removed from active governance](directives/_directives.md) - #Memory #Archived #Contextual #Historical #CLI #CLIv2 #RawData
- [Historical CLI-v2 architecture, interface, toolchain, and operation contracts](documents/_documents.md) - #Memory #Archived #Contextual #Historical #CLI #CLIv2 #RawData
- [Historical CLI-v2 explorations and early CLI design ideas](ideas/_ideas.md) - #Memory #Archived #Contextual #Historical #CLI #CLIv2 #RawData
- [Historical CLI-v2 implementation tasks, workflow work, evidence, and transfer state](implementation-history/_implementation-history.md) - #Memory #Archived #Contextual #Historical #CLI #CLIv2 #RawData
- [Historical CLI-v2 implementation shapes kept only as raw input for the new CLI](patterns/_patterns.md) - #Memory #Archived #Contextual #Historical #CLI #CLIv2 #RawData
- [Historical CLI-v2 implementation backlog removed from active Working Memory](working/_working.md) - #Memory #Archived #Contextual #Historical #CLI #CLIv2 #RawData
