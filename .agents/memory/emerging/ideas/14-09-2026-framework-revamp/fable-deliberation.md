# Fable's deliberation on `src/open-forge`

Companion to [fable-open-forge.md](fable-open-forge.md) (first language pass, written before reading the Writing Directive) and [fable-open-forge-2.md](fable-open-forge-2.md) (second pass, written after it). This file holds the reasoning and the answers to the structural questions.

## Short answers

- **Language.** The prose is correct and consistent, but it reads like a specification, not like a colleague. The main costs are abstract nouns doing the work of verbs, rules that repeat across files in slightly different words, three loader axioms that are really CLI contracts, and one leaked Extension term ("Task") in the base loader. A pure language pass saves roughly 9% of the startup set. The bigger wins are structural and listed below with estimates.
- **Observations to Orchestration?** Keep the route in the base, but demote its loading. The route earns its place for solo agents (the "next session would rediscover this" case). What it does not earn is a #KeepInMind refresh on every task. Orchestration can add its own Observation guidance in its scope.
- **Documents to Planning or Project Documents?** No. The Documents route is the only home for "what is true now" in the base. Removing it leaves Decisions as the sole durable store, which is the ADR-only trap the memory model exists to avoid. The Extension supplies starters. The route is the container.
- **Category identity.** Six of seven Core categories and three of four Memory states have a crisp identity. The weak spots are Guidance (identity fine, shipped content blurs it), the Checkpoint/Handoff split (real but expensive for what it buys in the base), and the Directives description (describes loading, not purpose).
- **Scoping and linking as the core model.** Yes, it is a good idea, and it is the right idea for a files-only, agent-agnostic framework. Its two honest costs are selection quality (descriptions carry the load) and entrypoint maintenance. Both have concrete mitigations below.

## Language

### What the current prose does well

- Every category opens with its question. That is the best single idea in the files. A reader can hold the whole Framework as seven Core questions plus four Memory questions.
- Terms are used consistently. `accepted`, `route`, `scope`, `entrypoint`, `Contextual`, `CurrentTruth` mean the same thing everywhere.
- The Memory "before creating a durable record, ask" checklist is concrete and actionable. More of the files should look like that.
- Negative rules mostly close real ambiguities ("Tags, repetition, and agent confidence do not create acceptance").

### What makes it hard to read

**Abstractions stand in for actions.** "Validation establishes whether evidence supports a claim. Acceptance establishes which knowledge or decisions may be treated as current within their scope." Both sentences are true and neither tells the agent what to do. Rewritten: "Validation asks whether evidence supports a claim. Acceptance decides whether something counts as current."

**"Material", "meaning", and "source" carry too much.** "Material" appears about thirty times as a generic noun for "stuff". "Accepted current meaning" and "durable meaning" are things an agent cannot point at. Where the concrete noun is known (record, rule, file, decision), use it.

**The same rule is restated per category.** "Check `Entries` when ...", "Read `Entries` when ...", "Read `Entries` before deciding that ..." appear in eleven files. That is one loader rule plus a one-line trigger per category. The variants (Read versus Check) also read as if they might mean different things. They do not.

**Sixteen authority axioms in one flat list.** They cover five different questions: what wins, how to work with the user, where meaning lives, how rules combine, and one lifecycle rule about Analysis. Grouped by question they read in one pass. Flat, the reader has to build the groups themselves.

**Three axioms belong to the CLI.** "Management affects file lifecycle only", "Each manager defines the route shapes it recognizes ...", and "A familiar slug or tag does not create ... managed status" describe how installers behave. An agent needs one sentence from that: tools may manage files, and that changes nothing about meaning. The one agent-facing consequence (do not reorder managed segments when inserting a scope) is stated once, where scopes are explained.

**One Extension term leaked into the base.** "Let Analysis terminate in an actionable Task ... the Task and its subtasks own current implementation discoveries and execution evidence." "Task" and "subtasks" are Planning Extension vocabulary. The rule itself is good (it came from the authors' findings log, finding 3) and survives in base words: "Once an Analysis reaches a conclusion and work proceeds from it, treat the Analysis as sealed. Record what execution discovers in the working record for that work."

**Checkpoints and Handoffs explain each other.** Both entrypoints spend about a third of their text on when to create a Handoff. Handoffs should own that rule.

**"Sibling" is undefined for the installed reader.** The Directives and Workflows axioms rely on "sibling Directive" and "direct sibling Workflow file". The repository Dictionary defines these, but installed workspaces do not receive the Dictionary. The loader never defines "sibling". "Listed beside its entrypoint" says the same thing in words the loader already implies. (The first pass got this wrong and widened the rule to child entrypoints. The second pass fixes it.)

**Definitions repeat the question.** "## What behavior is required in this scope?" followed by "Directives state the instructions that must be followed within their scope." The Writing Standard requires the definition sentence, so the second pass keeps it, but makes each one do a job the question cannot: draw the line to a sibling category, or name the mechanism.

### Human, not bureaucratic

The user asked for a "very human" voice. What that means in a rules file:

- Address the reader when it removes ambiguity, otherwise use the plain imperative. The Writing Standard's "one technically literate person explaining the system to another" is the right target.
- One rule per sentence, and the verb near the front.
- A "why" clause when the rule would otherwise be misread. "Do not stop only to ask whether to continue" is clearer with the reason attached: accepted direction already answers that question.
- Concrete examples where a rule is abstract. The Observations entrypoint gains three words of example ("a surprising result, a recurring failure, a detail the next session would otherwise rediscover") and becomes usable.

The first pass leaned into "you". The Writing Standard prefers the actor omitted, and the shipped files already use that shape, so the second pass pulls "you" back to the places where dropping it would cost clarity. Both passes are in the repo, so the user can pick the voice they prefer per file.

### Measured effect of the language passes

Estimates use characters divided by four (tiktoken is not installed here). The README's published figures use tiktoken, so treat these as relative, not absolute.

| Set | Original | First pass | Change |
| --- | ---: | ---: | ---: |
| Loader | 2,552 | 2,432 | -5% |
| Startup set | 7,987 | 7,242 | -9% |
| Complete Framework | 9,889 | 9,075 | -8% |

The second pass is a few percent larger than the first because the definition sentences and backticks return. A language pass alone will not halve the startup cost. The files are already tight. The structural options below are where the real savings are.

### Beyond language: structural options with estimates

These change loading or routes, so they are proposals, not part of the rewrites.

| Option | Startup saving (est.) | Cost |
| --- | ---: | --- |
| Make `handoffs/` on demand (drop #LoadNow) | ~350 | An agent resuming must find the Handoff through the Checkpoint or Working `Entries`. Checkpoints already link to durable sources. |
| Drop #KeepInMind from `observations/` (keep #LoadNow or on demand) | ~260 plus one refresh obligation per task | The Emerging refresh rule already says "read the visible Entries and save useful candidate material". The child tag is redundant with the parent. |
| Merge Handoffs into Checkpoints as a `#Sealed` tag | ~350 and one entrypoint | Loses a dedicated place to look for transfer notes. The dogfood repo created ten Handoffs, all from the orchestrated CLI programme, all now archived. |
| Move the manager axioms to CLI docs | ~60 | Done in both passes. |
| Drop Adaptive Collaboration from the base | 0 at startup (it is on demand) | The loader already carries its core rule. The file adds the "how". Keep. |

Taken together, a lean startup set lands around 6,500 estimated units, against 7,987 today, with no rule lost.

## Should Observations move to the Orchestration Extension?

**Recommendation: keep the route in the base. Remove #KeepInMind from the child. Let Orchestration add its own Observation guidance in its scope.**

The evidence from this repository's own memory:

- Twelve Observations exist. Nine are orchestration-level (audits of agent flows, delegation gaps, review-rationale trials, a supervised-worker trial). Three are solo-agent gotchas: formatting broke generated `Entries`, a wording change broke a test fixture, a product name collision.
- That matches the user's intuition about who writes them in practice. It does not show the route is useless without an overseer. The three gotchas are exactly what a single agent in a single-repo workspace should write down, and the base has no other home for a not-yet-confirmed fact. Ideas are possibilities. Analysis is reasoning. A Document is accepted. An Observation is "I saw this, it might matter, I am not sure yet."

Why not move it:

- The README's pitch is "a useful finding ... can become Memory that supports later work". Observations is the entry door for that finding. Without it, the door is Ideas, which then has to hold facts, or Documents, which then holds unconfirmed facts as current truth.
- The Orchestration Extension has no Memory content today. Making it the owner of a Memory route means a workspace without Orchestration cannot store an observation without inventing a route. That is a worse first experience than one small on-demand entrypoint.

What does deserve to move: the refresh cost. Observations is the only leaf below Memory that carries #KeepInMind. Emerging already carries it and its refresh rule already covers "save candidate material before it is lost". The child tag doubles the obligation for no extra behavior. Drop it. Orchestration can then say, in its own Workflow, "record an Observation when a delegated agent's behavior surprises you", which is the overseer use the user has in mind.

## Should Documents move to the Planning (or Project Documents) Extension?

**Recommendation: no.**

The Documents route answers "what is true now, and how does it work?" Decisions answer "why was this chosen?" The memory model decision records this split as deliberate: an ADR-only store makes current truth reconstructable only by reading every decision. That is the failure mode most teams with ADRs know well.

The Project Documents Extension provides Vision, Architecture, Principles, and Maintenance Contract starters plus two Workflows. Those are starters for particular documents. The route is the container for any document: "how the auth module works", "the release checklist we agreed on", "what the API contract is". Every workspace accumulates those. None of them need the Extension.

The dependency the user senses runs the other way. The Extension depends on the route existing. The route depends on nothing.

If the concern is that an empty `documents/` entrypoint at startup is dead weight: it costs about 190 estimated units and it makes the accepted-knowledge inventory visible at startup, which is the ACE promise ("cheaply determine what matters, where it lives, what is authoritative"). Keep it.

## Do the categories have a proper identity?

| Category | Identity | Assessment |
| --- | --- | --- |
| Directives | Must | Crisp. The description ("Required instructions loaded through selected routes") describes loading rather than purpose. "Instructions that must be followed within their scope" would be better. |
| Guidance | Should, adaptable | Concept is crisp. The shipped Adaptive Collaboration file partially duplicates loader axioms and reads like a mini-Workflow ("present in this order: 1, 2, 3, 4"). The repo's own five Guidance files run 4 to 8 KB each. Guidance is where long essays land. Worth a sentence in the entrypoint: "Keep Guidance short enough to consult mid-task." |
| Patterns | Shape, continuing | Crisp, and well separated from Templates by the ownership rule. |
| Skills | Native capability | Thin but real. It is the only category whose content is not Open Forge's format, and the entrypoint says so. |
| Templates | Starting content, one-time | Crisp. Being on demand (no #LoadNow) is the right call and the one category that models "earn your loading". |
| Workflows | Recipe to a goal | Crisp. The Goal/Steps/Completion contract is a good discipline. |
| Maps | Where | Crisp. Slight overlap with Documents that declare an external source authoritative. Acceptable. |
| Working | Temporary | Crisp. |
| Checkpoints / Handoffs | Live versus sealed | Real distinction, subtle in practice. Both entrypoints spend text distinguishing themselves. In the dogfood repo Handoffs appear only in the orchestrated programme. A `#Sealed` tag on a Checkpoint would express the same thing with one route. Recommend at least making Handoffs on demand. |
| Emerging: Analysis / Ideas / Observations | Reasoning / possibilities / noticed facts | Clear three-way split. See the Observations section for loading. |
| Crystallized: Decisions / Documents | Why / what | The strongest split in the model. Keep both, keep both loaded. |
| Archived | History | Crisp. |

Two identity questions worth a decision rather than a rewrite:

- **Is "Analysis" a Memory role or a working artifact?** The loader now has a lifecycle rule for it (seal after conclusion). That rule reads like a Planning concern. If Analysis is expected to feed a Task, the Planning Extension is the natural home for the seal rule, with the base keeping only "Analysis is not accepted truth". The second pass keeps the rule in the loader because it was there, reworded in base vocabulary.
- **Does Guidance need a length rule?** Not a Framework rule, but the shipped example sets the tone. A 1,250-unit Guidance file loaded on demand mid-task is a real cost. The base could ship a shorter Adaptive Collaboration and leave the long form to the repository.

## Is scoping and linking the right core model?

Yes. For a framework that must be plain files, work with any agent, and stay inspectable, there is no better substrate. What it gets right:

- **Cost scales with what is selected, not what is stored.** Sibling scopes are free until opened. This is the property that makes "grow your own framework" honest.
- **Inheritance without copying.** A child adds; ancestors stay active. That mirrors how humans organize rule books and it is exactly what nested `.cursorrules`-style files fail to express.
- **One mechanism for every arrangement.** A project inside a Memory state, a Memory lifecycle inside a project scope, a discipline scope across Core categories. The scope-and-slugs decision handles all of it with one rule.
- **Git-native.** Every change is a diff. Every route is a folder. Nothing hides in an index.

Where it costs, and what to do about it:

1. **Selection quality rides on descriptions.** The agent picks routes from one-line descriptions. A stale or vague description sends the agent the wrong way, and no tool can validate relevance. Mitigation: the Writing Standard's description rules are good, and `open-forge find --tag` gives a second signal. A future step is an optional `applies-to:` hint in frontmatter (paths or globs) so a code path can suggest a scope. That is what glob-attached rule files in editor tools get right, and it composes with routes rather than replacing them.
2. **Depth is a tax without the CLI.** `memory/emerging/observations/_observations.md` is four reads deep. `open-forge context` batches this. Agents without the CLI pay per hop. Mitigation: keep the shipped tree shallow (it is at the limit now), and make the on-demand leaves genuinely on demand so the startup chain stays short.
3. **Entrypoint maintenance is the friction point for growth.** Every new folder needs `_folder.md` with `Entries` kept in sync. `open-forge index` solves it with the CLI. By hand it is the step people skip. Mitigation: the docs should show the manual three-step (create folder, create entrypoint, add the entry in the parent) once, plainly, early.
4. **Inheritance is remembered, not enforced.** An agent deep in a scope must hold ancestor axioms in mind. Nothing checks. The Principles document already says this honestly ("honest reliability"). Keep saying it.

Alternatives considered: a single always-loaded file (does not scale), a vector index (hidden, needs a runtime, not inspectable), glob-attached rule files (good for code paths, no memory lifecycle, no inheritance), Skill-style progressive disclosure (the same idea, narrower). Open Forge generalizes the last one to every kind of knowledge and adds the memory lifecycle. That is the right place to stand.

## Notes on the writing rules themselves

Reading the Writing Directive, Standard, Project Voice, and Dictionary after the first pass, three tensions are worth naming:

- **The Dictionary defines terms the installed reader never sees.** "sibling file", "direct sibling file", "child entrypoint" are Dictionary terms used in shipped axioms. Either the loader's Terms should define "sibling" or the shipped files should avoid it. The second pass avoids it.
- **"You" versus actor omission.** Project Voice invites "you" for READMEs. The Standard omits the actor in rules. Both are fine, but the boundary should be stated once so authors do not drift. The second pass follows the Standard for shipped files.
- **The required definition sentence under each question.** The Standard requires it. In the current files it mostly restates the question. A one-line addition to the Standard ("the definition names what the category is or how it differs from its nearest sibling; it does not restate the question") would keep the rule and remove the padding.

## README and docs

[README-v2.md](README-v2.md) keeps ACE and "grow your own framework" as the two ideas and drops the rest to what a newcomer needs to decide whether to try it. [docs-v2/](docs-v2/) restructures the three guides so the quick path comes first and reference follows.

One sync item: the current `docs/cli.md` documents `--view`, `--json`, and `--verbose`. The accepted global-flags contract still lists them, but the consolidated G4 proposal in Task 30 replaces them with `--detail` and `--format`. The docs rewrite follows the accepted contract. When G4 lands, the global options table in `docs-v2/cli.md` needs the same change.
