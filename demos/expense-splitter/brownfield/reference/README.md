# Reference records

What a careful run might produce, for comparing with what your agent wrote. They aren't the only right answer. Read them after your run, not before.

| Record                                                                            | Where it would live                      | What it does                                                                                     |
| --------------------------------------------------------------------------------- | ---------------------------------------- | ------------------------------------------------------------------------------------------------ |
| [Project sources map](project-sources.md)                                         | `.agents/maps/`                          | Points later tasks to the README, the author's notes, and the tests, and says when to read each  |
| [Percentage split rounding](../seeds/4-records/decision-uneven-split-rounding.md) | `.agents/memory/crystallized/decisions/` | Records the choice made while adding the feature: whole cents, leftover cents to the payer first |

Notice what isn't here: no Decision for the rules the app already had. Decisions record choices from the moment you adopt Open Forge, for changes made from then on. The older rules were decided before that, and their reasons live in `NOTES.md`, which the Map makes findable. In this demo, the one new choice is how percentage splits round.

The Decision isn't where the feature is described. Once percentage splits ship, the README documents `--percent`, and the Decision's `Current Sources` section links to it. The README says what's true now, and the Decision says why.

If a rule from the notes keeps getting broken, add it as a Directive in a scope for the code that handles money.
