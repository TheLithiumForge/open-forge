---
open-forge:
  description: Implement parser-owned input, terminal policy, normalized invocation, workspace absence, and exact binding selection
  tags: [Memory, Working, CLI, Task, Foundation, Parsing, Invocation, SystemCommandLine, Contextual, Complete]
---

# Implement Parsing And Invocation

## Task State

- State: Complete.
- Implementer: Mastermind.
- Parent: [CLI Foundation](../_foundation.md).
- Plan step: F2 parser and invocation portion.

## Expected Outcome

One `System.CommandLine` parse produces typed global input, exact selected binding,
terminal mode, and normalized invocation without raw-argument duplication or
domain execution.

## Required Components

- `CliRootDefinitionFactory` builds root and global option symbols from typed
  definitions supplied by the composition model.
- `CliParser` owns one `ParseResult` per invocation and returns one typed
  `CliParseOutcome` containing parser errors or parser-owned values.
- `CliGlobalInputReader` reads option and terminal facts only from typed parse
  results.
- `CliDelimiterPolicy` and `CliDelimiterGuard` inspect only accepted attached
  delimiters the library cannot expose. Definitions supply exact spellings.
- `CliTerminalValidator` applies parser errors first, then shared conflict policy,
  before terminal short-circuiting.
- `CliBindingSelector` uses exact selected `Command` identity and returns one
  registered binding or a typed root/group/no-leaf outcome.
- `CliInvocationResolver` forms one immutable invocation and preserves genuine
  workspace absence. It does not perform domain work.

## Required Ordering

1. Parse once.
2. Return library diagnostics unchanged as typed shell facts.
3. Run the bounded delimiter guard only for parser-valid input.
4. Read global values and validate terminal composition.
5. Select the binding by exact `Command` object.
6. Short-circuit valid help or version before workspace selection and command
   binding.
7. Resolve normalized process-wide invocation for a selected leaf.

## Evidence

Unit cases cover root, group, and leaf selection; unknown commands and options;
argument and option arity; repeated scalar and Boolean options; finite-value
conversion; every accepted delimiter form; parser-error precedence; terminal
conflicts; JSON terminal no-op; help/version bypass; exact symbol identity; no
leaf; and workspace absence.

An Integration case proves the pinned package behavior used by occurrence and
arity decisions. Do not assert undocumented behavior without executable evidence.

## Protected Boundaries

- No manual token parser, prefix matching, following-token inspection, value
  parsing, command string dispatch, command request, workspace filesystem check,
  renderer, writer, or process exit.
- No route or future command symbol is introduced.

## Stop Conditions

Stop if a required public syntax distinction cannot be represented by the pinned
library plus the bounded guard, or if selecting a binding requires string or
reflective dispatch. Record the exact parser fact and return to Architecture.

## Completion

Complete when every parser-owned and terminal fact has one owner, all direct
evidence passes, and the pipeline can consume one immutable invocation outcome.
