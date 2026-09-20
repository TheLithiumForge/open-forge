---
open-forge:
  description: Every place YAML or Markdown is parsed by hand instead of by the libraries already depended on, with the measured defects each one produces
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Parsing, Markdown, Yaml, Correctness]
---

# Hand-Rolled Parsing

## Conclusion

The project depends on **Markdig 1.3.2** and **YamlDotNet 18.1.0**, and both are
used correctly in the places that matter most. But there are **four hand-rolled
parsers** sitting alongside them, and three produce measured, reproducible
defects — including one that makes a file with valid frontmatter report
_"Required source metadata is missing"_ because of three invisible bytes.

The most damaging pattern is not a single bad parser. It is that **the same
question is answered twice by different code**, so two commands disagree about
the same file.

## Inventory

| Location                                                                            | Lines | Parses                 | Verdict                                         |
| ----------------------------------------------------------------------------------- | ----- | ---------------------- | ----------------------------------------------- |
| `Framework/Documents/Yaml/YamlDocumentParser.cs`                                    | 149   | YAML                   | **fine** — wraps YamlDotNet's real event stream |
| `Framework/Documents/Markdown/MarkdownDocumentParser.cs`                            | 150   | Markdown               | **fine** — uses Markdig                         |
| `Framework/Documents/Markdown/MarkdownFrontmatterParser.cs`                         | 78    | frontmatter fences     | **defective** — §1                              |
| `Commands/Route/Inspect/Shared/Profile/RouteInspectAxiomsProfileBuilder.Parsing.cs` | 77    | `## Axioms` sections   | **defective** — §2                              |
| `Commands/Route/Inspect/Shared/Profile/RouteInspectMarkdownStructure.cs`            | 79    | code fences            | supports §2                                     |
| `Framework/Documents/Markdown/MarkdownGeneratedRegionParser.cs`                     | 227   | `<!-- … -->` markers   | **removable** — §3                              |
| `Framework/Sources/Metadata/SourceAuthoredMetadataParser.cs` (`ParseSkill`)         | ~70   | YAML node tree by hand | **defective** — §4                              |
| `scripts/agent-tooling/agent-projections/*.ts`                                      | —     | frontmatter by regex   | **defective** — §5                              |

`YamlDocumentParser` and `MarkdownDocumentParser` are the good cases and should
be the only entry points. Everything else either duplicates them or bypasses
them.

## 1. `MarkdownFrontmatterParser` — fence detection by string equality

Frontmatter is located by scanning for a line exactly equal to `---`:

```csharp
// MarkdownFrontmatterParser.cs:12-16
if (!TryReadLine(source, 0, out var openingLine) || !string.Equals(
        source[openingLine.Start..openingLine.End], "---", StringComparison.Ordinal))
```

Measured against a routed file whose frontmatter is otherwise valid and complete:

| Input                                        | Result           |
| -------------------------------------------- | ---------------- |
| `---` (baseline)                             | complete         |
| `--- ` — trailing space on the opening fence | **not detected** |
| `--- ` — trailing space on the closing fence | **not detected** |
| **UTF-8 BOM before the fence**               | **not detected** |
| `...` — the YAML document-end marker         | **not detected** |

Markdig's frontmatter extension tolerates all of these. Every one is produced by
ordinary tooling.

**The BOM case is the serious one on Windows.** Visual Studio, Notepad, and
`Out-File` / `Set-Content` in Windows PowerShell all write a UTF-8 BOM by
default. A user who opens a routed `.md` in one of those, changes nothing, and
saves gets:

```
Unresolved: .agents/guidance/probe.md: Required source metadata is missing.
WARNING  Required route metadata is missing [route.metadata-required-missing]
```

The frontmatter is right there in the file, visibly correct. The message names
the wrong problem, and the cause is invisible in every editor.

**Fix:** use Markdig's `UseYamlFrontMatter()` extension — the pipeline is already
constructed in `MarkdownPipelineFactory.cs` — and strip a BOM before parsing.
The 78-line file goes away.

## 2. Two implementations of "find the `## Axioms` section" that disagree

This is the more instructive defect, because both implementations are reachable
from the CLI at the same time.

**Implementation A**, via Markdig's parsed section tree:

```csharp
// RouteSourceStructureReader.cs:111
var sections = document.Sections.Where(section => section.Heading.Level == 2
    && string.Equals(section.Heading.VisibleText, "Axioms", StringComparison.Ordinal))
```

**Implementation B**, by splitting lines:

```csharp
// RouteInspectAxiomsProfileBuilder.Parsing.cs:23-27
var lines = normalized.Split('\n', StringSplitOptions.None);
var headings = Enumerable.Range(0, lines.Length)
    .Where(index => structural[index] && lines[index] == "## Axioms")
```

`lines[index] == "## Axioms"` is exact string equality on a raw line.

Measured on one file, four CommonMark-valid ways to write the same heading:

| Heading                       | `doctor` (A) | `route inspect` (B)           |
| ----------------------------- | ------------ | ----------------------------- |
| `## Axioms`                   | valid        | `substantive local Axioms`    |
| `## Axioms ` (trailing space) | valid        | **`no local Axioms section`** |
| `##  Axioms` (two spaces)     | valid        | **`no local Axioms section`** |
| `## Axioms ##` (closed ATX)   | valid        | **`no local Axioms section`** |

Markdig normalizes all four. The hand-rolled check matches only the first, so
**`route inspect` reports "no local Axioms section" for a file that
demonstrably has one, while `doctor` says it is fine.**

The consequence is not cosmetic: `route inspect`'s _"Applicable rules (Axioms) —
Local / Inherited from"_ is the output a user reads to understand what governs a
scope. A trailing space — invisible, and inserted by many editors — silently
inverts it.

**Fix:** delete implementation B and call the structure reader. That also removes
`RouteInspectMarkdownStructure.cs`, a 79-line hand-rolled code-fence tracker that
exists only to support it.

**The general rule this argues for:** structure questions have exactly one
answer, computed once in `Framework/Documents`. A command that needs to know
about headings, sections, fences or frontmatter asks; it never re-derives.

## 3. `MarkdownGeneratedRegionParser` — 227 lines that should not exist

Already covered in
[structural-requirements-and-markers.md](structural-requirements-and-markers.md):
the generated region is located by scanning for `<!-- open-forge:generated-index:start -->`
and its closing marker, while `## Axioms` in the same codebase is found by
heading. Two mechanisms for one job, and the marker one is what produces
misplacement findings, the exact-bytes fingerprint fallback, and escaped `<`
in `route init` output.

Locating the region as _the content under the final `## Entries` heading_ deletes
this file and an entire failure class with it. Listed here for completeness
because it is the largest hand-rolled parser in the tree.

## 4. `ParseSkill` — walking the YAML tree by hand

`SourceAuthoredMetadataParser.TryReadSkillValues` iterates the parsed node tree
and returns `false` on any key it does not recognise:

```csharp
switch (key)
{
    case "name" when name is null:        …
    case "description" when description is null: …
    default: return false;       // → Malformed → blocked workspace
}
```

Two problems, and the second is the one that matters.

**It policies keys**, which the accepted framework documents forbid and which the
general path does not do. Measured: ordinary Markdown frontmatter accepts
`license`, `allowed-tools`, `author`, and nested objects without complaint.
`SKILL.md` alone rejects them.

**It is a second reading path.** `FrameworkDocumentMetadataParser` reads metadata
through YamlDotNet deserialization into typed models
(`FrameworkOpenForgeMetadataYamlModel` — `description`, `tags`,
`responsibility`; `FrameworkAuthoredMetadataYamlDocument` — plus root `name` and
`description`). `SourceAuthoredMetadataParser` reads the same concept by walking
an event-derived node tree by hand. The strict-key bug lives in the second path
and could not exist in the first.

**Fix:** read `open-forge.*` if present, fall back to root `name`/`description`,
ignore every other key. Preferably by deleting the hand-walk and using the typed
model that already exists.

## 5. Regex frontmatter in the TypeScript tooling

`scripts/agent-tooling/agent-projections/agent-model-policy.ts` and
`patch-codex-agent-models.ts`:

```ts
const frontmatter = text.match(/^---\s*\r?\n([\s\S]*?)\r?\n---\s*(?:\r?\n|$)/)?.[1];
const match = frontmatter.match(new RegExp(`^${escaped}\\s*:\\s*(.+)$`, "m"));
```

YAML by regular expression. It breaks on quoted values containing `:`, block
scalars, nested keys, comments, multi-line values, and anchors — the same
unquoted-colon class that already broke 66 committed files on the C# side.

There is **no YAML or Markdown library in the TypeScript dependency set** at all,
so this was written by hand out of necessity rather than preference.

Lower severity — this is delivery tooling, not the shipped CLI — but it is the
same pattern, and worth noting that it is _more_ lenient than the C# parser: the
regex tolerates `---\s*` where `MarkdownFrontmatterParser` does not. **The two
sides of the project disagree about what a frontmatter fence is.**

**Fix:** add a YAML dependency and parse properly, or narrow these scripts to
read a single well-known key and document that limitation explicitly.

## Recommended rule

One sentence, worth adding to the C# directives:

> Markdown and YAML are parsed only by `Framework/Documents`. No command,
> renderer, or script re-derives document structure from strings.

The measurable test is cheap and could be a lint rule: outside
`Framework/Documents`, no `StartsWith("#`, no `== "## `, no `"---"`, no
`Split('\n')` over document content.

Today that rule has exactly two violations in the CLI
(`RouteInspectAxiomsProfileBuilder.Parsing.cs` and its fence tracker) and two in
the scripts. Small enough to fix now and cheap to keep enforced.
