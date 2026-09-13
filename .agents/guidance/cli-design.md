---
open-forge:
  description: Design command-line tools with useful output, predictable interaction, and simple implementation boundaries
  tags: [Guidance, CLI, Design, UserExperience, Presentation, Development, Testing]
---

# CLI Design

## When This Helps

Use this guidance when designing or reviewing commands used by people, scripts,
or agents. It recommends reusable approaches. The project's accepted interface,
behavior and architecture documents define its exact commands, defaults, output
schemas and guarantees. Apply its writing standard to help and generated text.

## Make The Command Predictable

Give each command one complete job. Use a group when several real operations
share a subject. Keep paths shallow and vocabulary consistent. Use operands for
primary subjects and options for compatible selection, presentation or write
policy. One option should keep the same meaning wherever it appears.

Let the command-line library handle its supported delimiters, arity, repeated
options, conversion and help. Validate product requirements on parsed values.
Examples should show the ordinary spelling first and useful alternatives when
they serve a real journey, such as bundled input versus an explicit source.

Make the default useful for the common task and state it in help. Changing a
default or accepted input is a behavior decision, even if the implementation is
small. Derive help from the registered commands so it stays aligned with them.

## Show The Result And The Next Useful Action

Lead with what the command found or did and whether the result is complete.
For a search, matched identities and paths are usually the useful result. For a
mutation, distinguish planned changes, applied changes, retained files and
anything the user still needs to resolve. Make an empty result understandable.

A diagnostic should identify the condition, the affected source and a useful
action. Use the file path and available line/column for editing. Keep byte spans
and implementation evidence in a supporting view when they do not help the
immediate task. Say what is unknown instead of presenting a candidate as a fact.

Schematic diagnostic:

```text
WARNING  Broken link
docs/guide.md:44:16
The linked file was not found: ../design.md
Check the target path, then update or remove the link.
```

Prefer concrete terms such as “source”, “possible target” and “why included” to
unexplained internal vocabulary. Preserve distinctions that affect the action:
an unreadable file, a missing file and an ambiguous target need different advice.
Do not invent a repair command or promise that an action will succeed.

Group information by the task's domain relationships when that helps navigation.
Status labels can remain visible within those groups. Show shared supporting
details once, while preserving distinct occurrences, findings, evidence and
ordering. Repetition removal must not silently remove diagnostic kinds or change
counts. Measure output size on a real large result before claiming improvement.

## Offer Deliberate Levels Of Detail

Compact output should keep core identities, order, status, completeness and
required actions. Use short meaningful labels and stable rows. People and agents
both benefit from this; a separate AI view needs a distinct demonstrated use.
Avoid obscure abbreviations that save characters but require explanation.

Expanded output should explain the result with relevant evidence and context.
It should still read as a useful command result. Keep implementation diagnostics
in a separate verbose channel. A view changes presentation, not the operation's
selection, findings, effects or exit status. Implement each promised view for
each command. If fallback is supported, define it explicitly; a renderer failure
is not a reason to rerun the operation.

Selected authored content stays exact in every view. Mutation plans, safety
conditions and recovery details need enough space to remain actionable. A short
view is not useful if it hides the information needed to make a safe decision.

If JSON has different detail levels, define their retained fields and schema
identity explicitly. Removing whitespace and removing information are different
changes. Any filtering or result limit also needs an accepted contract for what
is selected, what is omitted, ordering, totals and how to obtain the rest. Visible
detail must not redefine the underlying operation's status or completeness.

## Support Terminals And Automation

Keep structured stdout parseable and send separate diagnostics to the documented
stream. Use stable status and exit meanings in both text and JSON. Scripts should
not need to parse human sentences to determine success or find a source identity.

Use a consistent semantic colour palette automatically where terminal capability
supports it. Keep written labels, respect plain-output preferences such as
`NO_COLOR`, and assess stdout and stderr independently. Keep colour out of JSON,
selected authored content and machine-readable rows. Test actual terminal and
redirected output; injected writers alone cannot reveal host initialization
effects. State unsupported-terminal fallback in the public contract.

Prompts should resolve a real missing choice or authority boundary. Honor complete
authorized requests without redundant confirmation. Define how noninteractive
invocations report missing input, and keep structured output free of prompts.
A dry run should expose the real plan and blockers without applying changes.
Report partial completion honestly; explain recovery only when it actually exists.

## Keep Rendering Separate From The Operation

Parse once into a typed request, execute once, then render the same concrete
result. Keep terminal capabilities and writers at the host/presentation boundary.
Render from typed statuses, severities, paths and evidence. Do not recognize
English words in completed output to add colour or recover structured facts.

Let accepted parsers and serializers own YAML, Markdown and JSON syntax. Consume
their models, tokens or syntax trees, then apply product rules. Exact source
editing may need original spans and bytes; that does not justify a second format
parser. Share identical format mechanics at the nearest common scope and keep
command-specific interpretation with its command.

Derive packaged-data inventories from their assets and manifests. Keep IDs and
dependencies in their declared source, and validate the resulting catalogue.
Avoid a second handwritten catalogue in code. Custom format handling or platform
machinery needs a demonstrated capability gap and an accepted, bounded reason.

## Keep Evidence And Guidance Useful

Use small authored fixtures for behavior tests. Editing unrelated repository
prose should not break those tests. Use focused, reviewed snapshots when wording
and layout are themselves the contract. Assert identity, status, streams, effects,
ordering and safety directly. Snapshot updates require review; they do not accept
an accidental behavior change. Keep shipped-data parity checks strict.

Record the accepted scope and a recoverable checkpoint before each refactoring
set. Qualify the final source with the checks its changed boundaries require.
Keep required fixtures, helpers and regression tests in tracked source. Build
outputs and trial logs may be disposable; exact reproduction and acceptance
records must remain recoverable without them. Remove temporary experiments.

Base compatibility work on actual supported consumers and versions. Do not add
speculative migration rules for an unreleased internal change. Current interface
and safety promises still apply before release. Remove solved exceptions from
active guidance and keep useful history in work records.

## Adapting The Guidance

Dense search results favor rows; a risky mutation may need a longer explanation.
Grouping saves repetition but adds navigation. Compact JSON saves output while
creating a schema obligation. Use real journeys to choose among these tradeoffs,
and update the project's contracts when an accepted choice changes behavior.
