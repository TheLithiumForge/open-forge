---
open-forge:
  description: Binding C# design and style rules for source and tests authored or reviewed across the workspace
  tags: [Directive, CSharp, Source, Testing, Design, Style, Readability]
---

# C# Directives

## Axioms

- Select this scope whenever C# source or C# tests are authored or reviewed anywhere in the workspace.
- Keep generic C# design and style in this workspace-wide scope. A narrower scope may add implementation or platform constraints, but it must not replace or duplicate these generic rules.

## Entries

- [Design cohesive C# call surfaces with truthful nullability, readable construction, and no speculative frameworks](design.md) - #LoadNow #Directive #CSharp #Design #Nullability #Initialization #Parameters #Records #Strategy #Efficiency #Readability
- [Apply readable, formatted C# style to source and tests across the workspace](style.md) - #LoadNow #Directive #CSharp #Style #Formatting #Readability #Testing
