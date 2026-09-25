# Reference records

What a careful run might produce, for comparing with what your agent wrote. They aren't the only right answer, and reading them first defeats the point of the demo.

| Record                                                                            | Where it would live                      | What it does                                                                                         |
| --------------------------------------------------------------------------------- | ---------------------------------------- | ---------------------------------------------------------------------------------------------------- |
| [Project sources map](project-sources.md)                                         | `.agents/maps/`                          | Points every later task at the README, the author's notes, and the tests, and says when to read each |
| [Percentage split rounding](../seeds/4-records/decision-uneven-split-rounding.md) | `.agents/memory/crystallized/decisions/` | Records the choice made while adding the feature: whole cents, leftover cents to the payer first     |

Notice what isn't here: no Decision for the rules the app already had. Those were decided before you arrived, and their reasons live in `NOTES.md`. The Map makes them findable. A Decision records a choice when it's made, which in this demo means the rounding rule for the new feature.

The Decision isn't where the feature is described. Once percentage splits ship, the README documents `--percent`, and the Decision's `Current Sources` section links to it. The README says what's true now, and the Decision says why.

If a rule from the notes keeps getting broken, that's the moment to add it as a Directive, scoped to the code that handles money.
