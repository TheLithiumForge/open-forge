---
open-forge:
  description: A method for classifying every finding code into a severity, the division of labour between index and doctor, and the extension source and reference resolution findings that support it
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Doctor, Index, Severity, Taxonomy]
---

# Severity And Command Division

## 1. Classifying every finding code

The inventory pass is real work, but it should not be done code-by-code from
intuition. Three questions, asked in order, determine the answer mechanically —
and they fall out of the four-kinds model in [finding-model.md](finding-model.md).

### The decision procedure

**Q1 — Is this a problem with the workspace, or a statement about the tool?**

If it describes what the CLI did or did not do, it is not a finding.
`reference.external-unchecked` (98 occurrences here) says _Open Forge does not
fetch URLs_ — a property of the tool. `reference.repeat` and `reference.cycle`
report traversal mechanics. These leave the severity ladder entirely: they become
a coverage line, a one-clause disclosure, or nothing.

**Q2 — Is it the problem, or something attached to the problem?**

Proposals and evidence are not siblings of a finding. `candidates-several`,
`candidate-filename`, `candidate-route-neighborhood`, `candidate-literal-content`
and `candidate-title` — **2,305 warnings, 75% of all warnings here** — are the
suggested fixes for the 594 broken links and the reasons behind them. They nest,
inherit the parent's severity, and never appear alone.

**Q3 — What happens if the user ignores it?**

Only now does severity apply, and there are exactly four answers:

| If ignored…                                       | Severity    | Exit contribution    |
| ------------------------------------------------- | ----------- | -------------------- |
| an operation will fail or data is wrong           | **error**   | blocks               |
| something will behave unexpectedly, later         | **warning** | attention            |
| nothing; it is context for a decision             | **info**    | none                 |
| nothing; it is only useful when debugging the CLI | **debug**   | none, `verbose` only |

`debug` is worth adding as a fourth level, and it is where most of the current
`informational` set belongs — not because it is unimportant, but because its
audience is someone diagnosing Open Forge, not someone using it.

### Applied to the measured set

| Code                              | Q1             | Q2           | Result                  |
| --------------------------------- | -------------- | ------------ | ----------------------- |
| `workspace.loader-missing`        | problem        | the problem  | **error**               |
| `reference.target-missing`        | problem        | the problem  | **warning**             |
| `workspace.frontmatter-malformed` | problem        | the problem  | **warning**             |
| `reference.fragment-missing`      | problem        | the problem  | **warning**             |
| `route.axioms-invalid`            | problem        | the problem  | **info**, configurable  |
| `reference.same-target-path`      | observation    | —            | **info**                |
| `reference.candidate-*` (5 codes) | problem        | **attached** | nested in parent        |
| `reference.target-valid`          | tool statement | —            | **coverage count**      |
| `reference.cycle` / `repeat`      | tool statement | —            | **debug** or removed    |
| `reference.external-unchecked`    | tool statement | —            | **one disclosure line** |

Two rules make the inventory pass fast rather than philosophical:

- **Q1 and Q2 remove most codes before severity is considered.** On this
  repository they account for 10,861 of 11,455 findings. Do those two passes first
  and the remaining severity decisions are few and obvious.
- **Anything that survives to Q3 and answers "nothing happens" is either `info`
  or `debug`**, and the test between them is _who is the audience_ — the user, or
  someone debugging the CLI.

### Configurability is part of the taxonomy

Some codes are legitimately house style rather than correctness —
`route.axioms-invalid` is the clearest. Those get a default severity plus a
`rules` entry, so a workspace can move them to `off`. Correctness codes are not
configurable. Deciding which bucket each code sits in is part of the same pass.

## 2. Division of labour: `index` versus `doctor`

The accepted direction, recorded because it resolves the earlier open question
about a `--fix` flag.

> `index` fixes anything indexing-related, in place, with no new flag.
> `doctor` reports everything wrong with the repository.

That is a clean split, and it holds up because **indexing already owns the write
path for exactly these files.** `index` parses every routed source, computes the
generated regions, and rewrites them. Everything below is a fact it has already
computed and a file it has already opened:

| Problem                              | Why `index` owns it                                  |
| ------------------------------------ | ---------------------------------------------------- |
| missing `## Entries` section         | it is about to write that region                     |
| Entries region not final             | it is rewriting the region's position                |
| missing `## Axioms` heading          | same parse, same file                                |
| missing frontmatter on a routed file | it needs the description to build the parent's entry |
| unquoted `": "` in a description     | it already failed to parse that scalar               |
| missing parent entrypoint            | it cannot project a child without one                |

The boundary is **derivability**, not effort: `index` applies a fix when the
correct result is uniquely determined by what it already knows. A missing
`## Entries` heading has one correct insertion. A `license:` key in `SKILL.md`
does not — dropping it would destroy user data — so that stays a `doctor`
finding.

`doctor` keeps everything else: broken links, lifecycle divergence, permission
state, malformed content whose repair requires a choice.

**The user-visible consequence** is that the annoying loop disappears. Today a
command fails, names a code, and leaves a mechanical edit to the user. With this
split, running `index` because your Entries are stale also makes the workspace
legal, and reports what it fixed:

```
Updated 2 generated Entries, and fixed 3 structural problems.

  .agents/guidance/team/_team.md     added the missing Entries section
  .agents/guidance/team/notes.md     quoted a description containing ':'
  .agents/skills/pdf/SKILL.md        left alone — 'license' is not a key we can drop
```

### Skills and `references/`

This resolves the `_references.md` question without a special case.

`SKILL.md` is already its folder's entrypoint — `SourceIdentity` gives it the
directory's ID. So `skills/pdf/SKILL.md` **is** the entrypoint for
`skills/pdf/`, and its `references/` subfolder is an ordinary child folder that
needs an entrypoint like any other.

The question is only whether the user must write that entrypoint by hand. Under
the division above, they do not: **a missing parent entrypoint is indexing-related
and uniquely derivable** — the folder name gives the ID, the folder name gives a
default description, and the Entries region is generated anyway. `index` creates
it and says so.

That avoids a second rule set. The global rule stays _"every folder in a route
chain has one entrypoint"_; what changes is that the CLI creates the obvious ones
instead of refusing. Deeper nesting then needs no special case at all — it is the
same rule applied at each level.

If the generated entrypoint is unwanted, that is a signal the folder should not
be routed, which is a different fix (exclusion) and a better conversation than
"add three files to satisfy the tool".

## 3. Supporting findings

### External Extension sources fail silently

Two undocumented requirements, one of which fails without a word.

**Manifest schema.** A wrong key produces a raw .NET serializer message with
internal type names:

```
INVALID: The Extension manifest is invalid: The JSON property 'schema' could not
be mapped to any .NET member contained in type
'OpenForge.Cli.Core.Framework.Extensions.Models.Serialization.ExtensionManifestDocument'.
```

Same class of defect as the bare `ArgumentException` from `repair`. The accepted
shape is `{id, name, description, version, dependencies}` and appears in no help
text.

**The `content/` directory.** `ExtensionPackageLayout.ContentDirectoryName =
"content"`. Payload files must live under `<package>/content/.agents/…`. With the
files at `<package>/.agents/…` instead:

```
$ open-forge extension install --source ./mypkg --automatic --dry-run
Status: complete
Effects: 0
```

**Exit 0, "complete", nothing installed, no explanation.** That is the "external
extensions just don't work" experience: not a failure, a silent success. Moving
the payload under `content/` produces `Effects: 2` and works correctly.

Fixes: name the expected keys on a manifest error; report `Effects: 0` on an
apply as `attention` with a cause (_"the package has no content directory"_);
document the layout in `extension install --help`.

### Reference resolution rejects the hybrid form

Measured on a healthy workspace:

| Form                                        | Result       |
| ------------------------------------------- | ------------ |
| `guidance` — ID                             | **accepted** |
| `.agents/guidance/_guidance.md` — full path | **accepted** |
| `guidance/_guidance.md` — **hybrid**        | **rejected** |
| `memory/emerging/ideas/_ideas.md` — hybrid  | **rejected** |

The two accepted forms are the endpoints; the natural middle is refused. That
middle is exactly what a generated `Entries` link produces relative to its
parent, and exactly what someone copying a reference out of a document would
write.

**Proposed normalization**, one function used by every command (which also fixes
the `./` and backslash inconsistencies found earlier):

1. strip a leading `./`, normalize separators, strip a trailing `/`
2. strip a leading `.agents/` if present
3. strip a trailing `.md` if present
4. resolve the remainder as an ID

Under that rule all four forms above resolve to `guidance`, and the ID becomes
what it already logically is — _the path with `.agents/` removed from the left and
`.md` from the right_. Accepting any prefix of that transformation is free.

## 4. Correction: `CLAUDE.md` is deliberate, not a gap

An earlier note treated `RootClaudePath = "CLAUDE.md"` being the only recognised
foreign primitive as an interop failure. That reading was wrong, and the
correction matters for the interop task's scope:

- `AGENTS.md` is the emerging cross-tool standard and Open Forge already writes
  it, so most runtimes are covered by the entry file itself.
- `.agents/skills/` is where several runtimes already look.
- `.claude/rules` and similar are handled by the Loader's own reach.
- Hooks and runtime-specific configuration are an interop concern rather than a
  routing concern.
- **Extensions can deliver content outside `.agents/`**, so any additional
  primitive can be shipped as an Extension without the core needing to know it.

That last point is the strongest: the extension mechanism is the general answer,
and it means the core does not need a growing list of recognised formats.

What survives from the earlier analysis is narrower and still true:

- **Do not break** on foreign content is still required, and still failing —
  a `SKILL.md` with `license:` blocks the workspace.
- The **silent-success** and **manifest error** defects above make the extension
  path — the very mechanism that covers the rest — hard to use.

So the interop task reduces to: fix the strict `SKILL.md` keys, make external
extension sources usable and honest, and stop treating unrecognised files as
blockers. No recognised-primitive registry is needed.
