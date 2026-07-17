---
open-forge:
  description: Accepted record of the dogfood v2 follow-up opinion on staged framework changes
  tags: [Memory, Document, Record, CurrentTruth, Dogfood, Evidence, Recommendation]
---

# Opinion — Staged Framework Changes (reviewed 2026-07-08)

Reviewed against `recommendations-general.md`, `recommendations-open-forge-native.md`, and the three-session dogfood experiment from the prior review. Two mechanisms drive this diff: a new reserved tag, `#OpenForge` (redefined from a plain branding tag into a load-policy tag meaning "load this by default"), and a new loader axiom — "follow all loaded axioms unless a higher-priority instruction conflicts." Everything else is wording precision on top of those two.

## What this gets right

**It closes the "nothing is mandatory" gap without adding opinionated Core content.** The native recommendations file guessed this would need a directive shipped via extension. Instead, the loader itself now declares every loaded axiom mandatory. That's a better fix than the one I proposed: it applies to every workspace immediately, with no extra install step, and it stays philosophically clean — a memory entrypoint's own operating contract ("write a handoff when work is transferred") is self-governance, not a business opinion, so making it mandatory in Core doesn't violate "Core must stay small and unopinionated" the way shipping a default `close-out-memory.md` directive would have. This is a more disciplined solution than either of my two files proposed.

**It closes the exact loophole the experiment found.** Session 1 of the dogfood run had an agent read the handoffs route, recognize a handoff would help, and talk itself out of writing one by misreading an unrelated "don't create parallel truth" caution. The new handoffs axiom removes that ambiguity directly: *"When work is transferred, delegated, interrupted, or handed to another agent or human, agents must create or update a handoff unless a more specific route already captures the complete resume context."* Concrete trigger, explicit `must`, explicit exception clause. Same pattern applied to observations ("before ending meaningful work, agents write observations...") and to sessions, which now get a stated low-friction default ("if useful work context does not clearly belong elsewhere yet, write it as session context first and reclassify it later") — precisely the fallback I flagged as missing from the taxonomy.

**It fixes root-route autoload more elegantly than my manual patch.** I fixed the v1 sandbox by hand-editing five files' frontmatter tags. This diff instead redefines a tag that was *already present on every default entrypoint* (`#OpenForge`) to carry load-policy meaning. Net effect is the same set of root routes now autoloading, but the mechanism generalizes: any future Open Forge-authored entrypoint inherits the behavior automatically, no per-file maintenance required.

**Workflow discovery gets an imperative push**, not a retrieval engine. "Before non-trivial work, read Entries and load matching workflows" replaces the old passive "use Entries when work matches a goal" framing — exactly the plain-text wording fix the native file recommended over adding embedding search.

**Directives still ship empty.** Correctly so — none of this introduces opinionated default behavior into Core; it tightens how Core describes its own mechanisms.

## The one thing I'd resolve before calling this settled

`#OpenForge` is applied identically to Core scaffolding *and* to first-party extension content — I checked `workflow-essentials` directly. Every workflow entrypoint and every skill entrypoint in that pack, all the way down to leaf skill files like `clarify-intent.md`, already carries `#OpenForge` in its tags. Traced through the new recursive rule ("nested Open Forge entrypoints load as their parent Entries become visible"), installing `workflow-essentials` doesn't just make the workflow *menu* visible by default — it forces the full text of all three workflow definitions and all fifteen skill files into every loaded context, on every request, regardless of relevance:

```
loader → workflows/_workflows.md (auto)
       → vision/_vision.md, architecture/_architecture.md, implementation/_implementation.md (all #OpenForge → auto, full ~55-line bodies)
loader → skills/_skills.md (auto)
       → 4 skill sub-categories (all #OpenForge → auto)
       → 15 individual skill files (all #OpenForge → auto)
```

That's likely several thousand tokens of permanent baseline for one modest extension, growing with every additional pack a workspace installs. I don't think this is what "default payload" in the routing descriptor meant to cover — that phrase reads like it's describing Core's empty scaffolding, but the tag itself doesn't distinguish Core from Extension, and nothing in the diff carves out an exception. `backlog.md`'s new line — *"investigate an optional all-in-one generated index for agent cold starts; judge token cost, staleness risk, authority confusion"* — suggests this is already on your radar rather than a blind spot, which is reassuring, but I'd resolve it explicitly rather than let it surface as a surprise the first time someone installs two or three extension packs. Two honest ways to resolve it: scope `#OpenForge` load-policy behavior to Core-owned entrypoints only and give extensions a separate, opt-in convention; or keep it as-is and document plainly that installing an extension means committing its full definition set to permanent context by design. Either is defensible — right now it reads as unresolved rather than chosen. I'll check what actually happens in practice as part of the v2 dogfood run.

## Smaller notes

- `directives/_directives.md`, `memory/_memory.md`, and the `working`/`crystallized` entrypoints now carry both `#OpenForge` and `#LoadWithParentEntrypoint`. Functionally redundant under the new rule (either tag alone would force the load) — harmless, but worth one line in the descriptor explaining whether that's intentional belt-and-suspenders or a cleanup left for later.
- The framework used to say directives are the only mandatory primitive. That's no longer quite true — any loaded axiom anywhere is now mandatory by the loader's own rule. Worth a short clarifying line somewhere (`agent-primitives.md` seems like the right owner) on what's still distinctive about directives specifically: portable, user-authored, arbitrary-scope mandatory rules, versus a primitive's own axioms, which are mandatory but scoped strictly to how that primitive is used. Not a defect, just a definition that's now slightly out of date relative to the mechanism it used to be the only instance of.

## Addendum — measured, not just traced

I installed the updated framework plus `workflow-essentials` into a fresh sandbox and counted the actual files that fall inside the `#OpenForge`/`#LoadWithParentEntrypoint` recursive chain: loader, all seven root entrypoints, the full memory tree (including `working/handoffs/` and `working/sessions/`, now correctly included), all four skill sub-categories, all three workflow definitions, and every leaf skill/workflow file under them. Total: **1,116 lines** of permanent baseline content, before a single word of user work. That's a concrete number to weigh against the "token cost, staleness risk" question `backlog.md` already flags — worth deciding deliberately rather than discovering per-extension.

## Net read

This is the right fix, done more carefully than what I'd have shipped myself, and it targets the actual causal mechanism the experiment isolated rather than the surface symptom. The extension-cascade question is the only thing I'd want an explicit answer on before treating this as done — everything else here is a genuine improvement with no real downside I can find.
