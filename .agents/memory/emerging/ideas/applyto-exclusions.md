---
open-forge:
  description: Explore a way to exclude files from an applyTo condition now that a leading ! is an ordinary glob character
  tags: [Memory, Idea, Contextual, Candidate, Framework, Loading, Glob, Frontmatter, CLI]
---

# applyTo Exclusions

## The idea

The maintainer wants some way to say "these files, but not those" in an
`applyTo` condition. For example, C# rules for every `.cs` file except generated
code:

```yaml
applyTo: ["**/*.cs"]
# and somehow: not "**/*.g.cs" or "**/obj/**"
```

Recorded on 2026-09-29 for a later discussion. Nothing is decided.

## Why it is open

A leading `!` became an ordinary character in the glob-dialect follow-up of
[Task 62](../../working/cli-development/tasks/task62/glob-dialect-plan.md),
because that is what POSIX globbing, Python `fnmatch`, VS Code, APM and GitHub
Copilot do. picomatch-based tools, and probably Claude Code, read a leading `!`
as negation instead, so `!` cannot carry exclusion without breaking APM and
Copilot interoperability.

## Options to compare

- A separate field, such as `excludeFrom` or `applyToExclude`, listing patterns
  that remove files from the condition. It keeps `applyTo` values portable, but
  no neighbouring tool reads it.
- Copilot's `excludeAgent`-style precedent does not cover files, so check
  whether APM, Copilot or Claude Code have added a file-exclusion field before
  inventing one.
- Negation only inside a list, as in gitignore, where an entry starting with `!`
  removes earlier matches. It is familiar, but it changes the meaning of values
  that APM and Copilot treat as literal.
- Rely on route scope instead, placing rules for generated code in a separate
  scope. It needs no syntax, but it cannot express exceptions inside one folder.

## What would decide it

Evidence that real rule sets need exceptions often enough to justify a field,
and a check of current APM, Copilot and Claude Code syntax so any choice stays
interoperable. Any addition keeps `applyTo` a filter, never a trigger.
