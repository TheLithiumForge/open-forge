---
open-forge:
  description: Running the CLI against the Open Forge repository itself, plus the configuration file sprawl, the ungrantable permission model, path grammar drift, and where the engineering effort actually went
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Dogfood, Configuration, Permissions, Naming]
---

# Repository Dogfood And Configuration

## Conclusion

Run against its own repository, `doctor` produces **8.8 MB across 165,919
lines**, and the single `ERROR` — the only actionable finding — is at line
**79,038 of 79,051** in _compact_ view. `repair` crashes with a bare
`ArgumentException`.

Separately: one intended `open-forge.json` became four files; the permission
model has **no grant mechanism of any kind** outside a TTY prompt that never
fires; and the measured code volume shows exactly the misallocation suspected —
355 lines of interaction against 18,309 for recovery, mutation and locking.

## `doctor` on this repository

```
$ open-forge doctor                    165,919 lines · 8,797,701 bytes · exit 3
$ open-forge doctor --view compact      79,051 lines · 5,692,181 bytes · 7.4s
```

At roughly four characters per token the default view is **~2.2 million
tokens**. An agent running the framework's own diagnostic on the framework's own
repository exceeds any context window by two orders of magnitude. This is the
tool whose purpose is context economy.

### What the 11,455 findings are

| Count | Code                                     | Severity  |
| ----- | ---------------------------------------- | --------- |
| 5,550 | `reference.target-valid`                 | INFO      |
| 1,817 | `reference.cycle`                        | INFO      |
| 912   | `reference.repeat`                       | INFO      |
| 592   | `reference.target-missing`               | WARNING   |
| 592   | `reference.candidates-several`           | INFO      |
| 592   | `reference.candidate-route-neighborhood` | INFO      |
| 592   | `reference.candidate-literal-content`    | INFO      |
| 441   | `reference.candidate-filename`           | INFO      |
| …     |                                          |           |
| **1** | `framework.install-incomplete`           | **ERROR** |

8,378 INFO, 3,076 WARNING, 1 ERROR. **Roughly half of all output is the CLI
saying a link is fine.**

### Cycles are not a defect in this framework

```
Link: .agents/directives/_directives.md:30:3
  Destination: open-forge/_open-forge.md
INFO  Link target is valid [reference.target-valid]; information only
INFO  Links form a cycle [reference.cycle]; information only
  Bounded local-reference evidence contains a repeated occurrence or cycle.
INFO  Links form a cycle [reference.cycle]; information only
INFO  Links form a cycle [reference.cycle]; information only
INFO  Links form a cycle [reference.cycle]; information only
INFO  Links form a cycle [reference.cycle]; information only
INFO  Links form a cycle [reference.cycle]; information only
```

Two defects in nine lines. The identical finding repeats **six times** for one
link, with the explanation only on the first. And a generated `Entries` tree is
inherently cyclic — a parent lists its child, the child links back — in a
framework whose Loader axiom is _"Link to related sources instead of repeating
their detail."_ Reporting the mandated structure as a finding, 1,817 times, is a
category error.

### Proposed result

- Delete `reference.target-valid`, `reference.cycle` and `reference.repeat` as
  findings. A valid link is not a finding. Report `Links: 5,550 checked, 592
broken` and list only the 592.
- Never emit the same finding twice for one subject.
- Severity order, so the single ERROR is line 1 and not line 79,038.
- Add a hard output ceiling. No `doctor` invocation should exceed a few hundred
  lines regardless of workspace size; past that, summarise and point at
  `--view verbose` or a written report file.

## `repair` crashes

```
$ open-forge repair --automatic --dry-run                            exit 1
Post-diagnosis: complete / workspace/path=complete, route/heading=complete, ...
No files changed (--dry-run).
FAILED: Repair failed unexpectedly: ArgumentException. [repair.operation-failed]
Next: open-forge repair --verbose
Inspect the bounded failure details and rerun Repair from a fresh plan.
```

A raw .NET exception type name reaches the user as the entire diagnosis. No
message, no subject, no file. The twenty lines above it report every phase as
`not-requested` and `Post-diagnosis: complete`, then the command reports failure.

Following its own advice:

```
$ open-forge repair --automatic --dry-run --verbose
... identical output, plus one line on stderr:
status=failed; findings=1; affected-paths=0
```

`--verbose` reveals nothing. The command names it as the way to _"inspect the
bounded failure details"_ and there are none.

### Proposed result

An unhandled exception is a bug report, not a finding. Print what is known —
the phase, the subject being processed, the exception message — and a stable
issue-reporting instruction. Never surface a bare CLR type name. And make the
`Next:` command produce something, or remove it.

## `find --view compact` is a different format, undocumented

```
$ open-forge find --tag CLI --view compact | cat -A
result=incomplete<TAB>coverage=incomplete<TAB>universe=default<TAB>matches=343
directives/open-forge<TAB>.agents/directives/open-forge/_open-forge.md
directives/open-forge/cli<TAB>.agents/directives/open-forge/cli/_cli.md
```

This is TSV — and it is the composable one-line-per-hit shape that `find` should
have. Three problems.

- **It is undocumented.** `find --help` says only _"Result detail: compact or
  expanded"_, identical wording to every other command.
- **`--view` means something different here.** Everywhere else it selects detail
  level; in `find` it switches output format entirely. One flag, two meanings.
- **The TSV is contaminated.** After the rows, prose findings are appended —
  `Source: … -> …`, `Layer: base`, `Region: frontmatter`, `Next: open-forge
doctor` — so it cannot actually be piped without filtering.

### Proposed result

Keep the TSV, promote it, document it, and stop mixing prose into it. Findings
go to stderr. This partially supersedes the "make `find` composable" task in
[command-output-design.md](command-output-design.md) — the format exists and
needs surfacing rather than inventing.

## Configuration sprawl

One intended `open-forge.json` became four files.

| File                                  | Purpose                                           | Size here              | Nature    |
| ------------------------------------- | ------------------------------------------------- | ---------------------- | --------- |
| `.agents/open-forge.lifecycle.json`   | Framework and Extension install record            | **17 KB** for 26 files | generated |
| `.agents/open-forge.permissions.json` | destination allow-list                            | absent until granted   | authored  |
| `.agents/open-forge.libraries.json`   | Library registrations and links                   | absent until attached  | generated |
| `open-forge.extensions.json`          | legacy `open-forge-old` receipt, at the repo root | 21 paths               | orphaned  |

Nothing in `src/cli` reads the fourth. The other three are created lazily, so a
fresh workspace has exactly one and a user cannot discover the others exist
until a command blocks and names one.

The real problem is not the count — it is that **generated state and authored
configuration are the same kind of file**. `lifecycle.json` is 17 KB of SHA-256
baselines that no human will ever edit. `permissions.json` is a short allow-list
that only a human can write. Putting both under the same naming convention, in
the same directory, with the same `open-forge.*.json` shape, teaches the user
that all four are equally off-limits.

### Proposed result

Split by nature, not by feature.

- **`open-forge.json`** at the workspace root — authored, small, hand-editable,
  reviewed in a diff. Holds the permission allow-list, the startup token budget
  from [loading-and-scope-discipline.md](loading-and-scope-discipline.md), and
  any future settings:

  ```json
  {
    "allow": ["docs/**", { "library": "shared", "from": "shared-src", "to": "docs" }],
    "startupTokenBudget": 8000
  }
  ```

- **`.agents/open-forge.state.json`** — generated, machine-owned, one file,
  covering Framework, Extension and Library records. Documented as
  "do not edit".

- Report the legacy `open-forge.extensions.json` once, as information, and offer
  to remove it.

## The permission model cannot be granted

This is the sharpest interaction defect found.

```
$ open-forge library attach shared shared-src --to docs
Library attach is blocked.                                           exit 5
BLOCKED: The selected Library destinations require workspace permission.
         [library-attach.permission-required]
  .agents/open-forge.permissions.json
Permissions: required; action none; outcome not-requested
```

It names a file that does not exist and that the user has never been told about.
**It never asks.** Three reasons compound:

1. `WorkspacePermissionDefinitions.ImplicitPathPrefix = ".agents/"` — everything
   under `.agents` is implicitly permitted, so a normal Extension install never
   triggers the prompt. The user has correctly never seen it.
2. The prompt requires `_interaction.CanPrompt`, which is
   `!StandardInputRedirected && !PromptOutputRedirected`. Every agent, script and
   CI run is redirected, so the prompt is skipped and the command blocks instead.
3. **There is no flag, no subcommand, and no documented file format to grant it.**
   A search for any `--allow*`, `--permission*`, `--grant*` or `--approve*` option
   across the whole source returns **zero matches**.

So outside a TTY, any destination outside `.agents` is permanently unreachable.

The schema is not published anywhere. Recovering it required reading
`WorkspacePermissionDocument.cs`:

```csharp
internal sealed record ExtensionPermissionGrant(string Id, ImmutableArray<string> Paths);
internal sealed record LibraryPermissionGrant(
    string Id, string SourceRoot, ImmutableArray<string> Paths, ImmutableArray<string> Directories);
```

Hand-writing the file from that does unblock it:

```json
{ "schemaVersion": 1, "extensions": [], "libraries": [{ "id": "shared2", "sourceRoot": "shared-src", "paths": ["docs/shared.md"], "directories": ["docs"] }] }
```

```
$ open-forge library attach shared2 shared-src --to docs
Status: complete
Permissions: granted; action none; outcome not-requested
```

**The only non-interactive path to a permission grant is reading the C# source.**

### Proposed result

The grant is an allow-list of paths — exactly what was originally intended as an
`open-forge.json` entry. Concretely:

- Move grants into `open-forge.json` under `allow`, as globs.
- Add `--allow <path>` to the commands that need it, which appends to that file
  and proceeds.
- Make the block message state the fix:

  ```
  Refusing to create links outside .agents without permission.

    docs/shared.md   from shared-src

    open-forge library attach shared shared-src --to docs --allow docs
    or add "docs/**" to the "allow" list in open-forge.json
  ```

- Ask on first use in a TTY, but never let the absence of a TTY be the only
  reason a workflow is impossible.

## Path grammar drifts between commands

Same workspace, same directory, same session:

| Form                           | `route inspect` | `library attach`       |
| ------------------------------ | --------------- | ---------------------- |
| `directives` / `shared-src`    | accepted        | accepted               |
| `./.agents/…` / `./shared-src` | **accepted**    | **rejected — invalid** |
| trailing `/`                   | rejected        | rejected               |
| backslash separators           | rejected        | rejected               |

The Loader's own text advertises `./.agents/...` as a valid form. `route inspect`
honours it; `library attach` rejects it as _"requires one portable
workspace-relative source root"_ — a phrase that does not mention the `./`.

Backslash paths are rejected everywhere, which on Windows means **shell tab
completion produces a path the CLI refuses**.

### Proposed result

One shared path normalizer used by every command: strip a leading `./`, accept
either separator, strip a trailing `/`, then resolve. Path grammar is not a
per-command decision.

## Where the effort went

Measured over `src/cli/core/OpenForge.Cli.Core` (196,370 lines):

| Concern                     | Lines   | Files |
| --------------------------- | ------- | ----- |
| Recovery, Mutation, Locking | 18,309  | 151   |
| Rendering and Presentation  | 25,477  | 324   |
| **Interaction**             | **355** | **6** |

Two conclusions, and the second is the uncomfortable one.

**Interaction is 355 lines** against 18,309 for durability machinery — a ratio of
**52:1**. That is why there is no wizard, why `extension install` cannot
multi-select, why a typo loops forever silently, and why permissions cannot be
granted. It is not that interaction was built badly; it was barely built.

**Presentation is not under-resourced — it is 25,477 lines.** Those 25,477 lines
produce a 194-line "no problems found" and an 8.8 MB `doctor`. The investment
went into an elaborate, faithful projection of every internal fact, with no
editorial layer deciding what is worth saying. More presentation code is not the
fix; a smaller, opinionated one is.

Supporting evidence for the same pattern: 71 files carry `*Human*` in the name
and there are **65 distinct `*Human*` types** — `DoctorCountsHumanRenderer`,
`ExtensionUpdatePathsHumanRenderer`, `ContextFramingHumanRenderer`, and so on.
"Human" names the format by negation — _not JSON_ — and says nothing about what
the type does. A reader cannot tell `DoctorEvidenceHumanRenderer` from
`DoctorFindingHumanRenderer` from the name.

### Proposed result

- Rename `*HumanRenderer*` to `*TextRenderer*` or, better, to what it renders:
  `DoctorFindingLine`, `DoctorSummary`, `ExtensionPathList`. Mechanical, and it
  makes the presentation layer navigable for the first time.
- Treat the interaction layer as a first-class subsystem with a real budget:
  a keystroke reader, a redrawable selection list, a confirmation renderer that
  shows the plan, and a non-interactive equivalent for every prompt.
- Before adding presentation code, delete some. The Phase 2 rewrite should end
  with fewer lines than it started.

## Git

Recorded as an accepted direction rather than a finding.

There is no git gate and there should not be one. `install`, `update` and
`repair` should run on a dirty tree. The wanted behaviour is a single advisory
line when the tree is dirty and the command is about to write:

```
Installed the Open Forge Framework.

  21 files, 20 directories in .agents

  Your working tree had 12 uncommitted changes before this ran.
  Commit or stash first if you want this install reviewable on its own.
```

Informational, after the fact, never blocking, and never a reason to fail.
