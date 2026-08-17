---
open-forge:
  description: Reusable default shape for one inspectable CLI operation
  responsibility: Define a product-agnostic command, input, stage, result, and review shape for predictable CLI operations
  tags: [Pattern, CLI, Interface, Command, Flag, Composition, Result, Testing, Predictability]
---

# Composable CLI Operation Pattern

This Pattern is a reusable default shape for a CLI operation that a reader can
discover, inspect, test, preview when appropriate, and review without knowing
its implementation. A product's current Interface and Behavior Contracts control
exact command names, flags, statuses, effects, and recovery rules. This Pattern
does not create that authority.

## Shape

Design one command path around one complete, coherent operation:

```text
<cli> <operation> [subjects] [modifiers]
<cli> <group> <operation> [subjects] [modifiers]
```

Use a direct command when one stable operation owns the subject and lifecycle.
Use a group only for several actual operations that share a coherent subject or
lifecycle. A group with one ceremonial child adds no useful boundary. A group
performs no domain operation or wizard by itself; its bare form shows help.

Keep the discoverable path shallow by default. The product may choose a deeper
path when its domain requires one, but the reason is a deliberate variation
point that should be visible in review. Every leaf performs one complete
operation. A modifier may select compatible input, add compatible behavior,
choose projection, or set one write policy. It must not turn the leaf into a
different operation.

## Subjects And Input

Let the command path select the operation. Use positional operands for primary
subjects and query values unless naming a predicate field is important to
understanding the input:

```text
<cli> inspect <subject>
<cli> move <source> <destination>
<cli> search <field-value>
```

The examples are schematic. A product may use a group for the second operation
or a different verb. Do not require a discriminator value merely to choose
between materially different operations. Split operations when their intent,
validation, effects, or result meaning differ.

An operand-free form may use one documented default subject only when the
subject is safely and finitely bounded. Do not infer a subject from filesystem
coincidence, ambient state, fuzzy matching, or an accidental nearby resource.
If no safe finite default exists, require the subject.

Repeated primary subjects use separate operands so that parsing, completion,
validation, and errors remain independent. A modifier may accept several named
members of one compatible dimension when its product-local grammar defines
ordering, duplicates, and unsupported combinations. Do not use an opaque
comma-separated value to hide several independent subjects.

## Guided Input

When interaction is useful, expose it from the simplest useful invocation of a
leaf whose subject can be safely enumerated. Interactive answers, explicit
operands, and modifiers must populate one typed request. Conflicting explicit
inputs are rejected; they do not create hidden precedence or disjunctive modes.

Non-interactive modes never prompt. Missing semantic input is rejected. Missing
authority or an unresolved choice cannot proceed. A recommendation may help a
person review a choice, but displaying it does not select or authorize it.

A product may provide a guided or automatic interaction modifier. Mark that
modifier as a deliberate variation point and define its deterministic defaults
and selection limits in the product contract. It must not silently add
replacement, deletion, ownership, safety-bypass, or fuzzy-choice authority.

## Orthogonal Modifiers

Classify every modifier before adding it. The names and spellings below are
roles, not prescribed flags:

| Role         | Purpose                                                                             |
| ------------ | ----------------------------------------------------------------------------------- |
| Context      | Selects shared execution context or environment                                     |
| Selection    | Adds or narrows compatible input or results                                         |
| Guided input | Uses explicit input plus documented deterministic choices without prompting         |
| Projection   | Selects parts or density of one result                                              |
| Write policy | Selects preview, consent, or one named safety boundary                              |
| Authority    | Widens one explicitly named owned or replacement boundary within the same operation |

Modifiers in compatible roles compose without hidden precedence. A bounded
Authority modifier must be idempotent and must not imply unrelated replacement,
deletion, adoption, ownership, identity, containment, verification, or recovery
authority. If a modifier changes the job, subject, validation, effect family,
result meaning, or recovery lifecycle, make it another operation.

Define each cross-command modifier once when its meaning is truly shared. A
well-formed shared modifier may be an explicit no-op when it has no applicable
behavior for the selected operation. A product-local contract remains the
detailed owner of applicability and errors.

## Preview And Application Parity

When a mutating operation supports preview, choose one product-local preview or
dry-run spelling and document it as a deliberate variation point. Preview uses
the same typed request, current facts, planner, and preflight as application. It
reports the selected effects and writes nothing. Application does not silently
use a different plan or input resolver.

Preview and application share the operation's ordinary result conditions. A
planned change alone does not create a special warning result
unless the product contract defines a finite independent condition. Preview
cannot produce an apply-only effect failure. Read-only operations stop after
their result and do not create an empty mutation plan.

If an operation has no meaningful mutation, do not add a preview mode merely to
make this Pattern appear uniform. If a product needs a different planning
model, record the variation and keep the relationship between preview and
application explicit.

## Typed Flow And Results

Keep the important stages visible and directly testable:

```text
validated input
  -> complete typed request
  -> current facts
  -> read result or complete mutation plan
  -> preflight when mutating
  -> preview or application consent
  -> revalidation and apply
  -> verification or local recovery
  -> typed operation result
  -> human or structured rendering
  -> process completion
```

Handlers return typed values. They do not write streams, select presentation, or
set process status. Human and structured renderers consume the same result and
do not rerun parsing, planning, effects, or verification.

The product contract must define its finite result vocabulary, precedence,
process mapping, and stream allocation. Keep primary result output separate
from bounded diagnostics. Structured output should render one complete result
from the same typed value; any compatibility exception is a deliberate product
variation.

## Determinism, Errors, And No-Ops

Resolve one request against one defined current-fact set. Define stable ordering,
duplicate handling, and conflict behavior. Repeating the same request against
the same relevant facts produces the same semantic result.

An idempotent write converges on one state. Repeating it after that state is
reached returns a verified no-op rather than an invented effect. If the operation
is intentionally non-idempotent, state that as a deliberate variation and make
the consent and result meaning explicit.

Every error identifies the operation, affected subject, cause, and useful next
action. Ordinary errors remain understandable without verbose diagnostics.

Recovery is local to the operation's effect family. Define its verification,
reverse or residual behavior, interruption handling, concurrent-change rule,
and handling of recovery artifacts next to the operation contract. Do not assume
that every mutation needs the same backup, journal, staging, tombstone, or
rollback model. A special recovery exception belongs in the product-local
contract and must not silently become a generic Pattern rule.

## Authority And Locality

Make authority explicit in the command path, operands, or a named modifier.
Interaction policy, preview, presentation, and safety bypasses do not grant
authority that the operation did not already define. A safety bypass must not
silently grant overwrite, delete, force, adoption, ownership, or recovery
authority.

Keep the command's syntax, request resolver, handler, planner, renderer, and
focused tests in the narrowest useful local scope. Promote support to a shared
scope only after multiple real operations need the same meaning. A shared
helper must not hide the operation's subjects, authority, stages, effects, or
result semantics.

## Testing And Review

Test the operation at its typed boundaries and through its observable interface.
At minimum, review or test:

- Help and command-path discovery, including the bare group behavior when a
  group exists.
- Explicit subjects, safely bounded defaults, missing input, and rejected
  discriminator-like modifiers.
- Repeated singleton and multi-value inputs, duplicate handling, and compatible
  modifier composition.
- Guided and non-interactive request resolution, including recommendation and
  unresolved-authority boundaries.
- Deterministic ordering, repeated unchanged requests, and verified no-op
  behavior.
- Preview/application parity, complete planned effects, no-write guarantees,
  and preview-specific variation points when applicable.
- Typed stage results, human and structured rendering from one result, stream
  separation, diagnostics, process mapping, and finite status precedence.
- Local error messages, next actions, verification, recovery, interruption,
  concurrency, and residual facts for the operation's effect family.
- Source locality, direct tests, and the absence of a generic abstraction that
  obscures command meaning.

Before accepting a new operation, confirm that its deliberate variation points
are named and owned by the product contract:

| Variation point          | Review question                                                          |
| ------------------------ | ------------------------------------------------------------------------ |
| Command depth and groups | Is the path as shallow as the domain allows, and is a group substantive? |
| Subject default          | Is the default finite, safe, deterministic, and explicit?                |
| Interaction              | Does non-interactive use remain complete without prompting?              |
| Modifier grammar         | Are roles, repetition, composition, and conflicts unambiguous?           |
| Preview                  | Does preview share request, facts, planning, and preflight?              |
| Authority                | Is every widened boundary explicit and no broader than named?            |
| Results and streams      | Are status, process, structured output, and diagnostics locally owned?   |
| Effects and recovery     | Does the local policy match the effect family and preserve facts?        |
| Locality and testing     | Can readers test and review the operation without a hidden engine?       |
