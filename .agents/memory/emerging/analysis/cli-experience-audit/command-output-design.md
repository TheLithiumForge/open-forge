---
open-forge:
  description: Proposed output for every command across each semantic status, with a three-tier view model and the shared rules that generate it
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Presentation, Design, ProgressiveDisclosure]
---

# Command Output Design

## Conclusion

Current output is organised around the program's internal state machine. It
should be organised around three questions, in this order:

1. **What happened?**
2. **What is wrong?**
3. **What should I do?**

Everything the CLI prints today that answers none of those belongs behind
`--view verbose`. On a healthy workspace that is most of it.

## The view model

Three tiers replace the current two. `expanded` is renamed `standard` and
becomes the default; today's expanded content becomes `verbose`.

| View                 | Audience                 | Rule                                                                                                                 |
| -------------------- | ------------------------ | -------------------------------------------------------------------------------------------------------------------- |
| `compressed`         | agents, scripts, CI      | The answer only. One line where one line suffices. No headers, no workspace echo, no zero-counts.                    |
| `standard` (default) | a person at a terminal   | The answer, plus anything that is actually true and actionable. No phase names, no fingerprints, no `not-requested`. |
| `verbose`            | debugging the CLI itself | Everything, including today's `Effects`, `residual`, `Preflight`, `Diagnosis coverage`, and per-finding evidence.    |

`--json` becomes a format, not a tier: `--json` implies the full data model at
whatever detail `--view` selects, minified, one schema version. The current
behaviour where `--view` silently switches between schemaVersion 1 and 2 must go.

## Six rules that generate most of the design

**R1 — Lead with the answer, in a sentence.** The first line is a complete
statement a person can act on. `Status: complete` as a separate line is the same
fact a third time (headline, enum, exit code); drop the enum from human output.

**R2 — Never print what is fine.** Valid links, unchanged files, `residual:
none`, `not-requested`, `not-applicable`, zero counts. Replace with one summary
line per category and then only the exceptions. This single rule removes roughly
120 of `doctor`'s 194 lines.

**R3 — Severity order, always.** Errors, then warnings, then a count of
informational findings. Today `doctor` prints 19 valid links before 2 broken
ones.

**R4 — Every finding names its subject.** File, line, and column where they
exist. A finding that cannot name a subject is not actionable and must not block.

**R5 — `Next:` is a command that works.** Copy-paste must run. It must be able
to answer the question that was asked; `--help` cannot answer "which IDs exist".

**R6 — Echo context only when it is surprising.** `Workspace:` and `Selected
by:` are worth printing when `--workspace` was used, when discovery walked up
from a subdirectory, or when the result is bad. Otherwise they are two lines of
tax on every invocation.

## Per command

Each block shows `standard` unless marked. Byte counts compare against measured
current output.

### `status`

Current: 104 lines / 4,238 B on a healthy workspace, including every managed
file listed twice.

**complete**

```
Open Forge is current.  20 routes · 19 startup files · ~8.3k tokens (81% of context)
```

`compressed`: the same line.

**attention / incomplete**

```
Open Forge is installed, with 2 things to look at.

  .agents/maps/_maps.md          content after the Entries section
  .agents/skills/pdf/SKILL.md    unsupported frontmatter key 'license'

  open-forge doctor    for detail
```

**blocked**

```
Open Forge cannot verify this workspace.

  The installation record was written for a different path:
    recorded  D:\src\myrepo
    current   D:\work\myrepo

  This happens after a clone, move, or rename.
  open-forge repair --rebind    to point the record at this workspace
```

The current output says `Open Forge is installed.` above `Status: blocked` and
never states the cause at all in compact view.

**Removed from standard:** the per-file `current` roster (20–43 lines), the
duplicate Framework listing, `characters` (identical to `bytes` in every
observed run), `Root categories`, `Added: none`, `Removed: none`, the Libraries
block when no library is registered, `Recovery` when both counts are zero.

**Removed from `--json`:** the 43 `baselineFingerprint` / `sourceAssetPath` /
`region` / `fingerprintKind` entries, ~10 KB of lifecycle bookkeeping that no
consumer of `status` needs. Move to `doctor --json --view verbose`.

### `doctor`

Current: 194 lines / 6,491 B on a healthy workspace; 114 lines in `compact`.
20 of its findings are "Link target is valid".

**complete**

```
No problems found.  6 checks · 21 links · 20 routes
```

**attention**

```
2 problems.

  .agents/loader.md:105          broken link -> patterns/_patterns.md
                                 the target does not exist
  .agents/maps/_maps.md:32       broken link -> nowhere/_nope.md

  open-forge repair              fix both automatically
```

**incomplete** — a check could not finish

```
1 problem, and 1 check could not finish.

  .agents/skills/pdf/SKILL.md    unsupported frontmatter key 'license'
                                 remove the key, or see: open-forge help skills

  Extensions: not checked — the installation record is missing.
```

`compressed`

```
2 errors, 3 warnings. .agents/loader.md:105, .agents/maps/_maps.md:32, .agents/skills/pdf/SKILL.md
```

**Rules applied:** R2 removes every `reference.target-valid` finding and every
`Findings: 0 errors, 0 warnings` / `Resolution: 0 exact repairs...` pair, which
currently repeat six times. R3 puts broken links above valid ones. The
`Resolution:` / `Observed:` / `Read from:` triplet moves to verbose.

`doctor` must also become the single source of truth: if any command blocks,
`doctor` has that finding; if `doctor` is empty, nothing blocks.

### `install`

Current: 57–60 lines, including 44 effect lines each repeating `residual: none`
and a `source:` identical to its own path.

**complete**

```
Installed the Open Forge Framework.

  21 files, 20 directories in .agents
  AGENTS.md, CLAUDE.md    Open Forge section added; your content kept
```

**complete, no change**

```
Already current. Nothing to do.
```

**dry-run**

```
Would install the Open Forge Framework into D:\work\myrepo

  .agents/                 20 directories, 21 files
  AGENTS.md, CLAUDE.md     an Open Forge section is added; existing content kept

  Nothing else is touched.  --view verbose lists every path.
```

**invalid — confirmation unavailable**

```
Install needs confirmation, and this session cannot prompt.

  open-forge install --automatic       proceed without asking
  open-forge install --dry-run         see the plan first
```

Three lines instead of sixty. Today the full 44-line plan prints first, every
line reading `not-started`, before the refusal.

**Interactive confirmation** must render the plan _before_ asking (see
[interaction-layer.md](interaction-layer.md)).

**Pre-existing unroutable content** must not block; report it afterwards:

```
Installed the Open Forge Framework.

  3 existing files under .agents are not routed and were left alone:
    .agents/skills/pdf-processing/SKILL.md         unsupported key 'license'
    .agents/skills/pdf-processing/references/*.md  no frontmatter

  Open Forge ignores them. They do not affect anything else.
```

### `update`

**complete, no change**: `Framework is current. Nothing to do.`

**complete, with change**

```
Updated 3 managed files.

  .agents/guidance/_guidance.md      new upstream content
  .agents/patterns/_patterns.md      new upstream content
  .agents/workflows/_workflows.md    new upstream content

  2 files you changed were kept:
    .agents/maps/_maps.md            --force to overwrite
    .agents/loader.md                --force to overwrite
```

The last block is the one thing a user genuinely needs from `update` and it is
currently buried in the effects list.

### `index`

Current output is already close to right (8 lines). Two fixes.

**complete, no change**: `Generated Entries are current. 21 regions.`

**complete, with change**

```
Updated 2 of 21 generated Entries.

  .agents/memory/emerging/ideas/_ideas.md      +1 entry
  .agents/skills/_skills.md                    +1 entry
```

**blocked** — must name the leaf, not the parent (currently names the parent):

```
Cannot rebuild .agents/skills/_skills.md.

  .agents/skills/pdf/SKILL.md    unsupported frontmatter key 'license'

  Nothing was written. The other 20 regions are already current.
```

Also fix the arithmetic: current output prints
`Regions: 20; updates: 0; already current: 19; verified: 19` — 20 regions, 19
accounted for.

### `repair`

Current: 19–21 lines of internal phase names, every one a zero or
`not-requested`, and it reports `complete` / `verified` after doing nothing.

**complete, nothing to do**

```
Nothing to repair.
```

**complete, repaired**

```
Repaired 2 links.

  .agents/loader.md:105        -> .agents/patterns/_patterns.md   restored
  .agents/maps/_maps.md:32     -> removed (no target found)
```

**incomplete — cannot repair automatically**

```
Repaired 1 of 3 problems.

  fixed     .agents/loader.md:105    -> patterns/_patterns.md

  needs a choice
            .agents/maps/_maps.md:32 -> nowhere/_nope.md
            candidates: none found
            open-forge repair --relink .agents/maps/_maps.md:32 <target>

  cannot fix
            .agents/skills/pdf/SKILL.md   unsupported frontmatter key 'license'
            remove the key by hand
```

**The critical rule:** `repair` must never report `complete` while `doctor` has
unresolved findings, and must never print `Verification: targets verified;
bytes verified; post-conditions verified` for a run that selected zero findings.
That line currently tells users a blocked workspace has been fixed.

**Removed from standard:** `Diagnosis coverage` (printed twice, once as
`Post-diagnosis`), `Preflight`, `Application`, `Plan: 0 steps / 0 effects /
0 no-ops`, `Selection details`, `Recovery: not-required; residual none (none)`.

### `cleanup`

**complete, nothing to do**: `No recovery data to remove.`

**complete, removed**

```
Removed 3 recovery items.  1.2 MB

  2 verified bundles, 1 incomplete draft
```

Current output never states what was removed; it lists `Candidate check`,
`Preflight`, `Workspace lock: not-requested`, `Final workspace check:
not-requested`, `Verification: verified`.

### `context`

Current: 845 lines / 37,925 B, exits **2** on a pristine install, and injects
field labels into the document stream.

This command's output _is_ its payload — it is read by a model. Presentation
rules differ:

- No `Status:`, no `Workspace:`, no per-source metadata header in the stream.
- Field labels (`Body: available`, `[Frontmatter: missing]`, `Route: none`,
  `Order: 15`) must never appear between documents.
- Strip the `<!-- open-forge:start -->` / `:end` and
  `<!-- open-forge:generated-index:start -->` markers.
- Emit one delimiter per source and nothing else:

```
=== .agents/loader.md (loader) ===
<content>

=== .agents/memory/_memory.md (memory) ===
<content>
```

- Diagnostics go to **stderr**, never into the payload.
- `--view compressed` should drop `- none - No entries - #Empty` blocks and
  empty Axioms sections, which currently repeat for every empty folder.
- The frontmatter warning on the shipped `AGENTS.md` and `loader.md` must be
  fixed at the source, not warned about on every run. `context` on a clean
  install must exit 0.

A summary belongs on stderr:

```
19 sources · 33,016 characters · ~8,254 tokens
```

### `find`

Current: 4–5 lines per hit, `Matched: none` on every result when no filter is
given, and a `Search details:` block echoing the query back.

`find` is the one command whose output people pipe. One line per hit:

```
$ open-forge find --tag Memory
memory                        .agents/memory/_memory.md                     Self-growing Markdown memory for active work
memory/archived               .agents/memory/archived/_archived.md          Useful history that no longer controls current work
memory/crystallized           .agents/memory/crystallized/_crystallized.md  Accepted knowledge that should remain current
```

**no matches**

```
No sources match --tag Zzz.

  Tags in this workspace: Analysis, Archived, Candidate, Core, Crystallized,
  Decision, Directive, Guidance, Map, Memory, Pattern, Skill, Template, Workflow
```

That answers the question. The current `Search details:` block does not, and it
has three different label alignments in eleven lines.

Add `--paths` for bare paths, so `open-forge find --tag Skill --paths | xargs
grep ...` works.

### `references`

Current: 24 lines listing every file it opened, before a 2-line answer that is
wrong — it reports 0 links for every source in a stock workspace because it
ignores generated Entries.

**complete**

```
.agents/memory/_memory.md

  in   1    .agents/loader.md:104
  out  4    archived/_archived.md, crystallized/_crystallized.md,
            emerging/_emerging.md, working/_working.md
```

**no links**

```
.agents/patterns/_patterns.md  —  no links in or out.
```

The `Inspected:` trace moves to `--view verbose`. And the correctness bug must
be fixed: either count generated links, or say
`4 generated links not shown — use --generated` rather than claiming
`coverage complete / No direct links found`.

### `route list`

Current: 99 lines for 13 routes, 6 metadata lines each, tags rendered as JSON
arrays, `Workspace:` printed with doubled backslashes.

**complete**

```
directives                      Required instructions loaded through selected routes
guidance                        Advice for recurring choices, tradeoffs, and work situations
  adaptive-collaboration        Explore ideas, match depth to the decision, integrate outcomes
maps                            Concise maps to important local and external sources
memory                          Self-growing Markdown memory for active work
  archived                      Useful history that no longer controls current work
  crystallized                  Accepted knowledge that should remain current
  ...

13 of 20 routes (depth 1).  --depth=all for the rest.
```

Indentation already encodes `Parent`, `Depth`, and `Selected as`, so all three
lines go. `Kind` and `Source: authored entrypoint; overwrite: none` go to
verbose. Tags print natively (`#LoadNow #Core`) or not at all at this tier.

The final line fixes a real discoverability problem: today the header says
`Coverage: complete` for a listing truncated to depth 1, with no hint that
7 routes are hidden or that `--depth=all` exists.

**invalid — unknown ID**: see [interaction-layer.md](interaction-layer.md).

### `route inspect`

**Keep the current structure.** It is the best-designed output in the CLI:
plain-language section headings — _Where this source belongs_, _When it is
read_, _Context size_ — each answering something a person actually wondered.
Every other command should be restructured this way.

Three defects to fix: the doubled backslashes in `Workspace:`, the raw `0xFA`
byte used as a separator (invalid UTF-8, renders as `ú` through any pipe), and
`1 files`.

### `route init` / `create` / `update` / `move` / `remove`

**complete**

```
Created .agents/memory/emerging/ideas/pricing/_pricing.md

  Listed in .agents/memory/emerging/ideas/_ideas.md
  The description and tags are placeholders — edit them before relying on this route.
```

Not exit 2. Creating a stub that needs authoring is the purpose of `route init`.

**Removed from standard:** the `Unchanged:` roster of every untouched file in
the workspace (22 lines today, unbounded in a real repo); the `Before:` /
`Expected:` payloads, which render as escaped file bodies with literal `\n` and
`\u003C` in `route init` and as raw SHA-256 hashes in `route create`. Neither is
useful; both go to verbose, and verbose should render a diff.

Also fix: `_ideas.md` currently appears in both `Effects` and `Unchanged`, and
`Description:` echoes empty even when `--description` was supplied.

### `extension list` / `install` / `remove`

**list, complete**

```
Installed   none

Available   development           Debugging and review workflows
            development-toolkit   Documents, memory starters, planning        5 packages
            memory-starters       Starter memory documents
            ...

  open-forge extension install <id>
```

**install, complete**

```
Installed the development Extension.

  3 workflows in .agents/workflows
  Listed in .agents/workflows/_workflows.md
```

**install, blocked** — must name the file that actually differs, and must be
right about it. Today it names `.agents/workflows/_workflows.md`, which is
byte-identical to a pristine install, after `route init` has run anywhere in the
workspace:

```
Cannot install — a managed file has changed since it was installed.

  .agents/workflows/_workflows.md    differs from its recorded baseline

  open-forge update            take the current Framework version
  open-forge doctor --verbose  see the difference
```

Selection needs a real multi-select
(see [interaction-layer.md](interaction-layer.md)).

### `library list` / `attach` / `sync` / `detach`

`library list` at 7 lines is already correctly sized. Drop `Source inventory:
not scanned (library list checks registered links only)` and the `Record:
missing` line when nothing is registered:

```
No Libraries registered.

  open-forge library attach <id> <source-root>
```

## Cross-cutting fixes

These are not per-command and should be done once, centrally.

| Fix                                                                                 | Affects                                     |
| ----------------------------------------------------------------------------------- | ------------------------------------------- |
| Drop the `Status: <enum>` line from human output (R1)                               | every command                               |
| Print `Workspace:` / `Selected by:` only when surprising (R6)                       | every command                               |
| Suppress every zero-count, `not-applicable`, `not-requested`, `residual: none` (R2) | every command                               |
| Sort findings by severity (R3)                                                      | doctor, route list, index, install          |
| Require a non-null subject on any blocking finding (R4)                             | install, index, extension                   |
| One JSON schema, minified by default, `--view` selects detail only                  | `--json`                                    |
| Emit UTF-8, never OEM bytes                                                         | route inspect                               |
| Never emit JSON escaping in human text (`\\`, `\n`, `\u003C`)                       | route list, route inspect, find, route init |
| Move the exit-code and stream tables out of every command's help                    | every `--help`                              |
| Separate `Options` from `Global options`                                            | every `--help`                              |
| Make `--verbose` do something on `status`, `doctor`, `library list`                 | those three                                 |
| Use dim and bold to build hierarchy, not just semantic colour                       | every command                               |

## Expected effect

Measured current output versus the designs above, on a healthy workspace.

| Command                   | Now (standard) | Proposed | Reduction |
| ------------------------- | -------------- | -------- | --------- |
| `status`                  | 104 lines      | 1 line   | 99%       |
| `doctor`                  | 194 lines      | 1 line   | 99%       |
| `route list`              | 99 lines       | 15 lines | 85%       |
| `install`                 | 57 lines       | 4 lines  | 93%       |
| `install` (refused)       | 60 lines       | 4 lines  | 93%       |
| `repair` (nothing to do)  | 19 lines       | 1 line   | 95%       |
| `cleanup` (nothing to do) | 9 lines        | 1 line   | 89%       |
| `references`              | 34 lines       | 4 lines  | 88%       |
| `find` (12 hits)          | 77 lines       | 12 lines | 84%       |
| `status --json`           | 25,203 B       | ~2,000 B | 92%       |

The information does not disappear. It moves to `--view verbose`, where the
current default belongs.
