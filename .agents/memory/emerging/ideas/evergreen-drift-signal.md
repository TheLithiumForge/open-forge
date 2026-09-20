---
open-forge:
  description: Give documentation duty a feedback loop instead of a norm, because agents reliably keep the part of an increment that something checks
  tags: [Idea, Emerging, Contextual, Candidate, Framework, Evergreen, Documentation, Dogfood, CLI]
---

# Evergreen Drift Needs A Signal, Not A Rule

## The observation

Across Task 30 G1 step 4 an agent updated task records continuously and left the
accepted contract documents stale until the maintainer asked for them. The
records were updated inside the working loop; the contracts were treated as a
later pass.

The maintainer asked why, and rejected "I did not get to it" as an answer. The
honest mechanism is worth recording, because it is structural rather than
personal and it will repeat with every agent.

## Why it happens

An increment had two halves. The code half had an automatic, immediate,
unambiguous verdict: the build and three test suites. The documentation half had
no verdict at all. Nothing failed, nothing turned red, nothing asked.

Given finite attention, the half with a feedback loop is the half that survives.
This is not a motivation problem and cannot be fixed by restating the duty more
firmly.

Two framework rules should have caught it, and could not:

- The Task's own acceptance rule says output-changing work must define and review
  its contract **before** renderer rewrites. The agent inverted the order and
  nothing objected, because nothing was watching the order.
- The Evergreen axiom says update affected material "before work depends on it
  and no later than closeout". Both triggers are judgements with no observable
  boundary. **`closeout` is not an event in Open Forge.** In a long session
  nothing ever announces it, so the rule asks the agent to interrupt itself at
  exactly the moment it is deepest in code.

A rule whose trigger only exists inside the agent's own attention is a norm.
Norms lose to feedback loops.

## Candidate remedies

Ordered by strength. They compose; the first is the cheapest real fix.

### 1. Make the contract example a test

The CLI contracts already carry fenced `json` examples of real command output.
Render the actual document for a seeded result and assert the contract's example
matches. A stale contract then fails the unit suite like anything else.

This repository is unusually ready for it: the examples are already machine
parseable, result seeds exist for some commands, and the snapshot package is
wired in. The work is building the missing seeds, not inventing a mechanism.

Cost: contract examples must stay exactly renderable, which constrains how they
are written. That constraint is arguably the point.

### 2. Bind the document to the commit, not to closeout

A mechanical check: a commit touching a command's presentation or rendering paths
must also touch that command's contract file. Runs in CI or as a hook, needs no
semantic understanding, and catches exactly the failure observed here.

Cost: false positives on refactors that genuinely change no contract, which need
an explicit escape.

### 3. Generate the shape, stop mirroring it

The type blocks in the interface contracts are hand-maintained mirrors of C#
records already described by a source-generated JSON context. Emitting them
removes the duty rather than policing it.

Cost: the contracts stop being wholly authored prose, and the generated region
needs an owner.

### 4. Make closeout an observable event

The deeper fix, and the one that belongs to Open Forge rather than to this CLI:
`closeout` should be something a tool can announce. The CLI already understands
routes and references, so it can answer "which #Evergreen records point at
material that changed after the record last did". That is the missing signal,
and it is the Framework dogfooding its own navigation.

Cost: needs a definition of "changed after" that survives a clone, where file
timestamps do not. Git history answers it, but the accepted dependency policy
keeps Git off the required path, so this needs a decision first.

## What would justify promotion

Any of these becoming a Directive needs evidence that it changes behaviour rather
than adding ceremony. The cheap experiment is remedy 1 on a single command: add
the rendering test, then see whether the next output change arrives with its
contract already updated. If the agent still forgets, the signal was not the
missing piece and this record is wrong.

Related: [Documentation comprehension probes](documentation-comprehension-probes.md)
asks whether a reader understands a document; this asks whether the document is
still true.
