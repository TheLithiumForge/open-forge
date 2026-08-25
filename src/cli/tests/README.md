# CLI Tests

The active CLI evidence projects are split by boundary:

- `unit/` contains pure and directly callable evidence.
- `integration/` contains Core modules and real owned operating-system boundaries.
- `end-to-end/` invokes the published executable. Its CLI project dependency
  publishes `open-forge-dev` under the repository-root `artifacts/` directory
  during an ordinary build.
- `support/` contains cohesive fixtures shared by at least two active projects.

Run `dotnet test` from the repository root or use the IDE test runner. Local runs
need no published-executable environment variables. `dotnet test --no-build`
requires the publication selected by an earlier build. Native evidence compiles
this project with one supported target RID rather than an executable-path
override.

The former preserved route-list suite was removed after its useful expectations
were adopted or found redundant. Read the selected implementation Task before
restoring any historical expectation as active evidence.
