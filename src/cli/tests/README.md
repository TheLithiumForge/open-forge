# CLI Tests

The active CLI evidence projects are split by boundary:

- `unit/` contains pure and directly callable evidence.
- `integration/` contains Core modules and real owned operating-system boundaries.
- `end-to-end/` invokes the built or published executable.
- `support/` contains cohesive fixtures shared by at least two active projects.
- `preserved/` contains candidate evidence from removed implementations and is not
  part of the active project graph.

Read the selected implementation Task before porting preserved evidence.
