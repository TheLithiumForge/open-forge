---
open-forge:
  description: Why models over-tag LoadNow and under-scope context, and what would make progressive disclosure a default habit rather than an axiom
  tags: [Memory, Analysis, Contextual, Candidate, Framework, Loading, Scope, ProgressiveDisclosure, Context]
---

# Loading And Scope Discipline

## Conclusion

Models using the framework follow routes correctly but do not internalise
`#LoadNow` discipline or task-shaped scoping. That is not a model failure. Four
properties of the current design make over-loading the rational choice, and the
strongest of them is that **the shipped framework demonstrates the opposite of
what it teaches**.

The deepest cause is last in this document and worth stating first: models
over-load because the on-demand path is not trustworthy. Fixing `find`,
`references`, and descriptions does more for loading discipline than any change
to the tag itself.

## The example contradicts the axiom

The Loader states the policy once:

> Each scoped `entrypoint` chooses the loading its contents justify. **Keep
> entries on demand by default.** Use #LoadNow only when missing the content
> would cost more than reading it whenever its parent loads.

Measured on a bare `open-forge install`:

|                                      | Count  |
| ------------------------------------ | ------ |
| Entrypoints in the shipped Framework | 19     |
| Carrying `#LoadNow`                  | **15** |
| Root routes under the Loader         | 8      |
| Root routes carrying `#LoadNow`      | **7**  |

Only `templates` is on demand. `status` on that bare install, with zero user
content, reports:

```
Startup: 19 files, 33,016 characters, ~8,254 tokens
Startup share: 81.1%
```

**81% of everything available loads before the task is known.** A model reading
"keep entries on demand by default" and then observing that seven of eight root
routes are `#LoadNow` will follow the example, not the sentence. Examples beat
axioms, and this example is unanimous.

The contrast with authored content is sharp. In this repository, which has real
content, **26 of 138 entrypoints (19%)** carry `#LoadNow` — the human author
follows the axiom. The 79% figure is the Framework's own defaults.

### Proposed result

Ship `#LoadNow` on `directives` only, and leave `loader.md` itself as the entry.
Everything else on demand, discovered through its description. The Framework
must model the behaviour it asks for. If `guidance`, `patterns`, `maps`,
`skills` and `workflows` genuinely need to be resident for every task, the
axiom is wrong and should be rewritten to say so — but they do not: a task that
needs a pattern can find it through `maps` or `find`.

## The policy is separated from the tag

`#LoadNow` is defined under **Tags And Loading**:

> `#LoadNow` — Read this entry in listed order when an already-loaded parent
> exposes it. If it is an entrypoint, apply the same rule to its Entries.

That is mechanics only. The _policy_ — when it is appropriate — lives in a
Routing axiom several sections earlier. A model asking "what does `#LoadNow`
do" gets the answer and never encounters the constraint.

### Proposed result

Put the decision procedure next to the tag definition, phrased as a test rather
than a preference:

> Before tagging `#LoadNow`, name the task shape that would fail without it. If
> you cannot name one, the entry is on demand. `#LoadNow` is a claim that every
> task in this scope needs this content, and it is paid on every task.

## The blast radius of the tag is invisible

`#LoadNow` is transitive: _"If it is an entrypoint, apply the same rule to its
Entries."_ Tagging one folder silently conscripts its entire `#LoadNow` subtree.
The author of the tag cannot see what they just added, at the moment they add it.

`route inspect` has exactly the right data:

```
Context size
  Own source: 1 file · 1.71 KiB · ~438 tokens
  Selecting this route adds: 0 files · 0 B · ~0 tokens
  Automatically read below it through #LoadNow: 2 files · 1.65 KiB · ~423 tokens
```

But it is only available after the decision, and only if someone goes looking.

### Proposed result

Print the delta at the moment of the decision. `route create`, `route update`
and `route init` should report:

```
Created memory/emerging/analysis/x  #LoadNow

  Startup context: 8,254 -> 9,102 tokens  (+848, +10%)
  This is now read before every task in this workspace.
```

This is the single highest-leverage change in the document. A price tag shown at
the point of purchase changes behaviour in a way a policy sentence does not.

## There is no budget, no attribution, no feedback

`status` reports `Startup share: 81.1%` against no target. Nothing says whether
that is good. It attributes _continuity_ sources:

```
Largest continuity sources
  memory/emerging: 1751 bytes
  memory/emerging/observations: 1119 bytes
```

but not startup sources — the much larger number is unattributed. Nothing warns.
`doctor` has no check for it.

Worse, the measurement disappears exactly when it is most needed. On this
repository, whose workspace is blocked by 66 malformed files
(see [interoperability-and-diagnosis.md](interoperability-and-diagnosis.md)):

```
Startup: unavailable files, unavailable characters, ~unavailable tokens
Startup share: unavailable
```

### Proposed result

- Attribute startup the way continuity is attributed, and sort by cost:

  ```
  Startup context: 8,254 tokens (81% of everything available)
    directives    2,900   #LoadNow
    guidance      1,850   #LoadNow
    patterns      1,400   #LoadNow
    ...
  ```

- Add an optional workspace `startupTokenBudget`. `doctor` reports over-budget
  as a warning with the same attribution, so the axiom becomes an enforced rule
  rather than advice.
- Compute context size from whatever sources parse, and report the rest as
  excluded. A blocked route should not erase the whole measurement.

## The decision is global, static, and made by the wrong party

The tag lives in the child's frontmatter. Consequences:

- **The child decides for every parent.** A scope exposed by two parents loads
  identically under both, even when it is core to one and incidental to the other.
- **The decision cannot vary by task.** The Loader says _"Use the request and
  accepted context to resolve unclear choices"_ and _"Recheck selection after an
  important task change"_ — but selection has no task input. A model cannot
  express "load `patterns` for this implementation task and not for this review."
- **Relevance is judged from descriptions alone.** That is a real judgment call
  the model must make on every entrypoint, with no tooling.

### Proposed result

Do **not** add conditional tags (`#LoadNow(when: implementing)`). That pushes
task semantics into a static file format and makes every entrypoint harder to
author and reason about.

Instead, keep tags static and give the _task_ a verb. This belongs in `rune`,
not the CLI — the CLI stays contract-scoped and `rune` owns relevance judgment:

```
$ rune scope "fix a CLI rendering bug"
  always      directives                     2,900
  selected    patterns/cli                   1,100   matched #CLI #Presentation
  selected    memory/crystallized/documents/cli   3,400
  skipped     memory/archived/*              #Archived
              guidance, templates, workflows  not matched

  6,400 tokens instead of 24,100
```

That turns scoping from an implicit obligation on the model into an explicit,
inspectable step — which is the only way it becomes second nature.

## The real reason models over-load

Models over-tag `#LoadNow` and over-select scopes for the same reason a person
would: **the on-demand path is not reliable, so loading everything is the safe
choice.** In the current CLI:

- `references` reports **zero links for every source** in a stock workspace, so
  the navigation graph is not traversable on demand
  (see [presentation-field-audit.md](presentation-field-audit.md), finding 6).
- `find` with no filter dumps all 21 sources with `Matched: none`, and with a
  filter repeats the query on every hit. It is not composable — the ID and the
  path are on different lines, so it cannot be piped.
- `route list` truncates at depth 1 while reporting `Coverage: complete`, and
  never mentions `--depth=all`. A model does not know what it has not seen.
- Descriptions are the only relevance signal, and `route init` generates
  `Draft route for X; replace this description before relying on it` — a
  placeholder that carries no signal at all, on every scaffolded scope.

A model that cannot trust retrieval will pre-load. Every fix in
[command-output-design.md](command-output-design.md) that makes `find`,
`references` and `route list` reliable is also a loading-discipline fix.

### Proposed result

Treat these as one workstream, not two. Ordered by effect on loading behaviour:

1. Fix `references` to traverse generated Entries. Without it there is no graph.
2. Make `find` composable — one line per hit, `--paths` for piping.
3. Make `route list` honest about truncation.
4. Require a real description before a route counts as authored, and make
   `#NeedsAuthoring` block nothing but visibly degrade relevance.
5. Only then adjust the shipped `#LoadNow` defaults, so the on-demand path
   people are pushed onto actually works.
