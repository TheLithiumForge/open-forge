---
open-forge:
  description: Concise `routes` to important local and external destinations and when to use them
  tags: [LoadNow, Core, Workspace]
---

# Workspace

Workspace `routes` map important local and external destinations without replacing their detailed truth

## Axioms

- Use `Entries` when current work needs an important local or external destination
- Each `route` file points to one or more related destinations and states what they contain and when they matter
- Each `route` file uses `description`-labelled Markdown links with useful tags; local destinations resolve relative to that file, and external destinations use their normal URL
- Keep the map intentionally coarse; prefer modules, projects, repositories, systems, or scopes over members and functions unless finer routing earns its cost
- The workspace chooses route filenames, grouping, and nesting depth
- Workspace `routes` do not replace #Memory or destination truth; the routed destination retains the details

## Entries

<!-- open-forge:generated-index:start -->
- [Current map of the repository's important authoritative sources and representations](sources-of-truth.md) - #Workspace #Repository #CurrentTruth #Evergreen
<!-- open-forge:generated-index:end -->
