---
open-forge:
  description: Expected results for adding expenses, splitting them evenly, and showing balances
  tags: [Memory, Document, Scenario]
---

# Expense scenarios

## Purpose

Define what the first useful version must do, so it can be checked the same way every time.

## Shared context

Each scenario starts from an empty ledger. Amounts are shown with two decimal places. The [rounding decision](../decisions/decision-payer-absorbs-rounding.md) defines who receives leftover cents.

## Scenarios

| Scenario                        | Situation                                                | Expected result                                  |
| ------------------------------- | -------------------------------------------------------- | ------------------------------------------------ |
| Even split with a leftover cent | Sam pays 10.00 for dinner, shared by Sam, Priya, and Lee | Sam 3.34, Priya 3.33, Lee 3.33                   |
| Balances                        | After the dinner, show balances                          | Sam is owed 6.66, Priya and Lee each owe 3.33    |
| Tiny amounts                    | Priya pays 0.05 for a taxi, shared with Sam              | Priya 0.03, Sam 0.02                             |
| Too many decimals               | Add an expense of 1.234                                  | Rejected with a clear message, and nothing saved |
| Nobody shares it                | Add an expense shared by nobody                          | Rejected with a clear message, and nothing saved |
| Stored form                     | Open the saved file                                      | Every amount is a whole number of cents          |

## Coverage boundaries

Uneven splits, settle-up suggestions, and editing are out of scope for the first version.
