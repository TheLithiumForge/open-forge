---
open-forge:
  description: Form one deterministic plain-docs plan from uninstall's selection, the strip parts and flatten before either command writes
  responsibility: Define how uninstall and export form and check their shared plan before each command applies or writes it
  tags: [Memory, Crystallized, CLI, Command, Contract, Strip, Uninstall, Export, Behavior, CurrentTruth]
---

# Plain-Docs Plan Behavior Contract

The [Interface Contract](interface.md) defines the flags, parts, document set and flatten. [`uninstall`](../../uninstall/behavior.md) applies the plan in place. [`export`](../../export/behavior.md) writes it to a destination.

## Plan Before Effects

Read the workspace layout, settings and ownership once. Invalid or unavailable settings stop the run as `<command>.workspace-unavailable`. Select removals as the [uninstall Behavior](../../uninstall/behavior.md#select-removals) defines. Enumerate the documents below the inventory roots without following links, and read each one as strict UTF-8. Parse each document through the shared Markdown and YAML readers. The plan adds no second parser. It uses the parser's frontmatter and link spans and edits only those spans, or the comment that holds a comment link.

Form every edit for every document before the first effect. Edits never overlap, and an edit inside a removed span is dropped. A document that cannot be edited safely blocks the whole plan before any write. Resolve link destinations through the shared local-destination rules, against each document's current location, then compute flattened spellings from the new locations.

## Determinism

The same workspace bytes and options give the same plan and the same bytes. Documents are visited in ordinal path order. Parts apply in the fixed order `frontmatter` or `metadata`, then `entrypoints`, then `links`, then flatten relinks. Plan entries are ordered by path, with folder removals last and deepest first. No time, randomness, environment or file-system enumeration order reaches the plan or the written bytes.

A preview, a refusal and a verified no-op take no lock and create no runtime state.

## Required Evidence

Cover each part alone and combined, root and `open-forge:` frontmatter, foreign keys, CRLF bodies, a frontmatter block without other keys, whole frontmatter removal with and without `metadata`, a native `SKILL.md` under both, entrypoints with and without user text, inline and reference links, links in `Entries` lists, fragment-only, folder, image, external and code-span links, comment links alone on their line and inline, flatten moves, relinks and collisions. Include the eval harness's cases as Integration fixtures that compare exact bytes, copied into the test project with a note that records their origin. Verify dry-run parity, no writes on preview or refusal and repeated runs.
