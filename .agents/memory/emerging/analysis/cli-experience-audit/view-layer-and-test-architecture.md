---
open-forge:
  description: Whether the render layer is the pure model-driven View it was meant to be, and a rebuilt test taxonomy based on where this codebase actually seams rather than on unit-integration-e2e convention
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Architecture, Presentation, Testing, Layers]
---

# View Layer And Test Architecture

## Part 1 — the View layer is what you expected

Measured across all 324 rendering and presentation files:

| Ambient dependency | Files using it                                     |
| ------------------ | -------------------------------------------------- |
| `Directory.`       | 0                                                  |
| `DateTime.`        | 0                                                  |
| `Console.`         | 0                                                  |
| `async` / `Task<`  | 0                                                  |
| `File.`            | 1 — a false positive (`state.OrdinaryFile.Sha256`) |
| `Environment.`     | 18 — **all 19 usages are `Environment.NewLine`**   |

**The render layer performs no I/O, reads no clock, touches no console, and is
entirely synchronous.** It is a pure function of its input.

The shape is also consistent. Every command has exactly three entry points with
an identical signature:

```csharp
internal static string  Render(CliPresentationRequest<TResult> presentation)   // human
internal static string  Render(CliPresentationRequest<TResult> presentation)   // json
internal static string? Render(CliPresentationRequest<TResult> presentation)   // diagnostic → stderr
```

38 such entry points across ~20 commands. The cascade you guessed at is exactly
what happens — one entry renderer dispatches on view, then fans out to fragment
renderers:

```csharp
// ContextHumanRenderer.cs:16
return presentation.Presentation.View switch
{
    CliView.Compact  => ContextCompactHumanRenderer.Render(presentation),
    CliView.Expanded => ContextExpandedHumanRenderer.Render(presentation),
    …
};
```

And they are wired as **method-group delegates** at composition:

```csharp
// CliExtensionComposer.cs:78
ExtensionListHumanRenderer.Render,
ExtensionListJsonRenderer.Render),
```

So: one command, one entry render per output format, driven purely by a data
model, injected as a delegate, triggerable however you like. **Your expectation
is met, and this is the best-architected part of the system.**

### What is actually wrong with it

Not the separation — the _absence of a decision_. A View in MVC does two jobs:
it decides **what to show** and then **how to show it**. This layer does the
second job impeccably, 324 files' worth, and the first job not at all.

There is no stage between "here is the complete model" and "render every field
of it". That is why 25,477 lines of correct, pure, well-separated rendering code
produce an 8.8 MB `doctor`. The missing piece is one function:

```
select(model, detail) → smaller model
```

Everything else about the layer is right and should be kept. This is a strong
position to be in: the expensive structural work is done, and what is missing is
a single new stage plus deletions.

## Part 2 — the test taxonomy

### Your instinct, checked

> unit = pure functions; integration = per module, pure or impure; e2e = per project

That is the conventional definition, and it is the wrong axis **for this
system** — which is why the layers feel murky. The conventional axis is _scope
of code under test_. But this codebase has a much sharper natural seam than
"module": the **model boundary**, where everything upstream is impure and
everything downstream is pure. That seam was proven above, not assumed.

Splitting by _scope_ cuts across that seam and produces exactly the confusion you
have: integration and e2e both test "the whole thing", differing only in whether
a process is spawned — a **mechanism** difference, not a **concern** difference.
That is the entire reason they duplicate.

### What the suite actually asserts

Assertions by what they target, across all 2,811 tests:

| Layer       | Model facts | JSON projection | Human text |
| ----------- | ----------- | --------------- | ---------- |
| unit        | 1,046       | 862             | 436        |
| integration | 2,001       | 2,090           | 197        |
| end-to-end  | 207         | 655             | 139        |
| **total**   | **3,254**   | **3,607**       | **772**    |

Two conclusions, both sharp.

**The JSON projection is tested more than the model it serializes** — 3,607
against 3,254. A serializer being asserted more heavily than its source is
duplication by definition: most of those assertions re-check model facts through
a second lens.

**Human text is tested 4.7× less than JSON.** The output every user and every
agent actually reads is the least-verified artifact in the system, and most of
those 772 are `Assert.Contains`, which cannot fail on noise.

This is the same "internal fidelity over external outcome" pattern found in the
naming and the code volume, now quantified at the assertion level. The suite
thoroughly checks the two things that are mechanically checkable and neglects the
one that needs judgment.

### The taxonomy this codebase wants

Four concerns, defined by _what can break_, not by scope:

| #     | Concern                                                               | Entry point                                           | Fixture  | Speed   |
| ----- | --------------------------------------------------------------------- | ----------------------------------------------------- | -------- | ------- |
| **A** | **Pure logic** — is this function right?                              | direct call                                           | none     | µs      |
| **B** | **Facts** — given this workspace, is the model right?                 | operation `ExecuteAsync`                              | temp dir | ms      |
| **C** | **Behaviour** — given this command line, is what the user gets right? | `CliCoreApplication.RunAsync(argv, env, writers, ct)` | temp dir | ms      |
| **D** | **Artifact** — does the shipped binary work?                          | spawn published exe                                   | temp dir | seconds |

The mapping to what exists:

- **A** ← most of unit. Renderers, parsers, planners, fingerprint computation,
  and the new selection layer. Property-based testing belongs here and is absent.
- **B** ← most of integration. Already well covered — 2,001 model assertions is
  the suite's genuine strength.
- **C** ← **does not exist today.** It is split between integration (which stops
  at the model or renders ad-hoc) and e2e (which spawns a process to get the same
  thing). This is where snapshots, exit codes, stream routing, interaction, and
  multi-command sequences all belong.
- **D** ← a small slice of e2e. Six concerns, a few dozen tests.

The one gap that matters: **C is where every defect in this audit lives, and it
is the layer that does not exist.** A and B are healthy. D is over-built.

### Why C cannot be e2e

Beyond speed, there is a hard constraint. `CliCompositionInputs` exposes
`StandardInputRedirected` and `PromptOutputRedirected` as injectable flags, and
`canPrompt` derives from them. A spawned test process **always** has redirected
stdin, so under D, `CanPrompt` is permanently `false` and the `repair` wizard,
`install` confirmation, `extension install` selection and permission prompt are
all unreachable. All four are broken. None was testable where it was assigned.

## What is good, bad, and what to transform

### Good — keep unchanged

- **The pure render layer.** No I/O, no clock, synchronous, delegate-injected,
  three consistent entry points per command. Proven above.
- **Model coverage.** 2,001 integration assertions on result facts. The data
  layer is genuinely well tested and is why B needs almost no work.
- **The write-freedom harness.** Base64 tree snapshot before and after every
  read-only run. Rare and valuable.
- **Closed-vocabulary tests.** `DoctorHumanVocabulary.HumanMappingsAreClosed`
  walks every enum through every renderer and asserts undefined throws.
- **Zero-versus-unknown discipline.** `UnknownCountsDoNotBecomeNoFindings`
  proves an unknown count never renders as `Findings: none`.
- **AOT execution of the in-process suite.** `native-integration` publishes and
  runs natively, so in-process testing exercises the real trimming constraints.

### Bad — delete rather than migrate

- **Composed-output snapshots in the unit layer.** `DoctorHumanSnapshots` is a
  30-line `const string` that freezes the `Observed:`/`Read from:` detachment
  bug as its expected value, cannot be regenerated, and is too small to catch
  what matters.
- **Verbatim copy assertions.** `PublishedDoctorProcessTests` pins _"Typed
  Extension bridge-registration role and observed-state authority is
  unavailable"_, making a copy rewrite a test failure.
- **`Assert.Contains` as the primary human-output check.** It passes whether the
  output is 4 lines or 104. This is the mechanism by which a 194-line "no
  problems" was invisible.
- **The e2e/integration overlap.** 87 of 91 e2e runs go through `--json` to
  assert what `RunAsync` returns directly, across 172 process-spawn call sites.
- **Redundant JSON assertions.** 3,607 of them, exceeding model assertions. Most
  re-verify facts the model tests already own.
- **Three separate workspace fixture builders** — `StatusIntegrationWorkspace`,
  `PublishedStatusWorkspace`, and the proposed scenario seeds.

### Transform

| From                              | To                                                                                                                      |
| --------------------------------- | ----------------------------------------------------------------------------------------------------------------------- |
| e2e (106 tests, 172 spawns)       | **D**: process smoke, six concerns, a few dozen tests                                                                   |
| integration                       | **B** (model facts, keep as is) + **C** (new: `RunAsync` behaviour, snapshots, interaction, sequences)                  |
| unit rendering tests              | **A**: fragment purity and the new selection rules; composed output moves to C                                          |
| 3,607 JSON assertions             | one schema-contract test per command in **C**, plus a schema validator. The model tests in **B** already own the facts. |
| 3 fixture builders                | one seed catalogue serving B, C and D                                                                                   |
| "preserves the exact…" test names | situation names: `doctor/healthy-workspace`, `install/into-existing-skill-workspace`                                    |

### The one thing to add

**Property assertions in A**, over generated models — the layer that does not
exist and costs least:

- `lines(brief) ≤ lines(standard) ≤ lines(verbose)`, always
- no output exceeds its configured `outputLines` threshold
- errors precede warnings precede informational
- a zero-finding result never contains `findings:`
- output is valid UTF-8 with no `\uXXXX`, `\\`, or literal `\n`
- rendering is idempotent and free of ambient state

These are possible _only because_ the render layer is pure — which is the
payoff for the architecture already being right. They catch classes rather than
instances, need no fixtures, and several fail today.

## Summary

The View layer is sound and needs one addition, not a rebuild. The test suite is
sound at the **fact** layer and absent at the **behaviour** layer, and the
integration/e2e confusion is a symptom of splitting by mechanism instead of by
concern.

The corrective is subtraction plus one new layer: delete the e2e duplication and
the frozen snapshots, collapse the redundant JSON assertions, and build C — the
layer where `argv` goes in, output comes out, and every defect in this audit
would have been caught.
