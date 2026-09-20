---
open-forge:
  description: "Historical CLI-v2 source: Build one bounded local-reference inventory and project it into context, inspection, diagnosis, and safe repair"
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# Local Reference Inventory

Use this Pattern when several CLI consumers need consistent local Markdown
reference facts without changing loading semantics.

## Boundary

The accepted [Local References contract](../../../../memory/crystallized/documents/cli/contracts/local-references.md) is authoritative for supported syntax, resolution, containment, traversal, fragments, cycles, bounds, diagnosis, and repair. This Pattern owns the reusable single-inventory and consumer-projection shape.

Build each source result through the [Markdown Document Facts](markdown-document-facts.md)
Pattern.

## Shape

```text
explicit consumer seeds
  -> one bounded document inspection
  -> typed frontmatter, headings, references, regions, and findings
  -> containing-file-relative resolution
  -> physical containment
  -> canonical real-path visited set
  -> fragment and status classification
  -> consumer-specific projection
```

One invocation owns one inventory. Consumers select or project it; they do not
reparse sources with private rules.

## Shape Constraints

- Keep scanner representation private behind stable document facts and preserve positions and logical identities in the inventory.
- Resolve every reference through one focused resolver and maintain one canonical visited set for the invocation.
- Keep route projection, context projection, diagnosis, and repair as consumers of the inventory rather than private scanners.
- Put bounds and bounded concurrency in named inventory configuration and return their typed evidence with the projection.

## Promotion

Keep a parser or resolver private until at least two real consumers need the
same behavior. When shared, move it only to their nearest common scope. Exact
types, status values, and diagnostic codes live in production source.

## Review

- Do all consumers see the same source, target, fragment, and status?
- Can a repeated link or cycle parse a document more than once?
- Could an unreliable identifier merge unrelated files?
- Does any local path escape physically while looking contained lexically?
- Does checking a relationship accidentally load it as context?
- Can a proposed repair prove the same existing target or heading?
- Does a limit produce explicit incomplete evidence rather than truncation?
- Do exact traversal and repair semantics come from the Local References contract rather than this inventory layout?
