---
open-forge:
  description: A solution-wide CLI audit found bounded legacy template-rendering debt while separating fixed output templates from legitimate iterative construction
  tags: [Memory, Observation, AgentLearning, Contextual, Candidate, CLI, CSharp, Rendering, Style, Evidence]
---

# CLI Template Rendering Audit Found Bounded Legacy Debt

## Observation

After the binding C# style Directive was strengthened to prefer one interpolated template for coherent text, an exact-base audit found several accepted CLI renderers and fixtures that still assemble fixed rows or blocks through concatenated fragments, `StringBuilder` micro-appends, or per-value invariant `ToString` calls.

The same audit also showed why the rule needs explicit boundaries. Parsers, escaping code, truncation, token construction, conditional natural-language grammar, and character-by-character transformations use incremental construction for semantic reasons. Replacing those mechanically would obscure policy rather than improve it.

## Evidence And Source

Audit source: immutable commit `32069d3e0516cde007bd35f31da82b88ff613759`. Two independent read-only audits and the Overseer's direct inspection agreed on the main production findings. No audited base file was changed.

### Fixed `StringBuilder` Rows And Blocks

- `Shell/Presentation/CliHelpRenderer.cs:23-29` emits each repeated help section through four operations instead of one fixed block.
- `Commands/References/Shared/Rendering/ReferencesCompactRenderer.cs:17-121` fragments summary, source, selection, section, occurrence, finding, and next-action rows.
- `Commands/References/Shared/Rendering/ReferencesExpandedRenderer.cs:21-163` fragments header, selection, section, occurrence, finding, and next-action rows or blocks.
- `Commands/References/Shared/Rendering/ReferencesExpandedRenderer.Values.cs:35-36` separately formats every numeric component of one location template.
- `tests/integration/.../RouteInspectProfileIntegrationWorkspace.cs:135-164` fragments fixed fixture blocks around legitimate conditional and iterative sections.

### Concatenated Or Separately Formatted Production Templates

- Route List and Route Inspect compact renderers concatenate coherent rows: `RouteListCompactRenderer.cs:52-69` and `RouteInspectCompactRenderer.cs:49-50`.
- Find compact and expanded renderers concatenate summaries, findings, heading rows, or delimited blocks: `FindCompactRenderer.cs:49-64` and `FindExpandedRenderer.cs:281-302`.
- Find, References, Route List, and Route Inspect help renderers split status rows and format exit codes separately.
- References diagnostics and location text separately format numeric values inside coherent text.
- Route Inspect measurement text separately formats counts, bytes, token estimates, and `0.##` size values instead of using an invariant template handler.
- `FindExpandedRenderer.cs:254` separately formats a position inside one labeled output row.

### Test Template Occurrences

- Markdown parser fixtures concatenate dynamic document fragments.
- Route List inventory fingerprints and several Route Inspect fixture composition boundaries concatenate already selected fields or blocks.
- Several output assertions append `Environment.NewLine` with `+`; a bounded mechanical cleanup could make each expected line one template while preserving platform newline behavior.

These are style and reviewability findings, not observed functional defects. Existing output is accepted and often byte-sensitive, so every correction needs exact-output evidence.

## Conclusion, Reasoning, And Tradeoffs

The debt is best addressed later in bounded behavior-neutral cleanup rather than folded into the active Context or Extension integration. The smallest useful grouping is:

1. Shell help block plus byte-exact help evidence.
2. References compact/expanded renderers, diagnostics, help, and full byte-exact presentation evidence.
3. Find and Route coherent-template cleanup grouped by owning command, preserving culture and escaping independently.
4. Test-only fixture and expected-newline cleanup after production templates, with every existing span and byte assertion retained.

This grouping preserves source locality and avoids one solution-wide whitespace-sensitive refactor. It also permits independent command-owned work after shared Shell help is settled.

## Explicit Non-Findings

- Character-by-character parsers, percent decoding, Markdown traversal, escaping, and sanitization
- Incremental path and containment algorithms
- Conditional suffix, truncation, optional-line, and natural-language list policy
- Sentinel-only conversions such as nullable values to `none` or `not applicable`
- `string.Join` used for deterministic list or fingerprint construction
- Structured logging templates; no candidate structured-logging API was found in `src/cli`
- Compile-time adjacent static literals used only for source layout

## Scope And Uncertainty

The audit covers the immutable replacement-CLI base and the binding template rule as of 2026-08-26. It does not claim that interpolation is faster, that all string concatenation is wrong, or that the listed output is currently incorrect. Line locations may move after Context and Extension integration; the named owning types and behaviors are the stable scope.

## Relevance

Without this record, later modernization may either miss the known fixed-template debt or over-apply the rule to parsers and escaping code. Keeping the findings and exceptions together supports a smaller, safer cleanup task and a more precise review.

## Follow-Up And Promotion Signals

- Discuss and authorize the cleanup only after the active Context and Extension branches are integrated.
- Freeze byte-exact output before changing each renderer or fixture group.
- Preserve exact culture providers, escaping, field order, indentation, blank lines, and final-newline behavior.
- If repeated audits find the same distinction difficult to apply, add examples to the Directive rather than banning all concatenation or incremental builders.

## Related Records And Sources

- [C# Style](../../../directives/csharp/style.md)
- [C# Callable Design](../../../directives/csharp/design.md)
- [Modern C# Improvements](../../working/cli-development/tasks/modern-csharp-improvements.md)
- [CLI Review Rationale And Anomalies](2026-08-18_cli-review-rationale-and-dogfooding-anomalies.md)
