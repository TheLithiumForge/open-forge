# Checks

Use these after the agent reports the first version as done. Start each check from an empty ledger.

## The product works

| Action                                                   | Expected result                                  |
| -------------------------------------------------------- | ------------------------------------------------ |
| Sam pays 10.00 for dinner, shared by Sam, Priya, and Lee | Sam 3.34, Priya 3.33, Lee 3.33                   |
| Then show balances                                       | Sam is owed 6.66, Priya and Lee each owe 3.33    |
| Priya pays 0.05 for a taxi, shared with Sam              | Priya 0.03, Sam 0.02                             |
| Add an expense of 1.234                                  | Rejected with a clear message, and nothing saved |
| Add an expense shared by nobody                          | Rejected with a clear message, and nothing saved |

At levels 1 and 2 the agent never heard the rounding rule, so a different, explicitly chosen rule is acceptable if it's recorded. What isn't acceptable is shares that don't add up to the total.

## The quiet failures

- **No floating point money.** Amounts are parsed into whole cents and stay that way. The saved file holds integers.
- **Shares always add up.** Every saved expense's shares sum exactly to its total, and something enforces it.
- **Tests exist** for splitting, rounding, and invalid input, and they pass.

## The knowledge

Only when Open Forge is installed:

- The important choices, such as how money is stored and who absorbs rounding, are recorded as Decisions with their reasons.
- A Vision or Architecture exists if you asked for one, and stayed proposed until you accepted it.
- Each accepted Decision links to the document that now holds its result, and that document links back to the Decision for the reason.
- When a choice changed during the build, the documents it affects were updated to match, not just the Decision.
- The next person to open the project could learn why the code is shaped this way without asking you.
