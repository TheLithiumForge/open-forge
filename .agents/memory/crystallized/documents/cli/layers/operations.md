---
open-forge:
  description: The Operations layer - where a command turns a request into one result, through selection, planning, application, and result formation
  responsibility: Define the shape of a command operation, its four internal stages, and the boundaries a command may not cross
  tags: [Memory, Crystallized, Document, CurrentTruth, Evergreen, CLI, Architecture, Operations, Commands, Planning]
---

# Operations Layer

Operations answer **"what does this command mean?"**

This is the only layer that holds product meaning. Shell knows nothing about
routes; Framework knows facts but no policy; Presentation knows how to write an
answer but not which answer is right. A command operation is where a request
becomes exactly one result.

The [land Architecture](../architecture.md) records how this layer sits against
the others. The [Command Contract Set](../command-contract-set.md) and the
per-command [contracts](../contracts/_contracts.md) define what each command
means; this record defines the shape they share.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## One Operation, One Result

An operation is invoked at most once per process. It receives one complete,
immutable request and returns one concrete result carrying the complete command
payload. It does not write output, choose a renderer, read the parse tree, or
consult process state.

Every terminal condition is a result, not an exception. An operation that cannot
proceed returns a result whose semantic status says so, with findings that name
the cause. Unknown finite values fail closed.

The result carries the complete facts established by the operation. It does not
apply a detail level, severity filter, text format, or stream decision. Before a
result reaches Presentation, any Framework fact needed in command output is
projected into a command-owned value under `Commands/<Owner>/Models/`. The
Presentation selector consumes that value rather than importing the Framework
model.

## The Four Stages

A command operation moves through the same four stages whether it reads or
writes. Read-only commands stop after the third.

### Selection

_What does this request actually ask for?_

Resolve requested references into retained identities, decide which sources and
which parts of them are in scope, and record what was requested separately from
what resolved. A reference that does not resolve is a finding, not a silent
omission.

Selection happens before reading, and it is the reason a read-only command does
not open every file in the workspace.

### Reading

_What is true right now?_

Ask Framework for facts: the source catalogue, route facts, parsed documents,
lifecycle records, permissions, Library inventory. The operation composes those
facts; it does not re-derive them. A command that parses a document format
itself has reached past this stage into Framework's job.

Facts are read once and retained. Re-reading the same fact during planning or
rendering is how two parts of one result come to disagree.

### Planning

_What would change, and is that safe?_

Form the complete intended change as data, before anything is written: every
directory creation, every file change, every generated region, every recovery
target, and the lifecycle record that will result. A plan is inspectable, which
is what makes `--dry-run` the same code path as an apply rather than a separate
one.

A plan that cannot be formed safely is a blocked result with the blocking
finding, not a partial plan.

### Application

_Apply the plan, verify it, and say what happened._

Mutating commands hold the workspace lock, revalidate the plan against the
filesystem, apply effects through Framework's mutation boundary, verify each
effect, and publish the lifecycle record. Recovery data is prepared before
effects and released after verification.

Every applied effect carries its outcome into the result. An effect whose
completion cannot be established is reported as unknown rather than assumed.

Read-only commands never reach this stage and never acquire the lock.

## What A Command May Not Do

- **Parse a document format.** Documents owns that. Ask for the facts.
- **Reach into another command.** No command imports another command's private
  `Shared` namespace. Where two commands need the same meaning, it moves to
  their nearest shared parent, and only when the meaning is genuinely identical.
- **Restate a shared vocabulary.** Wire names, human wording, the
  results-and-streams help body, and escaping each have one owner.
- **Decide what to show.** That is the selection stage in
  [Presentation](presentation.md). An operation puts everything it established
  into the result.
- **Expose Framework models to Presentation.** A command result projects the
  needed fact into an owner-local value first; Presentation may consume only
  that command-owned model.
- **Write output or choose an exit code.** Shell owns process completion.

## Zero Effects Is An Answer

An apply that produces no effects has to say why. "Already current" and "this
package delivered nothing" are different outcomes, and a result that renders both
as `Effects: 0` with a complete status has failed to answer the question the user
asked. The distinguishing fact is available at planning time; the result carries
it.
