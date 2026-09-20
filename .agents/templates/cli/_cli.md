---
open-forge:
  description: Copy-ready starting files for CLI documents, including command-local contract sets
  tags: [Template, CLI, Command, Contract, Interface, Behavior, TechnicalDesign]
---

# CLI Templates

This route keeps CLI Templates grouped by the artifact they start. These are
copy-ready starters, not current Documents or command instances. Copy and adapt
a starter into an independently maintained destination under the current
`contracts/{command-path}` topology. Keep one command or leaf operation's
Interface, Behavior, and optional Technical Design in that local scope. A group
entrypoint routes child operations; it does not create a second contract set.
The route does not predefine implementation-language or code Template scopes.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [Copy-ready document files for a command-local CLI contract set under contracts/{command-path}](documents/_documents.md) - #Template #CLI #Document #Command #Contract #Interface #Behavior #TechnicalDesign
