---
open-forge:
  description: Open Forge uses one canonical authoring form wherever Markdown carries Framework meaning while treating compatibility syntax as input-only
  tags: [Memory, Decision, CurrentTruth, Framework, Markdown, Authoring, Syntax, Compatibility]
---

# Canonical Markdown Authoring

Accepted 2026-07-26 during migration of the former formatting descriptor and extended 2026-07-27 with optional responsibility metadata.

- Open Forge defines one canonical authoring form wherever Markdown structure carries Framework meaning.
- Ordinary prose remains ordinary Markdown unless a Framework contract assigns semantic meaning to a structure.
- Generated output, examples, Templates, scaffolding, and public authoring help use only canonical forms.
- A parser may accept selected legacy or interoperability forms for reading and migration without presenting them as equivalent ways to author new content.
- Unsupported Markdown equivalents may still render for people, but Open Forge does not promise to interpret them as Framework structures.
- Human-readable files remain the semantic contract. CLI help, validation, generation, and scaffolding derive from accepted files instead of privately defining syntax.
- One canonical form reduces ambiguity, implementation surface, review cost, and agent decision overhead without restricting ordinary prose unnecessarily.
- `description` remains the natural-language pre-load selection surface.
- Optional `responsibility` states the stable boundary of what an opened file is responsible for defining. It guides future edits without creating authority or loading behavior.
- A responsibility changes through deliberate redefinition, splitting, or merging rather than being widened opportunistically to fit unrelated contents.

The [Open Forge Markdown scope](../documents/framework/markdown/_markdown.md) contains the complete accepted authoring form and compatibility boundary.
