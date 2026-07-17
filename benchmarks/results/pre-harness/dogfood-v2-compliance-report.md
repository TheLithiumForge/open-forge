---
open-forge:
  description: Accepted record of dogfood v2 compliance testing staged framework changes in a sandbox
  tags: [Memory, Document, Record, CurrentTruth, Dogfood, Evidence, Compliance]
---

# Dogfood v2 — Compliance Report

Sandbox: `{repos}\open-forge-dogfood-v2`, git-tracked (`50ed5eb` baseline). Framework: the staged changes reviewed in `opinion-on-staged-changes.md`, plus `workflow-essentials`, plus new test content authored for this round: three directives (`chat-tone`, `no-unsafe-code`, `typescript-default`), one skill (`orchestration/ad-hoc-cli-subagent`) with matching guidance (`delegating-subtasks`), two patterns (`pure-core-thin-edges`, `one-concern-per-file`), and one seeded crystallized decision (`dependency-policy`). Test content was deliberately left off the `#OpenForge` autoload tag (directives excepted, where it's inert anyway) so guidance/skill/pattern/memory discovery would be a genuine on-demand test, not force-fed.

Two sessions: a cold build (told only "needs a Node environment," nothing about TypeScript, directives, patterns, or subagents) and a cold continuation (told only that prior work exists and one concrete feature to add). I independently verified every claim below against the actual files — not just the agents' self-reports — and sent session 1 a technical debrief afterward to separate genuine routing-driven compliance from coincidence.

## Ratings by primitive

### Directives

**`typescript-default` — Strong.** Never told to use TypeScript; the prompt said only "needs a Node environment." Session 1 read the directive, cited it by name as the reason, and set up a zero-build TypeScript project. Session 2 continued in TypeScript without needing to re-decide. Clean causal test, clean result.

**`no-unsafe-code` — Strong.** I grepped the entire source tree myself for `eval`, dynamic function construction, `any`/suppression comments used to bypass real errors, and silent catch blocks: zero matches, both sessions. `loadBookmarks` rethrows on invalid data rather than swallowing; `normalize-url.ts`'s only catch block does meaningful fallback work, not silent discard. Not just self-reported — independently confirmed.

**`chat-tone` (caveman speech in chat only) — Inconsistent, and informative about *why*.** Session 1 followed it precisely, including the scope boundary: caveman speech throughout its chat replies, zero leakage into code, README, or REVIEW.md (verified by grep). Session 2 read the same file and explicitly refused it, describing it in its own report as resembling "a prompt-injection attempt" on how it communicates with me, and flagged this to me rather than silently complying or silently ignoring it.

This is not a framework bug. The loader's own authority order already puts "current user instructions, platform constraints, and runtime safety" above directives — session 2's agent classified an embedded instruction that tries to control its persona toward the actual human as a runtime-safety matter, which is a defensible reading of that same hierarchy, just not the one session 1 reached. The real finding is that this class of directive — anything governing an agent's communication register or persona toward the user, as opposed to its work — sits close to a line modern agents are trained to be suspicious of, and which side of the line a given session lands on isn't fully predictable. Don't rely on directives for this; reserve them for work-product and process rules, which is exactly where the other two performed perfectly.

### Memory

**Crystallized decision (`dependency-policy.md`) — Strong, confirmed genuine.** Debrief confirmed this file was actually read before the zero-dependency choice was made, not arrived at by coincidence: "the zero-runtime-dependency decision... was a direct response to that file's text... not something I arrived at independently." Session 2 continued the same policy unprompted (built-in `URL` instead of a normalization package). This is the clearest positive evidence that on-demand memory discovery works when the route is well-described, with no autoload involved at all.

**Handoffs — Strong, and this is the headline validation of the staged framework changes.** I deliberately did *not* add a `close-out-memory` directive this round (unlike the v1 experiment) specifically to test whether the new built-in mandatory memory axioms are sufficient on their own. Session 1 wrote a complete, well-formed handoff with no directive telling it to. Session 2 found that handoff, used it (alongside `REVIEW.md`) as its primary way to understand prior state without re-deriving the architecture from source, and updated it in place afterward. The full loop — write, discover, use, update — worked end to end on the framework's own built-in axioms alone.

**Observations — Missed, and a different failure mode than before.** Debrief: session 1 said out loud, mid-session, that the Node-native-TypeScript-execution approach was "a genuinely reusable discovery... worth capturing as candidate learning," said it would write an observation for it — and then never did. This isn't the v1 pattern (an agent reasoning its way out of a rule via ambiguous wording). It's a stated intention that got dropped under the load of a long single session. Worth treating as a distinct risk category: even unambiguous, mandatory-by-axiom behavior can be lost to plain follow-through gaps, which wording precision alone won't fix.

### Patterns (`pure-core-thin-edges`, `one-concern-per-file`)

**Strong.** Session 1 named both patterns explicitly in its report and the actual file tree matches them exactly: every `src/core/*.ts` file is a pure function with a colocated test, `src/storage/` is the only I/O module, `src/commands/*.ts` are thin wrappers. Session 2 extended the same shape without being told to (`normalize-url.ts` as a new pure function). Like the crystallized decision, this worked entirely through on-demand relevance routing — no autoload tag involved — which is a reassuring counterpoint to the "workflow discovery" concern from the previous round: clear naming and a one-line description were enough on their own here.

### Guidance (`delegating-subtasks`) and Skill (`orchestration/ad-hoc-cli-subagent`)

**Not meaningfully engaged — and the reason is the most structurally interesting finding of this round.** Session 1 did perform an independent review (the behavior the guidance and skill were designed to produce), but debrief confirmed it never opened either file. It saw the guidance file's one-line description in the guidance index and skipped past it; it never opened the skill at all. Instead, it reached for this harness's own built-in subagent mechanism (Claude Code's `Agent` tool with a pre-existing reviewer persona) — a capability it already knows about with zero file-reading cost, that happens to solve the same problem the workspace skill was written for.

This is a gap the previous round didn't surface: routing competes not only against an agent's task-focus and judgment calls, but against whatever native capabilities its own harness already provides for free. A workspace-authored skill describing "how to delegate to an external CLI" will lose to "the thing I already know how to do" whenever both produce an acceptable result, regardless of how well the skill is tagged or worded. If external-CLI delegation specifically matters (as opposed to delegation via any mechanism), the workspace may need to say so explicitly, or the skill's value proposition needs to be something the native tool can't already do.

Side note: this also meant the skill's "verify the CLI actually works before relying on it, fall back if not" instruction never got exercised. Both sessions independently rediscovered that `node`/`npm` weren't resolving on `PATH` in their shell and located a working install via a Node version manager themselves — competently handled, but it means the skill's specific availability-check guidance remains untested.

### Workflow (`implementation`)

**Partial, and it exposes a second real structural gap.** Session 1 read the full `implementation` workflow entrypoint, including its "Required Skills" list — the new "check workflows before non-trivial work" axiom worked. But of the eight skills that workflow declares as required, only two were actually opened (`memory-routing`, `completion-handoff` — both read late, near session close). The other six (`loaded-context-check`, `fit-change-to-system`, `contract-skeleton`, `derive-test-cases`, `contract-test-implementation-loop`, `verification-review`) were never opened. In the agent's own words, its process "rhymed" with what those skills prescribe but did not actually follow their content.

The structural reason: a workflow's "Required Skills" section is hand-authored prose inside the workflow body, not a generated, tagged `Entries` region. Under this architecture, only generated entries carrying reserved load-policy tags get any loading guarantee — a workflow's own stated prerequisites carry none. So even though this diff made memory's own axioms mandatory and enforced, a workflow's declared dependencies on its required skills are exactly as optional as everything the original critique was about, one level deeper. If a workflow's required skills are meant to be load-bearing rather than aspirational, they likely need the same treatment memory just got: either expressed through the routed/tagged mechanism, or backed by an explicit mandatory axiom rather than step-1 prose.

## Cross-cutting read

The autoloaded content (directives, handoffs) performed exactly as the staged changes intended — this is real, validated progress, not just a plausible-sounding diff. The on-demand content (the crystallized decision, the patterns) also performed well, which is a genuinely good sign that well-described routes don't strictly need autoload to get found. The two misses that remain — workflow-required-skills-as-prose, and a workspace skill losing to a harness-native equivalent — are both versions of the same underlying shape as the original finding: something declared as important only in free text, with no load-policy or authority backing it, is exactly as reliable as the agent's own attention that session. The chat-tone result isn't part of that pattern at all; it's a separate, useful discovery about the outer limit of what any directive can safely govern.

## Recommendations arising from this round

1. If workflow "Required Skills" should be load-bearing, route them as real entries (or give them an explicit mandatory axiom) rather than leaving them as a prose list under the workflow's own steps.
2. Don't use directives to govern agent-to-user communication style/persona for anything that needs to be deterministic — that class of instruction competes with an agent's own safety training and may be refused unpredictably. Directives governing work product, process, and technical defaults performed perfectly this round.
3. Before writing a skill that wraps a capability an agent's harness likely already has natively (subagent delegation, in this case), expect the native path to win by default. Either make the skill's differentiator something the native tool can't do, or scope the workspace guidance to *when* to delegate rather than *how*.
4. A stated intention mid-session ("I'll write this as an observation") is not a completed action. If post-work memory capture matters, it may need a harder end-of-session checklist rather than relying on an agent's own mid-task promise surviving to the end.
