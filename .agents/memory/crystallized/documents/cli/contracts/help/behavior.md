---
open-forge:
  description: Resolve built-in topics and append the shipped guide's file formats without workspace reads
  responsibility: Define Help selection, content assembly, result formation and read-only effects
  tags: [Memory, Crystallized, CLI, Command, Contract, Help, Behavior, CurrentTruth, Evergreen]
---

# Help Behavior Contract

## Status And Authority

The [Interface](interface.md) defines the public command and required evidence.
This contract defines deterministic selection and content assembly.
The [CLI Architecture](../../architecture.md) defines layers and serialization.

## Request Resolution

An absent topic selects the topic list. A topic that exactly matches a shipped
topic selects its content. Any other topic forms an invalid-input result with
the supplied topic and the available topics in its cause.

Command-name suggestions come from the composed command tree. They do not use
a second command catalogue. A matching top-level command adds its `--help`
invocation to the cause and next action. Other unknown topics point to Help's usage.
Parser and global-option failures follow the shared invalid-input path.

## Content Assembly

Sections 1 through 11 come from the embedded
[concepts resource](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Help/concepts.md).
Their bytes are preserved. Help does not correct or rewrite the authored topic at runtime.

Section 12 is the `## Writing workspace files` section of `.agents/README.md`
from the embedded Framework payload, from its heading up to the next heading of
the same or a higher level. It is never copied into a separate Help resource,
and its bytes are preserved apart from trailing blank lines. A shipped guide
without that section is a failed result.

The operation reads neither the selected workspace nor any workspace's loader,
settings or guide. Workspace changes, including a deleted or edited README,
cannot alter the built-in topic. It produces command-owned file-format facts
for presentation. Text and JSON use
the same complete Markdown, independent of detail and finding filters.

## Result Formation And Effects

A valid request forms one complete read-only result. An unknown topic forms one
invalid-input result. The result has no workspace or effects. The shared result
contract supplies completion and stream rules. Repeating a request returns the
same result with unchanged embedded inputs.

Help writes no files, installs no content, creates no recovery data and takes no
mutation lock. It never invokes a workspace command to obtain its topic.

## Behavioral Conformance

Use direct tests for selection, invalid results, cancellation and serialization.
Use composed-host tests for streams, content parity, workspace independence and
all detail levels. Use published-process tests for the public topic journeys.
Reviewed snapshots define stable wording, while direct assertions prove bytes,
status, effects and workspace independence.

## Related Current Sources

- [Help Interface Contract](interface.md)
- [Help Command Contract Set](_help.md)
- [Shared CLI Operation Contract](../../shared-operation-contract.md)
- [Shared Result Coordinates](../shared/result-coordinates/interface.md)
