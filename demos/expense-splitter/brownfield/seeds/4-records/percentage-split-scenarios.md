---
open-forge:
  description: Expected results for percentage splits, including rounding and invalid input
  tags: [Memory, Document, Scenario, Split]
---

# Percentage split scenarios

## Purpose

Define what `add ... --percent` must do, so the feature can be checked the same way every time.

## Shared context

Each scenario starts from an empty ledger. Amounts are shown as the app prints them. The [rounding decision](../decisions/decision-uneven-split-rounding.md) defines who receives leftover cents.

## Scenarios

| Scenario                          | Situation                                                                   | Expected result                                                    |
| --------------------------------- | --------------------------------------------------------------------------- | ------------------------------------------------------------------ |
| Exact percentages                 | `add "Groceries" 100.00 --paid-by sam --percent sam=33,priya=33,lee=34`     | sam 33.00, priya 33.00, lee 34.00                                  |
| Leftover cent, payer listed first | `add "Lunch" 10.01 --paid-by priya --percent priya=50,lee=50`               | priya 5.01, lee 5.00                                               |
| Leftover cent, payer listed last  | `add "Lunch" 10.01 --paid-by lee --percent priya=50,lee=50`                 | priya 5.00, lee 5.01                                               |
| Payer not in the split            | `add "Gift" 20.00 --paid-by sam --percent priya=50,lee=50`, then `balances` | sam is owed 20.00, priya and lee each owe 10.00                    |
| Percentages don't add up          | `add "Oops" 10.00 --paid-by sam --percent sam=50,priya=40`                  | Error naming 90 and 100. Nothing is saved.                         |
| Both split options                | `add "Oops" 10.00 --paid-by sam --with lee --percent sam=50,lee=50`         | Error asking for one of `--with` or `--percent`. Nothing is saved. |
| Even splits unchanged             | `add "Dinner" 10.00 --paid-by sam --with priya,lee`                         | sam 3.34, priya 3.33, lee 3.33                                     |

## Coverage boundaries

These scenarios specify the command-line behavior. Shares by weight, such as `sam=2,lee=1`, are not part of this feature.
