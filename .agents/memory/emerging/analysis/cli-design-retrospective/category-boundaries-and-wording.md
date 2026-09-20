---
open-forge:
  description: The Directives loading axiom that states half a rule, whether Workflows and Skills are redundant, whether Maps earns a root route, and an assessment of the agent-authored Memory classification axiom
  tags: [Memory, Analysis, Contextual, Candidate, Framework, Design, Categories, Axioms, Loading]
---

# Category Boundaries And Wording

## 1. The Directives loading axiom states half a rule

The agent is reading the text correctly. `directives/_directives.md` opens its
Axioms with:

> **"Every sibling Directive listed under an entrypoint's `Entries` carries
> #LoadNow."**

Unconditional. Nothing qualifies it. An agent that concludes "a scoped Directive
must be `#LoadNow`" has read exactly what is written.

It also contradicts the Loader:

> _"Keep entries on demand by default. Use #LoadNow only when missing the content
> would cost more than reading it whenever its parent loads."_

### What the payload actually does

The shipped and authored pattern is correct and is nowhere stated:

| Kind                                                                             | `#LoadNow` |
| -------------------------------------------------------------------------------- | ---------- |
| `directives/_directives.md` (root)                                               | yes        |
| `directives/decision-authority.md`, `execution-safety.md`, … (leaf)              | **yes**    |
| `directives/csharp/_csharp.md` (**scope gate**)                                  | **no**     |
| `directives/csharp/design.md`, `style.md` (leaf under gate)                      | **yes**    |
| `directives/open-forge/_open-forge.md`, `…/cli/_cli.md`, `…/testing/_testing.md` | **no**     |

Every leaf Directive is `#LoadNow`. Every **sub-entrypoint is on demand**. That
is what makes scoping work: `directives/csharp/*` costs nothing until someone
selects `csharp`, and then all of it loads at once — which is exactly right for
required instructions.

**The axiom states the leaf rule and omits the gate rule.** An agent that applies
the stated half consistently will tag the sub-entrypoint `#LoadNow` too, which
collapses the gate and makes every scoped Directive resident in every task. That
is precisely the failure observed.

### Proposed wording

Replace the first axiom with both halves, and name the mechanism:

```markdown
- A Directive file carries #LoadNow: when its scope is active, its Instructions
  are required and load with it.
- A Directive entrypoint does not carry #LoadNow. The entrypoint is the scope
  gate: nothing below it is read until the scope is selected. This is what keeps
  narrow Directives free for unrelated work.
```

Two sentences, and the rule becomes self-explaining. Worth noting that this is
the _only_ place in the framework where the gate/leaf distinction matters this
much, and it is unwritten.

## 2. The agent-authored Memory axiom is better than what it replaces

The proposed text:

> Memory supports the other categories with evidence, rationale, accepted
> context, and history. It does not define behavior. When an outcome belongs to
> another category, move it there and keep the supporting reasoning in Memory,
> linked in both directions.

Compared against the current wording, spread across two subsections:

> _"Recording content from another category does not give the record that
> category's role."_ (Authority And Classification)
>
> _"Put accepted behavior that should guide future work in the matching #Core
> route. Keep useful context or reasoning in Memory."_ (Placement And Lifecycle)

The replacement is better on four counts, and the reasons generalise:

- **It states a role, not a prohibition on a mistake.** _"Memory supports the
  other categories"_ is an identity. The current first line describes what fails
  to happen when you misfile — which only helps someone who already suspects
  they are misfiling.
- **It enumerates what Memory _does_ hold** — evidence, rationale, accepted
  context, history. That is the positive destination the current wording lacks,
  and it is the half that makes the negative clause safe to state.
- **It puts the boundary and the remedy in one place.** Today the "does not
  define behavior" idea and the "move it to #Core" idea are in different
  subsections, ~15 bullets apart. An agent reading the classification section
  never reaches the placement instruction.
- **It names bidirectional linking as the resolution.** This matters more than
  it looks: the reason material gets misfiled is that the author does not want to
  lose the connection. Saying explicitly _"move it and link back"_ removes the
  incentive to keep prescriptive content in Memory for continuity.

This directly addresses the 1,288-versus-40 imbalance measured in
[memory-authority-boundary.md](memory-authority-boundary.md). **Adopt it**, and
place it as the first line of `Authority And Classification`, before anything
else in that section.

One refinement worth considering — the earlier analysis argued for redirect over
prohibition, and this text can carry that cheaply:

> …It does not define behavior: instructions belong in `directives`, recommended
> approaches in `guidance`, reusable shapes in `patterns`, repeatable methods in
> `workflows`.

That is the one negative clause worth keeping, because it ships a destination for
each alternative rather than a rule to remember.

## 3. Workflows and Skills

### The measured position

|             | Files in this repo | Root route | `#LoadNow` |
| ----------- | ------------------ | ---------- | ---------- |
| `workflows` | **21**             | yes        | yes        |
| `skills`    | **0**              | yes        | yes        |

The overlap is currently one-sided: only Workflows is used. But that is weak
evidence, because `SKILL.md` handling is broken — a skill carrying `license:` or
`allowed-tools:` hard-blocks the workspace
([interoperability-and-diagnosis.md](../cli-experience-audit/interoperability-and-diagnosis.md)).
Nobody has had a fair chance to use Skills.

### The distinction that actually survives

Not "recipe versus capability" — both are documented methods. The real
difference is **who owns activation**:

|                                    | Workflow                                            | Skill                                   |
| ---------------------------------- | --------------------------------------------------- | --------------------------------------- |
| Discovered by                      | the loaded Framework context, via `Entries`         | the agent runtime, by its own scan      |
| Selected by                        | the agent reading a description                     | the runtime, often by description match |
| Scoped by                          | its route — `workflows/development/*` is scopable   | flat; the runtime decides globally      |
| Works without a supporting runtime | **yes**                                             | **no**                                  |
| Owned by                           | Open Forge lifecycle (install / update / extension) | the runtime's installer                 |

The strongest argument for keeping Workflows is the fourth row, and it is a
premise-level argument. The Loader states _"The plain files remain complete
without the CLI."_ By the same logic they should remain complete without a
particular agent runtime. **A Workflow is followable by any agent or by a human;
a Skill requires a runtime that implements `SKILL.md`.** Deleting Workflows
would make Open Forge's method layer dependent on a format one vendor defines.

The second-strongest is scoping. `workflows/development/*` can be narrowed to a
route. Skills are flat by construction, because the runtime — not the workspace
— decides what is active.

### The argument against, which is also real

An agent looking for "how do I do X" now has **two** places to look, with
adjacent questions:

- `workflows` — _"What repeatable method can help reach this goal?"_
- `skills` — _"Which specialized capability would help with this work?"_

That is the same placement ambiguity as `directives`/`guidance`/`patterns`, and
it has no aid.

And the content is drifting. `workflows/review.md` step 2 is a ~200-word
paragraph specifying coordinator intake, snapshot records, unit allocation, and
routing. That is not a recipe; it is an orchestration specification — arguably
exactly what a `SKILL.md` with a `references/` folder exists to hold.

### Assessment

**Do not delete Workflows.** The runtime-independence argument is decisive for a
framework whose premise is user-owned, tool-independent Markdown.

**But `skills` as a separate _root route_ is questionable**, and that is the
redundancy worth examining rather than Workflows. Consider:

`SKILL.md` is a **delivery format**, not a category. The framework's category
question is _"what repeatable method helps reach this goal?"_ — and a `SKILL.md`
package is one possible shape of that answer, differing in who activates it. The
machinery already supports this: `SourceIdentity` gives `SKILL.md` its folder's
ID, treating it as that folder's entrypoint, so a skill is already routable
anywhere.

The counter-argument is practical and may be decisive: **runtimes discover skills
by path.** If a runtime scans `.agents/skills/` or a sibling directory, the path
is load-bearing and cannot be reorganised for conceptual tidiness. That is worth
checking against the runtimes you actually target before acting.

Two viable outcomes:

- **Keep both, sharpen the questions.** Workflow = _a method you follow_.
  Skill = _a capability your runtime activates_. Add the redirect clause to each
  (`responsibility`), and say plainly in `skills` that a method with no runtime
  support belongs in `workflows`.
- **Fold `skills` under `workflows`** as a delivery variant, if runtime path
  discovery permits. One category, one question, two shapes.

The first is safe and cheap. The second is cleaner and needs the runtime check
first. Either beats deleting Workflows.

## 4. Maps

Measured: **1 file** in this repository, `#LoadNow`, a root route.

Its question — _"Where is a useful local or external source, and when should it
be used?"_ — is answered by every entrypoint's `Entries` descriptions, which
already carry exactly that. The one genuine gap is **external** sources, which
`Entries` cannot hold because they are not routed files.

**Assessment: not worth a root route.** Two better placements:

- External references become a `references` block in the entrypoint of whatever
  scope needs them. The thing pointing outward should sit next to the thing that
  needs the pointer.
- Or `maps` survives as a **pattern** — a documented shape for "a scope's
  external references" — rather than a top-level category with one file in the
  flagship workspace.

Either way, remove it from the `#LoadNow` set. A resident category holding one
file is pure cost.

## 5. Adding "changes"

**Agreed — do not add it**, and the reason is worth recording because it applies
to the next proposed category too.

A change set is **workstream state**: what is being done now, in what order, with
what status. That is `working`, and the measured evidence says users already
build it there — `working/cli-development/` holds a plan, a control record, and
100 task files, invented rather than provided
([taxonomy-and-adoption.md](../cli-experience-audit/taxonomy-and-adoption.md)).

The gap is not a missing category. It is that `working` models artifacts
(`checkpoints`, `handoffs`) instead of workstreams, so people build the
workstream shape by hand. Adding `changes` as a ninth root route would repeat the
mistake one level up: another artifact-shaped branch competing with the
workstream that actually contains it.

**The general rule this suggests:** before adding a root category, check whether
the need is _a new kind of thing_ or _a missing shape within an existing kind_.
`changes`, `tasks`, `plans` and `backlogs` are all the second — they are
components of a workstream, and they belong to a workstream template.

## Summary of proposed changes

| Item                        | Change                                                                                                               | Size          |
| --------------------------- | -------------------------------------------------------------------------------------------------------------------- | ------------- |
| Directives loading axiom    | State both halves — leaf carries `#LoadNow`, entrypoint is the scope gate                                            | one paragraph |
| Memory classification axiom | Adopt the agent-authored text, first in its section, with the redirect clause                                        | one paragraph |
| Workflows                   | Keep. Runtime independence is a premise-level argument                                                               | none          |
| Skills                      | Sharpen the question and add a redirect; consider folding under Workflows only after checking runtime path discovery | small–medium  |
| Maps                        | Remove from `#LoadNow`; demote to a pattern or to per-scope external references                                      | small         |
| `changes`                   | Do not add. It is a workstream component, not a category                                                             | none          |

## 6. Interoperability is a positioning requirement, not a feature

"Grow your own framework" is a promise about _what the user already has_. It
says: keep your files, keep your tools, add structure. That makes coexistence
with foreign AI primitives a **premise-level obligation**, not an integration
nice-to-have — and it changes the answer to the Skills question above.

### What the CLI currently recognises

Exactly one foreign primitive:

```csharp
// FrameworkPayloadAsset.cs:10
internal const string RootClaudePath = "CLAUDE.md";
```

That is the complete list. Meanwhile this repository contains, at root or one
level down: `opencode.json`, `apm.yml`, `apm_modules/`, `.github/`,
`open-forge.extensions.json` — and the ecosystem at large adds `.claude/`
(skills, hooks, commands, settings), `.cursor/rules`, `.mcp.json`,
`.windsurfrules`, `copilot-instructions.md`, and more arriving monthly.

**None of them is known to Open Forge.** They are invisible if they sit outside
`.agents/`, and — as this audit measured — actively hostile if they sit inside
it: a standard `SKILL.md` carrying `license:` hard-blocks the entire workspace.

That is the opposite of the promise. A framework that tells you to grow your own
and then breaks on the most widely published agent primitive in existence is
making a claim it does not honour.

### Three levels of interop, in order of value

**1. Do not break.** The minimum, and currently failed. Any file Open Forge did
not author is _skipped with a note_, never a blocker. Covered by G2 in the
backlog, and it is the only level that is strictly required.

**2. Recognise and route.** A known foreign primitive is surfaced in `Entries`
with its own description, so an agent reading the Framework sees it. `SKILL.md`
already works this way — `SourceIdentity` gives it the folder's ID and
`ParseSkill` reads its native `name`/`description`. **The pattern exists and was
built for exactly one format.** Generalising it is a small, well-shaped piece of
work:

```
foreign primitive → recognised path/filename
                  → native metadata reader (name + description)
                  → routed like anything else, no rewriting
```

Candidates in rough order of reach: `.claude/skills/*/SKILL.md`,
`CLAUDE.md` / `AGENTS.md` / `copilot-instructions.md` (instruction files),
`.claude/commands/*.md`, `.cursor/rules/*`, `.mcp.json` (capability inventory),
hooks configuration.

**3. Reconcile.** Detect that the same intent is expressed twice — a Directive
and a `CLAUDE.md` rule saying different things — and report the divergence. This
is where a framework earns its position rather than merely tolerating neighbours,
and it is the only level that requires real judgment. Not for v1.

### What this settles about Skills

The previous section left open whether `skills` should fold under `workflows`.
This resolves it: **keep `skills` as a route, and treat it as the first member of
a family rather than a special case.**

The reason is the one flagged there as decisive — runtimes discover by path — and
it generalises. `.claude/skills/`, `.claude/commands/`, `.cursor/rules/` are all
path-discovered by their runtimes. Open Forge cannot relocate them for
conceptual tidiness, so the framework must meet them where they live.

That reframes the category from _"a kind of Open Forge content"_ to _"a
recognised foreign format that Open Forge routes without owning"_. The question
changes accordingly:

- today: _"Which specialized capability would help with this work?"_
- better: _"Which capability does your runtime already provide here?"_

And the redirect writes itself: _a method with no runtime support belongs in
`workflows`_.

The generalisation also suggests the eventual shape is not one `skills` route but
a recognised-primitives concept, where `skills` is the first entry. Worth
deciding before adding the second.

## 7. Maps, revised

The earlier assessment — one file, `#LoadNow`, overlapping `Entries` — stands on
the measurements but was too quick to demote. The stated use is narrower and more
defensible than "a list of links":

> coarse, non-granular rough edges of the repo, plus external.

That is **orientation**, and it is genuinely not what `Entries` provides.
`Entries` is exhaustive, generated, and one level deep. A map is deliberately
_lossy_: "the CLI lives in `src/cli`, contracts in
`memory/crystallized/documents/cli`, delivery in `scripts/delivery`, and the
upstream .NET docs are here." Nothing else in the framework holds a coarse shape
of the whole, and a new agent needs exactly that before it needs any list.

The interop point above strengthens this. If foreign primitives are scattered
across `.claude/`, `.github/`, `apm.yml` and `opencode.json`, **something has to
say where they are and what they are for.** `Entries` cannot: those files are
not routed sources. That is a real job with no other owner.

Revised assessment:

- **Keep the concept.** Coarse repo orientation plus external references is a
  distinct need, and it is the natural home for "where the non-Open-Forge things
  live".
- **Sharpen the question** from _"Where is a useful source?"_ — which invites a
  link dump and explains the overlap — to something that forces coarseness:
  _"What shape is this repository, and what lives outside it?"_
- **Remove it from `#LoadNow` anyway.** One file that changes rarely should be
  read when orienting, not resident on every task. This is the one part of the
  earlier assessment that survives unchanged.
- **Ship a template**, since a map's failure mode is granularity drift into a
  duplicate index, and nothing currently guards against that.
