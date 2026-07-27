---
open-forge:
  description: Accepted record of Open Forge-native recommendations constrained by the framework's design principles
  tags: [Memory, Document, Record, CurrentTruth, Dogfood, Recommendation, Analysis]
---

# Open Forge — Improvement Recommendations (Native Lens)

Same ten findings as `recommendations-general.md`, re-derived under the constraints Open Forge already declares for itself: Core stays small and unopinionated ([Framework Architecture](../../../.agents/memory/crystallized/documents/framework/architecture.md)), no hidden runtime or vendor-shaped ritual ([README](../../../README.md)), routing metadata never becomes authority on its own ([Routing Model](../../../.agents/memory/crystallized/documents/framework/routing/model.md)), and customization proceeds local file → overwrite → base edit, in that order ([README](../../../README.md), [Overwrite Customization](../../../.agents/memory/crystallized/documents/framework/routing/overwrites.md)).

A few of the general recommendations survive almost unchanged — they were never actually in tension with the philosophy, just under-executed. Others get reframed into a shape the framework's own rules would accept. Numbering matches the general file for direct comparison.

## 1. Nothing is actually mandatory in a fresh install

**Reframe:** Don't put opinionated directive content in Core — `layers.md` is explicit that "#Core must stay small... it does not seed opinionated behavior." But `layers.md` and `agent-primitives.md` already sanction directives as *extension* content ("an #Extension may add primitive files... directives remain mandatory in scope"). So: ship the close-out-memory directive (and a pre-work workflow-check directive) as a small opt-in extension — either folded into `workflow-essentials` or as a new minimal pack — installed the same way `workflow-essentials` is today, via `open-forge extend`.

**Why this fits:** It gets the exact behavior the experiment validated (a real directive, not advisory prose, is what produced compliance) without breaking the rule that a bare `open-forge install` stays minimal. A user who wants a docile-by-default agent runs one more `extend` command; a user who wants pure minimal Core still gets it.

**Priority:** High.

## 2. Root-route autoload is too narrow by default

**Reframe:** A blanket "autoload everything by default" undoes a deliberate choice — `routing.md`'s alignment checks name exactly which entrypoints get `#LoadWithParentEntrypoint` as intentional scope, not an oversight. Making that the new shipped default for every workspace re-introduces the "complete defaults assume everyone works the same way" problem the README argues against. Instead: document this as a named, first-class *customization pattern* — a short section in `docs/cli.md` or the README's "Growing Your Framework" section showing exactly the five-file frontmatter edit you already did, with the tradeoff stated plainly (more guaranteed visibility vs. a larger loaded-by-default footprint). Let each workspace opt in deliberately, the same way it opts into an overwrite.

**Why this fits:** It keeps Core's default footprint small (the actual design goal) while making the fix a two-minute, well-documented action instead of something a user has to discover by watching an agent fail first, the way you did.

**Priority:** High (as documentation/pattern work, not a Core default change).

## 3. Autoload stops one layer too early inside Memory

**Reframe:** Deepening the autoload chain into `handoffs/`, `sessions/`, `decisions/`, `documents/` content bodies would conflict with `emerging/_emerging.md`'s own explicit stance against "preloading every memory child body." But there's a narrower move available: `working/handoffs/` and `working/sessions/` are bounded, self-pruning state (the entrypoint's own axioms already say to keep them small and extract-then-clear) — unlike `emerging/observations/`, which is deliberately excluded from autoload via `#LoadForPostWorkReview` specifically to avoid preloading unbounded candidate material. Extending `#LoadWithParentEntrypoint` to the `working/*` child *entrypoints only* (not their content files) is consistent with the existing distinction the framework already draws between working and emerging state.

**Why this fits:** It respects the same line the framework already drew between "small, bounded, safe to preload" (working) and "candidate, unbounded, review-only" (emerging) — it just applies that line one folder deeper than the current install does.

**Priority:** Medium.

## 4. Load-policy tags are pure convention — nothing checks compliance

**Reframe:** A transcript-scanning compliance engine is exactly the "hidden runtime" the README rejects — Open Forge's whole pitch is that a human can read every file that governs behavior. Keep any checking mechanical and file-based, not agent-behavior-based: extend the already-backlogged `doctor`/validation command (`.agents/memory/backlog.md` already lists this) to do *structural* git-log correlation only — e.g., "commits touched `src/` N times since the last entry under `working/handoffs/`, flag for human review." That's inspectable, deterministic, and requires no model calls, consistent with the CLI staying "intentionally small."

**Why this fits:** It gives you a real signal without adding the one thing Open Forge explicitly promises it won't be: an opaque process second-guessing the agent's actual reasoning.

**Priority:** Medium.

## 5. Axiom wording is soft enough to be talked around

**Reframe:** This one needs no philosophical translation — it's squarely inside work the project has already scoped for itself. `backlog.md`'s "Terminology Pass" section already calls for cleaning up route descriptions and loader tag meanings after dogfooding. Fold this finding directly into that planned pass, with one concrete addition: clarify, in `loader.md`'s Customization section (or wherever "do not create parallel truth" lives), the difference between *creating a new memory route/category* (needs discussion — the thing that axiom is actually protecting) and *writing a routine entry into an already-installed route* (the route's normal, expected use, no permission needed). That's the exact seam the experiment's session 1 agent exploited.

**Why this fits:** Formatting and agent-primitives already ask for compact, precise axioms; this is a precision bug inside work already planned, not new scope.

**Priority:** High.

## 6. No convention for cold-started subagents

**Reframe:** Defining a new formal "subagent contract" primitive would be adding a new top-level concept, which `layers.md`'s Growth Contract says needs strong justification ("new root categories or new #Memory states require stronger justification because they change the top-level routing model"). This doesn't need a new primitive — it's squarely what `workspace/` routes already exist for ("workspace routes point to important project locations and explain when to use them"). Recommend it as a pattern, not a Core addition: a user-authored `.agents/workspace/subagent-handoff.md` (or a line appended to the local `AGENTS.md` block) telling a freshly spawned agent to read the loader first. The framework's docs can show this as a worked example, the same way `docs/cli.md` shows scope-route examples, without shipping it as installed content.

**Why this fits:** "Grow your own framework primitives instead of inheriting a giant default pack" is stated as a core value — this is a textbook case of a workspace-specific need that should be grown locally, not centralized.

**Priority:** Medium.

## 7. Memory taxonomy has more decision points than the soft language can support

**Reframe:** Collapsing the working/emerging/crystallized/archived taxonomy would cut against the framework's central thesis — organic, self-growing memory *is* the product, not an incidental feature. The native fix isn't fewer categories, it's a stated default path through them: add one line to the axioms of `working/sessions/_sessions.md` specifically — "if unsure whether a note belongs here or in observations/analysis, write it here; it is the cheapest state to reclassify later" — giving agents a designated low-friction landing spot without touching the taxonomy's structure or its value for careful, deliberate use.

**Why this fits:** It preserves the full expressive model for users who want it, while giving agents under time pressure a stated "when in doubt" answer instead of silence — the same wording-precision fix as item 5, applied to a different axiom.

**Priority:** Medium.

## 8. No behavioral test coverage, only structural test coverage

**Reframe:** `docs/extensions.md` already anticipates this exactly: "Workflow tests are future work. They should exercise real installed payloads, not only source files." This isn't new scope, it's an existing open item that just needs a concrete method attached to it. Recommend formalizing the sandbox method used this session — install → apply a variant (routing fix, directive, extension) → run one real small task through an agent → diff the resulting repo tree against an expected shape — as a documented maintainer checklist in `docs/dev.md`, run before releases. Keep it a human-run exercise, not a CI gate, since agent output is non-deterministic and a flaky gate would undermine trust in the framework's plain-file promise rather than support it.

**Why this fits:** It's the exact test method already named as future work, made concrete, without inventing new machinery or claiming false determinism.

**Priority:** Medium.

## 9. The project doesn't dogfood its own shape yet

**Reframe:** No translation needed — this is already `backlog.md` item #2 under "Alpha Sequence." The native recommendation is simply: this session's experiment is a good forcing function to do it now rather than defer again, and the dogfooding pass should adopt the directive-as-extension approach from item 1 rather than editing Core, so the project's own workspace demonstrates the same customization path it recommends to users.

**Why this fits:** It's already-agreed scope; the only change is urgency, backed by fresh evidence of what happens without it.

**Priority:** High (already planned; this session adds a concrete reason to stop deferring).

## 10. Workflow discovery relies entirely on an agent correctly matching free text

**Reframe:** Embedding search or an LLM router is exactly the kind of "hidden runtime" the framework is built to avoid — it would mean the actual routing decision no longer lives in a file a human can read and diff. The native fix stays inside plain text: tighten the description and tag precision on `vision/_vision.md`, `architecture/_architecture.md`, and `implementation/_implementation.md` so their positive scope is unambiguous from the one-line entry alone (this is what `formatting.md` already asks every entry to achieve), and add one imperative line to `workflows/_workflows.md`'s own axioms — "before starting non-trivial work, check whether an established workflow applies" — rather than trusting the advisory "Use Entries when current work matches a repeatable goal" framing to prompt that check on its own.

**Why this fits:** It's the same fix as item 5 and item 7 — sharpen axiom wording from advisory to imperative — applied to the workflow-selection moment specifically, with no new mechanism introduced.

**Priority:** Low-to-Medium (cheap wording fix now; revisit retrieval only if wording alone proves insufficient after dogfooding).
