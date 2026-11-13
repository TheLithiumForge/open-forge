---
open-forge:
  description: Define pinned Tool Run dispatch and raw child process behavior
  tags: [Memory, Document, CLI, Contract, Tool, Run, CurrentTruth]
---

# tool run

`tool run` is a command that runs a declared and verified workspace tool pin.

It adds no output on success and keeps the external program's arguments and streams intact.

Status: V1 contract decided. Implementation pending.

## Contract

The [interface](interface.md) defines inputs and failures. The [behavior](behavior.md) defines verification and dispatch.

## Entries

- [Define cheap Run checks and complete Status hashing](behavior.md) - #Memory #Document #CLI #Contract #Tool #Run #CurrentTruth
- [Record Dispatch services, registration points and focused verification evidence](implementation.md) - #Memory #Document #CLI #Tool #Run
- [Define Tool Run operands, raw streams and strict executable selection](interface.md) - #Memory #Document #CLI #Contract #Tool #Run #CurrentTruth
