---
open-forge:
  description: "Historical CLI-v2 source: Commander validates command input while other untrusted boundaries use focused runtime validation without a mandatory schema library"
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Runtime Boundary Validation

## Context

The replacement CLI needs explicit typed values after argument parsing, safe decoding of workspace and package data, and trustworthy structured results. TypeScript types alone disappear at runtime, but applying two runtime validators to the same input adds code, dependency weight, startup work, and competing error contracts.

The accepted command framework already supports required and optional values, coercion, arrays, enums, ranges, regular expressions, and custom parsing functions for flags and positional parameters. Other untrusted boundaries, such as Markdown metadata or package manifests, do not pass through the command parser.

## Decision

Commander owns syntax, presence, coercion, and value validation for command options and positional arguments. `@commander-js/extra-typings` infers their TypeScript values from the chained definition. Command registration actions and command-local request resolvers turn those values into complete semantic requests before handlers plan. Do not revalidate parser-owned values with Zod or another general schema library.

Validate every other untrusted boundary once, where it enters the application. Start with a focused parser or validator named for that boundary and return an explicit TypeScript value or stable public finding. Domain code receives already validated values and does not repeatedly parse them.

Do not adopt Zod or another schema library as a default dependency. Introduce one only when representative implementation proves that repeated object-schema validation, schema generation, or error-path reporting becomes smaller and clearer than focused validators by enough to justify its runtime and dependency cost.

The accepted [frontmatter YAML boundary](cli-frontmatter-yaml-boundary.md) is a
focused syntax-parser exception, not a general object-schema policy. The
`yaml` package recognizes YAML syntax and positions inside one bounded region;
Open Forge still owns the narrow metadata schema, normalization, limits, and
stable findings.

Library-specific failures never define the public CLI contract. Map validation failures to the replacement's stable error or finding types at the boundary.

## Rationale

Commander plus its companion inferred typings already establishes typed command input, so a second schema pass would provide no independent safety. Focused validators keep small boundaries explicit and avoid committing every handler and data shape to a library before the replacement demonstrates that need.

This is not a prohibition on schema libraries. It makes their adoption evidence-driven and boundary-specific, preserving the option when a real family of structured contracts would otherwise create duplicated validation code.

## Consequences

- Command definitions are the single runtime source of truth for command flags and parameters.
- Command-local request resolvers consume those validated values and produce complete semantic requests without revalidating parser-owned syntax or leaking raw flags.
- Workspace metadata, manifests, receipts, plans, and result envelopes receive separate validation only where their trust boundary requires it.
- Each validator follows the same small-file, readability, strictness, and ESM rules as other TypeScript source.
- Tests cover accepted, rejected, and normalized values plus stable public error mapping without asserting a validation library's private output.
- A future schema-library proposal must name the affected boundaries and include representative readability, duplication, startup, and distribution evidence.
- A boundary-specific syntax parser cannot broaden accepted domain values or
  expose library-specific results to consumers.

## Authoritative Sources

- [Open Forge CLI Architecture](../../documents/cli/architecture.md)

## Decision Relationships

- [CLI command framework decision](cli-command-framework.md)
- [Agent-first CLI product contract](cli-agent-first-product-contract.md)
- [CLI testing architecture](cli-testing-architecture.md)
- [CLI frontmatter YAML boundary](cli-frontmatter-yaml-boundary.md)
