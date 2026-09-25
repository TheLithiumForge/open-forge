# Checks

Use these after the agent reports the feature as done. Start each command from an empty ledger by pointing `EXPENSES_FILE` at a new file.

## The feature works

| Command                                                                     | Expected result                                   |
| --------------------------------------------------------------------------- | ------------------------------------------------- |
| `add "Groceries" 100.00 --paid-by sam --percent sam=33,priya=33,lee=34`     | sam 33.00, priya 33.00, lee 34.00                 |
| `add "Lunch" 10.01 --paid-by priya --percent priya=50,lee=50`               | priya 5.01, lee 5.00                              |
| `add "Lunch" 10.01 --paid-by lee --percent priya=50,lee=50`                 | priya 5.00, lee 5.01                              |
| `add "Gift" 20.00 --paid-by sam --percent priya=50,lee=50`, then `balances` | sam is owed 20.00, priya and lee each owe 10.00   |
| `add "Oops" 10.00 --paid-by sam --percent sam=50,priya=40`                  | An error that names 90 and 100, and nothing saved |

## Nothing broke

- `npm test` passes, including every test that existed before.
- `add "Dinner" 10.00 --paid-by sam --with priya,lee` still gives sam 3.34, priya 3.33, lee 3.33.
- `addExpense` still refuses shares that don't add up to the total.

## The rules survived

These are the quiet failures. The existing tests don't catch them.

- **No floating point money.** New code computes shares with integer arithmetic on cents. Look for `parseFloat`, `toFixed`, or dividing an amount by 100 before splitting it.
- **The payer absorbs rounding.** The third row above checks this. Giving the extra cent to whoever is typed first fails it.
- **New tests cover the new rules.** At least one test pins the leftover-cent behavior for percentage splits.

## The knowledge survived

Only when Open Forge is installed:

- A Map points to the README, the author's notes, and the tests, so later tasks can find the reasons behind the code.
- The choice this feature made, how percentage splits round, is recorded as a Decision with its reasons.
- The README says what's true now: it documents `--percent`, and its "Not done yet" list no longer includes percentage splits.
- The Decision links to where the current behavior is described, such as the README and the tests, instead of being the only place it's written down.
- No Decision claims to have decided the rules the app already had. Those were decided before you arrived.
- Nothing was accepted on your behalf. Proposed records stayed proposed until you agreed.
