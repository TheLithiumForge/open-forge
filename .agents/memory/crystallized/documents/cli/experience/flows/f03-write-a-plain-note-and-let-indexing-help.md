---
open-forge:
  description: "Write a plain note and let indexing help"
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F03: Write a plain note and let indexing help

**Who:** A maintainer who creates Markdown directly and forgot the metadata convention.

**Execution:** See the separate [run record](../../../../../working/cli-experience-run.md). Inclusion records a reviewed user outcome, not a passing implementation.

## Review adjustment

Index plain and partially described Markdown without editing its metadata or body. Warn briefly about missing optional metadata, include the source in navigation, and allow immediate context retrieval. Metadata enrichment is optional, never a prerequisite.

This desired behavior is pending flow validation before test implementation. A current CLI disagreement is recorded as evidence, not copied into the expectation.

## Starting point

ROUTES without my-note.md. This flow is the requested target, not existing output behavior.

## Flow

Run the steps on the actual resulting state. A linked scenario supplies a verification boundary, not permission to reset the workspace between steps.

| Step | Action | Scenario | State passed forward |
| ---: | --- | --- | --- |
| 1 | Copy docs/cli-experience-fixtures/content/plain-note.md to .agents/guidance/my-note.md. | [X02](../scenarios/experience.md#x02) | The authored body exists exactly, without YAML. |
| 2 | open-forge index | [X02](../scenarios/experience.md#x02) | The plain note is navigable, its bytes are unchanged, and one useful metadata warning is visible. |
| 3 | open-forge doctor | [X05](../scenarios/experience.md#x05) | Any remaining authoring issue is described truthfully; no imaginary unfinished Framework update appears. |
| 4 | Optionally add a description and tags chosen by the author. | [X02](../scenarios/experience.md#x02) | Enrichment is optional; the original body remains intact. |
| 5 | open-forge index | [C05-02](../scenarios/commands/c05-index.md#c05-02) | If enriched, navigation uses the authored metadata and no longer warns about those missing fields; otherwise the note remains usable unchanged. |
| 6 | open-forge context guidance/my-note | [C08-02](../scenarios/commands/c08-context.md#c08-02) | The completed note can be read through its real route. |

## Alternatives and recovery

### Partial valid header

Use valid supplied fields, preserve all bytes, and warn about missing optional fields.

Scenarios: [X03](../scenarios/experience.md#x03).

### Preview first

Show the navigation change and metadata warning and no writes before applying.

Scenarios: [X04](../scenarios/experience.md#x04).

### Malformed or unreadable file

Do not guess a repair or call all four metadata conditions the same problem.

Scenarios: [X05](../scenarios/experience.md#x05).

## Final result

The author achieves a routable note without losing text or learning an internal parser model.

## Verification

Observe the resulting files and links independently of printed output. Record actions, stdout, stderr, exit, preserved bytes, and separate outcome/state/communication verdicts in a run record. Carry real state between steps; fork only at an explicit alternative.

