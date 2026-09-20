---
open-forge:
  description: Keep stable output-text IDs separate from existing code-to-document path annotations
  tags: [Memory, Working, CLI, Output, Contextual]
---

# Output Text Identities

## Direction

Existing `@OpenForge` and `@open-forge` path comments retain their code-to-document meaning: a repository-root path to a defining document under `.agents/`. Do not reinterpret them as message IDs.

The current convention is recorded in
`.agents/memory/emerging/ideas/code-to-context-references.md`. The scoped baseline
search found no current production C# markers;
this does not remove the convention or authorize reusing its token.

Use a distinct marker for a human-wording definition:

```csharp
// @OpenForgeText status.installed.current
public static string InstalledCurrent(string version) => /* existing wording */;
```

This is a schematic signature, not new approved wording or an imposed API. S3.01 inventories existing methods and freezes actual signatures. The marker is case-sensitive, followed by one lowercase dotted identifier. Each segment starts with a letter and contains letters/digits/hyphens; require at least two segments. Give each independently referenceable wording factory one unique definition ID. IDs describe meaning, not file paths, line numbers, current method names or severity/detail combinations. Do not create IDs for machine finding codes or JSON keys under the human wording marker.

A document can point back with:

```markdown
Expected wording: `status.installed.current`.
<!-- @OpenForgeTextRef status.installed.current -->
```

The definition is found with `rg -n -F '@OpenForgeText status.installed.current' src/cli/output-text`. Documentation should also link to the factory's file when useful; the stable ID survives member relocation. Detail, format, stream and scenario remain separate facets, since a wording fragment may serve many scenarios and views.

## Reference Validation

Prefer one small validator in the existing test infrastructure, not a runtime registry, external template language or new generator project. It checks definition-ID uniqueness, marker syntax and unresolved references in the selected current docs/flows. IDs are declared only at factories; reference records do not duplicate wording or define another list of approved IDs. Start with the precise comment-line convention, not a regex for all possible C# syntax.

Inventory current annotation parsers first: a prefix match for `@OpenForge` could mistake the new marker for a path. Update such consumers as part of S3.02 with both positive and negative fixtures. If no consumer exists, record that evidence rather than creating one for the old marker.

Match complete tokens case-sensitively: `@OpenForgeText` is not
`@OpenForgeTextRef`. Use the existing architecture-test area as the first validator
candidate, and follow the final G2 test-boundary decision. When several overloads
share wording, mark one canonical factory and forward to it; do not duplicate the
definition marker or invent a second operation-enum registry in OutputText.
Limit reference validation to the declared current-document roots and ignore
fenced syntax examples. Working plans and historical illustrative IDs are not
live references merely because they demonstrate the marker.

Use file links plus stable IDs immediately. A generated browseable index is optional only if the existing reviewer workflow cannot navigate the definitions; it must contain references, not a second prose catalogue. No aliases are needed for new IDs. If an adopted ID later changes, update all references or record an explicit compatibility alias without retaining duplicate definitions.

## Wording And Evidence

Factories accept typed values or narrow OutputText-owned values and return text. Keep outcome selection, domain mappings, exception classification, escaping, terminal styling, table width, authored-content spans and JSON serialization with their present semantic owners unless a separately frozen placement requires otherwise.

Exact text remains unchanged during extraction. Independent reviewed snapshots protect wording. Tests may call an independently selected factory to check selection/substitution, but must not derive expected output through the same production selector. Multiple named snapshots can cover a scenario's steps and output views.

S3 is the point at which current contracts replace duplicated transcripts with links to factories and reviewed snapshots. Preserve behavioral requirements while removing repeated prose. Do not turn an unapproved scenario or wording proposal into current behavior by assigning it an ID.

## Baseline Extraction Inputs

The scoped inventory found 32 Wording files: 28 command files plus four shared
ones. Shared attention is needed for `CliFindingWording`, `CliPromptWording`,
`ContentPartsWording` and `RouteSourceSelectionWording`. In particular,
`CliFindingWording.PlainCause` and `Resolution` include classification; their
policy stays outside the leaf text project.

Inventory also covers 31 HelpSections files, 29 DataTextRenderer files, 27
ReportSelector files, shared prompts, command Definitions and
`Shell/Definitions/CliSyntaxDefinitions.cs`. S3.01 refreshes these measured counts
against G2; they are a starting map, not a frozen extraction list. Root's
`CliExtensionComposer` is a direct wording consumer and remains coordinator-owned.
