---
open-forge:
  description: The mandatory Axioms section, the HTML comment markers the Markdig parser makes unnecessary, the CLI documentation carried in the loader, and a simplified v1 lifecycle record
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Structure, Markers, Lifecycle, Loader]
---

# Structural Requirements And Markers

## Conclusion

Four requirements make a custom scope expensive to create, and none of them
needs to exist in its current form. Three are one-line changes; the fourth
deletes 85% of the lifecycle record.

The common thread: the CLI already has the capability to be lenient — a full
CommonMark parser, heading-based section lookup, and a `git` repository next to
it — and chooses strictness anyway.

## 1. `## Axioms` is mandatory, with non-empty content

[`RouteSourceStructureReader.ReadAxioms`](../../../../../src/cli/core/OpenForge.Cli.Core/Framework/Sources/Operational/RouteSourceStructureReader.cs#L97-L128):

```csharp
// line 102 — applies to the Loader and every entrypoint
if (form != SourceDocumentForm.Loader && !SourceFormClassifier.IsEntrypoint(form))
    return RouteAxiomsObservation.NotApplicable();

// lines 111-112 — exactly one "## Axioms", case-sensitive Ordinal
var sections = document.Sections.Where(section => section.Heading.Level == 2
    && string.Equals(section.Heading.VisibleText, "Axioms", StringComparison.Ordinal)).ToArray();
if (sections.Length == 0)
    return RouteAxiomsObservation.Boundary(RouteAxiomsState.Missing);

// lines 120-125 — and the section body must not be whitespace
if (sections.Length != 1
    || section.Span.End <= section.Heading.Span.End
    || string.IsNullOrWhiteSpace(document.Source[section.Heading.Span.End..section.Span.End]))
    return RouteAxiomsObservation.Boundary(RouteAxiomsState.Invalid, location);
```

Surfaced by
[`RouteSourceDoctorInspector.cs:28-38`](../../../../../src/cli/core/OpenForge.Cli.Core/Commands/Doctor/Shared/Domains/RouteSourceDoctorInspector.cs#L28-L38)
as `route.axioms-invalid`
([`DoctorDefinitions.cs:63`](../../../../../src/cli/core/OpenForge.Cli.Core/Commands/Doctor/DoctorDefinitions.cs#L63),
title at
[`DoctorFindingTitles.cs:53`](../../../../../src/cli/core/OpenForge.Cli.Core/Commands/Doctor/Shared/Rendering/DoctorFindingTitles.cs#L53)):
_"The route source has missing, duplicate, or empty required Axioms structure."_

### Measured

A user-authored scope at `.agents/guidance/team/_team.md`:

| Content                          | `index` | `doctor` |
| -------------------------------- | ------- | -------- |
| no `## Axioms` heading           | 0       | **2**    |
| `## Axioms` heading, empty body  | 0       | **2**    |
| `## Axioms` heading + one bullet | 0       | 0        |

It does **not** block commands — a correction to the assumption that it does.
It is a permanent `doctor` warning on every custom scope, which is as annoying
and drives exactly the wrong behaviour.

### The proof that the rule is wrong

`route init` cannot satisfy its own requirement without writing a placeholder.
[`RouteInitScaffoldComposer.cs:27-29`](../../../../../src/cli/core/OpenForge.Cli.Core/Commands/Route/Init/Shared/Planning/RouteInitScaffoldComposer.cs#L27-L29):

```
## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.
```

A rule whose own scaffolder must emit boilerplate to pass it is a rule that
should accept the empty case directly. None of the 19 shipped entrypoints uses
this placeholder — they all have real Axioms — so its only purpose is to get
generated scopes past the check.

### Proposed result

- Delete the `IsNullOrWhiteSpace` clause at line 122. A `## Axioms` heading with
  nothing under it means "no local axioms", which is the common and correct case
  for a user scope.
- Consider dropping the heading requirement too, for non-Framework scopes. The
  Loader axiom is _"A child entrypoint adds only rules for its narrower scope"_ —
  most scopes add none.
- Stop `route init` emitting the placeholder bullet once the empty case is legal.
- Whatever survives must be **autofixable**, not a manual decision. See §5.

## 2. The HTML comment markers are unnecessary

The project depends on **Markdig**, a full CommonMark parser
([`MarkdownDocumentParser.cs:1-3`](../../../../../src/cli/core/OpenForge.Cli.Core/Framework/Documents/Markdown/MarkdownDocumentParser.cs#L1-L3),
`Markdig 1.3.2` pinned in `Directory.Packages.props:7`).

The same file that requires markers already knows the heading:

[`MarkdownGeneratedRegionSyntax.cs`](../../../../../src/cli/core/OpenForge.Cli.Core/Framework/Documents/Markdown/MarkdownGeneratedRegionSyntax.cs):

```csharp
internal const string EntriesHeadingText = "Entries";
internal const string EntriesHeadingLine = "## " + EntriesHeadingText;
internal const string MarkerPrefix       = "open-forge:generated-index:";
internal const string StartMarker        = "<!-- " + MarkerPrefix + "start -->";
internal const string EndMarker          = "<!-- " + MarkerPrefix + "end -->";
```

And `## Axioms` is located **by heading, with no markers at all**
(`RouteSourceStructureReader.cs:111`). So the codebase uses two different
mechanisms for the same job — and the marker-based one is the one that breaks.

The cost of markers, all measured elsewhere in this audit:

- A trailing edit after `## Entries` puts the region "outside its accepted final
  location", producing `route.generated-region-misplaced` and, in `status`, a
  workspace-wide `blocked`
  ([presentation-field-audit.md](presentation-field-audit.md), finding 7).
- A malformed marker pair makes the semantic fingerprint fall back to exact bytes
  ([lifecycle-baselines-and-architecture.md](lifecycle-baselines-and-architecture.md)).
- The markers leak into `context` output, which is a model's payload
  ([command-output-design.md](command-output-design.md)).
- They are what `route init` renders as `<!-- … -->` in human output.

The AGENTS.md managed region has the same shape:
[`FrameworkContentIdentity.cs:12`](../../../../../src/cli/core/OpenForge.Cli.Core/Framework/Lifecycle/FrameworkContentIdentity.cs#L12) —
`private const string ManagedStart = "<!-- open-forge:start -->";`

### Proposed result

- Locate the generated region as **the content under the final `## Entries`
  heading**, using the parser already in use. No markers.
- Same for AGENTS.md: an `## Open Forge` heading whose body is managed, with one
  human sentence under it — _"This section is generated by Open Forge. Edit
  above or below it."_ — instead of an invisible comment pair.
- Accept and strip existing markers on read for one release, so current
  workspaces migrate silently.
- This removes an entire class of failure: there is nothing to misplace,
  duplicate, or half-write.

## 3. Structural problems should be autofixable

Every structural finding in this audit is currently `ManualDecision`:
missing frontmatter, missing Axioms, a misplaced Entries region, a tag
containing a space, an unquoted colon in a description.

`index` is explicitly forbidden from touching any of it
([`IndexHelpSections.cs:41`](../../../../../src/cli/core/OpenForge.Cli.Core/Commands/Index/Shared/Rendering/IndexHelpSections.cs#L41)):

> _"Index changes only valid bounded generated Entries interiors. It does not
> repair markers, format complete files, modify overwrites, search for another
> workspace, or create Git commits."_

So the command that already rewrites these files, and already fails because of
these problems, is prohibited from fixing them — and `repair` does not cover
them either
([repository-dogfood-and-configuration.md](repository-dogfood-and-configuration.md):
`repair --automatic` selects 0 findings and crashes on this repository).

### Proposed result

Introduce a **safe-autofix** class of finding, distinct from `ManualDecision`,
covering everything mechanically derivable:

| Problem                              | Fix                                                                      |
| ------------------------------------ | ------------------------------------------------------------------------ |
| missing `## Axioms`                  | insert the heading (or stop requiring it — §1)                           |
| missing `## Entries`                 | insert the heading and an empty region                                   |
| Entries not final                    | move it to the end                                                       |
| tag containing a space               | reject at write time with the offending tag named                        |
| unquoted `": "` in a scalar          | quote the value                                                          |
| missing frontmatter on a routed file | insert a stub with the filename as description, tagged `#NeedsAuthoring` |

Apply them from `index --fix` (or `repair` once it works), and have every
blocking message name the flag:

```
.agents/guidance/team/_team.md has no Entries section.

  open-forge index --fix    add it
```

The current flow — a command fails, names a code, and leaves a mechanical edit
to the user — is the single most repeated frustration in this audit.

## 4. The loader carries CLI documentation

The shipped `loader.md` is 109 lines / 9,860 bytes and is read on every task.
Lines 82-96 are a CLI command list, **1,049 bytes — 10.6% of the loader**, or
roughly 260 tokens paid on every task:

```
### CLI
- `open-forge --help`
- `open-forge context [<source-reference>...]`
- `open-forge route list [<source-reference>]`
- `open-forge route inspect <source-reference>`
- `open-forge find [options]`
- `open-forge references <source-reference>`
- `open-forge index [<source-reference>...]`
- `open-forge status`
- `open-forge doctor`
```

Six of these nine are maintenance commands a reading task never uses. This is
the same over-loading the framework warns against
([loading-and-scope-discipline.md](loading-and-scope-discipline.md)), in the one
file that is always resident.

### Proposed result

Keep in the loader only the three commands that serve _reading and selection_ —
the loader's actual job:

```
### CLI

When the Open Forge CLI is available:

- `open-forge context [<source-reference>...]` - read startup or selected context
- `open-forge find [options]` - find sources by tag or heading
- `open-forge references <source-reference>` - see what links to and from a source

Everything else — installing, indexing, diagnosing, authoring routes — is in the
`open-forge-cli` Skill. The plain files remain complete without the CLI.
```

Move the rest into a shipped `open-forge-cli` Skill, which is on-demand by
construction and is exactly what the `skills` route exists for. It also gives
the core payload its first real skill, and a worked example of the format
(see the templates gap in
[lifecycle-baselines-and-architecture.md](lifecycle-baselines-and-architecture.md)).

## 5. A simpler v1 lifecycle record

Measured on a bare install:

| Section                          | Bytes      | Share   |
| -------------------------------- | ---------- | ------- |
| `framework.targets` (43 entries) | 9,588      | **85%** |
| everything else                  | 1,749      | 15%     |
| **total**                        | **11,337** |         |

The targets exist to answer one question: _may I overwrite this file?_ The
intended design was embedded-file + baseline fingerprint, matched to decide
overwrite safety. In practice that mechanism:

- stores two baselines per file, the second an **exact-bytes hash of generated
  content**, which is what blocks Extension installs
  ([lifecycle-baselines-and-architecture.md](lifecycle-baselines-and-architecture.md));
- breaks on a trailing space, a missing final newline, and **CRLF** — the last
  meaning a Windows clone with `core.autocrlf=true` is unusable on arrival;
- pins an absolute `workspacePath`, so a clone or rename is permanently blocked
  ([presentation-field-audit.md](presentation-field-audit.md), finding 1).

Every workspace this runs in is a git repository, and git already answers
"what changed" better than a fingerprint table ever will.

### Proposed result for v1

Reduce the record to what only Open Forge knows — **what was installed, and what
the user deliberately removed** — and delegate change detection to git:

```json
{
  "schemaVersion": 2,
  "framework": {
    "version": "0.1.0",
    "inventoryFingerprint": "a71d71e6…",
    "installed": [".agents/loader.md", ".agents/maps/_maps.md", "…"],
    "removedByUser": [".agents/templates/_templates.md"]
  },
  "extensions": {
    "development": { "version": "0.1.0", "installed": ["…"] }
  }
}
```

- `installed` — so `update` knows its footprint and does not re-add what the
  user removed.
- `removedByUser` — the one fact git cannot supply, because a deletion is
  indistinguishable from "never installed".
- `inventoryFingerprint` — one hash of the _source_ payload, so `update` knows
  whether there is anything new. Not per-file.
- **No per-file baselines, no `workspacePath`.**

`update` then asks git, and says so plainly:

```
Updating 21 managed files to Framework 0.2.0.

  3 have local modifications and will be overwritten:
    .agents/maps/_maps.md
    .agents/patterns/_patterns.md
    .agents/guidance/_guidance.md

  Your working tree has these staged or committed, so `git diff` will show
  exactly what changed. Continue? [y/N]
```

Which is honest, useful, formatting-tolerant, path-independent, and removes 85%
of the file plus the two ship-blockers that live in it.

For a workspace that is not a git repository, fall back to "overwrite and tell
the user", or refuse and say why. That is a small, explicit gap rather than a
mechanism that fails everywhere.

## 6. Projection and detail, revised

Recording the accepted narrowing from
[lifecycle-baselines-and-architecture.md](lifecycle-baselines-and-architecture.md):

- `--projection text|json|tsv` is an **option**, not a mode. Default `text`.
- `--detail` absorbs `--verbose`. There is no separate verbosity flag; verbose is
  simply the highest detail level, and diagnostics that currently go to stderr
  under `--verbose` become part of that level.
- Default `--detail brief`.

That leaves two presentation flags where there are currently three
(`--json`, `--view`, `--verbose`), each meaning one thing.
