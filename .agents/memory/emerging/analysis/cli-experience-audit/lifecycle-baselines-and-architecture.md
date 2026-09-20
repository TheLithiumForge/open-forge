---
open-forge:
  description: The exact-bytes baseline over generated Entries that makes extensions uninstallable, the missing authoring templates, and the naming and layering decisions for projection, granularity, and thresholds
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Lifecycle, Fingerprint, Architecture, Templates, Naming]
---

# Lifecycle Baselines And Architecture

## Conclusion

The lifecycle record stores **two** baselines per managed file. The first is
correct and does what it should. The second pins the _generated Entries region_
as **exact bytes**, and it is the reason nothing can be installed.

Measured consequence: **you cannot install an Extension into any workspace that
has ever had a route added.** That is every real workspace.

## The baseline model

```json
{
  "path": ".agents/workflows/_workflows.md",
  "sourceAssetPath": ".agents/workflows/_workflows.md",
  "region": null,
  "baselineFingerprint": "bfd0fe3b…",
  "fingerprintKind": "semantic"
},
{
  "path": ".agents/workflows/_workflows.md",
  "sourceAssetPath": null,
  "region": "entries",
  "baselineFingerprint": "a513fce8…",
  "fingerprintKind": "exact-bytes"
}
```

43 entries for 21 files — the duplication flagged earlier as a display defect is
real, and it is in the data model.

The `semantic` entry is the design that was asked for.
`MarkdownFingerprintReader` normalizes line endings, excludes the generated
region interior, and hashes the remainder:

```csharp
var output = region.State == MarkdownFingerprintRegionState.Valid
    ? RemoveInterior(normalized, generatedRegion)
    : normalized;
```

The `region: "entries"` / `exact-bytes` entry undoes it. It fingerprints
**generated content** — content the CLI itself rewrites on every `index`,
`route init`, `route create`, `route move`, and `route remove` — and pins it to
exact bytes.

## What that costs, measured

Fresh install, then one route added anywhere in the workspace:

```
$ open-forge install --automatic
$ open-forge extension install development --automatic --dry-run
Status: complete                                                    exit 0

$ open-forge route init memory/emerging/ideas/zz
$ open-forge extension install development --automatic --dry-run
Status: blocked                                                     exit 5
BLOCKED: The lifecycle Framework target differs from its persisted baseline.
  Target: .agents/workflows/_workflows.md
```

Three facts make this conclusive:

- `.agents/workflows/_workflows.md` is **byte-identical** to a pristine install.
  Diffed against a separate clean workspace.
- The lifecycle record is **unchanged**. Diffed before and after `route init`:
  43 targets before, 43 after, **0 differing fingerprints**.
- Removing the route and reindexing **restores** it to `complete`.

So neither input to the comparison changed, and the verdict flipped anyway. The
check is not comparing the target's bytes to its stored baseline; it is deriving
an intended projection from the whole route graph, so **any route change
anywhere makes every Framework target look divergent** — and names an innocent
file while doing it.

This is the root cause of the "`route init` poisons the baseline" finding in
[presentation-field-audit.md](presentation-field-audit.md), and it supersedes it.

### A narrower correction

`extension install` does name the right file when the divergence is a real byte
change to one of its own targets — touching `maps/_maps.md`, `guidance/_guidance.md`
or `directives/_directives.md` each produced the correct `Target:` line. The
wrong-file report is specific to this projection-derived path.

## What formatting survives

Fresh install, one managed file altered, then `extension install --dry-run` and
`update --dry-run`:

| Change to `.agents/patterns/_patterns.md` | `extension install` | `update`    |
| ----------------------------------------- | ------------------- | ----------- |
| none                                      | complete            | complete    |
| one trailing space appended               | **blocked**         | **blocked** |
| **CRLF line endings**                     | **blocked**         | attention   |
| final newline removed                     | **blocked**         | attention   |
| restored                                  | complete            | complete    |

**CRLF blocks the workspace despite `NormalizeLineEndings` existing.** The
normalizer is applied when computing the semantic fingerprint and does not
protect the path that actually gates installation. On Windows,
`core.autocrlf=true` is the default: a clone rewrites every managed file to CRLF
on checkout, and the workspace is unusable on arrival.

Note also that `update` and `extension install` return **different verdicts for
the same change** — `attention` versus `blocked`. And `update`'s output lists the
divergent file three times, once as `unchanged`:

```
REQUIRES ATTENTION: Local changes were kept because replacement was not requested.
  .agents/patterns/_patterns.md
  .agents/patterns/_patterns.md
  .agents/patterns/_patterns.md: unchanged
```

### Proposed result

- **Delete the `region: "entries"` baseline entirely.** Generated content has no
  business in an integrity record. If the CLI generates it, the CLI can
  regenerate it; there is nothing to protect. This removes 22 of 43 entries,
  halves the record, and fixes the ship-blocker.
- **Compare bytes to baseline, never a derived projection.** The question
  "may I overwrite this file" is answered by the file and its baseline alone.
- **Widen semantic normalization** beyond line endings: trim trailing whitespace
  per line, normalize the final newline, and normalize frontmatter key order.
  A `prettier` run, an editor's trim-on-save, or `git` autocrlf must not change
  the verdict. This is the whole point of a _semantic_ fingerprint.
- **Never fall back to `exact-bytes` silently.** When the generated region is
  malformed the reader degrades to hashing raw bytes, so a file that is already
  in trouble becomes maximally brittle. Report the malformed region instead.
- **One verdict per state.** `update` and `extension install` must not disagree
  about the same file.

## Missing authoring templates

A bare install ships **zero templates**:

```
$ find .agents/templates -type f
.agents/templates/_templates.md          # the empty entrypoint, nothing else
```

With the `development-toolkit` Extension installed, templates exist for
`documents`, `memory`, and `planning` only. There is nothing for **directives,
guidance, patterns, maps, skills, or workflows** — six of the eight root
categories.

Two consequences.

The Loader requires every authored entrypoint to carry a description, tags,
an `## Axioms` section, and an `## Entries` region with markers — and ships no
example of that shape for most categories. That is why `route init` scaffolds
`#NeedsAuthoring` placeholders, and it is a direct contributor to the malformed
frontmatter this audit kept producing: writing this report, a tag with a space
in it (`Progressive Disclosure`) was rejected as _"the source frontmatter or
metadata shape is malformed"_ with no further guidance.

And the templates that do exist arrive from an **Extension**, not the core. The
base install has no authoring support at all, while `templates` is simultaneously
the one root route not marked `#LoadNow`. It is empty and unloaded.

### Proposed result

- Ship a template per root category in the core payload: `directives`,
  `guidance`, `maps`, `patterns`, `skills`, `workflows`, plus `memory` and
  `documents`. One entrypoint template and one leaf template each.
- Make `route init` and `route create` scaffold **from the template for the
  target category**, not from a generic stub. The scaffold is then correct by
  construction and the `#NeedsAuthoring` placeholder becomes a description the
  author only has to sharpen.
- Write the templates and workflows in **user-facing, generic language**. They
  should read as "how you record a decision", not "how Open Forge stores a
  Decision". The shipped content is the first thing every user reads and it
  currently describes the framework's internals rather than the user's work.

## Naming and layering

### `--json` is a projection

`--json` is not a view of the same information at a different depth; it is a
**projection of the data model into a machine-consumable shape**. The codebase
already calls it that internally — `ExtensionInspectJsonDocumentProjector`.

Proposed: `--projection json|tsv|text`, defaulting to `text`. That makes room
for the TSV shape `find --view compact` already emits
(see [repository-dogfood-and-configuration.md](repository-dogfood-and-configuration.md))
and stops `--view` from meaning "format" in one command and "detail" everywhere
else. Every projection should be pipeable by construction: data on stdout,
diagnostics on stderr, no prose interleaved.

### `--view` is granularity

`compact` and `expanded` describe rendering density, not what the reader wants.
The distinction that matters is _how much detail_, and the current names hide
that the third tier — today's default — is really diagnostics.

Proposed: `--detail brief|normal|full`, or keep `--view` with
`brief|standard|verbose`. Either is better than `compact|expanded` because both
current names describe the ink, not the information. Whichever is chosen, the
flag must mean exactly one thing in every command.

### Default to the smallest useful level

Default `--detail brief`. Most invocations are a person checking state or an
agent reading a result, and both are better served by one line. A user who wants
more asks for it; a user drowning in 194 lines cannot ask for less without
knowing the flag exists.

This is also the token argument: an agent that runs `doctor` and receives one
line has spent nothing, and can escalate to `--detail full` on the rare occasion
it needs to.

### The presentation layer is a View

The measured shape supports this: 25,477 lines across 324 files rendering, and
the output is still wrong. The problem is not rendering capacity, it is that
every renderer independently decides what to say, so nothing decides what to
_omit_.

Proposed structure:

- **Model** — the existing typed result records. Already good, and already the
  thing the JSON projection serialises. Keep.
- **View selection** — one layer that takes a result model plus a detail level
  and returns _the subset worth showing_. This is the layer that does not exist.
  It owns: severity ordering, suppression of healthy findings, zero-count
  collapsing, truncation with a "…and N more" line, and the per-command size
  budget.
- **Renderers** — dumb formatters over the already-selected subset. Small,
  uniform, and mostly shared: a finding line, a key/value block, a tree, a table.

Two properties follow. Adding a fact to the model no longer changes any output
until the selection layer opts it in — the inverse of today, where every new fact
appears everywhere. And a size budget becomes enforceable in one place instead of 324.

Rename `*HumanRenderer*` as part of the same pass; see
[repository-dogfood-and-configuration.md](repository-dogfood-and-configuration.md).

### Thresholds belong in configuration

Several judgments are currently hard-coded or absent: what counts as an over-long
output, when a startup context is too large, when a finding is worth surfacing at
`brief`. These are workspace policy, not code.

Proposed, in the authored `open-forge.json`:

```json
{
  "thresholds": {
    "startupTokens": { "warn": 8000, "error": 16000 },
    "outputLines": { "warn": 200 },
    "routeDepth": { "warn": 6 },
    "scopeTokens": { "warn": 4000 },
    "brokenLinks": { "error": 1 }
  }
}
```

`doctor` reads them and reports against them; the scenarios layer asserts them;
the defaults ship in the core payload so an unconfigured workspace still gets
sensible behaviour. This also gives the loading-discipline work in
[loading-and-scope-discipline.md](loading-and-scope-discipline.md) a real
enforcement point rather than an axiom.
