---
open-forge:
  description: Whether Memory holds behavior it should not, measured by prescriptive language density, and whether the fix is a negative axiom or a redirect clause on scope responsibility
  tags: [Memory, Analysis, Contextual, Candidate, Framework, Design, Authority, Axioms, Boundary]
---

# Memory Authority Boundary

## Conclusion

The boundary you describe is **already stated, already correct, and violated at
scale**. Memory carries **32× more mandatory language than Directives** — the
category whose entire job is mandatory instruction.

The rule exists as a positive axiom and did not hold. But the fix is not a
negative axiom. It is a **redirect clause**: each scope names what it defines
_and where the adjacent thing goes_. That keeps a destination, works as a
two-sided boundary, stays finite, and is mechanically checkable — which a
prohibition is not.

## The formulation is right

> Memory is not authority and definitely not behavior. Memory can be current
> truth of what is, how is, why is — but not how to do.

Tested against the shipped scopes, it holds, including at the edges:

- `crystallized/decisions` — _"What was chosen, why, and what follows from the
  choice?"_ The "what follows" is a **fact about a decision**, not an
  instruction. "We chose X, so Y applies" is memory; "you must do Y" is not.
- `working/checkpoints` — _"Where does this workstream stand, and what comes
  next?"_ "What comes next" is **state**, not a rule.

The clean test is **grammatical mood**, and it is measurable:

> **Memory describes. Core prescribes.**

Descriptive: _is, was, holds, resulted in, remains_. Prescriptive: _must, must
not, never, always, do not_. That distinction is the whole boundary, and it can
be counted.

## It is already in the payload — twice

**The questions exist, and they are good.** Every entrypoint carries one:

| Scope                           | Question                                                      |
| ------------------------------- | ------------------------------------------------------------- |
| `directives`                    | What **behavior is required** in this scope?                  |
| `guidance`                      | What approach is **recommended**, and when does it fit?       |
| `patterns`                      | What reusable **shape** makes related work easy?              |
| `workflows`                     | What repeatable **method** can help reach this goal?          |
| `memory`                        | What is worth **remembering**?                                |
| `memory/crystallized/documents` | What is the **complete current explanation** of this subject? |
| `memory/crystallized/decisions` | What was **chosen**, why, and what follows?                   |

Your idea of "putting exactly these questions somewhere in src" is already
implemented, and better than most systems manage. The vocabulary separates
cleanly: _required / recommended / shape / method_ against _remembering /
explanation / chosen_.

**And the rule exists as an axiom**, in `memory/_memory.md` under _Placement And
Lifecycle_:

> _"Put accepted behavior that should guide future work in the matching #Core
> route. Keep useful context or reasoning in Memory."_

with a supporting line under _Authority And Classification_:

> _"Recording content from another category does not give the record that
> category's role."_

Both are exactly right. Both are buried — the first is the third bullet of the
third subsection of a ~25-bullet axiom list.

## It is violated at scale

Occurrences of mandatory language, measured across the repository:

| Term       | `memory/crystallized/documents` | `directives` |
| ---------- | ------------------------------- | ------------ |
| `must`     | **380**                         | 22           |
| `must not` | **29**                          | 7            |
| `never`    | **741**                         | 10           |
| `always`   | **44**                          | 1            |
| `Do not `  | **94**                          | —            |
| **total**  | **1,288**                       | **40**       |

The scope that asks _"What is the complete current **explanation**?"_ contains
741 instances of "never".

The bulk of it is the CLI contract set — 40,540 lines of _"The implementation
must…"_, _"…never falls back to compact"_ — living in
`memory/crystallized/documents/cli/`. By the framework's own axiom that is
accepted behavior guiding future work, and it belongs in a `#Core` route.

### Why this matters beyond tidiness

This is not a filing error. It has a consequence visible in
[contract-versus-code.md](contract-versus-code.md):

**Memory and Core have different review semantics.** A Directive is binding
instruction and is read as something to argue with. A Crystallized Document is
_"the complete current explanation"_ — a record of what is. When 1,288 binding
statements are filed as explanation, they inherit the wrong reading: **nobody
reviews an explanation for whether it should be true.**

The audit found that most defects were specified rather than introduced, and
that the process had no stage asking _"should this contract exist?"_ Part of the
answer is that the contracts were never in a category whose questions invite
that challenge. `directives` asks _"What behavior is required?"_ — which invites
"is it?". `crystallized/documents` asks _"What is the complete current
explanation?"_ — which invites only "is it complete?".

**The misplacement helped produce the outcome.**

## On negative axioms

You have avoided them, and that instinct is mostly right. The general case
against holds:

- They enumerate a space that cannot be enumerated. Every incident adds a
  "don't", and the list becomes scar tissue.
- They leave no destination. "Do not put behavior here" does not say where it
  goes, so the author guesses again.
- They read as distrust, and they age badly — a prohibition outlives the
  incident that caused it.

But this specific failure resists the positive form, and the evidence is that
**the positive rule was already there and was violated 1,288 times.**

The reason is that misplacement is not an ignorance failure. The author is not
missing a rule; they are writing genuinely accepted, genuinely durable content,
and _every category's positive question sounds plausible_ when you are in the
middle of writing prescriptive material. "What is the complete current
explanation of this subject?" is a perfectly reasonable description of a command
contract. Nothing in the positive form says _stop, this belongs one route over_.

**A boundary is inherently two-sided.** "This scope holds X" is weaker than
"this scope holds X, and Y goes there" precisely when X and Y are adjacent — and
`directives` / `guidance` / `patterns` / `workflows` / `crystallized/documents`
are the most adjacent categories in the framework.

### The recommendation: redirect, not prohibit

Do **not** adopt negative axioms as a general practice. Instead, use the
mechanism the Loader already defines and nothing uses.

The Loader defines `responsibility`:

> _"Optional sentence that helps an editor decide what belongs in a file by
> stating what it defines. It does not create authority or loading behavior."_

It is optional, unused by any command, and surfaced nowhere. Make it **required
on entrypoints, with two halves**:

```yaml
open-forge:
  responsibility: >
    Defines the complete current explanation of an accepted subject.
    Behavior that must be followed belongs in directives;
    recommended approaches belong in guidance.
```

That form is a **redirect**, not a prohibition:

- it keeps a positive destination — the author is sent somewhere, not stopped;
- it is two-sided, so it works as a boundary;
- it is finite — each scope names its two or three nearest neighbours, not every
  possible wrong;
- it ages well, because it describes the map rather than an incident;
- it is surfaceable: `route inspect` should print it, which answers the
  "what belongs here" gap identified in
  [taxonomy-and-adoption.md](../cli-experience-audit/taxonomy-and-adoption.md).

This is the negative content you actually want, in a positive shape.

### One genuine negative, at the top

If a single prohibition is added anywhere, make it one line under Memory's
Authority section, stated as identity rather than as a rule:

> **Memory describes. It does not prescribe.** A record may state what is, how
> it is, and why it is. Behavior that must be followed belongs in `directives`;
> a recommended approach belongs in `guidance`; a reusable shape belongs in
> `patterns`; a repeatable method belongs in `workflows`.

One sentence, a destination for each alternative, no enumeration of wrongs.
That is worth its cost; a general practice of negative axioms is not.

## A mechanical check

The measurement above is cheap enough to run as a `doctor` finding, and it is
the rare structural rule that can be checked without semantics:

```
route.prescriptive-language

  .agents/memory/crystallized/documents/cli/contracts/find/interface.md
    47 mandatory statements (must, never, always) in a Memory scope.
    Memory describes; behavior belongs in directives.
```

Tune it by density rather than count, exempt quoted material, and default it to
`info` — with the `rules` config from
[repository-dogfood-and-configuration.md](../cli-experience-audit/repository-dogfood-and-configuration.md)
letting a workspace turn it off. It would have caught this on the first contract
written.

## Related, and worth deciding together

The same "the mechanism exists and nothing uses it" pattern appears three times
in this audit:

- `responsibility` — defined by the Loader, unused by any command.
- The entrypoint questions — present, well-written, never checked against the
  content beneath them.
- The prescriptive/descriptive distinction — stated as an axiom, never enforced.

All three are answers to _"what belongs here?"_, and all three are inert. That
is a stronger finding than any of them alone: **the framework has good placement
guidance and no placement feedback.** Surfacing `responsibility` in
`route inspect`, checking language mood in `doctor`, and printing the scope's
question when a route is created would make the existing design work without
adding a single new concept.
