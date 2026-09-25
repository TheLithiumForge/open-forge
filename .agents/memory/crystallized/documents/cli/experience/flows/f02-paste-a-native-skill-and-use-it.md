---
open-forge:
  description: "Paste a native Skill and use it"
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F02: Paste a native Skill and use it

**Who:** A maintainer who expects drag-and-drop package installation to work.

**Execution:** See the separate [run record](../../../../../archived/beta-preparation/cli-experience-run.md). Inclusion records a reviewed user outcome, not a passing implementation.

## Starting point

The verified end state of F01, or an independently verified W1. Do not reinstall between the following steps.

## Flow

Run the steps on the actual resulting state. A linked scenario supplies a verification boundary, not permission to reset the workspace between steps.

| Step | Action | Scenario | State passed forward |
| ---: | --- | --- | --- |
| 1 | Copy docs/cli-experience-fixtures/content/native-skill/team-notes to .agents/skills/team-notes. | [X01](../scenarios/experience.md#x01) | The native SKILL.md, plain Markdown reference and asset exist unchanged; navigation has not yet been rebuilt. |
| 2 | open-forge index | [C05-02](../scenarios/commands/c05-index.md#c05-02) | The Skills list exposes the recognized native package; bind actual changed-list counts rather than reusing the guidance fixture counts. |
| 3 | open-forge route list skills --depth=all | [C11-02](../scenarios/commands/c11-route-list.md#c11-02) | A usable native Skill source is discoverable. |
| 4 | open-forge route inspect .agents/skills/team-notes/SKILL.md | [C12-02](../scenarios/commands/c12-route-inspect.md#c12-02) | Identity and reading behavior refer to the native package, not its support files as separate ordinary routes. |
| 5 | open-forge context .agents/skills/team-notes/SKILL.md | [C08-02](../scenarios/commands/c08-context.md#c08-02) | The intended Skill content is available to a caller. |
| 6 | open-forge index | [C05-01](../scenarios/commands/c05-index.md#c05-01) | The repeat is a no-op and all copied package bytes remain unchanged. |

## Alternatives and recovery

### Native YAML truly malformed

Name the actual unclosed header and preserve all package bytes; the user fixes that syntax, then retries.

Scenarios: [C05-07](../scenarios/commands/c05-index.md#c05-07).

### Native header lacks open-forge fields

This alone is not a defect. Do not rewrite a valid native Skill into an ordinary routed-source format.

Scenarios: [X01](../scenarios/experience.md#x01).

## Final result

The package is discoverable and readable without adding Open Forge metadata to native files or support resources.

## Verification

Observe the resulting files and links independently of printed output. Record actions, stdout, stderr, exit, preserved bytes, and separate outcome/state/communication verdicts in a run record. Carry real state between steps; fork only at an explicit alternative.
