---
open-forge:
  description: Task 66 council review of the published documentation for overclaims and polish, and of the shipped Framework and Extension files for consistency, redundancy, order, and readability
  tags: [Memory, Working, Task, Documentation, Framework, Extensions, Council, Writing, Contextual, Active]
---

# Task 66 — Council polish

## Current state

Open, with maintainer review deferred by the later wave selection. The council
results and committed changes below are retained review material. Their older
branch handoff and merge steps are historical. Use the
[current ledger](../project-control.md#active-task-ledger) before selecting
another change or integration step. This cleanup does not close the review.

## Outcome

Requested by the maintainer on 2026-09-28, after the first documentation
council in [Task 61](task61-documentation-accuracy-and-voice.md). Two councils,
each with a Claude Opus agent and OpenAI Codex's `gpt-6-astra` at high effort,
run read-only through Worker Watch:

- **Documentation:** every published page and the README. The main target is
  overclaimed agent behavior. Agents are not deterministic, so the text can say
  what the files contain and what the CLI does, but it should say that the
  rules ask an agent to do something rather than promise that it will. The
  maintainer rates the documentation at about 95 percent and wants polish.
- **Shipped files:** the base Framework under `src/open-forge/` and the
  Extension content under `src/extensions/`. The targets are logical
  consistency, rules that only repeat an inherited rule, rule order,
  readability, and structure, without changing what any rule requires.

**Direction:** the maintainer asked for results, applied as modular commits on
this branch. No merge into `develop` or `main` and no push: the maintainer does
those.

**Preserve / out of scope:** the CLI source, product behavior, the README's
opening paragraph, the diagram's four loading labels, and the requirement
strength of every shipped rule.

**Done when:**

- [x] Both councils have reported, and the synthesis records what was applied,
      what was declined, and why.
- [x] Applied changes keep the site building with link checks, and shipped-file
      changes keep their dogfood counterparts and maintenance contracts aligned.
- [ ] The maintainer reviews the branch.

## Current State

**Now:** both councils' agreed changes are committed on `task66-council-polish`,
which starts from `task61-documentation-polish`. The maintainer's answers to
the nine open questions are committed on `task66-council-decisions`. The
maintainer reviews, merges, and pushes.

**Evidence:**

- The site builds with broken-link checks, and `tsc` passes.
- `doctor --workspace src/open-forge` finds no problems.
- Repository `doctor` has the same 12 errors as before, all the existing
  ambiguous `beta-follow-ups.md` links. It adds 25 "Extension file changed"
  warnings for the dogfooded Extension files edited here. The install lock
  records the old hashes, and `open-forge extension update` would reconcile
  them. That is a lifecycle step for the maintainer.
- Every edited shipped file matches its repository counterpart outside
  generated `Entries`.
- No semicolon remains in shipped rule text apart from the inherited marker.
- The base now measures about 5.7k tokens at startup and 7.8k in total, and
  the README, the development guide, the introduction, the adoption guide, and
  the homepage say so.
- Not run: the end-to-end suite, including the updated F26 heading assertion,
  because building could restore packages. The integration snapshots are still
  stale from Task 61 and cover these bytes too.

## Documentation council

The documentation council had two independent members: OpenAI Codex's
`gpt-6-astra` at high effort, run read-only through Worker Watch, and a Claude
Opus agent checking claims against the shipped files. Neither saw the other's
position, and the synthesis did not count votes.

**Both found independently, and applied:**

- Pages stated requested agent behavior as a result. The document flow, the
  getting started pages, Highlights, the concept pages, the Extension pages,
  the README, and the homepage now say what the rules ask an agent to do.
- `context` shows what the rules select, not what a model read. The CLI guides
  no longer call it proof of what an agent received.
- Scenarios make it hard to rewrite a requirement quietly, not impossible.

**One member found, verified, and applied:**

- Setup review in the CLI guides uses `git status` as well as `git diff`.
- Libraries are fetched through `library sync`.
- `--automatic` applies only to commands that ask for confirmation.
- A plan lists what would change, and `--detail standard` shows every file.
- The Memory capture list now matches `_memory.md`.
- Decisions from the day of adoption onwards are advice, not a category rule.
- The loading page notes that its four terms describe loading instructions.
- Rechecking the unapplied items found three more overclaims: that the base
  contains no Extension-type content (it ships the `open-forge-cli` Skill),
  and the Review and Debugging recipes described as what the agent does.

**Declined, with the reason:**

- Rewording the `AGENTS.md` merge answer. It describes the CLI's behavior,
  which an end-to-end test covers, and it already asks the reader to review
  the combined instructions.

## Shipped-files council

The same two members reviewed `src/open-forge/` and the Extension content for
consistency, restated inherited rules, order, readability, and structure.
Every edit keeps each rule's meaning, scope, and requirement strength.

**Both found independently, and applied:**

- The Directives entrypoint now states its rules in order: the `LoadNow`
  requirement, the `## Instructions` section, root applicability, then child
  scopes.
- Every Template repeated two sentences of `_templates.md` guidance. The
  Templates now say only "Replace {prompts}. Remove this source guidance and
  optional sections that add no value."
- The CLI Skill said to follow a suggested next step unconditionally, beside a
  rule to report `blocked` results. It now says to take a suggested step
  within the task, then rerun the command. The report rule and the rule that
  lifecycle commands need the user's request are unchanged.
- Compound rules joined by semicolons are single sentences, across the base,
  the Extensions, and their READMEs.

**One member found, verified, and applied:**

- The loader, `_memory.md`, `_emerging.md`, `_archived.md`, the Vision recipe,
  the workflow catalogue, and the Scenario Templates no longer restate rules
  an ancestor or the loader already sets. `_memory.md` keeps "context" in its
  Durable Outcomes rule, so nothing is lost.
- `_templates.md` splits its adaptation rule into three and names the
  `#Template` tag.
- The Core Templates role list is a bulleted list.
- Adaptive Collaboration uses "Situation" and "Recommended Approach", and its
  convergence rule is four items.
- "Owns" became "defines" or "answers" where it meant authority over a
  question. It stays where it means possession.
- Checkpoints' closeout rule is three items under "Transfer And Closeout".
- The Analysis rules follow the order of an Analysis's life.
- Terms are consistent: "fixed snapshot", "actual transfer", "seals", and
  "candidate route".
- The Scenario Templates use Title Case headings.
- The Collaboration and Development READMEs match their siblings.
- The Handoffs and Checkpoints payload contracts and the Emerging contract
  follow the new wording, and the documentation quoting the old Template text
  and headings is updated.

**Declined, with the reason:**

- Deleting "Keep exact current specifications in the sources that define
  them." from `_decisions.md`. It is specific to Decisions, and the
  documentation relies on it.
- Deleting Adaptive Collaboration's sentence that restates the loader's
  "begin with the current understanding" rule. The Guidance loads on demand,
  so it costs no startup context, and it reinforces a behavior users notice.
- Rewording the Handoffs category description. It is accurate, and a new
  description changes generated `Entries` in every workspace.

## Decisions on the open questions

The maintainer answered the nine questions on 2026-09-28. They are applied on
`task66-council-decisions`, which starts from `task67-diagram-labels`, together
with the matching contracts, the repository's framework documents, and the
docs.

1. **Handoffs use any active working record.** `_handoffs.md`, the Handoff
   Template, the Handoffs contract, the Working Memory document, the
   dictionary, and the docs say "active working record" or "live working
   record". A Checkpoint is one kind. The Documents entrypoint now says "past
   decisions", so it doesn't assume Planning.
2. **A Checkpoint retires when its active need ends.** The Checkpoint Template
   and the Checkpoints contract now match `_checkpoints.md`. Closeout saves
   durable outcomes but doesn't retire a Checkpoint whose work will resume.
3. **The CLI Skill's next step stays as applied.** The Skill's lifecycle rule
   already stops a suggested install or update from running unasked.
4. **Adaptive Collaboration's ask rule** now includes cost and external
   effects, like the loader's.
5. **A recipe Goal** states the outcome it serves and, when that isn't
   obvious, when the recipe fits. The workflow catalogue, the Workflow
   Template, and the workflows contract say so.
6. **Unaccepted proposals stay out of Decisions.** In the maintainer's words,
   they are ideas, analysis, or anything else, not Decisions. `_decisions.md`
   and the Decision Template now say "An unaccepted proposal isn't a Decision.
   Keep it in a candidate route, such as Ideas or Analysis."
7. **Emerging's visibility rule is conditional:** "Keep uncertainty, source,
   and scope visible when they affect later use."
8. **No count on Instructions.** The maintainer called it semantics: the CLI
   doesn't check the count, and `## Instructions` is simply the heading for a
   Directive's rules. The Directives entrypoint, the Directive Template, three
   contracts, the Directives document, and two docs pages now say a Directive
   keeps its instructions under a non-empty `## Instructions` heading.
9. **Tags in Template prose use `#`**, such as "#Directive and #LoadNow" and
   "an #Evergreen source". No tool reads tags from body text.

**Repository-only follow-up:** three repository Guidance files still use the
old headings: `calibrated-agent-reasoning.md`, `adaptive-design-delivery.md`,
and `architectural-perspectives.md`. They weren't in the council's scope.
