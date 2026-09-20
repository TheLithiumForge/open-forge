---
open-forge:
  description: Open Task 34 to visually distinguish command names, paths, identifiers and arguments interpolated into user-facing sentences, and to establish it as an authoring rule
  tags: [Memory, Working, CLI, Task, Presentation, Wording, Accessibility, Contextual, Active]
---

# Task 34 — Interpolated Value Markup

## Task state

- State: **Open, not started.** Raised and ruled by the maintainer on
  2026-09-16.
- Owner: Root.
- Evidence: gathered below from the merged code.

## Accepted decision

**Every command name and every argument appearing in a user-facing string is
written as code**, in the string itself, everywhere it appears. That is the
whole rule.

Ruled by the maintainer on 2026-09-16, and deliberately kept simple:

- One convention, applied literally in the wording, with **no structural span
  mechanism** and no renderer-side markup pass.
- **No conditional behaviour.** The text does not differ between a terminal, a
  pipe, `NO_COLOR`, or JSON. One string, one truth, everywhere.
- Colour is explicitly **not** part of this Task. `CliTextStyle` already exists
  and may be layered on later if anyone wants it; it is not needed for the
  sentence to read correctly, and adding it here would reintroduce the
  conditional behaviour this ruling removes.

The snapshot churn is accepted as a one-time cost. It is not a reason to invent
a more elaborate design — the maintainer was explicit that contorting the text
model to avoid regenerating captures is the wrong trade.

## The problem

Values are dropped into sentences bare, so the sentence stops parsing as
English. The maintainer's example:

```text
Extension create needs confirmation, and this session cannot ask.
```

`Extension create` is a command name, but nothing marks it as one, so the
sentence reads as a broken clause rather than a statement about a command. The
same shape recurs throughout the shared wording:

```csharp
=> $"Cannot {command}: {problem}."
=> $"Cannot use {path} as the workspace: it does not exist or cannot be read."
=> $"{path} changed after the plan was made. Nothing was changed."
=> $"{path} cannot be written safely: {reason}."
=> $"The Entries section of {path} could not be identified: {reason}."
```

Every `{command}`, `{path}` and `{reason}` is unmarked. A reader cannot tell
where the interpolated value ends and the sentence resumes, which is worst
exactly when the value contains spaces — as command names always do.

## What already exists

The infrastructure is in place and is good. This Task is not about building a
styling system.

`Presentation/Shared/Text/CliTextStyle.cs` already provides the right primitive
and more:

```csharp
internal string Subject(string value) => _enabled ? $"[1m{value}[22m" : value;
internal string Dim(string value)     => _enabled ? $"[2m{value}[22m" : value;
internal string Information(string value) => Foreground(value, 36);
internal string Warning(string value)     => Foreground(value, 33);
internal string Error(string value)       => Foreground(value, 31);
```

Colour is correctly gated:

- `CliTextStyle.For(status, colors, json)` returns `Plain` for JSON
  unconditionally.
- `CliHostColorPolicy` honours the `NO_COLOR` environment variable and
  `Console.IsOutputRedirected` / `IsErrorRedirected`, per stream.
- `CommandColorIntegrationTests` covers it.

## The actual gap

`style.Subject(...)` is only ever applied at **field and table-cell level** —
`row.Path`, `row.Location`, `column == 0` — never inside a sentence. The
`*Wording` classes that compose sentences have **no access to `CliTextStyle` at
all**; they return finished `string`s.

So the markup cannot currently be applied where the problem is.

## The few things still to settle

Small, and all of them about applying the rule consistently rather than about
whether to apply it.

### The convention

Pick one code marker and use it everywhere. Backticks are the obvious candidate
and match how the catalogues already write commands and flags in prose. Whatever
is chosen, it appears literally in the string:

```text
`extension create` needs confirmation, and this session cannot ask.
```

### What counts as a command name or argument

Command names and flags are clear. Decide once, and write it down, whether these
also take the marker, then be consistent:

- filesystem paths — very common in these messages, and the case that reads
  worst unmarked;
- source and package identifiers;
- values echoed back from user input.

Erring toward marking them is likely right, since they are the spans a reader
needs to pick out of the sentence.

### Escaping

A marked value may itself contain the marker character. `CliText.Escape`
already handles control characters; check that it composes with the chosen
marker rather than assuming it does.

## Actionable boundary

- Apply the rule in the wording classes. No renderer changes, no span model, no
  conditional output.
- The same string goes to text and JSON. A JSON consumer receives the marker in
  the message field; that is intended, not a defect.
- Capture churn is expected and accepted. Still review regenerated captures
  per situation rather than bulk-accepting them — the churn being expected does
  not make an unintended change acceptable.
- This touches all 28 commands. It is mechanical, but it is wide.

## Acceptance

- Every command name and argument in every user-facing string carries the
  marker, in text and in JSON alike.
- The rule is recorded as a **standing authoring rule** where future work will
  meet it — the shared presentation rules in
  [00 — conventions](task30-g4/00-conventions.md) and the relevant Directive —
  so new messages follow it without being told.
- Every affected catalogue `message` column matches what is rendered.
- All four gates green.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.
